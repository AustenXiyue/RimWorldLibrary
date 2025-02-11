using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;

namespace System.Security;

/// <summary>Provides the main access point for classes interacting with the security system. This class cannot be inherited.</summary>
[ComVisible(true)]
public static class SecurityManager
{
	private static object _lockObject;

	private static ArrayList _hierarchy;

	private static IPermission _unmanagedCode;

	private static Hashtable _declsecCache;

	private static PolicyLevel _level;

	private static SecurityPermission _execution;

	/// <summary>Gets or sets a value indicating whether code must have <see cref="F:System.Security.Permissions.SecurityPermissionFlag.Execution" /> in order to execute.</summary>
	/// <returns>true if code must have <see cref="F:System.Security.Permissions.SecurityPermissionFlag.Execution" /> in order to execute; otherwise, false.</returns>
	/// <exception cref="T:System.Security.SecurityException">The code that calls this method does not have <see cref="F:System.Security.Permissions.SecurityPermissionFlag.ControlPolicy" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlPolicy" />
	/// </PermissionSet>
	[Obsolete]
	public static bool CheckExecutionRights
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	/// <summary>Gets or sets a value indicating whether security is enabled.</summary>
	/// <returns>true if security is enabled; otherwise, false.</returns>
	/// <exception cref="T:System.Security.SecurityException">The code that calls this method does not have <see cref="F:System.Security.Permissions.SecurityPermissionFlag.ControlPolicy" />. </exception>
	[Obsolete("The security manager cannot be turned off on MS runtime")]
	public static extern bool SecurityEnabled
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.Demand, ControlPolicy = true)]
		set;
	}

	private static IEnumerator Hierarchy
	{
		get
		{
			lock (_lockObject)
			{
				if (_hierarchy == null)
				{
					InitializePolicyHierarchy();
				}
			}
			return _hierarchy.GetEnumerator();
		}
	}

	internal static PolicyLevel ResolvingPolicyLevel
	{
		get
		{
			return _level;
		}
		set
		{
			_level = value;
		}
	}

	private static IPermission UnmanagedCode
	{
		get
		{
			lock (_lockObject)
			{
				if (_unmanagedCode == null)
				{
					_unmanagedCode = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
				}
			}
			return _unmanagedCode;
		}
	}

	static SecurityManager()
	{
		_execution = new SecurityPermission(SecurityPermissionFlag.Execution);
		_lockObject = new object();
	}

	internal static bool CheckElevatedPermissions()
	{
		return true;
	}

	[Conditional("ENABLE_SANDBOX")]
	internal static void EnsureElevatedPermissions()
	{
	}

	/// <summary>Gets the granted zone identity and URL identity permission sets for the current assembly.</summary>
	/// <param name="zone">An output parameter that contains an <see cref="T:System.Collections.ArrayList" /> of granted <see cref="P:System.Security.Permissions.ZoneIdentityPermissionAttribute.Zone" /> objects.</param>
	/// <param name="origin">An output parameter that contains an <see cref="T:System.Collections.ArrayList" /> of granted <see cref="T:System.Security.Permissions.UrlIdentityPermission" /> objects.</param>
	/// <exception cref="T:System.Security.SecurityException">The request for <see cref="T:System.Security.Permissions.StrongNameIdentityPermission" /> failed.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.StrongNameIdentityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PublicKeyBlob="00000000000000000400000000000000" Name="System.Windows.Forms" />
	/// </PermissionSet>
	[MonoTODO("CAS support is experimental (and unsupported). This method only works in FullTrust.")]
	[StrongNameIdentityPermission(SecurityAction.LinkDemand, PublicKey = "0x00000000000000000400000000000000")]
	public static void GetZoneAndOrigin(out ArrayList zone, out ArrayList origin)
	{
		zone = new ArrayList();
		origin = new ArrayList();
	}

	/// <summary>Determines whether a permission is granted to the caller.</summary>
	/// <returns>true if the permissions granted to the caller include the permission <paramref name="perm" />; otherwise, false.</returns>
	/// <param name="perm">The permission to test against the grant of the caller. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[Obsolete]
	public static bool IsGranted(IPermission perm)
	{
		if (perm == null)
		{
			return true;
		}
		if (!SecurityEnabled)
		{
			return true;
		}
		return IsGranted(Assembly.GetCallingAssembly(), perm);
	}

	internal static bool IsGranted(Assembly a, IPermission perm)
	{
		PermissionSet grantedPermissionSet = a.GrantedPermissionSet;
		if (grantedPermissionSet != null && !grantedPermissionSet.IsUnrestricted())
		{
			CodeAccessPermission target = (CodeAccessPermission)grantedPermissionSet.GetPermission(perm.GetType());
			if (!perm.IsSubsetOf(target))
			{
				return false;
			}
		}
		PermissionSet deniedPermissionSet = a.DeniedPermissionSet;
		if (deniedPermissionSet != null && !deniedPermissionSet.IsEmpty())
		{
			if (deniedPermissionSet.IsUnrestricted())
			{
				return false;
			}
			CodeAccessPermission codeAccessPermission = (CodeAccessPermission)a.DeniedPermissionSet.GetPermission(perm.GetType());
			if (codeAccessPermission != null && perm.IsSubsetOf(codeAccessPermission))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Loads a <see cref="T:System.Security.Policy.PolicyLevel" /> from the specified file.</summary>
	/// <returns>The loaded policy level.</returns>
	/// <param name="path">The physical file path to a file containing the security policy information. </param>
	/// <param name="type">One of the enumeration values that specifies the type of the policy level to be loaded. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The file indicated by the <paramref name="path" /> parameter does not exist. </exception>
	/// <exception cref="T:System.Security.SecurityException">The code that calls this method does not have <see cref="F:System.Security.Permissions.SecurityPermissionFlag.ControlPolicy" />.-or- The code that calls this method does not have <see cref="F:System.Security.Permissions.FileIOPermissionAccess.Read" />.-or- The code that calls this method does not have <see cref="F:System.Security.Permissions.FileIOPermissionAccess.Write" />.-or- The code that calls this method does not have <see cref="F:System.Security.Permissions.FileIOPermissionAccess.PathDiscovery" />. </exception>
	/// <exception cref="T:System.NotSupportedException">This method uses code access security (CAS) policy, which is obsolete in the .NET Framework 4.  To enable CAS policy for compatibility with earlier versions of the .NET Framework, use the &lt;legacyCasPolicy&gt; element.&lt;NetFx40_LegacySecurityPolicy&gt; Element.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[Obsolete]
	[SecurityPermission(SecurityAction.Demand, ControlPolicy = true)]
	public static PolicyLevel LoadPolicyLevelFromFile(string path, PolicyLevelType type)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		PolicyLevel policyLevel = null;
		try
		{
			policyLevel = new PolicyLevel(type.ToString(), type);
			policyLevel.LoadFromFile(path);
			return policyLevel;
		}
		catch (Exception innerException)
		{
			throw new ArgumentException(Locale.GetText("Invalid policy XML"), innerException);
		}
	}

	/// <summary>Loads a <see cref="T:System.Security.Policy.PolicyLevel" /> from the specified string.</summary>
	/// <returns>The loaded policy level.</returns>
	/// <param name="str">The XML representation of a security policy level in the same form in which it appears in a configuration file. </param>
	/// <param name="type">One of the enumeration values that specifies the type of the policy level to be loaded. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="str" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="str" /> parameter is not valid. </exception>
	/// <exception cref="T:System.Security.SecurityException">The code that calls this method does not have <see cref="F:System.Security.Permissions.SecurityPermissionFlag.ControlPolicy" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlPolicy" />
	/// </PermissionSet>
	[Obsolete]
	[SecurityPermission(SecurityAction.Demand, ControlPolicy = true)]
	public static PolicyLevel LoadPolicyLevelFromString(string str, PolicyLevelType type)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		PolicyLevel policyLevel = null;
		try
		{
			policyLevel = new PolicyLevel(type.ToString(), type);
			policyLevel.LoadFromString(str);
			return policyLevel;
		}
		catch (Exception innerException)
		{
			throw new ArgumentException(Locale.GetText("Invalid policy XML"), innerException);
		}
	}

	/// <summary>Provides an enumerator to access the security policy hierarchy by levels, such as computer policy and user policy.</summary>
	/// <returns>An enumerator for <see cref="T:System.Security.Policy.PolicyLevel" /> objects that compose the security policy hierarchy.</returns>
	/// <exception cref="T:System.NotSupportedException">This method uses code access security (CAS) policy, which is obsolete in the .NET Framework 4. To enable CAS policy for compatibility with earlier versions of the .NET Framework, use the &lt;legacyCasPolicy&gt; element.&lt;NetFx40_LegacySecurityPolicy&gt; Element.</exception>
	/// <exception cref="T:System.Security.SecurityException">The code that calls this method does not have <see cref="F:System.Security.Permissions.SecurityPermissionFlag.ControlPolicy" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlPolicy" />
	/// </PermissionSet>
	[Obsolete]
	[SecurityPermission(SecurityAction.Demand, ControlPolicy = true)]
	public static IEnumerator PolicyHierarchy()
	{
		return Hierarchy;
	}

	/// <summary>Determines what permissions to grant to code based on the specified evidence.</summary>
	/// <returns>The set of permissions that can be granted by the security system.</returns>
	/// <param name="evidence">The evidence set used to evaluate policy. </param>
	/// <exception cref="T:System.NotSupportedException">This method uses code access security (CAS) policy, which is obsolete in the .NET Framework 4. To enable CAS policy for compatibility with earlier versions of the .NET Framework, use the &lt;legacyCasPolicy&gt; element.&lt;NetFx40_LegacySecurityPolicy&gt; Element.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Obsolete]
	public static PermissionSet ResolvePolicy(Evidence evidence)
	{
		if (evidence == null)
		{
			return new PermissionSet(PermissionState.None);
		}
		PermissionSet ps = null;
		IEnumerator hierarchy = Hierarchy;
		while (hierarchy.MoveNext())
		{
			PolicyLevel pl = (PolicyLevel)hierarchy.Current;
			if (ResolvePolicyLevel(ref ps, pl, evidence))
			{
				break;
			}
		}
		ResolveIdentityPermissions(ps, evidence);
		return ps;
	}

	/// <summary>Determines what permissions to grant to code based on the specified evidence.</summary>
	/// <returns>The set of permissions that is appropriate for all of the provided evidence.</returns>
	/// <param name="evidences">An array of evidence objects used to evaluate policy. </param>
	/// <exception cref="T:System.NotSupportedException">This method uses code access security (CAS) policy, which is obsolete in the .NET Framework 4. To enable CAS policy for compatibility with earlier versions of the .NET Framework, use the &lt;legacyCasPolicy&gt; element.&lt;NetFx40_LegacySecurityPolicy&gt; Element.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Obsolete]
	[MonoTODO("(2.0) more tests are needed")]
	public static PermissionSet ResolvePolicy(Evidence[] evidences)
	{
		if (evidences == null || evidences.Length == 0 || (evidences.Length == 1 && evidences[0].Count == 0))
		{
			return new PermissionSet(PermissionState.None);
		}
		PermissionSet permissionSet = ResolvePolicy(evidences[0]);
		for (int i = 1; i < evidences.Length; i++)
		{
			permissionSet = permissionSet.Intersect(ResolvePolicy(evidences[i]));
		}
		return permissionSet;
	}

	/// <summary>Determines which permissions to grant to code based on the specified evidence, excluding the policy for the <see cref="T:System.AppDomain" /> level.</summary>
	/// <returns>The set of permissions that can be granted by the security system.</returns>
	/// <param name="evidence">The evidence set used to evaluate policy.</param>
	/// <exception cref="T:System.NotSupportedException">This method uses code access security (CAS) policy, which is obsolete in the .NET Framework 4. To enable CAS policy for compatibility with earlier versions of the .NET Framework, use the &lt;legacyCasPolicy&gt; element.&lt;NetFx40_LegacySecurityPolicy&gt; Element.</exception>
	[Obsolete]
	public static PermissionSet ResolveSystemPolicy(Evidence evidence)
	{
		if (evidence == null)
		{
			return new PermissionSet(PermissionState.None);
		}
		PermissionSet ps = null;
		IEnumerator hierarchy = Hierarchy;
		while (hierarchy.MoveNext())
		{
			PolicyLevel policyLevel = (PolicyLevel)hierarchy.Current;
			if (policyLevel.Type == PolicyLevelType.AppDomain || ResolvePolicyLevel(ref ps, policyLevel, evidence))
			{
				break;
			}
		}
		ResolveIdentityPermissions(ps, evidence);
		return ps;
	}

	/// <summary>Determines what permissions to grant to code based on the specified evidence and requests.</summary>
	/// <returns>The set of permissions that would be granted by the security system.</returns>
	/// <param name="evidence">The evidence set used to evaluate policy. </param>
	/// <param name="reqdPset">The required permissions the code needs to run. </param>
	/// <param name="optPset">The optional permissions that will be used if granted, but aren't required for the code to run. </param>
	/// <param name="denyPset">The denied permissions that must never be granted to the code even if policy otherwise permits it. </param>
	/// <param name="denied">An output parameter that contains the set of permissions not granted. </param>
	/// <exception cref="T:System.NotSupportedException">This method uses code access security (CAS) policy, which is obsolete in the .NET Framework 4. To enable CAS policy for compatibility with earlier versions of the .NET Framework, use the &lt;legacyCasPolicy&gt; element.&lt;NetFx40_LegacySecurityPolicy&gt; Element.</exception>
	/// <exception cref="T:System.Security.Policy.PolicyException">Policy fails to grant the minimum required permissions specified by the <paramref name="reqdPset" /> parameter. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Obsolete]
	public static PermissionSet ResolvePolicy(Evidence evidence, PermissionSet reqdPset, PermissionSet optPset, PermissionSet denyPset, out PermissionSet denied)
	{
		PermissionSet permissionSet = ResolvePolicy(evidence);
		if (reqdPset != null && !reqdPset.IsSubsetOf(permissionSet))
		{
			throw new PolicyException(Locale.GetText("Policy doesn't grant the minimal permissions required to execute the assembly."));
		}
		if (CheckExecutionRights)
		{
			bool flag = false;
			if (permissionSet != null)
			{
				if (permissionSet.IsUnrestricted())
				{
					flag = true;
				}
				else
				{
					IPermission permission = permissionSet.GetPermission(typeof(SecurityPermission));
					flag = _execution.IsSubsetOf(permission);
				}
			}
			if (!flag)
			{
				throw new PolicyException(Locale.GetText("Policy doesn't grant the right to execute the assembly."));
			}
		}
		denied = denyPset;
		return permissionSet;
	}

	/// <summary>Gets a collection of code groups matching the specified evidence.</summary>
	/// <returns>An enumeration of the set of code groups matching the evidence.</returns>
	/// <param name="evidence">The evidence set against which the policy is evaluated. </param>
	/// <exception cref="T:System.NotSupportedException">This method uses code access security (CAS) policy, which is obsolete in the .NET Framework 4. To enable CAS policy for compatibility with earlier versions of the .NET Framework, use the &lt;legacyCasPolicy&gt; element.&lt;NetFx40_LegacySecurityPolicy&gt; Element.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Obsolete]
	public static IEnumerator ResolvePolicyGroups(Evidence evidence)
	{
		if (evidence == null)
		{
			throw new ArgumentNullException("evidence");
		}
		ArrayList arrayList = new ArrayList();
		IEnumerator hierarchy = Hierarchy;
		while (hierarchy.MoveNext())
		{
			CodeGroup value = ((PolicyLevel)hierarchy.Current).ResolveMatchingCodeGroups(evidence);
			arrayList.Add(value);
		}
		return arrayList.GetEnumerator();
	}

	/// <summary>Saves the modified security policy state.</summary>
	/// <exception cref="T:System.NotSupportedException">This method uses code access security (CAS) policy, which is obsolete in the .NET Framework 4. To enable CAS policy for compatibility with earlier versions of the .NET Framework, use the &lt;legacyCasPolicy&gt; element.&lt;NetFx40_LegacySecurityPolicy&gt; Element.</exception>
	/// <exception cref="T:System.Security.SecurityException">The code that calls this method does not have <see cref="F:System.Security.Permissions.SecurityPermissionFlag.ControlPolicy" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[Obsolete]
	[SecurityPermission(SecurityAction.Demand, ControlPolicy = true)]
	public static void SavePolicy()
	{
		IEnumerator hierarchy = Hierarchy;
		while (hierarchy.MoveNext())
		{
			(hierarchy.Current as PolicyLevel).Save();
		}
	}

	/// <summary>Saves a modified security policy level loaded with <see cref="M:System.Security.SecurityManager.LoadPolicyLevelFromFile(System.String,System.Security.PolicyLevelType)" />.</summary>
	/// <param name="level">The policy level object to be saved. </param>
	/// <exception cref="T:System.Security.SecurityException">The code that calls this method does not have <see cref="F:System.Security.Permissions.SecurityPermissionFlag.ControlPolicy" />. </exception>
	/// <exception cref="T:System.NotSupportedException">This method uses code access security (CAS) policy, which is obsolete in the .NET Framework 4. To enable CAS policy for compatibility with earlier versions of the .NET Framework, use the &lt;legacyCasPolicy&gt; element.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[Obsolete]
	[SecurityPermission(SecurityAction.Demand, ControlPolicy = true)]
	public static void SavePolicyLevel(PolicyLevel level)
	{
		level.Save();
	}

	private static void InitializePolicyHierarchy()
	{
		string directoryName = Path.GetDirectoryName(Environment.GetMachineConfigPath());
		string path = Path.Combine(Environment.UnixGetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "mono");
		PolicyLevel policyLevel = (_level = new PolicyLevel("Enterprise", PolicyLevelType.Enterprise));
		policyLevel.LoadFromFile(Path.Combine(directoryName, "enterprisesec.config"));
		PolicyLevel policyLevel2 = (_level = new PolicyLevel("Machine", PolicyLevelType.Machine));
		policyLevel2.LoadFromFile(Path.Combine(directoryName, "security.config"));
		PolicyLevel policyLevel3 = (_level = new PolicyLevel("User", PolicyLevelType.User));
		policyLevel3.LoadFromFile(Path.Combine(path, "security.config"));
		_hierarchy = ArrayList.Synchronized(new ArrayList { policyLevel, policyLevel2, policyLevel3 });
		_level = null;
	}

	internal static bool ResolvePolicyLevel(ref PermissionSet ps, PolicyLevel pl, Evidence evidence)
	{
		PolicyStatement policyStatement = pl.Resolve(evidence);
		if (policyStatement != null)
		{
			if (ps == null)
			{
				ps = policyStatement.PermissionSet;
			}
			else
			{
				ps = ps.Intersect(policyStatement.PermissionSet);
				if (ps == null)
				{
					ps = new PermissionSet(PermissionState.None);
				}
			}
			if ((policyStatement.Attributes & PolicyStatementAttribute.LevelFinal) == PolicyStatementAttribute.LevelFinal)
			{
				return true;
			}
		}
		return false;
	}

	internal static void ResolveIdentityPermissions(PermissionSet ps, Evidence evidence)
	{
		if (ps.IsUnrestricted())
		{
			return;
		}
		IEnumerator hostEnumerator = evidence.GetHostEnumerator();
		while (hostEnumerator.MoveNext())
		{
			if (hostEnumerator.Current is IIdentityPermissionFactory identityPermissionFactory)
			{
				IPermission perm = identityPermissionFactory.CreateIdentityPermission(evidence);
				ps.AddPermission(perm);
			}
		}
	}

	internal static PermissionSet Decode(IntPtr permissions, int length)
	{
		PermissionSet permissionSet = null;
		lock (_lockObject)
		{
			if (_declsecCache == null)
			{
				_declsecCache = new Hashtable();
			}
			object key = (int)permissions;
			permissionSet = (PermissionSet)_declsecCache[key];
			if (permissionSet == null)
			{
				byte[] array = new byte[length];
				Marshal.Copy(permissions, array, 0, length);
				permissionSet = Decode(array);
				permissionSet.DeclarativeSecurity = true;
				_declsecCache.Add(key, permissionSet);
			}
		}
		return permissionSet;
	}

	internal static PermissionSet Decode(byte[] encodedPermissions)
	{
		if (encodedPermissions == null || encodedPermissions.Length < 1)
		{
			throw new SecurityException("Invalid metadata format.");
		}
		return encodedPermissions[0] switch
		{
			60 => new PermissionSet(Encoding.Unicode.GetString(encodedPermissions)), 
			46 => PermissionSet.CreateFromBinaryFormat(encodedPermissions), 
			_ => throw new SecurityException(Locale.GetText("Unknown metadata format.")), 
		};
	}

	private static void ThrowException(Exception ex)
	{
		throw ex;
	}

	/// <summary>Gets a permission set that is safe to grant to an application that has the provided evidence.</summary>
	/// <returns>A permission set that can be used as a grant set for the application that has the provided evidence.</returns>
	/// <param name="evidence">The host evidence to match to a permission set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="evidence" /> is null.</exception>
	public static PermissionSet GetStandardSandbox(Evidence evidence)
	{
		if (evidence == null)
		{
			throw new ArgumentNullException("evidence");
		}
		throw new NotImplementedException();
	}

	/// <summary>Determines whether the current thread requires a security context capture if its security state has to be re-created at a later point in time.</summary>
	/// <returns>false if the stack contains no partially trusted application domains, no partially trusted assemblies, and no currently active <see cref="M:System.Security.CodeAccessPermission.PermitOnly" /> or <see cref="M:System.Security.CodeAccessPermission.Deny" /> modifiers; true if the common language runtime cannot guarantee that the stack contains none of these. </returns>
	public static bool CurrentThreadRequiresSecurityContextCapture()
	{
		throw new NotImplementedException();
	}
}
