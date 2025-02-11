using System;
using System.Windows.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;

namespace MS.Internal.PtsHost;

internal abstract class FloaterBaseParagraph : BaseParagraph
{
	protected FloaterBaseParagraph(TextElement element, StructuralCache structuralCache)
		: base(element, structuralCache)
	{
	}

	public override void Dispose()
	{
		base.Dispose();
		GC.SuppressFinalize(this);
	}

	internal override void UpdGetParaChange(out PTS.FSKCHANGE fskch, out int fNoFurtherChanges)
	{
		fskch = PTS.FSKCHANGE.fskchNew;
		fNoFurtherChanges = PTS.FromBoolean(_stopAsking);
	}

	internal override void GetParaProperties(ref PTS.FSPAP fspap)
	{
		GetParaProperties(ref fspap, ignoreElementProps: false);
		fspap.idobj = PtsHost.FloaterParagraphId;
	}

	internal abstract override void CreateParaclient(out nint paraClientHandle);

	internal abstract override void CollapseMargin(BaseParaClient paraClient, MarginCollapsingState mcs, uint fswdir, bool suppressTopSpace, out int dvr);

	internal abstract void GetFloaterProperties(uint fswdirTrack, out PTS.FSFLOATERPROPS fsfloaterprops);

	internal unsafe virtual void GetFloaterPolygons(FloaterBaseParaClient paraClient, uint fswdirTrack, int ncVertices, int nfspt, int* rgcVertices, out int ccVertices, PTS.FSPOINT* rgfspt, out int cfspt, out int fWrapThrough)
	{
		ccVertices = (cfspt = (fWrapThrough = 0));
	}

	internal abstract void FormatFloaterContentFinite(FloaterBaseParaClient paraClient, nint pbrkrecIn, int fBRFromPreviousPage, nint footnoteRejector, int fEmptyOk, int fSuppressTopSpace, uint fswdir, int fAtMaxWidth, int durAvailable, int dvrAvailable, PTS.FSKSUPPRESSHARDBREAKBEFOREFIRSTPARA fsksuppresshardbreakbeforefirstparaIn, out PTS.FSFMTR fsfmtr, out nint pfsFloatContent, out nint pbrkrecOut, out int durFloaterWidth, out int dvrFloaterHeight, out PTS.FSBBOX fsbbox, out int cPolygons, out int cVertices);

	internal abstract void FormatFloaterContentBottomless(FloaterBaseParaClient paraClient, int fSuppressTopSpace, uint fswdir, int fAtMaxWidth, int durAvailable, int dvrAvailable, out PTS.FSFMTRBL fsfmtrbl, out nint pfsFloatContent, out int durFloaterWidth, out int dvrFloaterHeight, out PTS.FSBBOX fsbbox, out int cPolygons, out int cVertices);

	internal abstract void UpdateBottomlessFloaterContent(FloaterBaseParaClient paraClient, int fSuppressTopSpace, uint fswdir, int fAtMaxWidth, int durAvailable, int dvrAvailable, nint pfsFloatContent, out PTS.FSFMTRBL fsfmtrbl, out int durFloaterWidth, out int dvrFloaterHeight, out PTS.FSBBOX fsbbox, out int cPolygons, out int cVertices);

	internal abstract void GetMCSClientAfterFloater(uint fswdirTrack, MarginCollapsingState mcs, out nint pmcsclientOut);

	internal virtual void GetDvrUsedForFloater(uint fswdirTrack, MarginCollapsingState mcs, int dvrDisplaced, out int dvrUsed)
	{
		dvrUsed = 0;
	}
}
