using System;

namespace Microsoft.Win32;

/// <summary>Defines the known folders for custom places in file dialog boxes.</summary>
public static class FileDialogCustomPlaces
{
	/// <summary>Gets the folder for application-specific data for the current roaming user.</summary>
	/// <returns>The folder for application-specific data for the current roaming user.</returns>
	public static FileDialogCustomPlace RoamingApplicationData => new FileDialogCustomPlace(new Guid("3EB685DB-65F9-4CF6-A03A-E3EF65729F3D"));

	/// <summary>Gets the folder for application-specific data that is used by the current, non-roaming user.</summary>
	/// <returns>The folder for application-specific data that is used by the current, non-roaming user.</returns>
	public static FileDialogCustomPlace LocalApplicationData => new FileDialogCustomPlace(new Guid("F1B32785-6FBA-4FCF-9D55-7B8E7F157091"));

	/// <summary>Gets the Internet cookies folder for the current user.</summary>
	/// <returns>The Internet cookies folder for the current user.</returns>
	public static FileDialogCustomPlace Cookies => new FileDialogCustomPlace(new Guid("2B0F765D-C0E9-4171-908E-08A611B84FF6"));

	/// <summary>Gets the Contacts folder for the current user.</summary>
	/// <returns>The Contacts folder for the current user.</returns>
	public static FileDialogCustomPlace Contacts => new FileDialogCustomPlace(new Guid("56784854-C6CB-462b-8169-88E350ACB882"));

	/// <summary>Gets the Favorites folder for the current user.</summary>
	/// <returns>The  Favorites folder for the current user.</returns>
	public static FileDialogCustomPlace Favorites => new FileDialogCustomPlace(new Guid("1777F761-68AD-4D8A-87BD-30B759FA33DD"));

	/// <summary>Gets the folder that contains the program groups for the current user.</summary>
	/// <returns>The folder that contains the program groups for the current user.</returns>
	public static FileDialogCustomPlace Programs => new FileDialogCustomPlace(new Guid("A77F5D77-2E2B-44C3-A6A2-ABA601054A51"));

	/// <summary>Gets the Music folder for the current user.</summary>
	/// <returns>The Music folder for the current user.</returns>
	public static FileDialogCustomPlace Music => new FileDialogCustomPlace(new Guid("4BD8D571-6D19-48D3-BE97-422220080E43"));

	/// <summary>Gets the Pictures folder for the current user.</summary>
	/// <returns>The Pictures folder for the current user.</returns>
	public static FileDialogCustomPlace Pictures => new FileDialogCustomPlace(new Guid("33E28130-4E1E-4676-835A-98395C3BC3BB"));

	/// <summary>Gets the folder that contains the Send To menu items for the current user.</summary>
	/// <returns>The folder that contains the Send To menu items for the current user.</returns>
	public static FileDialogCustomPlace SendTo => new FileDialogCustomPlace(new Guid("8983036C-27C0-404B-8F08-102D10DCFD74"));

	/// <summary>Gets the folder that contains the Start menu items for the current user.</summary>
	/// <returns>The folder that contains the Start menu items for the current user.</returns>
	public static FileDialogCustomPlace StartMenu => new FileDialogCustomPlace(new Guid("625B53C3-AB48-4EC1-BA1F-A1EF4146FC19"));

	/// <summary>Gets the folder that corresponds to the Startup program group for the current user.</summary>
	/// <returns>The folder that corresponds to the Startup program group for the current user.</returns>
	public static FileDialogCustomPlace Startup => new FileDialogCustomPlace(new Guid("B97D20BB-F46A-4C97-BA10-5E3608430854"));

	/// <summary>Gets the System folder.</summary>
	/// <returns>The System folder.</returns>
	public static FileDialogCustomPlace System => new FileDialogCustomPlace(new Guid("1AC14E77-02E7-4E5D-B744-2EB1AE5198B7"));

	/// <summary>Gets the folder for document templates for the current user.</summary>
	/// <returns>The folder for document templates for the current user.</returns>
	public static FileDialogCustomPlace Templates => new FileDialogCustomPlace(new Guid("A63293E8-664E-48DB-A079-DF759E0509F7"));

	/// <summary>Gets the folder for storing files on the desktop for the current user.</summary>
	/// <returns>The folder for storing files on the desktop for the current user.</returns>
	public static FileDialogCustomPlace Desktop => new FileDialogCustomPlace(new Guid("B4BFCC3A-DB2C-424C-B029-7FE99A87C641"));

	/// <summary>Gets the Documents folder for the current user.</summary>
	/// <returns>The Documents folder for the current user.</returns>
	public static FileDialogCustomPlace Documents => new FileDialogCustomPlace(new Guid("FDD39AD0-238F-46AF-ADB4-6C85480369C7"));

	/// <summary>Gets the Program Files folder.</summary>
	/// <returns>The Program Files folder.</returns>
	public static FileDialogCustomPlace ProgramFiles => new FileDialogCustomPlace(new Guid("905E63B6-C1BF-494E-B29C-65B732D3D21A"));

	/// <summary>Gets the folder for components that are shared across applications.</summary>
	/// <returns>The folder for components that are shared across applications.</returns>
	public static FileDialogCustomPlace ProgramFilesCommon => new FileDialogCustomPlace(new Guid("F7F1ED05-9F6D-47A2-AAAE-29D317C6F066"));
}
