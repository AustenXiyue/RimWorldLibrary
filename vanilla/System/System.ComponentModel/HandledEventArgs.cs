using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides data for events that can be handled completely in an event handler. </summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class HandledEventArgs : EventArgs
{
	private bool handled;

	/// <summary>Gets or sets a value that indicates whether the event handler has completely handled the event or whether the system should continue its own processing.</summary>
	/// <returns>true if the event has been completely handled; otherwise, false.</returns>
	public bool Handled
	{
		get
		{
			return handled;
		}
		set
		{
			handled = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.HandledEventArgs" /> class with a default <see cref="P:System.ComponentModel.HandledEventArgs.Handled" /> property value of false.</summary>
	public HandledEventArgs()
		: this(defaultHandledValue: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.HandledEventArgs" /> class with the specified default value for the <see cref="P:System.ComponentModel.HandledEventArgs.Handled" /> property.</summary>
	/// <param name="defaultHandledValue">The default value for the <see cref="P:System.ComponentModel.HandledEventArgs.Handled" /> property.</param>
	public HandledEventArgs(bool defaultHandledValue)
	{
		handled = defaultHandledValue;
	}
}
