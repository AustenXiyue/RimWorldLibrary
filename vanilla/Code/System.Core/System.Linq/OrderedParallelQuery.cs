using System.Collections.Generic;
using System.Linq.Parallel;
using Unity;

namespace System.Linq;

/// <summary>Represents a sorted, parallel sequence.</summary>
/// <typeparam name="TSource">The type of elements in the source collection.</typeparam>
public class OrderedParallelQuery<TSource> : ParallelQuery<TSource>
{
	private QueryOperator<TSource> _sortOp;

	internal QueryOperator<TSource> SortOperator => _sortOp;

	internal IOrderedEnumerable<TSource> OrderedEnumerable => (IOrderedEnumerable<TSource>)_sortOp;

	internal OrderedParallelQuery(QueryOperator<TSource> sortOp)
		: base(sortOp.SpecifiedQuerySettings)
	{
		_sortOp = sortOp;
	}

	/// <summary>Returns an enumerator that iterates through the sequence.</summary>
	/// <returns>An enumerator that iterates through the sequence.</returns>
	public override IEnumerator<TSource> GetEnumerator()
	{
		return _sortOp.GetEnumerator();
	}

	internal OrderedParallelQuery()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
