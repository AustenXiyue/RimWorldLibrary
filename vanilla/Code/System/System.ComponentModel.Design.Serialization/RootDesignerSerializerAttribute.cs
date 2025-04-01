namespace System.ComponentModel.Design.Serialization;

/// <summary>Indicates the base serializer to use for a root designer object. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
[Obsolete("This attribute has been deprecated. Use DesignerSerializerAttribute instead.  For example, to specify a root designer for CodeDom, use DesignerSerializerAttribute(...,typeof(TypeCodeDomSerializer)).  http://go.microsoft.com/fwlink/?linkid=14202")]
public sealed class RootDesignerSerializerAttribute : Attribute
{
	private bool reloadable;

	private string serializerTypeName;

	private string serializerBaseTypeName;

	private string typeId;

	/// <summary>Gets a value indicating whether the root serializer supports reloading of the design document without first disposing the designer host.</summary>
	/// <returns>true if the root serializer supports reloading; otherwise, false.</returns>
	public bool Reloadable => reloadable;

	/// <summary>Gets the fully qualified type name of the serializer.</summary>
	/// <returns>The name of the type of the serializer.</returns>
	public string SerializerTypeName => serializerTypeName;

	/// <summary>Gets the fully qualified type name of the base type of the serializer.</summary>
	/// <returns>The name of the base type of the serializer.</returns>
	public string SerializerBaseTypeName => serializerBaseTypeName;

	/// <summary>Gets a unique ID for this attribute type.</summary>
	/// <returns>An object containing a unique ID for this attribute type.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.Serialization.RootDesignerSerializerAttribute" /> class using the specified attributes.</summary>
	/// <param name="serializerType">The data type of the serializer. </param>
	/// <param name="baseSerializerType">The base type of the serializer. A class can include multiple serializers as they all have different base types. </param>
	/// <param name="reloadable">true if this serializer supports dynamic reloading of the document; otherwise, false. </param>
	public RootDesignerSerializerAttribute(Type serializerType, Type baseSerializerType, bool reloadable)
	{
		serializerTypeName = serializerType.AssemblyQualifiedName;
		serializerBaseTypeName = baseSerializerType.AssemblyQualifiedName;
		this.reloadable = reloadable;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.Serialization.RootDesignerSerializerAttribute" /> class using the specified attributes.</summary>
	/// <param name="serializerTypeName">The fully qualified name of the data type of the serializer. </param>
	/// <param name="baseSerializerType">The name of the base type of the serializer. A class can include multiple serializers, as they all have different base types. </param>
	/// <param name="reloadable">true if this serializer supports dynamic reloading of the document; otherwise, false. </param>
	public RootDesignerSerializerAttribute(string serializerTypeName, Type baseSerializerType, bool reloadable)
	{
		this.serializerTypeName = serializerTypeName;
		serializerBaseTypeName = baseSerializerType.AssemblyQualifiedName;
		this.reloadable = reloadable;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.Serialization.RootDesignerSerializerAttribute" /> class using the specified attributes.</summary>
	/// <param name="serializerTypeName">The fully qualified name of the data type of the serializer. </param>
	/// <param name="baseSerializerTypeName">The name of the base type of the serializer. A class can include multiple serializers as they all have different base types. </param>
	/// <param name="reloadable">true if this serializer supports dynamic reloading of the document; otherwise, false. </param>
	public RootDesignerSerializerAttribute(string serializerTypeName, string baseSerializerTypeName, bool reloadable)
	{
		this.serializerTypeName = serializerTypeName;
		serializerBaseTypeName = baseSerializerTypeName;
		this.reloadable = reloadable;
	}
}
