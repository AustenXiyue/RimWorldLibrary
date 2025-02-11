using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.ComponentModel.Design;

/// <summary>Represents a unique command identifier that consists of a numeric command ID and a GUID menu group identifier.</summary>
[ComVisible(true)]
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
public class CommandID
{
	private readonly Guid menuGroup;

	private readonly int commandID;

	/// <summary>Gets the numeric command ID.</summary>
	/// <returns>The command ID number.</returns>
	public virtual int ID => commandID;

	/// <summary>Gets the GUID of the menu group that the menu command identified by this <see cref="T:System.ComponentModel.Design.CommandID" /> belongs to.</summary>
	/// <returns>The GUID of the command group for this command.</returns>
	public virtual Guid Guid => menuGroup;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.CommandID" /> class using the specified menu group GUID and command ID number.</summary>
	/// <param name="menuGroup">The GUID of the group that this menu command belongs to. </param>
	/// <param name="commandID">The numeric identifier of this menu command. </param>
	public CommandID(Guid menuGroup, int commandID)
	{
		this.menuGroup = menuGroup;
		this.commandID = commandID;
	}

	/// <summary>Determines whether two <see cref="T:System.ComponentModel.Design.CommandID" /> instances are equal.</summary>
	/// <returns>true if the specified object is equivalent to this one; otherwise, false.</returns>
	/// <param name="obj">The object to compare. </param>
	public override bool Equals(object obj)
	{
		if (!(obj is CommandID))
		{
			return false;
		}
		CommandID commandID = (CommandID)obj;
		if (commandID.menuGroup.Equals(menuGroup))
		{
			return commandID.commandID == this.commandID;
		}
		return false;
	}

	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return (menuGroup.GetHashCode() << 2) | commandID;
	}

	/// <summary>Returns a <see cref="T:System.String" /> that represents the current object.</summary>
	/// <returns>A string that contains the command ID information, both the GUID and integer identifier. </returns>
	public override string ToString()
	{
		return menuGroup.ToString() + " : " + commandID.ToString(CultureInfo.CurrentCulture);
	}
}
