using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Data;
using MS.Win32;

namespace System.Windows.Controls;

/// <summary>Enables a user to quickly access items in a set by typing prefixes of strings. </summary>
public sealed class TextSearch : DependencyObject
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextSearch.TextPath" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextSearch.TextPath" /> attached property.</returns>
	public static readonly DependencyProperty TextPathProperty = DependencyProperty.RegisterAttached("TextPath", typeof(string), typeof(TextSearch), new FrameworkPropertyMetadata(string.Empty));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextSearch.Text" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextSearch.Text" /> attached property.</returns>
	public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("Text", typeof(string), typeof(TextSearch), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

	private static readonly DependencyProperty CurrentPrefixProperty = DependencyProperty.RegisterAttached("CurrentPrefix", typeof(string), typeof(TextSearch), new FrameworkPropertyMetadata((object)null));

	private static readonly DependencyProperty IsActiveProperty = DependencyProperty.RegisterAttached("IsActive", typeof(bool), typeof(TextSearch), new FrameworkPropertyMetadata(false));

	private static readonly DependencyPropertyKey TextSearchInstancePropertyKey = DependencyProperty.RegisterAttachedReadOnly("TextSearchInstance", typeof(TextSearch), typeof(TextSearch), new FrameworkPropertyMetadata((object)null));

	private static readonly DependencyProperty TextSearchInstanceProperty = TextSearchInstancePropertyKey.DependencyProperty;

	private static readonly BindingExpressionUncommonField TextValueBindingExpression = new BindingExpressionUncommonField();

	private ItemsControl _attachedTo;

	private string _prefix;

	private List<string> _charsEntered;

	private bool _isActive;

	private int _matchedItemIndex;

	private DispatcherTimer _timeoutTimer;

	private TimeSpan TimeOut => TimeSpan.FromMilliseconds(SafeNativeMethods.GetDoubleClickTime() * 2);

	private string Prefix
	{
		get
		{
			return _prefix;
		}
		set
		{
			_prefix = value;
		}
	}

	private bool IsActive
	{
		get
		{
			return _isActive;
		}
		set
		{
			_isActive = value;
		}
	}

	private int MatchedItemIndex
	{
		get
		{
			return _matchedItemIndex;
		}
		set
		{
			_matchedItemIndex = value;
		}
	}

	private TextSearch(ItemsControl itemsControl)
	{
		if (itemsControl == null)
		{
			throw new ArgumentNullException("itemsControl");
		}
		_attachedTo = itemsControl;
		ResetState();
	}

	internal static TextSearch EnsureInstance(ItemsControl itemsControl)
	{
		TextSearch textSearch = (TextSearch)itemsControl.GetValue(TextSearchInstanceProperty);
		if (textSearch == null)
		{
			textSearch = new TextSearch(itemsControl);
			itemsControl.SetValue(TextSearchInstancePropertyKey, textSearch);
		}
		return textSearch;
	}

	/// <summary> Writes the <see cref="P:System.Windows.Controls.TextSearch.TextPath" /> attached property to the specified element. </summary>
	/// <param name="element">The element to which the property value is written.</param>
	/// <param name="path">The name of the property that identifies an item.</param>
	public static void SetTextPath(DependencyObject element, string path)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(TextPathProperty, path);
	}

	/// <summary>Returns the name of the property that identifies an item in the specified element's collection. </summary>
	/// <returns>The name of the property that identifies the item to the user.</returns>
	/// <param name="element">The element from which the property value is read.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static string GetTextPath(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (string)element.GetValue(TextPathProperty);
	}

	/// <summary> Writes the <see cref="P:System.Windows.Controls.TextSearch.Text" /> attached property value to the specified element. </summary>
	/// <param name="element">The element to which the property value is written.</param>
	/// <param name="text">The string that identifies the item.</param>
	public static void SetText(DependencyObject element, string text)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(TextProperty, text);
	}

	/// <summary>Returns the string to that identifies the specified item.</summary>
	/// <returns>The string that identifies the specified item.</returns>
	/// <param name="element">The element from which the property value is read.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static string GetText(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (string)element.GetValue(TextProperty);
	}

	internal bool DoSearch(string nextChar)
	{
		bool lookForFallbackMatchToo = false;
		int num = 0;
		ItemCollection items = _attachedTo.Items;
		if (IsActive)
		{
			num = MatchedItemIndex;
		}
		if (_charsEntered.Count > 0 && string.Compare(_charsEntered[_charsEntered.Count - 1], nextChar, ignoreCase: true, GetCulture(_attachedTo)) == 0)
		{
			lookForFallbackMatchToo = true;
		}
		string primaryTextPath = GetPrimaryTextPath(_attachedTo);
		bool wasNewCharUsed = false;
		int num2 = FindMatchingPrefix(_attachedTo, primaryTextPath, Prefix, nextChar, num, lookForFallbackMatchToo, ref wasNewCharUsed);
		if (num2 != -1)
		{
			if (!IsActive || num2 != num)
			{
				object item = items[num2];
				_attachedTo.NavigateToItem(item, num2, new ItemsControl.ItemNavigateArgs(Keyboard.PrimaryDevice, ModifierKeys.None));
				MatchedItemIndex = num2;
			}
			if (wasNewCharUsed)
			{
				AddCharToPrefix(nextChar);
			}
			if (!IsActive)
			{
				IsActive = true;
			}
		}
		if (IsActive)
		{
			ResetTimeout();
		}
		return num2 != -1;
	}

	internal bool DeleteLastCharacter()
	{
		if (IsActive && _charsEntered.Count > 0)
		{
			string text = _charsEntered[_charsEntered.Count - 1];
			string prefix = Prefix;
			_charsEntered.RemoveAt(_charsEntered.Count - 1);
			Prefix = prefix.Substring(0, prefix.Length - text.Length);
			ResetTimeout();
			return true;
		}
		return false;
	}

	private static void GetMatchingPrefixAndRemainingTextLength(string matchedText, string newText, CultureInfo cultureInfo, bool ignoreCase, out int matchedPrefixLength, out int textExcludingPrefixLength)
	{
		matchedPrefixLength = 0;
		textExcludingPrefixLength = 0;
		if (matchedText.Length < newText.Length)
		{
			matchedPrefixLength = matchedText.Length;
			textExcludingPrefixLength = 0;
			return;
		}
		int num = newText.Length;
		int num2 = num + 1;
		CompareInfo compareInfo = (cultureInfo ?? CultureInfo.CurrentCulture).CompareInfo;
		CompareOptions options = (ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
		do
		{
			if (num >= 1)
			{
				ReadOnlySpan<char> @string = matchedText.AsSpan(0, num);
				if (compareInfo.Compare(newText, @string, options) == 0)
				{
					matchedPrefixLength = num;
					textExcludingPrefixLength = matchedText.Length - num;
					break;
				}
			}
			if (num2 <= matchedText.Length)
			{
				ReadOnlySpan<char> @string = matchedText.AsSpan(0, num2);
				if (compareInfo.Compare(newText, @string, options) == 0)
				{
					matchedPrefixLength = num2;
					textExcludingPrefixLength = matchedText.Length - num2;
					break;
				}
			}
			num--;
			num2++;
		}
		while (num >= 1 || num2 <= matchedText.Length);
	}

	private static int FindMatchingPrefix(ItemsControl itemsControl, string primaryTextPath, string prefix, string newChar, int startItemIndex, bool lookForFallbackMatchToo, ref bool wasNewCharUsed)
	{
		ItemCollection items = itemsControl.Items;
		int num = -1;
		int num2 = -1;
		int count = items.Count;
		if (count == 0)
		{
			return -1;
		}
		string value = prefix + newChar;
		if (string.IsNullOrEmpty(value))
		{
			return -1;
		}
		BindingExpression bindingExpression = null;
		object item = itemsControl.Items[0];
		if (SystemXmlHelper.IsXmlNode(item) || !string.IsNullOrEmpty(primaryTextPath))
		{
			bindingExpression = CreateBindingExpression(itemsControl, item, primaryTextPath);
			TextValueBindingExpression.SetValue(itemsControl, bindingExpression);
		}
		bool flag = true;
		wasNewCharUsed = false;
		CultureInfo culture = GetCulture(itemsControl);
		int num3 = startItemIndex;
		while (num3 < count)
		{
			object obj = items[num3];
			if (obj != null)
			{
				string primaryText = GetPrimaryText(obj, bindingExpression, itemsControl);
				bool isTextSearchCaseSensitive = itemsControl.IsTextSearchCaseSensitive;
				if (primaryText != null && primaryText.StartsWith(value, !isTextSearchCaseSensitive, culture))
				{
					wasNewCharUsed = true;
					num = num3;
					break;
				}
				if (lookForFallbackMatchToo)
				{
					if (!flag && prefix != string.Empty)
					{
						if (primaryText != null && num2 == -1 && primaryText.StartsWith(prefix, !isTextSearchCaseSensitive, culture))
						{
							num2 = num3;
						}
					}
					else
					{
						flag = false;
					}
				}
			}
			num3++;
			if (num3 >= count)
			{
				num3 = 0;
			}
			if (num3 == startItemIndex)
			{
				break;
			}
		}
		if (bindingExpression != null)
		{
			TextValueBindingExpression.ClearValue(itemsControl);
		}
		if (num == -1 && num2 != -1)
		{
			num = num2;
		}
		return num;
	}

	internal static MatchedTextInfo FindMatchingPrefix(ItemsControl itemsControl, string prefix)
	{
		bool wasNewCharUsed = false;
		int num = FindMatchingPrefix(itemsControl, GetPrimaryTextPath(itemsControl), prefix, string.Empty, 0, lookForFallbackMatchToo: false, ref wasNewCharUsed);
		if (num >= 0)
		{
			CultureInfo culture = GetCulture(itemsControl);
			bool isTextSearchCaseSensitive = itemsControl.IsTextSearchCaseSensitive;
			string primaryTextFromItem = GetPrimaryTextFromItem(itemsControl, itemsControl.Items[num]);
			GetMatchingPrefixAndRemainingTextLength(primaryTextFromItem, prefix, culture, !isTextSearchCaseSensitive, out var matchedPrefixLength, out var textExcludingPrefixLength);
			return new MatchedTextInfo(num, primaryTextFromItem, matchedPrefixLength, textExcludingPrefixLength);
		}
		return MatchedTextInfo.NoMatch;
	}

	private void ResetTimeout()
	{
		if (_timeoutTimer == null)
		{
			_timeoutTimer = new DispatcherTimer(DispatcherPriority.Normal);
			_timeoutTimer.Tick += OnTimeout;
		}
		else
		{
			_timeoutTimer.Stop();
		}
		_timeoutTimer.Interval = TimeOut;
		_timeoutTimer.Start();
	}

	private void AddCharToPrefix(string newChar)
	{
		Prefix += newChar;
		_charsEntered.Add(newChar);
	}

	private static string GetPrimaryTextPath(ItemsControl itemsControl)
	{
		string text = (string)itemsControl.GetValue(TextPathProperty);
		if (string.IsNullOrEmpty(text))
		{
			text = itemsControl.DisplayMemberPath;
		}
		return text;
	}

	private static string GetPrimaryText(object item, BindingExpression primaryTextBinding, DependencyObject primaryTextBindingHome)
	{
		if (item is DependencyObject dependencyObject)
		{
			string text = (string)dependencyObject.GetValue(TextProperty);
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
		}
		if (primaryTextBinding != null && primaryTextBindingHome != null)
		{
			primaryTextBinding.Activate(item);
			return ConvertToPlainText(primaryTextBinding.Value);
		}
		return ConvertToPlainText(item);
	}

	private static string ConvertToPlainText(object o)
	{
		if (o is FrameworkElement frameworkElement)
		{
			string plainText = frameworkElement.GetPlainText();
			if (plainText != null)
			{
				return plainText;
			}
		}
		if (o == null)
		{
			return string.Empty;
		}
		return o.ToString();
	}

	internal static string GetPrimaryTextFromItem(ItemsControl itemsControl, object item)
	{
		if (item == null)
		{
			return string.Empty;
		}
		BindingExpression bindingExpression = CreateBindingExpression(itemsControl, item, GetPrimaryTextPath(itemsControl));
		TextValueBindingExpression.SetValue(itemsControl, bindingExpression);
		string primaryText = GetPrimaryText(item, bindingExpression, itemsControl);
		TextValueBindingExpression.ClearValue(itemsControl);
		return primaryText;
	}

	private static BindingExpression CreateBindingExpression(ItemsControl itemsControl, object item, string primaryTextPath)
	{
		Binding binding = new Binding();
		if (SystemXmlHelper.IsXmlNode(item))
		{
			binding.XPath = primaryTextPath;
			binding.Path = new PropertyPath("/InnerText");
		}
		else
		{
			binding.Path = new PropertyPath(primaryTextPath);
		}
		binding.Mode = BindingMode.OneWay;
		binding.Source = null;
		return (BindingExpression)BindingExpressionBase.CreateUntargetedBindingExpression(itemsControl, binding);
	}

	private void OnTimeout(object sender, EventArgs e)
	{
		ResetState();
	}

	private void ResetState()
	{
		IsActive = false;
		Prefix = string.Empty;
		MatchedItemIndex = -1;
		if (_charsEntered == null)
		{
			_charsEntered = new List<string>(10);
		}
		else
		{
			_charsEntered.Clear();
		}
		if (_timeoutTimer != null)
		{
			_timeoutTimer.Stop();
		}
		_timeoutTimer = null;
	}

	private static TextSearch GetInstance(DependencyObject d)
	{
		return EnsureInstance(d as ItemsControl);
	}

	private void TypeAKey(string c)
	{
		DoSearch(c);
	}

	private void CauseTimeOut()
	{
		if (_timeoutTimer != null)
		{
			_timeoutTimer.Stop();
			OnTimeout(_timeoutTimer, EventArgs.Empty);
		}
	}

	internal string GetCurrentPrefix()
	{
		return Prefix;
	}

	internal static string GetPrimaryText(FrameworkElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		string text = (string)element.GetValue(TextProperty);
		if (text != null && text != string.Empty)
		{
			return text;
		}
		return element.GetPlainText();
	}

	private static CultureInfo GetCulture(DependencyObject element)
	{
		object value = element.GetValue(FrameworkElement.LanguageProperty);
		CultureInfo result = null;
		if (value != null)
		{
			XmlLanguage xmlLanguage = (XmlLanguage)value;
			try
			{
				result = xmlLanguage.GetSpecificCulture();
			}
			catch (InvalidOperationException)
			{
			}
		}
		return result;
	}
}
