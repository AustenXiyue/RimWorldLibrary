using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.ABI.System.Collections.Generic;
using MS.Internal.WindowsRuntime.Windows.Globalization;
using WinRT;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Globalization;

[ObjectReferenceWrapper("_obj")]
[Guid("7D7DAF45-368D-4364-852B-DEC927037B85")]
internal class ILanguageExtensionSubtags : MS.Internal.WindowsRuntime.Windows.Globalization.ILanguageExtensionSubtags
{
	[Guid("7D7DAF45-368D-4364-852B-DEC927037B85")]
	internal struct Vftbl
	{
		public IInspectable.Vftbl IInspectableVftbl;

		public ILanguageExtensionSubtags_Delegates.GetExtensionSubtags_0 GetExtensionSubtags_0;

		private static readonly Vftbl AbiToProjectionVftable;

		public static readonly nint AbiToProjectionVftablePtr;

		unsafe static Vftbl()
		{
			AbiToProjectionVftable = new Vftbl
			{
				IInspectableVftbl = IInspectable.Vftbl.AbiToProjectionVftable,
				GetExtensionSubtags_0 = Do_Abi_GetExtensionSubtags_0
			};
			nint* ptr = (nint*)ComWrappersSupport.AllocateVtableMemory(typeof(Vftbl), Marshal.SizeOf<IInspectable.Vftbl>() + sizeof(nint));
			Marshal.StructureToPtr(AbiToProjectionVftable, (nint)ptr, fDeleteOld: false);
			AbiToProjectionVftablePtr = (nint)ptr;
		}

		private static int Do_Abi_GetExtensionSubtags_0(nint thisPtr, nint singleton, out nint value)
		{
			global::System.Collections.Generic.IReadOnlyList<string> readOnlyList = null;
			value = 0;
			try
			{
				readOnlyList = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Globalization.ILanguageExtensionSubtags>(thisPtr).GetExtensionSubtags(MarshalString.FromAbi(singleton));
				value = MS.Internal.WindowsRuntime.ABI.System.Collections.Generic.IReadOnlyList<string>.FromManaged(readOnlyList);
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}
	}

	protected readonly ObjectReference<Vftbl> _obj;

	public IObjectReference ObjRef => _obj;

	public nint ThisPtr => _obj.ThisPtr;

	public static ObjectReference<Vftbl> FromAbi(nint thisPtr)
	{
		return ObjectReference<Vftbl>.FromAbi(thisPtr);
	}

	public static implicit operator ILanguageExtensionSubtags(IObjectReference obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new ILanguageExtensionSubtags(obj);
	}

	public ObjectReference<I> AsInterface<I>()
	{
		return _obj.As<I>();
	}

	public A As<A>()
	{
		return _obj.AsType<A>();
	}

	public ILanguageExtensionSubtags(IObjectReference obj)
		: this(obj.As<Vftbl>())
	{
	}

	public ILanguageExtensionSubtags(ObjectReference<Vftbl> obj)
	{
		_obj = obj;
	}

	public global::System.Collections.Generic.IReadOnlyList<string> GetExtensionSubtags(string singleton)
	{
		MarshalString m = null;
		nint value = 0;
		try
		{
			m = MarshalString.CreateMarshaler(singleton);
			ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.GetExtensionSubtags_0(ThisPtr, MarshalString.GetAbi(m), out value));
			return MS.Internal.WindowsRuntime.ABI.System.Collections.Generic.IReadOnlyList<string>.FromAbi(value);
		}
		finally
		{
			MarshalString.DisposeMarshaler(m);
			MS.Internal.WindowsRuntime.ABI.System.Collections.Generic.IReadOnlyList<string>.DisposeAbi(value);
		}
	}
}
