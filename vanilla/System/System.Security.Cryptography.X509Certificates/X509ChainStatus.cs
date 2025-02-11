namespace System.Security.Cryptography.X509Certificates;

/// <summary>Provides a simple structure for storing X509 chain status and error information.</summary>
public struct X509ChainStatus
{
	private X509ChainStatusFlags status;

	private string info;

	/// <summary>Specifies the status of the X509 chain.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509ChainStatusFlags" /> value.</returns>
	public X509ChainStatusFlags Status
	{
		get
		{
			return status;
		}
		set
		{
			status = value;
		}
	}

	/// <summary>Specifies a description of the <see cref="P:System.Security.Cryptography.X509Certificates.X509Chain.ChainStatus" /> value.</summary>
	/// <returns>A localizable string.</returns>
	public string StatusInformation
	{
		get
		{
			return info;
		}
		set
		{
			info = value;
		}
	}

	internal X509ChainStatus(X509ChainStatusFlags flag)
	{
		status = flag;
		info = GetInformation(flag);
	}

	internal static string GetInformation(X509ChainStatusFlags flags)
	{
		switch (flags)
		{
		case X509ChainStatusFlags.NotTimeValid:
		case X509ChainStatusFlags.NotTimeNested:
		case X509ChainStatusFlags.Revoked:
		case X509ChainStatusFlags.NotSignatureValid:
		case X509ChainStatusFlags.NotValidForUsage:
		case X509ChainStatusFlags.UntrustedRoot:
		case X509ChainStatusFlags.RevocationStatusUnknown:
		case X509ChainStatusFlags.Cyclic:
		case X509ChainStatusFlags.InvalidExtension:
		case X509ChainStatusFlags.InvalidPolicyConstraints:
		case X509ChainStatusFlags.InvalidBasicConstraints:
		case X509ChainStatusFlags.InvalidNameConstraints:
		case X509ChainStatusFlags.HasNotSupportedNameConstraint:
		case X509ChainStatusFlags.HasNotDefinedNameConstraint:
		case X509ChainStatusFlags.HasNotPermittedNameConstraint:
		case X509ChainStatusFlags.HasExcludedNameConstraint:
		case X509ChainStatusFlags.PartialChain:
		case X509ChainStatusFlags.CtlNotTimeValid:
		case X509ChainStatusFlags.CtlNotSignatureValid:
		case X509ChainStatusFlags.CtlNotValidForUsage:
		case X509ChainStatusFlags.OfflineRevocation:
		case X509ChainStatusFlags.NoIssuanceChainPolicy:
			return global::Locale.GetText(flags.ToString());
		default:
			return string.Empty;
		}
	}
}
