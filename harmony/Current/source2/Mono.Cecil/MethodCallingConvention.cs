namespace Mono.Cecil;

internal enum MethodCallingConvention : byte
{
	Default = 0,
	C = 1,
	StdCall = 2,
	ThisCall = 3,
	FastCall = 4,
	VarArg = 5,
	Unmanaged = 9,
	Generic = 16
}
