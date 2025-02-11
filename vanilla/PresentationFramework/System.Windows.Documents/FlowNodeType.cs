namespace System.Windows.Documents;

internal enum FlowNodeType : byte
{
	Boundary = 0,
	Start = 1,
	Run = 2,
	End = 4,
	Object = 8,
	Virtual = 0x10,
	Noop = 0x20
}
