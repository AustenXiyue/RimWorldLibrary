using System.Windows;
using MS.Internal.Documents;

namespace MS.Internal.PtsHost;

internal sealed class InlineObject : EmbeddedObject
{
	private UIElementIsland _uiElementIsland;

	private TextParagraph _para;

	internal override DependencyObject Element => _uiElementIsland.Root;

	internal InlineObject(int dcp, UIElementIsland uiElementIsland, TextParagraph para)
		: base(dcp)
	{
		_para = para;
		_uiElementIsland = uiElementIsland;
		_uiElementIsland.DesiredSizeChanged += _para.OnUIElementDesiredSizeChanged;
	}

	internal override void Dispose()
	{
		if (_uiElementIsland != null)
		{
			_uiElementIsland.DesiredSizeChanged -= _para.OnUIElementDesiredSizeChanged;
		}
		base.Dispose();
	}

	internal override void Update(EmbeddedObject newObject)
	{
		InlineObject obj = newObject as InlineObject;
		ErrorHandler.Assert(obj != null, ErrorHandler.EmbeddedObjectTypeMismatch);
		ErrorHandler.Assert(obj._uiElementIsland == _uiElementIsland, ErrorHandler.EmbeddedObjectOwnerMismatch);
		obj._uiElementIsland = null;
	}
}
