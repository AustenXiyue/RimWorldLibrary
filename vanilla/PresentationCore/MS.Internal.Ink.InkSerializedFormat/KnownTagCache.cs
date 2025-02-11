namespace MS.Internal.Ink.InkSerializedFormat;

internal static class KnownTagCache
{
	internal enum KnownTagIndex : uint
	{
		Unknown = 0u,
		InkSpaceRectangle = 0u,
		GuidTable = 1u,
		DrawingAttributesTable = 2u,
		DrawingAttributesBlock = 3u,
		StrokeDescriptorTable = 4u,
		StrokeDescriptorBlock = 5u,
		Buttons = 6u,
		NoX = 7u,
		NoY = 8u,
		DrawingAttributesTableIndex = 9u,
		Stroke = 10u,
		StrokePropertyList = 11u,
		PointProperty = 12u,
		StrokeDescriptorTableIndex = 13u,
		CompressionHeader = 14u,
		TransformTable = 15u,
		Transform = 16u,
		TransformIsotropicScale = 17u,
		TransformAnisotropicScale = 18u,
		TransformRotate = 19u,
		TransformTranslate = 20u,
		TransformScaleAndTranslate = 21u,
		TransformQuad = 22u,
		TransformTableIndex = 23u,
		MetricTable = 24u,
		MetricBlock = 25u,
		MetricTableIndex = 26u,
		Mantissa = 27u,
		PersistenceFormat = 28u,
		HimetricSize = 29u,
		StrokeIds = 30u,
		ExtendedTransformTable = 31u
	}

	internal static uint MaximumPossibleKnownTags = 50u;

	internal static uint KnownTagCount = (byte)MaximumPossibleKnownTags;
}
