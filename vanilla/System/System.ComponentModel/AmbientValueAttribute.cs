namespace System.ComponentModel;

/// <summary>Specifies the value to pass to a property to cause the property to get its value from another source. This is known as ambience. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.All)]
public sealed class AmbientValueAttribute : Attribute
{
	private readonly object value;

	/// <summary>Gets the object that is the value of this <see cref="T:System.ComponentModel.AmbientValueAttribute" />.</summary>
	/// <returns>The object that is the value of this <see cref="T:System.ComponentModel.AmbientValueAttribute" />.</returns>
	public object Value => value;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.AmbientValueAttribute" /> class, given the value and its type.</summary>
	/// <param name="type">The <see cref="T:System.Type" /> of the <paramref name="value" /> parameter. </param>
	/// <param name="value">The value for this attribute. </param>
	public AmbientValueAttribute(Type type, string value)
	{
		try
		{
			this.value = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(value);
		}
		catch
		{
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.AmbientValueAttribute" /> class, given a Unicode character for its value.</summary>
	/// <param name="value">The value of this attribute. </param>
	public AmbientValueAttribute(char value)
	{
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.AmbientValueAttribute" /> class, given an 8-bit unsigned integer for its value.</summary>
	/// <param name="value">The value of this attribute. </param>
	public AmbientValueAttribute(byte value)
	{
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.AmbientValueAttribute" /> class, given a 16-bit signed integer for its value.</summary>
	/// <param name="value">The value of this attribute. </param>
	public AmbientValueAttribute(short value)
	{
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.AmbientValueAttribute" /> class, given a 32-bit signed integer for its value.</summary>
	/// <param name="value">The value of this attribute. </param>
	public AmbientValueAttribute(int value)
	{
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.AmbientValueAttribute" /> class, given a 64-bit signed integer for its value.</summary>
	/// <param name="value">The value of this attribute. </param>
	public AmbientValueAttribute(long value)
	{
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.AmbientValueAttribute" /> class, given a single-precision floating point number for its value.</summary>
	/// <param name="value">The value of this attribute. </param>
	public AmbientValueAttribute(float value)
	{
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.AmbientValueAttribute" /> class, given a double-precision floating-point number for its value.</summary>
	/// <param name="value">The value of this attribute. </param>
	public AmbientValueAttribute(double value)
	{
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.AmbientValueAttribute" /> class, given a Boolean value for its value.</summary>
	/// <param name="value">The value of this attribute. </param>
	public AmbientValueAttribute(bool value)
	{
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.AmbientValueAttribute" /> class, given a string for its value.</summary>
	/// <param name="value">The value of this attribute. </param>
	public AmbientValueAttribute(string value)
	{
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.AmbientValueAttribute" /> class, given an object for its value.</summary>
	/// <param name="value">The value of this attribute. </param>
	public AmbientValueAttribute(object value)
	{
		this.value = value;
	}

	/// <summary>Determines whether the specified <see cref="T:System.ComponentModel.AmbientValueAttribute" /> is equal to the current <see cref="T:System.ComponentModel.AmbientValueAttribute" />.</summary>
	/// <returns>true if the specified <see cref="T:System.ComponentModel.AmbientValueAttribute" /> is equal to the current <see cref="T:System.ComponentModel.AmbientValueAttribute" />; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.ComponentModel.AmbientValueAttribute" /> to compare with the current <see cref="T:System.ComponentModel.AmbientValueAttribute" />.</param>
	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is AmbientValueAttribute ambientValueAttribute)
		{
			if (value != null)
			{
				return value.Equals(ambientValueAttribute.Value);
			}
			return ambientValueAttribute.Value == null;
		}
		return false;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A hash code for the current <see cref="T:System.ComponentModel.AmbientValueAttribute" />.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
