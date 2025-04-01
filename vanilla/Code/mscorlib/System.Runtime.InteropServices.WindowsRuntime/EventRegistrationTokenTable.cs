using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Runtime.InteropServices.WindowsRuntime;

/// <summary>Stores mappings between delegates and event tokens, to support the implementation of a Windows Runtime event in managed code.</summary>
/// <typeparam name="T">The type of the event handler delegate for a particular event. </typeparam>
public sealed class EventRegistrationTokenTable<T> where T : class
{
	private Dictionary<EventRegistrationToken, T> m_tokens = new Dictionary<EventRegistrationToken, T>();

	private volatile T m_invokeList;

	/// <summary>Gets or sets a delegate of type <paramref name="T" /> whose invocation list includes all the event handler delegates that have been added, and that have not yet been removed. Invoking this delegate invokes all the event handlers. </summary>
	/// <returns>A delegate of type <paramref name="T" /> that represents all the event handler delegates that are currently registered for an event. </returns>
	public T InvocationList
	{
		get
		{
			return m_invokeList;
		}
		set
		{
			lock (m_tokens)
			{
				m_tokens.Clear();
				m_invokeList = null;
				if (value != null)
				{
					AddEventHandlerNoLock(value);
				}
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.WindowsRuntime.EventRegistrationTokenTable`1" /> class. </summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="T" /> is not a delegate type. </exception>
	public EventRegistrationTokenTable()
	{
		if (!typeof(Delegate).IsAssignableFrom(typeof(T)))
		{
			throw new InvalidOperationException(Environment.GetResourceString("Type '{0}' is not a delegate type.  EventTokenTable may only be used with delegate types.", typeof(T)));
		}
	}

	/// <summary>Adds the specified event handler to the table and to the invocation list, and returns a token that can be used to remove the event handler. </summary>
	/// <returns>A token that can be used to remove the event handler from the table and the invocation list. </returns>
	/// <param name="handler">The event handler to add. </param>
	public EventRegistrationToken AddEventHandler(T handler)
	{
		if (handler == null)
		{
			return new EventRegistrationToken(0uL);
		}
		lock (m_tokens)
		{
			return AddEventHandlerNoLock(handler);
		}
	}

	private EventRegistrationToken AddEventHandlerNoLock(T handler)
	{
		EventRegistrationToken eventRegistrationToken = GetPreferredToken(handler);
		while (m_tokens.ContainsKey(eventRegistrationToken))
		{
			eventRegistrationToken = new EventRegistrationToken(eventRegistrationToken.Value + 1);
		}
		m_tokens[eventRegistrationToken] = handler;
		Delegate a = (Delegate)(object)m_invokeList;
		a = Delegate.Combine(a, (Delegate)(object)handler);
		m_invokeList = (T)(object)a;
		return eventRegistrationToken;
	}

	[FriendAccessAllowed]
	internal T ExtractHandler(EventRegistrationToken token)
	{
		T value = null;
		lock (m_tokens)
		{
			if (m_tokens.TryGetValue(token, out value))
			{
				RemoveEventHandlerNoLock(token);
			}
		}
		return value;
	}

	private static EventRegistrationToken GetPreferredToken(T handler)
	{
		uint num = 0u;
		Delegate[] invocationList = ((Delegate)(object)handler).GetInvocationList();
		num = (uint)((invocationList.Length != 1) ? handler.GetHashCode() : invocationList[0].Method.GetHashCode());
		return new EventRegistrationToken(((ulong)(uint)typeof(T).MetadataToken << 32) | num);
	}

	/// <summary>Removes the event handler that is associated with the specified token from the table and the invocation list. </summary>
	/// <param name="token">The token that was returned when the event handler was added. </param>
	public void RemoveEventHandler(EventRegistrationToken token)
	{
		if (token.Value == 0L)
		{
			return;
		}
		lock (m_tokens)
		{
			RemoveEventHandlerNoLock(token);
		}
	}

	/// <summary>Removes the specified event handler delegate from the table and the invocation list. </summary>
	/// <param name="handler">The event handler to remove. </param>
	public void RemoveEventHandler(T handler)
	{
		if (handler == null)
		{
			return;
		}
		lock (m_tokens)
		{
			EventRegistrationToken preferredToken = GetPreferredToken(handler);
			if (m_tokens.TryGetValue(preferredToken, out var value) && value == handler)
			{
				RemoveEventHandlerNoLock(preferredToken);
				return;
			}
			foreach (KeyValuePair<EventRegistrationToken, T> token in m_tokens)
			{
				if (token.Value == (T)handler)
				{
					RemoveEventHandlerNoLock(token.Key);
					break;
				}
			}
		}
	}

	private void RemoveEventHandlerNoLock(EventRegistrationToken token)
	{
		if (m_tokens.TryGetValue(token, out var value))
		{
			m_tokens.Remove(token);
			Delegate source = (Delegate)(object)m_invokeList;
			source = Delegate.Remove(source, (Delegate)(object)value);
			m_invokeList = (T)(object)source;
		}
	}

	/// <summary>Returns the specified event registration token table, if it is not null; otherwise, returns a new event registration token table. </summary>
	/// <returns>The event registration token table that is specified by <paramref name="refEventTable" />, if it is not null; otherwise, a new event registration token table. </returns>
	/// <param name="refEventTable">An event registration token table, passed by reference. </param>
	public static EventRegistrationTokenTable<T> GetOrCreateEventRegistrationTokenTable(ref EventRegistrationTokenTable<T> refEventTable)
	{
		if (refEventTable == null)
		{
			Interlocked.CompareExchange(ref refEventTable, new EventRegistrationTokenTable<T>(), null);
		}
		return refEventTable;
	}
}
