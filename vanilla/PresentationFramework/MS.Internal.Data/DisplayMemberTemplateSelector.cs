using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MS.Internal.Data;

internal sealed class DisplayMemberTemplateSelector : DataTemplateSelector
{
	private string _displayMemberPath;

	private string _stringFormat;

	private DataTemplate _xmlNodeContentTemplate;

	private DataTemplate _clrNodeContentTemplate;

	public DisplayMemberTemplateSelector(string displayMemberPath, string stringFormat)
	{
		_displayMemberPath = displayMemberPath;
		_stringFormat = stringFormat;
	}

	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		if (SystemXmlHelper.IsXmlNode(item))
		{
			if (_xmlNodeContentTemplate == null)
			{
				_xmlNodeContentTemplate = new DataTemplate();
				FrameworkElementFactory frameworkElementFactory = ContentPresenter.CreateTextBlockFactory();
				Binding binding = new Binding();
				binding.XPath = _displayMemberPath;
				binding.StringFormat = _stringFormat;
				frameworkElementFactory.SetBinding(TextBlock.TextProperty, binding);
				_xmlNodeContentTemplate.VisualTree = frameworkElementFactory;
				_xmlNodeContentTemplate.Seal();
			}
			return _xmlNodeContentTemplate;
		}
		if (_clrNodeContentTemplate == null)
		{
			_clrNodeContentTemplate = new DataTemplate();
			FrameworkElementFactory frameworkElementFactory2 = ContentPresenter.CreateTextBlockFactory();
			Binding binding2 = new Binding();
			binding2.Path = new PropertyPath(_displayMemberPath);
			binding2.StringFormat = _stringFormat;
			frameworkElementFactory2.SetBinding(TextBlock.TextProperty, binding2);
			_clrNodeContentTemplate.VisualTree = frameworkElementFactory2;
			_clrNodeContentTemplate.Seal();
		}
		return _clrNodeContentTemplate;
	}
}
