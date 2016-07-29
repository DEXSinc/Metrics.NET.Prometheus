.nuget\NuGet.exe restore Metrics.NET.Prometheus.sln

set MSBUILD="C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
set XUNIT=".\packages\xunit.runner.console.2.0.0\tools\xunit.console.exe"
set SOLUTION_FILE="Metrics.NET.Prometheus.sln"

rd /S /Q .\bin\Debug
rd /S /Q .\bin\Release

%MSBUILD% %SOLUTION_FILE% /p:Configuration="Debug"
if %errorlevel% neq 0 exit /b %errorlevel%

%MSBUILD% %SOLUTION_FILE% /p:Configuration="Release"
if %errorlevel% neq 0 exit /b %errorlevel%