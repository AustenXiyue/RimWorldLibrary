using System.ComponentModel;
using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Abstract class for all facets that are used when simple types are derived by restriction.</summary>
public abstract class XmlSchemaFacet : XmlSchemaAnnotated
{
	private string value;

	private bool isFixed;

	private FacetType facetType;

	/// <summary>Gets or sets the value attribute of the facet.</summary>
	/// <returns>The value attribute.</returns>
	[XmlAttribute("value")]
	public string Value
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
		}
	}

	/// <summary>Gets or sets information that indicates that this facet is fixed.</summary>
	/// <returns>If true, value is fixed; otherwise, false. The default is false.Optional.</returns>
	[DefaultValue(false)]
	[XmlAttribute("fixed")]
	public virtual bool IsFixed
	{
		get
		{
			return isFixed;
		}
		set
		{
			if (!(this is XmlSchemaEnumerationFacet) && !(this is XmlSchemaPatternFacet))
			{
				isFixed = value;
			}
		}
	}

	internal FacetType FacetType
	{
		get
		{
			return facetType;
		}
		set
		{
			facetType = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaFacet" /> class.</summary>
	protected XmlSchemaFacet()
	{
	}
}
