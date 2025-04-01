using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Controls the permissions related to user interfaces and the Clipboard. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class UIPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission
{
	private UIPermissionWindow _window;

	private UIPermissionClipboard _clipboard;

	private const int version = 1;

	/// <summary>Gets or sets the Clipboard access represented by the permission.</summary>
	/// <returns>One of the <see cref="T:System.Security.Permissions.UIPermissionClipboard" /> values.</returns>
	public UIPermissionClipboard Clipboard
	{
		get
		{
			return _clipboard;
		}
		set
		{
			if (!Enum.IsDefined(typeof(UIPermissionClipboard), value))
			{
				throw new ArgumentException(string.Format(Locale.GetText("Invalid enum {0}"), value), "UIPermissionClipboard");
			}
			_clipboard = value;
		}
	}

	/// <summary>Gets or sets the window access represented by the permission.</summary>
	/// <returns>One of the <see cref="T:System.Security.Permissions.UIPermissionWindow" /> values.</returns>
	public UIPermissionWindow Window
	{
		get
		{
			return _window;
		}
		set
		{
			if (!Enum.IsDefined(typeof(UIPermissionWindow), value))
			{
				throw new ArgumentException(string.Format(Locale.GetText("Invalid enum {0}"), value), "UIPermissionWindow");
			}
			_window = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.UIPermission" /> class with either fully restricted or unrestricted access, as specified.</summary>
	/// <param name="state">One of the enumeration values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="state" /> parameter is not a valid <see cref="T:System.Security.Permissions.PermissionState" />. </exception>
	public UIPermission(PermissionState state)
	{
		if (CodeAccessPermission.CheckPermissionState(state, allowUnrestricted: true) == PermissionState.Unrestricted)
		{
			_clipboard = UIPermissionClipboard.AllClipboard;
			_window = UIPermissionWindow.AllWindows;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.UIPermission" /> class with the permissions for the Clipboard, and no access to windows.</summary>
	/// <param name="clipboardFlag">One of the enumeration values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="clipboardFlag" /> parameter is not a valid <see cref="T:System.Security.Permissions.UIPermissionClipboard" /> value. </exception>
	public UIPermission(UIPermissionClipboard clipboardFlag)
	{
		Clipboard = clipboardFlag;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.UIPermission" /> class with the permissions for windows, and no access to the Clipboard.</summary>
	/// <param name="windowFlag">One of the enumeration values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="windowFlag" /> parameter is not a valid <see cref="T:System.Security.Permissions.UIPermissionWindow" /> value. </exception>
	public UIPermission(UIPermissionWindow windowFlag)
	{
		Window = windowFlag;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.UIPermission" /> class with the specified permissions for windows and the Clipboard.</summary>
	/// <param name="windowFlag">One of the enumeration values. </param>
	/// <param name="clipboardFlag">One of the enumeration values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="windowFlag" /> parameter is not a valid <see cref="T:System.Security.Permissions.UIPermissionWindow" /> value.-or- The <paramref name="clipboardFlag" /> parameter is not a valid <see cref="T:System.Security.Permissions.UIPermissionClipboard" /> value. </exception>
	public UIPermission(UIPermissionWindow windowFlag, UIPermissionClipboard clipboardFlag)
	{
		Clipboard = clipboardFlag;
		Window = windowFlag;
	}

	/// <summary>Creates and returns an identical copy of the current permission.</summary>
	/// <returns>A copy of the current permission.</returns>
	public override IPermission Copy()
	{
		return new UIPermission(_window, _clipboard);
	}

	/// <summary>Reconstructs a permission with a specified state from an XML encoding.</summary>
	/// <param name="esd">The XML encoding used to reconstruct the permission. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="esd" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="esd" /> parameter is not a valid permission element.-or- The <paramref name="esd" /> parameter's version number is not valid. </exception>
	public override void FromXml(SecurityElement esd)
	{
		CodeAccessPermission.CheckSecurityElement(esd, "esd", 1, 1);
		if (CodeAccessPermission.IsUnrestricted(esd))
		{
			_window = UIPermissionWindow.AllWindows;
			_clipboard = UIPermissionClipboard.AllClipboard;
			return;
		}
		string text = esd.Attribute("Window");
		if (text == null)
		{
			_window = UIPermissionWindow.NoWindows;
		}
		else
		{
			_window = (UIPermissionWindow)Enum.Parse(typeof(UIPermissionWindow), text);
		}
		string text2 = esd.Attribute("Clipboard");
		if (text2 == null)
		{
			_clipboard = UIPermissionClipboard.NoClipboard;
		}
		else
		{
			_clipboard = (UIPermissionClipboard)Enum.Parse(typeof(UIPermissionClipboard), text2);
		}
	}

	/// <summary>Creates and returns a permission that is the intersection of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the intersection of the current permission and the specified permission. This new permission is null if the intersection is empty.</returns>
	/// <param name="target">A permission to intersect with the current permission. It must be the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	public override IPermission Intersect(IPermission target)
	{
		UIPermission uIPermission = Cast(target);
		if (uIPermission == null)
		{
			return null;
		}
		UIPermissionWindow uIPermissionWindow = ((_window < uIPermission._window) ? _window : uIPermission._window);
		UIPermissionClipboard uIPermissionClipboard = ((_clipboard < uIPermission._clipboard) ? _clipboard : uIPermission._clipboard);
		if (IsEmpty(uIPermissionWindow, uIPermissionClipboard))
		{
			return null;
		}
		return new UIPermission(uIPermissionWindow, uIPermissionClipboard);
	}

	/// <summary>Determines whether the current permission is a subset of the specified permission.</summary>
	/// <returns>true if the current permission is a subset of the specified permission; otherwise, false.</returns>
	/// <param name="target">A permission to test for the subset relationship. This permission must be the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	public override bool IsSubsetOf(IPermission target)
	{
		UIPermission uIPermission = Cast(target);
		if (uIPermission == null)
		{
			return IsEmpty(_window, _clipboard);
		}
		if (uIPermission.IsUnrestricted())
		{
			return true;
		}
		if (_window <= uIPermission._window)
		{
			return _clipboard <= uIPermission._clipboard;
		}
		return false;
	}

	/// <summary>Returns a value indicating whether the current permission is unrestricted.</summary>
	/// <returns>true if the current permission is unrestricted; otherwise, false.</returns>
	public bool IsUnrestricted()
	{
		if (_window == UIPermissionWindow.AllWindows)
		{
			return _clipboard == UIPermissionClipboard.AllClipboard;
		}
		return false;
	}

	/// <summary>Creates an XML encoding of the permission and its current state.</summary>
	/// <returns>An XML encoding of the permission, including any state information.</returns>
	public override SecurityElement ToXml()
	{
		SecurityElement securityElement = Element(1);
		if (_window == UIPermissionWindow.AllWindows && _clipboard == UIPermissionClipboard.AllClipboard)
		{
			securityElement.AddAttribute("Unrestricted", "true");
		}
		else
		{
			if (_window != 0)
			{
				securityElement.AddAttribute("Window", _window.ToString());
			}
			if (_clipboard != 0)
			{
				securityElement.AddAttribute("Clipboard", _clipboard.ToString());
			}
		}
		return securityElement;
	}

	/// <summary>Creates a permission that is the union of the permission and the specified permission.</summary>
	/// <returns>A new permission that represents the union of the current permission and the specified permission.</returns>
	/// <param name="target">A permission to combine with the current permission. It must be the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	public override IPermission Union(IPermission target)
	{
		UIPermission uIPermission = Cast(target);
		if (uIPermission == null)
		{
			return Copy();
		}
		UIPermissionWindow uIPermissionWindow = ((_window > uIPermission._window) ? _window : uIPermission._window);
		UIPermissionClipboard uIPermissionClipboard = ((_clipboard > uIPermission._clipboard) ? _clipboard : uIPermission._clipboard);
		if (IsEmpty(uIPermissionWindow, uIPermissionClipboard))
		{
			return null;
		}
		return new UIPermission(uIPermissionWindow, uIPermissionClipboard);
	}

	int IBuiltInPermission.GetTokenIndex()
	{
		return 7;
	}

	private bool IsEmpty(UIPermissionWindow w, UIPermissionClipboard c)
	{
		if (w == UIPermissionWindow.NoWindows)
		{
			return c == UIPermissionClipboard.NoClipboard;
		}
		return false;
	}

	private UIPermission Cast(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		UIPermission obj = target as UIPermission;
		if (obj == null)
		{
			CodeAccessPermission.ThrowInvalidPermission(target, typeof(UIPermission));
		}
		return obj;
	}
}
