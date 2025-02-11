namespace System.Windows.Media.Media3D;

/// <summary>Abstract class that represents the parameters of a 3D hit test.</summary>
public abstract class HitTestParameters3D
{
	internal Visual3D CurrentVisual;

	internal Model3D CurrentModel;

	internal GeometryModel3D CurrentGeometry;

	private Matrix3D? _hitTestProjectionMatrix;

	private Matrix3DStack _visualTransformStack = new Matrix3DStack();

	private Matrix3DStack _modelTransformStack = new Matrix3DStack();

	internal bool HasWorldTransformMatrix
	{
		get
		{
			if (_visualTransformStack.Count <= 0)
			{
				return _modelTransformStack.Count > 0;
			}
			return true;
		}
	}

	internal Matrix3D WorldTransformMatrix
	{
		get
		{
			if (_modelTransformStack.IsEmpty)
			{
				return _visualTransformStack.Top;
			}
			if (_visualTransformStack.IsEmpty)
			{
				return _modelTransformStack.Top;
			}
			return _modelTransformStack.Top * _visualTransformStack.Top;
		}
	}

	internal bool HasModelTransformMatrix => _modelTransformStack.Count > 0;

	internal Matrix3D ModelTransformMatrix => _modelTransformStack.Top;

	internal bool HasHitTestProjectionMatrix => _hitTestProjectionMatrix.HasValue;

	internal Matrix3D HitTestProjectionMatrix
	{
		get
		{
			return _hitTestProjectionMatrix.Value;
		}
		set
		{
			_hitTestProjectionMatrix = value;
		}
	}

	internal HitTestParameters3D()
	{
	}

	internal void PushVisualTransform(Transform3D transform)
	{
		if (transform != null && transform != Transform3D.Identity)
		{
			_visualTransformStack.Push(transform.Value);
		}
	}

	internal void PushModelTransform(Transform3D transform)
	{
		if (transform != null && transform != Transform3D.Identity)
		{
			_modelTransformStack.Push(transform.Value);
		}
	}

	internal void PopTransform(Transform3D transform)
	{
		if (transform != null && transform != Transform3D.Identity)
		{
			if (_modelTransformStack.Count > 0)
			{
				_modelTransformStack.Pop();
			}
			else
			{
				_visualTransformStack.Pop();
			}
		}
	}
}
