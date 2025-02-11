namespace System.Windows.Media;

/// <summary>Provides error exception data for media events.</summary>
public sealed class ExceptionEventArgs : EventArgs
{
	private Exception _errorException;

	/// <summary>Gets the exception that details the cause of the failure.</summary>
	/// <returns>The exception that details the error condition.</returns>
	/// <exception cref="T:System.Security.SecurityException">The attempt to access the media or image file is denied.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The media or image file is not found.</exception>
	/// <exception cref="T:System.IO.FileFormatException">The media or image format is not supported by any installed codec.-or-The file format is not recognized.</exception>
	/// <exception cref="T:System.Windows.Media.InvalidWmpVersionException">The detected version of Microsoft Windows Media Player is not supported.</exception>
	/// <exception cref="T:System.NotSupportedException">The operation is not supported.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">A COM error code appears. </exception>
	public Exception ErrorException => _errorException;

	internal ExceptionEventArgs(Exception errorException)
	{
		if (errorException == null)
		{
			throw new ArgumentNullException("errorException");
		}
		_errorException = errorException;
	}
}
