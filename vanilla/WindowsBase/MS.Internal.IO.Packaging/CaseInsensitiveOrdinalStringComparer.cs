using System.Collections;

namespace MS.Internal.IO.Packaging;

internal class CaseInsensitiveOrdinalStringComparer : IEqualityComparer, IComparer
{
	bool IEqualityComparer.Equals(object x, object y)
	{
		Invariant.Assert(x is string && y is string);
		return string.CompareOrdinal(((string)x).ToUpperInvariant(), ((string)y).ToUpperInvariant()) == 0;
	}

	int IComparer.Compare(object x, object y)
	{
		Invariant.Assert(x is string && y is string);
		return string.CompareOrdinal(((string)x).ToUpperInvariant(), ((string)y).ToUpperInvariant());
	}

	int IEqualityComparer.GetHashCode(object str)
	{
		Invariant.Assert(str is string);
		return ((string)str).ToUpperInvariant().GetHashCode();
	}
}
