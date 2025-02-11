using MS.Internal.KnownBoxes;

namespace System.Windows.Automation;

/// <summary>Provides a means of getting or setting the value of the associated properties of the instance of the <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> element. </summary>
public static class AutomationProperties
{
	/// <summary>Identifies the <see cref="P:System.Windows.Automation.AutomationProperties.AutomationId" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Automation.AutomationProperties.AutomationId" /> dependency property.</returns>
	public static readonly DependencyProperty AutomationIdProperty = DependencyProperty.RegisterAttached("AutomationId", typeof(string), typeof(AutomationProperties), new UIPropertyMetadata(string.Empty), IsNotNull);

	/// <summary>Identifies the <see cref="P:System.Windows.Automation.AutomationProperties.Name" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Automation.AutomationProperties.Name" /> attached property. </returns>
	public static readonly DependencyProperty NameProperty = DependencyProperty.RegisterAttached("Name", typeof(string), typeof(AutomationProperties), new UIPropertyMetadata(string.Empty), IsNotNull);

	/// <summary>Identifies the <see cref="P:System.Windows.Automation.AutomationProperties.HelpText" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Automation.AutomationProperties.HelpText" /> dependency property.</returns>
	public static readonly DependencyProperty HelpTextProperty = DependencyProperty.RegisterAttached("HelpText", typeof(string), typeof(AutomationProperties), new UIPropertyMetadata(string.Empty), IsNotNull);

	/// <summary>Identifies the <see cref="P:System.Windows.Automation.AutomationProperties.AcceleratorKey" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Automation.AutomationProperties.AcceleratorKey" /> dependency property.</returns>
	public static readonly DependencyProperty AcceleratorKeyProperty = DependencyProperty.RegisterAttached("AcceleratorKey", typeof(string), typeof(AutomationProperties), new UIPropertyMetadata(string.Empty), IsNotNull);

	/// <summary>Identifies the <see cref="P:System.Windows.Automation.AutomationProperties.AccessKey" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Automation.AutomationProperties.AccessKey" /> dependency property.</returns>
	public static readonly DependencyProperty AccessKeyProperty = DependencyProperty.RegisterAttached("AccessKey", typeof(string), typeof(AutomationProperties), new UIPropertyMetadata(string.Empty), IsNotNull);

	/// <summary>Identifies the <see cref="P:System.Windows.Automation.AutomationProperties.ItemStatus" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Automation.AutomationProperties.ItemStatus" /> dependency property.</returns>
	public static readonly DependencyProperty ItemStatusProperty = DependencyProperty.RegisterAttached("ItemStatus", typeof(string), typeof(AutomationProperties), new UIPropertyMetadata(string.Empty), IsNotNull);

	/// <summary>Identifies the <see cref="P:System.Windows.Automation.AutomationProperties.ItemType" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Automation.AutomationProperties.ItemType" /> dependency property.</returns>
	public static readonly DependencyProperty ItemTypeProperty = DependencyProperty.RegisterAttached("ItemType", typeof(string), typeof(AutomationProperties), new UIPropertyMetadata(string.Empty), IsNotNull);

	/// <summary>Identifies the <see cref="P:System.Windows.Automation.AutomationProperties.IsColumnHeader" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Automation.AutomationProperties.IsColumnHeader" /> dependency property.</returns>
	public static readonly DependencyProperty IsColumnHeaderProperty = DependencyProperty.RegisterAttached("IsColumnHeader", typeof(bool), typeof(AutomationProperties), new UIPropertyMetadata(BooleanBoxes.FalseBox));

	/// <summary>Identifies the <see cref="P:System.Windows.Automation.AutomationProperties.IsRowHeader" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Automation.AutomationProperties.IsRowHeader" /> dependency property.</returns>
	public static readonly DependencyProperty IsRowHeaderProperty = DependencyProperty.RegisterAttached("IsRowHeader", typeof(bool), typeof(AutomationProperties), new UIPropertyMetadata(BooleanBoxes.FalseBox));

	/// <summary>Identifies the <see cref="P:System.Windows.Automation.AutomationProperties.IsRequiredForForm" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Automation.AutomationProperties.IsRequiredForForm" /> dependency property.</returns>
	public static readonly DependencyProperty IsRequiredForFormProperty = DependencyProperty.RegisterAttached("IsRequiredForForm", typeof(bool), typeof(AutomationProperties), new UIPropertyMetadata(BooleanBoxes.FalseBox));

	/// <summary>Identifies the <see cref="P:System.Windows.Automation.AutomationProperties.LabeledBy" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Automation.AutomationProperties.LabeledBy" /> dependency property.</returns>
	public static readonly DependencyProperty LabeledByProperty = DependencyProperty.RegisterAttached("LabeledBy", typeof(UIElement), typeof(AutomationProperties), new UIPropertyMetadata((object)null));

	/// <summary>Identifies the <see cref="P:System.Windows.Automation.AutomationProperties.IsOffscreenBehavior" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Automation.AutomationProperties.IsOffscreenBehavior" /> dependency property.</returns>
	public static readonly DependencyProperty IsOffscreenBehaviorProperty = DependencyProperty.RegisterAttached("IsOffscreenBehavior", typeof(IsOffscreenBehavior), typeof(AutomationProperties), new UIPropertyMetadata(IsOffscreenBehavior.Default));

	public static readonly DependencyProperty LiveSettingProperty = DependencyProperty.RegisterAttached("LiveSetting", typeof(AutomationLiveSetting), typeof(AutomationProperties), new UIPropertyMetadata(AutomationLiveSetting.Off));

	public static readonly DependencyProperty PositionInSetProperty = DependencyProperty.RegisterAttached("PositionInSet", typeof(int), typeof(AutomationProperties), new UIPropertyMetadata(-1));

	public static readonly DependencyProperty SizeOfSetProperty = DependencyProperty.RegisterAttached("SizeOfSet", typeof(int), typeof(AutomationProperties), new UIPropertyMetadata(-1));

	public static readonly DependencyProperty HeadingLevelProperty = DependencyProperty.RegisterAttached("HeadingLevel", typeof(AutomationHeadingLevel), typeof(AutomationProperties), new UIPropertyMetadata(AutomationHeadingLevel.None));

	public static readonly DependencyProperty IsDialogProperty = DependencyProperty.RegisterAttached("IsDialog", typeof(bool), typeof(AutomationProperties), new UIPropertyMetadata(false));

	internal const int AutomationPositionInSetDefault = -1;

	internal const int AutomationSizeOfSetDefault = -1;

	/// <summary>Sets the <see cref="P:System.Windows.Automation.AutomationProperties.AutomationId" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> on which to set the property.</param>
	/// <param name="value">The UI Automation identifier value to set.</param>
	public static void SetAutomationId(DependencyObject element, string value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(AutomationIdProperty, value);
	}

	/// <summary>Gets the <see cref="P:System.Windows.Automation.AutomationProperties.AutomationId" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <returns>The UI Automation identifier for the specified element.</returns>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> to check.</param>
	public static string GetAutomationId(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (string)element.GetValue(AutomationIdProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Automation.AutomationProperties.Name" /> attached property.</summary>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> on which to set the property.</param>
	/// <param name="value">The name value to set.</param>
	public static void SetName(DependencyObject element, string value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(NameProperty, value);
	}

	/// <summary>Gets the <see cref="P:System.Windows.Automation.AutomationProperties.Name" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <returns>The name of the specified element.</returns>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> to check.</param>
	public static string GetName(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (string)element.GetValue(NameProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Automation.AutomationProperties.HelpText" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> on which to set the property.</param>
	/// <param name="value">The help text value to set.</param>
	public static void SetHelpText(DependencyObject element, string value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(HelpTextProperty, value);
	}

	/// <summary>Gets the <see cref="P:System.Windows.Automation.AutomationProperties.HelpText" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <returns>A string containing the help text for the specified element. The string that is returned generally is the same text that is provided in the tooltip for the control.</returns>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> to check.</param>
	public static string GetHelpText(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (string)element.GetValue(HelpTextProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Automation.AutomationProperties.AcceleratorKey" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> on which to set the property.</param>
	/// <param name="value">The accelerator key value to set.</param>
	public static void SetAcceleratorKey(DependencyObject element, string value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(AcceleratorKeyProperty, value);
	}

	/// <summary>Gets the <see cref="P:System.Windows.Automation.AutomationProperties.AcceleratorKey" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <returns>A string that contains the accelerator key.</returns>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> to check.</param>
	public static string GetAcceleratorKey(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (string)element.GetValue(AcceleratorKeyProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Automation.AutomationProperties.AccessKey" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> on which to set the property.</param>
	/// <param name="value">The access key value to set.</param>
	public static void SetAccessKey(DependencyObject element, string value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(AccessKeyProperty, value);
	}

	/// <summary>Gets the <see cref="P:System.Windows.Automation.AutomationProperties.AccessKey" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <returns>The access key for the specified element.</returns>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> to check.</param>
	public static string GetAccessKey(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (string)element.GetValue(AccessKeyProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Automation.AutomationProperties.ItemStatus" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> on which to set the property.</param>
	/// <param name="value">The item status value to set.</param>
	public static void SetItemStatus(DependencyObject element, string value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(ItemStatusProperty, value);
	}

	/// <summary>Gets the <see cref="P:System.Windows.Automation.AutomationProperties.ItemStatus" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <returns>The <see cref="P:System.Windows.Automation.AutomationElement.AutomationElementInformation.ItemStatus" /> of the given element.</returns>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> to check.</param>
	public static string GetItemStatus(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (string)element.GetValue(ItemStatusProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Automation.AutomationProperties.ItemType" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> on which to set the property.</param>
	/// <param name="value">The item type value to set.</param>
	public static void SetItemType(DependencyObject element, string value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(ItemTypeProperty, value);
	}

	/// <summary>Gets the <see cref="P:System.Windows.Automation.AutomationProperties.ItemType" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <returns>The <see cref="P:System.Windows.Automation.AutomationElement.AutomationElementInformation.ItemType" /> of the given element.</returns>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> to check.</param>
	public static string GetItemType(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (string)element.GetValue(ItemTypeProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Automation.AutomationProperties.IsColumnHeader" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> on which to set the property.</param>
	/// <param name="value">The value to set; true if the element is meant to be a column header, otherwise false</param>
	public static void SetIsColumnHeader(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsColumnHeaderProperty, value);
	}

	/// <summary>Gets the <see cref="P:System.Windows.Automation.AutomationProperties.IsColumnHeader" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <returns>A boolean that indicates whether the specified element is a <see cref="F:System.Windows.Automation.TablePattern.ColumnHeadersProperty" />.</returns>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> to check.</param>
	public static bool GetIsColumnHeader(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsColumnHeaderProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Automation.AutomationProperties.IsRowHeader" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> on which to set the property.</param>
	/// <param name="value">The value to set; true if the element is meant to be a row header, otherwise false.</param>
	public static void SetIsRowHeader(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsRowHeaderProperty, value);
	}

	/// <summary>Gets the <see cref="P:System.Windows.Automation.AutomationProperties.IsRowHeader" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <returns>A boolean that indicates whether the specified element is a <see cref="F:System.Windows.Automation.TablePattern.RowHeadersProperty" />.</returns>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> to check.</param>
	public static bool GetIsRowHeader(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsRowHeaderProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Automation.AutomationProperties.IsRequiredForForm" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> on which to set the property.</param>
	/// <param name="value">The value to set; true if the element is meant to be required to be filled out on a form, otherwise false.</param>
	public static void SetIsRequiredForForm(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsRequiredForFormProperty, value);
	}

	/// <summary>Gets the <see cref="P:System.Windows.Automation.AutomationProperties.IsRequiredForForm" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <returns>A boolean that indicates whether the specified element is <see cref="P:System.Windows.Automation.AutomationElement.AutomationElementInformation.IsRequiredForForm" />.</returns>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> to check.</param>
	public static bool GetIsRequiredForForm(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsRequiredForFormProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Automation.AutomationProperties.LabeledBy" /> attached property.</summary>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> on which to set the property.</param>
	/// <param name="value">The labeled by value to set.</param>
	public static void SetLabeledBy(DependencyObject element, UIElement value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(LabeledByProperty, value);
	}

	/// <summary>Gets the <see cref="P:System.Windows.Automation.AutomationProperties.LabeledBy" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <returns>The element that is targeted by the label. </returns>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> to check.</param>
	public static UIElement GetLabeledBy(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (UIElement)element.GetValue(LabeledByProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Automation.AutomationProperties.IsOffScreenBehavior" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> on which to set the property.</param>
	/// <param name="value">A value that specifies how an element determines how the <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsOffscreen" /> property is determined.</param>
	public static void SetIsOffscreenBehavior(DependencyObject element, IsOffscreenBehavior value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsOffscreenBehaviorProperty, value);
	}

	/// <summary>Gets the <see cref="P:System.Windows.Automation.AutomationProperties.IsOffScreenBehavior" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <returns>A value that specifies how an element determines how the <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsOffscreen" /> property is determined.</returns>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> to check.</param>
	public static IsOffscreenBehavior GetIsOffscreenBehavior(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (IsOffscreenBehavior)element.GetValue(IsOffscreenBehaviorProperty);
	}

	public static void SetLiveSetting(DependencyObject element, AutomationLiveSetting value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(LiveSettingProperty, value);
	}

	public static AutomationLiveSetting GetLiveSetting(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (AutomationLiveSetting)element.GetValue(LiveSettingProperty);
	}

	public static void SetPositionInSet(DependencyObject element, int value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(PositionInSetProperty, value);
	}

	public static int GetPositionInSet(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (int)element.GetValue(PositionInSetProperty);
	}

	public static void SetSizeOfSet(DependencyObject element, int value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(SizeOfSetProperty, value);
	}

	public static int GetSizeOfSet(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (int)element.GetValue(SizeOfSetProperty);
	}

	public static void SetHeadingLevel(DependencyObject element, AutomationHeadingLevel value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(HeadingLevelProperty, value);
	}

	public static AutomationHeadingLevel GetHeadingLevel(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (AutomationHeadingLevel)element.GetValue(HeadingLevelProperty);
	}

	public static void SetIsDialog(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsDialogProperty, value);
	}

	public static bool GetIsDialog(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsDialogProperty);
	}

	private static bool IsNotNull(object value)
	{
		return value != null;
	}
}
