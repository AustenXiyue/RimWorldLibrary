using System.ComponentModel;
using System.Globalization;
using System.Xaml;

namespace System.Windows.Markup;

/// <summary>Converts the string name of an event setter handler to a delegate representation.</summary>
public sealed class EventSetterHandlerConverter : TypeConverter
{
	private static Type s_ServiceProviderContextType;

	/// <summary>Returns whether this converter can convert an object of one type to a <see cref="T:System.Delegate" />.</summary>
	/// <returns>true if this converter can perform the conversion; otherwise, false. </returns>
	/// <param name="typeDescriptorContext">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you want to convert from. </param>
	public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Returns whether this converter can convert the object to the specified type.</summary>
	/// <returns>Always returns false.</returns>
	/// <param name="typeDescriptorContext">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="destinationType">A <see cref="T:System.Type" /> that represents the type you want to convert to. </param>
	public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
	{
		return false;
	}

	/// <summary>Converts the specified string to a new <see cref="T:System.Delegate" /> for the event handler.</summary>
	/// <returns>A new <see cref="T:System.Delegate" /> that represents the referenced event handler.</returns>
	/// <param name="typeDescriptorContext">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="cultureInfo">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture. </param>
	/// <param name="source">The source string to convert.</param>
	/// <exception cref="T:System.NotSupportedException">The necessary services are not available.-or-Could not perform the specific conversion.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeDescriptorContext" /> or <paramref name="source" /> are null.</exception>
	public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
	{
		if (typeDescriptorContext == null)
		{
			throw new ArgumentNullException("typeDescriptorContext");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (s_ServiceProviderContextType == null)
		{
			s_ServiceProviderContextType = typeof(IRootObjectProvider).Assembly.GetType("MS.Internal.Xaml.ServiceProviderContext");
		}
		if (typeDescriptorContext.GetType() != s_ServiceProviderContextType)
		{
			throw new ArgumentException(SR.TextRange_InvalidParameterValue, "typeDescriptorContext");
		}
		if (typeDescriptorContext.GetService(typeof(IRootObjectProvider)) is IRootObjectProvider rootObjectProvider && source is string && typeDescriptorContext.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget { TargetObject: EventSetter targetObject } && source is string text)
		{
			string method = text.Trim();
			return Delegate.CreateDelegate(targetObject.Event.HandlerType, rootObjectProvider.RootObject, method);
		}
		throw GetConvertFromException(source);
	}

	/// <summary>Converts the specified value object to the specified type. Always throws an exception.</summary>
	/// <returns>Always throws an exception.</returns>
	/// <param name="typeDescriptorContext">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="cultureInfo">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture. </param>
	/// <param name="value">The value to convert.</param>
	/// <param name="destinationType">The type to convert the <paramref name="value" /> parameter to. </param>
	/// <exception cref="T:System.NotSupportedException">Thrown in all cases.</exception>
	public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)
	{
		throw GetConvertToException(value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.EventSetterHandlerConverter" /> class.</summary>
	public EventSetterHandlerConverter()
	{
	}
}
