using System.Windows.Input;

namespace MS.Internal.Ink.InkSerializedFormat;

internal struct MetricEntryList
{
	public KnownTagCache.KnownTagIndex Tag;

	public StylusPointPropertyInfo PropertyMetrics;

	public MetricEntryList(KnownTagCache.KnownTagIndex tag, StylusPointPropertyInfo prop)
	{
		Tag = tag;
		PropertyMetrics = prop;
	}
}
