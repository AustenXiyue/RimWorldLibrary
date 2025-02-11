namespace System.Security;

/// <summary>Specifies that an assembly cannot cause an elevation of privilege. </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class SecurityTransparentAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Security.SecurityTransparentAttribute" /> class. </summary>
	public SecurityTransparentAttribute()
	{
	}
}
