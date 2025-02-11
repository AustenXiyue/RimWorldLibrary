using System.Windows.Markup;
using System.Windows.Media.Animation;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Represents a <see cref="T:System.Windows.Media.GeneralTransform" /> that is a composite of the transforms in its <see cref="T:System.Windows.Media.GeneralTransformCollection" />.</summary>
[ContentProperty("Children")]
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public sealed class GeneralTransformGroup : GeneralTransform
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.GeneralTransformGroup.Children" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.GeneralTransformGroup.Children" /> dependency property.</returns>
	public static readonly DependencyProperty ChildrenProperty;

	internal static GeneralTransformCollection s_Children;

	/// <summary>Gets the inverse transform of this <see cref="T:System.Windows.Media.GeneralTransformGroup" />, if it has an inverse.</summary>
	/// <returns>The inverse transform of this <see cref="T:System.Windows.Media.GeneralTransformGroup" />, if it has an inverse; otherwise, null.</returns>
	public override GeneralTransform Inverse
	{
		get
		{
			ReadPreamble();
			if (Children == null || Children.Count == 0)
			{
				return null;
			}
			GeneralTransformGroup generalTransformGroup = new GeneralTransformGroup();
			for (int num = Children.Count - 1; num >= 0; num--)
			{
				GeneralTransform inverse = Children.Internal_GetItem(num).Inverse;
				if (inverse == null)
				{
					return null;
				}
				generalTransformGroup.Children.Add(inverse);
			}
			return generalTransformGroup;
		}
	}

	internal override Transform AffineTransform
	{
		[FriendAccessAllowed]
		get
		{
			if (Children == null || Children.Count == 0)
			{
				return null;
			}
			Matrix identity = Matrix.Identity;
			foreach (GeneralTransform child in Children)
			{
				Transform affineTransform = child.AffineTransform;
				if (affineTransform != null)
				{
					identity *= affineTransform.Value;
				}
			}
			return new MatrixTransform(identity);
		}
	}

	/// <summary>Gets or sets the collection of <see cref="T:System.Windows.Media.GeneralTransformGroup" /> objects that form this <see cref="T:System.Windows.Media.GeneralTransformGroup" />.  </summary>
	/// <returns>The collection of <see cref="T:System.Windows.Media.GeneralTransformGroup" /> objects that form this <see cref="T:System.Windows.Media.GeneralTransformGroup" />. The default value is an empty collection.</returns>
	public GeneralTransformCollection Children
	{
		get
		{
			return (GeneralTransformCollection)GetValue(ChildrenProperty);
		}
		set
		{
			SetValueInternal(ChildrenProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GeneralTransformGroup" /> class.</summary>
	public GeneralTransformGroup()
	{
	}

	/// <summary>Attempts to transform the specified point.</summary>
	/// <returns>true if <paramref name="inPoint" /> was transformed; otherwise, false.</returns>
	/// <param name="inPoint">The point to transform.</param>
	/// <param name="result">The result of transforming <paramref name="inPoint" />.</param>
	public override bool TryTransform(Point inPoint, out Point result)
	{
		result = inPoint;
		if (Children == null || Children.Count == 0)
		{
			return false;
		}
		bool result2 = true;
		for (int i = 0; i < Children.Count; i++)
		{
			if (!Children.Internal_GetItem(i).TryTransform(inPoint, out result))
			{
				result2 = false;
			}
			inPoint = result;
		}
		return result2;
	}

	/// <summary>Transforms the specified bounding box to the smallest axis-aligned bounding box possible that contains all the points in the original bounding box.</summary>
	/// <returns>The transformed bounding box, which is the smallest axis-aligned bounding box possible that contains all the points in the original bounding box.</returns>
	/// <param name="rect">The bounding box to transform.</param>
	public override Rect TransformBounds(Rect rect)
	{
		if (Children == null || Children.Count == 0)
		{
			return rect;
		}
		Rect rect2 = rect;
		for (int i = 0; i < Children.Count; i++)
		{
			rect2 = Children.Internal_GetItem(i).TransformBounds(rect2);
		}
		return rect2;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GeneralTransformGroup" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GeneralTransformGroup Clone()
	{
		return (GeneralTransformGroup)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GeneralTransformGroup" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GeneralTransformGroup CloneCurrentValue()
	{
		return (GeneralTransformGroup)base.CloneCurrentValue();
	}

	protected override Freezable CreateInstanceCore()
	{
		return new GeneralTransformGroup();
	}

	static GeneralTransformGroup()
	{
		s_Children = GeneralTransformCollection.Empty;
		Type typeFromHandle = typeof(GeneralTransformGroup);
		ChildrenProperty = Animatable.RegisterProperty("Children", typeof(GeneralTransformCollection), typeFromHandle, new FreezableDefaultValueFactory(GeneralTransformCollection.Empty), null, null, isIndependentlyAnimated: false, null);
	}
}
