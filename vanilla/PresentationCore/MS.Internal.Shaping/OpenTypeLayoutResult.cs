namespace MS.Internal.Shaping;

internal enum OpenTypeLayoutResult
{
	Success,
	InvalidParameter,
	TableNotFound,
	ScriptNotFound,
	LangSysNotFound,
	BadFontTable,
	UnderConstruction
}
