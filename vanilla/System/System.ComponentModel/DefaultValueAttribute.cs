namespace System.ComponentModel;

/// <summary>Specifies the default value for a property.</summary>
[AttributeUsage(AttributeTargets.All)]
public class DefaultValueAttribute : Attribute
{
	private object value;

	/// <summary>Gets the default value of the property this attribute is bound to.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the default value of the property this attribute is bound to.</returns>
	public virtual object Value => value;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class, converting the specified value to the specified type, and using an invariant culture as the translation context.</summary>
	/// <param name="type">A <see cref="T:System.Type" /> that represents the type to convert the value to. </param>
	/// <param name="value">A <see cref="T:System.String" /> that can be converted to the type using the <see cref="T:System.ComponentModel.TypeConverter" /> for the type and the U.S. English culture. </param>
	public DefaultValueAttribute(Type type, string value)
	{
		try
		{
			this.value = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(value);
		}
		catch
		{
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class using a Unicode character.</summary>
	/// <param name="value">A Unicode character that is the default value. </param>
	public DefaultValueAttribute(char value)
	{
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class using an 8-bit unsigned integer.</summary>
	/// <param name="value">An 8-bit unsigned integer that is the default value. </param>
	public DefaultValueAttribute(byte value)
	{
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class using a 16-bit signed integer.</summary>
	/// <param name="value">A 16-bit signed integer that is the default value. </param>
	public DefaultValueAttribute(short value)
	{
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class using a 32-bit signed integer.</summary>
	/// <param name="value">A 32-bit signed integer that is the default value. </param>
	public DefaultValueAttribute(int value)
	{
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class using a 64-bit signed integer.</summary>
	/// <param name="value">A 64-bit signed integer that is the default value. </param>
	public DefaultValueAttribute(long value)
	{
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class using a single-precision floating point number.</summary>
	/// <param name="value">A single-precision floating point number that is the default value. </param>
	public DefaultValueAttribute(float value)
	{
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class using a double-precision floating point number.</summary>
	/// <param name="value">A double-precision floating point number that is the default value. </param>
	public DefaultValueAttribute(double value)
	{
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class using a <see cref="T:System.Boolean" /> value.</summary>
	/// <param name="value">A <see cref="T:System.Boolean" /> that is the default value. </param>
	public DefaultValueAttribute(bool value)
	{
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class using a <see cref="T:System.String" />.</summary>
	/// <param name="value">A <see cref="T:System.String" /> that is the default value. </param>
	public DefaultValueAttribute(string value)
	{
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class.</summary>
	/// <param name="value">An <see cref="T:System.Object" /> that represents the default value. </param>
	public DefaultValueAttribute(object value)
	{
		this.value = value;
	}

	/// <summary>Returns whether the value of the given object is equal to the current <see cref="T:System.ComponentModel.DefaultValueAttribute" />.</summary>
	/// <returns>true if the value of the given object is equal to that of the current; otherwise, false.</returns>
	/// <param name="obj">The object to test the value equality of. </param>
	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is DefaultValueAttribute defaultValueAttribute)
		{
			if (Value != null)
			{
				return Value.Equals(defaultValueAttribute.Value);
			}
			return defaultValueAttribute.Value == null;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Sets the default value for the property to which this attribute is bound.</summary>
	/// <param name="value">The default value.</param>
	protected void SetValue(object value)
	{
		this.value = value;
	}
}
