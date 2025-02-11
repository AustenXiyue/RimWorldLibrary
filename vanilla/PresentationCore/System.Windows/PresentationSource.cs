using System.Collections;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows;

/// <summary>Provides an abstract base for classes that present content from another technology as part of an interoperation scenario. In addition, this class provides static methods for working with these sources, as well as the basic visual-layer presentation architecture.</summary>
public abstract class PresentationSource : DispatcherObject
{
	private int _menuModeCount;

	private static readonly DependencyProperty RootSourceProperty;

	private static readonly DependencyProperty CachedSourceProperty;

	private static readonly DependencyProperty GetsSourceChangedEventProperty;

	private static readonly RoutedEvent SourceChangedEvent;

	private static readonly object _globalLock;

	private static WeakReferenceList _sources;

	private static WeakReferenceList _watchers;

	/// <summary>Gets the visual target for the visuals being presented in the source. </summary>
	/// <returns>A visual target (instance of a <see cref="T:System.Windows.Media.CompositionTarget" /> derived class).</returns>
	public CompositionTarget CompositionTarget => GetCompositionTargetCore();

	/// <summary>When overridden in a derived class, gets or sets the root visual being presented in the source. </summary>
	/// <returns>The root visual.</returns>
	public abstract Visual RootVisual { get; set; }

	/// <summary>When overridden in a derived class, gets a value that declares whether the object is disposed. </summary>
	/// <returns>true if the object is disposed; otherwise, false.</returns>
	public abstract bool IsDisposed { get; }

	/// <summary>Returns a list of sources. </summary>
	/// <returns>A list of weak references. </returns>
	public static IEnumerable CurrentSources => CriticalCurrentSources;

	internal static WeakReferenceList CriticalCurrentSources => _sources;

	/// <summary>Occurs when content is rendered and ready for user interaction. </summary>
	public event EventHandler ContentRendered;

	/// <summary>Provides initialization for base class values when called by the constructor of a derived class. </summary>
	protected PresentationSource()
	{
	}

	static PresentationSource()
	{
		RootSourceProperty = DependencyProperty.RegisterAttached("RootSource", typeof(PresentationSource), typeof(PresentationSource), new PropertyMetadata((object)null));
		CachedSourceProperty = DependencyProperty.RegisterAttached("CachedSource", typeof(PresentationSource), typeof(PresentationSource), new PropertyMetadata((object)null));
		GetsSourceChangedEventProperty = DependencyProperty.RegisterAttached("IsBeingWatched", typeof(bool), typeof(PresentationSource), new PropertyMetadata(false));
		SourceChangedEvent = EventManager.RegisterRoutedEvent("SourceChanged", RoutingStrategy.Direct, typeof(SourceChangedEventHandler), typeof(PresentationSource));
		_globalLock = new object();
		_sources = new WeakReferenceList(_globalLock);
		_watchers = new WeakReferenceList(_globalLock);
	}

	internal virtual IInputProvider GetInputProvider(Type inputDevice)
	{
		return null;
	}

	/// <summary>Returns the source in which a provided <see cref="T:System.Windows.Media.Visual" /> is presented.</summary>
	/// <returns>The <see cref="T:System.Windows.PresentationSource" /> in which the visual is being presented, or null if <paramref name="visual" /> is disposed.</returns>
	/// <param name="visual">The <see cref="T:System.Windows.Media.Visual" /> to find the source for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="visual" /> is null.</exception>
	public static PresentationSource FromVisual(Visual visual)
	{
		return CriticalFromVisual(visual);
	}

	/// <summary>Returns the source in which a provided <see cref="T:System.Windows.DependencyObject" /> is presented.</summary>
	/// <returns>The <see cref="T:System.Windows.PresentationSource" /> in which the dependency object is being presented.</returns>
	/// <param name="dependencyObject">The <see cref="T:System.Windows.DependencyObject" /> to find the source for.</param>
	public static PresentationSource FromDependencyObject(DependencyObject dependencyObject)
	{
		return CriticalFromVisual(dependencyObject);
	}

	/// <summary>Adds a handler for the SourceChanged event to the provided element.</summary>
	/// <param name="element">The element to add the handler to.</param>
	/// <param name="handler">The hander implementation to add.</param>
	public static void AddSourceChangedHandler(IInputElement element, SourceChangedEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (!InputElement.IsValid(element))
		{
			throw new ArgumentException(SR.Format(SR.Invalid_IInputElement, element.GetType()), "element");
		}
		DependencyObject dependencyObject = (DependencyObject)element;
		if (handler == null)
		{
			return;
		}
		if (dependencyObject is UIElement uIElement)
		{
			uIElement.AddHandler(SourceChangedEvent, handler);
			FrugalObjectList<RoutedEventHandlerInfo> frugalObjectList = uIElement.EventHandlersStore[SourceChangedEvent];
			if (1 == frugalObjectList.Count)
			{
				uIElement.VisualAncestorChanged += uIElement.OnVisualAncestorChanged;
				AddElementToWatchList(uIElement);
			}
			return;
		}
		if (dependencyObject is UIElement3D uIElement3D)
		{
			uIElement3D.AddHandler(SourceChangedEvent, handler);
			FrugalObjectList<RoutedEventHandlerInfo> frugalObjectList = uIElement3D.EventHandlersStore[SourceChangedEvent];
			if (1 == frugalObjectList.Count)
			{
				uIElement3D.VisualAncestorChanged += uIElement3D.OnVisualAncestorChanged;
				AddElementToWatchList(uIElement3D);
			}
			return;
		}
		if (dependencyObject is ContentElement contentElement)
		{
			contentElement.AddHandler(SourceChangedEvent, handler);
			FrugalObjectList<RoutedEventHandlerInfo> frugalObjectList = contentElement.EventHandlersStore[SourceChangedEvent];
			if (1 == frugalObjectList.Count)
			{
				AddElementToWatchList(contentElement);
			}
			return;
		}
		throw new InvalidOperationException(SR.Format(SR.Invalid_IInputElement, dependencyObject.GetType()));
	}

	/// <summary>Removes a handler for the SourceChanged event from the provided element.</summary>
	/// <param name="e">The element to remove the handler from.</param>
	/// <param name="handler">The handler implementation to remove.</param>
	public static void RemoveSourceChangedHandler(IInputElement e, SourceChangedEventHandler handler)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		if (!InputElement.IsValid(e))
		{
			throw new ArgumentException(SR.Format(SR.Invalid_IInputElement, e.GetType()), "e");
		}
		DependencyObject dependencyObject = (DependencyObject)e;
		if (handler == null)
		{
			return;
		}
		FrugalObjectList<RoutedEventHandlerInfo> frugalObjectList = null;
		if (dependencyObject is UIElement uIElement)
		{
			uIElement.RemoveHandler(SourceChangedEvent, handler);
			EventHandlersStore eventHandlersStore = uIElement.EventHandlersStore;
			if (eventHandlersStore != null)
			{
				frugalObjectList = eventHandlersStore[SourceChangedEvent];
			}
			if (frugalObjectList == null || frugalObjectList.Count == 0)
			{
				uIElement.VisualAncestorChanged -= uIElement.OnVisualAncestorChanged;
				RemoveElementFromWatchList(uIElement);
			}
			return;
		}
		if (dependencyObject is UIElement3D uIElement3D)
		{
			uIElement3D.RemoveHandler(SourceChangedEvent, handler);
			EventHandlersStore eventHandlersStore = uIElement3D.EventHandlersStore;
			if (eventHandlersStore != null)
			{
				frugalObjectList = eventHandlersStore[SourceChangedEvent];
			}
			if (frugalObjectList == null || frugalObjectList.Count == 0)
			{
				uIElement3D.VisualAncestorChanged -= uIElement3D.OnVisualAncestorChanged;
				RemoveElementFromWatchList(uIElement3D);
			}
			return;
		}
		if (dependencyObject is ContentElement contentElement)
		{
			contentElement.RemoveHandler(SourceChangedEvent, handler);
			EventHandlersStore eventHandlersStore = contentElement.EventHandlersStore;
			if (eventHandlersStore != null)
			{
				frugalObjectList = eventHandlersStore[SourceChangedEvent];
			}
			if (frugalObjectList == null || frugalObjectList.Count == 0)
			{
				RemoveElementFromWatchList(contentElement);
			}
			return;
		}
		throw new InvalidOperationException(SR.Format(SR.Invalid_IInputElement, dependencyObject.GetType()));
	}

	[FriendAccessAllowed]
	internal static void OnAncestorChanged(ContentElement ce)
	{
		if (ce == null)
		{
			throw new ArgumentNullException("ce");
		}
		if ((bool)ce.GetValue(GetsSourceChangedEventProperty))
		{
			UpdateSourceOfElement(ce, null, null);
		}
	}

	internal void PushMenuMode()
	{
		_menuModeCount++;
		if (1 == _menuModeCount)
		{
			OnEnterMenuMode();
		}
	}

	internal void PopMenuMode()
	{
		if (_menuModeCount <= 0)
		{
			throw new InvalidOperationException();
		}
		_menuModeCount--;
		if (_menuModeCount == 0)
		{
			OnLeaveMenuMode();
		}
	}

	internal virtual void OnEnterMenuMode()
	{
	}

	internal virtual void OnLeaveMenuMode()
	{
	}

	/// <summary>When overridden in a derived class, returns a visual target for the given source. </summary>
	/// <returns>Returns a <see cref="T:System.Windows.Media.CompositionTarget" /> that is target for rendering the visual.</returns>
	protected abstract CompositionTarget GetCompositionTargetCore();

	/// <summary>Provides notification that the root <see cref="T:System.Windows.Media.Visual" /> has changed. </summary>
	/// <param name="oldRoot">The old root <see cref="T:System.Windows.Media.Visual" />.</param>
	/// <param name="newRoot">The new root <see cref="T:System.Windows.Media.Visual" />.</param>
	protected void RootChanged(Visual oldRoot, Visual newRoot)
	{
		PresentationSource presentationSource = null;
		if (oldRoot == newRoot)
		{
			return;
		}
		if (oldRoot != null)
		{
			presentationSource = (PresentationSource)oldRoot.GetValue(RootSourceProperty);
			oldRoot.ClearValue(RootSourceProperty);
		}
		newRoot?.SetValue(RootSourceProperty, this);
		UIElement uIElement = oldRoot as UIElement;
		UIElement uIElement2 = newRoot as UIElement;
		uIElement?.UpdateIsVisibleCache();
		uIElement2?.UpdateIsVisibleCache();
		uIElement?.OnPresentationSourceChanged(attached: false);
		uIElement2?.OnPresentationSourceChanged(attached: true);
		WeakReferenceListEnumerator enumerator = _watchers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			DependencyObject dependencyObject = (DependencyObject)enumerator.Current;
			if (dependencyObject.Dispatcher == base.Dispatcher)
			{
				PresentationSource presentationSource2 = (PresentationSource)dependencyObject.GetValue(CachedSourceProperty);
				if (presentationSource == presentationSource2 || presentationSource2 == null)
				{
					UpdateSourceOfElement(dependencyObject, null, null);
				}
			}
		}
	}

	/// <summary>Adds a <see cref="T:System.Windows.PresentationSource" /> derived class instance to the list of known presentation sources.</summary>
	protected void AddSource()
	{
		_sources.Add(this);
	}

	/// <summary>Removes a <see cref="T:System.Windows.PresentationSource" /> derived class instance from the list of known presentation sources.</summary>
	protected void RemoveSource()
	{
		_sources.Remove(this);
	}

	/// <summary>Sets the list of listeners for the <see cref="E:System.Windows.PresentationSource.ContentRendered" /> event to null. </summary>
	protected void ClearContentRenderedListeners()
	{
		this.ContentRendered = null;
	}

	internal static void OnVisualAncestorChanged(DependencyObject uie, AncestorChangedEventArgs e)
	{
		if ((bool)uie.GetValue(GetsSourceChangedEventProperty))
		{
			UpdateSourceOfElement(uie, e.Ancestor, e.OldParent);
		}
	}

	[FriendAccessAllowed]
	internal static PresentationSource CriticalFromVisual(DependencyObject v)
	{
		return CriticalFromVisual(v, enable2DTo3DTransition: true);
	}

	[FriendAccessAllowed]
	internal static PresentationSource CriticalFromVisual(DependencyObject v, bool enable2DTo3DTransition)
	{
		if (v == null)
		{
			throw new ArgumentNullException("v");
		}
		PresentationSource presentationSource = FindSource(v, enable2DTo3DTransition);
		if (presentationSource != null && presentationSource.IsDisposed)
		{
			presentationSource = null;
		}
		return presentationSource;
	}

	internal static object FireContentRendered(object arg)
	{
		PresentationSource presentationSource = (PresentationSource)arg;
		if (presentationSource.ContentRendered != null)
		{
			presentationSource.ContentRendered(arg, EventArgs.Empty);
		}
		return null;
	}

	[FriendAccessAllowed]
	internal static bool UnderSamePresentationSource(params DependencyObject[] visuals)
	{
		if (visuals == null || visuals.Length == 0)
		{
			return true;
		}
		PresentationSource presentationSource = CriticalFromVisual(visuals[0]);
		int num = visuals.Length;
		for (int i = 1; i < num; i++)
		{
			if (CriticalFromVisual(visuals[i]) != presentationSource)
			{
				return false;
			}
		}
		return true;
	}

	private static void AddElementToWatchList(DependencyObject element)
	{
		if (_watchers.Add(element))
		{
			element.SetValue(CachedSourceProperty, FindSource(element));
			element.SetValue(GetsSourceChangedEventProperty, value: true);
		}
	}

	private static void RemoveElementFromWatchList(DependencyObject element)
	{
		if (_watchers.Remove(element))
		{
			element.ClearValue(CachedSourceProperty);
			element.ClearValue(GetsSourceChangedEventProperty);
		}
	}

	private static PresentationSource FindSource(DependencyObject o)
	{
		return FindSource(o, enable2DTo3DTransition: true);
	}

	private static PresentationSource FindSource(DependencyObject o, bool enable2DTo3DTransition)
	{
		PresentationSource result = null;
		DependencyObject rootVisual = InputElement.GetRootVisual(o, enable2DTo3DTransition);
		if (rootVisual != null)
		{
			result = (PresentationSource)rootVisual.GetValue(RootSourceProperty);
		}
		return result;
	}

	private static bool UpdateSourceOfElement(DependencyObject doTarget, DependencyObject doAncestor, DependencyObject doOldParent)
	{
		bool result = false;
		PresentationSource presentationSource = FindSource(doTarget);
		PresentationSource presentationSource2 = (PresentationSource)doTarget.GetValue(CachedSourceProperty);
		if (presentationSource2 != presentationSource)
		{
			doTarget.SetValue(CachedSourceProperty, presentationSource);
			SourceChangedEventArgs sourceChangedEventArgs = new SourceChangedEventArgs(presentationSource2, presentationSource);
			sourceChangedEventArgs.RoutedEvent = SourceChangedEvent;
			if (doTarget is UIElement uIElement)
			{
				uIElement.RaiseEvent(sourceChangedEventArgs);
			}
			else if (doTarget is ContentElement contentElement)
			{
				contentElement.RaiseEvent(sourceChangedEventArgs);
			}
			else
			{
				if (!(doTarget is UIElement3D uIElement3D))
				{
					throw new InvalidOperationException(SR.Format(SR.Invalid_IInputElement, doTarget.GetType()));
				}
				uIElement3D.RaiseEvent(sourceChangedEventArgs);
			}
			result = true;
		}
		return result;
	}
}
