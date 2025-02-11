namespace System.Runtime.Serialization;

/// <summary>When applied to the member of a type, specifies that the member is not part of a data contract and is not serialized.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class IgnoreDataMemberAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.IgnoreDataMemberAttribute" /> class. </summary>
	public IgnoreDataMemberAttribute()
	{
	}
}
