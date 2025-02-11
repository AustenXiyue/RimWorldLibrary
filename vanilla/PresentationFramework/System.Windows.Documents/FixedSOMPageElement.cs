namespace System.Windows.Documents;

internal abstract class FixedSOMPageElement : FixedSOMContainer
{
	protected FixedSOMPage _page;

	public FixedSOMPage FixedSOMPage => _page;

	public abstract bool IsRTL { get; }

	public FixedSOMPageElement(FixedSOMPage page)
	{
		_page = page;
	}
}
