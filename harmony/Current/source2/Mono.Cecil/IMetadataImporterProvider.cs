namespace Mono.Cecil;

internal interface IMetadataImporterProvider
{
	IMetadataImporter GetMetadataImporter(ModuleDefinition module);
}
