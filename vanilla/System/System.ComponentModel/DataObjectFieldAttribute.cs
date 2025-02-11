namespace System.ComponentModel;

/// <summary>Provides metadata for a property representing a data field. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DataObjectFieldAttribute : Attribute
{
	private bool _primaryKey;

	private bool _isIdentity;

	private bool _isNullable;

	private int _length;

	/// <summary>Gets a value indicating whether a property represents an identity field in the underlying data.</summary>
	/// <returns>true if the property represents an identity field in the underlying data; otherwise, false. The default value is false.</returns>
	public bool IsIdentity => _isIdentity;

	/// <summary>Gets a value indicating whether a property represents a field that can be null in the underlying data store.</summary>
	/// <returns>true if the property represents a field that can be null in the underlying data store; otherwise, false.</returns>
	public bool IsNullable => _isNullable;

	/// <summary>Gets the length of the property in bytes.</summary>
	/// <returns>The length of the property in bytes, or -1 if not set.</returns>
	public int Length => _length;

	/// <summary>Gets a value indicating whether a property is in the primary key in the underlying data.</summary>
	/// <returns>true if the property is in the primary key of the data store; otherwise, false.</returns>
	public bool PrimaryKey => _primaryKey;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataObjectFieldAttribute" /> class and indicates whether the field is the primary key for the data row.</summary>
	/// <param name="primaryKey">true to indicate that the field is in the primary key of the data row; otherwise, false.</param>
	public DataObjectFieldAttribute(bool primaryKey)
		: this(primaryKey, isIdentity: false, isNullable: false, -1)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataObjectFieldAttribute" /> class and indicates whether the field is the primary key for the data row, and whether the field is a database identity field.</summary>
	/// <param name="primaryKey">true to indicate that the field is in the primary key of the data row; otherwise, false.</param>
	/// <param name="isIdentity">true to indicate that the field is an identity field that uniquely identifies the data row; otherwise, false.</param>
	public DataObjectFieldAttribute(bool primaryKey, bool isIdentity)
		: this(primaryKey, isIdentity, isNullable: false, -1)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataObjectFieldAttribute" /> class and indicates whether the field is the primary key for the data row, whether the field is a database identity field, and whether the field can be null.</summary>
	/// <param name="primaryKey">true to indicate that the field is in the primary key of the data row; otherwise, false.</param>
	/// <param name="isIdentity">true to indicate that the field is an identity field that uniquely identifies the data row; otherwise, false.</param>
	/// <param name="isNullable">true to indicate that the field can be null in the data store; otherwise, false.</param>
	public DataObjectFieldAttribute(bool primaryKey, bool isIdentity, bool isNullable)
		: this(primaryKey, isIdentity, isNullable, -1)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataObjectFieldAttribute" /> class and indicates whether the field is the primary key for the data row, whether it is a database identity field, and whether it can be null and sets the length of the field.</summary>
	/// <param name="primaryKey">true to indicate that the field is in the primary key of the data row; otherwise, false.</param>
	/// <param name="isIdentity">true to indicate that the field is an identity field that uniquely identifies the data row; otherwise, false.</param>
	/// <param name="isNullable">true to indicate that the field can be null in the data store; otherwise, false.</param>
	/// <param name="length">The length of the field in bytes.</param>
	public DataObjectFieldAttribute(bool primaryKey, bool isIdentity, bool isNullable, int length)
	{
		_primaryKey = primaryKey;
		_isIdentity = isIdentity;
		_isNullable = isNullable;
		_length = length;
	}

	/// <summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	/// <returns>true if this instance is the same as the instance specified by the <paramref name="obj" /> parameter; otherwise, false.</returns>
	/// <param name="obj">An object to compare with this instance of <see cref="T:System.ComponentModel.DataObjectFieldAttribute" />.</param>
	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is DataObjectFieldAttribute dataObjectFieldAttribute && dataObjectFieldAttribute.IsIdentity == IsIdentity && dataObjectFieldAttribute.IsNullable == IsNullable && dataObjectFieldAttribute.Length == Length)
		{
			return dataObjectFieldAttribute.PrimaryKey == PrimaryKey;
		}
		return false;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
