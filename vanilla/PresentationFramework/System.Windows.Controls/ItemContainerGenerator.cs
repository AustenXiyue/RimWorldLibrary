using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Controls;
using MS.Utility;

namespace System.Windows.Controls;

/// <summary>Generates the user interface (UI) on behalf of its host, such as an <see cref="T:System.Windows.Controls.ItemsControl" />.</summary>
public sealed class ItemContainerGenerator : IRecyclingItemContainerGenerator, IItemContainerGenerator, IWeakEventListener
{
	private class Generator : IDisposable
	{
		private ItemContainerGenerator _factory;

		private GeneratorDirection _direction;

		private bool _done;

		private GeneratorState _cachedState;

		internal Generator(ItemContainerGenerator factory, GeneratorPosition position, GeneratorDirection direction, bool allowStartAtRealizedItem)
		{
			_factory = factory;
			_direction = direction;
			_factory.MapChanged += OnMapChanged;
			_factory.MoveToPosition(position, direction, allowStartAtRealizedItem, ref _cachedState);
			_done = _factory.ItemsInternal.Count == 0;
			_factory.SetStatus(GeneratorStatus.GeneratingContainers);
		}

		public DependencyObject GenerateNext(bool stopAtRealized, out bool isNewlyRealized)
		{
			DependencyObject dependencyObject = null;
			isNewlyRealized = false;
			while (dependencyObject == null)
			{
				UnrealizedItemBlock unrealizedItemBlock = _cachedState.Block as UnrealizedItemBlock;
				IList itemsInternal = _factory.ItemsInternal;
				int itemIndex = _cachedState.ItemIndex;
				_ = _direction;
				if (_cachedState.Block == _factory._itemMap)
				{
					_done = true;
				}
				if (unrealizedItemBlock == null && stopAtRealized)
				{
					_done = true;
				}
				if (0 > itemIndex || itemIndex >= itemsInternal.Count)
				{
					_done = true;
				}
				if (_done)
				{
					isNewlyRealized = false;
					return null;
				}
				object obj = itemsInternal[itemIndex];
				if (unrealizedItemBlock != null)
				{
					isNewlyRealized = true;
					CollectionViewGroup collectionViewGroup = obj as CollectionViewGroup;
					bool flag = _factory._generatesGroupItems && collectionViewGroup == null;
					if (_factory._recyclableContainers.Count <= 0 || _factory.Host.IsItemItsOwnContainer(obj) || flag)
					{
						dependencyObject = ((collectionViewGroup != null && _factory.IsGrouping) ? _factory.ContainerForGroup(collectionViewGroup) : _factory.Host.GetContainerForItem(obj));
					}
					else
					{
						dependencyObject = _factory._recyclableContainers.Dequeue();
						isNewlyRealized = false;
					}
					if (dependencyObject != null)
					{
						LinkContainerToItem(dependencyObject, obj);
						_factory.Realize(unrealizedItemBlock, _cachedState.Offset, obj, dependencyObject);
						_factory.SetAlternationIndex(_cachedState.Block, _cachedState.Offset, _direction);
					}
				}
				else
				{
					isNewlyRealized = false;
					dependencyObject = ((RealizedItemBlock)_cachedState.Block).ContainerAt(_cachedState.Offset);
				}
				_cachedState.ItemIndex = itemIndex;
				if (_direction == GeneratorDirection.Forward)
				{
					_cachedState.Block.MoveForward(ref _cachedState, allowMovePastRealizedItem: true);
				}
				else
				{
					_cachedState.Block.MoveBackward(ref _cachedState, allowMovePastRealizedItem: true);
				}
			}
			return dependencyObject;
		}

		void IDisposable.Dispose()
		{
			if (_factory != null)
			{
				_factory.MapChanged -= OnMapChanged;
				_done = true;
				if (!_factory._isGeneratingBatches)
				{
					_factory.SetStatus(GeneratorStatus.ContainersGenerated);
				}
				_factory._generator = null;
				_factory = null;
			}
			GC.SuppressFinalize(this);
		}

		private void OnMapChanged(ItemBlock block, int offset, int count, ItemBlock newBlock, int newOffset, int deltaCount)
		{
			if (block != null)
			{
				if (block == _cachedState.Block && offset <= _cachedState.Offset && _cachedState.Offset < offset + count)
				{
					_cachedState.Block = newBlock;
					_cachedState.Offset += newOffset - offset;
					_cachedState.Count += deltaCount;
				}
			}
			else if (offset >= 0)
			{
				if (offset < _cachedState.Count || (offset == _cachedState.Count && newBlock != null && newBlock != _cachedState.Block))
				{
					_cachedState.Count += count;
					_cachedState.ItemIndex += count;
				}
				else if (offset < _cachedState.Count + _cachedState.Offset)
				{
					_cachedState.Offset += count;
					_cachedState.ItemIndex += count;
				}
				else if (offset == _cachedState.Count + _cachedState.Offset)
				{
					if (count > 0)
					{
						_cachedState.Offset += count;
						_cachedState.ItemIndex += count;
					}
					else if (_cachedState.Offset == _cachedState.Block.ItemCount)
					{
						_cachedState.Block = _cachedState.Block.Next;
						_cachedState.Offset = 0;
					}
				}
			}
			else
			{
				_cachedState.Block = newBlock;
				_cachedState.Offset += _cachedState.Count;
				_cachedState.Count = 0;
			}
		}
	}

	private class BatchGenerator : IDisposable
	{
		private ItemContainerGenerator _factory;

		public BatchGenerator(ItemContainerGenerator factory)
		{
			_factory = factory;
			_factory._isGeneratingBatches = true;
			_factory.SetStatus(GeneratorStatus.GeneratingContainers);
		}

		void IDisposable.Dispose()
		{
			if (_factory != null)
			{
				_factory._isGeneratingBatches = false;
				_factory.SetStatus(GeneratorStatus.ContainersGenerated);
				_factory = null;
			}
			GC.SuppressFinalize(this);
		}
	}

	private delegate void MapChangedHandler(ItemBlock block, int offset, int count, ItemBlock newBlock, int newOffset, int deltaCount);

	private class ItemBlock
	{
		public const int BlockSize = 16;

		private int _count;

		private ItemBlock _prev;

		private ItemBlock _next;

		public int ItemCount
		{
			get
			{
				return _count;
			}
			set
			{
				_count = value;
			}
		}

		public ItemBlock Prev
		{
			get
			{
				return _prev;
			}
			set
			{
				_prev = value;
			}
		}

		public ItemBlock Next
		{
			get
			{
				return _next;
			}
			set
			{
				_next = value;
			}
		}

		public virtual int ContainerCount => int.MaxValue;

		public virtual DependencyObject ContainerAt(int index)
		{
			return null;
		}

		public virtual object ItemAt(int index)
		{
			return null;
		}

		public void InsertAfter(ItemBlock prev)
		{
			Next = prev.Next;
			Prev = prev;
			Prev.Next = this;
			Next.Prev = this;
		}

		public void InsertBefore(ItemBlock next)
		{
			InsertAfter(next.Prev);
		}

		public void Remove()
		{
			Prev.Next = Next;
			Next.Prev = Prev;
		}

		public void MoveForward(ref GeneratorState state, bool allowMovePastRealizedItem)
		{
			if (IsMoveAllowed(allowMovePastRealizedItem))
			{
				state.ItemIndex++;
				if (++state.Offset >= ItemCount)
				{
					state.Block = Next;
					state.Offset = 0;
					state.Count += ItemCount;
				}
			}
		}

		public void MoveBackward(ref GeneratorState state, bool allowMovePastRealizedItem)
		{
			if (IsMoveAllowed(allowMovePastRealizedItem))
			{
				if (--state.Offset < 0)
				{
					state.Block = Prev;
					state.Offset = state.Block.ItemCount - 1;
					state.Count -= state.Block.ItemCount;
				}
				state.ItemIndex--;
			}
		}

		public int MoveForward(ref GeneratorState state, bool allowMovePastRealizedItem, int count)
		{
			if (IsMoveAllowed(allowMovePastRealizedItem))
			{
				if (count < ItemCount - state.Offset)
				{
					state.Offset += count;
				}
				else
				{
					count = ItemCount - state.Offset;
					state.Block = Next;
					state.Offset = 0;
					state.Count += ItemCount;
				}
				state.ItemIndex += count;
			}
			return count;
		}

		public int MoveBackward(ref GeneratorState state, bool allowMovePastRealizedItem, int count)
		{
			if (IsMoveAllowed(allowMovePastRealizedItem))
			{
				if (count <= state.Offset)
				{
					state.Offset -= count;
				}
				else
				{
					count = state.Offset + 1;
					state.Block = Prev;
					state.Offset = state.Block.ItemCount - 1;
					state.Count -= state.Block.ItemCount;
				}
				state.ItemIndex -= count;
			}
			return count;
		}

		protected virtual bool IsMoveAllowed(bool allowMovePastRealizedItem)
		{
			return allowMovePastRealizedItem;
		}
	}

	private class UnrealizedItemBlock : ItemBlock
	{
		public override int ContainerCount => 0;

		protected override bool IsMoveAllowed(bool allowMovePastRealizedItem)
		{
			return true;
		}
	}

	private class RealizedItemBlock : ItemBlock
	{
		private BlockEntry[] _entry = new BlockEntry[16];

		public override int ContainerCount => base.ItemCount;

		public override DependencyObject ContainerAt(int index)
		{
			return _entry[index].Container;
		}

		public override object ItemAt(int index)
		{
			return _entry[index].Item;
		}

		public void CopyEntries(RealizedItemBlock src, int offset, int count, int newOffset)
		{
			if (offset < newOffset)
			{
				for (int num = count - 1; num >= 0; num--)
				{
					_entry[newOffset + num] = src._entry[offset + num];
				}
				if (src != this)
				{
					src.ClearEntries(offset, count);
				}
				else
				{
					src.ClearEntries(offset, newOffset - offset);
				}
			}
			else
			{
				for (int num = 0; num < count; num++)
				{
					_entry[newOffset + num] = src._entry[offset + num];
				}
				if (src != this)
				{
					src.ClearEntries(offset, count);
				}
				else
				{
					src.ClearEntries(newOffset + count, offset - newOffset);
				}
			}
		}

		public void ClearEntries(int offset, int count)
		{
			for (int i = 0; i < count; i++)
			{
				_entry[offset + i].Item = null;
				_entry[offset + i].Container = null;
			}
		}

		public void RealizeItem(int index, object item, DependencyObject container)
		{
			_entry[index].Item = item;
			_entry[index].Container = container;
		}

		public int OffsetOfItem(object item)
		{
			for (int i = 0; i < base.ItemCount; i++)
			{
				if (ItemsControl.EqualsEx(_entry[i].Item, item))
				{
					return i;
				}
			}
			return -1;
		}
	}

	private struct BlockEntry
	{
		private object _item;

		private DependencyObject _container;

		public object Item
		{
			get
			{
				return _item;
			}
			set
			{
				_item = value;
			}
		}

		public DependencyObject Container
		{
			get
			{
				return _container;
			}
			set
			{
				_container = value;
			}
		}
	}

	private struct GeneratorState
	{
		private ItemBlock _block;

		private int _offset;

		private int _count;

		private int _itemIndex;

		public ItemBlock Block
		{
			get
			{
				return _block;
			}
			set
			{
				_block = value;
			}
		}

		public int Offset
		{
			get
			{
				return _offset;
			}
			set
			{
				_offset = value;
			}
		}

		public int Count
		{
			get
			{
				return _count;
			}
			set
			{
				_count = value;
			}
		}

		public int ItemIndex
		{
			get
			{
				return _itemIndex;
			}
			set
			{
				_itemIndex = value;
			}
		}
	}

	private class EmptyGroupItem : GroupItem
	{
		public void SetGenerator(ItemContainerGenerator generator)
		{
			base.Generator = generator;
			generator.ItemsChanged += OnItemsChanged;
		}

		private void OnItemsChanged(object sender, ItemsChangedEventArgs e)
		{
			CollectionViewGroup collectionViewGroup = (CollectionViewGroup)GetValue(ItemForItemContainerProperty);
			if (collectionViewGroup.ItemCount > 0)
			{
				ItemContainerGenerator generator = base.Generator;
				generator.ItemsChanged -= OnItemsChanged;
				generator.Parent.OnSubgroupBecameNonEmpty(this, collectionViewGroup);
			}
		}
	}

	internal static readonly DependencyProperty ItemForItemContainerProperty = DependencyProperty.RegisterAttached("ItemForItemContainer", typeof(object), typeof(ItemContainerGenerator), new FrameworkPropertyMetadata((object)null));

	private Generator _generator;

	private IGeneratorHost _host;

	private ItemBlock _itemMap;

	private GeneratorStatus _status;

	private int _itemsGenerated;

	private int _startIndexForUIFromItem;

	private DependencyObject _peer;

	private int _level;

	private IList _items;

	private ReadOnlyCollection<object> _itemsReadOnly;

	private GroupStyle _groupStyle;

	private ItemContainerGenerator _parent;

	private ArrayList _emptyGroupItems;

	private int _alternationCount;

	private Type _containerType;

	private Queue<DependencyObject> _recyclableContainers = new Queue<DependencyObject>();

	private bool _generatesGroupItems;

	private bool _isGeneratingBatches;

	/// <summary>The generation status of the <see cref="T:System.Windows.Controls.ItemContainerGenerator" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Controls.Primitives.GeneratorStatus" /> value that represents the generation status of the <see cref="T:System.Windows.Controls.ItemContainerGenerator" />.</returns>
	public GeneratorStatus Status => _status;

	/// <summary>Gets the collection of items that belong to this <see cref="T:System.Windows.Controls.ItemContainerGenerator" />.</summary>
	/// <returns>The collection of items that belong to this <see cref="T:System.Windows.Controls.ItemContainerGenerator" />.</returns>
	public ReadOnlyCollection<object> Items
	{
		get
		{
			if (_itemsReadOnly == null && _items != null)
			{
				_itemsReadOnly = new ReadOnlyCollection<object>(new ListOfObject(_items));
			}
			return _itemsReadOnly;
		}
	}

	internal IEnumerable RecyclableContainers => _recyclableContainers;

	internal ItemContainerGenerator Parent => _parent;

	internal int Level => _level;

	internal GroupStyle GroupStyle
	{
		get
		{
			return _groupStyle;
		}
		set
		{
			if (_groupStyle != value)
			{
				if (_groupStyle != null)
				{
					PropertyChangedEventManager.RemoveHandler(_groupStyle, OnGroupStylePropertyChanged, string.Empty);
				}
				_groupStyle = value;
				if (_groupStyle != null)
				{
					PropertyChangedEventManager.AddHandler(_groupStyle, OnGroupStylePropertyChanged, string.Empty);
				}
			}
		}
	}

	internal IList ItemsInternal
	{
		get
		{
			return _items;
		}
		set
		{
			if (_items != value)
			{
				INotifyCollectionChanged notifyCollectionChanged = _items as INotifyCollectionChanged;
				if (_items != Host.View && notifyCollectionChanged != null)
				{
					CollectionChangedEventManager.RemoveHandler(notifyCollectionChanged, OnCollectionChanged);
				}
				_items = value;
				_itemsReadOnly = null;
				notifyCollectionChanged = _items as INotifyCollectionChanged;
				if (_items != Host.View && notifyCollectionChanged != null)
				{
					CollectionChangedEventManager.AddHandler(notifyCollectionChanged, OnCollectionChanged);
				}
			}
		}
	}

	private IGeneratorHost Host => _host;

	private DependencyObject Peer => _peer;

	private bool IsGrouping => ItemsInternal != Host.View;

	/// <summary>The <see cref="E:System.Windows.Controls.ItemContainerGenerator.ItemsChanged" /> event is raised by a <see cref="T:System.Windows.Controls.ItemContainerGenerator" /> to inform layouts that the items collection has changed.</summary>
	public event ItemsChangedEventHandler ItemsChanged;

	/// <summary>The <see cref="E:System.Windows.Controls.ItemContainerGenerator.StatusChanged" /> event is raised by a <see cref="T:System.Windows.Controls.ItemContainerGenerator" /> to inform controls that its status has changed. </summary>
	public event EventHandler StatusChanged;

	internal event EventHandler PanelChanged;

	private event MapChangedHandler MapChanged;

	internal ItemContainerGenerator(IGeneratorHost host)
		: this(null, host, host as DependencyObject, 0)
	{
		CollectionChangedEventManager.AddHandler(host.View, OnCollectionChanged);
	}

	private ItemContainerGenerator(ItemContainerGenerator parent, GroupItem groupItem)
		: this(parent, parent.Host, groupItem, parent.Level + 1)
	{
	}

	private ItemContainerGenerator(ItemContainerGenerator parent, IGeneratorHost host, DependencyObject peer, int level)
	{
		_parent = parent;
		_host = host;
		_peer = peer;
		_level = level;
		OnRefresh();
	}

	private void SetStatus(GeneratorStatus value)
	{
		if (value == _status)
		{
			return;
		}
		_status = value;
		switch (_status)
		{
		case GeneratorStatus.GeneratingContainers:
			if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info))
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientStringBegin, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info, "ItemsControl.Generator");
				_itemsGenerated = 0;
			}
			else
			{
				_itemsGenerated = int.MinValue;
			}
			break;
		case GeneratorStatus.ContainersGenerated:
		{
			string text = null;
			if (_itemsGenerated >= 0)
			{
				if (Host is DependencyObject dependencyObject)
				{
					text = (string)dependencyObject.GetValue(FrameworkElement.NameProperty);
				}
				if (text == null || text.Length == 0)
				{
					text = Host.GetHashCode().ToString(CultureInfo.InvariantCulture);
				}
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientStringEnd, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info, string.Format(CultureInfo.InvariantCulture, "ItemContainerGenerator for {0} {1} - {2} items", Host.GetType().Name, text, _itemsGenerated));
			}
			break;
		}
		}
		if (this.StatusChanged != null)
		{
			this.StatusChanged(this, EventArgs.Empty);
		}
	}

	/// <summary>Returns the ItemContainerGenerator appropriate for use by the specified panel.</summary>
	/// <returns>An ItemContainerGenerator appropriate for use by the specified panel.</returns>
	/// <param name="panel">The panel for which to return an appropriate ItemContainerGenerator.</param>
	ItemContainerGenerator IItemContainerGenerator.GetItemContainerGeneratorForPanel(Panel panel)
	{
		if (!panel.IsItemsHost)
		{
			throw new ArgumentException(SR.PanelIsNotItemsHost, "panel");
		}
		ItemsPresenter itemsPresenter = ItemsPresenter.FromPanel(panel);
		if (itemsPresenter != null)
		{
			return itemsPresenter.Generator;
		}
		if (panel.TemplatedParent != null)
		{
			return this;
		}
		return null;
	}

	/// <summary>Prepares the generator to generate items, starting at the specified GeneratorPosition, and in the specified GeneratorDirection.</summary>
	/// <returns>An IDisposable object that tracks the lifetime of the generation process.</returns>
	/// <param name="position">A GeneratorPosition that specifies the position of the item to start generating items at.</param>
	/// <param name="direction">A GeneratorDirection that specifies the direction which to generate items.</param>
	IDisposable IItemContainerGenerator.StartAt(GeneratorPosition position, GeneratorDirection direction)
	{
		return ((IItemContainerGenerator)this).StartAt(position, direction, allowStartAtRealizedItem: false);
	}

	/// <summary>Prepares the generator to generate items, starting at the specified GeneratorPosition, and in the specified GeneratorDirection, and controlling whether or not to start at a generated (realized) item.</summary>
	/// <returns>An IDisposable object that tracks the lifetime of the generation process.</returns>
	/// <param name="position">A GeneratorPosition that specifies the position of the item to start generating items at.</param>
	/// <param name="direction">A GeneratorDirection that specifies the direction which to generate items.</param>
	/// <param name="allowStartAtRealizedItem">A Boolean that specifies whether to start at a generated (realized) item.</param>
	IDisposable IItemContainerGenerator.StartAt(GeneratorPosition position, GeneratorDirection direction, bool allowStartAtRealizedItem)
	{
		if (_generator != null)
		{
			throw new InvalidOperationException(SR.GenerationInProgress);
		}
		_generator = new Generator(this, position, direction, allowStartAtRealizedItem);
		return _generator;
	}

	/// <summary>Returns an object that manages the <see cref="P:System.Windows.Controls.ItemContainerGenerator.Status" /> property.</summary>
	/// <returns>An object that manages the <see cref="P:System.Windows.Controls.ItemContainerGenerator.Status" /> property.</returns>
	public IDisposable GenerateBatches()
	{
		if (_isGeneratingBatches)
		{
			throw new InvalidOperationException(SR.GenerationInProgress);
		}
		return new BatchGenerator(this);
	}

	/// <summary>Returns the container element used to display the next item.</summary>
	/// <returns>A DependencyObject that is the container element which is used to display the next item.</returns>
	DependencyObject IItemContainerGenerator.GenerateNext()
	{
		if (_generator == null)
		{
			throw new InvalidOperationException(SR.GenerationNotInProgress);
		}
		bool isNewlyRealized;
		return _generator.GenerateNext(stopAtRealized: true, out isNewlyRealized);
	}

	/// <summary>Returns the container element used to display the next item, and whether the container element has been newly generated (realized).</summary>
	/// <returns>A DependencyObject that is the container element which is used to display the next item.</returns>
	/// <param name="isNewlyRealized">Is true if the returned DependencyObject is newly generated (realized); otherwise, false.</param>
	DependencyObject IItemContainerGenerator.GenerateNext(out bool isNewlyRealized)
	{
		if (_generator == null)
		{
			throw new InvalidOperationException(SR.GenerationNotInProgress);
		}
		return _generator.GenerateNext(stopAtRealized: false, out isNewlyRealized);
	}

	/// <summary>Prepares the specified element as the container for the corresponding item.</summary>
	/// <param name="container">The container to prepare. Normally, container is the result of the previous call to GenerateNext.</param>
	void IItemContainerGenerator.PrepareItemContainer(DependencyObject container)
	{
		object item = container.ReadLocalValue(ItemForItemContainerProperty);
		Host.PrepareItemContainer(container, item);
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="position">Removes one or more generated (realized) items.</param>
	/// <param name="count">The Int32 number of elements to remove, starting at <paramref name="position" />.</param>
	void IItemContainerGenerator.Remove(GeneratorPosition position, int count)
	{
		Remove(position, count, isRecycling: false);
	}

	private void Remove(GeneratorPosition position, int count, bool isRecycling)
	{
		if (position.Offset != 0)
		{
			throw new ArgumentException(SR.Format(SR.RemoveRequiresOffsetZero, position.Index, position.Offset), "position");
		}
		if (count <= 0)
		{
			throw new ArgumentException(SR.Format(SR.RemoveRequiresPositiveCount, count), "count");
		}
		if (_itemMap == null)
		{
			return;
		}
		int index = position.Index;
		int num = index;
		ItemBlock next = _itemMap.Next;
		while (next != _itemMap && num >= next.ContainerCount)
		{
			num -= next.ContainerCount;
			next = next.Next;
		}
		RealizedItemBlock realizedItemBlock = next as RealizedItemBlock;
		int num2 = num + count - 1;
		while (next != _itemMap)
		{
			if (!(next is RealizedItemBlock))
			{
				throw new InvalidOperationException(SR.Format(SR.CannotRemoveUnrealizedItems, index, count));
			}
			if (num2 < next.ContainerCount)
			{
				break;
			}
			num2 -= next.ContainerCount;
			next = next.Next;
		}
		RealizedItemBlock realizedItemBlock2 = next as RealizedItemBlock;
		RealizedItemBlock realizedItemBlock3 = realizedItemBlock;
		int num3 = num;
		while (realizedItemBlock3 != realizedItemBlock2 || num3 <= num2)
		{
			DependencyObject dependencyObject = realizedItemBlock3.ContainerAt(num3);
			UnlinkContainerFromItem(dependencyObject, realizedItemBlock3.ItemAt(num3));
			bool flag = _generatesGroupItems && !(dependencyObject is GroupItem);
			if (isRecycling && !flag)
			{
				if (_containerType == null)
				{
					_containerType = dependencyObject.GetType();
				}
				else if (_containerType != dependencyObject.GetType())
				{
					throw new InvalidOperationException(SR.CannotRecyleHeterogeneousTypes);
				}
				_recyclableContainers.Enqueue(dependencyObject);
			}
			if (++num3 >= realizedItemBlock3.ContainerCount && realizedItemBlock3 != realizedItemBlock2)
			{
				realizedItemBlock3 = realizedItemBlock3.Next as RealizedItemBlock;
				num3 = 0;
			}
		}
		bool flag2 = num == 0;
		bool flag3 = num2 == realizedItemBlock2.ItemCount - 1;
		bool flag4 = flag2 && realizedItemBlock.Prev is UnrealizedItemBlock;
		bool flag5 = flag3 && realizedItemBlock2.Next is UnrealizedItemBlock;
		ItemBlock itemBlock = null;
		UnrealizedItemBlock unrealizedItemBlock;
		int num4;
		int num5;
		if (flag4)
		{
			unrealizedItemBlock = (UnrealizedItemBlock)realizedItemBlock.Prev;
			num4 = unrealizedItemBlock.ItemCount;
			num5 = -unrealizedItemBlock.ItemCount;
		}
		else if (flag5)
		{
			unrealizedItemBlock = (UnrealizedItemBlock)realizedItemBlock2.Next;
			num4 = 0;
			num5 = num;
		}
		else
		{
			unrealizedItemBlock = new UnrealizedItemBlock();
			num4 = 0;
			num5 = num;
			itemBlock = (flag2 ? realizedItemBlock.Prev : realizedItemBlock);
		}
		for (next = realizedItemBlock; next != realizedItemBlock2; next = next.Next)
		{
			int itemCount = next.ItemCount;
			MoveItems(next, num, itemCount - num, unrealizedItemBlock, num4, num5);
			num4 += itemCount - num;
			num = 0;
			num5 -= itemCount;
			if (next.ItemCount == 0)
			{
				next.Remove();
			}
		}
		int count2 = next.ItemCount - 1 - num2;
		MoveItems(next, num, num2 - num + 1, unrealizedItemBlock, num4, num5);
		RealizedItemBlock realizedItemBlock4 = realizedItemBlock2;
		if (!flag3)
		{
			if (realizedItemBlock == realizedItemBlock2 && !flag2)
			{
				realizedItemBlock4 = new RealizedItemBlock();
			}
			MoveItems(next, num2 + 1, count2, realizedItemBlock4, 0, num2 + 1);
		}
		if (itemBlock != null)
		{
			unrealizedItemBlock.InsertAfter(itemBlock);
		}
		if (realizedItemBlock4 != realizedItemBlock2)
		{
			realizedItemBlock4.InsertAfter(unrealizedItemBlock);
		}
		RemoveAndCoalesceBlocksIfNeeded(next);
	}

	/// <summary>Removes all generated (realized) items.</summary>
	void IItemContainerGenerator.RemoveAll()
	{
		RemoveAllInternal(saveRecycleQueue: false);
	}

	internal void RemoveAllInternal(bool saveRecycleQueue)
	{
		ItemBlock itemMap = _itemMap;
		_itemMap = null;
		try
		{
			if (itemMap == null)
			{
				return;
			}
			for (ItemBlock next = itemMap.Next; next != itemMap; next = next.Next)
			{
				if (next is RealizedItemBlock realizedItemBlock)
				{
					for (int i = 0; i < realizedItemBlock.ContainerCount; i++)
					{
						UnlinkContainerFromItem(realizedItemBlock.ContainerAt(i), realizedItemBlock.ItemAt(i));
					}
				}
			}
		}
		finally
		{
			PrepareGrouping();
			_itemMap = new ItemBlock();
			ItemBlock itemMap2 = _itemMap;
			ItemBlock prev = (_itemMap.Next = _itemMap);
			itemMap2.Prev = prev;
			UnrealizedItemBlock unrealizedItemBlock = new UnrealizedItemBlock();
			unrealizedItemBlock.InsertAfter(_itemMap);
			unrealizedItemBlock.ItemCount = ItemsInternal.Count;
			if (!saveRecycleQueue)
			{
				ResetRecyclableContainers();
			}
			SetAlternationCount();
			if (this.MapChanged != null)
			{
				this.MapChanged(null, -1, 0, unrealizedItemBlock, 0, 0);
			}
		}
	}

	private void ResetRecyclableContainers()
	{
		_recyclableContainers = new Queue<DependencyObject>();
		_containerType = null;
		_generatesGroupItems = false;
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="position">The index of the first element to reuse. <paramref name="position" /> must refer to a previously generated (realized) item.</param>
	/// <param name="count">The number of elements to reuse, starting at <paramref name="position" />.</param>
	void IRecyclingItemContainerGenerator.Recycle(GeneratorPosition position, int count)
	{
		Remove(position, count, isRecycling: true);
	}

	/// <summary>Returns the GeneratorPosition object that maps to the item at the specified index.</summary>
	/// <returns>A GeneratorPosition object that maps to the item at the specified index.</returns>
	/// <param name="itemIndex">The index of desired item. </param>
	GeneratorPosition IItemContainerGenerator.GeneratorPositionFromIndex(int itemIndex)
	{
		GetBlockAndPosition(itemIndex, out var position, out var block, out var _);
		if (block == _itemMap && position.Index == -1)
		{
			int offset = position.Offset + 1;
			position.Offset = offset;
		}
		return position;
	}

	/// <summary>Returns the index that maps to the specified GeneratorPosition.</summary>
	/// <returns>An Int32 that is the index which maps to the specified GeneratorPosition.</returns>
	/// <param name="position">The index of desired item.</param>
	int IItemContainerGenerator.IndexFromGeneratorPosition(GeneratorPosition position)
	{
		int num = position.Index;
		if (num == -1)
		{
			if (position.Offset >= 0)
			{
				return position.Offset - 1;
			}
			return ItemsInternal.Count + position.Offset;
		}
		if (_itemMap != null)
		{
			int num2 = 0;
			for (ItemBlock next = _itemMap.Next; next != _itemMap; next = next.Next)
			{
				if (num < next.ContainerCount)
				{
					return num2 + num + position.Offset;
				}
				num2 += next.ItemCount;
				num -= next.ContainerCount;
			}
		}
		return -1;
	}

	/// <summary>Returns the item that corresponds to the specified, generated <see cref="T:System.Windows.UIElement" />.</summary>
	/// <returns>A <see cref="T:System.Windows.DependencyObject" /> that is the item which corresponds to the specified, generated <see cref="T:System.Windows.UIElement" />. If the <see cref="T:System.Windows.UIElement" /> has not been generated, <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> is returned. </returns>
	/// <param name="container">The <see cref="T:System.Windows.DependencyObject" /> that corresponds to the item to be returned.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="container" /> is null.</exception>
	public object ItemFromContainer(DependencyObject container)
	{
		if (container == null)
		{
			throw new ArgumentNullException("container");
		}
		object obj = container.ReadLocalValue(ItemForItemContainerProperty);
		if (obj != DependencyProperty.UnsetValue && !Host.IsHostForItemContainer(container))
		{
			obj = DependencyProperty.UnsetValue;
		}
		return obj;
	}

	/// <summary>Returns the <see cref="T:System.Windows.UIElement" /> corresponding to the given item.</summary>
	/// <returns>A <see cref="T:System.Windows.UIElement" /> that corresponds to the given item. Returns null if the item does not belong to the item collection, or if a <see cref="T:System.Windows.UIElement" /> has not been generated for it.</returns>
	/// <param name="item">The <see cref="T:System.Object" /> item to find the <see cref="T:System.Windows.UIElement" /> for.</param>
	public DependencyObject ContainerFromItem(object item)
	{
		DoLinearSearch((object state, object o, DependencyObject d) => ItemsControl.EqualsEx(o, state), item, out var _, out var container, out var _, returnLocalIndex: false);
		return container;
	}

	/// <summary>Returns the index to an item that corresponds to the specified, generated <see cref="T:System.Windows.UIElement" />. </summary>
	/// <returns>An <see cref="T:System.Int32" /> index to an item that corresponds to the specified, generated <see cref="T:System.Windows.UIElement" /> or -1 if <paramref name="container" /> is not found. </returns>
	/// <param name="container">The <see cref="T:System.Windows.DependencyObject" /> that corresponds to the item to the index to be returned.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="container" /> is null.</exception>
	public int IndexFromContainer(DependencyObject container)
	{
		return IndexFromContainer(container, returnLocalIndex: false);
	}

	/// <summary>Returns the index to an item that corresponds to the specified, generated <see cref="T:System.Windows.UIElement" />, optionally recursively searching hierarchical items.</summary>
	/// <returns>An <see cref="T:System.Int32" /> index to an item that corresponds to the specified, generated <see cref="T:System.Windows.UIElement" /> or -1 if <paramref name="container" /> is not found.</returns>
	/// <param name="container">The <see cref="T:System.Windows.DependencyObject" /> that corresponds to the item to the index to be returned.</param>
	/// <param name="returnLocalIndex">true to search the current level of hierarchical items; false to recursively search hierarchical items.</param>
	public int IndexFromContainer(DependencyObject container, bool returnLocalIndex)
	{
		if (container == null)
		{
			throw new ArgumentNullException("container");
		}
		DoLinearSearch((DependencyObject state, object o, DependencyObject d) => d == state, container, out var _, out var _, out var itemIndex, returnLocalIndex);
		return itemIndex;
	}

	internal bool FindItem<TState>(Func<TState, object, DependencyObject, bool> match, TState matchState, out DependencyObject container, out int itemIndex)
	{
		object item;
		return DoLinearSearch(match, matchState, out item, out container, out itemIndex, returnLocalIndex: false);
	}

	private bool DoLinearSearch<TState>(Func<TState, object, DependencyObject, bool> match, TState matchState, out object item, out DependencyObject container, out int itemIndex, bool returnLocalIndex)
	{
		item = null;
		container = null;
		itemIndex = 0;
		if (_itemMap != null)
		{
			ItemBlock next = _itemMap.Next;
			int num = 0;
			while (num <= _startIndexForUIFromItem && next != _itemMap)
			{
				num += next.ItemCount;
				next = next.Next;
			}
			next = next.Prev;
			num -= next.ItemCount;
			RealizedItemBlock realizedItemBlock = next as RealizedItemBlock;
			int num2;
			if (realizedItemBlock != null)
			{
				num2 = _startIndexForUIFromItem - num;
				if (num2 >= realizedItemBlock.ItemCount)
				{
					num2 = 0;
				}
			}
			else
			{
				num2 = 0;
			}
			ItemBlock itemBlock = next;
			int i = num2;
			int num3 = next.ItemCount;
			while (true)
			{
				if (realizedItemBlock != null)
				{
					for (; i < num3; i++)
					{
						bool flag = match(matchState, realizedItemBlock.ItemAt(i), realizedItemBlock.ContainerAt(i));
						if (flag)
						{
							item = realizedItemBlock.ItemAt(i);
							container = realizedItemBlock.ContainerAt(i);
						}
						else if (!returnLocalIndex && IsGrouping && realizedItemBlock.ItemAt(i) is CollectionViewGroup)
						{
							flag = ((GroupItem)realizedItemBlock.ContainerAt(i)).Generator.DoLinearSearch(match, matchState, out item, out container, out var itemIndex2, returnLocalIndex: false);
							if (flag)
							{
								itemIndex = itemIndex2;
							}
						}
						if (flag)
						{
							_startIndexForUIFromItem = num + i;
							itemIndex += GetRealizedItemBlockCount(realizedItemBlock, i, returnLocalIndex) + GetCount(itemBlock, returnLocalIndex);
							return true;
						}
					}
					if (itemBlock == next && i == num2)
					{
						break;
					}
				}
				num += itemBlock.ItemCount;
				i = 0;
				itemBlock = itemBlock.Next;
				if (itemBlock == _itemMap)
				{
					itemBlock = itemBlock.Next;
					num = 0;
				}
				num3 = itemBlock.ItemCount;
				realizedItemBlock = itemBlock as RealizedItemBlock;
				if (itemBlock == next)
				{
					if (realizedItemBlock == null)
					{
						break;
					}
					num3 = num2;
				}
			}
		}
		itemIndex = -1;
		item = null;
		container = null;
		return false;
	}

	private int GetCount()
	{
		return GetCount(_itemMap);
	}

	private int GetCount(ItemBlock stop)
	{
		return GetCount(stop, returnLocalIndex: false);
	}

	private int GetCount(ItemBlock stop, bool returnLocalIndex)
	{
		if (_itemMap == null)
		{
			return 0;
		}
		int num = 0;
		for (ItemBlock next = _itemMap.Next; next != stop; next = next.Next)
		{
			num += next.ItemCount;
		}
		if (!returnLocalIndex && IsGrouping)
		{
			int num2 = num;
			num = 0;
			for (int i = 0; i < num2; i++)
			{
				num += (Items[i] as CollectionViewGroup)?.ItemCount ?? 1;
			}
		}
		return num;
	}

	private int GetRealizedItemBlockCount(RealizedItemBlock rib, int end, bool returnLocalIndex)
	{
		if (!IsGrouping || returnLocalIndex)
		{
			return end;
		}
		int num = 0;
		for (int i = 0; i < end; i++)
		{
			num = ((!(rib.ItemAt(i) is CollectionViewGroup collectionViewGroup)) ? (num + 1) : (num + collectionViewGroup.ItemCount));
		}
		return num;
	}

	/// <summary>Returns the element corresponding to the item at the given index within the <see cref="T:System.Windows.Controls.ItemCollection" />.</summary>
	/// <returns>Returns the element corresponding to the item at the given index within the <see cref="T:System.Windows.Controls.ItemCollection" /> or returns null if the item is not realized.</returns>
	/// <param name="index">The index of the desired item.</param>
	public DependencyObject ContainerFromIndex(int index)
	{
		if (_itemMap == null)
		{
			return null;
		}
		int num = 0;
		if (IsGrouping)
		{
			num = index;
			index = 0;
			int count = ItemsInternal.Count;
			while (index < count)
			{
				int num2 = ((!(ItemsInternal[index] is CollectionViewGroup collectionViewGroup)) ? 1 : collectionViewGroup.ItemCount);
				if (num < num2)
				{
					break;
				}
				num -= num2;
				index++;
			}
		}
		for (ItemBlock next = _itemMap.Next; next != _itemMap; next = next.Next)
		{
			if (index < next.ItemCount)
			{
				DependencyObject dependencyObject = next.ContainerAt(index);
				if (dependencyObject is GroupItem groupItem)
				{
					dependencyObject = groupItem.Generator.ContainerFromIndex(num);
				}
				return dependencyObject;
			}
			index -= next.ItemCount;
		}
		return null;
	}

	internal void Refresh()
	{
		OnRefresh();
	}

	internal void Release()
	{
		((IItemContainerGenerator)this).RemoveAll();
	}

	internal void Verify()
	{
		if (_itemMap == null)
		{
			return;
		}
		List<string> list = new List<string>();
		int num = 0;
		for (ItemBlock next = _itemMap.Next; next != _itemMap; next = next.Next)
		{
			num += next.ItemCount;
		}
		if (num != _items.Count)
		{
			list.Add(SR.Format(SR.Generator_CountIsWrong, num, _items.Count));
		}
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (ItemBlock next2 = _itemMap.Next; next2 != _itemMap; next2 = next2.Next)
		{
			if (next2 is RealizedItemBlock realizedItemBlock)
			{
				for (int i = 0; i < realizedItemBlock.ItemCount; i++)
				{
					int num5 = num4 + i;
					object obj = realizedItemBlock.ItemAt(i);
					object obj2 = ((num5 < _items.Count) ? _items[num5] : null);
					if (!ItemsControl.EqualsEx(obj, obj2))
					{
						if (num3 < 3)
						{
							list.Add(SR.Format(SR.Generator_ItemIsWrong, num5, obj, obj2));
							num3++;
						}
						num2++;
					}
				}
			}
			num4 += next2.ItemCount;
		}
		if (num2 > num3)
		{
			list.Add(SR.Format(SR.Generator_MoreErrors, num2 - num3));
		}
		if (list.Count <= 0)
		{
			return;
		}
		CultureInfo invariantEnglishUS = System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS;
		DependencyObject peer = Peer;
		string text = (string)peer.GetValue(FrameworkElement.NameProperty);
		if (string.IsNullOrWhiteSpace(text))
		{
			text = SR.Generator_Unnamed;
		}
		List<string> list2 = new List<string>();
		GetCollectionChangedSources(0, FormatCollectionChangedSource, list2);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(SR.Generator_Readme0);
		stringBuilder.Append(SR.Format(SR.Generator_Readme1, peer, text));
		stringBuilder.Append("  ");
		stringBuilder.AppendLine(SR.Generator_Readme2);
		foreach (string item in list)
		{
			stringBuilder.AppendFormat(invariantEnglishUS, "  {0}", item);
			stringBuilder.AppendLine();
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine(SR.Generator_Readme3);
		foreach (string item2 in list2)
		{
			stringBuilder.AppendFormat(invariantEnglishUS, "  {0}", item2);
			stringBuilder.AppendLine();
		}
		stringBuilder.AppendLine(SR.Generator_Readme4);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine(SR.Generator_Readme5);
		stringBuilder.AppendLine();
		stringBuilder.Append(SR.Generator_Readme6);
		stringBuilder.Append("  ");
		stringBuilder.Append(SR.Format(SR.Generator_Readme7, "PresentationTraceSources.TraceLevel", "High"));
		stringBuilder.Append("  ");
		stringBuilder.AppendLine(SR.Format(SR.Generator_Readme8, "System.Diagnostics.PresentationTraceSources.SetTraceLevel(myItemsControl.ItemContainerGenerator, System.Diagnostics.PresentationTraceLevel.High)"));
		stringBuilder.AppendLine(SR.Generator_Readme9);
		Exception innerException = new Exception(stringBuilder.ToString());
		throw new InvalidOperationException(SR.Generator_Inconsistent, innerException);
	}

	private void FormatCollectionChangedSource(int level, object source, bool? isLikely, List<string> sources)
	{
		Type type = source.GetType();
		if (!isLikely.HasValue)
		{
			isLikely = true;
			string assemblyQualifiedName = type.AssemblyQualifiedName;
			int num = assemblyQualifiedName.LastIndexOf("PublicKeyToken=");
			if (num >= 0)
			{
				ReadOnlySpan<char> span = assemblyQualifiedName.AsSpan(num + "PublicKeyToken=".Length);
				if (MemoryExtensions.Equals(span, "31bf3856ad364e35", StringComparison.OrdinalIgnoreCase) || MemoryExtensions.Equals(span, "b77a5c561934e089", StringComparison.OrdinalIgnoreCase))
				{
					isLikely = false;
				}
			}
		}
		char c = ((isLikely == true) ? '*' : ' ');
		string arg = new string(' ', level);
		sources.Add(string.Format(System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS, "{0} {1} {2}", c, arg, type.FullName));
	}

	private void GetCollectionChangedSources(int level, Action<int, object, bool?, List<string>> format, List<string> sources)
	{
		format(level, this, false, sources);
		Host.View.GetCollectionChangedSources(level + 1, format, sources);
	}

	internal void ChangeAlternationCount()
	{
		if (_itemMap == null)
		{
			return;
		}
		SetAlternationCount();
		if (!IsGrouping || GroupStyle == null)
		{
			return;
		}
		for (ItemBlock next = _itemMap.Next; next != _itemMap; next = next.Next)
		{
			for (int i = 0; i < next.ContainerCount; i++)
			{
				if (((RealizedItemBlock)next).ContainerAt(i) is GroupItem groupItem)
				{
					groupItem.Generator.ChangeAlternationCount();
				}
			}
		}
	}

	private void ChangeAlternationCount(int newAlternationCount)
	{
		if (_alternationCount == newAlternationCount)
		{
			return;
		}
		ItemBlock next = _itemMap.Next;
		int num = 0;
		while (num == next.ContainerCount)
		{
			next = next.Next;
		}
		if (next != _itemMap)
		{
			if (newAlternationCount > 0)
			{
				_alternationCount = newAlternationCount;
				SetAlternationIndex((RealizedItemBlock)next, num, GeneratorDirection.Forward);
			}
			else if (_alternationCount > 0)
			{
				while (next != _itemMap)
				{
					for (num = 0; num < next.ContainerCount; num++)
					{
						ItemsControl.ClearAlternationIndex(((RealizedItemBlock)next).ContainerAt(num));
					}
					next = next.Next;
				}
			}
		}
		_alternationCount = newAlternationCount;
	}

	internal void OnPanelChanged()
	{
		if (this.PanelChanged != null)
		{
			this.PanelChanged(this, EventArgs.Empty);
		}
	}

	private void MoveToPosition(GeneratorPosition position, GeneratorDirection direction, bool allowStartAtRealizedItem, ref GeneratorState state)
	{
		ItemBlock itemMap = _itemMap;
		if (itemMap == null)
		{
			return;
		}
		int num = 0;
		if (position.Index != -1)
		{
			int num2 = 0;
			int num3 = position.Index;
			itemMap = itemMap.Next;
			while (num3 >= itemMap.ContainerCount)
			{
				num2 += itemMap.ItemCount;
				num3 -= itemMap.ContainerCount;
				num += itemMap.ItemCount;
				itemMap = itemMap.Next;
			}
			state.Block = itemMap;
			state.Offset = num3;
			state.Count = num2;
			state.ItemIndex = num + num3;
		}
		else
		{
			state.Block = itemMap;
			state.Offset = 0;
			state.Count = 0;
			state.ItemIndex = num - 1;
		}
		int num4 = position.Offset;
		if (num4 == 0 && (!allowStartAtRealizedItem || state.Block == _itemMap))
		{
			num4 = ((direction == GeneratorDirection.Forward) ? 1 : (-1));
		}
		if (num4 > 0)
		{
			state.Block.MoveForward(ref state, allowMovePastRealizedItem: true);
			for (num4--; num4 > 0; num4 -= state.Block.MoveForward(ref state, allowStartAtRealizedItem, num4))
			{
			}
		}
		else if (num4 < 0)
		{
			if (state.Block == _itemMap)
			{
				int itemIndex = (state.Count = ItemsInternal.Count);
				state.ItemIndex = itemIndex;
			}
			state.Block.MoveBackward(ref state, allowMovePastRealizedItem: true);
			for (num4++; num4 < 0; num4 += state.Block.MoveBackward(ref state, allowStartAtRealizedItem, -num4))
			{
			}
		}
	}

	private void Realize(UnrealizedItemBlock block, int offset, object item, DependencyObject container)
	{
		RealizedItemBlock realizedItemBlock2;
		int num;
		if (offset == 0 && block.Prev is RealizedItemBlock { ItemCount: <16 } realizedItemBlock)
		{
			realizedItemBlock2 = realizedItemBlock;
			num = realizedItemBlock.ItemCount;
			MoveItems(block, offset, 1, realizedItemBlock2, num, -realizedItemBlock.ItemCount);
			MoveItems(block, 1, block.ItemCount, block, 0, 1);
		}
		else if (offset == block.ItemCount - 1 && block.Next is RealizedItemBlock { ItemCount: <16 } realizedItemBlock3)
		{
			realizedItemBlock2 = realizedItemBlock3;
			num = 0;
			MoveItems(realizedItemBlock2, 0, realizedItemBlock2.ItemCount, realizedItemBlock2, 1, -1);
			MoveItems(block, offset, 1, realizedItemBlock2, num, offset);
		}
		else
		{
			realizedItemBlock2 = new RealizedItemBlock();
			num = 0;
			if (offset == 0)
			{
				realizedItemBlock2.InsertBefore(block);
				MoveItems(block, offset, 1, realizedItemBlock2, num, 0);
				MoveItems(block, 1, block.ItemCount, block, 0, 1);
			}
			else if (offset == block.ItemCount - 1)
			{
				realizedItemBlock2.InsertAfter(block);
				MoveItems(block, offset, 1, realizedItemBlock2, num, offset);
			}
			else
			{
				UnrealizedItemBlock unrealizedItemBlock = new UnrealizedItemBlock();
				unrealizedItemBlock.InsertAfter(block);
				realizedItemBlock2.InsertAfter(block);
				MoveItems(block, offset + 1, block.ItemCount - offset - 1, unrealizedItemBlock, 0, offset + 1);
				MoveItems(block, offset, 1, realizedItemBlock2, 0, offset);
			}
		}
		RemoveAndCoalesceBlocksIfNeeded(block);
		realizedItemBlock2.RealizeItem(num, item, container);
	}

	private void RemoveAndCoalesceBlocksIfNeeded(ItemBlock block)
	{
		if (block != null && block != _itemMap && block.ItemCount == 0)
		{
			block.Remove();
			if (block.Prev is UnrealizedItemBlock && block.Next is UnrealizedItemBlock)
			{
				MoveItems(block.Next, 0, block.Next.ItemCount, block.Prev, block.Prev.ItemCount, -block.Prev.ItemCount - 1);
				block.Next.Remove();
			}
		}
	}

	private void MoveItems(ItemBlock block, int offset, int count, ItemBlock newBlock, int newOffset, int deltaCount)
	{
		RealizedItemBlock realizedItemBlock = block as RealizedItemBlock;
		RealizedItemBlock realizedItemBlock2 = newBlock as RealizedItemBlock;
		if (realizedItemBlock != null && realizedItemBlock2 != null)
		{
			realizedItemBlock2.CopyEntries(realizedItemBlock, offset, count, newOffset);
		}
		else if (realizedItemBlock != null && realizedItemBlock.ItemCount > count)
		{
			realizedItemBlock.ClearEntries(offset, count);
		}
		block.ItemCount -= count;
		newBlock.ItemCount += count;
		if (this.MapChanged != null)
		{
			this.MapChanged(block, offset, count, newBlock, newOffset, deltaCount);
		}
	}

	private void SetAlternationIndex(ItemBlock block, int offset, GeneratorDirection direction)
	{
		if (_alternationCount <= 0)
		{
			return;
		}
		int num;
		RealizedItemBlock realizedItemBlock;
		if (direction != GeneratorDirection.Backward)
		{
			offset--;
			while (offset < 0 || block is UnrealizedItemBlock)
			{
				block = block.Prev;
				offset = block.ContainerCount - 1;
			}
			realizedItemBlock = block as RealizedItemBlock;
			num = ((block == _itemMap) ? (-1) : ItemsControl.GetAlternationIndex(realizedItemBlock.ContainerAt(offset)));
			while (true)
			{
				for (offset++; offset == block.ContainerCount; offset = 0)
				{
					block = block.Next;
				}
				if (block != _itemMap)
				{
					num = (num + 1) % _alternationCount;
					realizedItemBlock = block as RealizedItemBlock;
					ItemsControl.SetAlternationIndex(realizedItemBlock.ContainerAt(offset), num);
					continue;
				}
				break;
			}
			return;
		}
		offset++;
		while (offset >= block.ContainerCount || block is UnrealizedItemBlock)
		{
			block = block.Next;
			offset = 0;
		}
		realizedItemBlock = block as RealizedItemBlock;
		num = ((block == _itemMap) ? 1 : ItemsControl.GetAlternationIndex(realizedItemBlock.ContainerAt(offset)));
		while (true)
		{
			for (offset--; offset < 0; offset = block.ContainerCount - 1)
			{
				block = block.Prev;
			}
			if (block != _itemMap)
			{
				num = (_alternationCount + num - 1) % _alternationCount;
				realizedItemBlock = block as RealizedItemBlock;
				ItemsControl.SetAlternationIndex(realizedItemBlock.ContainerAt(offset), num);
				continue;
			}
			break;
		}
	}

	private DependencyObject ContainerForGroup(CollectionViewGroup group)
	{
		_generatesGroupItems = true;
		if (!ShouldHide(group))
		{
			GroupItem groupItem = new GroupItem();
			LinkContainerToItem(groupItem, group);
			groupItem.Generator = new ItemContainerGenerator(this, groupItem);
			return groupItem;
		}
		AddEmptyGroupItem(group);
		return null;
	}

	private void PrepareGrouping()
	{
		GroupStyle groupStyle;
		IList list;
		if (Level == 0)
		{
			groupStyle = Host.GetGroupStyle(null, 0);
			if (groupStyle == null)
			{
				list = Host.View;
			}
			else
			{
				list = Host.View.CollectionView?.Groups;
				if (list == null)
				{
					list = Host.View;
					if (list.Count > 0)
					{
						groupStyle = null;
					}
				}
			}
		}
		else if (((GroupItem)Peer).ReadLocalValue(ItemForItemContainerProperty) is CollectionViewGroup collectionViewGroup)
		{
			groupStyle = ((!collectionViewGroup.IsBottomLevel) ? Host.GetGroupStyle(collectionViewGroup, Level) : null);
			list = collectionViewGroup.Items;
		}
		else
		{
			groupStyle = null;
			list = Host.View;
		}
		GroupStyle = groupStyle;
		ItemsInternal = list;
		if (Level == 0 && Host != null)
		{
			Host.SetIsGrouping(IsGrouping);
		}
	}

	private void SetAlternationCount()
	{
		int newAlternationCount = ((!IsGrouping || GroupStyle == null) ? Host.AlternationCount : (GroupStyle.IsAlternationCountSet ? GroupStyle.AlternationCount : ((_parent == null) ? Host.AlternationCount : _parent._alternationCount)));
		ChangeAlternationCount(newAlternationCount);
	}

	private bool ShouldHide(CollectionViewGroup group)
	{
		if (GroupStyle.HidesIfEmpty)
		{
			return group.ItemCount == 0;
		}
		return false;
	}

	private void AddEmptyGroupItem(CollectionViewGroup group)
	{
		EmptyGroupItem emptyGroupItem = new EmptyGroupItem();
		LinkContainerToItem(emptyGroupItem, group);
		emptyGroupItem.SetGenerator(new ItemContainerGenerator(this, emptyGroupItem));
		if (_emptyGroupItems == null)
		{
			_emptyGroupItems = new ArrayList();
		}
		_emptyGroupItems.Add(emptyGroupItem);
	}

	private void OnSubgroupBecameNonEmpty(EmptyGroupItem groupItem, CollectionViewGroup group)
	{
		UnlinkContainerFromItem(groupItem, group);
		if (_emptyGroupItems != null)
		{
			_emptyGroupItems.Remove(groupItem);
		}
		if (this.ItemsChanged != null)
		{
			GeneratorPosition position = PositionFromIndex(ItemsInternal.IndexOf(group));
			this.ItemsChanged(this, new ItemsChangedEventArgs(NotifyCollectionChangedAction.Add, position, 1, 0));
		}
	}

	private void OnSubgroupBecameEmpty(CollectionViewGroup group)
	{
		if (!ShouldHide(group))
		{
			return;
		}
		GeneratorPosition position = PositionFromIndex(ItemsInternal.IndexOf(group));
		if (position.Offset == 0 && position.Index >= 0)
		{
			((IItemContainerGenerator)this).Remove(position, 1);
			if (this.ItemsChanged != null)
			{
				this.ItemsChanged(this, new ItemsChangedEventArgs(NotifyCollectionChangedAction.Remove, position, 1, 1));
			}
			AddEmptyGroupItem(group);
		}
	}

	private GeneratorPosition PositionFromIndex(int itemIndex)
	{
		GetBlockAndPosition(itemIndex, out var position, out var _, out var _);
		return position;
	}

	private void GetBlockAndPosition(object item, int itemIndex, bool deletedFromItems, out GeneratorPosition position, out ItemBlock block, out int offsetFromBlockStart, out int correctIndex)
	{
		if (itemIndex >= 0)
		{
			GetBlockAndPosition(itemIndex, out position, out block, out offsetFromBlockStart);
			correctIndex = itemIndex;
		}
		else
		{
			GetBlockAndPosition(item, deletedFromItems, out position, out block, out offsetFromBlockStart, out correctIndex);
		}
	}

	private void GetBlockAndPosition(int itemIndex, out GeneratorPosition position, out ItemBlock block, out int offsetFromBlockStart)
	{
		position = new GeneratorPosition(-1, 0);
		block = null;
		offsetFromBlockStart = itemIndex;
		if (_itemMap == null || itemIndex < 0)
		{
			return;
		}
		int num = 0;
		block = _itemMap.Next;
		while (block != _itemMap)
		{
			if (offsetFromBlockStart >= block.ItemCount)
			{
				num += block.ContainerCount;
				offsetFromBlockStart -= block.ItemCount;
				block = block.Next;
				continue;
			}
			if (block.ContainerCount > 0)
			{
				position = new GeneratorPosition(num + offsetFromBlockStart, 0);
			}
			else
			{
				position = new GeneratorPosition(num - 1, offsetFromBlockStart + 1);
			}
			break;
		}
	}

	private void GetBlockAndPosition(object item, bool deletedFromItems, out GeneratorPosition position, out ItemBlock block, out int offsetFromBlockStart, out int correctIndex)
	{
		correctIndex = 0;
		int num = 0;
		offsetFromBlockStart = 0;
		int num2 = (deletedFromItems ? 1 : 0);
		position = new GeneratorPosition(-1, 0);
		if (_itemMap == null)
		{
			block = null;
			return;
		}
		for (block = _itemMap.Next; block != _itemMap; block = block.Next)
		{
			if (block is RealizedItemBlock realizedItemBlock)
			{
				offsetFromBlockStart = realizedItemBlock.OffsetOfItem(item);
				if (offsetFromBlockStart >= 0)
				{
					position = new GeneratorPosition(num + offsetFromBlockStart, 0);
					correctIndex += offsetFromBlockStart;
					break;
				}
			}
			else if (block is UnrealizedItemBlock)
			{
				bool flag = false;
				if (block.Next is RealizedItemBlock { ContainerCount: >0 } realizedItemBlock2)
				{
					flag = ItemsControl.EqualsEx(realizedItemBlock2.ItemAt(0), ItemsInternal[correctIndex + block.ItemCount - num2]);
				}
				else if (block.Next == _itemMap)
				{
					flag = block.Prev == _itemMap || ItemsInternal.Count == correctIndex + block.ItemCount - num2;
				}
				if (flag)
				{
					offsetFromBlockStart = 0;
					position = new GeneratorPosition(num - 1, 1);
					break;
				}
			}
			correctIndex += block.ItemCount;
			num += block.ContainerCount;
		}
		if (block != _itemMap)
		{
			return;
		}
		throw new InvalidOperationException(SR.CannotFindRemovedItem);
	}

	internal static void LinkContainerToItem(DependencyObject container, object item)
	{
		container.ClearValue(ItemForItemContainerProperty);
		container.SetValue(ItemForItemContainerProperty, item);
		if (container != item)
		{
			container.SetValue(FrameworkElement.DataContextProperty, item);
		}
	}

	private void UnlinkContainerFromItem(DependencyObject container, object item)
	{
		UnlinkContainerFromItem(container, item, _host);
	}

	internal static void UnlinkContainerFromItem(DependencyObject container, object item, IGeneratorHost host)
	{
		container.ClearValue(ItemForItemContainerProperty);
		host.ClearContainerForItem(container, item);
		if (container != item)
		{
			DependencyProperty dataContextProperty = FrameworkElement.DataContextProperty;
			container.SetValue(dataContextProperty, BindingExpressionBase.DisconnectedItem);
		}
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the listener handled the event.</returns>
	/// <param name="managerType">The type of the <see cref="T:System.Windows.WeakEventManager" /> calling this method.</param>
	/// <param name="sender">Object that originated the event.</param>
	/// <param name="e">Event data.</param>
	bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		return false;
	}

	private void OnGroupStylePropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == "Panel")
		{
			OnPanelChanged();
		}
		else
		{
			OnRefresh();
		}
	}

	private void ValidateAndCorrectIndex(object item, ref int index)
	{
		if (index < 0)
		{
			index = ItemsInternal.IndexOf(item);
			if (index < 0)
			{
				throw new InvalidOperationException(SR.Format(SR.CollectionAddEventMissingItem, item));
			}
		}
	}

	private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		if (sender != ItemsInternal && args.Action != NotifyCollectionChangedAction.Reset)
		{
			return;
		}
		switch (args.Action)
		{
		case NotifyCollectionChangedAction.Add:
			if (args.NewItems.Count != 1)
			{
				throw new NotSupportedException(SR.RangeActionsNotSupported);
			}
			OnItemAdded(args.NewItems[0], args.NewStartingIndex);
			break;
		case NotifyCollectionChangedAction.Remove:
			if (args.OldItems.Count != 1)
			{
				throw new NotSupportedException(SR.RangeActionsNotSupported);
			}
			OnItemRemoved(args.OldItems[0], args.OldStartingIndex);
			break;
		case NotifyCollectionChangedAction.Replace:
			if (!FrameworkCompatibilityPreferences.TargetsDesktop_V4_0 && args.OldItems.Count != 1)
			{
				throw new NotSupportedException(SR.RangeActionsNotSupported);
			}
			OnItemReplaced(args.OldItems[0], args.NewItems[0], args.NewStartingIndex);
			break;
		case NotifyCollectionChangedAction.Move:
			if (!FrameworkCompatibilityPreferences.TargetsDesktop_V4_0 && args.OldItems.Count != 1)
			{
				throw new NotSupportedException(SR.RangeActionsNotSupported);
			}
			OnItemMoved(args.OldItems[0], args.OldStartingIndex, args.NewStartingIndex);
			break;
		case NotifyCollectionChangedAction.Reset:
			OnRefresh();
			break;
		default:
			throw new NotSupportedException(SR.Format(SR.UnexpectedCollectionChangeAction, args.Action));
		}
		if (PresentationTraceSources.GetTraceLevel(this) >= PresentationTraceLevel.High)
		{
			Verify();
		}
	}

	private void OnItemAdded(object item, int index)
	{
		if (_itemMap == null)
		{
			return;
		}
		ValidateAndCorrectIndex(item, ref index);
		GeneratorPosition position = new GeneratorPosition(-1, 0);
		ItemBlock itemBlock = _itemMap.Next;
		int num = index;
		int num2 = 0;
		while (itemBlock != _itemMap && num >= itemBlock.ItemCount)
		{
			num -= itemBlock.ItemCount;
			position.Index += itemBlock.ContainerCount;
			num2 = ((itemBlock.ContainerCount <= 0) ? (num2 + itemBlock.ItemCount) : 0);
			itemBlock = itemBlock.Next;
		}
		position.Offset = num2 + num + 1;
		UnrealizedItemBlock unrealizedItemBlock = itemBlock as UnrealizedItemBlock;
		if (unrealizedItemBlock != null)
		{
			MoveItems(unrealizedItemBlock, num, 1, unrealizedItemBlock, num + 1, 0);
			UnrealizedItemBlock unrealizedItemBlock2 = unrealizedItemBlock;
			int itemCount = unrealizedItemBlock2.ItemCount + 1;
			unrealizedItemBlock2.ItemCount = itemCount;
		}
		else if ((num == 0 || itemBlock == _itemMap) && (unrealizedItemBlock = itemBlock.Prev as UnrealizedItemBlock) != null)
		{
			UnrealizedItemBlock unrealizedItemBlock3 = unrealizedItemBlock;
			int itemCount = unrealizedItemBlock3.ItemCount + 1;
			unrealizedItemBlock3.ItemCount = itemCount;
		}
		else
		{
			unrealizedItemBlock = new UnrealizedItemBlock();
			unrealizedItemBlock.ItemCount = 1;
			if (num > 0 && itemBlock is RealizedItemBlock realizedItemBlock)
			{
				RealizedItemBlock realizedItemBlock2 = new RealizedItemBlock();
				MoveItems(realizedItemBlock, num, realizedItemBlock.ItemCount - num, realizedItemBlock2, 0, num);
				realizedItemBlock2.InsertAfter(realizedItemBlock);
				position.Index += itemBlock.ContainerCount;
				position.Offset = 1;
				itemBlock = realizedItemBlock2;
			}
			unrealizedItemBlock.InsertBefore(itemBlock);
		}
		if (this.MapChanged != null)
		{
			this.MapChanged(null, index, 1, unrealizedItemBlock, 0, 0);
		}
		if (this.ItemsChanged != null)
		{
			this.ItemsChanged(this, new ItemsChangedEventArgs(NotifyCollectionChangedAction.Add, position, 1, 0));
		}
	}

	private void OnItemRemoved(object item, int itemIndex)
	{
		DependencyObject dependencyObject = null;
		int itemUICount = 0;
		GetBlockAndPosition(item, itemIndex, deletedFromItems: true, out var position, out var block, out var offsetFromBlockStart, out var _);
		RealizedItemBlock realizedItemBlock = block as RealizedItemBlock;
		if (realizedItemBlock != null)
		{
			itemUICount = 1;
			dependencyObject = realizedItemBlock.ContainerAt(offsetFromBlockStart);
		}
		MoveItems(block, offsetFromBlockStart + 1, block.ItemCount - offsetFromBlockStart - 1, block, offsetFromBlockStart, 0);
		ItemBlock itemBlock = block;
		int itemCount = itemBlock.ItemCount - 1;
		itemBlock.ItemCount = itemCount;
		if (realizedItemBlock != null)
		{
			SetAlternationIndex(block, offsetFromBlockStart, GeneratorDirection.Forward);
		}
		RemoveAndCoalesceBlocksIfNeeded(block);
		if (this.MapChanged != null)
		{
			this.MapChanged(null, itemIndex, -1, null, 0, 0);
		}
		if (this.ItemsChanged != null)
		{
			this.ItemsChanged(this, new ItemsChangedEventArgs(NotifyCollectionChangedAction.Remove, position, 1, itemUICount));
		}
		if (dependencyObject != null)
		{
			UnlinkContainerFromItem(dependencyObject, item);
		}
		if (Level > 0 && ItemsInternal.Count == 0 && ((GroupItem)Peer).ReadLocalValue(ItemForItemContainerProperty) is CollectionViewGroup group)
		{
			Parent.OnSubgroupBecameEmpty(group);
		}
	}

	private void OnItemReplaced(object oldItem, object newItem, int index)
	{
		GetBlockAndPosition(oldItem, index, deletedFromItems: false, out var position, out var block, out var offsetFromBlockStart, out var _);
		if (!(block is RealizedItemBlock realizedItemBlock))
		{
			return;
		}
		DependencyObject dependencyObject = realizedItemBlock.ContainerAt(offsetFromBlockStart);
		if (oldItem != dependencyObject && !_host.IsItemItsOwnContainer(newItem))
		{
			realizedItemBlock.RealizeItem(offsetFromBlockStart, newItem, dependencyObject);
			LinkContainerToItem(dependencyObject, newItem);
			_host.PrepareItemContainer(dependencyObject, newItem);
			return;
		}
		DependencyObject containerForItem = _host.GetContainerForItem(newItem);
		realizedItemBlock.RealizeItem(offsetFromBlockStart, newItem, containerForItem);
		LinkContainerToItem(containerForItem, newItem);
		if (this.ItemsChanged != null)
		{
			this.ItemsChanged(this, new ItemsChangedEventArgs(NotifyCollectionChangedAction.Replace, position, 1, 1));
		}
		UnlinkContainerFromItem(dependencyObject, oldItem);
	}

	private void OnItemMoved(object item, int oldIndex, int newIndex)
	{
		if (_itemMap == null)
		{
			return;
		}
		DependencyObject dependencyObject = null;
		int itemUICount = 0;
		GetBlockAndPosition(item, oldIndex, deletedFromItems: true, out var position, out var block, out var offsetFromBlockStart, out var _);
		GeneratorPosition oldPosition = position;
		if (block is RealizedItemBlock realizedItemBlock)
		{
			itemUICount = 1;
			dependencyObject = realizedItemBlock.ContainerAt(offsetFromBlockStart);
		}
		MoveItems(block, offsetFromBlockStart + 1, block.ItemCount - offsetFromBlockStart - 1, block, offsetFromBlockStart, 0);
		ItemBlock itemBlock = block;
		int itemCount = itemBlock.ItemCount - 1;
		itemBlock.ItemCount = itemCount;
		RemoveAndCoalesceBlocksIfNeeded(block);
		position = new GeneratorPosition(-1, 0);
		block = _itemMap.Next;
		offsetFromBlockStart = newIndex;
		while (block != _itemMap && offsetFromBlockStart >= block.ItemCount)
		{
			offsetFromBlockStart -= block.ItemCount;
			if (block.ContainerCount > 0)
			{
				position.Index += block.ContainerCount;
				position.Offset = 0;
			}
			else
			{
				position.Offset += block.ItemCount;
			}
			block = block.Next;
		}
		position.Offset += offsetFromBlockStart + 1;
		UnrealizedItemBlock unrealizedItemBlock = block as UnrealizedItemBlock;
		if (unrealizedItemBlock != null)
		{
			MoveItems(unrealizedItemBlock, offsetFromBlockStart, 1, unrealizedItemBlock, offsetFromBlockStart + 1, 0);
			UnrealizedItemBlock unrealizedItemBlock2 = unrealizedItemBlock;
			itemCount = unrealizedItemBlock2.ItemCount + 1;
			unrealizedItemBlock2.ItemCount = itemCount;
		}
		else if ((offsetFromBlockStart == 0 || block == _itemMap) && (unrealizedItemBlock = block.Prev as UnrealizedItemBlock) != null)
		{
			UnrealizedItemBlock unrealizedItemBlock3 = unrealizedItemBlock;
			itemCount = unrealizedItemBlock3.ItemCount + 1;
			unrealizedItemBlock3.ItemCount = itemCount;
		}
		else
		{
			unrealizedItemBlock = new UnrealizedItemBlock();
			unrealizedItemBlock.ItemCount = 1;
			if (offsetFromBlockStart > 0 && block is RealizedItemBlock realizedItemBlock2)
			{
				RealizedItemBlock realizedItemBlock3 = new RealizedItemBlock();
				MoveItems(realizedItemBlock2, offsetFromBlockStart, realizedItemBlock2.ItemCount - offsetFromBlockStart, realizedItemBlock3, 0, offsetFromBlockStart);
				realizedItemBlock3.InsertAfter(realizedItemBlock2);
				position.Index += block.ContainerCount;
				position.Offset = 1;
				offsetFromBlockStart = 0;
				block = realizedItemBlock3;
			}
			unrealizedItemBlock.InsertBefore(block);
		}
		DependencyObject parentInternal = VisualTreeHelper.GetParentInternal(dependencyObject);
		if (this.ItemsChanged != null)
		{
			this.ItemsChanged(this, new ItemsChangedEventArgs(NotifyCollectionChangedAction.Move, position, oldPosition, 1, itemUICount));
		}
		if (dependencyObject != null)
		{
			if (parentInternal == null || VisualTreeHelper.GetParentInternal(dependencyObject) != parentInternal)
			{
				UnlinkContainerFromItem(dependencyObject, item);
			}
			else
			{
				Realize(unrealizedItemBlock, offsetFromBlockStart, item, dependencyObject);
			}
		}
		if (_alternationCount > 0)
		{
			int itemIndex = Math.Min(oldIndex, newIndex);
			GetBlockAndPosition(itemIndex, out position, out block, out offsetFromBlockStart);
			SetAlternationIndex(block, offsetFromBlockStart, GeneratorDirection.Forward);
		}
	}

	private void OnRefresh()
	{
		((IItemContainerGenerator)this).RemoveAll();
		if (this.ItemsChanged != null)
		{
			GeneratorPosition position = new GeneratorPosition(0, 0);
			this.ItemsChanged(this, new ItemsChangedEventArgs(NotifyCollectionChangedAction.Reset, position, 0, 0));
		}
	}
}
