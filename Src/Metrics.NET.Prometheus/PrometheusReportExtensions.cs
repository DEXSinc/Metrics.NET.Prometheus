using Metrics.Reports;
using Metrics.Visualization;

namespace Metrics.NET.Prometheus
{
    /// <summary>
    /// 
    /// </summary>
    public static class PrometheusReportExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static MetricsEndpointReports WithPrometheusEndpointReport(this MetricsEndpointReports config,
            string path = "/prometheus")
        {
            return config.WithEndpointReport(path, (d, h, c) => new MetricsEndpointResponse(PrometheusReport.RenderMetrics(d, h), "text/plain"));
        }
    }
}
