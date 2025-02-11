using System.Collections.Generic;

namespace System.Windows.Media;

internal sealed class UniqueEventHelper<TEventArgs> : UniqueEventHelperBase<EventHandler<TEventArgs>> where TEventArgs : EventArgs
{
	internal UniqueEventHelper<TEventArgs> Clone()
	{
		UniqueEventHelper<TEventArgs> uniqueEventHelper = new UniqueEventHelper<TEventArgs>();
		if (_delegates != null)
		{
			uniqueEventHelper._delegates = new Dictionary<EventHandler<TEventArgs>, int>(_delegates);
		}
		return uniqueEventHelper;
	}

	internal void InvokeEvents(object sender, TEventArgs args)
	{
		EventHandler<TEventArgs>[] array = CopyHandlers();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](sender, args);
		}
	}
}
internal sealed class UniqueEventHelper : UniqueEventHelperBase<EventHandler>
{
	internal UniqueEventHelper Clone()
	{
		UniqueEventHelper uniqueEventHelper = new UniqueEventHelper();
		if (_delegates != null)
		{
			uniqueEventHelper._delegates = new Dictionary<EventHandler, int>(_delegates);
		}
		return uniqueEventHelper;
	}

	internal void InvokeEvents(object sender, EventArgs args)
	{
		EventHandler[] array = CopyHandlers();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](sender, args);
		}
	}
}
