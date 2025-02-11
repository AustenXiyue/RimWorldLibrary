namespace System.Windows.Markup;

internal class TemplateComponentConnector : IComponentConnector
{
	private IStyleConnector _styleConnector;

	private IComponentConnector _componentConnector;

	internal TemplateComponentConnector(IComponentConnector componentConnector, IStyleConnector styleConnector)
	{
		_styleConnector = styleConnector;
		_componentConnector = componentConnector;
	}

	public void InitializeComponent()
	{
		_componentConnector.InitializeComponent();
	}

	public void Connect(int connectionId, object target)
	{
		if (_styleConnector != null)
		{
			_styleConnector.Connect(connectionId, target);
		}
		else
		{
			_componentConnector.Connect(connectionId, target);
		}
	}
}
