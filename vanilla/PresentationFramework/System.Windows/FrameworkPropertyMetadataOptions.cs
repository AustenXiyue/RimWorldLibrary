namespace System.Windows;

/// <summary>Specifies the types of framework-level property behavior that pertain to a particular dependency property in the Windows Presentation Foundation (WPF) property system.</summary>
[Flags]
public enum FrameworkPropertyMetadataOptions
{
	/// <summary>No options are specified; the dependency property uses the default behavior of the Windows Presentation Foundation (WPF) property system.</summary>
	None = 0,
	/// <summary>The measure pass of layout compositions is affected by value changes to this dependency property. </summary>
	AffectsMeasure = 1,
	/// <summary>The arrange pass of layout composition is affected by value changes to this dependency property. </summary>
	AffectsArrange = 2,
	/// <summary>The measure pass on the parent element is affected by value changes to this dependency property.</summary>
	AffectsParentMeasure = 4,
	/// <summary>The arrange pass on the parent element is affected by value changes to this dependency property.</summary>
	AffectsParentArrange = 8,
	/// <summary>Some aspect of rendering or layout composition (other than measure or arrange) is affected by value changes to this dependency property.</summary>
	AffectsRender = 0x10,
	/// <summary>The values of this dependency property are inherited by child elements.</summary>
	Inherits = 0x20,
	/// <summary>The values of this dependency property span separated trees for purposes of property value inheritance. </summary>
	OverridesInheritanceBehavior = 0x40,
	/// <summary>Data binding to this dependency property is not allowed.</summary>
	NotDataBindable = 0x80,
	/// <summary>The <see cref="T:System.Windows.Data.BindingMode" /> for data bindings on this dependency property defaults to <see cref="F:System.Windows.Data.BindingMode.TwoWay" />.</summary>
	BindsTwoWayByDefault = 0x100,
	/// <summary>The values of this dependency property should be saved or restored by journaling processes, or when navigating by Uniform resource identifiers (URIs). </summary>
	Journal = 0x400,
	/// <summary>The subproperties on the value of this dependency property do not affect any aspect of rendering.</summary>
	SubPropertiesDoNotAffectRender = 0x800
}
