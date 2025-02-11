using System.Collections;

namespace System.Windows.Markup;

internal class ParserStack : ArrayList
{
	internal object CurrentContext
	{
		get
		{
			if (Count <= 0)
			{
				return null;
			}
			return this[Count - 1];
		}
	}

	internal object ParentContext
	{
		get
		{
			if (Count <= 1)
			{
				return null;
			}
			return this[Count - 2];
		}
	}

	internal object GrandParentContext
	{
		get
		{
			if (Count <= 2)
			{
				return null;
			}
			return this[Count - 3];
		}
	}

	internal object GreatGrandParentContext
	{
		get
		{
			if (Count <= 3)
			{
				return null;
			}
			return this[Count - 4];
		}
	}

	internal ParserStack()
	{
	}

	private ParserStack(ICollection collection)
		: base(collection)
	{
	}

	public void Push(object o)
	{
		Add(o);
	}

	public object Pop()
	{
		object? result = this[Count - 1];
		RemoveAt(Count - 1);
		return result;
	}

	public object Peek()
	{
		return this[Count - 1];
	}

	public override object Clone()
	{
		return new ParserStack(this);
	}
}
