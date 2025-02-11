using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace System.Windows.Documents;

internal sealed class FixedSOMPageConstructor
{
	private FixedSOMFixedBlock _currentFixedBlock;

	private int _pageIndex;

	private FixedPage _fixedPage;

	private FixedSOMPage _fixedSOMPage;

	private List<FixedNode> _fixedNodes;

	private FixedSOMLineCollection _lines;

	private GeometryWalker _geometryWalker;

	public FixedSOMPage FixedSOMPage => _fixedSOMPage;

	public FixedSOMPageConstructor(FixedPage fixedPage, int pageIndex)
	{
		_fixedPage = fixedPage;
		_pageIndex = pageIndex;
		_fixedSOMPage = new FixedSOMPage();
		_fixedSOMPage.CultureInfo = _fixedPage.Language.GetCompatibleCulture();
		_fixedNodes = new List<FixedNode>();
		_lines = new FixedSOMLineCollection();
	}

	public FixedSOMPage ConstructPageStructure(List<FixedNode> fixedNodes)
	{
		foreach (FixedNode fixedNode in fixedNodes)
		{
			DependencyObject element = _fixedPage.GetElement(fixedNode);
			if (element is Glyphs)
			{
				_ProcessGlyphsElement(element as Glyphs, fixedNode);
			}
			else if (element is Image || (element is Path && (element as Path).Fill is ImageBrush))
			{
				_ProcessImage(element, fixedNode);
			}
		}
		foreach (FixedSOMSemanticBox semanticBox in _fixedSOMPage.SemanticBoxes)
		{
			(semanticBox as FixedSOMContainer).SemanticBoxes.Sort();
		}
		_DetectTables();
		_CombinePass();
		_CreateGroups(_fixedSOMPage);
		_fixedSOMPage.SemanticBoxes.Sort();
		return _fixedSOMPage;
	}

	public void ProcessPath(Path path, Matrix transform)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		Geometry data = path.Data;
		bool flag = path.Fill != null;
		bool flag2 = path.Stroke != null;
		if (data == null || (!flag && !flag2))
		{
			return;
		}
		Transform renderTransform = path.RenderTransform;
		if (renderTransform != null)
		{
			transform *= renderTransform.Value;
		}
		if (flag && _ProcessFilledRect(transform, data.Bounds))
		{
			flag = false;
			if (!flag2)
			{
				return;
			}
		}
		if (data is StreamGeometry geometry)
		{
			if (_geometryWalker == null)
			{
				_geometryWalker = new GeometryWalker(this);
			}
			_geometryWalker.FindLines(geometry, flag2, flag, transform);
			return;
		}
		PathGeometry pathGeometry = PathGeometry.CreateFromGeometry(data);
		if (pathGeometry != null)
		{
			if (flag)
			{
				_ProcessSolidPath(transform, pathGeometry);
			}
			if (flag2)
			{
				_ProcessOutlinePath(transform, pathGeometry);
			}
		}
	}

	private void _ProcessImage(DependencyObject obj, FixedNode fixedNode)
	{
		FixedSOMImage fixedSOMImage = null;
		while (true)
		{
			if (obj is Image image)
			{
				fixedSOMImage = FixedSOMImage.Create(_fixedPage, image, fixedNode);
				break;
			}
			if (obj is Path path)
			{
				fixedSOMImage = FixedSOMImage.Create(_fixedPage, path, fixedNode);
				break;
			}
		}
		FixedSOMFixedBlock fixedSOMFixedBlock = new FixedSOMFixedBlock(_fixedSOMPage);
		fixedSOMFixedBlock.AddImage(fixedSOMImage);
		_fixedSOMPage.AddFixedBlock(fixedSOMFixedBlock);
		_currentFixedBlock = fixedSOMFixedBlock;
	}

	private void _ProcessGlyphsElement(Glyphs glyphs, FixedNode node)
	{
		string unicodeString = glyphs.UnicodeString;
		if (unicodeString.Length == 0 || glyphs.FontRenderingEmSize <= 0.0)
		{
			return;
		}
		GlyphRun glyphRun = glyphs.ToGlyphRun();
		if (glyphRun == null)
		{
			return;
		}
		Rect rect = glyphRun.ComputeAlignmentBox();
		rect.Offset(glyphs.OriginX, glyphs.OriginY);
		GlyphTypeface glyphTypeface = glyphRun.GlyphTypeface;
		GeneralTransform trans = glyphs.TransformToAncestor(_fixedPage);
		int num = -1;
		double num2 = 0.0;
		double num3 = 0.0;
		int num4 = 0;
		int num5 = 0;
		double num6 = rect.Left;
		int num7 = num;
		do
		{
			num = unicodeString.IndexOf(" ", num + 1, unicodeString.Length - num - 1, StringComparison.Ordinal);
			if (num < 0)
			{
				continue;
			}
			num7 = ((glyphRun.ClusterMap == null || glyphRun.ClusterMap.Count <= 0) ? num : glyphRun.ClusterMap[num]);
			double num8 = glyphTypeface.AdvanceWidths[glyphRun.GlyphIndices[num7]] * glyphRun.FontRenderingEmSize;
			double num9 = glyphRun.AdvanceWidths[num7];
			if (!(num9 / num8 > 2.0))
			{
				continue;
			}
			num2 = 0.0;
			for (int i = num4; i < num7; i++)
			{
				num2 += glyphRun.AdvanceWidths[i];
			}
			num3 += num2;
			num4 = num7 + 1;
			if (_lines.IsVerticallySeparated(glyphRun.BaselineOrigin.X + num3, rect.Top, glyphRun.BaselineOrigin.X + num3 + num9, rect.Bottom))
			{
				Rect boundingRect = new Rect(num6, rect.Top, num2 + num8, rect.Height);
				int endIndex = num;
				if ((num == 0 || unicodeString[num - 1] == ' ') && num != unicodeString.Length - 1)
				{
					endIndex = num + 1;
				}
				_CreateTextRun(boundingRect, trans, glyphs, node, num5, endIndex);
				num6 = num6 + num2 + num9;
				num5 = num + 1;
			}
			num3 += num9;
		}
		while (num >= 0 && num < unicodeString.Length - 1);
		if (num5 < unicodeString.Length)
		{
			Rect boundingRect2 = new Rect(num6, rect.Top, rect.Right - num6, rect.Height);
			_CreateTextRun(boundingRect2, trans, glyphs, node, num5, unicodeString.Length);
		}
	}

	private void _CreateTextRun(Rect boundingRect, GeneralTransform trans, Glyphs glyphs, FixedNode node, int startIndex, int endIndex)
	{
		if (startIndex < endIndex)
		{
			FixedSOMTextRun textRun = FixedSOMTextRun.Create(boundingRect, trans, glyphs, node, startIndex, endIndex, allowReverseGlyphs: true);
			FixedSOMFixedBlock fixedSOMFixedBlock = _GetContainingFixedBlock(textRun);
			if (fixedSOMFixedBlock == null)
			{
				fixedSOMFixedBlock = new FixedSOMFixedBlock(_fixedSOMPage);
				fixedSOMFixedBlock.AddTextRun(textRun);
				_fixedSOMPage.AddFixedBlock(fixedSOMFixedBlock);
			}
			else
			{
				fixedSOMFixedBlock.AddTextRun(textRun);
			}
			_currentFixedBlock = fixedSOMFixedBlock;
		}
	}

	private FixedSOMFixedBlock _GetContainingFixedBlock(FixedSOMTextRun textRun)
	{
		FixedSOMFixedBlock result = null;
		if (_currentFixedBlock == null)
		{
			return null;
		}
		if (_currentFixedBlock != null && _IsCombinable(_currentFixedBlock, textRun))
		{
			result = _currentFixedBlock;
		}
		else
		{
			Rect boundingRect = textRun.BoundingRect;
			Rect boundingRect2 = _currentFixedBlock.BoundingRect;
			if (Math.Abs(boundingRect.Left - boundingRect2.Left) <= textRun.DefaultCharWidth || Math.Abs(boundingRect.Right - boundingRect2.Right) <= textRun.DefaultCharWidth)
			{
				return null;
			}
			foreach (FixedSOMSemanticBox semanticBox in _fixedSOMPage.SemanticBoxes)
			{
				if (semanticBox is FixedSOMFixedBlock && _IsCombinable(semanticBox as FixedSOMFixedBlock, textRun))
				{
					result = semanticBox as FixedSOMFixedBlock;
				}
			}
		}
		return result;
	}

	private bool _IsCombinable(FixedSOMFixedBlock fixedBlock, FixedSOMTextRun textRun)
	{
		if (fixedBlock.SemanticBoxes.Count == 0)
		{
			return false;
		}
		if (fixedBlock.IsFloatingImage)
		{
			return false;
		}
		Rect boundingRect = textRun.BoundingRect;
		Rect boundingRect2 = fixedBlock.BoundingRect;
		FixedSOMTextRun fixedSOMTextRun = null;
		if (fixedBlock.SemanticBoxes[fixedBlock.SemanticBoxes.Count - 1] is FixedSOMTextRun fixedSOMTextRun2 && boundingRect.Bottom <= fixedSOMTextRun2.BoundingRect.Top)
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		double num = boundingRect.Height * 0.2;
		if (boundingRect.Bottom - num < boundingRect2.Top)
		{
			flag = true;
			fixedSOMTextRun = fixedBlock.SemanticBoxes[0] as FixedSOMTextRun;
		}
		else if (boundingRect.Top + num > boundingRect2.Bottom)
		{
			flag2 = true;
			fixedSOMTextRun = fixedBlock.SemanticBoxes[fixedBlock.SemanticBoxes.Count - 1] as FixedSOMTextRun;
		}
		if ((fixedBlock.IsWhiteSpace || textRun.IsWhiteSpace) && (fixedBlock != _currentFixedBlock || fixedSOMTextRun != null || !_IsSpatiallyCombinable(boundingRect2, boundingRect, textRun.DefaultCharWidth * 3.0, 0.0)))
		{
			return false;
		}
		if (fixedBlock.Matrix.M11 != textRun.Matrix.M11 || fixedBlock.Matrix.M12 != textRun.Matrix.M12 || fixedBlock.Matrix.M21 != textRun.Matrix.M21 || fixedBlock.Matrix.M22 != textRun.Matrix.M22)
		{
			return false;
		}
		if (fixedSOMTextRun != null)
		{
			double num2 = fixedBlock.LineHeight / boundingRect.Height;
			if (num2 < 1.0)
			{
				num2 = 1.0 / num2;
			}
			if (num2 > 1.1 && !FixedTextBuilder.IsSameLine(fixedSOMTextRun.BoundingRect.Top - boundingRect.Top, boundingRect.Height, fixedSOMTextRun.BoundingRect.Height))
			{
				return false;
			}
		}
		double num3 = textRun.DefaultCharWidth;
		if (num3 < 1.0)
		{
			num3 = 1.0;
		}
		double num4 = 0.0;
		double num5 = fixedBlock.LineHeight / boundingRect.Height;
		if (num5 < 1.0)
		{
			num5 = 1.0 / num5;
		}
		if (!_IsSpatiallyCombinable(inflateH: (fixedBlock != _currentFixedBlock || fixedSOMTextRun != null || !(num5 < 1.5)) ? (num3 * 1.5) : 200.0, rect1: boundingRect2, rect2: boundingRect, inflateV: boundingRect.Height * 0.7))
		{
			return false;
		}
		if (fixedBlock.SemanticBoxes[fixedBlock.SemanticBoxes.Count - 1] is FixedSOMElement { FixedNode: var fixedNode } && fixedNode.CompareTo(textRun.FixedNode) == 0)
		{
			return false;
		}
		if (flag || flag2)
		{
			double num6 = 0.0;
			double num7 = 0.0;
			double num8 = boundingRect.Height * 0.2;
			if (flag2)
			{
				num7 = boundingRect2.Bottom - num8;
				num6 = boundingRect.Top + num8;
			}
			else
			{
				num7 = boundingRect.Bottom - num8;
				num6 = boundingRect2.Top + num8;
			}
			double left = ((boundingRect2.Left > boundingRect.Left) ? boundingRect2.Left : boundingRect.Left);
			double right = ((boundingRect2.Right < boundingRect.Right) ? boundingRect2.Right : boundingRect.Right);
			return !_lines.IsHorizontallySeparated(left, num7, right, num6);
		}
		double num9 = ((boundingRect2.Right < boundingRect.Right) ? boundingRect2.Right : boundingRect.Right);
		double num10 = ((boundingRect2.Left > boundingRect.Left) ? boundingRect2.Left : boundingRect.Left);
		if (num9 < num10)
		{
			return !_lines.IsVerticallySeparated(num9, boundingRect.Top, num10, boundingRect.Bottom);
		}
		return true;
	}

	private bool _IsSpatiallyCombinable(FixedSOMSemanticBox box1, FixedSOMSemanticBox box2, double inflateH, double inflateV)
	{
		return _IsSpatiallyCombinable(box1.BoundingRect, box2.BoundingRect, inflateH, inflateV);
	}

	private bool _IsSpatiallyCombinable(Rect rect1, Rect rect2, double inflateH, double inflateV)
	{
		if (rect1.IntersectsWith(rect2))
		{
			return true;
		}
		rect1.Inflate(inflateH, inflateV);
		if (rect1.IntersectsWith(rect2))
		{
			return true;
		}
		return false;
	}

	private void _DetectTables()
	{
		double minLineSeparation = FixedSOMLineRanges.MinLineSeparation;
		List<FixedSOMLineRanges> horizontalLines = _lines.HorizontalLines;
		List<FixedSOMLineRanges> verticalLines = _lines.VerticalLines;
		if (horizontalLines.Count < 2 || verticalLines.Count < 2)
		{
			return;
		}
		List<FixedSOMTableRow> list = new List<FixedSOMTableRow>();
		FixedSOMTableRow fixedSOMTableRow = null;
		for (int i = 0; i < horizontalLines.Count; i++)
		{
			int j = 0;
			int k = -1;
			int num = -1;
			int num2 = -1;
			int num3 = -1;
			double line = horizontalLines[i].Line + minLineSeparation;
			for (int l = 0; l < horizontalLines[i].Count; l++)
			{
				double num4 = horizontalLines[i].Start[l] - minLineSeparation;
				double num5 = horizontalLines[i].End[l] + minLineSeparation;
				int num6 = -1;
				for (; j < verticalLines.Count && verticalLines[j].Line < num4; j++)
				{
				}
				for (; j < verticalLines.Count && verticalLines[j].Line < num5; j++)
				{
					int lineAt = verticalLines[j].GetLineAt(line);
					if (lineAt == -1)
					{
						continue;
					}
					double num7 = verticalLines[j].End[lineAt];
					if (num6 != -1 && horizontalLines[k].Line < num7 + minLineSeparation && horizontalLines[k].End[num] + minLineSeparation > verticalLines[j].Line)
					{
						double line2 = horizontalLines[i].Line;
						double line3 = horizontalLines[k].Line;
						double line4 = verticalLines[num6].Line;
						double line5 = verticalLines[j].Line;
						FixedSOMTableCell cell = new FixedSOMTableCell(line4, line2, line5, line3);
						if (num6 != num3 || k != num2)
						{
							fixedSOMTableRow = new FixedSOMTableRow();
							list.Add(fixedSOMTableRow);
						}
						fixedSOMTableRow.AddCell(cell);
						num3 = j;
						num2 = k;
					}
					num6 = -1;
					for (k = i + 1; k < horizontalLines.Count && horizontalLines[k].Line < num7 + minLineSeparation; k++)
					{
						num = horizontalLines[k].GetLineAt(verticalLines[j].Line + minLineSeparation);
						if (num != -1)
						{
							num6 = j;
							break;
						}
					}
				}
			}
		}
		_FillTables(list);
	}

	public void _AddLine(Point startP, Point endP, Matrix transform)
	{
		startP = transform.Transform(startP);
		endP = transform.Transform(endP);
		if (startP.X == endP.X)
		{
			_lines.AddVertical(startP, endP);
		}
		else if (startP.Y == endP.Y)
		{
			_lines.AddHorizontal(startP, endP);
		}
	}

	private void _CombinePass()
	{
		if (_fixedSOMPage.SemanticBoxes.Count < 2)
		{
			return;
		}
		int count;
		do
		{
			count = _fixedSOMPage.SemanticBoxes.Count;
			List<FixedSOMSemanticBox> semanticBoxes = _fixedSOMPage.SemanticBoxes;
			for (int i = 0; i < semanticBoxes.Count; i++)
			{
				if (semanticBoxes[i] is FixedSOMTable fixedSOMTable)
				{
					for (int j = i + 1; j < semanticBoxes.Count; j++)
					{
						if (semanticBoxes[j] is FixedSOMTable fixedSOMTable2 && fixedSOMTable.AddContainer(fixedSOMTable2))
						{
							semanticBoxes.Remove(fixedSOMTable2);
						}
					}
				}
				else
				{
					if (!(semanticBoxes[i] is FixedSOMFixedBlock { IsFloatingImage: false } fixedSOMFixedBlock))
					{
						continue;
					}
					for (int k = i + 1; k < semanticBoxes.Count; k++)
					{
						if (semanticBoxes[k] is FixedSOMFixedBlock { IsFloatingImage: false, Matrix: var matrix } fixedSOMFixedBlock2 && matrix.Equals(fixedSOMFixedBlock.Matrix) && _IsSpatiallyCombinable(fixedSOMFixedBlock, fixedSOMFixedBlock2, 0.0, 0.0))
						{
							fixedSOMFixedBlock.CombineWith(fixedSOMFixedBlock2);
							semanticBoxes.Remove(fixedSOMFixedBlock2);
						}
					}
				}
			}
		}
		while (_fixedSOMPage.SemanticBoxes.Count > 1 && _fixedSOMPage.SemanticBoxes.Count != count);
	}

	internal bool _ProcessFilledRect(Matrix transform, Rect bounds)
	{
		if (bounds.Height > bounds.Width && bounds.Width < 10.0 && bounds.Height > bounds.Width * 5.0)
		{
			double x = bounds.Left + 0.5 * bounds.Width;
			_AddLine(new Point(x, bounds.Top), new Point(x, bounds.Bottom), transform);
			return true;
		}
		if (bounds.Height < 10.0 && bounds.Width > bounds.Height * 5.0)
		{
			double y = bounds.Top + 0.5 * bounds.Height;
			_AddLine(new Point(bounds.Left, y), new Point(bounds.Right, y), transform);
			return true;
		}
		return false;
	}

	private void _ProcessSolidPath(Matrix transform, PathGeometry pathGeom)
	{
		PathFigureCollection figures = pathGeom.Figures;
		if (figures == null || figures.Count <= 1)
		{
			return;
		}
		foreach (PathFigure item in figures)
		{
			PathGeometry pathGeometry = new PathGeometry();
			pathGeometry.Figures.Add(item);
			_ProcessFilledRect(transform, pathGeometry.Bounds);
		}
	}

	private void _ProcessOutlinePath(Matrix transform, PathGeometry pathGeom)
	{
		foreach (PathFigure figure in pathGeom.Figures)
		{
			PathSegmentCollection segments = figure.Segments;
			Point startPoint = figure.StartPoint;
			Point startP = startPoint;
			foreach (PathSegment item in segments)
			{
				if (item is ArcSegment)
				{
					startP = (item as ArcSegment).Point;
				}
				else if (item is BezierSegment)
				{
					startP = (item as BezierSegment).Point3;
				}
				else if (item is LineSegment)
				{
					Point point = (item as LineSegment).Point;
					_AddLine(startP, point, transform);
					startP = point;
				}
				else if (item is PolyBezierSegment)
				{
					PointCollection points = (item as PolyBezierSegment).Points;
					startP = points[points.Count - 1];
				}
				else if (item is PolyLineSegment)
				{
					foreach (Point point2 in (item as PolyLineSegment).Points)
					{
						_AddLine(startP, point2, transform);
						startP = point2;
					}
				}
				else if (item is PolyQuadraticBezierSegment)
				{
					PointCollection points2 = (item as PolyQuadraticBezierSegment).Points;
					startP = points2[points2.Count - 1];
				}
				else if (item is QuadraticBezierSegment)
				{
					startP = (item as QuadraticBezierSegment).Point2;
				}
			}
			if (figure.IsClosed)
			{
				_AddLine(startP, startPoint, transform);
			}
		}
	}

	private void _FillTables(List<FixedSOMTableRow> tableRows)
	{
		List<FixedSOMTable> list = new List<FixedSOMTable>();
		foreach (FixedSOMTableRow tableRow in tableRows)
		{
			FixedSOMTable fixedSOMTable = null;
			double num = 0.01;
			foreach (FixedSOMTable item in list)
			{
				if (Math.Abs(item.BoundingRect.Left - tableRow.BoundingRect.Left) < num && Math.Abs(item.BoundingRect.Right - tableRow.BoundingRect.Right) < num && Math.Abs(item.BoundingRect.Bottom - tableRow.BoundingRect.Top) < num)
				{
					fixedSOMTable = item;
					break;
				}
			}
			if (fixedSOMTable == null)
			{
				fixedSOMTable = new FixedSOMTable(_fixedSOMPage);
				list.Add(fixedSOMTable);
			}
			fixedSOMTable.AddRow(tableRow);
		}
		for (int i = 0; i < list.Count - 1; i++)
		{
			for (int j = i + 1; j < list.Count; j++)
			{
				if (list[i].BoundingRect.Contains(list[j].BoundingRect) && list[i].AddContainer(list[j]))
				{
					list.RemoveAt(j--);
				}
				else if (list[j].BoundingRect.Contains(list[i].BoundingRect) && list[j].AddContainer(list[i]))
				{
					list.RemoveAt(i--);
					if (i < 0)
					{
						break;
					}
				}
			}
		}
		foreach (FixedSOMTable item2 in list)
		{
			if (item2.IsSingleCelled)
			{
				continue;
			}
			bool flag = false;
			int num2 = 0;
			while (num2 < _fixedSOMPage.SemanticBoxes.Count)
			{
				if (_fixedSOMPage.SemanticBoxes[num2] is FixedSOMFixedBlock && item2.AddContainer(_fixedSOMPage.SemanticBoxes[num2] as FixedSOMContainer))
				{
					_fixedSOMPage.SemanticBoxes.RemoveAt(num2);
					flag = true;
				}
				else
				{
					num2++;
				}
			}
			if (!flag)
			{
				continue;
			}
			item2.DeleteEmptyRows();
			item2.DeleteEmptyColumns();
			foreach (FixedSOMTableRow semanticBox in item2.SemanticBoxes)
			{
				foreach (FixedSOMTableCell semanticBox2 in semanticBox.SemanticBoxes)
				{
					int num3 = 0;
					while (num3 < semanticBox2.SemanticBoxes.Count)
					{
						if (semanticBox2.SemanticBoxes[num3] is FixedSOMTable { IsEmpty: not false } fixedSOMTable2)
						{
							semanticBox2.SemanticBoxes.Remove(fixedSOMTable2);
						}
						else
						{
							num3++;
						}
					}
					_CreateGroups(semanticBox2);
					semanticBox2.SemanticBoxes.Sort();
				}
			}
			_fixedSOMPage.AddTable(item2);
		}
	}

	private void _CreateGroups(FixedSOMContainer container)
	{
		if (container.SemanticBoxes.Count <= 0)
		{
			return;
		}
		List<FixedSOMSemanticBox> list = new List<FixedSOMSemanticBox>();
		FixedSOMGroup fixedSOMGroup = new FixedSOMGroup(_fixedSOMPage);
		FixedSOMPageElement fixedSOMPageElement = container.SemanticBoxes[0] as FixedSOMPageElement;
		FixedSOMPageElement fixedSOMPageElement2 = null;
		fixedSOMGroup.AddContainer(fixedSOMPageElement);
		list.Add(fixedSOMGroup);
		for (int i = 1; i < container.SemanticBoxes.Count; i++)
		{
			fixedSOMPageElement2 = container.SemanticBoxes[i] as FixedSOMPageElement;
			if (!_IsSpatiallyCombinable(fixedSOMPageElement, fixedSOMPageElement2, 0.0, 30.0) || !(fixedSOMPageElement2.BoundingRect.Top >= fixedSOMPageElement.BoundingRect.Top))
			{
				fixedSOMGroup = new FixedSOMGroup(_fixedSOMPage);
				list.Add(fixedSOMGroup);
			}
			fixedSOMGroup.AddContainer(fixedSOMPageElement2);
			fixedSOMPageElement = fixedSOMPageElement2;
		}
		container.SemanticBoxes = list;
	}
}
