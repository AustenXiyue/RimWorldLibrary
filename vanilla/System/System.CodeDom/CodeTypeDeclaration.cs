using System.Reflection;

namespace System.CodeDom;

/// <summary>Represents a type declaration for a class, structure, interface, or enumeration.</summary>
[Serializable]
public class CodeTypeDeclaration : CodeTypeMember
{
	private readonly CodeTypeReferenceCollection _baseTypes = new CodeTypeReferenceCollection();

	private readonly CodeTypeMemberCollection _members = new CodeTypeMemberCollection();

	private bool _isEnum;

	private bool _isStruct;

	private int _populated;

	private const int BaseTypesCollection = 1;

	private const int MembersCollection = 2;

	private CodeTypeParameterCollection _typeParameters;

	/// <summary>Gets or sets the attributes of the type.</summary>
	/// <returns>A <see cref="T:System.Reflection.TypeAttributes" /> object that indicates the attributes of the type.</returns>
	public TypeAttributes TypeAttributes { get; set; } = TypeAttributes.Public;

	/// <summary>Gets the base types of the type.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeReferenceCollection" /> object that indicates the base types of the type.</returns>
	public CodeTypeReferenceCollection BaseTypes
	{
		get
		{
			if ((_populated & 1) == 0)
			{
				_populated |= 1;
				this.PopulateBaseTypes?.Invoke(this, EventArgs.Empty);
			}
			return _baseTypes;
		}
	}

	/// <summary>Gets or sets a value indicating whether the type is a class or reference type.</summary>
	/// <returns>true if the type is a class or reference type; otherwise, false.</returns>
	public bool IsClass
	{
		get
		{
			if ((TypeAttributes & TypeAttributes.ClassSemanticsMask) == 0 && !_isEnum)
			{
				return !_isStruct;
			}
			return false;
		}
		set
		{
			if (value)
			{
				TypeAttributes &= ~TypeAttributes.ClassSemanticsMask;
				TypeAttributes |= TypeAttributes.NotPublic;
				_isStruct = false;
				_isEnum = false;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether the type is a value type (struct).</summary>
	/// <returns>true if the type is a value type; otherwise, false.</returns>
	public bool IsStruct
	{
		get
		{
			return _isStruct;
		}
		set
		{
			if (value)
			{
				TypeAttributes &= ~TypeAttributes.ClassSemanticsMask;
				_isEnum = false;
			}
			_isStruct = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether the type is an enumeration.</summary>
	/// <returns>true if the type is an enumeration; otherwise, false.</returns>
	public bool IsEnum
	{
		get
		{
			return _isEnum;
		}
		set
		{
			if (value)
			{
				TypeAttributes &= ~TypeAttributes.ClassSemanticsMask;
				_isStruct = false;
			}
			_isEnum = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether the type is an interface.</summary>
	/// <returns>true if the type is an interface; otherwise, false.</returns>
	public bool IsInterface
	{
		get
		{
			return (TypeAttributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.ClassSemanticsMask;
		}
		set
		{
			if (value)
			{
				TypeAttributes &= ~TypeAttributes.ClassSemanticsMask;
				TypeAttributes |= TypeAttributes.ClassSemanticsMask;
				_isStruct = false;
				_isEnum = false;
			}
			else
			{
				TypeAttributes &= ~TypeAttributes.ClassSemanticsMask;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether the type declaration is complete or partial.</summary>
	/// <returns>true if the class or structure declaration is a partial representation of the implementation; false if the declaration is a complete implementation of the class or structure. The default is false.</returns>
	public bool IsPartial { get; set; }

	/// <summary>Gets the collection of class members for the represented type.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeMemberCollection" /> object that indicates the class members.</returns>
	public CodeTypeMemberCollection Members
	{
		get
		{
			if ((_populated & 2) == 0)
			{
				_populated |= 2;
				this.PopulateMembers?.Invoke(this, EventArgs.Empty);
			}
			return _members;
		}
	}

	/// <summary>Gets the type parameters for the type declaration.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeParameterCollection" /> that contains the type parameters for the type declaration.</returns>
	public CodeTypeParameterCollection TypeParameters => _typeParameters ?? (_typeParameters = new CodeTypeParameterCollection());

	/// <summary>Occurs when the <see cref="P:System.CodeDom.CodeTypeDeclaration.BaseTypes" /> collection is accessed for the first time.</summary>
	public event EventHandler PopulateBaseTypes;

	/// <summary>Occurs when the <see cref="P:System.CodeDom.CodeTypeDeclaration.Members" /> collection is accessed for the first time.</summary>
	public event EventHandler PopulateMembers;

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeDeclaration" /> class.</summary>
	public CodeTypeDeclaration()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeDeclaration" /> class with the specified name.</summary>
	/// <param name="name">The name for the new type. </param>
	public CodeTypeDeclaration(string name)
	{
		base.Name = name;
	}
}
