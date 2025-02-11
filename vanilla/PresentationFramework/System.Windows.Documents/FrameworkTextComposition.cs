using System.Runtime.InteropServices;
using System.Windows.Input;
using MS.Internal;
using MS.Win32;

namespace System.Windows.Documents;

/// <summary>Represents a composition during the text input events of a <see cref="T:System.Windows.Controls.TextBox" />. </summary>
public class FrameworkTextComposition : TextComposition
{
	private ITextPointer _resultStart;

	private ITextPointer _resultEnd;

	private ITextPointer _compositionStart;

	private ITextPointer _compositionEnd;

	private int _offset;

	private int _length;

	private readonly object _owner;

	private bool _pendingComplete;

	/// <summary>Gets the offset of the finalized text when the <see cref="E:System.Windows.Input.TextCompositionManager.TextInput" /> event occurs.</summary>
	/// <returns>The offset of the finalized text when the <see cref="E:System.Windows.Input.TextCompositionManager.TextInput" /> event occurs.</returns>
	public int ResultOffset
	{
		get
		{
			if (_ResultStart != null)
			{
				return _offset;
			}
			return -1;
		}
	}

	/// <summary>Gets the length of the finalized text in Unicode symbols when the <see cref="E:System.Windows.Input.TextCompositionManager.TextInput" /> event occurs.</summary>
	/// <returns>The length of the finalized text in Unicode symbols when the <see cref="E:System.Windows.Input.TextCompositionManager.TextInput" /> event occurs.</returns>
	public int ResultLength
	{
		get
		{
			if (_ResultStart != null)
			{
				return _length;
			}
			return -1;
		}
	}

	/// <summary>Gets the position at which the composition text occurs in the <see cref="T:System.Windows.Controls.TextBox" />.</summary>
	/// <returns>The position at which the composition text occurs in the <see cref="T:System.Windows.Controls.TextBox" />.</returns>
	public int CompositionOffset
	{
		get
		{
			if (_CompositionStart != null)
			{
				return _offset;
			}
			return -1;
		}
	}

	/// <summary>Gets the length of the current composition in Unicode symbols.</summary>
	/// <returns>The length of the current composition in Unicode symbols.</returns>
	public int CompositionLength
	{
		get
		{
			if (_CompositionStart != null)
			{
				return _length;
			}
			return -1;
		}
	}

	internal ITextPointer _ResultStart => _resultStart;

	internal ITextPointer _ResultEnd => _resultEnd;

	internal ITextPointer _CompositionStart => _compositionStart;

	internal ITextPointer _CompositionEnd => _compositionEnd;

	internal bool PendingComplete => _pendingComplete;

	internal object Owner => _owner;

	internal FrameworkTextComposition(InputManager inputManager, IInputElement source, object owner)
		: base(inputManager, source, string.Empty, TextCompositionAutoComplete.Off)
	{
		_owner = owner;
	}

	/// <summary>Finalizes the composition. </summary>
	public override void Complete()
	{
		_pendingComplete = true;
	}

	internal static void CompleteCurrentComposition(MS.Win32.UnsafeNativeMethods.ITfDocumentMgr documentMgr)
	{
		documentMgr.GetBase(out var context);
		MS.Win32.UnsafeNativeMethods.ITfCompositionView composition = GetComposition(context);
		if (composition != null)
		{
			(context as MS.Win32.UnsafeNativeMethods.ITfContextOwnerCompositionServices).TerminateComposition(composition);
			Marshal.ReleaseComObject(composition);
		}
		Marshal.ReleaseComObject(context);
	}

	internal static MS.Win32.UnsafeNativeMethods.ITfCompositionView GetCurrentCompositionView(MS.Win32.UnsafeNativeMethods.ITfDocumentMgr documentMgr)
	{
		documentMgr.GetBase(out var context);
		MS.Win32.UnsafeNativeMethods.ITfCompositionView composition = GetComposition(context);
		Marshal.ReleaseComObject(context);
		return composition;
	}

	internal void SetResultPositions(ITextPointer start, ITextPointer end, string text)
	{
		Invariant.Assert(start != null);
		Invariant.Assert(end != null);
		Invariant.Assert(text != null);
		_compositionStart = null;
		_compositionEnd = null;
		_resultStart = start.GetFrozenPointer(LogicalDirection.Backward);
		_resultEnd = end.GetFrozenPointer(LogicalDirection.Forward);
		base.Text = text;
		base.CompositionText = string.Empty;
		_offset = ((_resultStart == null) ? (-1) : _resultStart.Offset);
		_length = ((_resultStart == null) ? (-1) : _resultStart.GetOffsetToPosition(_resultEnd));
	}

	internal void SetCompositionPositions(ITextPointer start, ITextPointer end, string text)
	{
		Invariant.Assert(start != null);
		Invariant.Assert(end != null);
		Invariant.Assert(text != null);
		_compositionStart = start.GetFrozenPointer(LogicalDirection.Backward);
		_compositionEnd = end.GetFrozenPointer(LogicalDirection.Forward);
		_resultStart = null;
		_resultEnd = null;
		base.Text = string.Empty;
		base.CompositionText = text;
		_offset = ((_compositionStart == null) ? (-1) : _compositionStart.Offset);
		_length = ((_compositionStart == null) ? (-1) : _compositionStart.GetOffsetToPosition(_compositionEnd));
	}

	private static MS.Win32.UnsafeNativeMethods.ITfCompositionView GetComposition(MS.Win32.UnsafeNativeMethods.ITfContext context)
	{
		MS.Win32.UnsafeNativeMethods.ITfCompositionView[] array = new MS.Win32.UnsafeNativeMethods.ITfCompositionView[1];
		((MS.Win32.UnsafeNativeMethods.ITfContextComposition)context).EnumCompositions(out var enumView);
		enumView.Next(1, array, out var _);
		Marshal.ReleaseComObject(enumView);
		return array[0];
	}
}
