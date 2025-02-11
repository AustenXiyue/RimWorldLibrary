namespace MS.Internal.Ink.InkSerializedFormat;

internal class TransformDescriptor
{
	private double[] _transform = new double[6];

	private uint _size;

	private KnownTagCache.KnownTagIndex _tag;

	public KnownTagCache.KnownTagIndex Tag
	{
		get
		{
			return _tag;
		}
		set
		{
			_tag = value;
		}
	}

	public uint Size
	{
		get
		{
			return _size;
		}
		set
		{
			_size = value;
		}
	}

	public double[] Transform => _transform;

	public bool Compare(TransformDescriptor that)
	{
		if (that.Tag == Tag)
		{
			if (that.Size == _size)
			{
				for (int i = 0; i < _size; i++)
				{
					if (!DoubleUtil.AreClose(that.Transform[i], _transform[i]))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}
		return false;
	}
}
