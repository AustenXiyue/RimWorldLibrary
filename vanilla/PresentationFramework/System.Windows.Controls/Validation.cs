using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal.Controls;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls;

/// <summary>Provides methods and attached properties that support data validation.</summary>
public static class Validation
{
	/// <summary>Identifies the <see cref="E:System.Windows.Controls.Validation.Error" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.Validation.Error" /> attached event.</returns>
	public static readonly RoutedEvent ErrorEvent = EventManager.RegisterRoutedEvent("ValidationError", RoutingStrategy.Bubble, typeof(EventHandler<ValidationErrorEventArgs>), typeof(Validation));

	internal static readonly DependencyPropertyKey ErrorsPropertyKey = DependencyProperty.RegisterAttachedReadOnly("Errors", typeof(ReadOnlyObservableCollection<ValidationError>), typeof(Validation), new FrameworkPropertyMetadata(ValidationErrorCollection.Empty, FrameworkPropertyMetadataOptions.NotDataBindable));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Validation.Errors" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Validation.Errors" /> attached property.</returns>
	public static readonly DependencyProperty ErrorsProperty = ErrorsPropertyKey.DependencyProperty;

	internal static readonly DependencyProperty ValidationErrorsInternalProperty = DependencyProperty.RegisterAttached("ErrorsInternal", typeof(ValidationErrorCollection), typeof(Validation), new FrameworkPropertyMetadata(null, OnErrorsInternalChanged));

	internal static readonly DependencyPropertyKey HasErrorPropertyKey = DependencyProperty.RegisterAttachedReadOnly("HasError", typeof(bool), typeof(Validation), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.NotDataBindable, OnHasErrorChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Validation.HasError" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Validation.HasError" /> attached property.</returns>
	public static readonly DependencyProperty HasErrorProperty = HasErrorPropertyKey.DependencyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Validation.ErrorTemplate" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Validation.ErrorTemplate" /> attached property.</returns>
	public static readonly DependencyProperty ErrorTemplateProperty = DependencyProperty.RegisterAttached("ErrorTemplate", typeof(ControlTemplate), typeof(Validation), new FrameworkPropertyMetadata(CreateDefaultErrorTemplate(), FrameworkPropertyMetadataOptions.NotDataBindable, OnErrorTemplateChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Validation.ValidationAdornerSite" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Validation.ValidationAdornerSite" /> attached property.</returns>
	public static readonly DependencyProperty ValidationAdornerSiteProperty = DependencyProperty.RegisterAttached("ValidationAdornerSite", typeof(DependencyObject), typeof(Validation), new FrameworkPropertyMetadata(null, OnValidationAdornerSiteChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Validation.ValidationAdornerSiteFor" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Validation.ValidationAdornerSiteFor" /> attached property.</returns>
	public static readonly DependencyProperty ValidationAdornerSiteForProperty = DependencyProperty.RegisterAttached("ValidationAdornerSiteFor", typeof(DependencyObject), typeof(Validation), new FrameworkPropertyMetadata(null, OnValidationAdornerSiteForChanged));

	private static readonly DependencyProperty ValidationAdornerProperty = DependencyProperty.RegisterAttached("ValidationAdorner", typeof(TemplatedAdorner), typeof(Validation), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.NotDataBindable));

	/// <summary>Adds an event handler for the <see cref="E:System.Windows.Controls.Validation.Error" /> attached event to the specified object.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> object to add <paramref name="handler" /> to.</param>
	/// <param name="handler">The handler to add.</param>
	public static void AddErrorHandler(DependencyObject element, EventHandler<ValidationErrorEventArgs> handler)
	{
		UIElement.AddHandler(element, ErrorEvent, handler);
	}

	/// <summary>Adds an event handler for the <see cref="E:System.Windows.Controls.Validation.Error" /> attached event from the specified object.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> object to remove <paramref name="handler" /> from.</param>
	/// <param name="handler">The handler to remove.</param>
	public static void RemoveErrorHandler(DependencyObject element, EventHandler<ValidationErrorEventArgs> handler)
	{
		UIElement.RemoveHandler(element, ErrorEvent, handler);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.Validation.Errors" /> attached property of the specified element.</summary>
	/// <returns>A <see cref="T:System.Collections.ObjectModel.ReadOnlyObservableCollection`1" /> of <see cref="T:System.Windows.Controls.ValidationError" /> objects.</returns>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> object to read the value from.</param>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="element" /> is null.</exception>
	public static ReadOnlyObservableCollection<ValidationError> GetErrors(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (ReadOnlyObservableCollection<ValidationError>)element.GetValue(ErrorsProperty);
	}

	private static void OnErrorsInternalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.NewValue is ValidationErrorCollection list)
		{
			d.SetValue(ErrorsPropertyKey, new ReadOnlyObservableCollection<ValidationError>(list));
		}
		else
		{
			d.ClearValue(ErrorsPropertyKey);
		}
	}

	internal static ValidationErrorCollection GetErrorsInternal(DependencyObject target)
	{
		return (ValidationErrorCollection)target.GetValue(ValidationErrorsInternalProperty);
	}

	private static void OnHasErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is Control d2)
		{
			Control.OnVisualStatePropertyChanged(d2, e);
		}
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.Validation.HasError" /> attached property of the specified element.</summary>
	/// <returns>The value of the <see cref="P:System.Windows.Controls.Validation.HasError" /> attached property of the specified element.</returns>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> object to read the value from.</param>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="element" /> is null.</exception>
	public static bool GetHasError(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(HasErrorProperty);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.Validation.ErrorTemplate" /> attached property of the specified element.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.ControlTemplate" /> used to generate validation error feedback on the adorner layer.</returns>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> object to read the value from.</param>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="element" /> is null.</exception>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static ControlTemplate GetErrorTemplate(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return element.GetValue(ErrorTemplateProperty) as ControlTemplate;
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.Validation.ErrorTemplate" /> attached property to the specified element.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> object to set <paramref name="value" /> on.</param>
	/// <param name="value">The <see cref="T:System.Windows.Controls.ControlTemplate" /> to use to generate validation error feedback on the adorner layer.</param>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="element" /> is null.</exception>
	public static void SetErrorTemplate(DependencyObject element, ControlTemplate value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (!object.Equals(element.ReadLocalValue(ErrorTemplateProperty), value))
		{
			element.SetValue(ErrorTemplateProperty, value);
		}
	}

	private static void OnErrorTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (GetHasError(d))
		{
			ShowValidationAdorner(d, show: false);
			ShowValidationAdorner(d, show: true);
		}
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.Validation.ValidationAdornerSite" /> attached property for the specified element.</summary>
	/// <returns>The value of the <see cref="P:System.Windows.Controls.Validation.ValidationAdornerSite" />.</returns>
	/// <param name="element">The element from which to get the <see cref="P:System.Windows.Controls.Validation.ValidationAdornerSite" />.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static DependencyObject GetValidationAdornerSite(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return element.GetValue(ValidationAdornerSiteProperty) as DependencyObject;
	}

	/// <summary>Sets the <see cref="P:System.Windows.Controls.Validation.ValidationAdornerSite" /> attached property to the specified value on the specified element.</summary>
	/// <param name="element">The element on which to set the <see cref="P:System.Windows.Controls.Validation.ValidationAdornerSite" />.</param>
	/// <param name="value">The <see cref="P:System.Windows.Controls.Validation.ValidationAdornerSite" /> of the specified element.</param>
	public static void SetValidationAdornerSite(DependencyObject element, DependencyObject value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(ValidationAdornerSiteProperty, value);
	}

	private static void OnValidationAdornerSiteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange)
		{
			return;
		}
		DependencyObject dependencyObject = (DependencyObject)e.OldValue;
		DependencyObject dependencyObject2 = (DependencyObject)e.NewValue;
		dependencyObject?.ClearValue(ValidationAdornerSiteForProperty);
		if (dependencyObject2 != null && d != GetValidationAdornerSiteFor(dependencyObject2))
		{
			SetValidationAdornerSiteFor(dependencyObject2, d);
		}
		if (GetHasError(d))
		{
			if (dependencyObject == null)
			{
				dependencyObject = d;
			}
			ShowValidationAdornerHelper(d, dependencyObject, show: false);
			ShowValidationAdorner(d, show: true);
		}
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.Validation.ValidationAdornerSiteFor" /> attached property for the specified element.</summary>
	/// <returns>The value of the <see cref="P:System.Windows.Controls.Validation.ValidationAdornerSiteFor" />.</returns>
	/// <param name="element">The element from which to get the <see cref="P:System.Windows.Controls.Validation.ValidationAdornerSiteFor" />.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static DependencyObject GetValidationAdornerSiteFor(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return element.GetValue(ValidationAdornerSiteForProperty) as DependencyObject;
	}

	/// <summary>Sets the <see cref="P:System.Windows.Controls.Validation.ValidationAdornerSiteFor" /> attached property to the specified value on the specified element.</summary>
	/// <param name="element">The element on which to set the <see cref="P:System.Windows.Controls.Validation.ValidationAdornerSiteFor" />.</param>
	/// <param name="value">The <see cref="P:System.Windows.Controls.Validation.ValidationAdornerSiteFor" /> of the specified element.</param>
	public static void SetValidationAdornerSiteFor(DependencyObject element, DependencyObject value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(ValidationAdornerSiteForProperty, value);
	}

	private static void OnValidationAdornerSiteForChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			DependencyObject dependencyObject = (DependencyObject)e.OldValue;
			DependencyObject dependencyObject2 = (DependencyObject)e.NewValue;
			dependencyObject?.ClearValue(ValidationAdornerSiteProperty);
			if (dependencyObject2 != null && d != GetValidationAdornerSite(dependencyObject2))
			{
				SetValidationAdornerSite(dependencyObject2, d);
			}
		}
	}

	internal static void ShowValidationAdorner(DependencyObject targetElement, bool show)
	{
		if (!HasValidationGroup(targetElement as FrameworkElement))
		{
			DependencyObject dependencyObject = GetValidationAdornerSite(targetElement);
			if (dependencyObject == null)
			{
				dependencyObject = targetElement;
			}
			ShowValidationAdornerHelper(targetElement, dependencyObject, show);
		}
	}

	private static bool HasValidationGroup(FrameworkElement fe)
	{
		if (fe != null)
		{
			if (HasValidationGroup(VisualStateManager.GetVisualStateGroupsInternal(fe)))
			{
				return true;
			}
			if (fe.StateGroupsRoot != null)
			{
				return HasValidationGroup(VisualStateManager.GetVisualStateGroupsInternal(fe.StateGroupsRoot));
			}
		}
		return false;
	}

	private static bool HasValidationGroup(IList<VisualStateGroup> groups)
	{
		if (groups != null)
		{
			for (int i = 0; i < groups.Count; i++)
			{
				if (groups[i].Name == "ValidationStates")
				{
					return true;
				}
			}
		}
		return false;
	}

	private static void ShowValidationAdornerHelper(DependencyObject targetElement, DependencyObject adornerSite, bool show)
	{
		ShowValidationAdornerHelper(targetElement, adornerSite, show, tryAgain: true);
	}

	private static object ShowValidationAdornerOperation(object arg)
	{
		object[] obj = (object[])arg;
		DependencyObject targetElement = (DependencyObject)obj[0];
		DependencyObject dependencyObject = (DependencyObject)obj[1];
		bool show = (bool)obj[2];
		if (dependencyObject is UIElement { IsVisible: false } uIElement)
		{
			uIElement.IsVisibleChanged += ShowValidationAdornerWhenAdornerSiteGetsVisible;
		}
		else
		{
			ShowValidationAdornerHelper(targetElement, dependencyObject, show, tryAgain: false);
		}
		return null;
	}

	private static void ShowValidationAdornerWhenAdornerSiteGetsVisible(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is UIElement uIElement)
		{
			uIElement.IsVisibleChanged -= ShowValidationAdornerWhenAdornerSiteGetsVisible;
			DependencyObject dependencyObject = GetValidationAdornerSiteFor(uIElement);
			if (dependencyObject == null)
			{
				dependencyObject = uIElement;
			}
			ShowValidationAdornerHelper(dependencyObject, uIElement, (bool)e.NewValue && GetHasError(dependencyObject), tryAgain: false);
		}
	}

	private static void ShowValidationAdornerHelper(DependencyObject targetElement, DependencyObject adornerSite, bool show, bool tryAgain)
	{
		if (!(adornerSite is UIElement uIElement))
		{
			return;
		}
		AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(uIElement);
		if (adornerLayer == null)
		{
			if (tryAgain)
			{
				adornerSite.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new DispatcherOperationCallback(ShowValidationAdornerOperation), new object[3]
				{
					targetElement,
					adornerSite,
					BooleanBoxes.Box(show)
				});
			}
			return;
		}
		TemplatedAdorner templatedAdorner = uIElement.ReadLocalValue(ValidationAdornerProperty) as TemplatedAdorner;
		if (show && templatedAdorner == null)
		{
			ControlTemplate errorTemplate = GetErrorTemplate(uIElement);
			if (errorTemplate == null)
			{
				errorTemplate = GetErrorTemplate(targetElement);
			}
			if (errorTemplate != null)
			{
				templatedAdorner = new TemplatedAdorner(uIElement, errorTemplate);
				adornerLayer.Add(templatedAdorner);
				uIElement.SetValue(ValidationAdornerProperty, templatedAdorner);
			}
		}
		else if (!show && templatedAdorner != null)
		{
			templatedAdorner.ClearChild();
			adornerLayer.Remove(templatedAdorner);
			uIElement.ClearValue(ValidationAdornerProperty);
		}
	}

	/// <summary>Marks the specified <see cref="T:System.Windows.Data.BindingExpression" /> object as invalid with the specified <see cref="T:System.Windows.Controls.ValidationError" /> object.</summary>
	/// <param name="bindingExpression">The <see cref="T:System.Windows.Data.BindingExpression" /> object to mark as invalid.</param>
	/// <param name="validationError">The <see cref="T:System.Windows.Controls.ValidationError" /> object to use.</param>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="bindingExpression" /> is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="validationError" /> is null.</exception>
	public static void MarkInvalid(BindingExpressionBase bindingExpression, ValidationError validationError)
	{
		if (bindingExpression == null)
		{
			throw new ArgumentNullException("bindingExpression");
		}
		if (validationError == null)
		{
			throw new ArgumentNullException("validationError");
		}
		bindingExpression.UpdateValidationError(validationError);
	}

	/// <summary>Removes all <see cref="T:System.Windows.Controls.ValidationError" /> objects from the specified <see cref="T:System.Windows.Data.BindingExpressionBase" /> object.</summary>
	/// <param name="bindingExpression">The object to turn valid.</param>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="bindingExpression" /> is null.</exception>
	public static void ClearInvalid(BindingExpressionBase bindingExpression)
	{
		if (bindingExpression == null)
		{
			throw new ArgumentNullException("bindingExpression");
		}
		bindingExpression.UpdateValidationError(null);
	}

	internal static void AddValidationError(ValidationError validationError, DependencyObject targetElement, bool shouldRaiseEvent)
	{
		if (targetElement != null)
		{
			ValidationErrorCollection errorsInternal = GetErrorsInternal(targetElement);
			bool flag;
			if (errorsInternal == null)
			{
				flag = true;
				errorsInternal = new ValidationErrorCollection();
				errorsInternal.Add(validationError);
				targetElement.SetValue(ValidationErrorsInternalProperty, errorsInternal);
			}
			else
			{
				flag = errorsInternal.Count == 0;
				errorsInternal.Add(validationError);
			}
			if (flag)
			{
				targetElement.SetValue(HasErrorPropertyKey, BooleanBoxes.TrueBox);
			}
			if (shouldRaiseEvent)
			{
				OnValidationError(targetElement, validationError, ValidationErrorEventAction.Added);
			}
			if (flag)
			{
				ShowValidationAdorner(targetElement, show: true);
			}
		}
	}

	internal static void RemoveValidationError(ValidationError validationError, DependencyObject targetElement, bool shouldRaiseEvent)
	{
		if (targetElement == null)
		{
			return;
		}
		ValidationErrorCollection errorsInternal = GetErrorsInternal(targetElement);
		if (errorsInternal == null || errorsInternal.Count == 0 || !errorsInternal.Contains(validationError))
		{
			return;
		}
		if (errorsInternal.Count == 1)
		{
			targetElement.ClearValue(HasErrorPropertyKey);
			targetElement.ClearValue(ValidationErrorsInternalProperty);
			if (shouldRaiseEvent)
			{
				OnValidationError(targetElement, validationError, ValidationErrorEventAction.Removed);
			}
			ShowValidationAdorner(targetElement, show: false);
		}
		else
		{
			errorsInternal.Remove(validationError);
			if (shouldRaiseEvent)
			{
				OnValidationError(targetElement, validationError, ValidationErrorEventAction.Removed);
			}
		}
	}

	private static void OnValidationError(DependencyObject source, ValidationError validationError, ValidationErrorEventAction action)
	{
		ValidationErrorEventArgs e = new ValidationErrorEventArgs(validationError, action);
		if (source is ContentElement)
		{
			((ContentElement)source).RaiseEvent(e);
		}
		else if (source is UIElement)
		{
			((UIElement)source).RaiseEvent(e);
		}
		else if (source is UIElement3D)
		{
			((UIElement3D)source).RaiseEvent(e);
		}
	}

	private static ControlTemplate CreateDefaultErrorTemplate()
	{
		ControlTemplate controlTemplate = new ControlTemplate(typeof(Control));
		FrameworkElementFactory frameworkElementFactory = new FrameworkElementFactory(typeof(Border), "Border");
		frameworkElementFactory.SetValue(Border.BorderBrushProperty, Brushes.Red);
		frameworkElementFactory.SetValue(Border.BorderThicknessProperty, new Thickness(1.0));
		FrameworkElementFactory child = new FrameworkElementFactory(typeof(AdornedElementPlaceholder), "Placeholder");
		frameworkElementFactory.AppendChild(child);
		controlTemplate.VisualTree = frameworkElementFactory;
		controlTemplate.Seal();
		return controlTemplate;
	}
}
