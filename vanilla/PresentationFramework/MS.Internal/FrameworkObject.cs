using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MS.Internal;

internal struct FrameworkObject
{
	private FrameworkElement _fe;

	private FrameworkContentElement _fce;

	private DependencyObject _do;

	internal FrameworkElement FE => _fe;

	internal FrameworkContentElement FCE => _fce;

	internal DependencyObject DO => _do;

	internal bool IsFE => _fe != null;

	internal bool IsFCE => _fce != null;

	internal bool IsValid
	{
		get
		{
			if (_fe == null)
			{
				return _fce != null;
			}
			return true;
		}
	}

	internal DependencyObject Parent
	{
		get
		{
			if (IsFE)
			{
				return _fe.Parent;
			}
			if (IsFCE)
			{
				return _fce.Parent;
			}
			return null;
		}
	}

	internal int TemplateChildIndex
	{
		get
		{
			if (IsFE)
			{
				return _fe.TemplateChildIndex;
			}
			if (IsFCE)
			{
				return _fce.TemplateChildIndex;
			}
			return -1;
		}
	}

	internal DependencyObject TemplatedParent
	{
		get
		{
			if (IsFE)
			{
				return _fe.TemplatedParent;
			}
			if (IsFCE)
			{
				return _fce.TemplatedParent;
			}
			return null;
		}
	}

	internal Style ThemeStyle
	{
		get
		{
			if (IsFE)
			{
				return _fe.ThemeStyle;
			}
			if (IsFCE)
			{
				return _fce.ThemeStyle;
			}
			return null;
		}
	}

	internal XmlLanguage Language
	{
		get
		{
			if (IsFE)
			{
				return _fe.Language;
			}
			if (IsFCE)
			{
				return _fce.Language;
			}
			return null;
		}
	}

	internal FrameworkTemplate TemplateInternal
	{
		get
		{
			if (IsFE)
			{
				return _fe.TemplateInternal;
			}
			return null;
		}
	}

	internal FrameworkObject FrameworkParent
	{
		get
		{
			if (IsFE)
			{
				DependencyObject dependencyObject = _fe.ContextVerifiedGetParent();
				if (dependencyObject != null)
				{
					Invariant.Assert(dependencyObject is FrameworkElement || dependencyObject is FrameworkContentElement);
					if (_fe.IsParentAnFE)
					{
						return new FrameworkObject((FrameworkElement)dependencyObject, null);
					}
					return new FrameworkObject(null, (FrameworkContentElement)dependencyObject);
				}
				FrameworkObject containingFrameworkElement = GetContainingFrameworkElement(_fe.InternalVisualParent);
				if (containingFrameworkElement.IsValid)
				{
					return containingFrameworkElement;
				}
				containingFrameworkElement.Reset(_fe.GetUIParentCore());
				if (containingFrameworkElement.IsValid)
				{
					return containingFrameworkElement;
				}
				containingFrameworkElement.Reset(Helper.FindMentor(_fe.InheritanceContext));
				return containingFrameworkElement;
			}
			if (IsFCE)
			{
				DependencyObject parent = _fce.Parent;
				if (parent != null)
				{
					Invariant.Assert(parent is FrameworkElement || parent is FrameworkContentElement);
					if (_fce.IsParentAnFE)
					{
						return new FrameworkObject((FrameworkElement)parent, null);
					}
					return new FrameworkObject(null, (FrameworkContentElement)parent);
				}
				parent = ContentOperations.GetParent(_fce);
				FrameworkObject containingFrameworkElement2 = GetContainingFrameworkElement(parent);
				if (containingFrameworkElement2.IsValid)
				{
					return containingFrameworkElement2;
				}
				containingFrameworkElement2.Reset(Helper.FindMentor(_fce.InheritanceContext));
				return containingFrameworkElement2;
			}
			return GetContainingFrameworkElement(_do);
		}
	}

	internal Style Style
	{
		get
		{
			if (IsFE)
			{
				return _fe.Style;
			}
			if (IsFCE)
			{
				return _fce.Style;
			}
			return null;
		}
		set
		{
			if (IsFE)
			{
				_fe.Style = value;
			}
			else if (IsFCE)
			{
				_fce.Style = value;
			}
		}
	}

	internal bool IsStyleSetFromGenerator
	{
		get
		{
			if (IsFE)
			{
				return _fe.IsStyleSetFromGenerator;
			}
			if (IsFCE)
			{
				return _fce.IsStyleSetFromGenerator;
			}
			return false;
		}
		set
		{
			if (IsFE)
			{
				_fe.IsStyleSetFromGenerator = value;
			}
			else if (IsFCE)
			{
				_fce.IsStyleSetFromGenerator = value;
			}
		}
	}

	internal DependencyObject EffectiveParent
	{
		get
		{
			DependencyObject dependencyObject = (IsFE ? VisualTreeHelper.GetParent(_fe) : (IsFCE ? _fce.Parent : ((!(_do is Visual reference)) ? ((!(_do is ContentElement reference2)) ? ((!(_do is Visual3D reference3)) ? null : VisualTreeHelper.GetParent(reference3)) : ContentOperations.GetParent(reference2)) : VisualTreeHelper.GetParent(reference))));
			if (dependencyObject == null && _do != null)
			{
				dependencyObject = _do.InheritanceContext;
			}
			return dependencyObject;
		}
	}

	internal FrameworkObject PreferVisualParent => GetPreferVisualParent(force: false);

	internal bool IsLoaded
	{
		get
		{
			if (IsFE)
			{
				return _fe.IsLoaded;
			}
			if (IsFCE)
			{
				return _fce.IsLoaded;
			}
			return BroadcastEventHelper.IsParentLoaded(_do);
		}
	}

	internal bool IsInitialized
	{
		get
		{
			if (IsFE)
			{
				return _fe.IsInitialized;
			}
			if (IsFCE)
			{
				return _fce.IsInitialized;
			}
			return true;
		}
	}

	internal bool ThisHasLoadedChangeEventHandler
	{
		get
		{
			if (IsFE)
			{
				return _fe.ThisHasLoadedChangeEventHandler;
			}
			if (IsFCE)
			{
				return _fce.ThisHasLoadedChangeEventHandler;
			}
			return false;
		}
	}

	internal bool SubtreeHasLoadedChangeHandler
	{
		get
		{
			if (IsFE)
			{
				return _fe.SubtreeHasLoadedChangeHandler;
			}
			if (IsFCE)
			{
				return _fce.SubtreeHasLoadedChangeHandler;
			}
			return false;
		}
		set
		{
			if (IsFE)
			{
				_fe.SubtreeHasLoadedChangeHandler = value;
			}
			else if (IsFCE)
			{
				_fce.SubtreeHasLoadedChangeHandler = value;
			}
		}
	}

	internal InheritanceBehavior InheritanceBehavior
	{
		get
		{
			if (IsFE)
			{
				return _fe.InheritanceBehavior;
			}
			if (IsFCE)
			{
				return _fce.InheritanceBehavior;
			}
			return InheritanceBehavior.Default;
		}
	}

	internal bool StoresParentTemplateValues
	{
		get
		{
			if (IsFE)
			{
				return _fe.StoresParentTemplateValues;
			}
			if (IsFCE)
			{
				return _fce.StoresParentTemplateValues;
			}
			return false;
		}
		set
		{
			if (IsFE)
			{
				_fe.StoresParentTemplateValues = value;
			}
			else if (IsFCE)
			{
				_fce.StoresParentTemplateValues = value;
			}
		}
	}

	internal bool HasResourceReference
	{
		set
		{
			if (IsFE)
			{
				_fe.HasResourceReference = value;
			}
			else if (IsFCE)
			{
				_fce.HasResourceReference = value;
			}
		}
	}

	internal bool HasTemplateChanged
	{
		set
		{
			if (IsFE)
			{
				_fe.HasTemplateChanged = value;
			}
		}
	}

	internal bool ShouldLookupImplicitStyles
	{
		get
		{
			if (IsFE)
			{
				return _fe.ShouldLookupImplicitStyles;
			}
			if (IsFCE)
			{
				return _fce.ShouldLookupImplicitStyles;
			}
			return false;
		}
		set
		{
			if (IsFE)
			{
				_fe.ShouldLookupImplicitStyles = value;
			}
			else if (IsFCE)
			{
				_fce.ShouldLookupImplicitStyles = value;
			}
		}
	}

	internal event RoutedEventHandler Loaded
	{
		add
		{
			if (IsFE)
			{
				_fe.Loaded += value;
			}
			else if (IsFCE)
			{
				_fce.Loaded += value;
			}
			else
			{
				UnexpectedCall();
			}
		}
		remove
		{
			if (IsFE)
			{
				_fe.Loaded -= value;
			}
			else if (IsFCE)
			{
				_fce.Loaded -= value;
			}
			else
			{
				UnexpectedCall();
			}
		}
	}

	internal event RoutedEventHandler Unloaded
	{
		add
		{
			if (IsFE)
			{
				_fe.Unloaded += value;
			}
			else if (IsFCE)
			{
				_fce.Unloaded += value;
			}
			else
			{
				UnexpectedCall();
			}
		}
		remove
		{
			if (IsFE)
			{
				_fe.Unloaded -= value;
			}
			else if (IsFCE)
			{
				_fce.Unloaded -= value;
			}
			else
			{
				UnexpectedCall();
			}
		}
	}

	internal event InheritedPropertyChangedEventHandler InheritedPropertyChanged
	{
		add
		{
			if (IsFE)
			{
				_fe.InheritedPropertyChanged += value;
			}
			else if (IsFCE)
			{
				_fce.InheritedPropertyChanged += value;
			}
			else
			{
				UnexpectedCall();
			}
		}
		remove
		{
			if (IsFE)
			{
				_fe.InheritedPropertyChanged -= value;
			}
			else if (IsFCE)
			{
				_fce.InheritedPropertyChanged -= value;
			}
			else
			{
				UnexpectedCall();
			}
		}
	}

	internal event EventHandler ResourcesChanged
	{
		add
		{
			if (IsFE)
			{
				_fe.ResourcesChanged += value;
			}
			else if (IsFCE)
			{
				_fce.ResourcesChanged += value;
			}
			else
			{
				UnexpectedCall();
			}
		}
		remove
		{
			if (IsFE)
			{
				_fe.ResourcesChanged -= value;
			}
			else if (IsFCE)
			{
				_fce.ResourcesChanged -= value;
			}
			else
			{
				UnexpectedCall();
			}
		}
	}

	internal FrameworkObject(DependencyObject d)
	{
		_do = d;
		_fe = d as FrameworkElement;
		_fce = d as FrameworkContentElement;
	}

	internal FrameworkObject(DependencyObject d, bool throwIfNeither)
		: this(d)
	{
		if (throwIfNeither && _fe == null && _fce == null)
		{
			object p = ((d != null) ? ((object)d.GetType()) : ((object)"NULL"));
			throw new InvalidOperationException(SR.Format(SR.MustBeFrameworkDerived, p));
		}
	}

	internal FrameworkObject(FrameworkElement fe, FrameworkContentElement fce)
	{
		_fe = fe;
		_fce = fce;
		if (fe != null)
		{
			_do = fe;
		}
		else
		{
			_do = fce;
		}
	}

	internal void Reset(DependencyObject d)
	{
		_do = d;
		_fe = d as FrameworkElement;
		_fce = d as FrameworkContentElement;
	}

	internal static FrameworkObject GetContainingFrameworkElement(DependencyObject current)
	{
		FrameworkObject result = new FrameworkObject(current);
		while (!result.IsValid && result.DO != null)
		{
			if (result.DO is Visual reference)
			{
				result.Reset(VisualTreeHelper.GetParent(reference));
			}
			else if (result.DO is ContentElement reference2)
			{
				result.Reset(ContentOperations.GetParent(reference2));
			}
			else if (result.DO is Visual3D reference3)
			{
				result.Reset(VisualTreeHelper.GetParent(reference3));
			}
			else
			{
				result.Reset(null);
			}
		}
		return result;
	}

	internal static bool IsEffectiveAncestor(DependencyObject d1, DependencyObject d2)
	{
		FrameworkObject frameworkObject = new FrameworkObject(d2);
		while (frameworkObject.DO != null)
		{
			if (frameworkObject.DO == d1)
			{
				return true;
			}
			frameworkObject.Reset(frameworkObject.EffectiveParent);
		}
		return false;
	}

	internal void ChangeLogicalParent(DependencyObject newParent)
	{
		if (IsFE)
		{
			_fe.ChangeLogicalParent(newParent);
		}
		else if (IsFCE)
		{
			_fce.ChangeLogicalParent(newParent);
		}
	}

	internal void BeginInit()
	{
		if (IsFE)
		{
			_fe.BeginInit();
		}
		else if (IsFCE)
		{
			_fce.BeginInit();
		}
		else
		{
			UnexpectedCall();
		}
	}

	internal void EndInit()
	{
		if (IsFE)
		{
			_fe.EndInit();
		}
		else if (IsFCE)
		{
			_fce.EndInit();
		}
		else
		{
			UnexpectedCall();
		}
	}

	internal object FindName(string name, out DependencyObject scopeOwner)
	{
		if (IsFE)
		{
			return _fe.FindName(name, out scopeOwner);
		}
		if (IsFCE)
		{
			return _fce.FindName(name, out scopeOwner);
		}
		scopeOwner = null;
		return null;
	}

	internal FrameworkObject GetPreferVisualParent(bool force)
	{
		if (!force && InheritanceBehavior != 0)
		{
			return new FrameworkObject(null);
		}
		FrameworkObject rawPreferVisualParent = GetRawPreferVisualParent();
		switch (rawPreferVisualParent.InheritanceBehavior)
		{
		case InheritanceBehavior.SkipToAppNow:
		case InheritanceBehavior.SkipToThemeNow:
		case InheritanceBehavior.SkipAllNow:
			rawPreferVisualParent.Reset(null);
			break;
		}
		return rawPreferVisualParent;
	}

	private FrameworkObject GetRawPreferVisualParent()
	{
		if (_do == null)
		{
			return new FrameworkObject(null);
		}
		DependencyObject dependencyObject = (IsFE ? VisualTreeHelper.GetParent(_fe) : (IsFCE ? null : ((_do == null) ? null : ((_do is Visual reference) ? VisualTreeHelper.GetParent(reference) : null))));
		if (dependencyObject != null)
		{
			return new FrameworkObject(dependencyObject);
		}
		DependencyObject dependencyObject2 = (IsFE ? _fe.Parent : (IsFCE ? _fce.Parent : ((_do == null) ? null : ((_do is ContentElement reference2) ? ContentOperations.GetParent(reference2) : null))));
		if (dependencyObject2 != null)
		{
			return new FrameworkObject(dependencyObject2);
		}
		DependencyObject dependencyObject3 = ((!(_do is UIElement uIElement)) ? ((!(_do is ContentElement contentElement)) ? null : contentElement.GetUIParentCore()) : uIElement.GetUIParentCore());
		if (dependencyObject3 != null)
		{
			return new FrameworkObject(dependencyObject3);
		}
		return new FrameworkObject(_do.InheritanceContext);
	}

	internal void RaiseEvent(RoutedEventArgs args)
	{
		if (IsFE)
		{
			_fe.RaiseEvent(args);
		}
		else if (IsFCE)
		{
			_fce.RaiseEvent(args);
		}
	}

	internal void OnLoaded(RoutedEventArgs args)
	{
		if (IsFE)
		{
			_fe.OnLoaded(args);
		}
		else if (IsFCE)
		{
			_fce.OnLoaded(args);
		}
	}

	internal void OnUnloaded(RoutedEventArgs args)
	{
		if (IsFE)
		{
			_fe.OnUnloaded(args);
		}
		else if (IsFCE)
		{
			_fce.OnUnloaded(args);
		}
	}

	internal void ChangeSubtreeHasLoadedChangedHandler(DependencyObject mentor)
	{
		if (IsFE)
		{
			_fe.ChangeSubtreeHasLoadedChangedHandler(mentor);
		}
		else if (IsFCE)
		{
			_fce.ChangeSubtreeHasLoadedChangedHandler(mentor);
		}
	}

	internal void OnInheritedPropertyChanged(ref InheritablePropertyChangeInfo info)
	{
		if (IsFE)
		{
			_fe.RaiseInheritedPropertyChangedEvent(ref info);
		}
		else if (IsFCE)
		{
			_fce.RaiseInheritedPropertyChangedEvent(ref info);
		}
	}

	internal void SetShouldLookupImplicitStyles()
	{
		if (!ShouldLookupImplicitStyles)
		{
			FrameworkObject frameworkParent = FrameworkParent;
			if (frameworkParent.IsValid && frameworkParent.ShouldLookupImplicitStyles)
			{
				ShouldLookupImplicitStyles = true;
			}
		}
	}

	private void UnexpectedCall()
	{
		Invariant.Assert(condition: false, "Call to FrameworkObject expects either FE or FCE");
	}

	public override string ToString()
	{
		if (IsFE)
		{
			return _fe.ToString();
		}
		if (IsFCE)
		{
			return _fce.ToString();
		}
		return "Null";
	}
}
