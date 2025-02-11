namespace System.Xaml;

internal class WriterDelegate : XamlWriter, IXamlLineInfoConsumer
{
	private XamlNodeAddDelegate _addDelegate;

	private XamlLineInfoAddDelegate _addLineInfoDelegate;

	private XamlSchemaContext _schemaContext;

	public override XamlSchemaContext SchemaContext => _schemaContext;

	public bool ShouldProvideLineInfo
	{
		get
		{
			ThrowIsDisposed();
			return _addLineInfoDelegate != null;
		}
	}

	public WriterDelegate(XamlNodeAddDelegate add, XamlLineInfoAddDelegate addlineInfoDelegate, XamlSchemaContext xamlSchemaContext)
	{
		_addDelegate = add;
		_addLineInfoDelegate = addlineInfoDelegate;
		_schemaContext = xamlSchemaContext;
	}

	public override void WriteGetObject()
	{
		ThrowIsDisposed();
		_addDelegate(XamlNodeType.GetObject, null);
	}

	public override void WriteStartObject(XamlType xamlType)
	{
		ThrowIsDisposed();
		_addDelegate(XamlNodeType.StartObject, xamlType);
	}

	public override void WriteEndObject()
	{
		ThrowIsDisposed();
		_addDelegate(XamlNodeType.EndObject, null);
	}

	public override void WriteStartMember(XamlMember member)
	{
		ThrowIsDisposed();
		_addDelegate(XamlNodeType.StartMember, member);
	}

	public override void WriteEndMember()
	{
		ThrowIsDisposed();
		_addDelegate(XamlNodeType.EndMember, null);
	}

	public override void WriteValue(object value)
	{
		ThrowIsDisposed();
		_addDelegate(XamlNodeType.Value, value);
	}

	public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
	{
		ThrowIsDisposed();
		_addDelegate(XamlNodeType.NamespaceDeclaration, namespaceDeclaration);
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!disposing || base.IsDisposed)
			{
				return;
			}
			_addDelegate(XamlNodeType.None, XamlNode.InternalNodeType.EndOfStream);
			_addDelegate = delegate
			{
				throw new XamlException(System.SR.WriterIsClosed);
			};
			if (_addLineInfoDelegate != null)
			{
				_addLineInfoDelegate = delegate
				{
					throw new XamlException(System.SR.WriterIsClosed);
				};
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	public void SetLineInfo(int lineNumber, int linePosition)
	{
		ThrowIsDisposed();
		_addLineInfoDelegate(lineNumber, linePosition);
	}

	private void ThrowIsDisposed()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException("XamlWriter");
		}
	}
}
