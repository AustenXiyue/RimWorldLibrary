using System;
using System.Runtime.InteropServices;
using MS.Internal;

namespace MS.Win32.Recognizer;

internal class ContextSafeHandle : SafeHandle
{
	private RecognizerSafeHandle _recognizerHandle;

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

	private ContextSafeHandle()
		: this(ownHandle: true)
	{
	}

	private ContextSafeHandle(bool ownHandle)
		: base(IntPtr.Zero, ownHandle)
	{
	}

	protected override bool ReleaseHandle()
	{
		int hr = UnsafeNativeMethods.DestroyContext(handle);
		_recognizerHandle = null;
		return HRESULT.Succeeded(hr);
	}

	internal void AddReferenceOnRecognizer(RecognizerSafeHandle handle)
	{
		_recognizerHandle = handle;
	}
}
