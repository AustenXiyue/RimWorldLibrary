using System.Collections.Generic;
using Unity;

namespace System.Runtime.Serialization;

/// <summary>Provides data for the <see cref="T:System.Exception.SerializeObjectState" /> event.</summary>
public sealed class SafeSerializationEventArgs : EventArgs
{
	private StreamingContext m_streamingContext;

	private List<object> m_serializedStates;

	internal IList<object> SerializedStates => m_serializedStates;

	/// <summary>Gets or sets an object that describes the source and destination of a serialized stream.</summary>
	/// <returns>An object that describes the source and destination of a serialized stream.</returns>
	public StreamingContext StreamingContext => m_streamingContext;

	internal SafeSerializationEventArgs(StreamingContext streamingContext)
	{
		m_serializedStates = new List<object>();
		base._002Ector();
		m_streamingContext = streamingContext;
	}

	/// <summary>Stores the state of the exception.</summary>
	/// <param name="serializedState">A state object that is serialized with the instance.</param>
	public void AddSerializedState(ISafeSerializationData serializedState)
	{
		if (serializedState == null)
		{
			throw new ArgumentNullException("serializedState");
		}
		if (!serializedState.GetType().IsSerializable)
		{
			throw new ArgumentException(Environment.GetResourceString("Type '{0}' in Assembly '{1}' is not marked as serializable.", serializedState.GetType(), serializedState.GetType().Assembly.FullName));
		}
		m_serializedStates.Add(serializedState);
	}

	internal SafeSerializationEventArgs()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
