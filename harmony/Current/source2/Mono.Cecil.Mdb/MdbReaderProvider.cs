using System.IO;
using Mono.Cecil.Cil;
using Mono.CompilerServices.SymbolWriter;

namespace Mono.Cecil.Mdb;

internal sealed class MdbReaderProvider : ISymbolReaderProvider
{
	public ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName)
	{
		Mixin.CheckModule(module);
		Mixin.CheckFileName(fileName);
		return new MdbReader(module, MonoSymbolFile.ReadSymbolFile(Mixin.GetMdbFileName(fileName)));
	}

	public ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream)
	{
		Mixin.CheckModule(module);
		Mixin.CheckStream(symbolStream);
		return new MdbReader(module, MonoSymbolFile.ReadSymbolFile(symbolStream));
	}
}
