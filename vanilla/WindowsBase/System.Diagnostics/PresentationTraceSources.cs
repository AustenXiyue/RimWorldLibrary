using System.Windows;
using MS.Internal;

namespace System.Diagnostics;

/// <summary>Provides debug tracing support that is specifically targeted for Windows Presentation Foundation (WPF) applications. </summary>
public static class PresentationTraceSources
{
	internal static TraceSource _DependencyPropertySource;

	internal static TraceSource _FreezableSource;

	internal static TraceSource _NameScopeSource;

	internal static TraceSource _RoutedEventSource;

	internal static TraceSource _AnimationSource;

	internal static TraceSource _DataBindingSource;

	internal static TraceSource _DocumentsSource;

	internal static TraceSource _ResourceDictionarySource;

	internal static TraceSource _MarkupSource;

	internal static TraceSource _HwndHostSource;

	internal static TraceSource _ShellSource;

	/// <summary>Identifies the <see cref="P:System.Diagnostics.PresentationTraceSources.TraceLevel" /> attached property.</summary>
	public static readonly DependencyProperty TraceLevelProperty = DependencyProperty.RegisterAttached("TraceLevel", typeof(PresentationTraceLevel), typeof(PresentationTraceSources));

	/// <summary>Gets a dependency property trace source.</summary>
	/// <returns>A dependency property trace source.</returns>
	public static TraceSource DependencyPropertySource
	{
		get
		{
			if (_DependencyPropertySource == null)
			{
				_DependencyPropertySource = CreateTraceSource("System.Windows.DependencyProperty");
			}
			return _DependencyPropertySource;
		}
	}

	/// <summary>Gets a Freezable trace source.</summary>
	/// <returns>A Freezable trace source.</returns>
	public static TraceSource FreezableSource
	{
		get
		{
			if (_FreezableSource == null)
			{
				_FreezableSource = CreateTraceSource("System.Windows.Freezable");
			}
			return _FreezableSource;
		}
	}

	/// <summary>Gets a name scope trace source.</summary>
	/// <returns>A name scope trace source.</returns>
	public static TraceSource NameScopeSource
	{
		get
		{
			if (_NameScopeSource == null)
			{
				_NameScopeSource = CreateTraceSource("System.Windows.NameScope");
			}
			return _NameScopeSource;
		}
	}

	/// <summary>Gets a routed event trace source.</summary>
	/// <returns>A routed event trace source.</returns>
	public static TraceSource RoutedEventSource
	{
		get
		{
			if (_RoutedEventSource == null)
			{
				_RoutedEventSource = CreateTraceSource("System.Windows.RoutedEvent");
			}
			return _RoutedEventSource;
		}
	}

	/// <summary>Gets an animation trace source.</summary>
	/// <returns>An animation trace source.</returns>
	public static TraceSource AnimationSource
	{
		get
		{
			if (_AnimationSource == null)
			{
				_AnimationSource = CreateTraceSource("System.Windows.Media.Animation");
			}
			return _AnimationSource;
		}
	}

	/// <summary>Gets a data-binding trace source.</summary>
	/// <returns>A data-binding trace source.</returns>
	public static TraceSource DataBindingSource
	{
		get
		{
			if (_DataBindingSource == null)
			{
				_DataBindingSource = CreateTraceSource("System.Windows.Data");
			}
			return _DataBindingSource;
		}
	}

	/// <summary>Gets a document trace source.</summary>
	/// <returns>A document trace source.</returns>
	public static TraceSource DocumentsSource
	{
		get
		{
			if (_DocumentsSource == null)
			{
				_DocumentsSource = CreateTraceSource("System.Windows.Documents");
			}
			return _DocumentsSource;
		}
	}

	/// <summary>Gets a resource dictionary trace source.</summary>
	/// <returns>A resource dictionary trace source.</returns>
	public static TraceSource ResourceDictionarySource
	{
		get
		{
			if (_ResourceDictionarySource == null)
			{
				_ResourceDictionarySource = CreateTraceSource("System.Windows.ResourceDictionary");
			}
			return _ResourceDictionarySource;
		}
	}

	/// <summary>Gets a markup trace source.</summary>
	/// <returns>A markup trace source.</returns>
	public static TraceSource MarkupSource
	{
		get
		{
			if (_MarkupSource == null)
			{
				_MarkupSource = CreateTraceSource("System.Windows.Markup");
			}
			return _MarkupSource;
		}
	}

	/// <summary>Gets an hwnd host trace source.</summary>
	/// <returns>An hwnd host trace source.</returns>
	public static TraceSource HwndHostSource
	{
		get
		{
			if (_HwndHostSource == null)
			{
				_HwndHostSource = CreateTraceSource("System.Windows.Interop.HwndHost");
			}
			return _HwndHostSource;
		}
	}

	/// <summary>Gets a shell trace source.</summary>
	/// <returns>A shell trace source.</returns>
	public static TraceSource ShellSource
	{
		get
		{
			if (_ShellSource == null)
			{
				_ShellSource = CreateTraceSource("System.Windows.Shell");
			}
			return _ShellSource;
		}
	}

	internal static event TraceRefreshEventHandler TraceRefresh;

	/// <summary>Gets the value of the <see cref="P:System.Diagnostics.PresentationTraceSources.TraceLevel" /> attached property for a specified element.</summary>
	/// <returns>The <see cref="P:System.Diagnostics.PresentationTraceSources.TraceLevel" /> property value for the element.</returns>
	/// <param name="element">The element from which the property value is read.</param>
	public static PresentationTraceLevel GetTraceLevel(object element)
	{
		return TraceLevelStore.GetTraceLevel(element);
	}

	/// <summary>Sets the value of the <see cref="P:System.Diagnostics.PresentationTraceSources.TraceLeve" />l attached property to a specified element.</summary>
	/// <param name="element">The element to which the attached property is written.</param>
	/// <param name="traceLevel">The needed <see cref="T:System.Diagnostics.PresentationTraceLevel" /> value.</param>
	public static void SetTraceLevel(object element, PresentationTraceLevel traceLevel)
	{
		TraceLevelStore.SetTraceLevel(element, traceLevel);
	}

	/// <summary>Refreshes trace sources, by forcing the app.config file to be re-read.</summary>
	public static void Refresh()
	{
		AvTrace.OnRefresh();
		Trace.Refresh();
		if (PresentationTraceSources.TraceRefresh != null)
		{
			PresentationTraceSources.TraceRefresh();
		}
	}

	private static TraceSource CreateTraceSource(string sourceName)
	{
		TraceSource traceSource = new TraceSource(sourceName);
		if (traceSource.Switch.Level == SourceLevels.Off && AvTrace.IsDebuggerAttached())
		{
			traceSource.Switch.Level = SourceLevels.Warning;
		}
		return traceSource;
	}
}
