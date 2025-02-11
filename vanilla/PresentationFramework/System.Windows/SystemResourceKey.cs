using System.ComponentModel;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using MS.Internal.KnownBoxes;

namespace System.Windows;

[TypeConverter(typeof(SystemKeyConverter))]
internal class SystemResourceKey : ResourceKey
{
	private const short SystemResourceKeyIDStart = 0;

	private const short SystemResourceKeyIDEnd = 232;

	private const short SystemResourceKeyIDExtendedStart = 233;

	private const short SystemResourceKeyIDExtendedEnd = 236;

	private const short SystemResourceKeyBAMLIDStart = 0;

	private const short SystemResourceKeyBAMLIDEnd = 232;

	private const short SystemResourceBAMLIDStart = 232;

	private const short SystemResourceBAMLIDEnd = 464;

	private const short SystemResourceKeyBAMLIDExtendedStart = 464;

	private const short SystemResourceKeyBAMLIDExtendedEnd = 467;

	private const short SystemResourceBAMLIDExtendedStart = 467;

	private const short SystemResourceBAMLIDExtendedEnd = 470;

	private static SystemThemeKey _cacheSeparatorStyle;

	private static SystemThemeKey _cacheCheckBoxStyle;

	private static SystemThemeKey _cacheToggleButtonStyle;

	private static SystemThemeKey _cacheButtonStyle;

	private static SystemThemeKey _cacheRadioButtonStyle;

	private static SystemThemeKey _cacheComboBoxStyle;

	private static SystemThemeKey _cacheTextBoxStyle;

	private static SystemThemeKey _cacheMenuStyle;

	private static ComponentResourceKey _focusBorderBrushKey;

	private static ComponentResourceKey _textBlockComboBoxStyleKey;

	private static SystemThemeKey _menuItemSeparatorStyleKey;

	private static ComponentResourceKey _columnHeaderDropSeparatorStyleKey;

	private static ComponentResourceKey _columnFloatingHeaderStyleKey;

	private static SystemThemeKey _gridViewItemContainerStyleKey;

	private static SystemThemeKey _scrollViewerStyleKey;

	private static SystemThemeKey _gridViewStyleKey;

	private static SystemThemeKey _statusBarSeparatorStyleKey;

	private SystemResourceKeyID _id;

	[ThreadStatic]
	private static SystemResourceKey _srk;

	internal object Resource => _id switch
	{
		SystemResourceKeyID.ActiveBorderBrush => SystemColors.ActiveBorderBrush, 
		SystemResourceKeyID.ActiveCaptionBrush => SystemColors.ActiveCaptionBrush, 
		SystemResourceKeyID.ActiveCaptionTextBrush => SystemColors.ActiveCaptionTextBrush, 
		SystemResourceKeyID.AppWorkspaceBrush => SystemColors.AppWorkspaceBrush, 
		SystemResourceKeyID.ControlBrush => SystemColors.ControlBrush, 
		SystemResourceKeyID.ControlDarkBrush => SystemColors.ControlDarkBrush, 
		SystemResourceKeyID.ControlDarkDarkBrush => SystemColors.ControlDarkDarkBrush, 
		SystemResourceKeyID.ControlLightBrush => SystemColors.ControlLightBrush, 
		SystemResourceKeyID.ControlLightLightBrush => SystemColors.ControlLightLightBrush, 
		SystemResourceKeyID.ControlTextBrush => SystemColors.ControlTextBrush, 
		SystemResourceKeyID.DesktopBrush => SystemColors.DesktopBrush, 
		SystemResourceKeyID.GradientActiveCaptionBrush => SystemColors.GradientActiveCaptionBrush, 
		SystemResourceKeyID.GradientInactiveCaptionBrush => SystemColors.GradientInactiveCaptionBrush, 
		SystemResourceKeyID.GrayTextBrush => SystemColors.GrayTextBrush, 
		SystemResourceKeyID.HighlightBrush => SystemColors.HighlightBrush, 
		SystemResourceKeyID.HighlightTextBrush => SystemColors.HighlightTextBrush, 
		SystemResourceKeyID.HotTrackBrush => SystemColors.HotTrackBrush, 
		SystemResourceKeyID.InactiveBorderBrush => SystemColors.InactiveBorderBrush, 
		SystemResourceKeyID.InactiveCaptionBrush => SystemColors.InactiveCaptionBrush, 
		SystemResourceKeyID.InactiveCaptionTextBrush => SystemColors.InactiveCaptionTextBrush, 
		SystemResourceKeyID.InfoBrush => SystemColors.InfoBrush, 
		SystemResourceKeyID.InfoTextBrush => SystemColors.InfoTextBrush, 
		SystemResourceKeyID.MenuBrush => SystemColors.MenuBrush, 
		SystemResourceKeyID.MenuBarBrush => SystemColors.MenuBarBrush, 
		SystemResourceKeyID.MenuHighlightBrush => SystemColors.MenuHighlightBrush, 
		SystemResourceKeyID.MenuTextBrush => SystemColors.MenuTextBrush, 
		SystemResourceKeyID.ScrollBarBrush => SystemColors.ScrollBarBrush, 
		SystemResourceKeyID.WindowBrush => SystemColors.WindowBrush, 
		SystemResourceKeyID.WindowFrameBrush => SystemColors.WindowFrameBrush, 
		SystemResourceKeyID.WindowTextBrush => SystemColors.WindowTextBrush, 
		SystemResourceKeyID.InactiveSelectionHighlightBrush => SystemColors.InactiveSelectionHighlightBrush, 
		SystemResourceKeyID.InactiveSelectionHighlightTextBrush => SystemColors.InactiveSelectionHighlightTextBrush, 
		SystemResourceKeyID.ActiveBorderColor => SystemColors.ActiveBorderColor, 
		SystemResourceKeyID.ActiveCaptionColor => SystemColors.ActiveCaptionColor, 
		SystemResourceKeyID.ActiveCaptionTextColor => SystemColors.ActiveCaptionTextColor, 
		SystemResourceKeyID.AppWorkspaceColor => SystemColors.AppWorkspaceColor, 
		SystemResourceKeyID.ControlColor => SystemColors.ControlColor, 
		SystemResourceKeyID.ControlDarkColor => SystemColors.ControlDarkColor, 
		SystemResourceKeyID.ControlDarkDarkColor => SystemColors.ControlDarkDarkColor, 
		SystemResourceKeyID.ControlLightColor => SystemColors.ControlLightColor, 
		SystemResourceKeyID.ControlLightLightColor => SystemColors.ControlLightLightColor, 
		SystemResourceKeyID.ControlTextColor => SystemColors.ControlTextColor, 
		SystemResourceKeyID.DesktopColor => SystemColors.DesktopColor, 
		SystemResourceKeyID.GradientActiveCaptionColor => SystemColors.GradientActiveCaptionColor, 
		SystemResourceKeyID.GradientInactiveCaptionColor => SystemColors.GradientInactiveCaptionColor, 
		SystemResourceKeyID.GrayTextColor => SystemColors.GrayTextColor, 
		SystemResourceKeyID.HighlightColor => SystemColors.HighlightColor, 
		SystemResourceKeyID.HighlightTextColor => SystemColors.HighlightTextColor, 
		SystemResourceKeyID.HotTrackColor => SystemColors.HotTrackColor, 
		SystemResourceKeyID.InactiveBorderColor => SystemColors.InactiveBorderColor, 
		SystemResourceKeyID.InactiveCaptionColor => SystemColors.InactiveCaptionColor, 
		SystemResourceKeyID.InactiveCaptionTextColor => SystemColors.InactiveCaptionTextColor, 
		SystemResourceKeyID.InfoColor => SystemColors.InfoColor, 
		SystemResourceKeyID.InfoTextColor => SystemColors.InfoTextColor, 
		SystemResourceKeyID.MenuColor => SystemColors.MenuColor, 
		SystemResourceKeyID.MenuBarColor => SystemColors.MenuBarColor, 
		SystemResourceKeyID.MenuHighlightColor => SystemColors.MenuHighlightColor, 
		SystemResourceKeyID.MenuTextColor => SystemColors.MenuTextColor, 
		SystemResourceKeyID.ScrollBarColor => SystemColors.ScrollBarColor, 
		SystemResourceKeyID.WindowColor => SystemColors.WindowColor, 
		SystemResourceKeyID.WindowFrameColor => SystemColors.WindowFrameColor, 
		SystemResourceKeyID.WindowTextColor => SystemColors.WindowTextColor, 
		SystemResourceKeyID.ThinHorizontalBorderHeight => SystemParameters.ThinHorizontalBorderHeight, 
		SystemResourceKeyID.ThinVerticalBorderWidth => SystemParameters.ThinVerticalBorderWidth, 
		SystemResourceKeyID.CursorWidth => SystemParameters.CursorWidth, 
		SystemResourceKeyID.CursorHeight => SystemParameters.CursorHeight, 
		SystemResourceKeyID.ThickHorizontalBorderHeight => SystemParameters.ThickHorizontalBorderHeight, 
		SystemResourceKeyID.ThickVerticalBorderWidth => SystemParameters.ThickVerticalBorderWidth, 
		SystemResourceKeyID.FixedFrameHorizontalBorderHeight => SystemParameters.FixedFrameHorizontalBorderHeight, 
		SystemResourceKeyID.FixedFrameVerticalBorderWidth => SystemParameters.FixedFrameVerticalBorderWidth, 
		SystemResourceKeyID.FocusHorizontalBorderHeight => SystemParameters.FocusHorizontalBorderHeight, 
		SystemResourceKeyID.FocusVerticalBorderWidth => SystemParameters.FocusVerticalBorderWidth, 
		SystemResourceKeyID.FullPrimaryScreenWidth => SystemParameters.FullPrimaryScreenWidth, 
		SystemResourceKeyID.FullPrimaryScreenHeight => SystemParameters.FullPrimaryScreenHeight, 
		SystemResourceKeyID.HorizontalScrollBarButtonWidth => SystemParameters.HorizontalScrollBarButtonWidth, 
		SystemResourceKeyID.HorizontalScrollBarHeight => SystemParameters.HorizontalScrollBarHeight, 
		SystemResourceKeyID.HorizontalScrollBarThumbWidth => SystemParameters.HorizontalScrollBarThumbWidth, 
		SystemResourceKeyID.IconWidth => SystemParameters.IconWidth, 
		SystemResourceKeyID.IconHeight => SystemParameters.IconHeight, 
		SystemResourceKeyID.IconGridWidth => SystemParameters.IconGridWidth, 
		SystemResourceKeyID.IconGridHeight => SystemParameters.IconGridHeight, 
		SystemResourceKeyID.MaximizedPrimaryScreenWidth => SystemParameters.MaximizedPrimaryScreenWidth, 
		SystemResourceKeyID.MaximizedPrimaryScreenHeight => SystemParameters.MaximizedPrimaryScreenHeight, 
		SystemResourceKeyID.MaximumWindowTrackWidth => SystemParameters.MaximumWindowTrackWidth, 
		SystemResourceKeyID.MaximumWindowTrackHeight => SystemParameters.MaximumWindowTrackHeight, 
		SystemResourceKeyID.MenuCheckmarkWidth => SystemParameters.MenuCheckmarkWidth, 
		SystemResourceKeyID.MenuCheckmarkHeight => SystemParameters.MenuCheckmarkHeight, 
		SystemResourceKeyID.MenuButtonWidth => SystemParameters.MenuButtonWidth, 
		SystemResourceKeyID.MenuButtonHeight => SystemParameters.MenuButtonHeight, 
		SystemResourceKeyID.MinimumWindowWidth => SystemParameters.MinimumWindowWidth, 
		SystemResourceKeyID.MinimumWindowHeight => SystemParameters.MinimumWindowHeight, 
		SystemResourceKeyID.MinimizedWindowWidth => SystemParameters.MinimizedWindowWidth, 
		SystemResourceKeyID.MinimizedWindowHeight => SystemParameters.MinimizedWindowHeight, 
		SystemResourceKeyID.MinimizedGridWidth => SystemParameters.MinimizedGridWidth, 
		SystemResourceKeyID.MinimizedGridHeight => SystemParameters.MinimizedGridHeight, 
		SystemResourceKeyID.MinimumWindowTrackWidth => SystemParameters.MinimumWindowTrackWidth, 
		SystemResourceKeyID.MinimumWindowTrackHeight => SystemParameters.MinimumWindowTrackHeight, 
		SystemResourceKeyID.PrimaryScreenWidth => SystemParameters.PrimaryScreenWidth, 
		SystemResourceKeyID.PrimaryScreenHeight => SystemParameters.PrimaryScreenHeight, 
		SystemResourceKeyID.WindowCaptionButtonWidth => SystemParameters.WindowCaptionButtonWidth, 
		SystemResourceKeyID.WindowCaptionButtonHeight => SystemParameters.WindowCaptionButtonHeight, 
		SystemResourceKeyID.ResizeFrameHorizontalBorderHeight => SystemParameters.ResizeFrameHorizontalBorderHeight, 
		SystemResourceKeyID.ResizeFrameVerticalBorderWidth => SystemParameters.ResizeFrameVerticalBorderWidth, 
		SystemResourceKeyID.SmallIconWidth => SystemParameters.SmallIconWidth, 
		SystemResourceKeyID.SmallIconHeight => SystemParameters.SmallIconHeight, 
		SystemResourceKeyID.SmallWindowCaptionButtonWidth => SystemParameters.SmallWindowCaptionButtonWidth, 
		SystemResourceKeyID.SmallWindowCaptionButtonHeight => SystemParameters.SmallWindowCaptionButtonHeight, 
		SystemResourceKeyID.VirtualScreenWidth => SystemParameters.VirtualScreenWidth, 
		SystemResourceKeyID.VirtualScreenHeight => SystemParameters.VirtualScreenHeight, 
		SystemResourceKeyID.VerticalScrollBarWidth => SystemParameters.VerticalScrollBarWidth, 
		SystemResourceKeyID.VerticalScrollBarButtonHeight => SystemParameters.VerticalScrollBarButtonHeight, 
		SystemResourceKeyID.WindowCaptionHeight => SystemParameters.WindowCaptionHeight, 
		SystemResourceKeyID.KanjiWindowHeight => SystemParameters.KanjiWindowHeight, 
		SystemResourceKeyID.MenuBarHeight => SystemParameters.MenuBarHeight, 
		SystemResourceKeyID.SmallCaptionHeight => SystemParameters.SmallCaptionHeight, 
		SystemResourceKeyID.VerticalScrollBarThumbHeight => SystemParameters.VerticalScrollBarThumbHeight, 
		SystemResourceKeyID.IsImmEnabled => BooleanBoxes.Box(SystemParameters.IsImmEnabled), 
		SystemResourceKeyID.IsMediaCenter => BooleanBoxes.Box(SystemParameters.IsMediaCenter), 
		SystemResourceKeyID.IsMenuDropRightAligned => BooleanBoxes.Box(SystemParameters.IsMenuDropRightAligned), 
		SystemResourceKeyID.IsMiddleEastEnabled => BooleanBoxes.Box(SystemParameters.IsMiddleEastEnabled), 
		SystemResourceKeyID.IsMousePresent => BooleanBoxes.Box(SystemParameters.IsMousePresent), 
		SystemResourceKeyID.IsMouseWheelPresent => BooleanBoxes.Box(SystemParameters.IsMouseWheelPresent), 
		SystemResourceKeyID.IsPenWindows => BooleanBoxes.Box(SystemParameters.IsPenWindows), 
		SystemResourceKeyID.IsRemotelyControlled => BooleanBoxes.Box(SystemParameters.IsRemotelyControlled), 
		SystemResourceKeyID.IsRemoteSession => BooleanBoxes.Box(SystemParameters.IsRemoteSession), 
		SystemResourceKeyID.ShowSounds => BooleanBoxes.Box(SystemParameters.ShowSounds), 
		SystemResourceKeyID.IsSlowMachine => BooleanBoxes.Box(SystemParameters.IsSlowMachine), 
		SystemResourceKeyID.SwapButtons => BooleanBoxes.Box(SystemParameters.SwapButtons), 
		SystemResourceKeyID.IsTabletPC => BooleanBoxes.Box(SystemParameters.IsTabletPC), 
		SystemResourceKeyID.VirtualScreenLeft => SystemParameters.VirtualScreenLeft, 
		SystemResourceKeyID.VirtualScreenTop => SystemParameters.VirtualScreenTop, 
		SystemResourceKeyID.FocusBorderWidth => SystemParameters.FocusBorderWidth, 
		SystemResourceKeyID.FocusBorderHeight => SystemParameters.FocusBorderHeight, 
		SystemResourceKeyID.HighContrast => BooleanBoxes.Box(SystemParameters.HighContrast), 
		SystemResourceKeyID.DropShadow => BooleanBoxes.Box(SystemParameters.DropShadow), 
		SystemResourceKeyID.FlatMenu => BooleanBoxes.Box(SystemParameters.FlatMenu), 
		SystemResourceKeyID.WorkArea => SystemParameters.WorkArea, 
		SystemResourceKeyID.IconHorizontalSpacing => SystemParameters.IconHorizontalSpacing, 
		SystemResourceKeyID.IconVerticalSpacing => SystemParameters.IconVerticalSpacing, 
		SystemResourceKeyID.IconTitleWrap => SystemParameters.IconTitleWrap, 
		SystemResourceKeyID.IconFontSize => SystemFonts.IconFontSize, 
		SystemResourceKeyID.IconFontFamily => SystemFonts.IconFontFamily, 
		SystemResourceKeyID.IconFontStyle => SystemFonts.IconFontStyle, 
		SystemResourceKeyID.IconFontWeight => SystemFonts.IconFontWeight, 
		SystemResourceKeyID.IconFontTextDecorations => SystemFonts.IconFontTextDecorations, 
		SystemResourceKeyID.KeyboardCues => BooleanBoxes.Box(SystemParameters.KeyboardCues), 
		SystemResourceKeyID.KeyboardDelay => SystemParameters.KeyboardDelay, 
		SystemResourceKeyID.KeyboardPreference => BooleanBoxes.Box(SystemParameters.KeyboardPreference), 
		SystemResourceKeyID.KeyboardSpeed => SystemParameters.KeyboardSpeed, 
		SystemResourceKeyID.SnapToDefaultButton => BooleanBoxes.Box(SystemParameters.SnapToDefaultButton), 
		SystemResourceKeyID.WheelScrollLines => SystemParameters.WheelScrollLines, 
		SystemResourceKeyID.MouseHoverTime => SystemParameters.MouseHoverTime, 
		SystemResourceKeyID.MouseHoverHeight => SystemParameters.MouseHoverHeight, 
		SystemResourceKeyID.MouseHoverWidth => SystemParameters.MouseHoverWidth, 
		SystemResourceKeyID.MenuDropAlignment => BooleanBoxes.Box(SystemParameters.MenuDropAlignment), 
		SystemResourceKeyID.MenuFade => BooleanBoxes.Box(SystemParameters.MenuFade), 
		SystemResourceKeyID.MenuShowDelay => SystemParameters.MenuShowDelay, 
		SystemResourceKeyID.ComboBoxAnimation => BooleanBoxes.Box(SystemParameters.ComboBoxAnimation), 
		SystemResourceKeyID.ClientAreaAnimation => BooleanBoxes.Box(SystemParameters.ClientAreaAnimation), 
		SystemResourceKeyID.CursorShadow => BooleanBoxes.Box(SystemParameters.CursorShadow), 
		SystemResourceKeyID.GradientCaptions => BooleanBoxes.Box(SystemParameters.GradientCaptions), 
		SystemResourceKeyID.HotTracking => BooleanBoxes.Box(SystemParameters.HotTracking), 
		SystemResourceKeyID.ListBoxSmoothScrolling => BooleanBoxes.Box(SystemParameters.ListBoxSmoothScrolling), 
		SystemResourceKeyID.MenuAnimation => BooleanBoxes.Box(SystemParameters.MenuAnimation), 
		SystemResourceKeyID.SelectionFade => BooleanBoxes.Box(SystemParameters.SelectionFade), 
		SystemResourceKeyID.StylusHotTracking => BooleanBoxes.Box(SystemParameters.StylusHotTracking), 
		SystemResourceKeyID.ToolTipAnimation => BooleanBoxes.Box(SystemParameters.ToolTipAnimation), 
		SystemResourceKeyID.ToolTipFade => BooleanBoxes.Box(SystemParameters.ToolTipFade), 
		SystemResourceKeyID.UIEffects => BooleanBoxes.Box(SystemParameters.UIEffects), 
		SystemResourceKeyID.MinimizeAnimation => BooleanBoxes.Box(SystemParameters.MinimizeAnimation), 
		SystemResourceKeyID.Border => SystemParameters.Border, 
		SystemResourceKeyID.CaretWidth => SystemParameters.CaretWidth, 
		SystemResourceKeyID.ForegroundFlashCount => SystemParameters.ForegroundFlashCount, 
		SystemResourceKeyID.DragFullWindows => BooleanBoxes.Box(SystemParameters.DragFullWindows), 
		SystemResourceKeyID.BorderWidth => SystemParameters.BorderWidth, 
		SystemResourceKeyID.ScrollWidth => SystemParameters.ScrollWidth, 
		SystemResourceKeyID.ScrollHeight => SystemParameters.ScrollHeight, 
		SystemResourceKeyID.CaptionWidth => SystemParameters.CaptionWidth, 
		SystemResourceKeyID.CaptionHeight => SystemParameters.CaptionHeight, 
		SystemResourceKeyID.SmallCaptionWidth => SystemParameters.SmallCaptionWidth, 
		SystemResourceKeyID.MenuWidth => SystemParameters.MenuWidth, 
		SystemResourceKeyID.MenuHeight => SystemParameters.MenuHeight, 
		SystemResourceKeyID.CaptionFontSize => SystemFonts.CaptionFontSize, 
		SystemResourceKeyID.CaptionFontFamily => SystemFonts.CaptionFontFamily, 
		SystemResourceKeyID.CaptionFontStyle => SystemFonts.CaptionFontStyle, 
		SystemResourceKeyID.CaptionFontWeight => SystemFonts.CaptionFontWeight, 
		SystemResourceKeyID.CaptionFontTextDecorations => SystemFonts.CaptionFontTextDecorations, 
		SystemResourceKeyID.SmallCaptionFontSize => SystemFonts.SmallCaptionFontSize, 
		SystemResourceKeyID.SmallCaptionFontFamily => SystemFonts.SmallCaptionFontFamily, 
		SystemResourceKeyID.SmallCaptionFontStyle => SystemFonts.SmallCaptionFontStyle, 
		SystemResourceKeyID.SmallCaptionFontWeight => SystemFonts.SmallCaptionFontWeight, 
		SystemResourceKeyID.SmallCaptionFontTextDecorations => SystemFonts.SmallCaptionFontTextDecorations, 
		SystemResourceKeyID.MenuFontSize => SystemFonts.MenuFontSize, 
		SystemResourceKeyID.MenuFontFamily => SystemFonts.MenuFontFamily, 
		SystemResourceKeyID.MenuFontStyle => SystemFonts.MenuFontStyle, 
		SystemResourceKeyID.MenuFontWeight => SystemFonts.MenuFontWeight, 
		SystemResourceKeyID.MenuFontTextDecorations => SystemFonts.MenuFontTextDecorations, 
		SystemResourceKeyID.StatusFontSize => SystemFonts.StatusFontSize, 
		SystemResourceKeyID.StatusFontFamily => SystemFonts.StatusFontFamily, 
		SystemResourceKeyID.StatusFontStyle => SystemFonts.StatusFontStyle, 
		SystemResourceKeyID.StatusFontWeight => SystemFonts.StatusFontWeight, 
		SystemResourceKeyID.StatusFontTextDecorations => SystemFonts.StatusFontTextDecorations, 
		SystemResourceKeyID.MessageFontSize => SystemFonts.MessageFontSize, 
		SystemResourceKeyID.MessageFontFamily => SystemFonts.MessageFontFamily, 
		SystemResourceKeyID.MessageFontStyle => SystemFonts.MessageFontStyle, 
		SystemResourceKeyID.MessageFontWeight => SystemFonts.MessageFontWeight, 
		SystemResourceKeyID.MessageFontTextDecorations => SystemFonts.MessageFontTextDecorations, 
		SystemResourceKeyID.ComboBoxPopupAnimation => SystemParameters.ComboBoxPopupAnimation, 
		SystemResourceKeyID.MenuPopupAnimation => SystemParameters.MenuPopupAnimation, 
		SystemResourceKeyID.ToolTipPopupAnimation => SystemParameters.ToolTipPopupAnimation, 
		SystemResourceKeyID.PowerLineStatus => SystemParameters.PowerLineStatus, 
		_ => null, 
	};

	internal SystemResourceKeyID InternalKey => _id;

	public override Assembly Assembly => null;

	internal static ComponentResourceKey DataGridFocusBorderBrushKey
	{
		get
		{
			if (_focusBorderBrushKey == null)
			{
				_focusBorderBrushKey = new ComponentResourceKey(typeof(DataGrid), "FocusBorderBrushKey");
			}
			return _focusBorderBrushKey;
		}
	}

	internal static ComponentResourceKey DataGridComboBoxColumnTextBlockComboBoxStyleKey
	{
		get
		{
			if (_textBlockComboBoxStyleKey == null)
			{
				_textBlockComboBoxStyleKey = new ComponentResourceKey(typeof(DataGrid), "TextBlockComboBoxStyleKey");
			}
			return _textBlockComboBoxStyleKey;
		}
	}

	internal static ResourceKey MenuItemSeparatorStyleKey
	{
		get
		{
			if (_menuItemSeparatorStyleKey == null)
			{
				_menuItemSeparatorStyleKey = new SystemThemeKey(SystemResourceKeyID.MenuItemSeparatorStyle);
			}
			return _menuItemSeparatorStyleKey;
		}
	}

	internal static ComponentResourceKey DataGridColumnHeaderColumnFloatingHeaderStyleKey
	{
		get
		{
			if (_columnFloatingHeaderStyleKey == null)
			{
				_columnFloatingHeaderStyleKey = new ComponentResourceKey(typeof(DataGrid), "ColumnFloatingHeaderStyleKey");
			}
			return _columnFloatingHeaderStyleKey;
		}
	}

	internal static ComponentResourceKey DataGridColumnHeaderColumnHeaderDropSeparatorStyleKey
	{
		get
		{
			if (_columnHeaderDropSeparatorStyleKey == null)
			{
				_columnHeaderDropSeparatorStyleKey = new ComponentResourceKey(typeof(DataGrid), "ColumnHeaderDropSeparatorStyleKey");
			}
			return _columnHeaderDropSeparatorStyleKey;
		}
	}

	internal static ResourceKey GridViewItemContainerStyleKey
	{
		get
		{
			if (_gridViewItemContainerStyleKey == null)
			{
				_gridViewItemContainerStyleKey = new SystemThemeKey(SystemResourceKeyID.GridViewItemContainerStyle);
			}
			return _gridViewItemContainerStyleKey;
		}
	}

	internal static ResourceKey GridViewScrollViewerStyleKey
	{
		get
		{
			if (_scrollViewerStyleKey == null)
			{
				_scrollViewerStyleKey = new SystemThemeKey(SystemResourceKeyID.GridViewScrollViewerStyle);
			}
			return _scrollViewerStyleKey;
		}
	}

	internal static ResourceKey GridViewStyleKey
	{
		get
		{
			if (_gridViewStyleKey == null)
			{
				_gridViewStyleKey = new SystemThemeKey(SystemResourceKeyID.GridViewStyle);
			}
			return _gridViewStyleKey;
		}
	}

	internal static ResourceKey StatusBarSeparatorStyleKey
	{
		get
		{
			if (_statusBarSeparatorStyleKey == null)
			{
				_statusBarSeparatorStyleKey = new SystemThemeKey(SystemResourceKeyID.StatusBarSeparatorStyle);
			}
			return _statusBarSeparatorStyleKey;
		}
	}

	internal static ResourceKey ToolBarButtonStyleKey
	{
		get
		{
			if (_cacheButtonStyle == null)
			{
				_cacheButtonStyle = new SystemThemeKey(SystemResourceKeyID.ToolBarButtonStyle);
			}
			return _cacheButtonStyle;
		}
	}

	internal static ResourceKey ToolBarToggleButtonStyleKey
	{
		get
		{
			if (_cacheToggleButtonStyle == null)
			{
				_cacheToggleButtonStyle = new SystemThemeKey(SystemResourceKeyID.ToolBarToggleButtonStyle);
			}
			return _cacheToggleButtonStyle;
		}
	}

	internal static ResourceKey ToolBarSeparatorStyleKey
	{
		get
		{
			if (_cacheSeparatorStyle == null)
			{
				_cacheSeparatorStyle = new SystemThemeKey(SystemResourceKeyID.ToolBarSeparatorStyle);
			}
			return _cacheSeparatorStyle;
		}
	}

	internal static ResourceKey ToolBarCheckBoxStyleKey
	{
		get
		{
			if (_cacheCheckBoxStyle == null)
			{
				_cacheCheckBoxStyle = new SystemThemeKey(SystemResourceKeyID.ToolBarCheckBoxStyle);
			}
			return _cacheCheckBoxStyle;
		}
	}

	internal static ResourceKey ToolBarRadioButtonStyleKey
	{
		get
		{
			if (_cacheRadioButtonStyle == null)
			{
				_cacheRadioButtonStyle = new SystemThemeKey(SystemResourceKeyID.ToolBarRadioButtonStyle);
			}
			return _cacheRadioButtonStyle;
		}
	}

	internal static ResourceKey ToolBarComboBoxStyleKey
	{
		get
		{
			if (_cacheComboBoxStyle == null)
			{
				_cacheComboBoxStyle = new SystemThemeKey(SystemResourceKeyID.ToolBarComboBoxStyle);
			}
			return _cacheComboBoxStyle;
		}
	}

	internal static ResourceKey ToolBarTextBoxStyleKey
	{
		get
		{
			if (_cacheTextBoxStyle == null)
			{
				_cacheTextBoxStyle = new SystemThemeKey(SystemResourceKeyID.ToolBarTextBoxStyle);
			}
			return _cacheTextBoxStyle;
		}
	}

	internal static ResourceKey ToolBarMenuStyleKey
	{
		get
		{
			if (_cacheMenuStyle == null)
			{
				_cacheMenuStyle = new SystemThemeKey(SystemResourceKeyID.ToolBarMenuStyle);
			}
			return _cacheMenuStyle;
		}
	}

	internal static short GetSystemResourceKeyIdFromBamlId(short bamlId, out bool isKey)
	{
		isKey = true;
		if (bamlId > 232 && bamlId < 464)
		{
			bamlId -= 232;
			isKey = false;
		}
		else if (bamlId > 464 && bamlId < 467)
		{
			bamlId -= 231;
		}
		else if (bamlId > 467 && bamlId < 470)
		{
			bamlId -= 234;
			isKey = false;
		}
		return bamlId;
	}

	internal static short GetBamlIdBasedOnSystemResourceKeyId(Type targetType, string memberName)
	{
		short result = 0;
		string text = null;
		bool flag = false;
		bool flag2 = true;
		SystemResourceKeyID systemResourceKeyID = SystemResourceKeyID.InternalSystemColorsStart;
		if (memberName.EndsWith("Key", ignoreCase: false, TypeConverterHelper.InvariantEnglishUS))
		{
			text = memberName.Remove(memberName.Length - 3);
			if (KnownTypes.Types[403] == targetType || KnownTypes.Types[669] == targetType || KnownTypes.Types[604] == targetType)
			{
				text = targetType.Name + text;
			}
			flag = true;
		}
		else
		{
			text = memberName;
		}
		try
		{
			systemResourceKeyID = (SystemResourceKeyID)Enum.Parse(typeof(SystemResourceKeyID), text);
		}
		catch (ArgumentException)
		{
			flag2 = false;
		}
		if (flag2)
		{
			result = (((short)systemResourceKeyID > 233 && (short)systemResourceKeyID < 236) ? ((!flag) ? ((short)(-((short)systemResourceKeyID - 233 + 467))) : ((short)(-((short)systemResourceKeyID - 233 + 464)))) : ((!flag) ? ((short)(-((short)systemResourceKeyID + 232))) : ((short)(-(short)systemResourceKeyID))));
		}
		return result;
	}

	internal static ResourceKey GetResourceKey(short id)
	{
		return id switch
		{
			1 => SystemColors.ActiveBorderBrushKey, 
			2 => SystemColors.ActiveCaptionBrushKey, 
			3 => SystemColors.ActiveCaptionTextBrushKey, 
			4 => SystemColors.AppWorkspaceBrushKey, 
			5 => SystemColors.ControlBrushKey, 
			6 => SystemColors.ControlDarkBrushKey, 
			7 => SystemColors.ControlDarkDarkBrushKey, 
			8 => SystemColors.ControlLightBrushKey, 
			9 => SystemColors.ControlLightLightBrushKey, 
			10 => SystemColors.ControlTextBrushKey, 
			11 => SystemColors.DesktopBrushKey, 
			12 => SystemColors.GradientActiveCaptionBrushKey, 
			13 => SystemColors.GradientInactiveCaptionBrushKey, 
			14 => SystemColors.GrayTextBrushKey, 
			15 => SystemColors.HighlightBrushKey, 
			16 => SystemColors.HighlightTextBrushKey, 
			17 => SystemColors.HotTrackBrushKey, 
			18 => SystemColors.InactiveBorderBrushKey, 
			19 => SystemColors.InactiveCaptionBrushKey, 
			20 => SystemColors.InactiveCaptionTextBrushKey, 
			21 => SystemColors.InfoBrushKey, 
			22 => SystemColors.InfoTextBrushKey, 
			23 => SystemColors.MenuBrushKey, 
			24 => SystemColors.MenuBarBrushKey, 
			25 => SystemColors.MenuHighlightBrushKey, 
			26 => SystemColors.MenuTextBrushKey, 
			27 => SystemColors.ScrollBarBrushKey, 
			28 => SystemColors.WindowBrushKey, 
			29 => SystemColors.WindowFrameBrushKey, 
			30 => SystemColors.WindowTextBrushKey, 
			234 => SystemColors.InactiveSelectionHighlightBrushKey, 
			235 => SystemColors.InactiveSelectionHighlightTextBrushKey, 
			31 => SystemColors.ActiveBorderColorKey, 
			32 => SystemColors.ActiveCaptionColorKey, 
			33 => SystemColors.ActiveCaptionTextColorKey, 
			34 => SystemColors.AppWorkspaceColorKey, 
			35 => SystemColors.ControlColorKey, 
			36 => SystemColors.ControlDarkColorKey, 
			37 => SystemColors.ControlDarkDarkColorKey, 
			38 => SystemColors.ControlLightColorKey, 
			39 => SystemColors.ControlLightLightColorKey, 
			40 => SystemColors.ControlTextColorKey, 
			41 => SystemColors.DesktopColorKey, 
			42 => SystemColors.GradientActiveCaptionColorKey, 
			43 => SystemColors.GradientInactiveCaptionColorKey, 
			44 => SystemColors.GrayTextColorKey, 
			45 => SystemColors.HighlightColorKey, 
			46 => SystemColors.HighlightTextColorKey, 
			47 => SystemColors.HotTrackColorKey, 
			48 => SystemColors.InactiveBorderColorKey, 
			49 => SystemColors.InactiveCaptionColorKey, 
			50 => SystemColors.InactiveCaptionTextColorKey, 
			51 => SystemColors.InfoColorKey, 
			52 => SystemColors.InfoTextColorKey, 
			53 => SystemColors.MenuColorKey, 
			54 => SystemColors.MenuBarColorKey, 
			55 => SystemColors.MenuHighlightColorKey, 
			56 => SystemColors.MenuTextColorKey, 
			57 => SystemColors.ScrollBarColorKey, 
			58 => SystemColors.WindowColorKey, 
			59 => SystemColors.WindowFrameColorKey, 
			60 => SystemColors.WindowTextColorKey, 
			95 => SystemParameters.ThinHorizontalBorderHeightKey, 
			96 => SystemParameters.ThinVerticalBorderWidthKey, 
			97 => SystemParameters.CursorWidthKey, 
			98 => SystemParameters.CursorHeightKey, 
			99 => SystemParameters.ThickHorizontalBorderHeightKey, 
			100 => SystemParameters.ThickVerticalBorderWidthKey, 
			101 => SystemParameters.FixedFrameHorizontalBorderHeightKey, 
			102 => SystemParameters.FixedFrameVerticalBorderWidthKey, 
			103 => SystemParameters.FocusHorizontalBorderHeightKey, 
			104 => SystemParameters.FocusVerticalBorderWidthKey, 
			105 => SystemParameters.FullPrimaryScreenWidthKey, 
			106 => SystemParameters.FullPrimaryScreenHeightKey, 
			107 => SystemParameters.HorizontalScrollBarButtonWidthKey, 
			108 => SystemParameters.HorizontalScrollBarHeightKey, 
			109 => SystemParameters.HorizontalScrollBarThumbWidthKey, 
			110 => SystemParameters.IconWidthKey, 
			111 => SystemParameters.IconHeightKey, 
			112 => SystemParameters.IconGridWidthKey, 
			113 => SystemParameters.IconGridHeightKey, 
			114 => SystemParameters.MaximizedPrimaryScreenWidthKey, 
			115 => SystemParameters.MaximizedPrimaryScreenHeightKey, 
			116 => SystemParameters.MaximumWindowTrackWidthKey, 
			117 => SystemParameters.MaximumWindowTrackHeightKey, 
			118 => SystemParameters.MenuCheckmarkWidthKey, 
			119 => SystemParameters.MenuCheckmarkHeightKey, 
			120 => SystemParameters.MenuButtonWidthKey, 
			121 => SystemParameters.MenuButtonHeightKey, 
			122 => SystemParameters.MinimumWindowWidthKey, 
			123 => SystemParameters.MinimumWindowHeightKey, 
			124 => SystemParameters.MinimizedWindowWidthKey, 
			125 => SystemParameters.MinimizedWindowHeightKey, 
			126 => SystemParameters.MinimizedGridWidthKey, 
			127 => SystemParameters.MinimizedGridHeightKey, 
			128 => SystemParameters.MinimumWindowTrackWidthKey, 
			129 => SystemParameters.MinimumWindowTrackHeightKey, 
			130 => SystemParameters.PrimaryScreenWidthKey, 
			131 => SystemParameters.PrimaryScreenHeightKey, 
			132 => SystemParameters.WindowCaptionButtonWidthKey, 
			133 => SystemParameters.WindowCaptionButtonHeightKey, 
			134 => SystemParameters.ResizeFrameHorizontalBorderHeightKey, 
			135 => SystemParameters.ResizeFrameVerticalBorderWidthKey, 
			136 => SystemParameters.SmallIconWidthKey, 
			137 => SystemParameters.SmallIconHeightKey, 
			138 => SystemParameters.SmallWindowCaptionButtonWidthKey, 
			139 => SystemParameters.SmallWindowCaptionButtonHeightKey, 
			140 => SystemParameters.VirtualScreenWidthKey, 
			141 => SystemParameters.VirtualScreenHeightKey, 
			142 => SystemParameters.VerticalScrollBarWidthKey, 
			143 => SystemParameters.VerticalScrollBarButtonHeightKey, 
			144 => SystemParameters.WindowCaptionHeightKey, 
			145 => SystemParameters.KanjiWindowHeightKey, 
			146 => SystemParameters.MenuBarHeightKey, 
			147 => SystemParameters.SmallCaptionHeightKey, 
			148 => SystemParameters.VerticalScrollBarThumbHeightKey, 
			149 => SystemParameters.IsImmEnabledKey, 
			150 => SystemParameters.IsMediaCenterKey, 
			151 => SystemParameters.IsMenuDropRightAlignedKey, 
			152 => SystemParameters.IsMiddleEastEnabledKey, 
			153 => SystemParameters.IsMousePresentKey, 
			154 => SystemParameters.IsMouseWheelPresentKey, 
			155 => SystemParameters.IsPenWindowsKey, 
			156 => SystemParameters.IsRemotelyControlledKey, 
			157 => SystemParameters.IsRemoteSessionKey, 
			158 => SystemParameters.ShowSoundsKey, 
			159 => SystemParameters.IsSlowMachineKey, 
			160 => SystemParameters.SwapButtonsKey, 
			161 => SystemParameters.IsTabletPCKey, 
			162 => SystemParameters.VirtualScreenLeftKey, 
			163 => SystemParameters.VirtualScreenTopKey, 
			164 => SystemParameters.FocusBorderWidthKey, 
			165 => SystemParameters.FocusBorderHeightKey, 
			166 => SystemParameters.HighContrastKey, 
			167 => SystemParameters.DropShadowKey, 
			168 => SystemParameters.FlatMenuKey, 
			169 => SystemParameters.WorkAreaKey, 
			170 => SystemParameters.IconHorizontalSpacingKey, 
			171 => SystemParameters.IconVerticalSpacingKey, 
			172 => SystemParameters.IconTitleWrapKey, 
			88 => SystemFonts.IconFontSizeKey, 
			89 => SystemFonts.IconFontFamilyKey, 
			90 => SystemFonts.IconFontStyleKey, 
			91 => SystemFonts.IconFontWeightKey, 
			92 => SystemFonts.IconFontTextDecorationsKey, 
			173 => SystemParameters.KeyboardCuesKey, 
			174 => SystemParameters.KeyboardDelayKey, 
			175 => SystemParameters.KeyboardPreferenceKey, 
			176 => SystemParameters.KeyboardSpeedKey, 
			177 => SystemParameters.SnapToDefaultButtonKey, 
			178 => SystemParameters.WheelScrollLinesKey, 
			179 => SystemParameters.MouseHoverTimeKey, 
			180 => SystemParameters.MouseHoverHeightKey, 
			181 => SystemParameters.MouseHoverWidthKey, 
			182 => SystemParameters.MenuDropAlignmentKey, 
			183 => SystemParameters.MenuFadeKey, 
			184 => SystemParameters.MenuShowDelayKey, 
			185 => SystemParameters.ComboBoxAnimationKey, 
			186 => SystemParameters.ClientAreaAnimationKey, 
			187 => SystemParameters.CursorShadowKey, 
			188 => SystemParameters.GradientCaptionsKey, 
			189 => SystemParameters.HotTrackingKey, 
			190 => SystemParameters.ListBoxSmoothScrollingKey, 
			191 => SystemParameters.MenuAnimationKey, 
			192 => SystemParameters.SelectionFadeKey, 
			193 => SystemParameters.StylusHotTrackingKey, 
			194 => SystemParameters.ToolTipAnimationKey, 
			195 => SystemParameters.ToolTipFadeKey, 
			196 => SystemParameters.UIEffectsKey, 
			197 => SystemParameters.MinimizeAnimationKey, 
			198 => SystemParameters.BorderKey, 
			199 => SystemParameters.CaretWidthKey, 
			200 => SystemParameters.ForegroundFlashCountKey, 
			201 => SystemParameters.DragFullWindowsKey, 
			202 => SystemParameters.BorderWidthKey, 
			203 => SystemParameters.ScrollWidthKey, 
			204 => SystemParameters.ScrollHeightKey, 
			205 => SystemParameters.CaptionWidthKey, 
			206 => SystemParameters.CaptionHeightKey, 
			207 => SystemParameters.SmallCaptionWidthKey, 
			208 => SystemParameters.MenuWidthKey, 
			209 => SystemParameters.MenuHeightKey, 
			63 => SystemFonts.CaptionFontSizeKey, 
			64 => SystemFonts.CaptionFontFamilyKey, 
			65 => SystemFonts.CaptionFontStyleKey, 
			66 => SystemFonts.CaptionFontWeightKey, 
			67 => SystemFonts.CaptionFontTextDecorationsKey, 
			68 => SystemFonts.SmallCaptionFontSizeKey, 
			69 => SystemFonts.SmallCaptionFontFamilyKey, 
			70 => SystemFonts.SmallCaptionFontStyleKey, 
			71 => SystemFonts.SmallCaptionFontWeightKey, 
			72 => SystemFonts.SmallCaptionFontTextDecorationsKey, 
			73 => SystemFonts.MenuFontSizeKey, 
			74 => SystemFonts.MenuFontFamilyKey, 
			75 => SystemFonts.MenuFontStyleKey, 
			76 => SystemFonts.MenuFontWeightKey, 
			77 => SystemFonts.MenuFontTextDecorationsKey, 
			78 => SystemFonts.StatusFontSizeKey, 
			79 => SystemFonts.StatusFontFamilyKey, 
			80 => SystemFonts.StatusFontStyleKey, 
			81 => SystemFonts.StatusFontWeightKey, 
			82 => SystemFonts.StatusFontTextDecorationsKey, 
			83 => SystemFonts.MessageFontSizeKey, 
			84 => SystemFonts.MessageFontFamilyKey, 
			85 => SystemFonts.MessageFontStyleKey, 
			86 => SystemFonts.MessageFontWeightKey, 
			87 => SystemFonts.MessageFontTextDecorationsKey, 
			210 => SystemParameters.ComboBoxPopupAnimationKey, 
			211 => SystemParameters.MenuPopupAnimationKey, 
			212 => SystemParameters.ToolTipPopupAnimationKey, 
			215 => SystemParameters.FocusVisualStyleKey, 
			216 => SystemParameters.NavigationChromeDownLevelStyleKey, 
			217 => SystemParameters.NavigationChromeStyleKey, 
			219 => MenuItem.SeparatorStyleKey, 
			220 => GridView.GridViewScrollViewerStyleKey, 
			221 => GridView.GridViewStyleKey, 
			222 => GridView.GridViewItemContainerStyleKey, 
			223 => StatusBar.SeparatorStyleKey, 
			224 => ToolBar.ButtonStyleKey, 
			225 => ToolBar.ToggleButtonStyleKey, 
			226 => ToolBar.SeparatorStyleKey, 
			227 => ToolBar.CheckBoxStyleKey, 
			228 => ToolBar.RadioButtonStyleKey, 
			229 => ToolBar.ComboBoxStyleKey, 
			230 => ToolBar.TextBoxStyleKey, 
			231 => ToolBar.MenuStyleKey, 
			213 => SystemParameters.PowerLineStatusKey, 
			_ => null, 
		};
	}

	internal static ResourceKey GetSystemResourceKey(string keyName)
	{
		return keyName switch
		{
			"SystemParameters.FocusVisualStyleKey" => SystemParameters.FocusVisualStyleKey, 
			"ToolBar.ButtonStyleKey" => ToolBarButtonStyleKey, 
			"ToolBar.ToggleButtonStyleKey" => ToolBarToggleButtonStyleKey, 
			"ToolBar.CheckBoxStyleKey" => ToolBarCheckBoxStyleKey, 
			"ToolBar.RadioButtonStyleKey" => ToolBarRadioButtonStyleKey, 
			"ToolBar.ComboBoxStyleKey" => ToolBarComboBoxStyleKey, 
			"ToolBar.TextBoxStyleKey" => ToolBarTextBoxStyleKey, 
			"ToolBar.MenuStyleKey" => ToolBarMenuStyleKey, 
			"ToolBar.SeparatorStyleKey" => ToolBarSeparatorStyleKey, 
			"MenuItem.SeparatorStyleKey" => MenuItemSeparatorStyleKey, 
			"StatusBar.SeparatorStyleKey" => StatusBarSeparatorStyleKey, 
			"SystemParameters.NavigationChromeStyleKey" => SystemParameters.NavigationChromeStyleKey, 
			"SystemParameters.NavigationChromeDownLevelStyleKey" => SystemParameters.NavigationChromeDownLevelStyleKey, 
			"GridView.GridViewStyleKey" => GridViewStyleKey, 
			"GridView.GridViewScrollViewerStyleKey" => GridViewScrollViewerStyleKey, 
			"GridView.GridViewItemContainerStyleKey" => GridViewItemContainerStyleKey, 
			"DataGridColumnHeader.ColumnFloatingHeaderStyleKey" => DataGridColumnHeaderColumnFloatingHeaderStyleKey, 
			"DataGridColumnHeader.ColumnHeaderDropSeparatorStyleKey" => DataGridColumnHeaderColumnHeaderDropSeparatorStyleKey, 
			"DataGrid.FocusBorderBrushKey" => DataGridFocusBorderBrushKey, 
			"DataGridComboBoxColumn.TextBlockComboBoxStyleKey" => DataGridComboBoxColumnTextBlockComboBoxStyleKey, 
			_ => null, 
		};
	}

	internal static object GetResource(short id)
	{
		if (_srk == null)
		{
			_srk = new SystemResourceKey((SystemResourceKeyID)id);
		}
		else
		{
			_srk._id = (SystemResourceKeyID)id;
		}
		return _srk.Resource;
	}

	internal SystemResourceKey(SystemResourceKeyID id)
	{
		_id = id;
	}

	public override bool Equals(object o)
	{
		if (o is SystemResourceKey systemResourceKey)
		{
			return systemResourceKey._id == _id;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)_id;
	}

	public override string ToString()
	{
		return _id.ToString();
	}
}
