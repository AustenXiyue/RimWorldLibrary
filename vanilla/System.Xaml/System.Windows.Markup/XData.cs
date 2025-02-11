using System.IO;
using System.Xml;

namespace System.Windows.Markup;

/// <summary>Represents literal data that can appear as the value for a Value node.</summary>
[ContentProperty("Text")]
public sealed class XData
{
	private XmlReader _reader;

	private string _text;

	/// <summary>Gets or sets the literal value string that this <see cref="T:System.Windows.Markup.XData" /> wraps.</summary>
	/// <returns>The literal value string.</returns>
	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			_text = value;
			_reader = null;
		}
	}

	/// <summary>Gets or sets a reader for the literal data.</summary>
	/// <returns>A reader for the literal data.</returns>
	public object XmlReader
	{
		get
		{
			if (_reader == null)
			{
				StringReader input = new StringReader(Text);
				_reader = System.Xml.XmlReader.Create(input);
			}
			return _reader;
		}
		set
		{
			_reader = value as XmlReader;
			_text = null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XData" /> class. </summary>
	public XData()
	{
	}
}
