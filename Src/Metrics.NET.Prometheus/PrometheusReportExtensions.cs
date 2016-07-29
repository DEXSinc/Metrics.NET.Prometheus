using Metrics.Reports;
using Metrics.Visualization;

namespace Metrics.NET.Prometheus
{
    public static class PrometheusReportExtensions
    {
        public static MetricsEndpointReports WithPrometheusEndpointReport(this MetricsEndpointReports config,
            string path = "/prometheus")
        {
            return config.WithEndpointReport(path, (d, h, c) => new MetricsEndpointResponse(PrometheusReport.RenderMetrics(d, h), "text/plain"));
        }
    }
}
