using System;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.ABI.Windows.Data.Text;
using WinRT;
using WinRT.Interop;

namespace MS.Internal.WindowsRuntime.Windows.Data.Text;

[WindowsRuntimeType]
[ProjectedRuntimeClass("_default")]
internal sealed class AlternateWordForm : ICustomQueryInterface, IEquatable<AlternateWordForm>
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct InterfaceTag<I>
	{
	}

	private IObjectReference _inner;

	private readonly Lazy<MS.Internal.WindowsRuntime.ABI.Windows.Data.Text.IAlternateWordForm> _defaultLazy;

	public nint ThisPtr => _default.ThisPtr;

	private MS.Internal.WindowsRuntime.ABI.Windows.Data.Text.IAlternateWordForm _default => _defaultLazy.Value;

	public string AlternateText => _default.AlternateText;

	public AlternateNormalizationFormat NormalizationFormat => _default.NormalizationFormat;

	public TextSegment SourceTextSegment => _default.SourceTextSegment;

	public static AlternateWordForm FromAbi(nint thisPtr)
	{
		if (thisPtr == IntPtr.Zero)
		{
			return null;
		}
		object obj = MarshalInspectable.FromAbi(thisPtr);
		if (!(obj is AlternateWordForm))
		{
			return new AlternateWordForm((MS.Internal.WindowsRuntime.ABI.Windows.Data.Text.IAlternateWordForm)obj);
		}
		return (AlternateWordForm)obj;
	}

	public AlternateWordForm(MS.Internal.WindowsRuntime.ABI.Windows.Data.Text.IAlternateWordForm ifc)
	{
		_defaultLazy = new Lazy<MS.Internal.WindowsRuntime.ABI.Windows.Data.Text.IAlternateWordForm>(() => ifc);
	}

	public static bool operator ==(AlternateWordForm x, AlternateWordForm y)
	{
		return (x?.ThisPtr ?? IntPtr.Zero) == (y?.ThisPtr ?? IntPtr.Zero);
	}

	public static bool operator !=(AlternateWordForm x, AlternateWordForm y)
	{
		return !(x == y);
	}

	public bool Equals(AlternateWordForm other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		if (obj is AlternateWordForm alternateWordForm)
		{
			return this == alternateWordForm;
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

	private IAlternateWordForm AsInternal(InterfaceTag<IAlternateWordForm> _)
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
