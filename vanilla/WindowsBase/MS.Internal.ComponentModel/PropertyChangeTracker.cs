using System;
using System.Windows;

namespace MS.Internal.ComponentModel;

internal class PropertyChangeTracker : Expression
{
	internal EventHandler Changed;

	private DependencyObject _object;

	private DependencyProperty _property;

	internal bool CanClose => Changed == null;

	internal PropertyChangeTracker(DependencyObject obj, DependencyProperty property)
		: base(ExpressionMode.SupportsUnboundSources)
	{
		_object = obj;
		_property = property;
		ChangeSources(_object, _property, new DependencySource[1]
		{
			new DependencySource(obj, property)
		});
	}

	internal override void OnPropertyInvalidation(DependencyObject d, DependencyPropertyChangedEventArgs args)
	{
		DependencyProperty property = args.Property;
		if (_object == d && _property == property && Changed != null)
		{
			Changed(_object, EventArgs.Empty);
		}
	}

	internal void Close()
	{
		_object = null;
		_property = null;
		ChangeSources(null, null, null);
	}
}
