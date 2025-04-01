using System.Collections;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.ComponentModel.Design;

/// <summary>Represents a Windows menu or toolbar command item.</summary>
[ComVisible(true)]
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
public class MenuCommand
{
	private EventHandler execHandler;

	private EventHandler statusHandler;

	private CommandID commandID;

	private int status;

	private IDictionary properties;

	private const int ENABLED = 2;

	private const int INVISIBLE = 16;

	private const int CHECKED = 4;

	private const int SUPPORTED = 1;

	/// <summary>Gets or sets a value indicating whether this menu item is checked.</summary>
	/// <returns>true if the item is checked; otherwise, false.</returns>
	public virtual bool Checked
	{
		get
		{
			return (status & 4) != 0;
		}
		set
		{
			SetStatus(4, value);
		}
	}

	/// <summary>Gets a value indicating whether this menu item is available.</summary>
	/// <returns>true if the item is enabled; otherwise, false.</returns>
	public virtual bool Enabled
	{
		get
		{
			return (status & 2) != 0;
		}
		set
		{
			SetStatus(2, value);
		}
	}

	/// <summary>Gets the public properties associated with the <see cref="T:System.ComponentModel.Design.MenuCommand" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IDictionary" /> containing the public properties of the <see cref="T:System.ComponentModel.Design.MenuCommand" />. </returns>
	public virtual IDictionary Properties
	{
		get
		{
			if (properties == null)
			{
				properties = new HybridDictionary();
			}
			return properties;
		}
	}

	/// <summary>Gets or sets a value indicating whether this menu item is supported.</summary>
	/// <returns>true if the item is supported, which is the default; otherwise, false.</returns>
	public virtual bool Supported
	{
		get
		{
			return (status & 1) != 0;
		}
		set
		{
			SetStatus(1, value);
		}
	}

	/// <summary>Gets or sets a value indicating whether this menu item is visible.</summary>
	/// <returns>true if the item is visible; otherwise, false.</returns>
	public virtual bool Visible
	{
		get
		{
			return (status & 0x10) == 0;
		}
		set
		{
			SetStatus(16, !value);
		}
	}

	/// <summary>Gets the <see cref="T:System.ComponentModel.Design.CommandID" /> associated with this menu command.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.Design.CommandID" /> associated with the menu command.</returns>
	public virtual CommandID CommandID => commandID;

	/// <summary>Gets the OLE command status code for this menu item.</summary>
	/// <returns>An integer containing a mixture of status flags that reflect the state of this menu item.</returns>
	public virtual int OleStatus => status;

	/// <summary>Occurs when the menu command changes.</summary>
	public event EventHandler CommandChanged
	{
		add
		{
			statusHandler = (EventHandler)Delegate.Combine(statusHandler, value);
		}
		remove
		{
			statusHandler = (EventHandler)Delegate.Remove(statusHandler, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.MenuCommand" /> class.</summary>
	/// <param name="handler">The event to raise when the user selects the menu item or toolbar button. </param>
	/// <param name="command">The unique command ID that links this menu command to the environment's menu. </param>
	public MenuCommand(EventHandler handler, CommandID command)
	{
		execHandler = handler;
		commandID = command;
		status = 3;
	}

	private void SetStatus(int mask, bool value)
	{
		int num = status;
		num = ((!value) ? (num & ~mask) : (num | mask));
		if (num != status)
		{
			status = num;
			OnCommandChanged(EventArgs.Empty);
		}
	}

	/// <summary>Invokes the command.</summary>
	public virtual void Invoke()
	{
		if (execHandler == null)
		{
			return;
		}
		try
		{
			execHandler(this, EventArgs.Empty);
		}
		catch (CheckoutException ex)
		{
			if (ex == CheckoutException.Canceled)
			{
				return;
			}
			throw;
		}
	}

	/// <summary>Invokes the command with the given parameter.</summary>
	/// <param name="arg">An optional argument for use by the command.</param>
	public virtual void Invoke(object arg)
	{
		Invoke();
	}

	/// <summary>Raises the <see cref="E:System.ComponentModel.Design.MenuCommand.CommandChanged" /> event.</summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
	protected virtual void OnCommandChanged(EventArgs e)
	{
		if (statusHandler != null)
		{
			statusHandler(this, e);
		}
	}

	/// <summary>Returns a string representation of this menu command.</summary>
	/// <returns>A string containing the value of the <see cref="P:System.ComponentModel.Design.MenuCommand.CommandID" /> property appended with the names of any flags that are set, separated by pipe bars (|). These flag properties include <see cref="P:System.ComponentModel.Design.MenuCommand.Checked" />, <see cref="P:System.ComponentModel.Design.MenuCommand.Enabled" />, <see cref="P:System.ComponentModel.Design.MenuCommand.Supported" />, and <see cref="P:System.ComponentModel.Design.MenuCommand.Visible" />.</returns>
	public override string ToString()
	{
		string text = CommandID.ToString() + " : ";
		if ((status & 1) != 0)
		{
			text += "Supported";
		}
		if ((status & 2) != 0)
		{
			text += "|Enabled";
		}
		if ((status & 0x10) == 0)
		{
			text += "|Visible";
		}
		if ((status & 4) != 0)
		{
			text += "|Checked";
		}
		return text;
	}
}
