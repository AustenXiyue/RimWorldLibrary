using System;
using System.IO;
using System.Windows;
using System.Windows.Ink;

namespace MS.Internal.Ink;

internal class ISFClipboardData : ClipboardData
{
	private StrokeCollection _strokes;

	internal StrokeCollection Strokes => _strokes;

	internal ISFClipboardData()
	{
	}

	internal ISFClipboardData(StrokeCollection strokes)
	{
		_strokes = strokes;
	}

	internal override bool CanPaste(IDataObject dataObject)
	{
		return dataObject.GetDataPresent(StrokeCollection.InkSerializedFormat, autoConvert: false);
	}

	protected override bool CanCopy()
	{
		if (Strokes != null)
		{
			return Strokes.Count != 0;
		}
		return false;
	}

	protected override void DoCopy(IDataObject dataObject)
	{
		MemoryStream memoryStream = new MemoryStream();
		Strokes.Save(memoryStream);
		memoryStream.Position = 0L;
		dataObject.SetData(StrokeCollection.InkSerializedFormat, memoryStream);
	}

	protected override void DoPaste(IDataObject dataObject)
	{
		MemoryStream memoryStream = dataObject.GetData(StrokeCollection.InkSerializedFormat) as MemoryStream;
		StrokeCollection strokeCollection = null;
		bool flag = false;
		if (memoryStream != null && memoryStream != Stream.Null)
		{
			try
			{
				strokeCollection = new StrokeCollection(memoryStream);
				flag = true;
			}
			catch (ArgumentException)
			{
				flag = false;
			}
		}
		_strokes = (flag ? strokeCollection : new StrokeCollection());
	}
}
