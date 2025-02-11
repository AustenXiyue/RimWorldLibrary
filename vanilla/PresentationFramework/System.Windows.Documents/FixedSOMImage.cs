using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace System.Windows.Documents;

internal sealed class FixedSOMImage : FixedSOMElement
{
	private Uri _source;

	private string _name;

	private string _helpText;

	internal Uri Source => _source;

	internal string Name => _name;

	internal string HelpText => _helpText;

	private FixedSOMImage(Rect imageRect, GeneralTransform trans, Uri sourceUri, FixedNode node, DependencyObject o)
		: base(node, trans)
	{
		_boundingRect = trans.TransformBounds(imageRect);
		_source = sourceUri;
		_startIndex = 0;
		_endIndex = 1;
		_name = AutomationProperties.GetName(o);
		_helpText = AutomationProperties.GetHelpText(o);
	}

	public static FixedSOMImage Create(FixedPage page, Image image, FixedNode fixedNode)
	{
		Uri sourceUri = null;
		if (image.Source is BitmapImage)
		{
			sourceUri = (image.Source as BitmapImage).UriSource;
		}
		else if (image.Source is BitmapFrame)
		{
			sourceUri = new Uri((image.Source as BitmapFrame).ToString(), UriKind.RelativeOrAbsolute);
		}
		Rect imageRect = new Rect(image.RenderSize);
		GeneralTransform trans = image.TransformToAncestor(page);
		return new FixedSOMImage(imageRect, trans, sourceUri, fixedNode, image);
	}

	public static FixedSOMImage Create(FixedPage page, Path path, FixedNode fixedNode)
	{
		ImageSource imageSource = ((ImageBrush)path.Fill).ImageSource;
		Uri sourceUri = null;
		if (imageSource is BitmapImage)
		{
			sourceUri = (imageSource as BitmapImage).UriSource;
		}
		else if (imageSource is BitmapFrame)
		{
			sourceUri = new Uri((imageSource as BitmapFrame).ToString(), UriKind.RelativeOrAbsolute);
		}
		Rect bounds = path.Data.Bounds;
		GeneralTransform trans = path.TransformToAncestor(page);
		return new FixedSOMImage(bounds, trans, sourceUri, fixedNode, path);
	}
}
