using System.Windows.Media;
using System.Windows.Media.Animation;

namespace System.Windows;

/// <summary>Represents a text decoration, which a visual ornamentation that is added to text (such as an underline).</summary>
[Localizability(LocalizationCategory.None)]
public sealed class TextDecoration : Animatable
{
	/// <summary>Identifies the <see cref="P:System.Windows.TextDecoration.Pen" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.TextDecoration.Pen" /> dependency property. </returns>
	public static readonly DependencyProperty PenProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.TextDecoration.PenOffset" /> dependency property. </summary>
	/// <returns>The identifier for the  <see cref="P:System.Windows.TextDecoration.PenOffset" /> dependency property. </returns>
	public static readonly DependencyProperty PenOffsetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.TextDecoration.PenOffsetUnit" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.TextDecoration.PenOffsetUnit" /> dependency property. </returns>
	public static readonly DependencyProperty PenOffsetUnitProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.TextDecoration.PenThicknessUnit" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.TextDecoration.PenThicknessUnit" /> dependency property. </returns>
	public static readonly DependencyProperty PenThicknessUnitProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.TextDecoration.Location" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.TextDecoration.Location" /> dependency property. </returns>
	public static readonly DependencyProperty LocationProperty;

	internal const double c_PenOffset = 0.0;

	internal const TextDecorationUnit c_PenOffsetUnit = TextDecorationUnit.FontRecommended;

	internal const TextDecorationUnit c_PenThicknessUnit = TextDecorationUnit.FontRecommended;

	internal const TextDecorationLocation c_Location = TextDecorationLocation.Underline;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Pen" /> used to draw the text decoration.  </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Pen" /> used to draw the text decoration. If this value is null, the decoration color matches the text to which it is applied and the decoration's thickness is set to the font's recommended thickness.</returns>
	public Pen Pen
	{
		get
		{
			return (Pen)GetValue(PenProperty);
		}
		set
		{
			SetValueInternal(PenProperty, value);
		}
	}

	/// <summary>Gets or sets the text decoration's offset from its <see cref="P:System.Windows.TextDecoration.Location" />.  </summary>
	/// <returns>The text decoration's offset from its <see cref="P:System.Windows.TextDecoration.Location" />. The default is 0.</returns>
	public double PenOffset
	{
		get
		{
			return (double)GetValue(PenOffsetProperty);
		}
		set
		{
			SetValueInternal(PenOffsetProperty, value);
		}
	}

	/// <summary>Gets the units in which the <see cref="P:System.Windows.TextDecoration.PenOffset" /> value is expressed.  </summary>
	/// <returns>The units in which the <see cref="P:System.Windows.TextDecoration.PenOffset" /> value is expressed. The default is <see cref="F:System.Windows.TextDecorationUnit.FontRecommended" />.</returns>
	public TextDecorationUnit PenOffsetUnit
	{
		get
		{
			return (TextDecorationUnit)GetValue(PenOffsetUnitProperty);
		}
		set
		{
			SetValueInternal(PenOffsetUnitProperty, value);
		}
	}

	/// <summary>Gets the units in which the <see cref="P:System.Windows.Media.Pen.Thickness" /> of the text decoration's <see cref="P:System.Windows.TextDecoration.Pen" /> is expressed.  </summary>
	/// <returns>The units in which the <see cref="P:System.Windows.Media.Pen.Thickness" /> of the text decoration's <see cref="P:System.Windows.TextDecoration.Pen" /> is expressed. The default is <see cref="F:System.Windows.TextDecorationUnit.FontRecommended" />.</returns>
	public TextDecorationUnit PenThicknessUnit
	{
		get
		{
			return (TextDecorationUnit)GetValue(PenThicknessUnitProperty);
		}
		set
		{
			SetValueInternal(PenThicknessUnitProperty, value);
		}
	}

	/// <summary>Gets or sets the vertical location at which the text decoration is drawn.  </summary>
	/// <returns>The vertical location at which the text decoration is drawn.</returns>
	public TextDecorationLocation Location
	{
		get
		{
			return (TextDecorationLocation)GetValue(LocationProperty);
		}
		set
		{
			SetValueInternal(LocationProperty, value);
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.TextDecoration" />, making deep copies of this object's values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new TextDecoration Clone()
	{
		return (TextDecoration)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.TextDecoration" /> object, making deep copies of this object's current values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property value is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property value is true.</returns>
	public new TextDecoration CloneCurrentValue()
	{
		return (TextDecoration)base.CloneCurrentValue();
	}

	protected override Freezable CreateInstanceCore()
	{
		return new TextDecoration();
	}

	static TextDecoration()
	{
		Type typeFromHandle = typeof(TextDecoration);
		PenProperty = Animatable.RegisterProperty("Pen", typeof(Pen), typeFromHandle, null, null, null, isIndependentlyAnimated: false, null);
		PenOffsetProperty = Animatable.RegisterProperty("PenOffset", typeof(double), typeFromHandle, 0.0, null, null, isIndependentlyAnimated: false, null);
		PenOffsetUnitProperty = Animatable.RegisterProperty("PenOffsetUnit", typeof(TextDecorationUnit), typeFromHandle, TextDecorationUnit.FontRecommended, null, ValidateEnums.IsTextDecorationUnitValid, isIndependentlyAnimated: false, null);
		PenThicknessUnitProperty = Animatable.RegisterProperty("PenThicknessUnit", typeof(TextDecorationUnit), typeFromHandle, TextDecorationUnit.FontRecommended, null, ValidateEnums.IsTextDecorationUnitValid, isIndependentlyAnimated: false, null);
		LocationProperty = Animatable.RegisterProperty("Location", typeof(TextDecorationLocation), typeFromHandle, TextDecorationLocation.Underline, null, ValidateEnums.IsTextDecorationLocationValid, isIndependentlyAnimated: false, null);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.TextDecoration" /> class.</summary>
	public TextDecoration()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.TextDecoration" /> class with the specified <see cref="P:System.Windows.TextDecoration.Location" />, <see cref="P:System.Windows.TextDecoration.Pen" />, <see cref="P:System.Windows.TextDecoration.PenOffset" />, <see cref="P:System.Windows.TextDecoration.PenOffsetUnit" />, and <see cref="P:System.Windows.TextDecoration.PenThicknessUnit" /> values.  </summary>
	/// <param name="location">The location of the text decoration.</param>
	/// <param name="pen">The <see cref="T:System.Windows.Media.Pen" /> used to draw the text decoration. If this value is null, the text decoration color matches the text color to which it is applied, and the text decoration's thickness is set to the font's recommended thickness.</param>
	/// <param name="penOffset">The vertical displacement from the text decoration's location. A negative value moves the decoration lower, while a positive value moves the decoration higher.</param>
	/// <param name="penOffsetUnit">The units used to interpret the value of <paramref name="penOffset" />.</param>
	/// <param name="penThicknessUnit">The units used to interpret the value of the <see cref="P:System.Windows.Media.Pen.Thickness" /> for the <paramref name="pen" />.</param>
	public TextDecoration(TextDecorationLocation location, Pen pen, double penOffset, TextDecorationUnit penOffsetUnit, TextDecorationUnit penThicknessUnit)
	{
		Location = location;
		Pen = pen;
		PenOffset = penOffset;
		PenOffsetUnit = penOffsetUnit;
		PenThicknessUnit = penThicknessUnit;
	}

	internal bool ValueEquals(TextDecoration textDecoration)
	{
		if (textDecoration == null)
		{
			return false;
		}
		if (this == textDecoration)
		{
			return true;
		}
		if (Location == textDecoration.Location && PenOffset == textDecoration.PenOffset && PenOffsetUnit == textDecoration.PenOffsetUnit && PenThicknessUnit == textDecoration.PenThicknessUnit)
		{
			if (Pen != null)
			{
				return Pen.Equals(textDecoration.Pen);
			}
			return textDecoration.Pen == null;
		}
		return false;
	}
}
