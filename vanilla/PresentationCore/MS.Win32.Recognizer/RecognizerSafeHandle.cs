using System;
using System.Runtime.InteropServices;
using MS.Internal;

namespace MS.Win32.Recognizer;

internal class RecognizerSafeHandle : SafeHandle
{
	public override bool IsInvalid
	{
		get
		{
			if (!base.IsClosed)
			{
				return handle == IntPtr.Zero;
			}
			return true;
		}
	}

	private RecognizerSafeHandle()
		: this(ownHandle: true)
	{
	}

	private RecognizerSafeHandle(bool ownHandle)
		: base(IntPtr.Zero, ownHandle)
	{
	}

	protected override bool ReleaseHandle()
	{
		return HRESULT.Succeeded(UnsafeNativeMethods.DestroyRecognizer(handle));
	}
}
