namespace System.Windows.Controls;

internal interface IStackMeasure
{
	bool IsScrolling { get; }

	UIElementCollection InternalChildren { get; }

	Orientation Orientation { get; }

	bool CanVerticallyScroll { get; }

	bool CanHorizontallyScroll { get; }

	void OnScrollChange();
}
