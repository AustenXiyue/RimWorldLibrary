using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Hosting;

/// <summary>Discovers attributed parts in the dynamic link library (DLL) and EXE files in an application's directory and path.</summary>
public class ApplicationCatalog : ComposablePartCatalog, ICompositionElement
{
	private bool _isDisposed;

	private volatile AggregateCatalog _innerCatalog;

	private readonly object _thisLock = new object();

	private ICompositionElement _definitionOrigin;

	private ReflectionContext _reflectionContext;

	private AggregateCatalog InnerCatalog
	{
		get
		{
			if (_innerCatalog == null)
			{
				lock (_thisLock)
				{
					if (_innerCatalog == null)
					{
						string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
						Assumes.NotNull(baseDirectory);
						List<ComposablePartCatalog> list = new List<ComposablePartCatalog>();
						list.Add(CreateCatalog(baseDirectory, "*.exe"));
						list.Add(CreateCatalog(baseDirectory, "*.dll"));
						string relativeSearchPath = AppDomain.CurrentDomain.RelativeSearchPath;
						if (!string.IsNullOrEmpty(relativeSearchPath))
						{
							string[] array = relativeSearchPath.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
							foreach (string path in array)
							{
								string text = Path.Combine(baseDirectory, path);
								if (Directory.Exists(text))
								{
									list.Add(CreateCatalog(text, "*.dll"));
								}
							}
						}
						AggregateCatalog innerCatalog = new AggregateCatalog(list);
						_innerCatalog = innerCatalog;
					}
				}
			}
			return _innerCatalog;
		}
	}

	/// <summary>Gets the display name of the application catalog.</summary>
	/// <returns>A string that contains a human-readable display name of the <see cref="T:System.ComponentModel.Composition.Hosting.DirectoryCatalog" /> object.</returns>
	string ICompositionElement.DisplayName => GetDisplayName();

	/// <summary>Gets the composition element from which the application catalog originated.</summary>
	/// <returns>Always null.</returns>
	ICompositionElement ICompositionElement.Origin => null;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.ApplicationCatalog" /> class.</summary>
	public ApplicationCatalog()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.ApplicationCatalog" /> class by using the specified source for parts.</summary>
	/// <param name="definitionOrigin">The element used by diagnostics to identify the source for parts.</param>
	public ApplicationCatalog(ICompositionElement definitionOrigin)
	{
		Requires.NotNull(definitionOrigin, "definitionOrigin");
		_definitionOrigin = definitionOrigin;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.ApplicationCatalog" /> class by using the specified reflection context.</summary>
	/// <param name="reflectionContext">The reflection context.</param>
	public ApplicationCatalog(ReflectionContext reflectionContext)
	{
		Requires.NotNull(reflectionContext, "reflectionContext");
		_reflectionContext = reflectionContext;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.ApplicationCatalog" /> class by using the specified reflection context and source for parts.</summary>
	/// <param name="reflectionContext">The reflection context.</param>
	/// <param name="definitionOrigin">The element used by diagnostics to identify the source for parts.</param>
	public ApplicationCatalog(ReflectionContext reflectionContext, ICompositionElement definitionOrigin)
	{
		Requires.NotNull(reflectionContext, "reflectionContext");
		Requires.NotNull(definitionOrigin, "definitionOrigin");
		_reflectionContext = reflectionContext;
		_definitionOrigin = definitionOrigin;
	}

	internal ComposablePartCatalog CreateCatalog(string location, string pattern)
	{
		if (_reflectionContext != null)
		{
			if (_definitionOrigin == null)
			{
				return new DirectoryCatalog(location, pattern, _reflectionContext);
			}
			return new DirectoryCatalog(location, pattern, _reflectionContext, _definitionOrigin);
		}
		if (_definitionOrigin == null)
		{
			return new DirectoryCatalog(location, pattern);
		}
		return new DirectoryCatalog(location, pattern, _definitionOrigin);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.ComponentModel.Composition.Hosting.DirectoryCatalog" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!_isDisposed)
			{
				IDisposable disposable = null;
				lock (_thisLock)
				{
					disposable = _innerCatalog;
					_innerCatalog = null;
					_isDisposed = true;
				}
				disposable?.Dispose();
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	/// <summary>Returns an enumerator that iterates through the collection.</summary>
	/// <returns>An enumerator that can be used to iterate through the collection.</returns>
	public override IEnumerator<ComposablePartDefinition> GetEnumerator()
	{
		ThrowIfDisposed();
		return InnerCatalog.GetEnumerator();
	}

	/// <summary>Gets the export definitions that match the constraint expressed by the specified import definition.</summary>
	/// <returns>A collection of objects that contain the <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" /> objects and their associated <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> objects that match the specified constraint.</returns>
	/// <param name="definition">The conditions of the <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" /> objects to be returned.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.DirectoryCatalog" /> object has been disposed of.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="definition" /> is null.</exception>
	public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
	{
		ThrowIfDisposed();
		Requires.NotNull(definition, "definition");
		return InnerCatalog.GetExports(definition);
	}

	[DebuggerStepThrough]
	private void ThrowIfDisposed()
	{
		if (_isDisposed)
		{
			throw ExceptionBuilder.CreateObjectDisposed(this);
		}
	}

	private string GetDisplayName()
	{
		return string.Format(CultureInfo.CurrentCulture, "{0} (Path=\"{1}\") (PrivateProbingPath=\"{2}\")", GetType().Name, AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath);
	}

	/// <summary>Retrieves a string representation of the application catalog.</summary>
	/// <returns>A string representation of the catalog.</returns>
	public override string ToString()
	{
		return GetDisplayName();
	}
}
