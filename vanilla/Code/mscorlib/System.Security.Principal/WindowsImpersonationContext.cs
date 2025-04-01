using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity;

namespace System.Security.Principal;

/// <summary>Represents the Windows user prior to an impersonation operation. </summary>
[ComVisible(true)]
public class WindowsImpersonationContext : IDisposable
{
	private IntPtr _token;

	private bool undo;

	internal WindowsImpersonationContext(IntPtr token)
	{
		_token = DuplicateToken(token);
		if (!SetCurrentToken(token))
		{
			throw new SecurityException("Couldn't impersonate token.");
		}
		undo = false;
	}

	/// <summary>Releases all resources used by the <see cref="T:System.Security.Principal.WindowsImpersonationContext" />.</summary>
	[ComVisible(false)]
	public void Dispose()
	{
		if (!undo)
		{
			Undo();
		}
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Security.Principal.WindowsImpersonationContext" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	[ComVisible(false)]
	protected virtual void Dispose(bool disposing)
	{
		if (!undo)
		{
			Undo();
		}
		if (disposing)
		{
			GC.SuppressFinalize(this);
		}
	}

	/// <summary>Reverts the user context to the Windows user represented by this object.</summary>
	/// <exception cref="T:System.Security.SecurityException">An attempt is made to use this method for any purpose other than to revert identity to self. </exception>
	public void Undo()
	{
		if (!RevertToSelf())
		{
			CloseToken(_token);
			throw new SecurityException("Couldn't switch back to original token.");
		}
		CloseToken(_token);
		undo = true;
		GC.SuppressFinalize(this);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool CloseToken(IntPtr token);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern IntPtr DuplicateToken(IntPtr token);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool SetCurrentToken(IntPtr token);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool RevertToSelf();

	internal WindowsImpersonationContext()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
