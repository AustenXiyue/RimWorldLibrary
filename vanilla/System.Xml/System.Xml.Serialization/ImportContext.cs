using System.Collections;
using System.Collections.Specialized;

namespace System.Xml.Serialization;

/// <summary>Describes the context in which a set of schema is bound to .NET Framework code entities.</summary>
public class ImportContext
{
	private bool shareTypes;

	private SchemaObjectCache cache;

	private Hashtable mappings;

	private Hashtable elements;

	private CodeIdentifiers typeIdentifiers;

	internal SchemaObjectCache Cache
	{
		get
		{
			if (cache == null)
			{
				cache = new SchemaObjectCache();
			}
			return cache;
		}
	}

	internal Hashtable Elements
	{
		get
		{
			if (elements == null)
			{
				elements = new Hashtable();
			}
			return elements;
		}
	}

	internal Hashtable Mappings
	{
		get
		{
			if (mappings == null)
			{
				mappings = new Hashtable();
			}
			return mappings;
		}
	}

	/// <summary>Gets a set of code entities to which the context applies.</summary>
	/// <returns>A <see cref="T:System.Xml.Serialization.CodeIdentifiers" /> that specifies the code entities to which the context applies.</returns>
	public CodeIdentifiers TypeIdentifiers
	{
		get
		{
			if (typeIdentifiers == null)
			{
				typeIdentifiers = new CodeIdentifiers();
			}
			return typeIdentifiers;
		}
	}

	/// <summary>Gets a value that determines whether custom types are shared.</summary>
	/// <returns>true, if custom types are shared among schema; otherwise, false.</returns>
	public bool ShareTypes => shareTypes;

	/// <summary>Gets a collection of warnings that are generated when importing the code entity descriptions.</summary>
	/// <returns>A <see cref="T:System.Collections.Specialized.StringCollection" /> that contains warnings that were generated when importing the code entity descriptions.</returns>
	public StringCollection Warnings => Cache.Warnings;

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.ImportContext" /> class for the given code identifiers, with the given type-sharing option.</summary>
	/// <param name="identifiers">The code entities to which the context applies.</param>
	/// <param name="shareTypes">A <see cref="T:System.Boolean" /> value that determines whether custom types are shared among schema.</param>
	public ImportContext(CodeIdentifiers identifiers, bool shareTypes)
	{
		typeIdentifiers = identifiers;
		this.shareTypes = shareTypes;
	}

	internal ImportContext()
		: this(null, shareTypes: false)
	{
	}
}
