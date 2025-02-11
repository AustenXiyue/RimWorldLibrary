using MS.Internal.Xaml.Context;

namespace System.Xaml;

internal class DeferringWriter : XamlWriter, IXamlLineInfoConsumer
{
	private DeferringMode _mode;

	private bool _handled;

	private ObjectWriterContext _context;

	private XamlNodeList _deferredList;

	private XamlWriter _deferredWriter;

	private IXamlLineInfoConsumer _deferredLineInfoConsumer;

	private int _deferredTreeDepth;

	public bool Handled => _handled;

	public DeferringMode Mode => _mode;

	public override XamlSchemaContext SchemaContext => _context.SchemaContext;

	public bool ShouldProvideLineInfo => true;

	public DeferringWriter(ObjectWriterContext context)
	{
		_context = context;
		_mode = DeferringMode.Off;
	}

	public void Clear()
	{
		_handled = false;
		_mode = DeferringMode.Off;
		_deferredList = null;
		_deferredTreeDepth = -1;
	}

	public XamlNodeList CollectTemplateList()
	{
		XamlNodeList deferredList = _deferredList;
		_deferredList = null;
		_mode = DeferringMode.Off;
		return deferredList;
	}

	public override void WriteGetObject()
	{
		WriteObject(null, fromMember: true, "WriteGetObject");
	}

	public override void WriteStartObject(XamlType xamlType)
	{
		WriteObject(xamlType, fromMember: false, "WriteStartObject");
	}

	private void WriteObject(XamlType xamlType, bool fromMember, string methodName)
	{
		_handled = false;
		switch (_mode)
		{
		case DeferringMode.TemplateReady:
			throw new XamlInternalException(System.SR.Format(System.SR.TemplateNotCollected, methodName));
		case DeferringMode.TemplateStarting:
			StartDeferredList();
			_mode = DeferringMode.TemplateDeferring;
			goto case DeferringMode.TemplateDeferring;
		case DeferringMode.TemplateDeferring:
			if (fromMember)
			{
				_deferredWriter.WriteGetObject();
			}
			else
			{
				_deferredWriter.WriteStartObject(xamlType);
			}
			_deferredTreeDepth++;
			_handled = true;
			break;
		default:
			throw new XamlInternalException(System.SR.Format(System.SR.MissingCase, _mode.ToString(), methodName));
		case DeferringMode.Off:
			break;
		}
	}

	public override void WriteEndObject()
	{
		_handled = false;
		switch (_mode)
		{
		case DeferringMode.TemplateReady:
			throw new XamlInternalException(System.SR.Format(System.SR.TemplateNotCollected, "WriteEndObject"));
		case DeferringMode.TemplateDeferring:
			_deferredWriter.WriteEndObject();
			_handled = true;
			_deferredTreeDepth--;
			if (_deferredTreeDepth == 0)
			{
				_deferredWriter.Close();
				_deferredWriter = null;
				_mode = DeferringMode.TemplateReady;
			}
			break;
		default:
			throw new XamlInternalException(System.SR.Format(System.SR.MissingCase, _mode.ToString(), "WriteEndObject"));
		case DeferringMode.Off:
			break;
		}
	}

	public override void WriteStartMember(XamlMember property)
	{
		_handled = false;
		switch (_mode)
		{
		case DeferringMode.Off:
			if (property.DeferringLoader != null)
			{
				_mode = DeferringMode.TemplateStarting;
			}
			break;
		case DeferringMode.TemplateReady:
			throw new XamlInternalException(System.SR.Format(System.SR.TemplateNotCollected, "WriteMember"));
		case DeferringMode.TemplateDeferring:
			_deferredWriter.WriteStartMember(property);
			_handled = true;
			break;
		default:
			throw new XamlInternalException(System.SR.Format(System.SR.MissingCase, _mode.ToString(), "WriteMember"));
		}
	}

	public override void WriteEndMember()
	{
		_handled = false;
		switch (_mode)
		{
		case DeferringMode.TemplateReady:
			throw new XamlInternalException(System.SR.Format(System.SR.TemplateNotCollected, "WriteEndMember"));
		case DeferringMode.TemplateDeferring:
			_deferredWriter.WriteEndMember();
			_handled = true;
			break;
		default:
			throw new XamlInternalException(System.SR.Format(System.SR.MissingCase, _mode.ToString(), "WriteEndMember"));
		case DeferringMode.Off:
			break;
		}
	}

	public override void WriteValue(object value)
	{
		_handled = false;
		switch (_mode)
		{
		case DeferringMode.TemplateReady:
			throw new XamlInternalException(System.SR.Format(System.SR.TemplateNotCollected, "WriteValue"));
		case DeferringMode.TemplateStarting:
			if (value is XamlNodeList)
			{
				_deferredList = (XamlNodeList)value;
				_mode = DeferringMode.TemplateReady;
				_handled = true;
				break;
			}
			StartDeferredList();
			_mode = DeferringMode.TemplateDeferring;
			goto case DeferringMode.TemplateDeferring;
		case DeferringMode.TemplateDeferring:
			_deferredWriter.WriteValue(value);
			_handled = true;
			break;
		default:
			throw new XamlInternalException(System.SR.Format(System.SR.MissingCase, _mode.ToString(), "WriteValue"));
		case DeferringMode.Off:
			break;
		}
	}

	public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
	{
		switch (_mode)
		{
		case DeferringMode.Off:
			break;
		case DeferringMode.TemplateReady:
			throw new XamlInternalException(System.SR.Format(System.SR.TemplateNotCollected, "WriteNamespace"));
		case DeferringMode.TemplateStarting:
			StartDeferredList();
			_mode = DeferringMode.TemplateDeferring;
			goto case DeferringMode.TemplateDeferring;
		case DeferringMode.TemplateDeferring:
			_deferredWriter.WriteNamespace(namespaceDeclaration);
			_handled = true;
			break;
		default:
			throw new XamlInternalException(System.SR.Format(System.SR.MissingCase, _mode.ToString(), "WriteNamespace"));
		}
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && !base.IsDisposed && _deferredWriter != null)
			{
				_deferredWriter.Close();
				_deferredWriter = null;
				_deferredLineInfoConsumer = null;
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	public void SetLineInfo(int lineNumber, int linePosition)
	{
		switch (_mode)
		{
		case DeferringMode.Off:
			break;
		case DeferringMode.TemplateReady:
			throw new XamlInternalException(System.SR.Format(System.SR.TemplateNotCollected, "SetLineInfo"));
		case DeferringMode.TemplateStarting:
			StartDeferredList();
			goto case DeferringMode.TemplateDeferring;
		case DeferringMode.TemplateDeferring:
			if (_deferredLineInfoConsumer != null)
			{
				_deferredLineInfoConsumer.SetLineInfo(lineNumber, linePosition);
			}
			break;
		default:
			throw new XamlInternalException(System.SR.Format(System.SR.MissingCase, _mode.ToString(), "SetLineInfo"));
		}
	}

	private void StartDeferredList()
	{
		if (_deferredList == null)
		{
			_deferredList = new XamlNodeList(_context.SchemaContext);
			_deferredWriter = _deferredList.Writer;
			_deferredLineInfoConsumer = _deferredWriter as IXamlLineInfoConsumer;
			_deferredTreeDepth = 0;
		}
	}
}
