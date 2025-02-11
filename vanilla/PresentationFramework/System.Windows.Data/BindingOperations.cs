using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using MS.Internal.Data;

namespace System.Windows.Data;

/// <summary>Provides static methods to manipulate bindings, including <see cref="T:System.Windows.Data.Binding" />, <see cref="T:System.Windows.Data.MultiBinding" />, and <see cref="T:System.Windows.Data.PriorityBinding" /> objects.</summary>
public static class BindingOperations
{
	internal class ExceptionLogger : IDisposable
	{
		private List<Exception> _log = new List<Exception>();

		internal List<Exception> Log => _log;

		internal void LogException(Exception ex)
		{
			_log.Add(ex);
		}

		void IDisposable.Dispose()
		{
			Interlocked.CompareExchange(ref _exceptionLogger, null, this);
		}
	}

	private static ExceptionLogger _exceptionLogger;

	/// <summary>Gets an object that replaces the <see cref="P:System.Windows.FrameworkElement.DataContext" /> when an item container is removed from the visual tree.</summary>
	/// <returns>An object that replaces the <see cref="P:System.Windows.FrameworkElement.DataContext" /> when an item container is removed from the visual tree.</returns>
	public static object DisconnectedSource => BindingExpressionBase.DisconnectedItem;

	internal static bool IsCleanupEnabled
	{
		get
		{
			return DataBindEngine.CurrentDataBindEngine.CleanupEnabled;
		}
		set
		{
			DataBindEngine.CurrentDataBindEngine.CleanupEnabled = value;
		}
	}

	internal static bool TraceAccessorTableSize
	{
		get
		{
			return DataBindEngine.CurrentDataBindEngine.AccessorTable.TraceSize;
		}
		set
		{
			DataBindEngine.CurrentDataBindEngine.AccessorTable.TraceSize = value;
		}
	}

	/// <summary>Occurs when the data-binding system notices a collection.</summary>
	public static event EventHandler<CollectionRegisteringEventArgs> CollectionRegistering;

	/// <summary>Occurs when the data-binding system notices a collection view.</summary>
	public static event EventHandler<CollectionViewRegisteringEventArgs> CollectionViewRegistering;

	/// <summary>Creates and associates a new instance of <see cref="T:System.Windows.Data.BindingExpressionBase" /> with the specified binding target property.</summary>
	/// <returns>The instance of <see cref="T:System.Windows.Data.BindingExpressionBase" /> created for and associated with the specified property. The <see cref="T:System.Windows.Data.BindingExpressionBase" /> class is the base class of <see cref="T:System.Windows.Data.BindingExpression" />, <see cref="T:System.Windows.Data.MultiBindingExpression" />, and <see cref="T:System.Windows.Data.PriorityBindingExpression" />.</returns>
	/// <param name="target">The binding target of the binding.</param>
	/// <param name="dp">The target property of the binding.</param>
	/// <param name="binding">The <see cref="T:System.Windows.Data.BindingBase" /> object that describes the binding.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="target" /> parameter cannot be null.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="dp" /> parameter cannot be null.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="binding" /> parameter cannot be null.</exception>
	public static BindingExpressionBase SetBinding(DependencyObject target, DependencyProperty dp, BindingBase binding)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		if (binding == null)
		{
			throw new ArgumentNullException("binding");
		}
		BindingExpressionBase bindingExpressionBase = binding.CreateBindingExpression(target, dp);
		target.SetValue(dp, bindingExpressionBase);
		return bindingExpressionBase;
	}

	/// <summary>Retrieves the <see cref="T:System.Windows.Data.BindingBase" /> object that is set on the specified property.</summary>
	/// <returns>The <see cref="T:System.Windows.Data.BindingBase" /> object that is set on the given property or null if no binding object has been set.</returns>
	/// <param name="target">The object where <paramref name="dp" /> is.</param>
	/// <param name="dp">The binding target property from which to retrieve the <see cref="T:System.Windows.Data.BindingBase" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="target" /> and <paramref name="dp" /> parameters cannot be null.</exception>
	public static BindingBase GetBindingBase(DependencyObject target, DependencyProperty dp)
	{
		return GetBindingExpressionBase(target, dp)?.ParentBindingBase;
	}

	/// <summary>Retrieves the <see cref="T:System.Windows.Data.Binding" /> object that is set on the specified property.</summary>
	/// <returns>The <see cref="T:System.Windows.Data.Binding" /> object set on the given property or null if no <see cref="T:System.Windows.Data.Binding" /> object has been set.</returns>
	/// <param name="target">The object where <paramref name="dp" /> is.</param>
	/// <param name="dp">The binding target property from which to retrieve the binding.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="target" /> and <paramref name="dp" /> parameters cannot be null.</exception>
	public static Binding GetBinding(DependencyObject target, DependencyProperty dp)
	{
		return GetBindingBase(target, dp) as Binding;
	}

	/// <summary>Retrieves the <see cref="T:System.Windows.Data.PriorityBinding" /> object that is set on the specified property.</summary>
	/// <returns>The <see cref="T:System.Windows.Data.PriorityBinding" /> object set on the given property or null if no <see cref="T:System.Windows.Data.PriorityBinding" /> object has been set.</returns>
	/// <param name="target">The object where <paramref name="dp" /> is.</param>
	/// <param name="dp">The binding target property from which to retrieve the binding.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="target" /> and <paramref name="dp" /> parameters cannot be null.</exception>
	public static PriorityBinding GetPriorityBinding(DependencyObject target, DependencyProperty dp)
	{
		return GetBindingBase(target, dp) as PriorityBinding;
	}

	/// <summary>Retrieves the <see cref="T:System.Windows.Data.MultiBinding" /> object that is set on the specified property.</summary>
	/// <returns>The <see cref="T:System.Windows.Data.MultiBinding" /> object set on the given property or null if no <see cref="T:System.Windows.Data.MultiBinding" /> object has been set.</returns>
	/// <param name="target">The object where <paramref name="dp" /> is.</param>
	/// <param name="dp">The binding target property from which to retrieve the binding.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="target" /> and <paramref name="dp" /> parameters cannot be null.</exception>
	public static MultiBinding GetMultiBinding(DependencyObject target, DependencyProperty dp)
	{
		return GetBindingBase(target, dp) as MultiBinding;
	}

	/// <summary>Retrieves the <see cref="T:System.Windows.Data.BindingExpressionBase" /> object that is set on the specified property.</summary>
	/// <returns>The <see cref="T:System.Windows.Data.BindingExpressionBase" /> object that is set on the given property or null if no binding object has been set.</returns>
	/// <param name="target">The object where <paramref name="dp" /> is.</param>
	/// <param name="dp">The binding target property from which to retrieve the <see cref="T:System.Windows.Data.BindingExpressionBase" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="target" /> and <paramref name="dp" /> parameters cannot be null.</exception>
	public static BindingExpressionBase GetBindingExpressionBase(DependencyObject target, DependencyProperty dp)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		return StyleHelper.GetExpression(target, dp) as BindingExpressionBase;
	}

	/// <summary>Returns the <see cref="T:System.Windows.Data.BindingExpression" /> object associated with the specified binding target property on the specified object.</summary>
	/// <returns>The <see cref="T:System.Windows.Data.BindingExpression" /> object associated with the given property or null if none exists. If a <see cref="T:System.Windows.Data.PriorityBindingExpression" /> object is set on the property, the <see cref="P:System.Windows.Data.PriorityBindingExpression.ActiveBindingExpression" /> is returned.</returns>
	/// <param name="target">The binding target object where <paramref name="dp" /> is.</param>
	/// <param name="dp">The binding target property from which to retrieve the <see cref="T:System.Windows.Data.BindingExpression" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="target" /> and <paramref name="dp" /> parameters cannot be null.</exception>
	public static BindingExpression GetBindingExpression(DependencyObject target, DependencyProperty dp)
	{
		BindingExpressionBase bindingExpressionBase = GetBindingExpressionBase(target, dp);
		if (bindingExpressionBase is PriorityBindingExpression priorityBindingExpression)
		{
			bindingExpressionBase = priorityBindingExpression.ActiveBindingExpression;
		}
		return bindingExpressionBase as BindingExpression;
	}

	/// <summary>Returns the <see cref="T:System.Windows.Data.MultiBindingExpression" /> object associated with the specified binding target property on the specified object.</summary>
	/// <returns>The <see cref="T:System.Windows.Data.MultiBindingExpression" /> object associated with the given property or null if none exists.</returns>
	/// <param name="target">The binding target object where <paramref name="dp" /> is.</param>
	/// <param name="dp">The binding target property from which to retrieve the <see cref="T:System.Windows.Data.MultiBindingExpression" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="target" /> and <paramref name="dp" /> parameters cannot be null.</exception>
	public static MultiBindingExpression GetMultiBindingExpression(DependencyObject target, DependencyProperty dp)
	{
		return GetBindingExpressionBase(target, dp) as MultiBindingExpression;
	}

	/// <summary>Returns the <see cref="T:System.Windows.Data.PriorityBindingExpression" /> object associated with the specified binding target property on the specified object.</summary>
	/// <returns>The <see cref="T:System.Windows.Data.PriorityBindingExpression" /> object associated with the given property or null if none exists.</returns>
	/// <param name="target">The binding target object where <paramref name="dp" /> is.</param>
	/// <param name="dp">The binding target property from which to retrieve the <see cref="T:System.Windows.Data.PriorityBindingExpression" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="target" /> and <paramref name="dp" /> parameters cannot be null.</exception>
	public static PriorityBindingExpression GetPriorityBindingExpression(DependencyObject target, DependencyProperty dp)
	{
		return GetBindingExpressionBase(target, dp) as PriorityBindingExpression;
	}

	/// <summary>Removes the binding from a property if there is one.</summary>
	/// <param name="target">The object from which to remove the binding.</param>
	/// <param name="dp">The dependency property from which to remove the binding.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="target" /> and <paramref name="dp" /> parameters cannot be null.</exception>
	public static void ClearBinding(DependencyObject target, DependencyProperty dp)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		if (IsDataBound(target, dp))
		{
			target.ClearValue(dp);
		}
	}

	/// <summary>Removes all bindings, including bindings of type <see cref="T:System.Windows.Data.Binding" />, <see cref="T:System.Windows.Data.MultiBinding" />, and <see cref="T:System.Windows.Data.PriorityBinding" />, from the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <param name="target">The object from which to remove bindings.</param>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="target" /> is null.</exception>
	public static void ClearAllBindings(DependencyObject target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		LocalValueEnumerator localValueEnumerator = target.GetLocalValueEnumerator();
		ArrayList arrayList = new ArrayList(8);
		while (localValueEnumerator.MoveNext())
		{
			LocalValueEntry current = localValueEnumerator.Current;
			if (IsDataBound(target, current.Property))
			{
				arrayList.Add(current.Property);
			}
		}
		for (int i = 0; i < arrayList.Count; i++)
		{
			target.ClearValue((DependencyProperty)arrayList[i]);
		}
	}

	/// <summary>Returns a value that indicates whether the specified property is currently data-bound.</summary>
	/// <returns>true if the specified property is data-bound; otherwise, false.</returns>
	/// <param name="target">The object where <paramref name="dp" /> is.</param>
	/// <param name="dp">The dependency property to check.</param>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="target" /> is null.</exception>
	public static bool IsDataBound(DependencyObject target, DependencyProperty dp)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		return StyleHelper.GetExpression(target, dp) is BindingExpressionBase;
	}

	/// <summary>Enables a collection to be accessed across multiple threads and specifies the callback that should be used to synchronize access to the collection.</summary>
	/// <param name="collection">The collection that needs synchronized access.</param>
	/// <param name="context">An object that is passed to the callback.</param>
	/// <param name="synchronizationCallback">The callback that is invoked whenever access to the collection is required. </param>
	public static void EnableCollectionSynchronization(IEnumerable collection, object context, CollectionSynchronizationCallback synchronizationCallback)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (synchronizationCallback == null)
		{
			throw new ArgumentNullException("synchronizationCallback");
		}
		ViewManager.Current.RegisterCollectionSynchronizationCallback(collection, context, synchronizationCallback);
	}

	/// <summary>Enables a collection to be accessed across multiple threads and specifies the lock object that should be used to synchronize access to the collection.</summary>
	/// <param name="collection">The collection that needs synchronized access.</param>
	/// <param name="lockObject">The object to lock when accessing the collection.</param>
	public static void EnableCollectionSynchronization(IEnumerable collection, object lockObject)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (lockObject == null)
		{
			throw new ArgumentNullException("lockObject");
		}
		ViewManager.Current.RegisterCollectionSynchronizationCallback(collection, lockObject, null);
	}

	/// <summary>Remove the synchronization registered for the specified collection.</summary>
	/// <param name="collection">The collection to remove synchronized access from.</param>
	public static void DisableCollectionSynchronization(IEnumerable collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		ViewManager.Current.RegisterCollectionSynchronizationCallback(collection, null, null);
	}

	/// <summary>Provides access to a collection by using the synchronization mechanism that the application specified when it called EnableCollectionSynchronization.</summary>
	/// <param name="collection">The collection to access.</param>
	/// <param name="accessMethod">The action to perform on the collection.</param>
	/// <param name="writeAccess">true if <paramref name="accessMethod" /> will write to the collection; otherwise, false.</param>
	public static void AccessCollection(IEnumerable collection, Action accessMethod, bool writeAccess)
	{
		(ViewManager.Current ?? throw new InvalidOperationException(SR.Format(SR.AccessCollectionAfterShutDown, collection))).AccessCollection(collection, accessMethod, writeAccess);
	}

	/// <summary>Gets all <see cref="T:System.Windows.Data.BindingExpressionBase" /> objects that have invalid values or target values have not been updated the source. </summary>
	/// <returns>A collection of <see cref="T:System.Windows.Data.BindingExpressionBase" /> objects that are associated with the specified element and have invalid values or target values have not been updated the source. </returns>
	/// <param name="root">The root <see cref="T:System.Windows.UIElement" /> to get binding groups for.  This method returns <see cref="T:System.Windows.Data.BindingExpressionBase" /> objects that are associated with this element or its descendant elements. </param>
	public static ReadOnlyCollection<BindingExpressionBase> GetSourceUpdatingBindings(DependencyObject root)
	{
		return new ReadOnlyCollection<BindingExpressionBase>(DataBindEngine.CurrentDataBindEngine.CommitManager.GetBindingsInScope(root));
	}

	/// <summary>Gets all <see cref="T:System.Windows.Data.BindingGroup" /> objects that have invalid values or target values have not been updated the source. </summary>
	/// <returns>A collection of <see cref="T:System.Windows.Data.BindingGroup" /> objects that are associated with the specified element and have invalid values or target values have not been updated the source.</returns>
	/// <param name="root">The root <see cref="T:System.Windows.UIElement" /> to get binding groups for.  This method returns <see cref="T:System.Windows.Data.BindingGroup" /> objects that are associated with this element or its descendant elements.</param>
	public static ReadOnlyCollection<BindingGroup> GetSourceUpdatingBindingGroups(DependencyObject root)
	{
		return new ReadOnlyCollection<BindingGroup>(DataBindEngine.CurrentDataBindEngine.CommitManager.GetBindingGroupsInScope(root));
	}

	internal static bool IsValidUpdateSourceTrigger(UpdateSourceTrigger value)
	{
		if ((uint)value <= 3u)
		{
			return true;
		}
		return false;
	}

	internal static bool Cleanup()
	{
		return DataBindEngine.CurrentDataBindEngine.Cleanup();
	}

	[Conditional("DEBUG")]
	internal static void PrintStats()
	{
	}

	internal static void OnCollectionRegistering(IEnumerable collection, object parent)
	{
		if (BindingOperations.CollectionRegistering != null)
		{
			BindingOperations.CollectionRegistering(null, new CollectionRegisteringEventArgs(collection, parent));
		}
	}

	internal static void OnCollectionViewRegistering(CollectionView view)
	{
		if (BindingOperations.CollectionViewRegistering != null)
		{
			BindingOperations.CollectionViewRegistering(null, new CollectionViewRegisteringEventArgs(view));
		}
	}

	internal static IDisposable EnableExceptionLogging()
	{
		ExceptionLogger value = new ExceptionLogger();
		Interlocked.CompareExchange(ref _exceptionLogger, value, null);
		return _exceptionLogger;
	}

	internal static void LogException(Exception ex)
	{
		_exceptionLogger?.LogException(ex);
	}
}
