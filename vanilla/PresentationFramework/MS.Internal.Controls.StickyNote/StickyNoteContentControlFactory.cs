using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Xml;

namespace MS.Internal.Controls.StickyNote;

internal static class StickyNoteContentControlFactory
{
	private class StickyNoteRichTextBox : StickyNoteContentControl
	{
		public override bool IsEmpty
		{
			get
			{
				RichTextBox richTextBox = (RichTextBox)base.InnerControl;
				return new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).IsEmpty;
			}
		}

		public override StickyNoteType Type => StickyNoteType.Text;

		public StickyNoteRichTextBox(RichTextBox rtb)
			: base(rtb)
		{
			DataObject.AddPastingHandler(rtb, OnPastingDataObject);
		}

		public override void Clear()
		{
			((RichTextBox)base.InnerControl).Document = new FlowDocument(new Paragraph(new Run()));
		}

		public override void Save(XmlNode node)
		{
			RichTextBox richTextBox = (RichTextBox)base.InnerControl;
			TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
			if (textRange.IsEmpty)
			{
				return;
			}
			using MemoryStream memoryStream = new MemoryStream();
			textRange.Save(memoryStream, DataFormats.Xaml);
			if (memoryStream.Length.CompareTo(1610612733L) > 0)
			{
				throw new InvalidOperationException(SR.MaximumNoteSizeExceeded);
			}
			node.InnerText = Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
		}

		public override void Load(XmlNode node)
		{
			RichTextBox richTextBox = (RichTextBox)base.InnerControl;
			FlowDocument flowDocument = new FlowDocument();
			TextRange textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd, useRestrictiveXamlXmlReader: true);
			using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(node.InnerText)))
			{
				textRange.Load(stream, DataFormats.Xaml);
			}
			richTextBox.Document = flowDocument;
		}

		private void OnPastingDataObject(object sender, DataObjectPastingEventArgs e)
		{
			if (e.FormatToApply == DataFormats.Rtf)
			{
				UTF8Encoding uTF8Encoding = new UTF8Encoding();
				string s = e.DataObject.GetData(DataFormats.Rtf) as string;
				MemoryStream stream = new MemoryStream(uTF8Encoding.GetBytes(s));
				FlowDocument flowDocument = new FlowDocument();
				TextRange textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
				textRange.Load(stream, DataFormats.Rtf);
				MemoryStream memoryStream = new MemoryStream();
				textRange.Save(memoryStream, DataFormats.Xaml);
				DataObject dataObject = new DataObject();
				dataObject.SetData(DataFormats.Xaml, uTF8Encoding.GetString(memoryStream.GetBuffer()));
				e.DataObject = dataObject;
				e.FormatToApply = DataFormats.Xaml;
			}
			else if (e.FormatToApply == DataFormats.Bitmap || e.FormatToApply == DataFormats.EnhancedMetafile || e.FormatToApply == DataFormats.MetafilePicture || e.FormatToApply == DataFormats.Tiff)
			{
				e.CancelCommand();
			}
			else if (e.FormatToApply == DataFormats.XamlPackage)
			{
				e.FormatToApply = DataFormats.Xaml;
			}
		}
	}

	private class StickyNoteInkCanvas : StickyNoteContentControl
	{
		public override bool IsEmpty => ((InkCanvas)base.InnerControl).Strokes.Count == 0;

		public override StickyNoteType Type => StickyNoteType.Ink;

		public StickyNoteInkCanvas(InkCanvas canvas)
			: base(canvas)
		{
		}

		public override void Clear()
		{
			((InkCanvas)base.InnerControl).Strokes.Clear();
		}

		public override void Save(XmlNode node)
		{
			StrokeCollection strokes = ((InkCanvas)base.InnerControl).Strokes;
			using MemoryStream memoryStream = new MemoryStream();
			strokes.Save(memoryStream);
			if (memoryStream.Length.CompareTo(1610612733L) > 0)
			{
				throw new InvalidOperationException(SR.MaximumNoteSizeExceeded);
			}
			node.InnerText = Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
		}

		public override void Load(XmlNode node)
		{
			StrokeCollection strokes = null;
			if (string.IsNullOrEmpty(node.InnerText))
			{
				strokes = new StrokeCollection();
			}
			else
			{
				using MemoryStream stream = new MemoryStream(Convert.FromBase64String(node.InnerText));
				strokes = new StrokeCollection(stream);
			}
			((InkCanvas)base.InnerControl).Strokes = strokes;
		}
	}

	public static StickyNoteContentControl CreateContentControl(StickyNoteType type, UIElement content)
	{
		StickyNoteContentControl result = null;
		switch (type)
		{
		case StickyNoteType.Text:
			result = new StickyNoteRichTextBox((content as RichTextBox) ?? throw new InvalidOperationException(SR.Format(SR.InvalidStickyNoteTemplate, type, typeof(RichTextBox), "PART_ContentControl")));
			break;
		case StickyNoteType.Ink:
			result = new StickyNoteInkCanvas((content as InkCanvas) ?? throw new InvalidOperationException(SR.Format(SR.InvalidStickyNoteTemplate, type, typeof(InkCanvas), "PART_ContentControl")));
			break;
		}
		return result;
	}
}
