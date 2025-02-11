using System.Runtime.CompilerServices;
using MS.Internal.Globalization;

namespace System.Windows;

/// <summary>The <see cref="T:System.Windows.Localization" /> class defines attached properties for localization attributes and comments.</summary>
public static class Localization
{
	/// <summary>Identifies the <see cref="P:System.Windows.Localization.Comments" /> attached property. </summary>
	/// <returns>The <see cref="P:System.Windows.Localization.Comments" /> attached property identifier.</returns>
	public static readonly DependencyProperty CommentsProperty = DependencyProperty.RegisterAttached("Comments", typeof(string), typeof(Localization));

	/// <summary>Identifies the <see cref="P:System.Windows.Localization.Attributes" /> attached property. </summary>
	/// <returns>The <see cref="P:System.Windows.Localization.Attributes" /> attached property identifier.</returns>
	public static readonly DependencyProperty AttributesProperty = DependencyProperty.RegisterAttached("Attributes", typeof(string), typeof(Localization));

	private static ConditionalWeakTable<object, string> _commentsOnObjects = new ConditionalWeakTable<object, string>();

	private static ConditionalWeakTable<object, string> _attributesOnObjects = new ConditionalWeakTable<object, string>();

	/// <summary>Gets the value of the <see cref="F:System.Windows.Localization.CommentsProperty" /> attached property from a specified element.</summary>
	/// <returns>A <see cref="T:System.String" /> value that represents the localization comment.</returns>
	/// <param name="element">A <see cref="T:System.Object" /> that represents the element whose attached property you want to retrieve.</param>
	[AttachedPropertyBrowsableForType(typeof(object))]
	public static string GetComments(object element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return GetValue(element, CommentsProperty);
	}

	/// <summary>Sets the <see cref="F:System.Windows.Localization.CommentsProperty" /> attached property to the specified element.</summary>
	/// <param name="element">A <see cref="T:System.Object" /> that represents the element whose attached property you want to set.</param>
	/// <param name="comments">A <see cref="T:System.String" /> that specifies the localization comments.</param>
	public static void SetComments(object element, string comments)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		LocComments.ParsePropertyComments(comments);
		SetValue(element, CommentsProperty, comments);
	}

	/// <summary>Gets the value of the <see cref="F:System.Windows.Localization.AttributesProperty" /> attached property from a specified element.</summary>
	/// <returns>A <see cref="T:System.String" /> value that represents the localization attribute.</returns>
	/// <param name="element">A <see cref="T:System.Object" /> that represents the element whose attached property you want to retrieve.</param>
	[AttachedPropertyBrowsableForType(typeof(object))]
	public static string GetAttributes(object element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return GetValue(element, AttributesProperty);
	}

	/// <summary>Sets the <see cref="F:System.Windows.Localization.AttributesProperty" /> attached property for the specified element.</summary>
	/// <param name="element">A <see cref="T:System.Object" /> that represents the element whose attached property you want to set.</param>
	/// <param name="attributes">A <see cref="T:System.String" /> that specifies the localization attributes.</param>
	public static void SetAttributes(object element, string attributes)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		LocComments.ParsePropertyLocalizabilityAttributes(attributes);
		SetValue(element, AttributesProperty, attributes);
	}

	private static string GetValue(object element, DependencyProperty property)
	{
		if (element is DependencyObject dependencyObject)
		{
			return (string)dependencyObject.GetValue(property);
		}
		string value;
		if (property == CommentsProperty)
		{
			_commentsOnObjects.TryGetValue(element, out value);
		}
		else
		{
			_attributesOnObjects.TryGetValue(element, out value);
		}
		return value;
	}

	private static void SetValue(object element, DependencyProperty property, string value)
	{
		if (element is DependencyObject dependencyObject)
		{
			dependencyObject.SetValue(property, value);
		}
		else if (property == CommentsProperty)
		{
			_commentsOnObjects.Remove(element);
			_commentsOnObjects.Add(element, value);
		}
		else
		{
			_attributesOnObjects.Remove(element);
			_attributesOnObjects.Add(element, value);
		}
	}
}
