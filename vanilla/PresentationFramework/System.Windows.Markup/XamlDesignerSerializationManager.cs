using System.ComponentModel;
using System.Xml;

namespace System.Windows.Markup;

/// <summary>Provides services for XAML serialization by XAML designers or other callers that require advanced serialization.</summary>
public class XamlDesignerSerializationManager : ServiceProviders
{
	private XamlWriterMode _xamlWriterMode;

	private XmlWriter _xmlWriter;

	/// <summary>Gets or sets the XAML writer mode.</summary>
	/// <returns>The XAML writer mode.</returns>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">Not a valid member of the <see cref="T:System.Windows.Markup.XamlWriterMode" /> enumeration.</exception>
	public XamlWriterMode XamlWriterMode
	{
		get
		{
			return _xamlWriterMode;
		}
		set
		{
			if (!IsValidXamlWriterMode(value))
			{
				throw new InvalidEnumArgumentException("value", (int)value, typeof(XamlWriterMode));
			}
			_xamlWriterMode = value;
		}
	}

	internal XmlWriter XmlWriter => _xmlWriter;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XamlDesignerSerializationManager" /> class.</summary>
	/// <param name="xmlWriter">The XML writer implementation to use as basis for the <see cref="T:System.Windows.Markup.XamlDesignerSerializationManager" />. </param>
	public XamlDesignerSerializationManager(XmlWriter xmlWriter)
	{
		_xamlWriterMode = XamlWriterMode.Value;
		_xmlWriter = xmlWriter;
	}

	internal void ClearXmlWriter()
	{
		_xmlWriter = null;
	}

	private static bool IsValidXamlWriterMode(XamlWriterMode value)
	{
		if (value != XamlWriterMode.Value)
		{
			return value == XamlWriterMode.Expression;
		}
		return true;
	}
}
