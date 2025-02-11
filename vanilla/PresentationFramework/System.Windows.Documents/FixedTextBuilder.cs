using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Media;
using System.Windows.Shapes;
using MS.Utility;

namespace System.Windows.Documents;

internal sealed class FixedTextBuilder
{
	internal sealed class FlowModelBuilder
	{
		private sealed class LogicalHyperlink
		{
			private UIElement _uiElement;

			private Uri _uri;

			private Geometry _geometry;

			private Rect _boundingRect;

			private bool _used;

			public Uri Uri => _uri;

			public Geometry Geometry => _geometry;

			public Rect BoundingRect => _boundingRect;

			public UIElement UIElement => _uiElement;

			public bool Used
			{
				get
				{
					return _used;
				}
				set
				{
					_used = value;
				}
			}

			public LogicalHyperlink(Uri uri, Geometry geom, UIElement uiElement)
			{
				_uiElement = uiElement;
				_uri = uri;
				_geometry = geom;
				_boundingRect = geom.Bounds;
				_used = false;
			}
		}

		private sealed class LogicalHyperlinkContainer : IEnumerable<LogicalHyperlink>, IEnumerable
		{
			private List<LogicalHyperlink> _hyperlinks;

			public LogicalHyperlinkContainer()
			{
				_hyperlinks = new List<LogicalHyperlink>();
			}

			IEnumerator<LogicalHyperlink> IEnumerable<LogicalHyperlink>.GetEnumerator()
			{
				return _hyperlinks.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _hyperlinks.GetEnumerator();
			}

			public void AddLogicalHyperlink(Uri uri, Geometry geometry, UIElement uiElement)
			{
				LogicalHyperlink item = new LogicalHyperlink(uri, geometry, uiElement);
				_hyperlinks.Add(item);
			}

			public Uri GetUri(FixedSOMElement element, FixedPage p, out UIElement shadowElement)
			{
				shadowElement = null;
				if (!(p.GetElement(element.FixedNode) is UIElement uIElement))
				{
					return null;
				}
				LogicalHyperlink logicalHyperlink = null;
				Uri uri = FixedPage.GetNavigateUri(uIElement);
				if (uri == null && _hyperlinks.Count > 0)
				{
					Transform t = uIElement.TransformToAncestor(p) as Transform;
					Geometry geom;
					if (uIElement is Glyphs)
					{
						GlyphRun glyphRun = ((Glyphs)uIElement).ToGlyphRun();
						Rect rect = glyphRun.ComputeAlignmentBox();
						rect.Offset(glyphRun.BaselineOrigin.X, glyphRun.BaselineOrigin.Y);
						geom = new RectangleGeometry(rect);
					}
					else if (uIElement is Path)
					{
						geom = ((Path)uIElement).Data;
					}
					else
					{
						Image image = (Image)uIElement;
						geom = new RectangleGeometry(new Rect(0.0, 0.0, image.Width, image.Height));
					}
					logicalHyperlink = _GetHyperlinkFromGeometry(geom, t);
					if (logicalHyperlink != null)
					{
						uri = logicalHyperlink.Uri;
						shadowElement = logicalHyperlink.UIElement;
					}
				}
				if (uri == null)
				{
					return null;
				}
				return FixedPage.GetLinkUri(p, uri);
			}

			public void MarkAsUsed(UIElement uiElement)
			{
				for (int i = 0; i < _hyperlinks.Count; i++)
				{
					LogicalHyperlink logicalHyperlink = _hyperlinks[i];
					if (logicalHyperlink.UIElement == uiElement)
					{
						logicalHyperlink.Used = true;
						break;
					}
				}
			}

			private LogicalHyperlink _GetHyperlinkFromGeometry(Geometry geom, Transform t)
			{
				Geometry geometry = geom;
				if (t != null && !t.Value.IsIdentity)
				{
					geometry = PathGeometry.CreateFromGeometry(geom);
					geometry.Transform = t;
				}
				double num = geometry.GetArea() * 0.99;
				Rect bounds = geometry.Bounds;
				for (int i = 0; i < _hyperlinks.Count; i++)
				{
					if (bounds.IntersectsWith(_hyperlinks[i].BoundingRect) && Geometry.Combine(geometry, _hyperlinks[i].Geometry, GeometryCombineMode.Intersect, Transform.Identity).GetArea() > num)
					{
						return _hyperlinks[i];
					}
				}
				return null;
			}
		}

		private int _pageIndex;

		private FixedTextContainer _container;

		private FixedTextBuilder _builder;

		private List<FixedSOMTextRun> _textRuns;

		private List<FlowNode> _flowNodes;

		private List<FixedNode> _fixedNodes;

		private List<FixedNode> _nodesInLine;

		private List<FixedLineResult> _lineResults;

		private Rect _lineLayoutBox;

		private Stack _endNodes;

		private Stack _fixedElements;

		private FixedElement _currentFixedElement;

		private FixedFlowMap _mapping;

		private FixedPageStructure _pageStructure;

		private Glyphs _lastGlyphs;

		private FixedSOMTextRun _currentRun;

		private LogicalHyperlinkContainer _logicalHyperlinkContainer;

		private FixedPage _fixedPage;

		private Uri _currentNavUri;

		public FixedElement FixedElement => _currentFixedElement;

		public FlowModelBuilder(FixedTextBuilder builder, FixedPageStructure pageStructure, FixedPage page)
		{
			_builder = builder;
			_container = builder._container;
			_pageIndex = pageStructure.PageIndex;
			_textRuns = new List<FixedSOMTextRun>();
			_flowNodes = new List<FlowNode>();
			_fixedNodes = new List<FixedNode>();
			_nodesInLine = new List<FixedNode>();
			_lineResults = new List<FixedLineResult>();
			_endNodes = new Stack();
			_fixedElements = new Stack();
			_mapping = builder._fixedFlowMap;
			_pageStructure = pageStructure;
			_currentFixedElement = _container.ContainerElement;
			_lineLayoutBox = Rect.Empty;
			_logicalHyperlinkContainer = new LogicalHyperlinkContainer();
			_fixedPage = page;
		}

		public void FindHyperlinkPaths(FrameworkElement elem)
		{
			foreach (UIElement child in LogicalTreeHelper.GetChildren(elem))
			{
				if (child is Canvas elem2)
				{
					FindHyperlinkPaths(elem2);
				}
				if (!(child is Path) || ((Path)child).Fill is ImageBrush)
				{
					continue;
				}
				Uri navigateUri = FixedPage.GetNavigateUri(child);
				if (navigateUri != null && ((Path)child).Data != null)
				{
					Transform transform = child.TransformToAncestor(_fixedPage) as Transform;
					Geometry geometry = ((Path)child).Data;
					if (transform != null && !transform.Value.IsIdentity)
					{
						geometry = PathGeometry.CreateFromGeometry(geometry);
						geometry.Transform = transform;
					}
					_logicalHyperlinkContainer.AddLogicalHyperlink(navigateUri, geometry, child);
				}
			}
		}

		public void AddLeftoverHyperlinks()
		{
			foreach (LogicalHyperlink item in (IEnumerable<LogicalHyperlink>)_logicalHyperlinkContainer)
			{
				if (!item.Used)
				{
					_AddStartNode(FixedElement.ElementType.Paragraph);
					_AddStartNode(FixedElement.ElementType.Hyperlink);
					_currentFixedElement.SetValue(Hyperlink.NavigateUriProperty, item.Uri);
					_currentFixedElement.SetValue(FixedElement.HelpTextProperty, (string)item.UIElement.GetValue(AutomationProperties.HelpTextProperty));
					_currentFixedElement.SetValue(FixedElement.NameProperty, (string)item.UIElement.GetValue(AutomationProperties.NameProperty));
					_AddEndNode();
					_AddEndNode();
				}
			}
		}

		public void AddStartNode(FixedElement.ElementType type)
		{
			_FinishTextRun(addSpace: true);
			_FinishHyperlink();
			_AddStartNode(type);
		}

		public void AddEndNode()
		{
			_FinishTextRun(addSpace: false);
			_FinishHyperlink();
			_AddEndNode();
		}

		public void AddElement(FixedSOMElement element)
		{
			FixedPage fixedPage = _builder.GetFixedPage(element.FixedNode);
			UIElement shadowElement;
			Uri uri = _logicalHyperlinkContainer.GetUri(element, fixedPage, out shadowElement);
			if (element is FixedSOMTextRun)
			{
				FixedSOMTextRun fixedSOMTextRun = element as FixedSOMTextRun;
				if (_currentRun == null || !fixedSOMTextRun.HasSameRichProperties(_currentRun) || uri != _currentNavUri || (uri != null && uri.ToString() != _currentNavUri.ToString()))
				{
					if (_currentRun != null)
					{
						_ = fixedSOMTextRun.FixedBlock;
						FixedSOMTextRun fixedSOMTextRun2 = _textRuns[_textRuns.Count - 1];
						Glyphs glyphsElement = _builder.GetGlyphsElement(fixedSOMTextRun2.FixedNode);
						Glyphs glyphsElement2 = _builder.GetGlyphsElement(fixedSOMTextRun.FixedNode);
						GlyphComparison comparison = _builder._CompareGlyphs(glyphsElement, glyphsElement2);
						bool addSpace = false;
						if (_builder._IsNonContiguous(fixedSOMTextRun2, fixedSOMTextRun, comparison))
						{
							addSpace = true;
						}
						_FinishTextRun(addSpace);
					}
					_SetHyperlink(uri, fixedSOMTextRun.FixedNode, shadowElement);
					_AddStartNode(FixedElement.ElementType.Run);
					fixedSOMTextRun.SetRTFProperties(_currentFixedElement);
					_currentRun = fixedSOMTextRun;
				}
				_textRuns.Add((FixedSOMTextRun)element);
				if (_fixedNodes.Count == 0 || _fixedNodes[_fixedNodes.Count - 1] != element.FixedNode)
				{
					_fixedNodes.Add(element.FixedNode);
				}
			}
			else if (element is FixedSOMImage)
			{
				FixedSOMImage fixedSOMImage = (FixedSOMImage)element;
				_FinishTextRun(addSpace: true);
				_SetHyperlink(uri, fixedSOMImage.FixedNode, shadowElement);
				_AddStartNode(FixedElement.ElementType.InlineUIContainer);
				FlowNode flowNode = new FlowNode(_NewScopeId(), FlowNodeType.Object, null);
				_container.OnNewFlowElement(_currentFixedElement, FixedElement.ElementType.Object, new FlowPosition(_container, flowNode, 0), new FlowPosition(_container, flowNode, 1), fixedSOMImage.Source, _pageIndex);
				_flowNodes.Add(flowNode);
				element.FlowNode = flowNode;
				flowNode.FixedSOMElements = new FixedSOMElement[1] { element };
				_mapping.AddFixedElement(element);
				_fixedNodes.Add(element.FixedNode);
				FixedElement obj = (FixedElement)flowNode.Cookie;
				obj.SetValue(FixedElement.NameProperty, fixedSOMImage.Name);
				obj.SetValue(FixedElement.HelpTextProperty, fixedSOMImage.HelpText);
				_AddEndNode();
			}
		}

		public void FinishMapping()
		{
			_FinishLine();
			_mapping.MappingReplace(_pageStructure.FlowStart, _flowNodes);
			_pageStructure.SetFlowBoundary(_flowNodes[0], _flowNodes[_flowNodes.Count - 1]);
			_pageStructure.SetupLineResults(_lineResults.ToArray());
		}

		private void _AddStartNode(FixedElement.ElementType type)
		{
			FlowNode flowNode = new FlowNode(_NewScopeId(), FlowNodeType.Start, _pageIndex);
			FlowNode flowNode2 = new FlowNode(_NewScopeId(), FlowNodeType.End, _pageIndex);
			_container.OnNewFlowElement(_currentFixedElement, type, new FlowPosition(_container, flowNode, 1), new FlowPosition(_container, flowNode2, 0), null, _pageIndex);
			_fixedElements.Push(_currentFixedElement);
			_currentFixedElement = (FixedElement)flowNode.Cookie;
			_flowNodes.Add(flowNode);
			_endNodes.Push(flowNode2);
		}

		private void _AddEndNode()
		{
			_flowNodes.Add((FlowNode)_endNodes.Pop());
			_currentFixedElement = (FixedElement)_fixedElements.Pop();
		}

		private void _FinishTextRun(bool addSpace)
		{
			if (_textRuns.Count > 0)
			{
				int num = 0;
				FixedSOMTextRun fixedSOMTextRun = null;
				for (int i = 0; i < _textRuns.Count; i++)
				{
					fixedSOMTextRun = _textRuns[i];
					Glyphs glyphsElement = _builder.GetGlyphsElement(fixedSOMTextRun.FixedNode);
					GlyphComparison glyphComparison = _builder._CompareGlyphs(_lastGlyphs, glyphsElement);
					if (glyphComparison == GlyphComparison.DifferentLine)
					{
						_FinishLine();
					}
					_lastGlyphs = glyphsElement;
					_lineLayoutBox.Union(fixedSOMTextRun.BoundingRect);
					fixedSOMTextRun.LineIndex = _lineResults.Count;
					if (_nodesInLine.Count == 0 || _nodesInLine[_nodesInLine.Count - 1] != fixedSOMTextRun.FixedNode)
					{
						_nodesInLine.Add(fixedSOMTextRun.FixedNode);
					}
					num += fixedSOMTextRun.EndIndex - fixedSOMTextRun.StartIndex;
					if (i > 0 && _builder._IsNonContiguous(_textRuns[i - 1], fixedSOMTextRun, glyphComparison))
					{
						_textRuns[i - 1].Text = _textRuns[i - 1].Text + " ";
						num++;
					}
				}
				if (addSpace && fixedSOMTextRun.Text.Length > 0 && !fixedSOMTextRun.Text.EndsWith(" ", StringComparison.Ordinal) && !IsHyphen(fixedSOMTextRun.Text[fixedSOMTextRun.Text.Length - 1]))
				{
					fixedSOMTextRun.Text += " ";
					num++;
				}
				if (num != 0)
				{
					FlowNode flowNode = new FlowNode(_NewScopeId(), FlowNodeType.Run, num);
					FixedSOMElement[] fixedSOMElements = _textRuns.ToArray();
					flowNode.FixedSOMElements = fixedSOMElements;
					int num2 = 0;
					foreach (FixedSOMTextRun textRun in _textRuns)
					{
						textRun.FlowNode = flowNode;
						textRun.OffsetInFlowNode = num2;
						_mapping.AddFixedElement(textRun);
						num2 += textRun.Text.Length;
					}
					_flowNodes.Add(flowNode);
					_textRuns.Clear();
				}
			}
			if (_currentRun != null)
			{
				_AddEndNode();
				_currentRun = null;
			}
		}

		private void _FinishHyperlink()
		{
			if (_currentNavUri != null)
			{
				_AddEndNode();
				_currentNavUri = null;
			}
		}

		private void _SetHyperlink(Uri navUri, FixedNode node, UIElement shadowHyperlink)
		{
			if (!(navUri != _currentNavUri) && (!(navUri != null) || !(navUri.ToString() != _currentNavUri.ToString())))
			{
				return;
			}
			if (_currentNavUri != null)
			{
				_AddEndNode();
			}
			if (navUri != null)
			{
				_AddStartNode(FixedElement.ElementType.Hyperlink);
				_currentFixedElement.SetValue(Hyperlink.NavigateUriProperty, navUri);
				if (_fixedPage.GetElement(node) is UIElement uIElement)
				{
					_currentFixedElement.SetValue(FixedElement.HelpTextProperty, (string)uIElement.GetValue(AutomationProperties.HelpTextProperty));
					_currentFixedElement.SetValue(FixedElement.NameProperty, (string)uIElement.GetValue(AutomationProperties.NameProperty));
					if (shadowHyperlink != null)
					{
						_logicalHyperlinkContainer.MarkAsUsed(shadowHyperlink);
					}
				}
			}
			_currentNavUri = navUri;
		}

		private void _FinishLine()
		{
			if (_nodesInLine.Count > 0)
			{
				FixedLineResult item = new FixedLineResult(_nodesInLine.ToArray(), _lineLayoutBox);
				_lineResults.Add(item);
				_nodesInLine.Clear();
				_lineLayoutBox = Rect.Empty;
			}
		}

		private int _NewScopeId()
		{
			return _builder._nextScopeId++;
		}
	}

	internal enum GlyphComparison
	{
		DifferentLine,
		SameLine,
		Adjacent,
		Unknown
	}

	internal const char FLOWORDER_SEPARATOR = ' ';

	internal static CultureInfo[] AdjacentLanguage = new CultureInfo[10]
	{
		new CultureInfo("zh-HANS"),
		new CultureInfo("zh-HANT"),
		new CultureInfo("zh-HK"),
		new CultureInfo("zh-MO"),
		new CultureInfo("zh-CN"),
		new CultureInfo("zh-SG"),
		new CultureInfo("zh-TW"),
		new CultureInfo("ja-JP"),
		new CultureInfo("ko-KR"),
		new CultureInfo("th-TH")
	};

	internal static char[] HyphenSet = new char[7] { '-', '‐', '‑', '‒', '–', '−', '\u00ad' };

	private readonly FixedTextContainer _container;

	private List<FixedPageStructure> _pageStructures;

	private int _nextScopeId;

	private FixedFlowMap _fixedFlowMap;

	private static bool[] _cTable = new bool[33]
	{
		true, false, true, true, false, true, true, false, true, true,
		true, true, true, true, true, true, false, false, true, true,
		true, true, true, true, false, false, true, true, false, true,
		true, true, true
	};

	internal FixedFlowMap FixedFlowMap => _fixedFlowMap;

	internal static bool AlwaysAdjacent(CultureInfo ci)
	{
		CultureInfo[] adjacentLanguage = AdjacentLanguage;
		foreach (CultureInfo obj in adjacentLanguage)
		{
			if (ci.Equals(obj))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsHyphen(char target)
	{
		char[] hyphenSet = HyphenSet;
		for (int i = 0; i < hyphenSet.Length; i++)
		{
			if (hyphenSet[i] == target)
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsSpace(char target)
	{
		return target == ' ';
	}

	internal FixedTextBuilder(FixedTextContainer container)
	{
		_container = container;
		_Init();
	}

	internal void AddVirtualPage()
	{
		FixedPageStructure fixedPageStructure = new FixedPageStructure(_pageStructures.Count);
		_pageStructures.Add(fixedPageStructure);
		_fixedFlowMap.FlowOrderInsertBefore(_fixedFlowMap.FlowEndEdge, fixedPageStructure.FlowStart);
	}

	internal bool EnsureTextOMForPage(int pageIndex)
	{
		FixedPageStructure fixedPageStructure = _pageStructures[pageIndex];
		if (!fixedPageStructure.Loaded)
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXEnsureOMBegin);
			try
			{
				FixedPage fixedPage = _container.FixedDocument.SyncGetPage(pageIndex, forceReload: false);
				if (fixedPage == null)
				{
					return false;
				}
				Size size = _container.FixedDocument.ComputePageSize(fixedPage);
				fixedPage.Measure(size);
				fixedPage.Arrange(new Rect(new Point(0.0, 0.0), size));
				bool flag = true;
				StoryFragments pageStructure = fixedPage.GetPageStructure();
				if (pageStructure != null)
				{
					flag = false;
					FixedDSBuilder fixedDSBuilder = new FixedDSBuilder(fixedPage, pageStructure);
					fixedPageStructure.FixedDSBuilder = fixedDSBuilder;
				}
				if (flag)
				{
					FixedSOMPageConstructor fixedSOMPageConstructor2 = (fixedPageStructure.PageConstructor = new FixedSOMPageConstructor(fixedPage, pageIndex));
					fixedPageStructure.FixedSOMPage = fixedSOMPageConstructor2.FixedSOMPage;
				}
				_CreateFixedMappingAndElementForPage(fixedPageStructure, fixedPage, flag);
			}
			finally
			{
				EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXEnsureOMEnd);
			}
			return true;
		}
		return false;
	}

	internal FixedPage GetFixedPage(FixedNode node)
	{
		return _container.FixedDocument.SyncGetPageWithCheck(node.Page);
	}

	internal Glyphs GetGlyphsElement(FixedNode node)
	{
		return GetFixedPage(node)?.GetGlyphsElement(node);
	}

	internal FixedNode[] GetNextLine(FixedNode currentNode, bool forward, ref int count)
	{
		if (_IsBoundaryPage(currentNode.Page))
		{
			return null;
		}
		EnsureTextOMForPage(currentNode.Page);
		FixedPageStructure fixedPageStructure = _pageStructures[currentNode.Page];
		if (_IsStartVisual(currentNode[1]))
		{
			FixedNode[] firstLine = fixedPageStructure.FirstLine;
			if (firstLine == null)
			{
				return null;
			}
			currentNode = firstLine[0];
			count--;
		}
		else if (_IsEndVisual(currentNode[1]))
		{
			FixedNode[] lastLine = fixedPageStructure.LastLine;
			if (lastLine == null)
			{
				return null;
			}
			currentNode = lastLine[0];
			count--;
		}
		if (!(_fixedFlowMap.MappingGetFixedSOMElement(currentNode, 0) is FixedSOMTextRun { LineIndex: var lineIndex }))
		{
			return null;
		}
		return fixedPageStructure.GetNextLine(lineIndex, forward, ref count);
	}

	internal FixedNode[] GetLine(int pageIndex, Point pt)
	{
		EnsureTextOMForPage(pageIndex);
		return _pageStructures[pageIndex].FindSnapToLine(pt);
	}

	internal FixedNode[] GetFirstLine(int pageIndex)
	{
		EnsureTextOMForPage(pageIndex);
		return _pageStructures[pageIndex].FirstLine;
	}

	internal FlowPosition CreateFlowPosition(FixedPosition fixedPosition)
	{
		EnsureTextOMForPage(fixedPosition.Page);
		FixedSOMElement fixedSOMElement = _fixedFlowMap.MappingGetFixedSOMElement(fixedPosition.Node, fixedPosition.Offset);
		if (fixedSOMElement != null)
		{
			FlowNode flowNode = fixedSOMElement.FlowNode;
			int num = fixedPosition.Offset;
			if (fixedSOMElement is FixedSOMTextRun { IsReversed: not false } fixedSOMTextRun)
			{
				num = fixedSOMTextRun.EndIndex - fixedSOMTextRun.StartIndex - num;
			}
			int offset = fixedSOMElement.OffsetInFlowNode + num - fixedSOMElement.StartIndex;
			return new FlowPosition(_container, flowNode, offset);
		}
		return null;
	}

	internal FlowPosition GetPageStartFlowPosition(int pageIndex)
	{
		EnsureTextOMForPage(pageIndex);
		FlowNode flowStart = _pageStructures[pageIndex].FlowStart;
		return new FlowPosition(_container, flowStart, 0);
	}

	internal FlowPosition GetPageEndFlowPosition(int pageIndex)
	{
		EnsureTextOMForPage(pageIndex);
		FlowNode flowEnd = _pageStructures[pageIndex].FlowEnd;
		return new FlowPosition(_container, flowEnd, 1);
	}

	internal bool GetFixedPosition(FlowPosition position, LogicalDirection textdir, out FixedPosition fixedp)
	{
		fixedp = new FixedPosition(FixedFlowMap.FixedStartEdge, 0);
		position.GetFlowNode(textdir, out var flowNode, out var offsetStart);
		FixedSOMElement[] fixedSOMElements = flowNode.FixedSOMElements;
		if (fixedSOMElements == null)
		{
			return false;
		}
		int num = 0;
		int num2 = fixedSOMElements.Length - 1;
		while (num2 > num)
		{
			int num3 = num2 + num + 1 >> 1;
			if (fixedSOMElements[num3].OffsetInFlowNode > offsetStart)
			{
				num2 = num3 - 1;
			}
			else
			{
				num = num3;
			}
		}
		FixedSOMElement fixedSOMElement = fixedSOMElements[num];
		if (num > 0 && textdir == LogicalDirection.Backward && fixedSOMElement.OffsetInFlowNode == offsetStart)
		{
			FixedSOMElement fixedSOMElement2 = fixedSOMElements[num - 1];
			int num4 = offsetStart - fixedSOMElement2.OffsetInFlowNode + fixedSOMElement2.StartIndex;
			if (num4 == fixedSOMElement2.EndIndex)
			{
				if (fixedSOMElement2 is FixedSOMTextRun { IsReversed: not false } fixedSOMTextRun)
				{
					num4 = fixedSOMTextRun.EndIndex - fixedSOMTextRun.StartIndex - num4;
				}
				fixedp = new FixedPosition(fixedSOMElement2.FixedNode, num4);
				return true;
			}
		}
		FixedSOMTextRun fixedSOMTextRun2 = fixedSOMElement as FixedSOMTextRun;
		int num5 = offsetStart - fixedSOMElement.OffsetInFlowNode + fixedSOMElement.StartIndex;
		if (fixedSOMTextRun2 != null && fixedSOMTextRun2.IsReversed)
		{
			num5 = fixedSOMTextRun2.EndIndex - fixedSOMTextRun2.StartIndex - num5;
		}
		fixedp = new FixedPosition(fixedSOMElement.FixedNode, num5);
		return true;
	}

	internal bool GetFixedNodesForFlowRange(FlowPosition pStart, FlowPosition pEnd, out FixedSOMElement[] somElements, out int firstElementStart, out int lastElementEnd)
	{
		somElements = null;
		firstElementStart = 0;
		lastElementEnd = 0;
		int num = 0;
		int num2 = -1;
		pStart.GetFlowNodes(pEnd, out var flowNodes, out var offsetStart, out var offsetEnd);
		if (flowNodes.Length == 0)
		{
			return false;
		}
		ArrayList arrayList = new ArrayList();
		FlowNode flowNode = flowNodes[0];
		FlowNode flowNode2 = flowNodes[^1];
		FlowNode[] array = flowNodes;
		foreach (FlowNode flowNode3 in array)
		{
			int num3 = 0;
			int num4 = int.MaxValue;
			if (flowNode3 == flowNode)
			{
				num3 = offsetStart;
			}
			if (flowNode3 == flowNode2)
			{
				num4 = offsetEnd;
			}
			if (flowNode3.Type == FlowNodeType.Object)
			{
				FixedSOMElement[] fixedSOMElements = flowNode3.FixedSOMElements;
				arrayList.Add(fixedSOMElements[0]);
			}
			if (flowNode3.Type != FlowNodeType.Run)
			{
				continue;
			}
			FixedSOMElement[] fixedSOMElements2 = flowNode3.FixedSOMElements;
			foreach (FixedSOMElement fixedSOMElement in fixedSOMElements2)
			{
				int offsetInFlowNode = fixedSOMElement.OffsetInFlowNode;
				if (offsetInFlowNode >= num4)
				{
					break;
				}
				int num5 = offsetInFlowNode + fixedSOMElement.EndIndex - fixedSOMElement.StartIndex;
				if (num5 > num3)
				{
					arrayList.Add(fixedSOMElement);
					if (num3 >= offsetInFlowNode && flowNode3 == flowNode)
					{
						num = fixedSOMElement.StartIndex + num3 - offsetInFlowNode;
					}
					if (num4 <= num5 && flowNode3 == flowNode2)
					{
						num2 = fixedSOMElement.StartIndex + num4 - offsetInFlowNode;
						break;
					}
					if (num4 == num5 + 1)
					{
						num2 = fixedSOMElement.EndIndex;
					}
				}
			}
		}
		somElements = (FixedSOMElement[])arrayList.ToArray(typeof(FixedSOMElement));
		if (somElements.Length == 0)
		{
			return false;
		}
		if (flowNode.Type == FlowNodeType.Object)
		{
			firstElementStart = offsetStart;
		}
		else
		{
			firstElementStart = num;
		}
		if (flowNode2.Type == FlowNodeType.Object)
		{
			lastElementEnd = offsetEnd;
		}
		else
		{
			lastElementEnd = num2;
		}
		return true;
	}

	internal string GetFlowText(FlowNode flowNode)
	{
		StringBuilder stringBuilder = new StringBuilder();
		FixedSOMElement[] fixedSOMElements = flowNode.FixedSOMElements;
		for (int i = 0; i < fixedSOMElements.Length; i++)
		{
			FixedSOMTextRun fixedSOMTextRun = (FixedSOMTextRun)fixedSOMElements[i];
			stringBuilder.Append(fixedSOMTextRun.Text);
		}
		return stringBuilder.ToString();
	}

	internal static bool MostlyRTL(string s)
	{
		int num = 0;
		int num2 = 0;
		foreach (char c in s)
		{
			if (_IsRTL(c))
			{
				num++;
			}
			else if (c != ' ')
			{
				num2++;
			}
		}
		if (num > 0)
		{
			if (num2 != 0)
			{
				return num / num2 >= 2;
			}
			return true;
		}
		return false;
	}

	internal static bool IsSameLine(double verticalDistance, double fontSize1, double fontSize2)
	{
		double num = ((fontSize1 < fontSize2) ? fontSize1 : fontSize2);
		return ((verticalDistance > 0.0) ? (fontSize1 - verticalDistance) : (fontSize2 + verticalDistance)) / num > 0.5;
	}

	internal static bool IsNonContiguous(CultureInfo ciPrev, CultureInfo ciCurrent, bool isSidewaysPrev, bool isSidewaysCurrent, string strPrev, string strCurrent, GlyphComparison comparison)
	{
		if (ciPrev != ciCurrent)
		{
			return true;
		}
		if (AlwaysAdjacent(ciPrev))
		{
			return false;
		}
		if (isSidewaysPrev != isSidewaysCurrent)
		{
			return true;
		}
		if (strPrev.Length == 0 || strCurrent.Length == 0)
		{
			return false;
		}
		if (!isSidewaysPrev)
		{
			int length = strPrev.Length;
			char target = strPrev[length - 1];
			if (IsSpace(target))
			{
				return false;
			}
			if (comparison != 0 && comparison != GlyphComparison.Unknown)
			{
				return comparison != GlyphComparison.Adjacent;
			}
			if (!IsHyphen(target))
			{
				return true;
			}
		}
		return false;
	}

	private void _Init()
	{
		_nextScopeId = 0;
		_fixedFlowMap = new FixedFlowMap();
		_pageStructures = new List<FixedPageStructure>();
	}

	private FixedNode _NewFixedNode(int pageIndex, int nestingLevel, int level1Index, int[] pathPrefix, int childIndex)
	{
		switch (nestingLevel)
		{
		case 1:
			return FixedNode.Create(pageIndex, nestingLevel, childIndex, -1, null);
		case 2:
			return FixedNode.Create(pageIndex, nestingLevel, level1Index, childIndex, null);
		default:
		{
			int[] array = new int[pathPrefix.Length + 1];
			pathPrefix.CopyTo(array, 0);
			array[^1] = childIndex;
			return FixedNode.Create(pageIndex, nestingLevel, -1, -1, array);
		}
		}
	}

	private bool _IsImage(object o)
	{
		if (o is Path path)
		{
			if (path.Fill is ImageBrush)
			{
				return path.Data != null;
			}
			return false;
		}
		return o is Image;
	}

	private bool _IsNonContiguous(FixedSOMTextRun prevRun, FixedSOMTextRun currentRun, GlyphComparison comparison)
	{
		if (prevRun.FixedNode == currentRun.FixedNode)
		{
			return currentRun.StartIndex != prevRun.EndIndex;
		}
		return IsNonContiguous(prevRun.CultureInfo, currentRun.CultureInfo, prevRun.IsSideways, currentRun.IsSideways, prevRun.Text, currentRun.Text, comparison);
	}

	private GlyphComparison _CompareGlyphs(Glyphs glyph1, Glyphs glyph2)
	{
		GlyphComparison result = GlyphComparison.DifferentLine;
		if (glyph1 == glyph2)
		{
			result = GlyphComparison.SameLine;
		}
		else if (glyph1 != null && glyph2 != null)
		{
			GlyphRun glyphRun = glyph1.ToGlyphRun();
			GlyphRun glyphRun2 = glyph2.ToGlyphRun();
			if (glyphRun != null && glyphRun2 != null)
			{
				Rect rect = glyphRun.ComputeAlignmentBox();
				rect.Offset(glyph1.OriginX, glyph1.OriginY);
				Rect rect2 = glyphRun2.ComputeAlignmentBox();
				rect2.Offset(glyph2.OriginX, glyph2.OriginY);
				bool flag = (glyph1.BidiLevel & 1) == 0;
				bool flag2 = (glyph2.BidiLevel & 1) == 0;
				GeneralTransform generalTransform = glyph2.TransformToVisual(glyph1);
				Point point = (flag ? rect.TopRight : rect.TopLeft);
				Point result2 = (flag2 ? rect2.TopLeft : rect2.TopRight);
				generalTransform?.TryTransform(result2, out result2);
				if (IsSameLine(result2.Y - point.Y, rect.Height, rect2.Height))
				{
					result = GlyphComparison.SameLine;
					if (flag == flag2)
					{
						double num = Math.Abs(result2.X - point.X);
						double num2 = Math.Max(rect.Height, rect2.Height);
						if (num / num2 < 0.05)
						{
							result = GlyphComparison.Adjacent;
						}
					}
				}
			}
		}
		return result;
	}

	private void _CreateFixedMappingAndElementForPage(FixedPageStructure pageStructure, FixedPage page, bool constructSOM)
	{
		List<FixedNode> list = new List<FixedNode>();
		_GetFixedNodes(pageStructure, page.Children, 1, -1, null, constructSOM, list, Matrix.Identity);
		FlowModelBuilder flowModelBuilder = new FlowModelBuilder(this, pageStructure, page);
		flowModelBuilder.FindHyperlinkPaths(page);
		if (constructSOM)
		{
			pageStructure.FixedSOMPage.MarkupOrder = list;
			pageStructure.ConstructFixedSOMPage(list);
			_CreateFlowNodes(pageStructure.FixedSOMPage, flowModelBuilder);
			pageStructure.PageConstructor = null;
		}
		else
		{
			pageStructure.FixedDSBuilder.ConstructFlowNodes(flowModelBuilder, list);
		}
		flowModelBuilder.FinishMapping();
	}

	private void _GetFixedNodes(FixedPageStructure pageStructure, IEnumerable oneLevel, int nestingLevel, int level1Index, int[] pathPrefix, bool constructLines, List<FixedNode> fixedNodes, Matrix transform)
	{
		int pageIndex = pageStructure.PageIndex;
		_NewScopeId();
		int num = 0;
		IEnumerator enumerator = oneLevel.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (!constructLines && enumerator.Current is IFrameworkInputElement { Name: not null } frameworkInputElement && frameworkInputElement.Name.Length != 0)
			{
				pageStructure.FixedDSBuilder.BuildNameHashTable(frameworkInputElement.Name, enumerator.Current as UIElement, fixedNodes.Count);
			}
			if (_IsImage(enumerator.Current) || (enumerator.Current is Glyphs && (enumerator.Current as Glyphs).MeasurementGlyphRun != null))
			{
				fixedNodes.Add(_NewFixedNode(pageIndex, nestingLevel, level1Index, pathPrefix, num));
			}
			else if (constructLines && enumerator.Current is Path)
			{
				pageStructure.PageConstructor.ProcessPath(enumerator.Current as Path, transform);
			}
			else if (enumerator.Current is Canvas)
			{
				Transform identity = Transform.Identity;
				Canvas obj = enumerator.Current as Canvas;
				IEnumerable children = obj.Children;
				identity = obj.RenderTransform;
				if (identity == null)
				{
					identity = Transform.Identity;
				}
				if (children != null)
				{
					int[] array = null;
					if (nestingLevel >= 2)
					{
						if (nestingLevel == 2)
						{
							array = new int[2] { level1Index, 0 };
						}
						else
						{
							array = new int[pathPrefix.Length + 1];
							pathPrefix.CopyTo(array, 0);
						}
						array[^1] = num;
					}
					_GetFixedNodes(pageStructure, children, nestingLevel + 1, (nestingLevel == 1) ? num : (-1), array, constructLines, fixedNodes, identity.Value * transform);
				}
			}
			num++;
		}
	}

	private void _CreateFlowNodes(FixedSOMPage somPage, FlowModelBuilder flowBuilder)
	{
		flowBuilder.AddStartNode(FixedElement.ElementType.Section);
		somPage.SetRTFProperties(flowBuilder.FixedElement);
		foreach (FixedSOMContainer semanticBox in somPage.SemanticBoxes)
		{
			_CreateFlowNodes(semanticBox, flowBuilder);
		}
		flowBuilder.AddLeftoverHyperlinks();
		flowBuilder.AddEndNode();
	}

	private void _CreateFlowNodes(FixedSOMContainer node, FlowModelBuilder flowBuilder)
	{
		FixedElement.ElementType[] elementTypes = node.ElementTypes;
		FixedElement.ElementType[] array = elementTypes;
		foreach (FixedElement.ElementType type in array)
		{
			flowBuilder.AddStartNode(type);
			node.SetRTFProperties(flowBuilder.FixedElement);
		}
		foreach (FixedSOMSemanticBox semanticBox in node.SemanticBoxes)
		{
			if (semanticBox is FixedSOMElement)
			{
				flowBuilder.AddElement((FixedSOMElement)semanticBox);
			}
			else if (semanticBox is FixedSOMContainer)
			{
				_CreateFlowNodes((FixedSOMContainer)semanticBox, flowBuilder);
			}
		}
		array = elementTypes;
		for (int i = 0; i < array.Length; i++)
		{
			_ = array[i];
			flowBuilder.AddEndNode();
		}
	}

	private bool _IsStartVisual(int visualIndex)
	{
		return visualIndex == int.MinValue;
	}

	private bool _IsEndVisual(int visualIndex)
	{
		return visualIndex == int.MaxValue;
	}

	private bool _IsBoundaryPage(int pageIndex)
	{
		if (pageIndex != int.MinValue)
		{
			return pageIndex == int.MaxValue;
		}
		return true;
	}

	private int _NewScopeId()
	{
		return _nextScopeId++;
	}

	private static bool _IsRTL(char c)
	{
		if (c < 'א' || c > '؋')
		{
			switch (c)
			{
			default:
				if (c >= '٭' && c <= 'ە' && c != '\u0670')
				{
					break;
				}
				switch (c)
				{
				default:
					switch (c)
					{
					default:
						if ((c < 'ݍ' || c > 'ޥ') && c != 'ޱ' && c != 'יִ' && (c < 'ײַ' || c > 'ﴽ' || c == '﬩') && (c < 'ﵐ' || c > '﷼'))
						{
							if (c >= 'ﹰ')
							{
								return c <= 'ﻼ';
							}
							return false;
						}
						break;
					case 'ܐ':
					case 'ܒ':
					case 'ܓ':
					case 'ܔ':
					case 'ܕ':
					case 'ܖ':
					case 'ܗ':
					case 'ܘ':
					case 'ܙ':
					case 'ܚ':
					case 'ܛ':
					case 'ܜ':
					case 'ܝ':
					case 'ܞ':
					case 'ܟ':
					case 'ܠ':
					case 'ܡ':
					case 'ܢ':
					case 'ܣ':
					case 'ܤ':
					case 'ܥ':
					case 'ܦ':
					case 'ܧ':
					case 'ܨ':
					case 'ܩ':
					case 'ܪ':
					case 'ܫ':
					case 'ܬ':
					case 'ܭ':
					case 'ܮ':
					case 'ܯ':
						break;
					}
					break;
				case '\u06dd':
				case 'ۥ':
				case 'ۦ':
				case 'ۮ':
				case 'ۯ':
				case 'ۺ':
				case 'ۻ':
				case 'ۼ':
				case '۽':
				case '۾':
				case 'ۿ':
				case '܀':
				case '܁':
				case '܂':
				case '܃':
				case '܄':
				case '܅':
				case '܆':
				case '܇':
				case '܈':
				case '܉':
				case '܊':
				case '܋':
				case '܌':
				case '܍':
					break;
				}
				break;
			case '؍':
			case '؛':
			case '\u061c':
			case '؝':
			case '؞':
			case '؟':
			case 'ؠ':
			case 'ء':
			case 'آ':
			case 'أ':
			case 'ؤ':
			case 'إ':
			case 'ئ':
			case 'ا':
			case 'ب':
			case 'ة':
			case 'ت':
			case 'ث':
			case 'ج':
			case 'ح':
			case 'خ':
			case 'د':
			case 'ذ':
			case 'ر':
			case 'ز':
			case 'س':
			case 'ش':
			case 'ص':
			case 'ض':
			case 'ط':
			case 'ظ':
			case 'ع':
			case 'غ':
			case 'ػ':
			case 'ؼ':
			case 'ؽ':
			case 'ؾ':
			case 'ؿ':
			case 'ـ':
			case 'ف':
			case 'ق':
			case 'ك':
			case 'ل':
			case 'م':
			case 'ن':
			case 'ه':
			case 'و':
			case 'ى':
			case 'ي':
				break;
			}
		}
		return true;
	}
}
