namespace System.Windows.Markup.Localizer;

/// <summary>Specifies error conditions that may be encountered by the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizer" />.</summary>
public enum BamlLocalizerError
{
	/// <summary>More than one element has the same <see cref="P:System.Windows.Markup.Localizer.BamlLocalizableResourceKey.Uid" /> value.</summary>
	DuplicateUid,
	/// <summary>The localized BAML  contains more than one reference to the same element.</summary>
	DuplicateElement,
	/// <summary>The element's substitution contains incomplete child placeholders.</summary>
	IncompleteElementPlaceholder,
	/// <summary>XML comments do not have the correct format.</summary>
	InvalidCommentingXml,
	/// <summary>The localization commenting text contains invalid attributes.</summary>
	InvalidLocalizationAttributes,
	/// <summary>The localization commenting text contains invalid comments.</summary>
	InvalidLocalizationComments,
	/// <summary>The <see cref="P:System.Windows.Markup.Localizer.BamlLocalizableResourceKey.Uid" /> does not correspond to any element in the BAML source.</summary>
	InvalidUid,
	/// <summary>Indicates a mismatch between substitution and source. The substitution must contain all the element placeholders in the source.</summary>
	MismatchedElements,
	/// <summary>The substitution of an element's content cannot be parsed as XML, therefore any formatting tags in the substitution are not recognized. The substitution is instead applied as plain text.</summary>
	SubstitutionAsPlaintext,
	/// <summary>A child element does not have a <see cref="P:System.Windows.Markup.Localizer.BamlLocalizableResourceKey.Uid" />. As a result, it cannot be represented as a placeholder in the parent's content string.</summary>
	UidMissingOnChildElement,
	/// <summary>A formatting tag in the substitution is not recognized.</summary>
	UnknownFormattingTag
}
