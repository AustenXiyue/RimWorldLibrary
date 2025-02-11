using MS.Internal.WindowsBase;

namespace System.Windows.Markup;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal delegate bool IsXmlNamespaceSupportedCallback(string xmlNamespace, out string newXmlNamespace);
