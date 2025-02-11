namespace System.ComponentModel;

/// <summary>Specifies the editor to use to change a property. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public sealed class EditorAttribute : Attribute
{
	private string baseTypeName;

	private string typeName;

	private string typeId;

	/// <summary>Gets the name of the base class or interface serving as a lookup key for this editor.</summary>
	/// <returns>The name of the base class or interface serving as a lookup key for this editor.</returns>
	public string EditorBaseTypeName => baseTypeName;

	/// <summary>Gets the name of the editor class in the <see cref="P:System.Type.AssemblyQualifiedName" /> format.</summary>
	/// <returns>The name of the editor class in the <see cref="P:System.Type.AssemblyQualifiedName" /> format.</returns>
	public string EditorTypeName => typeName;

	/// <summary>Gets a unique ID for this attribute type.</summary>
	/// <returns>A unique ID for this attribute type.</returns>
	public override object TypeId
	{
		get
		{
			if (typeId == null)
			{
				string text = baseTypeName;
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

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.EditorAttribute" /> class with the default editor, which is no editor.</summary>
	public EditorAttribute()
	{
		typeName = string.Empty;
		baseTypeName = string.Empty;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.EditorAttribute" /> class with the type name and base type name of the editor.</summary>
	/// <param name="typeName">The fully qualified type name of the editor. </param>
	/// <param name="baseTypeName">The fully qualified type name of the base class or interface to use as a lookup key for the editor. This class must be or derive from <see cref="T:System.Drawing.Design.UITypeEditor" />. </param>
	public EditorAttribute(string typeName, string baseTypeName)
	{
		typeName.ToUpperInvariant();
		this.typeName = typeName;
		this.baseTypeName = baseTypeName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.EditorAttribute" /> class with the type name and the base type.</summary>
	/// <param name="typeName">The fully qualified type name of the editor. </param>
	/// <param name="baseType">The <see cref="T:System.Type" /> of the base class or interface to use as a lookup key for the editor. This class must be or derive from <see cref="T:System.Drawing.Design.UITypeEditor" />. </param>
	public EditorAttribute(string typeName, Type baseType)
	{
		typeName.ToUpperInvariant();
		this.typeName = typeName;
		baseTypeName = baseType.AssemblyQualifiedName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.EditorAttribute" /> class with the type and the base type.</summary>
	/// <param name="type">A <see cref="T:System.Type" /> that represents the type of the editor. </param>
	/// <param name="baseType">The <see cref="T:System.Type" /> of the base class or interface to use as a lookup key for the editor. This class must be or derive from <see cref="T:System.Drawing.Design.UITypeEditor" />. </param>
	public EditorAttribute(Type type, Type baseType)
	{
		typeName = type.AssemblyQualifiedName;
		baseTypeName = baseType.AssemblyQualifiedName;
	}

	/// <summary>Returns whether the value of the given object is equal to the current <see cref="T:System.ComponentModel.EditorAttribute" />.</summary>
	/// <returns>true if the value of the given object is equal to that of the current object; otherwise, false.</returns>
	/// <param name="obj">The object to test the value equality of. </param>
	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is EditorAttribute editorAttribute && editorAttribute.typeName == typeName)
		{
			return editorAttribute.baseTypeName == baseTypeName;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
