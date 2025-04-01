using System.Collections;
using System.Reflection;

namespace System.Diagnostics;

/// <summary>Identifies a switch used in an assembly, class, or member.</summary>
/// <filterpriority>1</filterpriority>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event)]
public sealed class SwitchAttribute : Attribute
{
	private Type type;

	private string name;

	private string description;

	/// <summary>Gets or sets the display name of the switch.</summary>
	/// <returns>The display name of the switch.</returns>
	/// <exception cref="T:System.ArgumentNullException">
	///   <see cref="P:System.Diagnostics.SwitchAttribute.SwitchName" /> is set to null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Diagnostics.SwitchAttribute.SwitchName" /> is set to an empty string.</exception>
	/// <filterpriority>2</filterpriority>
	public string SwitchName
	{
		get
		{
			return name;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException(global::SR.GetString("Argument {0} cannot be null or zero-length.", "value"), "value");
			}
			name = value;
		}
	}

	/// <summary>Gets or sets the type of the switch.</summary>
	/// <returns>The type of the switch.</returns>
	/// <exception cref="T:System.ArgumentNullException">
	///   <see cref="P:System.Diagnostics.SwitchAttribute.SwitchType" /> is set to null.</exception>
	/// <filterpriority>2</filterpriority>
	public Type SwitchType
	{
		get
		{
			return type;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			type = value;
		}
	}

	/// <summary>Gets or sets the description of the switch.</summary>
	/// <returns>The description of the switch.</returns>
	/// <filterpriority>2</filterpriority>
	public string SwitchDescription
	{
		get
		{
			return description;
		}
		set
		{
			description = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.SwitchAttribute" /> class, specifying the name and the type of the switch. </summary>
	/// <param name="switchName">The display name of the switch.</param>
	/// <param name="switchType">The type of the switch.</param>
	public SwitchAttribute(string switchName, Type switchType)
	{
		SwitchName = switchName;
		SwitchType = switchType;
	}

	/// <summary>Returns all switch attributes for the specified assembly.</summary>
	/// <returns>An array that contains all the switch attributes for the assembly.</returns>
	/// <param name="assembly">The assembly to check for switch attributes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assembly" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public static SwitchAttribute[] GetAll(Assembly assembly)
	{
		if (assembly == null)
		{
			throw new ArgumentNullException("assembly");
		}
		ArrayList arrayList = new ArrayList();
		object[] customAttributes = assembly.GetCustomAttributes(typeof(SwitchAttribute), inherit: false);
		arrayList.AddRange(customAttributes);
		Type[] types = assembly.GetTypes();
		for (int i = 0; i < types.Length; i++)
		{
			GetAllRecursive(types[i], arrayList);
		}
		SwitchAttribute[] array = new SwitchAttribute[arrayList.Count];
		arrayList.CopyTo(array, 0);
		return array;
	}

	private static void GetAllRecursive(Type type, ArrayList switchAttribs)
	{
		GetAllRecursive((MemberInfo)type, switchAttribs);
		MemberInfo[] members = type.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		for (int i = 0; i < members.Length; i++)
		{
			if (!(members[i] is Type))
			{
				GetAllRecursive(members[i], switchAttribs);
			}
		}
	}

	private static void GetAllRecursive(MemberInfo member, ArrayList switchAttribs)
	{
		object[] customAttributes = member.GetCustomAttributes(typeof(SwitchAttribute), inherit: false);
		switchAttribs.AddRange(customAttributes);
	}
}
