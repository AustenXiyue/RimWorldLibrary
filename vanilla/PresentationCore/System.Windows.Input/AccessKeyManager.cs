using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Interop;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace System.Windows.Input;

/// <summary>Maintains the registration of all access keys and the handling of interop keyboard commands between Windows Forms, Win32, and Windows Presentation Foundation (WPF).</summary>
public sealed class AccessKeyManager
{
	private enum ProcessKeyResult
	{
		NoMatch,
		MoreMatches,
		LastMatch
	}

	private struct AccessKeyInformation
	{
		public UIElement target;

		private static AccessKeyInformation _empty;

		private object _scope;

		public object Scope
		{
			get
			{
				return _scope;
			}
			set
			{
				_scope = value;
			}
		}

		public static AccessKeyInformation Empty => _empty;
	}

	/// <summary>Identifies the <see cref="E:System.Windows.Input.AccessKeyManager.AccessKeyPressed" /> routed event. </summary>
	public static readonly RoutedEvent AccessKeyPressedEvent = EventManager.RegisterRoutedEvent("AccessKeyPressed", RoutingStrategy.Bubble, typeof(AccessKeyPressedEventHandler), typeof(AccessKeyManager));

	private static readonly DependencyProperty AccessKeyElementProperty = DependencyProperty.RegisterAttached("AccessKeyElement", typeof(WeakReference), typeof(AccessKeyManager));

	private Hashtable _keyToElements = new Hashtable(10);

	[ThreadStatic]
	private static AccessKeyManager _accessKeyManager;

	private static AccessKeyManager Current
	{
		get
		{
			if (_accessKeyManager == null)
			{
				_accessKeyManager = new AccessKeyManager();
			}
			return _accessKeyManager;
		}
	}

	/// <summary>Associates the specified access keys with the specified element.</summary>
	/// <param name="key">The access key.</param>
	/// <param name="element">The element to associate <paramref name="key" /> with.</param>
	public static void Register(string key, IInputElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		key = NormalizeKey(key);
		AccessKeyManager current = Current;
		lock (current._keyToElements)
		{
			ArrayList arrayList = (ArrayList)current._keyToElements[key];
			if (arrayList == null)
			{
				arrayList = new ArrayList(1);
				current._keyToElements[key] = arrayList;
			}
			else
			{
				PurgeDead(arrayList, null);
			}
			arrayList.Add(new WeakReference(element));
		}
	}

	/// <summary>Disassociates the specified access keys from the specified element.</summary>
	/// <param name="key">The access key.</param>
	/// <param name="element">The element from which to disassociate <paramref name="key" />.</param>
	public static void Unregister(string key, IInputElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		key = NormalizeKey(key);
		AccessKeyManager current = Current;
		lock (current._keyToElements)
		{
			ArrayList arrayList = (ArrayList)current._keyToElements[key];
			if (arrayList != null)
			{
				PurgeDead(arrayList, element);
				if (arrayList.Count == 0)
				{
					current._keyToElements.Remove(key);
				}
			}
		}
	}

	/// <summary>Indicates whether the specified key is registered as an access keys in the specified scope.</summary>
	/// <returns>true if the key is registered; otherwise, false.</returns>
	/// <param name="scope">The presentation source to query for <paramref name="key" />.</param>
	/// <param name="key">The key to query.</param>
	public static bool IsKeyRegistered(object scope, string key)
	{
		key = NormalizeKey(key);
		List<IInputElement> targetsForScope = Current.GetTargetsForScope(scope, key, null, AccessKeyInformation.Empty);
		if (targetsForScope != null)
		{
			return targetsForScope.Count > 0;
		}
		return false;
	}

	/// <summary>Processes the specified access keys as if a <see cref="E:System.Windows.UIElement.KeyDown" /> event for the key was passed to the <see cref="T:System.Windows.Input.AccessKeyManager" />. </summary>
	/// <returns>true if there are more keys that match; otherwise, false.</returns>
	/// <param name="scope">The scope for the access key.</param>
	/// <param name="key">The access key.</param>
	/// <param name="isMultiple">Indicates if <paramref name="key" /> has multiple matches.</param>
	public static bool ProcessKey(object scope, string key, bool isMultiple)
	{
		key = NormalizeKey(key);
		return Current.ProcessKeyForScope(scope, key, isMultiple, userInitiated: false) == ProcessKeyResult.MoreMatches;
	}

	private static string NormalizeKey(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		string nextTextElement = StringInfo.GetNextTextElement(key);
		if (key != nextTextElement)
		{
			throw new ArgumentException(SR.Format(SR.AccessKeyManager_NotAUnicodeCharacter, "key"));
		}
		return nextTextElement.ToUpperInvariant();
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.AccessKeyManager.AccessKeyPressed" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be added.</param>
	public static void AddAccessKeyPressedHandler(DependencyObject element, AccessKeyPressedEventHandler handler)
	{
		UIElement.AddHandler(element, AccessKeyPressedEvent, handler);
	}

	/// <summary>Removes the specified <see cref="E:System.Windows.Input.AccessKeyManager.AccessKeyPressed" /> event handler from the specified object.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be removed.</param>
	public static void RemoveAccessKeyPressedHandler(DependencyObject element, AccessKeyPressedEventHandler handler)
	{
		UIElement.RemoveHandler(element, AccessKeyPressedEvent, handler);
	}

	private AccessKeyManager()
	{
		InputManager.Current.PostProcessInput += PostProcessInput;
	}

	private void PostProcessInput(object sender, ProcessInputEventArgs e)
	{
		if (!e.StagingItem.Input.Handled)
		{
			if (e.StagingItem.Input.RoutedEvent == Keyboard.KeyDownEvent)
			{
				OnKeyDown((KeyEventArgs)e.StagingItem.Input);
			}
			else if (e.StagingItem.Input.RoutedEvent == TextCompositionManager.TextInputEvent)
			{
				OnText((TextCompositionEventArgs)e.StagingItem.Input);
			}
		}
	}

	private ProcessKeyResult ProcessKeyForSender(object sender, string key, bool existsElsewhere, bool userInitiated)
	{
		key = key.ToUpperInvariant();
		IInputElement sender2 = sender as IInputElement;
		List<IInputElement> targetsForSender = GetTargetsForSender(sender2, key);
		return ProcessKey(targetsForSender, key, existsElsewhere, userInitiated);
	}

	private ProcessKeyResult ProcessKeyForScope(object scope, string key, bool existsElsewhere, bool userInitiated)
	{
		List<IInputElement> targetsForScope = GetTargetsForScope(scope, key, null, AccessKeyInformation.Empty);
		return ProcessKey(targetsForScope, key, existsElsewhere, userInitiated);
	}

	private ProcessKeyResult ProcessKey(List<IInputElement> targets, string key, bool existsElsewhere, bool userInitiated)
	{
		if (targets != null)
		{
			bool flag = true;
			UIElement uIElement = null;
			bool flag2 = false;
			int num = 0;
			for (int i = 0; i < targets.Count; i++)
			{
				UIElement uIElement2 = targets[i] as UIElement;
				if (!uIElement2.IsEnabled)
				{
					continue;
				}
				if (uIElement == null)
				{
					uIElement = uIElement2;
					num = i;
				}
				else
				{
					if (flag2)
					{
						uIElement = uIElement2;
						num = i;
					}
					flag = false;
				}
				flag2 = uIElement2.HasEffectiveKeyboardFocus;
			}
			if (uIElement != null)
			{
				AccessKeyEventArgs accessKeyEventArgs = new AccessKeyEventArgs(key, !flag || existsElsewhere, userInitiated);
				try
				{
					uIElement.InvokeAccessKey(accessKeyEventArgs);
				}
				finally
				{
					accessKeyEventArgs.ClearUserInitiated();
				}
				if (num != targets.Count - 1)
				{
					return ProcessKeyResult.MoreMatches;
				}
				return ProcessKeyResult.LastMatch;
			}
		}
		return ProcessKeyResult.NoMatch;
	}

	private void OnText(TextCompositionEventArgs e)
	{
		string text = e.Text;
		if (text == null || text.Length == 0)
		{
			text = e.SystemText;
		}
		if (text != null && text.Length > 0 && ProcessKeyForSender(e.OriginalSource, text, existsElsewhere: false, e.UserInitiated) != 0)
		{
			e.Handled = true;
		}
	}

	private void OnKeyDown(KeyEventArgs e)
	{
		_ = (KeyboardDevice)e.Device;
		string text = null;
		switch (e.RealKey)
		{
		case Key.Return:
			text = "\r";
			break;
		case Key.Escape:
			text = "\u001b";
			break;
		}
		if (text != null && ProcessKeyForSender(e.OriginalSource, text, existsElsewhere: false, e.UserInitiated) != 0)
		{
			e.Handled = true;
		}
	}

	private List<IInputElement> GetTargetsForSender(IInputElement sender, string key)
	{
		AccessKeyInformation infoForElement = GetInfoForElement(sender, key);
		return GetTargetsForScope(infoForElement.Scope, key, sender, infoForElement);
	}

	private List<IInputElement> GetTargetsForScope(object scope, string key, IInputElement sender, AccessKeyInformation senderInfo)
	{
		if (scope == null)
		{
			scope = CriticalGetActiveSource();
			if (scope == null)
			{
				return null;
			}
		}
		if (CoreCompatibilityPreferences.GetIsAltKeyRequiredInAccessKeyDefaultScope() && scope is PresentationSource && (Keyboard.Modifiers & ModifierKeys.Alt) != ModifierKeys.Alt)
		{
			return null;
		}
		List<IInputElement> list;
		lock (_keyToElements)
		{
			list = CopyAndPurgeDead(_keyToElements[key] as ArrayList);
		}
		if (list == null)
		{
			return null;
		}
		List<IInputElement> list2 = new List<IInputElement>(1);
		for (int i = 0; i < list.Count; i++)
		{
			IInputElement inputElement = list[i];
			if (inputElement != sender)
			{
				if (IsTargetable(inputElement))
				{
					AccessKeyInformation infoForElement = GetInfoForElement(inputElement, key);
					if (infoForElement.target != null && scope == infoForElement.Scope)
					{
						list2.Add(infoForElement.target);
					}
				}
			}
			else if (senderInfo.target != null)
			{
				list2.Add(senderInfo.target);
			}
		}
		return list2;
	}

	private AccessKeyInformation GetInfoForElement(IInputElement element, string key)
	{
		AccessKeyInformation result = default(AccessKeyInformation);
		if (element != null)
		{
			AccessKeyPressedEventArgs accessKeyPressedEventArgs = new AccessKeyPressedEventArgs(key);
			element.RaiseEvent(accessKeyPressedEventArgs);
			result.Scope = accessKeyPressedEventArgs.Scope;
			result.target = accessKeyPressedEventArgs.Target;
			if (result.Scope == null)
			{
				result.Scope = GetSourceForElement(element);
			}
		}
		else
		{
			result.Scope = CriticalGetActiveSource();
		}
		return result;
	}

	private PresentationSource GetSourceForElement(IInputElement element)
	{
		PresentationSource result = null;
		if (element is DependencyObject o)
		{
			DependencyObject containingVisual = InputElement.GetContainingVisual(o);
			if (containingVisual != null)
			{
				result = PresentationSource.CriticalFromVisual(containingVisual);
			}
		}
		return result;
	}

	private PresentationSource GetActiveSource()
	{
		nint activeWindow = MS.Win32.UnsafeNativeMethods.GetActiveWindow();
		if (activeWindow != IntPtr.Zero)
		{
			return HwndSource.FromHwnd(activeWindow);
		}
		return null;
	}

	private PresentationSource CriticalGetActiveSource()
	{
		nint activeWindow = MS.Win32.UnsafeNativeMethods.GetActiveWindow();
		if (activeWindow != IntPtr.Zero)
		{
			return HwndSource.CriticalFromHwnd(activeWindow);
		}
		return null;
	}

	private bool IsTargetable(IInputElement element)
	{
		DependencyObject containingUIElement = InputElement.GetContainingUIElement((DependencyObject)element);
		if (containingUIElement != null && IsVisible(containingUIElement) && IsEnabled(containingUIElement))
		{
			return true;
		}
		return false;
	}

	private static bool IsVisible(DependencyObject element)
	{
		while (element != null)
		{
			UIElement uIElement = element as UIElement;
			UIElement3D uIElement3D = element as UIElement3D;
			if ((uIElement?.Visibility ?? uIElement3D.Visibility) != 0)
			{
				return false;
			}
			element = UIElementHelper.GetUIParent(element);
		}
		return true;
	}

	private static bool IsEnabled(DependencyObject element)
	{
		return (bool)element.GetValue(UIElement.IsEnabledProperty);
	}

	private static void PurgeDead(ArrayList elements, object elementToRemove)
	{
		int num = 0;
		while (num < elements.Count)
		{
			object target = ((WeakReference)elements[num]).Target;
			if (target == null || target == elementToRemove)
			{
				elements.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	private static List<IInputElement> CopyAndPurgeDead(ArrayList elements)
	{
		if (elements == null)
		{
			return null;
		}
		List<IInputElement> list = new List<IInputElement>(elements.Count);
		int num = 0;
		while (num < elements.Count)
		{
			object target = ((WeakReference)elements[num]).Target;
			if (target == null)
			{
				elements.RemoveAt(num);
				continue;
			}
			list.Add((IInputElement)target);
			num++;
		}
		return list;
	}

	internal static string InternalGetAccessKeyCharacter(DependencyObject d)
	{
		return Current.GetAccessKeyCharacter(d);
	}

	private string GetAccessKeyCharacter(DependencyObject d)
	{
		WeakReference weakReference = (WeakReference)d.GetValue(AccessKeyElementProperty);
		IInputElement inputElement = ((weakReference != null) ? ((IInputElement)weakReference.Target) : null);
		if (inputElement != null)
		{
			AccessKeyPressedEventArgs accessKeyPressedEventArgs = new AccessKeyPressedEventArgs();
			inputElement.RaiseEvent(accessKeyPressedEventArgs);
			if (accessKeyPressedEventArgs.Target == d)
			{
				foreach (DictionaryEntry keyToElement in Current._keyToElements)
				{
					ArrayList arrayList = (ArrayList)keyToElement.Value;
					for (int i = 0; i < arrayList.Count; i++)
					{
						if (((WeakReference)arrayList[i]).Target == inputElement)
						{
							return (string)keyToElement.Key;
						}
					}
				}
			}
		}
		d.ClearValue(AccessKeyElementProperty);
		foreach (DictionaryEntry keyToElement2 in Current._keyToElements)
		{
			ArrayList arrayList2 = (ArrayList)keyToElement2.Value;
			for (int j = 0; j < arrayList2.Count; j++)
			{
				WeakReference weakReference2 = (WeakReference)arrayList2[j];
				IInputElement inputElement2 = (IInputElement)weakReference2.Target;
				if (inputElement2 == null)
				{
					continue;
				}
				AccessKeyPressedEventArgs accessKeyPressedEventArgs2 = new AccessKeyPressedEventArgs();
				inputElement2.RaiseEvent(accessKeyPressedEventArgs2);
				if (accessKeyPressedEventArgs2.Target != null)
				{
					accessKeyPressedEventArgs2.Target.SetValue(AccessKeyElementProperty, weakReference2);
					if (accessKeyPressedEventArgs2.Target == d)
					{
						return (string)keyToElement2.Key;
					}
				}
			}
		}
		return string.Empty;
	}
}
