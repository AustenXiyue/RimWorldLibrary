namespace System.Windows.Controls;

/// <summary>Provides a resource key for an <see cref="T:System.Windows.Controls.ItemContainerTemplate" /> object.</summary>
public class ItemContainerTemplateKey : TemplateKey
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ItemContainerTemplateKey" /> class.</summary>
	public ItemContainerTemplateKey()
		: base(TemplateType.TableTemplate)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ItemContainerTemplateKey" /> class with the specified data type.</summary>
	/// <param name="dataType">The type for which this template is designed.</param>
	public ItemContainerTemplateKey(object dataType)
		: base(TemplateType.TableTemplate, dataType)
	{
	}
}
