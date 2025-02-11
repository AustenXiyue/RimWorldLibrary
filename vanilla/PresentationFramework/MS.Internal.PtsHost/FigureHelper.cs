using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal static class FigureHelper
{
	internal static bool IsVerticalPageAnchor(FigureVerticalAnchor verticalAnchor)
	{
		if (verticalAnchor != 0 && verticalAnchor != FigureVerticalAnchor.PageBottom)
		{
			return verticalAnchor == FigureVerticalAnchor.PageCenter;
		}
		return true;
	}

	internal static bool IsVerticalContentAnchor(FigureVerticalAnchor verticalAnchor)
	{
		if (verticalAnchor != FigureVerticalAnchor.ContentTop && verticalAnchor != FigureVerticalAnchor.ContentBottom)
		{
			return verticalAnchor == FigureVerticalAnchor.ContentCenter;
		}
		return true;
	}

	internal static bool IsHorizontalPageAnchor(FigureHorizontalAnchor horizontalAnchor)
	{
		if (horizontalAnchor != 0 && horizontalAnchor != FigureHorizontalAnchor.PageRight)
		{
			return horizontalAnchor == FigureHorizontalAnchor.PageCenter;
		}
		return true;
	}

	internal static bool IsHorizontalContentAnchor(FigureHorizontalAnchor horizontalAnchor)
	{
		if (horizontalAnchor != FigureHorizontalAnchor.ContentLeft && horizontalAnchor != FigureHorizontalAnchor.ContentRight)
		{
			return horizontalAnchor == FigureHorizontalAnchor.ContentCenter;
		}
		return true;
	}

	internal static bool IsHorizontalColumnAnchor(FigureHorizontalAnchor horizontalAnchor)
	{
		if (horizontalAnchor != FigureHorizontalAnchor.ColumnLeft && horizontalAnchor != FigureHorizontalAnchor.ColumnRight)
		{
			return horizontalAnchor == FigureHorizontalAnchor.ColumnCenter;
		}
		return true;
	}

	internal static double CalculateFigureWidth(StructuralCache structuralCache, Figure figure, FigureLength figureLength, out bool isWidthAuto)
	{
		isWidthAuto = figureLength.IsAuto;
		FigureHorizontalAnchor horizontalAnchor = figure.HorizontalAnchor;
		double num;
		if (figureLength.IsPage || (figureLength.IsAuto && IsHorizontalPageAnchor(horizontalAnchor)))
		{
			num = structuralCache.CurrentFormatContext.PageWidth * figureLength.Value;
		}
		else if (figureLength.IsAbsolute)
		{
			num = CalculateFigureCommon(figureLength);
		}
		else
		{
			GetColumnMetrics(structuralCache, out var cColumns, out var width, out var gap, out var _);
			if (figureLength.IsContent || (figureLength.IsAuto && IsHorizontalContentAnchor(horizontalAnchor)))
			{
				num = (width * (double)cColumns + gap * (double)(cColumns - 1)) * figureLength.Value;
			}
			else
			{
				double value = figureLength.Value;
				int num2 = (int)value;
				if ((double)num2 == value && num2 > 0)
				{
					num2--;
				}
				num = width * value + gap * (double)num2;
			}
		}
		Invariant.Assert(!double.IsNaN(num));
		return num;
	}

	internal static double CalculateFigureHeight(StructuralCache structuralCache, Figure figure, FigureLength figureLength, out bool isHeightAuto)
	{
		double num;
		if (figureLength.IsPage)
		{
			num = structuralCache.CurrentFormatContext.PageHeight * figureLength.Value;
		}
		else if (figureLength.IsContent)
		{
			Thickness pageMargin = structuralCache.CurrentFormatContext.PageMargin;
			num = (structuralCache.CurrentFormatContext.PageHeight - pageMargin.Top - pageMargin.Bottom) * figureLength.Value;
		}
		else if (figureLength.IsColumn)
		{
			GetColumnMetrics(structuralCache, out var cColumns, out var width, out var gap, out var _);
			double num2 = figureLength.Value;
			if (num2 > (double)cColumns)
			{
				num2 = cColumns;
			}
			int num3 = (int)num2;
			if ((double)num3 == num2 && num3 > 0)
			{
				num3--;
			}
			num = width * num2 + gap * (double)num3;
		}
		else
		{
			num = CalculateFigureCommon(figureLength);
		}
		if (!double.IsNaN(num))
		{
			if (IsVerticalPageAnchor(figure.VerticalAnchor))
			{
				num = Math.Max(1.0, Math.Min(num, structuralCache.CurrentFormatContext.PageHeight));
			}
			else
			{
				Thickness pageMargin2 = structuralCache.CurrentFormatContext.PageMargin;
				num = Math.Max(1.0, Math.Min(num, structuralCache.CurrentFormatContext.PageHeight - pageMargin2.Top - pageMargin2.Bottom));
			}
			TextDpi.EnsureValidPageWidth(ref num);
			isHeightAuto = false;
		}
		else
		{
			num = structuralCache.CurrentFormatContext.PageHeight;
			isHeightAuto = true;
		}
		return num;
	}

	internal static double CalculateFigureCommon(FigureLength figureLength)
	{
		if (figureLength.IsAuto)
		{
			return double.NaN;
		}
		if (figureLength.IsAbsolute)
		{
			return figureLength.Value;
		}
		Invariant.Assert(condition: false, "Unknown figure length type specified.");
		return 0.0;
	}

	internal static void GetColumnMetrics(StructuralCache structuralCache, out int cColumns, out double width, out double gap, out double rule)
	{
		ColumnPropertiesGroup columnPropertiesGroup = new ColumnPropertiesGroup(structuralCache.PropertyOwner);
		FontFamily pageFontFamily = (FontFamily)structuralCache.PropertyOwner.GetValue(TextElement.FontFamilyProperty);
		double lineHeight = DynamicPropertyReader.GetLineHeightValue(structuralCache.PropertyOwner);
		double pageFontSize = (double)structuralCache.PropertyOwner.GetValue(TextElement.FontSizeProperty);
		Size pageSize = structuralCache.CurrentFormatContext.PageSize;
		Thickness pageMargin = structuralCache.CurrentFormatContext.PageMargin;
		double num = pageSize.Width - (pageMargin.Left + pageMargin.Right);
		cColumns = PtsHelper.CalculateColumnCount(columnPropertiesGroup, lineHeight, num, pageFontSize, pageFontFamily, enableColumns: true);
		rule = columnPropertiesGroup.ColumnRuleWidth;
		PtsHelper.GetColumnMetrics(columnPropertiesGroup, num, pageFontSize, pageFontFamily, enableColumns: true, cColumns, ref lineHeight, out width, out var freeSpace, out gap);
		if (columnPropertiesGroup.IsColumnWidthFlexible && columnPropertiesGroup.ColumnSpaceDistribution == ColumnSpaceDistribution.Between)
		{
			width += freeSpace / (double)cColumns;
		}
		width = Math.Min(width, num);
	}
}
