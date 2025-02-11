using System.Windows.Markup;
using System.Windows.Media.Animation;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Media3D;

/// <summary>Represents a <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> that is a composite of the transforms in its <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DCollection" />.</summary>
[ContentProperty("Children")]
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public sealed class GeneralTransform3DGroup : GeneralTransform3D
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.GeneralTransform3DGroup.Children" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.GeneralTransform3DGroup.Children" /> dependency property.</returns>
	public static readonly DependencyProperty ChildrenProperty;

	internal static GeneralTransform3DCollection s_Children;

	/// <summary>Gets the inverse transform of this <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DGroup" />, if it has an inverse.</summary>
	/// <returns>The inverse transform of this <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DGroup" />, if it has an inverse; otherwise, null.</returns>
	public override GeneralTransform3D Inverse
	{
		get
		{
			GeneralTransform3DCollection children = Children;
			if (children == null || children.Count == 0)
			{
				return null;
			}
			GeneralTransform3DGroup generalTransform3DGroup = new GeneralTransform3DGroup();
			for (int num = children.Count - 1; num >= 0; num--)
			{
				GeneralTransform3D inverse = children._collection[num].Inverse;
				if (inverse == null)
				{
					return null;
				}
				generalTransform3DGroup.Children.Add(inverse);
			}
			return generalTransform3DGroup;
		}
	}

	internal override Transform3D AffineTransform
	{
		[FriendAccessAllowed]
		get
		{
			GeneralTransform3DCollection children = Children;
			if (children == null || children.Count == 0)
			{
				return null;
			}
			Matrix3D matrix = Matrix3D.Identity;
			int i = 0;
			for (int count = children.Count; i < count; i++)
			{
				children._collection[i].AffineTransform.Append(ref matrix);
			}
			return new MatrixTransform3D(matrix);
		}
	}

	/// <summary>Gets or sets the collection of <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DGroup" /> objects that form this <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DGroup" />.  </summary>
	/// <returns>The collection of <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DGroup" /> objects that form this <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DGroup" />. The default is an empty collection.</returns>
	public GeneralTransform3DCollection Children
	{
		get
		{
			return (GeneralTransform3DCollection)GetValue(ChildrenProperty);
		}
		set
		{
			SetValueInternal(ChildrenProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DGroup" /> class.</summary>
	public GeneralTransform3DGroup()
	{
	}

	/// <summary>Attempts to transform the specified 3-D point.</summary>
	/// <returns>true if <paramref name="inPoint" /> was transformed; otherwise, false.</returns>
	/// <param name="inPoint">The 3-D point to transform.</param>
	/// <param name="result">The result of transforming <paramref name="inPoint" />.</param>
	public override bool TryTransform(Point3D inPoint, out Point3D result)
	{
		result = inPoint;
		GeneralTransform3DCollection children = Children;
		if (children == null || children.Count == 0)
		{
			return false;
		}
		bool result2 = true;
		int i = 0;
		for (int count = children.Count; i < count; i++)
		{
			if (!children._collection[i].TryTransform(inPoint, out result))
			{
				result2 = false;
				break;
			}
			inPoint = result;
		}
		return result2;
	}

	/// <summary>Transforms the specified 3-D bounding box to the smallest axis-aligned 3-D bounding box possible that contains all the points in the original bounding box.</summary>
	/// <returns>The transformed bounding box. </returns>
	/// <param name="rect">The 3-D bounding box to transform.</param>
	public override Rect3D TransformBounds(Rect3D rect)
	{
		GeneralTransform3DCollection children = Children;
		if (children == null || children.Count == 0)
		{
			return rect;
		}
		Rect3D rect3D = rect;
		int i = 0;
		for (int count = children.Count; i < count; i++)
		{
			rect3D = children._collection[i].TransformBounds(rect3D);
		}
		return rect3D;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DGroup" />, making deep copies of this object's values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new GeneralTransform3DGroup Clone()
	{
		return (GeneralTransform3DGroup)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DGroup" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new GeneralTransform3DGroup CloneCurrentValue()
	{
		return (GeneralTransform3DGroup)base.CloneCurrentValue();
	}

	protected override Freezable CreateInstanceCore()
	{
		return new GeneralTransform3DGroup();
	}

	static GeneralTransform3DGroup()
	{
		s_Children = GeneralTransform3DCollection.Empty;
		Type typeFromHandle = typeof(GeneralTransform3DGroup);
		ChildrenProperty = Animatable.RegisterProperty("Children", typeof(GeneralTransform3DCollection), typeFromHandle, new FreezableDefaultValueFactory(GeneralTransform3DCollection.Empty), null, null, isIndependentlyAnimated: false, null);
	}
}
