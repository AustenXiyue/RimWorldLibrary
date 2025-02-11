using System;
using System.Reflection;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace WinRT;

internal abstract class IObjectReference : IDisposable
{
	protected bool disposed;

	private readonly nint _thisPtr;

	public nint ThisPtr
	{
		get
		{
			ThrowIfDisposed();
			return _thisPtr;
		}
	}

	protected IUnknownVftbl VftblIUnknown
	{
		get
		{
			ThrowIfDisposed();
			return VftblIUnknownUnsafe;
		}
	}

	protected virtual IUnknownVftbl VftblIUnknownUnsafe { get; }

	public bool IsReferenceToManagedObject
	{
		get
		{
			using ObjectReference<IUnknownVftbl> objectReference = As<IUnknownVftbl>();
			return objectReference.VftblIUnknown.Equals(IUnknownVftbl.AbiToProjectionVftbl);
		}
	}

	protected IObjectReference(nint thisPtr)
	{
		if (thisPtr == IntPtr.Zero)
		{
			throw new ArgumentNullException("thisPtr");
		}
		_thisPtr = thisPtr;
	}

	~IObjectReference()
	{
		Dispose(disposing: false);
	}

	public ObjectReference<T> As<T>()
	{
		return As<T>(GuidGenerator.GetIID(typeof(T)));
	}

	public ObjectReference<T> As<T>(Guid iid)
	{
		ThrowIfDisposed();
		Marshal.ThrowExceptionForHR(VftblIUnknown.QueryInterface(ThisPtr, ref iid, out var vftbl));
		return ObjectReference<T>.Attach(ref vftbl);
	}

	public int TryAs<T>(out ObjectReference<T> objRef)
	{
		return TryAs(GuidGenerator.GetIID(typeof(T)), out objRef);
	}

	public virtual int TryAs<T>(Guid iid, out ObjectReference<T> objRef)
	{
		objRef = null;
		ThrowIfDisposed();
		nint vftbl;
		int num = VftblIUnknown.QueryInterface(ThisPtr, ref iid, out vftbl);
		if (num >= 0)
		{
			objRef = ObjectReference<T>.Attach(ref vftbl);
		}
		return num;
	}

	public IObjectReference As(Guid iid)
	{
		return As<IUnknownVftbl>(iid);
	}

	public T AsType<T>()
	{
		ThrowIfDisposed();
		ConstructorInfo constructor = typeof(T).GetConstructor(new Type[1] { typeof(IObjectReference) });
		if (constructor != null)
		{
			object[] parameters = new IObjectReference[1] { this };
			return (T)constructor.Invoke(parameters);
		}
		throw new InvalidOperationException("Target type is not a projected interface.");
	}

	public nint GetRef()
	{
		ThrowIfDisposed();
		AddRef();
		return ThisPtr;
	}

	protected void ThrowIfDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException("ObjectReference");
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposed)
		{
			Release();
			disposed = true;
		}
	}

	protected virtual void AddRef()
	{
		VftblIUnknown.AddRef(ThisPtr);
	}

	protected virtual void Release()
	{
		IUnknownVftbl._Release release = VftblIUnknown.Release;
		if (release == null)
		{
			release = Marshal.PtrToStructure<IUnknownVftbl>(Marshal.PtrToStructure<VftblPtr>(ThisPtr).Vftbl).Release;
		}
		release(ThisPtr);
	}
}
