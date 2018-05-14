using System;
using Metrics.MetricData;
using Xunit;

namespace Metrics.NET.Prometheus.Tests
{
    /// <summary>
    ///     Tests for the OutputSetItems option.
    /// </summary>
    public class RenderSetItemsTests
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        ///      Gauges don't support <c>OutputSetItems = true</c>.
        /// </summary>>
        [Fact]
        public void GaugeTest()
        {
            var context = new DefaultMetricsContext();
            var config = new PrometheusReportConfig
            {
                OutputSetItems = true
            };

            context.Gauge("Gauge", () => 1, Unit.None, MetricTags.None);

            string renderedMetrics = RenderTestMetrics(context, config);
            string expectedMetrics = FormatPrometheusText(@"# TYPE Gauge gauge
gauge 1 0

");
            Assert.Equal(expectedMetrics, renderedMetrics);
        }

        /// <summary>
        ///     Counters support <c>OutputSetItems = true</c>.
        /// </summary>>
        [Fact]
        public void CounterTest()
        {
            var context = new DefaultMetricsContext();
            var config = new PrometheusReportConfig
            {
                OutputSetItems = true
            };

            var counter = context.Counter("Counter", Unit.None, MetricTags.None);
            counter.Increment(1);

            var counterSubItems = context.Counter("CounterSubItems", Unit.None, MetricTags.None);
            counterSubItems.Increment("Item1", 2);
            counterSubItems.Increment("Item2", 3);

            string renderedMetrics = RenderTestMetrics(context, config);
            string expectedMetrics = FormatPrometheusText(@"# TYPE Counter gauge
counter 1 0

# TYPE CounterSubItems gauge
countersubitems{item=""Item1""} 2 0
countersubitems{item=""Item2""} 3 0

");
            Assert.Equal(expectedMetrics, renderedMetrics);
        }

        /// <summary>
        ///     Counters support <c>OutputSetItems = false</c>.
        /// </summary>>
        [Fact]
        public void CounterSetItemsDisabledTest()
        {
            var context = new DefaultMetricsContext();
            var config = new PrometheusReportConfig
            {
                OutputSetItems = false
            };

            var counter = context.Counter("Counter", Unit.None, MetricTags.None);
            counter.Increment(1);

            var counterSubItems = context.Counter("CounterSubItems", Unit.None, MetricTags.None);
            counterSubItems.Increment("Item1", 2);
            counterSubItems.Increment("Item2", 3);

            string renderedMetrics = RenderTestMetrics(context, config);
            string expectedMetrics = FormatPrometheusText(@"# TYPE Counter gauge
counter 1 0

# TYPE CounterSubItems gauge
countersubitems 5 0

");
            Assert.Equal(expectedMetrics, renderedMetrics);
        }

        /// <summary>
        ///     Meters support <c>OutputSetItems = true</c>.
        /// </summary>
        [Fact]
        public void MeterTest()
        {
            var context = new DefaultMetricsContext();
            var config = new PrometheusReportConfig
            {
                OutputSetItems = true
            };

            var meter = context.Meter("Meter", Unit.None, TimeUnit.Seconds, MetricTags.None);
            meter.Mark(1);

            var meterSubItems = context.Meter("MeterSubItems", Unit.None, TimeUnit.Seconds, MetricTags.None);
            meterSubItems.Mark("Item1", 2);
            meterSubItems.Mark("Item2", 3);

            string renderedMetrics = RenderTestMetrics(context, config);
            string expectedMetrics = FormatPrometheusText(@"# TYPE Meter gauge
meter 1 0

# TYPE MeterSubItems gauge
metersubitems{item=""Item1""} 2 0
metersubitems{item=""Item2""} 3 0

");
            Assert.Equal(expectedMetrics, renderedMetrics);
        }

        /// <summary>
        ///     Meters support <c>OutputSetItems = false</c>.
        /// </summary>
        [Fact]
        public void MeterSetItemsDisabledTest()
        {
            var context = new DefaultMetricsContext();
            var config = new PrometheusReportConfig
            {
                OutputSetItems = false
            };

            var meter = context.Meter("Meter", Unit.None, TimeUnit.Seconds, MetricTags.None);
            meter.Mark(1);

            var meterSubItems = context.Meter("MeterSubItems", Unit.None, TimeUnit.Seconds, MetricTags.None);
            meterSubItems.Mark("Item1", 2);
            meterSubItems.Mark("Item2", 3);

            string renderedMetrics = RenderTestMetrics(context, config);
            string expectedMetrics = FormatPrometheusText(@"# TYPE Meter gauge
meter 1 0

# TYPE MeterSubItems gauge
metersubitems 5 0

");
            Assert.Equal(expectedMetrics, renderedMetrics);
        }

        /// <summary>
        ///     Meters support <c>OutputSetItems = true</c> with additional tags.
        /// </summary>
        [Fact]
        public void MeterWithTagsTest()
        {
            var context = new DefaultMetricsContext();
            var config = new PrometheusReportConfig
            {
                OutputSetItems = true
            };

            var meter = context.Meter("Meter", Unit.None, TimeUnit.Seconds, new[] {"tag0", "tag1"});
            meter.Mark(1);

            var meterSubItems = context.Meter("MeterSubItems", Unit.None, TimeUnit.Seconds, new[] { "tag0", "tag1" });
            meterSubItems.Mark("Item1", 2);
            meterSubItems.Mark("Item2", 3);

            string renderedMetrics = RenderTestMetrics(context, config);
            string expectedMetrics = FormatPrometheusText(@"# TYPE Meter gauge
meter{tag0=""tag0"",tag1=""tag1""} 1 0

# TYPE MeterSubItems gauge
metersubitems{item=""Item1"",tag0=""tag0"",tag1=""tag1""} 2 0
metersubitems{item=""Item2"",tag0=""tag0"",tag1=""tag1""} 3 0

");
            Assert.Equal(expectedMetrics, renderedMetrics);
        }

        /// <summary>
        ///     Histograms don't support <c>OutputSetItems = true</c>.
        /// </summary>
        [Fact]
        public void HistorgramTest()
        {
            var context = new DefaultMetricsContext();
            var config = new PrometheusReportConfig
            {
                OutputSetItems = true
            };

            var histogram = context.Histogram("Historgram", Unit.None, SamplingType.Default, MetricTags.None);
            histogram.Update(1);

            var histogramUserValue = context.Histogram("HistorgramSubItems", Unit.None, SamplingType.Default, MetricTags.None);
            histogramUserValue.Update(2, "UserValue1");
            histogramUserValue.Update(3, "UserValue2");

            string renderedMetrics = RenderTestMetrics(context, config);
            string expectedMetrics = FormatPrometheusText(@"# TYPE Historgram gauge
historgram 1 0

# TYPE HistorgramSubItems gauge
historgramsubitems 2 0

");
            Assert.Equal(expectedMetrics, renderedMetrics);
        }

        /// <summary>
        ///     Timers don't support <c>OutputSetItems = true</c>.
        /// </summary>
        [Fact]
        public void TimerTest()
        {
            var context = new DefaultMetricsContext();
            var config = new PrometheusReportConfig
            {
                OutputSetItems = true
            };

            var timer = context.Timer("Timer", Unit.None, SamplingType.Default, TimeUnit.Seconds, TimeUnit.Seconds, MetricTags.None);
            timer.Record(1, TimeUnit.Seconds);

            var timerUserValue = context.Timer("TimerSubItems", Unit.None, SamplingType.Default, TimeUnit.Seconds, TimeUnit.Seconds, MetricTags.None);
            timerUserValue.Record(2, TimeUnit.Seconds, "UserValue1");
            timerUserValue.Record(3, TimeUnit.Seconds, "UserValue2");

            string renderedMetrics = RenderTestMetrics(context, config);
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
            Assert.Equal(expectedMetrics, renderedMetrics);
        }

        private static string RenderTestMetrics(DefaultMetricsContext ctx, PrometheusReportConfig config)
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
            return PrometheusReport.RenderMetrics(testData, () => new HealthStatus(), config);
        }

        private static string FormatPrometheusText(string typeGaugeGaugeGauge)
        {
            return typeGaugeGaugeGauge.Replace("\r\n", "\n");
        }
    }
}