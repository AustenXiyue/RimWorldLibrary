using System.ComponentModel;
using System.Windows.Markup;

namespace System.Windows.Input;

/// <summary>Specifies the set of modifier keys. </summary>
[TypeConverter(typeof(ModifierKeysConverter))]
[ValueSerializer(typeof(ModifierKeysValueSerializer))]
[Flags]
public enum ModifierKeys
{
	/// <summary>No modifiers are pressed. </summary>
	None = 0,
	/// <summary>The ALT key. </summary>
	Alt = 1,
	/// <summary>The CTRL key. </summary>
	Control = 2,
	/// <summary>The SHIFT key. </summary>
	Shift = 4,
	/// <summary>The Windows logo key.</summary>
	Windows = 8
}
