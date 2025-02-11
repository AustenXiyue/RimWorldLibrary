namespace System.Windows.Markup.Localizer;

/// <summary>Represents localizability settings for an element in BAML. </summary>
public class ElementLocalizability
{
	private string _formattingTag;

	private LocalizabilityAttribute _attribute;

	/// <summary>Gets or sets the associated element's formatting tag. </summary>
	/// <returns>The formatting tag string.</returns>
	public string FormattingTag
	{
		get
		{
			return _formattingTag;
		}
		set
		{
			_formattingTag = value;
		}
	}

	/// <summary> Gets or sets the associated element's localizability attribute. </summary>
	/// <returns>The associated element's localizability attribute.</returns>
	public LocalizabilityAttribute Attribute
	{
		get
		{
			return _attribute;
		}
		set
		{
			_attribute = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.Localizer.ElementLocalizability" /> class. </summary>
	public ElementLocalizability()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.Localizer.ElementLocalizability" /> class with a specified formatting tag and localizability attribute. </summary>
	/// <param name="formattingTag">A formatting tag name. Assign a non-empty value to this parameter to indicate inline formatting.</param>
	/// <param name="attribute">The associated element's localizability attribute.</param>
	public ElementLocalizability(string formattingTag, LocalizabilityAttribute attribute)
	{
		_formattingTag = formattingTag;
		_attribute = attribute;
	}
}
