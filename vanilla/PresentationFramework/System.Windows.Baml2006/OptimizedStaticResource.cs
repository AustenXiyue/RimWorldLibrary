namespace System.Windows.Baml2006;

internal class OptimizedStaticResource
{
	private bool _isStatic;

	private bool _isType;

	private static readonly byte TypeExtensionValueMask = 1;

	private static readonly byte StaticExtensionValueMask = 2;

	public short KeyId { get; set; }

	public object KeyValue { get; set; }

	public bool IsKeyStaticExtension => _isStatic;

	public bool IsKeyTypeExtension => _isType;

	public OptimizedStaticResource(byte flags, short keyId)
	{
		_isType = (flags & TypeExtensionValueMask) != 0;
		_isStatic = (flags & StaticExtensionValueMask) != 0;
		KeyId = keyId;
	}
}
