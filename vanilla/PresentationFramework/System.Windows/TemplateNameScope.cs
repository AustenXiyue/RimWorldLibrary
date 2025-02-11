using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Markup;
using MS.Internal;

namespace System.Windows;

internal class TemplateNameScope : INameScope
{
	private List<DependencyObject> _affectedChildren;

	private static UncommonField<Hashtable> _templatedNonFeChildrenField = StyleHelper.TemplatedNonFeChildrenField;

	private DependencyObject _templatedParent;

	private FrameworkTemplate _frameworkTemplate;

	private bool _isTemplatedParentAnFE;

	private ProvideValueServiceProvider _provideValueServiceProvider;

	private HybridDictionary _nameMap;

	internal TemplateNameScope(DependencyObject templatedParent)
		: this(templatedParent, null, null)
	{
	}

	internal TemplateNameScope(DependencyObject templatedParent, List<DependencyObject> affectedChildren, FrameworkTemplate frameworkTemplate)
	{
		_affectedChildren = affectedChildren;
		_frameworkTemplate = frameworkTemplate;
		_templatedParent = templatedParent;
		_isTemplatedParentAnFE = true;
	}

	void INameScope.RegisterName(string name, object scopedElement)
	{
		if (!(scopedElement is FrameworkContentElement) && !(scopedElement is FrameworkElement))
		{
			RegisterNameInternal(name, scopedElement);
		}
	}

	internal void RegisterNameInternal(string name, object scopedElement)
	{
		Helper.DowncastToFEorFCE(scopedElement as DependencyObject, out var fe, out var fce, throwIfNeither: false);
		if (_templatedParent == null)
		{
			if (_nameMap == null)
			{
				_nameMap = new HybridDictionary();
			}
			_nameMap[name] = scopedElement;
			if (fe != null || fce != null)
			{
				SetTemplateParentValues(name, scopedElement);
			}
			return;
		}
		if (fe == null && fce == null)
		{
			Hashtable hashtable = _templatedNonFeChildrenField.GetValue(_templatedParent);
			if (hashtable == null)
			{
				hashtable = new Hashtable(1);
				_templatedNonFeChildrenField.SetValue(_templatedParent, hashtable);
			}
			hashtable[name] = scopedElement;
			return;
		}
		_affectedChildren.Add(scopedElement as DependencyObject);
		int num3;
		if (fe != null)
		{
			fe._templatedParent = _templatedParent;
			fe.IsTemplatedParentAnFE = _isTemplatedParentAnFE;
			int num2 = (fe.TemplateChildIndex = (int)_frameworkTemplate.ChildIndexFromChildName[name]);
			num3 = num2;
		}
		else
		{
			fce._templatedParent = _templatedParent;
			fce.IsTemplatedParentAnFE = _isTemplatedParentAnFE;
			int num2 = (fce.TemplateChildIndex = (int)_frameworkTemplate.ChildIndexFromChildName[name]);
			num3 = num2;
		}
		if (_frameworkTemplate._TemplateChildLoadedDictionary[num3] is FrameworkTemplate.TemplateChildLoadedFlags templateChildLoadedFlags && (templateChildLoadedFlags.HasLoadedChangedHandler || templateChildLoadedFlags.HasUnloadedChangedHandler))
		{
			BroadcastEventHelper.AddHasLoadedChangeHandlerFlagInAncestry((fe != null) ? ((DependencyObject)fe) : ((DependencyObject)fce));
		}
		StyleHelper.CreateInstanceDataForChild(StyleHelper.TemplateDataField, _templatedParent, (fe != null) ? ((DependencyObject)fe) : ((DependencyObject)fce), num3, _frameworkTemplate.HasInstanceValues, ref _frameworkTemplate.ChildRecordFromChildIndex);
	}

	void INameScope.UnregisterName(string name)
	{
	}

	object INameScope.FindName(string name)
	{
		if (_templatedParent != null)
		{
			FrameworkObject frameworkObject = new FrameworkObject(_templatedParent);
			if (frameworkObject.IsFE)
			{
				return StyleHelper.FindNameInTemplateContent(frameworkObject.FE, name, frameworkObject.FE.TemplateInternal);
			}
			return null;
		}
		if (_nameMap == null || name == null || name == string.Empty)
		{
			return null;
		}
		return _nameMap[name];
	}

	private void SetTemplateParentValues(string name, object element)
	{
		FrameworkTemplate.SetTemplateParentValues(name, element, _frameworkTemplate, ref _provideValueServiceProvider);
	}
}
