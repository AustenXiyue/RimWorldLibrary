using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input.Tracing;
using Microsoft.Win32;
using MS.Win32;

namespace System.Windows.Input.StylusWisp;

public class WispTabletDeviceCollection : TabletDeviceCollection
{
	private const int VistaMajorVersion = 6;

	private TabletDevice[] _tablets = Array.Empty<TabletDevice>();

	private uint _indexMouseTablet = uint.MaxValue;

	private bool _inUpdateTablets;

	private bool _hasUpdateTabletsBeenCalledReentrantly;

	private List<TabletDevice> _deferredTablets = new List<TabletDevice>();

	internal List<TabletDevice> DeferredTablets => _deferredTablets;

	internal WispTabletDeviceCollection()
	{
		WispLogic currentStylusLogicAs = StylusLogic.GetCurrentStylusLogicAs<WispLogic>();
		bool flag = currentStylusLogicAs.Enabled;
		if (!flag)
		{
			flag = ShouldEnableTablets();
		}
		if (flag)
		{
			UpdateTablets();
			if (!currentStylusLogicAs.Enabled)
			{
				currentStylusLogicAs.EnableCore();
			}
		}
	}

	internal static bool ShouldEnableTablets()
	{
		bool result = false;
		if (StylusLogic.IsStylusAndTouchSupportEnabled && IsWisptisRegistered() && HasTabletDevices())
		{
			result = true;
		}
		return result;
	}

	private static bool IsWisptisRegistered()
	{
		bool result = false;
		RegistryKey registryKey = null;
		object obj = null;
		bool num = Environment.OSVersion.Version.Major >= 6;
		string name = (num ? "Interface\\{C247F616-BBEB-406A-AED3-F75E656599AE}" : "CLSID\\{A5B020FD-E04B-4e67-B65A-E7DEED25B2CF}\\LocalServer32");
		string value = (num ? "ITablet2" : "wisptis.exe");
		registryKey = Registry.ClassesRoot.OpenSubKey(name);
		if (registryKey != null)
		{
			obj = registryKey.GetValue("");
		}
		if (registryKey != null)
		{
			if (obj is string text && text.LastIndexOf(value, StringComparison.OrdinalIgnoreCase) != -1)
			{
				result = true;
			}
			registryKey.Close();
		}
		return result;
	}

	private static bool HasTabletDevices()
	{
		uint numDevices = 0u;
		if ((int)MS.Win32.UnsafeNativeMethods.GetRawInputDeviceList(null, ref numDevices, (uint)Marshal.SizeOf(typeof(MS.Win32.NativeMethods.RAWINPUTDEVICELIST))) >= 0 && numDevices != 0)
		{
			MS.Win32.NativeMethods.RAWINPUTDEVICELIST[] array = new MS.Win32.NativeMethods.RAWINPUTDEVICELIST[numDevices];
			int rawInputDeviceList = (int)MS.Win32.UnsafeNativeMethods.GetRawInputDeviceList(array, ref numDevices, (uint)Marshal.SizeOf(typeof(MS.Win32.NativeMethods.RAWINPUTDEVICELIST)));
			if (rawInputDeviceList > 0)
			{
				for (int i = 0; i < rawInputDeviceList; i++)
				{
					if (array[i].dwType != 2)
					{
						continue;
					}
					MS.Win32.NativeMethods.RID_DEVICE_INFO ridInfo = default(MS.Win32.NativeMethods.RID_DEVICE_INFO);
					ridInfo.cbSize = (uint)Marshal.SizeOf(typeof(MS.Win32.NativeMethods.RID_DEVICE_INFO));
					uint sizeInBytes = ridInfo.cbSize;
					if ((int)MS.Win32.UnsafeNativeMethods.GetRawInputDeviceInfo(array[i].hDevice, 536870923u, ref ridInfo, ref sizeInBytes) > 0 && ridInfo.hid.usUsagePage == 13)
					{
						ushort usUsage = ridInfo.hid.usUsage;
						if ((uint)(usUsage - 1) <= 3u)
						{
							return true;
						}
					}
				}
			}
			else
			{
				_ = 0;
			}
		}
		return false;
	}

	internal void UpdateTablets()
	{
		if (_tablets == null)
		{
			throw new ObjectDisposedException("TabletDeviceCollection");
		}
		if (_inUpdateTablets)
		{
			StylusTraceLogger.LogReentrancy("UpdateTablets");
			_hasUpdateTabletsBeenCalledReentrantly = true;
			return;
		}
		try
		{
			_inUpdateTablets = true;
			do
			{
				_hasUpdateTabletsBeenCalledReentrantly = false;
				UpdateTabletsImpl();
			}
			while (_hasUpdateTabletsBeenCalledReentrantly);
		}
		finally
		{
			_inUpdateTablets = false;
			_hasUpdateTabletsBeenCalledReentrantly = false;
		}
	}

	private void UpdateTabletsImpl()
	{
		PenThread penThread = ((_tablets.Length != 0) ? _tablets[0].As<WispTabletDevice>().PenThread : PenThreadPool.GetPenThreadForPenContext(null));
		if (penThread == null)
		{
			return;
		}
		TabletDeviceInfo[] array = penThread.WorkerGetTabletsInfo();
		uint indexMouseTablet = uint.MaxValue;
		for (uint num = 0u; num < array.Length; num++)
		{
			if (array[num].PimcTablet != null && array[num].DeviceType == (TabletDeviceType)(-1))
			{
				indexMouseTablet = num;
				array[num].PimcTablet = null;
			}
		}
		uint num2 = 0u;
		for (uint num3 = 0u; num3 < array.Length; num3++)
		{
			if (array[num3].PimcTablet != null)
			{
				num2++;
			}
		}
		TabletDevice[] array2 = new TabletDevice[num2];
		uint num4 = 0u;
		uint num5 = 0u;
		for (uint num6 = 0u; num6 < array.Length; num6++)
		{
			if (array[num6].PimcTablet == null)
			{
				continue;
			}
			int id = array[num6].Id;
			if (num4 < _tablets.Length && _tablets[num4] != null && _tablets[num4].Id == id)
			{
				array2[num4] = _tablets[num4];
				_tablets[num4] = null;
				num5++;
			}
			else
			{
				TabletDevice tabletDevice = null;
				for (uint num7 = 0u; num7 < _tablets.Length; num7++)
				{
					if (_tablets[num7] != null && _tablets[num7].Id == id)
					{
						tabletDevice = _tablets[num7];
						_tablets[num7] = null;
						break;
					}
				}
				if (tabletDevice == null)
				{
					try
					{
						tabletDevice = new TabletDevice(new WispTabletDevice(array[num6], penThread));
					}
					catch (InvalidOperationException ex)
					{
						if (ex.Data.Contains("System.Windows.Input.StylusLogic"))
						{
							continue;
						}
						throw;
					}
				}
				array2[num4] = tabletDevice;
			}
			num4++;
		}
		if (num5 == _tablets.Length && num5 == num4 && num4 == num2)
		{
			Array.Copy(array2, 0L, _tablets, 0L, num2);
			_indexMouseTablet = indexMouseTablet;
		}
		else
		{
			if (num4 != num2)
			{
				TabletDevice[] array3 = new TabletDevice[num4];
				Array.Copy(array2, 0L, array3, 0L, num4);
				array2 = array3;
			}
			DisposeTablets();
			_tablets = array2;
			base.TabletDevices = new List<TabletDevice>(_tablets);
			_indexMouseTablet = indexMouseTablet;
		}
		DisposeDeferredTablets();
	}

	internal bool HandleTabletAdded(uint wisptisIndex, ref uint tabletIndexChanged)
	{
		if (_tablets == null)
		{
			throw new ObjectDisposedException("TabletDeviceCollection");
		}
		tabletIndexChanged = uint.MaxValue;
		PenThread penThread = ((_tablets.Length != 0) ? _tablets[0].As<WispTabletDevice>().PenThread : PenThreadPool.GetPenThreadForPenContext(null));
		if (penThread == null)
		{
			return true;
		}
		TabletDeviceInfo tabletDeviceInfo = penThread.WorkerGetTabletInfo(wisptisIndex);
		if (tabletDeviceInfo.PimcTablet == null)
		{
			return true;
		}
		if (tabletDeviceInfo.DeviceType == (TabletDeviceType)(-1))
		{
			_indexMouseTablet = wisptisIndex;
			return false;
		}
		uint num = uint.MaxValue;
		for (uint num2 = 0u; num2 < _tablets.Length; num2++)
		{
			if (_tablets[num2].Id == tabletDeviceInfo.Id)
			{
				num = num2;
				break;
			}
		}
		uint num3 = uint.MaxValue;
		if (num == uint.MaxValue)
		{
			num3 = wisptisIndex;
			if (num3 > _indexMouseTablet)
			{
				num3--;
			}
			else
			{
				_indexMouseTablet++;
			}
			if (num3 <= _tablets.Length)
			{
				try
				{
					AddTablet(num3, new TabletDevice(new WispTabletDevice(tabletDeviceInfo, penThread)));
				}
				catch (InvalidOperationException ex)
				{
					if (ex.Data.Contains("System.Windows.Input.StylusLogic"))
					{
						return true;
					}
					throw;
				}
				tabletIndexChanged = num3;
				return true;
			}
			return true;
		}
		return false;
	}

	internal uint HandleTabletRemoved(uint wisptisIndex)
	{
		if (_tablets == null)
		{
			throw new ObjectDisposedException("TabletDeviceCollection");
		}
		if (wisptisIndex == _indexMouseTablet)
		{
			_indexMouseTablet = uint.MaxValue;
			return uint.MaxValue;
		}
		uint num = wisptisIndex;
		if (wisptisIndex > _indexMouseTablet)
		{
			num--;
		}
		else
		{
			_indexMouseTablet--;
		}
		if (num >= _tablets.Length)
		{
			return uint.MaxValue;
		}
		RemoveTablet(num);
		return num;
	}

	private void AddTablet(uint index, TabletDevice tabletDevice)
	{
		TabletDevice[] array = new TabletDevice[base.Count + 1];
		uint num = (uint)_tablets.Length - index;
		Array.Copy(_tablets, 0L, array, 0L, index);
		array[index] = tabletDevice;
		Array.Copy(_tablets, index, array, index + 1, num);
		_tablets = array;
		base.TabletDevices = new List<TabletDevice>(_tablets);
	}

	private void RemoveTablet(uint index)
	{
		WispTabletDevice wispTabletDevice = _tablets[index].As<WispTabletDevice>();
		TabletDevice[] array = new TabletDevice[_tablets.Length - 1];
		uint num = (uint)(_tablets.Length - (int)index - 1);
		Array.Copy(_tablets, 0L, array, 0L, index);
		Array.Copy(_tablets, index + 1, array, index, num);
		_tablets = array;
		base.TabletDevices = new List<TabletDevice>(_tablets);
		wispTabletDevice.DisposeOrDeferDisposal();
		if (wispTabletDevice.IsDisposalPending)
		{
			_deferredTablets.Add(wispTabletDevice.TabletDevice);
		}
	}

	internal WispStylusDevice UpdateStylusDevices(int tabletId, int stylusId)
	{
		if (_tablets == null)
		{
			throw new ObjectDisposedException("TabletDeviceCollection");
		}
		int i = 0;
		for (int num = _tablets.Length; i < num; i++)
		{
			WispTabletDevice wispTabletDevice = _tablets[i].As<WispTabletDevice>();
			if (wispTabletDevice.Id == tabletId)
			{
				return wispTabletDevice.UpdateStylusDevices(stylusId);
			}
		}
		return null;
	}

	internal void DisposeTablets()
	{
		if (_tablets == null)
		{
			return;
		}
		int i = 0;
		for (int num = _tablets.Length; i < num; i++)
		{
			if (_tablets[i] != null)
			{
				WispTabletDevice wispTabletDevice = _tablets[i].TabletDeviceImpl.As<WispTabletDevice>();
				wispTabletDevice.DisposeOrDeferDisposal();
				if (wispTabletDevice.IsDisposalPending)
				{
					_deferredTablets.Add(wispTabletDevice.TabletDevice);
				}
			}
		}
		_tablets = null;
		base.TabletDevices = null;
	}

	internal void DisposeDeferredTablets()
	{
		List<TabletDevice> list = new List<TabletDevice>();
		foreach (TabletDevice deferredTablet in _deferredTablets)
		{
			WispTabletDevice wispTabletDevice = deferredTablet.TabletDeviceImpl.As<WispTabletDevice>();
			wispTabletDevice.DisposeOrDeferDisposal();
			if (wispTabletDevice.IsDisposalPending)
			{
				list.Add(deferredTablet);
			}
		}
		_deferredTablets = list;
	}

	internal PenContext[] CreateContexts(nint hwnd, PenContexts contexts)
	{
		PenContext[] array = new PenContext[base.Count + _deferredTablets.Count];
		int num = 0;
		TabletDevice[] tablets = _tablets;
		foreach (TabletDevice tabletDevice in tablets)
		{
			array[num++] = tabletDevice.As<WispTabletDevice>().CreateContext(hwnd, contexts);
		}
		foreach (TabletDevice deferredTablet in _deferredTablets)
		{
			array[num++] = deferredTablet.As<WispTabletDevice>().CreateContext(hwnd, contexts);
		}
		return array;
	}
}
