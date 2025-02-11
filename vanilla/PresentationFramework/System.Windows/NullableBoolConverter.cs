using System.Collections;
using System.ComponentModel;

namespace System.Windows;

/// <summary>Converts to and from the <see cref="T:System.Nullable`1" /> type (using the <see cref="T:System.Boolean" /> type constraint on the generic). </summary>
public class NullableBoolConverter : NullableConverter
{
	[ThreadStatic]
	private static StandardValuesCollection _standardValues;

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.NullableBoolConverter" />  class. </summary>
	public NullableBoolConverter()
		: base(typeof(bool?))
	{
	}

	/// <summary>Returns whether this object supports a standard set of values that can be picked from a list. </summary>
	/// <returns>This implementation always returns true.</returns>
	/// <param name="context">Provides contextual information about a component, such as its container and property descriptor.</param>
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	/// <summary>Returns whether the collection of standard values returned from <see cref="M:System.Windows.NullableBoolConverter.GetStandardValues(System.ComponentModel.ITypeDescriptorContext)" /> is an exclusive list. </summary>
	/// <returns>This implementation always returns true.</returns>
	/// <param name="context">Provides contextual information about a component, such as its container and property descriptor.</param>
	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		return true;
	}

	/// <summary>Returns a collection of standard values for the data type that this type converter is designed for. </summary>
	/// <returns>A collection that holds a standard set of valid values. For this implementation, those values are true, false, and null.</returns>
	/// <param name="context">Provides contextual information about a component, such as its container and property descriptor. </param>
	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	{
		if (_standardValues == null)
		{
			_standardValues = new StandardValuesCollection(new ArrayList(3) { true, false, null }.ToArray());
		}
		return _standardValues;
	}
}
