namespace Unity.Burst;

internal readonly struct EagerCompilationRequest
{
	public readonly string EncodedMethod;

	public readonly string Options;

	public EagerCompilationRequest(string encodedMethod, string options)
	{
		EncodedMethod = encodedMethod;
		Options = options;
	}
}
