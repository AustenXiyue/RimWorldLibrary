using System.ComponentModel;

namespace MS.Internal;

internal abstract class SystemXmlLinqExtensionMethods
{
	internal abstract bool IsXElement(object item);

	internal abstract string GetXElementTagName(object item);

	internal abstract bool IsXLinqCollectionProperty(PropertyDescriptor pd);

	internal abstract bool IsXLinqNonIdempotentProperty(PropertyDescriptor pd);
}
