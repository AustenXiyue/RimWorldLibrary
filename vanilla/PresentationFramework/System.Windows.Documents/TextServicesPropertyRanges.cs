using System.Runtime.InteropServices;
using MS.Win32;

namespace System.Windows.Documents;

internal class TextServicesPropertyRanges
{
	private Guid _guid;

	private TextStore _textstore;

	protected Guid Guid => _guid;

	protected TextStore TextStore => _textstore;

	internal TextServicesPropertyRanges(TextStore textstore, Guid guid)
	{
		_guid = guid;
		_textstore = textstore;
	}

	internal virtual void OnRange(MS.Win32.UnsafeNativeMethods.ITfProperty property, int ecReadonly, MS.Win32.UnsafeNativeMethods.ITfRange range)
	{
	}

	internal virtual void OnEndEdit(MS.Win32.UnsafeNativeMethods.ITfContext context, int ecReadOnly, MS.Win32.UnsafeNativeMethods.ITfEditRecord editRecord)
	{
		MS.Win32.UnsafeNativeMethods.ITfProperty property = null;
		MS.Win32.UnsafeNativeMethods.IEnumTfRanges propertyUpdate = GetPropertyUpdate(editRecord);
		MS.Win32.UnsafeNativeMethods.ITfRange[] array = new MS.Win32.UnsafeNativeMethods.ITfRange[1];
		int fetched;
		while (propertyUpdate.Next(1, array, out fetched) == 0)
		{
			ConvertToTextPosition(array[0], out var _, out var _);
			if (property == null)
			{
				context.GetProperty(ref _guid, out property);
			}
			if (property.EnumRanges(ecReadOnly, out var ranges, array[0]) == 0)
			{
				MS.Win32.UnsafeNativeMethods.ITfRange[] array2 = new MS.Win32.UnsafeNativeMethods.ITfRange[1];
				while (ranges.Next(1, array2, out fetched) == 0)
				{
					OnRange(property, ecReadOnly, array2[0]);
					Marshal.ReleaseComObject(array2[0]);
				}
				Marshal.ReleaseComObject(ranges);
			}
			Marshal.ReleaseComObject(array[0]);
		}
		Marshal.ReleaseComObject(propertyUpdate);
		if (property != null)
		{
			Marshal.ReleaseComObject(property);
		}
	}

	protected void ConvertToTextPosition(MS.Win32.UnsafeNativeMethods.ITfRange range, out ITextPointer start, out ITextPointer end)
	{
		(range as MS.Win32.UnsafeNativeMethods.ITfRangeACP).GetExtent(out var start2, out var count);
		if (count < 0)
		{
			start = null;
			end = null;
		}
		else
		{
			start = _textstore.CreatePointerAtCharOffset(start2, LogicalDirection.Forward);
			end = _textstore.CreatePointerAtCharOffset(start2 + count, LogicalDirection.Forward);
		}
	}

	protected static object GetValue(int ecReadOnly, MS.Win32.UnsafeNativeMethods.ITfProperty property, MS.Win32.UnsafeNativeMethods.ITfRange range)
	{
		if (property == null)
		{
			return null;
		}
		property.GetValue(ecReadOnly, range, out var value);
		return value;
	}

	private unsafe MS.Win32.UnsafeNativeMethods.IEnumTfRanges GetPropertyUpdate(MS.Win32.UnsafeNativeMethods.ITfEditRecord editRecord)
	{
		MS.Win32.UnsafeNativeMethods.IEnumTfRanges ranges;
		fixed (Guid* guid = &_guid)
		{
			nint properties = (nint)guid;
			editRecord.GetTextAndPropertyUpdates(0, ref properties, 1, out ranges);
		}
		return ranges;
	}
}
