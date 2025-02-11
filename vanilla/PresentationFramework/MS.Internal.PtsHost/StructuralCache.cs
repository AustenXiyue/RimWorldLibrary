using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.PtsHost.UnsafeNativeMethods;

namespace MS.Internal.PtsHost;

internal sealed class StructuralCache
{
	[Flags]
	private enum Flags
	{
		FormattedOnce = 1,
		ContentChangeInProgress = 2,
		FormattingInProgress = 8
	}

	internal abstract class DocumentOperationContext
	{
		protected readonly StructuralCache _owner;

		internal Size DocumentPageSize => _owner._currentPage.Size;

		internal Thickness DocumentPageMargin => _owner._currentPage.Margin;

		internal DocumentOperationContext(StructuralCache owner, FlowDocumentPage page)
		{
			Invariant.Assert(owner != null, "Invalid owner object.");
			Invariant.Assert(page != null, "Invalid page object.");
			Invariant.Assert(owner._currentPage == null, "Page formatting reentrancy detected. Trying to create second _DocumentPageContext for the same StructuralCache.");
			_owner = owner;
			_owner._currentPage = page;
			_owner._illegalTreeChangeDetected = false;
			owner.PtsContext.Enter();
		}

		protected void Dispose()
		{
			Invariant.Assert(_owner._currentPage != null, "DocumentPageContext is already disposed.");
			try
			{
				_owner.PtsContext.Leave();
			}
			finally
			{
				_owner._currentPage = null;
			}
		}
	}

	internal class DocumentFormatContext : DocumentOperationContext, IDisposable
	{
		private struct DocumentFormatInfo
		{
			internal Size PageSize;

			internal Thickness PageMargin;

			internal bool IncrementalUpdate;

			internal bool FinitePage;
		}

		private DocumentFormatInfo _currentFormatInfo;

		private Stack<DocumentFormatInfo> _documentFormatInfoStack = new Stack<DocumentFormatInfo>();

		internal double PageHeight => _currentFormatInfo.PageSize.Height;

		internal double PageWidth => _currentFormatInfo.PageSize.Width;

		internal Size PageSize => _currentFormatInfo.PageSize;

		internal Thickness PageMargin => _currentFormatInfo.PageMargin;

		internal bool IncrementalUpdate => _currentFormatInfo.IncrementalUpdate;

		internal bool FinitePage => _currentFormatInfo.FinitePage;

		internal PTS.FSRECT PageRect => new PTS.FSRECT(new Rect(0.0, 0.0, PageWidth, PageHeight));

		internal PTS.FSRECT PageMarginRect => new PTS.FSRECT(new Rect(PageMargin.Left, PageMargin.Top, PageSize.Width - PageMargin.Left - PageMargin.Right, PageSize.Height - PageMargin.Top - PageMargin.Bottom));

		internal TextPointer DependentMax
		{
			set
			{
				_owner._currentPage.DependentMax = value;
			}
		}

		internal DocumentFormatContext(StructuralCache owner, FlowDocumentPage page)
			: base(owner, page)
		{
			_owner._documentFormatContext = this;
		}

		void IDisposable.Dispose()
		{
			_owner._documentFormatContext = null;
			Dispose();
			GC.SuppressFinalize(this);
		}

		internal void OnFormatLine()
		{
			_owner._currentPage.OnFormatLine();
		}

		internal void PushNewPageData(Size pageSize, Thickness pageMargin, bool incrementalUpdate, bool finitePage)
		{
			_documentFormatInfoStack.Push(_currentFormatInfo);
			_currentFormatInfo.PageSize = pageSize;
			_currentFormatInfo.PageMargin = pageMargin;
			_currentFormatInfo.IncrementalUpdate = incrementalUpdate;
			_currentFormatInfo.FinitePage = finitePage;
		}

		internal void PopPageData()
		{
			_currentFormatInfo = _documentFormatInfoStack.Pop();
		}
	}

	internal class DocumentArrangeContext : DocumentOperationContext, IDisposable
	{
		private struct DocumentArrangeInfo
		{
			internal PageContext PageContext;

			internal PTS.FSRECT ColumnRect;

			internal bool FinitePage;
		}

		private DocumentArrangeInfo _currentArrangeInfo;

		private Stack<DocumentArrangeInfo> _documentArrangeInfoStack = new Stack<DocumentArrangeInfo>();

		internal PageContext PageContext => _currentArrangeInfo.PageContext;

		internal PTS.FSRECT ColumnRect => _currentArrangeInfo.ColumnRect;

		internal bool FinitePage => _currentArrangeInfo.FinitePage;

		internal DocumentArrangeContext(StructuralCache owner, FlowDocumentPage page)
			: base(owner, page)
		{
			_owner._documentArrangeContext = this;
		}

		internal void PushNewPageData(PageContext pageContext, PTS.FSRECT columnRect, bool finitePage)
		{
			_documentArrangeInfoStack.Push(_currentArrangeInfo);
			_currentArrangeInfo.PageContext = pageContext;
			_currentArrangeInfo.ColumnRect = columnRect;
			_currentArrangeInfo.FinitePage = finitePage;
		}

		internal void PopPageData()
		{
			_currentArrangeInfo = _documentArrangeInfoStack.Pop();
		}

		void IDisposable.Dispose()
		{
			GC.SuppressFinalize(this);
			_owner._documentArrangeContext = null;
			Dispose();
		}
	}

	internal class DocumentVisualValidationContext : DocumentOperationContext, IDisposable
	{
		internal DocumentVisualValidationContext(StructuralCache owner, FlowDocumentPage page)
			: base(owner, page)
		{
		}

		void IDisposable.Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose();
		}
	}

	private readonly FlowDocument _owner;

	private PtsContext _ptsContext;

	private Section _section;

	private TextContainer _textContainer;

	private TextFormatterHost _textFormatterHost;

	private FlowDocumentPage _currentPage;

	private DocumentFormatContext _documentFormatContext;

	private DocumentArrangeContext _documentArrangeContext;

	private DtrList _dtrs;

	private bool _illegalTreeChangeDetected;

	private bool _forceReformat;

	private bool _destroyStructure;

	private BackgroundFormatInfo _backgroundFormatInfo;

	private FlowDirection _pageFlowDirection;

	private NaturalLanguageHyphenator _hyphenator;

	private Flags _flags;

	internal DependencyObject PropertyOwner => _textContainer.Parent;

	internal FlowDocument FormattingOwner => _owner;

	internal Section Section
	{
		get
		{
			EnsurePtsContext();
			return _section;
		}
	}

	internal NaturalLanguageHyphenator Hyphenator
	{
		get
		{
			EnsureHyphenator();
			return _hyphenator;
		}
	}

	internal PtsContext PtsContext
	{
		get
		{
			EnsurePtsContext();
			return _ptsContext;
		}
	}

	internal DocumentFormatContext CurrentFormatContext => _documentFormatContext;

	internal DocumentArrangeContext CurrentArrangeContext => _documentArrangeContext;

	internal TextFormatterHost TextFormatterHost
	{
		get
		{
			EnsurePtsContext();
			return _textFormatterHost;
		}
	}

	internal TextContainer TextContainer => _textContainer;

	internal FlowDirection PageFlowDirection
	{
		get
		{
			return _pageFlowDirection;
		}
		set
		{
			_pageFlowDirection = value;
		}
	}

	internal bool ForceReformat
	{
		get
		{
			return _forceReformat;
		}
		set
		{
			_forceReformat = value;
		}
	}

	internal bool DestroyStructure => _destroyStructure;

	internal DtrList DtrList => _dtrs;

	internal bool IsDeferredVisualCreationSupported
	{
		get
		{
			if (_currentPage != null)
			{
				return !_currentPage.FinitePage;
			}
			return false;
		}
	}

	internal BackgroundFormatInfo BackgroundFormatInfo => _backgroundFormatInfo;

	internal bool IsOptimalParagraphEnabled
	{
		get
		{
			if (PtsContext.IsOptimalParagraphEnabled)
			{
				return (bool)PropertyOwner.GetValue(FlowDocument.IsOptimalParagraphEnabledProperty);
			}
			return false;
		}
	}

	internal bool IsFormattingInProgress
	{
		get
		{
			return CheckFlags(Flags.FormattingInProgress);
		}
		set
		{
			SetFlags(value, Flags.FormattingInProgress);
		}
	}

	internal bool IsContentChangeInProgress
	{
		get
		{
			return CheckFlags(Flags.ContentChangeInProgress);
		}
		set
		{
			SetFlags(value, Flags.ContentChangeInProgress);
		}
	}

	internal bool IsFormattedOnce
	{
		get
		{
			return CheckFlags(Flags.FormattedOnce);
		}
		set
		{
			SetFlags(value, Flags.FormattedOnce);
		}
	}

	internal StructuralCache(FlowDocument owner, TextContainer textContainer)
	{
		Invariant.Assert(owner != null);
		Invariant.Assert(textContainer != null);
		Invariant.Assert(textContainer.Parent != null);
		_owner = owner;
		_textContainer = textContainer;
		_backgroundFormatInfo = new BackgroundFormatInfo(this);
	}

	~StructuralCache()
	{
		if (_ptsContext != null)
		{
			PtsCache.ReleaseContext(_ptsContext);
		}
	}

	internal IDisposable SetDocumentFormatContext(FlowDocumentPage currentPage)
	{
		if (!CheckFlags(Flags.FormattedOnce))
		{
			SetFlags(value: true, Flags.FormattedOnce);
			_owner.InitializeForFirstFormatting();
		}
		return new DocumentFormatContext(this, currentPage);
	}

	internal IDisposable SetDocumentArrangeContext(FlowDocumentPage currentPage)
	{
		return new DocumentArrangeContext(this, currentPage);
	}

	internal IDisposable SetDocumentVisualValidationContext(FlowDocumentPage currentPage)
	{
		return new DocumentVisualValidationContext(this, currentPage);
	}

	internal void DetectInvalidOperation()
	{
		if (_illegalTreeChangeDetected)
		{
			throw new InvalidOperationException(SR.IllegalTreeChangeDetectedPostAction);
		}
	}

	internal void OnInvalidOperationDetected()
	{
		if (_currentPage != null)
		{
			_illegalTreeChangeDetected = true;
		}
	}

	internal void InvalidateFormatCache(bool destroyStructure)
	{
		if (_section != null)
		{
			_section.InvalidateFormatCache();
			_destroyStructure |= destroyStructure;
			_forceReformat = true;
		}
	}

	internal void AddDirtyTextRange(DirtyTextRange dtr)
	{
		if (_dtrs == null)
		{
			_dtrs = new DtrList();
		}
		_dtrs.Merge(dtr);
	}

	internal DtrList DtrsFromRange(int dcpNew, int cchOld)
	{
		if (_dtrs == null)
		{
			return null;
		}
		return _dtrs.DtrsFromRange(dcpNew, cchOld);
	}

	internal void ClearUpdateInfo(bool destroyStructureCache)
	{
		_dtrs = null;
		_forceReformat = false;
		_destroyStructure = false;
		if (_section != null && !_ptsContext.Disposed)
		{
			if (destroyStructureCache)
			{
				_section.DestroyStructure();
			}
			_section.ClearUpdateInfo();
		}
	}

	internal void ThrottleBackgroundFormatting()
	{
		_backgroundFormatInfo.ThrottleBackgroundFormatting();
	}

	internal bool HasPtsContext()
	{
		return _ptsContext != null;
	}

	private void EnsureHyphenator()
	{
		if (_hyphenator == null)
		{
			_hyphenator = new NaturalLanguageHyphenator();
		}
	}

	private void EnsurePtsContext()
	{
		if (_ptsContext == null)
		{
			TextFormattingMode textFormattingMode = TextOptions.GetTextFormattingMode(PropertyOwner);
			_ptsContext = new PtsContext(isOptimalParagraphEnabled: true, textFormattingMode);
			_textFormatterHost = new TextFormatterHost(_ptsContext.TextFormatter, textFormattingMode, _owner.PixelsPerDip);
			_section = new Section(this);
		}
	}

	private void SetFlags(bool value, Flags flags)
	{
		_flags = (value ? (_flags | flags) : (_flags & ~flags));
	}

	private bool CheckFlags(Flags flags)
	{
		return (_flags & flags) == flags;
	}
}
