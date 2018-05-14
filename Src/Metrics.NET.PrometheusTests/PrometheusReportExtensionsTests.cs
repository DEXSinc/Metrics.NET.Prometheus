using System.Text;
using Metrics.Reports;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Metrics.NET.Prometheus.Tests
{
    [TestClass]
    public class PrometheusReportExtensionsTests
    {
        [TestMethod]
        public void WithPrometheusEndpointReportTest()
        {
            var context = new DefaultMetricsContext();
            var endpointReports = new MetricsEndpointReports(context.DataProvider, () => new HealthStatus());
            var reports = endpointReports.WithPrometheusEndpointReport();
            Assert.AreEqual(endpointReports, reports);
        }

        [TestMethod]
        public void WithPrometheusEndpointReportPathTest()
        {
            var context = new DefaultMetricsContext();
            var endpointReports = new MetricsEndpointReports(context.DataProvider, () => new HealthStatus());
            var reports = endpointReports.WithPrometheusEndpointReport(path: "test/prometheus");
            Assert.AreEqual(endpointReports, reports);
        }

        [TestMethod]
        public void WithPrometheusEndpointReportPathConfigTest()
        {
            var context = new DefaultMetricsContext();
            var endpointReports = new MetricsEndpointReports(context.DataProvider, () => new HealthStatus());
            var config = new PrometheusReportConfig
            {
                OutputSetItems = true
            };
            var reports = endpointReports.WithPrometheusEndpointReport(path: "test/prometheus", reportConfig: config);
            Assert.AreEqual(endpointReports, reports);
        }

        [TestMethod]
        public void WithPrometheusEndpointReportPathEncodingTest()
        {
            var context = new DefaultMetricsContext();
            var endpointReports = new MetricsEndpointReports(context.DataProvider, () => new HealthStatus());
            var reports = endpointReports.WithPrometheusEndpointReport("test/prometheus", new UTF8Encoding(false));
            Assert.AreEqual(endpointReports, reports);
        }

        [TestMethod]
        public void WithPrometheusEndpointReportPathEncodingConfigTest()
        {
            var context = new DefaultMetricsContext();
            var endpointReports = new MetricsEndpointReports(context.DataProvider, () => new HealthStatus());
            var config = new PrometheusReportConfig
            {
                OutputSetItems = true
            };
            var reports = endpointReports.WithPrometheusEndpointReport("test/prometheus", new UTF8Encoding(false), config);
            Assert.AreEqual(endpointReports, reports);
        }
    }
}