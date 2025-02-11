namespace System.Windows.Documents;

internal struct FixedNode : IComparable
{
	private readonly int[] _path;

	internal int Page => _path[0];

	internal int this[int level] => _path[level];

	internal int ChildLevels => _path.Length - 1;

	internal static FixedNode Create(int pageIndex, int childLevels, int level1Index, int level2Index, int[] childPath)
	{
		return childLevels switch
		{
			1 => new FixedNode(pageIndex, level1Index), 
			2 => new FixedNode(pageIndex, level1Index, level2Index), 
			_ => Create(pageIndex, childPath), 
		};
	}

	internal static FixedNode Create(int pageIndex, int[] childPath)
	{
		int[] array = new int[childPath.Length + 1];
		array[0] = pageIndex;
		childPath.CopyTo(array, 1);
		return new FixedNode(array);
	}

	private FixedNode(int page, int level1Index)
	{
		_path = new int[2];
		_path[0] = page;
		_path[1] = level1Index;
	}

	private FixedNode(int page, int level1Index, int level2Index)
	{
		_path = new int[3];
		_path[0] = page;
		_path[1] = level1Index;
		_path[2] = level2Index;
	}

	private FixedNode(int[] path)
	{
		_path = path;
	}

	public int CompareTo(object o)
	{
		if (o == null)
		{
			throw new ArgumentNullException("o");
		}
		if (o.GetType() != typeof(FixedNode))
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, o.GetType(), typeof(FixedNode)), "o");
		}
		FixedNode fixedNode = (FixedNode)o;
		return CompareTo(fixedNode);
	}

	public int CompareTo(FixedNode fixedNode)
	{
		int num = Page.CompareTo(fixedNode.Page);
		if (num == 0)
		{
			for (int i = 1; i <= ChildLevels && i <= fixedNode.ChildLevels; i++)
			{
				int num2 = this[i];
				int num3 = fixedNode[i];
				if (num2 != num3)
				{
					return num2.CompareTo(num3);
				}
			}
		}
		return num;
	}

	internal int ComparetoIndex(int[] childPath)
	{
		for (int i = 0; i < childPath.Length && i < _path.Length - 1; i++)
		{
			if (childPath[i] != _path[i + 1])
			{
				return childPath[i].CompareTo(_path[i + 1]);
			}
		}
		return 0;
	}

	public static bool operator <(FixedNode fp1, FixedNode fp2)
	{
		return fp1.CompareTo(fp2) < 0;
	}

	public static bool operator <=(FixedNode fp1, FixedNode fp2)
	{
		return fp1.CompareTo(fp2) <= 0;
	}

	public static bool operator >(FixedNode fp1, FixedNode fp2)
	{
		return fp1.CompareTo(fp2) > 0;
	}

	public static bool operator >=(FixedNode fp1, FixedNode fp2)
	{
		return fp1.CompareTo(fp2) >= 0;
	}

	public override bool Equals(object o)
	{
		if (o is FixedNode)
		{
			return Equals((FixedNode)o);
		}
		return false;
	}

	public bool Equals(FixedNode fixedp)
	{
		return CompareTo(fixedp) == 0;
	}

	public static bool operator ==(FixedNode fp1, FixedNode fp2)
	{
		return fp1.Equals(fp2);
	}

	public static bool operator !=(FixedNode fp1, FixedNode fp2)
	{
		return !fp1.Equals(fp2);
	}

	public override int GetHashCode()
	{
		int num = 0;
		int[] path = _path;
		foreach (int num2 in path)
		{
			num = 43 * num + num2;
		}
		return num;
	}
}
