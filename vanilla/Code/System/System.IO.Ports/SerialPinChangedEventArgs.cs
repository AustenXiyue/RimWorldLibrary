using Unity;

namespace System.IO.Ports;

/// <summary>Provides data for the <see cref="E:System.IO.Ports.SerialPort.PinChanged" /> event.</summary>
public class SerialPinChangedEventArgs : EventArgs
{
	private SerialPinChange eventType;

	/// <summary>Gets or sets the event type.</summary>
	/// <returns>One of the <see cref="T:System.IO.Ports.SerialPinChange" /> values.</returns>
	public SerialPinChange EventType => eventType;

	internal SerialPinChangedEventArgs(SerialPinChange eventType)
	{
		this.eventType = eventType;
	}

	internal SerialPinChangedEventArgs()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
