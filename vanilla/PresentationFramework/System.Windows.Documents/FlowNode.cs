namespace System.Windows.Documents;

internal sealed class FlowNode : IComparable
{
	private readonly int _scopeId;

	private readonly FlowNodeType _type;

	private int _fp;

	private object _cookie;

	private FixedSOMElement[] _elements;

	internal int Fp => _fp;

	internal int ScopeId => _scopeId;

	internal FlowNodeType Type => _type;

	internal object Cookie => _cookie;

	internal FixedSOMElement[] FixedSOMElements
	{
		get
		{
			return _elements;
		}
		set
		{
			_elements = value;
		}
	}

	internal FlowNode(int scopeId, FlowNodeType type, object cookie)
	{
		_scopeId = scopeId;
		_type = type;
		_cookie = cookie;
	}

	public static bool IsNull(FlowNode flow)
	{
		return flow == null;
	}

	public override int GetHashCode()
	{
		return _scopeId.GetHashCode() ^ _fp.GetHashCode();
	}

	public override bool Equals(object o)
	{
		if (o == null || GetType() != o.GetType())
		{
			return false;
		}
		FlowNode flowNode = (FlowNode)o;
		return _fp == flowNode._fp;
	}

	public int CompareTo(object o)
	{
		if (o == null)
		{
			throw new ArgumentNullException("o");
		}
		if (!(o is FlowNode flowNode))
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, o.GetType(), typeof(FlowNode)), "o");
		}
		if (this == flowNode)
		{
			return 0;
		}
		int num = _fp - flowNode._fp;
		if (num == 0)
		{
			return 0;
		}
		if (num < 0)
		{
			return -1;
		}
		return 1;
	}

	internal void SetFp(int fp)
	{
		_fp = fp;
	}

	internal void IncreaseFp()
	{
		_fp++;
	}

	internal void DecreaseFp()
	{
		_fp--;
	}

	internal void AttachElement(FixedElement fixedElement)
	{
		_cookie = fixedElement;
	}
}
