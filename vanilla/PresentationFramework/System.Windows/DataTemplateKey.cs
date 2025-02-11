namespace System.Windows;

/// <summary>Represents the resource key for the <see cref="T:System.Windows.DataTemplate" /> class.</summary>
public class DataTemplateKey : TemplateKey
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.DataTemplateKey" /> class.</summary>
	public DataTemplateKey()
		: base(TemplateType.DataTemplate)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.DataTemplateKey" /> class with the specified type.</summary>
	/// <param name="dataType">The type for which this template is designed. This is either a <see cref="T:System.Type" /> (to indicate that the <see cref="T:System.Windows.DataTemplate" /> is used to display items of the given type), or a string (to indicate that the <see cref="T:System.Windows.DataTemplate" /> is used to display <see cref="T:System.Xml.XmlNode" /> elements with the given tag name).</param>
	public DataTemplateKey(object dataType)
		: base(TemplateType.DataTemplate, dataType)
	{
	}
}
