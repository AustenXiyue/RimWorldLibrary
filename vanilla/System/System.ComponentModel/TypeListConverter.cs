using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides a type converter that can be used to populate a list box with available types.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public abstract class TypeListConverter : TypeConverter
{
	private Type[] types;

	private StandardValuesCollection values;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.TypeListConverter" /> class using the type array as the available types.</summary>
	/// <param name="types">The array of type <see cref="T:System.Type" /> to use as the available types. </param>
	protected TypeListConverter(Type[] types)
	{
		this.types = types;
	}

	/// <summary>Gets a value indicating whether this converter can convert the specified <see cref="T:System.Type" /> of the source object using the given context.</summary>
	/// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	/// <param name="sourceType">The <see cref="T:System.Type" /> of the source object.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Gets a value indicating whether this converter can convert an object to the given destination type using the context.</summary>
	/// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="destinationType">A <see cref="T:System.Type" /> that represents the type you wish to convert to. </param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts the specified object to the native type of the converter.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> that specifies the culture used to represent the font. </param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert. </param>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			Type[] array = types;
			foreach (Type type in array)
			{
				if (value.Equals(type.FullName))
				{
					return type;
				}
			}
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Converts the given value object to the specified destination type.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the converted <paramref name="value" />.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">An optional <see cref="T:System.Globalization.CultureInfo" />. If not supplied, the current culture is assumed. </param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert. </param>
	/// <param name="destinationType">The <see cref="T:System.Type" /> to convert the value to. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationType" /> is null. </exception>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (destinationType == typeof(string))
		{
			if (value == null)
			{
				return global::SR.GetString("(none)");
			}
			return ((Type)value).FullName;
		}
		if (destinationType == typeof(InstanceDescriptor) && value is Type)
		{
			MethodInfo method = typeof(Type).GetMethod("GetType", new Type[1] { typeof(string) });
			if (method != null)
			{
				return new InstanceDescriptor(method, new object[1] { ((Type)value).AssemblyQualifiedName });
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Gets a collection of standard values for the data type this validator is designed for.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" /> that holds a standard set of valid values, or null if the data type does not support a standard set of values.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	{
		if (values == null)
		{
			object[] destinationArray;
			if (types != null)
			{
				destinationArray = new object[types.Length];
				Array.Copy(types, destinationArray, types.Length);
			}
			else
			{
				destinationArray = null;
			}
			values = new StandardValuesCollection(destinationArray);
		}
		return values;
	}

	/// <summary>Gets a value indicating whether the list of standard values returned from the <see cref="M:System.ComponentModel.TypeListConverter.GetStandardValues(System.ComponentModel.ITypeDescriptorContext)" /> method is an exclusive list.</summary>
	/// <returns>true because the <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" /> returned from <see cref="M:System.ComponentModel.TypeListConverter.GetStandardValues(System.ComponentModel.ITypeDescriptorContext)" /> is an exhaustive list of possible values. This method never returns false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		return true;
	}

	/// <summary>Gets a value indicating whether this object supports a standard set of values that can be picked from a list using the specified context.</summary>
	/// <returns>true because <see cref="M:System.ComponentModel.TypeListConverter.GetStandardValues(System.ComponentModel.ITypeDescriptorContext)" /> should be called to find a common set of values the object supports. This method never returns false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		return true;
	}
}
