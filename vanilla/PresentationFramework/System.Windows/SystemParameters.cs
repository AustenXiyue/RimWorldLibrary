using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.FontCache;
using MS.Internal.Interop;
using MS.Utility;
using MS.Win32;
using Standard;

namespace System.Windows;

/// <summary>Contains properties that you can use to query system settings. </summary>
public static class SystemParameters
{
	private enum CacheSlot
	{
		DpiX,
		FocusBorderWidth,
		FocusBorderHeight,
		HighContrast,
		MouseVanish,
		DropShadow,
		FlatMenu,
		WorkAreaInternal,
		WorkArea,
		IconMetrics,
		KeyboardCues,
		KeyboardDelay,
		KeyboardPreference,
		KeyboardSpeed,
		SnapToDefaultButton,
		WheelScrollLines,
		MouseHoverTime,
		MouseHoverHeight,
		MouseHoverWidth,
		MenuDropAlignment,
		MenuFade,
		MenuShowDelay,
		ComboBoxAnimation,
		ClientAreaAnimation,
		CursorShadow,
		GradientCaptions,
		HotTracking,
		ListBoxSmoothScrolling,
		MenuAnimation,
		SelectionFade,
		StylusHotTracking,
		ToolTipAnimation,
		ToolTipFade,
		UIEffects,
		MinimizeAnimation,
		Border,
		CaretWidth,
		ForegroundFlashCount,
		DragFullWindows,
		NonClientMetrics,
		ThinHorizontalBorderHeight,
		ThinVerticalBorderWidth,
		CursorWidth,
		CursorHeight,
		ThickHorizontalBorderHeight,
		ThickVerticalBorderWidth,
		MinimumHorizontalDragDistance,
		MinimumVerticalDragDistance,
		FixedFrameHorizontalBorderHeight,
		FixedFrameVerticalBorderWidth,
		FocusHorizontalBorderHeight,
		FocusVerticalBorderWidth,
		FullPrimaryScreenWidth,
		FullPrimaryScreenHeight,
		HorizontalScrollBarButtonWidth,
		HorizontalScrollBarHeight,
		HorizontalScrollBarThumbWidth,
		IconWidth,
		IconHeight,
		IconGridWidth,
		IconGridHeight,
		MaximizedPrimaryScreenWidth,
		MaximizedPrimaryScreenHeight,
		MaximumWindowTrackWidth,
		MaximumWindowTrackHeight,
		MenuCheckmarkWidth,
		MenuCheckmarkHeight,
		MenuButtonWidth,
		MenuButtonHeight,
		MinimumWindowWidth,
		MinimumWindowHeight,
		MinimizedWindowWidth,
		MinimizedWindowHeight,
		MinimizedGridWidth,
		MinimizedGridHeight,
		MinimumWindowTrackWidth,
		MinimumWindowTrackHeight,
		PrimaryScreenWidth,
		PrimaryScreenHeight,
		WindowCaptionButtonWidth,
		WindowCaptionButtonHeight,
		ResizeFrameHorizontalBorderHeight,
		ResizeFrameVerticalBorderWidth,
		SmallIconWidth,
		SmallIconHeight,
		SmallWindowCaptionButtonWidth,
		SmallWindowCaptionButtonHeight,
		VirtualScreenWidth,
		VirtualScreenHeight,
		VerticalScrollBarWidth,
		VerticalScrollBarButtonHeight,
		WindowCaptionHeight,
		KanjiWindowHeight,
		MenuBarHeight,
		VerticalScrollBarThumbHeight,
		IsImmEnabled,
		IsMediaCenter,
		IsMenuDropRightAligned,
		IsMiddleEastEnabled,
		IsMousePresent,
		IsMouseWheelPresent,
		IsPenWindows,
		IsRemotelyControlled,
		IsRemoteSession,
		ShowSounds,
		IsSlowMachine,
		SwapButtons,
		IsTabletPC,
		VirtualScreenLeft,
		VirtualScreenTop,
		PowerLineStatus,
		IsGlassEnabled,
		UxThemeName,
		UxThemeColor,
		WindowCornerRadius,
		WindowGlassColor,
		WindowGlassBrush,
		WindowNonClientFrameThickness,
		WindowResizeBorderThickness,
		NumSlots
	}

	private static BitArray _cacheValid = new BitArray(119);

	private static bool _isGlassEnabled;

	private static string _uxThemeName;

	private static string _uxThemeColor;

	private static CornerRadius _windowCornerRadius;

	private static Color _windowGlassColor;

	private static Brush _windowGlassBrush;

	private static Thickness _windowNonClientFrameThickness;

	private static Thickness _windowResizeBorderThickness;

	private static int _dpiX;

	private static bool _setDpiX = true;

	private static double _focusBorderWidth;

	private static double _focusBorderHeight;

	private static bool _highContrast;

	private static bool _mouseVanish;

	private static bool _dropShadow;

	private static bool _flatMenu;

	private static MS.Win32.NativeMethods.RECT _workAreaInternal;

	private static Rect _workArea;

	private static MS.Win32.NativeMethods.ICONMETRICS _iconMetrics;

	private static bool _keyboardCues;

	private static int _keyboardDelay;

	private static bool _keyboardPref;

	private static int _keyboardSpeed;

	private static bool _snapToDefButton;

	private static int _wheelScrollLines;

	private static int _mouseHoverTime;

	private static double _mouseHoverHeight;

	private static double _mouseHoverWidth;

	private static bool _menuDropAlignment;

	private static bool _menuFade;

	private static int _menuShowDelay;

	private static bool _comboBoxAnimation;

	private static bool _clientAreaAnimation;

	private static bool _cursorShadow;

	private static bool _gradientCaptions;

	private static bool _hotTracking;

	private static bool _listBoxSmoothScrolling;

	private static bool _menuAnimation;

	private static bool _selectionFade;

	private static bool _stylusHotTracking;

	private static bool _toolTipAnimation;

	private static bool _tooltipFade;

	private static bool _uiEffects;

	private static bool _minAnimation;

	private static int _border;

	private static double _caretWidth;

	private static bool _dragFullWindows;

	private static int _foregroundFlashCount;

	private static MS.Win32.NativeMethods.NONCLIENTMETRICS _ncm;

	private static double _thinHorizontalBorderHeight;

	private static double _thinVerticalBorderWidth;

	private static double _cursorWidth;

	private static double _cursorHeight;

	private static double _thickHorizontalBorderHeight;

	private static double _thickVerticalBorderWidth;

	private static double _minimumHorizontalDragDistance;

	private static double _minimumVerticalDragDistance;

	private static double _fixedFrameHorizontalBorderHeight;

	private static double _fixedFrameVerticalBorderWidth;

	private static double _focusHorizontalBorderHeight;

	private static double _focusVerticalBorderWidth;

	private static double _fullPrimaryScreenHeight;

	private static double _fullPrimaryScreenWidth;

	private static double _horizontalScrollBarHeight;

	private static double _horizontalScrollBarButtonWidth;

	private static double _horizontalScrollBarThumbWidth;

	private static double _iconWidth;

	private static double _iconHeight;

	private static double _iconGridWidth;

	private static double _iconGridHeight;

	private static double _maximizedPrimaryScreenWidth;

	private static double _maximizedPrimaryScreenHeight;

	private static double _maximumWindowTrackWidth;

	private static double _maximumWindowTrackHeight;

	private static double _menuCheckmarkWidth;

	private static double _menuCheckmarkHeight;

	private static double _menuButtonWidth;

	private static double _menuButtonHeight;

	private static double _minimumWindowWidth;

	private static double _minimumWindowHeight;

	private static double _minimizedWindowWidth;

	private static double _minimizedWindowHeight;

	private static double _minimizedGridWidth;

	private static double _minimizedGridHeight;

	private static double _minimumWindowTrackWidth;

	private static double _minimumWindowTrackHeight;

	private static double _primaryScreenWidth;

	private static double _primaryScreenHeight;

	private static double _windowCaptionButtonWidth;

	private static double _windowCaptionButtonHeight;

	private static double _resizeFrameHorizontalBorderHeight;

	private static double _resizeFrameVerticalBorderWidth;

	private static double _smallIconWidth;

	private static double _smallIconHeight;

	private static double _smallWindowCaptionButtonWidth;

	private static double _smallWindowCaptionButtonHeight;

	private static double _virtualScreenWidth;

	private static double _virtualScreenHeight;

	private static double _verticalScrollBarWidth;

	private static double _verticalScrollBarButtonHeight;

	private static double _windowCaptionHeight;

	private static double _kanjiWindowHeight;

	private static double _menuBarHeight;

	private static double _verticalScrollBarThumbHeight;

	private static bool _isImmEnabled;

	private static bool _isMediaCenter;

	private static bool _isMenuDropRightAligned;

	private static bool _isMiddleEastEnabled;

	private static bool _isMousePresent;

	private static bool _isMouseWheelPresent;

	private static bool _isPenWindows;

	private static bool _isRemotelyControlled;

	private static bool _isRemoteSession;

	private static bool _showSounds;

	private static bool _isSlowMachine;

	private static bool _swapButtons;

	private static bool _isTabletPC;

	private static double _virtualScreenLeft;

	private static double _virtualScreenTop;

	private static PowerLineStatus _powerLineStatus;

	private static SystemResourceKey _cacheThinHorizontalBorderHeight;

	private static SystemResourceKey _cacheThinVerticalBorderWidth;

	private static SystemResourceKey _cacheCursorWidth;

	private static SystemResourceKey _cacheCursorHeight;

	private static SystemResourceKey _cacheThickHorizontalBorderHeight;

	private static SystemResourceKey _cacheThickVerticalBorderWidth;

	private static SystemResourceKey _cacheFixedFrameHorizontalBorderHeight;

	private static SystemResourceKey _cacheFixedFrameVerticalBorderWidth;

	private static SystemResourceKey _cacheFocusHorizontalBorderHeight;

	private static SystemResourceKey _cacheFocusVerticalBorderWidth;

	private static SystemResourceKey _cacheFullPrimaryScreenWidth;

	private static SystemResourceKey _cacheFullPrimaryScreenHeight;

	private static SystemResourceKey _cacheHorizontalScrollBarButtonWidth;

	private static SystemResourceKey _cacheHorizontalScrollBarHeight;

	private static SystemResourceKey _cacheHorizontalScrollBarThumbWidth;

	private static SystemResourceKey _cacheIconWidth;

	private static SystemResourceKey _cacheIconHeight;

	private static SystemResourceKey _cacheIconGridWidth;

	private static SystemResourceKey _cacheIconGridHeight;

	private static SystemResourceKey _cacheMaximizedPrimaryScreenWidth;

	private static SystemResourceKey _cacheMaximizedPrimaryScreenHeight;

	private static SystemResourceKey _cacheMaximumWindowTrackWidth;

	private static SystemResourceKey _cacheMaximumWindowTrackHeight;

	private static SystemResourceKey _cacheMenuCheckmarkWidth;

	private static SystemResourceKey _cacheMenuCheckmarkHeight;

	private static SystemResourceKey _cacheMenuButtonWidth;

	private static SystemResourceKey _cacheMenuButtonHeight;

	private static SystemResourceKey _cacheMinimumWindowWidth;

	private static SystemResourceKey _cacheMinimumWindowHeight;

	private static SystemResourceKey _cacheMinimizedWindowWidth;

	private static SystemResourceKey _cacheMinimizedWindowHeight;

	private static SystemResourceKey _cacheMinimizedGridWidth;

	private static SystemResourceKey _cacheMinimizedGridHeight;

	private static SystemResourceKey _cacheMinimumWindowTrackWidth;

	private static SystemResourceKey _cacheMinimumWindowTrackHeight;

	private static SystemResourceKey _cachePrimaryScreenWidth;

	private static SystemResourceKey _cachePrimaryScreenHeight;

	private static SystemResourceKey _cacheWindowCaptionButtonWidth;

	private static SystemResourceKey _cacheWindowCaptionButtonHeight;

	private static SystemResourceKey _cacheResizeFrameHorizontalBorderHeight;

	private static SystemResourceKey _cacheResizeFrameVerticalBorderWidth;

	private static SystemResourceKey _cacheSmallIconWidth;

	private static SystemResourceKey _cacheSmallIconHeight;

	private static SystemResourceKey _cacheSmallWindowCaptionButtonWidth;

	private static SystemResourceKey _cacheSmallWindowCaptionButtonHeight;

	private static SystemResourceKey _cacheVirtualScreenWidth;

	private static SystemResourceKey _cacheVirtualScreenHeight;

	private static SystemResourceKey _cacheVerticalScrollBarWidth;

	private static SystemResourceKey _cacheVerticalScrollBarButtonHeight;

	private static SystemResourceKey _cacheWindowCaptionHeight;

	private static SystemResourceKey _cacheKanjiWindowHeight;

	private static SystemResourceKey _cacheMenuBarHeight;

	private static SystemResourceKey _cacheSmallCaptionHeight;

	private static SystemResourceKey _cacheVerticalScrollBarThumbHeight;

	private static SystemResourceKey _cacheIsImmEnabled;

	private static SystemResourceKey _cacheIsMediaCenter;

	private static SystemResourceKey _cacheIsMenuDropRightAligned;

	private static SystemResourceKey _cacheIsMiddleEastEnabled;

	private static SystemResourceKey _cacheIsMousePresent;

	private static SystemResourceKey _cacheIsMouseWheelPresent;

	private static SystemResourceKey _cacheIsPenWindows;

	private static SystemResourceKey _cacheIsRemotelyControlled;

	private static SystemResourceKey _cacheIsRemoteSession;

	private static SystemResourceKey _cacheShowSounds;

	private static SystemResourceKey _cacheIsSlowMachine;

	private static SystemResourceKey _cacheSwapButtons;

	private static SystemResourceKey _cacheIsTabletPC;

	private static SystemResourceKey _cacheVirtualScreenLeft;

	private static SystemResourceKey _cacheVirtualScreenTop;

	private static SystemResourceKey _cacheFocusBorderWidth;

	private static SystemResourceKey _cacheFocusBorderHeight;

	private static SystemResourceKey _cacheHighContrast;

	private static SystemResourceKey _cacheDropShadow;

	private static SystemResourceKey _cacheFlatMenu;

	private static SystemResourceKey _cacheWorkArea;

	private static SystemResourceKey _cacheIconHorizontalSpacing;

	private static SystemResourceKey _cacheIconVerticalSpacing;

	private static SystemResourceKey _cacheIconTitleWrap;

	private static SystemResourceKey _cacheKeyboardCues;

	private static SystemResourceKey _cacheKeyboardDelay;

	private static SystemResourceKey _cacheKeyboardPreference;

	private static SystemResourceKey _cacheKeyboardSpeed;

	private static SystemResourceKey _cacheSnapToDefaultButton;

	private static SystemResourceKey _cacheWheelScrollLines;

	private static SystemResourceKey _cacheMouseHoverTime;

	private static SystemResourceKey _cacheMouseHoverHeight;

	private static SystemResourceKey _cacheMouseHoverWidth;

	private static SystemResourceKey _cacheMenuDropAlignment;

	private static SystemResourceKey _cacheMenuFade;

	private static SystemResourceKey _cacheMenuShowDelay;

	private static SystemResourceKey _cacheComboBoxAnimation;

	private static SystemResourceKey _cacheClientAreaAnimation;

	private static SystemResourceKey _cacheCursorShadow;

	private static SystemResourceKey _cacheGradientCaptions;

	private static SystemResourceKey _cacheHotTracking;

	private static SystemResourceKey _cacheListBoxSmoothScrolling;

	private static SystemResourceKey _cacheMenuAnimation;

	private static SystemResourceKey _cacheSelectionFade;

	private static SystemResourceKey _cacheStylusHotTracking;

	private static SystemResourceKey _cacheToolTipAnimation;

	private static SystemResourceKey _cacheToolTipFade;

	private static SystemResourceKey _cacheUIEffects;

	private static SystemResourceKey _cacheMinimizeAnimation;

	private static SystemResourceKey _cacheBorder;

	private static SystemResourceKey _cacheCaretWidth;

	private static SystemResourceKey _cacheForegroundFlashCount;

	private static SystemResourceKey _cacheDragFullWindows;

	private static SystemResourceKey _cacheBorderWidth;

	private static SystemResourceKey _cacheScrollWidth;

	private static SystemResourceKey _cacheScrollHeight;

	private static SystemResourceKey _cacheCaptionWidth;

	private static SystemResourceKey _cacheCaptionHeight;

	private static SystemResourceKey _cacheSmallCaptionWidth;

	private static SystemResourceKey _cacheMenuWidth;

	private static SystemResourceKey _cacheMenuHeight;

	private static SystemResourceKey _cacheComboBoxPopupAnimation;

	private static SystemResourceKey _cacheMenuPopupAnimation;

	private static SystemResourceKey _cacheToolTipPopupAnimation;

	private static SystemResourceKey _cachePowerLineStatus;

	private static SystemThemeKey _cacheFocusVisualStyle;

	private static SystemThemeKey _cacheNavigationChromeStyle;

	private static SystemThemeKey _cacheNavigationChromeDownLevelStyle;

	/// <summary>Gets the width, in pixels, of the left and right edges of the focus rectangle.  </summary>
	/// <returns>The edge width.</returns>
	public static double FocusBorderWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[1])
				{
					_cacheValid[1] = true;
					int value = 0;
					if (MS.Win32.UnsafeNativeMethods.SystemParametersInfo(8206, 0, ref value, 0))
					{
						_focusBorderWidth = ConvertPixel(value);
						continue;
					}
					_cacheValid[1] = false;
					throw new Win32Exception();
				}
			}
			return _focusBorderWidth;
		}
	}

	/// <summary>Gets the height, in pixels, of the upper and lower edges of the focus rectangle.  </summary>
	/// <returns>The edge height.</returns>
	public static double FocusBorderHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[2])
				{
					_cacheValid[2] = true;
					int value = 0;
					if (MS.Win32.UnsafeNativeMethods.SystemParametersInfo(8208, 0, ref value, 0))
					{
						_focusBorderHeight = ConvertPixel(value);
						continue;
					}
					_cacheValid[2] = false;
					throw new Win32Exception();
				}
			}
			return _focusBorderHeight;
		}
	}

	/// <summary>Gets information about the High Contrast accessibility feature. </summary>
	/// <returns>true if the HIGHCONTRASTON option is selected; otherwise, false.</returns>
	public static bool HighContrast
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[3])
				{
					_cacheValid[3] = true;
					MS.Win32.NativeMethods.HIGHCONTRAST_I rc = default(MS.Win32.NativeMethods.HIGHCONTRAST_I);
					rc.cbSize = Marshal.SizeOf(typeof(MS.Win32.NativeMethods.HIGHCONTRAST_I));
					if (MS.Win32.UnsafeNativeMethods.SystemParametersInfo(66, rc.cbSize, ref rc, 0))
					{
						_highContrast = (rc.dwFlags & 1) == 1;
						continue;
					}
					_cacheValid[3] = false;
					throw new Win32Exception();
				}
			}
			return _highContrast;
		}
	}

	internal static bool MouseVanish
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[4])
				{
					_cacheValid[4] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(4128, 0, ref _mouseVanish, 0))
					{
						_cacheValid[4] = false;
						throw new Win32Exception();
					}
				}
			}
			return _mouseVanish;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.FocusBorderWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey FocusBorderWidthKey
	{
		get
		{
			if (_cacheFocusBorderWidth == null)
			{
				_cacheFocusBorderWidth = CreateInstance(SystemResourceKeyID.FocusBorderWidth);
			}
			return _cacheFocusBorderWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.FocusBorderHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey FocusBorderHeightKey
	{
		get
		{
			if (_cacheFocusBorderHeight == null)
			{
				_cacheFocusBorderHeight = CreateInstance(SystemResourceKeyID.FocusBorderHeight);
			}
			return _cacheFocusBorderHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.HighContrast" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey HighContrastKey
	{
		get
		{
			if (_cacheHighContrast == null)
			{
				_cacheHighContrast = CreateInstance(SystemResourceKeyID.HighContrast);
			}
			return _cacheHighContrast;
		}
	}

	/// <summary>Gets a value indicating whether the drop shadow effect is enabled. </summary>
	/// <returns>true if the drop shadow effect is enabled; otherwise, false.</returns>
	public static bool DropShadow
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[5])
				{
					_cacheValid[5] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(4132, 0, ref _dropShadow, 0))
					{
						_cacheValid[5] = false;
						throw new Win32Exception();
					}
				}
			}
			return _dropShadow;
		}
	}

	/// <summary>Gets a value indicating whether native menus appear as a flat menu.  </summary>
	/// <returns>true if the flat menu appearance is set; otherwise, false.</returns>
	public static bool FlatMenu
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[6])
				{
					_cacheValid[6] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(4130, 0, ref _flatMenu, 0))
					{
						_cacheValid[6] = false;
						throw new Win32Exception();
					}
				}
			}
			return _flatMenu;
		}
	}

	internal static MS.Win32.NativeMethods.RECT WorkAreaInternal
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[7])
				{
					_cacheValid[7] = true;
					_workAreaInternal = default(MS.Win32.NativeMethods.RECT);
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(48, 0, ref _workAreaInternal, 0))
					{
						_cacheValid[7] = false;
						throw new Win32Exception();
					}
				}
			}
			return _workAreaInternal;
		}
	}

	/// <summary>Gets the size of the work area on the primary display monitor. </summary>
	/// <returns>A RECT structure that receives the work area coordinates, expressed as virtual screen coordinates.</returns>
	public static Rect WorkArea
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[8])
				{
					_cacheValid[8] = true;
					MS.Win32.NativeMethods.RECT workAreaInternal = WorkAreaInternal;
					_workArea = new Rect(ConvertPixel(workAreaInternal.left), ConvertPixel(workAreaInternal.top), ConvertPixel(workAreaInternal.Width), ConvertPixel(workAreaInternal.Height));
				}
			}
			return _workArea;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.DropShadow" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey DropShadowKey
	{
		get
		{
			if (_cacheDropShadow == null)
			{
				_cacheDropShadow = CreateInstance(SystemResourceKeyID.DropShadow);
			}
			return _cacheDropShadow;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.FlatMenu" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey FlatMenuKey
	{
		get
		{
			if (_cacheFlatMenu == null)
			{
				_cacheFlatMenu = CreateInstance(SystemResourceKeyID.FlatMenu);
			}
			return _cacheFlatMenu;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.WorkArea" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey WorkAreaKey
	{
		get
		{
			if (_cacheWorkArea == null)
			{
				_cacheWorkArea = CreateInstance(SystemResourceKeyID.WorkArea);
			}
			return _cacheWorkArea;
		}
	}

	internal static MS.Win32.NativeMethods.ICONMETRICS IconMetrics
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[9])
				{
					_cacheValid[9] = true;
					_iconMetrics = new MS.Win32.NativeMethods.ICONMETRICS();
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(45, _iconMetrics.cbSize, _iconMetrics, 0))
					{
						_cacheValid[9] = false;
						throw new Win32Exception();
					}
				}
			}
			return _iconMetrics;
		}
	}

	/// <summary>Gets the width, in pixels, of an icon cell. The system uses this rectangle to arrange icons in large icon view. </summary>
	/// <returns>The width of an icon cell.</returns>
	public static double IconHorizontalSpacing => ConvertPixel(IconMetrics.iHorzSpacing);

	/// <summary>Gets the height, in pixels, of an icon cell. The system uses this rectangle to arrange icons in large icon view. </summary>
	/// <returns>The height of an icon cell.</returns>
	public static double IconVerticalSpacing => ConvertPixel(IconMetrics.iVertSpacing);

	/// <summary>Gets a value indicating whether icon-title wrapping is enabled. </summary>
	/// <returns>true if icon-title wrapping is enabled; otherwise false.</returns>
	public static bool IconTitleWrap => IconMetrics.iTitleWrap != 0;

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.IconHorizontalSpacing" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IconHorizontalSpacingKey
	{
		get
		{
			if (_cacheIconHorizontalSpacing == null)
			{
				_cacheIconHorizontalSpacing = CreateInstance(SystemResourceKeyID.IconHorizontalSpacing);
			}
			return _cacheIconHorizontalSpacing;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.IconVerticalSpacing" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IconVerticalSpacingKey
	{
		get
		{
			if (_cacheIconVerticalSpacing == null)
			{
				_cacheIconVerticalSpacing = CreateInstance(SystemResourceKeyID.IconVerticalSpacing);
			}
			return _cacheIconVerticalSpacing;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.IconTitleWrap" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IconTitleWrapKey
	{
		get
		{
			if (_cacheIconTitleWrap == null)
			{
				_cacheIconTitleWrap = CreateInstance(SystemResourceKeyID.IconTitleWrap);
			}
			return _cacheIconTitleWrap;
		}
	}

	/// <summary>Gets a value indicating whether menu access keys are always underlined. </summary>
	/// <returns>true if menu access keys are always underlined; false if they are underlined only when the menu is activated by the keyboard.</returns>
	public static bool KeyboardCues
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[10])
				{
					_cacheValid[10] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(4106, 0, ref _keyboardCues, 0))
					{
						_cacheValid[10] = false;
						throw new Win32Exception();
					}
				}
			}
			return _keyboardCues;
		}
	}

	/// <summary>Gets the keyboard repeat-delay setting, which is a value in the range from 0 (approximately 250 milliseconds delay) through 3 (approximately 1 second delay). </summary>
	/// <returns>The keyboard repeat-delay setting.</returns>
	public static int KeyboardDelay
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[11])
				{
					_cacheValid[11] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(22, 0, ref _keyboardDelay, 0))
					{
						_cacheValid[11] = false;
						throw new Win32Exception();
					}
				}
			}
			return _keyboardDelay;
		}
	}

	/// <summary>Gets a value indicating whether the user relies on the keyboard instead of the mouse, and whether the user wants applications to display keyboard interfaces that are typically hidden. </summary>
	/// <returns>true if the user relies on the keyboard; otherwise, false.</returns>
	public static bool KeyboardPreference
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[12])
				{
					_cacheValid[12] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(68, 0, ref _keyboardPref, 0))
					{
						_cacheValid[12] = false;
						throw new Win32Exception();
					}
				}
			}
			return _keyboardPref;
		}
	}

	/// <summary>Gets the keyboard repeat-speed setting, which is a value in the range from 0 (approximately 2.5 repetitions per second) through 31 (approximately 30 repetitions per second). </summary>
	/// <returns>The keyboard repeat-speed setting.</returns>
	public static int KeyboardSpeed
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[13])
				{
					_cacheValid[13] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(10, 0, ref _keyboardSpeed, 0))
					{
						_cacheValid[13] = false;
						throw new Win32Exception();
					}
				}
			}
			return _keyboardSpeed;
		}
	}

	/// <summary>Gets a value indicating whether the snap-to-default button is enabled. If enabled, the mouse cursor automatically moves to the default button of a dialog box, such as OK or Apply.  </summary>
	/// <returns>true when the feature is enabled; otherwise, false.</returns>
	public static bool SnapToDefaultButton
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[14])
				{
					_cacheValid[14] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(95, 0, ref _snapToDefButton, 0))
					{
						_cacheValid[14] = false;
						throw new Win32Exception();
					}
				}
			}
			return _snapToDefButton;
		}
	}

	/// <summary>Gets a value that indicates the number of lines to scroll when the mouse wheel is rotated. </summary>
	/// <returns>The number of lines.</returns>
	public static int WheelScrollLines
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[15])
				{
					_cacheValid[15] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(104, 0, ref _wheelScrollLines, 0))
					{
						_cacheValid[15] = false;
						throw new Win32Exception();
					}
				}
			}
			return _wheelScrollLines;
		}
	}

	/// <summary>Gets the time, in milliseconds, that the mouse pointer must remain in the hover rectangle to generate a mouse-hover event.  </summary>
	/// <returns>The time, in milliseconds, that the mouse must be in the hover rectangle to generate a mouse-hover event.</returns>
	public static TimeSpan MouseHoverTime => TimeSpan.FromMilliseconds(MouseHoverTimeMilliseconds);

	internal static int MouseHoverTimeMilliseconds
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[16])
				{
					_cacheValid[16] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(102, 0, ref _mouseHoverTime, 0))
					{
						_cacheValid[16] = false;
						throw new Win32Exception();
					}
				}
			}
			return _mouseHoverTime;
		}
	}

	/// <summary>Gets the height, in pixels, of the rectangle within which the mouse pointer has to stay to generate a mouse-hover event. </summary>
	/// <returns>The height of a rectangle used for a mouse-hover event.</returns>
	public static double MouseHoverHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[17])
				{
					_cacheValid[17] = true;
					int value = 0;
					if (MS.Win32.UnsafeNativeMethods.SystemParametersInfo(100, 0, ref value, 0))
					{
						_mouseHoverHeight = ConvertPixel(value);
						continue;
					}
					_cacheValid[17] = false;
					throw new Win32Exception();
				}
			}
			return _mouseHoverHeight;
		}
	}

	/// <summary>Gets the width, in pixels, of the rectangle within which the mouse pointer has to stay to generate a mouse-hover event.  </summary>
	/// <returns>The width of a rectangle used for a mouse-hover event.</returns>
	public static double MouseHoverWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[18])
				{
					_cacheValid[18] = true;
					int value = 0;
					if (MS.Win32.UnsafeNativeMethods.SystemParametersInfo(98, 0, ref value, 0))
					{
						_mouseHoverWidth = ConvertPixel(value);
						continue;
					}
					_cacheValid[18] = false;
					throw new Win32Exception();
				}
			}
			return _mouseHoverWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.KeyboardCues" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey KeyboardCuesKey
	{
		get
		{
			if (_cacheKeyboardCues == null)
			{
				_cacheKeyboardCues = CreateInstance(SystemResourceKeyID.KeyboardCues);
			}
			return _cacheKeyboardCues;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.KeyboardDelay" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey KeyboardDelayKey
	{
		get
		{
			if (_cacheKeyboardDelay == null)
			{
				_cacheKeyboardDelay = CreateInstance(SystemResourceKeyID.KeyboardDelay);
			}
			return _cacheKeyboardDelay;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.KeyboardPreference" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey KeyboardPreferenceKey
	{
		get
		{
			if (_cacheKeyboardPreference == null)
			{
				_cacheKeyboardPreference = CreateInstance(SystemResourceKeyID.KeyboardPreference);
			}
			return _cacheKeyboardPreference;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.KeyboardSpeed" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey KeyboardSpeedKey
	{
		get
		{
			if (_cacheKeyboardSpeed == null)
			{
				_cacheKeyboardSpeed = CreateInstance(SystemResourceKeyID.KeyboardSpeed);
			}
			return _cacheKeyboardSpeed;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.SnapToDefaultButton" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey SnapToDefaultButtonKey
	{
		get
		{
			if (_cacheSnapToDefaultButton == null)
			{
				_cacheSnapToDefaultButton = CreateInstance(SystemResourceKeyID.SnapToDefaultButton);
			}
			return _cacheSnapToDefaultButton;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.WheelScrollLines" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey WheelScrollLinesKey
	{
		get
		{
			if (_cacheWheelScrollLines == null)
			{
				_cacheWheelScrollLines = CreateInstance(SystemResourceKeyID.WheelScrollLines);
			}
			return _cacheWheelScrollLines;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MouseHoverTime" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MouseHoverTimeKey
	{
		get
		{
			if (_cacheMouseHoverTime == null)
			{
				_cacheMouseHoverTime = CreateInstance(SystemResourceKeyID.MouseHoverTime);
			}
			return _cacheMouseHoverTime;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MouseHoverHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MouseHoverHeightKey
	{
		get
		{
			if (_cacheMouseHoverHeight == null)
			{
				_cacheMouseHoverHeight = CreateInstance(SystemResourceKeyID.MouseHoverHeight);
			}
			return _cacheMouseHoverHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MouseHoverWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MouseHoverWidthKey
	{
		get
		{
			if (_cacheMouseHoverWidth == null)
			{
				_cacheMouseHoverWidth = CreateInstance(SystemResourceKeyID.MouseHoverWidth);
			}
			return _cacheMouseHoverWidth;
		}
	}

	/// <summary>Gets a value indicating whether pop-up menus are left-aligned or right-aligned, relative to the corresponding menu item. </summary>
	/// <returns>true if left-aligned; otherwise, false.</returns>
	public static bool MenuDropAlignment
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[19])
				{
					_cacheValid[19] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(27, 0, ref _menuDropAlignment, 0))
					{
						_cacheValid[19] = false;
						throw new Win32Exception();
					}
				}
			}
			return _menuDropAlignment;
		}
	}

	/// <summary>Gets a value indicating whether menu fade animation is enabled. </summary>
	/// <returns>true when fade animation is enabled; otherwise, false.</returns>
	public static bool MenuFade
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[20])
				{
					_cacheValid[20] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(4114, 0, ref _menuFade, 0))
					{
						_cacheValid[20] = false;
						throw new Win32Exception();
					}
				}
			}
			return _menuFade;
		}
	}

	/// <summary>Gets the time, in milliseconds, that the system waits before displaying a shortcut menu when the mouse cursor is over a submenu item.  </summary>
	/// <returns>The delay time.</returns>
	public static int MenuShowDelay
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[21])
				{
					_cacheValid[21] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(106, 0, ref _menuShowDelay, 0))
					{
						_cacheValid[21] = false;
						throw new Win32Exception();
					}
				}
			}
			return _menuShowDelay;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MenuDropAlignment" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MenuDropAlignmentKey
	{
		get
		{
			if (_cacheMenuDropAlignment == null)
			{
				_cacheMenuDropAlignment = CreateInstance(SystemResourceKeyID.MenuDropAlignment);
			}
			return _cacheMenuDropAlignment;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MenuFade" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MenuFadeKey
	{
		get
		{
			if (_cacheMenuFade == null)
			{
				_cacheMenuFade = CreateInstance(SystemResourceKeyID.MenuFade);
			}
			return _cacheMenuFade;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MenuShowDelay" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MenuShowDelayKey
	{
		get
		{
			if (_cacheMenuShowDelay == null)
			{
				_cacheMenuShowDelay = CreateInstance(SystemResourceKeyID.MenuShowDelay);
			}
			return _cacheMenuShowDelay;
		}
	}

	/// <summary>Gets the system value of the <see cref="P:System.Windows.Controls.Primitives.Popup.PopupAnimation" /> property for combo boxes. </summary>
	/// <returns>A pop-up animation value.</returns>
	public static PopupAnimation ComboBoxPopupAnimation
	{
		get
		{
			if (ComboBoxAnimation)
			{
				return PopupAnimation.Slide;
			}
			return PopupAnimation.None;
		}
	}

	/// <summary>Gets a value indicating whether the slide-open effect for combo boxes is enabled. </summary>
	/// <returns>true for enabled; otherwise, false.</returns>
	public static bool ComboBoxAnimation
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[22])
				{
					_cacheValid[22] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(4100, 0, ref _comboBoxAnimation, 0))
					{
						_cacheValid[22] = false;
						throw new Win32Exception();
					}
				}
			}
			return _comboBoxAnimation;
		}
	}

	/// <summary>Gets a value indicating whether the client area animation feature is enabled.</summary>
	/// <returns>A Boolean value; true if client area animation is enabled, false otherwise.</returns>
	public static bool ClientAreaAnimation
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[23])
				{
					_cacheValid[23] = true;
					if (Environment.OSVersion.Version.Major >= 6)
					{
						if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(4162, 0, ref _clientAreaAnimation, 0))
						{
							_cacheValid[23] = false;
							throw new Win32Exception();
						}
					}
					else
					{
						_clientAreaAnimation = true;
					}
				}
			}
			return _clientAreaAnimation;
		}
	}

	/// <summary>Gets a value indicating whether the cursor has a shadow around it. </summary>
	/// <returns>true if the shadow is enabled; otherwise, false.</returns>
	public static bool CursorShadow
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[24])
				{
					_cacheValid[24] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(4122, 0, ref _cursorShadow, 0))
					{
						_cacheValid[24] = false;
						throw new Win32Exception();
					}
				}
			}
			return _cursorShadow;
		}
	}

	/// <summary>Gets a value indicating whether the gradient effect for window title bars is enabled. </summary>
	/// <returns>true if the gradient effect is enabled; otherwise, false.</returns>
	public static bool GradientCaptions
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[25])
				{
					_cacheValid[25] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(4104, 0, ref _gradientCaptions, 0))
					{
						_cacheValid[25] = false;
						throw new Win32Exception();
					}
				}
			}
			return _gradientCaptions;
		}
	}

	/// <summary>Gets a value indicating whether hot tracking of user-interface elements, such as menu names on menu bars, is enabled. </summary>
	/// <returns>true if hot tracking is enabled; otherwise, false.</returns>
	public static bool HotTracking
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[26])
				{
					_cacheValid[26] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(4110, 0, ref _hotTracking, 0))
					{
						_cacheValid[26] = false;
						throw new Win32Exception();
					}
				}
			}
			return _hotTracking;
		}
	}

	/// <summary>Gets a value indicating whether the smooth-scrolling effect for list boxes is enabled. </summary>
	/// <returns>true if the smooth-scrolling effect is enabled; otherwise, false.</returns>
	public static bool ListBoxSmoothScrolling
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[27])
				{
					_cacheValid[27] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(4102, 0, ref _listBoxSmoothScrolling, 0))
					{
						_cacheValid[27] = false;
						throw new Win32Exception();
					}
				}
			}
			return _listBoxSmoothScrolling;
		}
	}

	/// <summary>Gets the system value of the <see cref="P:System.Windows.Controls.Primitives.Popup.PopupAnimation" /> property for menus. </summary>
	/// <returns>The pop-up animation property.</returns>
	public static PopupAnimation MenuPopupAnimation
	{
		get
		{
			if (MenuAnimation)
			{
				if (MenuFade)
				{
					return PopupAnimation.Fade;
				}
				return PopupAnimation.Scroll;
			}
			return PopupAnimation.None;
		}
	}

	/// <summary>Gets a value indicating whether the menu animation feature is enabled. </summary>
	/// <returns>true if menu animation is enabled; otherwise, false.</returns>
	public static bool MenuAnimation
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[28])
				{
					_cacheValid[28] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(4098, 0, ref _menuAnimation, 0))
					{
						_cacheValid[28] = false;
						throw new Win32Exception();
					}
				}
			}
			return _menuAnimation;
		}
	}

	/// <summary>Gets a value indicating whether the selection fade effect is enabled. </summary>
	/// <returns>true if the fade effect is enabled; otherwise, false.</returns>
	public static bool SelectionFade
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[29])
				{
					_cacheValid[29] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(4116, 0, ref _selectionFade, 0))
					{
						_cacheValid[29] = false;
						throw new Win32Exception();
					}
				}
			}
			return _selectionFade;
		}
	}

	/// <summary>Gets a value indicating whether hot tracking of a stylus is enabled.  </summary>
	/// <returns>true if hot tracking of a stylus is enabled; otherwise false.</returns>
	public static bool StylusHotTracking
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[30])
				{
					_cacheValid[30] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(4112, 0, ref _stylusHotTracking, 0))
					{
						_cacheValid[30] = false;
						throw new Win32Exception();
					}
				}
			}
			return _stylusHotTracking;
		}
	}

	/// <summary>Gets the system value of the <see cref="P:System.Windows.Controls.Primitives.Popup.PopupAnimation" /> property for ToolTips. </summary>
	/// <returns>A system value for the pop-up animation property.</returns>
	public static PopupAnimation ToolTipPopupAnimation
	{
		get
		{
			if (ToolTipAnimation && ToolTipFade)
			{
				return PopupAnimation.Fade;
			}
			return PopupAnimation.None;
		}
	}

	/// <summary>Gets a value indicating whether <see cref="T:System.Windows.Controls.ToolTip" /> animation is enabled.  </summary>
	/// <returns>true if ToolTip animation is enabled; otherwise, false.</returns>
	public static bool ToolTipAnimation
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[31])
				{
					_cacheValid[31] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(4118, 0, ref _toolTipAnimation, 0))
					{
						_cacheValid[31] = false;
						throw new Win32Exception();
					}
				}
			}
			return _toolTipAnimation;
		}
	}

	/// <summary>Gets a value indicating whether ToolTip animation uses a fade effect or a slide effect.  </summary>
	/// <returns>true if a fade effect is used; false if a slide effect is used.</returns>
	public static bool ToolTipFade
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[32])
				{
					_cacheValid[32] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(4120, 0, ref _tooltipFade, 0))
					{
						_cacheValid[32] = false;
						throw new Win32Exception();
					}
				}
			}
			return _tooltipFade;
		}
	}

	/// <summary>Gets a value that indicates whether all user interface (UI) effects are enabled.   </summary>
	/// <returns>true if all UI effects are enabled; false if they are disabled.</returns>
	public static bool UIEffects
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[33])
				{
					_cacheValid[33] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(4158, 0, ref _uiEffects, 0))
					{
						_cacheValid[33] = false;
						throw new Win32Exception();
					}
				}
			}
			return _uiEffects;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.ComboBoxAnimation" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey ComboBoxAnimationKey
	{
		get
		{
			if (_cacheComboBoxAnimation == null)
			{
				_cacheComboBoxAnimation = CreateInstance(SystemResourceKeyID.ComboBoxAnimation);
			}
			return _cacheComboBoxAnimation;
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.ClientAreaAnimation" /> property.</summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey ClientAreaAnimationKey
	{
		get
		{
			if (_cacheClientAreaAnimation == null)
			{
				_cacheClientAreaAnimation = CreateInstance(SystemResourceKeyID.ClientAreaAnimation);
			}
			return _cacheClientAreaAnimation;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.CursorShadow" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey CursorShadowKey
	{
		get
		{
			if (_cacheCursorShadow == null)
			{
				_cacheCursorShadow = CreateInstance(SystemResourceKeyID.CursorShadow);
			}
			return _cacheCursorShadow;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.GradientCaptions" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey GradientCaptionsKey
	{
		get
		{
			if (_cacheGradientCaptions == null)
			{
				_cacheGradientCaptions = CreateInstance(SystemResourceKeyID.GradientCaptions);
			}
			return _cacheGradientCaptions;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.HotTracking" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey HotTrackingKey
	{
		get
		{
			if (_cacheHotTracking == null)
			{
				_cacheHotTracking = CreateInstance(SystemResourceKeyID.HotTracking);
			}
			return _cacheHotTracking;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.ListBoxSmoothScrolling" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey ListBoxSmoothScrollingKey
	{
		get
		{
			if (_cacheListBoxSmoothScrolling == null)
			{
				_cacheListBoxSmoothScrolling = CreateInstance(SystemResourceKeyID.ListBoxSmoothScrolling);
			}
			return _cacheListBoxSmoothScrolling;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MenuAnimation" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MenuAnimationKey
	{
		get
		{
			if (_cacheMenuAnimation == null)
			{
				_cacheMenuAnimation = CreateInstance(SystemResourceKeyID.MenuAnimation);
			}
			return _cacheMenuAnimation;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.SelectionFade" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey SelectionFadeKey
	{
		get
		{
			if (_cacheSelectionFade == null)
			{
				_cacheSelectionFade = CreateInstance(SystemResourceKeyID.SelectionFade);
			}
			return _cacheSelectionFade;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.StylusHotTracking" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey StylusHotTrackingKey
	{
		get
		{
			if (_cacheStylusHotTracking == null)
			{
				_cacheStylusHotTracking = CreateInstance(SystemResourceKeyID.StylusHotTracking);
			}
			return _cacheStylusHotTracking;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.ToolTipAnimation" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey ToolTipAnimationKey
	{
		get
		{
			if (_cacheToolTipAnimation == null)
			{
				_cacheToolTipAnimation = CreateInstance(SystemResourceKeyID.ToolTipAnimation);
			}
			return _cacheToolTipAnimation;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.ToolTipFade" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey ToolTipFadeKey
	{
		get
		{
			if (_cacheToolTipFade == null)
			{
				_cacheToolTipFade = CreateInstance(SystemResourceKeyID.ToolTipFade);
			}
			return _cacheToolTipFade;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.UIEffects" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey UIEffectsKey
	{
		get
		{
			if (_cacheUIEffects == null)
			{
				_cacheUIEffects = CreateInstance(SystemResourceKeyID.UIEffects);
			}
			return _cacheUIEffects;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.ComboBoxPopupAnimation" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey ComboBoxPopupAnimationKey
	{
		get
		{
			if (_cacheComboBoxPopupAnimation == null)
			{
				_cacheComboBoxPopupAnimation = CreateInstance(SystemResourceKeyID.ComboBoxPopupAnimation);
			}
			return _cacheComboBoxPopupAnimation;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MenuPopupAnimation" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MenuPopupAnimationKey
	{
		get
		{
			if (_cacheMenuPopupAnimation == null)
			{
				_cacheMenuPopupAnimation = CreateInstance(SystemResourceKeyID.MenuPopupAnimation);
			}
			return _cacheMenuPopupAnimation;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.ToolTipPopupAnimation" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey ToolTipPopupAnimationKey
	{
		get
		{
			if (_cacheToolTipPopupAnimation == null)
			{
				_cacheToolTipPopupAnimation = CreateInstance(SystemResourceKeyID.ToolTipPopupAnimation);
			}
			return _cacheToolTipPopupAnimation;
		}
	}

	/// <summary>Gets the animation effects associated with user actions. </summary>
	/// <returns>true if the minimize window animations feature is enabled; otherwise, false.</returns>
	public static bool MinimizeAnimation
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[34])
				{
					_cacheValid[34] = true;
					MS.Win32.NativeMethods.ANIMATIONINFO aNIMATIONINFO = new MS.Win32.NativeMethods.ANIMATIONINFO();
					if (MS.Win32.UnsafeNativeMethods.SystemParametersInfo(72, aNIMATIONINFO.cbSize, aNIMATIONINFO, 0))
					{
						_minAnimation = aNIMATIONINFO.iMinAnimate != 0;
						continue;
					}
					_cacheValid[34] = false;
					throw new Win32Exception();
				}
			}
			return _minAnimation;
		}
	}

	/// <summary>Gets the border multiplier factor that determines the width of a window's sizing border. </summary>
	/// <returns>A multiplier.</returns>
	public static int Border
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[35])
				{
					_cacheValid[35] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(5, 0, ref _border, 0))
					{
						_cacheValid[35] = false;
						throw new Win32Exception();
					}
				}
			}
			return _border;
		}
	}

	/// <summary>Gets the caret width, in pixels, for edit controls. </summary>
	/// <returns>The caret width.</returns>
	public static double CaretWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[36])
				{
					_cacheValid[36] = true;
					int value = 0;
					using (DpiUtil.WithDpiAwarenessContext(DpiAwarenessContextValue.Unaware))
					{
						if (MS.Win32.UnsafeNativeMethods.SystemParametersInfo(8198, 0, ref value, 0))
						{
							_caretWidth = value;
							continue;
						}
						_cacheValid[36] = false;
						throw new Win32Exception();
					}
				}
			}
			return _caretWidth;
		}
	}

	/// <summary>Gets a value indicating whether dragging of full windows is enabled. </summary>
	/// <returns>true if dragging of full windows is enabled; otherwise, false.</returns>
	public static bool DragFullWindows
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[38])
				{
					_cacheValid[38] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(38, 0, ref _dragFullWindows, 0))
					{
						_cacheValid[38] = false;
						throw new Win32Exception();
					}
				}
			}
			return _dragFullWindows;
		}
	}

	/// <summary>Gets the number of times the Set Foreground Window flashes the taskbar button when rejecting a foreground switch request.</summary>
	/// <returns>A flash count.</returns>
	public static int ForegroundFlashCount
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[37])
				{
					_cacheValid[37] = true;
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(8196, 0, ref _foregroundFlashCount, 0))
					{
						_cacheValid[37] = false;
						throw new Win32Exception();
					}
				}
			}
			return _foregroundFlashCount;
		}
	}

	internal static MS.Win32.NativeMethods.NONCLIENTMETRICS NonClientMetrics
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[39])
				{
					_cacheValid[39] = true;
					_ncm = new MS.Win32.NativeMethods.NONCLIENTMETRICS();
					if (!MS.Win32.UnsafeNativeMethods.SystemParametersInfo(41, _ncm.cbSize, _ncm, 0))
					{
						_cacheValid[39] = false;
						throw new Win32Exception();
					}
				}
			}
			return _ncm;
		}
	}

	/// <summary>Gets the metric that determines the border width of the nonclient area of a nonminimized window. </summary>
	/// <returns>A border width.</returns>
	public static double BorderWidth => ConvertPixel(NonClientMetrics.iBorderWidth);

	/// <summary>Gets the metric that determines the scroll width of the nonclient area of a nonminimized window. </summary>
	/// <returns>The scroll width, in pixels.</returns>
	public static double ScrollWidth => ConvertPixel(NonClientMetrics.iScrollWidth);

	/// <summary>Gets the metric that determines the scroll height of the nonclient area of a nonminimized window. </summary>
	/// <returns>The scroll height, in pixels.</returns>
	public static double ScrollHeight => ConvertPixel(NonClientMetrics.iScrollHeight);

	/// <summary>Gets the metric that determines the caption width for the nonclient area of a nonminimized window. </summary>
	/// <returns>The caption width.</returns>
	public static double CaptionWidth => ConvertPixel(NonClientMetrics.iCaptionWidth);

	/// <summary>Gets the metric that determines the caption height for the nonclient area of a nonminimized window. </summary>
	/// <returns>The caption height.</returns>
	public static double CaptionHeight => ConvertPixel(NonClientMetrics.iCaptionHeight);

	/// <summary>Gets the metric that determines the width of the small caption of the nonclient area of a nonminimized window. </summary>
	/// <returns>The caption width, in pixels.</returns>
	public static double SmallCaptionWidth => ConvertPixel(NonClientMetrics.iSmCaptionWidth);

	/// <summary>Gets the metric that determines the height of the small caption of the nonclient area of a nonminimized window. </summary>
	/// <returns>The caption height, in pixels.</returns>
	public static double SmallCaptionHeight => ConvertPixel(NonClientMetrics.iSmCaptionHeight);

	/// <summary>Gets the metric that determines the width of the menu. </summary>
	/// <returns>The menu width, in pixels.</returns>
	public static double MenuWidth => ConvertPixel(NonClientMetrics.iMenuWidth);

	/// <summary>Gets the metric that determines the height of the menu. </summary>
	/// <returns>The menu height.</returns>
	public static double MenuHeight => ConvertPixel(NonClientMetrics.iMenuHeight);

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MinimizeAnimation" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MinimizeAnimationKey
	{
		get
		{
			if (_cacheMinimizeAnimation == null)
			{
				_cacheMinimizeAnimation = CreateInstance(SystemResourceKeyID.MinimizeAnimation);
			}
			return _cacheMinimizeAnimation;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.Border" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey BorderKey
	{
		get
		{
			if (_cacheBorder == null)
			{
				_cacheBorder = CreateInstance(SystemResourceKeyID.Border);
			}
			return _cacheBorder;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.CaretWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey CaretWidthKey
	{
		get
		{
			if (_cacheCaretWidth == null)
			{
				_cacheCaretWidth = CreateInstance(SystemResourceKeyID.CaretWidth);
			}
			return _cacheCaretWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.ForegroundFlashCount" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey ForegroundFlashCountKey
	{
		get
		{
			if (_cacheForegroundFlashCount == null)
			{
				_cacheForegroundFlashCount = CreateInstance(SystemResourceKeyID.ForegroundFlashCount);
			}
			return _cacheForegroundFlashCount;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.DragFullWindows" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey DragFullWindowsKey
	{
		get
		{
			if (_cacheDragFullWindows == null)
			{
				_cacheDragFullWindows = CreateInstance(SystemResourceKeyID.DragFullWindows);
			}
			return _cacheDragFullWindows;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.BorderWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey BorderWidthKey
	{
		get
		{
			if (_cacheBorderWidth == null)
			{
				_cacheBorderWidth = CreateInstance(SystemResourceKeyID.BorderWidth);
			}
			return _cacheBorderWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.ScrollWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey ScrollWidthKey
	{
		get
		{
			if (_cacheScrollWidth == null)
			{
				_cacheScrollWidth = CreateInstance(SystemResourceKeyID.ScrollWidth);
			}
			return _cacheScrollWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.ScrollHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey ScrollHeightKey
	{
		get
		{
			if (_cacheScrollHeight == null)
			{
				_cacheScrollHeight = CreateInstance(SystemResourceKeyID.ScrollHeight);
			}
			return _cacheScrollHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.CaptionWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey CaptionWidthKey
	{
		get
		{
			if (_cacheCaptionWidth == null)
			{
				_cacheCaptionWidth = CreateInstance(SystemResourceKeyID.CaptionWidth);
			}
			return _cacheCaptionWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.CaptionHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey CaptionHeightKey
	{
		get
		{
			if (_cacheCaptionHeight == null)
			{
				_cacheCaptionHeight = CreateInstance(SystemResourceKeyID.CaptionHeight);
			}
			return _cacheCaptionHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.SmallCaptionWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey SmallCaptionWidthKey
	{
		get
		{
			if (_cacheSmallCaptionWidth == null)
			{
				_cacheSmallCaptionWidth = CreateInstance(SystemResourceKeyID.SmallCaptionWidth);
			}
			return _cacheSmallCaptionWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MenuWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MenuWidthKey
	{
		get
		{
			if (_cacheMenuWidth == null)
			{
				_cacheMenuWidth = CreateInstance(SystemResourceKeyID.MenuWidth);
			}
			return _cacheMenuWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MenuHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MenuHeightKey
	{
		get
		{
			if (_cacheMenuHeight == null)
			{
				_cacheMenuHeight = CreateInstance(SystemResourceKeyID.MenuHeight);
			}
			return _cacheMenuHeight;
		}
	}

	/// <summary>Gets a value that indicates the height, in pixels, of a horizontal window border. </summary>
	/// <returns>The height of a border.</returns>
	public static double ThinHorizontalBorderHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[40])
				{
					_cacheValid[40] = true;
					_thinHorizontalBorderHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXBORDER));
				}
			}
			return _thinHorizontalBorderHeight;
		}
	}

	/// <summary>Gets a value that indicates the width, in pixels, of a vertical window border. </summary>
	/// <returns>The width of a border.</returns>
	public static double ThinVerticalBorderWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[41])
				{
					_cacheValid[41] = true;
					_thinVerticalBorderWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYBORDER));
				}
			}
			return _thinVerticalBorderWidth;
		}
	}

	/// <summary>Gets the width, in pixels, of a cursor. </summary>
	/// <returns>The cursor width.</returns>
	public static double CursorWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[42])
				{
					_cacheValid[42] = true;
					_cursorWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXCURSOR));
				}
			}
			return _cursorWidth;
		}
	}

	/// <summary>Gets the height, in pixels, of a cursor. </summary>
	/// <returns>The cursor height.</returns>
	public static double CursorHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[43])
				{
					_cacheValid[43] = true;
					_cursorHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYCURSOR));
				}
			}
			return _cursorHeight;
		}
	}

	/// <summary>Gets a value that indicates the height, in pixels, of a 3-D border.   </summary>
	/// <returns>The height of a border.</returns>
	public static double ThickHorizontalBorderHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[44])
				{
					_cacheValid[44] = true;
					_thickHorizontalBorderHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXEDGE));
				}
			}
			return _thickHorizontalBorderHeight;
		}
	}

	/// <summary>Gets a value that indicates the width, in pixels, of a 3-D border.   </summary>
	/// <returns>The width of a border.</returns>
	public static double ThickVerticalBorderWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[45])
				{
					_cacheValid[45] = true;
					_thickVerticalBorderWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYEDGE));
				}
			}
			return _thickVerticalBorderWidth;
		}
	}

	/// <summary>Gets the width of a rectangle centered on a drag point to allow for limited movement of the mouse pointer before a drag operation begins.  </summary>
	/// <returns>The width of the rectangle, in pixels.</returns>
	public static double MinimumHorizontalDragDistance
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[46])
				{
					_cacheValid[46] = true;
					_minimumHorizontalDragDistance = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXDRAG));
				}
			}
			return _minimumHorizontalDragDistance;
		}
	}

	/// <summary>Gets the height of a rectangle centered on a drag point to allow for limited movement of the mouse pointer before a drag operation begins.  </summary>
	/// <returns>The height of the rectangle, in pixels.</returns>
	public static double MinimumVerticalDragDistance
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[47])
				{
					_cacheValid[47] = true;
					_minimumVerticalDragDistance = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYDRAG));
				}
			}
			return _minimumVerticalDragDistance;
		}
	}

	/// <summary>Gets the height of the horizontal border of the frame around a window. </summary>
	/// <returns>The border height.</returns>
	public static double FixedFrameHorizontalBorderHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[48])
				{
					_cacheValid[48] = true;
					_fixedFrameHorizontalBorderHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXFIXEDFRAME));
				}
			}
			return _fixedFrameHorizontalBorderHeight;
		}
	}

	/// <summary>Gets the width of the vertical border of the frame around a window. </summary>
	/// <returns>The border width.</returns>
	public static double FixedFrameVerticalBorderWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[49])
				{
					_cacheValid[49] = true;
					_fixedFrameVerticalBorderWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYFIXEDFRAME));
				}
			}
			return _fixedFrameVerticalBorderWidth;
		}
	}

	/// <summary>Gets the height of the upper and lower edges of the focus rectangle.  </summary>
	/// <returns>The edge height.</returns>
	public static double FocusHorizontalBorderHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[50])
				{
					_cacheValid[50] = true;
					_focusHorizontalBorderHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXFOCUSBORDER));
				}
			}
			return _focusHorizontalBorderHeight;
		}
	}

	/// <summary>Gets the width of the left and right edges of the focus rectangle.  </summary>
	/// <returns>The edge width.</returns>
	public static double FocusVerticalBorderWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[51])
				{
					_cacheValid[51] = true;
					_focusVerticalBorderWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYFOCUSBORDER));
				}
			}
			return _focusVerticalBorderWidth;
		}
	}

	/// <summary>Gets the width, in pixels, of the client area for a full-screen window on the primary display monitor.  </summary>
	/// <returns>The width of the client area.</returns>
	public static double FullPrimaryScreenWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[52])
				{
					_cacheValid[52] = true;
					_fullPrimaryScreenWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXFULLSCREEN));
				}
			}
			return _fullPrimaryScreenWidth;
		}
	}

	/// <summary>Gets the height, in pixels, of the client area for a full-screen window on the primary display monitor.  </summary>
	/// <returns>The height of the client area.</returns>
	public static double FullPrimaryScreenHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[53])
				{
					_cacheValid[53] = true;
					_fullPrimaryScreenHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYFULLSCREEN));
				}
			}
			return _fullPrimaryScreenHeight;
		}
	}

	/// <summary>Gets the width, in pixels, of the arrow bitmap on a horizontal scroll bar. </summary>
	/// <returns>The width of the arrow bitmap.</returns>
	public static double HorizontalScrollBarButtonWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[54])
				{
					_cacheValid[54] = true;
					_horizontalScrollBarButtonWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXHSCROLL));
				}
			}
			return _horizontalScrollBarButtonWidth;
		}
	}

	/// <summary>Gets the height of a horizontal scroll bar, in pixels. </summary>
	/// <returns>The height of the scroll bar.</returns>
	public static double HorizontalScrollBarHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[55])
				{
					_cacheValid[55] = true;
					_horizontalScrollBarHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYHSCROLL));
				}
			}
			return _horizontalScrollBarHeight;
		}
	}

	/// <summary>Gets the width, in pixels, of the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> in a horizontal scroll bar. </summary>
	/// <returns>The width of the thumb.</returns>
	public static double HorizontalScrollBarThumbWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[56])
				{
					_cacheValid[56] = true;
					_horizontalScrollBarThumbWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXHTHUMB));
				}
			}
			return _horizontalScrollBarThumbWidth;
		}
	}

	/// <summary>Gets the default width of an icon. </summary>
	/// <returns>The icon width.</returns>
	public static double IconWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[57])
				{
					_cacheValid[57] = true;
					_iconWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXICON));
				}
			}
			return _iconWidth;
		}
	}

	/// <summary>Gets the default height of an icon. </summary>
	/// <returns>The icon height.</returns>
	public static double IconHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[58])
				{
					_cacheValid[58] = true;
					_iconHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYICON));
				}
			}
			return _iconHeight;
		}
	}

	/// <summary>Gets the width of a grid that a large icon will fit into. </summary>
	/// <returns>The grid width.</returns>
	public static double IconGridWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[59])
				{
					_cacheValid[59] = true;
					_iconGridWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXICONSPACING));
				}
			}
			return _iconGridWidth;
		}
	}

	/// <summary>Gets the height of a grid in which a large icon will fit. </summary>
	/// <returns>The grid height.</returns>
	public static double IconGridHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[60])
				{
					_cacheValid[60] = true;
					_iconGridHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYICONSPACING));
				}
			}
			return _iconGridHeight;
		}
	}

	/// <summary>Gets a value that indicates the width, in pixels, of a maximized top-level window on the primary display monitor.  </summary>
	/// <returns>The window width.</returns>
	public static double MaximizedPrimaryScreenWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[61])
				{
					_cacheValid[61] = true;
					_maximizedPrimaryScreenWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXMAXIMIZED));
				}
			}
			return _maximizedPrimaryScreenWidth;
		}
	}

	/// <summary>Gets a value that indicates the height, in pixels, of a maximized top-level window on the primary display monitor.  </summary>
	/// <returns>The window height.</returns>
	public static double MaximizedPrimaryScreenHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[62])
				{
					_cacheValid[62] = true;
					_maximizedPrimaryScreenHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYMAXIMIZED));
				}
			}
			return _maximizedPrimaryScreenHeight;
		}
	}

	/// <summary>Gets a value that indicates the maximum width, in pixels, of a window that has a caption and sizing borders.  </summary>
	/// <returns>The maximum window width.</returns>
	public static double MaximumWindowTrackWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[63])
				{
					_cacheValid[63] = true;
					_maximumWindowTrackWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXMAXTRACK));
				}
			}
			return _maximumWindowTrackWidth;
		}
	}

	/// <summary>Gets a value that indicates the maximum height, in pixels, of a window that has a caption and sizing borders.  </summary>
	/// <returns>The maximum window height.</returns>
	public static double MaximumWindowTrackHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[64])
				{
					_cacheValid[64] = true;
					_maximumWindowTrackHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYMAXTRACK));
				}
			}
			return _maximumWindowTrackHeight;
		}
	}

	/// <summary>Gets a value that indicates the width, in pixels, of the default menu check-mark bitmap.  </summary>
	/// <returns>The width of the bitmap.</returns>
	public static double MenuCheckmarkWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[65])
				{
					_cacheValid[65] = true;
					_menuCheckmarkWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXMENUCHECK));
				}
			}
			return _menuCheckmarkWidth;
		}
	}

	/// <summary>Gets a value that indicates the height, in pixels, of the default menu check-mark bitmap.  </summary>
	/// <returns>The height of a bitmap.</returns>
	public static double MenuCheckmarkHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[66])
				{
					_cacheValid[66] = true;
					_menuCheckmarkHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYMENUCHECK));
				}
			}
			return _menuCheckmarkHeight;
		}
	}

	/// <summary>Gets a value that indicates the width, in pixels, of a menu bar button.  </summary>
	/// <returns>The width of a menu bar button.</returns>
	public static double MenuButtonWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[67])
				{
					_cacheValid[67] = true;
					_menuButtonWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXMENUSIZE));
				}
			}
			return _menuButtonWidth;
		}
	}

	/// <summary>Gets a value that indicates the height, in pixels, of a menu bar button.  </summary>
	/// <returns>The height of a menu bar button.</returns>
	public static double MenuButtonHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[68])
				{
					_cacheValid[68] = true;
					_menuButtonHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYMENUSIZE));
				}
			}
			return _menuButtonHeight;
		}
	}

	/// <summary>Gets a value that indicates the minimum width, in pixels, of a window.  </summary>
	/// <returns>The minimum width of a window.</returns>
	public static double MinimumWindowWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[69])
				{
					_cacheValid[69] = true;
					_minimumWindowWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXMIN));
				}
			}
			return _minimumWindowWidth;
		}
	}

	/// <summary>Gets a value that indicates the minimum height, in pixels, of a window.  </summary>
	/// <returns>The minimum height of a window.</returns>
	public static double MinimumWindowHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[70])
				{
					_cacheValid[70] = true;
					_minimumWindowHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYMIN));
				}
			}
			return _minimumWindowHeight;
		}
	}

	/// <summary>Gets a value that indicates the width, in pixels, of a minimized window.  </summary>
	/// <returns>The width of a minimized window.</returns>
	public static double MinimizedWindowWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[71])
				{
					_cacheValid[71] = true;
					_minimizedWindowWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXMINIMIZED));
				}
			}
			return _minimizedWindowWidth;
		}
	}

	/// <summary>Gets a value that indicates the height, in pixels, of a minimized window.  </summary>
	/// <returns>The height of a minimized window.</returns>
	public static double MinimizedWindowHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[72])
				{
					_cacheValid[72] = true;
					_minimizedWindowHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYMINIMIZED));
				}
			}
			return _minimizedWindowHeight;
		}
	}

	/// <summary>Gets a value that indicates the width, in pixels, of a grid cell for a minimized window.  </summary>
	/// <returns>The width of a grid cell for a minimized window.</returns>
	public static double MinimizedGridWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[73])
				{
					_cacheValid[73] = true;
					_minimizedGridWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXMINSPACING));
				}
			}
			return _minimizedGridWidth;
		}
	}

	/// <summary>Gets a value that indicates the height, in pixels, of a grid cell for a minimized window.  </summary>
	/// <returns>The height of a grid cell for a minimized window.</returns>
	public static double MinimizedGridHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[74])
				{
					_cacheValid[74] = true;
					_minimizedGridHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYMINSPACING));
				}
			}
			return _minimizedGridHeight;
		}
	}

	/// <summary>Gets a value that indicates the minimum tracking width of a window, in pixels.   </summary>
	/// <returns>The minimum tracking width of a window.</returns>
	public static double MinimumWindowTrackWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[75])
				{
					_cacheValid[75] = true;
					_minimumWindowTrackWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXMINTRACK));
				}
			}
			return _minimumWindowTrackWidth;
		}
	}

	/// <summary>Gets a value that indicates the minimum tracking height of a window, in pixels.   </summary>
	/// <returns>The minimun tracking height of a window.</returns>
	public static double MinimumWindowTrackHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[76])
				{
					_cacheValid[76] = true;
					_minimumWindowTrackHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYMINTRACK));
				}
			}
			return _minimumWindowTrackHeight;
		}
	}

	/// <summary>Gets a value that indicates the screen width, in pixels, of the primary display monitor.   </summary>
	/// <returns>The width of the screen.</returns>
	public static double PrimaryScreenWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[77])
				{
					_cacheValid[77] = true;
					_primaryScreenWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXSCREEN));
				}
			}
			return _primaryScreenWidth;
		}
	}

	/// <summary>Gets a value that indicates the screen height, in pixels, of the primary display monitor.   </summary>
	/// <returns>The height of the screen.</returns>
	public static double PrimaryScreenHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[78])
				{
					_cacheValid[78] = true;
					_primaryScreenHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYSCREEN));
				}
			}
			return _primaryScreenHeight;
		}
	}

	/// <summary>Gets a value that indicates the width, in pixels, of a button in the title bar of a window.  </summary>
	/// <returns>The width of a caption button.</returns>
	public static double WindowCaptionButtonWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[79])
				{
					_cacheValid[79] = true;
					_windowCaptionButtonWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXSIZE));
				}
			}
			return _windowCaptionButtonWidth;
		}
	}

	/// <summary>Gets a value that indicates the height, in pixels, of a button in the title bar of a window.  </summary>
	/// <returns>The height of a caption button.</returns>
	public static double WindowCaptionButtonHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[80])
				{
					_cacheValid[80] = true;
					_windowCaptionButtonHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYSIZE));
				}
			}
			return _windowCaptionButtonHeight;
		}
	}

	/// <summary>Gets a value that indicates the height (thickness), in pixels, of the horizontal sizing border around the perimeter of a window that can be resized.   </summary>
	/// <returns>The height of the border.</returns>
	public static double ResizeFrameHorizontalBorderHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[81])
				{
					_cacheValid[81] = true;
					_resizeFrameHorizontalBorderHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXFRAME));
				}
			}
			return _resizeFrameHorizontalBorderHeight;
		}
	}

	/// <summary>Gets a value that indicates the width (thickness), in pixels, of the vertical sizing border around the perimeter of a window that can be resized.   </summary>
	/// <returns>The width of the border.</returns>
	public static double ResizeFrameVerticalBorderWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[82])
				{
					_cacheValid[82] = true;
					_resizeFrameVerticalBorderWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYFRAME));
				}
			}
			return _resizeFrameVerticalBorderWidth;
		}
	}

	/// <summary>Gets a value that indicates the recommended width, in pixels, of a small icon. </summary>
	/// <returns>The width of the icon.</returns>
	public static double SmallIconWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[83])
				{
					_cacheValid[83] = true;
					_smallIconWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXSMICON));
				}
			}
			return _smallIconWidth;
		}
	}

	/// <summary>Gets a value that indicates the recommended height, in pixels, of a small icon. </summary>
	/// <returns>The icon height.</returns>
	public static double SmallIconHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[84])
				{
					_cacheValid[84] = true;
					_smallIconHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYSMICON));
				}
			}
			return _smallIconHeight;
		}
	}

	/// <summary>Gets a value that indicates the width, in pixels, of small caption buttons.  </summary>
	/// <returns>The width of the caption button.</returns>
	public static double SmallWindowCaptionButtonWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[85])
				{
					_cacheValid[85] = true;
					_smallWindowCaptionButtonWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXSMSIZE));
				}
			}
			return _smallWindowCaptionButtonWidth;
		}
	}

	/// <summary>Gets a value that indicates the height, in pixels, of small caption buttons.  </summary>
	/// <returns>The height of the caption button.</returns>
	public static double SmallWindowCaptionButtonHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[86])
				{
					_cacheValid[86] = true;
					_smallWindowCaptionButtonHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYSMSIZE));
				}
			}
			return _smallWindowCaptionButtonHeight;
		}
	}

	/// <summary>Gets a value that indicates the width, in pixels, of the virtual screen.   </summary>
	/// <returns>The width of the virtual screen.</returns>
	public static double VirtualScreenWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[87])
				{
					_cacheValid[87] = true;
					_virtualScreenWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXVIRTUALSCREEN));
				}
			}
			return _virtualScreenWidth;
		}
	}

	/// <summary>Gets a value that indicates the height, in pixels, of the virtual screen.   </summary>
	/// <returns>The height of the virtual screen.</returns>
	public static double VirtualScreenHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[88])
				{
					_cacheValid[88] = true;
					_virtualScreenHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYVIRTUALSCREEN));
				}
			}
			return _virtualScreenHeight;
		}
	}

	/// <summary>Gets a value that indicates the width, in pixels, of a vertical scroll bar.  </summary>
	/// <returns>The width of a scroll bar.</returns>
	public static double VerticalScrollBarWidth
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[89])
				{
					_cacheValid[89] = true;
					_verticalScrollBarWidth = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CXVSCROLL));
				}
			}
			return _verticalScrollBarWidth;
		}
	}

	/// <summary>Gets a value that indicates the height, in pixels, of the arrow bitmap on a vertical scroll bar.  </summary>
	/// <returns>The height of a bitmap.</returns>
	public static double VerticalScrollBarButtonHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[90])
				{
					_cacheValid[90] = true;
					_verticalScrollBarButtonHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYVSCROLL));
				}
			}
			return _verticalScrollBarButtonHeight;
		}
	}

	/// <summary>Gets a value that indicates the height, in pixels, of a caption area.  </summary>
	/// <returns>The height of a caption area.</returns>
	public static double WindowCaptionHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[91])
				{
					_cacheValid[91] = true;
					_windowCaptionHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYCAPTION));
				}
			}
			return _windowCaptionHeight;
		}
	}

	/// <summary>Gets a value that indicates the height, in pixels, of the kanji window at the bottom of the screen for systems that use double-byte characters.  </summary>
	/// <returns>The window height.</returns>
	public static double KanjiWindowHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[92])
				{
					_cacheValid[92] = true;
					_kanjiWindowHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYKANJIWINDOW));
				}
			}
			return _kanjiWindowHeight;
		}
	}

	/// <summary>Gets a value that indicates the height, in pixels, of a single-line menu bar.  </summary>
	/// <returns>The height of the menu bar.</returns>
	public static double MenuBarHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[93])
				{
					_cacheValid[93] = true;
					_menuBarHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYMENU));
				}
			}
			return _menuBarHeight;
		}
	}

	/// <summary>Gets a value that indicates the height, in pixels, of the thumb in a vertical scroll bar.  </summary>
	/// <returns>The height of the thumb.</returns>
	public static double VerticalScrollBarThumbHeight
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[94])
				{
					_cacheValid[94] = true;
					_verticalScrollBarThumbHeight = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.CYVTHUMB));
				}
			}
			return _verticalScrollBarThumbHeight;
		}
	}

	/// <summary>Gets a value that indicates whether the system is ready to use a Unicode-based Input Method Editor (IME) on a Unicode application.  </summary>
	/// <returns>true if the Input Method Manager/Input Method Editor features are enabled; otherwise, false. </returns>
	public static bool IsImmEnabled
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[95])
				{
					_cacheValid[95] = true;
					_isImmEnabled = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.IMMENABLED) != 0;
				}
			}
			return _isImmEnabled;
		}
	}

	/// <summary>Gets a value that indicates whether the current operating system is the Microsoft Windows XP Media Center Edition. </summary>
	/// <returns>true if the current operating system is Windows XP Media Center Edition; otherwise, false.</returns>
	public static bool IsMediaCenter
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[96])
				{
					_cacheValid[96] = true;
					_isMediaCenter = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.MEDIACENTER) != 0;
				}
			}
			return _isMediaCenter;
		}
	}

	/// <summary>Gets a value that indicates whether drop-down menus are right-aligned with the corresponding menu item. </summary>
	/// <returns>true if drop-down menus are right-aligned; otherwise, false.</returns>
	public static bool IsMenuDropRightAligned
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[97])
				{
					_cacheValid[97] = true;
					_isMenuDropRightAligned = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.MENUDROPALIGNMENT) != 0;
				}
			}
			return _isMenuDropRightAligned;
		}
	}

	/// <summary>Gets a value that indicates whether the system is enabled for Hebrew and Arabic languages. </summary>
	/// <returns>true if the system is enabled for Hebrew and Arabic languages; otherwise, false.</returns>
	public static bool IsMiddleEastEnabled
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[98])
				{
					_cacheValid[98] = true;
					_isMiddleEastEnabled = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.MIDEASTENABLED) != 0;
				}
			}
			return _isMiddleEastEnabled;
		}
	}

	/// <summary>Gets a value that indicates whether a mouse is installed. </summary>
	/// <returns>true if a mouse is installed; otherwise, false.</returns>
	public static bool IsMousePresent
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[99])
				{
					_cacheValid[99] = true;
					_isMousePresent = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.MOUSEPRESENT) != 0;
				}
			}
			return _isMousePresent;
		}
	}

	/// <summary>Gets a value that indicates whether the installed mouse has a vertical scroll wheel. </summary>
	/// <returns>true if the installed mouse has a vertical scroll wheel; otherwise, false.</returns>
	public static bool IsMouseWheelPresent
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[100])
				{
					_cacheValid[100] = true;
					_isMouseWheelPresent = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.MOUSEWHEELPRESENT) != 0;
				}
			}
			return _isMouseWheelPresent;
		}
	}

	/// <summary>Gets a value that indicates whether Microsoft Windows for Pen Computing extensions are installed. </summary>
	/// <returns>true if Pen Computing extensions are installed; otherwise, false. </returns>
	public static bool IsPenWindows
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[101])
				{
					_cacheValid[101] = true;
					_isPenWindows = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.PENWINDOWS) != 0;
				}
			}
			return _isPenWindows;
		}
	}

	/// <summary>Gets a value that indicates whether the current session is remotely controlled. </summary>
	/// <returns>true if the current session is remotely controlled; otherwise, false.</returns>
	public static bool IsRemotelyControlled
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[102])
				{
					_cacheValid[102] = true;
					_isRemotelyControlled = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.REMOTECONTROL) != 0;
				}
			}
			return _isRemotelyControlled;
		}
	}

	/// <summary>Gets a value that indicates whether the calling process is associated with a Terminal Services client session. </summary>
	/// <returns>true if the calling process is associated with a Terminal Services client session; false if the calling process is associated with the Terminal Server console session.</returns>
	public static bool IsRemoteSession
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[103])
				{
					_cacheValid[103] = true;
					_isRemoteSession = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.REMOTESESSION) != 0;
				}
			}
			return _isRemoteSession;
		}
	}

	/// <summary>Gets a value that indicates whether the user requires information in visual format. </summary>
	/// <returns>true if the user requires an application to present information visually where it typically presents the information only in audible form; otherwise false.</returns>
	public static bool ShowSounds
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[104])
				{
					_cacheValid[104] = true;
					_showSounds = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.SHOWSOUNDS) != 0;
				}
			}
			return _showSounds;
		}
	}

	/// <summary>Gets a value that indicates whether the computer has a low-end (slow) processor. </summary>
	/// <returns>true if the computer has a low-end (slow) processor; otherwise, false.</returns>
	public static bool IsSlowMachine
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[105])
				{
					_cacheValid[105] = true;
					_isSlowMachine = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.SLOWMACHINE) != 0;
				}
			}
			return _isSlowMachine;
		}
	}

	/// <summary>Gets a value that indicates whether the functionality of the left and right mouse buttons are swapped.  </summary>
	/// <returns>true if the functionality of the left and right mouse buttons are swapped; otherwise false.</returns>
	public static bool SwapButtons
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[106])
				{
					_cacheValid[106] = true;
					_swapButtons = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.SWAPBUTTON) != 0;
				}
			}
			return _swapButtons;
		}
	}

	/// <summary>Gets a value that indicates whether the current operating system is Microsoft Windows XP Tablet PC Edition. </summary>
	/// <returns>true if the current operating system is Windows XP Tablet PC Edition; otherwise, false.</returns>
	public static bool IsTabletPC
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[107])
				{
					_cacheValid[107] = true;
					_isTabletPC = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.TABLETPC) != 0;
				}
			}
			return _isTabletPC;
		}
	}

	/// <summary>Gets a value that indicates the coordinate for the left side of the virtual screen.   </summary>
	/// <returns>A screen coordinate, in pixels.</returns>
	public static double VirtualScreenLeft
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[108])
				{
					_cacheValid[108] = true;
					_virtualScreenLeft = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.XVIRTUALSCREEN));
				}
			}
			return _virtualScreenLeft;
		}
	}

	/// <summary>Gets a value that indicates the upper coordinate of the virtual screen. </summary>
	/// <returns>A screen coordinate, in pixels.</returns>
	public static double VirtualScreenTop
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[109])
				{
					_cacheValid[109] = true;
					_virtualScreenTop = ConvertPixel(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(MS.Internal.Interop.SM.YVIRTUALSCREEN));
				}
			}
			return _virtualScreenTop;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.ThinHorizontalBorderHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey ThinHorizontalBorderHeightKey
	{
		get
		{
			if (_cacheThinHorizontalBorderHeight == null)
			{
				_cacheThinHorizontalBorderHeight = CreateInstance(SystemResourceKeyID.ThinHorizontalBorderHeight);
			}
			return _cacheThinHorizontalBorderHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.ThinVerticalBorderWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey ThinVerticalBorderWidthKey
	{
		get
		{
			if (_cacheThinVerticalBorderWidth == null)
			{
				_cacheThinVerticalBorderWidth = CreateInstance(SystemResourceKeyID.ThinVerticalBorderWidth);
			}
			return _cacheThinVerticalBorderWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.CursorWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey CursorWidthKey
	{
		get
		{
			if (_cacheCursorWidth == null)
			{
				_cacheCursorWidth = CreateInstance(SystemResourceKeyID.CursorWidth);
			}
			return _cacheCursorWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.CursorHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey CursorHeightKey
	{
		get
		{
			if (_cacheCursorHeight == null)
			{
				_cacheCursorHeight = CreateInstance(SystemResourceKeyID.CursorHeight);
			}
			return _cacheCursorHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.ThickHorizontalBorderHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey ThickHorizontalBorderHeightKey
	{
		get
		{
			if (_cacheThickHorizontalBorderHeight == null)
			{
				_cacheThickHorizontalBorderHeight = CreateInstance(SystemResourceKeyID.ThickHorizontalBorderHeight);
			}
			return _cacheThickHorizontalBorderHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.ThickVerticalBorderWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey ThickVerticalBorderWidthKey
	{
		get
		{
			if (_cacheThickVerticalBorderWidth == null)
			{
				_cacheThickVerticalBorderWidth = CreateInstance(SystemResourceKeyID.ThickVerticalBorderWidth);
			}
			return _cacheThickVerticalBorderWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.FixedFrameHorizontalBorderHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey FixedFrameHorizontalBorderHeightKey
	{
		get
		{
			if (_cacheFixedFrameHorizontalBorderHeight == null)
			{
				_cacheFixedFrameHorizontalBorderHeight = CreateInstance(SystemResourceKeyID.FixedFrameHorizontalBorderHeight);
			}
			return _cacheFixedFrameHorizontalBorderHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.FixedFrameVerticalBorderWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey FixedFrameVerticalBorderWidthKey
	{
		get
		{
			if (_cacheFixedFrameVerticalBorderWidth == null)
			{
				_cacheFixedFrameVerticalBorderWidth = CreateInstance(SystemResourceKeyID.FixedFrameVerticalBorderWidth);
			}
			return _cacheFixedFrameVerticalBorderWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.FocusHorizontalBorderHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey FocusHorizontalBorderHeightKey
	{
		get
		{
			if (_cacheFocusHorizontalBorderHeight == null)
			{
				_cacheFocusHorizontalBorderHeight = CreateInstance(SystemResourceKeyID.FocusHorizontalBorderHeight);
			}
			return _cacheFocusHorizontalBorderHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.FocusVerticalBorderWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey FocusVerticalBorderWidthKey
	{
		get
		{
			if (_cacheFocusVerticalBorderWidth == null)
			{
				_cacheFocusVerticalBorderWidth = CreateInstance(SystemResourceKeyID.FocusVerticalBorderWidth);
			}
			return _cacheFocusVerticalBorderWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.FullPrimaryScreenWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey FullPrimaryScreenWidthKey
	{
		get
		{
			if (_cacheFullPrimaryScreenWidth == null)
			{
				_cacheFullPrimaryScreenWidth = CreateInstance(SystemResourceKeyID.FullPrimaryScreenWidth);
			}
			return _cacheFullPrimaryScreenWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.FullPrimaryScreenHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey FullPrimaryScreenHeightKey
	{
		get
		{
			if (_cacheFullPrimaryScreenHeight == null)
			{
				_cacheFullPrimaryScreenHeight = CreateInstance(SystemResourceKeyID.FullPrimaryScreenHeight);
			}
			return _cacheFullPrimaryScreenHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.HorizontalScrollBarButtonWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey HorizontalScrollBarButtonWidthKey
	{
		get
		{
			if (_cacheHorizontalScrollBarButtonWidth == null)
			{
				_cacheHorizontalScrollBarButtonWidth = CreateInstance(SystemResourceKeyID.HorizontalScrollBarButtonWidth);
			}
			return _cacheHorizontalScrollBarButtonWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.HorizontalScrollBarHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey HorizontalScrollBarHeightKey
	{
		get
		{
			if (_cacheHorizontalScrollBarHeight == null)
			{
				_cacheHorizontalScrollBarHeight = CreateInstance(SystemResourceKeyID.HorizontalScrollBarHeight);
			}
			return _cacheHorizontalScrollBarHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.HorizontalScrollBarThumbWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey HorizontalScrollBarThumbWidthKey
	{
		get
		{
			if (_cacheHorizontalScrollBarThumbWidth == null)
			{
				_cacheHorizontalScrollBarThumbWidth = CreateInstance(SystemResourceKeyID.HorizontalScrollBarThumbWidth);
			}
			return _cacheHorizontalScrollBarThumbWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.IconWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IconWidthKey
	{
		get
		{
			if (_cacheIconWidth == null)
			{
				_cacheIconWidth = CreateInstance(SystemResourceKeyID.IconWidth);
			}
			return _cacheIconWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.IconHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IconHeightKey
	{
		get
		{
			if (_cacheIconHeight == null)
			{
				_cacheIconHeight = CreateInstance(SystemResourceKeyID.IconHeight);
			}
			return _cacheIconHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.IconGridWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IconGridWidthKey
	{
		get
		{
			if (_cacheIconGridWidth == null)
			{
				_cacheIconGridWidth = CreateInstance(SystemResourceKeyID.IconGridWidth);
			}
			return _cacheIconGridWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.IconGridHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IconGridHeightKey
	{
		get
		{
			if (_cacheIconGridHeight == null)
			{
				_cacheIconGridHeight = CreateInstance(SystemResourceKeyID.IconGridHeight);
			}
			return _cacheIconGridHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MaximizedPrimaryScreenWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MaximizedPrimaryScreenWidthKey
	{
		get
		{
			if (_cacheMaximizedPrimaryScreenWidth == null)
			{
				_cacheMaximizedPrimaryScreenWidth = CreateInstance(SystemResourceKeyID.MaximizedPrimaryScreenWidth);
			}
			return _cacheMaximizedPrimaryScreenWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MaximizedPrimaryScreenHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MaximizedPrimaryScreenHeightKey
	{
		get
		{
			if (_cacheMaximizedPrimaryScreenHeight == null)
			{
				_cacheMaximizedPrimaryScreenHeight = CreateInstance(SystemResourceKeyID.MaximizedPrimaryScreenHeight);
			}
			return _cacheMaximizedPrimaryScreenHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MaximumWindowTrackWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MaximumWindowTrackWidthKey
	{
		get
		{
			if (_cacheMaximumWindowTrackWidth == null)
			{
				_cacheMaximumWindowTrackWidth = CreateInstance(SystemResourceKeyID.MaximumWindowTrackWidth);
			}
			return _cacheMaximumWindowTrackWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MaximumWindowTrackHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MaximumWindowTrackHeightKey
	{
		get
		{
			if (_cacheMaximumWindowTrackHeight == null)
			{
				_cacheMaximumWindowTrackHeight = CreateInstance(SystemResourceKeyID.MaximumWindowTrackHeight);
			}
			return _cacheMaximumWindowTrackHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MenuCheckmarkWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MenuCheckmarkWidthKey
	{
		get
		{
			if (_cacheMenuCheckmarkWidth == null)
			{
				_cacheMenuCheckmarkWidth = CreateInstance(SystemResourceKeyID.MenuCheckmarkWidth);
			}
			return _cacheMenuCheckmarkWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MenuCheckmarkHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MenuCheckmarkHeightKey
	{
		get
		{
			if (_cacheMenuCheckmarkHeight == null)
			{
				_cacheMenuCheckmarkHeight = CreateInstance(SystemResourceKeyID.MenuCheckmarkHeight);
			}
			return _cacheMenuCheckmarkHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MenuButtonWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MenuButtonWidthKey
	{
		get
		{
			if (_cacheMenuButtonWidth == null)
			{
				_cacheMenuButtonWidth = CreateInstance(SystemResourceKeyID.MenuButtonWidth);
			}
			return _cacheMenuButtonWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MenuButtonHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MenuButtonHeightKey
	{
		get
		{
			if (_cacheMenuButtonHeight == null)
			{
				_cacheMenuButtonHeight = CreateInstance(SystemResourceKeyID.MenuButtonHeight);
			}
			return _cacheMenuButtonHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MinimumWindowWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MinimumWindowWidthKey
	{
		get
		{
			if (_cacheMinimumWindowWidth == null)
			{
				_cacheMinimumWindowWidth = CreateInstance(SystemResourceKeyID.MinimumWindowWidth);
			}
			return _cacheMinimumWindowWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MinimumWindowHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MinimumWindowHeightKey
	{
		get
		{
			if (_cacheMinimumWindowHeight == null)
			{
				_cacheMinimumWindowHeight = CreateInstance(SystemResourceKeyID.MinimumWindowHeight);
			}
			return _cacheMinimumWindowHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MinimizedWindowWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MinimizedWindowWidthKey
	{
		get
		{
			if (_cacheMinimizedWindowWidth == null)
			{
				_cacheMinimizedWindowWidth = CreateInstance(SystemResourceKeyID.MinimizedWindowWidth);
			}
			return _cacheMinimizedWindowWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MinimizedWindowHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MinimizedWindowHeightKey
	{
		get
		{
			if (_cacheMinimizedWindowHeight == null)
			{
				_cacheMinimizedWindowHeight = CreateInstance(SystemResourceKeyID.MinimizedWindowHeight);
			}
			return _cacheMinimizedWindowHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MinimizedGridWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MinimizedGridWidthKey
	{
		get
		{
			if (_cacheMinimizedGridWidth == null)
			{
				_cacheMinimizedGridWidth = CreateInstance(SystemResourceKeyID.MinimizedGridWidth);
			}
			return _cacheMinimizedGridWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MinimizedGridHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MinimizedGridHeightKey
	{
		get
		{
			if (_cacheMinimizedGridHeight == null)
			{
				_cacheMinimizedGridHeight = CreateInstance(SystemResourceKeyID.MinimizedGridHeight);
			}
			return _cacheMinimizedGridHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MinimumWindowTrackWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MinimumWindowTrackWidthKey
	{
		get
		{
			if (_cacheMinimumWindowTrackWidth == null)
			{
				_cacheMinimumWindowTrackWidth = CreateInstance(SystemResourceKeyID.MinimumWindowTrackWidth);
			}
			return _cacheMinimumWindowTrackWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MinimumWindowTrackHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MinimumWindowTrackHeightKey
	{
		get
		{
			if (_cacheMinimumWindowTrackHeight == null)
			{
				_cacheMinimumWindowTrackHeight = CreateInstance(SystemResourceKeyID.MinimumWindowTrackHeight);
			}
			return _cacheMinimumWindowTrackHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.PrimaryScreenWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey PrimaryScreenWidthKey
	{
		get
		{
			if (_cachePrimaryScreenWidth == null)
			{
				_cachePrimaryScreenWidth = CreateInstance(SystemResourceKeyID.PrimaryScreenWidth);
			}
			return _cachePrimaryScreenWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.PrimaryScreenHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey PrimaryScreenHeightKey
	{
		get
		{
			if (_cachePrimaryScreenHeight == null)
			{
				_cachePrimaryScreenHeight = CreateInstance(SystemResourceKeyID.PrimaryScreenHeight);
			}
			return _cachePrimaryScreenHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.WindowCaptionButtonWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey WindowCaptionButtonWidthKey
	{
		get
		{
			if (_cacheWindowCaptionButtonWidth == null)
			{
				_cacheWindowCaptionButtonWidth = CreateInstance(SystemResourceKeyID.WindowCaptionButtonWidth);
			}
			return _cacheWindowCaptionButtonWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.WindowCaptionButtonHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey WindowCaptionButtonHeightKey
	{
		get
		{
			if (_cacheWindowCaptionButtonHeight == null)
			{
				_cacheWindowCaptionButtonHeight = CreateInstance(SystemResourceKeyID.WindowCaptionButtonHeight);
			}
			return _cacheWindowCaptionButtonHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.ResizeFrameHorizontalBorderHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey ResizeFrameHorizontalBorderHeightKey
	{
		get
		{
			if (_cacheResizeFrameHorizontalBorderHeight == null)
			{
				_cacheResizeFrameHorizontalBorderHeight = CreateInstance(SystemResourceKeyID.ResizeFrameHorizontalBorderHeight);
			}
			return _cacheResizeFrameHorizontalBorderHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.ResizeFrameVerticalBorderWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey ResizeFrameVerticalBorderWidthKey
	{
		get
		{
			if (_cacheResizeFrameVerticalBorderWidth == null)
			{
				_cacheResizeFrameVerticalBorderWidth = CreateInstance(SystemResourceKeyID.ResizeFrameVerticalBorderWidth);
			}
			return _cacheResizeFrameVerticalBorderWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.SmallIconWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey SmallIconWidthKey
	{
		get
		{
			if (_cacheSmallIconWidth == null)
			{
				_cacheSmallIconWidth = CreateInstance(SystemResourceKeyID.SmallIconWidth);
			}
			return _cacheSmallIconWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.SmallIconHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey SmallIconHeightKey
	{
		get
		{
			if (_cacheSmallIconHeight == null)
			{
				_cacheSmallIconHeight = CreateInstance(SystemResourceKeyID.SmallIconHeight);
			}
			return _cacheSmallIconHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.SmallWindowCaptionButtonWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey SmallWindowCaptionButtonWidthKey
	{
		get
		{
			if (_cacheSmallWindowCaptionButtonWidth == null)
			{
				_cacheSmallWindowCaptionButtonWidth = CreateInstance(SystemResourceKeyID.SmallWindowCaptionButtonWidth);
			}
			return _cacheSmallWindowCaptionButtonWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.SmallWindowCaptionButtonHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey SmallWindowCaptionButtonHeightKey
	{
		get
		{
			if (_cacheSmallWindowCaptionButtonHeight == null)
			{
				_cacheSmallWindowCaptionButtonHeight = CreateInstance(SystemResourceKeyID.SmallWindowCaptionButtonHeight);
			}
			return _cacheSmallWindowCaptionButtonHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.VirtualScreenWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey VirtualScreenWidthKey
	{
		get
		{
			if (_cacheVirtualScreenWidth == null)
			{
				_cacheVirtualScreenWidth = CreateInstance(SystemResourceKeyID.VirtualScreenWidth);
			}
			return _cacheVirtualScreenWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.VirtualScreenHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey VirtualScreenHeightKey
	{
		get
		{
			if (_cacheVirtualScreenHeight == null)
			{
				_cacheVirtualScreenHeight = CreateInstance(SystemResourceKeyID.VirtualScreenHeight);
			}
			return _cacheVirtualScreenHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.VerticalScrollBarWidth" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey VerticalScrollBarWidthKey
	{
		get
		{
			if (_cacheVerticalScrollBarWidth == null)
			{
				_cacheVerticalScrollBarWidth = CreateInstance(SystemResourceKeyID.VerticalScrollBarWidth);
			}
			return _cacheVerticalScrollBarWidth;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.VerticalScrollBarButtonHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey VerticalScrollBarButtonHeightKey
	{
		get
		{
			if (_cacheVerticalScrollBarButtonHeight == null)
			{
				_cacheVerticalScrollBarButtonHeight = CreateInstance(SystemResourceKeyID.VerticalScrollBarButtonHeight);
			}
			return _cacheVerticalScrollBarButtonHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.WindowCaptionHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey WindowCaptionHeightKey
	{
		get
		{
			if (_cacheWindowCaptionHeight == null)
			{
				_cacheWindowCaptionHeight = CreateInstance(SystemResourceKeyID.WindowCaptionHeight);
			}
			return _cacheWindowCaptionHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.KanjiWindowHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey KanjiWindowHeightKey
	{
		get
		{
			if (_cacheKanjiWindowHeight == null)
			{
				_cacheKanjiWindowHeight = CreateInstance(SystemResourceKeyID.KanjiWindowHeight);
			}
			return _cacheKanjiWindowHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.MenuBarHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MenuBarHeightKey
	{
		get
		{
			if (_cacheMenuBarHeight == null)
			{
				_cacheMenuBarHeight = CreateInstance(SystemResourceKeyID.MenuBarHeight);
			}
			return _cacheMenuBarHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.SmallCaptionHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey SmallCaptionHeightKey
	{
		get
		{
			if (_cacheSmallCaptionHeight == null)
			{
				_cacheSmallCaptionHeight = CreateInstance(SystemResourceKeyID.SmallCaptionHeight);
			}
			return _cacheSmallCaptionHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.VerticalScrollBarThumbHeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey VerticalScrollBarThumbHeightKey
	{
		get
		{
			if (_cacheVerticalScrollBarThumbHeight == null)
			{
				_cacheVerticalScrollBarThumbHeight = CreateInstance(SystemResourceKeyID.VerticalScrollBarThumbHeight);
			}
			return _cacheVerticalScrollBarThumbHeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.IsImmEnabled" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IsImmEnabledKey
	{
		get
		{
			if (_cacheIsImmEnabled == null)
			{
				_cacheIsImmEnabled = CreateInstance(SystemResourceKeyID.IsImmEnabled);
			}
			return _cacheIsImmEnabled;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.IsMediaCenter" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IsMediaCenterKey
	{
		get
		{
			if (_cacheIsMediaCenter == null)
			{
				_cacheIsMediaCenter = CreateInstance(SystemResourceKeyID.IsMediaCenter);
			}
			return _cacheIsMediaCenter;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.IsMenuDropRightAligned" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IsMenuDropRightAlignedKey
	{
		get
		{
			if (_cacheIsMenuDropRightAligned == null)
			{
				_cacheIsMenuDropRightAligned = CreateInstance(SystemResourceKeyID.IsMenuDropRightAligned);
			}
			return _cacheIsMenuDropRightAligned;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.IsMiddleEastEnabled" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IsMiddleEastEnabledKey
	{
		get
		{
			if (_cacheIsMiddleEastEnabled == null)
			{
				_cacheIsMiddleEastEnabled = CreateInstance(SystemResourceKeyID.IsMiddleEastEnabled);
			}
			return _cacheIsMiddleEastEnabled;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.IsMousePresent" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IsMousePresentKey
	{
		get
		{
			if (_cacheIsMousePresent == null)
			{
				_cacheIsMousePresent = CreateInstance(SystemResourceKeyID.IsMousePresent);
			}
			return _cacheIsMousePresent;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.IsMouseWheelPresent" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IsMouseWheelPresentKey
	{
		get
		{
			if (_cacheIsMouseWheelPresent == null)
			{
				_cacheIsMouseWheelPresent = CreateInstance(SystemResourceKeyID.IsMouseWheelPresent);
			}
			return _cacheIsMouseWheelPresent;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.IsPenWindows" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IsPenWindowsKey
	{
		get
		{
			if (_cacheIsPenWindows == null)
			{
				_cacheIsPenWindows = CreateInstance(SystemResourceKeyID.IsPenWindows);
			}
			return _cacheIsPenWindows;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.IsRemotelyControlled" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IsRemotelyControlledKey
	{
		get
		{
			if (_cacheIsRemotelyControlled == null)
			{
				_cacheIsRemotelyControlled = CreateInstance(SystemResourceKeyID.IsRemotelyControlled);
			}
			return _cacheIsRemotelyControlled;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.IsRemoteSession" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IsRemoteSessionKey
	{
		get
		{
			if (_cacheIsRemoteSession == null)
			{
				_cacheIsRemoteSession = CreateInstance(SystemResourceKeyID.IsRemoteSession);
			}
			return _cacheIsRemoteSession;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.ShowSounds" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey ShowSoundsKey
	{
		get
		{
			if (_cacheShowSounds == null)
			{
				_cacheShowSounds = CreateInstance(SystemResourceKeyID.ShowSounds);
			}
			return _cacheShowSounds;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.IsSlowMachine" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IsSlowMachineKey
	{
		get
		{
			if (_cacheIsSlowMachine == null)
			{
				_cacheIsSlowMachine = CreateInstance(SystemResourceKeyID.IsSlowMachine);
			}
			return _cacheIsSlowMachine;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.SwapButtons" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey SwapButtonsKey
	{
		get
		{
			if (_cacheSwapButtons == null)
			{
				_cacheSwapButtons = CreateInstance(SystemResourceKeyID.SwapButtons);
			}
			return _cacheSwapButtons;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.IsTabletPC" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IsTabletPCKey
	{
		get
		{
			if (_cacheIsTabletPC == null)
			{
				_cacheIsTabletPC = CreateInstance(SystemResourceKeyID.IsTabletPC);
			}
			return _cacheIsTabletPC;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.VirtualScreenLeft" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey VirtualScreenLeftKey
	{
		get
		{
			if (_cacheVirtualScreenLeft == null)
			{
				_cacheVirtualScreenLeft = CreateInstance(SystemResourceKeyID.VirtualScreenLeft);
			}
			return _cacheVirtualScreenLeft;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.VirtualScreenTop" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey VirtualScreenTopKey
	{
		get
		{
			if (_cacheVirtualScreenTop == null)
			{
				_cacheVirtualScreenTop = CreateInstance(SystemResourceKeyID.VirtualScreenTop);
			}
			return _cacheVirtualScreenTop;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the FocusVisualStyle property. </summary>
	/// <returns>The resource key.</returns>
	public static ResourceKey FocusVisualStyleKey
	{
		get
		{
			if (_cacheFocusVisualStyle == null)
			{
				_cacheFocusVisualStyle = new SystemThemeKey(SystemResourceKeyID.FocusVisualStyle);
			}
			return _cacheFocusVisualStyle;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.NavigationChromeStyleKey" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey NavigationChromeStyleKey
	{
		get
		{
			if (_cacheNavigationChromeStyle == null)
			{
				_cacheNavigationChromeStyle = new SystemThemeKey(SystemResourceKeyID.NavigationChromeStyle);
			}
			return _cacheNavigationChromeStyle;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.NavigationChromeDownLevelStyleKey" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey NavigationChromeDownLevelStyleKey
	{
		get
		{
			if (_cacheNavigationChromeDownLevelStyle == null)
			{
				_cacheNavigationChromeDownLevelStyle = new SystemThemeKey(SystemResourceKeyID.NavigationChromeDownLevelStyle);
			}
			return _cacheNavigationChromeDownLevelStyle;
		}
	}

	/// <summary>Gets a value indicating whether the system power is online, or that the system power status is unknown.</summary>
	/// <returns>A value in the enumeration.</returns>
	public static PowerLineStatus PowerLineStatus
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[110])
				{
					_cacheValid[110] = true;
					MS.Win32.NativeMethods.SYSTEM_POWER_STATUS systemPowerStatus = default(MS.Win32.NativeMethods.SYSTEM_POWER_STATUS);
					if (MS.Win32.UnsafeNativeMethods.GetSystemPowerStatus(ref systemPowerStatus))
					{
						_powerLineStatus = (PowerLineStatus)systemPowerStatus.ACLineStatus;
						continue;
					}
					_cacheValid[110] = false;
					throw new Win32Exception();
				}
			}
			return _powerLineStatus;
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemParameters.PowerLineStatus" /> property.</summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey PowerLineStatusKey
	{
		get
		{
			if (_cachePowerLineStatus == null)
			{
				_cachePowerLineStatus = CreateInstance(SystemResourceKeyID.PowerLineStatus);
			}
			return _cachePowerLineStatus;
		}
	}

	/// <summary>Gets a value that indicates whether glass window frames are being used.</summary>
	/// <returns>true if glass window frames are being used; otherwise, false.</returns>
	public static bool IsGlassEnabled
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[111])
				{
					_cacheValid[111] = true;
					_isGlassEnabled = NativeMethods.DwmIsCompositionEnabled();
				}
			}
			return _isGlassEnabled;
		}
	}

	/// <summary>Gets the theme name.</summary>
	/// <returns>The theme name.</returns>
	public static string UxThemeName
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[112])
				{
					_cacheValid[112] = true;
					if (!NativeMethods.IsThemeActive())
					{
						_uxThemeName = "Classic";
						continue;
					}
					NativeMethods.GetCurrentThemeName(out var themeFileName, out var _, out var _);
					_uxThemeName = Path.GetFileNameWithoutExtension(themeFileName);
				}
			}
			return _uxThemeName;
		}
	}

	/// <summary>Gets the color theme name.</summary>
	/// <returns>The color theme name.</returns>
	public static string UxThemeColor
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[113])
				{
					_cacheValid[113] = true;
					if (!NativeMethods.IsThemeActive())
					{
						_uxThemeColor = "";
						continue;
					}
					NativeMethods.GetCurrentThemeName(out var _, out var color, out var _);
					_uxThemeColor = color;
				}
			}
			return _uxThemeColor;
		}
	}

	/// <summary>Gets the radius of the corners for a window.</summary>
	/// <returns>The degree to which the corners of a window are rounded.</returns>
	public static CornerRadius WindowCornerRadius
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[114])
				{
					_cacheValid[114] = true;
					CornerRadius cornerRadius = default(CornerRadius);
					_windowCornerRadius = UxThemeName.ToUpperInvariant() switch
					{
						"LUNA" => new CornerRadius(6.0, 6.0, 0.0, 0.0), 
						"AERO" => (!NativeMethods.DwmIsCompositionEnabled()) ? new CornerRadius(6.0, 6.0, 0.0, 0.0) : new CornerRadius(8.0), 
						_ => new CornerRadius(0.0), 
					};
				}
			}
			return _windowCornerRadius;
		}
	}

	/// <summary>Gets the color that is used to paint the glass window frame.</summary>
	/// <returns>The color that is used to paint the glass window frame.</returns>
	public static Color WindowGlassColor
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[115])
				{
					_cacheValid[115] = true;
					NativeMethods.DwmGetColorizationColor(out var pcrColorization, out var pfOpaqueBlend);
					pcrColorization |= (uint)(pfOpaqueBlend ? (-16777216) : 0);
					_windowGlassColor = Utility.ColorFromArgbDword(pcrColorization);
				}
			}
			return _windowGlassColor;
		}
	}

	/// <summary>Gets the brush that paints the glass window frame.</summary>
	/// <returns>The brush that paints the glass window frame.</returns>
	public static Brush WindowGlassBrush
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[116])
				{
					_cacheValid[116] = true;
					SolidColorBrush solidColorBrush = new SolidColorBrush(WindowGlassColor);
					solidColorBrush.Freeze();
					_windowGlassBrush = solidColorBrush;
				}
			}
			return _windowGlassBrush;
		}
	}

	/// <summary>Gets the size of the resizing border around the window.</summary>
	/// <returns>The size of the resizing border around the window, in device-independent units (1/96th of an inch).</returns>
	public static Thickness WindowResizeBorderThickness
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[118])
				{
					_cacheValid[118] = true;
					Size size = DpiHelper.DeviceSizeToLogical(new Size(NativeMethods.GetSystemMetrics(Standard.SM.CXFRAME), NativeMethods.GetSystemMetrics(Standard.SM.CYFRAME)), (double)DpiX / 96.0, (double)Dpi / 96.0);
					_windowResizeBorderThickness = new Thickness(size.Width, size.Height, size.Width, size.Height);
				}
			}
			return _windowResizeBorderThickness;
		}
	}

	/// <summary>Gets the size of the non-client area of the window.</summary>
	/// <returns>The size of the non-client area of the window, in device-independent units (1/96th of an inch).</returns>
	public static Thickness WindowNonClientFrameThickness
	{
		get
		{
			lock (_cacheValid)
			{
				while (!_cacheValid[117])
				{
					_cacheValid[117] = true;
					Size size = DpiHelper.DeviceSizeToLogical(new Size(NativeMethods.GetSystemMetrics(Standard.SM.CXFRAME), NativeMethods.GetSystemMetrics(Standard.SM.CYFRAME)), (double)DpiX / 96.0, (double)Dpi / 96.0);
					int systemMetrics = NativeMethods.GetSystemMetrics(Standard.SM.CYCAPTION);
					double y = DpiHelper.DevicePixelsToLogical(new Point(0.0, systemMetrics), (double)DpiX / 96.0, (double)Dpi / 96.0).Y;
					_windowNonClientFrameThickness = new Thickness(size.Width, size.Height + y, size.Width, size.Height);
				}
			}
			return _windowNonClientFrameThickness;
		}
	}

	internal static int Dpi => Util.Dpi;

	internal static int DpiX
	{
		get
		{
			if (_setDpiX)
			{
				lock (_cacheValid)
				{
					if (_setDpiX)
					{
						_setDpiX = false;
						HandleRef hWnd = new HandleRef(null, IntPtr.Zero);
						nint dC = MS.Win32.UnsafeNativeMethods.GetDC(hWnd);
						if (dC == IntPtr.Zero)
						{
							throw new Win32Exception();
						}
						try
						{
							_dpiX = MS.Win32.UnsafeNativeMethods.GetDeviceCaps(new HandleRef(null, dC), 88);
							_cacheValid[0] = true;
						}
						finally
						{
							MS.Win32.UnsafeNativeMethods.ReleaseDC(hWnd, new HandleRef(null, dC));
						}
					}
				}
			}
			return _dpiX;
		}
	}

	/// <summary>Occurs when one of the properties changes.</summary>
	public static event PropertyChangedEventHandler StaticPropertyChanged;

	private static void OnPropertiesChanged(params string[] propertyNames)
	{
		PropertyChangedEventHandler staticPropertyChanged = SystemParameters.StaticPropertyChanged;
		if (staticPropertyChanged != null)
		{
			for (int i = 0; i < propertyNames.Length; i++)
			{
				staticPropertyChanged(null, new PropertyChangedEventArgs(propertyNames[i]));
			}
		}
	}

	private static bool InvalidateProperty(int slot, string name)
	{
		if (!SystemResources.ClearSlot(_cacheValid, slot))
		{
			return false;
		}
		OnPropertiesChanged(name);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static SystemResourceKey CreateInstance(SystemResourceKeyID KeyId)
	{
		return new SystemResourceKey(KeyId);
	}

	internal static void InvalidateCache()
	{
		int[] array = new int[43]
		{
			8207, 8209, 67, 4129, 4133, 4131, 47, 46, 4107, 23,
			69, 11, 96, 105, 103, 101, 99, 28, 4115, 107,
			4101, 4163, 4123, 4105, 4111, 4103, 4099, 4117, 4113, 4119,
			4121, 4159, 73, 8199, 8197, 37, 6, 42, 76, 77,
			49, 57, 33
		};
		for (int i = 0; i < array.Length; i++)
		{
			InvalidateCache(array[i]);
		}
	}

	internal static bool InvalidateDeviceDependentCache()
	{
		bool flag = false;
		if (SystemResources.ClearSlot(_cacheValid, 99))
		{
			flag |= _isMousePresent != IsMousePresent;
		}
		if (SystemResources.ClearSlot(_cacheValid, 100))
		{
			flag |= _isMouseWheelPresent != IsMouseWheelPresent;
		}
		if (flag)
		{
			OnPropertiesChanged("IsMousePresent", "IsMouseWheelPresent");
		}
		return flag;
	}

	internal static bool InvalidateDisplayDependentCache()
	{
		bool flag = false;
		if (SystemResources.ClearSlot(_cacheValid, 7))
		{
			MS.Win32.NativeMethods.RECT workAreaInternal = _workAreaInternal;
			MS.Win32.NativeMethods.RECT workAreaInternal2 = WorkAreaInternal;
			flag |= workAreaInternal.left != workAreaInternal2.left;
			flag |= workAreaInternal.top != workAreaInternal2.top;
			flag |= workAreaInternal.right != workAreaInternal2.right;
			flag |= workAreaInternal.bottom != workAreaInternal2.bottom;
		}
		if (SystemResources.ClearSlot(_cacheValid, 8))
		{
			flag |= _workArea != WorkArea;
		}
		if (SystemResources.ClearSlot(_cacheValid, 52))
		{
			flag |= _fullPrimaryScreenWidth != FullPrimaryScreenWidth;
		}
		if (SystemResources.ClearSlot(_cacheValid, 53))
		{
			flag |= _fullPrimaryScreenHeight != FullPrimaryScreenHeight;
		}
		if (SystemResources.ClearSlot(_cacheValid, 61))
		{
			flag |= _maximizedPrimaryScreenWidth != MaximizedPrimaryScreenWidth;
		}
		if (SystemResources.ClearSlot(_cacheValid, 62))
		{
			flag |= _maximizedPrimaryScreenHeight != MaximizedPrimaryScreenHeight;
		}
		if (SystemResources.ClearSlot(_cacheValid, 77))
		{
			flag |= _primaryScreenWidth != PrimaryScreenWidth;
		}
		if (SystemResources.ClearSlot(_cacheValid, 78))
		{
			flag |= _primaryScreenHeight != PrimaryScreenHeight;
		}
		if (SystemResources.ClearSlot(_cacheValid, 87))
		{
			flag |= _virtualScreenWidth != VirtualScreenWidth;
		}
		if (SystemResources.ClearSlot(_cacheValid, 88))
		{
			flag |= _virtualScreenHeight != VirtualScreenHeight;
		}
		if (SystemResources.ClearSlot(_cacheValid, 108))
		{
			flag |= _virtualScreenLeft != VirtualScreenLeft;
		}
		if (SystemResources.ClearSlot(_cacheValid, 109))
		{
			flag |= _virtualScreenTop != VirtualScreenTop;
		}
		if (flag)
		{
			OnPropertiesChanged("WorkArea", "FullPrimaryScreenWidth", "FullPrimaryScreenHeight", "MaximizedPrimaryScreenWidth", "MaximizedPrimaryScreenHeight", "PrimaryScreenWidth", "PrimaryScreenHeight", "VirtualScreenWidth", "VirtualScreenHeight", "VirtualScreenLeft", "VirtualScreenTop");
		}
		return flag;
	}

	internal static bool InvalidatePowerDependentCache()
	{
		bool flag = false;
		if (SystemResources.ClearSlot(_cacheValid, 110))
		{
			flag |= _powerLineStatus != PowerLineStatus;
		}
		if (flag)
		{
			OnPropertiesChanged("PowerLineStatus");
		}
		return flag;
	}

	internal static bool InvalidateCache(int param)
	{
		switch (param)
		{
		case 8207:
		{
			bool flag3 = SystemResources.ClearSlot(_cacheValid, 1);
			if (SystemResources.ClearSlot(_cacheValid, 50))
			{
				flag3 |= _focusHorizontalBorderHeight != FocusHorizontalBorderHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 51))
			{
				flag3 |= _focusVerticalBorderWidth != FocusVerticalBorderWidth;
			}
			if (flag3)
			{
				OnPropertiesChanged("FocusBorderWidth", "FocusHorizontalBorderHeight", "FocusVerticalBorderWidth");
			}
			return flag3;
		}
		case 8209:
			return InvalidateProperty(2, "FocusBorderHeight");
		case 67:
			return InvalidateProperty(3, "HighContrast");
		case 4129:
			return InvalidateProperty(4, "MouseVanish");
		case 4133:
			return InvalidateProperty(5, "DropShadow");
		case 4131:
			return InvalidateProperty(6, "FlatMenu");
		case 47:
			return InvalidateDisplayDependentCache();
		case 46:
		{
			bool flag2 = SystemResources.ClearSlot(_cacheValid, 9);
			if (flag2)
			{
				SystemFonts.InvalidateIconMetrics();
			}
			if (SystemResources.ClearSlot(_cacheValid, 57))
			{
				flag2 |= _iconWidth != IconWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 58))
			{
				flag2 |= _iconHeight != IconHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 59))
			{
				flag2 |= _iconGridWidth != IconGridWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 60))
			{
				flag2 |= _iconGridHeight != IconGridHeight;
			}
			if (flag2)
			{
				OnPropertiesChanged("IconMetrics", "IconWidth", "IconHeight", "IconGridWidth", "IconGridHeight");
			}
			return flag2;
		}
		case 4107:
			return InvalidateProperty(10, "KeyboardCues");
		case 23:
			return InvalidateProperty(11, "KeyboardDelay");
		case 69:
			return InvalidateProperty(12, "KeyboardPreference");
		case 11:
			return InvalidateProperty(13, "KeyboardSpeed");
		case 96:
			return InvalidateProperty(14, "SnapToDefaultButton");
		case 105:
			return InvalidateProperty(15, "WheelScrollLines");
		case 103:
			return InvalidateProperty(16, "MouseHoverTime");
		case 101:
			return InvalidateProperty(17, "MouseHoverHeight");
		case 99:
			return InvalidateProperty(18, "MouseHoverWidth");
		case 28:
		{
			bool flag4 = SystemResources.ClearSlot(_cacheValid, 19);
			if (SystemResources.ClearSlot(_cacheValid, 97))
			{
				flag4 |= _isMenuDropRightAligned != IsMenuDropRightAligned;
			}
			if (flag4)
			{
				OnPropertiesChanged("MenuDropAlignment", "IsMenuDropRightAligned");
			}
			return flag4;
		}
		case 4115:
			return InvalidateProperty(20, "MenuFade");
		case 107:
			return InvalidateProperty(21, "MenuShowDelay");
		case 4101:
			return InvalidateProperty(22, "ComboBoxAnimation");
		case 4163:
			return InvalidateProperty(23, "ClientAreaAnimation");
		case 4123:
			return InvalidateProperty(24, "CursorShadow");
		case 4105:
			return InvalidateProperty(25, "GradientCaptions");
		case 4111:
			return InvalidateProperty(26, "HotTracking");
		case 4103:
			return InvalidateProperty(27, "ListBoxSmoothScrolling");
		case 4099:
			return InvalidateProperty(28, "MenuAnimation");
		case 4117:
			return InvalidateProperty(29, "SelectionFade");
		case 4113:
			return InvalidateProperty(30, "StylusHotTracking");
		case 4119:
			return InvalidateProperty(31, "ToolTipAnimation");
		case 4121:
			return InvalidateProperty(32, "ToolTipFade");
		case 4159:
			return InvalidateProperty(33, "UIEffects");
		case 73:
			return InvalidateProperty(34, "MinimizeAnimation");
		case 8199:
			return InvalidateProperty(36, "CaretWidth");
		case 8197:
			return InvalidateProperty(37, "ForegroundFlashCount");
		case 37:
			return InvalidateProperty(38, "DragFullWindows");
		case 6:
		case 42:
		{
			bool flag = SystemResources.ClearSlot(_cacheValid, 39);
			if (flag)
			{
				SystemFonts.InvalidateNonClientMetrics();
			}
			flag |= SystemResources.ClearSlot(_cacheValid, 35);
			if (SystemResources.ClearSlot(_cacheValid, 40))
			{
				flag |= _thinHorizontalBorderHeight != ThinHorizontalBorderHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 41))
			{
				flag |= _thinVerticalBorderWidth != ThinVerticalBorderWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 42))
			{
				flag |= _cursorWidth != CursorWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 43))
			{
				flag |= _cursorHeight != CursorHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 44))
			{
				flag |= _thickHorizontalBorderHeight != ThickHorizontalBorderHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 45))
			{
				flag |= _thickVerticalBorderWidth != ThickVerticalBorderWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 48))
			{
				flag |= _fixedFrameHorizontalBorderHeight != FixedFrameHorizontalBorderHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 49))
			{
				flag |= _fixedFrameVerticalBorderWidth != FixedFrameVerticalBorderWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 54))
			{
				flag |= _horizontalScrollBarButtonWidth != HorizontalScrollBarButtonWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 55))
			{
				flag |= _horizontalScrollBarHeight != HorizontalScrollBarHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 56))
			{
				flag |= _horizontalScrollBarThumbWidth != HorizontalScrollBarThumbWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 57))
			{
				flag |= _iconWidth != IconWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 58))
			{
				flag |= _iconHeight != IconHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 59))
			{
				flag |= _iconGridWidth != IconGridWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 60))
			{
				flag |= _iconGridHeight != IconGridHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 63))
			{
				flag |= _maximumWindowTrackWidth != MaximumWindowTrackWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 64))
			{
				flag |= _maximumWindowTrackHeight != MaximumWindowTrackHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 65))
			{
				flag |= _menuCheckmarkWidth != MenuCheckmarkWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 66))
			{
				flag |= _menuCheckmarkHeight != MenuCheckmarkHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 67))
			{
				flag |= _menuButtonWidth != MenuButtonWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 68))
			{
				flag |= _menuButtonHeight != MenuButtonHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 69))
			{
				flag |= _minimumWindowWidth != MinimumWindowWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 70))
			{
				flag |= _minimumWindowHeight != MinimumWindowHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 71))
			{
				flag |= _minimizedWindowWidth != MinimizedWindowWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 72))
			{
				flag |= _minimizedWindowHeight != MinimizedWindowHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 73))
			{
				flag |= _minimizedGridWidth != MinimizedGridWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 74))
			{
				flag |= _minimizedGridHeight != MinimizedGridHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 75))
			{
				flag |= _minimumWindowTrackWidth != MinimumWindowTrackWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 76))
			{
				flag |= _minimumWindowTrackHeight != MinimumWindowTrackHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 79))
			{
				flag |= _windowCaptionButtonWidth != WindowCaptionButtonWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 80))
			{
				flag |= _windowCaptionButtonHeight != WindowCaptionButtonHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 81))
			{
				flag |= _resizeFrameHorizontalBorderHeight != ResizeFrameHorizontalBorderHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 82))
			{
				flag |= _resizeFrameVerticalBorderWidth != ResizeFrameVerticalBorderWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 83))
			{
				flag |= _smallIconWidth != SmallIconWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 84))
			{
				flag |= _smallIconHeight != SmallIconHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 85))
			{
				flag |= _smallWindowCaptionButtonWidth != SmallWindowCaptionButtonWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 86))
			{
				flag |= _smallWindowCaptionButtonHeight != SmallWindowCaptionButtonHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 89))
			{
				flag |= _verticalScrollBarWidth != VerticalScrollBarWidth;
			}
			if (SystemResources.ClearSlot(_cacheValid, 90))
			{
				flag |= _verticalScrollBarButtonHeight != VerticalScrollBarButtonHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 91))
			{
				flag |= _windowCaptionHeight != WindowCaptionHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 93))
			{
				flag |= _menuBarHeight != MenuBarHeight;
			}
			if (SystemResources.ClearSlot(_cacheValid, 94))
			{
				flag |= _verticalScrollBarThumbHeight != VerticalScrollBarThumbHeight;
			}
			if (flag)
			{
				OnPropertiesChanged("NonClientMetrics", "Border", "ThinHorizontalBorderHeight", "ThinVerticalBorderWidth", "CursorWidth", "CursorHeight", "ThickHorizontalBorderHeight", "ThickVerticalBorderWidth", "FixedFrameHorizontalBorderHeight", "FixedFrameVerticalBorderWidth", "HorizontalScrollBarButtonWidth", "HorizontalScrollBarHeight", "HorizontalScrollBarThumbWidth", "IconWidth", "IconHeight", "IconGridWidth", "IconGridHeight", "MaximumWindowTrackWidth", "MaximumWindowTrackHeight", "MenuCheckmarkWidth", "MenuCheckmarkHeight", "MenuButtonWidth", "MenuButtonHeight", "MinimumWindowWidth", "MinimumWindowHeight", "MinimizedWindowWidth", "MinimizedWindowHeight", "MinimizedGridWidth", "MinimizedGridHeight", "MinimumWindowTrackWidth", "MinimumWindowTrackHeight", "WindowCaptionButtonWidth", "WindowCaptionButtonHeight", "ResizeFrameHorizontalBorderHeight", "ResizeFrameVerticalBorderWidth", "SmallIconWidth", "SmallIconHeight", "SmallWindowCaptionButtonWidth", "SmallWindowCaptionButtonHeight", "VerticalScrollBarWidth", "VerticalScrollBarButtonHeight", "MenuBarHeight", "VerticalScrollBarThumbHeight");
			}
			return flag | InvalidateDisplayDependentCache();
		}
		case 76:
			return InvalidateProperty(46, "MinimumHorizontalDragDistance");
		case 77:
			return InvalidateProperty(47, "MinimumVerticalDragDistance");
		case 49:
			return InvalidateProperty(101, "IsPenWindows");
		case 57:
			return InvalidateProperty(104, "ShowSounds");
		case 33:
			return InvalidateProperty(106, "SwapButtons");
		default:
			return false;
		}
	}

	internal static bool InvalidateIsGlassEnabled()
	{
		return InvalidateProperty(111, "IsGlassEnabled");
	}

	internal static void InvalidateDerivedThemeRelatedProperties()
	{
		InvalidateProperty(112, "UxThemeName");
		InvalidateProperty(113, "UxThemeColor");
		InvalidateProperty(114, "WindowCornerRadius");
	}

	internal static void InvalidateWindowGlassColorizationProperties()
	{
		InvalidateProperty(115, "WindowGlassColor");
		InvalidateProperty(116, "WindowGlassBrush");
	}

	internal static void InvalidateWindowFrameThicknessProperties()
	{
		InvalidateProperty(117, "WindowNonClientFrameThickness");
		InvalidateProperty(118, "WindowResizeBorderThickness");
	}

	internal static double ConvertPixel(int pixel)
	{
		int dpi = Dpi;
		if (dpi != 0)
		{
			return (double)pixel * 96.0 / (double)dpi;
		}
		return pixel;
	}
}
