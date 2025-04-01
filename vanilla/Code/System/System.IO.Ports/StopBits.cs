namespace System.IO.Ports;

/// <summary>Specifies the number of stop bits used on the <see cref="T:System.IO.Ports.SerialPort" /> object.</summary>
public enum StopBits
{
	/// <summary>No stop bits are used. This value is not supported by the <see cref="P:System.IO.Ports.SerialPort.StopBits" /> property. </summary>
	None,
	/// <summary>One stop bit is used.</summary>
	One,
	/// <summary>Two stop bits are used.</summary>
	Two,
	/// <summary>1.5 stop bits are used.</summary>
	OnePointFive
}
