using System.Windows;
using System.Windows.Media;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal class SectionVisual : DrawingVisual
{
	private Point[] _rulePositions;

	private double _ruleWidth;

	internal SectionVisual()
	{
	}

	internal void DrawColumnRules(ref PTS.FSTRACKDESCRIPTION[] arrayColumnDesc, double columnVStart, double columnHeight, ColumnPropertiesGroup columnProperties)
	{
		Point[] array = null;
		double columnRuleWidth = columnProperties.ColumnRuleWidth;
		if (arrayColumnDesc.Length > 1 && columnRuleWidth > 0.0)
		{
			int num = (arrayColumnDesc[1].fsrc.u - (arrayColumnDesc[0].fsrc.u + arrayColumnDesc[0].fsrc.du)) / 2;
			array = new Point[(arrayColumnDesc.Length - 1) * 2];
			for (int i = 1; i < arrayColumnDesc.Length; i++)
			{
				double x = TextDpi.FromTextDpi(arrayColumnDesc[i].fsrc.u - num);
				array[(i - 1) * 2].X = x;
				array[(i - 1) * 2].Y = columnVStart;
				array[(i - 1) * 2 + 1].X = x;
				array[(i - 1) * 2 + 1].Y = columnVStart + columnHeight;
			}
		}
		bool flag = _ruleWidth != columnRuleWidth;
		if (!flag && _rulePositions != array)
		{
			int num2 = ((_rulePositions != null) ? _rulePositions.Length : 0);
			int num3 = ((array != null) ? array.Length : 0);
			if (num2 == num3)
			{
				for (int j = 0; j < array.Length; j++)
				{
					if (!DoubleUtil.AreClose(array[j].X, _rulePositions[j].X) || !DoubleUtil.AreClose(array[j].Y, _rulePositions[j].Y))
					{
						flag = true;
						break;
					}
				}
			}
			else
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		_ruleWidth = columnRuleWidth;
		_rulePositions = array;
		using DrawingContext drawingContext = RenderOpen();
		if (array != null)
		{
			Pen pen = new Pen((Brush)FreezableOperations.GetAsFrozenIfPossible(columnProperties.ColumnRuleBrush), columnRuleWidth);
			if (pen.CanFreeze)
			{
				pen.Freeze();
			}
			for (int k = 0; k < array.Length; k += 2)
			{
				drawingContext.DrawLine(pen, array[k], array[k + 1]);
			}
		}
	}
}
