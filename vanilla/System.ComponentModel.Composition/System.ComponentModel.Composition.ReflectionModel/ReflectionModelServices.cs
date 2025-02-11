using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.ReflectionModel;

/// <summary>Provides extension methods to create and retrieve reflection-based parts.</summary>
public static class ReflectionModelServices
{
	/// <summary>Gets the type of a part from a specified part definition.</summary>
	/// <returns>The type of the defined part.</returns>
	/// <param name="partDefinition">The part definition to examine.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="partDefinition" /> is null.</exception>
	public static Lazy<Type> GetPartType(ComposablePartDefinition partDefinition)
	{
		Requires.NotNull(partDefinition, "partDefinition");
		return ((partDefinition as ReflectionComposablePartDefinition) ?? throw ExceptionBuilder.CreateReflectionModelInvalidPartDefinition("partDefinition", partDefinition.GetType())).GetLazyPartType();
	}

	/// <summary>Determines whether the specified part requires disposal.</summary>
	/// <returns>true if the part requires disposal; otherwise, false.</returns>
	/// <param name="partDefinition">The part to examine.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="partDefinition" /> is null.</exception>
	public static bool IsDisposalRequired(ComposablePartDefinition partDefinition)
	{
		Requires.NotNull(partDefinition, "partDefinition");
		return ((partDefinition as ReflectionComposablePartDefinition) ?? throw ExceptionBuilder.CreateReflectionModelInvalidPartDefinition("partDefinition", partDefinition.GetType())).IsDisposalRequired;
	}

	/// <summary>Gets the exporting member from a specified export definition.</summary>
	/// <returns>The member specified in the export definition.</returns>
	/// <param name="exportDefinition">The export definition to examine.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="exportDefinition" /> is null.</exception>
	public static LazyMemberInfo GetExportingMember(ExportDefinition exportDefinition)
	{
		Requires.NotNull(exportDefinition, "exportDefinition");
		return ((exportDefinition as ReflectionMemberExportDefinition) ?? throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.ReflectionModel_InvalidExportDefinition, exportDefinition.GetType()), "exportDefinition")).ExportingLazyMember;
	}

	/// <summary>Gets the importing member from a specified import definition.</summary>
	/// <returns>The member specified in the import definition.</returns>
	/// <param name="importDefinition">The import definition to examine.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="importDefinition" /> is null.</exception>
	public static LazyMemberInfo GetImportingMember(ImportDefinition importDefinition)
	{
		Requires.NotNull(importDefinition, "importDefinition");
		return ((importDefinition as ReflectionMemberImportDefinition) ?? throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.ReflectionModel_InvalidMemberImportDefinition, importDefinition.GetType()), "importDefinition")).ImportingLazyMember;
	}

	/// <summary>Gets the importing parameter from a specified import definition.</summary>
	/// <returns>The parameter specified in the import definition.</returns>
	/// <param name="importDefinition">The import definition to examine.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="importDefinition" /> is null.</exception>
	public static Lazy<ParameterInfo> GetImportingParameter(ImportDefinition importDefinition)
	{
		Requires.NotNull(importDefinition, "importDefinition");
		return ((importDefinition as ReflectionParameterImportDefinition) ?? throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.ReflectionModel_InvalidParameterImportDefinition, importDefinition.GetType()), "importDefinition")).ImportingLazyParameter;
	}

	/// <summary>Determines whether an import definition represents a member or a parameter.</summary>
	/// <returns>true if the import definition represents a parameter; otherwise, false.</returns>
	/// <param name="importDefinition">The import definition to examine.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="importDefinition" /> is null.</exception>
	public static bool IsImportingParameter(ImportDefinition importDefinition)
	{
		Requires.NotNull(importDefinition, "importDefinition");
		if (!(importDefinition is ReflectionImportDefinition))
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.ReflectionModel_InvalidImportDefinition, importDefinition.GetType()), "importDefinition");
		}
		return importDefinition is ReflectionParameterImportDefinition;
	}

	/// <summary>Indicates whether a specified import definition represents an export factory (<see cref="T:System.ComponentModel.Composition.ExportFactory`1" /> or <see cref="T:System.ComponentModel.Composition.ExportFactory`2" /> object).</summary>
	/// <returns>true if the specified import definition represents an export factory; otherwise, false.</returns>
	/// <param name="importDefinition">The import definition to check.</param>
	public static bool IsExportFactoryImportDefinition(ImportDefinition importDefinition)
	{
		Requires.NotNull(importDefinition, "importDefinition");
		if (!(importDefinition is ReflectionImportDefinition))
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.ReflectionModel_InvalidImportDefinition, importDefinition.GetType()), "importDefinition");
		}
		return importDefinition is IPartCreatorImportDefinition;
	}

	/// <summary>Returns a representation of an import definition as an export factory product.</summary>
	/// <returns>The representation of the import definition.</returns>
	/// <param name="importDefinition">The import definition to represent.</param>
	public static ContractBasedImportDefinition GetExportFactoryProductImportDefinition(ImportDefinition importDefinition)
	{
		Requires.NotNull(importDefinition, "importDefinition");
		return ((importDefinition as IPartCreatorImportDefinition) ?? throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.ReflectionModel_InvalidImportDefinition, importDefinition.GetType()), "importDefinition")).ProductImportDefinition;
	}

	/// <summary>Creates a part definition with the specified part type, imports, exports, metadata, and origin.</summary>
	/// <returns>A part definition created from the specified parameters.</returns>
	/// <param name="partType">The type of the part.</param>
	/// <param name="isDisposalRequired">true if the part requires disposal; otherwise, false.</param>
	/// <param name="imports">A collection of the part's imports.</param>
	/// <param name="exports">A collection of the part's exports.</param>
	/// <param name="metadata">The part's metadata.</param>
	/// <param name="origin">The part's origin.</param>
	public static ComposablePartDefinition CreatePartDefinition(Lazy<Type> partType, bool isDisposalRequired, Lazy<IEnumerable<ImportDefinition>> imports, Lazy<IEnumerable<ExportDefinition>> exports, Lazy<IDictionary<string, object>> metadata, ICompositionElement origin)
	{
		Requires.NotNull(partType, "partType");
		return new ReflectionComposablePartDefinition(new ReflectionPartCreationInfo(partType, isDisposalRequired, imports, exports, metadata, origin));
	}

	/// <summary>Creates an export definition from the specified member, with the specified contract name, metadata, and origin.</summary>
	/// <returns>An export definition created from the specified parameters.</returns>
	/// <param name="exportingMember">The member to export.</param>
	/// <param name="contractName">The contract name to use for the export.</param>
	/// <param name="metadata">The metadata for the export.</param>
	/// <param name="origin">The object that the export originates from.</param>
	public static ExportDefinition CreateExportDefinition(LazyMemberInfo exportingMember, string contractName, Lazy<IDictionary<string, object>> metadata, ICompositionElement origin)
	{
		Requires.NotNullOrEmpty(contractName, "contractName");
		Requires.IsInMembertypeSet(exportingMember.MemberType, "exportingMember", MemberTypes.Field | MemberTypes.Method | MemberTypes.Property | MemberTypes.TypeInfo | MemberTypes.NestedType);
		return new ReflectionMemberExportDefinition(exportingMember, new LazyExportDefinition(contractName, metadata), origin);
	}

	/// <summary>Creates an import definition for the specified member by using the specified contract name, type identity, import metadata, cardinality, recomposition policy, and creation policy.</summary>
	/// <returns>An import definition created from the specified parameters.</returns>
	/// <param name="importingMember">The member to import into.</param>
	/// <param name="contractName">The contract name to use for the import.</param>
	/// <param name="requiredTypeIdentity">The required type identity for the import.</param>
	/// <param name="requiredMetadata">The required metadata for the import.</param>
	/// <param name="cardinality">The cardinality of the import.</param>
	/// <param name="isRecomposable">true to indicate that the import is recomposable; otherwise, false.</param>
	/// <param name="requiredCreationPolicy">One of the enumeration values that specifies the import's creation policy.</param>
	/// <param name="origin">The object to import into.</param>
	public static ContractBasedImportDefinition CreateImportDefinition(LazyMemberInfo importingMember, string contractName, string requiredTypeIdentity, IEnumerable<KeyValuePair<string, Type>> requiredMetadata, ImportCardinality cardinality, bool isRecomposable, CreationPolicy requiredCreationPolicy, ICompositionElement origin)
	{
		return CreateImportDefinition(importingMember, contractName, requiredTypeIdentity, requiredMetadata, cardinality, isRecomposable, requiredCreationPolicy, MetadataServices.EmptyMetadata, isExportFactory: false, origin);
	}

	/// <summary>Creates an import definition for the specified member by using the specified contract name, type identity, import and contract metadata, cardinality, recomposition policy, and creation policy.</summary>
	/// <returns>An import definition created from the specified parameters.</returns>
	/// <param name="importingMember">The member to import into.</param>
	/// <param name="contractName">The contract name to use for the import.</param>
	/// <param name="requiredTypeIdentity">The required type identity for the import.</param>
	/// <param name="requiredMetadata">The required metadata for the import.</param>
	/// <param name="cardinality">The cardinality of the import.</param>
	/// <param name="isRecomposable">true to indicate that the import is recomposable; otherwise, false.</param>
	/// <param name="requiredCreationPolicy">One of the enumeration values that specifies the import's creation policy.</param>
	/// <param name="metadata">The contract metadata.</param>
	/// <param name="isExportFactory">true to indicate that the import represents an <see cref="T:System.ComponentModel.Composition.ExportFactory`1" />; otherwise, false.</param>
	/// <param name="origin">The object to import into.</param>
	public static ContractBasedImportDefinition CreateImportDefinition(LazyMemberInfo importingMember, string contractName, string requiredTypeIdentity, IEnumerable<KeyValuePair<string, Type>> requiredMetadata, ImportCardinality cardinality, bool isRecomposable, CreationPolicy requiredCreationPolicy, IDictionary<string, object> metadata, bool isExportFactory, ICompositionElement origin)
	{
		return CreateImportDefinition(importingMember, contractName, requiredTypeIdentity, requiredMetadata, cardinality, isRecomposable, isPreRequisite: false, requiredCreationPolicy, metadata, isExportFactory, origin);
	}

	/// <summary>Creates an import definition for the specified member by using the specified contract name, type identity, import and contract metadata, cardinality, recomposition policy, and creation policy.</summary>
	/// <returns>An import definition created from the specified parameters.</returns>
	/// <param name="importingMember">The member to import into.</param>
	/// <param name="contractName">The contract name to use for the import.</param>
	/// <param name="requiredTypeIdentity">The required type identity for the import.</param>
	/// <param name="requiredMetadata">The required metadata for the import.</param>
	/// <param name="cardinality">The cardinality of the import.</param>
	/// <param name="isRecomposable">true to indicate that the import is recomposable; otherwise, false.</param>
	/// <param name="isPreRequisite">true to indicate that the import is a prerequisite; otherwise, false.</param>
	/// <param name="requiredCreationPolicy">One of the enumeration values that specifies the import's creation policy.</param>
	/// <param name="metadata">The contract metadata.</param>
	/// <param name="isExportFactory">true to indicate that the import represents an <see cref="T:System.ComponentModel.Composition.ExportFactory`1" />; otherwise, false.</param>
	/// <param name="origin">The object to import into.</param>
	public static ContractBasedImportDefinition CreateImportDefinition(LazyMemberInfo importingMember, string contractName, string requiredTypeIdentity, IEnumerable<KeyValuePair<string, Type>> requiredMetadata, ImportCardinality cardinality, bool isRecomposable, bool isPreRequisite, CreationPolicy requiredCreationPolicy, IDictionary<string, object> metadata, bool isExportFactory, ICompositionElement origin)
	{
		Requires.NotNullOrEmpty(contractName, "contractName");
		Requires.IsInMembertypeSet(importingMember.MemberType, "importingMember", MemberTypes.Field | MemberTypes.Property);
		if (isExportFactory)
		{
			return new PartCreatorMemberImportDefinition(importingMember, origin, new ContractBasedImportDefinition(contractName, requiredTypeIdentity, requiredMetadata, cardinality, isRecomposable, isPreRequisite, CreationPolicy.NonShared, metadata));
		}
		return new ReflectionMemberImportDefinition(importingMember, contractName, requiredTypeIdentity, requiredMetadata, cardinality, isRecomposable, isPreRequisite, requiredCreationPolicy, metadata, origin);
	}

	/// <summary>Creates an import definition for the specified parameter by using the specified contract name, type identity, import metadata, cardinality, and creation policy.</summary>
	/// <returns>An import definition created from the specified parameters.</returns>
	/// <param name="parameter">The parameter to import.</param>
	/// <param name="contractName">The contract name to use for the import.</param>
	/// <param name="requiredTypeIdentity">The required type identity for the import.</param>
	/// <param name="requiredMetadata">The required metadata for the import.</param>
	/// <param name="cardinality">The cardinality of the import.</param>
	/// <param name="requiredCreationPolicy">One of the enumeration values that specifies the import's creation policy.</param>
	/// <param name="origin">The object to import into.</param>
	public static ContractBasedImportDefinition CreateImportDefinition(Lazy<ParameterInfo> parameter, string contractName, string requiredTypeIdentity, IEnumerable<KeyValuePair<string, Type>> requiredMetadata, ImportCardinality cardinality, CreationPolicy requiredCreationPolicy, ICompositionElement origin)
	{
		return CreateImportDefinition(parameter, contractName, requiredTypeIdentity, requiredMetadata, cardinality, requiredCreationPolicy, MetadataServices.EmptyMetadata, isExportFactory: false, origin);
	}

	/// <summary>Creates an import definition for the specified parameter by using the specified contract name, type identity, import and contract metadata, cardinality, and creation policy.</summary>
	/// <returns>An import definition created from the specified parameters.</returns>
	/// <param name="parameter">The parameter to import.</param>
	/// <param name="contractName">The contract name to use for the import.</param>
	/// <param name="requiredTypeIdentity">The required type identity for the import.</param>
	/// <param name="requiredMetadata">The required metadata for the import.</param>
	/// <param name="cardinality">The cardinality of the import.</param>
	/// <param name="requiredCreationPolicy">One of the enumeration values that specifies the import's creation policy.</param>
	/// <param name="metadata">The contract metadata</param>
	/// <param name="isExportFactory">true to indicate that the import represents an <see cref="T:System.ComponentModel.Composition.ExportFactory`1" />; otherwise, false.</param>
	/// <param name="origin">The object to import into.</param>
	public static ContractBasedImportDefinition CreateImportDefinition(Lazy<ParameterInfo> parameter, string contractName, string requiredTypeIdentity, IEnumerable<KeyValuePair<string, Type>> requiredMetadata, ImportCardinality cardinality, CreationPolicy requiredCreationPolicy, IDictionary<string, object> metadata, bool isExportFactory, ICompositionElement origin)
	{
		Requires.NotNull(parameter, "parameter");
		Requires.NotNullOrEmpty(contractName, "contractName");
		if (isExportFactory)
		{
			return new PartCreatorParameterImportDefinition(parameter, origin, new ContractBasedImportDefinition(contractName, requiredTypeIdentity, requiredMetadata, cardinality, isRecomposable: false, isPrerequisite: true, CreationPolicy.NonShared, metadata));
		}
		return new ReflectionParameterImportDefinition(parameter, contractName, requiredTypeIdentity, requiredMetadata, cardinality, requiredCreationPolicy, metadata, origin);
	}

	/// <summary>Indicates whether a generic part definition can be specialized with the provided parameters.</summary>
	/// <returns>true if the specialization succeeds; otherwise, false.</returns>
	/// <param name="partDefinition">The part definition.</param>
	/// <param name="genericParameters">A collection of types to specify the generic parameters.</param>
	/// <param name="specialization">When this method returns, contains the specialized part definition. This parameter is treated as uninitialized.</param>
	public static bool TryMakeGenericPartDefinition(ComposablePartDefinition partDefinition, IEnumerable<Type> genericParameters, out ComposablePartDefinition specialization)
	{
		Requires.NotNull(partDefinition, "partDefinition");
		specialization = null;
		return ((partDefinition as ReflectionComposablePartDefinition) ?? throw ExceptionBuilder.CreateReflectionModelInvalidPartDefinition("partDefinition", partDefinition.GetType())).TryMakeGenericPartDefinition(genericParameters.ToArray(), out specialization);
	}
}
