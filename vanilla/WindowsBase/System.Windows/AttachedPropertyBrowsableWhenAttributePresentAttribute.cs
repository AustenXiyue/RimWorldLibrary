using System.ComponentModel;

namespace System.Windows;

/// <summary>Specifies that an attached property is only browsable on an element that also has another specific  .NET Framework attribute applied to its class definition.</summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class AttachedPropertyBrowsableWhenAttributePresentAttribute : AttachedPropertyBrowsableAttribute
{
	private Type _attributeType;

	/// <summary>Gets the type of the  .NET Framework attribute that must also be applied on a class.</summary>
	/// <returns>The  .NET Framework attribute type.</returns>
	public Type AttributeType => _attributeType;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.AttachedPropertyBrowsableWhenAttributePresentAttribute" /> class.</summary>
	/// <param name="attributeType">The <see cref="T:System.Type" /> of the  .NET Framework attribute that must also be applied on a class in order for the attached property to be browsable on the class where <see cref="T:System.Windows.AttachedPropertyBrowsableWhenAttributePresentAttribute" /> is applied..</param>
	public AttachedPropertyBrowsableWhenAttributePresentAttribute(Type attributeType)
	{
		if (attributeType == null)
		{
			throw new ArgumentNullException("attributeType");
		}
		_attributeType = attributeType;
	}

	/// <summary>Determines whether the current <see cref="T:System.Windows.AttachedPropertyBrowsableWhenAttributePresentAttribute" /> .NET Framework attribute is equal to a specified object.</summary>
	/// <returns>true if the specified <see cref="T:System.Windows.AttachedPropertyBrowsableWhenAttributePresentAttribute" /> is equal to the current <see cref="T:System.Windows.AttachedPropertyBrowsableWhenAttributePresentAttribute" />; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Windows.AttachedPropertyBrowsableWhenAttributePresentAttribute" /> to compare to the current <see cref="T:System.Windows.AttachedPropertyBrowsableWhenAttributePresentAttribute" />.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is AttachedPropertyBrowsableWhenAttributePresentAttribute attachedPropertyBrowsableWhenAttributePresentAttribute))
		{
			return false;
		}
		return _attributeType == attachedPropertyBrowsableWhenAttributePresentAttribute._attributeType;
	}

	/// <summary>Returns the hash code for this <see cref="T:System.Windows.AttachedPropertyBrowsableWhenAttributePresentAttribute" /> .NET Framework attribute.</summary>
	/// <returns>An unsigned 32-bit integer value.</returns>
	public override int GetHashCode()
	{
		return _attributeType.GetHashCode();
	}

	internal override bool IsBrowsable(DependencyObject d, DependencyProperty dp)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		Attribute attribute = TypeDescriptor.GetAttributes(d)[_attributeType];
		if (attribute != null)
		{
			return !attribute.IsDefaultAttribute();
		}
		return false;
	}
}
