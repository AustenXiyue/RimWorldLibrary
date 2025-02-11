using System.Diagnostics;
using System.Reflection;

namespace System.Windows.Markup;

/// <summary>Provides information that the <see cref="T:System.Windows.Markup.XamlTypeMapper" /> uses for mapping between an XML namespace, a CLR namespace, and the assembly that contains the relevant types for that CLR namespace.</summary>
[DebuggerDisplay("'{_xmlNamespace}'={_clrNamespace}:{_assemblyName}")]
public class NamespaceMapEntry
{
	private string _xmlNamespace;

	private string _assemblyName;

	private string _assemblyPath;

	private Assembly _assembly;

	private string _clrNamespace;

	/// <summary>Gets or sets the mapping prefix for the XML namespace being mapped to. </summary>
	/// <returns>The mapping prefix for the XML namespace.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value <see cref="P:System.Windows.Markup.NamespaceMapEntry.XmlNamespace" /> is being set to is null.</exception>
	public string XmlNamespace
	{
		get
		{
			return _xmlNamespace;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (_xmlNamespace == null)
			{
				_xmlNamespace = value;
			}
		}
	}

	/// <summary>Gets or sets the assembly name that contains the types in the CLR namespace. </summary>
	/// <returns>The assembly name.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value <see cref="P:System.Windows.Markup.NamespaceMapEntry.AssemblyName" /> is being set to is null.</exception>
	public string AssemblyName
	{
		get
		{
			return _assemblyName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (_assemblyName == null)
			{
				_assemblyName = value;
			}
		}
	}

	/// <summary>Gets or sets the CLR namespace that contains the types being mapped. </summary>
	/// <returns>The CLR namespace.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value <see cref="P:System.Windows.Markup.NamespaceMapEntry.ClrNamespace" /> is being set to is null.</exception>
	public string ClrNamespace
	{
		get
		{
			return _clrNamespace;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (_clrNamespace == null)
			{
				_clrNamespace = value;
			}
		}
	}

	internal Assembly Assembly
	{
		get
		{
			if (null == _assembly && _assemblyName.Length > 0)
			{
				_assembly = ReflectionHelper.LoadAssembly(_assemblyName, _assemblyPath);
			}
			return _assembly;
		}
	}

	internal string AssemblyPath
	{
		get
		{
			return _assemblyPath;
		}
		set
		{
			_assemblyPath = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.NamespaceMapEntry" /> class. </summary>
	public NamespaceMapEntry()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.NamespaceMapEntry" /> class, using provided XML namespace, CLR namespace, and assembly information. </summary>
	/// <param name="xmlNamespace">The mapping prefix for the XML namespace.</param>
	/// <param name="assemblyName">The assembly that contains the CLR namespace and types to map to the XML namespace.</param>
	/// <param name="clrNamespace">The CLR  namespace in the assembly that contains the relevant types.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xmlNamespace" /> is null-or- <paramref name="assemblyName" /> is null-or- <paramref name="clrNamespace" /> is null.</exception>
	public NamespaceMapEntry(string xmlNamespace, string assemblyName, string clrNamespace)
	{
		if (xmlNamespace == null)
		{
			throw new ArgumentNullException("xmlNamespace");
		}
		if (assemblyName == null)
		{
			throw new ArgumentNullException("assemblyName");
		}
		if (clrNamespace == null)
		{
			throw new ArgumentNullException("clrNamespace");
		}
		_xmlNamespace = xmlNamespace;
		_assemblyName = assemblyName;
		_clrNamespace = clrNamespace;
	}

	internal NamespaceMapEntry(string xmlNamespace, string assemblyName, string clrNamespace, string assemblyPath)
		: this(xmlNamespace, assemblyName, clrNamespace)
	{
		_assemblyPath = assemblyPath;
	}
}
