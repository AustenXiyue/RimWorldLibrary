namespace Iced.Intel.BlockEncoderInternal;

internal sealed class CodeWriterImpl : CodeWriter
{
	public uint BytesWritten;

	private readonly CodeWriter codeWriter;

	public CodeWriterImpl(CodeWriter codeWriter)
	{
		if (codeWriter == null)
		{
			ThrowHelper.ThrowArgumentNullException_codeWriter();
		}
		this.codeWriter = codeWriter;
	}

	public override void WriteByte(byte value)
	{
		BytesWritten++;
		codeWriter.WriteByte(value);
	}
}
