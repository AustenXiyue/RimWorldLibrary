namespace System.Windows.Media;

/// <summary>Implements a set of predefined <see cref="T:System.Windows.Media.DashStyle" /> objects. </summary>
public static class DashStyles
{
	private static DashStyle _solid;

	private static DashStyle _dash;

	private static DashStyle _dot;

	private static DashStyle _dashDot;

	private static DashStyle _dashDotDot;

	/// <summary>Gets a <see cref="T:System.Windows.Media.DashStyle" /> with an empty <see cref="P:System.Windows.Media.DashStyle.Dashes" /> property. </summary>
	/// <returns>A dash sequence with no dashes.</returns>
	public static DashStyle Solid
	{
		get
		{
			if (_solid == null)
			{
				DashStyle dashStyle = new DashStyle();
				dashStyle.Freeze();
				_solid = dashStyle;
			}
			return _solid;
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.DashStyle" /> with a <see cref="P:System.Windows.Media.DashStyle.Dashes" /> property equal to 2,2. </summary>
	/// <returns>A dash sequence of 2,2, which describes a sequence composed of a dash that is twice as long as the pen <see cref="P:System.Windows.Media.Pen.Thickness" /> followed by a space that is twice as long as the <see cref="P:System.Windows.Media.Pen.Thickness" />.</returns>
	public static DashStyle Dash
	{
		get
		{
			if (_dash == null)
			{
				DashStyle dashStyle = new DashStyle(new double[2] { 2.0, 2.0 }, 1.0);
				dashStyle.Freeze();
				_dash = dashStyle;
			}
			return _dash;
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.DashStyle" /> with a <see cref="P:System.Windows.Media.DashStyle.Dashes" /> property equal to 0,2. </summary>
	/// <returns>A dash sequence of 0,2.</returns>
	public static DashStyle Dot
	{
		get
		{
			if (_dot == null)
			{
				DashStyle dashStyle = new DashStyle(new double[2] { 0.0, 2.0 }, 0.0);
				dashStyle.Freeze();
				_dot = dashStyle;
			}
			return _dot;
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.DashStyle" /> with a <see cref="P:System.Windows.Media.DashStyle.Dashes" /> property equal to 2,2,0,2. </summary>
	/// <returns>A dash sequence of 2,2,0,2.</returns>
	public static DashStyle DashDot
	{
		get
		{
			if (_dashDot == null)
			{
				DashStyle dashStyle = new DashStyle(new double[4] { 2.0, 2.0, 0.0, 2.0 }, 1.0);
				dashStyle.Freeze();
				_dashDot = dashStyle;
			}
			return _dashDot;
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.DashStyle" /> with a <see cref="P:System.Windows.Media.DashStyle.Dashes" /> property equal to 2,2,0,2,0,2. </summary>
	/// <returns>A dash sequence of 2,2,0,2,0,2.</returns>
	public static DashStyle DashDotDot
	{
		get
		{
			if (_dashDotDot == null)
			{
				DashStyle dashStyle = new DashStyle(new double[6] { 2.0, 2.0, 0.0, 2.0, 0.0, 2.0 }, 1.0);
				dashStyle.Freeze();
				_dashDotDot = dashStyle;
			}
			return _dashDotDot;
		}
	}
}
