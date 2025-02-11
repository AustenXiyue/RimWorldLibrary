using System;
using System.Printing;
using System.Printing.Interop;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using MS.Win32;

namespace MS.Internal.Printing;

internal class Win32PrintDialog
{
	private sealed class PrintDlgExMarshaler : IDisposable
	{
		private Win32PrintDialog _dialog;

		private nint _unmanagedPrintDlgEx;

		private nint _ownerHandle;

		internal nint UnmanagedPrintDlgEx => _unmanagedPrintDlgEx;

		internal PrintDlgExMarshaler(nint owner, Win32PrintDialog dialog)
		{
			_ownerHandle = owner;
			_dialog = dialog;
			_unmanagedPrintDlgEx = IntPtr.Zero;
		}

		~PrintDlgExMarshaler()
		{
			Dispose(disposing: true);
		}

		internal uint SyncFromStruct()
		{
			if (_unmanagedPrintDlgEx == IntPtr.Zero)
			{
				return 0u;
			}
			uint num = AcquireResultFromPrintDlgExStruct(_unmanagedPrintDlgEx);
			if (num == 1 || num == 2)
			{
				ExtractPrintDataAndDevMode(_unmanagedPrintDlgEx, out var printerName, out var flags, out var pageRange, out var devModeHandle);
				_dialog.PrintQueue = AcquirePrintQueue(printerName);
				_dialog.PrintTicket = AcquirePrintTicket(devModeHandle, printerName);
				if ((flags & 2) == 2)
				{
					if (pageRange.PageFrom > pageRange.PageTo)
					{
						int pageTo = pageRange.PageTo;
						pageRange.PageTo = pageRange.PageFrom;
						pageRange.PageFrom = pageTo;
					}
					_dialog.PageRangeSelection = PageRangeSelection.UserPages;
					_dialog.PageRange = pageRange;
				}
				else if ((flags & 1) == 1)
				{
					_dialog.PageRangeSelection = PageRangeSelection.SelectedPages;
				}
				else if ((flags & 0x400000) == 4194304)
				{
					_dialog.PageRangeSelection = PageRangeSelection.CurrentPage;
				}
				else
				{
					_dialog.PageRangeSelection = PageRangeSelection.AllPages;
				}
			}
			return num;
		}

		internal void SyncToStruct()
		{
			if (_unmanagedPrintDlgEx != IntPtr.Zero)
			{
				FreeUnmanagedPrintDlgExStruct(_unmanagedPrintDlgEx);
			}
			if (_ownerHandle == IntPtr.Zero)
			{
				_ownerHandle = MS.Win32.UnsafeNativeMethods.GetDesktopWindow();
			}
			_unmanagedPrintDlgEx = AllocateUnmanagedPrintDlgExStruct();
		}

		private void Dispose(bool disposing)
		{
			if (disposing && _unmanagedPrintDlgEx != IntPtr.Zero)
			{
				FreeUnmanagedPrintDlgExStruct(_unmanagedPrintDlgEx);
				_unmanagedPrintDlgEx = IntPtr.Zero;
			}
		}

		private void ExtractPrintDataAndDevMode(nint unmanagedBuffer, out string printerName, out uint flags, out PageRange pageRange, out nint devModeHandle)
		{
			nint zero = IntPtr.Zero;
			nint zero2 = IntPtr.Zero;
			if (!Is64Bit())
			{
				NativeMethods.PRINTDLGEX32 pRINTDLGEX = Marshal.PtrToStructure<NativeMethods.PRINTDLGEX32>(unmanagedBuffer);
				devModeHandle = pRINTDLGEX.hDevMode;
				zero = pRINTDLGEX.hDevNames;
				flags = pRINTDLGEX.Flags;
				zero2 = pRINTDLGEX.lpPageRanges;
			}
			else
			{
				NativeMethods.PRINTDLGEX64 pRINTDLGEX2 = Marshal.PtrToStructure<NativeMethods.PRINTDLGEX64>(unmanagedBuffer);
				devModeHandle = pRINTDLGEX2.hDevMode;
				zero = pRINTDLGEX2.hDevNames;
				flags = pRINTDLGEX2.Flags;
				zero2 = pRINTDLGEX2.lpPageRanges;
			}
			if ((flags & 2) == 2 && zero2 != IntPtr.Zero)
			{
				NativeMethods.PRINTPAGERANGE pRINTPAGERANGE = Marshal.PtrToStructure<NativeMethods.PRINTPAGERANGE>(zero2);
				pageRange = new PageRange((int)pRINTPAGERANGE.nFromPage, (int)pRINTPAGERANGE.nToPage);
			}
			else
			{
				pageRange = new PageRange(1);
			}
			if (zero != IntPtr.Zero)
			{
				nint num = IntPtr.Zero;
				try
				{
					num = UnsafeNativeMethods.GlobalLock(zero);
					int num2 = checked(Marshal.PtrToStructure<NativeMethods.DEVNAMES>(num).wDeviceOffset * Marshal.SystemDefaultCharSize);
					printerName = Marshal.PtrToStringAuto(num + num2);
					return;
				}
				finally
				{
					if (num != IntPtr.Zero)
					{
						UnsafeNativeMethods.GlobalUnlock(zero);
					}
				}
			}
			printerName = string.Empty;
		}

		private PrintQueue AcquirePrintQueue(string printerName)
		{
			PrintQueue printQueue = null;
			EnumeratedPrintQueueTypes[] enumerationFlag = new EnumeratedPrintQueueTypes[2]
			{
				EnumeratedPrintQueueTypes.Local,
				EnumeratedPrintQueueTypes.Connections
			};
			PrintQueueIndexedProperty[] propertiesFilter = new PrintQueueIndexedProperty[2]
			{
				PrintQueueIndexedProperty.Name,
				PrintQueueIndexedProperty.QueueAttributes
			};
			using (LocalPrintServer localPrintServer = new LocalPrintServer())
			{
				foreach (PrintQueue printQueue2 in localPrintServer.GetPrintQueues(propertiesFilter, enumerationFlag))
				{
					if (printerName.Equals(printQueue2.FullName, StringComparison.OrdinalIgnoreCase))
					{
						printQueue = printQueue2;
						break;
					}
				}
			}
			if (printQueue != null)
			{
				printQueue.InPartialTrust = true;
			}
			return printQueue;
		}

		private PrintTicket AcquirePrintTicket(nint devModeHandle, string printQueueName)
		{
			PrintTicket printTicket = null;
			byte[] array = null;
			nint num = IntPtr.Zero;
			try
			{
				num = UnsafeNativeMethods.GlobalLock(devModeHandle);
				NativeMethods.DEVMODE dEVMODE = Marshal.PtrToStructure<NativeMethods.DEVMODE>(num);
				array = new byte[dEVMODE.dmSize + dEVMODE.dmDriverExtra];
				Marshal.Copy(num, array, 0, array.Length);
			}
			finally
			{
				if (num != IntPtr.Zero)
				{
					UnsafeNativeMethods.GlobalUnlock(devModeHandle);
				}
			}
			using PrintTicketConverter printTicketConverter = new PrintTicketConverter(printQueueName, PrintTicketConverter.MaxPrintSchemaVersion);
			return printTicketConverter.ConvertDevModeToPrintTicket(array);
		}

		private uint AcquireResultFromPrintDlgExStruct(nint unmanagedBuffer)
		{
			uint num = 0u;
			if (!Is64Bit())
			{
				return Marshal.PtrToStructure<NativeMethods.PRINTDLGEX32>(unmanagedBuffer).dwResultAction;
			}
			return Marshal.PtrToStructure<NativeMethods.PRINTDLGEX64>(unmanagedBuffer).dwResultAction;
		}

		private nint AllocateUnmanagedPrintDlgExStruct()
		{
			nint num = IntPtr.Zero;
			NativeMethods.PRINTPAGERANGE structure = default(NativeMethods.PRINTPAGERANGE);
			structure.nToPage = (uint)_dialog.PageRange.PageTo;
			structure.nFromPage = (uint)_dialog.PageRange.PageFrom;
			uint flags = 1835008u;
			try
			{
				if (!Is64Bit())
				{
					NativeMethods.PRINTDLGEX32 pRINTDLGEX = new NativeMethods.PRINTDLGEX32();
					pRINTDLGEX.hwndOwner = _ownerHandle;
					pRINTDLGEX.nMinPage = _dialog.MinPage;
					pRINTDLGEX.nMaxPage = _dialog.MaxPage;
					pRINTDLGEX.Flags = flags;
					if (_dialog.SelectedPagesEnabled)
					{
						if (_dialog.PageRangeSelection == PageRangeSelection.SelectedPages)
						{
							pRINTDLGEX.Flags |= 1u;
						}
					}
					else
					{
						pRINTDLGEX.Flags |= 4u;
					}
					if (_dialog.CurrentPageEnabled)
					{
						if (_dialog.PageRangeSelection == PageRangeSelection.CurrentPage)
						{
							pRINTDLGEX.Flags |= 4194304u;
						}
					}
					else
					{
						pRINTDLGEX.Flags |= 8388608u;
					}
					if (_dialog.PageRangeEnabled)
					{
						pRINTDLGEX.lpPageRanges = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeMethods.PRINTPAGERANGE)));
						pRINTDLGEX.nMaxPageRanges = 1u;
						if (_dialog.PageRangeSelection == PageRangeSelection.UserPages)
						{
							pRINTDLGEX.nPageRanges = 1u;
							Marshal.StructureToPtr(structure, pRINTDLGEX.lpPageRanges, fDeleteOld: false);
							pRINTDLGEX.Flags |= 2u;
						}
						else
						{
							pRINTDLGEX.nPageRanges = 0u;
						}
					}
					else
					{
						pRINTDLGEX.lpPageRanges = IntPtr.Zero;
						pRINTDLGEX.nMaxPageRanges = 0u;
						pRINTDLGEX.Flags |= 8u;
					}
					if (_dialog.PrintQueue != null)
					{
						pRINTDLGEX.hDevNames = AllocateAndInitializeDevNames(_dialog.PrintQueue.FullName);
						if (_dialog.PrintTicket != null)
						{
							pRINTDLGEX.hDevMode = AllocateAndInitializeDevMode(_dialog.PrintQueue.FullName, _dialog.PrintTicket);
						}
					}
					num = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeMethods.PRINTDLGEX32)));
					Marshal.StructureToPtr(pRINTDLGEX, num, fDeleteOld: false);
				}
				else
				{
					NativeMethods.PRINTDLGEX64 pRINTDLGEX2 = new NativeMethods.PRINTDLGEX64();
					pRINTDLGEX2.hwndOwner = _ownerHandle;
					pRINTDLGEX2.nMinPage = _dialog.MinPage;
					pRINTDLGEX2.nMaxPage = _dialog.MaxPage;
					pRINTDLGEX2.Flags = flags;
					if (_dialog.SelectedPagesEnabled)
					{
						if (_dialog.PageRangeSelection == PageRangeSelection.SelectedPages)
						{
							pRINTDLGEX2.Flags |= 1u;
						}
					}
					else
					{
						pRINTDLGEX2.Flags |= 4u;
					}
					if (_dialog.CurrentPageEnabled)
					{
						if (_dialog.PageRangeSelection == PageRangeSelection.CurrentPage)
						{
							pRINTDLGEX2.Flags |= 4194304u;
						}
					}
					else
					{
						pRINTDLGEX2.Flags |= 8388608u;
					}
					if (_dialog.PageRangeEnabled)
					{
						pRINTDLGEX2.lpPageRanges = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeMethods.PRINTPAGERANGE)));
						pRINTDLGEX2.nMaxPageRanges = 1u;
						if (_dialog.PageRangeSelection == PageRangeSelection.UserPages)
						{
							pRINTDLGEX2.nPageRanges = 1u;
							Marshal.StructureToPtr(structure, pRINTDLGEX2.lpPageRanges, fDeleteOld: false);
							pRINTDLGEX2.Flags |= 2u;
						}
						else
						{
							pRINTDLGEX2.nPageRanges = 0u;
						}
					}
					else
					{
						pRINTDLGEX2.lpPageRanges = IntPtr.Zero;
						pRINTDLGEX2.nMaxPageRanges = 0u;
						pRINTDLGEX2.Flags |= 8u;
					}
					if (_dialog.PrintQueue != null)
					{
						pRINTDLGEX2.hDevNames = AllocateAndInitializeDevNames(_dialog.PrintQueue.FullName);
						if (_dialog.PrintTicket != null)
						{
							pRINTDLGEX2.hDevMode = AllocateAndInitializeDevMode(_dialog.PrintQueue.FullName, _dialog.PrintTicket);
						}
					}
					num = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeMethods.PRINTDLGEX64)));
					Marshal.StructureToPtr(pRINTDLGEX2, num, fDeleteOld: false);
				}
			}
			catch (Exception)
			{
				if (num != IntPtr.Zero)
				{
					FreeUnmanagedPrintDlgExStruct(num);
					num = IntPtr.Zero;
				}
				throw;
			}
			return num;
		}

		private void FreeUnmanagedPrintDlgExStruct(nint unmanagedBuffer)
		{
			if (unmanagedBuffer != IntPtr.Zero)
			{
				nint zero = IntPtr.Zero;
				nint zero2 = IntPtr.Zero;
				nint zero3 = IntPtr.Zero;
				if (!Is64Bit())
				{
					NativeMethods.PRINTDLGEX32? pRINTDLGEX = Marshal.PtrToStructure<NativeMethods.PRINTDLGEX32>(unmanagedBuffer);
					zero = pRINTDLGEX.hDevMode;
					zero2 = pRINTDLGEX.hDevNames;
					zero3 = pRINTDLGEX.lpPageRanges;
				}
				else
				{
					NativeMethods.PRINTDLGEX64? pRINTDLGEX2 = Marshal.PtrToStructure<NativeMethods.PRINTDLGEX64>(unmanagedBuffer);
					zero = pRINTDLGEX2.hDevMode;
					zero2 = pRINTDLGEX2.hDevNames;
					zero3 = pRINTDLGEX2.lpPageRanges;
				}
				if (zero != IntPtr.Zero)
				{
					UnsafeNativeMethods.GlobalFree(zero);
				}
				if (zero2 != IntPtr.Zero)
				{
					UnsafeNativeMethods.GlobalFree(zero2);
				}
				if (zero3 != IntPtr.Zero)
				{
					UnsafeNativeMethods.GlobalFree(zero3);
				}
				Marshal.FreeHGlobal(unmanagedBuffer);
			}
		}

		private bool Is64Bit()
		{
			return Marshal.SizeOf(IntPtr.Zero) == 8;
		}

		private nint AllocateAndInitializeDevNames(string printerName)
		{
			nint zero = IntPtr.Zero;
			char[] array = printerName.ToCharArray();
			zero = Marshal.AllocHGlobal(checked((array.Length + 3) * Marshal.SystemDefaultCharSize + Marshal.SizeOf(typeof(NativeMethods.DEVNAMES))));
			ushort num = (ushort)Marshal.SizeOf(typeof(NativeMethods.DEVNAMES));
			NativeMethods.DEVNAMES structure = default(NativeMethods.DEVNAMES);
			structure.wDeviceOffset = (ushort)(num / Marshal.SystemDefaultCharSize);
			nint num2;
			nint destination;
			checked
			{
				structure.wDriverOffset = (ushort)(structure.wDeviceOffset + array.Length + 1);
				structure.wOutputOffset = (ushort)(structure.wDriverOffset + 1);
				structure.wDefault = 0;
				Marshal.StructureToPtr(structure, zero, fDeleteOld: false);
				num2 = (nint)(unchecked((long)zero) + unchecked((long)num));
				destination = (nint)(unchecked((long)num2) + unchecked((long)checked(array.Length * Marshal.SystemDefaultCharSize)));
			}
			byte[] array2 = new byte[3 * Marshal.SystemDefaultCharSize];
			Array.Clear(array2, 0, array2.Length);
			Marshal.Copy(array, 0, num2, array.Length);
			Marshal.Copy(array2, 0, destination, array2.Length);
			return zero;
		}

		private nint AllocateAndInitializeDevMode(string printerName, PrintTicket printTicket)
		{
			byte[] array = null;
			using (PrintTicketConverter printTicketConverter = new PrintTicketConverter(printerName, PrintTicketConverter.MaxPrintSchemaVersion))
			{
				array = printTicketConverter.ConvertPrintTicketToDevMode(printTicket, BaseDevModeType.UserDefault);
			}
			nint num = Marshal.AllocHGlobal(array.Length);
			Marshal.Copy(array, 0, num, array.Length);
			return num;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}

	private PrintTicket _printTicket;

	private PrintQueue _printQueue;

	private PageRangeSelection _pageRangeSelection;

	private PageRange _pageRange;

	private bool _pageRangeEnabled;

	private bool _selectedPagesEnabled;

	private bool _currentPageEnabled;

	private uint _minPage;

	private uint _maxPage;

	private const char RightToLeftMark = '\u200f';

	internal PrintTicket PrintTicket
	{
		get
		{
			return _printTicket;
		}
		set
		{
			_printTicket = value;
		}
	}

	internal PrintQueue PrintQueue
	{
		get
		{
			return _printQueue;
		}
		set
		{
			_printQueue = value;
		}
	}

	internal uint MinPage
	{
		get
		{
			return _minPage;
		}
		set
		{
			_minPage = value;
		}
	}

	internal uint MaxPage
	{
		get
		{
			return _maxPage;
		}
		set
		{
			_maxPage = value;
		}
	}

	internal PageRangeSelection PageRangeSelection
	{
		get
		{
			return _pageRangeSelection;
		}
		set
		{
			_pageRangeSelection = value;
		}
	}

	internal PageRange PageRange
	{
		get
		{
			return _pageRange;
		}
		set
		{
			_pageRange = value;
		}
	}

	internal bool PageRangeEnabled
	{
		get
		{
			return _pageRangeEnabled;
		}
		set
		{
			_pageRangeEnabled = value;
		}
	}

	internal bool SelectedPagesEnabled
	{
		get
		{
			return _selectedPagesEnabled;
		}
		set
		{
			_selectedPagesEnabled = value;
		}
	}

	internal bool CurrentPageEnabled
	{
		get
		{
			return _currentPageEnabled;
		}
		set
		{
			_currentPageEnabled = value;
		}
	}

	public Win32PrintDialog()
	{
		_printTicket = null;
		_printQueue = null;
		_minPage = 1u;
		_maxPage = 9999u;
		_pageRangeSelection = PageRangeSelection.AllPages;
	}

	internal uint ShowDialog()
	{
		uint result = 0u;
		nint num = IntPtr.Zero;
		if (Application.Current != null && Application.Current.MainWindow != null)
		{
			num = new WindowInteropHelper(Application.Current.MainWindow).CriticalHandle;
		}
		try
		{
			if (_printQueue == null || _printTicket == null)
			{
				ProbeForPrintingSupport();
			}
			using PrintDlgExMarshaler printDlgExMarshaler = new PrintDlgExMarshaler(num, this);
			printDlgExMarshaler.SyncToStruct();
			if (UnsafeNativeMethods.PrintDlgEx(printDlgExMarshaler.UnmanagedPrintDlgEx) == 0)
			{
				result = printDlgExMarshaler.SyncFromStruct();
			}
		}
		catch (Exception ex)
		{
			if (!string.Equals(ex.GetType().FullName, "System.Printing.PrintingNotSupportedException", StringComparison.Ordinal))
			{
				throw;
			}
			string printDialogInstallPrintSupportMessageBox = SR.PrintDialogInstallPrintSupportMessageBox;
			string printDialogInstallPrintSupportCaption = SR.PrintDialogInstallPrintSupportCaption;
			MessageBoxOptions messageBoxOptions = ((printDialogInstallPrintSupportCaption != null && printDialogInstallPrintSupportCaption.Length > 0 && printDialogInstallPrintSupportCaption[0] == '\u200f') ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
			int type = (int)((MessageBoxOptions)64 | messageBoxOptions);
			if (num == IntPtr.Zero)
			{
				num = MS.Win32.UnsafeNativeMethods.GetActiveWindow();
			}
			if (MS.Win32.UnsafeNativeMethods.MessageBox(new HandleRef(null, num), printDialogInstallPrintSupportMessageBox, printDialogInstallPrintSupportCaption, type) != 0)
			{
				result = 0u;
			}
		}
		return result;
	}

	private void ProbeForPrintingSupport()
	{
		string deviceName = ((_printQueue != null) ? _printQueue.FullName : string.Empty);
		try
		{
			using (new PrintTicketConverter(deviceName, 1))
			{
			}
		}
		catch (PrintQueueException)
		{
		}
	}
}
