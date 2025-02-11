using System;
using System.Runtime.InteropServices;
using WinRT;
using WinRT.Interop;

namespace ABI.WinRT.Interop;

[Guid("04a2dbf3-df83-116c-0946-0812abf6e07d")]
internal class ILanguageExceptionErrorInfo : global::WinRT.Interop.ILanguageExceptionErrorInfo
{
	[Guid("04a2dbf3-df83-116c-0946-0812abf6e07d")]
	internal struct Vftbl
	{
		internal delegate int _GetLanguageException(nint thisPtr, out nint languageException);

		public IUnknownVftbl IUnknownVftbl;

		public _GetLanguageException GetLanguageException_0;
	}

	protected readonly ObjectReference<Vftbl> _obj;

	public nint ThisPtr => _obj.ThisPtr;

	public static ObjectReference<Vftbl> FromAbi(nint thisPtr)
	{
		return ObjectReference<Vftbl>.FromAbi(thisPtr);
	}

	public static implicit operator ILanguageExceptionErrorInfo(IObjectReference obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new ILanguageExceptionErrorInfo(obj);
	}

	public static implicit operator ILanguageExceptionErrorInfo(ObjectReference<Vftbl> obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new ILanguageExceptionErrorInfo(obj);
	}

	public ObjectReference<I> AsInterface<I>()
	{
		return _obj.As<I>();
	}

	public A As<A>()
	{
		return _obj.AsType<A>();
	}

	public ILanguageExceptionErrorInfo(IObjectReference obj)
		: this(obj.As<Vftbl>())
	{
	}

	public ILanguageExceptionErrorInfo(ObjectReference<Vftbl> obj)
	{
		_obj = obj;
	}

	public IObjectReference GetLanguageException()
	{
		nint languageException = IntPtr.Zero;
		try
		{
			Marshal.ThrowExceptionForHR(_obj.Vftbl.GetLanguageException_0(ThisPtr, out languageException));
			return ObjectReference<IUnknownVftbl>.Attach(ref languageException);
		}
		finally
		{
			using (ObjectReference<IUnknownVftbl>.Attach(ref languageException))
			{
			}
		}
	}
}
