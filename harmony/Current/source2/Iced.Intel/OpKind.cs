namespace Iced.Intel;

internal enum OpKind
{
	Register,
	NearBranch16,
	NearBranch32,
	NearBranch64,
	FarBranch16,
	FarBranch32,
	Immediate8,
	Immediate8_2nd,
	Immediate16,
	Immediate32,
	Immediate64,
	Immediate8to16,
	Immediate8to32,
	Immediate8to64,
	Immediate32to64,
	MemorySegSI,
	MemorySegESI,
	MemorySegRSI,
	MemorySegDI,
	MemorySegEDI,
	MemorySegRDI,
	MemoryESDI,
	MemoryESEDI,
	MemoryESRDI,
	Memory
}
