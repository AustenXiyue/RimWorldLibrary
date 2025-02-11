using System.Collections.Generic;

namespace MS.Internal.Ink.InkSerializedFormat;

internal class StrokeDescriptor
{
	private List<KnownTagCache.KnownTagIndex> _strokeDescriptor = new List<KnownTagCache.KnownTagIndex>();

	private uint _Size;

	public uint Size
	{
		get
		{
			return _Size;
		}
		set
		{
			_Size = value;
		}
	}

	public List<KnownTagCache.KnownTagIndex> Template => _strokeDescriptor;

	public bool IsEqual(StrokeDescriptor strd)
	{
		if (_strokeDescriptor.Count != strd.Template.Count)
		{
			return false;
		}
		for (int i = 0; i < _strokeDescriptor.Count; i++)
		{
			if (_strokeDescriptor[i] != strd.Template[i])
			{
				return false;
			}
		}
		return true;
	}
}
