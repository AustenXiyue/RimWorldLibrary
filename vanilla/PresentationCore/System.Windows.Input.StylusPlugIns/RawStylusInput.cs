using System.Windows.Media;
using MS.Internal.PresentationCore;

namespace System.Windows.Input.StylusPlugIns;

/// <summary>Provides information about input from a <see cref="T:System.Windows.Input.StylusDevice" /> to a <see cref="T:System.Windows.Input.StylusPlugIns.StylusPlugIn" />.</summary>
public class RawStylusInput
{
	private RawStylusInputReport _report;

	private GeneralTransform _tabletToElementTransform;

	private StylusPlugInCollection _targetPlugInCollection;

	private StylusPointCollection _stylusPoints;

	private StylusPlugIn _currentNotifyPlugIn;

	private RawStylusInputCustomDataList _customData;

	/// <summary>Gets the identifier of the current stylus device.</summary>
	/// <returns>The identifier of the current <see cref="T:System.Windows.Input.StylusDevice" />.</returns>
	public int StylusDeviceId => _report.StylusDeviceId;

	/// <summary>Gets the identifier of the current tablet device.</summary>
	/// <returns>The identifier of the current <see cref="T:System.Windows.Input.TabletDevice" />.</returns>
	public int TabletDeviceId => _report.TabletDeviceId;

	/// <summary>Gets the time at which the input occurred.</summary>
	/// <returns>The time at which the input occurred.</returns>
	public int Timestamp => _report.Timestamp;

	internal bool StylusPointsModified => _stylusPoints != null;

	internal StylusPlugInCollection Target => _targetPlugInCollection;

	internal RawStylusInputReport Report => _report;

	internal GeneralTransform ElementTransform => _tabletToElementTransform;

	internal RawStylusInputCustomDataList CustomDataList
	{
		get
		{
			if (_customData == null)
			{
				_customData = new RawStylusInputCustomDataList();
			}
			return _customData;
		}
	}

	internal StylusPlugIn CurrentNotifyPlugIn
	{
		get
		{
			return _currentNotifyPlugIn;
		}
		set
		{
			_currentNotifyPlugIn = value;
		}
	}

	internal RawStylusInput(RawStylusInputReport report, GeneralTransform tabletToElementTransform, StylusPlugInCollection targetPlugInCollection)
	{
		if (report == null)
		{
			throw new ArgumentNullException("report");
		}
		if (tabletToElementTransform.Inverse == null)
		{
			throw new ArgumentException(SR.Stylus_MatrixNotInvertable, "tabletToElementTransform");
		}
		if (targetPlugInCollection == null)
		{
			throw new ArgumentNullException("targetPlugInCollection");
		}
		_report = report;
		_tabletToElementTransform = tabletToElementTransform;
		_targetPlugInCollection = targetPlugInCollection;
	}

	/// <summary>Gets the stylus points that are collected from the stylus.</summary>
	/// <returns>The stylus points that are collected from the stylus.</returns>
	public StylusPointCollection GetStylusPoints()
	{
		return GetStylusPoints(Transform.Identity);
	}

	internal StylusPointCollection GetStylusPoints(GeneralTransform transform)
	{
		if (_stylusPoints == null)
		{
			GeneralTransformGroup generalTransformGroup = new GeneralTransformGroup();
			if (StylusDeviceId == 0)
			{
				generalTransformGroup.Children.Add(new MatrixTransform(_report.InputSource.CompositionTarget.TransformFromDevice));
			}
			generalTransformGroup.Children.Add(_tabletToElementTransform);
			if (transform != null)
			{
				generalTransformGroup.Children.Add(transform);
			}
			return new StylusPointCollection(_report.StylusPointDescription, _report.GetRawPacketData(), generalTransformGroup, Matrix.Identity);
		}
		return _stylusPoints.Clone(transform, _stylusPoints.Description);
	}

	/// <summary>Sets the stylus points that are passed to the application thread.</summary>
	/// <param name="stylusPoints">The stylus points to pass to the application thread.</param>
	public void SetStylusPoints(StylusPointCollection stylusPoints)
	{
		if (stylusPoints == null)
		{
			throw new ArgumentNullException("stylusPoints");
		}
		if (!StylusPointDescription.AreCompatible(stylusPoints.Description, _report.StylusPointDescription))
		{
			throw new ArgumentException(SR.IncompatibleStylusPointDescriptions, "stylusPoints");
		}
		if (stylusPoints.Count == 0)
		{
			throw new ArgumentException(SR.Stylus_StylusPointsCantBeEmpty, "stylusPoints");
		}
		_stylusPoints = stylusPoints.Clone();
	}

	/// <summary>Subscribes to the application thread's corresponding stylus methods.</summary>
	/// <param name="callbackData">The data to pass to the application thread.</param>
	public void NotifyWhenProcessed(object callbackData)
	{
		if (_currentNotifyPlugIn == null)
		{
			throw new InvalidOperationException(SR.Stylus_CanOnlyCallForDownMoveOrUp);
		}
		if (_customData == null)
		{
			_customData = new RawStylusInputCustomDataList();
		}
		_customData.Add(new RawStylusInputCustomData(_currentNotifyPlugIn, callbackData));
	}
}
