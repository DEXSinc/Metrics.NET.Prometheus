using System.Runtime.CompilerServices;
using System.Text;
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
        public static MetricsEndpointReports WithPrometheusEndpointReport(this MetricsEndpointReports config, string path = "/prometheus")
        {
            return WithPrometheusEndpointReport(config, path, Encoding.ASCII);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="path"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static MetricsEndpointReports WithPrometheusEndpointReport(this MetricsEndpointReports config,
            string path, Encoding encoding)
        {
            return config.WithEndpointReport(path, (d, h, c) => new MetricsEndpointResponse(PrometheusReport.RenderMetrics(d, h), "text/plain", encoding));
        }
    }
}
