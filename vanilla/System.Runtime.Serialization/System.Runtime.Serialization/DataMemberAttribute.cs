namespace System.Runtime.Serialization;

/// <summary>When applied to the member of a type, specifies that the member is part of a data contract and is serializable by the <see cref="T:System.Runtime.Serialization.DataContractSerializer" />.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class DataMemberAttribute : Attribute
{
	private string name;

	private bool isNameSetExplicitly;

	private int order = -1;

	private bool isRequired;

	private bool emitDefaultValue = true;

	/// <summary>Gets or sets a data member name. </summary>
	/// <returns>The name of the data member. The default is the name of the target that the attribute is applied to. </returns>
	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
			isNameSetExplicitly = true;
		}
	}

	public bool IsNameSetExplicitly => isNameSetExplicitly;

	/// <summary>Gets or sets the order of serialization and deserialization of a member.</summary>
	/// <returns>The numeric order of serialization or deserialization.</returns>
	public int Order
	{
		get
		{
			return order;
		}
		set
		{
			if (value < 0)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("Property 'Order' in DataMemberAttribute attribute cannot be a negative number.")));
			}
			order = value;
		}
	}

	/// <summary>Gets or sets a value that instructs the serialization engine that the member must be present when reading or deserializing.</summary>
	/// <returns>true, if the member is required; otherwise, false.</returns>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">the member is not present.</exception>
	public bool IsRequired
	{
		get
		{
			return isRequired;
		}
		set
		{
			isRequired = value;
		}
	}

	/// <summary>Gets or sets a value that specifies whether to serialize the default value for a field or property being serialized. </summary>
	/// <returns>true if the default value for a member should be generated in the serialization stream; otherwise, false. The default is true.</returns>
	public bool EmitDefaultValue
	{
		get
		{
			return emitDefaultValue;
		}
		set
		{
			emitDefaultValue = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.DataMemberAttribute" /> class. </summary>
	public DataMemberAttribute()
	{
	}
}
