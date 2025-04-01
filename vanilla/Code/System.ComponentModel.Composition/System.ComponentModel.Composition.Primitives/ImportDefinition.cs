using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Primitives;

/// <summary>Represents an import that is required by a <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> object.</summary>
public class ImportDefinition
{
	internal static readonly string EmptyContractName = string.Empty;

	private readonly Expression<Func<ExportDefinition, bool>> _constraint;

	private readonly ImportCardinality _cardinality = ImportCardinality.ExactlyOne;

	private readonly string _contractName = EmptyContractName;

	private readonly bool _isRecomposable;

	private readonly bool _isPrerequisite = true;

	private Func<ExportDefinition, bool> _compiledConstraint;

	private readonly IDictionary<string, object> _metadata = MetadataServices.EmptyMetadata;

	/// <summary>Gets the name of the contract.</summary>
	/// <returns>The contract name.</returns>
	public virtual string ContractName => _contractName;

	/// <summary>Gets the metadata associated with this import.</summary>
	/// <returns>A collection that contains the metadata associated with this import.</returns>
	public virtual IDictionary<string, object> Metadata => _metadata;

	/// <summary>Gets the cardinality of the exports required by the import definition.</summary>
	/// <returns>One of the enumeration values that indicates the cardinality of the <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects required by the <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" />. The default is <see cref="F:System.ComponentModel.Composition.Primitives.ImportCardinality.ExactlyOne" />.</returns>
	public virtual ImportCardinality Cardinality => _cardinality;

	/// <summary>Gets an expression that defines conditions that the import must satisfy to match the import definition.</summary>
	/// <returns>An expression that contains a <see cref="T:System.Func`2" /> object that defines the conditions an <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> must satisfy to match the <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" />.</returns>
	/// <exception cref="T:System.NotImplementedException">The property was not overridden by a derived class.</exception>
	public virtual Expression<Func<ExportDefinition, bool>> Constraint
	{
		get
		{
			if (_constraint != null)
			{
				return _constraint;
			}
			throw ExceptionBuilder.CreateNotOverriddenByDerived("Constraint");
		}
	}

	/// <summary>Gets a value that indicates whether the import definition must be satisfied before a part can start producing exported objects.</summary>
	/// <returns>true if the <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" /> must be satisfied before a <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> object can start producing exported objects; otherwise, false. The default is true.</returns>
	public virtual bool IsPrerequisite => _isPrerequisite;

	/// <summary>Gets a value that indicates whether the import definition can be satisfied multiple times.</summary>
	/// <returns>true if the <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" /> can be satisfied multiple times throughout the lifetime of a <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> object; otherwise, false. The default is false.</returns>
	public virtual bool IsRecomposable => _isRecomposable;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" /> class.</summary>
	protected ImportDefinition()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" /> class with the specified constraint, contract name, and cardinality, and indicates whether the import definition is recomposable or a prerequisite.</summary>
	/// <param name="constraint">An expression that contains a <see cref="T:System.Func`2" /> object that defines the conditions an <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> must match to satisfy the import definition.</param>
	/// <param name="contractName">The contract name.</param>
	/// <param name="cardinality">One of the enumeration values that indicates the cardinality of the <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects required by the import definition.</param>
	/// <param name="isRecomposable">true to specify that the import definition can be satisfied multiple times throughout the lifetime of a <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> object; otherwise, false.</param>
	/// <param name="isPrerequisite">true to specify that the import definition must be satisfied before a <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> can start producing exported objects; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="constraint" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="cardinality" /> is not one of the values of <see cref="T:System.ComponentModel.Composition.Primitives.ImportCardinality" />.</exception>
	public ImportDefinition(Expression<Func<ExportDefinition, bool>> constraint, string contractName, ImportCardinality cardinality, bool isRecomposable, bool isPrerequisite)
		: this(contractName, cardinality, isRecomposable, isPrerequisite, MetadataServices.EmptyMetadata)
	{
		Requires.NotNull(constraint, "constraint");
		_constraint = constraint;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" /> class with the specified constraint, contract name, cardinality, and metadata, and indicates whether the import definition is recomposable or a prerequisite.</summary>
	/// <param name="constraint">An expression that contains a <see cref="T:System.Func`2" /> object that defines the conditions an <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> must match to satisfy the import definition.</param>
	/// <param name="contractName">The contract name.</param>
	/// <param name="cardinality">One of the enumeration values that indicates the cardinality of the <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects required by the import definition.</param>
	/// <param name="isRecomposable">true to specify that the import definition can be satisfied multiple times throughout the lifetime of a <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> object; otherwise, false.</param>
	/// <param name="isPrerequisite">true to specify that the import definition must be satisfied before a <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> can start producing exported objects; otherwise, false.</param>
	/// <param name="metadata">The metadata associated with the import.</param>
	public ImportDefinition(Expression<Func<ExportDefinition, bool>> constraint, string contractName, ImportCardinality cardinality, bool isRecomposable, bool isPrerequisite, IDictionary<string, object> metadata)
		: this(contractName, cardinality, isRecomposable, isPrerequisite, metadata)
	{
		Requires.NotNull(constraint, "constraint");
		_constraint = constraint;
	}

	internal ImportDefinition(string contractName, ImportCardinality cardinality, bool isRecomposable, bool isPrerequisite, IDictionary<string, object> metadata)
	{
		if (cardinality != ImportCardinality.ExactlyOne && cardinality != ImportCardinality.ZeroOrMore && cardinality != 0)
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.ArgumentOutOfRange_InvalidEnum, "cardinality", cardinality, typeof(ImportCardinality).Name), "cardinality");
		}
		_contractName = contractName ?? EmptyContractName;
		_cardinality = cardinality;
		_isRecomposable = isRecomposable;
		_isPrerequisite = isPrerequisite;
		if (metadata != null)
		{
			_metadata = metadata;
		}
	}

	/// <summary>Gets a value that indicates whether the export represented by the specified definition satisfies the constraints of this import definition.</summary>
	/// <returns>true if the constraints are satisfied; otherwise, false.</returns>
	/// <param name="exportDefinition">The export definition to test.</param>
	public virtual bool IsConstraintSatisfiedBy(ExportDefinition exportDefinition)
	{
		Requires.NotNull(exportDefinition, "exportDefinition");
		if (_compiledConstraint == null)
		{
			_compiledConstraint = Constraint.Compile();
		}
		return _compiledConstraint(exportDefinition);
	}

	/// <summary>Returns a string representation of the import definition.</summary>
	/// <returns>A string representation of the import definition.</returns>
	public override string ToString()
	{
		return Constraint.Body.ToString();
	}
}
