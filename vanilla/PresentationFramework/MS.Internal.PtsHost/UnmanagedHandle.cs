using System;

namespace MS.Internal.PtsHost;

internal class UnmanagedHandle : IDisposable
{
	private nint _handle;

	private readonly PtsContext _ptsContext;

	internal nint Handle => _handle;

	internal PtsContext PtsContext => _ptsContext;

	protected UnmanagedHandle(PtsContext ptsContext)
	{
		_ptsContext = ptsContext;
		_handle = ptsContext.CreateHandle(this);
	}

	public virtual void Dispose()
	{
		try
		{
			_ptsContext.ReleaseHandle(_handle);
		}
		finally
		{
			_handle = IntPtr.Zero;
		}
		GC.SuppressFinalize(this);
	}
}
