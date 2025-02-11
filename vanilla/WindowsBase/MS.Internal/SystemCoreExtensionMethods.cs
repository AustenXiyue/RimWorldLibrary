using System;

namespace MS.Internal;

internal abstract class SystemCoreExtensionMethods
{
	internal abstract bool IsIDynamicMetaObjectProvider(object item);

	internal abstract object NewDynamicPropertyAccessor(Type ownerType, string propertyName);

	internal abstract object GetIndexerAccessor(int rank);
}
