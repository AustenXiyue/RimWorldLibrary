namespace System.Windows.Documents;

[Flags]
internal enum FindFlags
{
	None = 0,
	MatchCase = 1,
	FindInReverse = 2,
	FindWholeWordsOnly = 4,
	MatchDiacritics = 8,
	MatchKashida = 0x10,
	MatchAlefHamza = 0x20
}
