using System.Globalization;
using System.Text;
using System.Windows.Markup;
using MS.Internal.FontFace;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Represents the metrics used to lay out a character in a device font.</summary>
public class CharacterMetrics
{
	private enum FieldIndex
	{
		BlackBoxWidth,
		BlackBoxHeight,
		Baseline,
		LeftSideBearing,
		RightSideBearing,
		TopSideBearing,
		BottomSideBearing
	}

	private double _blackBoxWidth;

	private double _blackBoxHeight;

	private double _baseline;

	private double _leftSideBearing;

	private double _rightSideBearing;

	private double _topSideBearing;

	private double _bottomSideBearing;

	private const int NumFields = 7;

	private const int NumRequiredFields = 2;

	private const int HashMultiplier = 101;

	/// <summary>Gets or sets a comma-delimited string representing metric values.</summary>
	/// <returns>A value of type <see cref="T:System.String" />.</returns>
	public string Metrics
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(_blackBoxWidth.ToString(System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS));
			stringBuilder.Append(',');
			stringBuilder.Append(_blackBoxHeight.ToString(System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS));
			int lastIndex = 1;
			AppendField(_baseline, FieldIndex.Baseline, ref lastIndex, stringBuilder);
			AppendField(_leftSideBearing, FieldIndex.LeftSideBearing, ref lastIndex, stringBuilder);
			AppendField(_rightSideBearing, FieldIndex.RightSideBearing, ref lastIndex, stringBuilder);
			AppendField(_topSideBearing, FieldIndex.TopSideBearing, ref lastIndex, stringBuilder);
			AppendField(_bottomSideBearing, FieldIndex.BottomSideBearing, ref lastIndex, stringBuilder);
			return stringBuilder.ToString();
		}
		set
		{
			double[] array = ParseMetrics(value);
			CompositeFontParser.VerifyNonNegativeMultiplierOfEm("BlackBoxWidth", ref array[0]);
			CompositeFontParser.VerifyNonNegativeMultiplierOfEm("BlackBoxHeight", ref array[1]);
			CompositeFontParser.VerifyMultiplierOfEm("Baseline", ref array[2]);
			CompositeFontParser.VerifyMultiplierOfEm("LeftSideBearing", ref array[3]);
			CompositeFontParser.VerifyMultiplierOfEm("RightSideBearing", ref array[4]);
			CompositeFontParser.VerifyMultiplierOfEm("TopSideBearing", ref array[5]);
			CompositeFontParser.VerifyMultiplierOfEm("BottomSideBearing", ref array[6]);
			if (array[0] + array[3] + array[4] < 0.0)
			{
				throw new ArgumentException(SR.CharacterMetrics_NegativeHorizontalAdvance);
			}
			if (array[1] + array[5] + array[6] < 0.0)
			{
				throw new ArgumentException(SR.CharacterMetrics_NegativeVerticalAdvance);
			}
			_blackBoxWidth = array[0];
			_blackBoxHeight = array[1];
			_baseline = array[2];
			_leftSideBearing = array[3];
			_rightSideBearing = array[4];
			_topSideBearing = array[5];
			_bottomSideBearing = array[6];
		}
	}

	/// <summary>Gets the width of the black box for the character.</summary>
	/// <returns>A value of type <see cref="T:System.Double" /> representing the black box width.</returns>
	public double BlackBoxWidth => _blackBoxWidth;

	/// <summary>Gets the height of the black box for the character.</summary>
	/// <returns>A value of type <see cref="T:System.Double" /> representing the black box height.</returns>
	public double BlackBoxHeight => _blackBoxHeight;

	/// <summary>Gets the baseline value for the character.</summary>
	/// <returns>A value of type <see cref="T:System.Double" /> representing the baseline.</returns>
	public double Baseline => _baseline;

	/// <summary>Gets the recommended white space to the left of the black box.</summary>
	/// <returns>A value of type <see cref="T:System.Double" />.</returns>
	public double LeftSideBearing => _leftSideBearing;

	/// <summary>Gets the recommended white space to the right of the black box.</summary>
	/// <returns>A value of type <see cref="T:System.Double" />.</returns>
	public double RightSideBearing => _rightSideBearing;

	/// <summary>Gets the recommended white space above the black box.</summary>
	/// <returns>A value of type <see cref="T:System.Double" />.</returns>
	public double TopSideBearing => _topSideBearing;

	/// <summary>Gets the recommended white space below the black box.</summary>
	/// <returns>A value of type <see cref="T:System.Double" />.</returns>
	public double BottomSideBearing => _bottomSideBearing;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.CharacterMetrics" /> class.</summary>
	public CharacterMetrics()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.CharacterMetrics" /> class by specifying the metrics as a string.</summary>
	/// <param name="metrics">A comma-delimited <see cref="T:System.String" /> value that represents the metrics for the character.</param>
	public CharacterMetrics(string metrics)
	{
		if (metrics == null)
		{
			throw new ArgumentNullException("metrics");
		}
		Metrics = metrics;
	}

	private static void AppendField(double value, FieldIndex fieldIndex, ref int lastIndex, StringBuilder s)
	{
		if (value != 0.0)
		{
			s.Append(',', (int)(fieldIndex - lastIndex));
			lastIndex = (int)fieldIndex;
			s.Append(value.ToString(System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS));
		}
	}

	private static double[] ParseMetrics(string s)
	{
		double[] array = new double[7];
		int num = 0;
		int num2 = 0;
		while (true)
		{
			if (num < s.Length && s[num] == ' ')
			{
				num++;
				continue;
			}
			int i;
			for (i = num; i < s.Length && s[i] != ','; i++)
			{
			}
			int num3 = i;
			while (num3 > num && s[num3 - 1] == ' ')
			{
				num3--;
			}
			if (num3 > num)
			{
				ReadOnlySpan<char> s2 = s.AsSpan(num, num3 - num);
				if (!double.TryParse(s2, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS, out array[num2]))
				{
					throw new ArgumentException(SR.Format(SR.CannotConvertStringToType, s2.ToString(), "double"));
				}
			}
			else if (num2 < 2)
			{
				throw new ArgumentException(SR.CharacterMetrics_MissingRequiredField);
			}
			num2++;
			if (i >= s.Length)
			{
				break;
			}
			if (num2 == 7)
			{
				throw new ArgumentException(SR.CharacterMetrics_TooManyFields);
			}
			num = i + 1;
		}
		if (num2 < 2)
		{
			throw new ArgumentException(SR.CharacterMetrics_MissingRequiredField);
		}
		return array;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Media.CharacterMetrics" /> object is equal to the current <see cref="T:System.Windows.Media.CharacterMetrics" /> object.</summary>
	/// <returns>true if the objects are equal; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Windows.Media.CharacterMetrics" /> object to compare to the current <see cref="T:System.Windows.Media.CharacterMetrics" /> object.</param>
	public override bool Equals(object obj)
	{
		if (obj is CharacterMetrics characterMetrics && characterMetrics._blackBoxWidth == _blackBoxWidth && characterMetrics._blackBoxHeight == _blackBoxHeight && characterMetrics._leftSideBearing == _leftSideBearing && characterMetrics._rightSideBearing == _rightSideBearing && characterMetrics._topSideBearing == _topSideBearing)
		{
			return characterMetrics._bottomSideBearing == _bottomSideBearing;
		}
		return false;
	}

	/// <summary>Creates a hash code from the metric values of the <see cref="T:System.Windows.Media.CharacterMetrics" /> object.</summary>
	/// <returns>A value of type <see cref="T:System.Int32" />.</returns>
	public override int GetHashCode()
	{
		return ((((((int)(_blackBoxWidth * 300.0) * 101 + (int)(_blackBoxHeight * 300.0)) * 101 + (int)(_baseline * 300.0)) * 101 + (int)(_leftSideBearing * 300.0)) * 101 + (int)(_rightSideBearing * 300.0)) * 101 + (int)(_topSideBearing * 300.0)) * 101 + (int)(_bottomSideBearing * 300.0);
	}
}
