using System.ComponentModel;
using System.Globalization;
using System.Xaml;

namespace System.Windows.Markup;

/// <summary>Provides data for callbacks that are invoked when a XAML writer sets a value using a type converter call.</summary>
public class XamlSetTypeConverterEventArgs : XamlSetValueEventArgs
{
	/// <summary>Gets the <see cref="T:System.ComponentModel.TypeConverter" /> instance that is invoked and provides type conversion behavior.</summary>
	/// <returns>The type converter that provides type conversion behavior.</returns>
	public TypeConverter TypeConverter { get; private set; }

	/// <summary>Gets <see cref="T:System.IServiceProvider" /> information that can be used by the type converter class.</summary>
	/// <returns>Service provider information that can be used by the <paramref name="typeConverter" /> class.</returns>
	public ITypeDescriptorContext ServiceProvider { get; private set; }

	/// <summary>Gets <see cref="T:System.Globalization.CultureInfo" /> information that can be used by the type converter class when calling <see cref="M:System.ComponentModel.TypeConverter.ConvertFrom(System.ComponentModel.ITypeDescriptorContext,System.Globalization.CultureInfo,System.Object)" /> and other methods.</summary>
	/// <returns>Culture information that can be used by the type converter class </returns>
	public CultureInfo CultureInfo { get; private set; }

	internal object TargetObject { get; private set; }

	internal XamlType CurrentType { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XamlSetTypeConverterEventArgs" /> class. </summary>
	/// <param name="member">XAML type system / schema information for the member being set.</param>
	/// <param name="typeConverter">The specific type converter instance being invoked.</param>
	/// <param name="value">The value to provide for the member being set.</param>
	/// <param name="serviceProvider">Service provider information that can be used by the <paramref name="typeConverter" /> class.</param>
	/// <param name="cultureInfo">Culture information that can be used by the <paramref name="typeConverter" /> class when calling <see cref="M:System.ComponentModel.TypeConverter.ConvertFrom(System.ComponentModel.ITypeDescriptorContext,System.Globalization.CultureInfo,System.Object)" /> and other methods.</param>
	public XamlSetTypeConverterEventArgs(XamlMember member, TypeConverter typeConverter, object value, ITypeDescriptorContext serviceProvider, CultureInfo cultureInfo)
		: base(member, value)
	{
		TypeConverter = typeConverter;
		ServiceProvider = serviceProvider;
		CultureInfo = cultureInfo;
	}

	internal XamlSetTypeConverterEventArgs(XamlMember member, TypeConverter typeConverter, object value, ITypeDescriptorContext serviceProvider, CultureInfo cultureInfo, object targetObject)
		: this(member, typeConverter, value, serviceProvider, cultureInfo)
	{
		TargetObject = targetObject;
	}

	/// <summary>Provides a way to invoke a callback as defined on a base class of the current acting type.</summary>
	public override void CallBase()
	{
		if (!(CurrentType != null))
		{
			return;
		}
		XamlType baseType = CurrentType.BaseType;
		if (baseType != null)
		{
			CurrentType = baseType;
			if (baseType.SetTypeConverterHandler != null)
			{
				baseType.SetTypeConverterHandler(TargetObject, this);
			}
		}
	}
}
