using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using Microsoft.Win32;

namespace System.Runtime.Versioning;

/// <summary>Provides methods to aid developers in writing version-safe code. This class cannot be inherited.</summary>
public static class VersioningHelper
{
	private const ResourceScope ResTypeMask = ResourceScope.Machine | ResourceScope.Process | ResourceScope.AppDomain | ResourceScope.Library;

	private const ResourceScope VisibilityMask = ResourceScope.Private | ResourceScope.Assembly;

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecuritySafeCritical]
	private static extern int GetRuntimeId();

	/// <summary>Returns a version-safe name based on the specified resource name and the intended resource consumption source.</summary>
	/// <returns>A version-safe name.</returns>
	/// <param name="name">The name of the resource.</param>
	/// <param name="from">The scope of the resource.</param>
	/// <param name="to">The desired resource consumption scope.</param>
	public static string MakeVersionSafeName(string name, ResourceScope from, ResourceScope to)
	{
		return MakeVersionSafeName(name, from, to, null);
	}

	/// <summary>Returns a version-safe name based on the specified resource name, the intended resource consumption scope, and the type using the resource.</summary>
	/// <returns>A version-safe name.</returns>
	/// <param name="name">The name of the resource.</param>
	/// <param name="from">The beginning of the scope range.</param>
	/// <param name="to">The end of the scope range.</param>
	/// <param name="type">The <see cref="T:System.Type" /> of the resource.</param>
	/// <exception cref="T:System.ArgumentException">The values for <paramref name="from " />and <paramref name="to " />are invalid. The resource type in the <see cref="T:System.Runtime.Versioning.ResourceScope" />  enumeration is going from a more restrictive resource type to a more general resource type.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type " />is null.</exception>
	[SecuritySafeCritical]
	public static string MakeVersionSafeName(string name, ResourceScope from, ResourceScope to, Type type)
	{
		ResourceScope resourceScope = from & (ResourceScope.Machine | ResourceScope.Process | ResourceScope.AppDomain | ResourceScope.Library);
		ResourceScope resourceScope2 = to & (ResourceScope.Machine | ResourceScope.Process | ResourceScope.AppDomain | ResourceScope.Library);
		if (resourceScope > resourceScope2)
		{
			throw new ArgumentException(Environment.GetResourceString("Resource type in the ResourceScope enum is going from a more restrictive resource type to a more general one.  From: \"{0}\"  To: \"{1}\"", resourceScope, resourceScope2), "from");
		}
		SxSRequirements requirements = GetRequirements(to, from);
		if ((requirements & (SxSRequirements.AssemblyName | SxSRequirements.TypeName)) != 0 && type == null)
		{
			throw new ArgumentNullException("type", Environment.GetResourceString("The type parameter cannot be null when scoping the resource's visibility to Private or Assembly."));
		}
		StringBuilder stringBuilder = new StringBuilder(name);
		char value = '_';
		if ((requirements & SxSRequirements.ProcessID) != 0)
		{
			stringBuilder.Append(value);
			stringBuilder.Append('p');
			stringBuilder.Append(NativeMethods.GetCurrentProcessId());
		}
		if ((requirements & SxSRequirements.CLRInstanceID) != 0)
		{
			string cLRInstanceString = GetCLRInstanceString();
			stringBuilder.Append(value);
			stringBuilder.Append('r');
			stringBuilder.Append(cLRInstanceString);
		}
		if ((requirements & SxSRequirements.AppDomainID) != 0)
		{
			stringBuilder.Append(value);
			stringBuilder.Append("ad");
			stringBuilder.Append(AppDomain.CurrentDomain.Id);
		}
		if ((requirements & SxSRequirements.TypeName) != 0)
		{
			stringBuilder.Append(value);
			stringBuilder.Append(type.Name);
		}
		if ((requirements & SxSRequirements.AssemblyName) != 0)
		{
			stringBuilder.Append(value);
			stringBuilder.Append(type.Assembly.FullName);
		}
		return stringBuilder.ToString();
	}

	private static string GetCLRInstanceString()
	{
		return GetRuntimeId().ToString(CultureInfo.InvariantCulture);
	}

	private static SxSRequirements GetRequirements(ResourceScope consumeAsScope, ResourceScope calleeScope)
	{
		SxSRequirements sxSRequirements = SxSRequirements.None;
		switch (calleeScope & (ResourceScope.Machine | ResourceScope.Process | ResourceScope.AppDomain | ResourceScope.Library))
		{
		case ResourceScope.Machine:
			switch (consumeAsScope & (ResourceScope.Machine | ResourceScope.Process | ResourceScope.AppDomain | ResourceScope.Library))
			{
			case ResourceScope.Process:
				sxSRequirements |= SxSRequirements.ProcessID;
				break;
			case ResourceScope.AppDomain:
				sxSRequirements |= SxSRequirements.AppDomainID | SxSRequirements.ProcessID | SxSRequirements.CLRInstanceID;
				break;
			default:
				throw new ArgumentException(Environment.GetResourceString("Unknown value for the ResourceScope: {0}  Too many resource type bits may be set.", consumeAsScope), "consumeAsScope");
			case ResourceScope.Machine:
				break;
			}
			break;
		case ResourceScope.Process:
			if ((consumeAsScope & ResourceScope.AppDomain) != 0)
			{
				sxSRequirements |= SxSRequirements.AppDomainID | SxSRequirements.CLRInstanceID;
			}
			break;
		default:
			throw new ArgumentException(Environment.GetResourceString("Unknown value for the ResourceScope: {0}  Too many resource type bits may be set.", calleeScope), "calleeScope");
		case ResourceScope.AppDomain:
			break;
		}
		switch (calleeScope & (ResourceScope.Private | ResourceScope.Assembly))
		{
		case ResourceScope.None:
			switch (consumeAsScope & (ResourceScope.Private | ResourceScope.Assembly))
			{
			case ResourceScope.Assembly:
				sxSRequirements |= SxSRequirements.AssemblyName;
				break;
			case ResourceScope.Private:
				sxSRequirements |= SxSRequirements.AssemblyName | SxSRequirements.TypeName;
				break;
			default:
				throw new ArgumentException(Environment.GetResourceString("Unknown value for the ResourceScope: {0}  Too many resource visibility bits may be set.", consumeAsScope), "consumeAsScope");
			case ResourceScope.None:
				break;
			}
			break;
		case ResourceScope.Assembly:
			if ((consumeAsScope & ResourceScope.Private) != 0)
			{
				sxSRequirements |= SxSRequirements.TypeName;
			}
			break;
		default:
			throw new ArgumentException(Environment.GetResourceString("Unknown value for the ResourceScope: {0}  Too many resource visibility bits may be set.", calleeScope), "calleeScope");
		case ResourceScope.Private:
			break;
		}
		return sxSRequirements;
	}
}
