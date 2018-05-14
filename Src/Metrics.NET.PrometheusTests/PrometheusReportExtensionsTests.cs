using System.Text;
using Metrics.Reports;
using Xunit;

namespace Metrics.NET.Prometheus.Tests
{
    public class PrometheusReportExtensionsTests
    {
        [Fact]
        public void WithPrometheusEndpointReportTest()
        {
            var context = new DefaultMetricsContext();
            var endpointReports = new MetricsEndpointReports(context.DataProvider, () => new HealthStatus());
            var reports = endpointReports.WithPrometheusEndpointReport();
            Assert.Equal(endpointReports, reports);
        }

        [Fact]
        public void WithPrometheusEndpointReportPathTest()
        {
            var context = new DefaultMetricsContext();
            var endpointReports = new MetricsEndpointReports(context.DataProvider, () => new HealthStatus());
            var reports = endpointReports.WithPrometheusEndpointReport(path: "test/prometheus");
            Assert.Equal(endpointReports, reports);
        }

        [Fact]
        public void WithPrometheusEndpointReportPathConfigTest()
        {
            var context = new DefaultMetricsContext();
            var endpointReports = new MetricsEndpointReports(context.DataProvider, () => new HealthStatus());
            var config = new PrometheusReportConfig
            {
                OutputSetItems = true
            };
            var reports = endpointReports.WithPrometheusEndpointReport(path: "test/prometheus", reportConfig: config);
            Assert.Equal(endpointReports, reports);
        }

        [Fact]
        public void WithPrometheusEndpointReportPathEncodingTest()
        {
            var context = new DefaultMetricsContext();
            var endpointReports = new MetricsEndpointReports(context.DataProvider, () => new HealthStatus());
            var reports = endpointReports.WithPrometheusEndpointReport("test/prometheus", new UTF8Encoding(false));
            Assert.Equal(endpointReports, reports);
        }

        [Fact]
        public void WithPrometheusEndpointReportPathEncodingConfigTest()
        {
            var context = new DefaultMetricsContext();
            var endpointReports = new MetricsEndpointReports(context.DataProvider, () => new HealthStatus());
            var config = new PrometheusReportConfig
            {
                OutputSetItems = true
            };
            var reports = endpointReports.WithPrometheusEndpointReport("test/prometheus", new UTF8Encoding(false), config);
            Assert.Equal(endpointReports, reports);
        }
    }
}