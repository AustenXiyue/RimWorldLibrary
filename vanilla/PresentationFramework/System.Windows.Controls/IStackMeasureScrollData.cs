namespace System.Windows.Controls;

internal interface IStackMeasureScrollData
{
	Vector Offset { get; set; }

	Size Viewport { get; set; }

	Size Extent { get; set; }

	Vector ComputedOffset { get; set; }

	void SetPhysicalViewport(double value);
}
