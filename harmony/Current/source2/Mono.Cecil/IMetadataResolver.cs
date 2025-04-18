namespace Mono.Cecil;

internal interface IMetadataResolver
{
	TypeDefinition Resolve(TypeReference type);

	FieldDefinition Resolve(FieldReference field);

	MethodDefinition Resolve(MethodReference method);
}
