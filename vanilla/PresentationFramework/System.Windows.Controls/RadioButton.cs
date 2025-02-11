using System.Collections;
using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents a button that can be selected, but not cleared, by a user. The <see cref="P:System.Windows.Controls.Primitives.ToggleButton.IsChecked" /> property of a <see cref="T:System.Windows.Controls.RadioButton" /> can be set by clicking it, but it can only be cleared programmatically. </summary>
[Localizability(LocalizationCategory.RadioButton)]
public class RadioButton : ToggleButton
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.RadioButton.GroupName" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.RadioButton.GroupName" /> dependency property.</returns>
	public static readonly DependencyProperty GroupNameProperty;

	private static DependencyObjectType _dType;

	[ThreadStatic]
	private static Hashtable _groupNameToElements;

	private static readonly UncommonField<string> _currentlyRegisteredGroupName;

	/// <summary>Gets or sets the name that specifies which <see cref="T:System.Windows.Controls.RadioButton" /> controls are mutually exclusive.  </summary>
	/// <returns>The name that specifies which <see cref="T:System.Windows.Controls.RadioButton" /> controls are mutually exclusive.  The default is an empty string.</returns>
	[DefaultValue("")]
	[Localizability(LocalizationCategory.NeverLocalize)]
	public string GroupName
	{
		get
		{
			return (string)GetValue(GroupNameProperty);
		}
		set
		{
			SetValue(GroupNameProperty, value);
		}
	}

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	static RadioButton()
	{
		GroupNameProperty = DependencyProperty.Register("GroupName", typeof(string), typeof(RadioButton), new FrameworkPropertyMetadata(string.Empty, OnGroupNameChanged));
		_currentlyRegisteredGroupName = new UncommonField<string>();
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(RadioButton), new FrameworkPropertyMetadata(typeof(RadioButton)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(RadioButton));
		KeyboardNavigation.AcceptsReturnProperty.OverrideMetadata(typeof(RadioButton), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		ControlsTraceLogger.AddControl(TelemetryControls.RadioButton);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.RadioButton" /> class. </summary>
	public RadioButton()
	{
	}

	private static void OnGroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		RadioButton radioButton = (RadioButton)d;
		string text = e.NewValue as string;
		string value = _currentlyRegisteredGroupName.GetValue(radioButton);
		if (text != value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				Unregister(value, radioButton);
			}
			if (!string.IsNullOrEmpty(text))
			{
				Register(text, radioButton);
			}
		}
	}

	private static void Register(string groupName, RadioButton radioButton)
	{
		if (_groupNameToElements == null)
		{
			_groupNameToElements = new Hashtable(1);
		}
		lock (_groupNameToElements)
		{
			ArrayList arrayList = (ArrayList)_groupNameToElements[groupName];
			if (arrayList == null)
			{
				arrayList = new ArrayList(1);
				_groupNameToElements[groupName] = arrayList;
			}
			else
			{
				PurgeDead(arrayList, null);
			}
			arrayList.Add(new WeakReference(radioButton));
		}
		_currentlyRegisteredGroupName.SetValue(radioButton, groupName);
	}

	private static void Unregister(string groupName, RadioButton radioButton)
	{
		if (_groupNameToElements == null)
		{
			return;
		}
		lock (_groupNameToElements)
		{
			ArrayList arrayList = (ArrayList)_groupNameToElements[groupName];
			if (arrayList != null)
			{
				PurgeDead(arrayList, radioButton);
				if (arrayList.Count == 0)
				{
					_groupNameToElements.Remove(groupName);
				}
			}
		}
		_currentlyRegisteredGroupName.SetValue(radioButton, null);
	}

	private static void PurgeDead(ArrayList elements, object elementToRemove)
	{
		int num = 0;
		while (num < elements.Count)
		{
			object target = ((WeakReference)elements[num]).Target;
			if (target == null || target == elementToRemove)
			{
				elements.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	private void UpdateRadioButtonGroup()
	{
		string groupName = GroupName;
		if (!string.IsNullOrEmpty(groupName))
		{
			Visual visualRoot = KeyboardNavigation.GetVisualRoot(this);
			if (_groupNameToElements == null)
			{
				_groupNameToElements = new Hashtable(1);
			}
			lock (_groupNameToElements)
			{
				ArrayList arrayList = (ArrayList)_groupNameToElements[groupName];
				int num = 0;
				while (num < arrayList.Count)
				{
					if (!(((WeakReference)arrayList[num]).Target is RadioButton radioButton))
					{
						arrayList.RemoveAt(num);
						continue;
					}
					if (radioButton != this && radioButton.IsChecked == true && visualRoot == KeyboardNavigation.GetVisualRoot(radioButton))
					{
						radioButton.UncheckRadioButton();
					}
					num++;
				}
				return;
			}
		}
		DependencyObject parent = base.Parent;
		if (parent == null)
		{
			return;
		}
		IEnumerator enumerator = LogicalTreeHelper.GetChildren(parent).GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current is RadioButton radioButton2 && radioButton2 != this && string.IsNullOrEmpty(radioButton2.GroupName) && radioButton2.IsChecked == true)
			{
				radioButton2.UncheckRadioButton();
			}
		}
	}

	private void UncheckRadioButton()
	{
		SetCurrentValueInternal(ToggleButton.IsCheckedProperty, BooleanBoxes.FalseBox);
	}

	/// <summary>Provides an appropriate <see cref="T:System.Windows.Automation.Peers.RadioButtonAutomationPeer" /> implementation for this control, as part of the WPF automation infrastructure.</summary>
	/// <returns>The type-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new RadioButtonAutomationPeer(this);
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.Primitives.ToggleButton.IsChecked" /> property becomes true. </summary>
	/// <param name="e">Provides data for <see cref="T:System.Windows.RoutedEventArgs" />.</param>
	protected override void OnChecked(RoutedEventArgs e)
	{
		UpdateRadioButtonGroup();
		base.OnChecked(e);
	}

	/// <summary>Called by the <see cref="M:System.Windows.Controls.Primitives.ToggleButton.OnClick" /> method to implement a <see cref="T:System.Windows.Controls.RadioButton" /> control's toggle behavior. </summary>
	protected internal override void OnToggle()
	{
		SetCurrentValueInternal(ToggleButton.IsCheckedProperty, BooleanBoxes.TrueBox);
	}

	/// <summary> Called when the <see cref="P:System.Windows.Controls.AccessText.AccessKey" /> for a radio button is invoked. </summary>
	/// <param name="e">Provides data for <see cref="T:System.Windows.Input.AccessKeyEventArgs" />.</param>
	protected override void OnAccessKey(AccessKeyEventArgs e)
	{
		if (!base.IsKeyboardFocused)
		{
			Focus();
		}
		base.OnAccessKey(e);
	}
}
