using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace System.Windows.Markup;

internal class SystemKeyConverter : TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == null)
		{
			throw new ArgumentNullException("sourceType");
		}
		return base.CanConvertFrom(context, sourceType);
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (destinationType == typeof(MarkupExtension) && context is IValueSerializerContext)
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (destinationType == typeof(MarkupExtension) && CanConvertTo(context, destinationType))
		{
			SystemResourceKeyID internalKey;
			if (value is SystemResourceKey)
			{
				internalKey = (value as SystemResourceKey).InternalKey;
			}
			else
			{
				if (!(value is SystemThemeKey))
				{
					throw new ArgumentException(SR.Format(SR.MustBeOfType, "value", "SystemResourceKey or SystemThemeKey"));
				}
				internalKey = (value as SystemThemeKey).InternalKey;
			}
			Type systemClassType = GetSystemClassType(internalKey);
			if (context is IValueSerializerContext valueSerializerContext)
			{
				ValueSerializer valueSerializerFor = valueSerializerContext.GetValueSerializerFor(typeof(Type));
				if (valueSerializerFor != null)
				{
					return new StaticExtension(valueSerializerFor.ConvertToString(systemClassType, valueSerializerContext) + "." + GetSystemKeyName(internalKey));
				}
			}
		}
		return base.CanConvertTo(context, destinationType);
	}

	internal static Type GetSystemClassType(SystemResourceKeyID id)
	{
		if ((SystemResourceKeyID.InternalSystemColorsStart < id && id < SystemResourceKeyID.InternalSystemColorsEnd) || (SystemResourceKeyID.InternalSystemColorsExtendedStart < id && id < SystemResourceKeyID.InternalSystemColorsExtendedEnd))
		{
			return typeof(SystemColors);
		}
		if (SystemResourceKeyID.InternalSystemFontsStart < id && id < SystemResourceKeyID.InternalSystemFontsEnd)
		{
			return typeof(SystemFonts);
		}
		if (SystemResourceKeyID.InternalSystemParametersStart < id && id < SystemResourceKeyID.InternalSystemParametersEnd)
		{
			return typeof(SystemParameters);
		}
		if (SystemResourceKeyID.MenuItemSeparatorStyle == id)
		{
			return typeof(MenuItem);
		}
		if (SystemResourceKeyID.ToolBarButtonStyle <= id && id <= SystemResourceKeyID.ToolBarMenuStyle)
		{
			return typeof(ToolBar);
		}
		if (SystemResourceKeyID.StatusBarSeparatorStyle == id)
		{
			return typeof(StatusBar);
		}
		if (SystemResourceKeyID.GridViewScrollViewerStyle <= id && id <= SystemResourceKeyID.GridViewItemContainerStyle)
		{
			return typeof(GridView);
		}
		return null;
	}

	internal static string GetSystemClassName(SystemResourceKeyID id)
	{
		if ((SystemResourceKeyID.InternalSystemColorsStart < id && id < SystemResourceKeyID.InternalSystemColorsEnd) || (SystemResourceKeyID.InternalSystemColorsExtendedStart < id && id < SystemResourceKeyID.InternalSystemColorsExtendedEnd))
		{
			return "SystemColors";
		}
		if (SystemResourceKeyID.InternalSystemFontsStart < id && id < SystemResourceKeyID.InternalSystemFontsEnd)
		{
			return "SystemFonts";
		}
		if (SystemResourceKeyID.InternalSystemParametersStart < id && id < SystemResourceKeyID.InternalSystemParametersEnd)
		{
			return "SystemParameters";
		}
		if (SystemResourceKeyID.MenuItemSeparatorStyle == id)
		{
			return "MenuItem";
		}
		if (SystemResourceKeyID.ToolBarButtonStyle <= id && id <= SystemResourceKeyID.ToolBarMenuStyle)
		{
			return "ToolBar";
		}
		if (SystemResourceKeyID.StatusBarSeparatorStyle == id)
		{
			return "StatusBar";
		}
		if (SystemResourceKeyID.GridViewScrollViewerStyle <= id && id <= SystemResourceKeyID.GridViewItemContainerStyle)
		{
			return "GridView";
		}
		return string.Empty;
	}

	internal static string GetSystemKeyName(SystemResourceKeyID id)
	{
		if ((SystemResourceKeyID.InternalSystemColorsStart < id && id < SystemResourceKeyID.InternalSystemParametersEnd) || (SystemResourceKeyID.InternalSystemColorsExtendedStart < id && id < SystemResourceKeyID.InternalSystemColorsExtendedEnd) || (SystemResourceKeyID.GridViewScrollViewerStyle <= id && id <= SystemResourceKeyID.GridViewItemContainerStyle))
		{
			return Enum.GetName(typeof(SystemResourceKeyID), id) + "Key";
		}
		if (SystemResourceKeyID.MenuItemSeparatorStyle == id || SystemResourceKeyID.StatusBarSeparatorStyle == id)
		{
			return "SeparatorStyleKey";
		}
		if (SystemResourceKeyID.ToolBarButtonStyle <= id && id <= SystemResourceKeyID.ToolBarMenuStyle)
		{
			return (Enum.GetName(typeof(SystemResourceKeyID), id) + "Key").Remove(0, 7);
		}
		return string.Empty;
	}

	internal static string GetSystemPropertyName(SystemResourceKeyID id)
	{
		if (SystemResourceKeyID.InternalSystemColorsStart < id && id < SystemResourceKeyID.InternalSystemColorsExtendedEnd)
		{
			return Enum.GetName(typeof(SystemResourceKeyID), id);
		}
		return string.Empty;
	}
}
