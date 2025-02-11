using System.Reflection;

namespace System.Windows.Markup;

internal class XamlClrEventNode : XamlAttributeNode
{
	internal XamlClrEventNode(int lineNumber, int linePosition, int depth, string eventName, MemberInfo eventMember, string value)
		: base(XamlNodeType.ClrEvent, lineNumber, linePosition, depth, value)
	{
	}
}
