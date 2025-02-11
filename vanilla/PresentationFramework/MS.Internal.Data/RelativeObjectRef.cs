using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace MS.Internal.Data;

internal sealed class RelativeObjectRef : ObjectRef
{
	private RelativeSource _relativeSource;

	internal bool ReturnsDataContext => _relativeSource.Mode == RelativeSourceMode.PreviousData;

	protected override bool ProtectedUsesMentor
	{
		get
		{
			RelativeSourceMode mode = _relativeSource.Mode;
			if ((uint)mode <= 1u)
			{
				return true;
			}
			return false;
		}
	}

	internal RelativeObjectRef(RelativeSource relativeSource)
	{
		if (relativeSource == null)
		{
			throw new ArgumentNullException("relativeSource");
		}
		_relativeSource = relativeSource;
	}

	public override string ToString()
	{
		if (_relativeSource.Mode == RelativeSourceMode.FindAncestor)
		{
			return string.Format(CultureInfo.InvariantCulture, "RelativeSource {0}, AncestorType='{1}', AncestorLevel='{2}'", _relativeSource.Mode, _relativeSource.AncestorType, _relativeSource.AncestorLevel);
		}
		return string.Format(CultureInfo.InvariantCulture, "RelativeSource {0}", _relativeSource.Mode);
	}

	internal override object GetObject(DependencyObject d, ObjectRefArgs args)
	{
		return GetDataObjectImpl(d, args);
	}

	internal override object GetDataObject(DependencyObject d, ObjectRefArgs args)
	{
		object obj = GetDataObjectImpl(d, args);
		if (obj is DependencyObject dependencyObject && ReturnsDataContext)
		{
			obj = dependencyObject.GetValue(ItemContainerGenerator.ItemForItemContainerProperty);
			if (obj == null)
			{
				obj = dependencyObject.GetValue(FrameworkElement.DataContextProperty);
			}
		}
		return obj;
	}

	private object GetDataObjectImpl(DependencyObject d, ObjectRefArgs args)
	{
		if (d == null)
		{
			return null;
		}
		switch (_relativeSource.Mode)
		{
		case RelativeSourceMode.TemplatedParent:
			d = Helper.GetTemplatedParent(d);
			break;
		case RelativeSourceMode.PreviousData:
			return GetPreviousData(d);
		case RelativeSourceMode.FindAncestor:
			d = FindAncestorOfType(_relativeSource.AncestorType, _relativeSource.AncestorLevel, d, args.IsTracing);
			if (d == null)
			{
				return DependencyProperty.UnsetValue;
			}
			break;
		default:
			return null;
		case RelativeSourceMode.Self:
			break;
		}
		if (args.IsTracing)
		{
			TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.RelativeSource(_relativeSource.Mode, TraceData.Identify(d)));
		}
		return d;
	}

	protected override bool ProtectedTreeContextIsRequired(DependencyObject target)
	{
		if (_relativeSource.Mode != RelativeSourceMode.FindAncestor)
		{
			return _relativeSource.Mode == RelativeSourceMode.PreviousData;
		}
		return true;
	}

	internal override string Identify()
	{
		return string.Format(System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS, "RelativeSource ({0})", _relativeSource.Mode);
	}

	private object GetPreviousData(DependencyObject d)
	{
		while (d != null)
		{
			if (BindingExpression.HasLocalDataContext(d))
			{
				FrameworkElement frameworkElement;
				FrameworkElement frameworkElement2;
				if (d is ContentPresenter contentPresenter)
				{
					frameworkElement = contentPresenter;
					frameworkElement2 = contentPresenter.TemplatedParent as FrameworkElement;
					if (!(frameworkElement2 is ContentControl) && !(frameworkElement2 is HeaderedItemsControl))
					{
						frameworkElement2 = contentPresenter.Parent as GridViewRowPresenterBase;
					}
				}
				else
				{
					frameworkElement = d as FrameworkElement;
					frameworkElement2 = frameworkElement?.Parent as GridViewRowPresenterBase;
				}
				if (frameworkElement == null || frameworkElement2 == null || !ItemsControl.EqualsEx(frameworkElement.DataContext, frameworkElement2.DataContext))
				{
					break;
				}
				d = frameworkElement2;
				if (BindingExpression.HasLocalDataContext(frameworkElement2))
				{
					break;
				}
			}
			d = FrameworkElement.GetFrameworkParent(d);
		}
		if (d == null)
		{
			return DependencyProperty.UnsetValue;
		}
		Visual visual = d as Visual;
		DependencyObject dependencyObject = ((visual != null) ? VisualTreeHelper.GetParent(visual) : null);
		if (ItemsControl.GetItemsOwner(dependencyObject) == null)
		{
			if (TraceData.IsEnabled)
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.RefPreviousNotInContext);
			}
			return null;
		}
		Visual visual2 = dependencyObject as Visual;
		int num = visual2?.InternalVisualChildrenCount ?? 0;
		int num2 = -1;
		Visual prevChild = null;
		if (num != 0)
		{
			num2 = IndexOf(visual2, visual, out prevChild);
		}
		if (num2 > 0)
		{
			d = prevChild;
		}
		else
		{
			d = null;
			if (num2 < 0 && TraceData.IsEnabled)
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.RefNoWrapperInChildren);
			}
		}
		return d;
	}

	private DependencyObject FindAncestorOfType(Type type, int level, DependencyObject d, bool isTracing)
	{
		if (type == null)
		{
			if (TraceData.IsEnabled)
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.RefAncestorTypeNotSpecified);
			}
			return null;
		}
		if (level < 1)
		{
			if (TraceData.IsEnabled)
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.RefAncestorLevelInvalid);
			}
			return null;
		}
		FrameworkObject frameworkObject = new FrameworkObject(d);
		frameworkObject.Reset(frameworkObject.GetPreferVisualParent(force: true).DO);
		while (frameworkObject.DO != null)
		{
			if (isTracing)
			{
				TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.AncestorLookup(type.Name, TraceData.Identify(frameworkObject.DO)));
			}
			if (type.IsInstanceOfType(frameworkObject.DO) && --level <= 0)
			{
				break;
			}
			frameworkObject.Reset(frameworkObject.PreferVisualParent.DO);
		}
		return frameworkObject.DO;
	}

	private int IndexOf(Visual parent, Visual child, out Visual prevChild)
	{
		bool flag = false;
		prevChild = null;
		int internalVisualChildrenCount = parent.InternalVisualChildrenCount;
		int i;
		for (i = 0; i < internalVisualChildrenCount; i++)
		{
			Visual visual = parent.InternalGetVisualChild(i);
			if (child == visual)
			{
				flag = true;
				break;
			}
			prevChild = visual;
		}
		if (flag)
		{
			return i;
		}
		return -1;
	}
}
