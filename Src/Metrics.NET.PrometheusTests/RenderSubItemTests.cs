using System;
using Metrics.MetricData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Metrics.NET.Prometheus.Tests
{
    [TestClass]
    public class RenderSubItemTests
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [TestMethod]
        public void GaugeTest()
        {
            var ctx = new DefaultMetricsContext();

            ctx.Gauge("Gauge", () => 1, Unit.None, MetricTags.None);

            string renderedMetrics = RenderTestMetrics(ctx);

            string expectedMetrics = FormatPrometheusText(@"# TYPE Gauge gauge
gauge 1 0

");
            Assert.AreEqual(expectedMetrics, renderedMetrics);
        }
        
        [TestMethod]
        public void CounterTest()
        {
            var ctx = new DefaultMetricsContext();

            var counter = ctx.Counter("Counter", Unit.None, MetricTags.None);
            counter.Increment(1);

            var counterSubItems = ctx.Counter("CounterSubItems", Unit.None, MetricTags.None);
            counterSubItems.Increment("Item1", 2);
            counterSubItems.Increment("Item2", 3);

            string renderedMetrics = RenderTestMetrics(ctx);
            string expectedMetrics = FormatPrometheusText(@"# TYPE Counter gauge
counter 1 0

# TYPE CounterSubItems gauge
countersubitems 5 0

");
            Assert.AreEqual(expectedMetrics, renderedMetrics);
        }


        [TestMethod]
        public void MeterTest()
        {
            var ctx = new DefaultMetricsContext();

            var meter = ctx.Meter("Meter", Unit.None, TimeUnit.Seconds, MetricTags.None);
            meter.Mark(1);

            var meterSubItems = ctx.Meter("MeterSubItems", Unit.None, TimeUnit.Seconds, MetricTags.None);
            meterSubItems.Mark("Item1", 2);
            meterSubItems.Mark("Item2", 3);

            string renderedMetrics = RenderTestMetrics(ctx);
            string expectedMetrics = FormatPrometheusText(@"# TYPE Meter gauge
meter 1 0

# TYPE MeterSubItems gauge
metersubitems 5 0

");
            Assert.AreEqual(expectedMetrics, renderedMetrics);
        }

        [TestMethod]
        public void HistorgramTest()
        {
            var ctx = new DefaultMetricsContext();

            var histogram = ctx.Histogram("Historgram", Unit.None, SamplingType.Default, MetricTags.None);
            histogram.Update(1);

            var histogramUserValue = ctx.Histogram("HistorgramSubItems", Unit.None, SamplingType.Default, MetricTags.None);
            histogramUserValue.Update(2, "UserValue1");
            histogramUserValue.Update(3, "UserValue2");

            string renderedMetrics = RenderTestMetrics(ctx);
            string expectedMetrics = FormatPrometheusText(@"# TYPE Historgram gauge
historgram 1 0

# TYPE HistorgramSubItems gauge
historgramsubitems 2 0

");
            Assert.AreEqual(expectedMetrics, renderedMetrics);
        }

        [TestMethod]
        public void TimerTest()
        {
            var ctx = new DefaultMetricsContext();

            var timer = ctx.Timer("Timer", Unit.None, SamplingType.Default, TimeUnit.Seconds, TimeUnit.Seconds, MetricTags.None);
            timer.Record(1, TimeUnit.Seconds);

            var timerUserValue = ctx.Timer("TimerSubItems", Unit.None, SamplingType.Default, TimeUnit.Seconds, TimeUnit.Seconds, MetricTags.None);
            timerUserValue.Record(2, TimeUnit.Seconds, "UserValue1");
            timerUserValue.Record(3, TimeUnit.Seconds, "UserValue2");

            string renderedMetrics = RenderTestMetrics(ctx);
            string expectedMetrics = FormatPrometheusText(@"# HELP timer A summary of the timer duration in millisecond 
# TYPE timer summary
timer{quantile=""0.75"",} 1
timer{quantile=""0.95"",} 1
timer{quantile=""0.98"",} 1
timer{quantile=""0.99"",} 1
timer{quantile=""0.999"",} 1
timer_median 1
timer_sum 1
timer_count 1

# HELP timersubitems A summary of the timersubitems duration in millisecond 
# TYPE timersubitems summary
timersubitems{quantile=""0.75"",} 3
timersubitems{quantile=""0.95"",} 3
timersubitems{quantile=""0.98"",} 3
timersubitems{quantile=""0.99"",} 3
timersubitems{quantile=""0.999"",} 3
timersubitems_median 3
timersubitems_sum 5
timersubitems_count 2

");
            Assert.AreEqual(expectedMetrics, renderedMetrics);
        }

        private static string RenderTestMetrics(DefaultMetricsContext ctx)
        {
            var originalData = ctx.DataProvider.CurrentMetricsData;
            var testData = new MetricsData(
                originalData.Context,
                Epoch,
                originalData.Environment,
                originalData.Gauges,
                originalData.Counters,
                originalData.Meters,
                originalData.Histograms,
                originalData.Timers,
                originalData.ChildMetrics);
            return PrometheusReport.RenderMetrics(testData, () => new HealthStatus());
        }

        private static string FormatPrometheusText(string typeGaugeGaugeGauge)
        {
            return typeGaugeGaugeGauge.Replace("\r\n", "\n");
        }
    }
}