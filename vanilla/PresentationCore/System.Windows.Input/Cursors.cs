namespace System.Windows.Input;

/// <summary>Defines a set of default cursors. </summary>
public static class Cursors
{
	private static int _cursorTypeCount = 28;

	private static Cursor[] _stockCursors = new Cursor[_cursorTypeCount];

	/// <summary>Gets a special cursor that is invisible. </summary>
	/// <returns>The none cursor.</returns>
	public static Cursor None => EnsureCursor(CursorType.None);

	/// <summary>Gets a <see cref="T:System.Windows.Input.Cursor" /> with which indicates that a particular region is invalid for a given operation.</summary>
	/// <returns>The No cursor.</returns>
	public static Cursor No => EnsureCursor(CursorType.No);

	/// <summary>Gets the Arrow <see cref="T:System.Windows.Input.Cursor" />. </summary>
	/// <returns>The arrow cursor.</returns>
	public static Cursor Arrow => EnsureCursor(CursorType.Arrow);

	/// <summary>Gets the <see cref="T:System.Windows.Input.Cursor" /> that appears when an application is starting. </summary>
	/// <returns>The AppStarting cursor.</returns>
	public static Cursor AppStarting => EnsureCursor(CursorType.AppStarting);

	/// <summary>Gets the crosshair <see cref="T:System.Windows.Input.Cursor" />. </summary>
	/// <returns>The Crosshair cursor.</returns>
	public static Cursor Cross => EnsureCursor(CursorType.Cross);

	/// <summary>Gets a help <see cref="T:System.Windows.Input.Cursor" /> which is a combination of an arrow and a question mark. </summary>
	/// <returns>The help cursor.</returns>
	public static Cursor Help => EnsureCursor(CursorType.Help);

	/// <summary>Gets an I-beam <see cref="T:System.Windows.Input.Cursor" />, which is used to show where the text cursor appears when the mouse is clicked. </summary>
	/// <returns>The IBeam cursor.</returns>
	public static Cursor IBeam => EnsureCursor(CursorType.IBeam);

	/// <summary>Gets a four-headed sizing <see cref="T:System.Windows.Input.Cursor" />, which consists of four joined arrows that point north, south, east, and west.  </summary>
	/// <returns>A four-headed sizing cursor.</returns>
	public static Cursor SizeAll => EnsureCursor(CursorType.SizeAll);

	/// <summary>Gets a two-headed northeast/southwest sizing <see cref="T:System.Windows.Input.Cursor" />. </summary>
	/// <returns>A two-headed northeast/southwest sizing cursor.</returns>
	public static Cursor SizeNESW => EnsureCursor(CursorType.SizeNESW);

	/// <summary>Gets a two-headed north/south sizing <see cref="T:System.Windows.Input.Cursor" />. </summary>
	/// <returns>A two-headed north/south sizing cursor.</returns>
	public static Cursor SizeNS => EnsureCursor(CursorType.SizeNS);

	/// <summary>Gets a two-headed northwest/southeast sizing <see cref="T:System.Windows.Input.Cursor" />. </summary>
	/// <returns>A two-headed northwest/southwest sizing cursor.</returns>
	public static Cursor SizeNWSE => EnsureCursor(CursorType.SizeNWSE);

	/// <summary>Gets a two-headed west/east sizing <see cref="T:System.Windows.Input.Cursor" />.</summary>
	/// <returns>A two-headed west/east sizing cursor.</returns>
	public static Cursor SizeWE => EnsureCursor(CursorType.SizeWE);

	/// <summary>Gets an up arrow <see cref="T:System.Windows.Input.Cursor" />, which is typically used to identify an insertion point.  </summary>
	/// <returns>An up arrow cursor.</returns>
	public static Cursor UpArrow => EnsureCursor(CursorType.UpArrow);

	/// <summary>Specifies a wait (or hourglass) <see cref="T:System.Windows.Input.Cursor" />. </summary>
	/// <returns>A wait cursor.</returns>
	public static Cursor Wait => EnsureCursor(CursorType.Wait);

	/// <summary>Gets a hand <see cref="T:System.Windows.Input.Cursor" />. </summary>
	/// <returns>The hand cursor.</returns>
	public static Cursor Hand => EnsureCursor(CursorType.Hand);

	/// <summary>Gets a pen <see cref="T:System.Windows.Input.Cursor" />.</summary>
	/// <returns>The pen cursor.</returns>
	public static Cursor Pen => EnsureCursor(CursorType.Pen);

	/// <summary>Gets the scroll north/south cursor.</summary>
	/// <returns>A scroll north/south <see cref="T:System.Windows.Input.Cursor" />.</returns>
	public static Cursor ScrollNS => EnsureCursor(CursorType.ScrollNS);

	/// <summary>Gets a west/east scrolling <see cref="T:System.Windows.Input.Cursor" />.</summary>
	/// <returns>A west/east scrolling cursor.</returns>
	public static Cursor ScrollWE => EnsureCursor(CursorType.ScrollWE);

	/// <summary>Gets the scroll all <see cref="T:System.Windows.Input.Cursor" />.</summary>
	/// <returns>The scroll all cursor.</returns>
	public static Cursor ScrollAll => EnsureCursor(CursorType.ScrollAll);

	/// <summary>Gets the scroll north <see cref="T:System.Windows.Input.Cursor" />.</summary>
	/// <returns>A scroll north cursor.</returns>
	public static Cursor ScrollN => EnsureCursor(CursorType.ScrollN);

	/// <summary>Gets the scroll south <see cref="T:System.Windows.Input.Cursor" />.</summary>
	/// <returns>The scroll south cursor.</returns>
	public static Cursor ScrollS => EnsureCursor(CursorType.ScrollS);

	/// <summary>Gets the scroll west <see cref="T:System.Windows.Input.Cursor" />.</summary>
	/// <returns>The scroll west cursor.</returns>
	public static Cursor ScrollW => EnsureCursor(CursorType.ScrollW);

	/// <summary>Gets the scroll east <see cref="T:System.Windows.Input.Cursor" />.</summary>
	/// <returns>A scroll east cursor.</returns>
	public static Cursor ScrollE => EnsureCursor(CursorType.ScrollE);

	/// <summary>Gets a scroll northwest cursor.</summary>
	/// <returns>The scroll northwest <see cref="T:System.Windows.Input.Cursor" />.</returns>
	public static Cursor ScrollNW => EnsureCursor(CursorType.ScrollNW);

	/// <summary>Gets the scroll northeast cursor.</summary>
	/// <returns>A scroll northeast <see cref="T:System.Windows.Input.Cursor" />.</returns>
	public static Cursor ScrollNE => EnsureCursor(CursorType.ScrollNE);

	/// <summary>Gets the scroll southwest <see cref="T:System.Windows.Input.Cursor" />.</summary>
	/// <returns>The scroll southwest cursor.</returns>
	public static Cursor ScrollSW => EnsureCursor(CursorType.ScrollSW);

	/// <summary>Gets a south/east scrolling <see cref="T:System.Windows.Input.Cursor" />. </summary>
	/// <returns>The south/east scrolling cursor.</returns>
	public static Cursor ScrollSE => EnsureCursor(CursorType.ScrollSE);

	/// <summary>Gets the arrow with a compact disk <see cref="T:System.Windows.Input.Cursor" />. </summary>
	/// <returns>The arrowCd cursor.</returns>
	public static Cursor ArrowCD => EnsureCursor(CursorType.ArrowCD);

	internal static Cursor EnsureCursor(CursorType cursorType)
	{
		if (_stockCursors[(int)cursorType] == null)
		{
			_stockCursors[(int)cursorType] = new Cursor(cursorType);
		}
		return _stockCursors[(int)cursorType];
	}
}
