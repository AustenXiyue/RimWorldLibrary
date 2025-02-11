using System.Collections;
using System.Text;
using MS.Internal;

namespace System.Windows.Documents;

internal class DocumentNodeArray : ArrayList
{
	private bool _fMain;

	private DocumentNodeArray _dnaOpen;

	internal DocumentNode Top
	{
		get
		{
			if (Count <= 0)
			{
				return null;
			}
			return EntryAt(Count - 1);
		}
	}

	internal bool IsMain
	{
		set
		{
			_fMain = value;
		}
	}

	internal DocumentNodeArray()
		: base(100)
	{
		_fMain = false;
		_dnaOpen = null;
	}

	internal DocumentNode EntryAt(int nAt)
	{
		return (DocumentNode)this[nAt];
	}

	internal void Push(DocumentNode documentNode)
	{
		InsertNode(Count, documentNode);
	}

	internal DocumentNode Pop()
	{
		DocumentNode top = Top;
		if (Count > 0)
		{
			Excise(Count - 1, 1);
		}
		return top;
	}

	internal DocumentNode TopPending()
	{
		for (int num = Count - 1; num >= 0; num--)
		{
			DocumentNode documentNode = EntryAt(num);
			if (documentNode.IsPending)
			{
				return documentNode;
			}
		}
		return null;
	}

	internal bool TestTop(DocumentNodeType documentNodeType)
	{
		if (Count > 0)
		{
			return EntryAt(Count - 1).Type == documentNodeType;
		}
		return false;
	}

	internal void PreCoalesceChildren(ConverterState converterState, int nStart, bool bChild)
	{
		DocumentNodeArray documentNodeArray = new DocumentNodeArray();
		bool fVMerged = false;
		int num = EntryAt(nStart).ChildCount;
		if (nStart + num >= Count)
		{
			num = Count - nStart - 1;
		}
		int num2 = nStart + num;
		if (bChild)
		{
			nStart++;
		}
		for (int i = nStart; i <= num2; i++)
		{
			DocumentNode documentNode = EntryAt(i);
			if (documentNode.IsInline && documentNode.RequiresXamlDir && documentNode.ClosedParent != null)
			{
				int j;
				for (j = i + 1; j <= num2; j++)
				{
					DocumentNode documentNode2 = EntryAt(j);
					if (!documentNode2.IsInline || documentNode2.Type == DocumentNodeType.dnHyperlink || documentNode2.FormatState.DirChar != documentNode.FormatState.DirChar || documentNode2.ClosedParent != documentNode.ClosedParent)
					{
						break;
					}
				}
				int num3 = j - i;
				if (num3 > 1)
				{
					DocumentNode documentNode3 = new DocumentNode(DocumentNodeType.dnInline);
					documentNode3.FormatState = new FormatState(documentNode.Parent.FormatState);
					documentNode3.FormatState.DirChar = documentNode.FormatState.DirChar;
					InsertChildAt(documentNode.ClosedParent, documentNode3, i, num3);
					num2++;
				}
			}
			else if (documentNode.Type == DocumentNodeType.dnListItem)
			{
				PreCoalesceListItem(documentNode);
			}
			else if (documentNode.Type == DocumentNodeType.dnList)
			{
				PreCoalesceList(documentNode);
			}
			else if (documentNode.Type == DocumentNodeType.dnTable)
			{
				documentNodeArray.Add(documentNode);
				num2 += PreCoalesceTable(documentNode);
			}
			else if (documentNode.Type == DocumentNodeType.dnRow)
			{
				PreCoalesceRow(documentNode, ref fVMerged);
			}
		}
		if (fVMerged)
		{
			ProcessTableRowSpan(documentNodeArray);
		}
	}

	internal void CoalesceChildren(ConverterState converterState, int nStart)
	{
		if (nStart >= Count || nStart < 0)
		{
			return;
		}
		PreCoalesceChildren(converterState, nStart, bChild: false);
		int num = EntryAt(nStart).ChildCount;
		if (nStart + num >= Count)
		{
			num = Count - nStart - 1;
		}
		int num2 = nStart + num;
		for (int num3 = num2; num3 >= nStart; num3--)
		{
			DocumentNode documentNode = EntryAt(num3);
			if (documentNode.ChildCount == 0)
			{
				documentNode.Terminate(converterState);
			}
			else
			{
				documentNode.AppendXamlPrefix(converterState);
				StringBuilder stringBuilder = new StringBuilder(documentNode.Xaml);
				int childCount = documentNode.ChildCount;
				int num4 = num3 + childCount;
				for (int i = num3 + 1; i <= num4; i++)
				{
					DocumentNode documentNode2 = EntryAt(i);
					stringBuilder.Append(documentNode2.Xaml);
				}
				documentNode.Xaml = stringBuilder.ToString();
				documentNode.AppendXamlPostfix(converterState);
				documentNode.IsTerminated = true;
				Excise(num3 + 1, childCount);
				num2 -= childCount;
				AssertTreeInvariants();
			}
			if (documentNode.ColSpan == 0)
			{
				documentNode.Xaml = string.Empty;
			}
		}
	}

	internal void CoalesceOnlyChildren(ConverterState converterState, int nStart)
	{
		if (nStart >= Count || nStart < 0)
		{
			return;
		}
		PreCoalesceChildren(converterState, nStart, bChild: true);
		int num = EntryAt(nStart).ChildCount;
		if (nStart + num >= Count)
		{
			num = Count - nStart - 1;
		}
		int num2 = nStart + num;
		for (int num3 = num2; num3 >= nStart; num3--)
		{
			DocumentNode documentNode = EntryAt(num3);
			if (documentNode.ChildCount == 0 && num3 != nStart)
			{
				documentNode.Terminate(converterState);
			}
			else if (documentNode.ChildCount > 0)
			{
				if (num3 != nStart)
				{
					documentNode.AppendXamlPrefix(converterState);
				}
				StringBuilder stringBuilder = new StringBuilder(documentNode.Xaml);
				int childCount = documentNode.ChildCount;
				int num4 = num3 + childCount;
				for (int i = num3 + 1; i <= num4; i++)
				{
					DocumentNode documentNode2 = EntryAt(i);
					stringBuilder.Append(documentNode2.Xaml);
				}
				documentNode.Xaml = stringBuilder.ToString();
				if (num3 != nStart)
				{
					documentNode.AppendXamlPostfix(converterState);
					documentNode.IsTerminated = true;
				}
				Excise(num3 + 1, childCount);
				num2 -= childCount;
			}
		}
	}

	internal void CoalesceAll(ConverterState converterState)
	{
		for (int i = 0; i < Count; i++)
		{
			CoalesceChildren(converterState, i);
		}
	}

	internal void CloseAtHelper(int index, int nChildCount)
	{
		if (index >= Count || index < 0 || index + nChildCount >= Count)
		{
			return;
		}
		DocumentNode documentNode = EntryAt(index);
		if (documentNode.IsPending)
		{
			documentNode.IsPending = false;
			documentNode.ChildCount = nChildCount;
			int i = index + 1;
			DocumentNode documentNode2;
			for (int num = index + documentNode.ChildCount; i <= num; i += documentNode2.ChildCount + 1)
			{
				documentNode2 = EntryAt(i);
				documentNode2.Parent = documentNode;
			}
		}
	}

	internal void CloseAt(int index)
	{
		if (index >= Count || index < 0 || !EntryAt(index).IsPending)
		{
			return;
		}
		AssertTreeInvariants();
		AssertTreeSemanticInvariants();
		for (int num = Count - 1; num > index; num--)
		{
			if (EntryAt(num).IsPending)
			{
				CloseAt(num);
			}
		}
		CloseAtHelper(index, Count - index - 1);
		AssertTreeInvariants();
		AssertTreeSemanticInvariants();
	}

	internal void AssertTreeInvariants()
	{
		if (!Invariant.Strict)
		{
			return;
		}
		for (int i = 0; i < Count; i++)
		{
			DocumentNode documentNode = EntryAt(i);
			for (int j = i + 1; j <= documentNode.LastChildIndex; j++)
			{
			}
			for (DocumentNode parent = documentNode.Parent; parent != null; parent = parent.Parent)
			{
			}
		}
	}

	internal void AssertTreeSemanticInvariants()
	{
		if (!Invariant.Strict)
		{
			return;
		}
		for (int i = 0; i < Count; i++)
		{
			DocumentNode documentNode = EntryAt(i);
			_ = documentNode.Parent;
			switch (documentNode.Type)
			{
			}
		}
	}

	internal void CloseAll()
	{
		for (int i = 0; i < Count; i++)
		{
			if (EntryAt(i).IsPending)
			{
				CloseAt(i);
				break;
			}
		}
	}

	internal int CountOpenNodes(DocumentNodeType documentNodeType)
	{
		int num = 0;
		if (_dnaOpen != null)
		{
			_dnaOpen.CullOpen();
			for (int num2 = _dnaOpen.Count - 1; num2 >= 0; num2--)
			{
				DocumentNode documentNode = _dnaOpen.EntryAt(num2);
				if (documentNode.IsPending)
				{
					if (documentNode.Type == documentNodeType)
					{
						num++;
					}
					else if (documentNode.Type == DocumentNodeType.dnShape)
					{
						break;
					}
				}
			}
		}
		return num;
	}

	internal int CountOpenCells()
	{
		return CountOpenNodes(DocumentNodeType.dnCell);
	}

	internal DocumentNode GetOpenParentWhileParsing(DocumentNode dn)
	{
		if (_dnaOpen != null)
		{
			_dnaOpen.CullOpen();
			for (int num = _dnaOpen.Count - 1; num >= 0; num--)
			{
				DocumentNode documentNode = _dnaOpen.EntryAt(num);
				if (documentNode.IsPending && documentNode.Index < dn.Index)
				{
					return documentNode;
				}
			}
		}
		return null;
	}

	internal DocumentNodeType GetTableScope()
	{
		if (_dnaOpen != null)
		{
			_dnaOpen.CullOpen();
			for (int num = _dnaOpen.Count - 1; num >= 0; num--)
			{
				DocumentNode documentNode = _dnaOpen.EntryAt(num);
				if (documentNode.IsPending)
				{
					if (documentNode.Type == DocumentNodeType.dnTable || documentNode.Type == DocumentNodeType.dnTableBody || documentNode.Type == DocumentNodeType.dnRow || documentNode.Type == DocumentNodeType.dnCell)
					{
						return documentNode.Type;
					}
					if (documentNode.Type == DocumentNodeType.dnShape)
					{
						return DocumentNodeType.dnParagraph;
					}
				}
			}
		}
		return DocumentNodeType.dnParagraph;
	}

	internal MarkerList GetOpenMarkerStyles()
	{
		MarkerList markerList = new MarkerList();
		if (_dnaOpen != null)
		{
			_dnaOpen.CullOpen();
			int num = 0;
			for (int i = 0; i < _dnaOpen.Count; i++)
			{
				DocumentNode documentNode = _dnaOpen.EntryAt(i);
				if (documentNode.IsPending && documentNode.Type == DocumentNodeType.dnShape)
				{
					num = i + 1;
				}
			}
			for (int j = num; j < _dnaOpen.Count; j++)
			{
				DocumentNode documentNode2 = _dnaOpen.EntryAt(j);
				if (documentNode2.IsPending && documentNode2.Type == DocumentNodeType.dnList)
				{
					markerList.AddEntry(documentNode2.FormatState.Marker, documentNode2.FormatState.ILS, documentNode2.FormatState.StartIndex, documentNode2.FormatState.StartIndexDefault, documentNode2.VirtualListLevel);
				}
			}
		}
		return markerList;
	}

	internal MarkerList GetLastMarkerStyles(MarkerList mlHave, MarkerList mlWant)
	{
		MarkerList markerList = new MarkerList();
		if (mlHave.Count > 0 || mlWant.Count == 0)
		{
			return markerList;
		}
		bool flag = true;
		for (int num = Count - 1; num >= 0; num--)
		{
			DocumentNode documentNode = EntryAt(num);
			if (documentNode.Type == DocumentNodeType.dnCell || documentNode.Type == DocumentNodeType.dnTable)
			{
				break;
			}
			if (documentNode.Type == DocumentNodeType.dnListItem)
			{
				DocumentNode parentOfType = documentNode.GetParentOfType(DocumentNodeType.dnCell);
				if (parentOfType != null && !parentOfType.IsPending)
				{
					break;
				}
				DocumentNode parentOfType2 = documentNode.GetParentOfType(DocumentNodeType.dnShape);
				if (parentOfType2 == null || parentOfType2.IsPending)
				{
					for (DocumentNode parent = documentNode.Parent; parent != null; parent = parent.Parent)
					{
						if (parent.Type == DocumentNodeType.dnList)
						{
							MarkerListEntry markerListEntry = new MarkerListEntry();
							markerListEntry.Marker = parent.FormatState.Marker;
							markerListEntry.StartIndexOverride = parent.FormatState.StartIndex;
							markerListEntry.StartIndexDefault = parent.FormatState.StartIndexDefault;
							markerListEntry.VirtualListLevel = parent.VirtualListLevel;
							markerListEntry.ILS = parent.FormatState.ILS;
							markerList.Insert(0, markerListEntry);
							if (markerListEntry.Marker != MarkerStyle.MarkerBullet)
							{
								flag = false;
							}
						}
					}
					break;
				}
			}
		}
		if (markerList.Count == 1 && flag)
		{
			markerList.RemoveRange(0, 1);
		}
		return markerList;
	}

	internal void OpenLastList()
	{
		for (int num = Count - 1; num >= 0; num--)
		{
			DocumentNode documentNode = EntryAt(num);
			if (documentNode.Type == DocumentNodeType.dnListItem)
			{
				DocumentNode parentOfType = documentNode.GetParentOfType(DocumentNodeType.dnShape);
				if (parentOfType == null || parentOfType.IsPending)
				{
					for (DocumentNode documentNode2 = documentNode; documentNode2 != null; documentNode2 = documentNode2.Parent)
					{
						if (documentNode2.Type == DocumentNodeType.dnList || documentNode2.Type == DocumentNodeType.dnListItem)
						{
							documentNode2.IsPending = true;
							_dnaOpen.InsertOpenNode(documentNode2);
						}
					}
					break;
				}
			}
		}
	}

	internal void OpenLastCell()
	{
		for (int num = _dnaOpen.Count - 1; num >= 0; num--)
		{
			DocumentNode documentNode = _dnaOpen.EntryAt(num);
			if (documentNode.IsPending)
			{
				if (documentNode.Type == DocumentNodeType.dnCell)
				{
					break;
				}
				if (documentNode.Type == DocumentNodeType.dnTable || documentNode.Type == DocumentNodeType.dnTableBody || documentNode.Type == DocumentNodeType.dnRow)
				{
					for (int num2 = Count - 1; num2 >= 0; num2--)
					{
						DocumentNode documentNode2 = EntryAt(num2);
						if (documentNode2 == documentNode)
						{
							return;
						}
						if (documentNode2.Type == DocumentNodeType.dnCell && documentNode2.GetParentOfType(documentNode.Type) == documentNode)
						{
							DocumentNode documentNode3 = documentNode2;
							while (documentNode3 != null && documentNode3 != documentNode)
							{
								documentNode3.IsPending = true;
								_dnaOpen.InsertOpenNode(documentNode3);
								documentNode3 = documentNode3.Parent;
							}
							return;
						}
					}
				}
			}
		}
	}

	internal int FindPendingFrom(DocumentNodeType documentNodeType, int nStart, int nLow)
	{
		if (_dnaOpen != null)
		{
			_dnaOpen.CullOpen();
			for (int num = _dnaOpen.Count - 1; num >= 0; num--)
			{
				DocumentNode documentNode = _dnaOpen.EntryAt(num);
				if (documentNode.Index <= nStart)
				{
					if (documentNode.Index <= nLow)
					{
						break;
					}
					if (documentNode.IsPending)
					{
						if (documentNode.Type == documentNodeType)
						{
							return documentNode.Index;
						}
						if (documentNode.Type == DocumentNodeType.dnShape)
						{
							break;
						}
					}
				}
			}
		}
		return -1;
	}

	internal int FindPending(DocumentNodeType documentNodeType, int nLow)
	{
		return FindPendingFrom(documentNodeType, Count - 1, nLow);
	}

	internal int FindPending(DocumentNodeType documentNodeType)
	{
		return FindPending(documentNodeType, -1);
	}

	internal int FindUnmatched(DocumentNodeType dnType)
	{
		if (_dnaOpen != null)
		{
			for (int num = _dnaOpen.Count - 1; num >= 0; num--)
			{
				DocumentNode documentNode = _dnaOpen.EntryAt(num);
				if (documentNode.Type == dnType && !documentNode.IsMatched)
				{
					return documentNode.Index;
				}
			}
		}
		return -1;
	}

	internal void EstablishTreeRelationships()
	{
		for (int i = 0; i < Count; i++)
		{
			EntryAt(i).Index = i;
		}
		for (int i = 1; i < Count; i++)
		{
			DocumentNode documentNode = EntryAt(i);
			DocumentNode documentNode2 = EntryAt(i - 1);
			if (documentNode2.ChildCount == 0)
			{
				documentNode2 = documentNode2.Parent;
				while (documentNode2 != null && !documentNode2.IsAncestorOf(documentNode))
				{
					documentNode2 = documentNode2.Parent;
				}
			}
			documentNode.Parent = documentNode2;
		}
	}

	internal void CullOpen()
	{
		int num;
		for (num = Count - 1; num >= 0; num--)
		{
			DocumentNode documentNode = EntryAt(num);
			if (documentNode.Index >= 0 && documentNode.IsTrackedAsOpen)
			{
				break;
			}
		}
		int num2 = Count - (num + 1);
		if (num2 > 0)
		{
			RemoveRange(num + 1, num2);
		}
	}

	internal void InsertOpenNode(DocumentNode dn)
	{
		CullOpen();
		int num = Count;
		while (num > 0 && dn.Index <= EntryAt(num - 1).Index)
		{
			num--;
		}
		Insert(num, dn);
	}

	internal void InsertNode(int nAt, DocumentNode dn)
	{
		Insert(nAt, dn);
		if (!_fMain)
		{
			return;
		}
		dn.Index = nAt;
		dn.DNA = this;
		for (nAt++; nAt < Count; nAt++)
		{
			EntryAt(nAt).Index = nAt;
		}
		if (dn.IsTrackedAsOpen)
		{
			if (_dnaOpen == null)
			{
				_dnaOpen = new DocumentNodeArray();
			}
			_dnaOpen.InsertOpenNode(dn);
		}
	}

	internal void InsertChildAt(DocumentNode dnParent, DocumentNode dnNew, int nInsertAt, int nChild)
	{
		InsertNode(nInsertAt, dnNew);
		CloseAtHelper(nInsertAt, nChild);
		if (dnParent != null && dnParent.Parent == dnNew)
		{
			Invariant.Assert(condition: false, "Parent's Parent node shouldn't be the child node!");
		}
		dnNew.Parent = dnParent;
		while (dnParent != null)
		{
			dnParent.ChildCount++;
			dnParent = dnParent.ClosedParent;
		}
		AssertTreeInvariants();
	}

	internal void Excise(int nAt, int nExcise)
	{
		DocumentNode documentNode = EntryAt(nAt);
		if (_fMain)
		{
			int num = nAt + nExcise;
			for (int i = nAt; i < num; i++)
			{
				DocumentNode documentNode2 = EntryAt(i);
				documentNode2.Index = -1;
				documentNode2.DNA = null;
			}
		}
		RemoveRange(nAt, nExcise);
		if (!_fMain)
		{
			return;
		}
		for (DocumentNode parent = documentNode.Parent; parent != null; parent = parent.Parent)
		{
			if (!parent.IsPending)
			{
				parent.ChildCount -= nExcise;
			}
		}
		while (nAt < Count)
		{
			EntryAt(nAt).Index = nAt;
			nAt++;
		}
		AssertTreeInvariants();
	}

	private void PreCoalesceListItem(DocumentNode dn)
	{
		int index = dn.Index;
		long num = -1L;
		int num2 = index + dn.ChildCount;
		for (int i = index + 1; i <= num2; i++)
		{
			DocumentNode documentNode = EntryAt(i);
			if (documentNode.Type == DocumentNodeType.dnParagraph)
			{
				if (num == -1)
				{
					num = documentNode.NearMargin;
				}
				else if (documentNode.NearMargin < num && documentNode.IsNonEmpty)
				{
					num = documentNode.NearMargin;
				}
			}
		}
		dn.NearMargin = num;
		for (int j = index; j <= num2; j++)
		{
			DocumentNode documentNode2 = EntryAt(j);
			if (documentNode2.Type == DocumentNodeType.dnParagraph)
			{
				documentNode2.NearMargin -= num;
			}
		}
	}

	private void PreCoalesceList(DocumentNode dn)
	{
		int index = dn.Index;
		bool flag = false;
		DirState dirState = DirState.DirDefault;
		int num = index + dn.ChildCount;
		int num2 = index + 1;
		while (!flag && num2 <= num)
		{
			DocumentNode documentNode = EntryAt(num2);
			if (documentNode.Type == DocumentNodeType.dnParagraph && documentNode.IsNonEmpty)
			{
				if (dirState == DirState.DirDefault)
				{
					dirState = documentNode.FormatState.DirPara;
				}
				else if (dirState != documentNode.FormatState.DirPara)
				{
					flag = true;
				}
			}
			num2++;
		}
		if (flag || dirState == DirState.DirDefault)
		{
			return;
		}
		for (int i = index; i <= num; i++)
		{
			DocumentNode documentNode2 = EntryAt(i);
			if (documentNode2.Type == DocumentNodeType.dnList || documentNode2.Type == DocumentNodeType.dnListItem)
			{
				documentNode2.FormatState.DirPara = dirState;
			}
		}
	}

	private int PreCoalesceTable(DocumentNode dn)
	{
		int result = 0;
		int index = dn.Index;
		ColumnStateArray columnStateArray = dn.ComputeColumns();
		int minUnfilledRowIndex = columnStateArray.GetMinUnfilledRowIndex();
		if (minUnfilledRowIndex > 0)
		{
			DocumentNode documentNode = new DocumentNode(DocumentNodeType.dnTable);
			DocumentNode documentNode2 = new DocumentNode(DocumentNodeType.dnTableBody);
			documentNode.FormatState = new FormatState(dn.FormatState);
			documentNode.FormatState.RowFormat = EntryAt(minUnfilledRowIndex).FormatState.RowFormat;
			int num = minUnfilledRowIndex - dn.Index - 1;
			int num2 = dn.ChildCount - num;
			dn.ChildCount = num;
			EntryAt(index + 1).ChildCount = num - 1;
			InsertNode(minUnfilledRowIndex, documentNode2);
			CloseAtHelper(minUnfilledRowIndex, num2);
			InsertNode(minUnfilledRowIndex, documentNode);
			CloseAtHelper(minUnfilledRowIndex, num2 + 1);
			documentNode2.Parent = documentNode;
			documentNode.Parent = dn.ClosedParent;
			for (DocumentNode closedParent = documentNode.ClosedParent; closedParent != null; closedParent = closedParent.ClosedParent)
			{
				closedParent.ChildCount += 2;
			}
			result = 2;
			dn.ColumnStateArray = dn.ComputeColumns();
		}
		else
		{
			dn.ColumnStateArray = columnStateArray;
		}
		return result;
	}

	private void PreCoalesceRow(DocumentNode dn, ref bool fVMerged)
	{
		DocumentNodeArray rowsCells = dn.GetRowsCells();
		RowFormat rowFormat = dn.FormatState.RowFormat;
		ColumnStateArray columnStateArray = dn.GetParentOfType(DocumentNodeType.dnTable)?.ColumnStateArray;
		int num = ((rowsCells.Count < rowFormat.CellCount) ? rowsCells.Count : rowFormat.CellCount);
		int num2 = 0;
		int i = 0;
		while (i < num)
		{
			DocumentNode documentNode = rowsCells.EntryAt(i);
			CellFormat cellFormat = rowFormat.NthCellFormat(i);
			long cellX = cellFormat.CellX;
			if (cellFormat.IsVMerge)
			{
				fVMerged = true;
			}
			if (cellFormat.IsHMergeFirst)
			{
				for (i++; i < num; i++)
				{
					CellFormat cellFormat2 = rowFormat.NthCellFormat(i);
					if (cellFormat2.IsVMerge)
					{
						fVMerged = true;
					}
					if (cellFormat2.IsHMerge)
					{
						rowsCells.EntryAt(i).ColSpan = 0;
					}
				}
			}
			else
			{
				i++;
			}
			if (columnStateArray == null)
			{
				continue;
			}
			int num3 = num2;
			while (num2 < columnStateArray.Count)
			{
				ColumnState columnState = columnStateArray.EntryAt(num2);
				num2++;
				if (columnState.CellX == cellX || columnState.CellX > cellX)
				{
					break;
				}
			}
			if (num2 - num3 > documentNode.ColSpan)
			{
				documentNode.ColSpan = num2 - num3;
			}
		}
	}

	private void ProcessTableRowSpan(DocumentNodeArray dnaTables)
	{
		for (int i = 0; i < dnaTables.Count; i++)
		{
			DocumentNode documentNode = dnaTables.EntryAt(i);
			ColumnStateArray columnStateArray = documentNode.ColumnStateArray;
			if (columnStateArray == null || columnStateArray.Count == 0)
			{
				continue;
			}
			int count = columnStateArray.Count;
			DocumentNodeArray tableRows = documentNode.GetTableRows();
			DocumentNodeArray documentNodeArray = new DocumentNodeArray();
			for (int j = 0; j < count; j++)
			{
				documentNodeArray.Add(null);
			}
			for (int k = 0; k < tableRows.Count; k++)
			{
				DocumentNode documentNode2 = tableRows.EntryAt(k);
				RowFormat rowFormat = documentNode2.FormatState.RowFormat;
				DocumentNodeArray rowsCells = documentNode2.GetRowsCells();
				int num = count;
				if (rowFormat.CellCount < num)
				{
					num = rowFormat.CellCount;
				}
				if (rowsCells.Count < num)
				{
					num = rowsCells.Count;
				}
				int num2 = 0;
				for (int l = 0; l < num; l++)
				{
					if (num2 >= documentNodeArray.Count)
					{
						break;
					}
					DocumentNode documentNode3 = rowsCells.EntryAt(l);
					CellFormat cellFormat = rowFormat.NthCellFormat(l);
					if (cellFormat.IsVMerge)
					{
						DocumentNode documentNode4 = documentNodeArray.EntryAt(num2);
						if (documentNode4 != null)
						{
							documentNode4.RowSpan++;
						}
						num2 += documentNode3.ColSpan;
						documentNode3.ColSpan = 0;
						continue;
					}
					if (cellFormat.IsVMergeFirst)
					{
						documentNode3.RowSpan = 1;
						documentNodeArray[num2] = documentNode3;
					}
					else
					{
						documentNodeArray[num2] = null;
					}
					for (int m = num2 + 1; m < num2 + documentNode3.ColSpan; m++)
					{
						documentNodeArray[m] = null;
					}
					num2 += documentNode3.ColSpan;
				}
			}
		}
	}
}
