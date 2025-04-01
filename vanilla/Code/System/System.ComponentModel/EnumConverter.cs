using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides a type converter to convert <see cref="T:System.Enum" /> objects to and from various other representations.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class EnumConverter : TypeConverter
{
	private StandardValuesCollection values;

	private Type type;

	/// <summary>Specifies the type of the enumerator this converter is associated with.</summary>
	/// <returns>The type of the enumerator this converter is associated with.</returns>
	protected Type EnumType => type;

	/// <summary>Gets or sets a <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" /> that specifies the possible values for the enumeration.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" /> that specifies the possible values for the enumeration.</returns>
	protected StandardValuesCollection Values
	{
		get
		{
			return values;
		}
		set
		{
			values = value;
		}
	}

	/// <summary>Gets an <see cref="T:System.Collections.IComparer" /> that can be used to sort the values of the enumeration.</summary>
	/// <returns>An <see cref="T:System.Collections.IComparer" /> for sorting the enumeration values.</returns>
	protected virtual IComparer Comparer => System.InvariantComparer.Default;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.EnumConverter" /> class for the given type.</summary>
	/// <param name="type">A <see cref="T:System.Type" /> that represents the type of enumeration to associate with this enumeration converter. </param>
	public EnumConverter(Type type)
	{
		this.type = type;
	}

	/// <summary>Gets a value indicating whether this converter can convert an object in the given source type to an enumeration object using the specified context.</summary>
	/// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you wish to convert from. </param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string) || sourceType == typeof(Enum[]))
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
		if (destinationType == typeof(InstanceDescriptor) || destinationType == typeof(Enum[]))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts the specified value object to an enumeration object.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the converted <paramref name="value" />.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">An optional <see cref="T:System.Globalization.CultureInfo" />. If not supplied, the current culture is assumed. </param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not a valid value for the target type. </exception>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			try
			{
				string text = (string)value;
				if (text.IndexOf(',') != -1)
				{
					long num = 0L;
					string[] array = text.Split(',');
					foreach (string value2 in array)
					{
						num |= Convert.ToInt64((Enum)Enum.Parse(type, value2, ignoreCase: true), culture);
					}
					return Enum.ToObject(type, num);
				}
				return Enum.Parse(type, text, ignoreCase: true);
			}
			catch (Exception innerException)
			{
				throw new FormatException(global::SR.GetString("{0} is not a valid value for {1}.", (string)value, type.Name), innerException);
			}
		}
		if (value is Enum[])
		{
			long num2 = 0L;
			Enum[] array2 = (Enum[])value;
			foreach (Enum value3 in array2)
			{
				num2 |= Convert.ToInt64(value3, culture);
			}
			return Enum.ToObject(type, num2);
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
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not a valid value for the enumeration. </exception>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (destinationType == typeof(string) && value != null)
		{
			Type underlyingType = Enum.GetUnderlyingType(type);
			if (value is IConvertible && value.GetType() != underlyingType)
			{
				value = ((IConvertible)value).ToType(underlyingType, culture);
			}
			if (!type.IsDefined(typeof(FlagsAttribute), inherit: false) && !Enum.IsDefined(type, value))
			{
				throw new ArgumentException(global::SR.GetString("The value '{0}' is not a valid value for the enum '{1}'.", value.ToString(), type.Name));
			}
			return Enum.Format(type, value, "G");
		}
		if (destinationType == typeof(InstanceDescriptor) && value != null)
		{
			string text = ConvertToInvariantString(context, value);
			if (type.IsDefined(typeof(FlagsAttribute), inherit: false) && text.IndexOf(',') != -1)
			{
				Type underlyingType2 = Enum.GetUnderlyingType(type);
				if (value is IConvertible)
				{
					object obj = ((IConvertible)value).ToType(underlyingType2, culture);
					MethodInfo method = typeof(Enum).GetMethod("ToObject", new Type[2]
					{
						typeof(Type),
						underlyingType2
					});
					if (method != null)
					{
						return new InstanceDescriptor(method, new object[2] { type, obj });
					}
				}
			}
			else
			{
				FieldInfo field = type.GetField(text);
				if (field != null)
				{
					return new InstanceDescriptor(field, null);
				}
			}
		}
		if (destinationType == typeof(Enum[]) && value != null)
		{
			if (type.IsDefined(typeof(FlagsAttribute), inherit: false))
			{
				List<Enum> list = new List<Enum>();
				Array array = Enum.GetValues(type);
				long[] array2 = new long[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array2[i] = Convert.ToInt64((Enum)array.GetValue(i), culture);
				}
				long num = Convert.ToInt64((Enum)value, culture);
				bool flag = true;
				while (flag)
				{
					flag = false;
					long[] array3 = array2;
					foreach (long num2 in array3)
					{
						if ((num2 != 0L && (num2 & num) == num2) || num2 == num)
						{
							list.Add((Enum)Enum.ToObject(type, num2));
							flag = true;
							num &= ~num2;
							break;
						}
					}
					if (num == 0L)
					{
						break;
					}
				}
				if (!flag && num != 0L)
				{
					list.Add((Enum)Enum.ToObject(type, num));
				}
				return list.ToArray();
			}
			return new Enum[1] { (Enum)Enum.ToObject(type, value) };
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
			Type reflectionType = TypeDescriptor.GetReflectionType(type);
			if (reflectionType == null)
			{
				reflectionType = type;
			}
			FieldInfo[] fields = reflectionType.GetFields(BindingFlags.Static | BindingFlags.Public);
			ArrayList arrayList = null;
			if (fields != null && fields.Length != 0)
			{
				arrayList = new ArrayList(fields.Length);
			}
			if (arrayList != null)
			{
				FieldInfo[] array = fields;
				foreach (FieldInfo fieldInfo in array)
				{
					BrowsableAttribute browsableAttribute = null;
					object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(BrowsableAttribute), inherit: false);
					for (int j = 0; j < customAttributes.Length; j++)
					{
						browsableAttribute = ((Attribute)customAttributes[j]) as BrowsableAttribute;
					}
					if (browsableAttribute != null && !browsableAttribute.Browsable)
					{
						continue;
					}
					object obj = null;
					try
					{
						if (fieldInfo.Name != null)
						{
							obj = Enum.Parse(type, fieldInfo.Name);
						}
					}
					catch (ArgumentException)
					{
					}
					if (obj != null)
					{
						arrayList.Add(obj);
					}
				}
				IComparer comparer = Comparer;
				if (comparer != null)
				{
					arrayList.Sort(comparer);
				}
			}
			Array array2 = arrayList?.ToArray();
			values = new StandardValuesCollection(array2);
		}
		return values;
	}

	/// <summary>Gets a value indicating whether the list of standard values returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> is an exclusive list using the specified context.</summary>
	/// <returns>true if the <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" /> returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> is an exhaustive list of possible values; false if other values are possible.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		return !type.IsDefined(typeof(FlagsAttribute), inherit: false);
	}

	/// <summary>Gets a value indicating whether this object supports a standard set of values that can be picked from a list using the specified context.</summary>
	/// <returns>true because <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> should be called to find a common set of values the object supports. This method never returns false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	/// <summary>Gets a value indicating whether the given object value is valid for this type.</summary>
	/// <returns>true if the specified value is valid for this object; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="value">The <see cref="T:System.Object" /> to test. </param>
	public override bool IsValid(ITypeDescriptorContext context, object value)
	{
		return Enum.IsDefined(type, value);
	}
}
