using Unity;

namespace System.IO.Ports;

/// <summary>Provides data for the <see cref="E:System.IO.Ports.SerialPort.DataReceived" /> event.</summary>
public class SerialDataReceivedEventArgs : EventArgs
{
	private SerialData eventType;

	/// <summary>Gets or sets the event type.</summary>
	/// <returns>One of the <see cref="T:System.IO.Ports.SerialData" /> values.</returns>
	public SerialData EventType => eventType;

	internal SerialDataReceivedEventArgs(SerialData eventType)
	{
		this.eventType = eventType;
	}

	internal SerialDataReceivedEventArgs()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
