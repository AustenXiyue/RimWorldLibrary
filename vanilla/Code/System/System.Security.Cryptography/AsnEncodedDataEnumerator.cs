using System.Collections;
using Unity;

namespace System.Security.Cryptography;

/// <summary>Provides the ability to navigate through an <see cref="T:System.Security.Cryptography.AsnEncodedDataCollection" /> object. This class cannot be inherited.</summary>
public sealed class AsnEncodedDataEnumerator : IEnumerator
{
	private AsnEncodedDataCollection _collection;

	private int _position;

	/// <summary>Gets the current <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object in an <see cref="T:System.Security.Cryptography.AsnEncodedDataCollection" /> object.</summary>
	/// <returns>The current <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object in the collection.</returns>
	public AsnEncodedData Current
	{
		get
		{
			if (_position < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			return _collection[_position];
		}
	}

	/// <summary>Gets the current <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object in an <see cref="T:System.Security.Cryptography.AsnEncodedDataCollection" /> object.</summary>
	/// <returns>The current <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object.</returns>
	object IEnumerator.Current
	{
		get
		{
			if (_position < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			return _collection[_position];
		}
	}

	internal AsnEncodedDataEnumerator(AsnEncodedDataCollection collection)
	{
		_collection = collection;
		_position = -1;
	}

	/// <summary>Advances to the next <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object in an <see cref="T:System.Security.Cryptography.AsnEncodedDataCollection" /> object.</summary>
	/// <returns>true, if the enumerator was successfully advanced to the next element; false, if the enumerator has passed the end of the collection.</returns>
	/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
	public bool MoveNext()
	{
		if (++_position < _collection.Count)
		{
			return true;
		}
		_position = _collection.Count - 1;
		return false;
	}

	/// <summary>Sets an enumerator to its initial position.</summary>
	/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
	public void Reset()
	{
		_position = -1;
	}

	internal AsnEncodedDataEnumerator()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
