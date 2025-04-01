using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition.Hosting;

/// <summary>Contains static metadata keys used by the composition system.</summary>
public static class CompositionConstants
{
	private const string CompositionNamespace = "System.ComponentModel.Composition";

	/// <summary>Specifies the metadata key created by the composition system to mark a part with a creation policy.</summary>
	public const string PartCreationPolicyMetadataName = "System.ComponentModel.Composition.CreationPolicy";

	/// <summary>Specifies the metadata key created by the composition system to mark an import source.</summary>
	public const string ImportSourceMetadataName = "System.ComponentModel.Composition.ImportSource";

	/// <summary>Specifies the metadata key created by the composition system to mark an IsGenericPart method.</summary>
	public const string IsGenericPartMetadataName = "System.ComponentModel.Composition.IsGenericPart";

	/// <summary>Specifies the metadata key created by the composition system to mark a generic contract.</summary>
	public const string GenericContractMetadataName = "System.ComponentModel.Composition.GenericContractName";

	/// <summary>Specifies the metadata key created by the composition system to mark generic parameters.</summary>
	public const string GenericParametersMetadataName = "System.ComponentModel.Composition.GenericParameters";

	/// <summary>Specifies the metadata key created by the composition system to mark a part with a unique identifier.</summary>
	public const string ExportTypeIdentityMetadataName = "ExportTypeIdentity";

	internal const string GenericImportParametersOrderMetadataName = "System.ComponentModel.Composition.GenericImportParametersOrderMetadataName";

	internal const string GenericExportParametersOrderMetadataName = "System.ComponentModel.Composition.GenericExportParametersOrderMetadataName";

	internal const string GenericPartArityMetadataName = "System.ComponentModel.Composition.GenericPartArity";

	internal const string GenericParameterConstraintsMetadataName = "System.ComponentModel.Composition.GenericParameterConstraints";

	internal const string GenericParameterAttributesMetadataName = "System.ComponentModel.Composition.GenericParameterAttributes";

	internal const string ProductDefinitionMetadataName = "ProductDefinition";

	internal const string PartCreatorContractName = "System.ComponentModel.Composition.Contracts.ExportFactory";

	internal static readonly string PartCreatorTypeIdentity = AttributedModelServices.GetTypeIdentity(typeof(ComposablePartDefinition));
}
