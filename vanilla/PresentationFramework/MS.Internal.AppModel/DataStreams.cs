using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;

namespace MS.Internal.AppModel;

[Serializable]
internal class DataStreams
{
	private delegate void NodeOperation(object node);

	[ThreadStatic]
	private static BinaryFormatter _formatter;

	private HybridDictionary _subStreams = new HybridDictionary(3);

	private HybridDictionary _customJournaledObjects;

	internal bool HasAnyData
	{
		get
		{
			if (_subStreams == null || _subStreams.Count <= 0)
			{
				if (_customJournaledObjects != null)
				{
					return _customJournaledObjects.Count > 0;
				}
				return false;
			}
			return true;
		}
	}

	private BinaryFormatter Formatter
	{
		get
		{
			if (_formatter == null)
			{
				_formatter = new BinaryFormatter();
			}
			return _formatter;
		}
	}

	internal DataStreams()
	{
	}

	private bool HasSubStreams(object key)
	{
		if (_subStreams != null)
		{
			return _subStreams.Contains(key);
		}
		return false;
	}

	private ArrayList GetSubStreams(object key)
	{
		ArrayList arrayList = (ArrayList)_subStreams[key];
		if (arrayList == null)
		{
			arrayList = new ArrayList(3);
			_subStreams[key] = arrayList;
		}
		return arrayList;
	}

	private ArrayList SaveSubStreams(UIElement element)
	{
		ArrayList arrayList = null;
		if (element != null && element.PersistId != 0)
		{
			LocalValueEnumerator localValueEnumerator = element.GetLocalValueEnumerator();
			while (localValueEnumerator.MoveNext())
			{
				LocalValueEntry current = localValueEnumerator.Current;
				if (current.Property.GetMetadata(element.DependencyObjectType) is FrameworkPropertyMetadata { Journal: not false } && !(current.Value is Expression) && current.Property != Frame.SourceProperty)
				{
					if (arrayList == null)
					{
						arrayList = new ArrayList(3);
					}
					object value = element.GetValue(current.Property);
					byte[] dataBytes = null;
					if (value != null && !(value is Uri))
					{
						MemoryStream memoryStream = new MemoryStream();
						Formatter.Serialize(memoryStream, value);
						dataBytes = memoryStream.ToArray();
						((IDisposable)memoryStream).Dispose();
					}
					arrayList.Add(new SubStream(current.Property.Name, dataBytes));
				}
			}
		}
		return arrayList;
	}

	private void SaveState(object node)
	{
		if (!(node is UIElement { PersistId: var persistId } uIElement) || persistId == 0)
		{
			return;
		}
		ArrayList arrayList = SaveSubStreams(uIElement);
		if (arrayList != null && !_subStreams.Contains(persistId))
		{
			_subStreams[persistId] = arrayList;
		}
		if (!(node is IJournalState journalState))
		{
			return;
		}
		object journalState2 = journalState.GetJournalState(JournalReason.NewContentNavigation);
		if (journalState2 != null)
		{
			if (_customJournaledObjects == null)
			{
				_customJournaledObjects = new HybridDictionary(2);
			}
			if (!_customJournaledObjects.Contains(persistId))
			{
				_customJournaledObjects[persistId] = journalState2;
			}
		}
	}

	internal void PrepareForSerialization()
	{
		if (_customJournaledObjects == null)
		{
			return;
		}
		foreach (DictionaryEntry customJournaledObject in _customJournaledObjects)
		{
			((CustomJournalStateInternal)customJournaledObject.Value).PrepareForSerialization();
		}
	}

	private void LoadSubStreams(UIElement element, ArrayList subStreams)
	{
		for (int i = 0; i < subStreams.Count; i++)
		{
			SubStream subStream = (SubStream)subStreams[i];
			DependencyProperty dependencyProperty = DependencyProperty.FromName(subStream._propertyName, element.GetType());
			if (dependencyProperty != null)
			{
				object value = null;
				if (subStream._data != null)
				{
					value = Formatter.Deserialize(new MemoryStream(subStream._data));
				}
				element.SetValue(dependencyProperty, value);
			}
		}
	}

	private void LoadState(object node)
	{
		if (!(node is UIElement { PersistId: var persistId } uIElement) || persistId == 0)
		{
			return;
		}
		if (HasSubStreams(persistId))
		{
			ArrayList subStreams = GetSubStreams(persistId);
			LoadSubStreams(uIElement, subStreams);
		}
		if (_customJournaledObjects != null && _customJournaledObjects.Contains(persistId))
		{
			CustomJournalStateInternal state = (CustomJournalStateInternal)_customJournaledObjects[persistId];
			if (node is IJournalState journalState)
			{
				journalState.RestoreJournalState(state);
			}
		}
	}

	private void WalkLogicalTree(object node, NodeOperation operation)
	{
		if (node != null)
		{
			operation(node);
		}
		if (!(node is DependencyObject current))
		{
			return;
		}
		IEnumerator enumerator = LogicalTreeHelper.GetChildren(current).GetEnumerator();
		if (enumerator != null)
		{
			while (enumerator.MoveNext())
			{
				WalkLogicalTree(enumerator.Current, operation);
			}
		}
	}

	internal void Save(object root)
	{
		if (_subStreams == null)
		{
			_subStreams = new HybridDictionary(3);
		}
		else
		{
			_subStreams.Clear();
		}
		WalkLogicalTree(root, SaveState);
	}

	internal void Load(object root)
	{
		if (HasAnyData)
		{
			WalkLogicalTree(root, LoadState);
		}
	}

	internal void Clear()
	{
		_subStreams = null;
		_customJournaledObjects = null;
	}
}
