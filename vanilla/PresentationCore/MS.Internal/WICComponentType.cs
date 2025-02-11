namespace MS.Internal;

internal enum WICComponentType
{
	WICDecoder = 1,
	WICEncoder = 2,
	WICFormat = 4,
	WICFormatConverter = 8,
	WICMetadataReader = 0x10,
	WICMetadataWriter = 0x20
}
