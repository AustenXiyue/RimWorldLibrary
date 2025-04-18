namespace Mono.Cecil;

internal interface IConstantProvider : IMetadataTokenProvider
{
	bool HasConstant { get; set; }

	object Constant { get; set; }
}
