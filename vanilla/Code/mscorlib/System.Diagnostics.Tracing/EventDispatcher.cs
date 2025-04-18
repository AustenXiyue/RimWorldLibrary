namespace System.Diagnostics.Tracing;

internal class EventDispatcher
{
	internal readonly EventListener m_Listener;

	internal bool[] m_EventEnabled;

	internal bool m_activityFilteringEnabled;

	internal EventDispatcher m_Next;

	internal EventDispatcher(EventDispatcher next, bool[] eventEnabled, EventListener listener)
	{
		m_Next = next;
		m_EventEnabled = eventEnabled;
		m_Listener = listener;
	}
}
