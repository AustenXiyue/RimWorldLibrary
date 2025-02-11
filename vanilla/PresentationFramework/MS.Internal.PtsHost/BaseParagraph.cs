using System.Windows;
using System.Windows.Documents;
using MS.Internal.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal abstract class BaseParagraph : UnmanagedHandle
{
	protected PTS.FSKCHANGE _changeType;

	protected bool _stopAsking;

	protected int _lastFormatCch;

	internal BaseParagraph Next;

	internal BaseParagraph Previous;

	protected readonly StructuralCache _structuralCache;

	protected readonly DependencyObject _element;

	internal int ParagraphStartCharacterPosition
	{
		get
		{
			if (this is TextParagraph)
			{
				return TextContainerHelper.GetCPFromElement(StructuralCache.TextContainer, Element, ElementEdge.AfterStart);
			}
			return TextContainerHelper.GetCPFromElement(StructuralCache.TextContainer, Element, ElementEdge.BeforeStart);
		}
	}

	internal int ParagraphEndCharacterPosition
	{
		get
		{
			if (this is TextParagraph)
			{
				return TextContainerHelper.GetCPFromElement(StructuralCache.TextContainer, Element, ElementEdge.BeforeEnd);
			}
			return TextContainerHelper.GetCPFromElement(StructuralCache.TextContainer, Element, ElementEdge.AfterEnd);
		}
	}

	internal int Cch
	{
		get
		{
			int num = TextContainerHelper.GetCchFromElement(StructuralCache.TextContainer, Element);
			if (this is TextParagraph && Element is TextElement)
			{
				Invariant.Assert(num >= 2);
				num -= 2;
			}
			return num;
		}
	}

	internal int LastFormatCch => _lastFormatCch;

	internal StructuralCache StructuralCache => _structuralCache;

	internal DependencyObject Element => _element;

	protected BaseParagraph(DependencyObject element, StructuralCache structuralCache)
		: base(structuralCache.PtsContext)
	{
		_element = element;
		_structuralCache = structuralCache;
		_changeType = PTS.FSKCHANGE.fskchNone;
		_stopAsking = false;
		UpdateLastFormatPositions();
	}

	internal virtual void UpdGetParaChange(out PTS.FSKCHANGE fskch, out int fNoFurtherChanges)
	{
		fskch = _changeType;
		fNoFurtherChanges = PTS.FromBoolean(_stopAsking);
	}

	internal virtual void CollapseMargin(BaseParaClient paraClient, MarginCollapsingState mcs, uint fswdir, bool suppressTopSpace, out int dvr)
	{
		dvr = ((!(mcs == null || suppressTopSpace)) ? mcs.Margin : 0);
	}

	internal abstract void GetParaProperties(ref PTS.FSPAP fspap);

	internal abstract void CreateParaclient(out nint pfsparaclient);

	internal virtual void SetUpdateInfo(PTS.FSKCHANGE fskch, bool stopAsking)
	{
		_changeType = fskch;
		_stopAsking = stopAsking;
	}

	internal virtual void ClearUpdateInfo()
	{
		_changeType = PTS.FSKCHANGE.fskchNone;
		_stopAsking = true;
	}

	internal virtual bool InvalidateStructure(int startPosition)
	{
		return TextContainerHelper.GetCPFromElement(StructuralCache.TextContainer, Element, ElementEdge.BeforeStart) == startPosition;
	}

	internal virtual void InvalidateFormatCache()
	{
	}

	internal void UpdateLastFormatPositions()
	{
		_lastFormatCch = Cch;
	}

	protected void GetParaProperties(ref PTS.FSPAP fspap, bool ignoreElementProps)
	{
		if (!ignoreElementProps)
		{
			fspap.fKeepWithNext = PTS.FromBoolean(DynamicPropertyReader.GetKeepWithNext(_element));
			fspap.fBreakPageBefore = ((_element is Block) ? PTS.FromBoolean(StructuralCache.CurrentFormatContext.FinitePage && ((Block)_element).BreakPageBefore) : PTS.FromBoolean(condition: false));
			fspap.fBreakColumnBefore = ((_element is Block) ? PTS.FromBoolean(((Block)_element).BreakColumnBefore) : PTS.FromBoolean(condition: false));
		}
	}
}
