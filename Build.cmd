set DeployDir=2018.1
rmdir /s /q Deploy
rmdir /s /q bin
"%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe" Build.targets
mkdir Deploy\%DeployDir%
packages\NuGet.CommandLine.3.5.0\tools\NuGet.exe pack ResharperBuild.AttachAction\ResharperBuild.AttachAction.nuspec -NoPackageAnalysis -OutputDirectory Deploy\%DeployDir%
