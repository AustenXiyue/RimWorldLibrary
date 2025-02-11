using System.Windows.Media;

namespace System.Windows.Ink;

internal static class MatrixHelper
{
	internal static bool ContainsNaN(Matrix matrix)
	{
		if (double.IsNaN(matrix.M11) || double.IsNaN(matrix.M12) || double.IsNaN(matrix.M21) || double.IsNaN(matrix.M22) || double.IsNaN(matrix.OffsetX) || double.IsNaN(matrix.OffsetY))
		{
			return true;
		}
		return false;
	}

	internal static bool ContainsInfinity(Matrix matrix)
	{
		if (double.IsInfinity(matrix.M11) || double.IsInfinity(matrix.M12) || double.IsInfinity(matrix.M21) || double.IsInfinity(matrix.M22) || double.IsInfinity(matrix.OffsetX) || double.IsInfinity(matrix.OffsetY))
		{
			return true;
		}
		return false;
	}
}
