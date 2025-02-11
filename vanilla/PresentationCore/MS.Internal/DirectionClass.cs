namespace MS.Internal;

internal enum DirectionClass : byte
{
	Left,
	Right,
	ArabicNumber,
	EuropeanNumber,
	ArabicLetter,
	EuropeanSeparator,
	CommonSeparator,
	EuropeanTerminator,
	NonSpacingMark,
	BoundaryNeutral,
	GenericNeutral,
	ParagraphSeparator,
	LeftToRightEmbedding,
	LeftToRightOverride,
	RightToLeftEmbedding,
	RightToLeftOverride,
	PopDirectionalFormat,
	SegmentSeparator,
	WhiteSpace,
	OtherNeutral,
	ClassInvalid,
	ClassMax
}
