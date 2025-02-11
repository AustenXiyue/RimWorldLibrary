using System.Windows;
using MS.Internal.WindowsBase;

namespace MS.Internal;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal abstract class DefaultValueFactory
{
	internal abstract object DefaultValue { get; }

	internal abstract object CreateDefaultValue(DependencyObject owner, DependencyProperty property);
}
