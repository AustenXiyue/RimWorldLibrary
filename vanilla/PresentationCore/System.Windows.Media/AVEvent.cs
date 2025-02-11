namespace System.Windows.Media;

internal enum AVEvent
{
	AVMediaNone,
	AVMediaOpened,
	AVMediaClosed,
	AVMediaStarted,
	AVMediaStopped,
	AVMediaPaused,
	AVMediaRateChanged,
	AVMediaEnded,
	AVMediaFailed,
	AVMediaBufferingStarted,
	AVMediaBufferingEnded,
	AVMediaPrerolled,
	AVMediaScriptCommand,
	AVMediaNewFrame
}
