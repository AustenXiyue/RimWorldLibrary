using System.Runtime.CompilerServices;
using System.Windows.Automation.Provider;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Primitives.RangeBase" /> types to UI Automation.</summary>
public class RangeBaseAutomationPeer : FrameworkElementAutomationPeer, IRangeValueProvider
{
	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The value of the control.</returns>
	double IRangeValueProvider.Value => ((RangeBase)base.Owner).Value;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the range value is read-only; otherwise false. </returns>
	bool IRangeValueProvider.IsReadOnly => !IsEnabled();

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The maximum range value supported by the control.</returns>
	double IRangeValueProvider.Maximum => ((RangeBase)base.Owner).Maximum;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The minimum range value supported by the control.</returns>
	double IRangeValueProvider.Minimum => ((RangeBase)base.Owner).Minimum;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The large-change value.</returns>
	double IRangeValueProvider.LargeChange => ((RangeBase)base.Owner).LargeChange;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The small-change value.</returns>
	double IRangeValueProvider.SmallChange => ((RangeBase)base.Owner).SmallChange;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.RangeBaseAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.Primitives.RangeBase" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.RangeBaseAutomationPeer" />.</param>
	public RangeBaseAutomationPeer(RangeBase owner)
		: base(owner)
	{
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.Primitives.RangeBase" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.RangeBaseAutomationPeer" />.</summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.RangeValue" />, this method returns a this pointer; otherwise, this method returns null.</returns>
	/// <param name="patternInterface">A value in the enumeration.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.RangeValue)
		{
			return this;
		}
		return base.GetPattern(patternInterface);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void RaiseMinimumPropertyChangedEvent(double oldValue, double newValue)
	{
		RaisePropertyChangedEvent(RangeValuePatternIdentifiers.MinimumProperty, oldValue, newValue);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void RaiseMaximumPropertyChangedEvent(double oldValue, double newValue)
	{
		RaisePropertyChangedEvent(RangeValuePatternIdentifiers.MaximumProperty, oldValue, newValue);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void RaiseValuePropertyChangedEvent(double oldValue, double newValue)
	{
		RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty, oldValue, newValue);
	}

	internal virtual void SetValueCore(double val)
	{
		RangeBase rangeBase = (RangeBase)base.Owner;
		if (val < rangeBase.Minimum || val > rangeBase.Maximum)
		{
			throw new ArgumentOutOfRangeException("val");
		}
		rangeBase.Value = val;
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="val"> The value to set.</param>
	void IRangeValueProvider.SetValue(double val)
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		SetValueCore(val);
	}
}
