using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Diagnostics;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Internal;
using Microsoft.Internal.Collections;

namespace System.ComponentModel.Composition.Hosting;

/// <summary>Discovers attributed parts in the assemblies in a specified directory.</summary>
[DebuggerTypeProxy(typeof(DirectoryCatalogDebuggerProxy))]
public class DirectoryCatalog : ComposablePartCatalog, INotifyComposablePartCatalogChanged, ICompositionElement
{
	internal class DirectoryCatalogDebuggerProxy
	{
		private readonly DirectoryCatalog _catalog;

		public ReadOnlyCollection<Assembly> Assemblies => _catalog._assemblyCatalogs.Values.Select((AssemblyCatalog catalog) => catalog.Assembly).ToReadOnlyCollection();

		public ReflectionContext ReflectionContext => _catalog._reflectionContext;

		public string SearchPattern => _catalog.SearchPattern;

		public string Path => _catalog._path;

		public string FullPath => _catalog._fullPath;

		public ReadOnlyCollection<string> LoadedFiles => _catalog._loadedFiles;

		public ReadOnlyCollection<ComposablePartDefinition> Parts => _catalog.Parts.ToReadOnlyCollection();

		public DirectoryCatalogDebuggerProxy(DirectoryCatalog catalog)
		{
			Requires.NotNull(catalog, "catalog");
			_catalog = catalog;
		}
	}

	private readonly Lock _thisLock = new Lock();

	private readonly ICompositionElement _definitionOrigin;

	private ComposablePartCatalogCollection _catalogCollection;

	private Dictionary<string, AssemblyCatalog> _assemblyCatalogs;

	private volatile bool _isDisposed;

	private string _path;

	private string _fullPath;

	private string _searchPattern;

	private ReadOnlyCollection<string> _loadedFiles;

	private readonly ReflectionContext _reflectionContext;

	/// <summary>Gets the translated absolute path observed by the <see cref="T:System.ComponentModel.Composition.Hosting.DirectoryCatalog" /> object.</summary>
	/// <returns>The translated absolute path observed by the catalog.</returns>
	public string FullPath => _fullPath;

	/// <summary>Gets the collection of files currently loaded in the catalog.</summary>
	/// <returns>A collection of files currently loaded in the catalog.</returns>
	public ReadOnlyCollection<string> LoadedFiles
	{
		get
		{
			using (new ReadLock(_thisLock))
			{
				return _loadedFiles;
			}
		}
	}

	/// <summary>Gets the path observed by the <see cref="T:System.ComponentModel.Composition.Hosting.DirectoryCatalog" /> object.</summary>
	/// <returns>The path observed by the catalog.</returns>
	public string Path => _path;

	/// <summary>Gets the search pattern that is passed into the constructor of the <see cref="T:System.ComponentModel.Composition.Hosting.DirectoryCatalog" /> object.</summary>
	/// <returns>The search pattern the catalog uses to find files. The default is *.dll, which returns all DLL files.</returns>
	public string SearchPattern => _searchPattern;

	/// <summary>Gets the display name of the directory catalog.</summary>
	/// <returns>A string that contains a human-readable display name of the directory catalog.</returns>
	string ICompositionElement.DisplayName => GetDisplayName();

	/// <summary>Gets the composition element from which the directory catalog originated.</summary>
	/// <returns>Always null.</returns>
	ICompositionElement ICompositionElement.Origin => null;

	/// <summary>Occurs when the contents of the catalog has changed.</summary>
	public event EventHandler<ComposablePartCatalogChangeEventArgs> Changed;

	/// <summary>Occurs when the catalog is changing.</summary>
	public event EventHandler<ComposablePartCatalogChangeEventArgs> Changing;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.DirectoryCatalog" /> class by using <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> objects based on all the DLL files in the specified directory path.</summary>
	/// <param name="path">The path to the directory to scan for assemblies to add to the catalog.The path must be absolute or relative to <see cref="P:System.AppDomain.BaseDirectory" />.</param>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified <paramref name="path" /> is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more implementation-specific invalid characters.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified <paramref name="path" />, file name, or both exceed the system-defined maximum length. For example, on Windows-based computers, paths must be less than 248 characters and file names must be less than 260 characters. </exception>
	public DirectoryCatalog(string path)
		: this(path, "*.dll")
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.DirectoryCatalog" /> class by using <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> objects based on all the DLL files in the specified directory path, in the specified reflection context.</summary>
	/// <param name="path">The path to the directory to scan for assemblies to add to the catalog.The path must be absolute or relative to <see cref="P:System.AppDomain.BaseDirectory" />.</param>
	/// <param name="reflectionContext">The context used to create parts.</param>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified <paramref name="path" /> is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more implementation-specific invalid characters.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified <paramref name="path" />, file name, or both exceed the system-defined maximum length. For example, on Windows-based computers, paths must be less than 248 characters and file names must be less than 260 characters. </exception>
	public DirectoryCatalog(string path, ReflectionContext reflectionContext)
		: this(path, "*.dll", reflectionContext)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.DirectoryCatalog" /> class by using <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> objects based on all the DLL files in the specified directory path with the specified source for parts.</summary>
	/// <param name="path">The path to the directory to scan for assemblies to add to the catalog.The path must be absolute or relative to <see cref="P:System.AppDomain.BaseDirectory" />.</param>
	/// <param name="definitionOrigin">The element used by diagnostics to identify the source for parts.</param>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified <paramref name="path" /> is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more implementation-specific invalid characters.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified <paramref name="path" />, file name, or both exceed the system-defined maximum length. For example, on Windows-based computers, paths must be less than 248 characters and file names must be less than 260 characters. </exception>
	public DirectoryCatalog(string path, ICompositionElement definitionOrigin)
		: this(path, "*.dll", definitionOrigin)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.DirectoryCatalog" /> class by  using <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> objects based on all the DLL files in the specified directory path, in the specified reflection context.</summary>
	/// <param name="path">The path to the directory to scan for assemblies to add to the catalog.The path must be absolute or relative to <see cref="P:System.AppDomain.BaseDirectory" />.</param>
	/// <param name="reflectionContext">The context used to create parts.</param>
	/// <param name="definitionOrigin">The element used by diagnostics to identify the source for parts.</param>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified <paramref name="path" /> is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more implementation-specific invalid characters.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified <paramref name="path" />, file name, or both exceed the system-defined maximum length. For example, on Windows-based computers, paths must be less than 248 characters and file names must be less than 260 characters. </exception>
	public DirectoryCatalog(string path, ReflectionContext reflectionContext, ICompositionElement definitionOrigin)
		: this(path, "*.dll", reflectionContext, definitionOrigin)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.DirectoryCatalog" /> class by using <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> objects that match a specified search pattern in the specified directory path.</summary>
	/// <param name="path">The path to the directory to scan for assemblies to add to the catalog.The path must be absolute or relative to <see cref="P:System.AppDomain.BaseDirectory" />.</param>
	/// <param name="searchPattern">The search string. The format of the string should be the same as specified for the <see cref="M:System.IO.Directory.GetFiles(System.String,System.String)" /> method.</param>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified <paramref name="path" /> is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> or <paramref name="searchPattern" /> is null.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more implementation-specific invalid characters.-or-<paramref name="searchPattern" /> does not contain a valid pattern.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified <paramref name="path" />, file name, or both exceed the system-defined maximum length. For example, on Windows-based computers, paths must be less than 248 characters and file names must be less than 260 characters.</exception>
	public DirectoryCatalog(string path, string searchPattern)
	{
		Requires.NotNullOrEmpty(path, "path");
		Requires.NotNullOrEmpty(searchPattern, "searchPattern");
		_definitionOrigin = this;
		Initialize(path, searchPattern);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.DirectoryCatalog" /> class by using <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> objects based on the specified search pattern in the specified directory path with the specified source for parts.</summary>
	/// <param name="path">The path to the directory to scan for assemblies to add to the catalog.The path must be absolute or relative to <see cref="P:System.AppDomain.BaseDirectory" />.</param>
	/// <param name="searchPattern">The search string. The format of the string should be the same as specified for the <see cref="M:System.IO.Directory.GetFiles(System.String,System.String)" /> method.</param>
	/// <param name="definitionOrigin">The element used by diagnostics to identify the source for parts.</param>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified <paramref name="path" /> is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> or <paramref name="searchPattern" /> is null.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more implementation-specific invalid characters. -or-<paramref name="searchPattern" /> does not contain a valid pattern.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified <paramref name="path" />, file name, or both exceed the system-defined maximum length. For example, on Windows-based computers, paths must be less than 248 characters and file names must be less than 260 characters.</exception>
	public DirectoryCatalog(string path, string searchPattern, ICompositionElement definitionOrigin)
	{
		Requires.NotNullOrEmpty(path, "path");
		Requires.NotNullOrEmpty(searchPattern, "searchPattern");
		Requires.NotNull(definitionOrigin, "definitionOrigin");
		_definitionOrigin = definitionOrigin;
		Initialize(path, searchPattern);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.DirectoryCatalog" /> class by using <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> objects based on the specified search pattern in the specified directory path, using the specified reflection context.</summary>
	/// <param name="path">The path to the directory to scan for assemblies to add to the catalog.The path must be absolute or relative to <see cref="P:System.AppDomain.BaseDirectory" />.</param>
	/// <param name="searchPattern">The search string. The format of the string should be the same as specified for the <see cref="M:System.IO.Directory.GetFiles(System.String,System.String)" /> method.</param>
	/// <param name="reflectionContext">The context used to create parts.</param>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified <paramref name="path" /> is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> or <paramref name="searchPattern" /> is null.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more implementation-specific invalid characters.-or-<paramref name="searchPattern" /> does not contain a valid pattern.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified <paramref name="path" />, file name, or both exceed the system-defined maximum length. For example, on Windows-based computers, paths must be less than 248 characters and file names must be less than 260 characters.</exception>
	public DirectoryCatalog(string path, string searchPattern, ReflectionContext reflectionContext)
	{
		Requires.NotNullOrEmpty(path, "path");
		Requires.NotNullOrEmpty(searchPattern, "searchPattern");
		Requires.NotNull(reflectionContext, "reflectionContext");
		_reflectionContext = reflectionContext;
		_definitionOrigin = this;
		Initialize(path, searchPattern);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.DirectoryCatalog" /> class by using <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> objects based on the specified search pattern in the specified directory path, using the specified reflection context.</summary>
	/// <param name="path">The path to the directory to scan for assemblies to add to the catalog.The path must be absolute or relative to <see cref="P:System.AppDomain.BaseDirectory" />.</param>
	/// <param name="searchPattern">The search string. The format of the string should be the same as specified for the <see cref="M:System.IO.Directory.GetFiles(System.String,System.String)" /> method.</param>
	/// <param name="reflectionContext">The context used to create parts.</param>
	/// <param name="definitionOrigin">The element used by diagnostics to identify the source for parts.</param>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified <paramref name="path" /> is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> or <paramref name="searchPattern" /> is null.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more implementation-specific invalid characters.-or-<paramref name="searchPattern" /> does not contain a valid pattern.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified <paramref name="path" />, file name, or both exceed the system-defined maximum length. For example, on Windows-based computers, paths must be less than 248 characters and file names must be less than 260 characters.</exception>
	public DirectoryCatalog(string path, string searchPattern, ReflectionContext reflectionContext, ICompositionElement definitionOrigin)
	{
		Requires.NotNullOrEmpty(path, "path");
		Requires.NotNullOrEmpty(searchPattern, "searchPattern");
		Requires.NotNull(reflectionContext, "reflectionContext");
		Requires.NotNull(definitionOrigin, "definitionOrigin");
		_reflectionContext = reflectionContext;
		_definitionOrigin = definitionOrigin;
		Initialize(path, searchPattern);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.ComponentModel.Composition.Hosting.DirectoryCatalog" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!disposing || _isDisposed)
			{
				return;
			}
			bool flag = false;
			ComposablePartCatalogCollection composablePartCatalogCollection = null;
			try
			{
				using (new WriteLock(_thisLock))
				{
					if (!_isDisposed)
					{
						flag = true;
						composablePartCatalogCollection = _catalogCollection;
						_catalogCollection = null;
						_assemblyCatalogs = null;
						_isDisposed = true;
					}
				}
			}
			finally
			{
				composablePartCatalogCollection?.Dispose();
				if (flag)
				{
					_thisLock.Dispose();
				}
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
		return _catalogCollection.SelectMany((ComposablePartCatalog catalog) => catalog).GetEnumerator();
	}

	/// <summary>Gets the export definitions that match the constraint expressed by the specified import definition.</summary>
	/// <returns>A collection of objects that contain the <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" /> objects and their associated <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> objects that match the constraint specified by <paramref name="definition" />.</returns>
	/// <param name="definition">The conditions of the <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" /> objects to be returned.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.DirectoryCatalog" /> object has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="definition" /> is null.</exception>
	public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
	{
		ThrowIfDisposed();
		Requires.NotNull(definition, "definition");
		return _catalogCollection.SelectMany((ComposablePartCatalog catalog) => catalog.GetExports(definition));
	}

	/// <summary>Raises the <see cref="E:System.ComponentModel.Composition.Hosting.DirectoryCatalog.Changed" /> event.</summary>
	/// <param name="e">An object  that contains the event data. </param>
	protected virtual void OnChanged(ComposablePartCatalogChangeEventArgs e)
	{
		this.Changed?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.ComponentModel.Composition.Hosting.DirectoryCatalog.Changing" /> event.</summary>
	/// <param name="e">An object  that contains the event data.</param>
	protected virtual void OnChanging(ComposablePartCatalogChangeEventArgs e)
	{
		this.Changing?.Invoke(this, e);
	}

	/// <summary>Refreshes the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> objects with the latest files in the directory that match the search pattern. </summary>
	public void Refresh()
	{
		ThrowIfDisposed();
		Assumes.NotNull(_loadedFiles);
		ComposablePartDefinition[] addedDefinitions;
		ComposablePartDefinition[] removedDefinitions;
		while (true)
		{
			string[] files = GetFiles();
			object loadedFiles;
			string[] beforeFiles;
			using (new ReadLock(_thisLock))
			{
				loadedFiles = _loadedFiles;
				beforeFiles = _loadedFiles.ToArray();
			}
			DiffChanges(beforeFiles, files, out var catalogsToAdd, out var catalogsToRemove);
			if (catalogsToAdd.Count == 0 && catalogsToRemove.Count == 0)
			{
				return;
			}
			addedDefinitions = catalogsToAdd.SelectMany((Tuple<string, AssemblyCatalog> cat) => cat.Item2).ToArray();
			removedDefinitions = catalogsToRemove.SelectMany((Tuple<string, AssemblyCatalog> cat) => cat.Item2).ToArray();
			using AtomicComposition atomicComposition = new AtomicComposition();
			ComposablePartCatalogChangeEventArgs e = new ComposablePartCatalogChangeEventArgs(addedDefinitions, removedDefinitions, atomicComposition);
			OnChanging(e);
			using (new WriteLock(_thisLock))
			{
				if (loadedFiles != _loadedFiles)
				{
					continue;
				}
				foreach (Tuple<string, AssemblyCatalog> item in catalogsToAdd)
				{
					_assemblyCatalogs.Add(item.Item1, item.Item2);
					_catalogCollection.Add(item.Item2);
				}
				foreach (Tuple<string, AssemblyCatalog> item2 in catalogsToRemove)
				{
					_assemblyCatalogs.Remove(item2.Item1);
					_catalogCollection.Remove(item2.Item2);
				}
				_loadedFiles = files.ToReadOnlyCollection();
				atomicComposition.Complete();
				break;
			}
		}
		ComposablePartCatalogChangeEventArgs e2 = new ComposablePartCatalogChangeEventArgs(addedDefinitions, removedDefinitions, null);
		OnChanged(e2);
	}

	/// <summary>Gets a string representation of the directory catalog.</summary>
	/// <returns>A string representation of the catalog.</returns>
	public override string ToString()
	{
		return GetDisplayName();
	}

	private AssemblyCatalog CreateAssemblyCatalogGuarded(string assemblyFilePath)
	{
		Exception ex = null;
		try
		{
			return (_reflectionContext != null) ? new AssemblyCatalog(assemblyFilePath, _reflectionContext, this) : new AssemblyCatalog(assemblyFilePath, this);
		}
		catch (FileNotFoundException ex2)
		{
			ex = ex2;
		}
		catch (FileLoadException ex3)
		{
			ex = ex3;
		}
		catch (BadImageFormatException ex4)
		{
			ex = ex4;
		}
		catch (ReflectionTypeLoadException ex5)
		{
			ex = ex5;
		}
		CompositionTrace.AssemblyLoadFailed(this, assemblyFilePath, ex);
		return null;
	}

	private void DiffChanges(string[] beforeFiles, string[] afterFiles, out List<Tuple<string, AssemblyCatalog>> catalogsToAdd, out List<Tuple<string, AssemblyCatalog>> catalogsToRemove)
	{
		catalogsToAdd = new List<Tuple<string, AssemblyCatalog>>();
		catalogsToRemove = new List<Tuple<string, AssemblyCatalog>>();
		foreach (string item in afterFiles.Except(beforeFiles))
		{
			AssemblyCatalog assemblyCatalog = CreateAssemblyCatalogGuarded(item);
			if (assemblyCatalog != null)
			{
				catalogsToAdd.Add(new Tuple<string, AssemblyCatalog>(item, assemblyCatalog));
			}
		}
		IEnumerable<string> enumerable = beforeFiles.Except(afterFiles);
		using (new ReadLock(_thisLock))
		{
			foreach (string item2 in enumerable)
			{
				if (_assemblyCatalogs.TryGetValue(item2, out var value))
				{
					catalogsToRemove.Add(new Tuple<string, AssemblyCatalog>(item2, value));
				}
			}
		}
	}

	private string GetDisplayName()
	{
		return string.Format(CultureInfo.CurrentCulture, "{0} (Path=\"{1}\")", GetType().Name, _path);
	}

	private string[] GetFiles()
	{
		return Directory.GetFiles(_fullPath, _searchPattern);
	}

	private static string GetFullPath(string path)
	{
		if (!System.IO.Path.IsPathRooted(path) && AppDomain.CurrentDomain.BaseDirectory != null)
		{
			path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
		}
		return System.IO.Path.GetFullPath(path);
	}

	private void Initialize(string path, string searchPattern)
	{
		_path = path;
		_fullPath = GetFullPath(path);
		_searchPattern = searchPattern;
		_assemblyCatalogs = new Dictionary<string, AssemblyCatalog>();
		_catalogCollection = new ComposablePartCatalogCollection(null, null, null);
		_loadedFiles = GetFiles().ToReadOnlyCollection();
		foreach (string loadedFile in _loadedFiles)
		{
			AssemblyCatalog assemblyCatalog = null;
			assemblyCatalog = CreateAssemblyCatalogGuarded(loadedFile);
			if (assemblyCatalog != null)
			{
				_assemblyCatalogs.Add(loadedFile, assemblyCatalog);
				_catalogCollection.Add(assemblyCatalog);
			}
		}
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
