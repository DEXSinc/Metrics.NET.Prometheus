rd /S /Q .\Publishing\lib

call build.bat
if %errorlevel% neq 0 exit /b %errorlevel%

md .\Publishing\lib
md .\Publishing\lib\net45

copy .\artifacts\Release\Metrics.NET.Prometheus.dll .\Publishing\lib\net45\
copy .\artifacts\Release\Metrics.NET.Prometheus.xml .\Publishing\lib\net45\
copy .\artifacts\Release\Metrics.NET.Prometheus.pdb .\Publishing\lib\net45\

.\.nuget\NuGet.exe pack .\Publishing\Metrics.NET.Prometheus.nuspec -OutputDirectory .\Publishing
if %errorlevel% neq 0 exit /b %errorlevel%
