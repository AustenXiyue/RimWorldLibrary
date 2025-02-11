using System.Threading;

namespace System.Windows;

internal static class EventHelper
{
	public static void AddHandler<T>(ref Tuple<T, Delegate[]> field, T value) where T : Delegate
	{
		Tuple<T, Delegate[]> tuple;
		Tuple<T, Delegate[]> value2;
		do
		{
			tuple = field;
			T val = (T)Delegate.Combine((tuple != null) ? tuple.Item1 : null, value);
			value2 = ((val != null) ? Tuple.Create(val, val.GetInvocationList()) : null);
		}
		while (Interlocked.CompareExchange(ref field, value2, tuple) != tuple);
	}

	public static void RemoveHandler<T>(ref Tuple<T, Delegate[]> field, T value) where T : Delegate
	{
		Tuple<T, Delegate[]> tuple;
		Tuple<T, Delegate[]> value2;
		do
		{
			tuple = field;
			T val = (T)Delegate.Remove((tuple != null) ? tuple.Item1 : null, value);
			value2 = ((val != null) ? Tuple.Create(val, val.GetInvocationList()) : null);
		}
		while (Interlocked.CompareExchange(ref field, value2, tuple) != tuple);
	}
}
