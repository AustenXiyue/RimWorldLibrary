using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Unity;

namespace System.Globalization;

/// <summary>Enumerates the text elements of a string. </summary>
[Serializable]
[ComVisible(true)]
public class TextElementEnumerator : IEnumerator
{
	private string str;

	private int index;

	private int startIndex;

	[NonSerialized]
	private int strLen;

	[NonSerialized]
	private int currTextElementLen;

	[OptionalField(VersionAdded = 2)]
	private UnicodeCategory uc;

	[OptionalField(VersionAdded = 2)]
	private int charLen;

	private int endIndex;

	private int nextTextElementLen;

	/// <summary>Gets the current text element in the string.</summary>
	/// <returns>An object containing the current text element in the string.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first text element of the string or after the last text element. </exception>
	public object Current => GetTextElement();

	/// <summary>Gets the index of the text element that the enumerator is currently positioned over.</summary>
	/// <returns>The index of the text element that the enumerator is currently positioned over.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first text element of the string or after the last text element. </exception>
	public int ElementIndex
	{
		get
		{
			if (index == startIndex)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Enumeration has not started. Call MoveNext."));
			}
			return index - currTextElementLen;
		}
	}

	internal TextElementEnumerator(string str, int startIndex, int strLen)
	{
		this.str = str;
		this.startIndex = startIndex;
		this.strLen = strLen;
		Reset();
	}

	[OnDeserializing]
	private void OnDeserializing(StreamingContext ctx)
	{
		charLen = -1;
	}

	[OnDeserialized]
	private void OnDeserialized(StreamingContext ctx)
	{
		strLen = endIndex + 1;
		currTextElementLen = nextTextElementLen;
		if (charLen == -1)
		{
			uc = CharUnicodeInfo.InternalGetUnicodeCategory(str, index, out charLen);
		}
	}

	[OnSerializing]
	private void OnSerializing(StreamingContext ctx)
	{
		endIndex = strLen - 1;
		nextTextElementLen = currTextElementLen;
	}

	/// <summary>Advances the enumerator to the next text element of the string.</summary>
	/// <returns>true if the enumerator was successfully advanced to the next text element; false if the enumerator has passed the end of the string.</returns>
	public bool MoveNext()
	{
		if (index >= strLen)
		{
			index = strLen + 1;
			return false;
		}
		currTextElementLen = StringInfo.GetCurrentTextElementLen(str, index, strLen, ref uc, ref charLen);
		index += currTextElementLen;
		return true;
	}

	/// <summary>Gets the current text element in the string.</summary>
	/// <returns>A new string containing the current text element in the string being read.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first text element of the string or after the last text element. </exception>
	public string GetTextElement()
	{
		if (index == startIndex)
		{
			throw new InvalidOperationException(Environment.GetResourceString("Enumeration has not started. Call MoveNext."));
		}
		if (index > strLen)
		{
			throw new InvalidOperationException(Environment.GetResourceString("Enumeration already finished."));
		}
		return str.Substring(index - currTextElementLen, currTextElementLen);
	}

	/// <summary>Sets the enumerator to its initial position, which is before the first text element in the string.</summary>
	public void Reset()
	{
		index = startIndex;
		if (index < strLen)
		{
			uc = CharUnicodeInfo.InternalGetUnicodeCategory(str, index, out charLen);
		}
	}

	internal TextElementEnumerator()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
