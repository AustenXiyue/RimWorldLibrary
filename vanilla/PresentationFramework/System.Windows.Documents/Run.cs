using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Markup;
using MS.Internal;
using MS.Internal.Documents;

namespace System.Windows.Documents;

/// <summary>An inline-level flow content element intended to contain a run of formatted or unformatted text.</summary>
[ContentProperty("Text")]
public class Run : Inline
{
	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Run.Text" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Run.Text" /> dependency property.</returns>
	public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(Run), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextPropertyChanged, CoerceText));

	private int _changeEventNestingCount;

	private bool _isInsideDeferredSet;

	/// <summary>Gets or sets the unformatted text contents of this text <see cref="T:System.Windows.Documents.Run" />.</summary>
	/// <returns>A string that specifies the unformatted text contents of this text <see cref="T:System.Windows.Documents.Run" />. The default is <see cref="F:System.String.Empty" />.</returns>
	public string Text
	{
		get
		{
			return (string)GetValue(TextProperty);
		}
		set
		{
			SetValue(TextProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 13;

	/// <summary>Initializes a new, default instance of the <see cref="T:System.Windows.Documents.Run" /> class.</summary>
	public Run()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Run" /> class, taking a specified string as the initial contents of the text run.</summary>
	/// <param name="text">A string specifying the initial contents of the <see cref="T:System.Windows.Documents.Run" /> object.</param>
	public Run(string text)
		: this(text, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Run" /> class, taking a specified string as the initial contents of the text run, and a <see cref="T:System.Windows.Documents.TextPointer" /> specifying an insertion position for the text run.</summary>
	/// <param name="text">A string specifying the initial contents of the <see cref="T:System.Windows.Documents.Run" /> object.</param>
	/// <param name="insertionPosition">A <see cref="T:System.Windows.Documents.TextPointer" /> specifying an insertion position at which to insert the text run after it is created, or null for no automatic insertion.</param>
	public Run(string text, TextPointer insertionPosition)
	{
		insertionPosition?.TextContainer.BeginChange();
		try
		{
			insertionPosition?.InsertInline(this);
			if (text != null)
			{
				base.ContentStart.InsertTextInRun(text);
			}
		}
		finally
		{
			insertionPosition?.TextContainer.EndChange();
		}
	}

	internal override void OnTextUpdated()
	{
		ValueSource valueSource = DependencyPropertyHelper.GetValueSource(this, TextProperty);
		if (!_isInsideDeferredSet && (_changeEventNestingCount == 0 || (valueSource.BaseValueSource == BaseValueSource.Local && !valueSource.IsExpression)))
		{
			_changeEventNestingCount++;
			_isInsideDeferredSet = true;
			try
			{
				SetCurrentDeferredValue(TextProperty, new DeferredRunTextReference(this));
			}
			finally
			{
				_isInsideDeferredSet = false;
				_changeEventNestingCount--;
			}
		}
	}

	internal override void BeforeLogicalTreeChange()
	{
		_changeEventNestingCount++;
	}

	internal override void AfterLogicalTreeChange()
	{
		_changeEventNestingCount--;
	}

	/// <summary>Returns a value that indicates whether or not the effective value of the <see cref="P:System.Windows.Documents.Run.Text" /> property should be serialized during serialization of a <see cref="T:System.Windows.Documents.Run" /> object.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Documents.Run.Text" /> property should be serialized; otherwise, false.</returns>
	/// <param name="manager">A serialization service manager object for this object.</param>
	/// <exception cref="T:System.NullReferenceException">
	///   <paramref name="manager" /> is null.</exception>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeText(XamlDesignerSerializationManager manager)
	{
		if (manager != null)
		{
			return manager.XmlWriter == null;
		}
		return false;
	}

	private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Run run = (Run)d;
		if (run._changeEventNestingCount > 0)
		{
			return;
		}
		Invariant.Assert(!e.NewEntry.IsDeferredReference);
		string text = (string)e.NewValue;
		if (text == null)
		{
			text = string.Empty;
		}
		run._changeEventNestingCount++;
		try
		{
			TextContainer textContainer = run.TextContainer;
			textContainer.BeginChange();
			try
			{
				TextPointer contentStart = run.ContentStart;
				if (!run.IsEmpty)
				{
					textContainer.DeleteContentInternal(contentStart, run.ContentEnd);
				}
				contentStart.InsertTextInRun(text);
			}
			finally
			{
				textContainer.EndChange();
			}
		}
		finally
		{
			run._changeEventNestingCount--;
		}
		if (run.TextContainer.Parent is FlowDocument { Parent: RichTextBox parent } && run.HasExpression(run.LookupEntry(TextProperty.GlobalIndex), TextProperty))
		{
			UndoManager undoManager = parent.TextEditor._GetUndoManager();
			if (undoManager != null && undoManager.IsEnabled)
			{
				undoManager.Clear();
			}
		}
	}

	private static object CoerceText(DependencyObject d, object baseValue)
	{
		if (baseValue == null)
		{
			baseValue = string.Empty;
		}
		return baseValue;
	}
}
