using System;
using System.Diagnostics;

namespace Iced.Intel;

internal sealed class InstructionListDebugView
{
	private readonly InstructionList list;

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public Instruction[] Items => list.ToArray();

	public InstructionListDebugView(InstructionList list)
	{
		this.list = list ?? throw new ArgumentNullException("list");
	}
}
