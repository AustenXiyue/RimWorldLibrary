using System.Windows;
using MS.Internal.KnownBoxes;

namespace System.ComponentModel;

/// <summary>Provides attached properties used to communicate with a designer.</summary>
public static class DesignerProperties
{
	/// <summary>Identifies the <see cref="P:System.ComponentModel.DesignerProperties.IsInDesignMode" /> attached property.</summary>
	public static readonly DependencyProperty IsInDesignModeProperty = DependencyProperty.RegisterAttached("IsInDesignMode", typeof(bool), typeof(DesignerProperties), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior));

	/// <summary>Gets the value of the <see cref="P:System.ComponentModel.DesignerProperties.IsInDesignMode" /> attached property for the specified <see cref="T:System.Windows.UIElement" />.</summary>
	/// <returns>The <see cref="P:System.ComponentModel.DesignerProperties.IsInDesignMode" /> property value for the element.</returns>
	/// <param name="element">The element from which the property value is read.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="element" /> is null.</exception>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public static bool GetIsInDesignMode(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsInDesignModeProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.ComponentModel.DesignerProperties.IsInDesignMode" /> attached property to a specified element. </summary>
	/// <param name="element">The element to which the attached property is written.</param>
	/// <param name="value">The needed <see cref="T:System.Boolean" /> value.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="element" /> is null.</exception>
	public static void SetIsInDesignMode(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsInDesignModeProperty, value);
	}
}
