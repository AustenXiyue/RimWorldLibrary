using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using MS.Internal.Automation;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.TextBox" /> types to UI Automation.</summary>
public class TextBoxAutomationPeer : TextAutomationPeer, IValueProvider
{
	private TextAdaptor _textPattern;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the value is read-only; false if it can be modified.</returns>
	bool IValueProvider.IsReadOnly => ((TextBox)base.Owner).IsReadOnly;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>A string value of the control.</returns>
	string IValueProvider.Value => ((TextBox)base.Owner).Text;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.TextBoxAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.TextBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TextBoxAutomationPeer" />.</param>
	public TextBoxAutomationPeer(TextBox owner)
		: base(owner)
	{
		_textPattern = new TextAdaptor(this, owner.TextContainer);
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.TextBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TextBoxAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "TextBox".</returns>
	protected override string GetClassNameCore()
	{
		return "TextBox";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.TextBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TextBoxAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Edit" />
	/// </returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Edit;
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.TextBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TextBoxAutomationPeer" />.</summary>
	/// <returns>An object that supports the control pattern if <paramref name="patternInterface" /> is a supported value; otherwise, null.</returns>
	/// <param name="patternInterface">A value from the enumeration.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		object obj = null;
		if (patternInterface == PatternInterface.Value)
		{
			obj = this;
		}
		switch (patternInterface)
		{
		case PatternInterface.Text:
			if (_textPattern == null)
			{
				_textPattern = new TextAdaptor(this, ((TextBoxBase)base.Owner).TextContainer);
			}
			return _textPattern;
		case PatternInterface.Scroll:
		{
			TextBox textBox = (TextBox)base.Owner;
			if (textBox.ScrollViewer != null)
			{
				obj = textBox.ScrollViewer.CreateAutomationPeer();
				((AutomationPeer)obj).EventsSource = this;
			}
			break;
		}
		}
		if (patternInterface == PatternInterface.SynchronizedInput)
		{
			obj = base.GetPattern(patternInterface);
		}
		return obj;
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="value"> The value of a control.</param>
	void IValueProvider.SetValue(string value)
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		TextBox obj = (TextBox)base.Owner;
		if (obj.IsReadOnly)
		{
			throw new ElementNotEnabledException();
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		obj.Text = value;
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
