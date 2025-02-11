using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace System.Windows.Documents;

internal abstract class FixedSOMElement : FixedSOMSemanticBox
{
	protected FixedNode _fixedNode;

	protected int _startIndex;

	protected int _endIndex;

	protected Matrix _mat;

	private FlowNode _flowNode;

	private int _offsetInFlowNode;

	public FixedNode FixedNode => _fixedNode;

	public int StartIndex => _startIndex;

	public int EndIndex => _endIndex;

	internal FlowNode FlowNode
	{
		get
		{
			return _flowNode;
		}
		set
		{
			_flowNode = value;
		}
	}

	internal int OffsetInFlowNode
	{
		get
		{
			return _offsetInFlowNode;
		}
		set
		{
			_offsetInFlowNode = value;
		}
	}

	internal Matrix Matrix => _mat;

	protected FixedSOMElement(FixedNode fixedNode, int startIndex, int endIndex, GeneralTransform transform)
	{
		_fixedNode = fixedNode;
		_startIndex = startIndex;
		_endIndex = endIndex;
		Transform affineTransform = transform.AffineTransform;
		if (affineTransform != null)
		{
			_mat = affineTransform.Value;
		}
		else
		{
			_mat = Transform.Identity.Value;
		}
	}

	protected FixedSOMElement(FixedNode fixedNode, GeneralTransform transform)
	{
		_fixedNode = fixedNode;
		Transform affineTransform = transform.AffineTransform;
		if (affineTransform != null)
		{
			_mat = affineTransform.Value;
		}
		else
		{
			_mat = Transform.Identity.Value;
		}
	}

	public static FixedSOMElement CreateFixedSOMElement(FixedPage page, UIElement uiElement, FixedNode fixedNode, int startIndex, int endIndex)
	{
		FixedSOMElement result = null;
		if (uiElement is Glyphs)
		{
			Glyphs glyphs = uiElement as Glyphs;
			if (glyphs.UnicodeString.Length > 0)
			{
				GlyphRun glyphRun = glyphs.ToGlyphRun();
				Rect boundingRect = glyphRun.ComputeAlignmentBox();
				boundingRect.Offset(glyphs.OriginX, glyphs.OriginY);
				GeneralTransform transform = glyphs.TransformToAncestor(page);
				if (startIndex < 0)
				{
					startIndex = 0;
				}
				if (endIndex < 0)
				{
					endIndex = ((glyphRun.Characters != null) ? glyphRun.Characters.Count : 0);
				}
				result = FixedSOMTextRun.Create(boundingRect, transform, glyphs, fixedNode, startIndex, endIndex, allowReverseGlyphs: false);
			}
		}
		else if (uiElement is Image)
		{
			result = FixedSOMImage.Create(page, uiElement as Image, fixedNode);
		}
		else if (uiElement is Path)
		{
			result = FixedSOMImage.Create(page, uiElement as Path, fixedNode);
		}
		return result;
	}
}
