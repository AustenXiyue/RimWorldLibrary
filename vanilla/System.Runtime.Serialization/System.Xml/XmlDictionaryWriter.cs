using System.Globalization;
using System.IO;
using System.Runtime;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace System.Xml;

/// <summary>Represents an abstract class that Windows Communication Foundation (WCF) derives from <see cref="T:System.Xml.XmlWriter" /> to do serialization and deserialization.</summary>
public abstract class XmlDictionaryWriter : XmlWriter
{
	private class WriteValueFastAsyncResult : AsyncResult
	{
		private enum Operation
		{
			Read,
			Write,
			Complete
		}

		private bool completed;

		private int blockSize;

		private byte[] block;

		private int bytesRead;

		private Stream stream;

		private Operation nextOperation;

		private IStreamProvider streamProvider;

		private XmlDictionaryWriter writer;

		private AsyncEventArgs<XmlWriteBase64AsyncArguments> writerAsyncState;

		private XmlWriteBase64AsyncArguments writerAsyncArgs;

		private static AsyncCallback onReadComplete = Fx.ThunkCallback(OnReadComplete);

		private static AsyncEventArgsCallback onWriteComplete;

		public WriteValueFastAsyncResult(XmlDictionaryWriter writer, IStreamProvider value, AsyncCallback callback, object state)
			: base(callback, state)
		{
			streamProvider = value;
			this.writer = writer;
			stream = value.GetStream();
			if (stream == null)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString("Stream returned by IStreamProvider cannot be null.")));
			}
			blockSize = 256;
			bytesRead = 0;
			block = new byte[blockSize];
			nextOperation = Operation.Read;
			ContinueWork(completedSynchronously: true);
		}

		private void CompleteAndReleaseStream(bool completedSynchronously, Exception completionException = null)
		{
			if (completionException == null)
			{
				streamProvider.ReleaseStream(stream);
				stream = null;
			}
			Complete(completedSynchronously, completionException);
		}

		private void ContinueWork(bool completedSynchronously, Exception completionException = null)
		{
			try
			{
				while (true)
				{
					if (nextOperation == Operation.Read)
					{
						if (ReadAsync() != AsyncCompletionResult.Completed)
						{
							return;
						}
					}
					else if (nextOperation == Operation.Write)
					{
						if (WriteAsync() != AsyncCompletionResult.Completed)
						{
							return;
						}
					}
					else if (nextOperation == Operation.Complete)
					{
						break;
					}
				}
			}
			catch (Exception ex)
			{
				if (Fx.IsFatal(ex))
				{
					throw;
				}
				if (completedSynchronously)
				{
					throw;
				}
				if (completionException == null)
				{
					completionException = ex;
				}
			}
			if (!completed)
			{
				completed = true;
				CompleteAndReleaseStream(completedSynchronously, completionException);
			}
		}

		private AsyncCompletionResult ReadAsync()
		{
			IAsyncResult asyncResult = stream.BeginRead(block, 0, blockSize, onReadComplete, this);
			if (asyncResult.CompletedSynchronously)
			{
				HandleReadComplete(asyncResult);
				return AsyncCompletionResult.Completed;
			}
			return AsyncCompletionResult.Queued;
		}

		private void HandleReadComplete(IAsyncResult result)
		{
			bytesRead = stream.EndRead(result);
			if (bytesRead > 0)
			{
				nextOperation = Operation.Write;
			}
			else
			{
				nextOperation = Operation.Complete;
			}
		}

		private static void OnReadComplete(IAsyncResult result)
		{
			if (result.CompletedSynchronously)
			{
				return;
			}
			Exception completionException = null;
			WriteValueFastAsyncResult writeValueFastAsyncResult = (WriteValueFastAsyncResult)result.AsyncState;
			bool flag = false;
			try
			{
				writeValueFastAsyncResult.HandleReadComplete(result);
				flag = true;
			}
			catch (Exception ex)
			{
				if (Fx.IsFatal(ex))
				{
					throw;
				}
				completionException = ex;
			}
			if (!flag)
			{
				writeValueFastAsyncResult.nextOperation = Operation.Complete;
			}
			writeValueFastAsyncResult.ContinueWork(completedSynchronously: false, completionException);
		}

		private AsyncCompletionResult WriteAsync()
		{
			if (writerAsyncState == null)
			{
				writerAsyncArgs = new XmlWriteBase64AsyncArguments();
				writerAsyncState = new AsyncEventArgs<XmlWriteBase64AsyncArguments>();
			}
			if (onWriteComplete == null)
			{
				onWriteComplete = OnWriteComplete;
			}
			writerAsyncArgs.Buffer = block;
			writerAsyncArgs.Offset = 0;
			writerAsyncArgs.Count = bytesRead;
			writerAsyncState.Set(onWriteComplete, writerAsyncArgs, this);
			if (writer.WriteBase64Async(writerAsyncState) == AsyncCompletionResult.Completed)
			{
				HandleWriteComplete();
				writerAsyncState.Complete(completedSynchronously: true);
				return AsyncCompletionResult.Completed;
			}
			return AsyncCompletionResult.Queued;
		}

		private void HandleWriteComplete()
		{
			nextOperation = Operation.Read;
			if (blockSize < 65536 && bytesRead == blockSize)
			{
				blockSize *= 16;
				block = new byte[blockSize];
			}
		}

		private static void OnWriteComplete(IAsyncEventArgs asyncState)
		{
			WriteValueFastAsyncResult writeValueFastAsyncResult = (WriteValueFastAsyncResult)asyncState.AsyncState;
			Exception completionException = null;
			bool flag = false;
			try
			{
				if (asyncState.Exception != null)
				{
					completionException = asyncState.Exception;
				}
				else
				{
					writeValueFastAsyncResult.HandleWriteComplete();
					flag = true;
				}
			}
			catch (Exception ex)
			{
				if (Fx.IsFatal(ex))
				{
					throw;
				}
				completionException = ex;
			}
			if (!flag)
			{
				writeValueFastAsyncResult.nextOperation = Operation.Complete;
			}
			writeValueFastAsyncResult.ContinueWork(completedSynchronously: false, completionException);
		}

		internal static void End(IAsyncResult result)
		{
			AsyncResult.End<WriteValueFastAsyncResult>(result);
		}
	}

	private class WriteValueAsyncResult : AsyncResult
	{
		private enum Operation
		{
			Read,
			Write
		}

		private int blockSize;

		private byte[] block;

		private int bytesRead;

		private Stream stream;

		private Operation operation;

		private IStreamProvider streamProvider;

		private XmlDictionaryWriter writer;

		private Func<IAsyncResult, WriteValueAsyncResult, bool> writeBlockHandler;

		private static Func<IAsyncResult, WriteValueAsyncResult, bool> handleWriteBlock = HandleWriteBlock;

		private static Func<IAsyncResult, WriteValueAsyncResult, bool> handleWriteBlockAsync = HandleWriteBlockAsync;

		private static AsyncCallback onContinueWork = Fx.ThunkCallback(OnContinueWork);

		public WriteValueAsyncResult(XmlDictionaryWriter writer, IStreamProvider value, AsyncCallback callback, object state)
			: base(callback, state)
		{
			streamProvider = value;
			this.writer = writer;
			writeBlockHandler = ((this.writer.Settings != null && this.writer.Settings.Async) ? handleWriteBlockAsync : handleWriteBlock);
			stream = value.GetStream();
			if (stream == null)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString("Stream returned by IStreamProvider cannot be null.")));
			}
			blockSize = 256;
			bytesRead = 0;
			block = new byte[blockSize];
			if (ContinueWork(null))
			{
				CompleteAndReleaseStream(completedSynchronously: true, null);
			}
		}

		private void AdjustBlockSize()
		{
			if (blockSize < 65536 && bytesRead == blockSize)
			{
				blockSize *= 16;
				block = new byte[blockSize];
			}
		}

		private void CompleteAndReleaseStream(bool completedSynchronously, Exception completionException)
		{
			if (completionException == null)
			{
				streamProvider.ReleaseStream(stream);
				stream = null;
			}
			Complete(completedSynchronously, completionException);
		}

		private bool ContinueWork(IAsyncResult result)
		{
			while (true)
			{
				if (operation == Operation.Read)
				{
					if (!HandleReadBlock(result))
					{
						return false;
					}
					if (bytesRead <= 0)
					{
						return true;
					}
					operation = Operation.Write;
				}
				else
				{
					if (!writeBlockHandler(result, this))
					{
						break;
					}
					AdjustBlockSize();
					operation = Operation.Read;
				}
				result = null;
			}
			return false;
		}

		private bool HandleReadBlock(IAsyncResult result)
		{
			if (result == null)
			{
				result = stream.BeginRead(block, 0, blockSize, onContinueWork, this);
				if (!result.CompletedSynchronously)
				{
					return false;
				}
			}
			bytesRead = stream.EndRead(result);
			return true;
		}

		private static bool HandleWriteBlock(IAsyncResult result, WriteValueAsyncResult thisPtr)
		{
			if (result == null)
			{
				result = thisPtr.writer.BeginWriteBase64(thisPtr.block, 0, thisPtr.bytesRead, onContinueWork, thisPtr);
				if (!result.CompletedSynchronously)
				{
					return false;
				}
			}
			thisPtr.writer.EndWriteBase64(result);
			return true;
		}

		private static bool HandleWriteBlockAsync(IAsyncResult result, WriteValueAsyncResult thisPtr)
		{
			Task task = (Task)result;
			if (task == null)
			{
				task = thisPtr.writer.WriteBase64Async(thisPtr.block, 0, thisPtr.bytesRead);
				task.AsAsyncResult(onContinueWork, thisPtr);
				return false;
			}
			task.GetAwaiter().GetResult();
			return true;
		}

		private static void OnContinueWork(IAsyncResult result)
		{
			if (result.CompletedSynchronously && !(result is Task))
			{
				return;
			}
			Exception completionException = null;
			WriteValueAsyncResult writeValueAsyncResult = (WriteValueAsyncResult)result.AsyncState;
			bool flag = false;
			try
			{
				flag = writeValueAsyncResult.ContinueWork(result);
			}
			catch (Exception ex)
			{
				if (Fx.IsFatal(ex))
				{
					throw;
				}
				flag = true;
				completionException = ex;
			}
			if (flag)
			{
				writeValueAsyncResult.CompleteAndReleaseStream(completedSynchronously: false, completionException);
			}
		}

		public static void End(IAsyncResult result)
		{
			AsyncResult.End<WriteValueAsyncResult>(result);
		}
	}

	private class WriteBase64AsyncResult : ScheduleActionItemAsyncResult
	{
		private byte[] buffer;

		private int index;

		private int count;

		private XmlDictionaryWriter writer;

		public WriteBase64AsyncResult(byte[] buffer, int index, int count, XmlDictionaryWriter writer, AsyncCallback callback, object state)
			: base(callback, state)
		{
			this.buffer = buffer;
			this.index = index;
			this.count = count;
			this.writer = writer;
			Schedule();
		}

		protected override void OnDoWork()
		{
			writer.WriteBase64(buffer, index, count);
		}
	}

	private class XmlWrappedWriter : XmlDictionaryWriter
	{
		private XmlWriter writer;

		private int depth;

		private int prefix;

		public override WriteState WriteState => writer.WriteState;

		public override string XmlLang => writer.XmlLang;

		public override XmlSpace XmlSpace => writer.XmlSpace;

		public XmlWrappedWriter(XmlWriter writer)
		{
			this.writer = writer;
			depth = 0;
		}

		public override void Close()
		{
			writer.Close();
		}

		public override void Flush()
		{
			writer.Flush();
		}

		public override string LookupPrefix(string namespaceUri)
		{
			return writer.LookupPrefix(namespaceUri);
		}

		public override void WriteAttributes(XmlReader reader, bool defattr)
		{
			writer.WriteAttributes(reader, defattr);
		}

		public override void WriteBase64(byte[] buffer, int index, int count)
		{
			writer.WriteBase64(buffer, index, count);
		}

		public override void WriteBinHex(byte[] buffer, int index, int count)
		{
			writer.WriteBinHex(buffer, index, count);
		}

		public override void WriteCData(string text)
		{
			writer.WriteCData(text);
		}

		public override void WriteCharEntity(char ch)
		{
			writer.WriteCharEntity(ch);
		}

		public override void WriteChars(char[] buffer, int index, int count)
		{
			writer.WriteChars(buffer, index, count);
		}

		public override void WriteComment(string text)
		{
			writer.WriteComment(text);
		}

		public override void WriteDocType(string name, string pubid, string sysid, string subset)
		{
			writer.WriteDocType(name, pubid, sysid, subset);
		}

		public override void WriteEndAttribute()
		{
			writer.WriteEndAttribute();
		}

		public override void WriteEndDocument()
		{
			writer.WriteEndDocument();
		}

		public override void WriteEndElement()
		{
			writer.WriteEndElement();
			depth--;
		}

		public override void WriteEntityRef(string name)
		{
			writer.WriteEntityRef(name);
		}

		public override void WriteFullEndElement()
		{
			writer.WriteFullEndElement();
		}

		public override void WriteName(string name)
		{
			writer.WriteName(name);
		}

		public override void WriteNmToken(string name)
		{
			writer.WriteNmToken(name);
		}

		public override void WriteNode(XmlReader reader, bool defattr)
		{
			writer.WriteNode(reader, defattr);
		}

		public override void WriteProcessingInstruction(string name, string text)
		{
			writer.WriteProcessingInstruction(name, text);
		}

		public override void WriteQualifiedName(string localName, string namespaceUri)
		{
			writer.WriteQualifiedName(localName, namespaceUri);
		}

		public override void WriteRaw(char[] buffer, int index, int count)
		{
			writer.WriteRaw(buffer, index, count);
		}

		public override void WriteRaw(string data)
		{
			writer.WriteRaw(data);
		}

		public override void WriteStartAttribute(string prefix, string localName, string namespaceUri)
		{
			writer.WriteStartAttribute(prefix, localName, namespaceUri);
			this.prefix++;
		}

		public override void WriteStartDocument()
		{
			writer.WriteStartDocument();
		}

		public override void WriteStartDocument(bool standalone)
		{
			writer.WriteStartDocument(standalone);
		}

		public override void WriteStartElement(string prefix, string localName, string namespaceUri)
		{
			writer.WriteStartElement(prefix, localName, namespaceUri);
			depth++;
			this.prefix = 1;
		}

		public override void WriteString(string text)
		{
			writer.WriteString(text);
		}

		public override void WriteSurrogateCharEntity(char lowChar, char highChar)
		{
			writer.WriteSurrogateCharEntity(lowChar, highChar);
		}

		public override void WriteWhitespace(string whitespace)
		{
			writer.WriteWhitespace(whitespace);
		}

		public override void WriteValue(object value)
		{
			writer.WriteValue(value);
		}

		public override void WriteValue(string value)
		{
			writer.WriteValue(value);
		}

		public override void WriteValue(bool value)
		{
			writer.WriteValue(value);
		}

		public override void WriteValue(DateTime value)
		{
			writer.WriteValue(value);
		}

		public override void WriteValue(double value)
		{
			writer.WriteValue(value);
		}

		public override void WriteValue(int value)
		{
			writer.WriteValue(value);
		}

		public override void WriteValue(long value)
		{
			writer.WriteValue(value);
		}

		public override void WriteXmlnsAttribute(string prefix, string namespaceUri)
		{
			if (namespaceUri == null)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
			}
			if (prefix == null)
			{
				if (LookupPrefix(namespaceUri) != null)
				{
					return;
				}
				if (namespaceUri.Length == 0)
				{
					prefix = string.Empty;
				}
				else
				{
					string text = depth.ToString(NumberFormatInfo.InvariantInfo);
					string text2 = this.prefix.ToString(NumberFormatInfo.InvariantInfo);
					prefix = "d" + text + "p" + text2;
				}
			}
			WriteAttributeString("xmlns", prefix, null, namespaceUri);
		}
	}

	internal virtual bool FastAsync => false;

	/// <summary>This property always returns false. Its derived classes can override to return true if they support canonicalization.</summary>
	/// <returns>false in all cases.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool CanCanonicalize => false;

	internal virtual AsyncCompletionResult WriteBase64Async(AsyncEventArgs<XmlWriteBase64AsyncArguments> state)
	{
		throw FxTrace.Exception.AsError(new NotSupportedException());
	}

	/// <summary>Asynchronously encodes the specified binary bytes as Base64 and writes out the resulting text.</summary>
	/// <returns>The task that represents the asynchronous WriteBase64 operation.</returns>
	/// <param name="buffer">Byte array to encode. </param>
	/// <param name="index">The position in the buffer indicating the start of the bytes to write. </param>
	/// <param name="count">The number of bytes to write. </param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlDictionaryWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlDictionaryWriter" /> asynchronous method was called without setting the <see cref="P:System.Xml.XmlWriterSettings.Async" /> flag to true. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “Set XmlWriterSettings.Async to true if you want to use Async Methods.”</exception>
	public override Task WriteBase64Async(byte[] buffer, int index, int count)
	{
		return Task.Factory.FromAsync(BeginWriteBase64, EndWriteBase64, buffer, index, count, null);
	}

	internal virtual IAsyncResult BeginWriteBase64(byte[] buffer, int index, int count, AsyncCallback callback, object state)
	{
		return new WriteBase64AsyncResult(buffer, index, count, this, callback, state);
	}

	internal virtual void EndWriteBase64(IAsyncResult result)
	{
		ScheduleActionItemAsyncResult.End(result);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryWriter" /> that writes WCF binary XML format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryWriter" />.</returns>
	/// <param name="stream">The stream to write to.</param>
	public static XmlDictionaryWriter CreateBinaryWriter(Stream stream)
	{
		return CreateBinaryWriter(stream, null);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryWriter" /> that writes WCF binary XML format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryWriter" />.</returns>
	/// <param name="stream">The stream to write to.</param>
	/// <param name="dictionary">The <see cref="T:System.Xml.XmlDictionary" /> to use as the shared dictionary.</param>
	public static XmlDictionaryWriter CreateBinaryWriter(Stream stream, IXmlDictionary dictionary)
	{
		return CreateBinaryWriter(stream, dictionary, null);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryWriter" /> that writes WCF binary XML format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryWriter" />.</returns>
	/// <param name="stream">The stream to write to.</param>
	/// <param name="dictionary">The <see cref="T:System.Xml.XmlDictionary" /> to use as the shared dictionary.</param>
	/// <param name="session">The <see cref="T:System.Xml.XmlBinaryWriterSession" /> to use.</param>
	public static XmlDictionaryWriter CreateBinaryWriter(Stream stream, IXmlDictionary dictionary, XmlBinaryWriterSession session)
	{
		return CreateBinaryWriter(stream, dictionary, session, ownsStream: true);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryWriter" /> that writes WCF binary XML format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryWriter" />.</returns>
	/// <param name="stream">The stream from which to read.</param>
	/// <param name="dictionary">The <see cref="T:System.Xml.XmlDictionary" /> to use as the shared dictionary.</param>
	/// <param name="session">The <see cref="T:System.Xml.XmlBinaryWriterSession" /> to use.</param>
	/// <param name="ownsStream">true to indicate that the stream is closed by the writer when done; otherwise false.</param>
	public static XmlDictionaryWriter CreateBinaryWriter(Stream stream, IXmlDictionary dictionary, XmlBinaryWriterSession session, bool ownsStream)
	{
		XmlBinaryWriter xmlBinaryWriter = new XmlBinaryWriter();
		xmlBinaryWriter.SetOutput(stream, dictionary, session, ownsStream);
		return xmlBinaryWriter;
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryWriter" /> that writes text XML.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryWriter" />.</returns>
	/// <param name="stream">The stream to write to.</param>
	public static XmlDictionaryWriter CreateTextWriter(Stream stream)
	{
		return CreateTextWriter(stream, Encoding.UTF8, ownsStream: true);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryWriter" /> that writes text XML.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryWriter" />.</returns>
	/// <param name="stream">The stream to write to.</param>
	/// <param name="encoding">The character encoding of the output.</param>
	public static XmlDictionaryWriter CreateTextWriter(Stream stream, Encoding encoding)
	{
		return CreateTextWriter(stream, encoding, ownsStream: true);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryWriter" /> that writes text XML.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryWriter" />.</returns>
	/// <param name="stream">The stream to write to.</param>
	/// <param name="encoding">The character encoding of the stream.</param>
	/// <param name="ownsStream">true to indicate that the stream is closed by the writer when done; otherwise false.</param>
	public static XmlDictionaryWriter CreateTextWriter(Stream stream, Encoding encoding, bool ownsStream)
	{
		XmlUTF8TextWriter xmlUTF8TextWriter = new XmlUTF8TextWriter();
		xmlUTF8TextWriter.SetOutput(stream, encoding, ownsStream);
		return xmlUTF8TextWriter;
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryWriter" /> that writes XML in the MTOM format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryWriter" />.</returns>
	/// <param name="stream">The stream to write to.</param>
	/// <param name="encoding">The character encoding of the stream.</param>
	/// <param name="maxSizeInBytes">The maximum number of bytes that are buffered in the writer.</param>
	/// <param name="startInfo">An attribute in the ContentType SOAP header.</param>
	/// <filterpriority>2</filterpriority>
	public static XmlDictionaryWriter CreateMtomWriter(Stream stream, Encoding encoding, int maxSizeInBytes, string startInfo)
	{
		return CreateMtomWriter(stream, encoding, maxSizeInBytes, startInfo, null, null, writeMessageHeaders: true, ownsStream: true);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryWriter" /> that writes XML in the MTOM format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryWriter" />.</returns>
	/// <param name="stream">The stream to write to.</param>
	/// <param name="encoding">The character encoding of the stream.</param>
	/// <param name="maxSizeInBytes">The maximum number of bytes that are buffered in the writer.</param>
	/// <param name="startInfo">The content-type of the MIME part that contains the Infoset.</param>
	/// <param name="boundary">The MIME boundary in the message.</param>
	/// <param name="startUri">The content-id URI of the MIME part that contains the Infoset.</param>
	/// <param name="writeMessageHeaders">true to write message headers.</param>
	/// <param name="ownsStream">true to indicate that the stream is closed by the writer when done; otherwise false.</param>
	/// <filterpriority>2</filterpriority>
	public static XmlDictionaryWriter CreateMtomWriter(Stream stream, Encoding encoding, int maxSizeInBytes, string startInfo, string boundary, string startUri, bool writeMessageHeaders, bool ownsStream)
	{
		XmlMtomWriter xmlMtomWriter = new XmlMtomWriter();
		xmlMtomWriter.SetOutput(stream, encoding, maxSizeInBytes, startInfo, boundary, startUri, writeMessageHeaders, ownsStream);
		return xmlMtomWriter;
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryWriter" /> from an existing <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryWriter" />.</returns>
	/// <param name="writer">An instance of <see cref="T:System.Xml.XmlWriter" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="writer" /> is null.</exception>
	public static XmlDictionaryWriter CreateDictionaryWriter(XmlWriter writer)
	{
		if (writer == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
		}
		XmlDictionaryWriter xmlDictionaryWriter = writer as XmlDictionaryWriter;
		if (xmlDictionaryWriter == null)
		{
			xmlDictionaryWriter = new XmlWrappedWriter(writer);
		}
		return xmlDictionaryWriter;
	}

	/// <summary>Writes the specified start tag and associates it with the given namespace.</summary>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <exception cref="T:System.InvalidOperationException">The writer is closed.</exception>
	public void WriteStartElement(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		WriteStartElement(null, localName, namespaceUri);
	}

	/// <summary>Writes the specified start tag and associates it with the given namespace and prefix.</summary>
	/// <param name="prefix">The prefix of the element.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <exception cref="T:System.InvalidOperationException">The writer is closed.</exception>
	public virtual void WriteStartElement(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		WriteStartElement(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri));
	}

	/// <summary>Writes the start of an attribute with the specified local name, and namespace URI.</summary>
	/// <param name="localName">The local name of the attribute.</param>
	/// <param name="namespaceUri">The namespace URI of the attribute.</param>
	public void WriteStartAttribute(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		WriteStartAttribute(null, localName, namespaceUri);
	}

	/// <summary>Writes the start of an attribute with the specified prefix, local name, and namespace URI.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the attribute.</param>
	/// <param name="namespaceUri">The namespace URI of the attribute.</param>
	public virtual void WriteStartAttribute(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		WriteStartAttribute(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri));
	}

	/// <summary>Writes an attribute qualified name and value.</summary>
	/// <param name="localName">The local name of the attribute.</param>
	/// <param name="namespaceUri">The namespace URI of the attribute.</param>
	/// <param name="value">The attribute.</param>
	public void WriteAttributeString(XmlDictionaryString localName, XmlDictionaryString namespaceUri, string value)
	{
		WriteAttributeString(null, localName, namespaceUri, value);
	}

	/// <summary>Writes a namespace declaration attribute.</summary>
	/// <param name="prefix">The prefix that is bound to the given namespace.</param>
	/// <param name="namespaceUri">The namespace to which the prefix is bound.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="namespaceUri" /> is null.</exception>
	public virtual void WriteXmlnsAttribute(string prefix, string namespaceUri)
	{
		if (namespaceUri == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
		}
		if (prefix == null)
		{
			if (LookupPrefix(namespaceUri) != null)
			{
				return;
			}
			prefix = ((namespaceUri.Length == 0) ? string.Empty : ("d" + namespaceUri.Length.ToString(NumberFormatInfo.InvariantInfo)));
		}
		WriteAttributeString("xmlns", prefix, null, namespaceUri);
	}

	/// <summary>Writes a namespace declaration attribute.</summary>
	/// <param name="prefix">The prefix that is bound to the given namespace.</param>
	/// <param name="namespaceUri">The namespace to which the prefix is bound.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="namespaceUri" /> is null.</exception>
	public virtual void WriteXmlnsAttribute(string prefix, XmlDictionaryString namespaceUri)
	{
		WriteXmlnsAttribute(prefix, XmlDictionaryString.GetString(namespaceUri));
	}

	/// <summary>Writes a standard XML attribute in the current node.</summary>
	/// <param name="localName">The local name of the attribute.</param>
	/// <param name="value">The value of the attribute.</param>
	public virtual void WriteXmlAttribute(string localName, string value)
	{
		WriteAttributeString("xml", localName, null, value);
	}

	/// <summary>Writes an XML attribute in the current node.</summary>
	/// <param name="localName">The local name of the attribute.</param>
	/// <param name="value">The value of the attribute.</param>
	public virtual void WriteXmlAttribute(XmlDictionaryString localName, XmlDictionaryString value)
	{
		WriteXmlAttribute(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(value));
	}

	/// <summary>Writes an attribute qualified name and value.</summary>
	/// <param name="prefix">The prefix of the attribute.</param>
	/// <param name="localName">The local name of the attribute.</param>
	/// <param name="namespaceUri">The namespace URI of the attribute.</param>
	/// <param name="value">The attribute.</param>
	public void WriteAttributeString(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, string value)
	{
		WriteStartAttribute(prefix, localName, namespaceUri);
		WriteString(value);
		WriteEndAttribute();
	}

	/// <summary>Writes an element with a text content.</summary>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="value">The element content.</param>
	public void WriteElementString(XmlDictionaryString localName, XmlDictionaryString namespaceUri, string value)
	{
		WriteElementString(null, localName, namespaceUri, value);
	}

	/// <summary>Writes an element with a text content.</summary>
	/// <param name="prefix">The prefix of the element.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="value">The element content.</param>
	public void WriteElementString(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, string value)
	{
		WriteStartElement(prefix, localName, namespaceUri);
		WriteString(value);
		WriteEndElement();
	}

	/// <summary>Writes the given text content.</summary>
	/// <param name="value">The text to write.</param>
	public virtual void WriteString(XmlDictionaryString value)
	{
		WriteString(XmlDictionaryString.GetString(value));
	}

	/// <summary>Writes out the namespace-qualified name. This method looks up the prefix that is in scope for the given namespace.</summary>
	/// <param name="localName">The local name of the qualified name.</param>
	/// <param name="namespaceUri">The namespace URI of the qualified name.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="localName" /> is null.</exception>
	public virtual void WriteQualifiedName(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		if (localName == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localName"));
		}
		if (namespaceUri == null)
		{
			namespaceUri = XmlDictionaryString.Empty;
		}
		WriteQualifiedName(localName.Value, namespaceUri.Value);
	}

	/// <summary>Writes a <see cref="T:System.Xml.XmlDictionaryString" /> value.</summary>
	/// <param name="value">The <see cref="T:System.Xml.XmlDictionaryString" /> value.</param>
	public virtual void WriteValue(XmlDictionaryString value)
	{
		WriteValue(XmlDictionaryString.GetString(value));
	}

	/// <summary>Writes a value from an <see cref="T:System.Xml.IStreamProvider" />.</summary>
	/// <param name="value">The <see cref="T:System.Xml.IStreamProvider" /> value to write.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.Xml.XmlException">
	///   <paramref name="value" /> returns a null stream object.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlDictionaryWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual void WriteValue(IStreamProvider value)
	{
		if (value == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
		}
		Stream stream = value.GetStream();
		if (stream == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString("Stream returned by IStreamProvider cannot be null.")));
		}
		int num = 256;
		int num2 = 0;
		byte[] buffer = new byte[num];
		while (true)
		{
			num2 = stream.Read(buffer, 0, num);
			if (num2 <= 0)
			{
				break;
			}
			WriteBase64(buffer, 0, num2);
			if (num < 65536 && num2 == num)
			{
				num *= 16;
				buffer = new byte[num];
			}
		}
		value.ReleaseStream(stream);
	}

	/// <summary>Asynchronously writes a value from an <see cref="T:System.Xml.IStreamProvider" />.</summary>
	/// <returns>The task that represents the asynchronous WriteValue operation.</returns>
	/// <param name="value">The <see cref="T:System.Xml.IStreamProvider" /> value to write.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlDictionaryWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlDictionaryWriter" /> asynchronous method was called without setting the <see cref="P:System.Xml.XmlWriterSettings.Async" /> flag to true. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “Set XmlWriterSettings.Async to true if you want to use Async Methods.”</exception>
	public virtual Task WriteValueAsync(IStreamProvider value)
	{
		return Task.Factory.FromAsync(BeginWriteValue, EndWriteValue, value, null);
	}

	internal virtual IAsyncResult BeginWriteValue(IStreamProvider value, AsyncCallback callback, object state)
	{
		if (value == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
		}
		if (FastAsync)
		{
			return new WriteValueFastAsyncResult(this, value, callback, state);
		}
		return new WriteValueAsyncResult(this, value, callback, state);
	}

	internal virtual void EndWriteValue(IAsyncResult result)
	{
		if (FastAsync)
		{
			WriteValueFastAsyncResult.End(result);
		}
		else
		{
			WriteValueAsyncResult.End(result);
		}
	}

	/// <summary>Writes a Unique Id value.</summary>
	/// <param name="value">The Unique Id value to write.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public virtual void WriteValue(UniqueId value)
	{
		if (value == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
		}
		WriteString(value.ToString());
	}

	/// <summary>Writes a <see cref="T:System.Guid" /> value.</summary>
	/// <param name="value">The <see cref="T:System.Guid" /> value to write.</param>
	public virtual void WriteValue(Guid value)
	{
		WriteString(value.ToString());
	}

	/// <summary>Writes a <see cref="T:System.TimeSpan" /> value.</summary>
	/// <param name="value">The <see cref="T:System.TimeSpan" /> value to write.</param>
	public virtual void WriteValue(TimeSpan value)
	{
		WriteString(XmlConvert.ToString(value));
	}

	/// <summary>When implemented by a derived class, it starts the canonicalization.</summary>
	/// <param name="stream">The stream to write to.</param>
	/// <param name="includeComments">true to include comments; otherwise, false.</param>
	/// <param name="inclusivePrefixes">The prefixes to be included.</param>
	/// <exception cref="T:System.NotSupportedException">Method is not implemented yet.</exception>
	/// <filterpriority>2</filterpriority>
	public virtual void StartCanonicalization(Stream stream, bool includeComments, string[] inclusivePrefixes)
	{
		throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
	}

	/// <summary>When implemented by a derived class, it stops the canonicalization started by the matching <see cref="M:System.Xml.XmlDictionaryWriter.StartCanonicalization(System.IO.Stream,System.Boolean,System.String[])" /> call.</summary>
	/// <exception cref="T:System.NotSupportedException">Method is not implemented yet.</exception>
	/// <filterpriority>2</filterpriority>
	public virtual void EndCanonicalization()
	{
		throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
	}

	private void WriteElementNode(XmlDictionaryReader reader, bool defattr)
	{
		if (reader.TryGetLocalNameAsDictionaryString(out var localName) && reader.TryGetNamespaceUriAsDictionaryString(out var namespaceUri))
		{
			WriteStartElement(reader.Prefix, localName, namespaceUri);
		}
		else
		{
			WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
		}
		if ((defattr || (!reader.IsDefault && (reader.SchemaInfo == null || !reader.SchemaInfo.IsDefault))) && reader.MoveToFirstAttribute())
		{
			do
			{
				if (reader.TryGetLocalNameAsDictionaryString(out localName) && reader.TryGetNamespaceUriAsDictionaryString(out namespaceUri))
				{
					WriteStartAttribute(reader.Prefix, localName, namespaceUri);
				}
				else
				{
					WriteStartAttribute(reader.Prefix, reader.LocalName, reader.NamespaceURI);
				}
				while (reader.ReadAttributeValue())
				{
					if (reader.NodeType == XmlNodeType.EntityReference)
					{
						WriteEntityRef(reader.Name);
					}
					else
					{
						WriteTextNode(reader, isAttribute: true);
					}
				}
				WriteEndAttribute();
			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		if (reader.IsEmptyElement)
		{
			WriteEndElement();
		}
	}

	private void WriteArrayNode(XmlDictionaryReader reader, string prefix, string localName, string namespaceUri, Type type)
	{
		if (type == typeof(bool))
		{
			BooleanArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
			return;
		}
		if (type == typeof(short))
		{
			Int16ArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
			return;
		}
		if (type == typeof(int))
		{
			Int32ArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
			return;
		}
		if (type == typeof(long))
		{
			Int64ArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
			return;
		}
		if (type == typeof(float))
		{
			SingleArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
			return;
		}
		if (type == typeof(double))
		{
			DoubleArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
			return;
		}
		if (type == typeof(decimal))
		{
			DecimalArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
			return;
		}
		if (type == typeof(DateTime))
		{
			DateTimeArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
			return;
		}
		if (type == typeof(Guid))
		{
			GuidArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
			return;
		}
		if (type == typeof(TimeSpan))
		{
			TimeSpanArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
			return;
		}
		WriteElementNode(reader, defattr: false);
		reader.Read();
	}

	private void WriteArrayNode(XmlDictionaryReader reader, string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, Type type)
	{
		if (type == typeof(bool))
		{
			BooleanArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
			return;
		}
		if (type == typeof(short))
		{
			Int16ArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
			return;
		}
		if (type == typeof(int))
		{
			Int32ArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
			return;
		}
		if (type == typeof(long))
		{
			Int64ArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
			return;
		}
		if (type == typeof(float))
		{
			SingleArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
			return;
		}
		if (type == typeof(double))
		{
			DoubleArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
			return;
		}
		if (type == typeof(decimal))
		{
			DecimalArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
			return;
		}
		if (type == typeof(DateTime))
		{
			DateTimeArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
			return;
		}
		if (type == typeof(Guid))
		{
			GuidArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
			return;
		}
		if (type == typeof(TimeSpan))
		{
			TimeSpanArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
			return;
		}
		WriteElementNode(reader, defattr: false);
		reader.Read();
	}

	private void WriteArrayNode(XmlDictionaryReader reader, Type type)
	{
		if (reader.TryGetLocalNameAsDictionaryString(out var localName) && reader.TryGetNamespaceUriAsDictionaryString(out var namespaceUri))
		{
			WriteArrayNode(reader, reader.Prefix, localName, namespaceUri, type);
		}
		else
		{
			WriteArrayNode(reader, reader.Prefix, reader.LocalName, reader.NamespaceURI, type);
		}
	}

	/// <summary>Writes the text node that an <see cref="T:System.Xml.XmlDictionaryReader" /> is currently positioned on.</summary>
	/// <param name="reader">The <see cref="T:System.Xml.XmlDictionaryReader" /> to get the text value from.</param>
	/// <param name="isAttribute">true to indicate that the reader is positioned on an attribute value or element content; otherwise, false.</param>
	protected virtual void WriteTextNode(XmlDictionaryReader reader, bool isAttribute)
	{
		if (reader.TryGetValueAsDictionaryString(out var value))
		{
			WriteString(value);
		}
		else
		{
			WriteString(reader.Value);
		}
		if (!isAttribute)
		{
			reader.Read();
		}
	}

	/// <summary>Writes the current XML node from an <see cref="T:System.Xml.XmlReader" />.</summary>
	/// <param name="reader">The <see cref="T:System.Xml.XmlReader" />.</param>
	/// <param name="defattr">true to copy the default attributes from the <see cref="T:System.Xml.XmlReader" />; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="reader" /> is null.</exception>
	public override void WriteNode(XmlReader reader, bool defattr)
	{
		if (reader is XmlDictionaryReader reader2)
		{
			WriteNode(reader2, defattr);
		}
		else
		{
			base.WriteNode(reader, defattr);
		}
	}

	/// <summary>Writes the current XML node from an <see cref="T:System.Xml.XmlDictionaryReader" />.</summary>
	/// <param name="reader">The <see cref="T:System.Xml.XmlDictionaryReader" />.</param>
	/// <param name="defattr">true to copy the default attributes from the XmlReader; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="reader" /> is null.</exception>
	public virtual void WriteNode(XmlDictionaryReader reader, bool defattr)
	{
		if (reader == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));
		}
		int num = ((reader.NodeType == XmlNodeType.None) ? (-1) : reader.Depth);
		do
		{
			XmlNodeType nodeType = reader.NodeType;
			if (nodeType == XmlNodeType.Text || nodeType == XmlNodeType.Whitespace || nodeType == XmlNodeType.SignificantWhitespace)
			{
				WriteTextNode(reader, isAttribute: false);
				continue;
			}
			if (reader.Depth > num && reader.IsStartArray(out var type))
			{
				WriteArrayNode(reader, type);
				continue;
			}
			switch (nodeType)
			{
			case XmlNodeType.Element:
				WriteElementNode(reader, defattr);
				break;
			case XmlNodeType.CDATA:
				WriteCData(reader.Value);
				break;
			case XmlNodeType.EntityReference:
				WriteEntityRef(reader.Name);
				break;
			case XmlNodeType.ProcessingInstruction:
			case XmlNodeType.XmlDeclaration:
				WriteProcessingInstruction(reader.Name, reader.Value);
				break;
			case XmlNodeType.DocumentType:
				WriteDocType(reader.Name, reader.GetAttribute("PUBLIC"), reader.GetAttribute("SYSTEM"), reader.Value);
				break;
			case XmlNodeType.Comment:
				WriteComment(reader.Value);
				break;
			case XmlNodeType.EndElement:
				WriteFullEndElement();
				break;
			}
			if (!reader.Read())
			{
				break;
			}
		}
		while (num < reader.Depth || (num == reader.Depth && reader.NodeType == XmlNodeType.EndElement));
	}

	private void CheckArray(Array array, int offset, int count)
	{
		if (array == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("array"));
		}
		if (offset < 0)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString("The value of this argument must be non-negative.")));
		}
		if (offset > array.Length)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString("The specified offset exceeds the buffer size ({0} bytes).", array.Length)));
		}
		if (count < 0)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString("The value of this argument must be non-negative.")));
		}
		if (count > array.Length - offset)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString("The specified size exceeds the remaining buffer space ({0} bytes).", array.Length - offset)));
		}
	}

	/// <summary>Writes nodes from a <see cref="T:System.Boolean" /> array.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array that contains the data.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of values to write from the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual void WriteArray(string prefix, string localName, string namespaceUri, bool[] array, int offset, int count)
	{
		CheckArray(array, offset, count);
		for (int i = 0; i < count; i++)
		{
			WriteStartElement(prefix, localName, namespaceUri);
			WriteValue(array[offset + i]);
			WriteEndElement();
		}
	}

	/// <summary>Writes nodes from a <see cref="T:System.Boolean" /> array.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array that contains the nodes.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to get from the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, bool[] array, int offset, int count)
	{
		WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
	}

	/// <summary>Writes nodes from a <see cref="T:System.Int16" /> array.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array that contains the nodes.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to get from the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual void WriteArray(string prefix, string localName, string namespaceUri, short[] array, int offset, int count)
	{
		CheckArray(array, offset, count);
		for (int i = 0; i < count; i++)
		{
			WriteStartElement(prefix, localName, namespaceUri);
			WriteValue(array[offset + i]);
			WriteEndElement();
		}
	}

	/// <summary>Writes nodes from a <see cref="T:System.Int16" /> array.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array that contains the nodes.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to get from the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, short[] array, int offset, int count)
	{
		WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
	}

	/// <summary>Writes nodes from a <see cref="T:System.Int32" /> array.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array that contains the nodes.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to get from the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual void WriteArray(string prefix, string localName, string namespaceUri, int[] array, int offset, int count)
	{
		CheckArray(array, offset, count);
		for (int i = 0; i < count; i++)
		{
			WriteStartElement(prefix, localName, namespaceUri);
			WriteValue(array[offset + i]);
			WriteEndElement();
		}
	}

	/// <summary>Writes nodes from a <see cref="T:System.Int32" /> array.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array that contains the nodes.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to get from the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, int[] array, int offset, int count)
	{
		WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
	}

	/// <summary>Writes nodes from a <see cref="T:System.Int64" /> array.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array that contains the nodes.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to get from the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual void WriteArray(string prefix, string localName, string namespaceUri, long[] array, int offset, int count)
	{
		CheckArray(array, offset, count);
		for (int i = 0; i < count; i++)
		{
			WriteStartElement(prefix, localName, namespaceUri);
			WriteValue(array[offset + i]);
			WriteEndElement();
		}
	}

	/// <summary>Writes nodes from a <see cref="T:System.Int64" /> array.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array that contains the nodes.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to get from the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, long[] array, int offset, int count)
	{
		WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
	}

	/// <summary>Writes nodes from a <see cref="T:System.Single" /> array.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array that contains the nodes.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to get from the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual void WriteArray(string prefix, string localName, string namespaceUri, float[] array, int offset, int count)
	{
		CheckArray(array, offset, count);
		for (int i = 0; i < count; i++)
		{
			WriteStartElement(prefix, localName, namespaceUri);
			WriteValue(array[offset + i]);
			WriteEndElement();
		}
	}

	/// <summary>Writes nodes from a <see cref="T:System.Single" /> array.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array that contains the nodes.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to get from the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, float[] array, int offset, int count)
	{
		WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
	}

	/// <summary>Writes nodes from a <see cref="T:System.Double" /> array.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array that contains the nodes.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to get from the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual void WriteArray(string prefix, string localName, string namespaceUri, double[] array, int offset, int count)
	{
		CheckArray(array, offset, count);
		for (int i = 0; i < count; i++)
		{
			WriteStartElement(prefix, localName, namespaceUri);
			WriteValue(array[offset + i]);
			WriteEndElement();
		}
	}

	/// <summary>Writes nodes from a <see cref="T:System.Double" /> array.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array that contains the nodes.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to get from the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, double[] array, int offset, int count)
	{
		WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
	}

	/// <summary>Writes nodes from a <see cref="T:System.Decimal" /> array.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array that contains the nodes.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to get from the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual void WriteArray(string prefix, string localName, string namespaceUri, decimal[] array, int offset, int count)
	{
		CheckArray(array, offset, count);
		for (int i = 0; i < count; i++)
		{
			WriteStartElement(prefix, localName, namespaceUri);
			WriteValue(array[offset + i]);
			WriteEndElement();
		}
	}

	/// <summary>Writes nodes from a <see cref="T:System.Decimal" /> array.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array that contains the nodes.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to get from the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, decimal[] array, int offset, int count)
	{
		WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
	}

	/// <summary>Writes nodes from a <see cref="T:System.DateTime" /> array.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array that contains the nodes.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to get from the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual void WriteArray(string prefix, string localName, string namespaceUri, DateTime[] array, int offset, int count)
	{
		CheckArray(array, offset, count);
		for (int i = 0; i < count; i++)
		{
			WriteStartElement(prefix, localName, namespaceUri);
			WriteValue(array[offset + i]);
			WriteEndElement();
		}
	}

	/// <summary>Writes nodes from a <see cref="T:System.DateTime" /> array.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array that contains the nodes.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to get from the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, DateTime[] array, int offset, int count)
	{
		WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
	}

	/// <summary>Writes nodes from a <see cref="T:System.Guid" /> array.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array that contains the nodes.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to get from the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual void WriteArray(string prefix, string localName, string namespaceUri, Guid[] array, int offset, int count)
	{
		CheckArray(array, offset, count);
		for (int i = 0; i < count; i++)
		{
			WriteStartElement(prefix, localName, namespaceUri);
			WriteValue(array[offset + i]);
			WriteEndElement();
		}
	}

	/// <summary>Writes nodes from a <see cref="T:System.Guid" /> array.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array that contains the nodes.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to get from the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, Guid[] array, int offset, int count)
	{
		WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
	}

	/// <summary>Writes nodes from a <see cref="T:System.TimeSpan" /> array.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array that contains the nodes.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to get from the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual void WriteArray(string prefix, string localName, string namespaceUri, TimeSpan[] array, int offset, int count)
	{
		CheckArray(array, offset, count);
		for (int i = 0; i < count; i++)
		{
			WriteStartElement(prefix, localName, namespaceUri);
			WriteValue(array[offset + i]);
			WriteEndElement();
		}
	}

	/// <summary>Writes nodes from a <see cref="T:System.TimeSpan" /> array.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array that contains the nodes.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to get from the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, TimeSpan[] array, int offset, int count)
	{
		WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlDictionaryWriter" /> class.</summary>
	protected XmlDictionaryWriter()
	{
	}
}
