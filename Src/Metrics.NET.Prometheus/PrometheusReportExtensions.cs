using System.Text;
using Metrics.Endpoints;
using Metrics.Reports;

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
        public static MetricsEndpointReports WithPrometheusEndpointReport(this MetricsEndpointReports config, string path = "prometheus")
        {
            Encoding utf8WithoutBom = new UTF8Encoding(false);
            return WithPrometheusEndpointReport(config, path, utf8WithoutBom);
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
