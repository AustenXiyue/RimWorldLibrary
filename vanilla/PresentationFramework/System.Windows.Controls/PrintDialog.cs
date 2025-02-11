using System.Printing;
using System.Windows.Documents;
using System.Windows.Documents.Serialization;
using System.Windows.Media;
using System.Windows.Xps;
using System.Windows.Xps.Serialization;
using MS.Internal.Printing;

namespace System.Windows.Controls;

/// <summary>Invokes a standard Microsoft Windows print dialog box that configures a <see cref="T:System.Printing.PrintTicket" /> and <see cref="T:System.Printing.PrintQueue" /> according to user input and then prints a document. </summary>
public class PrintDialog
{
	internal class PrintDlgPrintTicketEventHandler
	{
		private PrintTicket _printTicket;

		public PrintDlgPrintTicketEventHandler(PrintTicket printTicket)
		{
			_printTicket = printTicket;
		}

		public void SetPrintTicket(object sender, WritingPrintTicketRequiredEventArgs args)
		{
			if (args.CurrentPrintTicketLevel == PrintTicketLevel.FixedDocumentSequencePrintTicket)
			{
				args.CurrentPrintTicket = _printTicket;
			}
		}
	}

	private PrintTicket _printTicket;

	private PrintQueue _printQueue;

	private PageRangeSelection _pageRangeSelection;

	private PageRange _pageRange;

	private bool _userPageRangeEnabled;

	private bool _selectedPagesEnabled;

	private bool _currentPageEnabled;

	private uint _minPage;

	private uint _maxPage;

	private double _printableAreaWidth;

	private double _printableAreaHeight;

	private bool _isPrintableAreaWidthUpdated;

	private bool _isPrintableAreaHeightUpdated;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Controls.PageRangeSelection" /> for this instance of <see cref="T:System.Windows.Controls.PrintDialog" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.PageRangeSelection" /> value that represents the type of page range to print. </returns>
	public PageRangeSelection PageRangeSelection
	{
		get
		{
			return _pageRangeSelection;
		}
		set
		{
			_pageRangeSelection = value;
		}
	}

	/// <summary>Gets or sets the range of pages to print when <see cref="P:System.Windows.Controls.PrintDialog.PageRangeSelection" /> is set to <see cref="F:System.Windows.Controls.PageRangeSelection.UserPages" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Controls.PageRange" /> that represents the range of pages that are printed. </returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Windows.Controls.PageRange" /> object that is being used to set the property has either the beginning of the range or the end of the range set to a value that is less than 1.</exception>
	public PageRange PageRange
	{
		get
		{
			return _pageRange;
		}
		set
		{
			if (value.PageTo <= 0 || value.PageFrom <= 0)
			{
				throw new ArgumentException(SR.PrintDialogInvalidPageRange, "PageRange");
			}
			_pageRange = value;
			if (_pageRange.PageFrom > _pageRange.PageTo)
			{
				int pageFrom = _pageRange.PageFrom;
				_pageRange.PageFrom = _pageRange.PageTo;
				_pageRange.PageTo = pageFrom;
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether users of the Print dialog box have the option to specify ranges of pages to print.</summary>
	/// <returns>true if the option is available; otherwise, false.</returns>
	public bool UserPageRangeEnabled
	{
		get
		{
			return _userPageRangeEnabled;
		}
		set
		{
			_userPageRangeEnabled = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the option to print the selected pages is enabled.</summary>
	/// <returns>true if the option to print the selected pages is enabled; otherwise, false.</returns>
	public bool SelectedPagesEnabled
	{
		get
		{
			return _selectedPagesEnabled;
		}
		set
		{
			_selectedPagesEnabled = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the option to print the current page is enabled.</summary>
	/// <returns>true if the option to print the current page is enabled; otherwise, false.</returns>
	public bool CurrentPageEnabled
	{
		get
		{
			return _currentPageEnabled;
		}
		set
		{
			_currentPageEnabled = value;
		}
	}

	/// <summary>Gets or sets the lowest page number that is allowed in page ranges.</summary>
	/// <returns>A <see cref="T:System.UInt32" /> that represents the lowest page number that can be used in a page range in the Print dialog box. </returns>
	/// <exception cref="T:System.ArgumentException">The property is being set to less than 1.</exception>
	public uint MinPage
	{
		get
		{
			return _minPage;
		}
		set
		{
			if (_minPage == 0)
			{
				throw new ArgumentException(SR.Format(SR.PrintDialogZeroNotAllowed, "MinPage"));
			}
			_minPage = value;
		}
	}

	/// <summary>Gets or sets the highest page number that is allowed in page ranges.</summary>
	/// <returns>A <see cref="T:System.UInt32" /> that represents the highest page number that can be used in a page range in the Print dialog box. </returns>
	/// <exception cref="T:System.ArgumentException">The property is being set to less than 1.</exception>
	public uint MaxPage
	{
		get
		{
			return _maxPage;
		}
		set
		{
			if (_maxPage == 0)
			{
				throw new ArgumentException(SR.Format(SR.PrintDialogZeroNotAllowed, "MaxPage"));
			}
			_maxPage = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Printing.PrintQueue" /> that represents the printer that is selected.</summary>
	/// <returns>The <see cref="T:System.Printing.PrintQueue" /> that the user selected. </returns>
	public PrintQueue PrintQueue
	{
		get
		{
			if (_printQueue == null)
			{
				_printQueue = AcquireDefaultPrintQueue();
			}
			return _printQueue;
		}
		set
		{
			_printQueue = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Printing.PrintTicket" /> that is used by the <see cref="T:System.Windows.Controls.PrintDialog" /> when the user clicks Print for the current print job.</summary>
	/// <returns>A <see cref="T:System.Printing.PrintTicket" /> that is used the next time the Print button in the dialog box is clicked.Setting this <see cref="P:System.Windows.Controls.PrintDialog.PrintTicket" /> property does not validate or modify the specified <see cref="T:System.Printing.PrintTicket" /> for a particular <see cref="T:System.Printing.PrintQueue" />.  If needed, use the <see cref="M:System.Printing.PrintQueue.MergeAndValidatePrintTicket(System.Printing.PrintTicket,System.Printing.PrintTicket)" /> method to create a <see cref="T:System.Printing.PrintQueue" />-specific <see cref="T:System.Printing.PrintTicket" /> that is valid for a given printer.</returns>
	public PrintTicket PrintTicket
	{
		get
		{
			if (_printTicket == null)
			{
				_printTicket = AcquireDefaultPrintTicket(PrintQueue);
			}
			return _printTicket;
		}
		set
		{
			_printTicket = value;
		}
	}

	/// <summary>Gets the width of the printable area of the page.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the width of the printable page area.</returns>
	public double PrintableAreaWidth
	{
		get
		{
			if ((!_isPrintableAreaWidthUpdated && !_isPrintableAreaHeightUpdated) || (_isPrintableAreaWidthUpdated && !_isPrintableAreaHeightUpdated))
			{
				_isPrintableAreaWidthUpdated = true;
				_isPrintableAreaHeightUpdated = false;
				UpdatePrintableAreaSize();
			}
			return _printableAreaWidth;
		}
	}

	/// <summary>Gets the height of the printable area of the page.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the height of the printable page area.</returns>
	public double PrintableAreaHeight
	{
		get
		{
			if ((!_isPrintableAreaWidthUpdated && !_isPrintableAreaHeightUpdated) || (!_isPrintableAreaWidthUpdated && _isPrintableAreaHeightUpdated))
			{
				_isPrintableAreaWidthUpdated = false;
				_isPrintableAreaHeightUpdated = true;
				UpdatePrintableAreaSize();
			}
			return _printableAreaHeight;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.PrintDialog" /> class.</summary>
	public PrintDialog()
	{
		_printQueue = null;
		_printTicket = null;
		_isPrintableAreaWidthUpdated = false;
		_isPrintableAreaHeightUpdated = false;
		_pageRangeSelection = PageRangeSelection.AllPages;
		_minPage = 1u;
		_maxPage = 9999u;
		_userPageRangeEnabled = false;
	}

	/// <summary>Invokes the <see cref="T:System.Windows.Controls.PrintDialog" /> as a modal dialog box. </summary>
	/// <returns>true if a user clicks Print; false if a user clicks Cancel; or null if a user closes the dialog box without clicking Print or Cancel.</returns>
	public bool? ShowDialog()
	{
		Win32PrintDialog win32PrintDialog = new Win32PrintDialog();
		win32PrintDialog.PrintTicket = _printTicket;
		win32PrintDialog.PrintQueue = _printQueue;
		win32PrintDialog.MinPage = Math.Max(1u, Math.Min(_minPage, _maxPage));
		win32PrintDialog.MaxPage = Math.Max(win32PrintDialog.MinPage, Math.Max(_minPage, _maxPage));
		win32PrintDialog.PageRangeEnabled = _userPageRangeEnabled;
		win32PrintDialog.SelectedPagesEnabled = _selectedPagesEnabled;
		win32PrintDialog.CurrentPageEnabled = _currentPageEnabled;
		win32PrintDialog.PageRange = new PageRange(Math.Max((int)win32PrintDialog.MinPage, _pageRange.PageFrom), Math.Min((int)win32PrintDialog.MaxPage, _pageRange.PageTo));
		win32PrintDialog.PageRangeSelection = _pageRangeSelection;
		uint num = win32PrintDialog.ShowDialog();
		if (num == 2 || num == 1)
		{
			_printTicket = win32PrintDialog.PrintTicket;
			_printQueue = win32PrintDialog.PrintQueue;
			_pageRange = win32PrintDialog.PageRange;
			_pageRangeSelection = win32PrintDialog.PageRangeSelection;
		}
		return num == 1;
	}

	/// <summary>Prints a visual (non-text) object, which is derived from the <see cref="T:System.Windows.Media.Visual" /> class, to the <see cref="T:System.Printing.PrintQueue" /> that is currently selected.</summary>
	/// <param name="visual">The <see cref="T:System.Windows.Media.Visual" /> to print.</param>
	/// <param name="description">A description of the job that is to be printed. This text appears in the user interface (UI) of the printer.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="visual" /> is null. </exception>
	public void PrintVisual(Visual visual, string description)
	{
		if (visual == null)
		{
			throw new ArgumentNullException("visual");
		}
		CreateWriter(description).Write(visual, _printTicket);
		_printableAreaWidth = 0.0;
		_printableAreaHeight = 0.0;
		_isPrintableAreaWidthUpdated = false;
		_isPrintableAreaHeightUpdated = false;
	}

	/// <summary>Prints a <see cref="T:System.Windows.Documents.DocumentPaginator" /> object to the <see cref="T:System.Printing.PrintQueue" /> that is currently selected.</summary>
	/// <param name="documentPaginator">The <see cref="T:System.Windows.Documents.DocumentPaginator" /> object to print.</param>
	/// <param name="description">A description of the job that is to be printed. This text appears in the user interface (UI) of the printer.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="documentPaginator" /> is null. </exception>
	public void PrintDocument(DocumentPaginator documentPaginator, string description)
	{
		if (documentPaginator == null)
		{
			throw new ArgumentNullException("documentPaginator");
		}
		CreateWriter(description).Write(documentPaginator, _printTicket);
		_printableAreaWidth = 0.0;
		_printableAreaHeight = 0.0;
		_isPrintableAreaWidthUpdated = false;
		_isPrintableAreaHeightUpdated = false;
	}

	private PrintQueue AcquireDefaultPrintQueue()
	{
		PrintQueue printQueue = null;
		try
		{
			return new LocalPrintServer().DefaultPrintQueue;
		}
		catch (PrintSystemException)
		{
			return null;
		}
	}

	private PrintTicket AcquireDefaultPrintTicket(PrintQueue printQueue)
	{
		PrintTicket printTicket = null;
		try
		{
			if (printQueue != null)
			{
				printTicket = printQueue.UserPrintTicket;
				if (printTicket == null)
				{
					printTicket = printQueue.DefaultPrintTicket;
				}
			}
		}
		catch (PrintSystemException)
		{
			printTicket = null;
		}
		if (printTicket == null)
		{
			printTicket = new PrintTicket();
		}
		return printTicket;
	}

	private void UpdatePrintableAreaSize()
	{
		PrintQueue printQueue = null;
		PrintTicket printTicket = null;
		PickCorrectPrintingEnvironment(ref printQueue, ref printTicket);
		PrintCapabilities printCapabilities = null;
		if (printQueue != null)
		{
			printCapabilities = printQueue.GetPrintCapabilities(printTicket);
		}
		if (printCapabilities != null && printCapabilities.OrientedPageMediaWidth.HasValue && printCapabilities.OrientedPageMediaHeight.HasValue)
		{
			_printableAreaWidth = printCapabilities.OrientedPageMediaWidth.Value;
			_printableAreaHeight = printCapabilities.OrientedPageMediaHeight.Value;
			return;
		}
		_printableAreaWidth = 816.0;
		_printableAreaHeight = 1056.0;
		if (printTicket.PageMediaSize != null && printTicket.PageMediaSize.Width.HasValue && printTicket.PageMediaSize.Height.HasValue)
		{
			_printableAreaWidth = printTicket.PageMediaSize.Width.Value;
			_printableAreaHeight = printTicket.PageMediaSize.Height.Value;
		}
		if (printTicket.PageOrientation.HasValue)
		{
			PageOrientation value = printTicket.PageOrientation.Value;
			if (value == PageOrientation.Landscape || value == PageOrientation.ReverseLandscape)
			{
				double printableAreaWidth = _printableAreaWidth;
				_printableAreaWidth = _printableAreaHeight;
				_printableAreaHeight = printableAreaWidth;
			}
		}
	}

	private XpsDocumentWriter CreateWriter(string description)
	{
		PrintQueue printQueue = null;
		PrintTicket printTicket = null;
		PickCorrectPrintingEnvironment(ref printQueue, ref printTicket);
		if (printQueue != null)
		{
			printQueue.CurrentJobSettings.Description = description;
		}
		XpsDocumentWriter xpsDocumentWriter = PrintQueue.CreateXpsDocumentWriter(printQueue);
		PrintDlgPrintTicketEventHandler @object = new PrintDlgPrintTicketEventHandler(printTicket);
		xpsDocumentWriter.WritingPrintTicketRequired += @object.SetPrintTicket;
		return xpsDocumentWriter;
	}

	private void PickCorrectPrintingEnvironment(ref PrintQueue printQueue, ref PrintTicket printTicket)
	{
		if (_printQueue == null)
		{
			_printQueue = AcquireDefaultPrintQueue();
		}
		if (_printTicket == null)
		{
			_printTicket = AcquireDefaultPrintTicket(_printQueue);
		}
		printQueue = _printQueue;
		printTicket = _printTicket;
	}
}
