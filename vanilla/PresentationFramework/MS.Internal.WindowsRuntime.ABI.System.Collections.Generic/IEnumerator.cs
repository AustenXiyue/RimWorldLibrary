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

[Guid("6A79E863-4300-459A-9966-CBB660963EE1")]
internal class IEnumerator<T> : global::System.Collections.Generic.IEnumerator<T>, IEnumerator, IDisposable, IIterator<T>
{
	public class FromAbiHelper : global::System.Collections.Generic.IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly IIterator<T> _iterator;

		private bool m_hadCurrent = true;

		private T m_current;

		private bool m_isInitialized;

		public T Current
		{
			get
			{
				if (!m_isInitialized)
				{
					throw new InvalidOperationException(ErrorStrings.InvalidOperation_EnumNotStarted);
				}
				if (!m_hadCurrent)
				{
					throw new InvalidOperationException(ErrorStrings.InvalidOperation_EnumEnded);
				}
				return m_current;
			}
		}

		object IEnumerator.Current
		{
			get
			{
				if (!m_isInitialized)
				{
					throw new InvalidOperationException(ErrorStrings.InvalidOperation_EnumNotStarted);
				}
				if (!m_hadCurrent)
				{
					throw new InvalidOperationException(ErrorStrings.InvalidOperation_EnumEnded);
				}
				return m_current;
			}
		}

		public FromAbiHelper(IObjectReference obj)
			: this((IIterator<T>)new IEnumerator<T>(obj))
		{
		}

		internal FromAbiHelper(IIterator<T> iterator)
		{
			_iterator = iterator;
		}

		public bool MoveNext()
		{
			if (!m_hadCurrent)
			{
				return false;
			}
			try
			{
				if (!m_isInitialized)
				{
					m_hadCurrent = _iterator.HasCurrent;
					m_isInitialized = true;
				}
				else
				{
					m_hadCurrent = _iterator._MoveNext();
				}
				if (m_hadCurrent)
				{
					m_current = _iterator._Current;
				}
			}
			catch (Exception e)
			{
				if (Marshal.GetHRForException(e) == -2147483636)
				{
					throw new InvalidOperationException(ErrorStrings.InvalidOperation_EnumFailedVersion);
				}
				throw;
			}
			return m_hadCurrent;
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}

		public void Dispose()
		{
		}
	}

	public sealed class ToAbiHelper : IIterator<T>
	{
		private readonly global::System.Collections.Generic.IEnumerator<T> m_enumerator;

		private bool m_firstItem = true;

		private bool m_hasCurrent;

		public T _Current
		{
			get
			{
				if (m_firstItem)
				{
					m_firstItem = false;
					_MoveNext();
				}
				if (!m_hasCurrent)
				{
					ExceptionHelpers.ThrowExceptionForHR(-2147483637);
				}
				return m_enumerator.Current;
			}
		}

		public bool HasCurrent
		{
			get
			{
				if (m_firstItem)
				{
					m_firstItem = false;
					_MoveNext();
				}
				return m_hasCurrent;
			}
		}

		public object Current => _Current;

		internal ToAbiHelper(global::System.Collections.Generic.IEnumerator<T> enumerator)
		{
			m_enumerator = enumerator;
		}

		public bool _MoveNext()
		{
			try
			{
				m_hasCurrent = m_enumerator.MoveNext();
			}
			catch (InvalidOperationException)
			{
				ExceptionHelpers.ThrowExceptionForHR(-2147483636);
			}
			return m_hasCurrent;
		}

		public uint GetMany(ref T[] items)
		{
			if (items == null)
			{
				return 0u;
			}
			int i;
			for (i = 0; i < items.Length; i++)
			{
				if (!HasCurrent)
				{
					break;
				}
				items[i] = _Current;
				_MoveNext();
			}
			if (typeof(T) == typeof(string))
			{
				string[] array = items as string[];
				for (int j = i; j < items.Length; j++)
				{
					array[j] = string.Empty;
				}
			}
			return (uint)i;
		}

		public bool MoveNext()
		{
			return _MoveNext();
		}
	}

	[Guid("6A79E863-4300-459A-9966-CBB660963EE1")]
	public struct Vftbl
	{
		internal IInspectable.Vftbl IInspectableVftbl;

		public Delegate get_Current_0;

		internal _get_PropertyAsBoolean get_HasCurrent_1;

		public IEnumerator_Delegates.MoveNext_2 MoveNext_2;

		public IEnumerator_Delegates.GetMany_3 GetMany_3;

		public static Guid PIID;

		private static readonly Type get_Current_0_Type;

		private static readonly Vftbl AbiToProjectionVftable;

		public static readonly nint AbiToProjectionVftablePtr;

		private static ConditionalWeakTable<global::System.Collections.Generic.IEnumerator<T>, ToAbiHelper> _adapterTable;

		internal unsafe Vftbl(nint thisPtr)
		{
			VftblPtr vftblPtr = Marshal.PtrToStructure<VftblPtr>(thisPtr);
			nint* vftbl = (nint*)vftblPtr.Vftbl;
			IInspectableVftbl = Marshal.PtrToStructure<IInspectable.Vftbl>(vftblPtr.Vftbl);
			get_Current_0 = Marshal.GetDelegateForFunctionPointer(vftbl[6], get_Current_0_Type);
			get_HasCurrent_1 = Marshal.GetDelegateForFunctionPointer<_get_PropertyAsBoolean>(vftbl[7]);
			MoveNext_2 = Marshal.GetDelegateForFunctionPointer<IEnumerator_Delegates.MoveNext_2>(vftbl[8]);
			GetMany_3 = Marshal.GetDelegateForFunctionPointer<IEnumerator_Delegates.GetMany_3>(vftbl[9]);
		}

		unsafe static Vftbl()
		{
			PIID = GuidGenerator.CreateIID(typeof(IEnumerator<T>));
			get_Current_0_Type = Expression.GetDelegateType(typeof(void*), Marshaler<T>.AbiType.MakeByRefType(), typeof(int));
			_adapterTable = new ConditionalWeakTable<global::System.Collections.Generic.IEnumerator<T>, ToAbiHelper>();
			AbiToProjectionVftable = new Vftbl
			{
				IInspectableVftbl = IInspectable.Vftbl.AbiToProjectionVftable,
				get_Current_0 = Delegate.CreateDelegate(get_Current_0_Type, typeof(Vftbl).GetMethod("Do_Abi_get_Current_0", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(Marshaler<T>.AbiType)),
				get_HasCurrent_1 = Do_Abi_get_HasCurrent_1,
				MoveNext_2 = Do_Abi_MoveNext_2,
				GetMany_3 = Do_Abi_GetMany_3
			};
			nint* ptr = (nint*)Marshal.AllocCoTaskMem(Marshal.SizeOf<IInspectable.Vftbl>() + sizeof(nint) * 4);
			Marshal.StructureToPtr(AbiToProjectionVftable.IInspectableVftbl, (nint)ptr, fDeleteOld: false);
			ptr[6] = Marshal.GetFunctionPointerForDelegate(AbiToProjectionVftable.get_Current_0);
			ptr[7] = Marshal.GetFunctionPointerForDelegate(AbiToProjectionVftable.get_HasCurrent_1);
			ptr[8] = Marshal.GetFunctionPointerForDelegate(AbiToProjectionVftable.MoveNext_2);
			ptr[9] = Marshal.GetFunctionPointerForDelegate(AbiToProjectionVftable.GetMany_3);
			AbiToProjectionVftablePtr = (nint)ptr;
		}

		private static ToAbiHelper FindAdapter(nint thisPtr)
		{
			global::System.Collections.Generic.IEnumerator<T> key = ComWrappersSupport.FindObject<global::System.Collections.Generic.IEnumerator<T>>(thisPtr);
			return _adapterTable.GetValue(key, (global::System.Collections.Generic.IEnumerator<T> enumerator) => new ToAbiHelper(enumerator));
		}

		private static int Do_Abi_MoveNext_2(nint thisPtr, out byte __return_value__)
		{
			bool flag = false;
			__return_value__ = 0;
			try
			{
				flag = FindAdapter(thisPtr)._MoveNext();
				__return_value__ = (flag ? ((byte)1) : ((byte)0));
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_GetMany_3(nint thisPtr, int __itemsSize, nint items, out uint __return_value__)
		{
			uint num = 0u;
			__return_value__ = 0u;
			T[] items2 = Marshaler<T>.FromAbiArray((__itemsSize, items));
			try
			{
				num = FindAdapter(thisPtr).GetMany(ref items2);
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

		private unsafe static int Do_Abi_get_Current_0<TAbi>(void* thisPtr, out TAbi __return_value__)
		{
			T val = default(T);
			__return_value__ = default(TAbi);
			try
			{
				val = FindAdapter(new IntPtr(thisPtr))._Current;
				__return_value__ = (TAbi)Marshaler<T>.FromManaged(val);
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_get_HasCurrent_1(nint thisPtr, out byte __return_value__)
		{
			bool flag = false;
			__return_value__ = 0;
			try
			{
				flag = FindAdapter(thisPtr).HasCurrent;
				__return_value__ = (flag ? ((byte)1) : ((byte)0));
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

	private FromAbiHelper _FromIterator;

	public IObjectReference ObjRef => _obj;

	public nint ThisPtr => _obj.ThisPtr;

	public T _Current
	{
		get
		{
			object[] array = new object[2] { ThisPtr, null };
			try
			{
				_obj.Vftbl.get_Current_0.DynamicInvokeAbi(array);
				return Marshaler<T>.FromAbi(array[1]);
			}
			finally
			{
				Marshaler<T>.DisposeAbi(array[1]);
			}
		}
	}

	public bool HasCurrent
	{
		get
		{
			byte value = 0;
			ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.get_HasCurrent_1(ThisPtr, out value));
			return value != 0;
		}
	}

	public T Current => _FromIterator.Current;

	object IEnumerator.Current => Current;

	public static IObjectReference CreateMarshaler(global::System.Collections.Generic.IEnumerator<T> obj)
	{
		if (obj != null)
		{
			return ComWrappersSupport.CreateCCWForObject(obj).As<Vftbl>(GuidGenerator.GetIID(typeof(IEnumerator<T>)));
		}
		return null;
	}

	public static nint GetAbi(IObjectReference objRef)
	{
		return objRef?.ThisPtr ?? IntPtr.Zero;
	}

	public static global::System.Collections.Generic.IEnumerator<T> FromAbi(nint thisPtr)
	{
		if (thisPtr != IntPtr.Zero)
		{
			return new IEnumerator<T>(ObjRefFromAbi(thisPtr));
		}
		return null;
	}

	internal static IIterator<T> FromAbiInternal(nint thisPtr)
	{
		return new IEnumerator<T>(ObjRefFromAbi(thisPtr));
	}

	public static nint FromManaged(global::System.Collections.Generic.IEnumerator<T> value)
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
		MarshalInterfaceHelper<IIterator<T>>.DisposeAbi(abi);
	}

	public static string GetGuidSignature()
	{
		return GuidGenerator.GetSignature(typeof(IEnumerator<T>));
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

	public static implicit operator IEnumerator<T>(IObjectReference obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new IEnumerator<T>(obj);
	}

	public static implicit operator IEnumerator<T>(ObjectReference<Vftbl> obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new IEnumerator<T>(obj);
	}

	public ObjectReference<I> AsInterface<I>()
	{
		return _obj.As<I>();
	}

	public A As<A>()
	{
		return _obj.AsType<A>();
	}

	public IEnumerator(IObjectReference obj)
		: this(obj.As<Vftbl>())
	{
	}

	public IEnumerator(ObjectReference<Vftbl> obj)
	{
		_obj = obj;
		_FromIterator = new FromAbiHelper(this);
	}

	public bool _MoveNext()
	{
		byte __return_value__ = 0;
		ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.MoveNext_2(ThisPtr, out __return_value__));
		return __return_value__ != 0;
	}

	public uint GetMany(ref T[] items)
	{
		object obj = null;
		int num = 0;
		nint num2 = 0;
		uint __return_value__ = 0u;
		try
		{
			obj = Marshaler<T>.CreateMarshalerArray(items);
			(num, num2) = Marshaler<T>.GetAbiArray(obj);
			ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.GetMany_3(ThisPtr, num, num2, out __return_value__));
			items = Marshaler<T>.FromAbiArray((num, num2));
			return __return_value__;
		}
		finally
		{
			Marshaler<T>.DisposeMarshalerArray(obj);
		}
	}

	public bool MoveNext()
	{
		return _FromIterator.MoveNext();
	}

	public void Reset()
	{
		_FromIterator.Reset();
	}

	public void Dispose()
	{
		_FromIterator.Dispose();
	}
}
