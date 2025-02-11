using System.Collections.Generic;

namespace System.Xaml;

/// <summary>Provides a buffer for writing nodes and reading them again.</summary>
public class XamlNodeQueue
{
	private Queue<XamlNode> _nodeQueue;

	private XamlNode _endOfStreamNode;

	private ReaderDelegate _reader;

	private XamlWriter _writer;

	private bool _hasLineInfo;

	/// <summary>Gets a XAML reader implementation delegate.</summary>
	/// <returns>A XAML reader implementation delegate.</returns>
	/// <exception cref="T:System.Xaml.XamlException">The XAML writer that is used for the node list has no valid XAML schema context.</exception>
	public XamlReader Reader
	{
		get
		{
			if (_reader == null)
			{
				_reader = new ReaderDelegate(_writer.SchemaContext, Next, _hasLineInfo);
			}
			return _reader;
		}
	}

	/// <summary>Gets the associated XAML writer.</summary>
	/// <returns>The associated XAML writer. </returns>
	public XamlWriter Writer => _writer;

	/// <summary>Gets a value that determines whether this <see cref="T:System.Xaml.XamlNodeQueue" /> does not contain nodes.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlNodeQueue" /> does not contain nodes; false if this <see cref="T:System.Xaml.XamlNodeQueue" /> contains nodes.</returns>
	public bool IsEmpty => _nodeQueue.Count == 0;

	/// <summary>Gets the number of nodes in the <see cref="T:System.Xaml.XamlNodeQueue" />.</summary>
	/// <returns>The number of nodes in the <see cref="T:System.Xaml.XamlNodeQueue" />.</returns>
	public int Count => _nodeQueue.Count;

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlNodeQueue" /> class using a provided XAML schema context.</summary>
	/// <param name="schemaContext">The schema context to use for node operations.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xamlSchemaContext" /> is null.</exception>
	public XamlNodeQueue(XamlSchemaContext schemaContext)
	{
		ArgumentNullException.ThrowIfNull(schemaContext, "schemaContext");
		_nodeQueue = new Queue<XamlNode>();
		_endOfStreamNode = new XamlNode(XamlNode.InternalNodeType.EndOfStream);
		_writer = new WriterDelegate(Add, AddLineInfo, schemaContext);
	}

	private void Add(XamlNodeType nodeType, object data)
	{
		if (nodeType != 0)
		{
			XamlNode item = new XamlNode(nodeType, data);
			_nodeQueue.Enqueue(item);
		}
		else
		{
			_nodeQueue.Enqueue(_endOfStreamNode);
		}
	}

	private void AddLineInfo(int lineNumber, int linePosition)
	{
		LineInfo lineInfo = new LineInfo(lineNumber, linePosition);
		XamlNode item = new XamlNode(lineInfo);
		_nodeQueue.Enqueue(item);
		if (!_hasLineInfo)
		{
			_hasLineInfo = true;
		}
		if (_reader != null && !_reader.HasLineInfo)
		{
			_reader.HasLineInfo = true;
		}
	}

	private XamlNode Next()
	{
		if (_nodeQueue.Count > 0)
		{
			return _nodeQueue.Dequeue();
		}
		return _endOfStreamNode;
	}
}
