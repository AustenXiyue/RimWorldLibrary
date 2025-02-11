using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.PresentationCore;

namespace MS.Internal.TextFormatting;

internal struct TextMetrics : ITextMetrics
{
	internal class FullTextLine : TextLine
	{
		[Flags]
		private enum StatusFlags
		{
			None = 0,
			IsDisposed = 1,
			HasOverflowed = 2,
			BoundingBoxComputed = 4,
			RightToLeft = 8,
			HasCollapsed = 0x10,
			KeepState = 0x20,
			IsTruncated = 0x40,
			IsJustified = 0x80
		}

		private enum CaretDirection
		{
			Forward,
			Backward,
			Backspace
		}

		private struct Overhang
		{
			internal double Leading;

			internal double Trailing;

			internal double Extent;

			internal double Before;
		}

		private TextMetrics _metrics;

		private int _cpFirst;

		private int _depthQueryMax;

		private int _paragraphWidth;

		private int _textMinWidthAtTrailing;

		private MS.Internal.SecurityCriticalDataForSet<nint> _ploline;

		private MS.Internal.SecurityCriticalDataForSet<nint> _ploc;

		private Overhang _overhang;

		private StatusFlags _statusFlags;

		private SpanVector _plsrunVector;

		private ArrayList _lsrunsMainText;

		private ArrayList _lsrunsMarkerText;

		private FullTextState _fullText;

		private FormattedTextSymbols _collapsingSymbol;

		private TextCollapsedRange _collapsedRange;

		private TextSource _textSource;

		private TextDecorationCollection _paragraphTextDecorations;

		private Brush _defaultTextDecorationsBrush;

		private TextFormattingMode _textFormattingMode;

		public override int TrailingWhitespaceLength
		{
			get
			{
				if (_metrics._textWidth == _metrics._textWidthAtTrailing)
				{
					return _metrics._cchNewline;
				}
				CharacterHit characterHit = CharacterHitFromDistance(_metrics._textWidthAtTrailing + _metrics._textStart);
				return _cpFirst + _metrics._cchLength - characterHit.FirstCharacterIndex - characterHit.TrailingLength;
			}
		}

		public override int Length => _metrics.Length;

		public override int DependentLength => _metrics.DependentLength;

		public override int NewlineLength => _metrics.NewlineLength;

		public override double Start => _metrics.Start;

		public override double Width => _metrics.Width;

		public override double WidthIncludingTrailingWhitespace => _metrics.WidthIncludingTrailingWhitespace;

		public override double Height => _metrics.Height;

		public override double TextHeight => _metrics.TextHeight;

		public override double Baseline => _metrics.Baseline;

		public override double TextBaseline => _metrics.TextBaseline;

		public override double MarkerBaseline => _metrics.MarkerBaseline;

		public override double MarkerHeight => _metrics.MarkerHeight;

		public override double Extent
		{
			get
			{
				CheckBoundingBox();
				return _overhang.Extent;
			}
		}

		public override double OverhangLeading
		{
			get
			{
				CheckBoundingBox();
				return _overhang.Leading;
			}
		}

		public override double OverhangTrailing
		{
			get
			{
				CheckBoundingBox();
				return _overhang.Trailing;
			}
		}

		public override double OverhangAfter
		{
			get
			{
				CheckBoundingBox();
				return _overhang.Extent - Height - _overhang.Before;
			}
		}

		public override bool HasOverflowed => (_statusFlags & StatusFlags.HasOverflowed) != 0;

		public override bool HasCollapsed => (_statusFlags & StatusFlags.HasCollapsed) != 0;

		public override bool IsTruncated => (_statusFlags & StatusFlags.IsTruncated) != 0;

		public int CpFirst => _cpFirst;

		public TextSource TextSource => _textSource;

		internal int BaselineOffset => _metrics._baselineOffset;

		internal int ParagraphWidth => _paragraphWidth;

		internal double MinWidth => _metrics._formatter.IdealToReal(_textMinWidthAtTrailing + _metrics._textStart, base.PixelsPerDip);

		internal bool RightToLeft => (_statusFlags & StatusFlags.RightToLeft) != 0;

		internal TextFormatterImp Formatter => _metrics._formatter;

		internal bool IsJustified => (_statusFlags & StatusFlags.IsJustified) != 0;

		internal TextDecorationCollection TextDecorations => _paragraphTextDecorations;

		internal Brush DefaultTextDecorationsBrush => _defaultTextDecorationsBrush;

		internal FullTextLine(FormatSettings settings, int cpFirst, int lineLength, int paragraphWidth, LineFlags lineFlags)
			: this(settings.TextFormattingMode, settings.Pap.Justify, settings.TextSource.PixelsPerDip)
		{
			if ((lineFlags & LineFlags.KeepState) != 0 || settings.Pap.AlwaysCollapsible)
			{
				_statusFlags |= StatusFlags.KeepState;
			}
			int finiteFormatWidth = settings.GetFiniteFormatWidth(paragraphWidth);
			FullTextState fullTextState = FullTextState.Create(settings, cpFirst, finiteFormatWidth);
			FormatLine(fullTextState, cpFirst, lineLength, fullTextState.FormatWidth, finiteFormatWidth, paragraphWidth, lineFlags, null);
		}

		~FullTextLine()
		{
			DisposeInternal(finalizing: true);
		}

		public override void Dispose()
		{
			DisposeInternal(finalizing: false);
			GC.SuppressFinalize(this);
		}

		private void DisposeInternal(bool finalizing)
		{
			if (_ploline.Value != IntPtr.Zero)
			{
				UnsafeNativeMethods.LoDisposeLine(_ploline.Value, finalizing);
				_ploline.Value = IntPtr.Zero;
				GC.KeepAlive(this);
			}
		}

		private FullTextLine(TextFormattingMode textFormattingMode, bool justify, double pixelsPerDip)
			: base(pixelsPerDip)
		{
			_textFormattingMode = textFormattingMode;
			if (justify)
			{
				_statusFlags |= StatusFlags.IsJustified;
			}
			_metrics = default(TextMetrics);
			_metrics._pixelsPerDip = pixelsPerDip;
			_ploline = new MS.Internal.SecurityCriticalDataForSet<nint>(IntPtr.Zero);
		}

		private unsafe void FormatLine(FullTextState fullText, int cpFirst, int lineLength, int formatWidth, int finiteFormatWidth, int paragraphWidth, LineFlags lineFlags, FormattedTextSymbols collapsingSymbol)
		{
			_metrics._formatter = fullText.Formatter;
			TextStore textStore = fullText.TextStore;
			TextStore textMarkerStore = fullText.TextMarkerStore;
			FormatSettings settings = textStore.Settings;
			ParaProp pap = settings.Pap;
			_paragraphTextDecorations = pap.TextDecorations;
			if (_paragraphTextDecorations != null)
			{
				if (_paragraphTextDecorations.Count != 0)
				{
					_defaultTextDecorationsBrush = pap.DefaultTextDecorationsBrush;
				}
				else
				{
					_paragraphTextDecorations = null;
				}
			}
			TextFormatterContext textFormatterContext = _metrics._formatter.AcquireContext(fullText, IntPtr.Zero);
			LsLInfo plslineInfo = default(LsLInfo);
			LsLineWidths lineWidths = default(LsLineWidths);
			fullText.SetTabs(textFormatterContext);
			int lineLength2 = 0;
			if (lineLength > 0)
			{
				lineLength2 = PrefetchLSRuns(textStore, cpFirst, lineLength);
			}
			nint ploline;
			LsErr lsErr = textFormatterContext.CreateLine(cpFirst, lineLength2, formatWidth, lineFlags, IntPtr.Zero, out ploline, out plslineInfo, out _depthQueryMax, out lineWidths);
			if (lsErr == LsErr.TooLongParagraph)
			{
				int num = fullText.CpMeasured;
				int num2 = 1;
				while (true)
				{
					if (num < 1)
					{
						num = 1;
					}
					textStore.InsertFakeLineBreak(num);
					lsErr = textFormatterContext.CreateLine(cpFirst, lineLength2, formatWidth, lineFlags, IntPtr.Zero, out ploline, out plslineInfo, out _depthQueryMax, out lineWidths);
					if (lsErr != LsErr.TooLongParagraph || num == 1)
					{
						break;
					}
					num = fullText.CpMeasured - num2;
					num2 *= 2;
				}
			}
			_ploline.Value = ploline;
			Exception callbackException = textFormatterContext.CallbackException;
			textFormatterContext.Release();
			if (lsErr != 0)
			{
				GC.SuppressFinalize(this);
				if (callbackException != null)
				{
					throw WrapException(callbackException);
				}
				TextFormatterContext.ThrowExceptionFromLsError(SR.Format(SR.CreateLineFailure, lsErr), lsErr);
			}
			GC.KeepAlive(textFormatterContext);
			_metrics.Compute(fullText, cpFirst, paragraphWidth, collapsingSymbol, ref lineWidths, &plslineInfo);
			_textMinWidthAtTrailing = lineWidths.upMinStartTrailing - _metrics._textStart;
			if (collapsingSymbol != null)
			{
				_collapsingSymbol = collapsingSymbol;
				_textMinWidthAtTrailing += TextFormatterImp.RealToIdeal(collapsingSymbol.Width);
			}
			else if (_metrics._textStart + _metrics._textWidthAtTrailing > finiteFormatWidth)
			{
				bool flag = true;
				if (_textFormattingMode == TextFormattingMode.Display)
				{
					double width = Width;
					double y = _metrics._formatter.IdealToReal(finiteFormatWidth, base.PixelsPerDip);
					flag = TextFormatterImp.CompareReal(width, y, base.PixelsPerDip, _textFormattingMode) > 0;
				}
				if (flag)
				{
					_statusFlags |= StatusFlags.HasOverflowed;
					_fullText = fullText;
				}
			}
			if (fullText != null && (fullText.KeepState || (_statusFlags & StatusFlags.KeepState) != 0))
			{
				_fullText = fullText;
			}
			_ploc = textFormatterContext.Ploc;
			_cpFirst = cpFirst;
			_paragraphWidth = paragraphWidth;
			if (pap.RightToLeft)
			{
				_statusFlags |= StatusFlags.RightToLeft;
			}
			if (plslineInfo.fForcedBreak != 0)
			{
				_statusFlags |= StatusFlags.IsTruncated;
			}
			_plsrunVector = textStore.PlsrunVector;
			_lsrunsMainText = textStore.LsrunList;
			if (textMarkerStore != null)
			{
				_lsrunsMarkerText = textMarkerStore.LsrunList;
			}
			_textSource = settings.TextSource;
		}

		private static Exception WrapException(Exception caughtException)
		{
			Type type = caughtException.GetType();
			if (type.IsPublic)
			{
				ConstructorInfo constructor = type.GetConstructor(new Type[1] { typeof(Exception) });
				if (constructor != null)
				{
					return (Exception)constructor.Invoke(new object[1] { caughtException });
				}
				constructor = type.GetConstructor(new Type[2]
				{
					typeof(string),
					typeof(Exception)
				});
				if (constructor != null)
				{
					return (Exception)constructor.Invoke(new object[2] { caughtException.Message, caughtException });
				}
			}
			return caughtException;
		}

		private void AppendCollapsingSymbol(FormattedTextSymbols symbol)
		{
			_collapsingSymbol = symbol;
			int num = TextFormatterImp.RealToIdeal(symbol.Width);
			_metrics.AppendCollapsingSymbolWidth(num);
			_textMinWidthAtTrailing += num;
		}

		private int PrefetchLSRuns(TextStore store, int cpFirst, int lineLength)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			LSRun lSRun;
			do
			{
				lSRun = store.FetchLSRun(cpFirst + num2, _textFormattingMode, isSideways: false, out var _, out var _, out var lsrunLength);
				if (lineLength == num && lSRun.Type == Plsrun.Reverse)
				{
					break;
				}
				num3 = lsrunLength;
				num4 = lSRun.Length;
				num2 += num3;
				num += num4;
			}
			while (!TextStore.IsNewline(lSRun.Type) && lineLength >= num);
			if (num == lineLength || num3 == num4)
			{
				return num2 - num + lineLength;
			}
			Invariant.Assert(num - lineLength == num4);
			return num2 - num3;
		}

		public override void Draw(DrawingContext drawingContext, Point origin, InvertAxes inversion)
		{
			if (drawingContext == null)
			{
				throw new ArgumentNullException("drawingContext");
			}
			if ((_statusFlags & StatusFlags.IsDisposed) != 0)
			{
				throw new ObjectDisposedException(SR.TextLineHasBeenDisposed);
			}
			MatrixTransform matrixTransform = TextFormatterImp.CreateAntiInversionTransform(inversion, _metrics._formatter.IdealToReal(_paragraphWidth, base.PixelsPerDip), _metrics._formatter.IdealToReal(_metrics._height, base.PixelsPerDip));
			if (matrixTransform == null)
			{
				DrawTextLine(drawingContext, origin, null);
				return;
			}
			drawingContext.PushTransform(matrixTransform);
			try
			{
				DrawTextLine(drawingContext, origin, matrixTransform);
			}
			finally
			{
				drawingContext.Pop();
			}
		}

		private void DrawTextLine(DrawingContext drawingContext, Point origin, MatrixTransform antiInversion)
		{
			Rect boundingBox = Rect.Empty;
			if (_ploline.Value != IntPtr.Zero)
			{
				LsErr lsErr = LsErr.None;
				LSRECT clipRect = new LSRECT(0, 0, _metrics._textWidthAtTrailing, _metrics._height);
				TextFormatterContext textFormatterContext;
				using (DrawingState owner = new DrawingState(drawingContext, origin, antiInversion, this))
				{
					textFormatterContext = _metrics._formatter.AcquireContext(owner, _ploc.Value);
					textFormatterContext.EmptyBoundingBox();
					LSPOINT pt = new LSPOINT(0, _metrics._baselineOffset);
					lsErr = UnsafeNativeMethods.LoDisplayLine(_ploline.Value, ref pt, 1u, ref clipRect);
				}
				boundingBox = textFormatterContext.BoundingBox;
				Exception callbackException = textFormatterContext.CallbackException;
				textFormatterContext.Release();
				if (lsErr != 0)
				{
					if (callbackException != null)
					{
						throw callbackException;
					}
					TextFormatterContext.ThrowExceptionFromLsError(SR.Format(SR.CreateLineFailure, lsErr), lsErr);
				}
				GC.KeepAlive(textFormatterContext);
			}
			if (_collapsingSymbol != null)
			{
				Point vectorToLineOrigin = default(Point);
				if (antiInversion != null)
				{
					vectorToLineOrigin = origin;
					double x = (origin.Y = 0.0);
					origin.X = x;
				}
				boundingBox.Union(DrawCollapsingSymbol(drawingContext, origin, vectorToLineOrigin));
			}
			BuildOverhang(origin, boundingBox);
			_statusFlags |= StatusFlags.BoundingBoxComputed;
		}

		private Rect DrawCollapsingSymbol(DrawingContext drawingContext, Point lineOrigin, Point vectorToLineOrigin)
		{
			int num = TextFormatterImp.RealToIdeal(_collapsingSymbol.Width);
			Point currentOrigin = LSRun.UVToXY(lineOrigin, vectorToLineOrigin, LSLineUToParagraphU(_metrics._textStart + _metrics._textWidthAtTrailing - num), _metrics._baselineOffset, this);
			return _collapsingSymbol.Draw(drawingContext, currentOrigin);
		}

		private void CheckBoundingBox()
		{
			if ((_statusFlags & StatusFlags.BoundingBoxComputed) == 0)
			{
				DrawTextLine(null, new Point(0.0, 0.0), null);
			}
		}

		public override TextLine Collapse(params TextCollapsingProperties[] collapsingPropertiesList)
		{
			if ((_statusFlags & StatusFlags.IsDisposed) != 0)
			{
				throw new ObjectDisposedException(SR.TextLineHasBeenDisposed);
			}
			if (!HasOverflowed && (_statusFlags & StatusFlags.KeepState) == 0)
			{
				return this;
			}
			if (collapsingPropertiesList == null || collapsingPropertiesList.Length == 0)
			{
				throw new ArgumentNullException("collapsingPropertiesList");
			}
			TextCollapsingProperties textCollapsingProperties = collapsingPropertiesList[0];
			double num = textCollapsingProperties.Width;
			if (TextFormatterImp.CompareReal(num, Width, base.PixelsPerDip, _textFormattingMode) > 0)
			{
				return this;
			}
			FormattedTextSymbols formattedTextSymbols = null;
			if (textCollapsingProperties.Symbol != null)
			{
				formattedTextSymbols = new FormattedTextSymbols(_metrics._formatter.GlyphingCache, textCollapsingProperties.Symbol, RightToLeft, TextFormatterImp.ToIdeal, (float)base.PixelsPerDip, _textFormattingMode, isSideways: false);
				num -= formattedTextSymbols.Width;
			}
			FullTextLine fullTextLine = new FullTextLine(_textFormattingMode, IsJustified, base.PixelsPerDip);
			fullTextLine._metrics._formatter = _metrics._formatter;
			fullTextLine._metrics._height = _metrics._height;
			fullTextLine._metrics._baselineOffset = _metrics._baselineOffset;
			if (num > 0.0)
			{
				int finiteFormatWidth = _fullText.TextStore.Settings.GetFiniteFormatWidth(TextFormatterImp.RealToIdeal(num));
				bool forceWrap = _fullText.ForceWrap;
				_fullText.ForceWrap = true;
				if ((_statusFlags & StatusFlags.KeepState) != 0)
				{
					fullTextLine._statusFlags |= StatusFlags.KeepState;
				}
				fullTextLine.FormatLine(_fullText, _cpFirst, 0, finiteFormatWidth, finiteFormatWidth, _paragraphWidth, (textCollapsingProperties.Style == TextCollapsingStyle.TrailingCharacter) ? LineFlags.BreakAlways : LineFlags.None, formattedTextSymbols);
				_fullText.ForceWrap = forceWrap;
				fullTextLine._metrics._cchDepend = 0;
			}
			else if (formattedTextSymbols != null)
			{
				fullTextLine.AppendCollapsingSymbol(formattedTextSymbols);
			}
			if (fullTextLine._metrics._cchLength < Length)
			{
				fullTextLine._collapsedRange = new TextCollapsedRange(_cpFirst + fullTextLine._metrics._cchLength, Length - fullTextLine._metrics._cchLength, Width - fullTextLine.Width);
				fullTextLine._metrics._cchLength = Length;
			}
			fullTextLine._statusFlags |= StatusFlags.HasCollapsed;
			fullTextLine._statusFlags &= ~StatusFlags.HasOverflowed;
			return fullTextLine;
		}

		public override IList<TextCollapsedRange> GetTextCollapsedRanges()
		{
			if ((_statusFlags & StatusFlags.IsDisposed) != 0)
			{
				throw new ObjectDisposedException(SR.TextLineHasBeenDisposed);
			}
			if (_collapsedRange == null)
			{
				return null;
			}
			return new TextCollapsedRange[1] { _collapsedRange };
		}

		public override CharacterHit GetCharacterHitFromDistance(double distance)
		{
			if ((_statusFlags & StatusFlags.IsDisposed) != 0)
			{
				throw new ObjectDisposedException(SR.TextLineHasBeenDisposed);
			}
			return CharacterHitFromDistance(ParagraphUToLSLineU(TextFormatterImp.RealToIdeal(distance)));
		}

		private CharacterHit CharacterHitFromDistance(int hitTestDistance)
		{
			CharacterHit result = new CharacterHit(_cpFirst, 0);
			if (_ploline.Value == IntPtr.Zero)
			{
				return result;
			}
			if (HasCollapsed && _collapsedRange != null && _collapsingSymbol != null)
			{
				int num = _metrics._textStart + _metrics._textWidthAtTrailing;
				int num2 = TextFormatterImp.RealToIdeal(_collapsingSymbol.Width);
				if (hitTestDistance >= num - num2)
				{
					if (num - hitTestDistance < num2 / 2)
					{
						return new CharacterHit(_collapsedRange.TextSourceCharacterIndex, _collapsedRange.Length);
					}
					return new CharacterHit(_collapsedRange.TextSourceCharacterIndex, 0);
				}
			}
			LsQSubInfo[] array = new LsQSubInfo[_depthQueryMax];
			QueryLinePointPcp(new Point(hitTestDistance, 0.0), array, out var actualDepthQuery, out var lsTextCell);
			if (actualDepthQuery > 0 && lsTextCell.dupCell > 0)
			{
				LSRun run = GetRun((Plsrun)array[actualDepthQuery - 1].plsrun);
				int num3 = lsTextCell.lscpEndCell + 1 - lsTextCell.lscpStartCell;
				int trailingLength = (run.IsHitTestable ? 1 : run.Length);
				if (run.IsHitTestable && (run.HasExtendedCharacter || run.NeedsCaretInfo))
				{
					trailingLength = num3;
					num3 = 1;
				}
				int num4 = ((array[actualDepthQuery - 1].lstflowSubLine == array[0].lstflowSubLine) ? 1 : (-1));
				hitTestDistance = (hitTestDistance - lsTextCell.pointUvStartCell.x) * num4;
				Invariant.Assert(num3 > 0);
				int num5 = lsTextCell.dupCell / num3;
				int num6 = lsTextCell.dupCell % num3;
				for (int i = 0; i < num3; i++)
				{
					int num7 = num5;
					if (num6 > 0)
					{
						num7++;
						num6--;
					}
					if (hitTestDistance <= num7)
					{
						if (hitTestDistance > num7 / 2)
						{
							return new CharacterHit(GetExternalCp(lsTextCell.lscpStartCell) + i, trailingLength);
						}
						return new CharacterHit(GetExternalCp(lsTextCell.lscpStartCell) + i, 0);
					}
					hitTestDistance -= num7;
				}
				return new CharacterHit(GetExternalCp(lsTextCell.lscpStartCell) + num3 - 1, trailingLength);
			}
			return result;
		}

		public override double GetDistanceFromCharacterHit(CharacterHit characterHit)
		{
			if ((_statusFlags & StatusFlags.IsDisposed) != 0)
			{
				throw new ObjectDisposedException(SR.TextLineHasBeenDisposed);
			}
			TextFormatterImp.VerifyCaretCharacterHit(characterHit, _cpFirst, _metrics._cchLength);
			return _metrics._formatter.IdealToReal(LSLineUToParagraphU(DistanceFromCharacterHit(characterHit)), base.PixelsPerDip);
		}

		private int DistanceFromCharacterHit(CharacterHit characterHit)
		{
			int result = 0;
			if (_ploline.Value == IntPtr.Zero)
			{
				return result;
			}
			if (characterHit.FirstCharacterIndex >= _cpFirst + _metrics._cchLength)
			{
				return _metrics._textStart + _metrics._textWidthAtTrailing;
			}
			if (HasCollapsed && _collapsedRange != null && characterHit.FirstCharacterIndex >= _collapsedRange.TextSourceCharacterIndex)
			{
				int num = _metrics._textStart + _metrics._textWidthAtTrailing;
				if (characterHit.FirstCharacterIndex >= _collapsedRange.TextSourceCharacterIndex + _collapsedRange.Length || characterHit.TrailingLength != 0 || _collapsingSymbol == null)
				{
					return num;
				}
				return num - TextFormatterImp.RealToIdeal(_collapsingSymbol.Width);
			}
			LsQSubInfo[] array = new LsQSubInfo[_depthQueryMax];
			int internalCp = GetInternalCp(characterHit.FirstCharacterIndex);
			QueryLineCpPpoint(internalCp, array, out var actualDepthQuery, out var lsTextCell);
			if (actualDepthQuery > 0)
			{
				return lsTextCell.pointUvStartCell.x + GetDistanceInsideTextCell(internalCp, characterHit.TrailingLength != 0, array, actualDepthQuery, ref lsTextCell);
			}
			return result;
		}

		private int GetDistanceInsideTextCell(int lscpCurrent, bool isTrailing, LsQSubInfo[] sublineInfo, int actualSublineCount, ref LsTextCell lsTextCell)
		{
			int num = 0;
			int num2 = ((sublineInfo[actualSublineCount - 1].lstflowSubLine == sublineInfo[0].lstflowSubLine) ? 1 : (-1));
			int num3 = lsTextCell.lscpEndCell + 1 - lsTextCell.lscpStartCell;
			int num4 = lscpCurrent - lsTextCell.lscpStartCell;
			LSRun run = GetRun((Plsrun)sublineInfo[actualSublineCount - 1].plsrun);
			if (run.IsHitTestable && (run.HasExtendedCharacter || run.NeedsCaretInfo))
			{
				num3 = 1;
			}
			Invariant.Assert(num3 > 0);
			int num5 = lsTextCell.dupCell / num3;
			int num6 = lsTextCell.dupCell % num3;
			for (int i = 1; i <= num3; i++)
			{
				int num7 = num5;
				if (num6 > 0)
				{
					num7++;
					num6--;
				}
				if (num4 < i)
				{
					if (isTrailing)
					{
						return (num + num7) * num2;
					}
					return num * num2;
				}
				num += num7;
			}
			return num * num2;
		}

		public override CharacterHit GetNextCaretCharacterHit(CharacterHit characterHit)
		{
			if ((_statusFlags & StatusFlags.IsDisposed) != 0)
			{
				throw new ObjectDisposedException(SR.TextLineHasBeenDisposed);
			}
			TextFormatterImp.VerifyCaretCharacterHit(characterHit, _cpFirst, _metrics._cchLength);
			if (_ploline.Value == IntPtr.Zero)
			{
				return characterHit;
			}
			if (!GetNextOrPreviousCaretStop(characterHit.FirstCharacterIndex, CaretDirection.Forward, out var caretStopIndex, out var offsetToNextCaretStopIndex))
			{
				return characterHit;
			}
			if (caretStopIndex <= characterHit.FirstCharacterIndex && characterHit.TrailingLength != 0)
			{
				if (!GetNextOrPreviousCaretStop(caretStopIndex + offsetToNextCaretStopIndex, CaretDirection.Forward, out caretStopIndex, out offsetToNextCaretStopIndex))
				{
					return characterHit;
				}
				return new CharacterHit(caretStopIndex, offsetToNextCaretStopIndex);
			}
			return new CharacterHit(caretStopIndex, offsetToNextCaretStopIndex);
		}

		public override CharacterHit GetPreviousCaretCharacterHit(CharacterHit characterHit)
		{
			return GetPreviousCaretCharacterHitByBehavior(characterHit, CaretDirection.Backward);
		}

		public override CharacterHit GetBackspaceCaretCharacterHit(CharacterHit characterHit)
		{
			return GetPreviousCaretCharacterHitByBehavior(characterHit, CaretDirection.Backspace);
		}

		private CharacterHit GetPreviousCaretCharacterHitByBehavior(CharacterHit characterHit, CaretDirection direction)
		{
			if ((_statusFlags & StatusFlags.IsDisposed) != 0)
			{
				throw new ObjectDisposedException(SR.TextLineHasBeenDisposed);
			}
			TextFormatterImp.VerifyCaretCharacterHit(characterHit, _cpFirst, _metrics._cchLength);
			if (_ploline.Value == IntPtr.Zero)
			{
				return characterHit;
			}
			if (characterHit.FirstCharacterIndex == _cpFirst && characterHit.TrailingLength == 0)
			{
				return characterHit;
			}
			if (!GetNextOrPreviousCaretStop(characterHit.FirstCharacterIndex, direction, out var caretStopIndex, out var offsetToNextCaretStopIndex))
			{
				return characterHit;
			}
			if (offsetToNextCaretStopIndex != 0 && characterHit.TrailingLength == 0 && caretStopIndex != _cpFirst && caretStopIndex >= characterHit.FirstCharacterIndex && !GetNextOrPreviousCaretStop(caretStopIndex - 1, direction, out caretStopIndex, out offsetToNextCaretStopIndex))
			{
				return characterHit;
			}
			return new CharacterHit(caretStopIndex, 0);
		}

		private bool GetNextOrPreviousCaretStop(int currentIndex, CaretDirection direction, out int caretStopIndex, out int offsetToNextCaretStopIndex)
		{
			caretStopIndex = currentIndex;
			offsetToNextCaretStopIndex = 0;
			if (HasCollapsed && _collapsedRange != null && currentIndex >= _collapsedRange.TextSourceCharacterIndex)
			{
				caretStopIndex = _collapsedRange.TextSourceCharacterIndex;
				if (currentIndex < _collapsedRange.TextSourceCharacterIndex + _collapsedRange.Length)
				{
					offsetToNextCaretStopIndex = _collapsedRange.Length;
				}
				return true;
			}
			LsQSubInfo[] array = new LsQSubInfo[_depthQueryMax];
			LsTextCell lsTextCell = default(LsTextCell);
			int lscpVisisble = GetInternalCp(currentIndex);
			if (!FindNextOrPreviousVisibleCp(lscpVisisble, direction, out lscpVisisble))
			{
				return false;
			}
			QueryLineCpPpoint(lscpVisisble, array, out var actualDepthQuery, out lsTextCell);
			caretStopIndex = GetExternalCp(lsTextCell.lscpStartCell);
			if (actualDepthQuery > 0 && lscpVisisble >= lsTextCell.lscpStartCell && lscpVisisble <= lsTextCell.lscpEndCell)
			{
				LSRun run = GetRun((Plsrun)array[actualDepthQuery - 1].plsrun);
				if (run.IsHitTestable)
				{
					if (run.HasExtendedCharacter || (direction != CaretDirection.Backspace && run.NeedsCaretInfo))
					{
						offsetToNextCaretStopIndex = lsTextCell.lscpEndCell + 1 - lsTextCell.lscpStartCell;
					}
					else
					{
						caretStopIndex = GetExternalCp(lscpVisisble);
						offsetToNextCaretStopIndex = 1;
					}
				}
				else
				{
					offsetToNextCaretStopIndex = Math.Min(Length, run.Length - caretStopIndex + run.OffsetToFirstCp + _cpFirst);
				}
			}
			return true;
		}

		private bool FindNextOrPreviousVisibleCp(int lscp, CaretDirection direction, out int lscpVisisble)
		{
			lscpVisisble = lscp;
			SpanRider spanRider = new SpanRider(_plsrunVector);
			if (direction == CaretDirection.Forward)
			{
				while (lscpVisisble < _metrics._lscpLim)
				{
					spanRider.At(lscpVisisble - _cpFirst);
					if (GetRun((Plsrun)spanRider.CurrentElement).IsVisible)
					{
						return true;
					}
					lscpVisisble += spanRider.Length;
				}
			}
			else
			{
				for (lscpVisisble = Math.Min(lscpVisisble, _metrics._lscpLim - 1); lscpVisisble >= _cpFirst; lscpVisisble = _cpFirst + spanRider.CurrentSpanStart - 1)
				{
					spanRider.At(lscpVisisble - _cpFirst);
					LSRun run = GetRun((Plsrun)spanRider.CurrentElement);
					if (run.IsVisible)
					{
						return true;
					}
					if (run.IsNewline)
					{
						lscpVisisble = _cpFirst + spanRider.CurrentSpanStart;
						return true;
					}
				}
			}
			lscpVisisble = lscp;
			return false;
		}

		private TextBounds[] CreateDegenerateBounds()
		{
			return new TextBounds[1]
			{
				new TextBounds(new Rect(0.0, 0.0, 0.0, Height), RightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight, null)
			};
		}

		private TextBounds CreateCollapsingSymbolBounds()
		{
			return new TextBounds(new Rect(Start + Width - _collapsingSymbol.Width, 0.0, _collapsingSymbol.Width, Height), RightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight, null);
		}

		public override IList<TextBounds> GetTextBounds(int firstTextSourceCharacterIndex, int textLength)
		{
			if ((_statusFlags & StatusFlags.IsDisposed) != 0)
			{
				throw new ObjectDisposedException(SR.TextLineHasBeenDisposed);
			}
			if (textLength == 0)
			{
				throw new ArgumentOutOfRangeException("textLength", SR.ParameterMustBeGreaterThanZero);
			}
			if (textLength < 0)
			{
				firstTextSourceCharacterIndex += textLength;
				textLength = -textLength;
			}
			if (firstTextSourceCharacterIndex < _cpFirst)
			{
				textLength += firstTextSourceCharacterIndex - _cpFirst;
				firstTextSourceCharacterIndex = _cpFirst;
			}
			if (firstTextSourceCharacterIndex > _cpFirst + _metrics._cchLength - textLength)
			{
				textLength = _cpFirst + _metrics._cchLength - firstTextSourceCharacterIndex;
			}
			if (_ploline.Value == IntPtr.Zero)
			{
				return CreateDegenerateBounds();
			}
			Point origin = new Point(0.0, 0.0);
			LsQSubInfo[] array = new LsQSubInfo[_depthQueryMax];
			int internalCp = GetInternalCp(firstTextSourceCharacterIndex);
			QueryLineCpPpoint(internalCp, array, out var actualDepthQuery, out var lsTextCell);
			if (actualDepthQuery <= 0)
			{
				return CreateDegenerateBounds();
			}
			LsQSubInfo[] array2 = new LsQSubInfo[_depthQueryMax];
			int internalCp2 = GetInternalCp(firstTextSourceCharacterIndex + textLength - 1);
			QueryLineCpPpoint(internalCp2, array2, out var actualDepthQuery2, out var lsTextCell2);
			if (actualDepthQuery2 <= 0)
			{
				return CreateDegenerateBounds();
			}
			bool flag = _collapsingSymbol != null && _collapsedRange != null && firstTextSourceCharacterIndex < _collapsedRange.TextSourceCharacterIndex && firstTextSourceCharacterIndex + textLength - _collapsedRange.TextSourceCharacterIndex > _collapsedRange.Length / 2;
			TextBounds[] array3 = null;
			ArrayList arrayList = null;
			bool isTrailing = false;
			bool isTrailing2 = true;
			if (internalCp > lsTextCell.lscpEndCell)
			{
				isTrailing = true;
			}
			if (internalCp2 < lsTextCell2.lscpStartCell)
			{
				isTrailing2 = false;
			}
			if (actualDepthQuery == actualDepthQuery2 && array[actualDepthQuery - 1].lscpFirstSubLine == array2[actualDepthQuery2 - 1].lscpFirstSubLine)
			{
				int num = ((!flag) ? 1 : 2);
				array3 = new TextBounds[num];
				array3[0] = new TextBounds(LSRun.RectUV(origin, new LSPOINT(LSLineUToParagraphU(GetDistanceInsideTextCell(internalCp, isTrailing, array, actualDepthQuery, ref lsTextCell) + lsTextCell.pointUvStartCell.x), 0), new LSPOINT(LSLineUToParagraphU(GetDistanceInsideTextCell(internalCp2, isTrailing2, array2, actualDepthQuery2, ref lsTextCell2) + lsTextCell2.pointUvStartCell.x), _metrics._height), this), Convert.LsTFlowToFlowDirection(array[actualDepthQuery - 1].lstflowSubLine), CalculateTextRunBounds(internalCp, internalCp2 + 1));
				if (num > 1)
				{
					array3[1] = CreateCollapsingSymbolBounds();
				}
			}
			else
			{
				arrayList = new ArrayList(2);
				int lscpCurrent = internalCp;
				int lscpEnd = Math.Min(internalCp2, array2[actualDepthQuery2 - 1].lscpFirstSubLine + array2[actualDepthQuery2 - 1].lsdcpSubLine - 1);
				int currentDistance = GetDistanceInsideTextCell(internalCp, isTrailing, array, actualDepthQuery, ref lsTextCell) + lsTextCell.pointUvStartCell.x;
				CollectTextBoundsToBaseLevel(arrayList, ref lscpCurrent, ref currentDistance, array, actualDepthQuery, lscpEnd, out var baseLevelDepth);
				if (baseLevelDepth < actualDepthQuery2)
				{
					CollectTextBoundsFromBaseLevel(arrayList, ref lscpCurrent, ref currentDistance, array2, actualDepthQuery2, baseLevelDepth);
				}
				AddValidTextBounds(arrayList, new TextBounds(LSRun.RectUV(origin, new LSPOINT(LSLineUToParagraphU(currentDistance), 0), new LSPOINT(LSLineUToParagraphU(GetDistanceInsideTextCell(internalCp2, isTrailing2, array2, actualDepthQuery2, ref lsTextCell2) + lsTextCell2.pointUvStartCell.x), _metrics._height), this), Convert.LsTFlowToFlowDirection(array2[actualDepthQuery2 - 1].lstflowSubLine), CalculateTextRunBounds(lscpCurrent, internalCp2 + 1)));
			}
			if (array3 == null)
			{
				if (arrayList.Count > 0)
				{
					if (flag)
					{
						AddValidTextBounds(arrayList, CreateCollapsingSymbolBounds());
					}
					array3 = new TextBounds[arrayList.Count];
					for (int i = 0; i < arrayList.Count; i++)
					{
						array3[i] = (TextBounds)arrayList[i];
					}
				}
				else
				{
					int horizontalPosition = LSLineUToParagraphU(GetDistanceInsideTextCell(internalCp, isTrailing, array, actualDepthQuery, ref lsTextCell) + lsTextCell.pointUvStartCell.x);
					array3 = new TextBounds[1]
					{
						new TextBounds(LSRun.RectUV(origin, new LSPOINT(horizontalPosition, 0), new LSPOINT(horizontalPosition, _metrics._height), this), Convert.LsTFlowToFlowDirection(array[actualDepthQuery - 1].lstflowSubLine), null)
					};
				}
			}
			return array3;
		}

		private void CollectTextBoundsToBaseLevel(ArrayList boundsList, ref int lscpCurrent, ref int currentDistance, LsQSubInfo[] sublines, int sublineDepth, int lscpEnd, out int baseLevelDepth)
		{
			baseLevelDepth = sublineDepth;
			if (lscpEnd >= sublines[sublineDepth - 1].lscpFirstSubLine + sublines[sublineDepth - 1].lsdcpSubLine)
			{
				AddValidTextBounds(boundsList, new TextBounds(LSRun.RectUV(new Point(0.0, 0.0), new LSPOINT(LSLineUToParagraphU(currentDistance), 0), new LSPOINT(LSLineUToParagraphU(GetEndOfSublineDistance(sublines, sublineDepth - 1)), _metrics._height), this), Convert.LsTFlowToFlowDirection(sublines[sublineDepth - 1].lstflowSubLine), CalculateTextRunBounds(lscpCurrent, sublines[sublineDepth - 1].lscpFirstSubLine + sublines[sublineDepth - 1].lsdcpSubLine)));
				baseLevelDepth = sublineDepth - 1;
				while (baseLevelDepth > 0 && lscpEnd >= sublines[baseLevelDepth - 1].lscpFirstSubLine + sublines[baseLevelDepth - 1].lsdcpSubLine)
				{
					int num = baseLevelDepth - 1;
					AddValidTextBounds(boundsList, new TextBounds(LSRun.RectUV(new Point(0.0, 0.0), new LSPOINT(LSLineUToParagraphU(GetEndOfRunDistance(sublines, num)), 0), new LSPOINT(LSLineUToParagraphU(GetEndOfSublineDistance(sublines, num)), _metrics._height), this), Convert.LsTFlowToFlowDirection(sublines[num].lstflowSubLine), CalculateTextRunBounds(sublines[num].lscpFirstRun + sublines[num].lsdcpRun, sublines[num].lscpFirstSubLine + sublines[num].lsdcpSubLine)));
					baseLevelDepth--;
				}
				Invariant.Assert(baseLevelDepth >= 1);
				lscpCurrent = sublines[baseLevelDepth - 1].lscpFirstRun + sublines[baseLevelDepth - 1].lsdcpRun;
				currentDistance = GetEndOfRunDistance(sublines, baseLevelDepth - 1);
			}
		}

		private void CollectTextBoundsFromBaseLevel(ArrayList boundsList, ref int lscpCurrent, ref int currentDistance, LsQSubInfo[] sublines, int sublineDepth, int baseLevelDepth)
		{
			Invariant.Assert(lscpCurrent <= sublines[baseLevelDepth - 1].lscpFirstRun);
			AddValidTextBounds(boundsList, new TextBounds(LSRun.RectUV(new Point(0.0, 0.0), new LSPOINT(LSLineUToParagraphU(currentDistance), 0), new LSPOINT(LSLineUToParagraphU(sublines[baseLevelDepth - 1].pointUvStartRun.x), _metrics._height), this), Convert.LsTFlowToFlowDirection(sublines[baseLevelDepth - 1].lstflowSubLine), CalculateTextRunBounds(lscpCurrent, sublines[baseLevelDepth - 1].lscpFirstRun)));
			for (int i = baseLevelDepth; i < sublineDepth - 1; i++)
			{
				AddValidTextBounds(boundsList, new TextBounds(LSRun.RectUV(new Point(0.0, 0.0), new LSPOINT(LSLineUToParagraphU(sublines[i].pointUvStartSubLine.x), 0), new LSPOINT(LSLineUToParagraphU(sublines[i].pointUvStartRun.x), _metrics._height), this), Convert.LsTFlowToFlowDirection(sublines[i].lstflowSubLine), CalculateTextRunBounds(sublines[i].lscpFirstSubLine, sublines[i].lscpFirstRun)));
			}
			lscpCurrent = sublines[sublineDepth - 1].lscpFirstSubLine;
			currentDistance = sublines[sublineDepth - 1].pointUvStartSubLine.x;
		}

		private int GetEndOfSublineDistance(LsQSubInfo[] sublines, int index)
		{
			return sublines[index].pointUvStartSubLine.x + ((sublines[index].lstflowSubLine == sublines[0].lstflowSubLine) ? sublines[index].dupSubLine : (-sublines[index].dupSubLine));
		}

		private int GetEndOfRunDistance(LsQSubInfo[] sublines, int index)
		{
			return sublines[index].pointUvStartRun.x + ((sublines[index].lstflowSubLine == sublines[0].lstflowSubLine) ? sublines[index].dupRun : (-sublines[index].dupRun));
		}

		private void AddValidTextBounds(ArrayList boundsList, TextBounds bounds)
		{
			if (bounds.Rectangle.Width != 0.0 && bounds.Rectangle.Height != 0.0)
			{
				boundsList.Add(bounds);
			}
		}

		private IList<TextRunBounds> CalculateTextRunBounds(int lscpFirst, int lscpEnd)
		{
			if (lscpEnd <= lscpFirst)
			{
				return null;
			}
			int num = lscpFirst;
			int num2 = lscpEnd - lscpFirst;
			SpanRider spanRider = new SpanRider(_plsrunVector);
			Point origin = new Point(0.0, 0.0);
			IList<TextRunBounds> list = new List<TextRunBounds>(2);
			while (num2 > 0)
			{
				spanRider.At(num - _cpFirst);
				Plsrun plsrun = (Plsrun)spanRider.CurrentElement;
				int num3 = Math.Min(spanRider.Length, num2);
				if (TextStore.IsContent(plsrun))
				{
					LSRun run = GetRun(plsrun);
					if (run.Type == Plsrun.Text || run.Type == Plsrun.InlineObject)
					{
						int externalCp = GetExternalCp(num);
						int num4 = num3;
						if (HasCollapsed && _collapsedRange != null && externalCp <= _collapsedRange.TextSourceCharacterIndex && externalCp + num4 >= _collapsedRange.TextSourceCharacterIndex && externalCp + num4 < _collapsedRange.TextSourceCharacterIndex + _collapsedRange.Length)
						{
							num4 = _collapsedRange.TextSourceCharacterIndex - externalCp;
						}
						if (num4 > 0)
						{
							TextRunBounds item = new TextRunBounds(LSRun.RectUV(origin, new LSPOINT(LSLineUToParagraphU(DistanceFromCharacterHit(new CharacterHit(externalCp, 0))), _metrics._baselineOffset - run.BaselineOffset + run.BaselineMoveOffset), new LSPOINT(LSLineUToParagraphU(DistanceFromCharacterHit(new CharacterHit(externalCp + num4 - 1, 1))), _metrics._baselineOffset - run.BaselineOffset + run.BaselineMoveOffset + run.Height), this), externalCp, externalCp + num4, run.TextRun);
							list.Add(item);
						}
					}
				}
				num2 -= num3;
				num += num3;
			}
			if (list.Count <= 0)
			{
				return null;
			}
			return list;
		}

		public override IList<TextSpan<TextRun>> GetTextRunSpans()
		{
			if ((_statusFlags & StatusFlags.IsDisposed) != 0)
			{
				throw new ObjectDisposedException(SR.TextLineHasBeenDisposed);
			}
			if (_plsrunVector == null)
			{
				return Array.Empty<TextSpan<TextRun>>();
			}
			List<TextSpan<TextRun>> list = new List<TextSpan<TextRun>>(2);
			TextRun textRun = null;
			int num = 0;
			int num2 = _metrics._cchLength;
			for (int i = 0; i < _plsrunVector.Count; i++)
			{
				if (num2 <= 0)
				{
					break;
				}
				Span span = _plsrunVector[i];
				int val = CpCount(span);
				val = Math.Min(val, num2);
				if (val > 0)
				{
					TextRun textRun2 = GetRun((Plsrun)span.element).TextRun;
					if (textRun != null && textRun2 != textRun)
					{
						list.Add(new TextSpan<TextRun>(num, textRun));
						num = 0;
					}
					textRun = textRun2;
					num += val;
					num2 -= val;
				}
			}
			if (textRun != null)
			{
				list.Add(new TextSpan<TextRun>(num, textRun));
			}
			return list;
		}

		public override IEnumerable<IndexedGlyphRun> GetIndexedGlyphRuns()
		{
			if ((_statusFlags & StatusFlags.IsDisposed) != 0)
			{
				throw new ObjectDisposedException(SR.TextLineHasBeenDisposed);
			}
			IEnumerable<IndexedGlyphRun> result = null;
			if (_ploline.Value != IntPtr.Zero)
			{
				TextFormatterContext textFormatterContext = _metrics._formatter.AcquireContext(new DrawingState(null, new Point(0.0, 0.0), null, this), _ploc.Value);
				LsErr lsErr = LsErr.None;
				LSPOINT pt = new LSPOINT(0, 0);
				lsErr = UnsafeNativeMethods.LoEnumLine(_ploline.Value, reverseOder: false, fGeometryneeded: false, ref pt);
				result = textFormatterContext.IndexedGlyphRuns;
				Exception callbackException = textFormatterContext.CallbackException;
				textFormatterContext.ClearIndexedGlyphRuns();
				textFormatterContext.Release();
				if (lsErr != 0)
				{
					if (callbackException != null)
					{
						throw callbackException;
					}
					TextFormatterContext.ThrowExceptionFromLsError(SR.Format(SR.EnumLineFailure, lsErr), lsErr);
				}
			}
			return result;
		}

		public override TextLineBreak GetTextLineBreak()
		{
			if ((_statusFlags & StatusFlags.IsDisposed) != 0)
			{
				throw new ObjectDisposedException(SR.TextLineHasBeenDisposed);
			}
			if ((_statusFlags & StatusFlags.HasCollapsed) != 0)
			{
				return null;
			}
			return _metrics.GetTextLineBreak(IntPtr.Zero);
		}

		private unsafe void QueryLinePointPcp(Point ptQuery, LsQSubInfo[] subLineInfo, out int actualDepthQuery, out LsTextCell lsTextCell)
		{
			LsErr lsErr = LsErr.None;
			lsTextCell = default(LsTextCell);
			fixed (LsQSubInfo* pSubLineInfo = subLineInfo)
			{
				LSPOINT ptQuery2 = new LSPOINT((int)ptQuery.X, (int)ptQuery.Y);
				lsErr = UnsafeNativeMethods.LoQueryLinePointPcp(_ploline.Value, ref ptQuery2, subLineInfo.Length, (nint)pSubLineInfo, out actualDepthQuery, out lsTextCell);
			}
			if (lsErr != 0)
			{
				TextFormatterContext.ThrowExceptionFromLsError(SR.Format(SR.QueryLineFailure, lsErr), lsErr);
			}
			if (lsTextCell.lscpEndCell < lsTextCell.lscpStartCell)
			{
				lsTextCell.lscpEndCell = lsTextCell.lscpStartCell;
			}
		}

		private unsafe void QueryLineCpPpoint(int lscpQuery, LsQSubInfo[] subLineInfo, out int actualDepthQuery, out LsTextCell lsTextCell)
		{
			LsErr lsErr = LsErr.None;
			lsTextCell = default(LsTextCell);
			int lscpQuery2 = ((lscpQuery < _metrics._lscpLim) ? lscpQuery : (_metrics._lscpLim - 1));
			fixed (LsQSubInfo* pSubLineInfo = subLineInfo)
			{
				lsErr = UnsafeNativeMethods.LoQueryLineCpPpoint(_ploline.Value, lscpQuery2, subLineInfo.Length, (nint)pSubLineInfo, out actualDepthQuery, out lsTextCell);
			}
			if (lsErr != 0)
			{
				TextFormatterContext.ThrowExceptionFromLsError(SR.Format(SR.QueryLineFailure, lsErr), lsErr);
			}
			if (lsTextCell.lscpEndCell < lsTextCell.lscpStartCell)
			{
				lsTextCell.lscpEndCell = lsTextCell.lscpStartCell;
			}
		}

		internal int LSLineUToParagraphU(int u)
		{
			return u + _metrics._paragraphToText - _metrics._textStart;
		}

		internal int ParagraphUToLSLineU(int u)
		{
			return u - _metrics._paragraphToText + _metrics._textStart;
		}

		private void BuildOverhang(Point origin, Rect boundingBox)
		{
			if (boundingBox.IsEmpty)
			{
				_overhang.Leading = (_overhang.Trailing = 0.0);
				_overhang.Before = 0.0;
				_overhang.Extent = 0.0;
				return;
			}
			boundingBox.X -= origin.X;
			boundingBox.Y -= origin.Y;
			if (RightToLeft)
			{
				double num = _metrics._formatter.IdealToReal(_paragraphWidth, base.PixelsPerDip);
				_overhang.Leading = num - Start - boundingBox.Right;
				_overhang.Trailing = boundingBox.Left - (num - Start - Width);
			}
			else
			{
				_overhang.Leading = boundingBox.Left - Start;
				_overhang.Trailing = Start + Width - boundingBox.Right;
			}
			_overhang.Extent = boundingBox.Bottom - boundingBox.Top;
			_overhang.Before = 0.0 - boundingBox.Top;
		}

		internal int GetInternalCp(int cp)
		{
			int num = _cpFirst;
			int num2 = cp;
			cp = num;
			foreach (Span item in _plsrunVector)
			{
				int num3 = CpCount(item);
				if (num3 > 0)
				{
					if (cp + num3 > num2)
					{
						num += ((num3 == item.length) ? (num2 - cp) : 0);
						break;
					}
					cp += num3;
				}
				num += item.length;
			}
			return num;
		}

		internal int GetExternalCp(int lscp)
		{
			if (lscp >= _metrics._lscpLim)
			{
				if (_collapsedRange != null)
				{
					return _collapsedRange.TextSourceCharacterIndex;
				}
				return _cpFirst + _metrics._cchLength;
			}
			SpanRider spanRider = new SpanRider(_plsrunVector);
			int offsetToFirstCp;
			do
			{
				spanRider.At(lscp - _cpFirst);
				offsetToFirstCp = GetRun((Plsrun)spanRider.CurrentElement).OffsetToFirstCp;
			}
			while (offsetToFirstCp < 0 && ++lscp < _metrics._lscpLim);
			return offsetToFirstCp + lscp - spanRider.CurrentSpanStart;
		}

		internal int CpCount(Span plsrunSpan)
		{
			Plsrun plsrun = (Plsrun)plsrunSpan.element;
			plsrun = TextStore.ToIndex(plsrun);
			if (plsrun >= Plsrun.FormatAnchor)
			{
				return GetRun(plsrun).Length;
			}
			return 0;
		}

		internal LSRun GetRun(Plsrun plsrun)
		{
			ArrayList arrayList = _lsrunsMainText;
			if (TextStore.IsMarker(plsrun))
			{
				arrayList = _lsrunsMarkerText;
			}
			plsrun = TextStore.ToIndex(plsrun);
			return (LSRun)(TextStore.IsContent(plsrun) ? arrayList[(int)(plsrun - 3)] : TextStore.ControlRuns[(uint)plsrun]);
		}
	}

	private TextFormatterImp _formatter;

	private int _lscpLim;

	private int _cchLength;

	private int _cchDepend;

	private int _cchNewline;

	private int _height;

	private int _textHeight;

	private int _baselineOffset;

	private int _textAscent;

	private int _textStart;

	private int _textWidth;

	private int _textWidthAtTrailing;

	private int _paragraphToText;

	private LSRun _lastRun;

	private double _pixelsPerDip;

	public int Length => _cchLength;

	public int DependentLength => _cchDepend;

	public int NewlineLength => _cchNewline;

	public double Start => _formatter.IdealToReal(_paragraphToText - _textStart, _pixelsPerDip);

	public double Width => _formatter.IdealToReal(_textWidthAtTrailing + _textStart, _pixelsPerDip);

	public double WidthIncludingTrailingWhitespace => _formatter.IdealToReal(_textWidth + _textStart, _pixelsPerDip);

	public double Height => _formatter.IdealToReal(_height, _pixelsPerDip);

	public double TextHeight => _formatter.IdealToReal(_textHeight, _pixelsPerDip);

	public double Baseline => _formatter.IdealToReal(_baselineOffset, _pixelsPerDip);

	public double TextBaseline => _formatter.IdealToReal(_textAscent, _pixelsPerDip);

	public double MarkerBaseline => Baseline;

	public double MarkerHeight => Height;

	internal unsafe void Compute(FullTextState fullText, int firstCharIndex, int paragraphWidth, FormattedTextSymbols collapsingSymbol, ref LsLineWidths lineWidths, LsLInfo* plsLineInfo)
	{
		_formatter = fullText.Formatter;
		TextStore textStore = fullText.TextStore;
		_pixelsPerDip = textStore.Settings.TextSource.PixelsPerDip;
		_textStart = lineWidths.upStartMainText;
		_textWidthAtTrailing = lineWidths.upStartTrailing;
		_textWidth = lineWidths.upLimLine;
		if (collapsingSymbol != null)
		{
			AppendCollapsingSymbolWidth(TextFormatterImp.RealToIdeal(collapsingSymbol.Width));
		}
		_textWidth -= _textStart;
		_textWidthAtTrailing -= _textStart;
		_cchNewline = textStore.CchEol;
		_lscpLim = plsLineInfo->cpLimToContinue;
		_lastRun = fullText.CountText(_lscpLim, firstCharIndex, out _cchLength);
		if (plsLineInfo->endr != LsEndRes.endrEndPara && plsLineInfo->endr != LsEndRes.endrSoftCR)
		{
			_cchNewline = 0;
			if (plsLineInfo->dcpDepend >= 0)
			{
				int lscpLim = Math.Max(plsLineInfo->cpLimToContinue + plsLineInfo->dcpDepend, fullText.LscpHyphenationLookAhead);
				fullText.CountText(lscpLim, firstCharIndex, out _cchDepend);
				_cchDepend -= _cchLength;
			}
		}
		ParaProp pap = textStore.Pap;
		if (_height <= 0)
		{
			if (pap.LineHeight > 0)
			{
				_height = pap.LineHeight;
				_baselineOffset = (int)Math.Round((double)_height * pap.DefaultTypeface.Baseline(pap.EmSize, 1.0 / 300.0, _pixelsPerDip, fullText.TextFormattingMode) / pap.DefaultTypeface.LineSpacing(pap.EmSize, 1.0 / 300.0, _pixelsPerDip, fullText.TextFormattingMode));
			}
			if (plsLineInfo->dvrMultiLineHeight == int.MaxValue)
			{
				_textAscent = (int)Math.Round(pap.DefaultTypeface.Baseline(pap.EmSize, 1.0 / 300.0, _pixelsPerDip, fullText.TextFormattingMode));
				_textHeight = (int)Math.Round(pap.DefaultTypeface.LineSpacing(pap.EmSize, 1.0 / 300.0, _pixelsPerDip, fullText.TextFormattingMode));
			}
			else
			{
				_textAscent = plsLineInfo->dvrAscent;
				_textHeight = _textAscent + plsLineInfo->dvrDescent;
				if (fullText.VerticalAdjust)
				{
					textStore.AdjustRunsVerticalOffset(plsLineInfo->cpLimToContinue - firstCharIndex, _height, _baselineOffset, out _textHeight, out _textAscent);
				}
			}
			if (_height <= 0)
			{
				_height = _textHeight;
				_baselineOffset = _textAscent;
			}
		}
		switch (pap.Align)
		{
		case TextAlignment.Right:
			_paragraphToText = paragraphWidth - _textWidthAtTrailing;
			break;
		case TextAlignment.Center:
			_paragraphToText = (int)Math.Round((double)(paragraphWidth + _textStart - _textWidthAtTrailing) * 0.5);
			break;
		default:
			_paragraphToText = pap.ParagraphIndent + _textStart;
			break;
		}
	}

	internal TextLineBreak GetTextLineBreak(nint ploline)
	{
		nint pbreakrec = IntPtr.Zero;
		if (ploline != IntPtr.Zero)
		{
			LsErr lsErr = UnsafeNativeMethods.LoAcquireBreakRecord(ploline, out pbreakrec);
			if (lsErr != 0)
			{
				TextFormatterContext.ThrowExceptionFromLsError(SR.Format(SR.AcquireBreakRecordFailure, lsErr), lsErr);
			}
		}
		if (_lastRun != null && _lastRun.TextModifierScope != null && !(_lastRun.TextRun is TextEndOfParagraph))
		{
			return new TextLineBreak(_lastRun.TextModifierScope, new MS.Internal.SecurityCriticalDataForSet<nint>(pbreakrec));
		}
		if (pbreakrec == IntPtr.Zero)
		{
			return null;
		}
		return new TextLineBreak(null, new MS.Internal.SecurityCriticalDataForSet<nint>(pbreakrec));
	}

	private void AppendCollapsingSymbolWidth(int symbolIdealWidth)
	{
		_textWidth += symbolIdealWidth;
		_textWidthAtTrailing += symbolIdealWidth;
	}
}
