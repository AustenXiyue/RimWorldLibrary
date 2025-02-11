namespace System.Xaml;

/// <summary>Provides default implementation and base class definitions for a XAML writer. This is not a working default XAML writer; you must either derive from <see cref="T:System.Xaml.XamlWriter" /> and implement its abstract members, or use an existing <see cref="T:System.Xaml.XamlWriter" /> derived class.</summary>
public abstract class XamlWriter : IDisposable
{
	/// <summary>When implemented in a derived class, gets the active XAML schema context.</summary>
	/// <returns>The active XAML schema context.</returns>
	public abstract XamlSchemaContext SchemaContext { get; }

	/// <summary>Gets whether <see cref="M:System.Xaml.XamlWriter.Dispose(System.Boolean)" /> has been called.</summary>
	/// <returns>true if <see cref="M:System.Xaml.XamlWriter.Dispose(System.Boolean)" /> has been called; otherwise, false.</returns>
	protected bool IsDisposed { get; private set; }

	/// <summary>When implemented in a derived class, produces an object for cases where the object is a default or implicit value of the property being set, instead of being specified as a discrete object value in the input XAML node set.</summary>
	public abstract void WriteGetObject();

	/// <summary>When implemented in a derived class, writes the representation of a start object node.</summary>
	/// <param name="type">The XAML type of the object to write.</param>
	public abstract void WriteStartObject(XamlType type);

	/// <summary>When implemented in a derived class, produces the representation of an end object node.</summary>
	public abstract void WriteEndObject();

	/// <summary>When implemented in a derived class, writes the representation of a start member node.</summary>
	/// <param name="xamlMember">The member node to write.</param>
	public abstract void WriteStartMember(XamlMember xamlMember);

	/// <summary>When implemented in a derived class, produces the representation of an end member node.</summary>
	public abstract void WriteEndMember();

	/// <summary>When implemented in a derived class, writes a value node.</summary>
	/// <param name="value">The value to write.</param>
	public abstract void WriteValue(object value);

	/// <summary>When implemented in a derived class, writes a XAML namespace declaration node.</summary>
	/// <param name="namespaceDeclaration">The namespace declaration to write.</param>
	public abstract void WriteNamespace(NamespaceDeclaration namespaceDeclaration);

	/// <summary>Performs switching based on node type from the XAML reader (<see cref="P:System.Xaml.XamlReader.NodeType" />) and calls the relevant Write method for the writer implementation.</summary>
	/// <param name="reader">The reader to use for node determination.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="reader" /> is null.</exception>
	/// <exception cref="T:System.NotImplementedException">The default implementation encountered a <see cref="T:System.Xaml.XamlNodeType" /> that is not in the default enumeration.</exception>
	public void WriteNode(XamlReader reader)
	{
		ArgumentNullException.ThrowIfNull(reader, "reader");
		switch (reader.NodeType)
		{
		case XamlNodeType.NamespaceDeclaration:
			WriteNamespace(reader.Namespace);
			break;
		case XamlNodeType.StartObject:
			WriteStartObject(reader.Type);
			break;
		case XamlNodeType.GetObject:
			WriteGetObject();
			break;
		case XamlNodeType.EndObject:
			WriteEndObject();
			break;
		case XamlNodeType.StartMember:
			WriteStartMember(reader.Member);
			break;
		case XamlNodeType.EndMember:
			WriteEndMember();
			break;
		case XamlNodeType.Value:
			WriteValue(reader.Value);
			break;
		default:
			throw new NotImplementedException(System.SR.MissingCaseXamlNodes);
		case XamlNodeType.None:
			break;
		}
	}

	/// <summary>See <see cref="M:System.IDisposable.Dispose" />.</summary>
	void IDisposable.Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Xaml.XamlWriter" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release the managed resources; otherwise, false.</param>
	protected virtual void Dispose(bool disposing)
	{
		IsDisposed = true;
	}

	/// <summary>Closes the XAML writer object.</summary>
	public void Close()
	{
		((IDisposable)this).Dispose();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlWriter" /> class.</summary>
	protected XamlWriter()
	{
	}
}
