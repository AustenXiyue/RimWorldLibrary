namespace System.Xml.Schema;

internal class KSStruct
{
	public int depth;

	public KeySequence ks;

	public LocatedActiveAxis[] fields;

	public KSStruct(KeySequence ks, int dim)
	{
		this.ks = ks;
		fields = new LocatedActiveAxis[dim];
	}
}
