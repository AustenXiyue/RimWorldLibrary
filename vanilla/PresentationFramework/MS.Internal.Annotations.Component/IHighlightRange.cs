using System.Windows.Annotations;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MS.Internal.Annotations.Component;

internal interface IHighlightRange
{
	Color Background { get; }

	Color SelectedBackground { get; }

	TextAnchor Range { get; }

	int Priority { get; }

	bool HighlightContent { get; }

	void AddChild(Shape child);

	void RemoveChild(Shape child);
}
