namespace System.Xaml;

/// <summary>Provides base definitions for classes that consume XAML input and produce XAML node streams.</summary>
public abstract class XamlReader : IDisposable
{
	/// <summary>When implemented in a derived class, gets the type of the current node.</summary>
	/// <returns>A value of the <see cref="T:System.Xaml.XamlNodeType" /> enumeration.</returns>
	public abstract XamlNodeType NodeType { get; }

	/// <summary>When implemented in a derived class, gets a value that reports whether the reader position is at end-of-file.</summary>
	/// <returns>true if the position is at the conceptual end-of-file of the XAML node stream; otherwise, false.</returns>
	public abstract bool IsEof { get; }

	/// <summary>When implemented in a derived class, gets the XAML namespace information from the current node.</summary>
	/// <returns>The XAML namespace information, if it is available; otherwise, null.</returns>
	public abstract NamespaceDeclaration Namespace { get; }

	/// <summary>When implemented in a derived class, gets the <see cref="T:System.Xaml.XamlType" /> of the current node.</summary>
	/// <returns>The <see cref="T:System.Xaml.XamlType" /> of the current node; or null, if the current reader position is not on an object.</returns>
	public abstract XamlType Type { get; }

	/// <summary>When implemented in a derived class, gets the value of the current node.</summary>
	/// <returns>The value of the current node; or null, if the current reader position is not on a <see cref="F:System.Xaml.XamlNodeType.Value" /> node type.</returns>
	public abstract object Value { get; }

	/// <summary>When implemented in a derived class, gets the current member at the reader position, if the reader position is on a <see cref="F:System.Xaml.XamlNodeType.StartMember" />.</summary>
	/// <returns>The current member; or null, if the reader position is not on a member.</returns>
	public abstract XamlMember Member { get; }

	/// <summary>When implemented in a derived class, gets an object that provides XAML schema context information for the information set.</summary>
	/// <returns>An object that provides XAML schema context information for the information set.</returns>
	public abstract XamlSchemaContext SchemaContext { get; }

	/// <summary>Gets whether <see cref="M:System.Xaml.XamlReader.Dispose(System.Boolean)" /> has been called.</summary>
	/// <returns>true if <see cref="M:System.Xaml.XamlReader.Dispose(System.Boolean)" /> has been called; otherwise, false.</returns>
	protected bool IsDisposed { get; private set; }

	/// <summary>When implemented in a derived class, provides the next XAML node from the source, if a node is available. </summary>
	/// <returns>true if a node is available; otherwise, false.</returns>
	public abstract bool Read();

	/// <summary>Skips the current node and advances the reader position to the next node.</summary>
	public virtual void Skip()
	{
		switch (NodeType)
		{
		case XamlNodeType.StartObject:
			SkipFromTo(XamlNodeType.StartObject, XamlNodeType.EndObject);
			break;
		case XamlNodeType.StartMember:
			SkipFromTo(XamlNodeType.StartMember, XamlNodeType.EndMember);
			break;
		}
		Read();
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Xaml.XamlReader" /> class.</summary>
	void IDisposable.Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Xaml.XamlReader" />, and optionally, releases the managed resources. </summary>
	/// <param name="disposing">true to release the managed resources; otherwise, false.</param>
	protected virtual void Dispose(bool disposing)
	{
		IsDisposed = true;
	}

	/// <summary>Closes the XAML node stream.</summary>
	public void Close()
	{
		((IDisposable)this).Dispose();
	}

	/// <summary>Returns a <see cref="T:System.Xaml.XamlReader" /> that is based on the current <see cref="T:System.Xaml.XamlReader" />, where the returned <see cref="T:System.Xaml.XamlReader" /> is used to iterate through a subtree of the XAML node structure.</summary>
	/// <returns>A new XAML reader instance for the subtree.</returns>
	public virtual XamlReader ReadSubtree()
	{
		return new XamlSubreader(this);
	}

	private void SkipFromTo(XamlNodeType startNodeType, XamlNodeType endNodeType)
	{
		int num = 1;
		while (num > 0)
		{
			Read();
			XamlNodeType nodeType = NodeType;
			if (nodeType == startNodeType)
			{
				num++;
			}
			else if (nodeType == endNodeType)
			{
				num--;
			}
		}
	}

	/// <summary>Initializes the <see cref="T:System.Xaml.XamlReader" /> class.</summary>
	protected XamlReader()
	{
	}
}
