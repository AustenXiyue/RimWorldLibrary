using Microsoft.Win32.SafeHandles;

namespace System.Security.Authentication.ExtendedProtection;

/// <summary>The <see cref="T:System.Security.Authentication.ExtendedProtection.ChannelBinding" /> class encapsulates a pointer to the opaque data used to bind an authenticated transaction to a secure channel.</summary>
public abstract class ChannelBinding : SafeHandleZeroOrMinusOneIsInvalid
{
	/// <summary>The <see cref="P:System.Security.Authentication.ExtendedProtection.ChannelBinding.Size" /> property gets the size, in bytes, of the channel binding token associated with the <see cref="T:System.Security.Authentication.ExtendedProtection.ChannelBinding" /> instance.</summary>
	/// <returns>The size, in bytes, of the channel binding token in the <see cref="T:System.Security.Authentication.ExtendedProtection.ChannelBinding" /> instance.</returns>
	public abstract int Size { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Authentication.ExtendedProtection.ChannelBinding" /> class.</summary>
	protected ChannelBinding()
		: this(ownsHandle: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Authentication.ExtendedProtection.ChannelBinding" /> class.</summary>
	/// <param name="ownsHandle">A Boolean value that indicates if the application owns the safe handle to a native memory region containing the byte data that would be passed to native calls that provide extended protection for integrated windows authentication.</param>
	protected ChannelBinding(bool ownsHandle)
		: base(ownsHandle)
	{
	}
}
