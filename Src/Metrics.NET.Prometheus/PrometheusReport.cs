using System;
using System.Globalization;
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
        private readonly StringBuilder _reportText;

        private PrometheusReport()
        {
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

        private void WriteStringMetric(string type, string name, string value, Unit unit, MetricTags tags)
        {
            // Type line
            _reportText.Append("# TYPE ");
            _reportText.Append(name);
            _reportText.Append(" ");
            _reportText.Append(type);
            _reportText.Append("\n");

            // Actual metric line
            _reportText.Append(name.ToLower());
            _reportText.Append(SuffixFromUnit(unit));
            if (tags.Tags.Length > 0)
            {
                _reportText.Append('{');
                for (int i = 0; i < tags.Tags.Length; i++)
                {
                    if (i != 0) _reportText.Append(",");
                    _reportText.Append("tag");
                    _reportText.Append(i);
                    _reportText.Append('=');
                    _reportText.Append('"');
                    _reportText.Append(FormatName(tags.Tags[i]));
                    _reportText.Append('"');
                }
                _reportText.Append('}');
            }
            _reportText.Append(' ');
            _reportText.Append(value);
            _reportText.Append(' ');
            _reportText.Append(CurrentTimeMillis());
            _reportText.Append("\n\n"); // Extra end-of-line
        }

        private void WriteDoubleMetric(string type, string name, double value, Unit unit, MetricTags tags)
        {
            WriteStringMetric(type, name, FormatDouble(value), unit, tags);
        }

        private void WriteLongMetric(string type, string name, long value, Unit unit, MetricTags tags)
        {
            WriteStringMetric(type, name, FormatLong(value), unit, tags);
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
            WriteLongMetric("gauge", name, value.Count, unit, tags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="unit"></param>
        /// <param name="rateUnit"></param>
        /// <param name="tags"></param>
        protected override void ReportMeter(string name, MeterValue value, Unit unit, TimeUnit rateUnit, MetricTags tags)
        {
            // Metrics.NET counters can be decremented, but Prometheus' are expected to be monotonically increasing,
            // so this maps to a Prometheus gauge.
            WriteLongMetric("gauge", name, value.Count, unit, tags);
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
            WriteLongMetric("gauge", name, value.Histogram.Count, unit, tags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        protected override void ReportHealth(HealthStatus status)
        {
            WriteLongMetric("gauge", "healthz", status.IsHealthy ? 1 : 0, Unit.None, MetricTags.None);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentMetricsData"></param>
        /// <param name="healthStatus"></param>
        /// <returns></returns>
        public static string RenderMetrics(MetricsData currentMetricsData, Func<HealthStatus> healthStatus)
        {
            var report = new PrometheusReport();
            report.RunReport(currentMetricsData, healthStatus, CancellationToken.None);
            return report._reportText.ToString();
        }
    }
}
