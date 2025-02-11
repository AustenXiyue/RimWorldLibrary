using System.Collections.Generic;
using System.ComponentModel;
using System.Xaml;
using System.Xaml.Schema;

namespace System.Windows.Markup;

/// <summary>Supports a markup technique of defining properties of a class in declarative XAML.</summary>
public class PropertyDefinition : MemberDefinition
{
	private IList<Attribute> attributes;

	/// <summary>Gets or sets the name of the property to define.</summary>
	/// <returns>The name of the property to define.</returns>
	public override string Name { get; set; }

	/// <summary>Gets or sets the <see cref="T:System.Xaml.XamlType" /> of the property to define.</summary>
	/// <returns>The XAML type identifier of the property to define.</returns>
	[TypeConverter(typeof(XamlTypeTypeConverter))]
	public XamlType Type { get; set; }

	/// <summary>Gets or sets the access level of the defined property.</summary>
	/// <returns>The access level of the defined property, as a string.</returns>
	[DefaultValue(null)]
	public string Modifier { get; set; }

	/// <summary>Gets a list of associated CLR attributes.</summary>
	/// <returns>A list of associated attributes.</returns>
	public IList<Attribute> Attributes
	{
		get
		{
			if (attributes == null)
			{
				attributes = new List<Attribute>();
			}
			return attributes;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.PropertyDefinition" /> class. </summary>
	public PropertyDefinition()
	{
	}
}
