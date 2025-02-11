using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace MS.Internal.Ink;

internal sealed class EraserBehavior : StylusEditingBehavior
{
	private InkCanvasEditingMode _cachedEraseMode;

	private IncrementalStrokeHitTester _incrementalStrokeHitTester;

	private Cursor _cachedPointEraserCursor;

	private StylusShape _cachedStylusShape;

	private StylusPointCollection _stylusPoints;

	internal EraserBehavior(EditingCoordinator editingCoordinator, InkCanvas inkCanvas)
		: base(editingCoordinator, inkCanvas)
	{
	}

	protected override void OnSwitchToMode(InkCanvasEditingMode mode)
	{
		switch (mode)
		{
		case InkCanvasEditingMode.Ink:
		case InkCanvasEditingMode.GestureOnly:
		case InkCanvasEditingMode.InkAndGesture:
			Commit(commit: true);
			base.EditingCoordinator.ChangeStylusEditingMode(this, mode);
			break;
		case InkCanvasEditingMode.EraseByPoint:
		case InkCanvasEditingMode.EraseByStroke:
			Commit(commit: true);
			base.EditingCoordinator.ChangeStylusEditingMode(this, mode);
			break;
		case InkCanvasEditingMode.Select:
		{
			StylusPointCollection stylusPointCollection = ((_stylusPoints != null) ? _stylusPoints.Clone() : null);
			Commit(commit: true);
			IStylusEditing stylusEditing = base.EditingCoordinator.ChangeStylusEditingMode(this, mode);
			if (stylusPointCollection != null)
			{
				stylusEditing?.AddStylusPoints(stylusPointCollection, userInitiated: false);
			}
			break;
		}
		case InkCanvasEditingMode.None:
			Commit(commit: true);
			base.EditingCoordinator.ChangeStylusEditingMode(this, mode);
			break;
		}
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		InkCanvasEditingMode activeEditingMode = base.EditingCoordinator.ActiveEditingMode;
		if (_cachedEraseMode != activeEditingMode)
		{
			_cachedEraseMode = activeEditingMode;
			base.EditingCoordinator.InvalidateBehaviorCursor(this);
		}
		else if (activeEditingMode == InkCanvasEditingMode.EraseByPoint)
		{
			bool flag = _cachedStylusShape != null;
			if (flag && (_cachedStylusShape.Width != base.InkCanvas.EraserShape.Width || _cachedStylusShape.Height != base.InkCanvas.EraserShape.Height || _cachedStylusShape.Rotation != base.InkCanvas.EraserShape.Rotation || _cachedStylusShape.GetType() != base.InkCanvas.EraserShape.GetType()))
			{
				ResetCachedPointEraserCursor();
				flag = false;
			}
			if (!flag)
			{
				base.EditingCoordinator.InvalidateBehaviorCursor(this);
			}
		}
	}

	protected override void StylusInputBegin(StylusPointCollection stylusPoints, bool userInitiated)
	{
		_incrementalStrokeHitTester = base.InkCanvas.Strokes.GetIncrementalStrokeHitTester(base.InkCanvas.EraserShape);
		if (InkCanvasEditingMode.EraseByPoint == _cachedEraseMode)
		{
			_incrementalStrokeHitTester.StrokeHit += OnPointEraseResultChanged;
		}
		else
		{
			_incrementalStrokeHitTester.StrokeHit += OnStrokeEraseResultChanged;
		}
		_stylusPoints = new StylusPointCollection(stylusPoints.Description, 100);
		_stylusPoints.Add(stylusPoints);
		_incrementalStrokeHitTester.AddPoints(stylusPoints);
		if (InkCanvasEditingMode.EraseByPoint == _cachedEraseMode)
		{
			base.EditingCoordinator.InvalidateBehaviorCursor(this);
		}
	}

	protected override void StylusInputContinue(StylusPointCollection stylusPoints, bool userInitiated)
	{
		_stylusPoints.Add(stylusPoints);
		_incrementalStrokeHitTester.AddPoints(stylusPoints);
	}

	protected override void StylusInputEnd(bool commit)
	{
		if (InkCanvasEditingMode.EraseByPoint == _cachedEraseMode)
		{
			_incrementalStrokeHitTester.StrokeHit -= OnPointEraseResultChanged;
		}
		else
		{
			_incrementalStrokeHitTester.StrokeHit -= OnStrokeEraseResultChanged;
		}
		_stylusPoints = null;
		_incrementalStrokeHitTester.EndHitTesting();
		_incrementalStrokeHitTester = null;
	}

	protected override Cursor GetCurrentCursor()
	{
		if (InkCanvasEditingMode.EraseByPoint == _cachedEraseMode)
		{
			if (_cachedPointEraserCursor == null)
			{
				_cachedStylusShape = base.InkCanvas.EraserShape;
				Matrix tranform = GetElementTransformMatrix();
				if (!tranform.IsIdentity)
				{
					if (tranform.HasInverse)
					{
						tranform.OffsetX = 0.0;
						tranform.OffsetY = 0.0;
					}
					else
					{
						tranform = Matrix.Identity;
					}
				}
				DpiScale dpi = base.InkCanvas.GetDpi();
				_cachedPointEraserCursor = PenCursorManager.GetPointEraserCursor(_cachedStylusShape, tranform, dpi.DpiScaleX, dpi.DpiScaleY);
			}
			return _cachedPointEraserCursor;
		}
		return PenCursorManager.GetStrokeEraserCursor();
	}

	protected override void OnTransformChanged()
	{
		ResetCachedPointEraserCursor();
	}

	private void ResetCachedPointEraserCursor()
	{
		_cachedPointEraserCursor = null;
		_cachedStylusShape = null;
	}

	private void OnStrokeEraseResultChanged(object sender, StrokeHitEventArgs e)
	{
		bool flag = false;
		try
		{
			InkCanvasStrokeErasingEventArgs inkCanvasStrokeErasingEventArgs = new InkCanvasStrokeErasingEventArgs(e.HitStroke);
			base.InkCanvas.RaiseStrokeErasing(inkCanvasStrokeErasingEventArgs);
			if (!inkCanvasStrokeErasingEventArgs.Cancel)
			{
				base.InkCanvas.Strokes.Remove(e.HitStroke);
				base.InkCanvas.RaiseInkErased();
			}
			flag = true;
		}
		finally
		{
			if (!flag)
			{
				Commit(commit: false);
			}
		}
	}

	private void OnPointEraseResultChanged(object sender, StrokeHitEventArgs e)
	{
		bool flag = false;
		try
		{
			InkCanvasStrokeErasingEventArgs inkCanvasStrokeErasingEventArgs = new InkCanvasStrokeErasingEventArgs(e.HitStroke);
			base.InkCanvas.RaiseStrokeErasing(inkCanvasStrokeErasingEventArgs);
			if (!inkCanvasStrokeErasingEventArgs.Cancel)
			{
				StrokeCollection pointEraseResults = e.GetPointEraseResults();
				StrokeCollection strokeCollection = new StrokeCollection();
				strokeCollection.Add(e.HitStroke);
				try
				{
					if (pointEraseResults.Count > 0)
					{
						base.InkCanvas.Strokes.Replace(strokeCollection, pointEraseResults);
					}
					else
					{
						base.InkCanvas.Strokes.Remove(strokeCollection);
					}
				}
				catch (ArgumentException ex)
				{
					if (!ex.Data.Contains("System.Windows.Ink.StrokeCollection"))
					{
						throw;
					}
				}
				base.InkCanvas.RaiseInkErased();
			}
			flag = true;
		}
		finally
		{
			if (!flag)
			{
				Commit(commit: false);
			}
		}
	}
}
