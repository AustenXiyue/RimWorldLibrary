namespace Mono.Cecil;

internal interface IReflectionImporterProvider
{
	IReflectionImporter GetReflectionImporter(ModuleDefinition module);
}
