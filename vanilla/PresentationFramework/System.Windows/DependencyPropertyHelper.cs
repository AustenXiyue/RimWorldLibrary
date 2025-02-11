using MS.Internal;

namespace System.Windows;

/// <summary>Provides a single helper method (<see cref="M:System.Windows.DependencyPropertyHelper.GetValueSource(System.Windows.DependencyObject,System.Windows.DependencyProperty)" />) that reports the property system source for the effective value of a dependency property.</summary>
public static class DependencyPropertyHelper
{
	/// <summary>Returns a structure that reports various metadata and property system characteristics of a specified dependency property on a particular <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <returns>A <see cref="T:System.Windows.ValueSource" /> structure that reports the specific information.</returns>
	/// <param name="dependencyObject">The element that contains the <paramref name="dependencyProperty" /> to report information for.</param>
	/// <param name="dependencyProperty">The identifier for the dependency property to report information for.</param>
	public static ValueSource GetValueSource(DependencyObject dependencyObject, DependencyProperty dependencyProperty)
	{
		if (dependencyObject == null)
		{
			throw new ArgumentNullException("dependencyObject");
		}
		if (dependencyProperty == null)
		{
			throw new ArgumentNullException("dependencyProperty");
		}
		dependencyObject.VerifyAccess();
		bool hasModifiers;
		bool isExpression;
		bool isAnimated;
		bool isCoerced;
		bool isCurrent;
		return new ValueSource(dependencyObject.GetValueSource(dependencyProperty, null, out hasModifiers, out isExpression, out isAnimated, out isCoerced, out isCurrent), isExpression, isAnimated, isCoerced, isCurrent);
	}

	public static bool IsTemplatedValueDynamic(DependencyObject elementInTemplate, DependencyProperty dependencyProperty)
	{
		if (elementInTemplate == null)
		{
			throw new ArgumentNullException("elementInTemplate");
		}
		if (dependencyProperty == null)
		{
			throw new ArgumentNullException("dependencyProperty");
		}
		FrameworkObject frameworkObject = new FrameworkObject(elementInTemplate);
		DependencyObject container = frameworkObject.TemplatedParent ?? throw new ArgumentException(SR.ElementMustBelongToTemplate, "elementInTemplate");
		int templateChildIndex = frameworkObject.TemplateChildIndex;
		return StyleHelper.IsValueDynamic(container, templateChildIndex, dependencyProperty);
	}
}
