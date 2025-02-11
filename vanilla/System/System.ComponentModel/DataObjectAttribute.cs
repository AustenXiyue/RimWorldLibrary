namespace System.ComponentModel;

/// <summary>Identifies a type as an object suitable for binding to an <see cref="T:System.Web.UI.WebControls.ObjectDataSource" /> object. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DataObjectAttribute : Attribute
{
	/// <summary>Indicates that the class is suitable for binding to an <see cref="T:System.Web.UI.WebControls.ObjectDataSource" /> object at design time. This field is read-only.</summary>
	public static readonly DataObjectAttribute DataObject = new DataObjectAttribute(isDataObject: true);

	/// <summary>Indicates that the class is not suitable for binding to an <see cref="T:System.Web.UI.WebControls.ObjectDataSource" /> object at design time. This field is read-only.</summary>
	public static readonly DataObjectAttribute NonDataObject = new DataObjectAttribute(isDataObject: false);

	/// <summary>Represents the default value of the <see cref="T:System.ComponentModel.DataObjectAttribute" /> class, which indicates that the class is suitable for binding to an <see cref="T:System.Web.UI.WebControls.ObjectDataSource" /> object at design time. This field is read-only.</summary>
	public static readonly DataObjectAttribute Default = NonDataObject;

	private bool _isDataObject;

	/// <summary>Gets a value indicating whether an object should be considered suitable for binding to an <see cref="T:System.Web.UI.WebControls.ObjectDataSource" /> object at design time.</summary>
	/// <returns>true if the object should be considered suitable for binding to an <see cref="T:System.Web.UI.WebControls.ObjectDataSource" /> object; otherwise, false.</returns>
	public bool IsDataObject => _isDataObject;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataObjectAttribute" /> class. </summary>
	public DataObjectAttribute()
		: this(isDataObject: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataObjectAttribute" /> class and indicates whether an object is suitable for binding to an <see cref="T:System.Web.UI.WebControls.ObjectDataSource" /> object.</summary>
	/// <param name="isDataObject">true if the object is suitable for binding to an <see cref="T:System.Web.UI.WebControls.ObjectDataSource" /> object; otherwise, false.</param>
	public DataObjectAttribute(bool isDataObject)
	{
		_isDataObject = isDataObject;
	}

	/// <summary>Determines whether this instance of <see cref="T:System.ComponentModel.DataObjectAttribute" /> fits the pattern of another object.</summary>
	/// <returns>true if this instance is the same as the instance specified by the <paramref name="obj" /> parameter; otherwise, false.</returns>
	/// <param name="obj">An object to compare with this instance of <see cref="T:System.ComponentModel.DataObjectAttribute" />. </param>
	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is DataObjectAttribute dataObjectAttribute)
		{
			return dataObjectAttribute.IsDataObject == IsDataObject;
		}
		return false;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return _isDataObject.GetHashCode();
	}

	/// <summary>Gets a value indicating whether the current value of the attribute is the default value for the attribute.</summary>
	/// <returns>true if the current value of the attribute is the default; otherwise, false.</returns>
	public override bool IsDefaultAttribute()
	{
		return Equals(Default);
	}
}
