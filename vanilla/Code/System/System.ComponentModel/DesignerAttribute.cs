using System.ComponentModel.Design;
using System.Globalization;

namespace System.ComponentModel;

/// <summary>Specifies the class used to implement design-time services for a component.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
public sealed class DesignerAttribute : Attribute
{
	private readonly string designerTypeName;

	private readonly string designerBaseTypeName;

	private string typeId;

	/// <summary>Gets the name of the base type of this designer.</summary>
	/// <returns>The name of the base type of this designer.</returns>
	public string DesignerBaseTypeName => designerBaseTypeName;

	/// <summary>Gets the name of the designer type associated with this designer attribute.</summary>
	/// <returns>The name of the designer type associated with this designer attribute.</returns>
	public string DesignerTypeName => designerTypeName;

	/// <summary>Gets a unique ID for this attribute type.</summary>
	/// <returns>A unique ID for this attribute type.</returns>
	public override object TypeId
	{
		get
		{
			if (typeId == null)
			{
				string text = designerBaseTypeName;
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

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DesignerAttribute" /> class using the name of the type that provides design-time services.</summary>
	/// <param name="designerTypeName">The concatenation of the fully qualified name of the type that provides design-time services for the component this attribute is bound to, and the name of the assembly this type resides in. </param>
	public DesignerAttribute(string designerTypeName)
	{
		designerTypeName.ToUpper(CultureInfo.InvariantCulture);
		this.designerTypeName = designerTypeName;
		designerBaseTypeName = typeof(IDesigner).FullName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DesignerAttribute" /> class using the type that provides design-time services.</summary>
	/// <param name="designerType">A <see cref="T:System.Type" /> that represents the class that provides design-time services for the component this attribute is bound to. </param>
	public DesignerAttribute(Type designerType)
	{
		designerTypeName = designerType.AssemblyQualifiedName;
		designerBaseTypeName = typeof(IDesigner).FullName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DesignerAttribute" /> class using the designer type and the base class for the designer.</summary>
	/// <param name="designerTypeName">The concatenation of the fully qualified name of the type that provides design-time services for the component this attribute is bound to, and the name of the assembly this type resides in. </param>
	/// <param name="designerBaseTypeName">The fully qualified name of the base class to associate with the designer class. </param>
	public DesignerAttribute(string designerTypeName, string designerBaseTypeName)
	{
		designerTypeName.ToUpper(CultureInfo.InvariantCulture);
		this.designerTypeName = designerTypeName;
		this.designerBaseTypeName = designerBaseTypeName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DesignerAttribute" /> class, using the name of the designer class and the base class for the designer.</summary>
	/// <param name="designerTypeName">The concatenation of the fully qualified name of the type that provides design-time services for the component this attribute is bound to, and the name of the assembly this type resides in. </param>
	/// <param name="designerBaseType">A <see cref="T:System.Type" /> that represents the base class to associate with the <paramref name="designerTypeName" />. </param>
	public DesignerAttribute(string designerTypeName, Type designerBaseType)
	{
		designerTypeName.ToUpper(CultureInfo.InvariantCulture);
		this.designerTypeName = designerTypeName;
		designerBaseTypeName = designerBaseType.AssemblyQualifiedName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DesignerAttribute" /> class using the types of the designer and designer base class.</summary>
	/// <param name="designerType">A <see cref="T:System.Type" /> that represents the class that provides design-time services for the component this attribute is bound to. </param>
	/// <param name="designerBaseType">A <see cref="T:System.Type" /> that represents the base class to associate with the <paramref name="designerType" />. </param>
	public DesignerAttribute(Type designerType, Type designerBaseType)
	{
		designerTypeName = designerType.AssemblyQualifiedName;
		designerBaseTypeName = designerBaseType.AssemblyQualifiedName;
	}

	/// <summary>Returns whether the value of the given object is equal to the current <see cref="T:System.ComponentModel.DesignerAttribute" />.</summary>
	/// <returns>true if the value of the given object is equal to that of the current; otherwise, false.</returns>
	/// <param name="obj">The object to test the value equality of. </param>
	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is DesignerAttribute designerAttribute && designerAttribute.designerBaseTypeName == designerBaseTypeName)
		{
			return designerAttribute.designerTypeName == designerTypeName;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return designerTypeName.GetHashCode() ^ designerBaseTypeName.GetHashCode();
	}
}
