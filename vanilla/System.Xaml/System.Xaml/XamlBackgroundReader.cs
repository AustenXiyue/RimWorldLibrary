using System.Threading;

namespace System.Xaml;

/// <summary>Implements a double-buffered <see cref="T:System.Xaml.XamlReader" /> that can split reading and writing to different threads.</summary>
public class XamlBackgroundReader : XamlReader, IXamlLineInfo
{
	private EventWaitHandle _providerFullEvent;

	private EventWaitHandle _dataReceivedEvent;

	private XamlNode[] _incoming;

	private int _inIdx;

	private XamlNode[] _outgoing;

	private int _outIdx;

	private int _outValid;

	private XamlNode _currentNode;

	private XamlReader _wrappedReader;

	private XamlReader _internalReader;

	private XamlWriter _writer;

	private bool _wrappedReaderHasLineInfo;

	private int _lineNumber;

	private int _linePosition;

	private Thread _thread;

	private Exception _caughtException;

	internal bool IncomingFull => _inIdx >= _incoming.Length;

	internal bool OutgoingEmpty => _outIdx >= _outValid;

	/// <summary>Gets the type of the current node.</summary>
	/// <returns>A value of the <see cref="T:System.Xaml.XamlNodeType" /> enumeration.</returns>
	public override XamlNodeType NodeType => _internalReader.NodeType;

	/// <summary>Gets a value that reports whether the reader position is at end-of-file.</summary>
	/// <returns>true if the position is at end-of-file; otherwise, false.</returns>
	public override bool IsEof => _internalReader.IsEof;

	/// <summary>Gets the XAML namespace from the current node.</summary>
	/// <returns>The XAML namespace, if it is available; otherwise, null.</returns>
	public override NamespaceDeclaration Namespace => _internalReader.Namespace;

	/// <summary>Gets the <see cref="T:System.Xaml.XamlType" /> of the current node.</summary>
	/// <returns>The <see cref="T:System.Xaml.XamlType" /> of the current node; or null, if the current reader position is not on an object.</returns>
	public override XamlType Type => _internalReader.Type;

	/// <summary>Gets the value of the current node.</summary>
	/// <returns>The value of the current node; or null, if the current reader position is not on a <see cref="F:System.Xaml.XamlNodeType.Value" /> node type.</returns>
	public override object Value => _internalReader.Value;

	/// <summary>Gets the current member at the reader position, if the reader position is on a <see cref="F:System.Xaml.XamlNodeType.StartMember" />.</summary>
	/// <returns>The current member; or null, if the position is not on a member.</returns>
	public override XamlMember Member => _internalReader.Member;

	/// <summary>Gets an object that provides schema context information for the information set.</summary>
	/// <returns>An object that provides schema context information for the information set.</returns>
	public override XamlSchemaContext SchemaContext => _internalReader.SchemaContext;

	/// <summary>Gets a value that specifies whether line information is available.</summary>
	/// <returns>true if line information is available; otherwise, false.</returns>
	public bool HasLineInfo => _wrappedReaderHasLineInfo;

	/// <summary>Gets the line number to report.</summary>
	/// <returns>The line number to report.</returns>
	public int LineNumber => _lineNumber;

	/// <summary>Gets the line position to report.</summary>
	/// <returns>The line position to report.</returns>
	public int LinePosition => _linePosition;

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlBackgroundReader" /> class. </summary>
	/// <param name="wrappedReader">The <see cref="T:System.Xaml.XamlReader" /> that this <see cref="T:System.Xaml.XamlBackgroundReader" /> is based on. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="wrappedReader" /> is null.</exception>
	public XamlBackgroundReader(XamlReader wrappedReader)
	{
		ArgumentNullException.ThrowIfNull(wrappedReader, "wrappedReader");
		Initialize(wrappedReader, 64);
	}

	private void Initialize(XamlReader wrappedReader, int bufferSize)
	{
		_providerFullEvent = new EventWaitHandle(initialState: false, EventResetMode.AutoReset);
		_dataReceivedEvent = new EventWaitHandle(initialState: false, EventResetMode.AutoReset);
		_incoming = new XamlNode[bufferSize];
		_outgoing = new XamlNode[bufferSize];
		_wrappedReader = wrappedReader;
		_wrappedReaderHasLineInfo = ((IXamlLineInfo)_wrappedReader).HasLineInfo;
		XamlNodeAddDelegate add = Add;
		XamlLineInfoAddDelegate addlineInfoDelegate = null;
		if (_wrappedReaderHasLineInfo)
		{
			addlineInfoDelegate = AddLineInfo;
		}
		_writer = new WriterDelegate(add, addlineInfoDelegate, _wrappedReader.SchemaContext);
		_internalReader = new ReaderDelegate(next: (!_wrappedReaderHasLineInfo) ? new XamlNodeNextDelegate(Next) : new XamlNodeNextDelegate(Next_ProcessLineInfo), schemaContext: _wrappedReader.SchemaContext, hasLineInfo: _wrappedReaderHasLineInfo);
		_currentNode = new XamlNode(XamlNode.InternalNodeType.StartOfStream);
	}

	/// <summary>Creates and starts a new <see cref="T:System.Threading.Thread" /> (constructed from <see cref="T:System.Threading.ParameterizedThreadStart" />) that handles a named thread for the <see cref="T:System.Xaml.XamlReader" />.</summary>
	/// <exception cref="T:System.InvalidOperationException">The thread is already started.</exception>
	public void StartThread()
	{
		StartThread("XAML reader thread");
	}

	/// <summary>Creates and starts a new <see cref="T:System.Threading.Thread" /> (constructed from <see cref="T:System.Threading.ParameterizedThreadStart" />) that handles a named thread for the <see cref="T:System.Xaml.XamlReader" />. You specify the thread name as a parameter.</summary>
	/// <param name="threadName">The name for the thread.</param>
	/// <exception cref="T:System.InvalidOperationException">The thread is already started.</exception>
	public void StartThread(string threadName)
	{
		if (_thread != null)
		{
			throw new InvalidOperationException(System.SR.ThreadAlreadyStarted);
		}
		ParameterizedThreadStart start = XamlReaderThreadStart;
		_thread = new Thread(start);
		_thread.Name = threadName;
		_thread.Start();
	}

	private void XamlReaderThreadStart(object none)
	{
		try
		{
			InterruptableTransform(_wrappedReader, _writer, closeWriter: true);
		}
		catch (Exception caughtException)
		{
			_writer.Close();
			_caughtException = caughtException;
		}
	}

	private void SwapBuffers()
	{
		XamlNode[] incoming = _incoming;
		_incoming = _outgoing;
		_outgoing = incoming;
		_outIdx = 0;
		_outValid = _inIdx;
		_inIdx = 0;
	}

	private void AddToBuffer(XamlNode node)
	{
		_incoming[_inIdx] = node;
		_inIdx++;
		if (IncomingFull)
		{
			_providerFullEvent.Set();
			_dataReceivedEvent.WaitOne();
		}
	}

	private void Add(XamlNodeType nodeType, object data)
	{
		if (!base.IsDisposed)
		{
			if (nodeType != 0)
			{
				AddToBuffer(new XamlNode(nodeType, data));
				return;
			}
			AddToBuffer(new XamlNode(XamlNode.InternalNodeType.EndOfStream));
			_providerFullEvent.Set();
		}
	}

	private void AddLineInfo(int lineNumber, int linePosition)
	{
		if (!base.IsDisposed)
		{
			LineInfo lineInfo = new LineInfo(lineNumber, linePosition);
			XamlNode node = new XamlNode(lineInfo);
			AddToBuffer(node);
		}
	}

	private XamlNode Next()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException("XamlBackgroundReader");
		}
		if (OutgoingEmpty)
		{
			if (_currentNode.IsEof)
			{
				return _currentNode;
			}
			_providerFullEvent.WaitOne();
			SwapBuffers();
			_dataReceivedEvent.Set();
		}
		_currentNode = _outgoing[_outIdx++];
		if (_currentNode.IsEof && _thread != null)
		{
			_thread.Join();
			if (_caughtException != null)
			{
				Exception caughtException = _caughtException;
				_caughtException = null;
				throw caughtException;
			}
		}
		return _currentNode;
	}

	private XamlNode Next_ProcessLineInfo()
	{
		bool flag = false;
		while (!flag)
		{
			Next();
			if (_currentNode.IsLineInfo)
			{
				_lineNumber = _currentNode.LineInfo.LineNumber;
				_linePosition = _currentNode.LineInfo.LinePosition;
			}
			else
			{
				flag = true;
			}
		}
		return _currentNode;
	}

	private void InterruptableTransform(XamlReader reader, XamlWriter writer, bool closeWriter)
	{
		IXamlLineInfo xamlLineInfo = reader as IXamlLineInfo;
		IXamlLineInfoConsumer xamlLineInfoConsumer = writer as IXamlLineInfoConsumer;
		bool flag = false;
		if (xamlLineInfo != null && xamlLineInfo.HasLineInfo && xamlLineInfoConsumer != null && xamlLineInfoConsumer.ShouldProvideLineInfo)
		{
			flag = true;
		}
		while (reader.Read() && !base.IsDisposed)
		{
			if (flag && xamlLineInfo.LineNumber != 0)
			{
				xamlLineInfoConsumer.SetLineInfo(xamlLineInfo.LineNumber, xamlLineInfo.LinePosition);
			}
			writer.WriteNode(reader);
		}
		if (closeWriter)
		{
			writer.Close();
		}
	}

	/// <summary>Provides the next XAML node from the source, if a node is available. </summary>
	/// <returns>true if a node is available; otherwise, false.</returns>
	public override bool Read()
	{
		return _internalReader.Read();
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Xaml.XamlBackgroundReader" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release managed resources; otherwise, false.</param>
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		_dataReceivedEvent.Set();
		((IDisposable)_dataReceivedEvent).Dispose();
		((IDisposable)_internalReader).Dispose();
		((IDisposable)_providerFullEvent).Dispose();
		((IDisposable)_writer).Dispose();
	}
}
