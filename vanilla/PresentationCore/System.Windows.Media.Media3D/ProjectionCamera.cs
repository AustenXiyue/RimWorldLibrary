using System.Windows.Media.Animation;

namespace System.Windows.Media.Media3D;

/// <summary>An abstract base class for perspective and orthographic projection cameras.</summary>
public abstract class ProjectionCamera : Camera
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.ProjectionCamera.NearPlaneDistance" /> dependency property.. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.ProjectionCamera.NearPlaneDistance" /> dependency property.</returns>
	public static readonly DependencyProperty NearPlaneDistanceProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.ProjectionCamera.FarPlaneDistance" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.ProjectionCamera.FarPlaneDistance" /> dependency property.</returns>
	public static readonly DependencyProperty FarPlaneDistanceProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.ProjectionCamera.Position" /> dependency property.. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.ProjectionCamera.Position" /> dependency property.</returns>
	public static readonly DependencyProperty PositionProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.ProjectionCamera.LookDirection" /> dependency property..</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.ProjectionCamera.LookDirection" /> dependency property.</returns>
	public static readonly DependencyProperty LookDirectionProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.ProjectionCamera.UpDirection" /> dependency property..</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.ProjectionCamera.UpDirection" /> dependency property.</returns>
	public static readonly DependencyProperty UpDirectionProperty;

	internal const double c_NearPlaneDistance = 0.125;

	internal const double c_FarPlaneDistance = double.PositiveInfinity;

	internal static Point3D s_Position;

	internal static Vector3D s_LookDirection;

	internal static Vector3D s_UpDirection;

	/// <summary>Gets or sets a value that specifies the distance from the camera of the camera's near clip plane.  </summary>
	/// <returns>Double that specifies the distance from the camera of the camera's near clip plane.</returns>
	public double NearPlaneDistance
	{
		get
		{
			return (double)GetValue(NearPlaneDistanceProperty);
		}
		set
		{
			SetValueInternal(NearPlaneDistanceProperty, value);
		}
	}

	/// <summary>Gets or sets a value that specifies the distance from the camera of the camera's far clip plane.  </summary>
	/// <returns>Double that specifies the distance from the camera of the camera's far clip plane.</returns>
	public double FarPlaneDistance
	{
		get
		{
			return (double)GetValue(FarPlaneDistanceProperty);
		}
		set
		{
			SetValueInternal(FarPlaneDistanceProperty, value);
		}
	}

	/// <summary> Gets or sets the position of the camera in world coordinates.  </summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Media3D.Point3D" /> that specifies the position of the camera.</returns>
	public Point3D Position
	{
		get
		{
			return (Point3D)GetValue(PositionProperty);
		}
		set
		{
			SetValueInternal(PositionProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> which defines the direction in which the camera is looking in world coordinates.  </summary>
	/// <returns>Vector3D that represents the direction of the camera's field of view.</returns>
	public Vector3D LookDirection
	{
		get
		{
			return (Vector3D)GetValue(LookDirectionProperty);
		}
		set
		{
			SetValueInternal(LookDirectionProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> which defines the upward direction of the camera.  </summary>
	/// <returns>Vector3D that represents the upward direction in the scene projection.</returns>
	public Vector3D UpDirection
	{
		get
		{
			return (Vector3D)GetValue(UpDirectionProperty);
		}
		set
		{
			SetValueInternal(UpDirectionProperty, value);
		}
	}

	internal ProjectionCamera()
	{
	}

	internal override Matrix3D GetViewMatrix()
	{
		Point3D position = Position;
		Vector3D lookDirection = LookDirection;
		Vector3D upDirection = UpDirection;
		return CreateViewMatrix(base.Transform, ref position, ref lookDirection, ref upDirection);
	}

	internal static Matrix3D CreateViewMatrix(Transform3D transform, ref Point3D position, ref Vector3D lookDirection, ref Vector3D upDirection)
	{
		Vector3D vector3D = -lookDirection;
		vector3D.Normalize();
		Vector3D vector3D2 = Vector3D.CrossProduct(upDirection, vector3D);
		vector3D2.Normalize();
		Vector3D vector = Vector3D.CrossProduct(vector3D, vector3D2);
		Vector3D vector2 = (Vector3D)position;
		double offsetX = 0.0 - Vector3D.DotProduct(vector3D2, vector2);
		double offsetY = 0.0 - Vector3D.DotProduct(vector, vector2);
		double offsetZ = 0.0 - Vector3D.DotProduct(vector3D, vector2);
		Matrix3D viewMatrix = new Matrix3D(vector3D2.X, vector.X, vector3D.X, 0.0, vector3D2.Y, vector.Y, vector3D.Y, 0.0, vector3D2.Z, vector.Z, vector3D.Z, 0.0, offsetX, offsetY, offsetZ, 1.0);
		Camera.PrependInverseTransform(transform, ref viewMatrix);
		return viewMatrix;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.ProjectionCamera" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new ProjectionCamera Clone()
	{
		return (ProjectionCamera)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.ProjectionCamera" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new ProjectionCamera CloneCurrentValue()
	{
		return (ProjectionCamera)base.CloneCurrentValue();
	}

	private static void NearPlaneDistancePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ProjectionCamera)d).PropertyChanged(NearPlaneDistanceProperty);
	}

	private static void FarPlaneDistancePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ProjectionCamera)d).PropertyChanged(FarPlaneDistanceProperty);
	}

	private static void PositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ProjectionCamera)d).PropertyChanged(PositionProperty);
	}

	private static void LookDirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ProjectionCamera)d).PropertyChanged(LookDirectionProperty);
	}

	private static void UpDirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ProjectionCamera)d).PropertyChanged(UpDirectionProperty);
	}

	static ProjectionCamera()
	{
		s_Position = default(Point3D);
		s_LookDirection = new Vector3D(0.0, 0.0, -1.0);
		s_UpDirection = new Vector3D(0.0, 1.0, 0.0);
		Type typeFromHandle = typeof(ProjectionCamera);
		NearPlaneDistanceProperty = Animatable.RegisterProperty("NearPlaneDistance", typeof(double), typeFromHandle, 0.125, NearPlaneDistancePropertyChanged, null, isIndependentlyAnimated: true, null);
		FarPlaneDistanceProperty = Animatable.RegisterProperty("FarPlaneDistance", typeof(double), typeFromHandle, double.PositiveInfinity, FarPlaneDistancePropertyChanged, null, isIndependentlyAnimated: true, null);
		PositionProperty = Animatable.RegisterProperty("Position", typeof(Point3D), typeFromHandle, default(Point3D), PositionPropertyChanged, null, isIndependentlyAnimated: true, null);
		LookDirectionProperty = Animatable.RegisterProperty("LookDirection", typeof(Vector3D), typeFromHandle, new Vector3D(0.0, 0.0, -1.0), LookDirectionPropertyChanged, null, isIndependentlyAnimated: true, null);
		UpDirectionProperty = Animatable.RegisterProperty("UpDirection", typeof(Vector3D), typeFromHandle, new Vector3D(0.0, 1.0, 0.0), UpDirectionPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
