using System.IO;
using Mono.Cecil.Cil;

namespace Mono.Cecil.Pdb;

internal sealed class PdbReaderProvider : ISymbolReaderProvider
{
	public ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName)
	{
		Mixin.CheckModule(module);
		if (module.HasDebugHeader && module.GetDebugHeader().GetEmbeddedPortablePdbEntry() != null)
		{
			return new EmbeddedPortablePdbReaderProvider().GetSymbolReader(module, fileName);
		}
		Mixin.CheckFileName(fileName);
		if (!Mixin.IsPortablePdb(Mixin.GetPdbFileName(fileName)))
		{
			return new NativePdbReaderProvider().GetSymbolReader(module, fileName);
		}
		return new PortablePdbReaderProvider().GetSymbolReader(module, fileName);
	}

	public ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream)
	{
		Mixin.CheckModule(module);
		Mixin.CheckStream(symbolStream);
		Mixin.CheckReadSeek(symbolStream);
		if (!Mixin.IsPortablePdb(symbolStream))
		{
			return new NativePdbReaderProvider().GetSymbolReader(module, symbolStream);
		}
		return new PortablePdbReaderProvider().GetSymbolReader(module, symbolStream);
	}
}
