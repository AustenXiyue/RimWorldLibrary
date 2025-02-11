using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.Windows.Foundation.Collections;
using WinRT;

namespace MS.Internal.WindowsRuntime.ABI.System.Collections.Generic;

[Guid("FAA585EA-6214-4217-AFDA-7F46DE5869B3")]
internal class IEnumerable<T> : global::System.Collections.Generic.IEnumerable<T>, IEnumerable, IIterable<T>
{
	public class FromAbiHelper : global::System.Collections.Generic.IEnumerable<T>, IEnumerable
	{
		private readonly IEnumerable<T> _iterable;

		public FromAbiHelper(IObjectReference obj)
			: this(new IEnumerable<T>(obj))
		{
		}

		public FromAbiHelper(IEnumerable<T> iterable)
		{
			_iterable = iterable;
		}

		public global::System.Collections.Generic.IEnumerator<T> GetEnumerator()
		{
			if (((IIterable<T>)_iterable).First() is IEnumerator<T> result)
			{
				return result;
			}
			throw new InvalidOperationException("Unexpected type for enumerator");
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	internal sealed class ToAbiHelper : IIterable<T>
	{
		private readonly IEnumerable<T> m_enumerable;

		internal ToAbiHelper(IEnumerable<T> enumerable)
		{
			m_enumerable = enumerable;
		}

		public IIterator<T> First()
		{
			return new IEnumerator<T>.ToAbiHelper(m_enumerable.GetEnumerator());
		}
	}

	[Guid("FAA585EA-6214-4217-AFDA-7F46DE5869B3")]
	public struct Vftbl
	{
		internal IInspectable.Vftbl IInspectableVftbl;

		public IEnumerable_Delegates.First_0 First_0;

		public static Guid PIID;

		private static readonly Vftbl AbiToProjectionVftable;

		public static readonly nint AbiToProjectionVftablePtr;

		internal unsafe Vftbl(nint thisPtr)
		{
			VftblPtr vftblPtr = Marshal.PtrToStructure<VftblPtr>(thisPtr);
			nint* vftbl = (nint*)vftblPtr.Vftbl;
			IInspectableVftbl = Marshal.PtrToStructure<IInspectable.Vftbl>(vftblPtr.Vftbl);
			First_0 = Marshal.GetDelegateForFunctionPointer<IEnumerable_Delegates.First_0>(vftbl[6]);
		}

		unsafe static Vftbl()
		{
			PIID = GuidGenerator.CreateIID(typeof(IEnumerable<T>));
			AbiToProjectionVftable = new Vftbl
			{
				IInspectableVftbl = IInspectable.Vftbl.AbiToProjectionVftable,
				First_0 = Do_Abi_First_0
			};
			nint* ptr = (nint*)Marshal.AllocCoTaskMem(Marshal.SizeOf<IInspectable.Vftbl>() + sizeof(nint));
			Marshal.StructureToPtr(AbiToProjectionVftable.IInspectableVftbl, (nint)ptr, fDeleteOld: false);
			ptr[6] = Marshal.GetFunctionPointerForDelegate(AbiToProjectionVftable.First_0);
			AbiToProjectionVftablePtr = (nint)ptr;
		}

		private static int Do_Abi_First_0(nint thisPtr, out nint __return_value__)
		{
			__return_value__ = 0;
			try
			{
				global::System.Collections.Generic.IEnumerable<T> enumerable = ComWrappersSupport.FindObject<global::System.Collections.Generic.IEnumerable<T>>(thisPtr);
				__return_value__ = MarshalInterface<global::System.Collections.Generic.IEnumerator<T>>.FromManaged(enumerable.GetEnumerator());
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}
	}

	public static Guid PIID = Vftbl.PIID;

	protected readonly ObjectReference<Vftbl> _obj;

	private FromAbiHelper _FromIterable;

	public IObjectReference ObjRef => _obj;

	public nint ThisPtr => _obj.ThisPtr;

	public static IObjectReference CreateMarshaler(global::System.Collections.Generic.IEnumerable<T> obj)
	{
		if (obj != null)
		{
			return ComWrappersSupport.CreateCCWForObject(obj).As<Vftbl>(GuidGenerator.GetIID(typeof(IEnumerable<T>)));
		}
		return null;
	}

	public static nint GetAbi(IObjectReference objRef)
	{
		return objRef?.ThisPtr ?? IntPtr.Zero;
	}

	public static global::System.Collections.Generic.IEnumerable<T> FromAbi(nint thisPtr)
	{
		if (thisPtr != IntPtr.Zero)
		{
			return new IEnumerable<T>(ObjRefFromAbi(thisPtr));
		}
		return null;
	}

	public static nint FromManaged(global::System.Collections.Generic.IEnumerable<T> value)
	{
		if (value != null)
		{
			return CreateMarshaler(value).GetRef();
		}
		return IntPtr.Zero;
	}

	public static void DisposeMarshaler(IObjectReference objRef)
	{
		objRef?.Dispose();
	}

	public static void DisposeAbi(nint abi)
	{
		MarshalInterfaceHelper<IIterable<T>>.DisposeAbi(abi);
	}

	public static string GetGuidSignature()
	{
		return GuidGenerator.GetSignature(typeof(IEnumerable<T>));
	}

	public static ObjectReference<Vftbl> ObjRefFromAbi(nint thisPtr)
	{
		if (thisPtr == IntPtr.Zero)
		{
			return null;
		}
		Vftbl vftblT = new Vftbl(thisPtr);
		return ObjectReference<Vftbl>.FromAbi(thisPtr, vftblT.IInspectableVftbl.IUnknownVftbl, vftblT);
	}

	public static implicit operator IEnumerable<T>(IObjectReference obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new IEnumerable<T>(obj);
	}

	public static implicit operator IEnumerable<T>(ObjectReference<Vftbl> obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new IEnumerable<T>(obj);
	}

	public ObjectReference<I> AsInterface<I>()
	{
		return _obj.As<I>();
	}

	public A As<A>()
	{
		return _obj.AsType<A>();
	}

	public IEnumerable(IObjectReference obj)
		: this(obj.As<Vftbl>())
	{
	}

	public IEnumerable(ObjectReference<Vftbl> obj)
	{
		_obj = obj;
		_FromIterable = new FromAbiHelper(this);
	}

	IIterator<T> IIterable<T>.First()
	{
		nint __return_value__ = 0;
		try
		{
			ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.First_0(ThisPtr, out __return_value__));
			return IEnumerator<T>.FromAbiInternal(__return_value__);
		}
		finally
		{
			IEnumerator<T>.DisposeAbi(__return_value__);
		}
	}

	public global::System.Collections.Generic.IEnumerator<T> GetEnumerator()
	{
		return _FromIterable.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
