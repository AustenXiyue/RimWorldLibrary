namespace System.Windows.Input.StylusPlugIns;

/// <summary>Represents a plug-in that can be added to a control's <see cref="P:System.Windows.UIElement.StylusPlugIns" /> property.</summary>
public abstract class StylusPlugIn
{
	private volatile bool __enabled = true;

	private bool _activeForInput;

	private StylusPlugInCollection _pic;

	/// <summary>Gets the <see cref="T:System.Windows.UIElement" /> to which the <see cref="T:System.Windows.Input.StylusPlugIns.StylusPlugIn" /> is attached. </summary>
	/// <returns>The <see cref="T:System.Windows.UIElement" /> to which the <see cref="T:System.Windows.Input.StylusPlugIns.StylusPlugIn" /> is attached.</returns>
	public UIElement Element
	{
		get
		{
			if (_pic == null)
			{
				return null;
			}
			return _pic.Element;
		}
	}

	/// <summary>Gets the cached bounds of the element.</summary>
	/// <returns>The cached bounds of the element.</returns>
	public Rect ElementBounds
	{
		get
		{
			if (_pic == null)
			{
				return default(Rect);
			}
			return _pic.Rect;
		}
	}

	/// <summary>Gets or sets whether the <see cref="T:System.Windows.Input.StylusPlugIns.StylusPlugIn" /> is active.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Input.StylusPlugIns.StylusPlugIn" /> is active; otherwise, false. The default is true.</returns>
	public bool Enabled
	{
		get
		{
			return __enabled;
		}
		set
		{
			if (_pic != null)
			{
				_pic.Element.VerifyAccess();
			}
			if (value == __enabled)
			{
				return;
			}
			if (_pic != null && _pic.IsActiveForInput)
			{
				using (_pic.Element.Dispatcher.DisableProcessing())
				{
					_pic.ExecuteWithPotentialLock(delegate
					{
						__enabled = value;
						if (!value)
						{
							InvalidateIsActiveForInput();
							OnEnabledChanged();
						}
						else
						{
							OnEnabledChanged();
							InvalidateIsActiveForInput();
						}
					});
					return;
				}
			}
			__enabled = value;
			if (!value)
			{
				InvalidateIsActiveForInput();
				OnEnabledChanged();
			}
			else
			{
				OnEnabledChanged();
				InvalidateIsActiveForInput();
			}
		}
	}

	/// <summary>Gets whether the <see cref="T:System.Windows.Input.StylusPlugIns.StylusPlugIn" /> is able to accept input.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Input.StylusPlugIns.StylusPlugIn" /> is able to accept input; otherwise false.</returns>
	public bool IsActiveForInput => _activeForInput;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusPlugIns.StylusPlugIn" /> class. </summary>
	protected StylusPlugIn()
	{
	}

	internal void Added(StylusPlugInCollection plugInCollection)
	{
		_pic = plugInCollection;
		OnAdded();
		InvalidateIsActiveForInput();
	}

	/// <summary>Occurs when the <see cref="T:System.Windows.Input.StylusPlugIns.StylusPlugIn" /> is added to an element.</summary>
	protected virtual void OnAdded()
	{
	}

	internal void Removed()
	{
		if (_activeForInput)
		{
			InvalidateIsActiveForInput();
		}
		OnRemoved();
		_pic = null;
	}

	/// <summary>Occurs when the <see cref="T:System.Windows.Input.StylusPlugIns.StylusPlugIn" /> is removed from an element.</summary>
	protected virtual void OnRemoved()
	{
	}

	internal void StylusEnterLeave(bool isEnter, RawStylusInput rawStylusInput, bool confirmed)
	{
		if (__enabled && _pic != null)
		{
			if (isEnter)
			{
				OnStylusEnter(rawStylusInput, confirmed);
			}
			else
			{
				OnStylusLeave(rawStylusInput, confirmed);
			}
		}
	}

	internal void RawStylusInput(RawStylusInput rawStylusInput)
	{
		if (__enabled && _pic != null)
		{
			switch (rawStylusInput.Report.Actions)
			{
			case RawStylusActions.Down:
				OnStylusDown(rawStylusInput);
				break;
			case RawStylusActions.Move:
				OnStylusMove(rawStylusInput);
				break;
			case RawStylusActions.Up:
				OnStylusUp(rawStylusInput);
				break;
			}
		}
	}

	/// <summary>Occurs on a pen thread when the cursor enters the bounds of an element.</summary>
	/// <param name="rawStylusInput">A <see cref="T:System.Windows.Input.StylusPlugIns.RawStylusInput" /> that contains information about input from the pen.</param>
	/// <param name="confirmed">true if the pen actually entered the bounds of the element; otherwise, false.</param>
	protected virtual void OnStylusEnter(RawStylusInput rawStylusInput, bool confirmed)
	{
	}

	/// <summary>Occurs on a pen thread when the cursor leaves the bounds of an element.</summary>
	/// <param name="rawStylusInput">A <see cref="T:System.Windows.Input.StylusPlugIns.RawStylusInput" /> that contains information about input from the pen.</param>
	/// <param name="confirmed">true if the pen actually left the bounds of the element; otherwise, false.</param>
	protected virtual void OnStylusLeave(RawStylusInput rawStylusInput, bool confirmed)
	{
	}

	/// <summary>Occurs on a thread in the pen thread pool when the tablet pen touches the digitizer.</summary>
	/// <param name="rawStylusInput">A <see cref="T:System.Windows.Input.StylusPlugIns.RawStylusInput" /> that contains information about input from the pen.</param>
	protected virtual void OnStylusDown(RawStylusInput rawStylusInput)
	{
	}

	/// <summary>Occurs on a pen thread when the tablet pen moves on the digitizer.</summary>
	/// <param name="rawStylusInput">A <see cref="T:System.Windows.Input.StylusPlugIns.RawStylusInput" /> that contains information about input from the pen.</param>
	protected virtual void OnStylusMove(RawStylusInput rawStylusInput)
	{
	}

	/// <summary>Occurs on a pen thread when the user lifts the tablet pen from the digitizer.</summary>
	/// <param name="rawStylusInput">A <see cref="T:System.Windows.Input.StylusPlugIns.RawStylusInput" /> that contains information about input from the pen.</param>
	protected virtual void OnStylusUp(RawStylusInput rawStylusInput)
	{
	}

	internal void FireCustomData(object callbackData, RawStylusActions action, bool targetVerified)
	{
		if (__enabled && _pic != null)
		{
			switch (action)
			{
			case RawStylusActions.Down:
				OnStylusDownProcessed(callbackData, targetVerified);
				break;
			case RawStylusActions.Move:
				OnStylusMoveProcessed(callbackData, targetVerified);
				break;
			case RawStylusActions.Up:
				OnStylusUpProcessed(callbackData, targetVerified);
				break;
			}
		}
	}

	/// <summary>Occurs on the application UI (user interface) thread when the tablet pen touches the digitizer.</summary>
	/// <param name="callbackData">The object that the application passed to the <see cref="M:System.Windows.Input.StylusPlugIns.RawStylusInput.NotifyWhenProcessed(System.Object)" /> method.</param>
	/// <param name="targetVerified">true if the pen's input was correctly routed to the <see cref="T:System.Windows.Input.StylusPlugIns.StylusPlugIn" />; otherwise, false.</param>
	protected virtual void OnStylusDownProcessed(object callbackData, bool targetVerified)
	{
	}

	/// <summary>Occurs on the application UI (user interface) thread when the tablet pen moves on the digitizer.</summary>
	/// <param name="callbackData">The object that the application passed to the <see cref="M:System.Windows.Input.StylusPlugIns.RawStylusInput.NotifyWhenProcessed(System.Object)" /> method.</param>
	/// <param name="targetVerified">true if the pen's input was correctly routed to the <see cref="T:System.Windows.Input.StylusPlugIns.StylusPlugIn" />; otherwise, false.</param>
	protected virtual void OnStylusMoveProcessed(object callbackData, bool targetVerified)
	{
	}

	/// <summary>Occurs on the application UI (user interface) thread when the user lifts the tablet pen from the digitizer.</summary>
	/// <param name="callbackData">The object that the application passed to the <see cref="M:System.Windows.Input.StylusPlugIns.RawStylusInput.NotifyWhenProcessed(System.Object)" /> method.</param>
	/// <param name="targetVerified">true if the pen's input was correctly routed to the <see cref="T:System.Windows.Input.StylusPlugIns.StylusPlugIn" />; otherwise, false.</param>
	protected virtual void OnStylusUpProcessed(object callbackData, bool targetVerified)
	{
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Input.StylusPlugIns.StylusPlugIn.Enabled" /> property changes.</summary>
	protected virtual void OnEnabledChanged()
	{
	}

	internal void InvalidateIsActiveForInput()
	{
		bool flag = _pic != null && Enabled && _pic.Contains(this) && _pic.IsActiveForInput;
		if (flag != _activeForInput)
		{
			_activeForInput = flag;
			OnIsActiveForInputChanged();
		}
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Input.StylusPlugIns.StylusPlugIn.IsActiveForInput" /> property changes.</summary>
	protected virtual void OnIsActiveForInputChanged()
	{
	}
}
