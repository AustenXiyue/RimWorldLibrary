using System.Collections;
using System.Runtime.InteropServices;
using MS.Win32;

namespace System.Windows.Documents;

internal class TextServicesDisplayAttributePropertyRanges : TextServicesPropertyRanges
{
	private static Hashtable _attributes;

	private CompositionAdorner _compositionAdorner;

	internal TextServicesDisplayAttributePropertyRanges(TextStore textstore)
		: base(textstore, MS.Win32.UnsafeNativeMethods.GUID_PROP_ATTRIBUTE)
	{
	}

	internal override void OnRange(MS.Win32.UnsafeNativeMethods.ITfProperty property, int ecReadOnly, MS.Win32.UnsafeNativeMethods.ITfRange range)
	{
		int int32Value = GetInt32Value(ecReadOnly, property, range);
		if (int32Value != 0)
		{
			TextServicesDisplayAttribute displayAttribute = GetDisplayAttribute(int32Value);
			if (displayAttribute != null)
			{
				ConvertToTextPosition(range, out var start, out var end);
				displayAttribute.Apply(start, end);
			}
		}
	}

	internal override void OnEndEdit(MS.Win32.UnsafeNativeMethods.ITfContext context, int ecReadOnly, MS.Win32.UnsafeNativeMethods.ITfEditRecord editRecord)
	{
		if (_compositionAdorner != null)
		{
			_compositionAdorner.Uninitialize();
			_compositionAdorner = null;
		}
		Guid guid = base.Guid;
		context.GetProperty(ref guid, out var property);
		if (property.EnumRanges(ecReadOnly, out var ranges, null) == 0)
		{
			MS.Win32.UnsafeNativeMethods.ITfRange[] array = new MS.Win32.UnsafeNativeMethods.ITfRange[1];
			int fetched;
			while (ranges.Next(1, array, out fetched) == 0)
			{
				TextServicesDisplayAttribute displayAttribute = GetDisplayAttribute(GetInt32Value(ecReadOnly, property, array[0]));
				if (displayAttribute != null && !displayAttribute.IsEmptyAttribute())
				{
					ConvertToTextPosition(array[0], out var start, out var end);
					if (start != null)
					{
						if (_compositionAdorner == null)
						{
							_compositionAdorner = new CompositionAdorner(base.TextStore.TextView);
							_compositionAdorner.Initialize(base.TextStore.TextView);
						}
						_compositionAdorner.AddAttributeRange(start, end, displayAttribute);
					}
				}
				Marshal.ReleaseComObject(array[0]);
			}
			if (_compositionAdorner != null)
			{
				base.TextStore.RenderScope.UpdateLayout();
				_compositionAdorner.InvalidateAdorner();
			}
			Marshal.ReleaseComObject(ranges);
		}
		Marshal.ReleaseComObject(property);
	}

	internal void OnLayoutUpdated()
	{
		if (_compositionAdorner != null)
		{
			_compositionAdorner.InvalidateAdorner();
		}
	}

	private static TextServicesDisplayAttribute GetDisplayAttribute(int guidatom)
	{
		TextServicesDisplayAttribute textServicesDisplayAttribute = null;
		if (_attributes == null)
		{
			_attributes = new Hashtable();
		}
		textServicesDisplayAttribute = (TextServicesDisplayAttribute)_attributes[guidatom];
		if (textServicesDisplayAttribute != null)
		{
			return textServicesDisplayAttribute;
		}
		if (MS.Win32.UnsafeNativeMethods.TF_CreateCategoryMgr(out var catmgr) != 0)
		{
			return null;
		}
		if (catmgr == null)
		{
			return null;
		}
		catmgr.GetGUID(guidatom, out var guid);
		Marshal.ReleaseComObject(catmgr);
		if (guid.Equals(MS.Win32.UnsafeNativeMethods.Guid_Null))
		{
			return null;
		}
		if (MS.Win32.UnsafeNativeMethods.TF_CreateDisplayAttributeMgr(out var dam) != 0)
		{
			return null;
		}
		if (dam == null)
		{
			return null;
		}
		dam.GetDisplayAttributeInfo(ref guid, out var info, out var _);
		if (info != null)
		{
			info.GetAttributeInfo(out var attr);
			textServicesDisplayAttribute = new TextServicesDisplayAttribute(attr);
			Marshal.ReleaseComObject(info);
			_attributes[guidatom] = textServicesDisplayAttribute;
		}
		Marshal.ReleaseComObject(dam);
		return textServicesDisplayAttribute;
	}

	private int GetInt32Value(int ecReadOnly, MS.Win32.UnsafeNativeMethods.ITfProperty property, MS.Win32.UnsafeNativeMethods.ITfRange range)
	{
		object value = TextServicesPropertyRanges.GetValue(ecReadOnly, property, range);
		if (value == null)
		{
			return 0;
		}
		return (int)value;
	}
}
