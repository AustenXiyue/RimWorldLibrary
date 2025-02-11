using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace System.Windows.Markup;

/// <summary>Encapsulates the XML language-related attributes of a <see cref="T:System.Windows.DependencyObject" />. </summary>
public sealed class XmlAttributeProperties
{
	/// <summary>Identifies the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlSpace" /> attached property.</summary>
	[Browsable(false)]
	[Localizability(LocalizationCategory.NeverLocalize)]
	public static readonly DependencyProperty XmlSpaceProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlnsDictionary" /> attached property.</summary>
	[Browsable(false)]
	public static readonly DependencyProperty XmlnsDictionaryProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlnsDefinition" /> attached property.</summary>
	[Browsable(false)]
	public static readonly DependencyProperty XmlnsDefinitionProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlNamespaceMaps" /> attached property.</summary>
	[Browsable(false)]
	public static readonly DependencyProperty XmlNamespaceMapsProperty;

	internal static readonly string XmlSpaceString;

	internal static readonly string XmlLangString;

	internal static readonly string XmlnsDefinitionString;

	private static MethodInfo _xmlSpaceSetter;

	internal static MethodInfo XmlSpaceSetter
	{
		get
		{
			if (_xmlSpaceSetter == null)
			{
				_xmlSpaceSetter = typeof(XmlAttributeProperties).GetMethod("SetXmlSpace", BindingFlags.Static | BindingFlags.Public);
			}
			return _xmlSpaceSetter;
		}
	}

	private XmlAttributeProperties()
	{
	}

	static XmlAttributeProperties()
	{
		XmlSpaceString = "xml:space";
		XmlLangString = "xml:lang";
		XmlnsDefinitionString = "xmlns";
		_xmlSpaceSetter = null;
		XmlSpaceProperty = DependencyProperty.RegisterAttached("XmlSpace", typeof(string), typeof(XmlAttributeProperties), new FrameworkPropertyMetadata("default"));
		XmlnsDictionaryProperty = DependencyProperty.RegisterAttached("XmlnsDictionary", typeof(XmlnsDictionary), typeof(XmlAttributeProperties), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
		XmlnsDefinitionProperty = DependencyProperty.RegisterAttached("XmlnsDefinition", typeof(string), typeof(XmlAttributeProperties), new FrameworkPropertyMetadata("http://schemas.microsoft.com/winfx/2006/xaml", FrameworkPropertyMetadataOptions.Inherits));
		XmlNamespaceMapsProperty = DependencyProperty.RegisterAttached("XmlNamespaceMaps", typeof(Hashtable), typeof(XmlAttributeProperties), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlSpace" /> attached property of the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <returns>The value of the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlSpace" /> attached property for the specified object.</returns>
	/// <param name="dependencyObject">The object to obtain the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlSpace" /> attached property value from.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dependencyObject" /> is null.</exception>
	[DesignerSerializationOptions(DesignerSerializationOptions.SerializeAsAttribute)]
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static string GetXmlSpace(DependencyObject dependencyObject)
	{
		if (dependencyObject == null)
		{
			throw new ArgumentNullException("dependencyObject");
		}
		return (string)dependencyObject.GetValue(XmlSpaceProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlSpace" /> attached property of the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <param name="dependencyObject">The object on which to set the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlSpace" /> attached property.</param>
	/// <param name="value">The string to use for an XML space.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dependencyObject" /> is null.</exception>
	public static void SetXmlSpace(DependencyObject dependencyObject, string value)
	{
		if (dependencyObject == null)
		{
			throw new ArgumentNullException("dependencyObject");
		}
		dependencyObject.SetValue(XmlSpaceProperty, value);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlnsDictionary" /> attached property of the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <returns>The value of the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlnsDictionary" /> attached property for the specified object.</returns>
	/// <param name="dependencyObject">The object to obtain the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlnsDictionary" /> attached property value from.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dependencyObject" /> is null.</exception>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static XmlnsDictionary GetXmlnsDictionary(DependencyObject dependencyObject)
	{
		if (dependencyObject == null)
		{
			throw new ArgumentNullException("dependencyObject");
		}
		return (XmlnsDictionary)dependencyObject.GetValue(XmlnsDictionaryProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlnsDictionary" /> attached property of the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <param name="dependencyObject">The object on which to set the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlnsDictionary" /> attached property.</param>
	/// <param name="value">The xmlns dictionary in string form.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dependencyObject" /> is null.</exception>
	public static void SetXmlnsDictionary(DependencyObject dependencyObject, XmlnsDictionary value)
	{
		if (dependencyObject == null)
		{
			throw new ArgumentNullException("dependencyObject");
		}
		if (!dependencyObject.IsSealed)
		{
			dependencyObject.SetValue(XmlnsDictionaryProperty, value);
		}
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlnsDefinition" /> attached property of the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <returns>The value of the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlnsDefinition" /> attached property for the specified object.</returns>
	/// <param name="dependencyObject">The object to obtain the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlnsDefinition" /> attached property value from.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dependencyObject" /> is null.</exception>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DesignerSerializationOptions(DesignerSerializationOptions.SerializeAsAttribute)]
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static string GetXmlnsDefinition(DependencyObject dependencyObject)
	{
		if (dependencyObject == null)
		{
			throw new ArgumentNullException("dependencyObject");
		}
		return (string)dependencyObject.GetValue(XmlnsDefinitionProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlnsDefinition" /> attached property of the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <param name="dependencyObject">The object on which to set the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlnsDefinition" /> property.</param>
	/// <param name="value">The XML namespace definition in string form.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dependencyObject" /> is null.</exception>
	public static void SetXmlnsDefinition(DependencyObject dependencyObject, string value)
	{
		if (dependencyObject == null)
		{
			throw new ArgumentNullException("dependencyObject");
		}
		dependencyObject.SetValue(XmlnsDefinitionProperty, value);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlNamespaceMaps" /> attached property of the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <returns>The value of the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlNamespaceMaps" /> property for the specified object.</returns>
	/// <param name="dependencyObject">The object to obtain the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlNamespaceMaps" /> property from.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dependencyObject" /> is null.</exception>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static string GetXmlNamespaceMaps(DependencyObject dependencyObject)
	{
		if (dependencyObject == null)
		{
			throw new ArgumentNullException("dependencyObject");
		}
		return (string)dependencyObject.GetValue(XmlNamespaceMapsProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlNamespaceMaps" /> attached property of the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <param name="dependencyObject">The object on which to set the <see cref="P:System.Windows.Markup.XmlAttributeProperties.XmlNamespaceMaps" /> attached property.</param>
	/// <param name="value">The string value to set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dependencyObject" /> is null.</exception>
	public static void SetXmlNamespaceMaps(DependencyObject dependencyObject, string value)
	{
		if (dependencyObject == null)
		{
			throw new ArgumentNullException("dependencyObject");
		}
		dependencyObject.SetValue(XmlNamespaceMapsProperty, value);
	}
}
