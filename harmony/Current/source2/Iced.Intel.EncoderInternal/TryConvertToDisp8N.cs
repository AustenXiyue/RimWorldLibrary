namespace Iced.Intel.EncoderInternal;

internal delegate bool TryConvertToDisp8N(Encoder encoder, OpCodeHandler handler, in Instruction instruction, int displ, out sbyte compressedValue);
