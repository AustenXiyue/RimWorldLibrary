using System.ComponentModel;
using System.Globalization;
using MS.Internal;

namespace System.Windows.Documents;

internal class DPTypeDescriptorContext : ITypeDescriptorContext, IServiceProvider
{
	private DependencyProperty _property;

	private object _propertyValue;

	IContainer ITypeDescriptorContext.Container => null;

	object ITypeDescriptorContext.Instance => _propertyValue;

	PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor => null;

	private DPTypeDescriptorContext(DependencyProperty property, object propertyValue)
	{
		Invariant.Assert(property != null, "property == null");
		Invariant.Assert(propertyValue != null, "propertyValue == null");
		Invariant.Assert(property.IsValidValue(propertyValue), "propertyValue must be of suitable type for the given dependency property");
		_property = property;
		_propertyValue = propertyValue;
	}

	internal static string GetStringValue(DependencyProperty property, object propertyValue)
	{
		string text = null;
		if (property == UIElement.BitmapEffectProperty)
		{
			return null;
		}
		if (property == Inline.TextDecorationsProperty)
		{
			text = TextDecorationsFixup((TextDecorationCollection)propertyValue);
		}
		else if (typeof(CultureInfo).IsAssignableFrom(property.PropertyType))
		{
			text = CultureInfoFixup(property, (CultureInfo)propertyValue);
		}
		if (text == null)
		{
			DPTypeDescriptorContext context = new DPTypeDescriptorContext(property, propertyValue);
			TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);
			Invariant.Assert(converter != null);
			if (converter.CanConvertTo(context, typeof(string)))
			{
				text = (string)converter.ConvertTo(context, CultureInfo.InvariantCulture, propertyValue, typeof(string));
			}
		}
		return text;
	}

	private static string TextDecorationsFixup(TextDecorationCollection textDecorations)
	{
		string result = null;
		if (TextDecorations.Underline.ValueEquals(textDecorations))
		{
			result = "Underline";
		}
		else if (TextDecorations.Strikethrough.ValueEquals(textDecorations))
		{
			result = "Strikethrough";
		}
		else if (TextDecorations.OverLine.ValueEquals(textDecorations))
		{
			result = "OverLine";
		}
		else if (TextDecorations.Baseline.ValueEquals(textDecorations))
		{
			result = "Baseline";
		}
		else if (textDecorations.Count == 0)
		{
			result = string.Empty;
		}
		return result;
	}

	private static string CultureInfoFixup(DependencyProperty property, CultureInfo cultureInfo)
	{
		string result = null;
		DPTypeDescriptorContext context = new DPTypeDescriptorContext(property, cultureInfo);
		TypeConverter typeConverter = new CultureInfoIetfLanguageTagConverter();
		if (typeConverter.CanConvertTo(context, typeof(string)))
		{
			result = (string)typeConverter.ConvertTo(context, CultureInfo.InvariantCulture, cultureInfo, typeof(string));
		}
		return result;
	}

	void ITypeDescriptorContext.OnComponentChanged()
	{
	}

	bool ITypeDescriptorContext.OnComponentChanging()
	{
		return false;
	}

	object IServiceProvider.GetService(Type serviceType)
	{
		return null;
	}
}
