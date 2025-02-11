namespace System.Windows.Markup;

internal class BamlKeyElementStartRecord : BamlDefAttributeKeyTypeRecord, IBamlDictionaryKey
{
	internal override BamlRecordType RecordType => BamlRecordType.KeyElementStart;

	internal BamlKeyElementStartRecord()
	{
		Pin();
	}
}
