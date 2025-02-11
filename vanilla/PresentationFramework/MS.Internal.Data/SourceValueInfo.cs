using MS.Utility;

namespace MS.Internal.Data;

internal struct SourceValueInfo
{
	public SourceValueType type;

	public DrillIn drillIn;

	public string name;

	public FrugalObjectList<IndexerParamInfo> paramList;

	public string propertyName;

	public SourceValueInfo(SourceValueType t, DrillIn d, string n)
	{
		type = t;
		drillIn = d;
		name = n;
		paramList = null;
		propertyName = null;
	}

	public SourceValueInfo(SourceValueType t, DrillIn d, FrugalObjectList<IndexerParamInfo> list)
	{
		type = t;
		drillIn = d;
		name = null;
		paramList = list;
		propertyName = null;
	}
}
