set DeployDir=2018.1
rmdir /s /q Deploy
rmdir /s /q bin
"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe" Build.targets
mkdir Deploy\%DeployDir%
"%USERPROFILE%\.nuget\packages\NuGet.CommandLine\4.5.1\tools\nuget.exe" pack ResharperBuild.AttachAction\ResharperBuild.AttachAction.nuspec -NoPackageAnalysis -OutputDirectory Deploy\%DeployDir%
