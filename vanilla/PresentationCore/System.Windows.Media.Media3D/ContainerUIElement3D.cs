using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Markup;

namespace System.Windows.Media.Media3D;

/// <summary>Represents a container for <see cref="T:System.Windows.Media.Media3D.Visual3D" /> objects.</summary>
[ContentProperty("Children")]
public sealed class ContainerUIElement3D : UIElement3D
{
	private readonly Visual3DCollection _children;

	protected override int Visual3DChildrenCount => _children.Count;

	/// <summary>Gets a <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" /> of child elements of this <see cref="T:System.Windows.Media.Media3D.ContainerUIElement3D" /> object.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" /> of child elements. The default is an empty collection.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public Visual3DCollection Children
	{
		get
		{
			VerifyAPIReadOnly();
			return _children;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.ContainerUIElement3D" /> class.</summary>
	public ContainerUIElement3D()
	{
		_children = new Visual3DCollection(this);
	}

	protected override Visual3D GetVisual3DChild(int index)
	{
		return _children[index];
	}

	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new UIElement3DAutomationPeer(this);
	}
}
