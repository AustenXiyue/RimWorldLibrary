using System.Collections.Generic;
using MS.Internal.Xaml.Context;

namespace System.Windows.Baml2006;

internal class Baml2006ReaderContext
{
	private Baml2006SchemaContext _schemaContext;

	private MS.Internal.Xaml.Context.XamlContextStack<Baml2006ReaderFrame> _stack = new MS.Internal.Xaml.Context.XamlContextStack<Baml2006ReaderFrame>(() => new Baml2006ReaderFrame());

	public Baml2006SchemaContext SchemaContext => _schemaContext;

	public Baml2006ReaderFrame CurrentFrame => _stack.CurrentFrame;

	public Baml2006ReaderFrame PreviousFrame => _stack.PreviousFrame;

	public List<KeyRecord> KeyList { get; set; }

	public int CurrentKey { get; set; }

	public KeyRecord LastKey
	{
		get
		{
			if (KeyList != null && KeyList.Count > 0)
			{
				return KeyList[KeyList.Count - 1];
			}
			return null;
		}
	}

	public bool InsideKeyRecord { get; set; }

	public bool InsideStaticResource { get; set; }

	public int TemplateStartDepth { get; set; }

	public int LineNumber { get; set; }

	public int LineOffset { get; set; }

	public Baml2006ReaderContext(Baml2006SchemaContext schemaContext)
	{
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		_schemaContext = schemaContext;
	}

	public void PushScope()
	{
		_stack.PushScope();
		CurrentFrame.FreezeFreezables = PreviousFrame.FreezeFreezables;
	}

	public void PopScope()
	{
		_stack.PopScope();
	}
}
