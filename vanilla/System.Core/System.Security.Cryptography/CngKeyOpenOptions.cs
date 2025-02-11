namespace System.Security.Cryptography;

/// <summary>Specifies options for opening a key.</summary>
[Flags]
public enum CngKeyOpenOptions
{
	/// <summary>No key open options are specified.</summary>
	None = 0,
	/// <summary>If the <see cref="F:System.Security.Cryptography.CngKeyOpenOptions.MachineKey" /> value is not specified, a user key is opened instead.</summary>
	UserKey = 0,
	/// <summary>A machine-wide key is opened.</summary>
	MachineKey = 0x20,
	/// <summary>UI prompting is suppressed.</summary>
	Silent = 0x40
}
