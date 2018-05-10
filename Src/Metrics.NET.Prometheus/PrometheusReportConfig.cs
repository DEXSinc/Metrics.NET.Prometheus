namespace Metrics.NET.Prometheus
{
    /// <summary>
    ///     Configuration for the prometheus report.
    /// </summary>
    public sealed class PrometheusReportConfig
    {
        /// <summary>
        ///     Setting this option to <c>true</c> will output the
        ///     individual counts of each counter/meter set item instead
        ///     of the sum of all items in the set.
        /// </summary>
        /// <example>
        /// Using this meter:
        /// <c>
        ///     var meterSubItems = ctx.Meter("MeterSubItems", Unit.None, TimeUnit.Seconds, MetricTags.None);
        ///     meterSubItems.Mark("Item1", 2);
        ///     meterSubItems.Mark("Item2", 3);
        /// </c> 
        /// 
        /// with this option disabled (default):
        /// <c>
        /// # TYPE MeterSubItems gauge
        /// metersubitems 5 0
        /// </c>
        /// 
        /// with this option enabled:
        /// <c>
        /// # TYPE MeterSubItems gauge
        /// metersubitems{item="Item1"} 2 0
        /// metersubitems{item="Item2"} 3 0
        /// </c>
        /// </example>
        public bool OutputSetItems { get; set; }
    }
}