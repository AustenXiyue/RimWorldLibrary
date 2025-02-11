using System;
using WinRT;

namespace MS.Internal.WindowsRuntime.Windows.Data.Text;

[WindowsRuntimeType]
internal struct TextSegment : IEquatable<TextSegment>
{
	public uint StartPosition;

	public uint Length;

	public TextSegment(uint _StartPosition, uint _Length)
	{
		StartPosition = _StartPosition;
		Length = _Length;
	}

	public static bool operator ==(TextSegment x, TextSegment y)
	{
		if (x.StartPosition == y.StartPosition)
		{
			return x.Length == y.Length;
		}
		return false;
	}

	public static bool operator !=(TextSegment x, TextSegment y)
	{
		return !(x == y);
	}

	public bool Equals(TextSegment other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		if (obj is TextSegment textSegment)
		{
			return this == textSegment;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return StartPosition.GetHashCode() ^ Length.GetHashCode();
	}
}
