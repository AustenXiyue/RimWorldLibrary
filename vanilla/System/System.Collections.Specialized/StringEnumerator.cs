using Unity;

namespace System.Collections.Specialized;

/// <summary>Supports a simple iteration over a <see cref="T:System.Collections.Specialized.StringCollection" />.</summary>
public class StringEnumerator
{
	private IEnumerator baseEnumerator;

	private IEnumerable temp;

	/// <summary>Gets the current element in the collection.</summary>
	/// <returns>The current element in the collection.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
	public string Current => (string)baseEnumerator.Current;

	internal StringEnumerator(StringCollection mappings)
	{
		temp = mappings;
		baseEnumerator = temp.GetEnumerator();
	}

	/// <summary>Advances the enumerator to the next element of the collection.</summary>
	/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
	/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
	public bool MoveNext()
	{
		return baseEnumerator.MoveNext();
	}

	/// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
	/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
	public void Reset()
	{
		baseEnumerator.Reset();
	}

	internal StringEnumerator()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
