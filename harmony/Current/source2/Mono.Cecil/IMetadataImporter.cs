namespace Mono.Cecil;

internal interface IMetadataImporter
{
	AssemblyNameReference ImportReference(AssemblyNameReference reference);

	TypeReference ImportReference(TypeReference type, IGenericParameterProvider context);

	FieldReference ImportReference(FieldReference field, IGenericParameterProvider context);

	MethodReference ImportReference(MethodReference method, IGenericParameterProvider context);
}
