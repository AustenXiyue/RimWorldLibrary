using System.Xaml;

namespace System.Windows.Baml2006;

internal class StaticResource
{
	public XamlNodeList ResourceNodeList { get; private set; }

	public StaticResource(XamlType type, XamlSchemaContext schemaContext)
	{
		ResourceNodeList = new XamlNodeList(schemaContext, 8);
		ResourceNodeList.Writer.WriteStartObject(type);
	}
}
