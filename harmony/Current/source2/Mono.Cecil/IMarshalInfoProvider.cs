namespace Mono.Cecil;

internal interface IMarshalInfoProvider : IMetadataTokenProvider
{
	bool HasMarshalInfo { get; }

	MarshalInfo MarshalInfo { get; set; }
}
