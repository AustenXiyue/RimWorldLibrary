using System.Windows.Automation.Peers;
using System.Windows.Markup;

namespace System.Windows.Media.Media3D;

/// <summary>Renders a 3-D model that supports input, focus, and events.</summary>
[ContentProperty("Model")]
public sealed class ModelUIElement3D : UIElement3D
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.ModelUIElement3D.Model" /> dependency property.</summary>
	public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof(Model3D), typeof(ModelUIElement3D), new PropertyMetadata(ModelPropertyChanged), (object _003Cp0_003E) => MediaContext.CurrentMediaContext.WriteAccessEnabled);

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Media3D.Model3D" /> to render.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Media3D.Model3D" /> to render.</returns>
	public Model3D Model
	{
		get
		{
			return (Model3D)GetValue(ModelProperty);
		}
		set
		{
			SetValue(ModelProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.ContainerUIElement3D" /> class.</summary>
	public ModelUIElement3D()
	{
	}

	private static void ModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ModelUIElement3D modelUIElement3D = (ModelUIElement3D)d;
		if (!e.IsASubPropertyChange)
		{
			modelUIElement3D.Visual3DModel = (Model3D)e.NewValue;
		}
	}

	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new UIElement3DAutomationPeer(this);
	}
}
