namespace System.Windows.Ink;

/// <summary>Specifies the available application-specific <paramref name="gesture" />.</summary>
public enum ApplicationGesture
{
	/// <summary>Recognizes all application-specific gestures.</summary>
	AllGestures = 0,
	/// <summary>Has no suggested semantic behavior or action. The arrow can be drawn in single stroke or in two strokes where one stroke is the line and the other is the arrow head. Do not use more than two strokes to draw the arrow.</summary>
	ArrowDown = 61497,
	/// <summary>Has no suggested semantic behavior or action. The arrow can be drawn in single stroke or in two strokes where one stroke is the line and the other is the arrow head. Do not use more than two strokes to draw the arrow.</summary>
	ArrowLeft = 61498,
	/// <summary>Has no suggested semantic behavior or action. The arrow can be drawn in single stroke or in two strokes where one stroke is the line and the other is the arrow head. Do not use more than two strokes to draw the arrow.</summary>
	ArrowRight = 61499,
	/// <summary>Has no suggested semantic behavior or action. The arrow can be drawn in single stroke or in two strokes where one stroke is the line and the other is the arrow head. Do not use more than two strokes to draw the arrow.</summary>
	ArrowUp = 61496,
	/// <summary>Has no suggested semantic behavior or action. The upward stroke must be twice as long as the smaller downward stroke.</summary>
	Check = 61445,
	/// <summary>Has no suggested semantic behavior or action. Both sides of the chevron must be drawn as equal as possible. The angle must be sharp and end in a point.</summary>
	ChevronDown = 61489,
	/// <summary>Has no suggested semantic behavior or action. Both sides of the chevron must be drawn as equal as possible. The angle must be sharp and end in a point.</summary>
	ChevronLeft = 61490,
	/// <summary>Has no suggested semantic behavior or action. Both sides of the chevron must be drawn as equal as possible. The angle must be sharp and end in a point.</summary>
	ChevronRight = 61491,
	/// <summary>Has no suggested semantic behavior or action. Both sides of the chevron must be drawn as equal as possible. The angle must be sharp and end in a point.</summary>
	ChevronUp = 61488,
	/// <summary>Has no suggested semantic behavior or action. The circle must be drawn in a single stroke without lifting the pen.</summary>
	Circle = 61472,
	/// <summary>Has no suggested semantic behavior or action. Start the curlicue on the word you intend to cut.</summary>
	Curlicue = 61456,
	/// <summary>Has no suggested semantic behavior or action. The two circles must overlap each other and be drawn in a single stroke without lifting the pen.</summary>
	DoubleCircle = 61473,
	/// <summary>Has no suggested semantic behavior or action. Start the double-curlicue on the word you intend to copy.</summary>
	DoubleCurlicue = 61457,
	/// <summary>Signifies a mouse double-click. The two taps must be made quickly to result in the least amount of slippage and the least duration between taps. In addition, the taps must be as close to each other as possible.</summary>
	DoubleTap = 61681,
	/// <summary>Has no suggested semantic behavior or action. This gesture must be drawn as a single fast flick in the downward direction.</summary>
	Down = 61529,
	/// <summary>Has no suggested semantic behavior or action. This gesture must be drawn in a single stroke starting with the down stroke. The two sides must be as equal in length as possible and at a right angle.</summary>
	DownLeft = 61546,
	/// <summary>Signifies pressing an ENTER key. This gesture must be drawn in a single stroke starting with the down stroke. The left stroke is about twice as long as the up stroke, and the two strokes must be at a right angle.</summary>
	DownLeftLong = 61542,
	/// <summary>Has no suggested semantic behavior or action. This gesture must be drawn in a single stroke starting with the down stroke. The two sides must be as equal in length as possible and at a right angle.</summary>
	DownRight = 61547,
	/// <summary>Signifies pressing the spacebar. This gesture must be drawn in a single stroke starting with the down stroke. The right stroke must be about twice as long as the up stroke, and the two strokes must be at a right angle.</summary>
	DownRightLong = 61543,
	/// <summary>Has no suggested semantic behavior or action. This gesture must be drawn in a single stroke starting with the down stroke. The two strokes must be as close to each other as possible.</summary>
	DownUp = 61537,
	/// <summary>Has no suggested semantic behavior or action. The line must be drawn first and then the dot drawn quickly and as close to the line as possible.</summary>
	Exclamation = 61604,
	/// <summary>Specifies a backspace. This gesture must be drawn as a single fast flick to the left.</summary>
	Left = 61530,
	/// <summary>Has no suggested semantic behavior or action. This gesture must be drawn in a single stroke starting with the left stroke. The two sides are as equal in length as possible and at a right angle.</summary>
	LeftDown = 61549,
	/// <summary>Has no suggested semantic behavior or action. This gesture must be drawn in a single stroke starting with the left stroke. The two sides are as equal in length as possible and at a right angle.</summary>
	LeftRight = 61538,
	/// <summary>Has no suggested semantic behavior or action. This gesture must be drawn in a single stroke starting with the left stroke. The two sides must be as equal in length as possible and at a right angle.</summary>
	LeftUp = 61548,
	/// <summary>Recognizes no application-specific gestures.</summary>
	NoGesture = 61440,
	/// <summary>Signifies a space. This gesture must be drawn as a single fast flick to the right.</summary>
	Right = 61531,
	/// <summary>Has no suggested semantic behavior or action. This gesture must be drawn in a single stroke starting with the right stroke. The two sides must be as equal in length as possible and at a right angle.</summary>
	RightDown = 61551,
	/// <summary>Has no suggested semantic behavior or action. This gesture must be drawn in a single stroke starting with the right stroke. The two strokes must be as close to each other as possible.</summary>
	RightLeft = 61539,
	/// <summary>Has no suggested semantic behavior or action. This gesture must be drawn in a single stroke starting with the right stroke. The two sides must be as equal in length as possible and at a right angle.</summary>
	RightUp = 61550,
	/// <summary>Erases content. This gesture must be drawn as a single stroke that has at least three back-and-forth motions.</summary>
	ScratchOut = 61441,
	/// <summary>Has no suggested semantic behavior or action. The semicircle must be drawn from left to right. The two ends of the semicircle should be as horizontally even as possible.</summary>
	SemicircleLeft = 61480,
	/// <summary>Has no suggested semantic behavior or action. The semicircle must be drawn from right to left. The two ends of the semicircle should be as horizontally even as possible.</summary>
	SemicircleRight = 61481,
	/// <summary>Has no suggested semantic behavior or action. The square can be drawn in one or two strokes. In one stroke, draw the entire square without lifting the pen. In two strokes, draw three sides of the square and use another stroke to draw the remaining side. Do not use more than two strokes to draw the square.</summary>
	Square = 61443,
	/// <summary>Has no suggested semantic behavior or action. The star must have exactly five points and be drawn in a single stroke without lifting the pen.</summary>
	Star = 61444,
	/// <summary>Signifies a mouse click. For the least amount of slippage, the tap must be made quickly.</summary>
	Tap = 61680,
	/// <summary>Has no suggested semantic behavior or action. The triangle must be drawn in a single stroke, without lifting the pen.</summary>
	Triangle = 61442,
	/// <summary>Has no suggested semantic behavior or action. This gesture must be drawn as a single fast flick in the upward direction.</summary>
	Up = 61528,
	/// <summary>Has no suggested semantic behavior or action. This gesture must be drawn in a single stroke starting with the up stroke. The two strokes must be as close to each other as possible.</summary>
	UpDown = 61536,
	/// <summary>Has no suggested semantic behavior or action. This gesture must be drawn in a single stroke starting with the up stroke. The two sides must be as equal in length as possible and at a right angle.</summary>
	UpLeft = 61544,
	/// <summary>Has no suggested semantic behavior or action. This gesture must be drawn in a single stroke starting with the up stroke. The left stroke must be about twice as long as the up stroke, and the two strokes must be at a right angle.</summary>
	UpLeftLong = 61540,
	/// <summary>Has no suggested semantic behavior or action. This gesture must be drawn in a single stroke starting with the up stroke. The two sides must be as equal in length as possible and at a right angle.</summary>
	UpRight = 61545,
	/// <summary>Signifies pressing a TAB key. This gesture must be drawn in a single stroke starting with the up stroke. The right stroke must be about twice as long as the up stroke, and the two strokes must be at a right angle.</summary>
	UpRightLong = 61541
}
