using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace MS.Internal.Controls.StickyNote;

internal abstract class StickyNoteContentControl
{
	protected FrameworkElement _innerControl;

	protected const long MaxBufferSize = 1610612733L;

	public abstract bool IsEmpty { get; }

	public abstract StickyNoteType Type { get; }

	public FrameworkElement InnerControl => _innerControl;

	protected StickyNoteContentControl(FrameworkElement innerControl)
	{
		SetInnerControl(innerControl);
	}

	private StickyNoteContentControl()
	{
	}

	public abstract void Save(XmlNode node);

	public abstract void Load(XmlNode node);

	public abstract void Clear();

	protected void SetInnerControl(FrameworkElement innerControl)
	{
		_innerControl = innerControl;
	}
}
