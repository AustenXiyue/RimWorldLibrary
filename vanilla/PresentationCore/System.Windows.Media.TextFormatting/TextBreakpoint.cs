using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Internal.TextFormatting;

namespace System.Windows.Media.TextFormatting;

[FriendAccessAllowed]
internal abstract class TextBreakpoint : ITextMetrics, IDisposable
{
	public abstract bool IsTruncated { get; }

	public abstract int Length { get; }

	public abstract int DependentLength { get; }

	public abstract int NewlineLength { get; }

	public abstract double Start { get; }

	public abstract double Width { get; }

	public abstract double WidthIncludingTrailingWhitespace { get; }

	public abstract double Height { get; }

	public abstract double TextHeight { get; }

	public abstract double Baseline { get; }

	public abstract double TextBaseline { get; }

	public abstract double MarkerBaseline { get; }

	public abstract double MarkerHeight { get; }

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
	}

	public abstract TextLineBreak GetTextLineBreak();

	internal abstract MS.Internal.SecurityCriticalDataForSet<nint> GetTextPenaltyResource();
}
