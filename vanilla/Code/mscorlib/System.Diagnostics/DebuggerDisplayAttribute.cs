using System.Runtime.InteropServices;

namespace System.Diagnostics;

/// <summary>Determines how a class or field is displayed in the debugger variable windows.</summary>
/// <filterpriority>1</filterpriority>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Delegate, AllowMultiple = true)]
[ComVisible(true)]
public sealed class DebuggerDisplayAttribute : Attribute
{
	private string name;

	private string value;

	private string type;

	private string targetName;

	private Type target;

	/// <summary>Gets the string to display in the value column of the debugger variable windows.</summary>
	/// <returns>The string to display in the value column of the debugger variable.</returns>
	/// <filterpriority>2</filterpriority>
	public string Value => value;

	/// <summary>Gets or sets the name to display in the debugger variable windows.</summary>
	/// <returns>The name to display in the debugger variable windows.</returns>
	/// <filterpriority>2</filterpriority>
	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	/// <summary>Gets or sets the string to display in the type column of the debugger variable windows.</summary>
	/// <returns>The string to display in the type column of the debugger variable windows.</returns>
	/// <filterpriority>2</filterpriority>
	public string Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
		}
	}

	/// <summary>Gets or sets the type of the attribute's target.</summary>
	/// <returns>The attribute's target type.</returns>
	/// <exception cref="T:System.ArgumentNullException">
	///   <see cref="P:System.Diagnostics.DebuggerDisplayAttribute.Target" /> is set to null.</exception>
	/// <filterpriority>2</filterpriority>
	public Type Target
	{
		get
		{
			return target;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			targetName = value.AssemblyQualifiedName;
			target = value;
		}
	}

	/// <summary>Gets or sets the type name of the attribute's target.</summary>
	/// <returns>The name of the attribute's target type.</returns>
	/// <filterpriority>2</filterpriority>
	public string TargetTypeName
	{
		get
		{
			return targetName;
		}
		set
		{
			targetName = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.DebuggerDisplayAttribute" /> class. </summary>
	/// <param name="value">The string to be displayed in the value column for instances of the type; an empty string ("") causes the value column to be hidden.</param>
	public DebuggerDisplayAttribute(string value)
	{
		if (value == null)
		{
			this.value = "";
		}
		else
		{
			this.value = value;
		}
		name = "";
		type = "";
	}
}
