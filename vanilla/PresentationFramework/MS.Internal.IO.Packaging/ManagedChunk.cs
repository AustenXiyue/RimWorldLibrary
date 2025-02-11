using MS.Internal.Interop;

namespace MS.Internal.IO.Packaging;

internal class ManagedChunk
{
	private uint _index;

	private CHUNK_BREAKTYPE _breakType;

	private CHUNKSTATE _flags;

	private uint _lcid;

	private ManagedFullPropSpec _attribute;

	private uint _idChunkSource;

	private uint _startSource;

	private uint _lenSource;

	internal uint ID
	{
		get
		{
			return _index;
		}
		set
		{
			_index = value;
		}
	}

	internal CHUNKSTATE Flags => _flags;

	internal CHUNK_BREAKTYPE BreakType
	{
		get
		{
			return _breakType;
		}
		set
		{
			Invariant.Assert(value >= CHUNK_BREAKTYPE.CHUNK_NO_BREAK && value <= CHUNK_BREAKTYPE.CHUNK_EOC);
			_breakType = value;
		}
	}

	internal uint Locale
	{
		get
		{
			return _lcid;
		}
		set
		{
			_lcid = value;
		}
	}

	internal uint ChunkSource => _idChunkSource;

	internal uint StartSource => _startSource;

	internal uint LenSource => _lenSource;

	internal ManagedFullPropSpec Attribute
	{
		get
		{
			return _attribute;
		}
		set
		{
			_attribute = value;
		}
	}

	internal ManagedChunk(uint index, CHUNK_BREAKTYPE breakType, ManagedFullPropSpec attribute, uint lcid, CHUNKSTATE flags)
	{
		Invariant.Assert(breakType >= CHUNK_BREAKTYPE.CHUNK_NO_BREAK && breakType <= CHUNK_BREAKTYPE.CHUNK_EOC);
		Invariant.Assert(attribute != null);
		_index = index;
		_breakType = breakType;
		_lcid = lcid;
		_attribute = attribute;
		_flags = flags;
		_idChunkSource = _index;
	}
}
