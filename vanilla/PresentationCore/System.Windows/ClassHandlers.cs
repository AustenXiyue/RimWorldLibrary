namespace System.Windows;

internal struct ClassHandlers
{
	internal RoutedEvent RoutedEvent;

	internal RoutedEventHandlerInfoList Handlers;

	internal bool HasSelfHandlers;

	public override bool Equals(object o)
	{
		return Equals((ClassHandlers)o);
	}

	public bool Equals(ClassHandlers classHandlers)
	{
		if (classHandlers.RoutedEvent == RoutedEvent)
		{
			return classHandlers.Handlers == Handlers;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static bool operator ==(ClassHandlers classHandlers1, ClassHandlers classHandlers2)
	{
		return classHandlers1.Equals(classHandlers2);
	}

	public static bool operator !=(ClassHandlers classHandlers1, ClassHandlers classHandlers2)
	{
		return !classHandlers1.Equals(classHandlers2);
	}
}
