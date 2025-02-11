namespace System.Windows.Documents.MsSpellCheckLib;

internal class RetriesExhaustedException : Exception
{
	internal RetriesExhaustedException()
	{
	}

	internal RetriesExhaustedException(string message)
		: base(message)
	{
	}

	internal RetriesExhaustedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
