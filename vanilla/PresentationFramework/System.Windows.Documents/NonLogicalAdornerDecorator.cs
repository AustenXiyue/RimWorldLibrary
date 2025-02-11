namespace System.Windows.Documents;

internal class NonLogicalAdornerDecorator : AdornerDecorator
{
	public override UIElement Child
	{
		get
		{
			return base.IntChild;
		}
		set
		{
			if (base.IntChild != value)
			{
				RemoveVisualChild(base.IntChild);
				RemoveVisualChild(base.AdornerLayer);
				base.IntChild = value;
				if (value != null)
				{
					AddVisualChild(value);
					AddVisualChild(base.AdornerLayer);
				}
				InvalidateMeasure();
			}
		}
	}
}
