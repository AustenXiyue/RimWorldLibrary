using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media.Composition;
using System.Windows.Media.Converters;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Defines functionality that enables transformations in a 2-D plane. Transformations include rotation (<see cref="T:System.Windows.Media.RotateTransform" />), scale (<see cref="T:System.Windows.Media.ScaleTransform" />), skew (<see cref="T:System.Windows.Media.SkewTransform" />), and translation (<see cref="T:System.Windows.Media.TranslateTransform" />). This class hierarchy differs from the <see cref="T:System.Windows.Media.Matrix" /> structure because it is a class and it supports animation and enumeration semantics. </summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
[TypeConverter(typeof(TransformConverter))]
[ValueSerializer(typeof(TransformValueSerializer))]
public abstract class Transform : GeneralTransform, DUCE.IResource
{
	private static Transform s_identity = MakeIdentityTransform();

	/// <summary>Gets an identity transform. </summary>
	/// <returns>An identity transform.</returns>
	public static Transform Identity => s_identity;

	/// <summary>Gets the current transformation as a <see cref="T:System.Windows.Media.Matrix" /> object. </summary>
	/// <returns>The current matrix transformation.</returns>
	public abstract Matrix Value { get; }

	internal abstract bool IsIdentity { get; }

	/// <summary>Gets the inverse of this transform, if it exists.</summary>
	/// <returns>The inverse of this transform, if it exists; otherwise, null.</returns>
	public override GeneralTransform Inverse
	{
		get
		{
			ReadPreamble();
			Matrix value = Value;
			if (!value.HasInverse)
			{
				return null;
			}
			value.Invert();
			return new MatrixTransform(value);
		}
	}

	internal override Transform AffineTransform
	{
		[FriendAccessAllowed]
		get
		{
			return this;
		}
	}

	internal Transform()
	{
	}

	private static Transform MakeIdentityTransform()
	{
		MatrixTransform matrixTransform = new MatrixTransform(Matrix.Identity);
		matrixTransform.Freeze();
		return matrixTransform;
	}

	internal virtual bool CanSerializeToString()
	{
		return false;
	}

	internal virtual void TransformRect(ref Rect rect)
	{
		Matrix matrix = Value;
		MatrixUtil.TransformRect(ref rect, ref matrix);
	}

	internal virtual void MultiplyValueByMatrix(ref Matrix result, ref Matrix matrixToMultiplyBy)
	{
		result = Value;
		MatrixUtil.MultiplyMatrix(ref result, ref matrixToMultiplyBy);
	}

	internal unsafe virtual void ConvertToD3DMATRIX(D3DMATRIX* milMatrix)
	{
		Matrix value = Value;
		MILUtilities.ConvertToD3DMATRIX(&value, milMatrix);
	}

	internal static void GetTransformValue(Transform transform, out Matrix currentTransformValue)
	{
		if (transform != null)
		{
			currentTransformValue = transform.Value;
		}
		else
		{
			currentTransformValue = Matrix.Identity;
		}
	}

	/// <summary>Attempts to transform the specified point and returns a value that indicates whether the transformation was successful. </summary>
	/// <returns>true if <paramref name="inPoint" /> was transformed; otherwise, false.</returns>
	/// <param name="inPoint">The point to transform.</param>
	/// <param name="result">The result of transforming <paramref name="inPoint" />.</param>
	public override bool TryTransform(Point inPoint, out Point result)
	{
		result = Value.Transform(inPoint);
		return true;
	}

	/// <summary>Transforms the specified bounding box and returns an axis-aligned bounding box that is exactly large enough to contain it.</summary>
	/// <returns>The smallest axis-aligned bounding box that can contain the transformed <paramref name="rect" />.</returns>
	/// <param name="rect">The bounding box to transform.</param>
	public override Rect TransformBounds(Rect rect)
	{
		TransformRect(ref rect);
		return rect;
	}

	/// <summary>Creates a modifiable copy of this <see cref="T:System.Windows.Media.Transform" /> by making deep copies of its values. </summary>
	/// <returns>A modifiable deep copy of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the cloned object returns false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new Transform Clone()
	{
		return (Transform)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Transform" /> object by making deep copies of its values. This method does not copy resource references, data bindings, or animations, although it does copy their current values. </summary>
	/// <returns>A modifiable deep copy of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the cloned object is false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new Transform CloneCurrentValue()
	{
		return (Transform)base.CloneCurrentValue();
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

	/// <summary>Creates a new <see cref="T:System.Windows.Media.Transform" /> from the specified string representation of a transformation matrix.</summary>
	/// <returns>A new transform that is constructed from the specified string.</returns>
	/// <param name="source">Six comma-delimited <see cref="T:System.Double" /> values that describe the new <see cref="T:System.Windows.Media.Transform" />. See also Remarks.</param>
	public static Transform Parse(string source)
	{
		IFormatProvider invariantEnglishUS = System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS;
		return Parsers.ParseTransform(source, invariantEnglishUS);
	}
}
