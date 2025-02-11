using System;
using System.ComponentModel;

namespace MS.Internal;

internal abstract class SystemDataExtensionMethods
{
	internal abstract bool IsDataView(IBindingList list);

	internal abstract bool IsDataRowView(object item);

	internal abstract bool IsSqlNull(object value);

	internal abstract bool IsSqlNullableType(Type type);

	internal abstract bool IsDataSetCollectionProperty(PropertyDescriptor pd);

	internal abstract object GetValue(object item, PropertyDescriptor pd, bool useFollowParent);

	internal abstract bool DetermineWhetherDBNullIsValid(object item, string columnName, object arg);
}
