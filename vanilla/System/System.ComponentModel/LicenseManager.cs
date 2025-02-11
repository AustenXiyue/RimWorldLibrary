using System.Collections;
using System.ComponentModel.Design;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides properties and methods to add a license to a component and to manage a <see cref="T:System.ComponentModel.LicenseProvider" />. This class cannot be inherited.</summary>
[HostProtection(SecurityAction.LinkDemand, ExternalProcessMgmt = true)]
public sealed class LicenseManager
{
	private class LicenseInteropHelper
	{
		internal class CLRLicenseContext : LicenseContext
		{
			private LicenseUsageMode usageMode;

			private Type type;

			private string key;

			public override LicenseUsageMode UsageMode => usageMode;

			public CLRLicenseContext(LicenseUsageMode usageMode, Type type)
			{
				this.usageMode = usageMode;
				this.type = type;
			}

			public override string GetSavedLicenseKey(Type type, Assembly resourceAssembly)
			{
				if (!(type == this.type))
				{
					return null;
				}
				return key;
			}

			public override void SetSavedLicenseKey(Type type, string key)
			{
				if (type == this.type)
				{
					this.key = key;
				}
			}
		}

		private const int S_OK = 0;

		private const int E_NOTIMPL = -2147467263;

		private const int CLASS_E_NOTLICENSED = -2147221230;

		private const int E_FAIL = -2147483640;

		private DesigntimeLicenseContext helperContext;

		private LicenseContext savedLicenseContext;

		private Type savedType;

		private static object AllocateAndValidateLicense(RuntimeTypeHandle rth, IntPtr bstrKey, int fDesignTime)
		{
			Type typeFromHandle = Type.GetTypeFromHandle(rth);
			CLRLicenseContext cLRLicenseContext = new CLRLicenseContext((fDesignTime != 0) ? LicenseUsageMode.Designtime : LicenseUsageMode.Runtime, typeFromHandle);
			if (fDesignTime == 0 && bstrKey != (IntPtr)0)
			{
				cLRLicenseContext.SetSavedLicenseKey(typeFromHandle, Marshal.PtrToStringBSTR(bstrKey));
			}
			try
			{
				return CreateWithContext(typeFromHandle, cLRLicenseContext);
			}
			catch (LicenseException ex)
			{
				throw new COMException(ex.Message, -2147221230);
			}
		}

		private static int RequestLicKey(RuntimeTypeHandle rth, ref IntPtr pbstrKey)
		{
			Type typeFromHandle = Type.GetTypeFromHandle(rth);
			if (!ValidateInternalRecursive(CurrentContext, typeFromHandle, null, allowExceptions: false, out var license, out var licenseKey))
			{
				return -2147483640;
			}
			if (licenseKey == null)
			{
				return -2147483640;
			}
			pbstrKey = Marshal.StringToBSTR(licenseKey);
			if (license != null)
			{
				license.Dispose();
				license = null;
			}
			return 0;
		}

		private void GetLicInfo(RuntimeTypeHandle rth, ref int pRuntimeKeyAvail, ref int pLicVerified)
		{
			pRuntimeKeyAvail = 0;
			pLicVerified = 0;
			Type typeFromHandle = Type.GetTypeFromHandle(rth);
			if (helperContext == null)
			{
				helperContext = new DesigntimeLicenseContext();
			}
			else
			{
				helperContext.savedLicenseKeys.Clear();
			}
			if (ValidateInternalRecursive(helperContext, typeFromHandle, null, allowExceptions: false, out var license, out var _))
			{
				if (helperContext.savedLicenseKeys.Contains(typeFromHandle.AssemblyQualifiedName))
				{
					pRuntimeKeyAvail = 1;
				}
				if (license != null)
				{
					license.Dispose();
					license = null;
					pLicVerified = 1;
				}
			}
		}

		private void GetCurrentContextInfo(ref int fDesignTime, ref IntPtr bstrKey, RuntimeTypeHandle rth)
		{
			savedLicenseContext = CurrentContext;
			savedType = Type.GetTypeFromHandle(rth);
			if (savedLicenseContext.UsageMode == LicenseUsageMode.Designtime)
			{
				fDesignTime = 1;
				bstrKey = (IntPtr)0;
			}
			else
			{
				fDesignTime = 0;
				string savedLicenseKey = savedLicenseContext.GetSavedLicenseKey(savedType, null);
				bstrKey = Marshal.StringToBSTR(savedLicenseKey);
			}
		}

		private void SaveKeyInCurrentContext(IntPtr bstrKey)
		{
			if (bstrKey != (IntPtr)0)
			{
				savedLicenseContext.SetSavedLicenseKey(savedType, Marshal.PtrToStringBSTR(bstrKey));
			}
		}
	}

	private static readonly object selfLock = new object();

	private static volatile LicenseContext context = null;

	private static object contextLockHolder = null;

	private static volatile Hashtable providers;

	private static volatile Hashtable providerInstances;

	private static object internalSyncObject = new object();

	/// <summary>Gets or sets the current <see cref="T:System.ComponentModel.LicenseContext" />, which specifies when you can use the licensed object.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.LicenseContext" /> that specifies when you can use the licensed object.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.ComponentModel.LicenseManager.CurrentContext" /> property is currently locked and cannot be changed.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static LicenseContext CurrentContext
	{
		get
		{
			if (context == null)
			{
				lock (internalSyncObject)
				{
					if (context == null)
					{
						context = new RuntimeLicenseContext();
					}
				}
			}
			return context;
		}
		set
		{
			lock (internalSyncObject)
			{
				if (contextLockHolder != null)
				{
					throw new InvalidOperationException(global::SR.GetString("The CurrentContext property of the LicenseManager is currently locked and cannot be changed."));
				}
				context = value;
			}
		}
	}

	/// <summary>Gets the <see cref="T:System.ComponentModel.LicenseUsageMode" /> which specifies when you can use the licensed object for the <see cref="P:System.ComponentModel.LicenseManager.CurrentContext" />.</summary>
	/// <returns>One of the <see cref="T:System.ComponentModel.LicenseUsageMode" /> values, as specified in the <see cref="P:System.ComponentModel.LicenseManager.CurrentContext" /> property.</returns>
	public static LicenseUsageMode UsageMode
	{
		get
		{
			if (context != null)
			{
				return context.UsageMode;
			}
			return LicenseUsageMode.Runtime;
		}
	}

	private LicenseManager()
	{
	}

	private static void CacheProvider(Type type, LicenseProvider provider)
	{
		if (providers == null)
		{
			providers = new Hashtable();
		}
		providers[type] = provider;
		if (provider != null)
		{
			if (providerInstances == null)
			{
				providerInstances = new Hashtable();
			}
			providerInstances[provider.GetType()] = provider;
		}
	}

	/// <summary>Creates an instance of the specified type, given a context in which you can use the licensed instance.</summary>
	/// <returns>An instance of the specified type.</returns>
	/// <param name="type">A <see cref="T:System.Type" /> that represents the type to create. </param>
	/// <param name="creationContext">A <see cref="T:System.ComponentModel.LicenseContext" /> that specifies when you can use the licensed instance. </param>
	public static object CreateWithContext(Type type, LicenseContext creationContext)
	{
		return CreateWithContext(type, creationContext, new object[0]);
	}

	/// <summary>Creates an instance of the specified type with the specified arguments, given a context in which you can use the licensed instance.</summary>
	/// <returns>An instance of the specified type with the given array of arguments.</returns>
	/// <param name="type">A <see cref="T:System.Type" /> that represents the type to create. </param>
	/// <param name="creationContext">A <see cref="T:System.ComponentModel.LicenseContext" /> that specifies when you can use the licensed instance. </param>
	/// <param name="args">An array of type <see cref="T:System.Object" /> that represents the arguments for the type. </param>
	public static object CreateWithContext(Type type, LicenseContext creationContext, object[] args)
	{
		object obj = null;
		lock (internalSyncObject)
		{
			LicenseContext currentContext = CurrentContext;
			try
			{
				CurrentContext = creationContext;
				LockContext(selfLock);
				try
				{
					return SecurityUtils.SecureCreateInstance(type, args);
				}
				catch (TargetInvocationException ex)
				{
					throw ex.InnerException;
				}
			}
			finally
			{
				UnlockContext(selfLock);
				CurrentContext = currentContext;
			}
		}
	}

	private static bool GetCachedNoLicenseProvider(Type type)
	{
		if (providers != null)
		{
			return providers.ContainsKey(type);
		}
		return false;
	}

	private static LicenseProvider GetCachedProvider(Type type)
	{
		if (providers != null)
		{
			return (LicenseProvider)providers[type];
		}
		return null;
	}

	private static LicenseProvider GetCachedProviderInstance(Type providerType)
	{
		if (providerInstances != null)
		{
			return (LicenseProvider)providerInstances[providerType];
		}
		return null;
	}

	private static IntPtr GetLicenseInteropHelperType()
	{
		return typeof(LicenseInteropHelper).TypeHandle.Value;
	}

	/// <summary>Returns whether the given type has a valid license.</summary>
	/// <returns>true if the given type is licensed; otherwise, false.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> to find a valid license for. </param>
	public static bool IsLicensed(Type type)
	{
		License license;
		bool result = ValidateInternal(type, null, allowExceptions: false, out license);
		if (license != null)
		{
			license.Dispose();
			license = null;
		}
		return result;
	}

	/// <summary>Determines whether a valid license can be granted for the specified type.</summary>
	/// <returns>true if a valid license can be granted; otherwise, false.</returns>
	/// <param name="type">A <see cref="T:System.Type" /> that represents the type of object that requests the <see cref="T:System.ComponentModel.License" />. </param>
	public static bool IsValid(Type type)
	{
		License license;
		bool result = ValidateInternal(type, null, allowExceptions: false, out license);
		if (license != null)
		{
			license.Dispose();
			license = null;
		}
		return result;
	}

	/// <summary>Determines whether a valid license can be granted for the specified instance of the type. This method creates a valid <see cref="T:System.ComponentModel.License" />.</summary>
	/// <returns>true if a valid <see cref="T:System.ComponentModel.License" /> can be granted; otherwise, false.</returns>
	/// <param name="type">A <see cref="T:System.Type" /> that represents the type of object that requests the license. </param>
	/// <param name="instance">An object of the specified type or a type derived from the specified type. </param>
	/// <param name="license">A <see cref="T:System.ComponentModel.License" /> that is a valid license, or null if a valid license cannot be granted. </param>
	public static bool IsValid(Type type, object instance, out License license)
	{
		return ValidateInternal(type, instance, allowExceptions: false, out license);
	}

	/// <summary>Prevents changes being made to the current <see cref="T:System.ComponentModel.LicenseContext" /> of the given object.</summary>
	/// <param name="contextUser">The object whose current context you want to lock. </param>
	/// <exception cref="T:System.InvalidOperationException">The context is already locked.</exception>
	public static void LockContext(object contextUser)
	{
		lock (internalSyncObject)
		{
			if (contextLockHolder != null)
			{
				throw new InvalidOperationException(global::SR.GetString("The CurrentContext property of the LicenseManager is already locked by another user."));
			}
			contextLockHolder = contextUser;
		}
	}

	/// <summary>Allows changes to be made to the current <see cref="T:System.ComponentModel.LicenseContext" /> of the given object.</summary>
	/// <param name="contextUser">The object whose current context you want to unlock. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="contextUser" /> represents a different user than the one specified in a previous call to <see cref="M:System.ComponentModel.LicenseManager.LockContext(System.Object)" />. </exception>
	public static void UnlockContext(object contextUser)
	{
		lock (internalSyncObject)
		{
			if (contextLockHolder != contextUser)
			{
				throw new ArgumentException(global::SR.GetString("The CurrentContext property of the LicenseManager can only be unlocked with the same contextUser."));
			}
			contextLockHolder = null;
		}
	}

	private static bool ValidateInternal(Type type, object instance, bool allowExceptions, out License license)
	{
		string licenseKey;
		return ValidateInternalRecursive(CurrentContext, type, instance, allowExceptions, out license, out licenseKey);
	}

	private static bool ValidateInternalRecursive(LicenseContext context, Type type, object instance, bool allowExceptions, out License license, out string licenseKey)
	{
		LicenseProvider licenseProvider = GetCachedProvider(type);
		if (licenseProvider == null && !GetCachedNoLicenseProvider(type))
		{
			LicenseProviderAttribute licenseProviderAttribute = (LicenseProviderAttribute)Attribute.GetCustomAttribute(type, typeof(LicenseProviderAttribute), inherit: false);
			if (licenseProviderAttribute != null)
			{
				Type licenseProvider2 = licenseProviderAttribute.LicenseProvider;
				licenseProvider = GetCachedProviderInstance(licenseProvider2);
				if (licenseProvider == null)
				{
					licenseProvider = (LicenseProvider)SecurityUtils.SecureCreateInstance(licenseProvider2);
				}
			}
			CacheProvider(type, licenseProvider);
		}
		license = null;
		bool flag = true;
		licenseKey = null;
		if (licenseProvider != null)
		{
			license = licenseProvider.GetLicense(context, type, instance, allowExceptions);
			if (license == null)
			{
				flag = false;
			}
			else
			{
				licenseKey = license.LicenseKey;
			}
		}
		if (flag && instance == null)
		{
			Type baseType = type.BaseType;
			if (baseType != typeof(object) && baseType != null)
			{
				if (license != null)
				{
					license.Dispose();
					license = null;
				}
				flag = ValidateInternalRecursive(context, baseType, null, allowExceptions, out license, out var _);
				if (license != null)
				{
					license.Dispose();
					license = null;
				}
			}
		}
		return flag;
	}

	/// <summary>Determines whether a license can be granted for the specified type.</summary>
	/// <param name="type">A <see cref="T:System.Type" /> that represents the type of object that requests the license. </param>
	/// <exception cref="T:System.ComponentModel.LicenseException">A <see cref="T:System.ComponentModel.License" /> cannot be granted. </exception>
	public static void Validate(Type type)
	{
		if (!ValidateInternal(type, null, allowExceptions: true, out var license))
		{
			throw new LicenseException(type);
		}
		if (license != null)
		{
			license.Dispose();
			license = null;
		}
	}

	/// <summary>Determines whether a license can be granted for the instance of the specified type.</summary>
	/// <returns>A valid <see cref="T:System.ComponentModel.License" />.</returns>
	/// <param name="type">A <see cref="T:System.Type" /> that represents the type of object that requests the license. </param>
	/// <param name="instance">An <see cref="T:System.Object" /> of the specified type or a type derived from the specified type. </param>
	/// <exception cref="T:System.ComponentModel.LicenseException">The type is licensed, but a <see cref="T:System.ComponentModel.License" /> cannot be granted. </exception>
	public static License Validate(Type type, object instance)
	{
		if (!ValidateInternal(type, instance, allowExceptions: true, out var license))
		{
			throw new LicenseException(type, instance);
		}
		return license;
	}
}
