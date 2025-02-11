using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using MS.Internal.Documents;

namespace MS.Internal.PtsHost;

internal sealed class BreakRecordTable
{
	private class BreakRecordTableEntry
	{
		public PageBreakRecord BreakRecord;

		public ReadOnlyCollection<TextSegment> TextSegments;

		public WeakReference DocumentPage;

		public TextPointer DependentMax;
	}

	private FlowDocumentPaginator _owner;

	private List<BreakRecordTableEntry> _breakRecords;

	internal int Count => _breakRecords.Count;

	internal bool IsClean
	{
		get
		{
			if (_breakRecords.Count == 0)
			{
				return false;
			}
			Invariant.Assert(_breakRecords[_breakRecords.Count - 1] != null, "Invalid BreakRecordTable entry.");
			return _breakRecords[_breakRecords.Count - 1].BreakRecord == null;
		}
	}

	internal BreakRecordTable(FlowDocumentPaginator owner)
	{
		_owner = owner;
		_breakRecords = new List<BreakRecordTableEntry>();
	}

	internal PageBreakRecord GetPageBreakRecord(int pageNumber)
	{
		PageBreakRecord pageBreakRecord = null;
		Invariant.Assert(pageNumber >= 0 && pageNumber <= _breakRecords.Count, "Invalid PageNumber.");
		if (pageNumber > 0)
		{
			Invariant.Assert(_breakRecords[pageNumber - 1] != null, "Invalid BreakRecordTable entry.");
			pageBreakRecord = _breakRecords[pageNumber - 1].BreakRecord;
			Invariant.Assert(pageBreakRecord != null, "BreakRecord can be null only for the first page.");
		}
		return pageBreakRecord;
	}

	internal FlowDocumentPage GetCachedDocumentPage(int pageNumber)
	{
		FlowDocumentPage flowDocumentPage = null;
		if (pageNumber < _breakRecords.Count)
		{
			Invariant.Assert(_breakRecords[pageNumber] != null, "Invalid BreakRecordTable entry.");
			WeakReference documentPage = _breakRecords[pageNumber].DocumentPage;
			if (documentPage != null)
			{
				flowDocumentPage = documentPage.Target as FlowDocumentPage;
				if (flowDocumentPage != null && flowDocumentPage.IsDisposed)
				{
					flowDocumentPage = null;
				}
			}
		}
		return flowDocumentPage;
	}

	internal bool GetPageNumberForContentPosition(TextPointer contentPosition, ref int pageNumber)
	{
		bool result = false;
		Invariant.Assert(pageNumber >= 0 && pageNumber <= _breakRecords.Count, "Invalid PageNumber.");
		while (pageNumber < _breakRecords.Count)
		{
			Invariant.Assert(_breakRecords[pageNumber] != null, "Invalid BreakRecordTable entry.");
			ReadOnlyCollection<TextSegment> textSegments = _breakRecords[pageNumber].TextSegments;
			if (textSegments == null)
			{
				break;
			}
			if (TextDocumentView.Contains(contentPosition, textSegments))
			{
				result = true;
				break;
			}
			pageNumber++;
		}
		return result;
	}

	internal void OnInvalidateLayout()
	{
		if (_breakRecords.Count > 0)
		{
			InvalidateBreakRecords(0, _breakRecords.Count);
			_owner.InitiateNextAsyncOperation();
			_owner.OnPagesChanged(0, 1073741823);
		}
	}

	internal void OnInvalidateLayout(ITextPointer start, ITextPointer end)
	{
		if (_breakRecords.Count > 0)
		{
			GetAffectedPages(start, end, out var pageStart, out var pageCount);
			pageCount = _breakRecords.Count - pageStart;
			if (pageCount > 0)
			{
				InvalidateBreakRecords(pageStart, pageCount);
				_owner.InitiateNextAsyncOperation();
				_owner.OnPagesChanged(pageStart, 1073741823);
			}
		}
	}

	internal void OnInvalidateRender()
	{
		if (_breakRecords.Count > 0)
		{
			DisposePages(0, _breakRecords.Count);
			_owner.OnPagesChanged(0, _breakRecords.Count);
		}
	}

	internal void OnInvalidateRender(ITextPointer start, ITextPointer end)
	{
		if (_breakRecords.Count > 0)
		{
			GetAffectedPages(start, end, out var pageStart, out var pageCount);
			if (pageCount > 0)
			{
				DisposePages(pageStart, pageCount);
				_owner.OnPagesChanged(pageStart, pageCount);
			}
		}
	}

	internal void UpdateEntry(int pageNumber, FlowDocumentPage page, PageBreakRecord brOut, TextPointer dependentMax)
	{
		Invariant.Assert(pageNumber >= 0 && pageNumber <= _breakRecords.Count, "The previous BreakRecord does not exist.");
		Invariant.Assert(page != null && page != DocumentPage.Missing, "Cannot update BRT with an invalid document page.");
		ITextView textView = (ITextView)((IServiceProvider)page).GetService(typeof(ITextView));
		Invariant.Assert(textView != null, "Cannot access ITextView for FlowDocumentPage.");
		bool isClean = IsClean;
		BreakRecordTableEntry breakRecordTableEntry = new BreakRecordTableEntry
		{
			BreakRecord = brOut,
			DocumentPage = new WeakReference(page),
			TextSegments = textView.TextSegments,
			DependentMax = dependentMax
		};
		if (pageNumber == _breakRecords.Count)
		{
			_breakRecords.Add(breakRecordTableEntry);
			_owner.OnPaginationProgress(pageNumber, 1);
		}
		else
		{
			if (_breakRecords[pageNumber].BreakRecord != null && _breakRecords[pageNumber].BreakRecord != breakRecordTableEntry.BreakRecord)
			{
				_breakRecords[pageNumber].BreakRecord.Dispose();
			}
			if (_breakRecords[pageNumber].DocumentPage != null && _breakRecords[pageNumber].DocumentPage.Target != null && _breakRecords[pageNumber].DocumentPage.Target != breakRecordTableEntry.DocumentPage.Target)
			{
				((FlowDocumentPage)_breakRecords[pageNumber].DocumentPage.Target).Dispose();
			}
			_breakRecords[pageNumber] = breakRecordTableEntry;
		}
		if (!isClean && IsClean)
		{
			_owner.OnPaginationCompleted();
		}
	}

	internal bool HasPageBreakRecord(int pageNumber)
	{
		Invariant.Assert(pageNumber >= 0, "Page number cannot be negative.");
		if (pageNumber == 0)
		{
			return true;
		}
		if (pageNumber > _breakRecords.Count)
		{
			return false;
		}
		Invariant.Assert(_breakRecords[pageNumber - 1] != null, "Invalid BreakRecordTable entry.");
		return _breakRecords[pageNumber - 1].BreakRecord != null;
	}

	private void DisposePages(int start, int count)
	{
		int num = start + count - 1;
		Invariant.Assert(start >= 0 && start < _breakRecords.Count, "Invalid starting index for BreakRecordTable invalidation.");
		Invariant.Assert(start + count <= _breakRecords.Count, "Partial invalidation of BreakRecordTable is not allowed.");
		while (num >= start)
		{
			Invariant.Assert(_breakRecords[num] != null, "Invalid BreakRecordTable entry.");
			WeakReference documentPage = _breakRecords[num].DocumentPage;
			if (documentPage != null && documentPage.Target != null)
			{
				((FlowDocumentPage)documentPage.Target).Dispose();
			}
			_breakRecords[num].DocumentPage = null;
			num--;
		}
	}

	private void InvalidateBreakRecords(int start, int count)
	{
		int num = start + count - 1;
		Invariant.Assert(start >= 0 && start < _breakRecords.Count, "Invalid starting index for BreakRecordTable invalidation.");
		Invariant.Assert(start + count == _breakRecords.Count, "Partial invalidation of BreakRecordTable is not allowed.");
		while (num >= start)
		{
			Invariant.Assert(_breakRecords[num] != null, "Invalid BreakRecordTable entry.");
			WeakReference documentPage = _breakRecords[num].DocumentPage;
			if (documentPage != null && documentPage.Target != null)
			{
				((FlowDocumentPage)documentPage.Target).Dispose();
			}
			if (_breakRecords[num].BreakRecord != null)
			{
				_breakRecords[num].BreakRecord.Dispose();
			}
			_breakRecords.RemoveAt(num);
			num--;
		}
	}

	private void GetAffectedPages(ITextPointer start, ITextPointer end, out int pageStart, out int pageCount)
	{
		for (pageStart = 0; pageStart < _breakRecords.Count; pageStart++)
		{
			Invariant.Assert(_breakRecords[pageStart] != null, "Invalid BreakRecordTable entry.");
			TextPointer dependentMax = _breakRecords[pageStart].DependentMax;
			if (dependentMax != null && start.CompareTo(dependentMax) <= 0)
			{
				break;
			}
			ReadOnlyCollection<TextSegment> textSegments = _breakRecords[pageStart].TextSegments;
			if (textSegments == null)
			{
				break;
			}
			bool flag = false;
			foreach (TextSegment item in textSegments)
			{
				if (start.CompareTo(item.End) <= 0)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		pageCount = _breakRecords.Count - pageStart;
	}
}
