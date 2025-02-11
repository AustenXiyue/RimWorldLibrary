using System.Windows.Media;

namespace System.Windows.Documents;

internal sealed class FixedSOMFixedBlock : FixedSOMPageElement
{
	private int _RTLCount;

	private int _LTRCount;

	private Matrix _matrix;

	public double LineHeight
	{
		get
		{
			FixedSOMTextRun lastTextRun = LastTextRun;
			if (lastTextRun != null)
			{
				if (base.SemanticBoxes.Count > 1 && base.SemanticBoxes[base.SemanticBoxes.Count - 2] is FixedSOMTextRun fixedSOMTextRun && lastTextRun.BoundingRect.Height / fixedSOMTextRun.BoundingRect.Height < 0.75 && fixedSOMTextRun.BoundingRect.Left != lastTextRun.BoundingRect.Left && fixedSOMTextRun.BoundingRect.Right != lastTextRun.BoundingRect.Right && fixedSOMTextRun.BoundingRect.Top != lastTextRun.BoundingRect.Top && fixedSOMTextRun.BoundingRect.Bottom != lastTextRun.BoundingRect.Bottom)
				{
					return fixedSOMTextRun.BoundingRect.Height;
				}
				return lastTextRun.BoundingRect.Height;
			}
			return 0.0;
		}
	}

	public bool IsFloatingImage
	{
		get
		{
			if (_semanticBoxes.Count == 1)
			{
				return _semanticBoxes[0] is FixedSOMImage;
			}
			return false;
		}
	}

	internal override FixedElement.ElementType[] ElementTypes => new FixedElement.ElementType[1];

	public bool IsWhiteSpace
	{
		get
		{
			if (_semanticBoxes.Count == 0)
			{
				return false;
			}
			foreach (FixedSOMSemanticBox semanticBox in _semanticBoxes)
			{
				if (!(semanticBox is FixedSOMTextRun { IsWhiteSpace: not false }))
				{
					return false;
				}
			}
			return true;
		}
	}

	public override bool IsRTL => _RTLCount > _LTRCount;

	public Matrix Matrix => _matrix;

	private FixedSOMTextRun LastTextRun
	{
		get
		{
			FixedSOMTextRun fixedSOMTextRun = null;
			int num = _semanticBoxes.Count - 1;
			while (num >= 0 && fixedSOMTextRun == null)
			{
				fixedSOMTextRun = _semanticBoxes[num] as FixedSOMTextRun;
				num--;
			}
			return fixedSOMTextRun;
		}
	}

	public FixedSOMFixedBlock(FixedSOMPage page)
		: base(page)
	{
	}

	public void CombineWith(FixedSOMFixedBlock block)
	{
		foreach (FixedSOMSemanticBox semanticBox in block.SemanticBoxes)
		{
			if (semanticBox is FixedSOMTextRun textRun)
			{
				AddTextRun(textRun);
			}
			else
			{
				Add(semanticBox);
			}
		}
	}

	public void AddTextRun(FixedSOMTextRun textRun)
	{
		_AddElement(textRun);
		textRun.FixedBlock = this;
		if (!textRun.IsWhiteSpace)
		{
			if (textRun.IsLTR)
			{
				_LTRCount++;
			}
			else
			{
				_RTLCount++;
			}
		}
	}

	public void AddImage(FixedSOMImage image)
	{
		_AddElement(image);
	}

	public override void SetRTFProperties(FixedElement element)
	{
		if (IsRTL)
		{
			element.SetValue(FrameworkElement.FlowDirectionProperty, FlowDirection.RightToLeft);
		}
	}

	private void _AddElement(FixedSOMElement element)
	{
		Add(element);
		if (_semanticBoxes.Count == 1)
		{
			_matrix = element.Matrix;
			_matrix.OffsetX = 0.0;
			_matrix.OffsetY = 0.0;
		}
	}
}
