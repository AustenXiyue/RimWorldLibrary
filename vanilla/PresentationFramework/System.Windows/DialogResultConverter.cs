using System.ComponentModel;
using System.Globalization;

namespace System.Windows;

/// <summary>Converts the <see cref="P:System.Windows.Window.DialogResult" /> property, which is a <see cref="T:System.Nullable`1" /> value of type <see cref="T:System.Boolean" />, to and from other types.</summary>
public class DialogResultConverter : TypeConverter
{
	/// <summary>
	///   <see cref="T:System.Windows.DialogResultConverter" /> does not support converting from other types to <see cref="P:System.Windows.Window.DialogResult" /> (a <see cref="T:System.Nullable`1" /> value of type <see cref="T:System.Boolean" />).</summary>
	/// <returns>A <see cref="T:System.Boolean" /> that always returns false.</returns>
	/// <param name="typeDescriptorContext">A <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	/// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type to convert from.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
	{
		return false;
	}

	/// <summary>
	///   <see cref="T:System.Windows.DialogResultConverter" /> does not support converting from <see cref="P:System.Windows.Window.DialogResult" /> (a <see cref="T:System.Nullable`1" /> value of type <see cref="T:System.Boolean" />) to other types.</summary>
	/// <returns>A <see cref="T:System.Boolean" /> that always returns false.</returns>
	/// <param name="typeDescriptorContext">A <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	/// <param name="destinationType">A <see cref="T:System.Type" /> that represents the type to convert to.</param>
	public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
	{
		return false;
	}

	/// <summary>
	///   <see cref="T:System.Windows.DialogResultConverter" /> does not support converting from <see cref="P:System.Windows.Window.DialogResult" /> (a <see cref="T:System.Nullable`1" /> value of type <see cref="T:System.Boolean" />) to other types.</summary>
	/// <returns>Always raises <see cref="T:System.InvalidOperationException" />.</returns>
	/// <param name="typeDescriptorContext">A <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	/// <param name="cultureInfo">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture. If null is passed, the current culture is assumed.</param>
	/// <param name="source">The <see cref="T:System.Object" /> to convert.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Windows.DialogResultConverter.ConvertFrom(System.ComponentModel.ITypeDescriptorContext,System.Globalization.CultureInfo,System.Object)" /> is called.</exception>
	public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
	{
		throw new InvalidOperationException(SR.CantSetInMarkup);
	}

	/// <summary>
	///   <see cref="T:System.Windows.DialogResultConverter" /> does not support converting from other types to <see cref="P:System.Windows.Window.DialogResult" /> (a <see cref="T:System.Nullable`1" /> value of type <see cref="T:System.Boolean" />).</summary>
	/// <returns>Always raises <see cref="T:System.InvalidOperationException" />.</returns>
	/// <param name="typeDescriptorContext">A <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	/// <param name="cultureInfo">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture. If null is passed, the current culture is assumed.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
	/// <param name="destinationType">The <see cref="T:System.Type" /> to convert the value parameter to.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Windows.DialogResultConverter.CanConvertTo(System.ComponentModel.ITypeDescriptorContext,System.Type)" /> is called.</exception>
	public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)
	{
		throw new InvalidOperationException(SR.CantSetInMarkup);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.DialogResultConverter" /> class.</summary>
	public DialogResultConverter()
	{
	}
}
