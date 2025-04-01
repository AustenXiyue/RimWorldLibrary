namespace System.ComponentModel;

/// <summary>Specifies the name of the category in which to group the property or event when displayed in a <see cref="T:System.Windows.Forms.PropertyGrid" /> control set to Categorized mode.</summary>
[AttributeUsage(AttributeTargets.All)]
public class CategoryAttribute : Attribute
{
	private static volatile CategoryAttribute appearance;

	private static volatile CategoryAttribute asynchronous;

	private static volatile CategoryAttribute behavior;

	private static volatile CategoryAttribute data;

	private static volatile CategoryAttribute design;

	private static volatile CategoryAttribute action;

	private static volatile CategoryAttribute format;

	private static volatile CategoryAttribute layout;

	private static volatile CategoryAttribute mouse;

	private static volatile CategoryAttribute key;

	private static volatile CategoryAttribute focus;

	private static volatile CategoryAttribute windowStyle;

	private static volatile CategoryAttribute dragDrop;

	private static volatile CategoryAttribute defAttr;

	private bool localized;

	private string categoryValue;

	/// <summary>Gets a <see cref="T:System.ComponentModel.CategoryAttribute" /> representing the Action category.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.CategoryAttribute" /> for the action category.</returns>
	public static CategoryAttribute Action
	{
		get
		{
			if (action == null)
			{
				action = new CategoryAttribute("Action");
			}
			return action;
		}
	}

	/// <summary>Gets a <see cref="T:System.ComponentModel.CategoryAttribute" /> representing the Appearance category.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.CategoryAttribute" /> for the appearance category.</returns>
	public static CategoryAttribute Appearance
	{
		get
		{
			if (appearance == null)
			{
				appearance = new CategoryAttribute("Appearance");
			}
			return appearance;
		}
	}

	/// <summary>Gets a <see cref="T:System.ComponentModel.CategoryAttribute" /> representing the Asynchronous category.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.CategoryAttribute" /> for the asynchronous category.</returns>
	public static CategoryAttribute Asynchronous
	{
		get
		{
			if (asynchronous == null)
			{
				asynchronous = new CategoryAttribute("Asynchronous");
			}
			return asynchronous;
		}
	}

	/// <summary>Gets a <see cref="T:System.ComponentModel.CategoryAttribute" /> representing the Behavior category.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.CategoryAttribute" /> for the behavior category.</returns>
	public static CategoryAttribute Behavior
	{
		get
		{
			if (behavior == null)
			{
				behavior = new CategoryAttribute("Behavior");
			}
			return behavior;
		}
	}

	/// <summary>Gets a <see cref="T:System.ComponentModel.CategoryAttribute" /> representing the Data category.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.CategoryAttribute" /> for the data category.</returns>
	public static CategoryAttribute Data
	{
		get
		{
			if (data == null)
			{
				data = new CategoryAttribute("Data");
			}
			return data;
		}
	}

	/// <summary>Gets a <see cref="T:System.ComponentModel.CategoryAttribute" /> representing the Default category.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.CategoryAttribute" /> for the default category.</returns>
	public static CategoryAttribute Default
	{
		get
		{
			if (defAttr == null)
			{
				defAttr = new CategoryAttribute();
			}
			return defAttr;
		}
	}

	/// <summary>Gets a <see cref="T:System.ComponentModel.CategoryAttribute" /> representing the Design category.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.CategoryAttribute" /> for the design category.</returns>
	public static CategoryAttribute Design
	{
		get
		{
			if (design == null)
			{
				design = new CategoryAttribute("Design");
			}
			return design;
		}
	}

	/// <summary>Gets a <see cref="T:System.ComponentModel.CategoryAttribute" /> representing the DragDrop category.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.CategoryAttribute" /> for the drag-and-drop category.</returns>
	public static CategoryAttribute DragDrop
	{
		get
		{
			if (dragDrop == null)
			{
				dragDrop = new CategoryAttribute("DragDrop");
			}
			return dragDrop;
		}
	}

	/// <summary>Gets a <see cref="T:System.ComponentModel.CategoryAttribute" /> representing the Focus category.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.CategoryAttribute" /> for the focus category.</returns>
	public static CategoryAttribute Focus
	{
		get
		{
			if (focus == null)
			{
				focus = new CategoryAttribute("Focus");
			}
			return focus;
		}
	}

	/// <summary>Gets a <see cref="T:System.ComponentModel.CategoryAttribute" /> representing the Format category.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.CategoryAttribute" /> for the format category.</returns>
	public static CategoryAttribute Format
	{
		get
		{
			if (format == null)
			{
				format = new CategoryAttribute("Format");
			}
			return format;
		}
	}

	/// <summary>Gets a <see cref="T:System.ComponentModel.CategoryAttribute" /> representing the Key category.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.CategoryAttribute" /> for the key category.</returns>
	public static CategoryAttribute Key
	{
		get
		{
			if (key == null)
			{
				key = new CategoryAttribute("Key");
			}
			return key;
		}
	}

	/// <summary>Gets a <see cref="T:System.ComponentModel.CategoryAttribute" /> representing the Layout category.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.CategoryAttribute" /> for the layout category.</returns>
	public static CategoryAttribute Layout
	{
		get
		{
			if (layout == null)
			{
				layout = new CategoryAttribute("Layout");
			}
			return layout;
		}
	}

	/// <summary>Gets a <see cref="T:System.ComponentModel.CategoryAttribute" /> representing the Mouse category.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.CategoryAttribute" /> for the mouse category.</returns>
	public static CategoryAttribute Mouse
	{
		get
		{
			if (mouse == null)
			{
				mouse = new CategoryAttribute("Mouse");
			}
			return mouse;
		}
	}

	/// <summary>Gets a <see cref="T:System.ComponentModel.CategoryAttribute" /> representing the WindowStyle category.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.CategoryAttribute" /> for the window style category.</returns>
	public static CategoryAttribute WindowStyle
	{
		get
		{
			if (windowStyle == null)
			{
				windowStyle = new CategoryAttribute("WindowStyle");
			}
			return windowStyle;
		}
	}

	/// <summary>Gets the name of the category for the property or event that this attribute is applied to.</summary>
	/// <returns>The name of the category for the property or event that this attribute is applied to.</returns>
	public string Category
	{
		get
		{
			if (!localized)
			{
				localized = true;
				string localizedString = GetLocalizedString(categoryValue);
				if (localizedString != null)
				{
					categoryValue = localizedString;
				}
			}
			return categoryValue;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.CategoryAttribute" /> class using the category name Default.</summary>
	public CategoryAttribute()
		: this("Default")
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.CategoryAttribute" /> class using the specified category name.</summary>
	/// <param name="category">The name of the category. </param>
	public CategoryAttribute(string category)
	{
		categoryValue = category;
		localized = false;
	}

	/// <summary>Returns whether the value of the given object is equal to the current <see cref="T:System.ComponentModel.CategoryAttribute" />..</summary>
	/// <returns>true if the value of the given object is equal to that of the current; otherwise, false.</returns>
	/// <param name="obj">The object to test the value equality of. </param>
	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is CategoryAttribute)
		{
			return Category.Equals(((CategoryAttribute)obj).Category);
		}
		return false;
	}

	/// <summary>Returns the hash code for this attribute.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override int GetHashCode()
	{
		return Category.GetHashCode();
	}

	/// <summary>Looks up the localized name of the specified category.</summary>
	/// <returns>The localized name of the category, or null if a localized name does not exist.</returns>
	/// <param name="value">The identifer for the category to look up. </param>
	protected virtual string GetLocalizedString(string value)
	{
		return value switch
		{
			"Action" => "Action", 
			"Appearance" => "Appearance", 
			"Behavior" => "Behavior", 
			"Data" => "Data", 
			"DDE" => "DDE", 
			"Design" => "Design", 
			"Focus" => "Focus", 
			"Font" => "Font", 
			"Key" => "Key", 
			"List" => "List", 
			"Layout" => "Layout", 
			"Mouse" => "Mouse", 
			"Position" => "Position", 
			"Text" => "Text", 
			"Scale" => "Scale", 
			"Config" => "Configurations", 
			"Default" => "Misc", 
			"DragDrop" => "Drag Drop", 
			"WindowStyle" => "Window Style", 
			_ => value, 
		};
	}

	/// <summary>Determines if this attribute is the default.</summary>
	/// <returns>true if the attribute is the default value for this attribute class; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override bool IsDefaultAttribute()
	{
		return Category.Equals(Default.Category);
	}
}
