using System.Windows.Documents;

namespace System.Windows.Media;

/// <summary>Represents data returned from calling the <see cref="M:System.Windows.Documents.AdornerLayer.AdornerHitTest(System.Windows.Point)" /> method.</summary>
public class AdornerHitTestResult : PointHitTestResult
{
	private readonly Adorner _adorner;

	/// <summary> Gets the visual that was hit. </summary>
	/// <returns>The visual that was hit.</returns>
	public Adorner Adorner => _adorner;

	internal AdornerHitTestResult(Visual visual, Point pt, Adorner adorner)
		: base(visual, pt)
	{
		_adorner = adorner;
	}
}
