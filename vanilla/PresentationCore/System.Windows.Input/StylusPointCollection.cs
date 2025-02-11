using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Contains a collection of <see cref="T:System.Windows.Input.StylusPoint" /> objects.</summary>
public class StylusPointCollection : Collection<StylusPoint>
{
	private StylusPointDescription _stylusPointDescription;

	/// <summary>Gets the <see cref="T:System.Windows.Input.StylusPointDescription" /> that is associated with the <see cref="T:System.Windows.Input.StylusPoint" /> objects in the <see cref="T:System.Windows.Input.StylusPointCollection" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.StylusPointDescription" /> that is associated with the <see cref="T:System.Windows.Input.StylusPoint" /> objects in the <see cref="T:System.Windows.Input.StylusPointCollection" />.</returns>
	public StylusPointDescription Description
	{
		get
		{
			if (_stylusPointDescription == null)
			{
				_stylusPointDescription = new StylusPointDescription();
			}
			return _stylusPointDescription;
		}
	}

	/// <summary>Occurs when the <see cref="T:System.Windows.Input.StylusPointCollection" /> changes.</summary>
	public event EventHandler Changed;

	internal event CancelEventHandler CountGoingToZero;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusPointCollection" /> class. </summary>
	public StylusPointCollection()
	{
		_stylusPointDescription = new StylusPointDescription();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusPointCollection" /> class that may initially contain the specified number of <see cref="T:System.Windows.Input.StylusPoint" /> objects.</summary>
	/// <param name="initialCapacity">The number of <see cref="T:System.Windows.Input.StylusPoint" /> objects the <see cref="T:System.Windows.Input.StylusPointCollection" /> can initially contain.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="initialCapacity" /> is negative.</exception>
	public StylusPointCollection(int initialCapacity)
		: this()
	{
		if (initialCapacity < 0)
		{
			throw new ArgumentException(SR.InvalidStylusPointConstructionZeroLengthCollection, "initialCapacity");
		}
		((List<StylusPoint>)base.Items).Capacity = initialCapacity;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusPointCollection" /> class that contains the properties specified in the <see cref="T:System.Windows.Input.StylusPointDescription" />.</summary>
	/// <param name="stylusPointDescription">A <see cref="T:System.Windows.Input.StylusPointDescription" /> that specifies the additional properties stored in each <see cref="T:System.Windows.Input.StylusPoint" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stylusPointDescription" /> is null.</exception>
	public StylusPointCollection(StylusPointDescription stylusPointDescription)
	{
		if (stylusPointDescription == null)
		{
			throw new ArgumentNullException();
		}
		_stylusPointDescription = stylusPointDescription;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusPointCollection" /> class that is the specified size and contains the properties specified in the <see cref="T:System.Windows.Input.StylusPointDescription" />.</summary>
	/// <param name="stylusPointDescription">A <see cref="T:System.Windows.Input.StylusPointDescription" /> that specifies the additional properties stored in each <see cref="T:System.Windows.Input.StylusPoint" />.</param>
	/// <param name="initialCapacity">The number of <see cref="T:System.Windows.Input.StylusPoint" /> objects the <see cref="T:System.Windows.Input.StylusPointCollection" /> can initially contain.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="initialCapacity" /> is negative.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stylusPointDescription" /> is null.</exception>
	public StylusPointCollection(StylusPointDescription stylusPointDescription, int initialCapacity)
		: this(stylusPointDescription)
	{
		if (initialCapacity < 0)
		{
			throw new ArgumentException(SR.InvalidStylusPointConstructionZeroLengthCollection, "initialCapacity");
		}
		((List<StylusPoint>)base.Items).Capacity = initialCapacity;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusPointCollection" /> class with the specified <see cref="T:System.Windows.Input.StylusPoint" /> objects. </summary>
	/// <param name="stylusPoints">A generic IEnumerable of type <see cref="T:System.Windows.Input.StylusPoint" /> to add to the <see cref="T:System.Windows.Input.StylusPointCollection" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stylusPoints" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="points" /> is 0.-or-The <see cref="T:System.Windows.Input.StylusPoint" /> objects in <paramref name="stylusPoints" /> have incompatible <see cref="T:System.Windows.Input.StylusPointDescription" /> objects.</exception>
	public StylusPointCollection(IEnumerable<StylusPoint> stylusPoints)
	{
		if (stylusPoints == null)
		{
			throw new ArgumentNullException("stylusPoints");
		}
		List<StylusPoint> list = new List<StylusPoint>(stylusPoints);
		if (list.Count == 0)
		{
			throw new ArgumentException(SR.InvalidStylusPointConstructionZeroLengthCollection, "stylusPoints");
		}
		_stylusPointDescription = list[0].Description;
		((List<StylusPoint>)base.Items).Capacity = list.Count;
		for (int i = 0; i < list.Count; i++)
		{
			Add(list[i]);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusPointCollection" /> class with specified points. </summary>
	/// <param name="points">A generic IEnumerable of type <see cref="T:System.Windows.Point" /> that specifies the <see cref="T:System.Windows.Input.StylusPoint" /> objects to add to the <see cref="T:System.Windows.Input.StylusPointCollection" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="points" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="points" /> is 0.</exception>
	public StylusPointCollection(IEnumerable<Point> points)
		: this()
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		List<StylusPoint> list = new List<StylusPoint>();
		foreach (Point point in points)
		{
			list.Add(new StylusPoint(point.X, point.Y));
		}
		if (list.Count == 0)
		{
			throw new ArgumentException(SR.InvalidStylusPointConstructionZeroLengthCollection, "points");
		}
		((List<StylusPoint>)base.Items).Capacity = list.Count;
		((List<StylusPoint>)base.Items).AddRange(list);
	}

	internal StylusPointCollection(StylusPointDescription stylusPointDescription, int[] rawPacketData, GeneralTransform tabletToView, Matrix tabletToViewMatrix)
	{
		if (stylusPointDescription == null)
		{
			throw new ArgumentNullException("stylusPointDescription");
		}
		_stylusPointDescription = stylusPointDescription;
		int inputArrayLengthPerPoint = stylusPointDescription.GetInputArrayLengthPerPoint();
		int num = rawPacketData.Length / inputArrayLengthPerPoint;
		((List<StylusPoint>)base.Items).Capacity = num;
		int num2 = 0;
		int num3 = 0;
		while (num2 < num)
		{
			Point result = new Point(rawPacketData[num3], rawPacketData[num3 + 1]);
			if (tabletToView != null)
			{
				tabletToView.TryTransform(result, out result);
			}
			else
			{
				result = tabletToViewMatrix.Transform(result);
			}
			int num4 = 2;
			bool containsTruePressure = stylusPointDescription.ContainsTruePressure;
			if (containsTruePressure)
			{
				num4++;
			}
			int[] additionalValues = null;
			int num5 = inputArrayLengthPerPoint - num4;
			if (num5 > 0)
			{
				int start = num3 + num4;
				additionalValues = rawPacketData.AsSpan(start, num5).ToArray();
			}
			StylusPoint item = new StylusPoint(result.X, result.Y, 0.5f, _stylusPointDescription, additionalValues, validateAdditionalData: false, validatePressureFactor: false);
			if (containsTruePressure)
			{
				int value = rawPacketData[num3 + 2];
				item.SetPropertyValue(StylusPointProperties.NormalPressure, value);
			}
			((List<StylusPoint>)base.Items).Add(item);
			num2++;
			num3 += inputArrayLengthPerPoint;
		}
	}

	/// <summary>Adds the specified <see cref="T:System.Windows.Input.StylusPointCollection" /> to the current <see cref="T:System.Windows.Input.StylusPointCollection" />.</summary>
	/// <param name="stylusPoints">The <see cref="T:System.Windows.Input.StylusPointCollection" /> to add to the current <see cref="T:System.Windows.Input.StylusPointCollection" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stylusPoints" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Windows.Input.StylusPointDescription" /> of <paramref name="stylusPoints" /> is not compatible with the <see cref="P:System.Windows.Input.StylusPointCollection.Description" /> property.</exception>
	public void Add(StylusPointCollection stylusPoints)
	{
		if (stylusPoints == null)
		{
			throw new ArgumentNullException("stylusPoints");
		}
		if (!StylusPointDescription.AreCompatible(stylusPoints.Description, _stylusPointDescription))
		{
			throw new ArgumentException(SR.IncompatibleStylusPointDescriptions, "stylusPoints");
		}
		int count = stylusPoints.Count;
		for (int i = 0; i < count; i++)
		{
			StylusPoint item = stylusPoints[i];
			item.Description = _stylusPointDescription;
			((List<StylusPoint>)base.Items).Add(item);
		}
		if (stylusPoints.Count > 0)
		{
			OnChanged(EventArgs.Empty);
		}
	}

	/// <summary>Removes all <see cref="T:System.Windows.Input.StylusPoint" /> objects from the <see cref="T:System.Windows.Input.StylusPointCollection" />.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Input.StylusPointCollection" /> is connected to a <see cref="T:System.Windows.Ink.Stroke" />.</exception>
	protected sealed override void ClearItems()
	{
		if (CanGoToZero())
		{
			base.ClearItems();
			OnChanged(EventArgs.Empty);
			return;
		}
		throw new InvalidOperationException(SR.InvalidStylusPointCollectionZeroCount);
	}

	/// <summary>Removes the <see cref="T:System.Windows.Input.StylusPoint" /> at the specified position from the <see cref="T:System.Windows.Input.StylusPointCollection" />.</summary>
	/// <param name="index">The position at which to remove the <see cref="T:System.Windows.Input.StylusPoint" />.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Input.StylusPointCollection" /> is connected to a <see cref="T:System.Windows.Ink.Stroke" /> and there is only one <see cref="T:System.Windows.Input.StylusPoint" /> in the <see cref="T:System.Windows.Input.StylusPointCollection" />.</exception>
	protected sealed override void RemoveItem(int index)
	{
		if (base.Count > 1 || CanGoToZero())
		{
			base.RemoveItem(index);
			OnChanged(EventArgs.Empty);
			return;
		}
		throw new InvalidOperationException(SR.InvalidStylusPointCollectionZeroCount);
	}

	/// <summary>Inserts the specified <see cref="T:System.Windows.Input.StylusPoint" /> into the <see cref="T:System.Windows.Input.StylusPointCollection" /> at the specified position.</summary>
	/// <param name="index">The position at which to insert the <see cref="T:System.Windows.Input.StylusPoint" />.</param>
	/// <param name="stylusPoint">The <see cref="T:System.Windows.Input.StylusPoint" /> to insert into the <see cref="T:System.Windows.Input.StylusPointCollection" />.</param>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Windows.Input.StylusPointDescription" /> of <paramref name="stylusPoint" /> is not compatible with the <see cref="P:System.Windows.Input.StylusPointCollection.Description" /> property.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is greater or equal to the number of <see cref="T:System.Windows.Input.StylusPoint" /> objects in the <see cref="T:System.Windows.Input.StylusPointCollection" />.</exception>
	protected sealed override void InsertItem(int index, StylusPoint stylusPoint)
	{
		if (!StylusPointDescription.AreCompatible(stylusPoint.Description, _stylusPointDescription))
		{
			throw new ArgumentException(SR.IncompatibleStylusPointDescriptions, "stylusPoint");
		}
		stylusPoint.Description = _stylusPointDescription;
		base.InsertItem(index, stylusPoint);
		OnChanged(EventArgs.Empty);
	}

	/// <summary>Sets the specified <see cref="T:System.Windows.Input.StylusPoint" /> at the specified position.</summary>
	/// <param name="index">The position at which to set the <see cref="T:System.Windows.Input.StylusPoint" />.</param>
	/// <param name="stylusPoint">The <see cref="T:System.Windows.Input.StylusPoint" /> to set at the specified position.</param>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Windows.Input.StylusPointDescription" /> of <paramref name="stylusPoint" /> is not compatible with the <see cref="P:System.Windows.Input.StylusPointCollection.Description" /> property.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is greater than the number of <see cref="T:System.Windows.Input.StylusPoint" /> objects in the <see cref="T:System.Windows.Input.StylusPointCollection" />.</exception>
	protected sealed override void SetItem(int index, StylusPoint stylusPoint)
	{
		if (!StylusPointDescription.AreCompatible(stylusPoint.Description, _stylusPointDescription))
		{
			throw new ArgumentException(SR.IncompatibleStylusPointDescriptions, "stylusPoint");
		}
		stylusPoint.Description = _stylusPointDescription;
		base.SetItem(index, stylusPoint);
		OnChanged(EventArgs.Empty);
	}

	/// <summary>Copies the <see cref="T:System.Windows.Input.StylusPointCollection" />.</summary>
	/// <returns>A new <see cref="T:System.Windows.Input.StylusPointCollection" /> that contains copies of the <see cref="T:System.Windows.Input.StylusPoint" /> objects in the current <see cref="T:System.Windows.Input.StylusPointCollection" />.</returns>
	public StylusPointCollection Clone()
	{
		return Clone(System.Windows.Media.Transform.Identity, Description, base.Count);
	}

	/// <summary>Converts a <see cref="T:System.Windows.Input.StylusPointCollection" /> into a point array.</summary>
	/// <returns>A point array that contains points that correspond to each <see cref="T:System.Windows.Input.StylusPoint" /> in the <see cref="T:System.Windows.Input.StylusPointCollection" />.</returns>
	/// <param name="stylusPoints">The stylus point collection to convert to a point array.</param>
	public static explicit operator Point[](StylusPointCollection stylusPoints)
	{
		if (stylusPoints == null)
		{
			return null;
		}
		Point[] array = new Point[stylusPoints.Count];
		for (int i = 0; i < stylusPoints.Count; i++)
		{
			array[i] = new Point(stylusPoints[i].X, stylusPoints[i].Y);
		}
		return array;
	}

	internal StylusPointCollection Clone(int count)
	{
		if (count > base.Count || count < 1)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		return Clone(System.Windows.Media.Transform.Identity, Description, count);
	}

	internal StylusPointCollection Clone(GeneralTransform transform, StylusPointDescription descriptionToUse)
	{
		return Clone(transform, descriptionToUse, base.Count);
	}

	private StylusPointCollection Clone(GeneralTransform transform, StylusPointDescription descriptionToUse, int count)
	{
		StylusPointCollection stylusPointCollection = new StylusPointCollection(descriptionToUse, count);
		bool flag = transform is Transform && ((Transform)transform).IsIdentity;
		for (int i = 0; i < count; i++)
		{
			if (flag)
			{
				((List<StylusPoint>)stylusPointCollection.Items).Add(base[i]);
				continue;
			}
			Point result = default(Point);
			StylusPoint item = base[i];
			result.X = item.X;
			result.Y = item.Y;
			transform.TryTransform(result, out result);
			item.X = result.X;
			item.Y = result.Y;
			((List<StylusPoint>)stylusPointCollection.Items).Add(item);
		}
		return stylusPointCollection;
	}

	/// <summary>Raises the <see cref="E:System.Windows.Input.StylusPointCollection.Changed" /> event.</summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
	protected virtual void OnChanged(EventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		if (this.Changed != null)
		{
			this.Changed(this, e);
		}
	}

	internal void Transform(GeneralTransform transform)
	{
		Point result = default(Point);
		for (int i = 0; i < base.Count; i++)
		{
			StylusPoint value = base[i];
			result.X = value.X;
			result.Y = value.Y;
			transform.TryTransform(result, out result);
			value.X = result.X;
			value.Y = result.Y;
			((List<StylusPoint>)base.Items)[i] = value;
		}
		if (base.Count > 0)
		{
			OnChanged(EventArgs.Empty);
		}
	}

	/// <summary>Finds the intersection of the specified <see cref="T:System.Windows.Input.StylusPointDescription" /> and the <see cref="P:System.Windows.Input.StylusPointCollection.Description" /> property.</summary>
	/// <returns>A <see cref="T:System.Windows.Input.StylusPointCollection" /> that has a <see cref="T:System.Windows.Input.StylusPointDescription" /> that is a subset of the specified <see cref="T:System.Windows.Input.StylusPointDescription" /> and the <see cref="T:System.Windows.Input.StylusPointDescription" /> that the current <see cref="T:System.Windows.Input.StylusPointCollection" /> uses.</returns>
	/// <param name="subsetToReformatTo">A <see cref="T:System.Windows.Input.StylusPointDescription" /> to intersect with the <see cref="T:System.Windows.Input.StylusPointDescription" /> of the current <see cref="T:System.Windows.Input.StylusPointCollection" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="subsetToReformatTo" /> is not a subset of the <see cref="P:System.Windows.Input.StylusPointCollection.Description" /> property.</exception>
	public StylusPointCollection Reformat(StylusPointDescription subsetToReformatTo)
	{
		return Reformat(subsetToReformatTo, System.Windows.Media.Transform.Identity);
	}

	internal StylusPointCollection Reformat(StylusPointDescription subsetToReformatTo, GeneralTransform transform)
	{
		if (!subsetToReformatTo.IsSubsetOf(Description))
		{
			throw new ArgumentException(SR.InvalidStylusPointDescriptionSubset, "subsetToReformatTo");
		}
		StylusPointDescription commonDescription = StylusPointDescription.GetCommonDescription(subsetToReformatTo, Description);
		if (StylusPointDescription.AreCompatible(Description, commonDescription) && transform is Transform && ((Transform)transform).IsIdentity)
		{
			return Clone(transform, commonDescription);
		}
		StylusPointCollection stylusPointCollection = new StylusPointCollection(commonDescription, base.Count);
		int expectedAdditionalDataCount = commonDescription.GetExpectedAdditionalDataCount();
		ReadOnlyCollection<StylusPointPropertyInfo> stylusPointProperties = commonDescription.GetStylusPointProperties();
		bool flag = transform is Transform && ((Transform)transform).IsIdentity;
		for (int i = 0; i < base.Count; i++)
		{
			StylusPoint stylusPoint = base[i];
			double x = stylusPoint.X;
			double y = stylusPoint.Y;
			float untruncatedPressureFactor = stylusPoint.GetUntruncatedPressureFactor();
			if (!flag)
			{
				Point result = new Point(x, y);
				transform.TryTransform(result, out result);
				x = result.X;
				y = result.Y;
			}
			int[] additionalValues = null;
			if (expectedAdditionalDataCount > 0)
			{
				additionalValues = new int[expectedAdditionalDataCount];
			}
			StylusPoint item = new StylusPoint(x, y, untruncatedPressureFactor, commonDescription, additionalValues, validateAdditionalData: false, validatePressureFactor: false);
			for (int j = 3; j < stylusPointProperties.Count; j++)
			{
				int propertyValue = stylusPoint.GetPropertyValue(stylusPointProperties[j]);
				item.SetPropertyValue(stylusPointProperties[j], propertyValue, copyBeforeWrite: false);
			}
			((List<StylusPoint>)stylusPointCollection.Items).Add(item);
		}
		return stylusPointCollection;
	}

	/// <summary>Converts the property values of the <see cref="T:System.Windows.Input.StylusPoint" /> objects into a 32-bit signed integer array.</summary>
	public int[] ToHiMetricArray()
	{
		int outputArrayLengthPerPoint = Description.GetOutputArrayLengthPerPoint();
		int[] array = new int[outputArrayLengthPerPoint * base.Count];
		int num = 0;
		int num2 = 0;
		while (num < base.Count)
		{
			StylusPoint stylusPoint = base[num];
			array[num2] = (int)Math.Round(stylusPoint.X * 26.458333333333332);
			array[num2 + 1] = (int)Math.Round(stylusPoint.Y * 26.458333333333332);
			array[num2 + 2] = stylusPoint.GetPropertyValue(StylusPointProperties.NormalPressure);
			if (outputArrayLengthPerPoint > 3)
			{
				int[] additionalData = stylusPoint.GetAdditionalData();
				int num3 = outputArrayLengthPerPoint - 3;
				for (int i = 0; i < num3; i++)
				{
					array[num2 + i + 3] = additionalData[i];
				}
			}
			num++;
			num2 += outputArrayLengthPerPoint;
		}
		return array;
	}

	internal void ToISFReadyArrays(out int[][] output, out bool shouldPersistPressure)
	{
		int num = Description.GetOutputArrayLengthPerPoint();
		if (Description.ButtonCount > 0)
		{
			num--;
		}
		output = new int[num][];
		for (int i = 0; i < num; i++)
		{
			output[i] = new int[base.Count];
		}
		StylusPointPropertyInfo propertyInfo = Description.GetPropertyInfo(StylusPointPropertyIds.NormalPressure);
		shouldPersistPressure = !StylusPointPropertyInfo.AreCompatible(propertyInfo, StylusPointPropertyInfoDefaults.NormalPressure);
		for (int j = 0; j < base.Count; j++)
		{
			StylusPoint stylusPoint = base[j];
			output[0][j] = (int)Math.Round(stylusPoint.X * 26.458333333333332);
			output[1][j] = (int)Math.Round(stylusPoint.Y * 26.458333333333332);
			output[2][j] = stylusPoint.GetPropertyValue(StylusPointProperties.NormalPressure);
			if (!shouldPersistPressure && !stylusPoint.HasDefaultPressure)
			{
				shouldPersistPressure = true;
			}
			if (num > 3)
			{
				int[] additionalData = stylusPoint.GetAdditionalData();
				int num2 = num - 3;
				for (int k = 0; k < num2; k++)
				{
					output[k + 3][j] = additionalData[k];
				}
			}
		}
	}

	private bool CanGoToZero()
	{
		if (this.CountGoingToZero == null)
		{
			return true;
		}
		CancelEventArgs cancelEventArgs = new CancelEventArgs();
		cancelEventArgs.Cancel = false;
		this.CountGoingToZero(this, cancelEventArgs);
		return !cancelEventArgs.Cancel;
	}
}
