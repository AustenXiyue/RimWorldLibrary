using System.IO.Packaging;
using System.Runtime.Serialization;
using MS.Internal;
using MS.Internal.AppModel;
using MS.Internal.Utility;

namespace System.Windows.Navigation;

/// <summary>Represents an entry in either back or forward navigation history.</summary>
[Serializable]
public class JournalEntry : DependencyObject, ISerializable
{
	/// <summary>Identifies the <see cref="P:System.Windows.Navigation.JournalEntry.Name" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Navigation.JournalEntry.Name" /> attached property.</returns>
	public static readonly DependencyProperty NameProperty = DependencyProperty.RegisterAttached("Name", typeof(string), typeof(JournalEntry), new PropertyMetadata(string.Empty));

	/// <summary>Identifies the <see cref="P:System.Windows.Navigation.JournalEntry.KeepAlive" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Navigation.JournalEntry.KeepAlive" /> attached property.</returns>
	public static readonly DependencyProperty KeepAliveProperty = DependencyProperty.RegisterAttached("KeepAlive", typeof(bool), typeof(JournalEntry), new PropertyMetadata(false));

	private int _id;

	private JournalEntryGroupState _jeGroupState;

	private Uri _source;

	private JournalEntryType _entryType;

	private CustomContentState _customContentState;

	private CustomJournalStateInternal _rootViewerState;

	/// <summary>Gets or sets the URI of the content that was navigated to.</summary>
	/// <returns>The URI of the content that was navigated to, or null if no URI is associated with the entry.</returns>
	public Uri Source
	{
		get
		{
			return _source;
		}
		set
		{
			_source = MS.Internal.Utility.BindUriHelper.GetUriRelativeToPackAppBase(value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Navigation.CustomContentState" /> object that is associated with this journal entry.</summary>
	/// <returns>The <see cref="T:System.Windows.Navigation.CustomContentState" /> object that is associated with this journal entry. If one is not associated, null is returned.</returns>
	public CustomContentState CustomContentState
	{
		get
		{
			return _customContentState;
		}
		internal set
		{
			_customContentState = value;
		}
	}

	/// <summary>Gets or sets the name of the journal entry.</summary>
	/// <returns>The name of the journal entry.</returns>
	public string Name
	{
		get
		{
			return (string)GetValue(NameProperty);
		}
		set
		{
			SetValue(NameProperty, value);
		}
	}

	internal JournalEntryGroupState JEGroupState
	{
		get
		{
			return _jeGroupState;
		}
		set
		{
			_jeGroupState = value;
		}
	}

	internal int Id
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
		}
	}

	internal Guid NavigationServiceId => _jeGroupState.NavigationServiceId;

	internal JournalEntryType EntryType
	{
		get
		{
			return _entryType;
		}
		set
		{
			_entryType = value;
		}
	}

	internal uint ContentId => _jeGroupState.ContentId;

	internal CustomJournalStateInternal RootViewerState
	{
		get
		{
			return _rootViewerState;
		}
		set
		{
			_rootViewerState = value;
		}
	}

	internal JournalEntry(JournalEntryGroupState jeGroupState, Uri uri)
	{
		_jeGroupState = jeGroupState;
		if (jeGroupState != null)
		{
			jeGroupState.GroupExitEntry = this;
		}
		Source = uri;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Navigation.JournalEntry" /> class. </summary>
	/// <param name="info">The serialization information.</param>
	/// <param name="context">The streaming context.</param>
	protected JournalEntry(SerializationInfo info, StreamingContext context)
	{
		_id = info.GetInt32("_id");
		_source = (Uri)info.GetValue("_source", typeof(Uri));
		_entryType = (JournalEntryType)info.GetValue("_entryType", typeof(JournalEntryType));
		_jeGroupState = (JournalEntryGroupState)info.GetValue("_jeGroupState", typeof(JournalEntryGroupState));
		_customContentState = (CustomContentState)info.GetValue("_customContentState", typeof(CustomContentState));
		_rootViewerState = (CustomJournalStateInternal)info.GetValue("_rootViewerState", typeof(CustomJournalStateInternal));
		Name = info.GetString("Name");
	}

	/// <summary>Called when this object is serialized.</summary>
	/// <param name="info">The data that is required to serialize the target object.</param>
	/// <param name="context">The streaming context.</param>
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("_id", _id);
		info.AddValue("_source", _source);
		info.AddValue("_entryType", _entryType);
		info.AddValue("_jeGroupState", _jeGroupState);
		info.AddValue("_customContentState", _customContentState);
		info.AddValue("_rootViewerState", _rootViewerState);
		info.AddValue("Name", Name);
	}

	/// <summary>Gets the <see cref="P:System.Windows.Navigation.JournalEntry.Name" /> attached property of the journal entry for the specified element. </summary>
	/// <returns>The <see cref="P:System.Windows.Navigation.JournalEntry.Name" /> attached property of the journal entry for the specified element. </returns>
	/// <param name="dependencyObject">The element from which to get the attached property value.</param>
	public static string GetName(DependencyObject dependencyObject)
	{
		if (dependencyObject == null)
		{
			return null;
		}
		return (string)dependencyObject.GetValue(NameProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Navigation.JournalEntry.Name" /> attached property of the specified element.</summary>
	/// <param name="dependencyObject">The element on which to set the attached property value.</param>
	/// <param name="name">The name to be assigned to the attached property.</param>
	public static void SetName(DependencyObject dependencyObject, string name)
	{
		if (dependencyObject == null)
		{
			throw new ArgumentNullException("dependencyObject");
		}
		dependencyObject.SetValue(NameProperty, name);
	}

	/// <summary>Returns the <see cref="P:System.Windows.Navigation.JournalEntry.KeepAlive" /> attached property of the journal entry for the specified element. </summary>
	/// <returns>The value of the <see cref="P:System.Windows.Navigation.JournalEntry.KeepAlive" /> attached property of the journal entry for the specified element. </returns>
	/// <param name="dependencyObject">The element from which to get the attached property value.</param>
	public static bool GetKeepAlive(DependencyObject dependencyObject)
	{
		if (dependencyObject == null)
		{
			throw new ArgumentNullException("dependencyObject");
		}
		return (bool)dependencyObject.GetValue(KeepAliveProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Navigation.JournalEntry.KeepAlive" /> attached property of the specified element.</summary>
	/// <param name="dependencyObject">The element on which to set the attached property value.</param>
	/// <param name="keepAlive">true to keep the journal entry in memory; otherwise, false.</param>
	public static void SetKeepAlive(DependencyObject dependencyObject, bool keepAlive)
	{
		if (dependencyObject == null)
		{
			throw new ArgumentNullException("dependencyObject");
		}
		dependencyObject.SetValue(KeepAliveProperty, keepAlive);
	}

	internal virtual bool IsPageFunction()
	{
		return false;
	}

	internal virtual bool IsAlive()
	{
		return false;
	}

	internal virtual void SaveState(object contentObject)
	{
		if (contentObject == null)
		{
			throw new ArgumentNullException("contentObject");
		}
		if (!IsAlive())
		{
			if (_jeGroupState.JournalDataStreams == null)
			{
				_jeGroupState.JournalDataStreams = new DataStreams();
			}
			_jeGroupState.JournalDataStreams.Save(contentObject);
		}
	}

	internal virtual void RestoreState(object contentObject)
	{
		if (contentObject == null)
		{
			throw new ArgumentNullException("contentObject");
		}
		if (!IsAlive())
		{
			DataStreams journalDataStreams = _jeGroupState.JournalDataStreams;
			if (journalDataStreams != null)
			{
				journalDataStreams.Load(contentObject);
				journalDataStreams.Clear();
			}
		}
	}

	internal virtual bool Navigate(INavigator navigator, NavigationMode navMode)
	{
		if (Source != null)
		{
			return navigator.Navigate(Source, new NavigateInfo(Source, navMode, this));
		}
		Invariant.Assert(condition: false, "Cannot navigate to a journal entry that does not have a Source.");
		return false;
	}

	internal static string GetDisplayName(Uri uri, Uri siteOfOrigin)
	{
		if (!uri.IsAbsoluteUri)
		{
			return uri.ToString();
		}
		string text;
		if (string.Compare(uri.Scheme, PackUriHelper.UriSchemePack, StringComparison.OrdinalIgnoreCase) == 0)
		{
			Uri uri2 = BaseUriHelper.MakeRelativeToSiteOfOriginIfPossible(uri);
			if (!uri2.IsAbsoluteUri)
			{
				text = new Uri(siteOfOrigin, uri2).ToString();
			}
			else
			{
				string text2 = uri.AbsolutePath + uri.Query + uri.Fragment;
				BaseUriHelper.GetAssemblyNameAndPart(new Uri(text2, UriKind.Relative), out var partName, out var assemblyName, out var _, out var _);
				text = (string.IsNullOrEmpty(assemblyName) ? text2 : partName);
			}
		}
		else
		{
			text = uri.ToString();
		}
		if (!string.IsNullOrEmpty(text) && text[0] == '/')
		{
			text = text.Substring(1);
		}
		return text;
	}

	internal bool IsNavigable()
	{
		return _entryType == JournalEntryType.Navigable;
	}
}
