namespace System.ComponentModel.Design.Serialization;

/// <summary>Indicates a serializer for the serialization manager to use to serialize the values of the type this attribute is applied to. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
public sealed class DesignerSerializerAttribute : Attribute
{
	private string serializerTypeName;

	private string serializerBaseTypeName;

	private string typeId;

	/// <summary>Gets the fully qualified type name of the serializer.</summary>
	/// <returns>The fully qualified type name of the serializer.</returns>
	public string SerializerTypeName => serializerTypeName;

	/// <summary>Gets the fully qualified type name of the serializer base type.</summary>
	/// <returns>The fully qualified type name of the serializer base type.</returns>
	public string SerializerBaseTypeName => serializerBaseTypeName;

	/// <summary>Indicates a unique ID for this attribute type.</summary>
	/// <returns>A unique ID for this attribute type.</returns>
	public override object TypeId
	{
		get
		{
			if (typeId == null)
			{
				string text = serializerBaseTypeName;
				int num = text.IndexOf(',');
				if (num != -1)
				{
					text = text.Substring(0, num);
				}
				typeId = GetType().FullName + text;
			}
			return typeId;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.Serialization.DesignerSerializerAttribute" /> class.</summary>
	/// <param name="serializerType">The data type of the serializer. </param>
	/// <param name="baseSerializerType">The base data type of the serializer. Multiple serializers can be supplied for a class as long as the serializers have different base types. </param>
	public DesignerSerializerAttribute(Type serializerType, Type baseSerializerType)
	{
		serializerTypeName = serializerType.AssemblyQualifiedName;
		serializerBaseTypeName = baseSerializerType.AssemblyQualifiedName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.Serialization.DesignerSerializerAttribute" /> class.</summary>
	/// <param name="serializerTypeName">The fully qualified name of the data type of the serializer. </param>
	/// <param name="baseSerializerType">The base data type of the serializer. Multiple serializers can be supplied for a class as long as the serializers have different base types. </param>
	public DesignerSerializerAttribute(string serializerTypeName, Type baseSerializerType)
	{
		this.serializerTypeName = serializerTypeName;
		serializerBaseTypeName = baseSerializerType.AssemblyQualifiedName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.Serialization.DesignerSerializerAttribute" /> class.</summary>
	/// <param name="serializerTypeName">The fully qualified name of the data type of the serializer. </param>
	/// <param name="baseSerializerTypeName">The fully qualified name of the base data type of the serializer. Multiple serializers can be supplied for a class as long as the serializers have different base types. </param>
	public DesignerSerializerAttribute(string serializerTypeName, string baseSerializerTypeName)
	{
		this.serializerTypeName = serializerTypeName;
		serializerBaseTypeName = baseSerializerTypeName;
	}
}
