namespace System.Windows.Media.Animation;

internal interface IClock
{
	TimeSpan CurrentTime { get; }
}
