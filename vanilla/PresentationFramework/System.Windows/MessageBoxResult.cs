namespace System.Windows;

/// <summary>Specifies which message box button that a user clicks. <see cref="T:System.Windows.MessageBoxResult" /> is returned by the <see cref="Overload:System.Windows.MessageBox.Show" /> method.</summary>
public enum MessageBoxResult
{
	/// <summary>The message box returns no result.</summary>
	None = 0,
	/// <summary>The result value of the message box is OK.</summary>
	OK = 1,
	/// <summary>The result value of the message box is Cancel.</summary>
	Cancel = 2,
	/// <summary>The result value of the message box is Yes.</summary>
	Yes = 6,
	/// <summary>The result value of the message box is No.</summary>
	No = 7
}
