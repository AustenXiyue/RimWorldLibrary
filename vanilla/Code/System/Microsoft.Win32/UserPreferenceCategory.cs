namespace Microsoft.Win32;

/// <summary>Defines identifiers that represent categories of user preferences.</summary>
public enum UserPreferenceCategory
{
	/// <summary>Indicates user preferences associated with accessibility features of the system for users with disabilities.</summary>
	Accessibility = 1,
	/// <summary>Indicates user preferences associated with system colors. This category includes such as the default color of windows or menus.</summary>
	Color,
	/// <summary>Indicates user preferences associated with the system desktop. This category includes the background image or background image layout of the desktop.</summary>
	Desktop,
	/// <summary>Indicates user preferences that are not associated with any other category.</summary>
	General,
	/// <summary>Indicates user preferences for icon settings, including icon height and spacing.</summary>
	Icon,
	/// <summary>Indicates user preferences for keyboard settings, such as the key down repeat rate and delay.</summary>
	Keyboard,
	/// <summary>Indicates user preferences for menu settings, such as menu delays and text alignment.</summary>
	Menu,
	/// <summary>Indicates user preferences for mouse settings, such as double-click time and mouse sensitivity.</summary>
	Mouse,
	/// <summary>Indicates user preferences for policy settings, such as user rights and access levels.</summary>
	Policy,
	/// <summary>Indicates the user preferences for system power settings. This category includes power feature settings, such as the idle time before the system automatically enters low power mode.</summary>
	Power,
	/// <summary>Indicates user preferences associated with the screensaver.</summary>
	Screensaver,
	/// <summary>Indicates user preferences associated with the dimensions and characteristics of windows on the system.</summary>
	Window,
	/// <summary>Indicates changes in user preferences for regional settings, such as the character encoding and culture strings.</summary>
	Locale,
	/// <summary>Indicates user preferences associated with visual styles, such as enabling or disabling visual styles and switching from one visual style to another.</summary>
	VisualStyle
}
