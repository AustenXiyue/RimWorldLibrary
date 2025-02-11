namespace MS.Internal.TextFormatting;

internal interface ITextMetrics
{
	int Length { get; }

	int DependentLength { get; }

	int NewlineLength { get; }

	double Start { get; }

	double Width { get; }

	double WidthIncludingTrailingWhitespace { get; }

	double Height { get; }

	double MarkerHeight { get; }

	double Baseline { get; }

	double MarkerBaseline { get; }
}
