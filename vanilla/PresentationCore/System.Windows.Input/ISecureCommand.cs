using System.ComponentModel;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

[FriendAccessAllowed]
[TypeConverter("System.Windows.Input.CommandConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
internal interface ISecureCommand : ICommand
{
}
