using System.Collections;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides automatic conversion between a nullable type and its underlying primitive type.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class NullableConverter : TypeConverter
{
	private Type nullableType;

	private Type simpleType;

	private TypeConverter simpleTypeConverter;

	/// <summary>Gets the nullable type.</summary>
	/// <returns>A <see cref="T:System.Type" /> that represents the nullable type.</returns>
	public Type NullableType => nullableType;

	/// <summary>Gets the underlying type.</summary>
	/// <returns>A <see cref="T:System.Type" /> that represents the underlying type.</returns>
	public Type UnderlyingType => simpleType;

	/// <summary>Gets the underlying type converter.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.TypeConverter" /> that represents the underlying type converter.</returns>
	public TypeConverter UnderlyingTypeConverter => simpleTypeConverter;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.NullableConverter" /> class.</summary>
	/// <param name="type">The specified nullable type.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="type" /> is not a nullable type.</exception>
	public NullableConverter(Type type)
	{
		nullableType = type;
		simpleType = Nullable.GetUnderlyingType(type);
		if (simpleType == null)
		{
			throw new ArgumentException(global::SR.GetString("The specified type is not a nullable type."), "type");
		}
		simpleTypeConverter = TypeDescriptor.GetConverter(simpleType);
	}

	/// <summary>Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.</summary>
	/// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" />  that provides a format context.</param>
	/// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you want to convert from.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == simpleType)
		{
			return true;
		}
		if (simpleTypeConverter != null)
		{
			return simpleTypeConverter.CanConvertFrom(context, sourceType);
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Converts the given object to the type of this converter, using the specified context and culture information.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null || value.GetType() == simpleType)
		{
			return value;
		}
		if (value is string && string.IsNullOrEmpty(value as string))
		{
			return null;
		}
		if (simpleTypeConverter != null)
		{
			return simpleTypeConverter.ConvertFrom(context, culture, value);
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Returns whether this converter can convert the object to the specified type, using the specified context.</summary>
	/// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	/// <param name="destinationType">A <see cref="T:System.Type" /> that represents the type you want to convert to.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == simpleType)
		{
			return true;
		}
		if (destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		if (simpleTypeConverter != null)
		{
			return simpleTypeConverter.CanConvertTo(context, destinationType);
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts the given value object to the specified type, using the specified context and culture information.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
	/// <param name="destinationType">The <see cref="T:System.Type" /> to convert the value parameter to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationType" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (destinationType == simpleType && nullableType.IsInstanceOfType(value))
		{
			return value;
		}
		if (destinationType == typeof(InstanceDescriptor))
		{
			return new InstanceDescriptor(nullableType.GetConstructor(new Type[1] { simpleType }), new object[1] { value }, isComplete: true);
		}
		if (value == null)
		{
			if (destinationType == typeof(string))
			{
				return string.Empty;
			}
		}
		else if (simpleTypeConverter != null)
		{
			return simpleTypeConverter.ConvertTo(context, culture, value, destinationType);
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <returns>An <see cref="T:System.Object" /> representing the given <see cref="T:System.Collections.IDictionary" />, or null if the object cannot be created. This method always returns null.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="propertyValues">An <see cref="T:System.Collections.IDictionary" /> of new property values. </param>
	public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
	{
		if (simpleTypeConverter != null)
		{
			return simpleTypeConverter.CreateInstance(context, propertyValues);
		}
		return base.CreateInstance(context, propertyValues);
	}

	/// <returns>true if changing a property on this object requires a call to <see cref="M:System.ComponentModel.TypeConverter.CreateInstance(System.Collections.IDictionary)" /> to create a new value; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
	{
		if (simpleTypeConverter != null)
		{
			return simpleTypeConverter.GetCreateInstanceSupported(context);
		}
		return base.GetCreateInstanceSupported(context);
	}

	/// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> with the properties that are exposed for this data type, or null if there are no properties.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="value">An <see cref="T:System.Object" /> that specifies the type of array for which to get properties. </param>
	/// <param name="attributes">An array of type <see cref="T:System.Attribute" /> that is used as a filter. </param>
	public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
	{
		if (simpleTypeConverter != null)
		{
			return simpleTypeConverter.GetProperties(context, value, attributes);
		}
		return base.GetProperties(context, value, attributes);
	}

	/// <returns>true if <see cref="M:System.ComponentModel.TypeConverter.GetProperties(System.Object)" /> should be called to find the properties of this object; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	public override bool GetPropertiesSupported(ITypeDescriptorContext context)
	{
		if (simpleTypeConverter != null)
		{
			return simpleTypeConverter.GetPropertiesSupported(context);
		}
		return base.GetPropertiesSupported(context);
	}

	/// <returns>A <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" /> that holds a standard set of valid values, or null if the data type does not support a standard set of values.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context that can be used to extract additional information about the environment from which this converter is invoked. This parameter or properties of this parameter can be null.</param>
	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	{
		if (simpleTypeConverter != null)
		{
			StandardValuesCollection standardValues = simpleTypeConverter.GetStandardValues(context);
			if (GetStandardValuesSupported(context) && standardValues != null)
			{
				object[] array = new object[standardValues.Count + 1];
				int num = 0;
				array[num++] = null;
				foreach (object item in standardValues)
				{
					array[num++] = item;
				}
				return new StandardValuesCollection(array);
			}
		}
		return base.GetStandardValues(context);
	}

	/// <returns>true if the <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" /> returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> is an exhaustive list of possible values; false if other values are possible.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		if (simpleTypeConverter != null)
		{
			return simpleTypeConverter.GetStandardValuesExclusive(context);
		}
		return base.GetStandardValuesExclusive(context);
	}

	/// <returns>true if <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> should be called to find a common set of values the object supports; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		if (simpleTypeConverter != null)
		{
			return simpleTypeConverter.GetStandardValuesSupported(context);
		}
		return base.GetStandardValuesSupported(context);
	}

	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="value">The <see cref="T:System.Object" /> to test for validity. </param>
	public override bool IsValid(ITypeDescriptorContext context, object value)
	{
		if (simpleTypeConverter != null)
		{
			if (value == null)
			{
				return true;
			}
			return simpleTypeConverter.IsValid(context, value);
		}
		return base.IsValid(context, value);
	}
}
