namespace MS.Internal.Interop;

internal struct STAT_CHUNK
{
	internal uint idChunk;

	internal CHUNK_BREAKTYPE breakType;

	internal CHUNKSTATE flags;

	internal uint locale;

	internal FULLPROPSPEC attribute;

	internal uint idChunkSource;

	internal uint cwcStartSource;

	internal uint cwcLenSource;
}
