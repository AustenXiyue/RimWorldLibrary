namespace System.Runtime.CompilerServices;

internal interface ITuple
{
	int Length { get; }

	object? this[int index] { get; }
}
