using System.Security;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using Unity;

namespace System.IO.MemoryMappedFiles;

/// <summary>Represents a randomly accessed view of a memory-mapped file.</summary>
public sealed class MemoryMappedViewAccessor : UnmanagedMemoryAccessor
{
	private MemoryMappedView m_view;

	/// <summary>Gets a handle to the view of a memory-mapped file.</summary>
	/// <returns>A wrapper for the operating system's handle to the view of the file. </returns>
	public SafeMemoryMappedViewHandle SafeMemoryMappedViewHandle
	{
		[SecurityCritical]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		get
		{
			if (m_view == null)
			{
				return null;
			}
			return m_view.ViewHandle;
		}
	}

	/// <summary>Gets the number of bytes by which the starting position of this view is offset from the beginning of the memory-mapped file. </summary>
	/// <returns>The number of bytes between the starting position of this view and the beginning of the memory-mapped file. </returns>
	/// <exception cref="T:System.InvalidOperationException">The object from which this instance was created is null. </exception>
	public long PointerOffset
	{
		get
		{
			if (m_view == null)
			{
				throw new InvalidOperationException(global::SR.GetString("The underlying MemoryMappedView object is null."));
			}
			return m_view.PointerOffset;
		}
	}

	[SecurityCritical]
	internal MemoryMappedViewAccessor(MemoryMappedView view)
	{
		m_view = view;
		Initialize(m_view.ViewHandle, m_view.PointerOffset, m_view.Size, MemoryMappedFile.GetFileAccess(m_view.Access));
	}

	[SecuritySafeCritical]
	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && m_view != null && !m_view.IsClosed)
			{
				Flush();
			}
		}
		finally
		{
			try
			{
				if (m_view != null)
				{
					m_view.Dispose();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
	}

	/// <summary>Clears all buffers for this view and causes any buffered data to be written to the underlying file.</summary>
	/// <exception cref="T:System.ObjectDisposedException">Methods were called after the accessor was closed.</exception>
	[SecurityCritical]
	public void Flush()
	{
		if (!base.IsOpen)
		{
			throw new ObjectDisposedException("MemoryMappedViewAccessor", global::SR.GetString("Cannot access a closed accessor."));
		}
		if (m_view != null)
		{
			m_view.Flush((IntPtr)base.Capacity);
		}
	}

	internal MemoryMappedViewAccessor()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
