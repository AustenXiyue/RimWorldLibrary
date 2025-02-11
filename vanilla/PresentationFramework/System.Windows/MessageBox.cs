using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using MS.Win32;

namespace System.Windows;

/// <summary>Displays a message box. </summary>
public sealed class MessageBox
{
	private const int IDOK = 1;

	private const int IDCANCEL = 2;

	private const int IDABORT = 3;

	private const int IDRETRY = 4;

	private const int IDIGNORE = 5;

	private const int IDYES = 6;

	private const int IDNO = 7;

	private const int DEFAULT_BUTTON1 = 0;

	private const int DEFAULT_BUTTON2 = 256;

	private const int DEFAULT_BUTTON3 = 512;

	private MessageBox()
	{
	}

	private static MessageBoxResult Win32ToMessageBoxResult(int value)
	{
		return value switch
		{
			1 => MessageBoxResult.OK, 
			2 => MessageBoxResult.Cancel, 
			6 => MessageBoxResult.Yes, 
			7 => MessageBoxResult.No, 
			_ => MessageBoxResult.No, 
		};
	}

	/// <summary>Displays a message box that has a message, title bar caption, button, and icon; and that accepts a default message box result, complies with the specified options, and returns a result.</summary>
	/// <returns>A <see cref="T:System.Windows.MessageBoxResult" /> value that specifies which message box button is clicked by the user.</returns>
	/// <param name="messageBoxText">A <see cref="T:System.String" /> that specifies the text to display.</param>
	/// <param name="caption">A <see cref="T:System.String" /> that specifies the title bar caption to display.</param>
	/// <param name="button">A <see cref="T:System.Windows.MessageBoxButton" /> value that specifies which button or buttons to display.</param>
	/// <param name="icon">A <see cref="T:System.Windows.MessageBoxImage" /> value that specifies the icon to display.</param>
	/// <param name="defaultResult">A <see cref="T:System.Windows.MessageBoxResult" /> value that specifies the default result of the message box.</param>
	/// <param name="options">A <see cref="T:System.Windows.MessageBoxOptions" /> value object that specifies the options.</param>
	public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
	{
		return ShowCore(IntPtr.Zero, messageBoxText, caption, button, icon, defaultResult, options);
	}

	/// <summary>Displays a message box that has a message, title bar caption, button, and icon; and that accepts a default message box result and returns a result.</summary>
	/// <returns>A <see cref="T:System.Windows.MessageBoxResult" /> value that specifies which message box button is clicked by the user.</returns>
	/// <param name="messageBoxText">A <see cref="T:System.String" /> that specifies the text to display.</param>
	/// <param name="caption">A <see cref="T:System.String" /> that specifies the title bar caption to display.</param>
	/// <param name="button">A <see cref="T:System.Windows.MessageBoxButton" /> value that specifies which button or buttons to display.</param>
	/// <param name="icon">A <see cref="T:System.Windows.MessageBoxImage" /> value that specifies the icon to display.</param>
	/// <param name="defaultResult">A <see cref="T:System.Windows.MessageBoxResult" /> value that specifies the default result of the message box.</param>
	public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
	{
		return ShowCore(IntPtr.Zero, messageBoxText, caption, button, icon, defaultResult, MessageBoxOptions.None);
	}

	/// <summary>Displays a message box that has a message, title bar caption, button, and icon; and that returns a result.</summary>
	/// <returns>A <see cref="T:System.Windows.MessageBoxResult" /> value that specifies which message box button is clicked by the user.</returns>
	/// <param name="messageBoxText">A <see cref="T:System.String" /> that specifies the text to display.</param>
	/// <param name="caption">A <see cref="T:System.String" /> that specifies the title bar caption to display.</param>
	/// <param name="button">A <see cref="T:System.Windows.MessageBoxButton" /> value that specifies which button or buttons to display.</param>
	/// <param name="icon">A <see cref="T:System.Windows.MessageBoxImage" /> value that specifies the icon to display.</param>
	public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
	{
		return ShowCore(IntPtr.Zero, messageBoxText, caption, button, icon, MessageBoxResult.None, MessageBoxOptions.None);
	}

	/// <summary>Displays a message box that has a message, title bar caption, and button; and that returns a result.</summary>
	/// <returns>A <see cref="T:System.Windows.MessageBoxResult" /> value that specifies which message box button is clicked by the user.</returns>
	/// <param name="messageBoxText">A <see cref="T:System.String" /> that specifies the text to display.</param>
	/// <param name="caption">A <see cref="T:System.String" /> that specifies the title bar caption to display.</param>
	/// <param name="button">A <see cref="T:System.Windows.MessageBoxButton" /> value that specifies which button or buttons to display.</param>
	public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button)
	{
		return ShowCore(IntPtr.Zero, messageBoxText, caption, button, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None);
	}

	/// <summary>Displays a message box that has a message and title bar caption; and that returns a result.</summary>
	/// <returns>A <see cref="T:System.Windows.MessageBoxResult" /> value that specifies which message box button is clicked by the user.</returns>
	/// <param name="messageBoxText">A <see cref="T:System.String" /> that specifies the text to display.</param>
	/// <param name="caption">A <see cref="T:System.String" /> that specifies the title bar caption to display.</param>
	public static MessageBoxResult Show(string messageBoxText, string caption)
	{
		return ShowCore(IntPtr.Zero, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None);
	}

	/// <summary>Displays a message box that has a message and that returns a result.</summary>
	/// <returns>A <see cref="T:System.Windows.MessageBoxResult" /> value that specifies which message box button is clicked by the user.</returns>
	/// <param name="messageBoxText">A <see cref="T:System.String" /> that specifies the text to display.</param>
	public static MessageBoxResult Show(string messageBoxText)
	{
		return ShowCore(IntPtr.Zero, messageBoxText, string.Empty, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None);
	}

	/// <summary>Displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; and accepts a default message box result, complies with the specified options, and returns a result.</summary>
	/// <returns>A <see cref="T:System.Windows.MessageBoxResult" /> value that specifies which message box button is clicked by the user.</returns>
	/// <param name="owner">A <see cref="T:System.Windows.Window" /> that represents the owner window of the message box.</param>
	/// <param name="messageBoxText">A <see cref="T:System.String" /> that specifies the text to display.</param>
	/// <param name="caption">A <see cref="T:System.String" /> that specifies the title bar caption to display.</param>
	/// <param name="button">A <see cref="T:System.Windows.MessageBoxButton" /> value that specifies which button or buttons to display.</param>
	/// <param name="icon">A <see cref="T:System.Windows.MessageBoxImage" /> value that specifies the icon to display.</param>
	/// <param name="defaultResult">A <see cref="T:System.Windows.MessageBoxResult" /> value that specifies the default result of the message box.</param>
	/// <param name="options">A <see cref="T:System.Windows.MessageBoxOptions" /> value object that specifies the options.</param>
	public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
	{
		return ShowCore(new WindowInteropHelper(owner).CriticalHandle, messageBoxText, caption, button, icon, defaultResult, options);
	}

	/// <summary>Displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; and accepts a default message box result and returns a result.</summary>
	/// <returns>A <see cref="T:System.Windows.MessageBoxResult" /> value that specifies which message box button is clicked by the user.</returns>
	/// <param name="owner">A <see cref="T:System.Windows.Window" /> that represents the owner window of the message box.</param>
	/// <param name="messageBoxText">A <see cref="T:System.String" /> that specifies the text to display.</param>
	/// <param name="caption">A <see cref="T:System.String" /> that specifies the title bar caption to display.</param>
	/// <param name="button">A <see cref="T:System.Windows.MessageBoxButton" /> value that specifies which button or buttons to display.</param>
	/// <param name="icon">A <see cref="T:System.Windows.MessageBoxImage" /> value that specifies the icon to display.</param>
	/// <param name="defaultResult">A <see cref="T:System.Windows.MessageBoxResult" /> value that specifies the default result of the message box.</param>
	public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
	{
		return ShowCore(new WindowInteropHelper(owner).CriticalHandle, messageBoxText, caption, button, icon, defaultResult, MessageBoxOptions.None);
	}

	/// <summary>Displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; and it also returns a result.</summary>
	/// <returns>A <see cref="T:System.Windows.MessageBoxResult" /> value that specifies which message box button is clicked by the user.</returns>
	/// <param name="owner">A <see cref="T:System.Windows.Window" /> that represents the owner window of the message box.</param>
	/// <param name="messageBoxText">A <see cref="T:System.String" /> that specifies the text to display.</param>
	/// <param name="caption">A <see cref="T:System.String" /> that specifies the title bar caption to display.</param>
	/// <param name="button">A <see cref="T:System.Windows.MessageBoxButton" /> value that specifies which button or buttons to display.</param>
	/// <param name="icon">A <see cref="T:System.Windows.MessageBoxImage" /> value that specifies the icon to display.</param>
	public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
	{
		return ShowCore(new WindowInteropHelper(owner).CriticalHandle, messageBoxText, caption, button, icon, MessageBoxResult.None, MessageBoxOptions.None);
	}

	/// <summary>Displays a message box in front of the specified window. The message box displays a message, title bar caption, and button; and it also returns a result.</summary>
	/// <returns>A <see cref="T:System.Windows.MessageBoxResult" /> value that specifies which message box button is clicked by the user.</returns>
	/// <param name="owner">A <see cref="T:System.Windows.Window" /> that represents the owner window of the message box.</param>
	/// <param name="messageBoxText">A <see cref="T:System.String" /> that specifies the text to display.</param>
	/// <param name="caption">A <see cref="T:System.String" /> that specifies the title bar caption to display.</param>
	/// <param name="button">A <see cref="T:System.Windows.MessageBoxButton" /> value that specifies which button or buttons to display.</param>
	public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button)
	{
		return ShowCore(new WindowInteropHelper(owner).CriticalHandle, messageBoxText, caption, button, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None);
	}

	/// <summary>Displays a message box in front of the specified window. The message box displays a message and title bar caption; and it returns a result.</summary>
	/// <returns>A <see cref="T:System.Windows.MessageBoxResult" /> value that specifies which message box button is clicked by the user.</returns>
	/// <param name="owner">A <see cref="T:System.Windows.Window" /> that represents the owner window of the message box.</param>
	/// <param name="messageBoxText">A <see cref="T:System.String" /> that specifies the text to display.</param>
	/// <param name="caption">A <see cref="T:System.String" /> that specifies the title bar caption to display.</param>
	public static MessageBoxResult Show(Window owner, string messageBoxText, string caption)
	{
		return ShowCore(new WindowInteropHelper(owner).CriticalHandle, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None);
	}

	/// <summary>Displays a message box in front of the specified window. The message box displays a message and returns a result.</summary>
	/// <returns>A <see cref="T:System.Windows.MessageBoxResult" /> value that specifies which message box button is clicked by the user.</returns>
	/// <param name="owner">A <see cref="T:System.Windows.Window" /> that represents the owner window of the message box.</param>
	/// <param name="messageBoxText">A <see cref="T:System.String" /> that specifies the text to display.</param>
	public static MessageBoxResult Show(Window owner, string messageBoxText)
	{
		return ShowCore(new WindowInteropHelper(owner).CriticalHandle, messageBoxText, string.Empty, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None);
	}

	private static int DefaultResultToButtonNumber(MessageBoxResult result, MessageBoxButton button)
	{
		if (result == MessageBoxResult.None)
		{
			return 0;
		}
		switch (button)
		{
		case MessageBoxButton.OK:
			return 0;
		case MessageBoxButton.OKCancel:
			if (result == MessageBoxResult.Cancel)
			{
				return 256;
			}
			return 0;
		case MessageBoxButton.YesNo:
			if (result == MessageBoxResult.No)
			{
				return 256;
			}
			return 0;
		case MessageBoxButton.YesNoCancel:
			return result switch
			{
				MessageBoxResult.No => 256, 
				MessageBoxResult.Cancel => 512, 
				_ => 0, 
			};
		default:
			return 0;
		}
	}

	internal static MessageBoxResult ShowCore(nint owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
	{
		if (!IsValidMessageBoxButton(button))
		{
			throw new InvalidEnumArgumentException("button", (int)button, typeof(MessageBoxButton));
		}
		if (!IsValidMessageBoxImage(icon))
		{
			throw new InvalidEnumArgumentException("icon", (int)icon, typeof(MessageBoxImage));
		}
		if (!IsValidMessageBoxResult(defaultResult))
		{
			throw new InvalidEnumArgumentException("defaultResult", (int)defaultResult, typeof(MessageBoxResult));
		}
		if (!IsValidMessageBoxOptions(options))
		{
			throw new InvalidEnumArgumentException("options", (int)options, typeof(MessageBoxOptions));
		}
		if ((options & (MessageBoxOptions.ServiceNotification | MessageBoxOptions.DefaultDesktopOnly)) != 0)
		{
			if (owner != IntPtr.Zero)
			{
				throw new ArgumentException(SR.CantShowMBServiceWithOwner);
			}
		}
		else if (owner == IntPtr.Zero)
		{
			owner = MS.Win32.UnsafeNativeMethods.GetActiveWindow();
		}
		int type = (int)((uint)button | (uint)icon | (uint)DefaultResultToButtonNumber(defaultResult, button)) | (int)options;
		return Win32ToMessageBoxResult(MS.Win32.UnsafeNativeMethods.MessageBox(new HandleRef(null, owner), messageBoxText, caption, type));
	}

	private static bool IsValidMessageBoxButton(MessageBoxButton value)
	{
		if (value != 0 && value != MessageBoxButton.OKCancel && value != MessageBoxButton.YesNo)
		{
			return value == MessageBoxButton.YesNoCancel;
		}
		return true;
	}

	private static bool IsValidMessageBoxImage(MessageBoxImage value)
	{
		if (value != MessageBoxImage.Asterisk && value != MessageBoxImage.Hand && value != MessageBoxImage.Exclamation && value != MessageBoxImage.Hand && value != MessageBoxImage.Asterisk && value != 0 && value != MessageBoxImage.Question && value != MessageBoxImage.Hand)
		{
			return value == MessageBoxImage.Exclamation;
		}
		return true;
	}

	private static bool IsValidMessageBoxResult(MessageBoxResult value)
	{
		if (value != MessageBoxResult.Cancel && value != MessageBoxResult.No && value != 0 && value != MessageBoxResult.OK)
		{
			return value == MessageBoxResult.Yes;
		}
		return true;
	}

	private static bool IsValidMessageBoxOptions(MessageBoxOptions value)
	{
		int num = -3801089;
		if (((uint)value & (uint)num) == 0)
		{
			return true;
		}
		return false;
	}
}
