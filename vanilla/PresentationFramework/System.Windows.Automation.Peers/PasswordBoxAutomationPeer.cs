using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Documents;
using MS.Internal.Automation;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.PasswordBox" /> types to UI Automation.</summary>
public class PasswordBoxAutomationPeer : TextAutomationPeer, IValueProvider
{
	private TextAdaptor _textPattern;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the value is read-only; false if it can be modified.</returns>
	bool IValueProvider.IsReadOnly => false;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>A string value of the control.</returns>
	string IValueProvider.Value
	{
		get
		{
			if (AccessibilitySwitches.UseNetFx47CompatibleAccessibilityFeatures)
			{
				throw new InvalidOperationException();
			}
			return string.Empty;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.PasswordBoxAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.PasswordBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.PasswordBoxAutomationPeer" />.</param>
	public PasswordBoxAutomationPeer(PasswordBox owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.PasswordBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.PasswordBoxAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "PasswordBox".</returns>
	protected override string GetClassNameCore()
	{
		return "PasswordBox";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.PasswordBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.PasswordBoxAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Edit" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Edit;
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.PasswordBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.PasswordBoxAutomationPeer" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.PatternInterface.Value" /> enumeration value.</returns>
	/// <param name="patternInterface">A value in the enumeration.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		object obj = null;
		switch (patternInterface)
		{
		case PatternInterface.Value:
			obj = this;
			break;
		case PatternInterface.Text:
			if (_textPattern == null)
			{
				_textPattern = new TextAdaptor(this, ((PasswordBox)base.Owner).TextContainer);
			}
			obj = _textPattern;
			break;
		case PatternInterface.Scroll:
		{
			PasswordBox passwordBox = (PasswordBox)base.Owner;
			if (passwordBox.ScrollViewer != null)
			{
				obj = passwordBox.ScrollViewer.CreateAutomationPeer();
				((AutomationPeer)obj).EventsSource = this;
			}
			break;
		}
		default:
			obj = base.GetPattern(patternInterface);
			break;
		}
		return obj;
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.PasswordBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.PasswordBoxAutomationPeer" /> contains protected content. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsPassword" />.</summary>
	/// <returns>true.</returns>
	protected override bool IsPasswordCore()
	{
		return true;
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="value"> The value of a control.</param>
	void IValueProvider.SetValue(string value)
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		PasswordBox obj = (PasswordBox)base.Owner;
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		obj.Password = value;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void RaiseValuePropertyChangedEvent(string oldValue, string newValue)
	{
		if (oldValue != newValue)
		{
			RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, oldValue, newValue);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void RaiseIsReadOnlyPropertyChangedEvent(bool oldValue, bool newValue)
	{
		if (oldValue != newValue)
		{
			RaisePropertyChangedEvent(ValuePatternIdentifiers.IsReadOnlyProperty, oldValue, newValue);
		}
	}

	internal override List<AutomationPeer> GetAutomationPeersFromRange(ITextPointer start, ITextPointer end)
	{
		return new List<AutomationPeer>();
	}
}
