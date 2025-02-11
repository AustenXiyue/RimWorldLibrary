using System.Collections;
using System.Globalization;
using MS.Internal;
using MS.Internal.WindowsBase;
using MS.Utility;

namespace System.Windows;

/// <summary>Defines an object that has a modifiable state and a read-only (frozen) state. Classes that derive from <see cref="T:System.Windows.Freezable" /> provide detailed change notification, can be made immutable, and can clone themselves. </summary>
public abstract class Freezable : DependencyObject, ISealable
{
	private class HandlerContextStorage
	{
		public object _handlerStorage;

		public object _contextStorage;
	}

	private struct FreezableContextPair
	{
		public readonly WeakReference Owner;

		public readonly DependencyProperty Property;

		public FreezableContextPair(DependencyObject dependObject, DependencyProperty dependProperty)
		{
			Owner = new WeakReference(dependObject);
			Property = dependProperty;
		}
	}

	private class EventStorage
	{
		private EventHandler[] _events;

		private int _logSize;

		private int _physSize;

		private bool _inUse;

		public int Count => _logSize;

		public int PhysicalSize => _physSize;

		public EventHandler this[int idx]
		{
			get
			{
				return _events[idx];
			}
			set
			{
				_events[idx] = value;
			}
		}

		public bool InUse
		{
			get
			{
				return _inUse;
			}
			set
			{
				_inUse = value;
			}
		}

		public EventStorage(int initialSize)
		{
			if (initialSize <= 0)
			{
				initialSize = 1;
			}
			_events = new EventHandler[initialSize];
			_logSize = 0;
			_physSize = initialSize;
			_inUse = false;
		}

		public void Add(EventHandler e)
		{
			if (_logSize == _physSize)
			{
				_physSize *= 2;
				EventHandler[] array = new EventHandler[_physSize];
				for (int i = 0; i < _logSize; i++)
				{
					array[i] = _events[i];
				}
				_events = array;
			}
			_events[_logSize] = e;
			_logSize++;
		}

		public void Clear()
		{
			_logSize = 0;
		}
	}

	[ThreadStatic]
	private static EventStorage _eventStorage;

	private DependencyProperty _property;

	private const int INITIAL_EVENTSTORAGE_SIZE = 4;

	/// <summary>Gets a value that indicates whether the object can be made unmodifiable. </summary>
	/// <returns>true if the current object can be made unmodifiable or is already unmodifiable; otherwise, false.</returns>
	public bool CanFreeze
	{
		get
		{
			if (!IsFrozenInternal)
			{
				return FreezeCore(isChecking: true);
			}
			return true;
		}
	}

	/// <summary>Gets a value that indicates whether the object is currently modifiable. </summary>
	/// <returns>true if the object is frozen and cannot be modified; false if the object can be modified.</returns>
	public bool IsFrozen
	{
		get
		{
			ReadPreamble();
			return IsFrozenInternal;
		}
	}

	internal bool IsFrozenInternal => base.Freezable_Frozen;

	private EventStorage CachedEventStorage
	{
		get
		{
			if (_eventStorage == null)
			{
				_eventStorage = new EventStorage(4);
			}
			return _eventStorage;
		}
	}

	bool ISealable.CanSeal => CanFreeze;

	bool ISealable.IsSealed => IsFrozen;

	private FrugalObjectList<FreezableContextPair> ContextList
	{
		get
		{
			if (HasHandlers)
			{
				return (FrugalObjectList<FreezableContextPair>)((HandlerContextStorage)_contextStorage)._contextStorage;
			}
			return (FrugalObjectList<FreezableContextPair>)_contextStorage;
		}
		set
		{
			if (HasHandlers)
			{
				((HandlerContextStorage)_contextStorage)._contextStorage = value;
			}
			else
			{
				_contextStorage = value;
			}
		}
	}

	private FrugalObjectList<EventHandler> HandlerList
	{
		get
		{
			if (HasContextInformation)
			{
				return (FrugalObjectList<EventHandler>)((HandlerContextStorage)_contextStorage)._handlerStorage;
			}
			return (FrugalObjectList<EventHandler>)_contextStorage;
		}
	}

	private EventHandler SingletonHandler
	{
		get
		{
			if (HasContextInformation)
			{
				return (EventHandler)((HandlerContextStorage)_contextStorage)._handlerStorage;
			}
			return (EventHandler)_contextStorage;
		}
	}

	private DependencyObject SingletonContext
	{
		get
		{
			if (HasHandlers)
			{
				return (DependencyObject)((HandlerContextStorage)_contextStorage)._contextStorage;
			}
			return (DependencyObject)_contextStorage;
		}
	}

	private DependencyProperty SingletonContextProperty => _property;

	private bool HasHandlers
	{
		get
		{
			if (!base.Freezable_UsingHandlerList)
			{
				return base.Freezable_UsingSingletonHandler;
			}
			return true;
		}
	}

	private bool HasContextInformation
	{
		get
		{
			if (!base.Freezable_UsingContextList)
			{
				return base.Freezable_UsingSingletonContext;
			}
			return true;
		}
	}

	internal override DependencyObject InheritanceContext
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			if (!base.Freezable_HasMultipleInheritanceContexts)
			{
				if (base.Freezable_UsingSingletonContext)
				{
					DependencyObject singletonContext = SingletonContext;
					if (singletonContext.CanBeInheritanceContext)
					{
						return singletonContext;
					}
				}
				else if (base.Freezable_UsingContextList)
				{
					FrugalObjectList<FreezableContextPair> contextList = ContextList;
					int count = contextList.Count;
					for (int i = 0; i < count; i++)
					{
						DependencyObject dependencyObject = (DependencyObject)contextList[i].Owner.Target;
						if (dependencyObject != null && dependencyObject.CanBeInheritanceContext)
						{
							return dependencyObject;
						}
					}
				}
			}
			return null;
		}
	}

	internal override bool HasMultipleInheritanceContexts
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			return base.Freezable_HasMultipleInheritanceContexts;
		}
	}

	/// <summary>Occurs when the <see cref="T:System.Windows.Freezable" /> or an object it contains is modified. </summary>
	public event EventHandler Changed
	{
		add
		{
			WritePreamble();
			if (value != null)
			{
				ChangedInternal += value;
			}
		}
		remove
		{
			WritePreamble();
			if (value != null)
			{
				ChangedInternal -= value;
			}
		}
	}

	internal event EventHandler ChangedInternal
	{
		add
		{
			HandlerAdd(value);
		}
		remove
		{
			HandlerRemove(value);
		}
	}

	/// <summary>Initializes a new instance of a <see cref="T:System.Windows.Freezable" /> derived class. </summary>
	protected Freezable()
	{
	}

	/// <summary>Creates a modifiable clone of the <see cref="T:System.Windows.Freezable" />, making deep copies of the object's values. When copying the object's dependency properties, this method copies expressions (which might no longer resolve) but not animations or their current values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public Freezable Clone()
	{
		ReadPreamble();
		Freezable freezable = CreateInstance();
		freezable.CloneCore(this);
		Debug_VerifyCloneCommon(this, freezable, isDeepClone: true);
		return freezable;
	}

	/// <summary>Creates a modifiable clone (deep copy) of the <see cref="T:System.Windows.Freezable" /> using its current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public Freezable CloneCurrentValue()
	{
		ReadPreamble();
		Freezable freezable = CreateInstance();
		freezable.CloneCurrentValueCore(this);
		Debug_VerifyCloneCommon(this, freezable, isDeepClone: true);
		return freezable;
	}

	/// <summary>Creates a frozen copy of the <see cref="T:System.Windows.Freezable" />, using base (non-animated) property values. Because the copy is frozen, any frozen sub-objects are copied by reference. </summary>
	/// <returns>A frozen copy of the <see cref="T:System.Windows.Freezable" />. The copy's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is set to true. </returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Freezable" /> cannot be frozen because it contains expressions or animated properties.</exception>
	public Freezable GetAsFrozen()
	{
		ReadPreamble();
		if (IsFrozenInternal)
		{
			return this;
		}
		Freezable freezable = CreateInstance();
		freezable.GetAsFrozenCore(this);
		Debug_VerifyCloneCommon(this, freezable, isDeepClone: false);
		freezable.Freeze();
		return freezable;
	}

	/// <summary>Creates a frozen copy of the <see cref="T:System.Windows.Freezable" /> using current property values. Because the copy is frozen, any frozen sub-objects are copied by reference.</summary>
	/// <returns>A frozen copy of the <see cref="T:System.Windows.Freezable" />. The copy's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is set to true.</returns>
	public Freezable GetCurrentValueAsFrozen()
	{
		ReadPreamble();
		if (IsFrozenInternal)
		{
			return this;
		}
		Freezable freezable = CreateInstance();
		freezable.GetCurrentValueAsFrozenCore(this);
		Debug_VerifyCloneCommon(this, freezable, isDeepClone: false);
		freezable.Freeze();
		return freezable;
	}

	/// <summary>Makes the current object unmodifiable and sets its <see cref="P:System.Windows.Freezable.IsFrozen" /> property to true. </summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Freezable" /> cannot be made unmodifiable. </exception>
	public void Freeze()
	{
		if (!CanFreeze)
		{
			throw new InvalidOperationException(SR.Freezable_CantFreeze);
		}
		Freeze(isChecking: false);
	}

	/// <summary>Overrides the <see cref="T:System.Windows.DependencyObject" /> implementation of <see cref="M:System.Windows.DependencyObject.OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs)" /> to also invoke any <see cref="E:System.Windows.Freezable.Changed" /> handlers in response to a changing dependency property of type <see cref="T:System.Windows.Freezable" />.</summary>
	/// <param name="e">Event data that contains information about which property changed, and its old and new values.</param>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (!e.IsASubPropertyChange || e.OperationType == OperationType.ChangeMutableDefaultValue)
		{
			WritePostscript();
		}
		Debug_DetectContextLeaks();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Freezable" /> class. </summary>
	/// <returns>The new instance.</returns>
	protected Freezable CreateInstance()
	{
		Freezable freezable = CreateInstanceCore();
		Debug_VerifyInstance("CreateInstance", this, freezable);
		return freezable;
	}

	/// <summary>When implemented in a derived class, creates a new instance of the <see cref="T:System.Windows.Freezable" /> derived class. </summary>
	/// <returns>The new instance.</returns>
	protected abstract Freezable CreateInstanceCore();

	/// <summary>Makes the instance a clone (deep copy) of the specified <see cref="T:System.Windows.Freezable" /> using base (non-animated) property values. </summary>
	/// <param name="sourceFreezable">The object to clone.</param>
	protected virtual void CloneCore(Freezable sourceFreezable)
	{
		CloneCoreCommon(sourceFreezable, useCurrentValue: false, cloneFrozenValues: true);
	}

	/// <summary>Makes the instance a modifiable clone (deep copy) of the specified <see cref="T:System.Windows.Freezable" /> using current property values.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Freezable" /> to be cloned.</param>
	protected virtual void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		CloneCoreCommon(sourceFreezable, useCurrentValue: true, cloneFrozenValues: true);
	}

	/// <summary>Makes the instance a frozen clone of the specified <see cref="T:System.Windows.Freezable" /> using base (non-animated) property values.</summary>
	/// <param name="sourceFreezable">The instance to copy.</param>
	protected virtual void GetAsFrozenCore(Freezable sourceFreezable)
	{
		CloneCoreCommon(sourceFreezable, useCurrentValue: false, cloneFrozenValues: false);
	}

	/// <summary>Makes the current instance a frozen clone of the specified <see cref="T:System.Windows.Freezable" />. If the object has animated dependency properties, their current animated values are copied.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Freezable" /> to copy and freeze.</param>
	protected virtual void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		CloneCoreCommon(sourceFreezable, useCurrentValue: true, cloneFrozenValues: false);
	}

	/// <summary>Makes the <see cref="T:System.Windows.Freezable" /> object unmodifiable or tests whether it can be made unmodifiable.</summary>
	/// <returns>If <paramref name="isChecking" /> is true, this method returns true if the <see cref="T:System.Windows.Freezable" /> can be made unmodifiable, or false if it cannot be made unmodifiable. If <paramref name="isChecking" /> is false, this method returns true if the if the specified <see cref="T:System.Windows.Freezable" /> is now unmodifiable, or false if it cannot be made unmodifiable. </returns>
	/// <param name="isChecking">true to return an indication of whether the object can be frozen (without actually freezing it); false to actually freeze the object.</param>
	protected virtual bool FreezeCore(bool isChecking)
	{
		EffectiveValueEntry[] effectiveValues = base.EffectiveValues;
		uint effectiveValuesCount = base.EffectiveValuesCount;
		for (uint num = 0u; num < effectiveValuesCount; num++)
		{
			DependencyProperty dependencyProperty = DependencyProperty.RegisteredPropertyList.List[effectiveValues[num].PropertyIndex];
			if (dependencyProperty != null)
			{
				EntryIndex entryIndex = new EntryIndex(num);
				PropertyMetadata metadata = dependencyProperty.GetMetadata(base.DependencyObjectType);
				if (!metadata.FreezeValueCallback(this, dependencyProperty, entryIndex, metadata, isChecking))
				{
					return false;
				}
			}
		}
		return true;
	}

	private EventStorage GetEventStorage()
	{
		EventStorage eventStorage = CachedEventStorage;
		if (eventStorage.InUse)
		{
			eventStorage = new EventStorage(eventStorage.PhysicalSize);
		}
		eventStorage.InUse = true;
		return eventStorage;
	}

	/// <summary>Called when the current <see cref="T:System.Windows.Freezable" /> object is modified. </summary>
	protected virtual void OnChanged()
	{
	}

	private void GetChangeHandlersAndInvalidateSubProperties(ref EventStorage calledHandlers)
	{
		OnChanged();
		if (base.Freezable_UsingSingletonContext)
		{
			DependencyObject singletonContext = SingletonContext;
			if (singletonContext is Freezable freezable)
			{
				freezable.GetChangeHandlersAndInvalidateSubProperties(ref calledHandlers);
			}
			if (SingletonContextProperty != null)
			{
				singletonContext.InvalidateSubProperty(SingletonContextProperty);
			}
		}
		else if (base.Freezable_UsingContextList)
		{
			FrugalObjectList<FreezableContextPair> contextList = ContextList;
			DependencyObject dependencyObject = null;
			int num = 0;
			int i = 0;
			for (int count = contextList.Count; i < count; i++)
			{
				FreezableContextPair freezableContextPair = contextList[i];
				DependencyObject dependencyObject2 = (DependencyObject)freezableContextPair.Owner.Target;
				if (dependencyObject2 != null)
				{
					if (dependencyObject2 != dependencyObject)
					{
						if (dependencyObject2 is Freezable freezable2)
						{
							freezable2.GetChangeHandlersAndInvalidateSubProperties(ref calledHandlers);
						}
						dependencyObject = dependencyObject2;
					}
					if (freezableContextPair.Property != null)
					{
						dependencyObject2.InvalidateSubProperty(freezableContextPair.Property);
					}
				}
				else
				{
					num++;
				}
			}
			PruneContexts(contextList, num);
		}
		GetHandlers(ref calledHandlers);
	}

	/// <summary>Ensures that the <see cref="T:System.Windows.Freezable" /> is being accessed from a valid thread. Inheritors of <see cref="T:System.Windows.Freezable" /> must call this method at the beginning of any API that reads data members that are not dependency properties.</summary>
	protected void ReadPreamble()
	{
		VerifyAccess();
	}

	/// <summary>Verifies that the <see cref="T:System.Windows.Freezable" /> is not frozen and that it is being accessed from a valid threading context. <see cref="T:System.Windows.Freezable" /> inheritors should call this method at the beginning of any API that writes to data members that are not dependency properties. </summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Freezable" /> instance is frozen and cannot have its members written to.</exception>
	protected void WritePreamble()
	{
		VerifyAccess();
		if (IsFrozenInternal)
		{
			throw new InvalidOperationException(SR.Format(SR.Freezable_CantBeFrozen, GetType().FullName));
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Freezable.Changed" /> event for the <see cref="T:System.Windows.Freezable" /> and invokes its <see cref="M:System.Windows.Freezable.OnChanged" /> method. Classes that derive from <see cref="T:System.Windows.Freezable" /> should call this method at the end of any API that modifies class members that are not stored as dependency properties.</summary>
	protected void WritePostscript()
	{
		FireChanged();
	}

	/// <summary>Ensures that appropriate context pointers are established for a <see cref="T:System.Windows.DependencyObjectType" /> data member that has just been set.</summary>
	/// <param name="oldValue">The previous value of the data member.</param>
	/// <param name="newValue">The current value of the data member.</param>
	protected void OnFreezablePropertyChanged(DependencyObject oldValue, DependencyObject newValue)
	{
		OnFreezablePropertyChanged(oldValue, newValue, null);
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="oldValue">The previous value of the data member.</param>
	/// <param name="newValue">The current value of the data member.</param>
	/// <param name="property">The property that changed. </param>
	protected void OnFreezablePropertyChanged(DependencyObject oldValue, DependencyObject newValue, DependencyProperty property)
	{
		if (newValue != null)
		{
			EnsureConsistentDispatchers(this, newValue);
		}
		if (oldValue != null)
		{
			RemoveSelfAsInheritanceContext(oldValue, property);
		}
		if (newValue != null)
		{
			ProvideSelfAsInheritanceContext(newValue, property);
		}
	}

	/// <summary>If the <paramref name="isChecking" /> parameter is true, this method indicates whether the specified <see cref="T:System.Windows.Freezable" /> can be made unmodifiable. If the <paramref name="isChecking" /> parameter is false, this method attempts to make the specified <see cref="T:System.Windows.Freezable" /> unmodifiable and indicates whether the operation succeeded.</summary>
	/// <returns>If <paramref name="isChecking" /> is true, this method returns true if the specified <see cref="T:System.Windows.Freezable" /> can be made unmodifiable, or false if it cannot be made unmodifiable. If <paramref name="isChecking" /> is false, this method returns true if the specified <see cref="T:System.Windows.Freezable" /> is now unmodifiable, or false if it cannot be made unmodifiable. </returns>
	/// <param name="freezable">The object to check or make unmodifiable. If <paramref name="isChecking" /> is true, the object is checked to determine whether it can be made unmodifiable. If <paramref name="isChecking" /> is false, the object is made unmodifiable, if possible.</param>
	/// <param name="isChecking">true to return an indication of whether the object can be frozen (without actually freezing it); false to actually freeze the object.</param>
	/// <exception cref="T:System.InvalidOperationException">When <paramref name="isChecking" /> is false, the attempt to make <paramref name="freezable" /> unmodifiable was unsuccessful; the object is now in an unknown state (it might be partially frozen).  </exception>
	protected internal static bool Freeze(Freezable freezable, bool isChecking)
	{
		return freezable?.Freeze(isChecking) ?? true;
	}

	void ISealable.Seal()
	{
		Freeze();
	}

	internal void ClearContextAndHandlers()
	{
		base.Freezable_UsingHandlerList = false;
		base.Freezable_UsingContextList = false;
		base.Freezable_UsingSingletonHandler = false;
		base.Freezable_UsingSingletonContext = false;
		_contextStorage = null;
		_property = null;
	}

	internal void FireChanged()
	{
		EventStorage calledHandlers = null;
		GetChangeHandlersAndInvalidateSubProperties(ref calledHandlers);
		if (calledHandlers != null)
		{
			int i = 0;
			for (int count = calledHandlers.Count; i < count; i++)
			{
				calledHandlers[i](this, EventArgs.Empty);
				calledHandlers[i] = null;
			}
			calledHandlers.Clear();
			calledHandlers.InUse = false;
		}
	}

	internal override void Seal()
	{
		Invariant.Assert(condition: false);
	}

	internal bool Freeze(bool isChecking)
	{
		if (isChecking)
		{
			ReadPreamble();
			return FreezeCore(isChecking: true);
		}
		if (!IsFrozenInternal)
		{
			WritePreamble();
			FreezeCore(isChecking: false);
			PropertyMetadata.RemoveAllCachedDefaultValues(this);
			DependencyObject.DependentListMapField.ClearValue(this);
			base.Freezable_Frozen = true;
			DetachFromDispatcher();
			FireChanged();
			ClearContextAndHandlers();
			WritePostscript();
		}
		return true;
	}

	private void CloneCoreCommon(Freezable sourceFreezable, bool useCurrentValue, bool cloneFrozenValues)
	{
		EffectiveValueEntry[] effectiveValues = sourceFreezable.EffectiveValues;
		uint effectiveValuesCount = sourceFreezable.EffectiveValuesCount;
		for (uint num = 0u; num < effectiveValuesCount; num++)
		{
			EffectiveValueEntry effectiveValueEntry = effectiveValues[num];
			DependencyProperty dependencyProperty = DependencyProperty.RegisteredPropertyList.List[effectiveValueEntry.PropertyIndex];
			if (dependencyProperty == null || dependencyProperty.ReadOnly)
			{
				continue;
			}
			EntryIndex entryIndex = new EntryIndex(num);
			object obj;
			if (useCurrentValue)
			{
				obj = sourceFreezable.GetValueEntry(entryIndex, dependencyProperty, null, RequestFlags.FullyResolved).Value;
			}
			else
			{
				obj = sourceFreezable.ReadLocalValueEntry(entryIndex, dependencyProperty, allowDeferredReferences: true);
				if (obj == DependencyProperty.UnsetValue)
				{
					continue;
				}
				if (effectiveValueEntry.IsExpression)
				{
					obj = ((Expression)obj).Copy(this, dependencyProperty);
				}
			}
			if (obj is Freezable freezable)
			{
				if (cloneFrozenValues)
				{
					Freezable freezable2 = freezable.CreateInstanceCore();
					if (useCurrentValue)
					{
						freezable2.CloneCurrentValueCore(freezable);
					}
					else
					{
						freezable2.CloneCore(freezable);
					}
					obj = freezable2;
					Debug_VerifyCloneCommon(freezable, freezable2, isDeepClone: true);
				}
				else if (!freezable.IsFrozen)
				{
					Freezable freezable2 = freezable.CreateInstanceCore();
					if (useCurrentValue)
					{
						freezable2.GetCurrentValueAsFrozenCore(freezable);
					}
					else
					{
						freezable2.GetAsFrozenCore(freezable);
					}
					obj = freezable2;
					Debug_VerifyCloneCommon(freezable, freezable2, isDeepClone: false);
				}
			}
			SetValue(dependencyProperty, obj);
		}
	}

	private static void EnsureConsistentDispatchers(DependencyObject owner, DependencyObject child)
	{
		if (owner.Dispatcher != null && child.Dispatcher != null && owner.Dispatcher != child.Dispatcher)
		{
			throw new InvalidOperationException(SR.Freezable_AttemptToUseInnerValueWithDifferentThread);
		}
	}

	private void RemoveContextInformation(DependencyObject context, DependencyProperty property)
	{
		bool flag = true;
		if (base.Freezable_UsingSingletonContext)
		{
			if (SingletonContext == context && SingletonContextProperty == property)
			{
				RemoveSingletonContext();
				flag = false;
			}
		}
		else if (base.Freezable_UsingContextList)
		{
			FrugalObjectList<FreezableContextPair> contextList = ContextList;
			int num = 0;
			int num2 = -1;
			int count = contextList.Count;
			for (int i = 0; i < count; i++)
			{
				FreezableContextPair freezableContextPair = contextList[i];
				object target = freezableContextPair.Owner.Target;
				if (target != null)
				{
					if (flag && freezableContextPair.Property == property && target == context)
					{
						num2 = i;
						flag = false;
					}
				}
				else
				{
					num++;
				}
			}
			if (num2 != -1)
			{
				contextList.RemoveAt(num2);
			}
			PruneContexts(contextList, num);
		}
		if (flag)
		{
			throw new ArgumentException(SR.Freezable_NotAContext, "context");
		}
	}

	private void RemoveSingletonContext()
	{
		if (HasHandlers)
		{
			_contextStorage = ((HandlerContextStorage)_contextStorage)._handlerStorage;
		}
		else
		{
			_contextStorage = null;
		}
		base.Freezable_UsingSingletonContext = false;
	}

	private void RemoveContextList()
	{
		if (HasHandlers)
		{
			_contextStorage = ((HandlerContextStorage)_contextStorage)._handlerStorage;
		}
		else
		{
			_contextStorage = null;
		}
		base.Freezable_UsingContextList = false;
	}

	internal override void AddInheritanceContext(DependencyObject context, DependencyProperty property)
	{
		if (!IsFrozenInternal)
		{
			DependencyObject inheritanceContext = InheritanceContext;
			AddContextInformation(context, property);
			if (inheritanceContext != InheritanceContext)
			{
				OnInheritanceContextChanged(EventArgs.Empty);
			}
		}
	}

	internal override void RemoveInheritanceContext(DependencyObject context, DependencyProperty property)
	{
		if (!IsFrozenInternal)
		{
			DependencyObject inheritanceContext = InheritanceContext;
			RemoveContextInformation(context, property);
			if (inheritanceContext != InheritanceContext)
			{
				OnInheritanceContextChanged(EventArgs.Empty);
			}
		}
	}

	internal void AddContextInformation(DependencyObject context, DependencyProperty property)
	{
		if (base.Freezable_UsingSingletonContext)
		{
			ConvertToContextList();
		}
		if (base.Freezable_UsingContextList)
		{
			AddContextToList(context, property);
		}
		else
		{
			AddSingletonContext(context, property);
		}
	}

	private void ConvertToContextList()
	{
		FrugalObjectList<FreezableContextPair> frugalObjectList = new FrugalObjectList<FreezableContextPair>(2);
		frugalObjectList.Add(new FreezableContextPair(SingletonContext, SingletonContextProperty));
		if (HasHandlers)
		{
			((HandlerContextStorage)_contextStorage)._contextStorage = frugalObjectList;
		}
		else
		{
			_contextStorage = frugalObjectList;
		}
		base.Freezable_UsingContextList = true;
		base.Freezable_UsingSingletonContext = false;
		_property = null;
	}

	private void AddSingletonContext(DependencyObject context, DependencyProperty property)
	{
		if (HasHandlers)
		{
			HandlerContextStorage handlerContextStorage = new HandlerContextStorage();
			handlerContextStorage._handlerStorage = _contextStorage;
			handlerContextStorage._contextStorage = context;
			_contextStorage = handlerContextStorage;
		}
		else
		{
			_contextStorage = context;
		}
		_property = property;
		base.Freezable_UsingSingletonContext = true;
	}

	private void AddContextToList(DependencyObject context, DependencyProperty property)
	{
		FrugalObjectList<FreezableContextPair> contextList = ContextList;
		int count = contextList.Count;
		int index = count;
		int num = 0;
		DependencyObject dependencyObject = null;
		bool flag = HasMultipleInheritanceContexts;
		bool flag2 = context.CanBeInheritanceContext && !base.IsInheritanceContextSealed;
		for (int i = 0; i < count; i++)
		{
			DependencyObject dependencyObject2 = (DependencyObject)contextList[i].Owner.Target;
			if (dependencyObject2 != null)
			{
				if (dependencyObject2 == context)
				{
					index = i + 1;
					flag2 = false;
				}
				if (flag2 && !flag)
				{
					if (dependencyObject2 != dependencyObject && dependencyObject2.CanBeInheritanceContext)
					{
						flag = true;
						base.Freezable_HasMultipleInheritanceContexts = true;
					}
					dependencyObject = dependencyObject2;
				}
			}
			else
			{
				num++;
			}
		}
		contextList.Insert(index, new FreezableContextPair(context, property));
		PruneContexts(contextList, num);
	}

	private void PruneContexts(FrugalObjectList<FreezableContextPair> oldList, int numDead)
	{
		int count = oldList.Count;
		if (count - numDead == 0)
		{
			RemoveContextList();
		}
		else
		{
			if (numDead <= 0)
			{
				return;
			}
			FrugalObjectList<FreezableContextPair> frugalObjectList = new FrugalObjectList<FreezableContextPair>(count - numDead);
			for (int i = 0; i < count; i++)
			{
				if (oldList[i].Owner.IsAlive)
				{
					frugalObjectList.Add(oldList[i]);
				}
			}
			ContextList = frugalObjectList;
		}
	}

	private void GetHandlers(ref EventStorage calledHandlers)
	{
		if (base.Freezable_UsingSingletonHandler)
		{
			if (calledHandlers == null)
			{
				calledHandlers = GetEventStorage();
			}
			calledHandlers.Add(SingletonHandler);
		}
		else if (base.Freezable_UsingHandlerList)
		{
			if (calledHandlers == null)
			{
				calledHandlers = GetEventStorage();
			}
			FrugalObjectList<EventHandler> handlerList = HandlerList;
			int i = 0;
			for (int count = handlerList.Count; i < count; i++)
			{
				calledHandlers.Add(handlerList[i]);
			}
		}
	}

	private void HandlerAdd(EventHandler handler)
	{
		if (base.Freezable_UsingSingletonHandler)
		{
			ConvertToHandlerList();
		}
		if (base.Freezable_UsingHandlerList)
		{
			HandlerList.Add(handler);
		}
		else
		{
			AddSingletonHandler(handler);
		}
	}

	private void HandlerRemove(EventHandler handler)
	{
		bool flag = true;
		if (base.Freezable_UsingSingletonHandler)
		{
			if (SingletonHandler == handler)
			{
				RemoveSingletonHandler();
				flag = false;
			}
		}
		else if (base.Freezable_UsingHandlerList)
		{
			FrugalObjectList<EventHandler> handlerList = HandlerList;
			int num = handlerList.IndexOf(handler);
			if (num >= 0)
			{
				handlerList.RemoveAt(num);
				flag = false;
			}
			if (handlerList.Count == 0)
			{
				RemoveHandlerList();
			}
		}
		if (flag)
		{
			throw new ArgumentException(SR.Freezable_UnregisteredHandler, "handler");
		}
	}

	private void RemoveSingletonHandler()
	{
		if (HasContextInformation)
		{
			_contextStorage = ((HandlerContextStorage)_contextStorage)._contextStorage;
		}
		else
		{
			_contextStorage = null;
		}
		base.Freezable_UsingSingletonHandler = false;
	}

	private void RemoveHandlerList()
	{
		if (HasContextInformation)
		{
			_contextStorage = ((HandlerContextStorage)_contextStorage)._contextStorage;
		}
		else
		{
			_contextStorage = null;
		}
		base.Freezable_UsingHandlerList = false;
	}

	private void ConvertToHandlerList()
	{
		EventHandler singletonHandler = SingletonHandler;
		FrugalObjectList<EventHandler> frugalObjectList = new FrugalObjectList<EventHandler>(2);
		frugalObjectList.Add(singletonHandler);
		if (HasContextInformation)
		{
			((HandlerContextStorage)_contextStorage)._handlerStorage = frugalObjectList;
		}
		else
		{
			_contextStorage = frugalObjectList;
		}
		base.Freezable_UsingHandlerList = true;
		base.Freezable_UsingSingletonHandler = false;
	}

	private void AddSingletonHandler(EventHandler handler)
	{
		if (HasContextInformation)
		{
			HandlerContextStorage handlerContextStorage = new HandlerContextStorage();
			handlerContextStorage._contextStorage = _contextStorage;
			handlerContextStorage._handlerStorage = handler;
			_contextStorage = handlerContextStorage;
		}
		else
		{
			_contextStorage = handler;
		}
		base.Freezable_UsingSingletonHandler = true;
	}

	private static void Debug_VerifyCloneCommon(Freezable original, object clone, bool isDeepClone)
	{
		if (!Invariant.Strict)
		{
			return;
		}
		Freezable freezable = (Freezable)clone;
		Debug_VerifyInstance("CloneCore", original, freezable);
		if (isDeepClone)
		{
			Invariant.Assert(clone != original, "CloneCore should not return the same instance as the original.");
		}
		Invariant.Assert(!freezable.HasHandlers, "CloneCore should not have handlers attached on construction.");
		if (!(original is IList list))
		{
			return;
		}
		IList list2 = clone as IList;
		Invariant.Assert(list.Count == list2.Count, "CloneCore didn't clone all of the elements in the list.");
		for (int i = 0; i < list2.Count; i++)
		{
			Freezable freezable2 = list[i] as Freezable;
			Freezable freezable3 = list2[i] as Freezable;
			if (isDeepClone && freezable3 != null && freezable3 != null)
			{
				Invariant.Assert(freezable2 != freezable3, "CloneCore didn't clone the elements in the list correctly.");
			}
		}
	}

	private static void Debug_VerifyInstance(string methodName, Freezable original, Freezable newInstance)
	{
		if (Invariant.Strict)
		{
			Invariant.Assert(newInstance != null, "{0} should not return null.", methodName);
			Invariant.Assert(newInstance.GetType() == original.GetType(), string.Format(CultureInfo.InvariantCulture, "{0} should return instance of same type. (Expected= '{1}', Actual='{2}')", methodName, original.GetType(), newInstance.GetType()));
			Invariant.Assert(!newInstance.IsFrozen, "{0} should return a mutable instance. Recieved a frozen instance.", methodName);
		}
	}

	private void Debug_DetectContextLeaks()
	{
		if (!Invariant.Strict)
		{
			return;
		}
		if (base.Freezable_UsingSingletonContext)
		{
			Debug_VerifyContextIsValid(SingletonContext, SingletonContextProperty);
		}
		else
		{
			if (!base.Freezable_UsingContextList)
			{
				return;
			}
			_ = ContextList;
			int i = 0;
			for (int count = ContextList.Count; i < count; i++)
			{
				FreezableContextPair freezableContextPair = ContextList[i];
				DependencyObject owner = (DependencyObject)freezableContextPair.Owner.Target;
				if (freezableContextPair.Owner.IsAlive)
				{
					Debug_VerifyContextIsValid(owner, freezableContextPair.Property);
				}
			}
		}
	}

	private void Debug_VerifyContextIsValid(DependencyObject owner, DependencyProperty property)
	{
		if (Invariant.Strict)
		{
			Invariant.Assert(owner != null, "We should not have null owners in the ContextList/SingletonContext.");
			if (property != null)
			{
				owner.GetValue(property);
				if (property.Name == "Visual" && property.OwnerType.FullName == "System.Windows.Media.VisualBrush")
				{
					_ = owner.GetType().FullName != "System.Windows.Media.VisualBrush";
				}
				else
					_ = 0;
			}
		}
	}
}
