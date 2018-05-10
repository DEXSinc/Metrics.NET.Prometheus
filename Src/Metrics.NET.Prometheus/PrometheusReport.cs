using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Metrics.MetricData;
using Metrics.Reporters;

namespace Metrics.NET.Prometheus
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class PrometheusReport : BaseReport
    {
        private readonly PrometheusReportConfig _config;
        private readonly StringBuilder _reportText;

        private PrometheusReport(PrometheusReportConfig config)
        {
            _config = config;
            _reportText = new StringBuilder();
        }

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private long CurrentTimeMillis()
        {
            var ts = CurrentContextTimestamp - Epoch;
            return ts.Ticks / TimeSpan.TicksPerMillisecond;
        }

        private static readonly Regex Rgx = new Regex("[^a-z0-9A-Z:_]");

        private static string FormatName(string name)
        {
            return Rgx.Replace(name, "_");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="metric"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected override string FormatMetricName<T>(string context, MetricValueSource<T> metric)
        {
            return FormatName(metric.Name);
        }


        private static string FormatLong(long value)
        {
            return value.ToString();
        }

        private static string FormatDouble(double value)
        {
            // This deals with the case where the locale is not set to en-US: we don't want to
            // change the locale, and just want to whitelist the values that prometheus accepts.
            if (double.IsNaN(value))
            {
                return "NaN";
            }
            if (double.IsPositiveInfinity(value))
            {
                return "Inf";
            }
            return double.IsNegativeInfinity(value) ? "-Inf" :
                value.ToString(CultureInfo.InvariantCulture);
        }

        private static string SuffixFromUnit(Unit unit)
        {
            if (unit.Name.Equals(Unit.KiloBytes.Name))
            {
                return "_in_kb";
            }
            if (unit.Name.Equals(Unit.MegaBytes.Name))
            {
                return "_in_mb";
            }
            if (unit.Name.Equals(Unit.Percent.Name))
            {
                return "_pct";
            }
            return "";
        }

        private void WriteStringMetric(string type, string name, string value, Unit unit, MetricTags tags, Tuple<string, string> customTag = null, bool writeEndOfLine = true, bool writeType = true)
        {
            if (writeType)
            {
                WriteTypeLineMetric(name, type);
            }

            // Actual metric line
            _reportText.Append(name.ToLower(CultureInfo.InvariantCulture));
            _reportText.Append(SuffixFromUnit(unit));
            if (tags.Tags.Length > 0 || customTag != null)
            {
                _reportText.Append(AddTags(tags, customTag: customTag));
            }

            _reportText.Append(' ');
            _reportText.Append(value);
            _reportText.Append(' ');
            _reportText.Append(CurrentTimeMillis());
            _reportText.Append("\n");

            if (writeEndOfLine)
            {
                _reportText.Append('\n'); // Extra end-of-line
            }
        }

        private void WriteLongMetricSummary(string type, string name, TimerValue value, MetricTags tags)
        {
            var n = name.ToLower(CultureInfo.InvariantCulture);

            // Help line
            _reportText.Append("# HELP ");
            _reportText.Append(n);
            _reportText.Append(" A summary of the ");
            _reportText.Append(n);
            _reportText.Append(" duration in millisecond ");
            _reportText.Append("\n");

            // Type line
            WriteTypeLineMetric(n, type);

            // Actual metric line

            _reportText.Append(n + "{quantile=\"0.75\"," + AddTags(tags, false) + "} " + FormatDouble(value.Histogram.Percentile75));
            _reportText.Append("\n");
            _reportText.Append(n + "{quantile=\"0.95\"," + AddTags(tags, false) + "} " + FormatDouble(value.Histogram.Percentile95));
            _reportText.Append("\n");
            _reportText.Append(n + "{quantile=\"0.98\"," + AddTags(tags, false) + "} " + FormatDouble(value.Histogram.Percentile98));
            _reportText.Append("\n");
            _reportText.Append(n + "{quantile=\"0.99\"," + AddTags(tags, false) + "} " + FormatDouble(value.Histogram.Percentile99));
            _reportText.Append("\n");
            _reportText.Append(n + "{quantile=\"0.999\"," + AddTags(tags, false) + "} " + FormatDouble(value.Histogram.Percentile999));
            _reportText.Append("\n");
            _reportText.Append(n + "_median" + AddTags(tags) + " " + FormatDouble(value.Histogram.Median));
            _reportText.Append("\n");
            _reportText.Append(n + "_sum" + AddTags(tags) + " " + FormatLong(value.TotalTime));
            _reportText.Append("\n");
            _reportText.Append(n + "_count" + AddTags(tags) + " " + FormatLong(value.Histogram.Count));
            _reportText.Append("\n\n"); // Extra end-of-line
        }

        private void WriteTypeLineMetric(string name, string type)
        {
            // Type line
            _reportText.Append("# TYPE ");
            _reportText.Append(name);
            _reportText.Append(" ");
            _reportText.Append(type);
            _reportText.Append("\n");
        }

        private string AddTags(
            MetricTags tags,
            bool withStartTag = true,
            Tuple<string, string> customTag = null)
        {
            var extraTags = new StringBuilder();
            if (tags.Tags.Length > 0 || customTag != null)
            {
                if (withStartTag)
                {
                    extraTags.Append("{");
                }

                if (customTag != null)
                {
                    extraTags.Append(customTag.Item1);
                    extraTags.Append('=');
                    extraTags.Append('"');
                    extraTags.Append(FormatName(customTag.Item2));
                    extraTags.Append('"');

                    if (tags.Tags.Length > 0)
                    {
                        extraTags.Append(',');
                    }
                }
                
                for (int i = 0; i < tags.Tags.Length; i++)
                {
                    if (i != 0) extraTags.Append(",");
                    extraTags.Append("tag");
                    extraTags.Append(i);
                    extraTags.Append('=');
                    extraTags.Append('"');
                    extraTags.Append(FormatName(tags.Tags[i]));
                    extraTags.Append('"');
                }

                if (withStartTag)
                {
                    extraTags.Append("}");
                }
            }

            return extraTags.ToString();
        }
        private void WriteDoubleMetric(string type, string name, double value, Unit unit, MetricTags tags)
        {
            WriteStringMetric(type, name, FormatDouble(value), unit, tags);
        }

        private void WriteLongMetric(string type, string name, long value, Unit unit, MetricTags tags)
        {
            WriteStringMetric(type, name, FormatLong(value), unit, tags);
        }
        
        private void WriteLongMetricSet(string type, string name, IEnumerable<SetItem> items, Unit unit, MetricTags tags)
        {
            WriteTypeLineMetric(name, type);

            foreach (var item in items)
            {
                var customTag = new Tuple<string, string>("item", item.Item ?? string.Empty);
                WriteStringMetric(
                    type,
                    name,
                    FormatLong(item.Count),
                    unit,
                    tags,
                    writeType: false,
                    writeEndOfLine: false,
                    customTag: customTag);
            }

            _reportText.Append('\n');
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextName"></param>
        protected override void StartReport(string contextName)
        {
            // Do nothing
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metricType"></param>
        protected override void StartMetricGroup(string metricType)
        {
            // Do nothing
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="unit"></param>
        /// <param name="tags"></param>
        protected override void ReportGauge(string name, double value, Unit unit, MetricTags tags)
        {
            WriteDoubleMetric("gauge", name, value, unit, tags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="unit"></param>
        /// <param name="tags"></param>
        protected override void ReportCounter(string name, CounterValue value, Unit unit, MetricTags tags)
        {
            // Metrics.NET counters can be decremented, but Prometheus' are expected to be monotonically increasing,
            // so this maps to a Prometheus gauge.
            if (_config.OutputSetItems && value.Items?.Length > 0)
            {
                var setItems = value.Items.Select(item => new SetItem(item.Item, item.Count));
                WriteLongMetricSet("gauge", name, setItems, unit, tags);
            }
            else
            {
                WriteLongMetric("gauge", name, value.Count, unit, tags);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="unit"></param>
        /// <param name="rateUnit"></param>
        /// <param name="tags"></param>
        protected override void ReportMeter(
            string name,
            MeterValue value,
            Unit unit,
            TimeUnit rateUnit,
            MetricTags tags)
        {
            // Metrics.NET counters can be decremented, but Prometheus' are expected to be monotonically increasing,
            // so this maps to a Prometheus gauge.
            if (_config.OutputSetItems && value.Items?.Length > 0)
            {
                var setItems = value.Items.Select(item => new SetItem(item.Item, item.Value?.Count ?? 0));
                WriteLongMetricSet("gauge", name, setItems, unit, tags);
            }
            else
            {
                WriteLongMetric("gauge", name, value.Count, unit, tags);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="unit"></param>
        /// <param name="tags"></param>
        protected override void ReportHistogram(string name, HistogramValue value, Unit unit, MetricTags tags)
        {
            // The semantics between prometheus and Metrics.NET are different enough that we just want to pass a gauge
            WriteLongMetric("gauge", name, value.Count, unit, tags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="unit"></param>
        /// <param name="rateUnit"></param>
        /// <param name="durationUnit"></param>
        /// <param name="tags"></param>
        protected override void ReportTimer(string name, TimerValue value, Unit unit, TimeUnit rateUnit,
            TimeUnit durationUnit, MetricTags tags)
        {
            // The semantics between prometheus and Metrics.NET are different enough that we just want to pass a gauge
            WriteLongMetricSummary("summary", name, value, tags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        protected override void ReportHealth(HealthStatus status)
        {
            WriteLongMetric("gauge", "healthz", status.IsHealthy ? 1 : 0, Unit.None, MetricTags.None);
            foreach (var stat in status.Results)
            {
                WriteLongMetric("gauge", stat.Name, stat.Check.IsHealthy ? 1 : 0, Unit.None, new MetricTags(stat.Check.Message));
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentMetricsData"></param>
        /// <param name="healthStatus"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static string RenderMetrics(
            MetricsData currentMetricsData,
            Func<HealthStatus> healthStatus,
            PrometheusReportConfig config)
        {
            var report = new PrometheusReport(config);
            report.RunReport(currentMetricsData, healthStatus, CancellationToken.None);
            return report._reportText.ToString();
        }

        private class SetItem
        {
            public string Item { get; }
            public long Count { get; }

            public SetItem(string item, long count)
            {
                Item = item;
                Count = count;
            }
        }
    }
}