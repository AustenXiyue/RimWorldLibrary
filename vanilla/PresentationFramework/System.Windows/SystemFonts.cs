using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace System.Windows;

/// <summary>Contains properties that expose the system resources that concern fonts. </summary>
public static class SystemFonts
{
	private const double FallbackFontSize = 11.0;

	private static TextDecorationCollection _iconFontTextDecorations;

	private static TextDecorationCollection _messageFontTextDecorations;

	private static TextDecorationCollection _statusFontTextDecorations;

	private static TextDecorationCollection _menuFontTextDecorations;

	private static TextDecorationCollection _smallCaptionFontTextDecorations;

	private static TextDecorationCollection _captionFontTextDecorations;

	private static FontFamily _iconFontFamily;

	private static FontFamily _messageFontFamily;

	private static FontFamily _statusFontFamily;

	private static FontFamily _menuFontFamily;

	private static FontFamily _smallCaptionFontFamily;

	private static FontFamily _captionFontFamily;

	private static SystemResourceKey _cacheIconFontSize;

	private static SystemResourceKey _cacheIconFontFamily;

	private static SystemResourceKey _cacheIconFontStyle;

	private static SystemResourceKey _cacheIconFontWeight;

	private static SystemResourceKey _cacheIconFontTextDecorations;

	private static SystemResourceKey _cacheCaptionFontSize;

	private static SystemResourceKey _cacheCaptionFontFamily;

	private static SystemResourceKey _cacheCaptionFontStyle;

	private static SystemResourceKey _cacheCaptionFontWeight;

	private static SystemResourceKey _cacheCaptionFontTextDecorations;

	private static SystemResourceKey _cacheSmallCaptionFontSize;

	private static SystemResourceKey _cacheSmallCaptionFontFamily;

	private static SystemResourceKey _cacheSmallCaptionFontStyle;

	private static SystemResourceKey _cacheSmallCaptionFontWeight;

	private static SystemResourceKey _cacheSmallCaptionFontTextDecorations;

	private static SystemResourceKey _cacheMenuFontSize;

	private static SystemResourceKey _cacheMenuFontFamily;

	private static SystemResourceKey _cacheMenuFontStyle;

	private static SystemResourceKey _cacheMenuFontWeight;

	private static SystemResourceKey _cacheMenuFontTextDecorations;

	private static SystemResourceKey _cacheStatusFontSize;

	private static SystemResourceKey _cacheStatusFontFamily;

	private static SystemResourceKey _cacheStatusFontStyle;

	private static SystemResourceKey _cacheStatusFontWeight;

	private static SystemResourceKey _cacheStatusFontTextDecorations;

	private static SystemResourceKey _cacheMessageFontSize;

	private static SystemResourceKey _cacheMessageFontFamily;

	private static SystemResourceKey _cacheMessageFontStyle;

	private static SystemResourceKey _cacheMessageFontWeight;

	private static SystemResourceKey _cacheMessageFontTextDecorations;

	/// <summary>Gets the font size from the logical font information for the current icon-title font. </summary>
	/// <returns>A font size.</returns>
	public static double IconFontSize => ConvertFontHeight(SystemParameters.IconMetrics.lfFont.lfHeight);

	/// <summary>Gets the font family from the logical font information for the current icon-title font.  </summary>
	/// <returns>A font family.</returns>
	public static FontFamily IconFontFamily
	{
		get
		{
			if (_iconFontFamily == null)
			{
				_iconFontFamily = new FontFamily(SystemParameters.IconMetrics.lfFont.lfFaceName);
			}
			return _iconFontFamily;
		}
	}

	/// <summary>Gets the font style from the logical font information for the current icon-title font. </summary>
	/// <returns>A font style.</returns>
	public static FontStyle IconFontStyle
	{
		get
		{
			if (SystemParameters.IconMetrics.lfFont.lfItalic == 0)
			{
				return FontStyles.Normal;
			}
			return FontStyles.Italic;
		}
	}

	/// <summary>Gets the font weight from the logical font information for the current icon-title font. </summary>
	/// <returns>A font weight.</returns>
	public static FontWeight IconFontWeight => FontWeight.FromOpenTypeWeight(SystemParameters.IconMetrics.lfFont.lfWeight);

	/// <summary>Gets the text decorations from the logical font information for the current icon-title font. </summary>
	/// <returns>A collection of text decorations.</returns>
	public static TextDecorationCollection IconFontTextDecorations
	{
		get
		{
			if (_iconFontTextDecorations == null)
			{
				_iconFontTextDecorations = new TextDecorationCollection();
				if (SystemParameters.IconMetrics.lfFont.lfUnderline != 0)
				{
					CopyTextDecorationCollection(TextDecorations.Underline, _iconFontTextDecorations);
				}
				if (SystemParameters.IconMetrics.lfFont.lfStrikeOut != 0)
				{
					CopyTextDecorationCollection(TextDecorations.Strikethrough, _iconFontTextDecorations);
				}
				_iconFontTextDecorations.Freeze();
			}
			return _iconFontTextDecorations;
		}
	}

	/// <summary>Gets the metric that determines the caption font-size for the nonclient area of a nonminimized window. </summary>
	/// <returns>A font size.</returns>
	public static double CaptionFontSize => ConvertFontHeight(SystemParameters.NonClientMetrics.lfCaptionFont.lfHeight);

	/// <summary>Gets the metric that determines the font family of the caption of the nonclient area of a nonminimized window. </summary>
	/// <returns>A font family.</returns>
	public static FontFamily CaptionFontFamily
	{
		get
		{
			if (_captionFontFamily == null)
			{
				_captionFontFamily = new FontFamily(SystemParameters.NonClientMetrics.lfCaptionFont.lfFaceName);
			}
			return _captionFontFamily;
		}
	}

	/// <summary>Gets the metric that determines the caption font-style for the nonclient area of a nonminimized window. </summary>
	/// <returns>A font style.</returns>
	public static FontStyle CaptionFontStyle
	{
		get
		{
			if (SystemParameters.NonClientMetrics.lfCaptionFont.lfItalic == 0)
			{
				return FontStyles.Normal;
			}
			return FontStyles.Italic;
		}
	}

	/// <summary>Gets the metric that determines the caption font-weight for the nonclient area of a nonminimized window. </summary>
	/// <returns>A font weight.</returns>
	public static FontWeight CaptionFontWeight => FontWeight.FromOpenTypeWeight(SystemParameters.NonClientMetrics.lfCaptionFont.lfWeight);

	/// <summary>Gets the metric that determines the caption text-decorations for the nonclient area of a nonminimized window. </summary>
	/// <returns>A collection of text decorations.</returns>
	public static TextDecorationCollection CaptionFontTextDecorations
	{
		get
		{
			if (_captionFontTextDecorations == null)
			{
				_captionFontTextDecorations = new TextDecorationCollection();
				if (SystemParameters.NonClientMetrics.lfCaptionFont.lfUnderline != 0)
				{
					CopyTextDecorationCollection(TextDecorations.Underline, _captionFontTextDecorations);
				}
				if (SystemParameters.NonClientMetrics.lfCaptionFont.lfStrikeOut != 0)
				{
					CopyTextDecorationCollection(TextDecorations.Strikethrough, _captionFontTextDecorations);
				}
				_captionFontTextDecorations.Freeze();
			}
			return _captionFontTextDecorations;
		}
	}

	/// <summary>Gets the metric that determines the font size of the small-caption text for the nonclient area of a nonminimized window. </summary>
	/// <returns>A font size.</returns>
	public static double SmallCaptionFontSize => ConvertFontHeight(SystemParameters.NonClientMetrics.lfSmCaptionFont.lfHeight);

	/// <summary>Gets the metric that determines the font family of the small-caption text for the nonclient area of a nonminimized window. </summary>
	/// <returns>A font family.</returns>
	public static FontFamily SmallCaptionFontFamily
	{
		get
		{
			if (_smallCaptionFontFamily == null)
			{
				_smallCaptionFontFamily = new FontFamily(SystemParameters.NonClientMetrics.lfSmCaptionFont.lfFaceName);
			}
			return _smallCaptionFontFamily;
		}
	}

	/// <summary>Gets the metric that determines the font style of the small-caption text for the nonclient area of a nonminimized window. </summary>
	/// <returns>A font style.</returns>
	public static FontStyle SmallCaptionFontStyle
	{
		get
		{
			if (SystemParameters.NonClientMetrics.lfSmCaptionFont.lfItalic == 0)
			{
				return FontStyles.Normal;
			}
			return FontStyles.Italic;
		}
	}

	/// <summary>Gets the metric that determines the font weight of the small-caption text for the nonclient area of a nonminimized window. </summary>
	/// <returns>A font weight.</returns>
	public static FontWeight SmallCaptionFontWeight => FontWeight.FromOpenTypeWeight(SystemParameters.NonClientMetrics.lfSmCaptionFont.lfWeight);

	/// <summary>Gets the metric that determines the decorations of the small-caption text for the nonclient area of a nonminimized window. </summary>
	/// <returns>A collection of text decorations.</returns>
	public static TextDecorationCollection SmallCaptionFontTextDecorations
	{
		get
		{
			if (_smallCaptionFontTextDecorations == null)
			{
				_smallCaptionFontTextDecorations = new TextDecorationCollection();
				if (SystemParameters.NonClientMetrics.lfSmCaptionFont.lfUnderline != 0)
				{
					CopyTextDecorationCollection(TextDecorations.Underline, _smallCaptionFontTextDecorations);
				}
				if (SystemParameters.NonClientMetrics.lfSmCaptionFont.lfStrikeOut != 0)
				{
					CopyTextDecorationCollection(TextDecorations.Strikethrough, _smallCaptionFontTextDecorations);
				}
				_smallCaptionFontTextDecorations.Freeze();
			}
			return _smallCaptionFontTextDecorations;
		}
	}

	/// <summary>Gets the metric that determines the font size of menu text. </summary>
	/// <returns>A font size.</returns>
	public static double MenuFontSize => ConvertFontHeight(SystemParameters.NonClientMetrics.lfMenuFont.lfHeight);

	/// <summary>Gets the metric that determines the font family for menu text. </summary>
	/// <returns>A font family.</returns>
	public static FontFamily MenuFontFamily
	{
		get
		{
			if (_menuFontFamily == null)
			{
				_menuFontFamily = new FontFamily(SystemParameters.NonClientMetrics.lfMenuFont.lfFaceName);
			}
			return _menuFontFamily;
		}
	}

	/// <summary>Gets the metric that determines the font style for menu text. </summary>
	/// <returns>A font style.</returns>
	public static FontStyle MenuFontStyle
	{
		get
		{
			if (SystemParameters.NonClientMetrics.lfMenuFont.lfItalic == 0)
			{
				return FontStyles.Normal;
			}
			return FontStyles.Italic;
		}
	}

	/// <summary>Gets the metric that determines the font weight for menu text. </summary>
	/// <returns>A font weight.</returns>
	public static FontWeight MenuFontWeight => FontWeight.FromOpenTypeWeight(SystemParameters.NonClientMetrics.lfMenuFont.lfWeight);

	/// <summary>Gets the metric that determines the text decorations for menu text. </summary>
	/// <returns>A collection of text decorations.</returns>
	public static TextDecorationCollection MenuFontTextDecorations
	{
		get
		{
			if (_menuFontTextDecorations == null)
			{
				_menuFontTextDecorations = new TextDecorationCollection();
				if (SystemParameters.NonClientMetrics.lfMenuFont.lfUnderline != 0)
				{
					CopyTextDecorationCollection(TextDecorations.Underline, _menuFontTextDecorations);
				}
				if (SystemParameters.NonClientMetrics.lfMenuFont.lfStrikeOut != 0)
				{
					CopyTextDecorationCollection(TextDecorations.Strikethrough, _menuFontTextDecorations);
				}
				_menuFontTextDecorations.Freeze();
			}
			return _menuFontTextDecorations;
		}
	}

	/// <summary>Gets the metric that determines the font size of the text used in status bars and ToolTips for the nonclient area of a nonminimized window. </summary>
	/// <returns>A font size.</returns>
	public static double StatusFontSize => ConvertFontHeight(SystemParameters.NonClientMetrics.lfStatusFont.lfHeight);

	/// <summary>Gets the metric that determines the font family of the text used in status bars and ToolTips for the nonclient area of a nonminimized window. </summary>
	/// <returns>A font family.</returns>
	public static FontFamily StatusFontFamily
	{
		get
		{
			if (_statusFontFamily == null)
			{
				_statusFontFamily = new FontFamily(SystemParameters.NonClientMetrics.lfStatusFont.lfFaceName);
			}
			return _statusFontFamily;
		}
	}

	/// <summary>Gets the metric that determines the font style of the text used in status bars and ToolTips for the nonclient area of a nonminimized window. </summary>
	/// <returns>A font style.</returns>
	public static FontStyle StatusFontStyle
	{
		get
		{
			if (SystemParameters.NonClientMetrics.lfStatusFont.lfItalic == 0)
			{
				return FontStyles.Normal;
			}
			return FontStyles.Italic;
		}
	}

	/// <summary>Gets the metric that determines the font weight of the text used in status bars and ToolTips for the nonclient area of a nonminimized window. </summary>
	/// <returns>A font weight.</returns>
	public static FontWeight StatusFontWeight => FontWeight.FromOpenTypeWeight(SystemParameters.NonClientMetrics.lfStatusFont.lfWeight);

	/// <summary>Gets the metric that determines the decorations of the text used in status bars and ToolTips for the nonclient area of a nonminimized window. </summary>
	/// <returns>A collection of text decoration.</returns>
	public static TextDecorationCollection StatusFontTextDecorations
	{
		get
		{
			if (_statusFontTextDecorations == null)
			{
				_statusFontTextDecorations = new TextDecorationCollection();
				if (SystemParameters.NonClientMetrics.lfStatusFont.lfUnderline != 0)
				{
					CopyTextDecorationCollection(TextDecorations.Underline, _statusFontTextDecorations);
				}
				if (SystemParameters.NonClientMetrics.lfStatusFont.lfStrikeOut != 0)
				{
					CopyTextDecorationCollection(TextDecorations.Strikethrough, _statusFontTextDecorations);
				}
				_statusFontTextDecorations.Freeze();
			}
			return _statusFontTextDecorations;
		}
	}

	/// <summary>Gets the metric that determines the font size of message box text. </summary>
	/// <returns>A font size.</returns>
	public static double MessageFontSize => ConvertFontHeight(SystemParameters.NonClientMetrics.lfMessageFont.lfHeight);

	/// <summary>Gets the metric that determines the font family for message box text. </summary>
	/// <returns>A font family.</returns>
	public static FontFamily MessageFontFamily
	{
		get
		{
			if (_messageFontFamily == null)
			{
				_messageFontFamily = new FontFamily(SystemParameters.NonClientMetrics.lfMessageFont.lfFaceName);
			}
			return _messageFontFamily;
		}
	}

	/// <summary>Gets the metric that determines the font style for message box text. </summary>
	/// <returns>A font style.</returns>
	public static FontStyle MessageFontStyle
	{
		get
		{
			if (SystemParameters.NonClientMetrics.lfMessageFont.lfItalic == 0)
			{
				return FontStyles.Normal;
			}
			return FontStyles.Italic;
		}
	}

	/// <summary>Gets the metric that determines the font weight for message box text. </summary>
	/// <returns>A font weight.</returns>
	public static FontWeight MessageFontWeight => FontWeight.FromOpenTypeWeight(SystemParameters.NonClientMetrics.lfMessageFont.lfWeight);

	/// <summary>Gets the metric that determines the decorations for message box text. </summary>
	/// <returns>A collection of text decorations.</returns>
	public static TextDecorationCollection MessageFontTextDecorations
	{
		get
		{
			if (_messageFontTextDecorations == null)
			{
				_messageFontTextDecorations = new TextDecorationCollection();
				if (SystemParameters.NonClientMetrics.lfMessageFont.lfUnderline != 0)
				{
					CopyTextDecorationCollection(TextDecorations.Underline, _messageFontTextDecorations);
				}
				if (SystemParameters.NonClientMetrics.lfMessageFont.lfStrikeOut != 0)
				{
					CopyTextDecorationCollection(TextDecorations.Strikethrough, _messageFontTextDecorations);
				}
				_messageFontTextDecorations.Freeze();
			}
			return _messageFontTextDecorations;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.IconFontSize" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IconFontSizeKey
	{
		get
		{
			if (_cacheIconFontSize == null)
			{
				_cacheIconFontSize = CreateInstance(SystemResourceKeyID.IconFontSize);
			}
			return _cacheIconFontSize;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.IconFontFamily" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IconFontFamilyKey
	{
		get
		{
			if (_cacheIconFontFamily == null)
			{
				_cacheIconFontFamily = CreateInstance(SystemResourceKeyID.IconFontFamily);
			}
			return _cacheIconFontFamily;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.IconFontStyle" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IconFontStyleKey
	{
		get
		{
			if (_cacheIconFontStyle == null)
			{
				_cacheIconFontStyle = CreateInstance(SystemResourceKeyID.IconFontStyle);
			}
			return _cacheIconFontStyle;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.IconFontWeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IconFontWeightKey
	{
		get
		{
			if (_cacheIconFontWeight == null)
			{
				_cacheIconFontWeight = CreateInstance(SystemResourceKeyID.IconFontWeight);
			}
			return _cacheIconFontWeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.IconFontTextDecorations" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey IconFontTextDecorationsKey
	{
		get
		{
			if (_cacheIconFontTextDecorations == null)
			{
				_cacheIconFontTextDecorations = CreateInstance(SystemResourceKeyID.IconFontTextDecorations);
			}
			return _cacheIconFontTextDecorations;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.CaptionFontSize" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey CaptionFontSizeKey
	{
		get
		{
			if (_cacheCaptionFontSize == null)
			{
				_cacheCaptionFontSize = CreateInstance(SystemResourceKeyID.CaptionFontSize);
			}
			return _cacheCaptionFontSize;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.CaptionFontFamily" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey CaptionFontFamilyKey
	{
		get
		{
			if (_cacheCaptionFontFamily == null)
			{
				_cacheCaptionFontFamily = CreateInstance(SystemResourceKeyID.CaptionFontFamily);
			}
			return _cacheCaptionFontFamily;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.CaptionFontStyle" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey CaptionFontStyleKey
	{
		get
		{
			if (_cacheCaptionFontStyle == null)
			{
				_cacheCaptionFontStyle = CreateInstance(SystemResourceKeyID.CaptionFontStyle);
			}
			return _cacheCaptionFontStyle;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.CaptionFontWeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey CaptionFontWeightKey
	{
		get
		{
			if (_cacheCaptionFontWeight == null)
			{
				_cacheCaptionFontWeight = CreateInstance(SystemResourceKeyID.CaptionFontWeight);
			}
			return _cacheCaptionFontWeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.CaptionFontTextDecorations" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey CaptionFontTextDecorationsKey
	{
		get
		{
			if (_cacheCaptionFontTextDecorations == null)
			{
				_cacheCaptionFontTextDecorations = CreateInstance(SystemResourceKeyID.CaptionFontTextDecorations);
			}
			return _cacheCaptionFontTextDecorations;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.SmallCaptionFontSize" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey SmallCaptionFontSizeKey
	{
		get
		{
			if (_cacheSmallCaptionFontSize == null)
			{
				_cacheSmallCaptionFontSize = CreateInstance(SystemResourceKeyID.SmallCaptionFontSize);
			}
			return _cacheSmallCaptionFontSize;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.SmallCaptionFontFamily" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey SmallCaptionFontFamilyKey
	{
		get
		{
			if (_cacheSmallCaptionFontFamily == null)
			{
				_cacheSmallCaptionFontFamily = CreateInstance(SystemResourceKeyID.SmallCaptionFontFamily);
			}
			return _cacheSmallCaptionFontFamily;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.SmallCaptionFontStyle" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey SmallCaptionFontStyleKey
	{
		get
		{
			if (_cacheSmallCaptionFontStyle == null)
			{
				_cacheSmallCaptionFontStyle = CreateInstance(SystemResourceKeyID.SmallCaptionFontStyle);
			}
			return _cacheSmallCaptionFontStyle;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.SmallCaptionFontWeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey SmallCaptionFontWeightKey
	{
		get
		{
			if (_cacheSmallCaptionFontWeight == null)
			{
				_cacheSmallCaptionFontWeight = CreateInstance(SystemResourceKeyID.SmallCaptionFontWeight);
			}
			return _cacheSmallCaptionFontWeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.SmallCaptionFontTextDecorations" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey SmallCaptionFontTextDecorationsKey
	{
		get
		{
			if (_cacheSmallCaptionFontTextDecorations == null)
			{
				_cacheSmallCaptionFontTextDecorations = CreateInstance(SystemResourceKeyID.SmallCaptionFontTextDecorations);
			}
			return _cacheSmallCaptionFontTextDecorations;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.MenuFontSize" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MenuFontSizeKey
	{
		get
		{
			if (_cacheMenuFontSize == null)
			{
				_cacheMenuFontSize = CreateInstance(SystemResourceKeyID.MenuFontSize);
			}
			return _cacheMenuFontSize;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.MenuFontFamily" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MenuFontFamilyKey
	{
		get
		{
			if (_cacheMenuFontFamily == null)
			{
				_cacheMenuFontFamily = CreateInstance(SystemResourceKeyID.MenuFontFamily);
			}
			return _cacheMenuFontFamily;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.MenuFontStyle" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MenuFontStyleKey
	{
		get
		{
			if (_cacheMenuFontStyle == null)
			{
				_cacheMenuFontStyle = CreateInstance(SystemResourceKeyID.MenuFontStyle);
			}
			return _cacheMenuFontStyle;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.MenuFontWeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MenuFontWeightKey
	{
		get
		{
			if (_cacheMenuFontWeight == null)
			{
				_cacheMenuFontWeight = CreateInstance(SystemResourceKeyID.MenuFontWeight);
			}
			return _cacheMenuFontWeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.MenuFontTextDecorations" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MenuFontTextDecorationsKey
	{
		get
		{
			if (_cacheMenuFontTextDecorations == null)
			{
				_cacheMenuFontTextDecorations = CreateInstance(SystemResourceKeyID.MenuFontTextDecorations);
			}
			return _cacheMenuFontTextDecorations;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.StatusFontSize" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey StatusFontSizeKey
	{
		get
		{
			if (_cacheStatusFontSize == null)
			{
				_cacheStatusFontSize = CreateInstance(SystemResourceKeyID.StatusFontSize);
			}
			return _cacheStatusFontSize;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.StatusFontFamily" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey StatusFontFamilyKey
	{
		get
		{
			if (_cacheStatusFontFamily == null)
			{
				_cacheStatusFontFamily = CreateInstance(SystemResourceKeyID.StatusFontFamily);
			}
			return _cacheStatusFontFamily;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.StatusFontStyle" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey StatusFontStyleKey
	{
		get
		{
			if (_cacheStatusFontStyle == null)
			{
				_cacheStatusFontStyle = CreateInstance(SystemResourceKeyID.StatusFontStyle);
			}
			return _cacheStatusFontStyle;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.StatusFontWeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey StatusFontWeightKey
	{
		get
		{
			if (_cacheStatusFontWeight == null)
			{
				_cacheStatusFontWeight = CreateInstance(SystemResourceKeyID.StatusFontWeight);
			}
			return _cacheStatusFontWeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.StatusFontTextDecorations" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey StatusFontTextDecorationsKey
	{
		get
		{
			if (_cacheStatusFontTextDecorations == null)
			{
				_cacheStatusFontTextDecorations = CreateInstance(SystemResourceKeyID.StatusFontTextDecorations);
			}
			return _cacheStatusFontTextDecorations;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.MessageFontSize" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MessageFontSizeKey
	{
		get
		{
			if (_cacheMessageFontSize == null)
			{
				_cacheMessageFontSize = CreateInstance(SystemResourceKeyID.MessageFontSize);
			}
			return _cacheMessageFontSize;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.MessageFontFamily" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MessageFontFamilyKey
	{
		get
		{
			if (_cacheMessageFontFamily == null)
			{
				_cacheMessageFontFamily = CreateInstance(SystemResourceKeyID.MessageFontFamily);
			}
			return _cacheMessageFontFamily;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.MessageFontStyle" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MessageFontStyleKey
	{
		get
		{
			if (_cacheMessageFontStyle == null)
			{
				_cacheMessageFontStyle = CreateInstance(SystemResourceKeyID.MessageFontStyle);
			}
			return _cacheMessageFontStyle;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.MessageFontWeight" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MessageFontWeightKey
	{
		get
		{
			if (_cacheMessageFontWeight == null)
			{
				_cacheMessageFontWeight = CreateInstance(SystemResourceKeyID.MessageFontWeight);
			}
			return _cacheMessageFontWeight;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.ResourceKey" /> for the <see cref="P:System.Windows.SystemFonts.MessageFontTextDecorations" /> property. </summary>
	/// <returns>A resource key.</returns>
	public static ResourceKey MessageFontTextDecorationsKey
	{
		get
		{
			if (_cacheMessageFontTextDecorations == null)
			{
				_cacheMessageFontTextDecorations = CreateInstance(SystemResourceKeyID.MessageFontTextDecorations);
			}
			return _cacheMessageFontTextDecorations;
		}
	}

	private static void CopyTextDecorationCollection(TextDecorationCollection from, TextDecorationCollection to)
	{
		int count = from.Count;
		for (int i = 0; i < count; i++)
		{
			to.Add(from[i]);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static SystemResourceKey CreateInstance(SystemResourceKeyID KeyId)
	{
		return new SystemResourceKey(KeyId);
	}

	private static double ConvertFontHeight(int height)
	{
		int dpi = SystemParameters.Dpi;
		if (dpi != 0)
		{
			return Math.Abs(height) * 96 / dpi;
		}
		return 11.0;
	}

	internal static void InvalidateIconMetrics()
	{
		_iconFontTextDecorations = null;
		_iconFontFamily = null;
	}

	internal static void InvalidateNonClientMetrics()
	{
		_messageFontTextDecorations = null;
		_statusFontTextDecorations = null;
		_menuFontTextDecorations = null;
		_smallCaptionFontTextDecorations = null;
		_captionFontTextDecorations = null;
		_messageFontFamily = null;
		_statusFontFamily = null;
		_menuFontFamily = null;
		_smallCaptionFontFamily = null;
		_captionFontFamily = null;
	}
}
