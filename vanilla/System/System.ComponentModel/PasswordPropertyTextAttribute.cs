namespace System.ComponentModel;

/// <summary>Indicates that an object's text representation is obscured by characters such as asterisks. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.All)]
public sealed class PasswordPropertyTextAttribute : Attribute
{
	/// <summary>Specifies that a text property is used as a password. This static (Shared in Visual Basic) field is read-only.</summary>
	public static readonly PasswordPropertyTextAttribute Yes = new PasswordPropertyTextAttribute(password: true);

	/// <summary>Specifies that a text property is not used as a password. This static (Shared in Visual Basic) field is read-only.</summary>
	public static readonly PasswordPropertyTextAttribute No = new PasswordPropertyTextAttribute(password: false);

	/// <summary>Specifies the default value for the <see cref="T:System.ComponentModel.PasswordPropertyTextAttribute" />.</summary>
	public static readonly PasswordPropertyTextAttribute Default = No;

	private bool _password;

	/// <summary>Gets a value indicating if the property for which the <see cref="T:System.ComponentModel.PasswordPropertyTextAttribute" /> is defined should be shown as password text.</summary>
	/// <returns>true if the property should be shown as password text; otherwise, false.</returns>
	public bool Password => _password;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.PasswordPropertyTextAttribute" /> class. </summary>
	public PasswordPropertyTextAttribute()
		: this(password: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.PasswordPropertyTextAttribute" /> class, optionally showing password text. </summary>
	/// <param name="password">true to indicate that the property should be shown as password text; otherwise, false. The default is false.</param>
	public PasswordPropertyTextAttribute(bool password)
	{
		_password = password;
	}

	/// <summary>Determines whether two <see cref="T:System.ComponentModel.PasswordPropertyTextAttribute" /> instances are equal.</summary>
	/// <returns>true if the specified <see cref="T:System.ComponentModel.PasswordPropertyTextAttribute" /> is equal to the current <see cref="T:System.ComponentModel.PasswordPropertyTextAttribute" />; otherwise, false.</returns>
	/// <param name="o">The <see cref="T:System.ComponentModel.PasswordPropertyTextAttribute" /> to compare with the current <see cref="T:System.ComponentModel.PasswordPropertyTextAttribute" />.</param>
	public override bool Equals(object o)
	{
		if (o is PasswordPropertyTextAttribute)
		{
			return ((PasswordPropertyTextAttribute)o).Password == _password;
		}
		return false;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A hash code for the current <see cref="T:System.ComponentModel.PasswordPropertyTextAttribute" />.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Returns an indication whether the value of this instance is the default value.</summary>
	/// <returns>true if this instance is the default attribute for the class; otherwise, false.</returns>
	public override bool IsDefaultAttribute()
	{
		return Equals(Default);
	}
}
