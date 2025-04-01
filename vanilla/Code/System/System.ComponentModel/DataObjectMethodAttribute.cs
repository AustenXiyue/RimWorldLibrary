namespace System.ComponentModel;

/// <summary>Identifies a data operation method exposed by a type, what type of operation the method performs, and whether the method is the default data method. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class DataObjectMethodAttribute : Attribute
{
	private bool _isDefault;

	private DataObjectMethodType _methodType;

	/// <summary>Gets a value indicating whether the method that the <see cref="T:System.ComponentModel.DataObjectMethodAttribute" /> is applied to is the default data method exposed by the data object for a specific method type.</summary>
	/// <returns>true if the method is the default method exposed by the object for a method type; otherwise, false.</returns>
	public bool IsDefault => _isDefault;

	/// <summary>Gets a <see cref="T:System.ComponentModel.DataObjectMethodType" /> value indicating the type of data operation the method performs.</summary>
	/// <returns>One of the <see cref="T:System.ComponentModel.DataObjectMethodType" /> values that identifies the type of data operation performed by the method to which the <see cref="T:System.ComponentModel.DataObjectMethodAttribute" /> is applied.</returns>
	public DataObjectMethodType MethodType => _methodType;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataObjectMethodAttribute" /> class and identifies the type of data operation the method performs.</summary>
	/// <param name="methodType">One of the <see cref="T:System.ComponentModel.DataObjectMethodType" /> values that describes the data operation the method performs.</param>
	public DataObjectMethodAttribute(DataObjectMethodType methodType)
		: this(methodType, isDefault: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataObjectMethodAttribute" /> class, identifies the type of data operation the method performs, and identifies whether the method is the default data method that the data object exposes.</summary>
	/// <param name="methodType">One of the <see cref="T:System.ComponentModel.DataObjectMethodType" /> values that describes the data operation the method performs.</param>
	/// <param name="isDefault">true to indicate the method that the attribute is applied to is the default method of the data object for the specified <paramref name="methodType" />; otherwise, false.</param>
	public DataObjectMethodAttribute(DataObjectMethodType methodType, bool isDefault)
	{
		_methodType = methodType;
		_isDefault = isDefault;
	}

	/// <summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	/// <returns>true if this instance is the same as the instance specified by the <paramref name="obj" /> parameter; otherwise, false.</returns>
	/// <param name="obj">An object to compare with this instance of <see cref="T:System.ComponentModel.DataObjectMethodAttribute" />.</param>
	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is DataObjectMethodAttribute dataObjectMethodAttribute && dataObjectMethodAttribute.MethodType == MethodType)
		{
			return dataObjectMethodAttribute.IsDefault == IsDefault;
		}
		return false;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		int methodType = (int)_methodType;
		return methodType.GetHashCode() ^ _isDefault.GetHashCode();
	}

	/// <summary>Gets a value indicating whether this instance shares a common pattern with a specified attribute.</summary>
	/// <returns>true if this instance is the same as the instance specified by the <paramref name="obj" /> parameter; otherwise, false.</returns>
	/// <param name="obj">An object to compare with this instance of <see cref="T:System.ComponentModel.DataObjectMethodAttribute" />.</param>
	public override bool Match(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is DataObjectMethodAttribute dataObjectMethodAttribute)
		{
			return dataObjectMethodAttribute.MethodType == MethodType;
		}
		return false;
	}
}
