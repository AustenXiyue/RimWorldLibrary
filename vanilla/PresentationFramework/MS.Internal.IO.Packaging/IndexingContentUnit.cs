using MS.Internal.Interop;

namespace MS.Internal.IO.Packaging;

internal class IndexingContentUnit : ManagedChunk
{
	private string _contents;

	internal string Text => _contents;

	internal IndexingContentUnit(string contents, uint chunkID, CHUNK_BREAKTYPE breakType, ManagedFullPropSpec attribute, uint lcid)
		: base(chunkID, breakType, attribute, lcid, CHUNKSTATE.CHUNK_TEXT)
	{
		_contents = contents;
	}

	internal void InitIndexingContentUnit(string contents, uint chunkID, CHUNK_BREAKTYPE breakType, ManagedFullPropSpec attribute, uint lcid)
	{
		_contents = contents;
		base.ID = chunkID;
		base.BreakType = breakType;
		base.Attribute = attribute;
		base.Locale = lcid;
	}
}
