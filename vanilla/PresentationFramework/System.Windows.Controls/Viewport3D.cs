using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MS.Internal;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls;

/// <summary>Renders the contained 3-D content within the 2-D layout bounds of the <see cref="T:System.Windows.Controls.Viewport3D" /> element.  </summary>
[ContentProperty("Children")]
[Localizability(LocalizationCategory.NeverLocalize)]
public class Viewport3D : FrameworkElement, IAddChild
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Viewport3D.Camera" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Viewport3D.Camera" /> dependency property.</returns>
	public static readonly DependencyProperty CameraProperty;

	private static readonly DependencyPropertyKey ChildrenPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Viewport3D.Children" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Viewport3D.Children" /> dependency property.</returns>
	public static readonly DependencyProperty ChildrenProperty;

	private readonly Viewport3DVisual _viewport3DVisual;

	/// <summary>Gets or sets a camera object that projects the 3-D contents of the <see cref="T:System.Windows.Controls.Viewport3D" /> to the 2-D surface of the <see cref="T:System.Windows.Controls.Viewport3D" />.  </summary>
	/// <returns>The camera that projects the 3-D contents to the 2-D surface.</returns>
	public Camera Camera
	{
		get
		{
			return (Camera)GetValue(CameraProperty);
		}
		set
		{
			SetValue(CameraProperty, value);
		}
	}

	/// <summary>Gets a collection of the <see cref="T:System.Windows.Media.Media3D.Visual3D" /> children of the <see cref="T:System.Windows.Controls.Viewport3D" />.  </summary>
	/// <returns>A collection of the <see cref="T:System.Windows.Media.Media3D.Visual3D" /> children of the <see cref="T:System.Windows.Controls.Viewport3D" />.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public Visual3DCollection Children => (Visual3DCollection)GetValue(ChildrenProperty);

	/// <summary>Gets an integer that represents the number of <see cref="T:System.Windows.Media.Visual" /> objects in the <see cref="P:System.Windows.Media.Media3D.ModelVisual3D.Children" /> collection of the <see cref="T:System.Windows.Media.Media3D.Visual3D" />.</summary>
	/// <returns>Integer that represents the number of Visuals in the Children collection of the <see cref="T:System.Windows.Media.Media3D.Visual3D" />.</returns>
	protected override int VisualChildrenCount => 1;

	static Viewport3D()
	{
		CameraProperty = Viewport3DVisual.CameraProperty.AddOwner(typeof(Viewport3D), new FrameworkPropertyMetadata(FreezableOperations.GetAsFrozen(new PerspectiveCamera()), OnCameraChanged));
		ChildrenPropertyKey = DependencyProperty.RegisterReadOnly("Children", typeof(Visual3DCollection), typeof(Viewport3D), new FrameworkPropertyMetadata((object)null));
		ChildrenProperty = ChildrenPropertyKey.DependencyProperty;
		UIElement.ClipToBoundsProperty.OverrideMetadata(typeof(Viewport3D), new PropertyMetadata(BooleanBoxes.TrueBox));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Viewport3D" /> class.</summary>
	public Viewport3D()
	{
		_viewport3DVisual = new Viewport3DVisual();
		_viewport3DVisual.CanBeInheritanceContext = false;
		AddVisualChild(_viewport3DVisual);
		SetValue(ChildrenPropertyKey, _viewport3DVisual.Children);
		_viewport3DVisual.SetInheritanceContextForChildren(this);
	}

	private static void OnCameraChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Viewport3D viewport3D = (Viewport3D)d;
		if (!e.IsASubPropertyChange)
		{
			viewport3D._viewport3DVisual.Camera = (Camera)e.NewValue;
		}
	}

	/// <summary>Creates and returns a <see cref="T:System.Windows.Automation.Peers.Viewport3DAutomationPeer" /> object for this <see cref="T:System.Windows.Controls.Viewport3D" />.</summary>
	/// <returns>
	///   <see cref="T:System.Windows.Automation.Peers.Viewport3DAutomationPeer" /> object for this <see cref="T:System.Windows.Controls.Viewport3D" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new Viewport3DAutomationPeer(this);
	}

	/// <summary>Causes the <see cref="T:System.Windows.Controls.Viewport3D" /> to arrange its visual content to fit a specified size. </summary>
	/// <returns>The final size of the arranged <see cref="T:System.Windows.Controls.Viewport3D" />.</returns>
	/// <param name="finalSize">Size that <see cref="T:System.Windows.Controls.Viewport3D" /> will assume.</param>
	protected override Size ArrangeOverride(Size finalSize)
	{
		Rect viewport = new Rect(default(Point), finalSize);
		_viewport3DVisual.Viewport = viewport;
		return finalSize;
	}

	/// <summary>Gets the <see cref="T:System.Windows.Media.Visual" /> at a specified position in the <see cref="P:System.Windows.Controls.Viewport3D.Children" /> collection of the <see cref="T:System.Windows.Controls.Viewport3D" />.</summary>
	/// <returns>Visual at the specified position in the <see cref="P:System.Windows.Controls.Viewport3D.Children" /> collection.</returns>
	/// <param name="index">Position of the element in the collection.</param>
	protected override Visual GetVisualChild(int index)
	{
		if (index == 0)
		{
			return _viewport3DVisual;
		}
		throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="value">   An object to add as a child.</param>
	void IAddChild.AddChild(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is Visual3D value2))
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(Visual3D)), "value");
		}
		Children.Add(value2);
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="text">   A string to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}
}
