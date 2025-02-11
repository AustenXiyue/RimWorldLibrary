using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal.Media3D;

namespace System.Windows.Media.Media3D;

/// <summary>Provides functionality for 3-D models. </summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public abstract class Model3D : Animatable, IFormattable, DUCE.IResource
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.Model3D.Transform" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.Model3D.Transform" /> dependency property.</returns>
	public static readonly DependencyProperty TransformProperty;

	internal static Transform3D s_Transform;

	/// <summary>Gets a <see cref="T:System.Windows.Media.Media3D.Rect3D" /> that specifies the axis-aligned bounding box of this <see cref="T:System.Windows.Media.Media3D.Model3D" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Media3D.Rect3D" /> bounding box for the model.</returns>
	public Rect3D Bounds
	{
		get
		{
			ReadPreamble();
			return CalculateSubgraphBoundsOuterSpace();
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Media3D.Transform3D" /> set on the model. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Media3D.Transform3D" /> set on the model. The default value is <see cref="T:System.Windows.Media.Media3D.MatrixTransform3D" />.</returns>
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

	internal Model3D()
	{
	}

	internal void RayHitTest(RayHitTestParameters rayParams)
	{
		Transform3D transform = Transform;
		rayParams.PushModelTransform(transform);
		RayHitTestCore(rayParams);
		rayParams.PopTransform(transform);
	}

	internal abstract void RayHitTestCore(RayHitTestParameters rayParams);

	internal Rect3D CalculateSubgraphBoundsOuterSpace()
	{
		Rect3D originalBox = CalculateSubgraphBoundsInnerSpace();
		return M3DUtil.ComputeTransformedAxisAlignedBoundingBox(ref originalBox, Transform);
	}

	internal abstract Rect3D CalculateSubgraphBoundsInnerSpace();

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.Model3D" />, and makes deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (although they might no longer resolve) but does not copy animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Model3D Clone()
	{
		return (Model3D)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.Model3D" /> object, and makes deep copies of this object's current values. Resource references, data bindings, and animations are not copied; however, their current values are copied.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Model3D CloneCurrentValue()
	{
		return (Model3D)base.CloneCurrentValue();
	}

	private static void TransformPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		Model3D model3D = (Model3D)d;
		Transform3D resource = (Transform3D)e.OldValue;
		Transform3D resource2 = (Transform3D)e.NewValue;
		if (model3D.Dispatcher != null)
		{
			DUCE.IResource resource3 = model3D;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					model3D.ReleaseResource(resource, channel);
					model3D.AddRefResource(resource2, channel);
				}
			}
		}
		model3D.PropertyChanged(TransformProperty);
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

	/// <summary>Creates a string representation of the Model3D. </summary>
	/// <returns>A string representation of the object.</returns>
	public override string ToString()
	{
		ReadPreamble();
		return ConvertToString(null, null);
	}

	/// <summary>Creates a string representation of the Model3D. </summary>
	/// <returns>A string representation of the Model3D.</returns>
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

	static Model3D()
	{
		s_Transform = Transform3D.Identity;
		Type typeFromHandle = typeof(Model3D);
		TransformProperty = Animatable.RegisterProperty("Transform", typeof(Transform3D), typeFromHandle, Transform3D.Identity, TransformPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
