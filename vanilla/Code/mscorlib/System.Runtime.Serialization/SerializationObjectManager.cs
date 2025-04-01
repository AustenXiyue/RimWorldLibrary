using System.Collections;
using System.Security;

namespace System.Runtime.Serialization;

/// <summary>Manages serialization processes at run time. This class cannot be inherited.</summary>
public sealed class SerializationObjectManager
{
	private Hashtable m_objectSeenTable = new Hashtable();

	private SerializationEventHandler m_onSerializedHandler;

	private StreamingContext m_context;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.SerializationObjectManager" /> class. </summary>
	/// <param name="context">An instance of the <see cref="T:System.Runtime.Serialization.StreamingContext" /> class that contains information about the current serialization operation.</param>
	public SerializationObjectManager(StreamingContext context)
	{
		m_context = context;
		m_objectSeenTable = new Hashtable();
	}

	/// <summary>Registers the object upon which events will be raised.</summary>
	/// <param name="obj">The object to register.</param>
	[SecurityCritical]
	public void RegisterObject(object obj)
	{
		SerializationEvents serializationEventsForType = SerializationEventsCache.GetSerializationEventsForType(obj.GetType());
		if (serializationEventsForType.HasOnSerializingEvents && m_objectSeenTable[obj] == null)
		{
			m_objectSeenTable[obj] = true;
			serializationEventsForType.InvokeOnSerializing(obj, m_context);
			AddOnSerialized(obj);
		}
	}

	/// <summary>Invokes the OnSerializing callback event if the type of the object has one; and registers the object for raising the OnSerialized event if the type of the object has one.</summary>
	public void RaiseOnSerializedEvent()
	{
		if (m_onSerializedHandler != null)
		{
			m_onSerializedHandler(m_context);
		}
	}

	[SecuritySafeCritical]
	private void AddOnSerialized(object obj)
	{
		SerializationEvents serializationEventsForType = SerializationEventsCache.GetSerializationEventsForType(obj.GetType());
		m_onSerializedHandler = serializationEventsForType.AddOnSerialized(obj, m_onSerializedHandler);
	}
}
