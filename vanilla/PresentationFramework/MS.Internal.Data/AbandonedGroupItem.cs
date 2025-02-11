namespace MS.Internal.Data;

internal class AbandonedGroupItem
{
	private LiveShapingItem _lsi;

	private CollectionViewGroupInternal _group;

	public LiveShapingItem Item => _lsi;

	public CollectionViewGroupInternal Group => _group;

	public AbandonedGroupItem(LiveShapingItem lsi, CollectionViewGroupInternal group)
	{
		_lsi = lsi;
		_group = group;
	}
}
