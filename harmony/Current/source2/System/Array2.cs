namespace System;

internal static class Array2
{
	private static class EmptyClass<T>
	{
		public static readonly T[] Empty = new T[0];
	}

	public static T[] Empty<T>()
	{
		return EmptyClass<T>.Empty;
	}
}
