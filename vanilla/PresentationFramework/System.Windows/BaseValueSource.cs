namespace System.Windows;

/// <summary>Identifies the property system source of a particular dependency property value.</summary>
public enum BaseValueSource
{
	/// <summary>Source is not known. This is the default value.</summary>
	Unknown,
	/// <summary>Source is the default value, as defined by property metadata. </summary>
	Default,
	/// <summary>Source is a value through property value inheritance.</summary>
	Inherited,
	/// <summary>Source is from a setter in the default style. The default style comes from the current theme. </summary>
	DefaultStyle,
	/// <summary>Source is from a trigger in the default style. The default style comes from the current theme.</summary>
	DefaultStyleTrigger,
	/// <summary>Source is from a style setter of a non-theme style.</summary>
	Style,
	/// <summary>Source is a trigger-based value in a template that is from a non-theme style. </summary>
	TemplateTrigger,
	/// <summary>Source is a trigger-based value of a non-theme style.</summary>
	StyleTrigger,
	/// <summary>Source is an implicit style reference (style was based on detected type or based type). This value is only returned for the Style property itself, not for properties that are set through setters or triggers of such a style.</summary>
	ImplicitStyleReference,
	/// <summary>Source is based on a parent template being used by an element.</summary>
	ParentTemplate,
	/// <summary>Source is a trigger-based value from a parent template that created the element.</summary>
	ParentTemplateTrigger,
	/// <summary>Source is a locally set value.</summary>
	Local
}
