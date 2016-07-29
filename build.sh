mono .nuget/nuget.exe restore Metrics.NET.Prometheus.sln 

xbuild Metrics.NET.Prometheus.sln /p:Configuration="Debug"
xbuild Metrics.NET.Prometheus.sln /p:Configuration="Release"