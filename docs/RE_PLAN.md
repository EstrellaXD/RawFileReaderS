# Thermo RAW 文件逆向工程方案 v2

## 项目概述

构建纯 Rust 实现的 Thermo RAW 文件读取库 `thermo-raw-rs`，无需 Thermo 官方 DLL，跨平台跨架构运行。通过反编译 Thermo .NET DLL 获取格式结构，用官方 RawFileReader 导出 ground truth 做交叉验证。

最终产物：
- `thermo-raw-rs` Rust crate（纯 Rust，零依赖 Thermo）
- PyO3 Python binding（`pip install thermo-raw`）
- CLI 工具（独立可执行文件）

---

## 核心策略：反编译驱动的逆向

关键发现：Thermo 的 RawFileReader 是 .NET 程序集，可用 ILSpy 反编译为可读 C# 代码。这将逆向难度从"黑盒 hex 猜测"降为"阅读源码提取格式规范"。

```
信息流：
NuGet DLL → ILSpy 反编译 → C# 源码 → 格式规范文档 → Rust 实现 → Ground Truth 验证
                                           ↑
                                    clean-room 隔离点
                                    （文档是中间产物，Rust 只看文档不看 C#）
```

法律合规：EU Software Directive 2009/24/EC Art.6 允许为互操作性目的反编译。采用 clean-room 方法：Phase 1 产出格式规范文档，Phase 3 的 Rust 实现只参考文档，不直接翻译 C# 代码。

---

## 项目结构

```
thermo-raw-rs/
├── Cargo.toml                          # workspace root
├── README.md
├── LICENSE-MIT
├── LICENSE-APACHE
│
├── crates/
│   ├── cfb-reader/                     # OLE2/CFBF 容器读取（公开规范，无需逆向）
│   │   ├── Cargo.toml
│   │   └── src/
│   │       ├── lib.rs
│   │       ├── header.rs               # CFBF 512-byte header
│   │       ├── fat.rs                  # FAT/DIFAT sector chain
│   │       ├── directory.rs            # directory entries
│   │       └── stream.rs              # stream 随机读取
│   │
│   ├── thermo-raw/                     # RAW 格式解析核心
│   │   ├── Cargo.toml                  # deps: cfb-reader, memmap2, rayon
│   │   └── src/
│   │       ├── lib.rs                  # pub API: RawFile, Scan, Chromatogram
│   │       ├── raw_file.rs             # 顶层入口，文件打开/关闭
│   │       ├── version.rs              # RAW 版本检测 (v57-v66+)
│   │       ├── run_header.rs           # RunHeader stream 解析
│   │       ├── scan_index.rs           # ScanIndex 解析 (offset table)
│   │       ├── scan_data.rs            # ScanData 解码 ← 核心难点
│   │       ├── scan_data_profile.rs    # Profile mode 数据解码
│   │       ├── scan_data_centroid.rs   # Centroid mode 数据解码
│   │       ├── trailer.rs              # TrailerExtra (scan-level metadata)
│   │       ├── chromatogram.rs         # TIC/BPC/XIC
│   │       ├── metadata.rs             # Sample info, instrument info
│   │       ├── scan_filter.rs          # Scan filter string 解析
│   │       ├── error.rs                # thiserror 错误类型
│   │       └── types.rs                # 公共类型定义
│   │
│   ├── thermo-raw-cli/                 # 命令行工具
│   │   ├── Cargo.toml                  # deps: thermo-raw, clap
│   │   └── src/
│   │       └── main.rs                 # dump, info, export, validate 子命令
│   │
│   └── thermo-raw-py/                  # Python binding (PyO3)
│       ├── Cargo.toml                  # deps: thermo-raw, pyo3, numpy
│       ├── pyproject.toml              # maturin build config
│       └── src/
│           └── lib.rs                  # Python API: open(), scan(), tic(), xic()
│
├── tools/
│   ├── ground-truth-exporter/          # C# 项目，用 Thermo NuGet 导出验证数据
│   │   ├── GroundTruthExporter.csproj
│   │   └── Program.cs
│   │
│   ├── decompiler/                     # 反编译脚本
│   │   └── decompile.sh               # ILSpy CLI 反编译 + 输出整理
│   │
│   └── hex-analyzer/                   # 逆向辅助 Rust 工具
│       ├── Cargo.toml
│       └── src/main.rs                 # 搜索 f64/f32/string, diff streams, auto-locate
│
├── docs/
│   ├── FORMAT_SPEC.md                  # ★ 核心产出：RAW 格式规范文档（clean-room 中间产物）
│   ├── OLE2_STRUCTURE.md               # OLE2 容器内 stream 列表与说明
│   ├── SCAN_DATA_ENCODING.md           # ScanData packet 编码细节
│   ├── VERSION_DIFFERENCES.md          # 不同 RAW 版本的结构差异
│   └── VALIDATION_REPORT.md            # 验证结果汇总
│
└── test-data/
    ├── raw-files/                      # .gitignore, 本地存放 RAW 文件
    ├── ground-truth/                   # C# 导出的 JSON 参考数据
    └── fixtures/                       # 小型测试固件（可提交到 git）
```

---

## Phase 0: 基础设施搭建

### Task 0.1: 初始化 Rust workspace

```bash
# 创建 workspace
mkdir -p thermo-raw-rs && cd thermo-raw-rs
cargo init --name thermo-raw-workspace
# 创建各 crate
cargo new crates/cfb-reader --lib
cargo new crates/thermo-raw --lib
cargo new crates/thermo-raw-cli
cargo new crates/thermo-raw-py --lib
cargo new tools/hex-analyzer
```

workspace Cargo.toml:

```toml
[workspace]
resolver = "2"
members = [
    "crates/cfb-reader",
    "crates/thermo-raw",
    "crates/thermo-raw-cli",
    "crates/thermo-raw-py",
    "tools/hex-analyzer",
]

[workspace.dependencies]
memmap2 = "0.9"
rayon = "1.10"
thiserror = "2"
serde = { version = "1", features = ["derive"] }
serde_json = "1"
clap = { version = "4", features = ["derive"] }
byteorder = "1"
```

### Task 0.2: Ground Truth Exporter (C#)

创建 `tools/ground-truth-exporter/GroundTruthExporter.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="ThermoFisher.CommonCore.RawFileReader" Version="*" />
    <PackageReference Include="ThermoFisher.CommonCore.Data" Version="*" />
    <PackageReference Include="ThermoFisher.CommonCore.MassPrecisionEstimator" Version="*" />
    <PackageReference Include="System.Text.Json" Version="8.*" />
  </ItemGroup>
</Project>
```

创建 `tools/ground-truth-exporter/Program.cs`:

```csharp
using System;
using System.IO;
using System.Text.Json;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader;

class Program
{
    static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    static void Main(string[] args)
    {
        if (args.Length < 1) { Console.Error.WriteLine("Usage: GroundTruthExporter <input.raw> [output_dir]"); return; }

        string rawPath = args[0];
        string outDir = args.Length > 1 ? args[1] : Path.Combine(Path.GetDirectoryName(rawPath)!, "ground_truth");
        Directory.CreateDirectory(outDir);

        using var raw = RawFileReaderAdapter.FileFactory(rawPath);
        raw.SelectInstrument(Device.MS, 1);

        var header = raw.RunHeaderEx;
        int firstScan = header.FirstSpectrum;
        int lastScan = header.LastSpectrum;

        // 1. File metadata
        var metadata = new
        {
            FileName = Path.GetFileName(rawPath),
            CreationDate = raw.FileHeader.CreationDate.ToString("o"),
            InstrumentModel = raw.GetInstrumentData().Model,
            InstrumentName = raw.GetInstrumentData().Name,
            SerialNumber = raw.GetInstrumentData().SerialNumber,
            SoftwareVersion = raw.GetInstrumentData().SoftwareVersion,
            SampleName = raw.SampleInformation.SampleName,
            Comment = raw.SampleInformation.Comment,
            FirstScan = firstScan,
            LastScan = lastScan,
            StartTime = header.StartTime,
            EndTime = header.EndTime,
            LowMass = header.LowMass,
            HighMass = header.HighMass,
            MassResolution = header.MassResolution,
        };
        File.WriteAllText(Path.Combine(outDir, "metadata.json"), JsonSerializer.Serialize(metadata, JsonOpts));

        // 2. Scan index (lightweight: RT + basic info per scan)
        var scanIndex = new List<object>();
        for (int scan = firstScan; scan <= lastScan; scan++)
        {
            var filter = raw.GetFilterForScanNumber(scan);
            var stats = raw.GetScanStatsForScanNumber(scan);
            scanIndex.Add(new
            {
                ScanNumber = scan,
                Rt = Math.Round(stats.StartTime, 6),
                MsLevel = (int)filter.MSOrder,
                Polarity = filter.Polarity.ToString(),
                ScanMode = filter.ScanMode.ToString(),
                MassAnalyzer = filter.MassAnalyzer.ToString(),
                TIC = stats.TIC,
                BasePeakMz = stats.BasePeakMass,
                BasePeakIntensity = stats.BasePeakIntensity,
                LowMass = stats.LowMass,
                HighMass = stats.HighMass,
                FilterString = filter.ToString(),
            });
        }
        File.WriteAllText(Path.Combine(outDir, "scan_index.json"), JsonSerializer.Serialize(scanIndex, JsonOpts));

        // 3. Per-scan centroid data (每个 scan 一个文件)
        string scansDir = Path.Combine(outDir, "scans");
        Directory.CreateDirectory(scansDir);

        for (int scan = firstScan; scan <= lastScan; scan++)
        {
            // Centroid
            var centroid = raw.GetCentroidStream(scan, false);
            double[]? centroidMz = null, centroidIntensity = null;
            if (centroid != null && centroid.Length > 0)
            {
                centroidMz = centroid.Masses;
                centroidIntensity = centroid.Intensities;
            }

            // Profile (如果可用)
            var segScan = raw.GetSegmentedScanFromScanNumber(scan, null);
            double[]? profileMz = null, profileIntensity = null;
            if (segScan != null && segScan.Positions != null && segScan.Positions.Length > 0)
            {
                profileMz = segScan.Positions;
                profileIntensity = segScan.Intensities;
            }

            var scanData = new
            {
                ScanNumber = scan,
                CentroidCount = centroidMz?.Length ?? 0,
                CentroidMz = centroidMz,
                CentroidIntensity = centroidIntensity,
                ProfileCount = profileMz?.Length ?? 0,
                ProfileMz = profileMz,
                ProfileIntensity = profileIntensity,
            };

            string scanFile = Path.Combine(scansDir, $"scan_{scan:D5}.json");
            File.WriteAllText(scanFile, JsonSerializer.Serialize(scanData, JsonOpts));

            if (scan % 500 == 0) Console.Error.WriteLine($"  Exported scan {scan}/{lastScan}");
        }

        // 4. Trailer Extra (scan-level metadata)
        string trailerDir = Path.Combine(outDir, "trailer_extra");
        Directory.CreateDirectory(trailerDir);

        // 获取 trailer extra header labels
        var trailerFields = raw.GetTrailerExtraHeaderInformation();
        var fieldLabels = trailerFields.Select(f => f.Label.Trim(':',' ')).ToArray();
        File.WriteAllText(Path.Combine(outDir, "trailer_fields.json"), JsonSerializer.Serialize(fieldLabels, JsonOpts));

        for (int scan = firstScan; scan <= lastScan; scan++)
        {
            var trailer = raw.GetTrailerExtraInformation(scan);
            var dict = new Dictionary<string, string>();
            for (int i = 0; i < trailer.Labels.Length; i++)
            {
                dict[trailer.Labels[i].Trim(':', ' ')] = trailer.Values[i]?.Trim() ?? "";
            }
            string trailerFile = Path.Combine(trailerDir, $"scan_{scan:D5}.json");
            File.WriteAllText(trailerFile, JsonSerializer.Serialize(dict, JsonOpts));
        }

        // 5. Chromatograms
        string chromDir = Path.Combine(outDir, "chromatograms");
        Directory.CreateDirectory(chromDir);

        // TIC
        var settings = new ChromatogramTraceSettings(TraceType.TIC);
        var ticData = raw.GetChromatogramData(new[] { settings }, firstScan, lastScan);
        if (ticData != null)
        {
            var tic = ChromatogramSignal.FromChromatogramData(ticData);
            if (tic != null && tic.Length > 0)
            {
                File.WriteAllText(Path.Combine(chromDir, "tic.json"), JsonSerializer.Serialize(new
                {
                    Rt = tic[0].Times,
                    Intensity = tic[0].Intensities,
                }, JsonOpts));
            }
        }

        // BasePeak
        var bpSettings = new ChromatogramTraceSettings(TraceType.BasePeak);
        var bpData = raw.GetChromatogramData(new[] { bpSettings }, firstScan, lastScan);
        if (bpData != null)
        {
            var bp = ChromatogramSignal.FromChromatogramData(bpData);
            if (bp != null && bp.Length > 0)
            {
                File.WriteAllText(Path.Combine(chromDir, "bpc.json"), JsonSerializer.Serialize(new
                {
                    Rt = bp[0].Times,
                    Intensity = bp[0].Intensities,
                }, JsonOpts));
            }
        }

        Console.Error.WriteLine($"Ground truth exported to {outDir}");
        Console.Error.WriteLine($"  Scans: {firstScan}-{lastScan} ({lastScan - firstScan + 1} total)");
    }
}
```

使用方式：

```bash
cd tools/ground-truth-exporter
dotnet run -- /path/to/sample.raw /path/to/output/ground_truth
```

### Task 0.3: 反编译 Thermo DLL

创建 `tools/decompiler/decompile.sh`:

```bash
#!/bin/bash
# 反编译 Thermo CommonCore DLL，输出格式规范参考
# 前置：dotnet tool install -g ilspycmd

set -euo pipefail

DLL_DIR="${1:?Usage: decompile.sh <dll_directory> [output_dir]}"
OUT_DIR="${2:-./decompiled}"

mkdir -p "$OUT_DIR"

# 核心 DLL 列表（按分析优先级排序）
DLLS=(
    "ThermoFisher.CommonCore.RawFileReader.dll"     # ★★★★★ 核心读取逻辑
    "ThermoFisher.CommonCore.Data.dll"               # ★★★★★ 数据模型/结构定义
    "ThermoFisher.CommonCore.MassPrecisionEstimator.dll"  # ★★ 精度相关
    "ThermoFisher.CommonCore.BackgroundSubtraction.dll"   # ★ 后处理
)

for dll in "${DLLS[@]}"; do
    dllpath="$DLL_DIR/$dll"
    if [ -f "$dllpath" ]; then
        name="${dll%.dll}"
        echo "Decompiling $dll → $OUT_DIR/$name/"
        ilspycmd "$dllpath" -p -o "$OUT_DIR/$name/" 2>/dev/null || \
            echo "  Warning: ilspycmd failed for $dll, trying with --nested-directories"
        echo "  Done. $(find "$OUT_DIR/$name/" -name '*.cs' | wc -l) files"
    else
        echo "Skipping $dll (not found in $DLL_DIR)"
    fi
done

echo ""
echo "=== 反编译完成 ==="
echo "重点关注文件（按逆向优先级）："
echo "  1. $OUT_DIR/ThermoFisher.CommonCore.RawFileReader/  → 文件读取核心逻辑"
echo "  2. $OUT_DIR/ThermoFisher.CommonCore.Data/           → 结构体定义"
echo ""
echo "搜索关键类/方法："
echo "  grep -rn 'ReadScanData\|ScanDataPacket\|ProfileChunk' $OUT_DIR/"
echo "  grep -rn 'RunHeader\|ScanIndex\|TrailerExtra' $OUT_DIR/"
echo "  grep -rn 'OleStream\|CompoundFile\|ReadStream' $OUT_DIR/"
```

### Task 0.4: Hex Analyzer 工具

创建 `tools/hex-analyzer/src/main.rs`:

```rust
use std::fs;
use std::path::PathBuf;
use clap::{Parser, Subcommand};

#[derive(Parser)]
#[command(name = "hex-analyzer", about = "Thermo RAW file reverse engineering helper")]
struct Cli {
    #[command(subcommand)]
    command: Commands,
}

#[derive(Subcommand)]
enum Commands {
    /// 在文件中搜索 f64 值（IEEE 754 little-endian）
    SearchF64 {
        file: PathBuf,
        value: f64,
        #[arg(long, default_value = "1e-9")]
        tolerance: f64,
        /// 限制搜索范围: offset:length
        #[arg(long)]
        range: Option<String>,
    },

    /// 在文件中搜索 f32 值
    SearchF32 {
        file: PathBuf,
        value: f32,
        #[arg(long, default_value = "1e-5")]
        tolerance: f32,
    },

    /// 在文件中搜索 u32 值
    SearchU32 {
        file: PathBuf,
        value: u32,
    },

    /// 在文件中搜索 UTF-16LE 字符串
    SearchUtf16 {
        file: PathBuf,
        pattern: String,
    },

    /// Hex dump 指定区域
    Dump {
        file: PathBuf,
        #[arg(long)]
        offset: usize,
        #[arg(long, default_value = "256")]
        length: usize,
        /// 同时显示各种类型的解码尝试
        #[arg(long)]
        interpret: bool,
    },

    /// 检测重复结构的 stride（给定已知 f64 值序列）
    DetectStride {
        file: PathBuf,
        /// 逗号分隔的 f64 值，如 "0.0032,0.0098,0.0164"
        values: String,
        #[arg(long, default_value = "1e-9")]
        tolerance: f64,
    },

    /// 两个文件的 binary diff（高亮差异区域）
    Diff {
        file_a: PathBuf,
        file_b: PathBuf,
        /// 最大显示差异数
        #[arg(long, default_value = "50")]
        max_diffs: usize,
    },

    /// 给定 ground truth JSON，自动定位字段在 binary 中的位置
    AutoLocate {
        /// RAW 文件路径
        raw_file: PathBuf,
        /// ground truth 目录（包含 scan_index.json, metadata.json 等）
        truth_dir: PathBuf,
    },
}

fn search_f64(data: &[u8], target: f64, tolerance: f64) -> Vec<(usize, f64)> {
    let mut hits = Vec::new();
    if data.len() < 8 { return hits; }
    for i in 0..=data.len() - 8 {
        let val = f64::from_le_bytes(data[i..i + 8].try_into().unwrap());
        if val.is_finite() && (val - target).abs() <= tolerance.max(target.abs() * tolerance) {
            hits.push((i, val));
        }
    }
    hits
}

fn search_f32(data: &[u8], target: f32, tolerance: f32) -> Vec<(usize, f32)> {
    let mut hits = Vec::new();
    if data.len() < 4 { return hits; }
    for i in 0..=data.len() - 4 {
        let val = f32::from_le_bytes(data[i..i + 4].try_into().unwrap());
        if val.is_finite() && (val - target).abs() <= tolerance.max(target.abs() * tolerance) {
            hits.push((i, val));
        }
    }
    hits
}

fn search_u32(data: &[u8], target: u32) -> Vec<usize> {
    let target_bytes = target.to_le_bytes();
    let mut hits = Vec::new();
    if data.len() < 4 { return hits; }
    for i in 0..=data.len() - 4 {
        if data[i..i + 4] == target_bytes {
            hits.push(i);
        }
    }
    hits
}

fn search_utf16le(data: &[u8], pattern: &str) -> Vec<usize> {
    let encoded: Vec<u8> = pattern.encode_utf16().flat_map(|c| c.to_le_bytes()).collect();
    let mut hits = Vec::new();
    if data.len() < encoded.len() { return hits; }
    for i in 0..=data.len() - encoded.len() {
        if data[i..i + encoded.len()] == encoded[..] {
            hits.push(i);
        }
    }
    hits
}

fn hex_dump(data: &[u8], offset: usize, length: usize, interpret: bool) {
    let end = (offset + length).min(data.len());
    let slice = &data[offset..end];

    for (i, chunk) in slice.chunks(16).enumerate() {
        let addr = offset + i * 16;
        print!("{:08X}  ", addr);
        for (j, byte) in chunk.iter().enumerate() {
            print!("{:02X} ", byte);
            if j == 7 { print!(" "); }
        }
        // padding
        for _ in chunk.len()..16 {
            print!("   ");
        }
        print!(" |");
        for byte in chunk {
            let c = if *byte >= 0x20 && *byte < 0x7F { *byte as char } else { '.' };
            print!("{}", c);
        }
        println!("|");

        if interpret && chunk.len() >= 8 {
            let pos = offset + i * 16;
            if pos + 8 <= data.len() {
                let f64_val = f64::from_le_bytes(data[pos..pos + 8].try_into().unwrap());
                let f32_val = f32::from_le_bytes(data[pos..pos + 4].try_into().unwrap());
                let u32_val = u32::from_le_bytes(data[pos..pos + 4].try_into().unwrap());
                let i32_val = i32::from_le_bytes(data[pos..pos + 4].try_into().unwrap());
                if f64_val.is_finite() && f64_val.abs() < 1e15 && f64_val.abs() > 1e-15 {
                    println!("          → f64: {:.10}", f64_val);
                }
                if f32_val.is_finite() && f32_val.abs() < 1e10 && f32_val.abs() > 1e-10 {
                    println!("          → f32: {:.6}", f32_val);
                }
                println!("          → u32: {}  i32: {}", u32_val, i32_val);
            }
        }
    }
}

fn detect_stride(data: &[u8], values: &[f64], tolerance: f64) -> Vec<(usize, usize, usize)> {
    // 搜索第一个值的所有命中
    let first_hits = search_f64(data, values[0], tolerance);
    let mut results = Vec::new();

    for (hit_offset, _) in &first_hits {
        // 对于每个第一值命中，尝试不同 stride
        for stride in (8..=256).step_by(4) {
            let mut all_match = true;
            for (vi, &val) in values.iter().enumerate().skip(1) {
                let expected_offset = hit_offset + vi * stride;
                if expected_offset + 8 > data.len() {
                    all_match = false;
                    break;
                }
                let found = f64::from_le_bytes(
                    data[expected_offset..expected_offset + 8].try_into().unwrap(),
                );
                if (found - val).abs() > tolerance.max(val.abs() * tolerance) {
                    all_match = false;
                    break;
                }
            }
            if all_match {
                results.push((*hit_offset, stride, values.len()));
            }
        }
    }
    results
}

fn main() {
    let cli = Cli::parse();

    match cli.command {
        Commands::SearchF64 { file, value, tolerance, range } => {
            let data = fs::read(&file).expect("Failed to read file");
            let (search_data, base_offset) = if let Some(r) = range {
                let parts: Vec<usize> = r.split(':').map(|s| s.parse().unwrap()).collect();
                let start = parts[0];
                let len = parts.get(1).copied().unwrap_or(data.len() - start);
                (&data[start..start + len], start)
            } else {
                (&data[..], 0)
            };
            let hits = search_f64(search_data, value, tolerance);
            println!("Searching for f64 {:.10} (±{}) in {}:", value, tolerance, file.display());
            println!("Found {} hits:", hits.len());
            for (offset, val) in &hits {
                println!("  offset 0x{:08X} ({:>10}): {:.15}", offset + base_offset, offset + base_offset, val);
            }
        }

        Commands::SearchF32 { file, value, tolerance } => {
            let data = fs::read(&file).expect("Failed to read file");
            let hits = search_f32(&data, value, tolerance);
            println!("Found {} hits for f32 {:.6}:", hits.len(), value);
            for (offset, val) in &hits {
                println!("  offset 0x{:08X}: {:.10}", offset, val);
            }
        }

        Commands::SearchU32 { file, value } => {
            let data = fs::read(&file).expect("Failed to read file");
            let hits = search_u32(&data, value);
            println!("Found {} hits for u32 {}:", hits.len(), value);
            for offset in &hits {
                println!("  offset 0x{:08X} ({:>10})", offset, offset);
            }
        }

        Commands::SearchUtf16 { file, pattern } => {
            let data = fs::read(&file).expect("Failed to read file");
            let hits = search_utf16le(&data, &pattern);
            println!("Found {} hits for UTF-16LE \"{}\":", hits.len(), pattern);
            for offset in &hits {
                println!("  offset 0x{:08X}", offset);
            }
        }

        Commands::Dump { file, offset, length, interpret } => {
            let data = fs::read(&file).expect("Failed to read file");
            hex_dump(&data, offset, length, interpret);
        }

        Commands::DetectStride { file, values, tolerance } => {
            let data = fs::read(&file).expect("Failed to read file");
            let vals: Vec<f64> = values.split(',').map(|s| s.trim().parse().unwrap()).collect();
            println!("Detecting stride for {} values: {:?}", vals.len(), vals);
            let results = detect_stride(&data, &vals, tolerance);
            if results.is_empty() {
                println!("No stride pattern found.");
            } else {
                println!("Found {} candidate(s):", results.len());
                for (offset, stride, matched) in &results {
                    println!(
                        "  base_offset=0x{:08X}  stride={} bytes  ({} values matched)",
                        offset, stride, matched
                    );
                }
            }
        }

        Commands::Diff { file_a, file_b, max_diffs } => {
            let a = fs::read(&file_a).expect("Failed to read file A");
            let b = fs::read(&file_b).expect("Failed to read file B");
            let min_len = a.len().min(b.len());
            println!("File A: {} ({} bytes)", file_a.display(), a.len());
            println!("File B: {} ({} bytes)", file_b.display(), b.len());

            let mut diff_count = 0;
            let mut in_diff = false;
            let mut diff_start = 0;

            for i in 0..min_len {
                if a[i] != b[i] {
                    if !in_diff {
                        diff_start = i;
                        in_diff = true;
                    }
                } else if in_diff {
                    let diff_len = i - diff_start;
                    println!("  DIFF at 0x{:08X}-0x{:08X} ({} bytes)", diff_start, i - 1, diff_len);
                    diff_count += 1;
                    in_diff = false;
                    if diff_count >= max_diffs { println!("  ... (max diffs reached)"); break; }
                }
            }
            if in_diff {
                println!("  DIFF at 0x{:08X}-0x{:08X} ({} bytes)", diff_start, min_len - 1, min_len - diff_start);
            }
            if a.len() != b.len() {
                println!("  SIZE DIFF: A={} B={} (delta={})", a.len(), b.len(), (a.len() as i64 - b.len() as i64).abs());
            }
        }

        Commands::AutoLocate { raw_file, truth_dir } => {
            let data = fs::read(&raw_file).expect("Failed to read RAW file");

            // 加载 scan_index.json 中前几个 scan 的 RT
            let index_path = truth_dir.join("scan_index.json");
            let index_str = fs::read_to_string(&index_path).expect("Failed to read scan_index.json");
            let index: serde_json::Value = serde_json::from_str(&index_str).unwrap();
            let scans = index.as_array().unwrap();

            // 提取前 10 个 scan 的 RT
            let rts: Vec<f64> = scans.iter().take(10).filter_map(|s| s["rt"].as_f64()).collect();
            println!("=== Auto-locating fields in {} ===", raw_file.display());
            println!("Using first {} RT values: {:?}", rts.len(), &rts[..3.min(rts.len())]);

            // 1. RT stride detection
            println!("\n--- Retention Time Stride Detection ---");
            let stride_results = detect_stride(&data, &rts, 1e-9);
            for (offset, stride, matched) in &stride_results {
                println!("  ✓ RT field at offset 0x{:08X}, stride {} bytes, {} matched", offset, stride, matched);
            }

            // 2. TIC values
            let first_tic = scans.first().and_then(|s| s["tic"].as_f64());
            if let Some(tic) = first_tic {
                println!("\n--- TIC Search (first scan: {:.2}) ---", tic);
                let hits = search_f64(&data, tic, tic * 1e-6);
                for (offset, val) in hits.iter().take(10) {
                    println!("  hit at 0x{:08X}: {:.6}", offset, val);
                }
            }

            // 3. Scan count
            let n_scans = scans.len() as u32;
            println!("\n--- Scan Count ({}) ---", n_scans);
            let hits = search_u32(&data, n_scans);
            for offset in hits.iter().take(20) {
                println!("  hit at 0x{:08X}", offset);
            }

            // 4. Metadata strings
            let meta_path = truth_dir.join("metadata.json");
            if let Ok(meta_str) = fs::read_to_string(&meta_path) {
                let meta: serde_json::Value = serde_json::from_str(&meta_str).unwrap();
                for key in &["instrumentModel", "sampleName", "serialNumber"] {
                    if let Some(val) = meta[key].as_str() {
                        if !val.is_empty() {
                            println!("\n--- UTF-16LE search: {} = \"{}\" ---", key, val);
                            let hits = search_utf16le(&data, val);
                            for offset in hits.iter().take(5) {
                                println!("  hit at 0x{:08X}", offset);
                            }
                        }
                    }
                }
            }
        }
    }
}
```

Cargo.toml for hex-analyzer:

```toml
[package]
name = "hex-analyzer"
version = "0.1.0"
edition = "2021"

[dependencies]
clap = { workspace = true }
serde_json = { workspace = true }
serde = { workspace = true }
```

### Task 0.5: 验证框架

创建 `crates/thermo-raw/src/validation.rs`（编译时仅在 test/bench 中使用）:

```rust
//! Ground truth validation framework.
//! Loads JSON exported by C# GroundTruthExporter and compares against Rust parser output.

use serde::Deserialize;
use std::path::Path;

#[derive(Debug, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct GroundTruthScanIndex {
    pub scan_number: u32,
    pub rt: f64,
    pub ms_level: u8,
    pub polarity: String,
    pub tic: f64,
    pub base_peak_mz: f64,
    pub base_peak_intensity: f64,
    pub filter_string: String,
}

#[derive(Debug, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct GroundTruthScanData {
    pub scan_number: u32,
    pub centroid_count: usize,
    pub centroid_mz: Option<Vec<f64>>,
    pub centroid_intensity: Option<Vec<f64>>,
    pub profile_count: usize,
    pub profile_mz: Option<Vec<f64>>,
    pub profile_intensity: Option<Vec<f64>>,
}

#[derive(Debug)]
pub struct ValidationResult {
    pub scan_number: u32,
    pub passed: bool,
    pub mz_max_error_ppm: f64,
    pub mz_mean_error_ppm: f64,
    pub intensity_max_relative_error: f64,
    pub rt_error_seconds: f64,
    pub peak_count_match: bool,
    pub errors: Vec<String>,
}

#[derive(Debug)]
pub struct FileValidationReport {
    pub total_scans: u32,
    pub passed_scans: u32,
    pub failed_scans: u32,
    pub pass_rate: f64,
    pub worst_mz_error_ppm: f64,
    pub worst_intensity_error: f64,
    pub failures: Vec<ValidationResult>,
}

/// 验收标准
pub struct ValidationCriteria {
    pub mz_tolerance_ppm: f64,         // 默认 0.1 ppm
    pub intensity_rel_tolerance: f64,  // 默认 1e-6
    pub rt_tolerance_minutes: f64,     // 默认 0.001 min
}

impl Default for ValidationCriteria {
    fn default() -> Self {
        Self {
            mz_tolerance_ppm: 0.1,
            intensity_rel_tolerance: 1e-6,
            rt_tolerance_minutes: 0.001,
        }
    }
}

pub fn load_scan_index(truth_dir: &Path) -> Vec<GroundTruthScanIndex> {
    let path = truth_dir.join("scan_index.json");
    let data = std::fs::read_to_string(path).expect("Failed to read scan_index.json");
    serde_json::from_str(&data).expect("Failed to parse scan_index.json")
}

pub fn load_scan_data(truth_dir: &Path, scan_number: u32) -> GroundTruthScanData {
    let path = truth_dir.join("scans").join(format!("scan_{:05}.json", scan_number));
    let data = std::fs::read_to_string(path).expect("Failed to read scan data");
    serde_json::from_str(&data).expect("Failed to parse scan data")
}

pub fn validate_mz_arrays(
    parsed: &[f64],
    truth: &[f64],
    tolerance_ppm: f64,
) -> (f64, f64, Vec<String>) {
    let mut max_error = 0.0_f64;
    let mut sum_error = 0.0_f64;
    let mut errors = Vec::new();

    if parsed.len() != truth.len() {
        errors.push(format!("Peak count mismatch: parsed={} truth={}", parsed.len(), truth.len()));
        return (f64::INFINITY, f64::INFINITY, errors);
    }

    for (i, (p, t)) in parsed.iter().zip(truth.iter()).enumerate() {
        let error_ppm = if *t != 0.0 { ((p - t) / t).abs() * 1e6 } else { 0.0 };
        max_error = max_error.max(error_ppm);
        sum_error += error_ppm;
        if error_ppm > tolerance_ppm {
            errors.push(format!("Peak {}: mz parsed={:.8} truth={:.8} error={:.4} ppm", i, p, t, error_ppm));
        }
    }

    let mean_error = if !truth.is_empty() { sum_error / truth.len() as f64 } else { 0.0 };
    (max_error, mean_error, errors)
}
```

---

## Phase 1: OLE2/CFBF 容器层

### 背景

Thermo RAW 文件使用 Microsoft Compound Binary File Format (CFBF/OLE2)。这个容器格式完全公开，规范见 [MS-CFB]。

### Task 1.1: cfb-reader crate 实现

可选方案：
- **方案 A**：使用现有 crate `cfb = "0.10"`（推荐，节省时间）
- **方案 B**：从零实现（更可控，但 OLE2 不是重点）

推荐方案 A，Cargo.toml:

```toml
[package]
name = "cfb-reader"
version = "0.1.0"
edition = "2021"

[dependencies]
cfb = "0.10"
```

封装层 `src/lib.rs`:

```rust
//! Thin wrapper around the `cfb` crate for Thermo RAW OLE2 container access.

use cfb::CompoundFile;
use std::io::{Read, Seek};
use std::path::Path;

pub struct Ole2Container<F: Read + Seek> {
    cf: CompoundFile<F>,
}

impl Ole2Container<std::fs::File> {
    pub fn open(path: impl AsRef<Path>) -> Result<Self, cfb::Error> {
        let cf = CompoundFile::open(path)?;
        Ok(Self { cf })
    }
}

impl<F: Read + Seek> Ole2Container<F> {
    /// 列出所有 stream 路径
    pub fn list_streams(&self) -> Vec<String> {
        self.cf
            .walk()
            .filter(|e| !e.is_storage())
            .map(|e| e.path().to_string_lossy().into_owned())
            .collect()
    }

    /// 读取指定 stream 的全部内容
    pub fn read_stream(&mut self, path: &str) -> Result<Vec<u8>, cfb::Error> {
        let mut stream = self.cf.open_stream(path)?;
        let mut buf = Vec::new();
        stream.read_to_end(&mut buf)?;
        Ok(buf)
    }

    /// 获取 stream 大小
    pub fn stream_len(&self, path: &str) -> Option<u64> {
        self.cf.entry(path).ok().map(|e| e.len())
    }
}
```

### Task 1.2: 验证 OLE2 读取

用 Python olefile 做交叉验证：

```python
# scripts/verify_ole2.py
import olefile
import json
import sys

raw_path = sys.argv[1]
ole = olefile.OleFileIO(raw_path)

entries = []
for entry in ole.listdir():
    path = "/".join(entry)
    try:
        size = ole.get_size(path)
    except:
        size = -1
    entries.append({"path": path, "size": size})

json.dump(entries, sys.stdout, indent=2)
```

**Phase 1 完成标志**：Rust 和 Python 列出的 stream 名称和大小完全一致。

---

## Phase 2: 反编译分析与格式规范文档

### Task 2.1: 获取并反编译 DLL

```bash
# 从 NuGet 下载（或从 repo 的 Libs 目录获取）
# Libs/Net8.0/ 或 Libs/Net471/ 中有 DLL 文件

# 安装 ILSpy CLI
dotnet tool install -g ilspycmd

# 执行反编译
bash tools/decompiler/decompile.sh /path/to/Libs/Net8.0/ ./decompiled/
```

### Task 2.2: 分析反编译代码，编写 FORMAT_SPEC.md

这是最关键的智力劳动步骤。在反编译代码中重点搜索：

```bash
# 文件头与版本
grep -rn 'FileHeader\|RawFileVersion\|MagicNumber' decompiled/

# RunHeader 结构
grep -rn 'RunHeader\|HeaderInfo\|FirstSpectrum\|LastSpectrum\|StartTime\|EndTime' decompiled/

# Scan index / offset table
grep -rn 'ScanIndex\|ScanOffset\|IndexEntry\|PacketOffset' decompiled/

# Scan data 解码（核心难点）
grep -rn 'ScanData\|ReadSpectrum\|ProfileData\|CentroidData\|PacketHeader' decompiled/
grep -rn 'Decompress\|Compress\|Encode\|Decode\|Unpack' decompiled/

# Trailer extra
grep -rn 'TrailerExtra\|ExtraHeader\|ExtraInfo' decompiled/

# 精度标志
grep -rn 'DoublePrecision\|SinglePrecision\|DataType\|Layout' decompiled/
```

产出 `docs/FORMAT_SPEC.md`，格式参考：

```markdown
# Thermo RAW Binary Format Specification

## 1. Container Layer
- OLE2/CFBF container (MS-CFB specification)
- Known stream names: [从反编译代码中提取]

## 2. File Header
- Stream: [stream name]
- Structure:
  | Offset | Type   | Field          | Description |
  |--------|--------|----------------|-------------|
  | 0      | u32    | magic          | ...         |
  | 4      | u32    | version        | ...         |
  ...

## 3. Run Header
[同上格式]

## 4. Scan Index
[同上格式]

## 5. Scan Data Packet
[同上格式，特别注明压缩/编码方式]

## 6. Trailer Extra
[同上格式]

## 7. Version Differences
[不同 RAW 版本的结构差异]
```

### Task 2.3: 使用 hex-analyzer 交叉验证

对照反编译得出的结构定义，用 hex-analyzer 在实际 RAW 文件上验证：

```bash
# 1. 导出 ground truth
cd tools/ground-truth-exporter && dotnet run -- /path/to/sample.raw /tmp/gt/

# 2. 自动定位字段
cargo run -p hex-analyzer -- auto-locate /path/to/sample.raw /tmp/gt/

# 3. 按反编译得到的 offset 做 hex dump 验证
cargo run -p hex-analyzer -- dump /path/to/sample.raw --offset 0x1234 --length 128 --interpret

# 4. 用连续 RT 值验证 stride
cargo run -p hex-analyzer -- detect-stride /path/to/sample.raw "0.0032,0.0098,0.0164,0.0230"
```

---

## Phase 3: Rust 核心实现

### 重要：Clean-room 原则

Rust 实现只参考 Phase 2 产出的 `docs/FORMAT_SPEC.md`，不参考反编译 C# 源码。确保 clean-room 隔离。

### Task 3.1: 公共类型定义

`crates/thermo-raw/src/types.rs`:

```rust
/// 质谱极性
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Polarity {
    Positive,
    Negative,
    Unknown,
}

/// MS 级别
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum MsLevel {
    Ms1,
    Ms2,
    Ms3,
    Other(u8),
}

/// 单个 scan 的完整数据
#[derive(Debug, Clone)]
pub struct Scan {
    pub scan_number: u32,
    pub rt: f64,                           // minutes
    pub ms_level: MsLevel,
    pub polarity: Polarity,
    pub tic: f64,
    pub base_peak_mz: f64,
    pub base_peak_intensity: f64,
    pub centroid_mz: Vec<f64>,
    pub centroid_intensity: Vec<f64>,
    pub profile_mz: Option<Vec<f64>>,
    pub profile_intensity: Option<Vec<f64>>,
    pub precursor: Option<PrecursorInfo>,
    pub filter_string: Option<String>,
}

/// MS2+ 前体离子信息
#[derive(Debug, Clone)]
pub struct PrecursorInfo {
    pub mz: f64,
    pub charge: Option<i32>,
    pub isolation_width: Option<f64>,
    pub activation_type: Option<String>,
    pub collision_energy: Option<f64>,
}

/// 色谱图
#[derive(Debug, Clone)]
pub struct Chromatogram {
    pub rt: Vec<f64>,
    pub intensity: Vec<f64>,
}

/// 文件元信息
#[derive(Debug, Clone)]
pub struct FileMetadata {
    pub creation_date: String,
    pub instrument_model: String,
    pub instrument_name: String,
    pub serial_number: String,
    pub software_version: String,
    pub sample_name: String,
    pub comment: String,
}
```

### Task 3.2: 顶层 API

`crates/thermo-raw/src/lib.rs`:

```rust
pub mod types;
pub mod error;
mod raw_file;
mod version;
mod run_header;
mod scan_index;
mod scan_data;
mod scan_data_profile;
mod scan_data_centroid;
mod trailer;
mod chromatogram;
mod metadata;
mod scan_filter;

#[cfg(test)]
mod validation;

pub use raw_file::RawFile;
pub use types::*;
pub use error::RawError;
```

`crates/thermo-raw/src/raw_file.rs`:

```rust
use crate::*;
use memmap2::Mmap;
use std::path::Path;

pub struct RawFile {
    mmap: Mmap,
    // 以下字段在 open() 时解析填充
    version: u32,
    metadata: FileMetadata,
    run_header: run_header::RunHeader,
    scan_index: Vec<scan_index::ScanIndexEntry>,
    // OLE2 stream offsets (预计算，避免反复查找)
    scan_data_offset: usize,
    scan_data_len: usize,
}

impl RawFile {
    /// 打开 RAW 文件
    pub fn open(path: impl AsRef<Path>) -> Result<Self, RawError> {
        // 1. mmap 文件
        // 2. 解析 OLE2 容器，定位各 stream
        // 3. 解析 RunHeader
        // 4. 解析 ScanIndex
        // 5. 缓存 metadata
        todo!("Implement based on FORMAT_SPEC.md")
    }

    pub fn version(&self) -> u32 { self.version }
    pub fn metadata(&self) -> &FileMetadata { &self.metadata }
    pub fn n_scans(&self) -> u32 { self.scan_index.len() as u32 }
    pub fn first_scan(&self) -> u32 { self.run_header.first_scan }
    pub fn last_scan(&self) -> u32 { self.run_header.last_scan }
    pub fn start_time(&self) -> f64 { self.run_header.start_time }
    pub fn end_time(&self) -> f64 { self.run_header.end_time }

    /// 读取单个 scan
    pub fn scan(&self, scan_number: u32) -> Result<Scan, RawError> {
        let idx = (scan_number - self.run_header.first_scan) as usize;
        let entry = self.scan_index.get(idx)
            .ok_or(RawError::ScanOutOfRange(scan_number))?;
        scan_data::decode_scan(&self.mmap, self.scan_data_offset, entry, scan_number)
    }

    /// 批量并行读取
    pub fn scans_parallel(&self, range: std::ops::Range<u32>) -> Result<Vec<Scan>, RawError> {
        use rayon::prelude::*;
        let first = self.run_header.first_scan;
        let entries: Vec<_> = range
            .map(|n| ((n - first) as usize, n))
            .filter_map(|(idx, n)| self.scan_index.get(idx).map(|e| (e, n)))
            .collect();

        entries
            .par_iter()
            .map(|(entry, scan_num)| scan_data::decode_scan(&self.mmap, self.scan_data_offset, entry, *scan_num))
            .collect()
    }

    /// TIC 色谱图（从 scan index 快速提取，不需要读 scan data）
    pub fn tic(&self) -> Chromatogram {
        let rt: Vec<f64> = self.scan_index.iter().map(|e| e.rt).collect();
        let intensity: Vec<f64> = self.scan_index.iter().map(|e| e.tic).collect();
        Chromatogram { rt, intensity }
    }

    /// XIC 提取（需要读每个 scan 的 centroid data）
    pub fn xic(&self, target_mz: f64, tolerance_ppm: f64) -> Result<Chromatogram, RawError> {
        use rayon::prelude::*;
        let half_width = target_mz * tolerance_ppm * 1e-6;
        let low = target_mz - half_width;
        let high = target_mz + half_width;

        let results: Vec<(f64, f64)> = self.scan_index
            .par_iter()
            .enumerate()
            .map(|(idx, entry)| {
                let scan_num = self.run_header.first_scan + idx as u32;
                let scan = self.scan(scan_num).unwrap_or_else(|_| Scan {
                    scan_number: scan_num,
                    rt: entry.rt,
                    ms_level: MsLevel::Ms1,
                    polarity: Polarity::Unknown,
                    tic: 0.0,
                    base_peak_mz: 0.0,
                    base_peak_intensity: 0.0,
                    centroid_mz: vec![],
                    centroid_intensity: vec![],
                    profile_mz: None,
                    profile_intensity: None,
                    precursor: None,
                    filter_string: None,
                });
                let intensity: f64 = scan.centroid_mz.iter()
                    .zip(scan.centroid_intensity.iter())
                    .filter(|(&mz, _)| mz >= low && mz <= high)
                    .map(|(_, &int)| int)
                    .sum();
                (entry.rt, intensity)
            })
            .collect();

        Ok(Chromatogram {
            rt: results.iter().map(|(rt, _)| *rt).collect(),
            intensity: results.iter().map(|(_, int)| *int).collect(),
        })
    }
}
```

### Task 3.3: Error 类型

`crates/thermo-raw/src/error.rs`:

```rust
use thiserror::Error;

#[derive(Error, Debug)]
pub enum RawError {
    #[error("IO error: {0}")]
    Io(#[from] std::io::Error),

    #[error("Not a valid Thermo RAW file (OLE2 magic mismatch)")]
    NotRawFile,

    #[error("Unsupported RAW file version: {0}")]
    UnsupportedVersion(u32),

    #[error("Stream not found: {0}")]
    StreamNotFound(String),

    #[error("Scan {0} out of range")]
    ScanOutOfRange(u32),

    #[error("Failed to decode scan data at offset {offset}: {reason}")]
    ScanDecodeError { offset: usize, reason: String },

    #[error("Corrupted data: {0}")]
    CorruptedData(String),

    #[error("OLE2/CFBF error: {0}")]
    CfbError(String),
}
```

### Task 3.4: CLI 工具

`crates/thermo-raw-cli/src/main.rs`:

```rust
use clap::{Parser, Subcommand};
use std::path::PathBuf;
use thermo_raw::RawFile;

#[derive(Parser)]
#[command(name = "thermo-raw", about = "Thermo RAW file reader CLI")]
struct Cli {
    #[command(subcommand)]
    command: Commands,
}

#[derive(Subcommand)]
enum Commands {
    /// 显示 RAW 文件基本信息
    Info { file: PathBuf },

    /// 列出 OLE2 容器内的 streams
    Streams { file: PathBuf },

    /// 导出指定 scan 的数据为 JSON
    Scan {
        file: PathBuf,
        #[arg(short, long)]
        number: u32,
    },

    /// 导出 TIC 为 CSV
    Tic {
        file: PathBuf,
        #[arg(short, long)]
        output: Option<PathBuf>,
    },

    /// 导出 XIC 为 CSV
    Xic {
        file: PathBuf,
        #[arg(short, long)]
        mz: f64,
        #[arg(short, long, default_value = "5.0")]
        ppm: f64,
        #[arg(short, long)]
        output: Option<PathBuf>,
    },

    /// 与 ground truth 数据对比验证
    Validate {
        file: PathBuf,
        #[arg(short, long)]
        truth_dir: PathBuf,
    },

    /// 导出所有 scan 数据（性能测试用）
    Benchmark {
        file: PathBuf,
        #[arg(long)]
        parallel: bool,
    },
}

fn main() -> anyhow::Result<()> {
    let cli = Cli::parse();
    match cli.command {
        Commands::Info { file } => {
            let raw = RawFile::open(&file)?;
            let meta = raw.metadata();
            println!("File:        {}", file.display());
            println!("Version:     {}", raw.version());
            println!("Instrument:  {}", meta.instrument_model);
            println!("Sample:      {}", meta.sample_name);
            println!("Scans:       {}-{} ({} total)", raw.first_scan(), raw.last_scan(), raw.n_scans());
            println!("RT range:    {:.4}-{:.4} min", raw.start_time(), raw.end_time());
        }
        Commands::Benchmark { file, parallel } => {
            let raw = RawFile::open(&file)?;
            let start = std::time::Instant::now();
            if parallel {
                let scans = raw.scans_parallel(raw.first_scan()..raw.last_scan() + 1)?;
                let elapsed = start.elapsed();
                println!("{} scans read in {:.1}ms ({:.1} scans/sec)",
                    scans.len(), elapsed.as_secs_f64() * 1000.0,
                    scans.len() as f64 / elapsed.as_secs_f64());
            } else {
                let mut count = 0u32;
                for i in raw.first_scan()..=raw.last_scan() {
                    let _ = raw.scan(i)?;
                    count += 1;
                }
                let elapsed = start.elapsed();
                println!("{} scans read in {:.1}ms ({:.1} scans/sec)",
                    count, elapsed.as_secs_f64() * 1000.0,
                    count as f64 / elapsed.as_secs_f64());
            }
        }
        _ => { println!("Not implemented yet"); }
    }
    Ok(())
}
```

### Task 3.5: PyO3 Python Binding

`crates/thermo-raw-py/src/lib.rs`:

```rust
use pyo3::prelude::*;
use pyo3::exceptions::PyValueError;
use numpy::{PyArray1, IntoPyArray};
use thermo_raw::{RawFile as InnerRawFile, MsLevel};

#[pyclass]
struct RawFile {
    inner: InnerRawFile,
}

#[pymethods]
impl RawFile {
    #[new]
    fn new(path: &str) -> PyResult<Self> {
        let inner = InnerRawFile::open(path)
            .map_err(|e| PyValueError::new_err(format!("{}", e)))?;
        Ok(Self { inner })
    }

    #[getter]
    fn n_scans(&self) -> u32 { self.inner.n_scans() }

    #[getter]
    fn first_scan(&self) -> u32 { self.inner.first_scan() }

    #[getter]
    fn last_scan(&self) -> u32 { self.inner.last_scan() }

    #[getter]
    fn start_time(&self) -> f64 { self.inner.start_time() }

    #[getter]
    fn end_time(&self) -> f64 { self.inner.end_time() }

    #[getter]
    fn instrument_model(&self) -> String { self.inner.metadata().instrument_model.clone() }

    #[getter]
    fn sample_name(&self) -> String { self.inner.metadata().sample_name.clone() }

    /// 返回 (mz_array, intensity_array) numpy arrays
    fn scan<'py>(&self, py: Python<'py>, scan_number: u32)
        -> PyResult<(Bound<'py, PyArray1<f64>>, Bound<'py, PyArray1<f64>>)>
    {
        let scan = self.inner.scan(scan_number)
            .map_err(|e| PyValueError::new_err(format!("{}", e)))?;
        let mz = scan.centroid_mz.into_pyarray(py);
        let intensity = scan.centroid_intensity.into_pyarray(py);
        Ok((mz, intensity))
    }

    /// TIC: 返回 (rt_array, intensity_array)
    fn tic<'py>(&self, py: Python<'py>)
        -> (Bound<'py, PyArray1<f64>>, Bound<'py, PyArray1<f64>>)
    {
        let chrom = self.inner.tic();
        let rt = chrom.rt.into_pyarray(py);
        let intensity = chrom.intensity.into_pyarray(py);
        (rt, intensity)
    }

    /// XIC: 返回 (rt_array, intensity_array)
    fn xic<'py>(&self, py: Python<'py>, mz: f64, ppm: Option<f64>)
        -> PyResult<(Bound<'py, PyArray1<f64>>, Bound<'py, PyArray1<f64>>)>
    {
        let ppm = ppm.unwrap_or(5.0);
        let chrom = self.inner.xic(mz, ppm)
            .map_err(|e| PyValueError::new_err(format!("{}", e)))?;
        let rt = chrom.rt.into_pyarray(py);
        let intensity = chrom.intensity.into_pyarray(py);
        Ok((rt, intensity))
    }

    /// 批量读取所有 MS1 scan, 返回 list of (mz, intensity) tuples
    fn all_ms1_scans<'py>(&self, py: Python<'py>)
        -> PyResult<Vec<(Bound<'py, PyArray1<f64>>, Bound<'py, PyArray1<f64>>)>>
    {
        let first = self.inner.first_scan();
        let last = self.inner.last_scan();
        let scans = self.inner.scans_parallel(first..last + 1)
            .map_err(|e| PyValueError::new_err(format!("{}", e)))?;
        let results: Vec<_> = scans.into_iter()
            .filter(|s| matches!(s.ms_level, MsLevel::Ms1))
            .map(|s| {
                let mz = s.centroid_mz.into_pyarray(py);
                let int = s.centroid_intensity.into_pyarray(py);
                (mz, int)
            })
            .collect();
        Ok(results)
    }
}

#[pymodule]
fn thermo_raw(m: &Bound<'_, PyModule>) -> PyResult<()> {
    m.add_class::<RawFile>()?;
    Ok(())
}
```

`crates/thermo-raw-py/pyproject.toml`:

```toml
[build-system]
requires = ["maturin>=1.0,<2.0"]
build-backend = "maturin"

[project]
name = "thermo-raw"
version = "0.1.0"
description = "Pure Rust Thermo RAW file reader with Python bindings"
requires-python = ">=3.9"
dependencies = ["numpy>=1.20"]

[tool.maturin]
features = ["pyo3/extension-module"]
```

Python 使用示例：

```python
from thermo_raw import RawFile

raw = RawFile("sample.raw")
print(f"Instrument: {raw.instrument_model}")
print(f"Scans: {raw.n_scans}")

# 读取单个 scan
mz, intensity = raw.scan(1)

# TIC
rt, tic = raw.tic()

# XIC
rt, xic = raw.xic(mz=132.0773, ppm=5.0)

# 批量读取（并行）
all_ms1 = raw.all_ms1_scans()
```

---

## Phase 4: 验证与优化

### Task 4.1: 分级验证

```bash
# Level 0: OLE2 容器
cargo test -p cfb-reader

# Level 1: Scan index (RT, scan count, metadata)
cargo run -p thermo-raw-cli -- validate sample.raw --truth-dir ground_truth/

# Level 2: Scan data (m/z, intensity 精度)
cargo run -p thermo-raw-cli -- validate sample.raw --truth-dir ground_truth/ --strict

# Level 3: 性能 benchmark
cargo run --release -p thermo-raw-cli -- benchmark sample.raw --parallel
```

验收标准：

| 指标 | 通过条件 |
|------|---------|
| m/z 精度 | 所有 peak 误差 < 0.1 ppm |
| intensity 精度 | 相对误差 < 1e-6 |
| RT 精度 | 误差 < 0.001 min |
| peak 数量 | 与 ground truth 完全一致 |
| metadata 字符串 | 完全一致 |
| 全文件读取速度 | > 10x Thermo DLL via subprocess |

### Task 4.2: 多文件/多机型测试矩阵

```
test-data/raw-files/
├── orbitrap_exploris/         # 优先级 1（你最常用）
│   ├── pos_fullscan.raw
│   ├── neg_fullscan.raw
│   └── ddms2.raw
├── qexactive/                 # 优先级 2
│   └── standard_mix.raw
├── version_matrix/            # 优先级 3
│   ├── v57.raw
│   ├── v64.raw
│   └── v66.raw
└── edge_cases/                # 优先级 4
    ├── single_scan.raw
    └── empty_scans.raw
```

### Task 4.3: 性能优化 Checklist

- [ ] mmap 替代 read-to-memory（大文件 > 2GB）
- [ ] rayon 并行 scan 解码
- [ ] SIMD m/z 范围搜索（AVX2 f64 compare）
- [ ] scan data 按需解码（lazy，不提前 allocate 全部 scan）
- [ ] XIC 提取时 binary search on sorted m/z array（跳过全遍历）
- [ ] Profile data optional（默认不解码，按需开启）

---

## 实施路线图

```
Week 1-2:   Phase 0 (基础设施)
              ✓ Rust workspace 初始化
              ✓ Ground Truth Exporter C# 实现 + 导出测试数据
              ✓ Hex Analyzer 工具
              ✓ 反编译 Thermo DLL

Week 3:     Phase 1 (OLE2) + Phase 2 开始
              ✓ cfb-reader crate
              ✓ 开始分析反编译代码
              ✓ 编写 FORMAT_SPEC.md (RunHeader, ScanIndex)

Week 4:     Phase 2 完成
              ✓ FORMAT_SPEC.md 完成 (ScanData encoding)
              ✓ hex-analyzer auto-locate 验证

Week 5-6:   Phase 3 (Rust 核心实现)
              ✓ run_header, scan_index, metadata 解析
              ✓ scan_data centroid 解码
              ✓ scan_data profile 解码
              ✓ trailer extra

Week 7:     Phase 3 (API + CLI + PyO3)
              ✓ 顶层 API
              ✓ CLI 工具
              ✓ PyO3 binding

Week 8:     Phase 4 (验证 + 优化)
              ✓ 全量验证 pass rate > 99.9%
              ✓ 性能 benchmark
              ✓ 多机型测试
              ✓ 文档 + 发布 0.1.0
```

---

## 风险与缓解

| 风险 | 可能性 | 影响 | 缓解策略 |
|------|--------|------|---------|
| DLL 反编译被混淆，代码不可读 | 低 | 高 | 回退到 hex 逆向 + unfinnigan 参考 |
| ScanData 使用未知压缩 | 中 | 高 | 反编译应能看到解压逻辑；fallback: 多文件对比 |
| 新版 RAW (>v66) 结构变化大 | 低 | 中 | 先覆盖你常用版本，后续迭代 |
| Profile data 编码特别复杂 | 中 | 低 | 先只支持 centroid，metabolomics 主要用 centroid |
| Thermo License 反逆向条款 | 低 | 中 | clean-room 隔离；EU 互操作性例外 |
