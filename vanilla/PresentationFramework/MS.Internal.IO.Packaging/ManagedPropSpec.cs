using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace MS.Internal.IO.Packaging;

internal class ManagedPropSpec
{
	private PropSpecType _propType;

	private uint _id;

	private string _name;

	internal PropSpecType PropType => _propType;

	internal string PropName
	{
		get
		{
			return _name;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_name = value;
			_id = 0u;
			_propType = PropSpecType.Name;
		}
	}

	internal uint PropId
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
			_name = null;
			_propType = PropSpecType.Id;
		}
	}

	internal ManagedPropSpec(uint id)
	{
		PropId = id;
	}

	internal ManagedPropSpec(PROPSPEC propSpec)
	{
		switch ((PropSpecType)propSpec.propType)
		{
		case PropSpecType.Id:
			PropId = propSpec.union.propId;
			break;
		case PropSpecType.Name:
			PropName = Marshal.PtrToStringUni(propSpec.union.name);
			break;
		default:
			throw new ArgumentException(SR.FilterPropSpecUnknownUnionSelector, "propSpec");
		}
	}
}
