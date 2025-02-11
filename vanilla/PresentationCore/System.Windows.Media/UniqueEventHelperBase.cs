using System.Collections.Generic;

namespace System.Windows.Media;

internal abstract class UniqueEventHelperBase<TEventHandler> where TEventHandler : Delegate
{
	protected Dictionary<TEventHandler, int> _delegates;

	internal void AddEvent(TEventHandler handler)
	{
		ArgumentNullException.ThrowIfNull(handler, "handler");
		Dictionary<TEventHandler, int> delegates = _delegates;
		if (delegates != null)
		{
			delegates.TryGetValue(handler, out var value);
			delegates[handler] = value + 1;
		}
		else
		{
			_delegates = new Dictionary<TEventHandler, int> { { handler, 1 } };
		}
	}

	internal void RemoveEvent(TEventHandler handler)
	{
		ArgumentNullException.ThrowIfNull(handler, "handler");
		Dictionary<TEventHandler, int> delegates = _delegates;
		if (delegates != null && delegates.TryGetValue(handler, out var value))
		{
			if (value == 1)
			{
				delegates.Remove(handler);
			}
			else
			{
				delegates[handler] = value - 1;
			}
		}
	}

	protected TEventHandler[] CopyHandlers()
	{
		Dictionary<TEventHandler, int> delegates = _delegates;
		if (delegates != null && delegates.Count > 0)
		{
			TEventHandler[] array = new TEventHandler[delegates.Count];
			delegates.Keys.CopyTo(array, 0);
			return array;
		}
		return Array.Empty<TEventHandler>();
	}
}
