using System.Windows.Media;
using MS.Internal.FontCache;
using MS.Internal.FontFace;

namespace MS.Internal.Shaping;

internal class ScaledShapeTypeface
{
	private ShapeTypeface _shapeTypeface;

	private double _scaleInEm;

	private bool _nullShape;

	internal ShapeTypeface ShapeTypeface => _shapeTypeface;

	internal double ScaleInEm => _scaleInEm;

	internal bool NullShape => _nullShape;

	internal ScaledShapeTypeface(GlyphTypeface glyphTypeface, IDeviceFont deviceFont, double scaleInEm, bool nullShape)
	{
		_shapeTypeface = new ShapeTypeface(glyphTypeface, deviceFont);
		_scaleInEm = scaleInEm;
		_nullShape = nullShape;
	}

	public override int GetHashCode()
	{
		return HashFn.HashScramble(HashFn.HashMultiply(HashFn.HashMultiply(_shapeTypeface.GetHashCode()) + (_nullShape ? 1 : 0)) + _scaleInEm.GetHashCode());
	}

	public override bool Equals(object o)
	{
		if (!(o is ScaledShapeTypeface scaledShapeTypeface))
		{
			return false;
		}
		if (_shapeTypeface.Equals(scaledShapeTypeface._shapeTypeface) && _scaleInEm == scaledShapeTypeface._scaleInEm)
		{
			return _nullShape == scaledShapeTypeface._nullShape;
		}
		return false;
	}
}
