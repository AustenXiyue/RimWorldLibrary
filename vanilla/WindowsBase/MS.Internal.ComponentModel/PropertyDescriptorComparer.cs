using System.Collections.Generic;
using System.ComponentModel;

namespace MS.Internal.ComponentModel;

internal class PropertyDescriptorComparer : IEqualityComparer<PropertyDescriptor>
{
	public bool Equals(PropertyDescriptor p1, PropertyDescriptor p2)
	{
		return p1 == p2;
	}

	public int GetHashCode(PropertyDescriptor p)
	{
		return p.GetHashCode();
	}
}
