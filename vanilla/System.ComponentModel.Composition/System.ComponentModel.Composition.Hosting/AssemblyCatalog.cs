using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Hosting;

/// <summary>Discovers attributed parts in a managed code assembly.</summary>
[DebuggerTypeProxy(typeof(AssemblyCatalogDebuggerProxy))]
public class AssemblyCatalog : ComposablePartCatalog, ICompositionElement
{
	private readonly object _thisLock = new object();

	private readonly ICompositionElement _definitionOrigin;

	private volatile Assembly _assembly;

	private volatile ComposablePartCatalog _innerCatalog;

	private int _isDisposed;

	private ReflectionContext _reflectionContext;

	private ComposablePartCatalog InnerCatalog
	{
		get
		{
			ThrowIfDisposed();
			if (_innerCatalog == null)
			{
				CatalogReflectionContextAttribute firstAttribute = _assembly.GetFirstAttribute<CatalogReflectionContextAttribute>();
				Assembly assembly = ((firstAttribute != null) ? firstAttribute.CreateReflectionContext().MapAssembly(_assembly) : _assembly);
				lock (_thisLock)
				{
					if (_innerCatalog == null)
					{
						TypeCatalog innerCatalog = ((_reflectionContext != null) ? new TypeCatalog(assembly.GetTypes(), _reflectionContext, _definitionOrigin) : new TypeCatalog(assembly.GetTypes(), _definitionOrigin));
						Thread.MemoryBarrier();
						_innerCatalog = innerCatalog;
					}
				}
			}
			return _innerCatalog;
		}
	}

	/// <summary>Gets the assembly whose attributed types are contained in the assembly catalog.</summary>
	/// <returns>The assembly whose attributed <see cref="T:System.Type" /> objects are contained in the <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" />.</returns>
	public Assembly Assembly => _assembly;

	/// <summary>Gets the display name of the <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> object.</summary>
	/// <returns>A string that represents the type and assembly of this <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> object.</returns>
	string ICompositionElement.DisplayName => GetDisplayName();

	/// <summary>Gets the composition element that this element originated from.</summary>
	/// <returns>Always null.</returns>
	ICompositionElement ICompositionElement.Origin => null;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> class with the specified code base.</summary>
	/// <param name="codeBase">A string that specifies the code base of the assembly (that is, the path to the assembly file) that contains the attributed <see cref="T:System.Type" /> objects to add to the <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> object.</param>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="codeBase" /> is not a valid assembly. -or-Version 2.0 or earlier of the common language runtime is currently loaded and <paramref name="codeBase" /> was compiled with a later version.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have path discovery permission. </exception>
	/// <exception cref="T:System.IO.FileLoadException">
	///   <paramref name="codeBase" /> could not be loaded.-or-<paramref name="codeBase" /> specified a directory.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="codeBase" /> is null.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="codeBase" /> is not found.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="codeBase" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. </exception>
	public AssemblyCatalog(string codeBase)
	{
		Requires.NotNullOrEmpty(codeBase, "codeBase");
		InitializeAssemblyCatalog(LoadAssembly(codeBase));
		_definitionOrigin = this;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> class with the specified code base and reflection context.</summary>
	/// <param name="codeBase">A string that specifies the code base of the assembly (that is, the path to the assembly file) that contains the attributed <see cref="T:System.Type" /> objects to add to the <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> object.</param>
	/// <param name="reflectionContext">The context used by the catalog to interpret types.</param>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="codeBase" /> is not a valid assembly. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="codeBase" /> was compiled with a later version.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have path discovery permission. </exception>
	/// <exception cref="T:System.IO.FileLoadException">
	///   <paramref name="codeBase" /> could not be loaded.-or-<paramref name="codeBase" /> specified a directory.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="codebase" /> or <paramref name="reflectionContext" /> is null.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="codeBase" /> is not found.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="codeBase" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. </exception>
	public AssemblyCatalog(string codeBase, ReflectionContext reflectionContext)
	{
		Requires.NotNullOrEmpty(codeBase, "codeBase");
		Requires.NotNull(reflectionContext, "reflectionContext");
		InitializeAssemblyCatalog(LoadAssembly(codeBase));
		_reflectionContext = reflectionContext;
		_definitionOrigin = this;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> class with the specified code base.</summary>
	/// <param name="codeBase">A string that specifies the code base of the assembly (that is, the path to the assembly file) that contains the attributed <see cref="T:System.Type" /> objects to add to the <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> object.</param>
	/// <param name="definitionOrigin">The element used by diagnostics to identify the sources of parts.</param>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="codeBase" /> is not a valid assembly. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="codeBase" /> was compiled with a later version.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have path discovery permission. </exception>
	/// <exception cref="T:System.IO.FileLoadException">
	///   <paramref name="codeBase" /> could not be loaded.-or-<paramref name="codeBase" /> specified a directory.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="codebase" /> or <paramref name="definitionOrigin" /> is null.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="codeBase" /> is not found.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="codeBase" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. </exception>
	public AssemblyCatalog(string codeBase, ICompositionElement definitionOrigin)
	{
		Requires.NotNullOrEmpty(codeBase, "codeBase");
		Requires.NotNull(definitionOrigin, "definitionOrigin");
		InitializeAssemblyCatalog(LoadAssembly(codeBase));
		_definitionOrigin = definitionOrigin;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> class with the specified code base and reflection context.</summary>
	/// <param name="codeBase">A string that specifies the code base of the assembly (that is, the path to the assembly file) that contains the attributed <see cref="T:System.Type" /> objects to add to the <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> object.</param>
	/// <param name="reflectionContext">The context used by the catalog to interpret types.</param>
	/// <param name="definitionOrigin">The element used by diagnostics to identify the sources of parts.</param>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="codeBase" /> is not a valid assembly. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="codeBase" /> was compiled with a later version.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have path discovery permission. </exception>
	/// <exception cref="T:System.IO.FileLoadException">
	///   <paramref name="codeBase" /> could not be loaded.-or-<paramref name="codeBase" /> specified a directory.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="codebase" />, <paramref name="definitionOrigin" /> or <paramref name="reflectionContext" /> is null.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="codeBase" /> is not found.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="codeBase" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. </exception>
	public AssemblyCatalog(string codeBase, ReflectionContext reflectionContext, ICompositionElement definitionOrigin)
	{
		Requires.NotNullOrEmpty(codeBase, "codeBase");
		Requires.NotNull(reflectionContext, "reflectionContext");
		Requires.NotNull(definitionOrigin, "definitionOrigin");
		InitializeAssemblyCatalog(LoadAssembly(codeBase));
		_reflectionContext = reflectionContext;
		_definitionOrigin = definitionOrigin;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> class with the specified assembly and reflection context.</summary>
	/// <param name="assembly">The assembly that contains the attributed <see cref="T:System.Type" /> objects to add to the <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> object.</param>
	/// <param name="reflectionContext">The context used by the catalog to interpret types.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="assembly" /> or <paramref name="reflectionContext" /> is null.-or-<paramref name="assembly" /> was loaded in the reflection-only context.</exception>
	public AssemblyCatalog(Assembly assembly, ReflectionContext reflectionContext)
	{
		Requires.NotNull(assembly, "assembly");
		Requires.NotNull(reflectionContext, "reflectionContext");
		InitializeAssemblyCatalog(assembly);
		_reflectionContext = reflectionContext;
		_definitionOrigin = this;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> class with the specified assembly and reflection context.</summary>
	/// <param name="assembly">The assembly that contains the attributed <see cref="T:System.Type" /> objects to add to the <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> object.</param>
	/// <param name="reflectionContext">The context used by the catalog to interpret types.</param>
	/// <param name="definitionOrigin">The element used by diagnostics to identify the sources of parts.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="assembly" />, <paramref name="definitionOrigin" />, or <paramref name="reflectionContext" /> is null.-or-<paramref name="assembly" /> was loaded in the reflection-only context.</exception>
	public AssemblyCatalog(Assembly assembly, ReflectionContext reflectionContext, ICompositionElement definitionOrigin)
	{
		Requires.NotNull(assembly, "assembly");
		Requires.NotNull(reflectionContext, "reflectionContext");
		Requires.NotNull(definitionOrigin, "definitionOrigin");
		InitializeAssemblyCatalog(assembly);
		_reflectionContext = reflectionContext;
		_definitionOrigin = definitionOrigin;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> class with the specified assembly.</summary>
	/// <param name="assembly">The assembly that contains the attributed <see cref="T:System.Type" /> objects to add to the <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> object.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="assembly" /> is null.-or-<paramref name="assembly" /> was loaded in the reflection-only context.</exception>
	public AssemblyCatalog(Assembly assembly)
	{
		Requires.NotNull(assembly, "assembly");
		InitializeAssemblyCatalog(assembly);
		_definitionOrigin = this;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> class with the specified assembly.</summary>
	/// <param name="assembly">The assembly that contains the attributed <see cref="T:System.Type" /> objects to add to the <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> object.</param>
	/// <param name="definitionOrigin">The element used by diagnostics to identify the sources of parts.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="assembly" /> or <paramref name="definitionOrigin" /> is null.-or-<paramref name="assembly" /> was loaded in the reflection-only context.</exception>
	public AssemblyCatalog(Assembly assembly, ICompositionElement definitionOrigin)
	{
		Requires.NotNull(assembly, "assembly");
		Requires.NotNull(definitionOrigin, "definitionOrigin");
		InitializeAssemblyCatalog(assembly);
		_definitionOrigin = definitionOrigin;
	}

	private void InitializeAssemblyCatalog(Assembly assembly)
	{
		_assembly = assembly;
	}

	/// <summary>Gets a collection of exports that match the conditions specified by the import definition.</summary>
	/// <returns>A collection of exports that match the conditions specified by <paramref name="definition" />.</returns>
	/// <param name="definition">Conditions that specify which exports to match.</param>
	public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
	{
		return InnerCatalog.GetExports(definition);
	}

	/// <summary>Gets a string representation of the assembly catalog.</summary>
	/// <returns>A representation of the assembly catalog.</returns>
	public override string ToString()
	{
		return GetDisplayName();
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		try
		{
			if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0 && disposing && _innerCatalog != null)
			{
				_innerCatalog.Dispose();
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	/// <summary>Returns an enumerator that iterates through the catalog.</summary>
	/// <returns>An enumerator that can be used to iterate through the catalog.</returns>
	public override IEnumerator<ComposablePartDefinition> GetEnumerator()
	{
		return InnerCatalog.GetEnumerator();
	}

	private void ThrowIfDisposed()
	{
		if (_isDisposed == 1)
		{
			throw ExceptionBuilder.CreateObjectDisposed(this);
		}
	}

	private string GetDisplayName()
	{
		return string.Format(CultureInfo.CurrentCulture, "{0} (Assembly=\"{1}\")", GetType().Name, Assembly.FullName);
	}

	private static Assembly LoadAssembly(string codeBase)
	{
		Requires.NotNullOrEmpty(codeBase, "codeBase");
		AssemblyName assemblyName;
		try
		{
			assemblyName = AssemblyName.GetAssemblyName(codeBase);
		}
		catch (ArgumentException)
		{
			assemblyName = new AssemblyName();
			assemblyName.CodeBase = codeBase;
		}
		return Assembly.Load(assemblyName);
	}
}
