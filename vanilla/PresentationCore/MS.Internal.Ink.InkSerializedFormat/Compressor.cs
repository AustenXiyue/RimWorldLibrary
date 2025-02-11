using System;
using MS.Internal.PresentationCore;

namespace MS.Internal.Ink.InkSerializedFormat;

internal class Compressor
{
	[ThreadStatic]
	private static AlgoModule _algoModule;

	private static AlgoModule AlgoModule
	{
		get
		{
			if (_algoModule == null)
			{
				_algoModule = new AlgoModule();
			}
			return _algoModule;
		}
	}

	internal static void DecompressPacketData(byte[] compressedInput, ref uint size, int[] decompressedPackets)
	{
		if (compressedInput == null || size > compressedInput.Length || decompressedPackets == null)
		{
			throw new InvalidOperationException(StrokeCollectionSerializer.ISFDebugMessage(SR.DecompressPacketDataFailed));
		}
		size = AlgoModule.DecompressPacketData(compressedInput, decompressedPackets);
	}

	internal static byte[] DecompressPropertyData(byte[] input)
	{
		if (input == null)
		{
			throw new InvalidOperationException(StrokeCollectionSerializer.ISFDebugMessage(SR.DecompressPropertyFailed));
		}
		return AlgoModule.DecompressPropertyData(input);
	}

	internal static byte[] CompressPropertyData(byte[] data, byte algorithm)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return AlgoModule.CompressPropertyData(data, algorithm);
	}

	internal static byte[] CompressPacketData(int[] input, ref byte algorithm)
	{
		if (input == null)
		{
			throw new InvalidOperationException(SR.IsfOperationFailed);
		}
		return AlgoModule.CompressPacketData(input, algorithm);
	}
}
