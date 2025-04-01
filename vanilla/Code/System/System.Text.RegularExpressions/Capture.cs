using Unity;

namespace System.Text.RegularExpressions;

/// <summary>Represents the results from a single successful subexpression capture. </summary>
[Serializable]
public class Capture
{
	internal string _text;

	internal int _index;

	internal int _length;

	/// <summary>The position in the original string where the first character of the captured substring is found.</summary>
	/// <returns>The zero-based starting position in the original string where the captured substring is found.</returns>
	public int Index => _index;

	/// <summary>Gets the length of the captured substring.</summary>
	/// <returns>The length of the captured substring.</returns>
	public int Length => _length;

	/// <summary>Gets the captured substring from the input string.</summary>
	/// <returns>The substring that is captured by the match.</returns>
	public string Value => _text.Substring(_index, _length);

	internal Capture(string text, int i, int l)
	{
		_text = text;
		_index = i;
		_length = l;
	}

	/// <summary>Retrieves the captured substring from the input string by calling the <see cref="P:System.Text.RegularExpressions.Capture.Value" /> property. </summary>
	/// <returns>The substring that was captured by the match.</returns>
	public override string ToString()
	{
		return Value;
	}

	internal string GetOriginalString()
	{
		return _text;
	}

	internal string GetLeftSubstring()
	{
		return _text.Substring(0, _index);
	}

	internal string GetRightSubstring()
	{
		return _text.Substring(_index + _length, _text.Length - _index - _length);
	}

	internal Capture()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
