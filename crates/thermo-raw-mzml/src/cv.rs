//! PSI-MS Controlled Vocabulary (CV) term constants for mzML output.
//!
//! Maps thermo-raw enums to their corresponding PSI-MS ontology accessions.
//! Reference: https://www.ebi.ac.uk/ols/ontologies/ms

#![allow(dead_code)]

use thermo_raw::scan_event::{ActivationType, AnalyzerType, IonizationType};
use thermo_raw::types::{MsLevel, Polarity};

// --- Ontology references ---
pub const CV_MS: &str = "MS";
pub const CV_UO: &str = "UO";

// --- Spectrum type ---
pub const MS1_SPECTRUM: &str = "MS:1000579";
pub const MSN_SPECTRUM: &str = "MS:1000580";
pub const MS_LEVEL: &str = "MS:1000511";

// --- Data representation ---
pub const CENTROID_SPECTRUM: &str = "MS:1000127";
pub const PROFILE_SPECTRUM: &str = "MS:1000128";

// --- Polarity ---
pub const POSITIVE_SCAN: &str = "MS:1000130";
pub const NEGATIVE_SCAN: &str = "MS:1000129";

// --- Scan properties ---
pub const SCAN_START_TIME: &str = "MS:1000016";
pub const TOTAL_ION_CURRENT: &str = "MS:1000285";
pub const BASE_PEAK_MZ: &str = "MS:1000504";
pub const BASE_PEAK_INTENSITY: &str = "MS:1000505";
pub const LOWEST_MZ: &str = "MS:1000528";
pub const HIGHEST_MZ: &str = "MS:1000527";

// --- Units ---
pub const MINUTE: &str = "UO:0000031";

// --- Binary data array ---
pub const MZ_ARRAY: &str = "MS:1000514";
pub const INTENSITY_ARRAY: &str = "MS:1000515";
pub const TIME_ARRAY: &str = "MS:1000595";
pub const FLOAT_64: &str = "MS:1000523";
pub const FLOAT_32: &str = "MS:1000521";
pub const ZLIB_COMPRESSION: &str = "MS:1000574";
pub const NO_COMPRESSION: &str = "MS:1000576";

// --- Chromatogram types ---
pub const TIC_CHROMATOGRAM: &str = "MS:1000235";
pub const BPC_CHROMATOGRAM: &str = "MS:1000628"; // basepeak chromatogram - selected ion current chromatogram

// --- Precursor/isolation ---
pub const SELECTED_ION_MZ: &str = "MS:1000744";
pub const CHARGE_STATE: &str = "MS:1000041";
pub const PEAK_INTENSITY: &str = "MS:1000042";
pub const ISOLATION_WINDOW_TARGET: &str = "MS:1000827";
pub const ISOLATION_WINDOW_LOWER: &str = "MS:1000828";
pub const ISOLATION_WINDOW_UPPER: &str = "MS:1000829";
pub const COLLISION_ENERGY: &str = "MS:1000045";

// --- Activation types ---
pub const ACTIVATION_CID: &str = "MS:1000133";
pub const ACTIVATION_HCD: &str = "MS:1000422";
pub const ACTIVATION_ETD: &str = "MS:1000598";
pub const ACTIVATION_ECD: &str = "MS:1000250";
pub const ACTIVATION_MPD: &str = "MS:1000435"; // photodissociation
pub const ACTIVATION_PQD: &str = "MS:1000599";
pub const ACTIVATION_UVPD: &str = "MS:1003246";

// --- Instrument / analyzer ---
pub const ORBITRAP: &str = "MS:1000484";
pub const ION_TRAP: &str = "MS:1000264";
pub const QUADRUPOLE: &str = "MS:1000081";
pub const TOF: &str = "MS:1000084";
pub const SECTOR: &str = "MS:1000080";

// --- Ionization ---
pub const ESI: &str = "MS:1000073";
pub const APCI: &str = "MS:1000070";
pub const MALDI: &str = "MS:1000075";
pub const EI: &str = "MS:1000389";
pub const CI: &str = "MS:1000386"; // chemical ionization
pub const NSI: &str = "MS:1000398"; // nanoelectrospray
pub const FAB: &str = "MS:1000074";

// --- Instrument configuration ---
pub const INSTRUMENT_MODEL: &str = "MS:1000031";
pub const INSTRUMENT_SERIAL: &str = "MS:1000529";
pub const THERMO_SCIENTIFIC: &str = "MS:1000483";

// --- Software ---
pub const DATA_PROCESSING: &str = "MS:1000544";
pub const CONVERSION_TO_MZML: &str = "MS:1000544";

// --- File content ---
pub const MS1_CONTENT: &str = "MS:1000579";
pub const MSN_CONTENT: &str = "MS:1000580";
pub const CENTROID_CONTENT: &str = "MS:1000127";

// --- File format ---
pub const THERMO_RAW_FORMAT: &str = "MS:1000563";

/// Map MS level to its CV accession value string.
pub fn ms_level_value(level: &MsLevel) -> &'static str {
    match level {
        MsLevel::Ms1 => "1",
        MsLevel::Ms2 => "2",
        MsLevel::Ms3 => "3",
        MsLevel::Other(_) => "2",
    }
}

/// Map MS level to spectrum type accession.
pub fn spectrum_type(level: &MsLevel) -> &str {
    match level {
        MsLevel::Ms1 => MS1_SPECTRUM,
        _ => MSN_SPECTRUM,
    }
}

/// Map polarity to CV accession.
pub fn polarity_accession(p: &Polarity) -> Option<&'static str> {
    match p {
        Polarity::Positive => Some(POSITIVE_SCAN),
        Polarity::Negative => Some(NEGATIVE_SCAN),
        Polarity::Unknown => None,
    }
}

/// Map activation type to CV accession.
pub fn activation_accession(act: &ActivationType) -> Option<&'static str> {
    match act {
        ActivationType::Cid => Some(ACTIVATION_CID),
        ActivationType::Hcd => Some(ACTIVATION_HCD),
        ActivationType::Etd => Some(ACTIVATION_ETD),
        ActivationType::Ecd => Some(ACTIVATION_ECD),
        ActivationType::Mpd => Some(ACTIVATION_MPD),
        ActivationType::Pqd => Some(ACTIVATION_PQD),
        ActivationType::Uvpd => Some(ACTIVATION_UVPD),
        _ => None,
    }
}

/// Map activation type string (from filter/trailer) to CV accession.
pub fn activation_str_to_accession(s: &str) -> Option<&'static str> {
    match s.to_uppercase().as_str() {
        "CID" => Some(ACTIVATION_CID),
        "HCD" => Some(ACTIVATION_HCD),
        "ETD" => Some(ACTIVATION_ETD),
        "ECD" => Some(ACTIVATION_ECD),
        "MPD" => Some(ACTIVATION_MPD),
        "PQD" => Some(ACTIVATION_PQD),
        "UVPD" => Some(ACTIVATION_UVPD),
        _ => None,
    }
}

/// Map analyzer type to CV accession.
pub fn analyzer_accession(a: &AnalyzerType) -> &'static str {
    match a {
        AnalyzerType::Ftms => ORBITRAP,
        AnalyzerType::Itms => ION_TRAP,
        AnalyzerType::Sqms | AnalyzerType::Tqms => QUADRUPOLE,
        AnalyzerType::Tofms => TOF,
        AnalyzerType::Sector => SECTOR,
        _ => ION_TRAP, // fallback
    }
}

/// Map analyzer type to a human-readable name for mzML.
pub fn analyzer_name(a: &AnalyzerType) -> &'static str {
    match a {
        AnalyzerType::Ftms => "orbitrap",
        AnalyzerType::Itms => "radial ejection linear ion trap",
        AnalyzerType::Sqms => "quadrupole",
        AnalyzerType::Tqms => "quadrupole",
        AnalyzerType::Tofms => "time-of-flight",
        AnalyzerType::Sector => "magnetic sector",
        _ => "ion trap",
    }
}

/// Map ionization type to CV accession.
pub fn ionization_accession(ion: &IonizationType) -> &'static str {
    match ion {
        IonizationType::Esi => ESI,
        IonizationType::Nsi => NSI,
        IonizationType::Apci => APCI,
        IonizationType::Maldi => MALDI,
        IonizationType::Ei => EI,
        IonizationType::Ci => CI,
        IonizationType::Fab => FAB,
        _ => ESI, // default fallback
    }
}

/// Map ionization type to a human-readable name.
pub fn ionization_name(ion: &IonizationType) -> &'static str {
    match ion {
        IonizationType::Esi => "electrospray ionization",
        IonizationType::Nsi => "nanoelectrospray",
        IonizationType::Apci => "atmospheric pressure chemical ionization",
        IonizationType::Maldi => "matrix-assisted laser desorption ionization",
        IonizationType::Ei => "electron ionization",
        IonizationType::Ci => "chemical ionization",
        IonizationType::Fab => "fast atom bombardment ionization",
        _ => "electrospray ionization",
    }
}
