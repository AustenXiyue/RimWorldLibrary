namespace System.ComponentModel;

/// <summary>Specifies a property that is offered by an extender provider. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.All)]
public sealed class ExtenderProvidedPropertyAttribute : Attribute
{
	private PropertyDescriptor extenderProperty;

	private IExtenderProvider provider;

	private Type receiverType;

	/// <summary>Gets the property that is being provided.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptor" /> encapsulating the property that is being provided.</returns>
	public PropertyDescriptor ExtenderProperty => extenderProperty;

	/// <summary>Gets the extender provider that is providing the property.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.IExtenderProvider" /> that is providing the property.</returns>
	public IExtenderProvider Provider => provider;

	/// <summary>Gets the type of object that can receive the property.</summary>
	/// <returns>A <see cref="T:System.Type" /> describing the type of object that can receive the property.</returns>
	public Type ReceiverType => receiverType;

	internal static ExtenderProvidedPropertyAttribute Create(PropertyDescriptor extenderProperty, Type receiverType, IExtenderProvider provider)
	{
		return new ExtenderProvidedPropertyAttribute
		{
			extenderProperty = extenderProperty,
			receiverType = receiverType,
			provider = provider
		};
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.ExtenderProvidedPropertyAttribute" /> class. </summary>
	public ExtenderProvidedPropertyAttribute()
	{
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.</summary>
	/// <returns>true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.</returns>
	/// <param name="obj">An <see cref="T:System.Object" /> to compare with this instance or null. </param>
	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is ExtenderProvidedPropertyAttribute extenderProvidedPropertyAttribute && extenderProvidedPropertyAttribute.extenderProperty.Equals(extenderProperty) && extenderProvidedPropertyAttribute.provider.Equals(provider))
		{
			return extenderProvidedPropertyAttribute.receiverType.Equals(receiverType);
		}
		return false;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Provides an indication whether the value of this instance is the default value for the derived class.</summary>
	/// <returns>true if this instance is the default attribute for the class; otherwise, false.</returns>
	public override bool IsDefaultAttribute()
	{
		return receiverType == null;
	}
}
