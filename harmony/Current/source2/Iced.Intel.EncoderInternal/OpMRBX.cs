namespace Iced.Intel.EncoderInternal;

internal sealed class OpMRBX : Op
{
	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		if (encoder.Verify(operand, OpKind.Memory, instruction.GetOpKind(operand)))
		{
			Register memoryBase = instruction.MemoryBase;
			if (instruction.MemoryDisplSize != 0 || instruction.MemoryDisplacement64 != 0L || instruction.MemoryIndexScale != 1 || instruction.MemoryIndex != Register.AL || (memoryBase != Register.BX && memoryBase != Register.EBX && memoryBase != Register.RBX))
			{
				encoder.ErrorMessage = $"Operand {operand}: Operand must be [bx+al], [ebx+al], or [rbx+al]";
			}
			else
			{
				encoder.SetAddrSize(memoryBase switch
				{
					Register.RBX => 8, 
					Register.EBX => 4, 
					_ => 2, 
				});
			}
		}
	}
}
