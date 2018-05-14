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
        /// <param name="reportConfig"></param>
        /// <returns></returns>
        public static MetricsEndpointReports WithPrometheusEndpointReport(this MetricsEndpointReports config, string path = "prometheus", PrometheusReportConfig reportConfig = null)
        {
            Encoding utf8WithoutBom = new UTF8Encoding(false);
            return WithPrometheusEndpointReport(config, path, utf8WithoutBom, reportConfig);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="path"></param>
        /// <param name="encoding"></param>
        /// <param name="reportConfig"></param>
        /// <returns></returns>
        public static MetricsEndpointReports WithPrometheusEndpointReport(
            this MetricsEndpointReports config,
            string path,
            Encoding encoding,
            PrometheusReportConfig reportConfig = null)
        {
            reportConfig = reportConfig ?? new PrometheusReportConfig();
            return config.WithEndpointReport(path, (d, h, c) => new MetricsEndpointResponse(PrometheusReport.RenderMetrics(d, h, reportConfig), "text/plain", encoding));
        }
    }
}
