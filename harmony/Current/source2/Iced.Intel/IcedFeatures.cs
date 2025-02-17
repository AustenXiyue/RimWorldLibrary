using System.Runtime.CompilerServices;

namespace Iced.Intel;

internal static class IcedFeatures
{
	public static bool HasGasFormatter => false;

	public static bool HasIntelFormatter => false;

	public static bool HasMasmFormatter => false;

	public static bool HasNasmFormatter => false;

	public static bool HasFastFormatter => false;

	public static bool HasDecoder => true;

	public static bool HasEncoder => true;

	public static bool HasBlockEncoder => true;

	public static bool HasOpCodeInfo => false;

	public static bool HasInstructionInfo => false;

	public static void Initialize()
	{
		RuntimeHelpers.RunClassConstructor(typeof(Decoder).TypeHandle);
	}
}
