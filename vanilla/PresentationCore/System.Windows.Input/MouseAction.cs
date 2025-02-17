using System.ComponentModel;
using System.Windows.Markup;

namespace System.Windows.Input;

/// <summary>Specifies constants that define actions performed by the mouse.</summary>
[TypeConverter(typeof(MouseActionConverter))]
[ValueSerializer(typeof(MouseActionValueSerializer))]
public enum MouseAction : byte
{
	/// <summary>No action. </summary>
	None,
	/// <summary>A left mouse button click.</summary>
	LeftClick,
	/// <summary>A right mouse button click. </summary>
	RightClick,
	/// <summary>A middle mouse button click.</summary>
	MiddleClick,
	/// <summary>A mouse wheel rotation. </summary>
	WheelClick,
	/// <summary>A left mouse button double-click.</summary>
	LeftDoubleClick,
	/// <summary>A right mouse button double-click.</summary>
	RightDoubleClick,
	/// <summary>A middle mouse button double-click.</summary>
	MiddleDoubleClick
}
