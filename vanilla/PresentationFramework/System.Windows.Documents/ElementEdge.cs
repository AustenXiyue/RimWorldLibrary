namespace System.Windows.Documents;

[Flags]
internal enum ElementEdge : byte
{
	BeforeStart = 1,
	AfterStart = 2,
	BeforeEnd = 4,
	AfterEnd = 8
}
