using System.Reflection;
using System.Windows.Input;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Interop;

/// <summary>Contains the parameters that are used to create an <see cref="T:System.Windows.Interop.HwndSource" /> object using the <see cref="M:System.Windows.Interop.HwndSource.#ctor(System.Windows.Interop.HwndSourceParameters)" /> constructor.</summary>
public struct HwndSourceParameters
{
	private int _classStyleBits;

	private int _styleBits;

	private int _extendedStyleBits;

	private int _x;

	private int _y;

	private int _width;

	private int _height;

	private string _name;

	private nint _parent;

	private HwndSourceHook _hwndSourceHook;

	private bool _adjustSizingForNonClientArea;

	private bool _hasAssignedSize;

	private bool _usesPerPixelOpacity;

	private bool _usesPerPixelTransparency;

	private bool? _treatAsInputRoot;

	private bool _treatAncestorsAsNonClientArea;

	private RestoreFocusMode? _restoreFocusMode;

	private bool? _acquireHwndFocusInMenuMode;

	private static bool _platformSupportsTransparentChildWindows = Utilities.IsOSWindows8OrNewer;

	/// <summary>Gets or sets the Microsoft Windows class style for the window. </summary>
	/// <returns>The window class style. See Window Class Styles for detailed information. The default is 0 (no window class style).</returns>
	public int WindowClassStyle
	{
		get
		{
			return _classStyleBits;
		}
		set
		{
			_classStyleBits = value;
		}
	}

	/// <summary>Gets or sets the style for the window. </summary>
	/// <returns>The window style. See the CreateWindowEx function for a complete list of style bits. See Remarks for defaults.</returns>
	public int WindowStyle
	{
		get
		{
			return _styleBits;
		}
		set
		{
			_styleBits = value | 0x2000000;
		}
	}

	/// <summary>Gets or sets the extended Microsoft Windows styles for the window. </summary>
	/// <returns>The extended window styles. See CreateWindowEx for a list of these styles. The default is 0 (no extended window styles).</returns>
	public int ExtendedWindowStyle
	{
		get
		{
			return _extendedStyleBits;
		}
		set
		{
			_extendedStyleBits = value;
		}
	}

	/// <summary>Gets or sets the left-edge position of the window. </summary>
	/// <returns>The left-edge position of the window. The default is CW_USEDEFAULT, as processed by CreateWindow.</returns>
	public int PositionX
	{
		get
		{
			return _x;
		}
		set
		{
			_x = value;
		}
	}

	/// <summary>Gets or sets the upper-edge position of the window. </summary>
	/// <returns>The upper-edge position of the window. The default is CW_USEDEFAULT, as processed by CreateWindow.</returns>
	public int PositionY
	{
		get
		{
			return _y;
		}
		set
		{
			_y = value;
		}
	}

	/// <summary>Gets or sets a value that indicates the width of the window. </summary>
	/// <returns>The window width, in device pixels. The default value is 1.</returns>
	public int Width
	{
		get
		{
			return _width;
		}
		set
		{
			_width = value;
			_hasAssignedSize = true;
		}
	}

	/// <summary>Gets or sets a value that indicates the height of the window. </summary>
	/// <returns>The height of the window, in device pixels. The default value is 1.</returns>
	public int Height
	{
		get
		{
			return _height;
		}
		set
		{
			_height = value;
			_hasAssignedSize = true;
		}
	}

	/// <summary>Gets a value that indicates whether a size was assigned. </summary>
	/// <returns>true if the window size was assigned; otherwise, false. The default is false, unless the structure was created with provided height and width, in which case the value is true.</returns>
	public bool HasAssignedSize => _hasAssignedSize;

	/// <summary>Gets or sets the name of the window. </summary>
	/// <returns>The window name.</returns>
	public string WindowName
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	/// <summary>Gets or sets the window handle (HWND) of the parent for the created window. </summary>
	/// <returns>The HWND of the parent window.</returns>
	public nint ParentWindow
	{
		get
		{
			return _parent;
		}
		set
		{
			_parent = value;
		}
	}

	/// <summary>Gets or sets the message hook for the window. </summary>
	/// <returns>The message hook for the window.</returns>
	public HwndSourceHook HwndSourceHook
	{
		get
		{
			return _hwndSourceHook;
		}
		set
		{
			_hwndSourceHook = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether to include the nonclient area for sizing. </summary>
	/// <returns>true if the layout manager sizing logic should include the nonclient area; otherwise, false. The default is false.</returns>
	public bool AdjustSizingForNonClientArea
	{
		get
		{
			return _adjustSizingForNonClientArea;
		}
		set
		{
			_adjustSizingForNonClientArea = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the parent windows of the <see cref="T:System.Windows.Interop.HwndSource" /> should be considered the non-client area of the window during layout passes.</summary>
	/// <returns>true if parent windows of the <see cref="T:System.Windows.Interop.HwndSource" /> should be considered the non-client area of the window during layout passes.; otherwise, false. The default is false.</returns>
	public bool TreatAncestorsAsNonClientArea
	{
		get
		{
			return _treatAncestorsAsNonClientArea;
		}
		set
		{
			_treatAncestorsAsNonClientArea = value;
		}
	}

	/// <summary>Gets a value that declares whether the per-pixel opacity of the source window content is respected.</summary>
	/// <returns>true if using per-pixel opacity; otherwise, false.</returns>
	public bool UsesPerPixelOpacity
	{
		get
		{
			return _usesPerPixelOpacity;
		}
		set
		{
			_usesPerPixelOpacity = value;
		}
	}

	public bool UsesPerPixelTransparency
	{
		get
		{
			return _usesPerPixelTransparency;
		}
		set
		{
			_usesPerPixelTransparency = value;
		}
	}

	/// <summary>Gets or sets how WPF handles restoring focus to the window.</summary>
	/// <returns>One of the enumeration values that specifies how WPF handles restoring focus for the window. The default is <see cref="F:System.Windows.Input.RestoreFocusMode.Auto" />.</returns>
	public RestoreFocusMode RestoreFocusMode
	{
		get
		{
			return _restoreFocusMode ?? Keyboard.DefaultRestoreFocusMode;
		}
		set
		{
			_restoreFocusMode = value;
		}
	}

	/// <summary>Gets or sets the value that determines whether to acquire Win32 focus for the WPF containing window when an <see cref="T:System.Windows.Interop.HwndSource" /> is created.</summary>
	/// <returns>true to acquire Win32 focus for the WPF containing window when the user interacts with menus; otherwise, false. null to use the value of <see cref="P:System.Windows.Interop.HwndSource.DefaultAcquireHwndFocusInMenuMode" />. </returns>
	public bool AcquireHwndFocusInMenuMode
	{
		get
		{
			return _acquireHwndFocusInMenuMode ?? HwndSource.DefaultAcquireHwndFocusInMenuMode;
		}
		set
		{
			_acquireHwndFocusInMenuMode = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Interop.HwndSource" /> should receive window messages raised by the message pump via the <see cref="T:System.Windows.Interop.ComponentDispatcher" />. </summary>
	/// <returns>true if the <see cref="T:System.Windows.Interop.HwndSource" /> should receive window messages raised by the message pump via the <see cref="T:System.Windows.Interop.ComponentDispatcher" />; otherwise, false.  The default is true if the <see cref="T:System.Windows.Interop.HwndSource" /> corresponds to a top-level window; otherwise, the default is false.</returns>
	public bool TreatAsInputRoot
	{
		get
		{
			return _treatAsInputRoot ?? ((_styleBits & 0x40000000) == 0);
		}
		set
		{
			_treatAsInputRoot = value;
		}
	}

	internal bool EffectivePerPixelOpacity
	{
		get
		{
			if (_usesPerPixelTransparency)
			{
				if (_usesPerPixelOpacity)
				{
					throw new InvalidOperationException(SR.UsesPerPixelOpacityIsObsolete);
				}
				if (!PlatformSupportsTransparentChildWindows)
				{
					return (WindowStyle & 0x40000000) == 0;
				}
				return true;
			}
			if (_usesPerPixelOpacity)
			{
				return (WindowStyle & 0x40000000) == 0;
			}
			return false;
		}
	}

	internal static bool PlatformSupportsTransparentChildWindows => _platformSupportsTransparentChildWindows;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Interop.HwndSourceParameters" /> class with a specified window name. </summary>
	/// <param name="name">The window's name.</param>
	public HwndSourceParameters(string name)
	{
		this = default(HwndSourceParameters);
		_styleBits = 268435456;
		_styleBits |= 12582912;
		_styleBits |= 524288;
		_styleBits |= 262144;
		_styleBits |= 131072;
		_styleBits |= 65536;
		_styleBits |= 33554432;
		_width = 1;
		_height = 1;
		_x = int.MinValue;
		_y = int.MinValue;
		WindowName = name;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Interop.HwndSourceParameters" /> class with a specified window name and initial size. </summary>
	/// <param name="name">The window's name.</param>
	/// <param name="width">The window's width, in pixels.</param>
	/// <param name="height">The window's height, in pixels.</param>
	public HwndSourceParameters(string name, int width, int height)
		: this(name)
	{
		Width = width;
		Height = height;
	}

	/// <summary>Returns the hash code for this <see cref="T:System.Windows.Interop.HwndSourceParameters" /> instance. </summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Sets the values that are used for the screen position of the window for the <see cref="T:System.Windows.Interop.HwndSource" />. </summary>
	/// <param name="x">The position of the left edge of the window.</param>
	/// <param name="y">The position of the upper edge of the window.</param>
	public void SetPosition(int x, int y)
	{
		_x = x;
		_y = y;
	}

	/// <summary>Sets the values that are used for the window size of the <see cref="T:System.Windows.Interop.HwndSource" />. </summary>
	/// <param name="width">The width of the window, in device pixels.</param>
	/// <param name="height">The height of the window, in device pixels.</param>
	public void SetSize(int width, int height)
	{
		_width = width;
		_height = height;
		_hasAssignedSize = true;
	}

	/// <summary>Determines whether an <see cref="T:System.Windows.Interop.HwndSourceParameters" /> structure is equal to another <see cref="T:System.Windows.Interop.HwndSourceParameters" /> structure.</summary>
	/// <returns>true if the structures are equal; otherwise, false.</returns>
	/// <param name="a">The first <see cref="T:System.Windows.Interop.HwndSourceParameters" /> structure to compare.</param>
	/// <param name="b">The second <see cref="T:System.Windows.Interop.HwndSourceParameters" /> structure to compare.</param>
	public static bool operator ==(HwndSourceParameters a, HwndSourceParameters b)
	{
		return a.Equals(b);
	}

	/// <summary>Determines whether an <see cref="T:System.Windows.Interop.HwndSourceParameters" /> structure is not equal to another <see cref="T:System.Windows.Interop.HwndSourceParameters" /> structure.</summary>
	/// <returns>true if the structures are not equal; otherwise, false.</returns>
	/// <param name="a">The first <see cref="T:System.Windows.Interop.HwndSourceParameters" /> structure to compare.</param>
	/// <param name="b">The second <see cref="T:System.Windows.Interop.HwndSourceParameters" /> structure to compare.</param>
	public static bool operator !=(HwndSourceParameters a, HwndSourceParameters b)
	{
		return !a.Equals(b);
	}

	/// <summary>Determines whether this structure is equal to a specified object. </summary>
	/// <returns>true if the comparison is equal; otherwise, false.</returns>
	/// <param name="obj">The objects to be tested for equality.</param>
	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		return Equals((HwndSourceParameters)obj);
	}

	/// <summary>Determines whether this structure is equal to a specified <see cref="T:System.Windows.Interop.HwndSourceParameters" /> structure. </summary>
	/// <returns>true if the structures are equal; otherwise, false.</returns>
	/// <param name="obj">The structure to be tested for equality.</param>
	public bool Equals(HwndSourceParameters obj)
	{
		if (_classStyleBits == obj._classStyleBits && _styleBits == obj._styleBits && _extendedStyleBits == obj._extendedStyleBits && _x == obj._x && _y == obj._y && _width == obj._width && _height == obj._height && _name == obj._name && _parent == obj._parent && _hwndSourceHook == obj._hwndSourceHook && _adjustSizingForNonClientArea == obj._adjustSizingForNonClientArea && _hasAssignedSize == obj._hasAssignedSize && _usesPerPixelOpacity == obj._usesPerPixelOpacity)
		{
			return _usesPerPixelTransparency == obj._usesPerPixelTransparency;
		}
		return false;
	}

	internal static void SetPlatformSupportsTransparentChildWindowsForTestingOnly(bool value)
	{
		if (string.Equals(Assembly.GetEntryAssembly().GetName().Name, "drthwndsource", StringComparison.CurrentCultureIgnoreCase))
		{
			_platformSupportsTransparentChildWindows = value;
		}
	}
}
