using System.Collections;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using MS.Win32;

namespace System.Windows;

/// <summary>Contains system colors, system brushes, and system resource keys that correspond to system display elements. </summary>
public static class SystemColors
{
	private enum CacheSlot
	{
		ActiveBorder,
		ActiveCaption,
		ActiveCaptionText,
		AppWorkspace,
		Control,
		ControlDark,
		ControlDarkDark,
		ControlLight,
		ControlLightLight,
		ControlText,
		Desktop,
		GradientActiveCaption,
		GradientInactiveCaption,
		GrayText,
		Highlight,
		HighlightText,
		HotTrack,
		InactiveBorder,
		InactiveCaption,
		InactiveCaptionText,
		Info,
		InfoText,
		Menu,
		MenuBar,
		MenuHighlight,
		MenuText,
		ScrollBar,
		Window,
		WindowFrame,
		WindowText,
		NumSlots
	}

	private const int AlphaShift = 24;

	private const int RedShift = 16;

	private const int GreenShift = 8;

	private const int BlueShift = 0;

	private const int Win32RedShift = 0;

	private const int Win32GreenShift = 8;

	private const int Win32BlueShift = 16;

	private static BitArray _colorCacheValid = new BitArray(30);

	private static Color[] _colorCache = new Color[30];

	private static BitArray _brushCacheValid = new BitArray(30);

	private static SolidColorBrush[] _brushCache = new SolidColorBrush[30];

	private static SystemResourceKey _cacheActiveBorderBrush;

	private static SystemResourceKey _cacheActiveCaptionBrush;

	private static SystemResourceKey _cacheActiveCaptionTextBrush;

	private static SystemResourceKey _cacheAppWorkspaceBrush;

	private static SystemResourceKey _cacheControlBrush;

	private static SystemResourceKey _cacheControlDarkBrush;

	private static SystemResourceKey _cacheControlDarkDarkBrush;

	private static SystemResourceKey _cacheControlLightBrush;

	private static SystemResourceKey _cacheControlLightLightBrush;

	private static SystemResourceKey _cacheControlTextBrush;

	private static SystemResourceKey _cacheDesktopBrush;

	private static SystemResourceKey _cacheGradientActiveCaptionBrush;

	private static SystemResourceKey _cacheGradientInactiveCaptionBrush;

	private static SystemResourceKey _cacheGrayTextBrush;

	private static SystemResourceKey _cacheHighlightBrush;

	private static SystemResourceKey _cacheHighlightTextBrush;

	private static SystemResourceKey _cacheHotTrackBrush;

	private static SystemResourceKey _cacheInactiveBorderBrush;

	private static SystemResourceKey _cacheInactiveCaptionBrush;

	private static SystemResourceKey _cacheInactiveCaptionTextBrush;

	private static SystemResourceKey _cacheInfoBrush;

	private static SystemResourceKey _cacheInfoTextBrush;

	private static SystemResourceKey _cacheMenuBrush;

	private static SystemResourceKey _cacheMenuBarBrush;

	private static SystemResourceKey _cacheMenuHighlightBrush;

	private static SystemResourceKey _cacheMenuTextBrush;

	private static SystemResourceKey _cacheScrollBarBrush;

	private static SystemResourceKey _cacheWindowBrush;

	private static SystemResourceKey _cacheWindowFrameBrush;

	private static SystemResourceKey _cacheWindowTextBrush;

	private static SystemResourceKey _cacheInactiveSelectionHighlightBrush;

	private static SystemResourceKey _cacheInactiveSelectionHighlightTextBrush;

	private static SystemResourceKey _cacheActiveBorderColor;

	private static SystemResourceKey _cacheActiveCaptionColor;

	private static SystemResourceKey _cacheActiveCaptionTextColor;

	private static SystemResourceKey _cacheAppWorkspaceColor;

	private static SystemResourceKey _cacheControlColor;

	private static SystemResourceKey _cacheControlDarkColor;

	private static SystemResourceKey _cacheControlDarkDarkColor;

	private static SystemResourceKey _cacheControlLightColor;

	private static SystemResourceKey _cacheControlLightLightColor;

	private static SystemResourceKey _cacheControlTextColor;

	private static SystemResourceKey _cacheDesktopColor;

	private static SystemResourceKey _cacheGradientActiveCaptionColor;

	private static SystemResourceKey _cacheGradientInactiveCaptionColor;

	private static SystemResourceKey _cacheGrayTextColor;

	private static SystemResourceKey _cacheHighlightColor;

	private static SystemResourceKey _cacheHighlightTextColor;

	private static SystemResourceKey _cacheHotTrackColor;

	private static SystemResourceKey _cacheInactiveBorderColor;

	private static SystemResourceKey _cacheInactiveCaptionColor;

	private static SystemResourceKey _cacheInactiveCaptionTextColor;

	private static SystemResourceKey _cacheInfoColor;

	private static SystemResourceKey _cacheInfoTextColor;

	private static SystemResourceKey _cacheMenuColor;

	private static SystemResourceKey _cacheMenuBarColor;

	private static SystemResourceKey _cacheMenuHighlightColor;

	private static SystemResourceKey _cacheMenuTextColor;

	private static SystemResourceKey _cacheScrollBarColor;

	private static SystemResourceKey _cacheWindowColor;

	private static SystemResourceKey _cacheWindowFrameColor;

	private static SystemResourceKey _cacheWindowTextColor;

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the color of the active window's border. </summary>
	/// <returns>The color of the active window's border.</returns>
	public static Color ActiveBorderColor => GetSystemColor(CacheSlot.ActiveBorder);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the background color of the active window's title bar. </summary>
	/// <returns>The background color of the active window's title bar.</returns>
	public static Color ActiveCaptionColor => GetSystemColor(CacheSlot.ActiveCaption);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the color of the text in the active window's title bar. </summary>
	/// <returns>The color of the active window's title bar.</returns>
	public static Color ActiveCaptionTextColor => GetSystemColor(CacheSlot.ActiveCaptionText);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the color of the application workspace. </summary>
	/// <returns>The color of the application workspace.</returns>
	public static Color AppWorkspaceColor => GetSystemColor(CacheSlot.AppWorkspace);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the face color of a three-dimensional display element. </summary>
	/// <returns>The face color of a three-dimensional display element.</returns>
	public static Color ControlColor => GetSystemColor(CacheSlot.Control);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the shadow color of a three-dimensional display element. </summary>
	/// <returns>The shadow color of a three-dimensional display element.</returns>
	public static Color ControlDarkColor => GetSystemColor(CacheSlot.ControlDark);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the dark shadow color of a three-dimensional display element. </summary>
	/// <returns>The dark shadow color of a three-dimensional display element.</returns>
	public static Color ControlDarkDarkColor => GetSystemColor(CacheSlot.ControlDarkDark);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the light color of a three-dimensional display element. </summary>
	/// <returns>The light color of a three-dimensional display element.</returns>
	public static Color ControlLightColor => GetSystemColor(CacheSlot.ControlLight);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the highlight color of a three-dimensional display element. </summary>
	/// <returns>The highlight color of a three-dimensional display element.</returns>
	public static Color ControlLightLightColor => GetSystemColor(CacheSlot.ControlLightLight);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the color of text in a three-dimensional display element. </summary>
	/// <returns>The color of text in a three-dimensional display element.</returns>
	public static Color ControlTextColor => GetSystemColor(CacheSlot.ControlText);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the color of the desktop. </summary>
	/// <returns>The color of the desktop.</returns>
	public static Color DesktopColor => GetSystemColor(CacheSlot.Desktop);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the right side color in the gradient of an active window's title bar. </summary>
	/// <returns>The right side color in the gradient.</returns>
	public static Color GradientActiveCaptionColor => GetSystemColor(CacheSlot.GradientActiveCaption);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the right side color in the gradient of an inactive window's title bar. </summary>
	/// <returns>The right side color in the gradient.</returns>
	public static Color GradientInactiveCaptionColor => GetSystemColor(CacheSlot.GradientInactiveCaption);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the color of disabled text. </summary>
	/// <returns>The color of disabled text.</returns>
	public static Color GrayTextColor => GetSystemColor(CacheSlot.GrayText);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the background color of selected items. </summary>
	/// <returns>The background color of selected items.</returns>
	public static Color HighlightColor => GetSystemColor(CacheSlot.Highlight);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the color of the text of selected items. </summary>
	/// <returns>The color of the text of selected items.</returns>
	public static Color HighlightTextColor => GetSystemColor(CacheSlot.HighlightText);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the color used to designate a hot-tracked item. </summary>
	/// <returns>The color used to designate a hot-tracked item.</returns>
	public static Color HotTrackColor => GetSystemColor(CacheSlot.HotTrack);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the color of an inactive window's border. </summary>
	/// <returns>The color of an inactive window's border.</returns>
	public static Color InactiveBorderColor => GetSystemColor(CacheSlot.InactiveBorder);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the background color of an inactive window's title bar. </summary>
	/// <returns>The background color of an inactive window's title bar.</returns>
	public static Color InactiveCaptionColor => GetSystemColor(CacheSlot.InactiveCaption);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the color of the text of an inactive window's title bar. </summary>
	/// <returns>The color of the text of an inactive window's title bar.</returns>
	public static Color InactiveCaptionTextColor => GetSystemColor(CacheSlot.InactiveCaptionText);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the background color for the <see cref="T:System.Windows.Controls.ToolTip" /> control. </summary>
	/// <returns>The background color for the <see cref="T:System.Windows.Controls.ToolTip" /> control.</returns>
	public static Color InfoColor => GetSystemColor(CacheSlot.Info);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the text color for the <see cref="T:System.Windows.Controls.ToolTip" /> control. </summary>
	/// <returns>The text color for the <see cref="T:System.Windows.Controls.ToolTip" /> control.</returns>
	public static Color InfoTextColor => GetSystemColor(CacheSlot.InfoText);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the color of a menu's background. </summary>
	/// <returns>The color of a menu's background.</returns>
	public static Color MenuColor => GetSystemColor(CacheSlot.Menu);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the background color for a menu bar. </summary>
	/// <returns>The background color for a menu bar.</returns>
	public static Color MenuBarColor => GetSystemColor(CacheSlot.MenuBar);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the color used to highlight a menu item. </summary>
	/// <returns>The color used to highlight a menu item.</returns>
	public static Color MenuHighlightColor => GetSystemColor(CacheSlot.MenuHighlight);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the color of a menu's text. </summary>
	/// <returns>The color of a menu's text.</returns>
	public static Color MenuTextColor => GetSystemColor(CacheSlot.MenuText);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the background color of a scroll bar. </summary>
	/// <returns>The background color of a scroll bar.</returns>
	public static Color ScrollBarColor => GetSystemColor(CacheSlot.ScrollBar);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the background color in the client area of a window. </summary>
	/// <returns>The background color in the client area of a window.</returns>
	public static Color WindowColor => GetSystemColor(CacheSlot.Window);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the color of a window frame. </summary>
	/// <returns>The color of a window frame.</returns>
	public static Color WindowFrameColor => GetSystemColor(CacheSlot.WindowFrame);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Color" /> structure that is the color of the text in the client area of a window. </summary>
	/// <returns>The color of the text in the client area of a window.</returns>
	public static Color WindowTextColor => GetSystemColor(CacheSlot.WindowText);

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.Color" /> of the active window's border. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.Color" /> of the active window's border.</returns>
	public static ResourceKey ActiveBorderColorKey
	{
		get
		{
			if (_cacheActiveBorderColor == null)
			{
				_cacheActiveBorderColor = CreateInstance(SystemResourceKeyID.ActiveBorderColor);
			}
			return _cacheActiveBorderColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the background <see cref="T:System.Windows.Media.Color" /> of the active window's title bar.</summary>
	/// <returns>The resource key for the background <see cref="T:System.Windows.Media.Color" /> of the active window's title bar.</returns>
	public static ResourceKey ActiveCaptionColorKey
	{
		get
		{
			if (_cacheActiveCaptionColor == null)
			{
				_cacheActiveCaptionColor = CreateInstance(SystemResourceKeyID.ActiveCaptionColor);
			}
			return _cacheActiveCaptionColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.Color" /> of the text in the active window's title bar. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.Color" /> of the text in the active window's title bar.</returns>
	public static ResourceKey ActiveCaptionTextColorKey
	{
		get
		{
			if (_cacheActiveCaptionTextColor == null)
			{
				_cacheActiveCaptionTextColor = CreateInstance(SystemResourceKeyID.ActiveCaptionTextColor);
			}
			return _cacheActiveCaptionTextColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.Color" /> of the application workspace. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.Color" /> of the application workspace.</returns>
	public static ResourceKey AppWorkspaceColorKey
	{
		get
		{
			if (_cacheAppWorkspaceColor == null)
			{
				_cacheAppWorkspaceColor = CreateInstance(SystemResourceKeyID.AppWorkspaceColor);
			}
			return _cacheAppWorkspaceColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the face <see cref="T:System.Windows.Media.Color" /> of a three-dimensional display element. </summary>
	/// <returns>The resource key for the face <see cref="T:System.Windows.Media.Color" /> of a three-dimensional display element.</returns>
	public static ResourceKey ControlColorKey
	{
		get
		{
			if (_cacheControlColor == null)
			{
				_cacheControlColor = CreateInstance(SystemResourceKeyID.ControlColor);
			}
			return _cacheControlColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the shadow <see cref="T:System.Windows.Media.Color" /> of a three-dimensional display element. </summary>
	/// <returns>The resource key for the shadow <see cref="T:System.Windows.Media.Color" /> of a three-dimensional display element.</returns>
	public static ResourceKey ControlDarkColorKey
	{
		get
		{
			if (_cacheControlDarkColor == null)
			{
				_cacheControlDarkColor = CreateInstance(SystemResourceKeyID.ControlDarkColor);
			}
			return _cacheControlDarkColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the dark shadow <see cref="T:System.Windows.Media.Color" /> of the highlight color of a three-dimensional display element. </summary>
	/// <returns>The resource key for the dark shadow <see cref="T:System.Windows.Media.Color" /> of a three-dimensional display element.</returns>
	public static ResourceKey ControlDarkDarkColorKey
	{
		get
		{
			if (_cacheControlDarkDarkColor == null)
			{
				_cacheControlDarkDarkColor = CreateInstance(SystemResourceKeyID.ControlDarkDarkColor);
			}
			return _cacheControlDarkDarkColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the highlight <see cref="T:System.Windows.Media.Color" /> of a three-dimensional display element. </summary>
	/// <returns>The resource key for the highlight <see cref="T:System.Windows.Media.Color" /> of a three-dimensional display element.</returns>
	public static ResourceKey ControlLightColorKey
	{
		get
		{
			if (_cacheControlLightColor == null)
			{
				_cacheControlLightColor = CreateInstance(SystemResourceKeyID.ControlLightColor);
			}
			return _cacheControlLightColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the highlight <see cref="T:System.Windows.Media.Color" /> of a three-dimensional display element. </summary>
	/// <returns>The resource key for the highlight <see cref="T:System.Windows.Media.Color" /> of a three-dimensional display element.</returns>
	public static ResourceKey ControlLightLightColorKey
	{
		get
		{
			if (_cacheControlLightLightColor == null)
			{
				_cacheControlLightLightColor = CreateInstance(SystemResourceKeyID.ControlLightLightColor);
			}
			return _cacheControlLightLightColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.Color" /> of text in a three-dimensional display element.</summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.Color" /> of text in a three-dimensional display element.</returns>
	public static ResourceKey ControlTextColorKey
	{
		get
		{
			if (_cacheControlTextColor == null)
			{
				_cacheControlTextColor = CreateInstance(SystemResourceKeyID.ControlTextColor);
			}
			return _cacheControlTextColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.Color" /> of the desktop. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.Color" /> of the desktop.</returns>
	public static ResourceKey DesktopColorKey
	{
		get
		{
			if (_cacheDesktopColor == null)
			{
				_cacheDesktopColor = CreateInstance(SystemResourceKeyID.DesktopColor);
			}
			return _cacheDesktopColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the right-side <see cref="T:System.Windows.Media.Color" /> in the gradient of an active window's title bar. </summary>
	/// <returns>The resource key for the right-side <see cref="T:System.Windows.Media.Color" /> in the gradient of an active window's title bar.</returns>
	public static ResourceKey GradientActiveCaptionColorKey
	{
		get
		{
			if (_cacheGradientActiveCaptionColor == null)
			{
				_cacheGradientActiveCaptionColor = CreateInstance(SystemResourceKeyID.GradientActiveCaptionColor);
			}
			return _cacheGradientActiveCaptionColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the right-side <see cref="T:System.Windows.Media.Color" /> in the gradient of an inactive window's title bar. </summary>
	/// <returns>The resource key for the right-side <see cref="T:System.Windows.Media.Color" /> in the gradient of an inactive window's title bar.</returns>
	public static ResourceKey GradientInactiveCaptionColorKey
	{
		get
		{
			if (_cacheGradientInactiveCaptionColor == null)
			{
				_cacheGradientInactiveCaptionColor = CreateInstance(SystemResourceKeyID.GradientInactiveCaptionColor);
			}
			return _cacheGradientInactiveCaptionColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.Color" /> of disabled text. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.Color" /> of disabled text.</returns>
	public static ResourceKey GrayTextColorKey
	{
		get
		{
			if (_cacheGrayTextColor == null)
			{
				_cacheGrayTextColor = CreateInstance(SystemResourceKeyID.GrayTextColor);
			}
			return _cacheGrayTextColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the background <see cref="T:System.Windows.Media.Color" /> of selected items. </summary>
	/// <returns>The resource key for the background <see cref="T:System.Windows.Media.Color" /> of selected items.</returns>
	public static ResourceKey HighlightColorKey
	{
		get
		{
			if (_cacheHighlightColor == null)
			{
				_cacheHighlightColor = CreateInstance(SystemResourceKeyID.HighlightColor);
			}
			return _cacheHighlightColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.Color" /> of a selected item's text. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.Color" /> of a selected item's text.</returns>
	public static ResourceKey HighlightTextColorKey
	{
		get
		{
			if (_cacheHighlightTextColor == null)
			{
				_cacheHighlightTextColor = CreateInstance(SystemResourceKeyID.HighlightTextColor);
			}
			return _cacheHighlightTextColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.Color" /> that designates a hot-tracked item. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.Color" /> that designates a hot-tracked item.</returns>
	public static ResourceKey HotTrackColorKey
	{
		get
		{
			if (_cacheHotTrackColor == null)
			{
				_cacheHotTrackColor = CreateInstance(SystemResourceKeyID.HotTrackColor);
			}
			return _cacheHotTrackColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.Color" /> of an inactive window's border.</summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.Color" /> of an inactive window's border.</returns>
	public static ResourceKey InactiveBorderColorKey
	{
		get
		{
			if (_cacheInactiveBorderColor == null)
			{
				_cacheInactiveBorderColor = CreateInstance(SystemResourceKeyID.InactiveBorderColor);
			}
			return _cacheInactiveBorderColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the background <see cref="T:System.Windows.Media.Color" /> of an inactive window's title bar. </summary>
	/// <returns>The resource key for the background <see cref="T:System.Windows.Media.Color" /> of an inactive window's title bar.</returns>
	public static ResourceKey InactiveCaptionColorKey
	{
		get
		{
			if (_cacheInactiveCaptionColor == null)
			{
				_cacheInactiveCaptionColor = CreateInstance(SystemResourceKeyID.InactiveCaptionColor);
			}
			return _cacheInactiveCaptionColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.Color" /> of the text of an inactive window's title bar.</summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.Color" /> of the text of an inactive window's title bar.</returns>
	public static ResourceKey InactiveCaptionTextColorKey
	{
		get
		{
			if (_cacheInactiveCaptionTextColor == null)
			{
				_cacheInactiveCaptionTextColor = CreateInstance(SystemResourceKeyID.InactiveCaptionTextColor);
			}
			return _cacheInactiveCaptionTextColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the background <see cref="T:System.Windows.Media.Color" /> of the <see cref="T:System.Windows.Controls.ToolTip" /> control.</summary>
	/// <returns>The resource key for the background <see cref="T:System.Windows.Media.Color" /> of the <see cref="T:System.Windows.Controls.ToolTip" /> control.</returns>
	public static ResourceKey InfoColorKey
	{
		get
		{
			if (_cacheInfoColor == null)
			{
				_cacheInfoColor = CreateInstance(SystemResourceKeyID.InfoColor);
			}
			return _cacheInfoColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.Color" /> of the text in a <see cref="T:System.Windows.Controls.ToolTip" /> control. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.Color" /> of the text in a <see cref="T:System.Windows.Controls.ToolTip" /> control.</returns>
	public static ResourceKey InfoTextColorKey
	{
		get
		{
			if (_cacheInfoTextColor == null)
			{
				_cacheInfoTextColor = CreateInstance(SystemResourceKeyID.InfoTextColor);
			}
			return _cacheInfoTextColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the background <see cref="T:System.Windows.Media.Color" /> of a menu. </summary>
	/// <returns>The resource key for the background <see cref="T:System.Windows.Media.Color" /> of a menu.</returns>
	public static ResourceKey MenuColorKey
	{
		get
		{
			if (_cacheMenuColor == null)
			{
				_cacheMenuColor = CreateInstance(SystemResourceKeyID.MenuColor);
			}
			return _cacheMenuColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the background <see cref="T:System.Windows.Media.Color" /> of a menu bar. </summary>
	/// <returns>The resource key for the background <see cref="T:System.Windows.Media.Color" /> of a menu bar.</returns>
	public static ResourceKey MenuBarColorKey
	{
		get
		{
			if (_cacheMenuBarColor == null)
			{
				_cacheMenuBarColor = CreateInstance(SystemResourceKeyID.MenuBarColor);
			}
			return _cacheMenuBarColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the background <see cref="T:System.Windows.Media.Color" /> of a highlighted menu item. </summary>
	/// <returns>The resource key for the background <see cref="T:System.Windows.Media.Color" /> of a highlighted menu item.</returns>
	public static ResourceKey MenuHighlightColorKey
	{
		get
		{
			if (_cacheMenuHighlightColor == null)
			{
				_cacheMenuHighlightColor = CreateInstance(SystemResourceKeyID.MenuHighlightColor);
			}
			return _cacheMenuHighlightColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.Color" /> of a menu's text. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.Color" /> of a menu's text.</returns>
	public static ResourceKey MenuTextColorKey
	{
		get
		{
			if (_cacheMenuTextColor == null)
			{
				_cacheMenuTextColor = CreateInstance(SystemResourceKeyID.MenuTextColor);
			}
			return _cacheMenuTextColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the background <see cref="T:System.Windows.Media.Color" /> of a scroll bar. </summary>
	/// <returns>The resource key for the background <see cref="T:System.Windows.Media.Color" /> of a scroll bar.</returns>
	public static ResourceKey ScrollBarColorKey
	{
		get
		{
			if (_cacheScrollBarColor == null)
			{
				_cacheScrollBarColor = CreateInstance(SystemResourceKeyID.ScrollBarColor);
			}
			return _cacheScrollBarColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the background <see cref="T:System.Windows.Media.Color" /> of a window's client area. </summary>
	/// <returns>The resource key for the background <see cref="T:System.Windows.Media.Color" /> of a window's client area.</returns>
	public static ResourceKey WindowColorKey
	{
		get
		{
			if (_cacheWindowColor == null)
			{
				_cacheWindowColor = CreateInstance(SystemResourceKeyID.WindowColor);
			}
			return _cacheWindowColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.Color" /> of a window frame. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.Color" /> of a window frame.</returns>
	public static ResourceKey WindowFrameColorKey
	{
		get
		{
			if (_cacheWindowFrameColor == null)
			{
				_cacheWindowFrameColor = CreateInstance(SystemResourceKeyID.WindowFrameColor);
			}
			return _cacheWindowFrameColor;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.Color" /> of text in a window's client area. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.Color" /> of text in a window's client area.</returns>
	public static ResourceKey WindowTextColorKey
	{
		get
		{
			if (_cacheWindowTextColor == null)
			{
				_cacheWindowTextColor = CreateInstance(SystemResourceKeyID.WindowTextColor);
			}
			return _cacheWindowTextColor;
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color of the active window's border. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.SolidColorBrush" /> with its <see cref="P:System.Windows.Media.SolidColorBrush.Color" /> set to the color of the active window's border. The returned brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static SolidColorBrush ActiveBorderBrush => MakeBrush(CacheSlot.ActiveBorder);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color of the background of the active window's title bar. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.SolidColorBrush" /> with its <see cref="P:System.Windows.Media.SolidColorBrush.Color" /> set to the background color of the active window's title bar. The returned brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static SolidColorBrush ActiveCaptionBrush => MakeBrush(CacheSlot.ActiveCaption);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color of the text in the active window's title bar. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.SolidColorBrush" /> with its <see cref="P:System.Windows.Media.SolidColorBrush.Color" /> set to the background color of the color of the text in the active window's title bar. The returned brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static SolidColorBrush ActiveCaptionTextBrush => MakeBrush(CacheSlot.ActiveCaptionText);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color of the application workspace. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.SolidColorBrush" /> with its <see cref="P:System.Windows.Media.SolidColorBrush.Color" /> set to the color of the application workspace. The returned brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static SolidColorBrush AppWorkspaceBrush => MakeBrush(CacheSlot.AppWorkspace);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the face color of a three-dimensional display element. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.SolidColorBrush" /> with its <see cref="P:System.Windows.Media.SolidColorBrush.Color" /> set to the face color of a three-dimensional display element. The returned brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static SolidColorBrush ControlBrush => MakeBrush(CacheSlot.Control);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the shadow color of a three-dimensional display element. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.SolidColorBrush" /> with its <see cref="P:System.Windows.Media.SolidColorBrush.Color" /> set to the shadow color of a three-dimensional display element. The returned brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static SolidColorBrush ControlDarkBrush => MakeBrush(CacheSlot.ControlDark);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the dark shadow color of a three-dimensional display element. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.SolidColorBrush" /> with its <see cref="P:System.Windows.Media.SolidColorBrush.Color" /> set to the dark shadow color of a three-dimensional display element. The returned brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static SolidColorBrush ControlDarkDarkBrush => MakeBrush(CacheSlot.ControlDarkDark);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the light color of a three-dimensional display element. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.SolidColorBrush" /> with its <see cref="P:System.Windows.Media.SolidColorBrush.Color" /> set to the light color of a three-dimensional display element. The returned brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static SolidColorBrush ControlLightBrush => MakeBrush(CacheSlot.ControlLight);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the highlight color of a three-dimensional display element. </summary>
	/// <returns>The highlight color of a three-dimensional display element.</returns>
	public static SolidColorBrush ControlLightLightBrush => MakeBrush(CacheSlot.ControlLightLight);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color of text in a three-dimensional display element. </summary>
	/// <returns>The color of text in a three-dimensional display element.</returns>
	public static SolidColorBrush ControlTextBrush => MakeBrush(CacheSlot.ControlText);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color of the desktop. </summary>
	/// <returns>The color of the desktop.</returns>
	public static SolidColorBrush DesktopBrush => MakeBrush(CacheSlot.Desktop);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the right side color in the gradient of an active window's title bar. </summary>
	/// <returns>The right side color in the gradient.</returns>
	public static SolidColorBrush GradientActiveCaptionBrush => MakeBrush(CacheSlot.GradientActiveCaption);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the right side color in the gradient of an inactive window's title bar. </summary>
	/// <returns>The right side color in the gradient.</returns>
	public static SolidColorBrush GradientInactiveCaptionBrush => MakeBrush(CacheSlot.GradientInactiveCaption);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color of disabled text. </summary>
	/// <returns>The color of disabled text.</returns>
	public static SolidColorBrush GrayTextBrush => MakeBrush(CacheSlot.GrayText);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the background of selected items. </summary>
	/// <returns>The background color of selected items.</returns>
	public static SolidColorBrush HighlightBrush => MakeBrush(CacheSlot.Highlight);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color of the text of selected items. </summary>
	/// <returns>The color of the text of selected items.</returns>
	public static SolidColorBrush HighlightTextBrush => MakeBrush(CacheSlot.HighlightText);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color used to designate a hot-tracked item. </summary>
	/// <returns>The color used to designate a hot-tracked item.</returns>
	public static SolidColorBrush HotTrackBrush => MakeBrush(CacheSlot.HotTrack);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color of an inactive window's border. </summary>
	/// <returns>The color of an inactive window's border.</returns>
	public static SolidColorBrush InactiveBorderBrush => MakeBrush(CacheSlot.InactiveBorder);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the background color of an inactive window's title bar. </summary>
	/// <returns>The background color of an inactive window's title bar.</returns>
	public static SolidColorBrush InactiveCaptionBrush => MakeBrush(CacheSlot.InactiveCaption);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color of the text of an inactive window's title bar. </summary>
	/// <returns>The color of the text of an inactive window's title bar.</returns>
	public static SolidColorBrush InactiveCaptionTextBrush => MakeBrush(CacheSlot.InactiveCaptionText);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the background color for the <see cref="T:System.Windows.Controls.ToolTip" /> control. </summary>
	/// <returns>The background color for the <see cref="T:System.Windows.Controls.ToolTip" /> control.</returns>
	public static SolidColorBrush InfoBrush => MakeBrush(CacheSlot.Info);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the text color for the <see cref="T:System.Windows.Controls.ToolTip" /> control. </summary>
	/// <returns>The text color for the <see cref="T:System.Windows.Controls.ToolTip" /> control.</returns>
	public static SolidColorBrush InfoTextBrush => MakeBrush(CacheSlot.InfoText);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color of a menu's background. </summary>
	/// <returns>The color of a menu's background.</returns>
	public static SolidColorBrush MenuBrush => MakeBrush(CacheSlot.Menu);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the background color for a menu bar. </summary>
	/// <returns>The background color for a menu bar.</returns>
	public static SolidColorBrush MenuBarBrush => MakeBrush(CacheSlot.MenuBar);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color used to highlight a menu item. </summary>
	/// <returns>The color used to highlight a menu item.</returns>
	public static SolidColorBrush MenuHighlightBrush => MakeBrush(CacheSlot.MenuHighlight);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color of a menu's text. </summary>
	/// <returns>The color of a menu's text.</returns>
	public static SolidColorBrush MenuTextBrush => MakeBrush(CacheSlot.MenuText);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the background color of a scroll bar. </summary>
	/// <returns>The background color of a scroll bar.</returns>
	public static SolidColorBrush ScrollBarBrush => MakeBrush(CacheSlot.ScrollBar);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the background color in the client area of a window. </summary>
	/// <returns>The background color in the client area of a window.</returns>
	public static SolidColorBrush WindowBrush => MakeBrush(CacheSlot.Window);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color of a window frame. </summary>
	/// <returns>The color of a window frame.</returns>
	public static SolidColorBrush WindowFrameBrush => MakeBrush(CacheSlot.WindowFrame);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color of the text in the client area of a window. </summary>
	/// <returns>The color of the text in the client area of a window.</returns>
	public static SolidColorBrush WindowTextBrush => MakeBrush(CacheSlot.WindowText);

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color used to highlight a selected item that is inactive.</summary>
	/// <returns>The color used to highlight an inactive selected item.</returns>
	public static SolidColorBrush InactiveSelectionHighlightBrush
	{
		get
		{
			if (SystemParameters.HighContrast)
			{
				return HighlightBrush;
			}
			return ControlBrush;
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color of an inactive selected item’s text.</summary>
	/// <returns>The color of an inactive selected item’s text.</returns>
	public static SolidColorBrush InactiveSelectionHighlightTextBrush
	{
		get
		{
			if (SystemParameters.HighContrast)
			{
				return HighlightTextBrush;
			}
			return ControlTextBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> used to paint the active window's border. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> used to paint the active window's border. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey ActiveBorderBrushKey
	{
		get
		{
			if (_cacheActiveBorderBrush == null)
			{
				_cacheActiveBorderBrush = CreateInstance(SystemResourceKeyID.ActiveBorderBrush);
			}
			return _cacheActiveBorderBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> used to paint the background of the active window's title bar.</summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> used to paint the background of the active window's title bar. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey ActiveCaptionBrushKey
	{
		get
		{
			if (_cacheActiveCaptionBrush == null)
			{
				_cacheActiveCaptionBrush = CreateInstance(SystemResourceKeyID.ActiveCaptionBrush);
			}
			return _cacheActiveCaptionBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the text in the active window's title bar. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the text in the active window's title bar. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey ActiveCaptionTextBrushKey
	{
		get
		{
			if (_cacheActiveCaptionTextBrush == null)
			{
				_cacheActiveCaptionTextBrush = CreateInstance(SystemResourceKeyID.ActiveCaptionTextBrush);
			}
			return _cacheActiveCaptionTextBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the application workspace.</summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the application workspace. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey AppWorkspaceBrushKey
	{
		get
		{
			if (_cacheAppWorkspaceBrush == null)
			{
				_cacheAppWorkspaceBrush = CreateInstance(SystemResourceKeyID.AppWorkspaceBrush);
			}
			return _cacheAppWorkspaceBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the face of a three-dimensional display element.</summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the face of a three-dimensional display element. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey ControlBrushKey
	{
		get
		{
			if (_cacheControlBrush == null)
			{
				_cacheControlBrush = CreateInstance(SystemResourceKeyID.ControlBrush);
			}
			return _cacheControlBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the shadow of a three-dimensional display element.</summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the shadow of a three-dimensional display element. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey ControlDarkBrushKey
	{
		get
		{
			if (_cacheControlDarkBrush == null)
			{
				_cacheControlDarkBrush = CreateInstance(SystemResourceKeyID.ControlDarkBrush);
			}
			return _cacheControlDarkBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the dark shadow of a three-dimensional display element. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the dark shadow of a three-dimensional display element. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey ControlDarkDarkBrushKey
	{
		get
		{
			if (_cacheControlDarkDarkBrush == null)
			{
				_cacheControlDarkDarkBrush = CreateInstance(SystemResourceKeyID.ControlDarkDarkBrush);
			}
			return _cacheControlDarkDarkBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the light area of a three-dimensional display element. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the light area of a three-dimensional display element. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey ControlLightBrushKey
	{
		get
		{
			if (_cacheControlLightBrush == null)
			{
				_cacheControlLightBrush = CreateInstance(SystemResourceKeyID.ControlLightBrush);
			}
			return _cacheControlLightBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the highlight of a three-dimensional display element. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the highlight of a three-dimensional display element. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey ControlLightLightBrushKey
	{
		get
		{
			if (_cacheControlLightLightBrush == null)
			{
				_cacheControlLightLightBrush = CreateInstance(SystemResourceKeyID.ControlLightLightBrush);
			}
			return _cacheControlLightLightBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints text in a three-dimensional display element. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints text in a three-dimensional display element. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey ControlTextBrushKey
	{
		get
		{
			if (_cacheControlTextBrush == null)
			{
				_cacheControlTextBrush = CreateInstance(SystemResourceKeyID.ControlTextBrush);
			}
			return _cacheControlTextBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the desktop. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the desktop. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey DesktopBrushKey
	{
		get
		{
			if (_cacheDesktopBrush == null)
			{
				_cacheDesktopBrush = CreateInstance(SystemResourceKeyID.DesktopBrush);
			}
			return _cacheDesktopBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color of the right side of the gradient of an active window's title bar. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color of the right side of the gradient of an active window's title bar. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey GradientActiveCaptionBrushKey
	{
		get
		{
			if (_cacheGradientActiveCaptionBrush == null)
			{
				_cacheGradientActiveCaptionBrush = CreateInstance(SystemResourceKeyID.GradientActiveCaptionBrush);
			}
			return _cacheGradientActiveCaptionBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that is the color of the right side of the gradient of an inactive window's title bar.</summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> used to paint the background of the inactive window's title bar. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey GradientInactiveCaptionBrushKey
	{
		get
		{
			if (_cacheGradientInactiveCaptionBrush == null)
			{
				_cacheGradientInactiveCaptionBrush = CreateInstance(SystemResourceKeyID.GradientInactiveCaptionBrush);
			}
			return _cacheGradientInactiveCaptionBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints disabled text. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints disabled text. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey GrayTextBrushKey
	{
		get
		{
			if (_cacheGrayTextBrush == null)
			{
				_cacheGrayTextBrush = CreateInstance(SystemResourceKeyID.GrayTextBrush);
			}
			return _cacheGrayTextBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the background of selected items. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the background of selected items. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey HighlightBrushKey
	{
		get
		{
			if (_cacheHighlightBrush == null)
			{
				_cacheHighlightBrush = CreateInstance(SystemResourceKeyID.HighlightBrush);
			}
			return _cacheHighlightBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the text of selected items. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the text of selected items. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey HighlightTextBrushKey
	{
		get
		{
			if (_cacheHighlightTextBrush == null)
			{
				_cacheHighlightTextBrush = CreateInstance(SystemResourceKeyID.HighlightTextBrush);
			}
			return _cacheHighlightTextBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints hot-tracked items. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints hot-tracked items. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey HotTrackBrushKey
	{
		get
		{
			if (_cacheHotTrackBrush == null)
			{
				_cacheHotTrackBrush = CreateInstance(SystemResourceKeyID.HotTrackBrush);
			}
			return _cacheHotTrackBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the border of an inactive window. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the border of an inactive window. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey InactiveBorderBrushKey
	{
		get
		{
			if (_cacheInactiveBorderBrush == null)
			{
				_cacheInactiveBorderBrush = CreateInstance(SystemResourceKeyID.InactiveBorderBrush);
			}
			return _cacheInactiveBorderBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the background of an inactive window's title bar. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the background of an inactive window's title bar. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey InactiveCaptionBrushKey
	{
		get
		{
			if (_cacheInactiveCaptionBrush == null)
			{
				_cacheInactiveCaptionBrush = CreateInstance(SystemResourceKeyID.InactiveCaptionBrush);
			}
			return _cacheInactiveCaptionBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the text of an inactive window's title bar. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the text of an inactive window's title bar. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey InactiveCaptionTextBrushKey
	{
		get
		{
			if (_cacheInactiveCaptionTextBrush == null)
			{
				_cacheInactiveCaptionTextBrush = CreateInstance(SystemResourceKeyID.InactiveCaptionTextBrush);
			}
			return _cacheInactiveCaptionTextBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the background of the <see cref="T:System.Windows.Controls.ToolTip" /> control.</summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the background of the <see cref="T:System.Windows.Controls.ToolTip" /> control. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey InfoBrushKey
	{
		get
		{
			if (_cacheInfoBrush == null)
			{
				_cacheInfoBrush = CreateInstance(SystemResourceKeyID.InfoBrush);
			}
			return _cacheInfoBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the text in a <see cref="T:System.Windows.Controls.ToolTip" /> control. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the text in a <see cref="T:System.Windows.Controls.ToolTip" /> control. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey InfoTextBrushKey
	{
		get
		{
			if (_cacheInfoTextBrush == null)
			{
				_cacheInfoTextBrush = CreateInstance(SystemResourceKeyID.InfoTextBrush);
			}
			return _cacheInfoTextBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the background of a menu. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the background of a menu. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey MenuBrushKey
	{
		get
		{
			if (_cacheMenuBrush == null)
			{
				_cacheMenuBrush = CreateInstance(SystemResourceKeyID.MenuBrush);
			}
			return _cacheMenuBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the background of a menu bar. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the background of a menu bar. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey MenuBarBrushKey
	{
		get
		{
			if (_cacheMenuBarBrush == null)
			{
				_cacheMenuBarBrush = CreateInstance(SystemResourceKeyID.MenuBarBrush);
			}
			return _cacheMenuBarBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints a highlighted menu item. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints a highlighted menu item. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey MenuHighlightBrushKey
	{
		get
		{
			if (_cacheMenuHighlightBrush == null)
			{
				_cacheMenuHighlightBrush = CreateInstance(SystemResourceKeyID.MenuHighlightBrush);
			}
			return _cacheMenuHighlightBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints a menu's text. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints a menu's text. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey MenuTextBrushKey
	{
		get
		{
			if (_cacheMenuTextBrush == null)
			{
				_cacheMenuTextBrush = CreateInstance(SystemResourceKeyID.MenuTextBrush);
			}
			return _cacheMenuTextBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the background of a scroll bar. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the background of a scroll bar. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey ScrollBarBrushKey
	{
		get
		{
			if (_cacheScrollBarBrush == null)
			{
				_cacheScrollBarBrush = CreateInstance(SystemResourceKeyID.ScrollBarBrush);
			}
			return _cacheScrollBarBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the background of a window's client area. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the background of a window's client area. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey WindowBrushKey
	{
		get
		{
			if (_cacheWindowBrush == null)
			{
				_cacheWindowBrush = CreateInstance(SystemResourceKeyID.WindowBrush);
			}
			return _cacheWindowBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints a window frame. </summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints a window frame. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey WindowFrameBrushKey
	{
		get
		{
			if (_cacheWindowFrameBrush == null)
			{
				_cacheWindowFrameBrush = CreateInstance(SystemResourceKeyID.WindowFrameBrush);
			}
			return _cacheWindowFrameBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the text in the client area of a window.</summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the text in the client area of a window. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey WindowTextBrushKey
	{
		get
		{
			if (_cacheWindowTextBrush == null)
			{
				_cacheWindowTextBrush = CreateInstance(SystemResourceKeyID.WindowTextBrush);
			}
			return _cacheWindowTextBrush;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the background of an inactive selected item.</summary>
	/// <returns>The <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints the background of an inactive selected item. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey InactiveSelectionHighlightBrushKey
	{
		get
		{
			if (FrameworkCompatibilityPreferences.GetAreInactiveSelectionHighlightBrushKeysSupported())
			{
				if (_cacheInactiveSelectionHighlightBrush == null)
				{
					_cacheInactiveSelectionHighlightBrush = CreateInstance(SystemResourceKeyID.InactiveSelectionHighlightBrush);
				}
				return _cacheInactiveSelectionHighlightBrush;
			}
			return ControlBrushKey;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints a an inactive selected item’s text.</summary>
	/// <returns>The <see cref="T:System.Windows.ResourceKey" /> for the <see cref="T:System.Windows.Media.SolidColorBrush" /> that paints a an inactive selected item’s text. This brush's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true, so it cannot be modified.</returns>
	public static ResourceKey InactiveSelectionHighlightTextBrushKey
	{
		get
		{
			if (FrameworkCompatibilityPreferences.GetAreInactiveSelectionHighlightBrushKeysSupported())
			{
				if (_cacheInactiveSelectionHighlightTextBrush == null)
				{
					_cacheInactiveSelectionHighlightTextBrush = CreateInstance(SystemResourceKeyID.InactiveSelectionHighlightTextBrush);
				}
				return _cacheInactiveSelectionHighlightTextBrush;
			}
			return ControlTextBrushKey;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static SystemResourceKey CreateInstance(SystemResourceKeyID KeyId)
	{
		return new SystemResourceKey(KeyId);
	}

	internal static bool InvalidateCache()
	{
		bool num = SystemResources.ClearBitArray(_colorCacheValid);
		bool flag = SystemResources.ClearBitArray(_brushCacheValid);
		return num || flag;
	}

	private static int Encode(int alpha, int red, int green, int blue)
	{
		return (red << 16) | (green << 8) | blue | (alpha << 24);
	}

	private static int FromWin32Value(int value)
	{
		return Encode(255, value & 0xFF, (value >> 8) & 0xFF, (value >> 16) & 0xFF);
	}

	private static Color GetSystemColor(CacheSlot slot)
	{
		lock (_colorCacheValid)
		{
			while (!_colorCacheValid[(int)slot])
			{
				_colorCacheValid[(int)slot] = true;
				uint num = (uint)FromWin32Value(SafeNativeMethods.GetSysColor(SlotToFlag(slot)));
				Color color = Color.FromArgb((byte)((num & 0xFF000000u) >> 24), (byte)((num & 0xFF0000) >> 16), (byte)((num & 0xFF00) >> 8), (byte)(num & 0xFF));
				_colorCache[(int)slot] = color;
			}
			return _colorCache[(int)slot];
		}
	}

	private static SolidColorBrush MakeBrush(CacheSlot slot)
	{
		lock (_brushCacheValid)
		{
			while (!_brushCacheValid[(int)slot])
			{
				_brushCacheValid[(int)slot] = true;
				SolidColorBrush solidColorBrush = new SolidColorBrush(GetSystemColor(slot));
				solidColorBrush.Freeze();
				_brushCache[(int)slot] = solidColorBrush;
			}
			return _brushCache[(int)slot];
		}
	}

	private static int SlotToFlag(CacheSlot slot)
	{
		return slot switch
		{
			CacheSlot.ActiveBorder => 10, 
			CacheSlot.ActiveCaption => 2, 
			CacheSlot.ActiveCaptionText => 9, 
			CacheSlot.AppWorkspace => 12, 
			CacheSlot.Control => 15, 
			CacheSlot.ControlDark => 16, 
			CacheSlot.ControlDarkDark => 21, 
			CacheSlot.ControlLight => 22, 
			CacheSlot.ControlLightLight => 20, 
			CacheSlot.ControlText => 18, 
			CacheSlot.Desktop => 1, 
			CacheSlot.GradientActiveCaption => 27, 
			CacheSlot.GradientInactiveCaption => 28, 
			CacheSlot.GrayText => 17, 
			CacheSlot.Highlight => 13, 
			CacheSlot.HighlightText => 14, 
			CacheSlot.HotTrack => 26, 
			CacheSlot.InactiveBorder => 11, 
			CacheSlot.InactiveCaption => 3, 
			CacheSlot.InactiveCaptionText => 19, 
			CacheSlot.Info => 24, 
			CacheSlot.InfoText => 23, 
			CacheSlot.Menu => 4, 
			CacheSlot.MenuBar => 30, 
			CacheSlot.MenuHighlight => 29, 
			CacheSlot.MenuText => 7, 
			CacheSlot.ScrollBar => 0, 
			CacheSlot.Window => 5, 
			CacheSlot.WindowFrame => 6, 
			CacheSlot.WindowText => 8, 
			_ => 0, 
		};
	}
}
