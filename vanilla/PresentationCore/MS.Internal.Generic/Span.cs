namespace MS.Internal.Generic;

internal struct Span<T>
{
	internal T Value;

	internal int Length;

	internal Span(T value, int length)
	{
		Value = value;
		Length = length;
	}
}
