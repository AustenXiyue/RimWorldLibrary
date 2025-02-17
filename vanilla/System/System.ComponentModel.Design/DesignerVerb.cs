using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text.RegularExpressions;

namespace System.ComponentModel.Design;

/// <summary>Represents a verb that can be invoked from a designer.</summary>
[ComVisible(true)]
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
public class DesignerVerb : MenuCommand
{
	/// <summary>Gets or sets the description of the menu item for the verb.</summary>
	/// <returns>A string describing the menu item. </returns>
	public string Description
	{
		get
		{
			object obj = Properties["Description"];
			if (obj == null)
			{
				return string.Empty;
			}
			return (string)obj;
		}
		set
		{
			Properties["Description"] = value;
		}
	}

	/// <summary>Gets the text description for the verb command on the menu.</summary>
	/// <returns>A description for the verb command.</returns>
	public string Text
	{
		get
		{
			object obj = Properties["Text"];
			if (obj == null)
			{
				return string.Empty;
			}
			return (string)obj;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.DesignerVerb" /> class.</summary>
	/// <param name="text">The text of the menu command that is shown to the user. </param>
	/// <param name="handler">The event handler that performs the actions of the verb. </param>
	public DesignerVerb(string text, EventHandler handler)
		: base(handler, StandardCommands.VerbFirst)
	{
		Properties["Text"] = ((text == null) ? null : Regex.Replace(text, "\\(\\&.\\)", ""));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.DesignerVerb" /> class.</summary>
	/// <param name="text">The text of the menu command that is shown to the user. </param>
	/// <param name="handler">The event handler that performs the actions of the verb. </param>
	/// <param name="startCommandID">The starting command ID for this verb. By default, the designer architecture sets aside a range of command IDs for verbs. You can override this by providing a custom command ID. </param>
	public DesignerVerb(string text, EventHandler handler, CommandID startCommandID)
		: base(handler, startCommandID)
	{
		Properties["Text"] = ((text == null) ? null : Regex.Replace(text, "\\(\\&.\\)", ""));
	}

	/// <summary>Overrides <see cref="M:System.Object.ToString" />.</summary>
	/// <returns>The verb's text, or an empty string ("") if the text field is empty.</returns>
	public override string ToString()
	{
		return Text + " : " + base.ToString();
	}
}
