namespace Mono.Cecil.Cil;

internal sealed class VariableDefinition : VariableReference
{
	public bool IsPinned => variable_type.IsPinned;

	public VariableDefinition(TypeReference variableType)
		: base(variableType)
	{
	}

	public override VariableDefinition Resolve()
	{
		return this;
	}
}
