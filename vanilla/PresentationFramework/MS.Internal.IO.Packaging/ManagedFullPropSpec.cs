using System;
using MS.Internal.Interop;

namespace MS.Internal.IO.Packaging;

internal class ManagedFullPropSpec
{
	private Guid _guid;

	private ManagedPropSpec _property;

	internal Guid Guid => _guid;

	internal ManagedPropSpec Property => _property;

	internal ManagedFullPropSpec(Guid guid, uint propId)
	{
		_guid = guid;
		_property = new ManagedPropSpec(propId);
	}

	internal ManagedFullPropSpec(FULLPROPSPEC nativePropSpec)
	{
		_guid = nativePropSpec.guid;
		_property = new ManagedPropSpec(nativePropSpec.property);
	}
}
