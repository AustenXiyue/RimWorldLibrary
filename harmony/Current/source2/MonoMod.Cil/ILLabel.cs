using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;

namespace MonoMod.Cil;

internal sealed class ILLabel
{
	private readonly ILContext Context;

	public Instruction? Target { get; set; }

	public IEnumerable<Instruction> Branches => Context.Instrs.Where((Instruction i) => i.Operand == this);

	internal ILLabel(ILContext context)
	{
		Context = context;
		Context._Labels.Add(this);
	}

	internal ILLabel(ILContext context, Instruction? target)
		: this(context)
	{
		Target = target;
	}
}
