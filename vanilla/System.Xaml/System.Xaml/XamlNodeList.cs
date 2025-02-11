using System.Collections.Generic;

namespace System.Xaml;

/// <summary>Provides a list of XAML nodes, which can be used for scenarios such as writing XAML nodes in a deferred manner.</summary>
public class XamlNodeList
{
	private List<XamlNode> _nodeList;

	private bool _readMode;

	private XamlWriter _writer;

	private bool _hasLineInfo;

	/// <summary>Gets the associated XAML writer.</summary>
	/// <returns>The associated XAML writer. </returns>
	public XamlWriter Writer => _writer;

	/// <summary>Gets the number of nodes in this <see cref="T:System.Xaml.XamlNodeList" />.</summary>
	/// <returns>The number of nodes in this <see cref="T:System.Xaml.XamlNodeList" />.</returns>
	public int Count => _nodeList.Count;

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlNodeList" /> class, using a provided schema context.</summary>
	/// <param name="schemaContext">The schema context to use for node operations.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="schemaContext" /> is null.</exception>
	public XamlNodeList(XamlSchemaContext schemaContext)
	{
		ArgumentNullException.ThrowIfNull(schemaContext, "schemaContext");
		Initialize(schemaContext, 0);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlNodeList" /> class, using a provided schema context and list size.</summary>
	/// <param name="schemaContext">The schema context to use for node operations.</param>
	/// <param name="size">The intended item count of the list.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="schemaContext" /> is null.</exception>
	public XamlNodeList(XamlSchemaContext schemaContext, int size)
	{
		ArgumentNullException.ThrowIfNull(schemaContext, "schemaContext");
		Initialize(schemaContext, size);
	}

	private void Initialize(XamlSchemaContext schemaContext, int size)
	{
		if (size == 0)
		{
			_nodeList = new List<XamlNode>();
		}
		else
		{
			_nodeList = new List<XamlNode>(size);
		}
		_writer = new WriterDelegate(Add, AddLineInfo, schemaContext);
	}

	/// <summary>Returns a XAML reader implementation delegate.</summary>
	/// <returns>A XAML reader implementation delegate.</returns>
	/// <exception cref="T:System.Xaml.XamlException">The <see cref="T:System.Xaml.XamlNodeList" /> is still in Write mode.-or-The writer that is used for the node list has no schema context.</exception>
	public XamlReader GetReader()
	{
		if (!_readMode)
		{
			throw new XamlException(System.SR.CloseXamlWriterBeforeReading);
		}
		if (_writer.SchemaContext == null)
		{
			throw new XamlException(System.SR.SchemaContextNotInitialized);
		}
		return new ReaderMultiIndexDelegate(_writer.SchemaContext, Index, _nodeList.Count, _hasLineInfo);
	}

	private void Add(XamlNodeType nodeType, object data)
	{
		if (!_readMode)
		{
			if (nodeType != 0)
			{
				XamlNode item = new XamlNode(nodeType, data);
				_nodeList.Add(item);
			}
			else
			{
				_readMode = true;
			}
			return;
		}
		throw new XamlException(System.SR.CannotWriteClosedWriter);
	}

	private void AddLineInfo(int lineNumber, int linePosition)
	{
		if (_readMode)
		{
			throw new XamlException(System.SR.CannotWriteClosedWriter);
		}
		XamlNode item = new XamlNode(new LineInfo(lineNumber, linePosition));
		_nodeList.Add(item);
		if (!_hasLineInfo)
		{
			_hasLineInfo = true;
		}
	}

	private XamlNode Index(int idx)
	{
		if (!_readMode)
		{
			throw new XamlException(System.SR.CloseXamlWriterBeforeReading);
		}
		return _nodeList[idx];
	}

	/// <summary>Clears the items in this list.</summary>
	public void Clear()
	{
		_nodeList.Clear();
		_readMode = false;
	}
}
