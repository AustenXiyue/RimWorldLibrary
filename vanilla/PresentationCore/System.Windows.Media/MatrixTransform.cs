using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal;

namespace System.Windows.Media;

/// <summary>Creates an arbitrary affine matrix transformation that is used to manipulate objects or coordinate systems in a 2-D plane. </summary>
public sealed class MatrixTransform : Transform
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.MatrixTransform.Matrix" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.MatrixTransform.Matrix" /> dependency property.</returns>
	public static readonly DependencyProperty MatrixProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static Matrix s_Matrix;

	/// <summary>Gets the <see cref="P:System.Windows.Media.MatrixTransform.Matrix" /> that represents this <see cref="T:System.Windows.Media.MatrixTransform" />.</summary>
	/// <returns>The matrix that represents this <see cref="T:System.Windows.Media.MatrixTransform" />.</returns>
	public override Matrix Value
	{
		get
		{
			ReadPreamble();
			return Matrix;
		}
	}

	internal override bool IsIdentity
	{
		get
		{
			if (Matrix.IsIdentity)
			{
				return base.CanFreeze;
			}
			return false;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Matrix" /> structure that defines this transformation.  </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Matrix" /> structure that defines this transformation. The default value is an identity <see cref="T:System.Windows.Media.Matrix" />. An identity matrix has a value of 1 in coefficients [1,1], [2,2], and [3,3]; and a value of 0 in the rest of the coefficients.</returns>
	public Matrix Matrix
	{
		get
		{
			return (Matrix)GetValue(MatrixProperty);
		}
		set
		{
			SetValueInternal(MatrixProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.MatrixTransform" /> class. </summary>
	public MatrixTransform()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.MatrixTransform" /> class with the specified transformation matrix values. </summary>
	/// <param name="m11">The value at position (1, 1) in the transformation matrix.</param>
	/// <param name="m12">The value at position (1, 2) in the transformation matrix.</param>
	/// <param name="m21">The value at position (2, 1) in the transformation matrix.</param>
	/// <param name="m22">The value at position (2, 2) in the transformation matrix.</param>
	/// <param name="offsetX">The X-axis translation factor, which is located at position (3,1) in the transformation matrix.</param>
	/// <param name="offsetY">The Y-axis translation factor, which is located at position (3,2) in the transformation matrix.</param>
	public MatrixTransform(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
	{
		Matrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.MatrixTransform" /> class with the specified transformation matrix. </summary>
	/// <param name="matrix">The transformation matrix of the new <see cref="T:System.Windows.Media.MatrixTransform" />.</param>
	public MatrixTransform(Matrix matrix)
	{
		Matrix = matrix;
	}

	internal override bool CanSerializeToString()
	{
		return base.CanFreeze;
	}

	internal override string ConvertToString(string format, IFormatProvider provider)
	{
		if (!CanSerializeToString())
		{
			return base.ConvertToString(format, provider);
		}
		return ((IFormattable)Matrix).ToString(format, provider);
	}

	internal override void TransformRect(ref Rect rect)
	{
		Matrix matrix = Matrix;
		MatrixUtil.TransformRect(ref rect, ref matrix);
	}

	internal override void MultiplyValueByMatrix(ref Matrix result, ref Matrix matrixToMultiplyBy)
	{
		result = Matrix;
		MatrixUtil.MultiplyMatrix(ref result, ref matrixToMultiplyBy);
	}

	/// <summary>Creates a modifiable copy of this <see cref="T:System.Windows.Media.MatrixTransform" /> by making deep copies of its values. </summary>
	/// <returns>A modifiable deep copy of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the cloned object returns false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new MatrixTransform Clone()
	{
		return (MatrixTransform)base.Clone();
	}

	/// <summary>Creates a modifiable copy of this <see cref="T:System.Windows.Media.MatrixTransform" /> object by making deep copies of its values. This method does not copy resource references, data bindings, or animations, although it does copy their current values. </summary>
	/// <returns>A modifiable deep copy of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the cloned object is false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new MatrixTransform CloneCurrentValue()
	{
		return (MatrixTransform)base.CloneCurrentValue();
	}

	private static void MatrixPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((MatrixTransform)d).PropertyChanged(MatrixProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new MatrixTransform();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(MatrixProperty, channel);
			DUCE.MILCMD_MATRIXTRANSFORM mILCMD_MATRIXTRANSFORM = default(DUCE.MILCMD_MATRIXTRANSFORM);
			mILCMD_MATRIXTRANSFORM.Type = MILCMD.MilCmdMatrixTransform;
			mILCMD_MATRIXTRANSFORM.Handle = _duceResource.GetHandle(channel);
			if (animationResourceHandle.IsNull)
			{
				mILCMD_MATRIXTRANSFORM.Matrix = CompositionResourceManager.MatrixToMilMatrix3x2D(Matrix);
			}
			mILCMD_MATRIXTRANSFORM.hMatrixAnimations = animationResourceHandle;
			channel.SendCommand((byte*)(&mILCMD_MATRIXTRANSFORM), sizeof(DUCE.MILCMD_MATRIXTRANSFORM));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_MATRIXTRANSFORM))
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

	static MatrixTransform()
	{
		Type typeFromHandle = typeof(MatrixTransform);
		MatrixProperty = Animatable.RegisterProperty("Matrix", typeof(Matrix), typeFromHandle, default(Matrix), MatrixPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
