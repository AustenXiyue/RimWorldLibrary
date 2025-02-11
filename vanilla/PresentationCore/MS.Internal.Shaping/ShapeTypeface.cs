using System.Windows.Media;
using MS.Internal.FontCache;
using MS.Internal.FontFace;

namespace MS.Internal.Shaping;

internal class ShapeTypeface
{
	private GlyphTypeface _glyphTypeface;

	private IDeviceFont _deviceFont;

	internal IDeviceFont DeviceFont => _deviceFont;

	internal GlyphTypeface GlyphTypeface => _glyphTypeface;

	internal ShapeTypeface(GlyphTypeface glyphTypeface, IDeviceFont deviceFont)
	{
		Invariant.Assert(glyphTypeface != null);
		_glyphTypeface = glyphTypeface;
		_deviceFont = deviceFont;
	}

	public override int GetHashCode()
	{
		return HashFn.HashMultiply(_glyphTypeface.GetHashCode()) + ((_deviceFont != null) ? _deviceFont.Name.GetHashCode() : 0);
	}

	public override bool Equals(object o)
	{
		if (!(o is ShapeTypeface shapeTypeface))
		{
			return false;
		}
		if (_deviceFont == null)
		{
			if (shapeTypeface._deviceFont != null)
			{
				return false;
			}
		}
		else if (shapeTypeface._deviceFont == null || shapeTypeface._deviceFont.Name != _deviceFont.Name)
		{
			return false;
		}
		return _glyphTypeface.Equals(shapeTypeface._glyphTypeface);
	}
}
