using System.Collections.Generic;
using System.IO;

namespace System.Windows.Documents;

internal interface ITextRange
{
	bool IgnoreTextUnitBoundaries { get; }

	ITextPointer Start { get; }

	ITextPointer End { get; }

	bool IsEmpty { get; }

	List<TextSegment> TextSegments { get; }

	bool HasConcreteTextContainer { get; }

	string Text { get; set; }

	string Xml { get; }

	bool IsTableCellRange { get; }

	int ChangeBlockLevel { get; }

	uint _ContentGeneration { get; set; }

	bool _IsTableCellRange { get; set; }

	List<TextSegment> _TextSegments { get; set; }

	int _ChangeBlockLevel { get; set; }

	ChangeBlockUndoRecord _ChangeBlockUndoRecord { get; set; }

	bool _IsChanged { get; set; }

	event EventHandler Changed;

	bool Contains(ITextPointer position);

	void Select(ITextPointer position1, ITextPointer position2);

	void SelectWord(ITextPointer position);

	void SelectParagraph(ITextPointer position);

	void ApplyTypingHeuristics(bool overType);

	object GetPropertyValue(DependencyProperty formattingProperty);

	UIElement GetUIElementSelected();

	bool CanSave(string dataFormat);

	void Save(Stream stream, string dataFormat);

	void Save(Stream stream, string dataFormat, bool preserveTextElements);

	void BeginChange();

	void BeginChangeNoUndo();

	void EndChange();

	void EndChange(bool disableScroll, bool skipEvents);

	IDisposable DeclareChangeBlock();

	IDisposable DeclareChangeBlock(bool disableScroll);

	void NotifyChanged(bool disableScroll, bool skipEvents);

	void FireChanged();
}
