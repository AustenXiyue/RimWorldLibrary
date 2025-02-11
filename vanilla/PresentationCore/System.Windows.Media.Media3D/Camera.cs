using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media.Media3D;

/// <summary>Specifies what portion of the 3D scene is rendered by the <see cref="T:System.Windows.Media.Media3D.Viewport3DVisual" /> or <see cref="T:System.Windows.Controls.Viewport3D" /> element.</summary>
public abstract class Camera : Animatable, IFormattable, DUCE.IResource
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.Camera.Transform" /> dependency property.</summary>
	public static readonly DependencyProperty TransformProperty;

	internal static Transform3D s_Transform;

	/// <summary>Gets or sets the Transform3D applied to the camera.  </summary>
	/// <returns>Transform3D applied to the camera.</returns>
	public Transform3D Transform
	{
		get
		{
			return (Transform3D)GetValue(TransformProperty);
		}
		set
		{
			SetValueInternal(TransformProperty, value);
		}
	}

	internal Camera()
	{
	}

	internal abstract RayHitTestParameters RayFromViewportPoint(Point point, Size viewSize, Rect3D boundingRect, out double distanceAdjustment);

	internal abstract Matrix3D GetViewMatrix();

	internal abstract Matrix3D GetProjectionMatrix(double aspectRatio);

	internal static void PrependInverseTransform(Transform3D transform, ref Matrix3D viewMatrix)
	{
		if (transform != null && transform != Transform3D.Identity)
		{
			PrependInverseTransform(transform.Value, ref viewMatrix);
		}
	}

	internal static void PrependInverseTransform(Matrix3D matrix, ref Matrix3D viewMatrix)
	{
		if (!matrix.InvertCore())
		{
			viewMatrix = new Matrix3D(double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN);
		}
		else
		{
			viewMatrix.Prepend(matrix);
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.Camera" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Camera Clone()
	{
		return (Camera)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.Camera" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Camera CloneCurrentValue()
	{
		return (Camera)base.CloneCurrentValue();
	}

	private static void TransformPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		Camera camera = (Camera)d;
		Transform3D resource = (Transform3D)e.OldValue;
		Transform3D resource2 = (Transform3D)e.NewValue;
		if (camera.Dispatcher != null)
		{
			DUCE.IResource resource3 = camera;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					camera.ReleaseResource(resource, channel);
					camera.AddRefResource(resource2, channel);
				}
			}
		}
		camera.PropertyChanged(TransformProperty);
	}

	internal abstract DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel);

	DUCE.ResourceHandle DUCE.IResource.AddRefOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			return AddRefOnChannelCore(channel);
		}
	}

	internal abstract void ReleaseOnChannelCore(DUCE.Channel channel);

	void DUCE.IResource.ReleaseOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			ReleaseOnChannelCore(channel);
		}
	}

	internal abstract DUCE.ResourceHandle GetHandleCore(DUCE.Channel channel);

	DUCE.ResourceHandle DUCE.IResource.GetHandle(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			return GetHandleCore(channel);
		}
	}

	internal abstract int GetChannelCountCore();

	int DUCE.IResource.GetChannelCount()
	{
		return GetChannelCountCore();
	}

	internal abstract DUCE.Channel GetChannelCore(int index);

	DUCE.Channel DUCE.IResource.GetChannel(int index)
	{
		return GetChannelCore(index);
	}

	/// <summary>Creates a string representation of this object based on the current culture settings. </summary>
	/// <returns>String representation of this object.</returns>
	public override string ToString()
	{
		ReadPreamble();
		return ConvertToString(null, null);
	}

	/// <summary>Creates a string representation of the Camera. </summary>
	/// <returns>String representation of this object.</returns>
	/// <param name="provider">Culture-specific formatting information.</param>
	public string ToString(IFormatProvider provider)
	{
		ReadPreamble();
		return ConvertToString(null, provider);
	}

	/// <summary>Formats the value of the current instance using the specified format.</summary>
	/// <returns>The value of the current instance in the specified format.</returns>
	/// <param name="format">The format to use.-or- A null reference (Nothing in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation. </param>
	/// <param name="provider">The provider to use to format the value.-or- A null reference (Nothing in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system. </param>
	string IFormattable.ToString(string format, IFormatProvider provider)
	{
		ReadPreamble();
		return ConvertToString(format, provider);
	}

	internal virtual string ConvertToString(string format, IFormatProvider provider)
	{
		return base.ToString();
	}

	static Camera()
	{
		s_Transform = Transform3D.Identity;
		Type typeFromHandle = typeof(Camera);
		TransformProperty = Animatable.RegisterProperty("Transform", typeof(Transform3D), typeFromHandle, Transform3D.Identity, TransformPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
