using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Baml2006;
using System.Windows.Diagnostics;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xaml;
using MS.Internal;
using MS.Internal.AppModel;
using MS.Internal.Utility;

namespace System.Windows;

/// <summary>Provides a hash table / dictionary implementation that contains WPF resources used by components and other elements of a WPF application. </summary>
[Localizability(LocalizationCategory.Ignore)]
[Ambient]
[UsableDuringInitialization(true)]
public class ResourceDictionary : IDictionary, ICollection, IEnumerable, ISupportInitialize, IUriContext, INameScope
{
	private class ResourceDictionaryEnumerator : IDictionaryEnumerator, IEnumerator
	{
		private ResourceDictionary _owner;

		private IEnumerator _keysEnumerator;

		object IEnumerator.Current => ((IDictionaryEnumerator)this).Entry;

		DictionaryEntry IDictionaryEnumerator.Entry
		{
			get
			{
				object current = _keysEnumerator.Current;
				object value = _owner[current];
				return new DictionaryEntry(current, value);
			}
		}

		object IDictionaryEnumerator.Key => _keysEnumerator.Current;

		object IDictionaryEnumerator.Value => _owner[_keysEnumerator.Current];

		internal ResourceDictionaryEnumerator(ResourceDictionary owner)
		{
			_owner = owner;
			_keysEnumerator = _owner.Keys.GetEnumerator();
		}

		bool IEnumerator.MoveNext()
		{
			return _keysEnumerator.MoveNext();
		}

		void IEnumerator.Reset()
		{
			_keysEnumerator.Reset();
		}
	}

	private class ResourceValuesEnumerator : IEnumerator
	{
		private ResourceDictionary _owner;

		private IEnumerator _keysEnumerator;

		object IEnumerator.Current => _owner[_keysEnumerator.Current];

		internal ResourceValuesEnumerator(ResourceDictionary owner)
		{
			_owner = owner;
			_keysEnumerator = _owner.Keys.GetEnumerator();
		}

		bool IEnumerator.MoveNext()
		{
			return _keysEnumerator.MoveNext();
		}

		void IEnumerator.Reset()
		{
			_keysEnumerator.Reset();
		}
	}

	private class ResourceValuesCollection : ICollection, IEnumerable
	{
		private ResourceDictionary _owner;

		int ICollection.Count => _owner.Count;

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot => this;

		internal ResourceValuesCollection(ResourceDictionary owner)
		{
			_owner = owner;
		}

		void ICollection.CopyTo(Array array, int index)
		{
			foreach (object key in _owner.Keys)
			{
				array.SetValue(_owner[key], index++);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new ResourceValuesEnumerator(_owner);
		}
	}

	private enum PrivateFlags : byte
	{
		IsInitialized = 1,
		IsInitializePending = 2,
		IsReadOnly = 4,
		IsThemeDictionary = 8,
		HasImplicitStyles = 0x10,
		CanBeAccessedAcrossThreads = 0x20,
		InvalidatesImplicitDataTemplateResources = 0x40,
		HasImplicitDataTemplates = 0x80
	}

	internal class ResourceDictionarySourceUriWrapper : Uri
	{
		internal Uri OriginalUri { get; set; }

		internal Uri VersionedUri { get; set; }

		public ResourceDictionarySourceUriWrapper(Uri originalUri, Uri versionedUri)
			: base(originalUri.OriginalString, UriKind.RelativeOrAbsolute)
		{
			OriginalUri = originalUri;
			VersionedUri = versionedUri;
		}
	}

	private enum FallbackState
	{
		Classic,
		Generic,
		None
	}

	internal bool IsSourcedFromThemeDictionary;

	private FallbackState _fallbackState;

	private Hashtable _baseDictionary;

	private WeakReferenceList _ownerFEs;

	private WeakReferenceList _ownerFCEs;

	private WeakReferenceList _ownerApps;

	private WeakReferenceList _deferredResourceReferences;

	private ObservableCollection<ResourceDictionary> _mergedDictionaries;

	private Uri _source;

	private Uri _baseUri;

	private PrivateFlags _flags;

	private List<KeyRecord> _deferredLocationList;

	private byte[] _buffer;

	private Stream _bamlStream;

	private long _startPosition;

	private int _contentSize;

	private object _rootElement;

	private int _numDefer;

	private WeakReference _inheritanceContext;

	private static readonly DependencyObject DummyInheritanceContext;

	private XamlObjectIds _contextXamlObjectIds = new XamlObjectIds();

	private IXamlObjectWriterFactory _objectWriterFactory;

	private XamlObjectWriterSettings _objectWriterSettings;

	private Baml2006Reader _reader;

	internal bool IsUnsafe { get; set; }

	/// <summary>Gets a collection of the <see cref="T:System.Windows.ResourceDictionary" /> dictionaries that constitute the various resource dictionaries in the merged dictionaries.</summary>
	/// <returns>The collection of merged dictionaries.</returns>
	public Collection<ResourceDictionary> MergedDictionaries
	{
		get
		{
			if (_mergedDictionaries == null)
			{
				_mergedDictionaries = new ResourceDictionaryCollection(this);
				_mergedDictionaries.CollectionChanged += OnMergedDictionariesChanged;
			}
			return _mergedDictionaries;
		}
	}

	/// <summary>Gets or sets the uniform resource identifier (URI) to load resources from.</summary>
	/// <returns>The source location of an external resource dictionary. </returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Uri Source
	{
		get
		{
			return _source;
		}
		set
		{
			if (value == null || string.IsNullOrEmpty(value.OriginalString))
			{
				throw new ArgumentException(SR.Format(SR.ResourceDictionaryLoadFromFailure, (value == null) ? "''" : value.ToString()));
			}
			ResourceDictionaryDiagnostics.RemoveResourceDictionaryForUri(_source, this);
			ResourceDictionarySourceUriWrapper resourceDictionarySourceUriWrapper = value as ResourceDictionarySourceUriWrapper;
			Uri orgUri;
			if (resourceDictionarySourceUriWrapper == null)
			{
				_source = value;
				orgUri = _source;
			}
			else
			{
				_source = resourceDictionarySourceUriWrapper.OriginalUri;
				orgUri = resourceDictionarySourceUriWrapper.VersionedUri;
			}
			Clear();
			Uri resolvedUri = MS.Internal.Utility.BindUriHelper.GetResolvedUri(_baseUri, orgUri);
			WebRequest request = WpfWebRequestHelper.CreateRequest(resolvedUri);
			WpfWebRequestHelper.ConfigCachePolicy(request, isRefresh: false);
			ContentType contentType = null;
			Stream stream = null;
			try
			{
				stream = WpfWebRequestHelper.GetResponseStream(request, out contentType);
			}
			catch (IOException)
			{
				if (IsSourcedFromThemeDictionary)
				{
					switch (_fallbackState)
					{
					case FallbackState.Classic:
					{
						_fallbackState = FallbackState.Generic;
						Uri source2 = ThemeDictionaryExtension.GenerateFallbackUri(this, "themes/classic");
						Source = source2;
						_fallbackState = FallbackState.Classic;
						break;
					}
					case FallbackState.Generic:
					{
						_fallbackState = FallbackState.None;
						Uri source = ThemeDictionaryExtension.GenerateFallbackUri(this, "themes/generic");
						Source = source;
						break;
					}
					}
					return;
				}
				throw;
			}
			if (!(MimeObjectFactory.GetObjectAndCloseStreamCore(stream, contentType, resolvedUri, canUseTopLevelBrowser: false, sandboxExternalContent: false, allowAsync: false, isJournalNavigation: false, out var _, IsUnsafe) is ResourceDictionary resourceDictionary))
			{
				throw new InvalidOperationException(SR.Format(SR.ResourceDictionaryLoadFromFailure, _source.ToString()));
			}
			_baseDictionary = resourceDictionary._baseDictionary;
			_mergedDictionaries = resourceDictionary._mergedDictionaries;
			CopyDeferredContentFrom(resourceDictionary);
			MoveDeferredResourceReferencesFrom(resourceDictionary);
			HasImplicitStyles = resourceDictionary.HasImplicitStyles;
			HasImplicitDataTemplates = resourceDictionary.HasImplicitDataTemplates;
			InvalidatesImplicitDataTemplateResources = resourceDictionary.InvalidatesImplicitDataTemplateResources;
			if (InheritanceContext != null)
			{
				AddInheritanceContextToValues();
			}
			if (_mergedDictionaries != null)
			{
				for (int i = 0; i < _mergedDictionaries.Count; i++)
				{
					PropagateParentOwners(_mergedDictionaries[i]);
				}
			}
			ResourceDictionaryDiagnostics.AddResourceDictionaryForUri(resolvedUri, this);
			if (!IsInitializePending)
			{
				NotifyOwners(new ResourcesChangeInfo(null, this));
			}
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Windows.Markup.IUriContext.BaseUri" />.</summary>
	/// <returns>The base URI of the current context.</returns>
	Uri IUriContext.BaseUri
	{
		get
		{
			return _baseUri;
		}
		set
		{
			_baseUri = value;
		}
	}

	/// <summary>Gets whether this <see cref="T:System.Windows.ResourceDictionary" /> is fixed-size. </summary>
	/// <returns>true if the hash table is fixed-size; otherwise, false.</returns>
	public bool IsFixedSize => _baseDictionary.IsFixedSize;

	/// <summary>Gets whether this <see cref="T:System.Windows.ResourceDictionary" /> is read-only. </summary>
	/// <returns>true if the hash table is read-only; otherwise, false.</returns>
	public bool IsReadOnly
	{
		get
		{
			return ReadPrivateFlag(PrivateFlags.IsReadOnly);
		}
		internal set
		{
			WritePrivateFlag(PrivateFlags.IsReadOnly, value);
			if (value)
			{
				SealValues();
			}
			if (_mergedDictionaries != null)
			{
				for (int i = 0; i < _mergedDictionaries.Count; i++)
				{
					_mergedDictionaries[i].IsReadOnly = value;
				}
			}
		}
	}

	[DefaultValue(false)]
	public bool InvalidatesImplicitDataTemplateResources
	{
		get
		{
			return ReadPrivateFlag(PrivateFlags.InvalidatesImplicitDataTemplateResources);
		}
		set
		{
			WritePrivateFlag(PrivateFlags.InvalidatesImplicitDataTemplateResources, value);
		}
	}

	/// <summary> Gets or sets the value associated with the given key. </summary>
	/// <returns>Value of the key.</returns>
	/// <param name="key">The desired key to get or set.</param>
	public object this[object key]
	{
		get
		{
			bool canCache;
			return GetValue(key, out canCache);
		}
		set
		{
			SealValue(value);
			if (CanBeAccessedAcrossThreads)
			{
				lock (((ICollection)this).SyncRoot)
				{
					SetValueWithoutLock(key, value);
					return;
				}
			}
			SetValueWithoutLock(key, value);
		}
	}

	/// <summary>Gets or sets the deferrable content for this resource dictionary.</summary>
	/// <returns>Always returns null.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DeferrableContent DeferrableContent
	{
		get
		{
			return null;
		}
		set
		{
			SetDeferrableContent(value);
		}
	}

	/// <summary>Gets a collection of all keys contained in this <see cref="T:System.Windows.ResourceDictionary" />. </summary>
	/// <returns>The collection of all keys.</returns>
	public ICollection Keys
	{
		get
		{
			object[] array = new object[Count];
			_baseDictionary.Keys.CopyTo(array, 0);
			return array;
		}
	}

	/// <summary> Gets a collection of all values associated with keys contained in this <see cref="T:System.Windows.ResourceDictionary" />. </summary>
	/// <returns>The collection of all values.</returns>
	public ICollection Values => new ResourceValuesCollection(this);

	/// <summary>Gets the number of entries in the base <see cref="T:System.Windows.ResourceDictionary" />. </summary>
	/// <returns>The current number of entries in the base dictionary.</returns>
	public int Count => _baseDictionary.Count;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.IsSynchronized" />.</summary>
	/// <returns>true if access to <see cref="T:System.Windows.ResourceDictionary" /> is synchronized (thread safe); otherwise, false. </returns>
	bool ICollection.IsSynchronized => _baseDictionary.IsSynchronized;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.SyncRoot" />.</summary>
	/// <returns>An object that can be used to synchronize access to <see cref="T:System.Windows.ResourceDictionary" />. </returns>
	object ICollection.SyncRoot
	{
		get
		{
			if (CanBeAccessedAcrossThreads)
			{
				return SystemResources.ThemeDictionaryLock;
			}
			return _baseDictionary.SyncRoot;
		}
	}

	internal WeakReferenceList FrameworkElementOwners => _ownerFEs;

	internal WeakReferenceList FrameworkContentElementOwners => _ownerFCEs;

	internal WeakReferenceList ApplicationOwners => _ownerApps;

	internal WeakReferenceList DeferredResourceReferences => _deferredResourceReferences;

	private DependencyObject InheritanceContext
	{
		get
		{
			if (_inheritanceContext == null)
			{
				return null;
			}
			return (DependencyObject)_inheritanceContext.Target;
		}
	}

	private bool IsInitialized
	{
		get
		{
			return ReadPrivateFlag(PrivateFlags.IsInitialized);
		}
		set
		{
			WritePrivateFlag(PrivateFlags.IsInitialized, value);
		}
	}

	private bool IsInitializePending
	{
		get
		{
			return ReadPrivateFlag(PrivateFlags.IsInitializePending);
		}
		set
		{
			WritePrivateFlag(PrivateFlags.IsInitializePending, value);
		}
	}

	private bool IsThemeDictionary
	{
		get
		{
			return ReadPrivateFlag(PrivateFlags.IsThemeDictionary);
		}
		set
		{
			if (IsThemeDictionary == value)
			{
				return;
			}
			WritePrivateFlag(PrivateFlags.IsThemeDictionary, value);
			if (value)
			{
				SealValues();
			}
			if (_mergedDictionaries != null)
			{
				for (int i = 0; i < _mergedDictionaries.Count; i++)
				{
					_mergedDictionaries[i].IsThemeDictionary = value;
				}
			}
		}
	}

	internal bool HasImplicitStyles
	{
		get
		{
			return ReadPrivateFlag(PrivateFlags.HasImplicitStyles);
		}
		set
		{
			WritePrivateFlag(PrivateFlags.HasImplicitStyles, value);
		}
	}

	internal bool HasImplicitDataTemplates
	{
		get
		{
			return ReadPrivateFlag(PrivateFlags.HasImplicitDataTemplates);
		}
		set
		{
			WritePrivateFlag(PrivateFlags.HasImplicitDataTemplates, value);
		}
	}

	internal bool CanBeAccessedAcrossThreads
	{
		get
		{
			return ReadPrivateFlag(PrivateFlags.CanBeAccessedAcrossThreads);
		}
		set
		{
			WritePrivateFlag(PrivateFlags.CanBeAccessedAcrossThreads, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.ResourceDictionary" /> class. </summary>
	public ResourceDictionary()
	{
		_baseDictionary = new Hashtable();
		IsThemeDictionary = SystemResources.IsSystemResourcesParsing;
	}

	static ResourceDictionary()
	{
		DummyInheritanceContext = new DependencyObject();
		DummyInheritanceContext.DetachFromDispatcher();
	}

	/// <summary>Copies the <see cref="T:System.Windows.ResourceDictionary" /> elements to a one-dimensional <see cref="T:System.Collections.DictionaryEntry" /> at the specified index. </summary>
	/// <param name="array">The one-dimensional array that is the destination of the <see cref="T:System.Collections.DictionaryEntry" /> objects copied from the <see cref="T:System.Windows.ResourceDictionary" /> instance. The array must have zero-based indexing. </param>
	/// <param name="arrayIndex">The zero-based index of <paramref name="array" /> where copying begins.</param>
	public void CopyTo(DictionaryEntry[] array, int arrayIndex)
	{
		if (CanBeAccessedAcrossThreads)
		{
			lock (((ICollection)this).SyncRoot)
			{
				CopyToWithoutLock(array, arrayIndex);
				return;
			}
		}
		CopyToWithoutLock(array, arrayIndex);
	}

	private void CopyToWithoutLock(DictionaryEntry[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		_baseDictionary.CopyTo(array, arrayIndex);
		int num = arrayIndex + Count;
		for (int i = arrayIndex; i < num; i++)
		{
			DictionaryEntry dictionaryEntry = array[i];
			object value = dictionaryEntry.Value;
			OnGettingValuePrivate(dictionaryEntry.Key, ref value, out var _);
			dictionaryEntry.Value = value;
		}
	}

	/// <summary>Not supported by this Dictionary implementation.</summary>
	/// <param name="name">See Remarks.</param>
	/// <param name="scopedElement">See Remarks.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases when this method is called.</exception>
	public void RegisterName(string name, object scopedElement)
	{
		throw new NotSupportedException(SR.NamesNotSupportedInsideResourceDictionary);
	}

	/// <summary>Not supported by this Dictionary implementation.</summary>
	/// <param name="name">See Remarks</param>
	public void UnregisterName(string name)
	{
	}

	/// <summary>Not supported by this Dictionary implementation.</summary>
	/// <returns>Always returns null.</returns>
	/// <param name="name">The name identifier for the object being requested.</param>
	public object FindName(string name)
	{
		return null;
	}

	private void SetValueWithoutLock(object key, object value)
	{
		if (IsReadOnly)
		{
			throw new InvalidOperationException(SR.ResourceDictionaryIsReadOnly);
		}
		if (_baseDictionary[key] != value)
		{
			ValidateDeferredResourceReferences(key);
			if (TraceResourceDictionary.IsEnabled)
			{
				TraceResourceDictionary.Trace(TraceEventType.Start, TraceResourceDictionary.AddResource, this, key, value);
			}
			_baseDictionary[key] = value;
			UpdateHasImplicitStyles(key);
			UpdateHasImplicitDataTemplates(key);
			NotifyOwners(new ResourcesChangeInfo(key));
			if (TraceResourceDictionary.IsEnabled)
			{
				TraceResourceDictionary.Trace(TraceEventType.Stop, TraceResourceDictionary.AddResource, this, key, value);
			}
		}
	}

	internal object GetValue(object key, out bool canCache)
	{
		if (CanBeAccessedAcrossThreads)
		{
			lock (((ICollection)this).SyncRoot)
			{
				return GetValueWithoutLock(key, out canCache);
			}
		}
		return GetValueWithoutLock(key, out canCache);
	}

	private object GetValueWithoutLock(object key, out bool canCache)
	{
		object value = _baseDictionary[key];
		if (value != null)
		{
			OnGettingValuePrivate(key, ref value, out canCache);
		}
		else
		{
			canCache = true;
			if (_mergedDictionaries != null)
			{
				for (int num = MergedDictionaries.Count - 1; num > -1; num--)
				{
					ResourceDictionary resourceDictionary = MergedDictionaries[num];
					if (resourceDictionary != null)
					{
						value = resourceDictionary.GetValue(key, out canCache);
						if (value != null)
						{
							break;
						}
					}
				}
			}
		}
		return value;
	}

	internal Type GetValueType(object key, out bool found)
	{
		found = false;
		Type result = null;
		object obj = _baseDictionary[key];
		if (obj != null)
		{
			found = true;
			result = ((!(obj is KeyRecord keyRecord)) ? obj.GetType() : GetTypeOfFirstObject(keyRecord));
		}
		else if (_mergedDictionaries != null)
		{
			for (int num = MergedDictionaries.Count - 1; num > -1; num--)
			{
				ResourceDictionary resourceDictionary = MergedDictionaries[num];
				if (resourceDictionary != null)
				{
					result = resourceDictionary.GetValueType(key, out found);
					if (found)
					{
						break;
					}
				}
			}
		}
		return result;
	}

	/// <summary>Adds a resource by key to this <see cref="T:System.Windows.ResourceDictionary" />. </summary>
	/// <param name="key">The name of the key to add.</param>
	/// <param name="value">The value of the resource to add.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.ResourceDictionary" /> is locked or read-only.</exception>
	/// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Hashtable" />. </exception>
	public void Add(object key, object value)
	{
		SealValue(value);
		if (CanBeAccessedAcrossThreads)
		{
			lock (((ICollection)this).SyncRoot)
			{
				AddWithoutLock(key, value);
				return;
			}
		}
		AddWithoutLock(key, value);
	}

	private void AddWithoutLock(object key, object value)
	{
		if (IsReadOnly)
		{
			throw new InvalidOperationException(SR.ResourceDictionaryIsReadOnly);
		}
		VisualDiagnostics.VerifyVisualTreeChange(InheritanceContext);
		if (TraceResourceDictionary.IsEnabled)
		{
			TraceResourceDictionary.Trace(TraceEventType.Start, TraceResourceDictionary.AddResource, this, key, value);
		}
		_baseDictionary.Add(key, value);
		UpdateHasImplicitStyles(key);
		UpdateHasImplicitDataTemplates(key);
		NotifyOwners(new ResourcesChangeInfo(key));
		if (TraceResourceDictionary.IsEnabled)
		{
			TraceResourceDictionary.Trace(TraceEventType.Stop, TraceResourceDictionary.AddResource, this, key, value);
		}
	}

	/// <summary>Clears all keys (and values) in the base <see cref="T:System.Windows.ResourceDictionary" />. This does not clear any merged dictionary items.</summary>
	public void Clear()
	{
		if (CanBeAccessedAcrossThreads)
		{
			lock (((ICollection)this).SyncRoot)
			{
				ClearWithoutLock();
				return;
			}
		}
		ClearWithoutLock();
	}

	private void ClearWithoutLock()
	{
		if (IsReadOnly)
		{
			throw new InvalidOperationException(SR.ResourceDictionaryIsReadOnly);
		}
		VisualDiagnostics.VerifyVisualTreeChange(InheritanceContext);
		if (Count > 0)
		{
			ValidateDeferredResourceReferences(null);
			RemoveInheritanceContextFromValues();
			_baseDictionary.Clear();
			NotifyOwners(ResourcesChangeInfo.CatastrophicDictionaryChangeInfo);
		}
	}

	/// <summary>Determines whether the <see cref="T:System.Windows.ResourceDictionary" /> contains an element with the specified key. </summary>
	/// <returns>true if <see cref="T:System.Windows.ResourceDictionary" /> contains a key-value pair with the specified key; otherwise, false.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Windows.ResourceDictionary" />.</param>
	public bool Contains(object key)
	{
		bool flag = _baseDictionary.Contains(key);
		if (flag && _baseDictionary[key] is KeyRecord item && _deferredLocationList.Contains(item))
		{
			return false;
		}
		if (_mergedDictionaries != null)
		{
			int num = MergedDictionaries.Count - 1;
			while (num > -1 && !flag)
			{
				ResourceDictionary resourceDictionary = MergedDictionaries[num];
				if (resourceDictionary != null)
				{
					flag = resourceDictionary.Contains(key);
				}
				num--;
			}
		}
		return flag;
	}

	private bool ContainsBamlObjectFactory(object key)
	{
		return GetBamlObjectFactory(key) != null;
	}

	private KeyRecord GetBamlObjectFactory(object key)
	{
		if (_baseDictionary.Contains(key))
		{
			return _baseDictionary[key] as KeyRecord;
		}
		if (_mergedDictionaries != null)
		{
			for (int num = MergedDictionaries.Count - 1; num > -1; num--)
			{
				ResourceDictionary resourceDictionary = MergedDictionaries[num];
				if (resourceDictionary != null)
				{
					KeyRecord bamlObjectFactory = resourceDictionary.GetBamlObjectFactory(key);
					if (bamlObjectFactory != null)
					{
						return bamlObjectFactory;
					}
				}
			}
		}
		return null;
	}

	/// <summary>Returns an <see cref="T:System.Collections.IDictionaryEnumerator" /> that can be used to iterate through the <see cref="T:System.Windows.ResourceDictionary" />. </summary>
	/// <returns>A specialized enumerator for the <see cref="T:System.Windows.ResourceDictionary" />.</returns>
	public IDictionaryEnumerator GetEnumerator()
	{
		return new ResourceDictionaryEnumerator(this);
	}

	/// <summary>Removes the entry with the specified key from the base dictionary. </summary>
	/// <param name="key">Key of the entry to remove.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.ResourceDictionary" /> is locked or read-only.</exception>
	public void Remove(object key)
	{
		if (CanBeAccessedAcrossThreads)
		{
			lock (((ICollection)this).SyncRoot)
			{
				RemoveWithoutLock(key);
				return;
			}
		}
		RemoveWithoutLock(key);
	}

	private void RemoveWithoutLock(object key)
	{
		if (IsReadOnly)
		{
			throw new InvalidOperationException(SR.ResourceDictionaryIsReadOnly);
		}
		VisualDiagnostics.VerifyVisualTreeChange(InheritanceContext);
		ValidateDeferredResourceReferences(key);
		RemoveInheritanceContext(_baseDictionary[key]);
		_baseDictionary.Remove(key);
		NotifyOwners(new ResourcesChangeInfo(key));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" />.</param>
	/// <param name="arrayIndex">The first position in the specified <see cref="T:System.Array" /> to receive the copied contents.</param>
	void ICollection.CopyTo(Array array, int arrayIndex)
	{
		CopyTo(array as DictionaryEntry[], arrayIndex);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IDictionary)this).GetEnumerator();
	}

	/// <summary>Begins the initialization phase for this <see cref="T:System.Windows.ResourceDictionary" />. </summary>
	/// <exception cref="T:System.InvalidOperationException">Called <see cref="M:System.Windows.ResourceDictionary.BeginInit" /> more than once before <see cref="M:System.Windows.ResourceDictionary.EndInit" /> was called.</exception>
	public void BeginInit()
	{
		if (IsInitializePending)
		{
			throw new InvalidOperationException(SR.NestedBeginInitNotSupported);
		}
		IsInitializePending = true;
		IsInitialized = false;
	}

	/// <summary>Ends the initialization phase, and invalidates the previous tree such that all changes made to keys during the initialization phase can be accounted for. </summary>
	public void EndInit()
	{
		if (!IsInitializePending)
		{
			throw new InvalidOperationException(SR.EndInitWithoutBeginInitNotSupported);
		}
		IsInitializePending = false;
		IsInitialized = true;
		NotifyOwners(new ResourcesChangeInfo(null, this));
	}

	private bool CanCache(KeyRecord keyRecord, object value)
	{
		if (keyRecord.SharedSet)
		{
			return keyRecord.Shared;
		}
		return true;
	}

	private void OnGettingValuePrivate(object key, ref object value, out bool canCache)
	{
		ResourceDictionaryDiagnostics.RecordLookupResult(key, this);
		OnGettingValue(key, ref value, out canCache);
		if (((key != null) & canCache) && !object.Equals(_baseDictionary[key], value))
		{
			if (InheritanceContext != null)
			{
				AddInheritanceContext(InheritanceContext, value);
			}
			_baseDictionary[key] = value;
		}
	}

	/// <summary>Occurs when the <see cref="T:System.Windows.ResourceDictionary" /> receives a request for a resource.</summary>
	/// <param name="key">The key of the resource to get.</param>
	/// <param name="value">The value of the requested resource.</param>
	/// <param name="canCache">true if the resource can be saved and used later; otherwise, false.</param>
	protected virtual void OnGettingValue(object key, ref object value, out bool canCache)
	{
		if (!(value is KeyRecord keyRecord))
		{
			canCache = true;
			return;
		}
		if (_deferredLocationList.Contains(keyRecord))
		{
			canCache = false;
			value = null;
			return;
		}
		_deferredLocationList.Add(keyRecord);
		try
		{
			if (TraceResourceDictionary.IsEnabled)
			{
				TraceResourceDictionary.Trace(TraceEventType.Start, TraceResourceDictionary.RealizeDeferContent, this, key, value);
			}
			value = CreateObject(keyRecord);
		}
		finally
		{
			if (TraceResourceDictionary.IsEnabled)
			{
				TraceResourceDictionary.Trace(TraceEventType.Stop, TraceResourceDictionary.RealizeDeferContent, this, key, value);
			}
		}
		_deferredLocationList.Remove(keyRecord);
		if (key != null)
		{
			canCache = CanCache(keyRecord, value);
			if (canCache)
			{
				SealValue(value);
				_numDefer--;
				if (_numDefer == 0)
				{
					CloseReader();
				}
			}
		}
		else
		{
			canCache = true;
		}
	}

	private void SetDeferrableContent(DeferrableContent deferrableContent)
	{
		Baml2006ReaderSettings baml2006ReaderSettings = new Baml2006ReaderSettings(deferrableContent.SchemaContext.Settings);
		baml2006ReaderSettings.IsBamlFragment = true;
		baml2006ReaderSettings.OwnsStream = true;
		baml2006ReaderSettings.BaseUri = null;
		Baml2006Reader baml2006Reader = new Baml2006Reader(deferrableContent.Stream, deferrableContent.SchemaContext, baml2006ReaderSettings);
		_objectWriterFactory = deferrableContent.ObjectWriterFactory;
		_objectWriterSettings = deferrableContent.ObjectWriterParentSettings;
		_deferredLocationList = new List<KeyRecord>();
		_rootElement = deferrableContent.RootObject;
		IList<KeyRecord> list = baml2006Reader.ReadKeys();
		if (_source == null)
		{
			if (_reader != null)
			{
				throw new InvalidOperationException(SR.ResourceDictionaryDuplicateDeferredContent);
			}
			_reader = baml2006Reader;
			SetKeys(list, deferrableContent.ServiceProvider);
		}
		else if (list.Count > 0)
		{
			throw new InvalidOperationException(SR.ResourceDictionaryDeferredContentFailure);
		}
	}

	private object GetKeyValue(KeyRecord key, IServiceProvider serviceProvider)
	{
		if (key.KeyString != null)
		{
			return key.KeyString;
		}
		if (key.KeyType != null)
		{
			return key.KeyType;
		}
		System.Xaml.XamlReader reader = key.KeyNodeList.GetReader();
		return EvaluateMarkupExtensionNodeList(reader, serviceProvider);
	}

	private object EvaluateMarkupExtensionNodeList(System.Xaml.XamlReader reader, IServiceProvider serviceProvider)
	{
		XamlObjectWriter xamlObjectWriter = _objectWriterFactory.GetXamlObjectWriter(null);
		XamlServices.Transform(reader, xamlObjectWriter);
		object obj = xamlObjectWriter.Result;
		if (obj is MarkupExtension markupExtension)
		{
			obj = markupExtension.ProvideValue(serviceProvider);
		}
		return obj;
	}

	private object GetStaticResourceKeyValue(StaticResource staticResource, IServiceProvider serviceProvider)
	{
		System.Xaml.XamlReader reader = staticResource.ResourceNodeList.GetReader();
		XamlType xamlType = reader.SchemaContext.GetXamlType(typeof(StaticResourceExtension));
		XamlMember member = xamlType.GetMember("ResourceKey");
		reader.Read();
		if (reader.NodeType == System.Xaml.XamlNodeType.StartObject && reader.Type == xamlType)
		{
			reader.Read();
			while (reader.NodeType == System.Xaml.XamlNodeType.StartMember && reader.Member != XamlLanguage.PositionalParameters && reader.Member != member)
			{
				reader.Skip();
			}
			if (reader.NodeType == System.Xaml.XamlNodeType.StartMember)
			{
				object result = null;
				reader.Read();
				if (reader.NodeType == System.Xaml.XamlNodeType.StartObject)
				{
					System.Xaml.XamlReader reader2 = reader.ReadSubtree();
					result = EvaluateMarkupExtensionNodeList(reader2, serviceProvider);
				}
				else if (reader.NodeType == System.Xaml.XamlNodeType.Value)
				{
					result = reader.Value;
				}
				return result;
			}
		}
		return null;
	}

	private void SetKeys(IList<KeyRecord> keyCollection, IServiceProvider serviceProvider)
	{
		_numDefer = keyCollection.Count;
		StaticResourceExtension staticResourceWorker = new StaticResourceExtension();
		for (int i = 0; i < keyCollection.Count; i++)
		{
			KeyRecord keyRecord = keyCollection[i];
			if (keyRecord != null)
			{
				object keyValue = GetKeyValue(keyRecord, serviceProvider);
				UpdateHasImplicitStyles(keyValue);
				UpdateHasImplicitDataTemplates(keyValue);
				if (keyRecord != null && keyRecord.HasStaticResources)
				{
					SetOptimizedStaticResources(keyRecord.StaticResources, serviceProvider, staticResourceWorker);
				}
				_baseDictionary.Add(keyValue, keyRecord);
				if (TraceResourceDictionary.IsEnabled)
				{
					TraceResourceDictionary.TraceActivityItem(TraceResourceDictionary.SetKey, this, keyValue);
				}
				continue;
			}
			throw new ArgumentException(SR.KeyCollectionHasInvalidKey);
		}
		NotifyOwners(new ResourcesChangeInfo(null, this));
	}

	private void SetOptimizedStaticResources(IList<object> staticResources, IServiceProvider serviceProvider, StaticResourceExtension staticResourceWorker)
	{
		for (int i = 0; i < staticResources.Count; i++)
		{
			object obj = null;
			if (staticResources[i] is OptimizedStaticResource optimizedStaticResource)
			{
				obj = optimizedStaticResource.KeyValue;
			}
			else
			{
				if (!(staticResources[i] is StaticResource staticResource))
				{
					continue;
				}
				obj = GetStaticResourceKeyValue(staticResource, serviceProvider);
			}
			staticResourceWorker.ResourceKey = obj;
			object obj2 = staticResourceWorker.TryProvideValueInternal(serviceProvider, allowDeferredReference: true, mustReturnDeferredResourceReference: true);
			staticResources[i] = new StaticResourceHolder(obj, obj2 as DeferredResourceReference);
		}
	}

	private Type GetTypeOfFirstObject(KeyRecord keyRecord)
	{
		return _reader.GetTypeOfFirstStartObject(keyRecord) ?? typeof(string);
	}

	private object CreateObject(KeyRecord key)
	{
		System.Xaml.XamlReader xamlReader = _reader.ReadObject(key);
		if (xamlReader == null)
		{
			return null;
		}
		Uri baseUri = ((_rootElement is IUriContext) ? ((IUriContext)_rootElement).BaseUri : _baseUri);
		return WpfXamlLoader.LoadDeferredContent(xamlReader, _objectWriterFactory, skipJournaledProperties: false, _rootElement, _objectWriterSettings, baseUri);
	}

	internal object Lookup(object key, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference, bool canCacheAsThemeResource)
	{
		bool canCache;
		if (allowDeferredResourceReference)
		{
			return FetchResource(key, allowDeferredResourceReference, mustReturnDeferredResourceReference, canCacheAsThemeResource, out canCache);
		}
		if (!mustReturnDeferredResourceReference)
		{
			return this[key];
		}
		return new DeferredResourceReferenceHolder(key, this[key]);
	}

	internal void AddOwner(DispatcherObject owner)
	{
		if (_inheritanceContext == null)
		{
			if (owner is DependencyObject target)
			{
				_inheritanceContext = new WeakReference(target);
				AddInheritanceContextToValues();
			}
			else
			{
				_inheritanceContext = new WeakReference(DummyInheritanceContext);
			}
		}
		if (owner is FrameworkElement frameworkElement)
		{
			if (_ownerFEs == null)
			{
				_ownerFEs = new WeakReferenceList(1);
			}
			else if (_ownerFEs.Contains(frameworkElement) && ContainsCycle(this))
			{
				throw new InvalidOperationException(SR.ResourceDictionaryInvalidMergedDictionary);
			}
			if (HasImplicitStyles)
			{
				frameworkElement.ShouldLookupImplicitStyles = true;
			}
			_ownerFEs.Add(frameworkElement);
		}
		else if (owner is FrameworkContentElement frameworkContentElement)
		{
			if (_ownerFCEs == null)
			{
				_ownerFCEs = new WeakReferenceList(1);
			}
			else if (_ownerFCEs.Contains(frameworkContentElement) && ContainsCycle(this))
			{
				throw new InvalidOperationException(SR.ResourceDictionaryInvalidMergedDictionary);
			}
			if (HasImplicitStyles)
			{
				frameworkContentElement.ShouldLookupImplicitStyles = true;
			}
			_ownerFCEs.Add(frameworkContentElement);
		}
		else if (owner is Application application)
		{
			if (_ownerApps == null)
			{
				_ownerApps = new WeakReferenceList(1);
			}
			else if (_ownerApps.Contains(application) && ContainsCycle(this))
			{
				throw new InvalidOperationException(SR.ResourceDictionaryInvalidMergedDictionary);
			}
			if (HasImplicitStyles)
			{
				application.HasImplicitStylesInResources = true;
			}
			_ownerApps.Add(application);
			CanBeAccessedAcrossThreads = true;
			SealValues();
		}
		AddOwnerToAllMergedDictionaries(owner);
		TryInitialize();
	}

	internal void RemoveOwner(DispatcherObject owner)
	{
		if (owner is FrameworkElement obj)
		{
			if (_ownerFEs != null)
			{
				_ownerFEs.Remove(obj);
				if (_ownerFEs.Count == 0)
				{
					_ownerFEs = null;
				}
			}
		}
		else if (owner is FrameworkContentElement obj2)
		{
			if (_ownerFCEs != null)
			{
				_ownerFCEs.Remove(obj2);
				if (_ownerFCEs.Count == 0)
				{
					_ownerFCEs = null;
				}
			}
		}
		else if (owner is Application obj3 && _ownerApps != null)
		{
			_ownerApps.Remove(obj3);
			if (_ownerApps.Count == 0)
			{
				_ownerApps = null;
			}
		}
		if (owner == InheritanceContext)
		{
			RemoveInheritanceContextFromValues();
			_inheritanceContext = null;
		}
		RemoveOwnerFromAllMergedDictionaries(owner);
	}

	internal bool ContainsOwner(DispatcherObject owner)
	{
		if (owner is FrameworkElement item)
		{
			if (_ownerFEs != null)
			{
				return _ownerFEs.Contains(item);
			}
			return false;
		}
		if (owner is FrameworkContentElement item2)
		{
			if (_ownerFCEs != null)
			{
				return _ownerFCEs.Contains(item2);
			}
			return false;
		}
		if (owner is Application item3)
		{
			if (_ownerApps != null)
			{
				return _ownerApps.Contains(item3);
			}
			return false;
		}
		return false;
	}

	private void TryInitialize()
	{
		if (!IsInitializePending && !IsInitialized)
		{
			IsInitialized = true;
		}
	}

	private void NotifyOwners(ResourcesChangeInfo info)
	{
		bool isInitialized = IsInitialized;
		bool flag = info.IsResourceAddOperation && HasImplicitStyles;
		if (isInitialized && InvalidatesImplicitDataTemplateResources)
		{
			info.SetIsImplicitDataTemplateChange();
		}
		if (!(isInitialized || flag))
		{
			return;
		}
		WeakReferenceListEnumerator enumerator;
		if (_ownerFEs != null)
		{
			enumerator = _ownerFEs.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is FrameworkElement frameworkElement)
				{
					if (flag)
					{
						frameworkElement.ShouldLookupImplicitStyles = true;
					}
					if (isInitialized)
					{
						TreeWalkHelper.InvalidateOnResourcesChange(frameworkElement, null, info);
					}
				}
			}
		}
		if (_ownerFCEs != null)
		{
			enumerator = _ownerFCEs.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is FrameworkContentElement frameworkContentElement)
				{
					if (flag)
					{
						frameworkContentElement.ShouldLookupImplicitStyles = true;
					}
					if (isInitialized)
					{
						TreeWalkHelper.InvalidateOnResourcesChange(null, frameworkContentElement, info);
					}
				}
			}
		}
		if (_ownerApps == null)
		{
			return;
		}
		enumerator = _ownerApps.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current is Application application)
			{
				if (flag)
				{
					application.HasImplicitStylesInResources = true;
				}
				if (isInitialized)
				{
					application.InvalidateResourceReferences(info);
				}
			}
		}
	}

	internal object FetchResource(object resourceKey, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference, out bool canCache)
	{
		return FetchResource(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference, canCacheAsThemeResource: true, out canCache);
	}

	private object FetchResource(object resourceKey, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference, bool canCacheAsThemeResource, out bool canCache)
	{
		if (allowDeferredResourceReference && (ContainsBamlObjectFactory(resourceKey) || (mustReturnDeferredResourceReference && Contains(resourceKey))))
		{
			canCache = false;
			DeferredResourceReference deferredResourceReference;
			if (!IsThemeDictionary)
			{
				deferredResourceReference = ((_ownerApps == null) ? new DeferredResourceReference(this, resourceKey) : new DeferredAppResourceReference(this, resourceKey));
				if (_deferredResourceReferences == null)
				{
					_deferredResourceReferences = new WeakReferenceList();
				}
				_deferredResourceReferences.Add(deferredResourceReference, skipFind: true);
			}
			else
			{
				deferredResourceReference = new DeferredThemeResourceReference(this, resourceKey, canCacheAsThemeResource);
			}
			ResourceDictionaryDiagnostics.RecordLookupResult(resourceKey, this);
			return deferredResourceReference;
		}
		return GetValue(resourceKey, out canCache);
	}

	private void ValidateDeferredResourceReferences(object resourceKey)
	{
		if (_deferredResourceReferences == null)
		{
			return;
		}
		WeakReferenceListEnumerator enumerator = _deferredResourceReferences.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current is DeferredResourceReference deferredResourceReference && (resourceKey == null || object.Equals(resourceKey, deferredResourceReference.Key)))
			{
				deferredResourceReference.GetValue(BaseValueSourceInternal.Unknown);
			}
		}
	}

	private void OnMergedDictionariesChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		List<ResourceDictionary> list = null;
		List<ResourceDictionary> list2 = null;
		ResourcesChangeInfo info;
		if (e.Action != NotifyCollectionChangedAction.Reset)
		{
			Invariant.Assert((e.NewItems != null && e.NewItems.Count > 0) || (e.OldItems != null && e.OldItems.Count > 0), "The NotifyCollectionChanged event fired when no dictionaries were added or removed");
			if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
			{
				list = new List<ResourceDictionary>(e.OldItems.Count);
				for (int i = 0; i < e.OldItems.Count; i++)
				{
					ResourceDictionary resourceDictionary = (ResourceDictionary)e.OldItems[i];
					list.Add(resourceDictionary);
					RemoveParentOwners(resourceDictionary);
				}
			}
			if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
			{
				list2 = new List<ResourceDictionary>(e.NewItems.Count);
				for (int j = 0; j < e.NewItems.Count; j++)
				{
					ResourceDictionary resourceDictionary = (ResourceDictionary)e.NewItems[j];
					list2.Add(resourceDictionary);
					if (!HasImplicitStyles && resourceDictionary.HasImplicitStyles)
					{
						HasImplicitStyles = true;
					}
					if (!HasImplicitDataTemplates && resourceDictionary.HasImplicitDataTemplates)
					{
						HasImplicitDataTemplates = true;
					}
					if (IsThemeDictionary)
					{
						resourceDictionary.IsThemeDictionary = true;
					}
					PropagateParentOwners(resourceDictionary);
				}
			}
			info = new ResourcesChangeInfo(list, list2, isStyleResourcesChange: false, isTemplateResourcesChange: false, null);
		}
		else
		{
			info = ResourcesChangeInfo.CatastrophicDictionaryChangeInfo;
		}
		NotifyOwners(info);
	}

	private void AddOwnerToAllMergedDictionaries(DispatcherObject owner)
	{
		if (_mergedDictionaries != null)
		{
			for (int i = 0; i < _mergedDictionaries.Count; i++)
			{
				_mergedDictionaries[i].AddOwner(owner);
			}
		}
	}

	private void RemoveOwnerFromAllMergedDictionaries(DispatcherObject owner)
	{
		if (_mergedDictionaries != null)
		{
			for (int i = 0; i < _mergedDictionaries.Count; i++)
			{
				_mergedDictionaries[i].RemoveOwner(owner);
			}
		}
	}

	private void PropagateParentOwners(ResourceDictionary mergedDictionary)
	{
		WeakReferenceListEnumerator enumerator;
		if (_ownerFEs != null)
		{
			Invariant.Assert(_ownerFEs.Count > 0);
			if (mergedDictionary._ownerFEs == null)
			{
				mergedDictionary._ownerFEs = new WeakReferenceList(_ownerFEs.Count);
			}
			enumerator = _ownerFEs.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is FrameworkElement owner)
				{
					mergedDictionary.AddOwner(owner);
				}
			}
		}
		if (_ownerFCEs != null)
		{
			Invariant.Assert(_ownerFCEs.Count > 0);
			if (mergedDictionary._ownerFCEs == null)
			{
				mergedDictionary._ownerFCEs = new WeakReferenceList(_ownerFCEs.Count);
			}
			enumerator = _ownerFCEs.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is FrameworkContentElement owner2)
				{
					mergedDictionary.AddOwner(owner2);
				}
			}
		}
		if (_ownerApps == null)
		{
			return;
		}
		Invariant.Assert(_ownerApps.Count > 0);
		if (mergedDictionary._ownerApps == null)
		{
			mergedDictionary._ownerApps = new WeakReferenceList(_ownerApps.Count);
		}
		enumerator = _ownerApps.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current is Application owner3)
			{
				mergedDictionary.AddOwner(owner3);
			}
		}
	}

	internal void RemoveParentOwners(ResourceDictionary mergedDictionary)
	{
		if (_ownerFEs != null)
		{
			WeakReferenceListEnumerator enumerator = _ownerFEs.GetEnumerator();
			while (enumerator.MoveNext())
			{
				FrameworkElement owner = enumerator.Current as FrameworkElement;
				mergedDictionary.RemoveOwner(owner);
			}
		}
		if (_ownerFCEs != null)
		{
			Invariant.Assert(_ownerFCEs.Count > 0);
			WeakReferenceListEnumerator enumerator = _ownerFCEs.GetEnumerator();
			while (enumerator.MoveNext())
			{
				FrameworkContentElement owner2 = enumerator.Current as FrameworkContentElement;
				mergedDictionary.RemoveOwner(owner2);
			}
		}
		if (_ownerApps != null)
		{
			Invariant.Assert(_ownerApps.Count > 0);
			WeakReferenceListEnumerator enumerator = _ownerApps.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Application owner3 = enumerator.Current as Application;
				mergedDictionary.RemoveOwner(owner3);
			}
		}
	}

	private bool ContainsCycle(ResourceDictionary origin)
	{
		for (int i = 0; i < MergedDictionaries.Count; i++)
		{
			ResourceDictionary resourceDictionary = MergedDictionaries[i];
			if (resourceDictionary == origin || resourceDictionary.ContainsCycle(origin))
			{
				return true;
			}
		}
		return false;
	}

	private void SealValues()
	{
		int count = _baseDictionary.Count;
		if (count > 0)
		{
			object[] array = new object[count];
			_baseDictionary.Values.CopyTo(array, 0);
			object[] array2 = array;
			foreach (object value in array2)
			{
				SealValue(value);
			}
		}
	}

	private void SealValue(object value)
	{
		DependencyObject inheritanceContext = InheritanceContext;
		if (inheritanceContext != null)
		{
			AddInheritanceContext(inheritanceContext, value);
		}
		if (IsThemeDictionary || _ownerApps != null || IsReadOnly)
		{
			StyleHelper.SealIfSealable(value);
		}
	}

	private void AddInheritanceContext(DependencyObject inheritanceContext, object value)
	{
		if (inheritanceContext.ProvideSelfAsInheritanceContext(value, VisualBrush.VisualProperty) && value is DependencyObject dependencyObject)
		{
			dependencyObject.IsInheritanceContextSealed = true;
		}
	}

	private void AddInheritanceContextToValues()
	{
		DependencyObject inheritanceContext = InheritanceContext;
		int count = _baseDictionary.Count;
		if (count > 0)
		{
			object[] array = new object[count];
			_baseDictionary.Values.CopyTo(array, 0);
			object[] array2 = array;
			foreach (object value in array2)
			{
				AddInheritanceContext(inheritanceContext, value);
			}
		}
	}

	private void RemoveInheritanceContext(object value)
	{
		DependencyObject dependencyObject = value as DependencyObject;
		DependencyObject inheritanceContext = InheritanceContext;
		if (dependencyObject != null && inheritanceContext != null && dependencyObject.IsInheritanceContextSealed && dependencyObject.InheritanceContext == inheritanceContext)
		{
			dependencyObject.IsInheritanceContextSealed = false;
			inheritanceContext.RemoveSelfAsInheritanceContext(dependencyObject, VisualBrush.VisualProperty);
		}
	}

	private void RemoveInheritanceContextFromValues()
	{
		foreach (object value in _baseDictionary.Values)
		{
			RemoveInheritanceContext(value);
		}
	}

	private void UpdateHasImplicitStyles(object key)
	{
		if (!HasImplicitStyles)
		{
			HasImplicitStyles = key as Type != null;
		}
	}

	private void UpdateHasImplicitDataTemplates(object key)
	{
		if (!HasImplicitDataTemplates)
		{
			HasImplicitDataTemplates = key is DataTemplateKey;
		}
	}

	private void WritePrivateFlag(PrivateFlags bit, bool value)
	{
		if (value)
		{
			_flags |= bit;
		}
		else
		{
			_flags &= (PrivateFlags)(byte)(~(int)bit);
		}
	}

	private bool ReadPrivateFlag(PrivateFlags bit)
	{
		return (_flags & bit) != 0;
	}

	private void CloseReader()
	{
		_reader.Close();
		_reader = null;
	}

	private void CopyDeferredContentFrom(ResourceDictionary loadedRD)
	{
		_buffer = loadedRD._buffer;
		_bamlStream = loadedRD._bamlStream;
		_startPosition = loadedRD._startPosition;
		_contentSize = loadedRD._contentSize;
		_objectWriterFactory = loadedRD._objectWriterFactory;
		_objectWriterSettings = loadedRD._objectWriterSettings;
		_rootElement = loadedRD._rootElement;
		_reader = loadedRD._reader;
		_numDefer = loadedRD._numDefer;
		_deferredLocationList = loadedRD._deferredLocationList;
		IsUnsafe = loadedRD.IsUnsafe;
	}

	private void MoveDeferredResourceReferencesFrom(ResourceDictionary loadedRD)
	{
		_deferredResourceReferences = loadedRD._deferredResourceReferences;
		if (_deferredResourceReferences != null)
		{
			WeakReferenceListEnumerator enumerator = _deferredResourceReferences.GetEnumerator();
			while (enumerator.MoveNext())
			{
				((DeferredResourceReference)enumerator.Current).Dictionary = this;
			}
		}
	}
}
