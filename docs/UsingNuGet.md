Using NuGet

1.  Create a local folder where the package files will be copied to. In
    this example X:\Nuget. Copy the RawFIleReader NuGet file to this
    folder.

2.  In Visual Studio open the Tools \| Options dialog and select NuGet
    Package Manager \| Package Sources (Figure 1).

<img
src="/Users/estrella/Developer/MorscherLab/RawFileReaderS/docs/images/media/image1.png"
style="width:6.5in;height:3.79167in" />

Figure 1.

3.  Click on the plus sign to add a new source. Select the ... button to
    the right of Source. In the dialog select he folder created in
    step 1. Assign a name to this item. I choose Thermo Packages.

4.  Click OK.

5.  In a project, such as the RawFileReader example program, select the
    Solution Explorer tab and make sure that the project doesn’t contain
    any references to the ThermoFisher.CommonCore libraries. If they are
    present, then delete them.

6.  Right click on References and select Manage NuGet Packages. In the
    Nuget Package Consolde click on the Thermo Packages item on the left
    (Figure 2). The package files in the folder created in step 1 will
    be displayed. Select Thermo Scientific Raw File Reader and install
    it. Since our NuGet package contains the Windows and MacOS
    assemblies, an extra step of manually browsing in the Add References
    dialog is necessary. The assemblies will be in
    packages/ThermoFisher.CommonCoreRawFileReader.4.0.26/lib/Windows.

(Note - The dialog shown is from Visual Studio 2015, Microsoft has
updated it for Visual Studio 2017.)

<img
src="/Users/estrella/Developer/MorscherLab/RawFileReaderS/docs/images/media/image2.png"
style="width:6.5in;height:4.35069in" />

Figure 2.

7.  Rebuild the project

8.  The project should look similar to this if the package installed in
    step 6 (Figure 3). The XML files may or may not be present in the
    project.

<img
src="/Users/estrella/Developer/MorscherLab/RawFileReaderS/docs/images/media/image3.png"
style="width:4.01042in;height:5.53125in" />

Figure 3.
