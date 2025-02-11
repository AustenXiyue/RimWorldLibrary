using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;

namespace MS.Internal.Data;

internal sealed class ElementObjectRef : ObjectRef
{
	private string _name;

	internal ElementObjectRef(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		_name = name.Trim();
	}

	internal override object GetObject(DependencyObject d, ObjectRefArgs args)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		object obj = null;
		if (args.ResolveNamesInTemplate)
		{
			if (d is FrameworkElement { TemplateInternal: not null })
			{
				obj = Helper.FindNameInTemplate(_name, d);
				if (args.IsTracing)
				{
					TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.ElementNameQueryTemplate(_name, TraceData.Identify(d)));
				}
			}
			if (obj == null)
			{
				args.NameResolvedInOuterScope = true;
			}
		}
		FrameworkObject frameworkObject = new FrameworkObject(d);
		while (obj == null && frameworkObject.DO != null)
		{
			obj = frameworkObject.FindName(_name, out var scopeOwner);
			if (d == scopeOwner && d is IComponentConnector && d.ReadLocalValue(NavigationService.NavigationServiceProperty) == DependencyProperty.UnsetValue)
			{
				DependencyObject dependencyObject = LogicalTreeHelper.GetParent(d);
				if (dependencyObject == null)
				{
					dependencyObject = Helper.FindMentor(d.InheritanceContext);
				}
				if (dependencyObject != null)
				{
					obj = null;
					frameworkObject.Reset(dependencyObject);
					continue;
				}
			}
			if (args.IsTracing)
			{
				TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.ElementNameQuery(_name, TraceData.Identify(frameworkObject.DO)));
			}
			if (obj != null)
			{
				continue;
			}
			args.NameResolvedInOuterScope = true;
			DependencyObject dependencyObject2 = new FrameworkObject(scopeOwner).TemplatedParent;
			if (dependencyObject2 == null && frameworkObject.FrameworkParent.DO is Panel { IsItemsHost: not false } panel)
			{
				dependencyObject2 = panel;
			}
			if (dependencyObject2 == null && scopeOwner == null && LogicalTreeHelper.GetParent(frameworkObject.DO) is ContentControl contentControl && contentControl.Content == frameworkObject.DO && contentControl.InheritanceBehavior == InheritanceBehavior.Default)
			{
				dependencyObject2 = contentControl;
			}
			if (dependencyObject2 == null && scopeOwner == null)
			{
				dependencyObject2 = frameworkObject.DO;
				while (true)
				{
					DependencyObject dependencyObject3 = LogicalTreeHelper.GetParent(dependencyObject2);
					if (dependencyObject3 == null)
					{
						dependencyObject3 = Helper.FindMentor(dependencyObject2.InheritanceContext);
					}
					if (dependencyObject3 == null)
					{
						break;
					}
					dependencyObject2 = dependencyObject3;
				}
				ContentPresenter contentPresenter = (VisualTreeHelper.IsVisualType(dependencyObject2) ? (VisualTreeHelper.GetParent(dependencyObject2) as ContentPresenter) : null);
				dependencyObject2 = ((contentPresenter != null && contentPresenter.TemplateInternal.CanBuildVisualTree) ? contentPresenter : null);
			}
			frameworkObject.Reset(dependencyObject2);
		}
		if (obj == null)
		{
			obj = DependencyProperty.UnsetValue;
			args.NameResolvedInOuterScope = false;
		}
		return obj;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "ElementName={0}", _name);
	}

	internal override string Identify()
	{
		return "ElementName";
	}
}
