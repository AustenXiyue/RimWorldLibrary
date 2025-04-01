using System.Runtime.CompilerServices;
using System.Security.Permissions;
using Unity;

namespace System.Security.Cryptography;

/// <summary>Provides simple data protection methods.</summary>
public sealed class DpapiDataProtector : DataProtector
{
	protected override bool PrependHashedPurposeToPlaintext
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(bool);
		}
	}

	/// <summary>Gets or sets the scope of the data protection.</summary>
	/// <returns>One of the enumeration values that specifies the scope of the data protection (either the current user or the local machine). The default is <see cref="F:System.Security.Cryptography.DataProtectionScope.CurrentUser" />.</returns>
	public DataProtectionScope Scope
	{
		[CompilerGenerated]
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(DataProtectionScope);
		}
		[CompilerGenerated]
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Security.Cryptography.DpapiDataProtector" /> class by using the specified application name, primary purpose, and specific purposes.</summary>
	/// <param name="appName">The name of the application.</param>
	/// <param name="primaryPurpose">The primary purpose for the data protector.</param>
	/// <param name="specificPurpose">The specific purpose(s) for the data protector.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="appName" /> is an empty string or null.-or-<paramref name="primaryPurpose" /> is an empty string or null.-or-<paramref name="specificPurposes" /> contains an empty string or null.</exception>
	[SecuritySafeCritical]
	[DataProtectionPermission(SecurityAction.Demand, Unrestricted = true)]
	public DpapiDataProtector(string appName, string primaryPurpose, string[] specificPurpose)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Determines if the data must be re-encrypted.</summary>
	/// <returns>true if the data must be re-encrypted; otherwise, false.</returns>
	/// <param name="encryptedData">The encrypted data to be checked.</param>
	public override bool IsReprotectRequired(byte[] encryptedData)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return default(bool);
	}

	[SecuritySafeCritical]
	[DataProtectionPermission(SecurityAction.Assert, ProtectData = true)]
	protected override byte[] ProviderProtect(byte[] userData)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	[SecuritySafeCritical]
	[DataProtectionPermission(SecurityAction.Assert, UnprotectData = true)]
	protected override byte[] ProviderUnprotect(byte[] encryptedData)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}
}
