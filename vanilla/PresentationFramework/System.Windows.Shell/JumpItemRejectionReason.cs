namespace System.Windows.Shell;

/// <summary>Describes why a <see cref="T:System.Windows.Shell.JumpItem" /> could not be added to the Jump List by the Windows shell.</summary>
public enum JumpItemRejectionReason
{
	/// <summary>The reason is not specified.</summary>
	None,
	/// <summary>The <see cref="T:System.Windows.Shell.JumpItem" /> references an invalid file path, or the operating system does not support Jump Lists.</summary>
	InvalidItem,
	/// <summary>The application is not registered to handle the file name extension of the <see cref="T:System.Windows.Shell.JumpItem" />.</summary>
	NoRegisteredHandler,
	/// <summary>The item was previously in the Jump List but was removed by the user.</summary>
	RemovedByUser
}
