namespace System.Runtime.Serialization;

/// <summary>Specifies that the field is an enumeration member and should be serialized.</summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class EnumMemberAttribute : Attribute
{
	private string value;

	private bool isValueSetExplicitly;

	/// <summary>Gets or sets the value associated with the enumeration member the attribute is applied to. </summary>
	/// <returns>The value associated with the enumeration member.</returns>
	public string Value
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
			isValueSetExplicitly = true;
		}
	}

	public bool IsValueSetExplicitly => isValueSetExplicitly;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.EnumMemberAttribute" /> class. </summary>
	public EnumMemberAttribute()
	{
	}
}
