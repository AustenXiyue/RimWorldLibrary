using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media.Media3D;

/// <summary>Creates a transformation specified by a <see cref="T:System.Windows.Media.Media3D.Matrix3D" />, used to manipulate objects or coordinate systems in 3-D world space. </summary>
public sealed class MatrixTransform3D : Transform3D
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.MatrixTransform3D.Matrix" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.MatrixTransform3D.Matrix" /> dependency property.</returns>
	public static readonly DependencyProperty MatrixProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static Matrix3D s_Matrix;

	/// <summary>Gets a matrix representation of the 3-D transformation.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> representation of the 3-D transformation.</returns>
	public override Matrix3D Value => Matrix;

	/// <summary>Gets a value that indicates whether the transform is affine. </summary>
	/// <returns>true if the transform is affine; otherwise, false.</returns>
	public override bool IsAffine => Matrix.IsAffine;

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> that specifies a 3-D transformation. </summary>
	/// <returns>A Matrix3D that specifies a 3-D transformation. </returns>
	public Matrix3D Matrix
	{
		get
		{
			return (Matrix3D)GetValue(MatrixProperty);
		}
		set
		{
			SetValueInternal(MatrixProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.MatrixTransform3D" /> class.</summary>
	public MatrixTransform3D()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.MatrixTransform3D" /> class using the specified <see cref="T:System.Windows.Media.Media3D.Matrix3D" />. </summary>
	/// <param name="matrix">A Matrix3D that specifies the transform.</param>
	public MatrixTransform3D(Matrix3D matrix)
	{
		Matrix = matrix;
	}

	internal override void Append(ref Matrix3D matrix)
	{
		matrix *= Matrix;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.MatrixTransform3D" />, and makes deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (although they might no longer resolve) but does not copy animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new MatrixTransform3D Clone()
	{
		return (MatrixTransform3D)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.MatrixTransform3D" /> object, and makes deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new MatrixTransform3D CloneCurrentValue()
	{
		return (MatrixTransform3D)base.CloneCurrentValue();
	}

	private static void MatrixPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((MatrixTransform3D)d).PropertyChanged(MatrixProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new MatrixTransform3D();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DUCE.MILCMD_MATRIXTRANSFORM3D mILCMD_MATRIXTRANSFORM3D = default(DUCE.MILCMD_MATRIXTRANSFORM3D);
			mILCMD_MATRIXTRANSFORM3D.Type = MILCMD.MilCmdMatrixTransform3D;
			mILCMD_MATRIXTRANSFORM3D.Handle = _duceResource.GetHandle(channel);
			mILCMD_MATRIXTRANSFORM3D.matrix = CompositionResourceManager.Matrix3DToD3DMATRIX(Matrix);
			channel.SendCommand((byte*)(&mILCMD_MATRIXTRANSFORM3D), sizeof(DUCE.MILCMD_MATRIXTRANSFORM3D));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_MATRIXTRANSFORM3D))
		{
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
			ReleaseOnChannelAnimations(channel);
		}
	}

	internal override DUCE.ResourceHandle GetHandleCore(DUCE.Channel channel)
	{
		return _duceResource.GetHandle(channel);
	}

	internal override int GetChannelCountCore()
	{
		return _duceResource.GetChannelCount();
	}

	internal override DUCE.Channel GetChannelCore(int index)
	{
		return _duceResource.GetChannel(index);
	}

	static MatrixTransform3D()
	{
		s_Matrix = Matrix3D.Identity;
		Type typeFromHandle = typeof(MatrixTransform3D);
		MatrixProperty = Animatable.RegisterProperty("Matrix", typeof(Matrix3D), typeFromHandle, Matrix3D.Identity, MatrixPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
