using System.ComponentModel;
using MS.Internal.WindowsBase;

namespace System.Windows.Markup;

/// <summary>Specifies the serialization flags for a property.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class DesignerSerializationOptionsAttribute : Attribute
{
	private DesignerSerializationOptions _designerSerializationOptions;

	/// <summary>Gets the <see cref="T:System.Windows.Markup.DesignerSerializationOptions" /> set on the attribute.</summary>
	/// <returns>The serialization option, as a value of the enumeration.</returns>
	public DesignerSerializationOptions DesignerSerializationOptions => _designerSerializationOptions;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.DesignerSerializationOptionsAttribute" /> class.</summary>
	/// <param name="designerSerializationOptions">Specifies how the property is to be serialized. </param>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="designerSerializationOptions" /> is not a valid <see cref="T:System.Windows.Markup.DesignerSerializationOptions" /> value.</exception>
	public DesignerSerializationOptionsAttribute(DesignerSerializationOptions designerSerializationOptions)
	{
		if (DesignerSerializationOptions.SerializeAsAttribute == designerSerializationOptions)
		{
			_designerSerializationOptions = designerSerializationOptions;
			return;
		}
		throw new InvalidEnumArgumentException(SR.Format(SR.Enum_Invalid, "DesignerSerializationOptions"));
	}
}
