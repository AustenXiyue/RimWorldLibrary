using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.ABI.Windows.Data.Text;
using WinRT;
using WinRT.Interop;

namespace MS.Internal.WindowsRuntime.Windows.Data.Text;

[WindowsRuntimeType]
[ProjectedRuntimeClass("_default")]
internal sealed class WordSegment : ICustomQueryInterface, IEquatable<WordSegment>
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct InterfaceTag<I>
	{
	}

	private IObjectReference _inner;

	private readonly Lazy<MS.Internal.WindowsRuntime.ABI.Windows.Data.Text.IWordSegment> _defaultLazy;

	public nint ThisPtr => _default.ThisPtr;

	private MS.Internal.WindowsRuntime.ABI.Windows.Data.Text.IWordSegment _default => _defaultLazy.Value;

	public IReadOnlyList<AlternateWordForm> AlternateForms => _default.AlternateForms;

	public TextSegment SourceTextSegment => _default.SourceTextSegment;

	public string Text => _default.Text;

	public static WordSegment FromAbi(nint thisPtr)
	{
		if (thisPtr == IntPtr.Zero)
		{
			return null;
		}
		object obj = MarshalInspectable.FromAbi(thisPtr);
		if (!(obj is WordSegment))
		{
			return new WordSegment((MS.Internal.WindowsRuntime.ABI.Windows.Data.Text.IWordSegment)obj);
		}
		return (WordSegment)obj;
	}

	public WordSegment(MS.Internal.WindowsRuntime.ABI.Windows.Data.Text.IWordSegment ifc)
	{
		_defaultLazy = new Lazy<MS.Internal.WindowsRuntime.ABI.Windows.Data.Text.IWordSegment>(() => ifc);
	}

	public static bool operator ==(WordSegment x, WordSegment y)
	{
		return (x?.ThisPtr ?? IntPtr.Zero) == (y?.ThisPtr ?? IntPtr.Zero);
	}

	public static bool operator !=(WordSegment x, WordSegment y)
	{
		return !(x == y);
	}

	public bool Equals(WordSegment other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		if (obj is WordSegment wordSegment)
		{
			return this == wordSegment;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((IntPtr)ThisPtr).GetHashCode();
	}

	private IObjectReference GetDefaultReference<T>()
	{
		return _default.AsInterface<T>();
	}

	private IObjectReference GetReferenceForQI()
	{
		return _inner ?? _default.ObjRef;
	}

	private IWordSegment AsInternal(InterfaceTag<IWordSegment> _)
	{
		return _default;
	}

	private bool IsOverridableInterface(Guid iid)
	{
		return false;
	}

	CustomQueryInterfaceResult ICustomQueryInterface.GetInterface(ref Guid iid, out nint ppv)
	{
		ppv = IntPtr.Zero;
		if (IsOverridableInterface(iid) || typeof(IInspectable).GUID == iid)
		{
			return CustomQueryInterfaceResult.NotHandled;
		}
		if (GetReferenceForQI().TryAs(iid, out ObjectReference<IUnknownVftbl> objRef) >= 0)
		{
			using (objRef)
			{
				ppv = objRef.GetRef();
				return CustomQueryInterfaceResult.Handled;
			}
		}
		return CustomQueryInterfaceResult.NotHandled;
	}
}
