using System;

namespace Mono.Cecil.Cil;

internal interface ISymbolReader : IDisposable
{
	ISymbolWriterProvider GetWriterProvider();

	bool ProcessDebugHeader(ImageDebugHeader header);

	MethodDebugInformation Read(MethodDefinition method);
}
