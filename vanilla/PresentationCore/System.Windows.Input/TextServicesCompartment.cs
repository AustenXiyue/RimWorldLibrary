using System.Runtime.InteropServices;
using MS.Internal;
using MS.Win32;

namespace System.Windows.Input;

internal class TextServicesCompartment
{
	private readonly SecurityCriticalData<MS.Win32.UnsafeNativeMethods.ITfCompartmentMgr> _compartmentmgr;

	private Guid _guid;

	private int _cookie;

	internal bool BooleanValue
	{
		get
		{
			object value = Value;
			if (value == null)
			{
				return false;
			}
			if ((int)value != 0)
			{
				return true;
			}
			return false;
		}
		set
		{
			Value = (value ? 1 : 0);
		}
	}

	internal int IntValue
	{
		get
		{
			object value = Value;
			if (value == null)
			{
				return 0;
			}
			return (int)value;
		}
		set
		{
			Value = value;
		}
	}

	internal object Value
	{
		get
		{
			MS.Win32.UnsafeNativeMethods.ITfCompartment iTfCompartment = GetITfCompartment();
			if (iTfCompartment == null)
			{
				return null;
			}
			iTfCompartment.GetValue(out var varValue);
			Marshal.ReleaseComObject(iTfCompartment);
			return varValue;
		}
		set
		{
			MS.Win32.UnsafeNativeMethods.ITfCompartment iTfCompartment = GetITfCompartment();
			if (iTfCompartment != null)
			{
				iTfCompartment.SetValue(0, ref value);
				Marshal.ReleaseComObject(iTfCompartment);
			}
		}
	}

	internal TextServicesCompartment(Guid guid, MS.Win32.UnsafeNativeMethods.ITfCompartmentMgr compartmentmgr)
	{
		_guid = guid;
		_compartmentmgr = new SecurityCriticalData<MS.Win32.UnsafeNativeMethods.ITfCompartmentMgr>(compartmentmgr);
		_cookie = -1;
	}

	internal void AdviseNotifySink(MS.Win32.UnsafeNativeMethods.ITfCompartmentEventSink sink)
	{
		MS.Win32.UnsafeNativeMethods.ITfCompartment iTfCompartment = GetITfCompartment();
		if (iTfCompartment != null)
		{
			MS.Win32.UnsafeNativeMethods.ITfSource obj = iTfCompartment as MS.Win32.UnsafeNativeMethods.ITfSource;
			Guid riid = MS.Win32.UnsafeNativeMethods.IID_ITfCompartmentEventSink;
			obj.AdviseSink(ref riid, sink, out _cookie);
			Marshal.ReleaseComObject(iTfCompartment);
			Marshal.ReleaseComObject(obj);
		}
	}

	internal void UnadviseNotifySink()
	{
		MS.Win32.UnsafeNativeMethods.ITfCompartment iTfCompartment = GetITfCompartment();
		if (iTfCompartment != null)
		{
			MS.Win32.UnsafeNativeMethods.ITfSource obj = iTfCompartment as MS.Win32.UnsafeNativeMethods.ITfSource;
			obj.UnadviseSink(_cookie);
			_cookie = -1;
			Marshal.ReleaseComObject(iTfCompartment);
			Marshal.ReleaseComObject(obj);
		}
	}

	internal MS.Win32.UnsafeNativeMethods.ITfCompartment GetITfCompartment()
	{
		_compartmentmgr.Value.GetCompartment(ref _guid, out var comp);
		return comp;
	}
}
