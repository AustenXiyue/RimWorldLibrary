using System.Collections;
using System.Collections.Generic;

namespace System.Xaml.Schema;

internal class ReferenceEqualityTuple<T1, T2> : Tuple<T1, T2>
{
	public ReferenceEqualityTuple(T1 item1, T2 item2)
		: base(item1, item2)
	{
	}

	public override bool Equals(object obj)
	{
		return ((IStructuralEquatable)this).Equals(obj, (IEqualityComparer)ReferenceEqualityComparer.Instance);
	}

	public override int GetHashCode()
	{
		return ((IStructuralEquatable)this).GetHashCode((IEqualityComparer)ReferenceEqualityComparer.Instance);
	}
}
internal class ReferenceEqualityTuple<T1, T2, T3> : Tuple<T1, T2, T3>
{
	public ReferenceEqualityTuple(T1 item1, T2 item2, T3 item3)
		: base(item1, item2, item3)
	{
	}

	public override bool Equals(object obj)
	{
		return ((IStructuralEquatable)this).Equals(obj, (IEqualityComparer)ReferenceEqualityComparer.Instance);
	}

	public override int GetHashCode()
	{
		return ((IStructuralEquatable)this).GetHashCode((IEqualityComparer)ReferenceEqualityComparer.Instance);
	}
}
