using System.Text;

namespace MS.Internal;

internal class DecoderFallbackWithFailureFlag : DecoderFallback
{
	private class FallbackBuffer : DecoderFallbackBuffer
	{
		private DecoderFallbackWithFailureFlag _parent;

		public override int Remaining => 0;

		public FallbackBuffer(DecoderFallbackWithFailureFlag parent)
		{
			_parent = parent;
		}

		public override bool Fallback(byte[] bytesUnknown, int index)
		{
			_parent.HasFailed = true;
			return false;
		}

		public override char GetNextChar()
		{
			return '\0';
		}

		public override bool MovePrevious()
		{
			return false;
		}
	}

	private bool _hasFailed;

	public override int MaxCharCount => 0;

	public bool HasFailed
	{
		get
		{
			return _hasFailed;
		}
		set
		{
			_hasFailed = value;
		}
	}

	public override DecoderFallbackBuffer CreateFallbackBuffer()
	{
		return new FallbackBuffer(this);
	}
}
