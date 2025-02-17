using System.Collections.Generic;
using System.ComponentModel.Composition.AttributedModel;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Internal;
using Microsoft.Internal.Collections;

namespace System.ComponentModel.Composition.Hosting;

/// <summary>Discovers attributed parts from a collection of types.</summary>
[DebuggerTypeProxy(typeof(ComposablePartCatalogDebuggerProxy))]
public class TypeCatalog : ComposablePartCatalog, ICompositionElement
{
	private readonly object _thisLock = new object();

	private Type[] _types;

	private volatile List<ComposablePartDefinition> _parts;

	private volatile bool _isDisposed;

	private readonly ICompositionElement _definitionOrigin;

	private readonly Lazy<IDictionary<string, List<ComposablePartDefinition>>> _contractPartIndex;

	/// <summary>Gets the display name of the type catalog.</summary>
	/// <returns>A string containing a human-readable display name of the <see cref="T:System.ComponentModel.Composition.Hosting.TypeCatalog" />.</returns>
	string ICompositionElement.DisplayName => GetDisplayName();

	/// <summary>Gets the composition element from which the type catalog originated.</summary>
	/// <returns>Always null. </returns>
	ICompositionElement ICompositionElement.Origin => null;

	private IEnumerable<ComposablePartDefinition> PartsInternal
	{
		get
		{
			if (_parts == null)
			{
				lock (_thisLock)
				{
					if (_parts == null)
					{
						Assumes.NotNull(_types);
						List<ComposablePartDefinition> list = new List<ComposablePartDefinition>();
						Type[] types = _types;
						for (int i = 0; i < types.Length; i++)
						{
							ComposablePartDefinition composablePartDefinition = AttributedModelDiscovery.CreatePartDefinitionIfDiscoverable(types[i], _definitionOrigin);
							if (composablePartDefinition != null)
							{
								list.Add(composablePartDefinition);
							}
						}
						Thread.MemoryBarrier();
						_types = null;
						_parts = list;
					}
				}
			}
			return _parts;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.TypeCatalog" /> class with the specified types.</summary>
	/// <param name="types">An array of attributed <see cref="T:System.Type" /> objects to add to the <see cref="T:System.ComponentModel.Composition.Hosting.TypeCatalog" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="types" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="types" /> contains an element that is null.-or-<paramref name="types" /> contains an element that was loaded in the reflection-only context.</exception>
	public TypeCatalog(params Type[] types)
		: this((IEnumerable<Type>)types)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.TypeCatalog" /> class with the specified types.</summary>
	/// <param name="types">A collection of attributed <see cref="T:System.Type" /> objects to add to the <see cref="T:System.ComponentModel.Composition.Hosting.TypeCatalog" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="types" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="types" /> contains an element that is null.-or-<paramref name="types" /> contains an element that was loaded in the reflection-only context.</exception>
	public TypeCatalog(IEnumerable<Type> types)
	{
		Requires.NotNull(types, "types");
		InitializeTypeCatalog(types);
		_definitionOrigin = this;
		_contractPartIndex = new Lazy<IDictionary<string, List<ComposablePartDefinition>>>(CreateIndex, isThreadSafe: true);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.TypeCatalog" /> class with the specified types and source for parts.</summary>
	/// <param name="types">A collection of attributed <see cref="T:System.Type" /> objects to add to the <see cref="T:System.ComponentModel.Composition.Hosting.TypeCatalog" /> object.</param>
	/// <param name="definitionOrigin">An element used by diagnostics to identify the source for parts.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="types" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="types" /> contains an element that is null.-or-<paramref name="types" /> contains an element that was loaded in the reflection-only context.</exception>
	public TypeCatalog(IEnumerable<Type> types, ICompositionElement definitionOrigin)
	{
		Requires.NotNull(types, "types");
		Requires.NotNull(definitionOrigin, "definitionOrigin");
		InitializeTypeCatalog(types);
		_definitionOrigin = definitionOrigin;
		_contractPartIndex = new Lazy<IDictionary<string, List<ComposablePartDefinition>>>(CreateIndex, isThreadSafe: true);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.TypeCatalog" /> class with the specified types in the specified reflection context.</summary>
	/// <param name="types">A collection of attributed <see cref="T:System.Type" /> objects to add to the <see cref="T:System.ComponentModel.Composition.Hosting.TypeCatalog" /> object.</param>
	/// <param name="reflectionContext">The context used to interpret the types.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="types" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="types" /> contains an element that is null.-or-<paramref name="types" /> contains an element that was loaded in the reflection-only context.</exception>
	public TypeCatalog(IEnumerable<Type> types, ReflectionContext reflectionContext)
	{
		Requires.NotNull(types, "types");
		Requires.NotNull(reflectionContext, "reflectionContext");
		InitializeTypeCatalog(types, reflectionContext);
		_definitionOrigin = this;
		_contractPartIndex = new Lazy<IDictionary<string, List<ComposablePartDefinition>>>(CreateIndex, isThreadSafe: true);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.TypeCatalog" /> class with the specified types in the specified reflection context and source for parts.</summary>
	/// <param name="types">A collection of attributed <see cref="T:System.Type" /> objects to add to the <see cref="T:System.ComponentModel.Composition.Hosting.TypeCatalog" /> object.</param>
	/// <param name="reflectionContext">The context used to interpret the types.</param>
	/// <param name="definitionOrigin">An element used by diagnostics to identify the source for parts.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="types" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="types" /> contains an element that is null.-or-<paramref name="types" /> contains an element that was loaded in the reflection-only context.</exception>
	public TypeCatalog(IEnumerable<Type> types, ReflectionContext reflectionContext, ICompositionElement definitionOrigin)
	{
		Requires.NotNull(types, "types");
		Requires.NotNull(reflectionContext, "reflectionContext");
		Requires.NotNull(definitionOrigin, "definitionOrigin");
		InitializeTypeCatalog(types, reflectionContext);
		_definitionOrigin = definitionOrigin;
		_contractPartIndex = new Lazy<IDictionary<string, List<ComposablePartDefinition>>>(CreateIndex, isThreadSafe: true);
	}

	private void InitializeTypeCatalog(IEnumerable<Type> types, ReflectionContext reflectionContext)
	{
		List<Type> list = new List<Type>();
		foreach (Type type in types)
		{
			if (type == null)
			{
				throw ExceptionBuilder.CreateContainsNullElement("types");
			}
			if (type.Assembly.ReflectionOnly)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.Argument_ElementReflectionOnlyType, "types"), "types");
			}
			TypeInfo typeInfo = type.GetTypeInfo();
			TypeInfo typeInfo2 = ((reflectionContext != null) ? reflectionContext.MapType(typeInfo) : typeInfo);
			if (typeInfo2 != null)
			{
				if (typeInfo2.Assembly.ReflectionOnly)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.Argument_ReflectionContextReturnsReflectionOnlyType, "reflectionContext"), "reflectionContext");
				}
				list.Add(typeInfo2);
			}
		}
		_types = list.ToArray();
	}

	private void InitializeTypeCatalog(IEnumerable<Type> types)
	{
		foreach (Type type in types)
		{
			if (type == null)
			{
				throw ExceptionBuilder.CreateContainsNullElement("types");
			}
		}
		_types = types.ToArray();
	}

	/// <summary>Returns an enumerator that iterates through the catalog.</summary>
	/// <returns>An enumerator that can be used to iterate through the catalog.</returns>
	public override IEnumerator<ComposablePartDefinition> GetEnumerator()
	{
		ThrowIfDisposed();
		return PartsInternal.GetEnumerator();
	}

	internal override IEnumerable<ComposablePartDefinition> GetCandidateParts(ImportDefinition definition)
	{
		Assumes.NotNull(definition);
		string contractName = definition.ContractName;
		if (string.IsNullOrEmpty(contractName))
		{
			return PartsInternal;
		}
		string value = definition.Metadata.GetValue<string>("System.ComponentModel.Composition.GenericContractName");
		List<ComposablePartDefinition> candidateParts = GetCandidateParts(contractName);
		List<ComposablePartDefinition> candidateParts2 = GetCandidateParts(value);
		return candidateParts.ConcatAllowingNull(candidateParts2);
	}

	private List<ComposablePartDefinition> GetCandidateParts(string contractName)
	{
		if (contractName == null)
		{
			return null;
		}
		List<ComposablePartDefinition> value = null;
		_contractPartIndex.Value.TryGetValue(contractName, out value);
		return value;
	}

	private IDictionary<string, List<ComposablePartDefinition>> CreateIndex()
	{
		Dictionary<string, List<ComposablePartDefinition>> dictionary = new Dictionary<string, List<ComposablePartDefinition>>(StringComparers.ContractName);
		foreach (ComposablePartDefinition item in PartsInternal)
		{
			foreach (string item2 in item.ExportDefinitions.Select((ExportDefinition export) => export.ContractName).Distinct())
			{
				List<ComposablePartDefinition> value = null;
				if (!dictionary.TryGetValue(item2, out value))
				{
					value = new List<ComposablePartDefinition>();
					dictionary.Add(item2, value);
				}
				value.Add(item);
			}
		}
		return dictionary;
	}

	/// <summary>Returns a string representation of the type catalog.</summary>
	/// <returns>A string representation of the type catalog.</returns>
	public override string ToString()
	{
		return GetDisplayName();
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.ComponentModel.Composition.Hosting.TypeCatalog" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_isDisposed = true;
		}
		base.Dispose(disposing);
	}

	private string GetDisplayName()
	{
		return string.Format(CultureInfo.CurrentCulture, Strings.TypeCatalog_DisplayNameFormat, GetType().Name, GetTypesDisplay());
	}

	private string GetTypesDisplay()
	{
		int num = PartsInternal.Count();
		if (num == 0)
		{
			return Strings.TypeCatalog_Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ReflectionComposablePartDefinition item in PartsInternal.Take(2))
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(item.GetPartType().GetDisplayName());
		}
		if (num > 2)
		{
			stringBuilder.Append(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
			stringBuilder.Append(" ...");
		}
		return stringBuilder.ToString();
	}

	[DebuggerStepThrough]
	private void ThrowIfDisposed()
	{
		if (_isDisposed)
		{
			throw ExceptionBuilder.CreateObjectDisposed(this);
		}
	}
}
