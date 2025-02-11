using System.Windows.Automation.Provider;
using System.Windows.Documents;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Documents.TableCell" /> types to UI Automation.</summary>
public class TableCellAutomationPeer : TextElementAutomationPeer, IGridItemProvider
{
	/// <summary>Gets the ordinal number of the row that contains the cell or item.</summary>
	/// <returns>A zero-based ordinal number that identifies the row containing the cell or item.</returns>
	int IGridItemProvider.Row => ((TableCell)base.Owner).RowIndex;

	/// <summary>Gets the ordinal number of the column that contains the cell or item.</summary>
	/// <returns>A zero-based ordinal number that identifies the column containing the cell or item.</returns>
	int IGridItemProvider.Column => ((TableCell)base.Owner).ColumnIndex;

	/// <summary>Gets the number of rows spanned by a cell or item.</summary>
	/// <returns>The number of rows spanned.</returns>
	int IGridItemProvider.RowSpan => ((TableCell)base.Owner).RowSpan;

	/// <summary>Gets the number of columns spanned by a cell or item.</summary>
	/// <returns>The number of columns spanned.</returns>
	int IGridItemProvider.ColumnSpan => ((TableCell)base.Owner).ColumnSpan;

	/// <summary>Gets a UI Automation provider that implements <see cref="T:System.Windows.Automation.GridPattern" /> and represents the container of the cell or item.</summary>
	/// <returns>A UI Automation provider that implements the <see cref="T:System.Windows.Automation.GridPattern" /> and represents the cell or item container.</returns>
	IRawElementProviderSimple IGridItemProvider.ContainingGrid
	{
		get
		{
			if ((TableCell)base.Owner != null)
			{
				return ProviderFromPeer(ContentElementAutomationPeer.CreatePeerForElement(((TableCell)base.Owner).Table));
			}
			return null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.TableCellAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Documents.TableCell" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TableCellAutomationPeer" />.</param>
	public TableCellAutomationPeer(TableCell owner)
		: base(owner)
	{
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Documents.TableCell" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TableCellAutomationPeer" />.</summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.GridItem" />, this method returns the current instance of the <see cref="T:System.Windows.Automation.Peers.TableCellAutomationPeer" />; otherwise, this method returns null.</returns>
	/// <param name="patternInterface">A value from the enumeration.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.GridItem)
		{
			return this;
		}
		return base.GetPattern(patternInterface);
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Documents.TableCell" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TableCellAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Custom" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Custom;
	}

	/// <summary>Gets the localized version of the control type for the <see cref="T:System.Windows.Documents.TableCell" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TableCellAutomationPeer" />.</summary>
	/// <returns>A string that contains "cell".</returns>
	protected override string GetLocalizedControlTypeCore()
	{
		return "cell";
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Documents.TableCell" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TableCellAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "TableCell".</returns>
	protected override string GetClassNameCore()
	{
		return "TableCell";
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Documents.TableCell" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TableCellAutomationPeer" /> is understood by the end user as interactive or as contributing to the logical structure of the control in the GUI. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsControlElement" />.</summary>
	/// <returns>true.</returns>
	protected override bool IsControlElementCore()
	{
		if (!base.IncludeInvisibleElementsInControlView)
		{
			return base.IsTextViewVisible == true;
		}
		return true;
	}

	internal void OnColumnSpanChanged(int oldValue, int newValue)
	{
		RaisePropertyChangedEvent(GridItemPatternIdentifiers.ColumnSpanProperty, oldValue, newValue);
	}

	internal void OnRowSpanChanged(int oldValue, int newValue)
	{
		RaisePropertyChangedEvent(GridItemPatternIdentifiers.RowSpanProperty, oldValue, newValue);
	}
}
