using System.Globalization;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides a type converter to convert <see cref="T:System.Array" /> objects to and from various other representations.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class ArrayConverter : CollectionConverter
{
	private class ArrayPropertyDescriptor : SimplePropertyDescriptor
	{
		private int index;

		public ArrayPropertyDescriptor(Type arrayType, Type elementType, int index)
			: base(arrayType, "[" + index + "]", elementType, null)
		{
			this.index = index;
		}

		public override object GetValue(object instance)
		{
			if (instance is Array)
			{
				Array array = (Array)instance;
				if (array.GetLength(0) > index)
				{
					return array.GetValue(index);
				}
			}
			return null;
		}

		public override void SetValue(object instance, object value)
		{
			if (instance is Array)
			{
				Array array = (Array)instance;
				if (array.GetLength(0) > index)
				{
					array.SetValue(value, index);
				}
				OnValueChanged(instance, EventArgs.Empty);
			}
		}
	}

	/// <summary>Converts the given value object to the specified destination type.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">The culture into which <paramref name="value" /> will be converted.</param>
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
		if (destinationType == typeof(string) && value is Array)
		{
			return global::SR.GetString("{0} Array", value.GetType().Name);
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Gets a collection of properties for the type of array specified by the value parameter.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> with the properties that are exposed for an array, or null if there are no properties.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="value">An <see cref="T:System.Object" /> that specifies the type of array to get the properties for. </param>
	/// <param name="attributes">An array of type <see cref="T:System.Attribute" /> that will be used as a filter. </param>
	public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
	{
		PropertyDescriptor[] array = null;
		if (value.GetType().IsArray)
		{
			int length = ((Array)value).GetLength(0);
			array = new PropertyDescriptor[length];
			Type type = value.GetType();
			Type elementType = type.GetElementType();
			for (int i = 0; i < length; i++)
			{
				array[i] = new ArrayPropertyDescriptor(type, elementType, i);
			}
		}
		return new PropertyDescriptorCollection(array);
	}

	/// <summary>Gets a value indicating whether this object supports properties.</summary>
	/// <returns>true because <see cref="M:System.ComponentModel.ArrayConverter.GetProperties(System.ComponentModel.ITypeDescriptorContext,System.Object,System.Attribute[])" /> should be called to find the properties of this object. This method never returns false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	public override bool GetPropertiesSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.ArrayConverter" /> class. </summary>
	public ArrayConverter()
	{
	}
}
