using System.Collections;

namespace System.Windows.Markup.Localizer;

/// <summary>Defines a specialized enumerator that can enumerate over the content of a <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" /> object.</summary>
public sealed class BamlLocalizationDictionaryEnumerator : IDictionaryEnumerator, IEnumerator
{
	private IEnumerator _enumerator;

	/// <summary>Gets the current position's <see cref="T:System.Collections.DictionaryEntry" /> object. </summary>
	/// <returns>An object containing the key and value of the entry at the current position.</returns>
	public DictionaryEntry Entry => (DictionaryEntry)_enumerator.Current;

	/// <summary>Gets the key of the current entry. </summary>
	/// <returns>The key of the current entry.</returns>
	public BamlLocalizableResourceKey Key => (BamlLocalizableResourceKey)Entry.Key;

	/// <summary>Gets the value of the current entry. </summary>
	/// <returns>The value of the current entry.</returns>
	public BamlLocalizableResource Value => (BamlLocalizableResource)Entry.Value;

	/// <summary>Gets the current object in the collection. </summary>
	/// <returns>The current object.</returns>
	public DictionaryEntry Current => Entry;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IEnumerator.Current" />.</summary>
	/// <returns>The current element in the collection.</returns>
	object IEnumerator.Current => Current;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IDictionaryEnumerator.Key" />.</summary>
	/// <returns>The key of the current element of the enumeration.</returns>
	object IDictionaryEnumerator.Key => Key;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IDictionaryEnumerator.Value" />.</summary>
	/// <returns>The value of the current element of the enumeration.</returns>
	object IDictionaryEnumerator.Value => Value;

	internal BamlLocalizationDictionaryEnumerator(IEnumerator enumerator)
	{
		_enumerator = enumerator;
	}

	/// <summary>Moves to the next item in the collection. </summary>
	/// <returns>true if the enumerator successfully advances to the next element. If there are no remaining elements, this method returns false.</returns>
	public bool MoveNext()
	{
		return _enumerator.MoveNext();
	}

	/// <summary>Returns the enumerator to its initial position, which is before the first object in the collection. </summary>
	public void Reset()
	{
		_enumerator.Reset();
	}
}
