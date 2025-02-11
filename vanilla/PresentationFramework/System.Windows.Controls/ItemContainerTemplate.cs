using System.Windows.Markup;

namespace System.Windows.Controls;

/// <summary>Provides the template for producing a container for an <see cref="T:System.Windows.Controls.ItemsControl" /> object. </summary>
[DictionaryKeyProperty("ItemContainerTemplateKey")]
public class ItemContainerTemplate : DataTemplate
{
	/// <summary>Gets the default key of the <see cref="T:System.Windows.Controls.ItemContainerTemplate" />. </summary>
	/// <returns>The default key of the <see cref="T:System.Windows.Controls.ItemContainerTemplate" />.</returns>
	public object ItemContainerTemplateKey
	{
		get
		{
			if (base.DataType == null)
			{
				return null;
			}
			return new ItemContainerTemplateKey(base.DataType);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ItemContainerTemplate" /> class.</summary>
	public ItemContainerTemplate()
	{
	}
}
