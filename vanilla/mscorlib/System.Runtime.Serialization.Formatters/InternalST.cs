using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Runtime.Serialization.Formatters;

/// <summary>Logs tracing messages when the .NET Framework serialization infrastructure is compiled.</summary>
[ComVisible(true)]
[SecurityCritical]
public sealed class InternalST
{
	private InternalST()
	{
	}

	/// <summary>Prints SOAP trace messages.</summary>
	/// <param name="messages">An array of trace messages to print.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.StrongNameIdentityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PublicKeyBlob="002400000480000094000000060200000024000052534131000400000100010007D1FA57C4AED9F0A32E84AA0FAEFD0DE9E8FD6AEC8F87FB03766C834C99921EB23BE79AD9D5DCC1DD9AD236132102900B723CF980957FC4E177108FC607774F29E8320E92EA05ECE4E821C0A5EFE8F1645C4C0C93C1AB99285D622CAA652C1DFAD63D745D6F2DE5F17E5EAF0FC4963D261C8A12436518206DC093344D5AD293" Name="System.Runtime.Serialization.Formatters.Soap" />
	/// </PermissionSet>
	[Conditional("_LOGGING")]
	public static void InfoSoap(params object[] messages)
	{
	}

	/// <summary>Checks if SOAP tracing is enabled.</summary>
	/// <returns>true, if tracing is enabled; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.StrongNameIdentityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PublicKeyBlob="002400000480000094000000060200000024000052534131000400000100010007D1FA57C4AED9F0A32E84AA0FAEFD0DE9E8FD6AEC8F87FB03766C834C99921EB23BE79AD9D5DCC1DD9AD236132102900B723CF980957FC4E177108FC607774F29E8320E92EA05ECE4E821C0A5EFE8F1645C4C0C93C1AB99285D622CAA652C1DFAD63D745D6F2DE5F17E5EAF0FC4963D261C8A12436518206DC093344D5AD293" Name="System.Runtime.Serialization.Formatters.Soap" />
	/// </PermissionSet>
	public static bool SoapCheckEnabled()
	{
		return BCLDebug.CheckEnabled("Soap");
	}

	/// <summary>Processes the specified array of messages.</summary>
	/// <param name="messages">An array of messages to process.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.StrongNameIdentityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PublicKeyBlob="002400000480000094000000060200000024000052534131000400000100010007D1FA57C4AED9F0A32E84AA0FAEFD0DE9E8FD6AEC8F87FB03766C834C99921EB23BE79AD9D5DCC1DD9AD236132102900B723CF980957FC4E177108FC607774F29E8320E92EA05ECE4E821C0A5EFE8F1645C4C0C93C1AB99285D622CAA652C1DFAD63D745D6F2DE5F17E5EAF0FC4963D261C8A12436518206DC093344D5AD293" Name="System.Runtime.Serialization.Formatters.Soap" />
	/// </PermissionSet>
	[Conditional("SER_LOGGING")]
	public static void Soap(params object[] messages)
	{
		if (!(messages[0] is string))
		{
			messages[0] = messages[0].GetType().Name + " ";
		}
		else
		{
			messages[0] = string.Concat(messages[0], " ");
		}
	}

	/// <summary>Asserts the specified message.</summary>
	/// <param name="condition">A Boolean value to use when asserting.</param>
	/// <param name="message">The message to use when asserting.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.StrongNameIdentityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PublicKeyBlob="002400000480000094000000060200000024000052534131000400000100010007D1FA57C4AED9F0A32E84AA0FAEFD0DE9E8FD6AEC8F87FB03766C834C99921EB23BE79AD9D5DCC1DD9AD236132102900B723CF980957FC4E177108FC607774F29E8320E92EA05ECE4E821C0A5EFE8F1645C4C0C93C1AB99285D622CAA652C1DFAD63D745D6F2DE5F17E5EAF0FC4963D261C8A12436518206DC093344D5AD293" Name="System.Runtime.Serialization.Formatters.Soap" />
	/// </PermissionSet>
	[Conditional("_DEBUG")]
	public static void SoapAssert(bool condition, string message)
	{
	}

	/// <summary>Sets the value of a field.</summary>
	/// <param name="fi">A <see cref="T:System.Reflection.FieldInfo" /> containing data about the target field.</param>
	/// <param name="target">The field to change.</param>
	/// <param name="value">The value to set.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.StrongNameIdentityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PublicKeyBlob="002400000480000094000000060200000024000052534131000400000100010007D1FA57C4AED9F0A32E84AA0FAEFD0DE9E8FD6AEC8F87FB03766C834C99921EB23BE79AD9D5DCC1DD9AD236132102900B723CF980957FC4E177108FC607774F29E8320E92EA05ECE4E821C0A5EFE8F1645C4C0C93C1AB99285D622CAA652C1DFAD63D745D6F2DE5F17E5EAF0FC4963D261C8A12436518206DC093344D5AD293" Name="System.Runtime.Serialization.Formatters.Soap" />
	/// </PermissionSet>
	public static void SerializationSetValue(FieldInfo fi, object target, object value)
	{
		if (fi == null)
		{
			throw new ArgumentNullException("fi");
		}
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		FormatterServices.SerializationSetValue(fi, target, value);
	}

	/// <summary>Loads a specified assembly to debug.</summary>
	/// <returns>The <see cref="T:System.Reflection.Assembly" /> to debug.</returns>
	/// <param name="assemblyString">The name of the assembly to load.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.StrongNameIdentityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PublicKeyBlob="002400000480000094000000060200000024000052534131000400000100010007D1FA57C4AED9F0A32E84AA0FAEFD0DE9E8FD6AEC8F87FB03766C834C99921EB23BE79AD9D5DCC1DD9AD236132102900B723CF980957FC4E177108FC607774F29E8320E92EA05ECE4E821C0A5EFE8F1645C4C0C93C1AB99285D622CAA652C1DFAD63D745D6F2DE5F17E5EAF0FC4963D261C8A12436518206DC093344D5AD293" Name="System.Runtime.Serialization.Formatters.Soap" />
	/// </PermissionSet>
	public static Assembly LoadAssemblyFromString(string assemblyString)
	{
		return FormatterServices.LoadAssemblyFromString(assemblyString);
	}
}
