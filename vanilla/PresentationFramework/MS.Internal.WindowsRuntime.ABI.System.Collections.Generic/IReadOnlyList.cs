using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.Windows.Foundation.Collections;
using WinRT;
using WinRT.Interop;

namespace MS.Internal.WindowsRuntime.ABI.System.Collections.Generic;

[Guid("BBE1FA4C-B0E3-4583-BAEF-1F1B2E483E56")]
internal class IReadOnlyList<T> : global::System.Collections.Generic.IReadOnlyList<T>, global::System.Collections.Generic.IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>
{
	public class FromAbiHelper : global::System.Collections.Generic.IReadOnlyList<T>, global::System.Collections.Generic.IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>
	{
		private readonly IReadOnlyList<T> _vectorView;

		private readonly IEnumerable<T> _enumerable;

		public int Count
		{
			get
			{
				uint size = _vectorView.Size;
				if (int.MaxValue < size)
				{
					throw new InvalidOperationException(ErrorStrings.InvalidOperation_CollectionBackingListTooLarge);
				}
				return (int)size;
			}
		}

		public T this[int index] => Indexer_Get(index);

		public FromAbiHelper(IObjectReference obj)
			: this(new IReadOnlyList<T>(obj))
		{
		}

		public FromAbiHelper(IReadOnlyList<T> vectorView)
		{
			_vectorView = vectorView;
			_enumerable = new IEnumerable<T>(vectorView.ObjRef);
		}

		private T Indexer_Get(int index)
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			try
			{
				return _vectorView.GetAt((uint)index);
			}
			catch (Exception ex)
			{
				if (-2147483637 == ex.HResult)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				throw;
			}
		}

		public global::System.Collections.Generic.IEnumerator<T> GetEnumerator()
		{
			return _enumerable.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public sealed class ToAbiHelper : IVectorView<T>, IIterable<T>
	{
		private readonly global::System.Collections.Generic.IReadOnlyList<T> _list;

		public uint Size => (uint)_list.Count;

		internal ToAbiHelper(global::System.Collections.Generic.IReadOnlyList<T> list)
		{
			_list = list;
		}

		IIterator<T> IIterable<T>.First()
		{
			return new IEnumerator<T>.ToAbiHelper(_list.GetEnumerator());
		}

		private static void EnsureIndexInt32(uint index, int limit = int.MaxValue)
		{
			if (int.MaxValue <= index || index >= (uint)limit)
			{
				ArgumentOutOfRangeException ex = new ArgumentOutOfRangeException("index", ErrorStrings.ArgumentOutOfRange_IndexLargerThanMaxValue);
				ex.SetHResult(-2147483637);
				throw ex;
			}
		}

		public T GetAt(uint index)
		{
			EnsureIndexInt32(index, _list.Count);
			try
			{
				return _list[(int)index];
			}
			catch (ArgumentOutOfRangeException ex)
			{
				ex.SetHResult(-2147483637);
				throw;
			}
		}

		public bool IndexOf(T value, out uint index)
		{
			int num = -1;
			int count = _list.Count;
			for (int i = 0; i < count; i++)
			{
				if (EqualityComparer<T>.Default.Equals(value, _list[i]))
				{
					num = i;
					break;
				}
			}
			if (-1 == num)
			{
				index = 0u;
				return false;
			}
			index = (uint)num;
			return true;
		}

		public uint GetMany(uint startIndex, ref T[] items)
		{
			if (startIndex == _list.Count)
			{
				return 0u;
			}
			EnsureIndexInt32(startIndex, _list.Count);
			if (items == null)
			{
				return 0u;
			}
			uint num = Math.Min((uint)items.Length, (uint)_list.Count - startIndex);
			for (uint num2 = 0u; num2 < num; num2++)
			{
				items[num2] = _list[(int)(num2 + startIndex)];
			}
			if (typeof(T) == typeof(string))
			{
				string[] array = items as string[];
				for (uint num3 = num; num3 < items.Length; num3++)
				{
					array[num3] = string.Empty;
				}
			}
			return num;
		}
	}

	[Guid("BBE1FA4C-B0E3-4583-BAEF-1F1B2E483E56")]
	public struct Vftbl
	{
		internal IInspectable.Vftbl IInspectableVftbl;

		public Delegate GetAt_0;

		internal _get_PropertyAsUInt32 get_Size_1;

		public Delegate IndexOf_2;

		public IReadOnlyList_Delegates.GetMany_3 GetMany_3;

		public static Guid PIID;

		private static readonly Type GetAt_0_Type;

		private static readonly Type IndexOf_2_Type;

		private static readonly Vftbl AbiToProjectionVftable;

		public static readonly nint AbiToProjectionVftablePtr;

		private static ConditionalWeakTable<global::System.Collections.Generic.IReadOnlyList<T>, ToAbiHelper> _adapterTable;

		internal unsafe Vftbl(nint thisPtr)
		{
			VftblPtr vftblPtr = Marshal.PtrToStructure<VftblPtr>(thisPtr);
			nint* vftbl = (nint*)vftblPtr.Vftbl;
			IInspectableVftbl = Marshal.PtrToStructure<IInspectable.Vftbl>(vftblPtr.Vftbl);
			GetAt_0 = Marshal.GetDelegateForFunctionPointer(vftbl[6], GetAt_0_Type);
			get_Size_1 = Marshal.GetDelegateForFunctionPointer<_get_PropertyAsUInt32>(vftbl[7]);
			IndexOf_2 = Marshal.GetDelegateForFunctionPointer(vftbl[8], IndexOf_2_Type);
			GetMany_3 = Marshal.GetDelegateForFunctionPointer<IReadOnlyList_Delegates.GetMany_3>(vftbl[9]);
		}

		unsafe static Vftbl()
		{
			PIID = GuidGenerator.CreateIID(typeof(IReadOnlyList<T>));
			GetAt_0_Type = Expression.GetDelegateType(typeof(void*), typeof(uint), Marshaler<T>.AbiType.MakeByRefType(), typeof(int));
			IndexOf_2_Type = Expression.GetDelegateType(typeof(void*), Marshaler<T>.AbiType, typeof(uint).MakeByRefType(), typeof(byte).MakeByRefType(), typeof(int));
			_adapterTable = new ConditionalWeakTable<global::System.Collections.Generic.IReadOnlyList<T>, ToAbiHelper>();
			AbiToProjectionVftable = new Vftbl
			{
				IInspectableVftbl = IInspectable.Vftbl.AbiToProjectionVftable,
				GetAt_0 = Delegate.CreateDelegate(GetAt_0_Type, typeof(Vftbl).GetMethod("Do_Abi_GetAt_0", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(Marshaler<T>.AbiType)),
				get_Size_1 = Do_Abi_get_Size_1,
				IndexOf_2 = Delegate.CreateDelegate(IndexOf_2_Type, typeof(Vftbl).GetMethod("Do_Abi_IndexOf_2", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(Marshaler<T>.AbiType)),
				GetMany_3 = Do_Abi_GetMany_3
			};
			nint* ptr = (nint*)Marshal.AllocCoTaskMem(Marshal.SizeOf<IInspectable.Vftbl>() + sizeof(nint) * 4);
			Marshal.StructureToPtr(AbiToProjectionVftable.IInspectableVftbl, (nint)ptr, fDeleteOld: false);
			ptr[6] = Marshal.GetFunctionPointerForDelegate(AbiToProjectionVftable.GetAt_0);
			ptr[7] = Marshal.GetFunctionPointerForDelegate(AbiToProjectionVftable.get_Size_1);
			ptr[8] = Marshal.GetFunctionPointerForDelegate(AbiToProjectionVftable.IndexOf_2);
			ptr[9] = Marshal.GetFunctionPointerForDelegate(AbiToProjectionVftable.GetMany_3);
			AbiToProjectionVftablePtr = (nint)ptr;
		}

		private static ToAbiHelper FindAdapter(nint thisPtr)
		{
			global::System.Collections.Generic.IReadOnlyList<T> key = ComWrappersSupport.FindObject<global::System.Collections.Generic.IReadOnlyList<T>>(thisPtr);
			return _adapterTable.GetValue(key, (global::System.Collections.Generic.IReadOnlyList<T> list) => new ToAbiHelper(list));
		}

		private unsafe static int Do_Abi_GetAt_0<TAbi>(void* thisPtr, uint index, out TAbi __return_value__)
		{
			T val = default(T);
			__return_value__ = default(TAbi);
			try
			{
				val = FindAdapter(new IntPtr(thisPtr)).GetAt(index);
				__return_value__ = (TAbi)Marshaler<T>.FromManaged(val);
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private unsafe static int Do_Abi_IndexOf_2<TAbi>(void* thisPtr, TAbi value, out uint index, out byte __return_value__)
		{
			bool flag = false;
			index = 0u;
			__return_value__ = 0;
			uint index2 = 0u;
			try
			{
				flag = FindAdapter(new IntPtr(thisPtr)).IndexOf(Marshaler<T>.FromAbi(value), out index2);
				index = index2;
				__return_value__ = (flag ? ((byte)1) : ((byte)0));
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_GetMany_3(nint thisPtr, uint startIndex, int __itemsSize, nint items, out uint __return_value__)
		{
			uint num = 0u;
			__return_value__ = 0u;
			T[] items2 = Marshaler<T>.FromAbiArray((__itemsSize, items));
			try
			{
				num = FindAdapter(thisPtr).GetMany(startIndex, ref items2);
				Marshaler<T>.CopyManagedArray(items2, items);
				__return_value__ = num;
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_get_Size_1(nint thisPtr, out uint __return_value__)
		{
			uint num = 0u;
			__return_value__ = 0u;
			try
			{
				num = FindAdapter(thisPtr).Size;
				__return_value__ = num;
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

	private FromAbiHelper _FromVectorView;

	public IObjectReference ObjRef => _obj;

	public nint ThisPtr => _obj.ThisPtr;

	public uint Size
	{
		get
		{
			uint value = 0u;
			ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.get_Size_1(ThisPtr, out value));
			return value;
		}
	}

	public int Count => _FromVectorView.Count;

	public T this[int index] => _FromVectorView[index];

	public static IObjectReference CreateMarshaler(global::System.Collections.Generic.IReadOnlyList<T> obj)
	{
		if (obj != null)
		{
			return ComWrappersSupport.CreateCCWForObject(obj).As<Vftbl>(GuidGenerator.GetIID(typeof(IReadOnlyList<T>)));
		}
		return null;
	}

	public static nint GetAbi(IObjectReference objRef)
	{
		return objRef?.ThisPtr ?? IntPtr.Zero;
	}

	public static global::System.Collections.Generic.IReadOnlyList<T> FromAbi(nint thisPtr)
	{
		if (thisPtr != IntPtr.Zero)
		{
			return new IReadOnlyList<T>(ObjRefFromAbi(thisPtr));
		}
		return null;
	}

	public static nint FromManaged(global::System.Collections.Generic.IReadOnlyList<T> value)
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
		MarshalInterfaceHelper<IVectorView<T>>.DisposeAbi(abi);
	}

	public static string GetGuidSignature()
	{
		return GuidGenerator.GetSignature(typeof(IReadOnlyList<T>));
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

	public static implicit operator IReadOnlyList<T>(IObjectReference obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new IReadOnlyList<T>(obj);
	}

	public static implicit operator IReadOnlyList<T>(ObjectReference<Vftbl> obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new IReadOnlyList<T>(obj);
	}

	public ObjectReference<I> AsInterface<I>()
	{
		return _obj.As<I>();
	}

	public A As<A>()
	{
		return _obj.AsType<A>();
	}

	public IReadOnlyList(IObjectReference obj)
		: this(obj.As<Vftbl>())
	{
	}

	public IReadOnlyList(ObjectReference<Vftbl> obj)
	{
		_obj = obj;
		_FromVectorView = new FromAbiHelper(this);
	}

	public T GetAt(uint index)
	{
		object[] array = new object[3] { ThisPtr, index, null };
		try
		{
			_obj.Vftbl.GetAt_0.DynamicInvokeAbi(array);
			return Marshaler<T>.FromAbi(array[2]);
		}
		finally
		{
			Marshaler<T>.DisposeAbi(array[2]);
		}
	}

	public bool IndexOf(T value, out uint index)
	{
		object obj = null;
		object[] array = new object[4] { ThisPtr, null, null, null };
		try
		{
			obj = Marshaler<T>.CreateMarshaler(value);
			array[1] = Marshaler<T>.GetAbi(obj);
			_obj.Vftbl.IndexOf_2.DynamicInvokeAbi(array);
			index = (uint)array[2];
			return (byte)array[3] != 0;
		}
		finally
		{
			Marshaler<T>.DisposeMarshaler(obj);
		}
	}

	public uint GetMany(uint startIndex, ref T[] items)
	{
		object obj = null;
		int num = 0;
		nint num2 = 0;
		uint __return_value__ = 0u;
		try
		{
			obj = Marshaler<T>.CreateMarshalerArray(items);
			(num, num2) = Marshaler<T>.GetAbiArray(obj);
			ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.GetMany_3(ThisPtr, startIndex, num, num2, out __return_value__));
			items = Marshaler<T>.FromAbiArray((num, num2));
			return __return_value__;
		}
		finally
		{
			Marshaler<T>.DisposeMarshalerArray(obj);
		}
	}

	public global::System.Collections.Generic.IEnumerator<T> GetEnumerator()
	{
		return _FromVectorView.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
