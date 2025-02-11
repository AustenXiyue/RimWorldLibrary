namespace System.CodeDom.Compiler;

/// <summary>Defines identifiers used to determine whether a code generator supports certain types of code elements.</summary>
[Flags]
public enum GeneratorSupport
{
	/// <summary>Indicates the generator supports arrays of arrays.</summary>
	ArraysOfArrays = 1,
	/// <summary>Indicates the generator supports a program entry point method designation. This is used when building executables.</summary>
	EntryPointMethod = 2,
	/// <summary>Indicates the generator supports goto statements.</summary>
	GotoStatements = 4,
	/// <summary>Indicates the generator supports referencing multidimensional arrays. Currently, the CodeDom cannot be used to instantiate multidimensional arrays.</summary>
	MultidimensionalArrays = 8,
	/// <summary>Indicates the generator supports static constructors.</summary>
	StaticConstructors = 0x10,
	/// <summary>Indicates the generator supports try...catch statements.</summary>
	TryCatchStatements = 0x20,
	/// <summary>Indicates the generator supports return type attribute declarations.</summary>
	ReturnTypeAttributes = 0x40,
	/// <summary>Indicates the generator supports value type declarations.</summary>
	DeclareValueTypes = 0x80,
	/// <summary>Indicates the generator supports enumeration declarations.</summary>
	DeclareEnums = 0x100,
	/// <summary>Indicates the generator supports delegate declarations.</summary>
	DeclareDelegates = 0x200,
	/// <summary>Indicates the generator supports interface declarations.</summary>
	DeclareInterfaces = 0x400,
	/// <summary>Indicates the generator supports event declarations.</summary>
	DeclareEvents = 0x800,
	/// <summary>Indicates the generator supports assembly attributes.</summary>
	AssemblyAttributes = 0x1000,
	/// <summary>Indicates the generator supports parameter attributes.</summary>
	ParameterAttributes = 0x2000,
	/// <summary>Indicates the generator supports reference and out parameters.</summary>
	ReferenceParameters = 0x4000,
	/// <summary>Indicates the generator supports chained constructor arguments.</summary>
	ChainedConstructorArguments = 0x8000,
	/// <summary>Indicates the generator supports the declaration of nested types.</summary>
	NestedTypes = 0x10000,
	/// <summary>Indicates the generator supports the declaration of members that implement multiple interfaces.</summary>
	MultipleInterfaceMembers = 0x20000,
	/// <summary>Indicates the generator supports public static members.</summary>
	PublicStaticMembers = 0x40000,
	/// <summary>Indicates the generator supports complex expressions.</summary>
	ComplexExpressions = 0x80000,
	/// <summary>Indicates the generator supports compilation with Win32 resources.</summary>
	Win32Resources = 0x100000,
	/// <summary>Indicates the generator supports compilation with .NET Framework resources. These can be default resources compiled directly into an assembly, or resources referenced in a satellite assembly.</summary>
	Resources = 0x200000,
	/// <summary>Indicates the generator supports partial type declarations.</summary>
	PartialTypes = 0x400000,
	/// <summary>Indicates the generator supports generic type references.</summary>
	GenericTypeReference = 0x800000,
	/// <summary>Indicates the generator supports generic type declarations.</summary>
	GenericTypeDeclaration = 0x1000000,
	/// <summary>Indicates the generator supports the declaration of indexer properties.</summary>
	DeclareIndexerProperties = 0x2000000
}
