using System.Collections;

namespace System.Windows.Documents;

internal class MarkerList : ArrayList
{
	internal MarkerList()
		: base(5)
	{
	}

	internal MarkerListEntry EntryAt(int index)
	{
		return (MarkerListEntry)this[index];
	}

	internal void AddEntry(MarkerStyle m, long nILS, long nStartIndexOverride, long nStartIndexDefault, long nLevel)
	{
		MarkerListEntry markerListEntry = new MarkerListEntry();
		markerListEntry.Marker = m;
		markerListEntry.StartIndexOverride = nStartIndexOverride;
		markerListEntry.StartIndexDefault = nStartIndexDefault;
		markerListEntry.VirtualListLevel = nLevel;
		markerListEntry.ILS = nILS;
		Add(markerListEntry);
	}
}
