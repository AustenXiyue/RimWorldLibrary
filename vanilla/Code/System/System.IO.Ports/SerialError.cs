namespace System.IO.Ports;

/// <summary>Specifies errors that occur on the <see cref="T:System.IO.Ports.SerialPort" /> object.</summary>
public enum SerialError
{
	/// <summary>An input buffer overflow has occurred. There is either no room in the input buffer, or a character was received after the end-of-file (EOF) character.</summary>
	RXOver = 1,
	/// <summary>A character-buffer overrun has occurred. The next character is lost.</summary>
	Overrun = 2,
	/// <summary>The hardware detected a parity error.</summary>
	RXParity = 4,
	/// <summary>The hardware detected a framing error.</summary>
	Frame = 8,
	/// <summary>The application tried to transmit a character, but the output buffer was full.</summary>
	TXFull = 0x100
}
