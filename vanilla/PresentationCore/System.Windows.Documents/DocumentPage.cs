using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>Represents a document page produced by a paginator.  </summary>
public class DocumentPage : IDisposable
{
	private sealed class MissingDocumentPage : DocumentPage
	{
		public MissingDocumentPage()
			: base(null, Size.Empty, Rect.Empty, Rect.Empty)
		{
		}

		public override void Dispose()
		{
		}
	}

	/// <summary>Represents a missing page. This property is static and read only. </summary>
	/// <returns>A <see cref="T:System.Windows.Documents.DocumentPage" /> with all its size properties set to zero.</returns>
	public static readonly DocumentPage Missing = new MissingDocumentPage();

	private Visual _visual;

	private Size _pageSize;

	private Rect _bleedBox;

	private Rect _contentBox;

	/// <summary>When overridden in a derived class, gets the visual representation of the page. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Visual" /> depicting the page. </returns>
	public virtual Visual Visual => _visual;

	/// <summary>When overridden in a derived class, gets the actual size of a page as it will be following any cropping. </summary>
	/// <returns>A <see cref="T:System.Windows.Size" /> representing the height and width of the page.</returns>
	public virtual Size Size
	{
		get
		{
			if (_pageSize == Size.Empty && _visual != null)
			{
				return VisualTreeHelper.GetContentBounds(_visual).Size;
			}
			return _pageSize;
		}
	}

	/// <summary>When overridden in a derived class, gets the area for print production-related bleeds, registration marks, and crop marks that may appear on the physical sheet outside the logical page boundaries. </summary>
	/// <returns>A <see cref="T:System.Windows.Rect" /> representing the size and location of the bleed box area. </returns>
	public virtual Rect BleedBox
	{
		get
		{
			if (_bleedBox == Rect.Empty)
			{
				return new Rect(Size);
			}
			return _bleedBox;
		}
	}

	/// <summary>When overridden in a derived class, gets the area of the page within the margins.</summary>
	/// <returns>A <see cref="T:System.Windows.Rect" /> representing the area of the page, not including the margins. </returns>
	public virtual Rect ContentBox
	{
		get
		{
			if (_contentBox == Rect.Empty)
			{
				return new Rect(Size);
			}
			return _contentBox;
		}
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Documents.DocumentPage.Visual" /> that depicts the <see cref="T:System.Windows.Documents.DocumentPage" /> is destroyed and can no longer be used for display.</summary>
	public event EventHandler PageDestroyed;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.DocumentPage" /> class by using the specified <see cref="T:System.Windows.Media.Visual" />. </summary>
	/// <param name="visual">The visual representation of the page.</param>
	public DocumentPage(Visual visual)
	{
		_visual = visual;
		_pageSize = Size.Empty;
		_bleedBox = Rect.Empty;
		_contentBox = Rect.Empty;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.DocumentPage" /> class by using the specified <see cref="T:System.Windows.Media.Visual" /> and box sizes. </summary>
	/// <param name="visual">The visual representation of the page.</param>
	/// <param name="pageSize">The size of the page, including margins, as it will be after any cropping.</param>
	/// <param name="bleedBox">The area for print production-related bleeds, registration marks, and crop marks that may appear on the physical sheet outside the logical page boundaries.</param>
	/// <param name="contentBox">The area of the page within the margins.</param>
	public DocumentPage(Visual visual, Size pageSize, Rect bleedBox, Rect contentBox)
	{
		_visual = visual;
		_pageSize = pageSize;
		_bleedBox = bleedBox;
		_contentBox = contentBox;
	}

	/// <summary>Releases all resources used by the <see cref="T:System.Windows.Documents.DocumentPage" />. </summary>
	public virtual void Dispose()
	{
		_visual = null;
		_pageSize = Size.Empty;
		_bleedBox = Rect.Empty;
		_contentBox = Rect.Empty;
		OnPageDestroyed(EventArgs.Empty);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Documents.DocumentPage.PageDestroyed" /> event. </summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
	protected void OnPageDestroyed(EventArgs e)
	{
		if (this.PageDestroyed != null)
		{
			this.PageDestroyed(this, e);
		}
	}

	/// <summary>Sets the <see cref="P:System.Windows.Documents.DocumentPage.Visual" /> that depicts the page.</summary>
	/// <param name="visual">The visual representation of the page.</param>
	protected void SetVisual(Visual visual)
	{
		_visual = visual;
	}

	/// <summary>Sets the <see cref="P:System.Windows.Documents.DocumentPage.Size" /> of the physical page as it will be after any cropping. </summary>
	/// <param name="size">The size of the page.</param>
	protected void SetSize(Size size)
	{
		_pageSize = size;
	}

	/// <summary>Sets the dimensions and location of the <see cref="P:System.Windows.Documents.DocumentPage.BleedBox" />. </summary>
	/// <param name="bleedBox">An object that specifies the size and location of a rectangle.</param>
	protected void SetBleedBox(Rect bleedBox)
	{
		_bleedBox = bleedBox;
	}

	/// <summary>Sets the dimension and location of the <see cref="P:System.Windows.Documents.DocumentPage.ContentBox" />. </summary>
	/// <param name="contentBox">An object that specifies the size and location of a rectangle.</param>
	protected void SetContentBox(Rect contentBox)
	{
		_contentBox = contentBox;
	}
}
