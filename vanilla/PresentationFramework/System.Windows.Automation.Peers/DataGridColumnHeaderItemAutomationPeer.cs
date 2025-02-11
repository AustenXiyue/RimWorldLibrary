using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes the <see cref="P:System.Windows.Controls.DataGridColumn.Header" /> of a <see cref="T:System.Windows.Controls.DataGridColumn" /> that is in a <see cref="T:System.Windows.Controls.DataGrid" /> to UI Automation.</summary>
public class DataGridColumnHeaderItemAutomationPeer : ItemAutomationPeer, IInvokeProvider, IScrollItemProvider, ITransformProvider, IVirtualizedItemProvider
{
	private DataGridColumn _column;

	/// <summary>Gets a value that specifies whether the column can be moved.</summary>
	/// <returns>false in all cases.</returns>
	bool ITransformProvider.CanMove => false;

	/// <summary>Gets a value that specifies whether the column can be resized.</summary>
	/// <returns>true if the element can be resized; otherwise false. </returns>
	bool ITransformProvider.CanResize
	{
		get
		{
			if (Column != null)
			{
				return Column.CanUserResize;
			}
			return false;
		}
	}

	/// <summary>Gets a value that specifies whether the control can be rotated.</summary>
	/// <returns>false in all cases.</returns>
	bool ITransformProvider.CanRotate => false;

	internal override bool AncestorsInvalid
	{
		get
		{
			return base.AncestorsInvalid;
		}
		set
		{
			base.AncestorsInvalid = value;
			if (!value)
			{
				AutomationPeer owningColumnHeaderPeer = OwningColumnHeaderPeer;
				if (owningColumnHeaderPeer != null)
				{
					owningColumnHeaderPeer.AncestorsInvalid = false;
				}
			}
		}
	}

	internal DataGridColumnHeader OwningHeader => GetWrapper() as DataGridColumnHeader;

	internal DataGrid OwningDataGrid => Column.DataGridOwner;

	internal DataGridColumn Column => _column;

	internal DataGridColumnHeaderAutomationPeer OwningColumnHeaderPeer => GetWrapperPeer() as DataGridColumnHeaderAutomationPeer;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.DataGridColumnHeaderItemAutomationPeer" /> class. </summary>
	/// <param name="item">The <see cref="P:System.Windows.Controls.DataGridColumn.Header" /> in the <see cref="T:System.Windows.Controls.DataGridColumn" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.DataGridColumnHeaderItemAutomationPeer" />.</param>
	/// <param name="column">The column that is associated with the <see cref="P:System.Windows.Controls.DataGridColumn.Header" />.</param>
	/// <param name="peer">The automation peer for the <see cref="T:System.Windows.Controls.Primitives.DataGridColumnHeadersPresenter" /> that is associated with the <see cref="T:System.Windows.Controls.DataGrid" />.</param>
	public DataGridColumnHeaderItemAutomationPeer(object item, DataGridColumn column, DataGridColumnHeadersPresenterAutomationPeer peer)
		: base(item, peer)
	{
		_column = column;
	}

	/// <summary>Gets the control type for the header that is associated with this <see cref="T:System.Windows.Automation.Peers.DataGridColumnHeaderItemAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.HeaderItem" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.HeaderItem;
	}

	/// <summary>Gets a name that differentiates the header that is represented by this <see cref="T:System.Windows.Automation.Peers.DataGridColumnHeaderItemAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>The type name of the <see cref="P:System.Windows.Controls.DataGridColumn.Header" /> property that is associated with this <see cref="T:System.Windows.Automation.Peers.DataGridColumnHeaderItemAutomationPeer" />.</returns>
	protected override string GetClassNameCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetClassName();
		}
		ThrowElementNotAvailableException();
		return string.Empty;
	}

	/// <summary>Returns the object that supports the specified control pattern of the element that is associated with this automation peer.</summary>
	/// <returns>The current <see cref="T:System.Windows.Automation.Peers.DataGridColumnHeaderItemAutomationPeer" /> object, if <paramref name="patternInterface" /> is a supported value; otherwise, null. For more information, see Remarks.</returns>
	/// <param name="patternInterface">An enumeration value that specifies the control pattern.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		switch (patternInterface)
		{
		case PatternInterface.Invoke:
			if (Column != null && Column.CanUserSort)
			{
				return this;
			}
			break;
		case PatternInterface.ScrollItem:
			if (Column != null)
			{
				return this;
			}
			break;
		case PatternInterface.Transform:
			if (Column != null && Column.CanUserResize)
			{
				return this;
			}
			break;
		case PatternInterface.VirtualizedItem:
			if (Column != null)
			{
				return this;
			}
			break;
		}
		return null;
	}

	/// <summary>Gets a value that indicates whether the element that is associated with this automation peer contains data that is presented to the user.</summary>
	/// <returns>false in all cases.</returns>
	protected override bool IsContentElementCore()
	{
		return false;
	}

	/// <summary>Sends a request to activate a control and initiate its single, unambiguous action.</summary>
	void IInvokeProvider.Invoke()
	{
		if (GetWrapperPeer() is UIElementAutomationPeer uIElementAutomationPeer)
		{
			((DataGridColumnHeader)uIElementAutomationPeer.Owner).Invoke();
		}
		else
		{
			ThrowElementNotAvailableException();
		}
	}

	/// <summary>Scrolls the header and column that is associated with the <see cref="T:System.Windows.Automation.Peers.DataGridColumnHeaderItemAutomationPeer" /> into view.</summary>
	void IScrollItemProvider.ScrollIntoView()
	{
		if (Column != null && OwningDataGrid != null)
		{
			OwningDataGrid.ScrollIntoView(null, Column);
		}
	}

	/// <summary>Throws an exception in all cases.</summary>
	/// <param name="x">This parameter is not used.</param>
	/// <param name="y">This parameter is not used.</param>
	/// <exception cref="T:System.InvalidOperationException">In all cases.</exception>
	void ITransformProvider.Move(double x, double y)
	{
		throw new InvalidOperationException(SR.DataGridColumnHeaderItemAutomationPeer_Unsupported);
	}

	/// <summary>Resizes the width of the column that is associated with the <see cref="T:System.Windows.Automation.Peers.DataGridColumnHeaderItemAutomationPeer" />. </summary>
	/// <param name="width">The new width of the column, in pixels.</param>
	/// <param name="height">This parameter is not used.</param>
	/// <exception cref="T:System.InvalidOperationException">The column that is associated with this <see cref="T:System.Windows.Automation.Peers.DataGridColumnHeaderItemAutomationPeer" /> cannot be resized.</exception>
	void ITransformProvider.Resize(double width, double height)
	{
		if (OwningDataGrid != null && Column.CanUserResize)
		{
			Column.Width = new DataGridLength(width);
			return;
		}
		throw new InvalidOperationException(SR.DataGridColumnHeaderItemAutomationPeer_Unresizable);
	}

	/// <summary>Throws an exception in all cases.</summary>
	/// <param name="degrees">This parameter is not used.</param>
	/// <exception cref="T:System.InvalidOperationException">In all cases.</exception>
	void ITransformProvider.Rotate(double degrees)
	{
		throw new InvalidOperationException(SR.DataGridColumnHeaderItemAutomationPeer_Unsupported);
	}

	/// <summary>Makes the virtual column fully accessible as a UI Automation element.</summary>
	void IVirtualizedItemProvider.Realize()
	{
		if (OwningDataGrid != null)
		{
			OwningDataGrid.ScrollIntoView(null, Column);
		}
	}
}
