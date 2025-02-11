namespace System.Windows.Media.Animation;

public interface IAnimation
{
	object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock);
}
