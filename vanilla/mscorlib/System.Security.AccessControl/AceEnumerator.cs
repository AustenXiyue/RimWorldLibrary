using System.Collections;
using Unity;

namespace System.Security.AccessControl;

/// <summary>Provides the ability to iterate through the access control entries (ACEs) in an access control list (ACL). </summary>
public sealed class AceEnumerator : IEnumerator
{
	private GenericAcl owner;

	private int current;

	/// <summary>Gets the current element in the <see cref="T:System.Security.AccessControl.GenericAce" /> collection. This property gets the type-friendly version of the object. </summary>
	/// <returns>The current element in the <see cref="T:System.Security.AccessControl.GenericAce" /> collection.</returns>
	public GenericAce Current
	{
		get
		{
			if (current >= 0)
			{
				return owner[current];
			}
			return null;
		}
	}

	object IEnumerator.Current => Current;

	internal AceEnumerator(GenericAcl owner)
	{
		current = -1;
		base._002Ector();
		this.owner = owner;
	}

	/// <summary>Advances the enumerator to the next element of the <see cref="T:System.Security.AccessControl.GenericAce" /> collection.</summary>
	/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
	/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
	public bool MoveNext()
	{
		if (current + 1 == owner.Count)
		{
			return false;
		}
		current++;
		return true;
	}

	/// <summary>Sets the enumerator to its initial position, which is before the first element in the <see cref="T:System.Security.AccessControl.GenericAce" /> collection.</summary>
	/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
	public void Reset()
	{
		current = -1;
	}

	internal AceEnumerator()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
