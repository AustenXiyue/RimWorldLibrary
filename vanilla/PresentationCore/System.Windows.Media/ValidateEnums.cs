using MS.Internal.PresentationCore;

namespace System.Windows.Media;

[FriendAccessAllowed]
internal static class ValidateEnums
{
	public static bool IsAlignmentXValid(object valueObject)
	{
		AlignmentX alignmentX = (AlignmentX)valueObject;
		if (alignmentX != 0 && alignmentX != AlignmentX.Center)
		{
			return alignmentX == AlignmentX.Right;
		}
		return true;
	}

	public static bool IsAlignmentYValid(object valueObject)
	{
		AlignmentY alignmentY = (AlignmentY)valueObject;
		if (alignmentY != 0 && alignmentY != AlignmentY.Center)
		{
			return alignmentY == AlignmentY.Bottom;
		}
		return true;
	}

	public static bool IsBrushMappingModeValid(object valueObject)
	{
		BrushMappingMode brushMappingMode = (BrushMappingMode)valueObject;
		if (brushMappingMode != 0)
		{
			return brushMappingMode == BrushMappingMode.RelativeToBoundingBox;
		}
		return true;
	}

	public static bool IsCachingHintValid(object valueObject)
	{
		CachingHint cachingHint = (CachingHint)valueObject;
		if (cachingHint != 0)
		{
			return cachingHint == CachingHint.Cache;
		}
		return true;
	}

	public static bool IsColorInterpolationModeValid(object valueObject)
	{
		ColorInterpolationMode colorInterpolationMode = (ColorInterpolationMode)valueObject;
		if (colorInterpolationMode != 0)
		{
			return colorInterpolationMode == ColorInterpolationMode.SRgbLinearInterpolation;
		}
		return true;
	}

	public static bool IsGeometryCombineModeValid(object valueObject)
	{
		GeometryCombineMode geometryCombineMode = (GeometryCombineMode)valueObject;
		if (geometryCombineMode != 0 && geometryCombineMode != GeometryCombineMode.Intersect && geometryCombineMode != GeometryCombineMode.Xor)
		{
			return geometryCombineMode == GeometryCombineMode.Exclude;
		}
		return true;
	}

	public static bool IsEdgeModeValid(object valueObject)
	{
		EdgeMode edgeMode = (EdgeMode)valueObject;
		if (edgeMode != 0)
		{
			return edgeMode == EdgeMode.Aliased;
		}
		return true;
	}

	public static bool IsBitmapScalingModeValid(object valueObject)
	{
		BitmapScalingMode bitmapScalingMode = (BitmapScalingMode)valueObject;
		if (bitmapScalingMode != 0 && bitmapScalingMode != BitmapScalingMode.LowQuality && bitmapScalingMode != BitmapScalingMode.HighQuality && bitmapScalingMode != BitmapScalingMode.LowQuality && bitmapScalingMode != BitmapScalingMode.HighQuality)
		{
			return bitmapScalingMode == BitmapScalingMode.NearestNeighbor;
		}
		return true;
	}

	public static bool IsClearTypeHintValid(object valueObject)
	{
		ClearTypeHint clearTypeHint = (ClearTypeHint)valueObject;
		if (clearTypeHint != 0)
		{
			return clearTypeHint == ClearTypeHint.Enabled;
		}
		return true;
	}

	public static bool IsTextRenderingModeValid(object valueObject)
	{
		TextRenderingMode textRenderingMode = (TextRenderingMode)valueObject;
		if (textRenderingMode != 0 && textRenderingMode != TextRenderingMode.Aliased && textRenderingMode != TextRenderingMode.Grayscale)
		{
			return textRenderingMode == TextRenderingMode.ClearType;
		}
		return true;
	}

	public static bool IsTextHintingModeValid(object valueObject)
	{
		TextHintingMode textHintingMode = (TextHintingMode)valueObject;
		if (textHintingMode != 0 && textHintingMode != TextHintingMode.Fixed)
		{
			return textHintingMode == TextHintingMode.Animated;
		}
		return true;
	}

	public static bool IsFillRuleValid(object valueObject)
	{
		FillRule fillRule = (FillRule)valueObject;
		if (fillRule != 0)
		{
			return fillRule == FillRule.Nonzero;
		}
		return true;
	}

	public static bool IsGradientSpreadMethodValid(object valueObject)
	{
		GradientSpreadMethod gradientSpreadMethod = (GradientSpreadMethod)valueObject;
		if (gradientSpreadMethod != 0 && gradientSpreadMethod != GradientSpreadMethod.Reflect)
		{
			return gradientSpreadMethod == GradientSpreadMethod.Repeat;
		}
		return true;
	}

	public static bool IsPenLineCapValid(object valueObject)
	{
		PenLineCap penLineCap = (PenLineCap)valueObject;
		if (penLineCap != 0 && penLineCap != PenLineCap.Square && penLineCap != PenLineCap.Round)
		{
			return penLineCap == PenLineCap.Triangle;
		}
		return true;
	}

	public static bool IsPenLineJoinValid(object valueObject)
	{
		PenLineJoin penLineJoin = (PenLineJoin)valueObject;
		if (penLineJoin != 0 && penLineJoin != PenLineJoin.Bevel)
		{
			return penLineJoin == PenLineJoin.Round;
		}
		return true;
	}

	public static bool IsStretchValid(object valueObject)
	{
		Stretch stretch = (Stretch)valueObject;
		if (stretch != 0 && stretch != Stretch.Fill && stretch != Stretch.Uniform)
		{
			return stretch == Stretch.UniformToFill;
		}
		return true;
	}

	public static bool IsTileModeValid(object valueObject)
	{
		TileMode tileMode = (TileMode)valueObject;
		if (tileMode != 0 && tileMode != TileMode.Tile && tileMode != TileMode.FlipX && tileMode != TileMode.FlipY)
		{
			return tileMode == TileMode.FlipXY;
		}
		return true;
	}

	public static bool IsSweepDirectionValid(object valueObject)
	{
		SweepDirection sweepDirection = (SweepDirection)valueObject;
		if (sweepDirection != 0)
		{
			return sweepDirection == SweepDirection.Clockwise;
		}
		return true;
	}
}
