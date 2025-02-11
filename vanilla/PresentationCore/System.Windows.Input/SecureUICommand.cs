using System.ComponentModel;

namespace System.Windows.Input;

[TypeConverter("System.Windows.Input.CommandConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
internal class SecureUICommand : RoutedUICommand, ISecureCommand, ICommand
{
	internal SecureUICommand(string name, Type ownerType, byte commandId)
		: base(name, ownerType, commandId)
	{
	}
}
