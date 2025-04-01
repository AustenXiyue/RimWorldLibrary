using System.Security;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using Unity;

namespace System.IO.MemoryMappedFiles;

/// <summary>Represents a view of a memory-mapped file as a sequentially accessed stream.</summary>
public sealed class MemoryMappedViewStream : UnmanagedMemoryStream
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

	/// <summary>Gets the number of bytes by which the starting position of this view is offset from the beginning of the memory-mapped file.</summary>
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
	internal MemoryMappedViewStream(MemoryMappedView view)
	{
		m_view = view;
		Initialize(m_view.ViewHandle, m_view.PointerOffset, m_view.Size, MemoryMappedFile.GetFileAccess(m_view.Access));
	}

	/// <summary>Sets the length of the current stream.</summary>
	/// <param name="value">The desired length of the current stream in bytes.</param>
	/// <exception cref="T:System.NotSupportedException">This method is not supported.</exception>
	public override void SetLength(long value)
	{
		throw new NotSupportedException(global::SR.GetString("MemoryMappedViewStreams are fixed length."));
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

	/// <summary>Clears all buffers for this stream and causes any buffered data to be written to the underlying file.</summary>
	[SecurityCritical]
	public override void Flush()
	{
		if (!CanSeek)
		{
			__Error.StreamIsClosed();
		}
		if (m_view != null)
		{
			m_view.Flush((IntPtr)base.Capacity);
		}
	}

	internal MemoryMappedViewStream()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
