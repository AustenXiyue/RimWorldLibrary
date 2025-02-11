using System.ComponentModel;
using System.Windows.Markup;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Media3D;

/// <summary>Provides a <see cref="T:System.Windows.Media.Media3D.Visual3D" /> that renders <see cref="T:System.Windows.Media.Media3D.Model3D" /> objects. </summary>
[ContentProperty("Children")]
public class ModelVisual3D : Visual3D, IAddChild
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.ModelVisual3D.Content" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.ModelVisual3D.Content" /> dependency property.</returns>
	public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(Model3D), typeof(ModelVisual3D), new PropertyMetadata(ContentPropertyChanged), (object _003Cp0_003E) => MediaContext.CurrentMediaContext.WriteAccessEnabled);

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.ModelVisual3D.Transform" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.ModelVisual3D.Transform" /> dependency property.</returns>
	public new static readonly DependencyProperty TransformProperty = Visual3D.TransformProperty;

	private readonly Visual3DCollection _children;

	/// <summary>Returns the number of child objects.</summary>
	/// <returns>The number of child objects.</returns>
	protected sealed override int Visual3DChildrenCount => _children.Count;

	/// <summary>Gets a collection of child <see cref="T:System.Windows.Media.Media3D.Visual3D" /> objects.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" /> that contains child <see cref="T:System.Windows.Media.Media3D.Visual3D" /> objects.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public Visual3DCollection Children
	{
		get
		{
			VerifyAPIReadOnly();
			return _children;
		}
	}

	/// <summary>Gets or sets the model that comprises the content of the <see cref="T:System.Windows.Media.Media3D.ModelVisual3D" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Media3D.Model3D" /> that comprises the content of the <see cref="T:System.Windows.Media.Media3D.ModelVisual3D" />.</returns>
	public Model3D Content
	{
		get
		{
			return (Model3D)GetValue(ContentProperty);
		}
		set
		{
			SetValue(ContentProperty, value);
		}
	}

	/// <summary>Gets or sets the transform set on the <see cref="T:System.Windows.Media.Media3D.ModelVisual3D" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Media3D.Transform3D" /> set on the <see cref="T:System.Windows.Media.Media3D.ModelVisual3D" />.</returns>
	public new Transform3D Transform
	{
		get
		{
			return (Transform3D)GetValue(TransformProperty);
		}
		set
		{
			SetValue(TransformProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.ModelVisual3D" /> class.</summary>
	public ModelVisual3D()
	{
		_children = new Visual3DCollection(this);
	}

	/// <summary>Returns the specified <see cref="T:System.Windows.Media.Media3D.Visual3D" /> in the parent collection.</summary>
	/// <returns>The child in the collection at the specified index.</returns>
	/// <param name="index">The index of the 3-D visual object in the collection.</param>
	protected sealed override Visual3D GetVisual3DChild(int index)
	{
		return _children[index];
	}

	/// <summary>Adds a child object.</summary>
	/// <param name="value">The child object to add.</param>
	void IAddChild.AddChild(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is Visual3D value2))
		{
			throw new ArgumentException(SR.Format(SR.Collection_BadType, GetType().Name, value.GetType().Name, typeof(Visual3D).Name));
		}
		Children.Add(value2);
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		for (int i = 0; i < text.Length; i++)
		{
			if (!char.IsWhiteSpace(text[i]))
			{
				throw new InvalidOperationException(SR.Format(SR.AddText_Invalid, GetType().Name));
			}
		}
	}

	private static void ContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ModelVisual3D modelVisual3D = (ModelVisual3D)d;
		if (!e.IsASubPropertyChange)
		{
			modelVisual3D.Visual3DModel = (Model3D)e.NewValue;
		}
	}
}
