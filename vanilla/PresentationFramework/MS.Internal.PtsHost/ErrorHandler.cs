namespace MS.Internal.PtsHost;

internal static class ErrorHandler
{
	internal static string PtsCacheAlreadyCreated = "PTS cache is already created.";

	internal static string PtsCacheAlreadyDestroyed = "PTS cache is already destroyed.";

	internal static string NullPtsHost = "Valid PtsHost object is required.";

	internal static string CreateContextFailed = "Failed to create PTS Context.";

	internal static string EnumIntegrationError = "Some enum values has been changed. Need to update dependent code.";

	internal static string NoNeedToDestroyPtsPage = "PTS page is not created, there is no need to destroy it.";

	internal static string NotSupportedFiniteIncremental = "Incremental update is not supported yet.";

	internal static string NotSupportedFootnotes = "Footnotes are not supported yet.";

	internal static string NotSupportedCompositeColumns = "Composite columns are not supported yet.";

	internal static string NotSupportedDropCap = "DropCap is not supported yet.";

	internal static string NotSupportedForcedLineBreaks = "Forced vertical line break is not supported yet.";

	internal static string NotSupportedMultiSection = "Multiply sections are not supported yet.";

	internal static string NotSupportedSubtrackShift = "Column shifting is not supported yet.";

	internal static string NullObjectInCreateHandle = "Valid object is required to create handle.";

	internal static string InvalidHandle = "No object associated with the handle or type mismatch.";

	internal static string HandleOutOfRange = "Object handle has to be within handle array's range.";

	internal static string BreakRecordDisposed = "Break record already disposed.";

	internal static string BreakRecordNotNeeded = "There is no need to create break record.";

	internal static string BrokenParaHasMcs = "Broken paragraph cannot have margin collapsing state.";

	internal static string BrokenParaHasTopSpace = "Top space should be always suppressed at the top of broken paragraph.";

	internal static string GoalReachedHasBreakRecord = "Goal is reached, so there should be no break record.";

	internal static string BrokenContentRequiresBreakRecord = "Goal is not reached, break record is required to continue.";

	internal static string PTSAssert = "PTS Assert:\n\t{0}\n\t{1}\n\t{2}\n\t{3}";

	internal static string ParaHandleMismatch = "Paragraph handle mismatch.";

	internal static string PTSObjectsCountMismatch = "Actual number of PTS objects does not match number of requested PTS objects.";

	internal static string SubmitForEmptyRange = "Submitting embedded objects for empty range.";

	internal static string SubmitInvalidList = "Submitting invalid list of embedded objects.";

	internal static string HandledInsideSegmentPara = "Paragraph structure invalidation should be handled by Segments.";

	internal static string EmptyParagraph = "There are no lines in the paragraph.";

	internal static string ParaStartsWithEOP = "NameTable is out of sync with TextContainer. The next paragraph begins with EOP.";

	internal static string FetchParaAtTextRangePosition = "Trying to fetch paragraph at not supported TextPointer - TextRange.";

	internal static string ParagraphCharacterCountMismatch = "Paragraph's character count is out of sync.";

	internal static string ContainerNeedsTextElement = "Container paragraph can be only created for TextElement.";

	internal static string CannotPositionInsideUIElement = "Cannot position TextPointer inside a UIElement.";

	internal static string CannotFindUIElement = "Cannot find specified UIElement in the TextContainer.";

	internal static string InvalidDocumentPage = "DocumentPage is not created for IDocumentPaginatorSource object.";

	internal static string NoVisualToTransfer = "Old paragraph does not have a visual node. Cannot transfer data.";

	internal static string UpdateShiftedNotValid = "Update shifted is not a valid update type for top level PTS objects.";

	internal static string ColumnVisualCountMismatch = "Number of column visuals does not match number of columns.";

	internal static string VisualTypeMismatch = "Visual does not match expected type.";

	internal static string EmbeddedObjectTypeMismatch = "EmbeddedObject type missmatch.";

	internal static string EmbeddedObjectOwnerMismatch = "Cannot transfer data from an embedded object representing another element.";

	internal static string LineAlreadyDestroyed = "Line has been already disposed.";

	internal static string OnlyOneRectIsExpected = "Expecting only one rect for text object.";

	internal static string NotInLineBoundary = "Requesting data outside of line's range.";

	internal static string FetchRunAtTextArrayStart = "Trying to fetch run at the beginning of TextContainer.";

	internal static string TextFormatterHostNotInitialized = "TextFormatter host is not initialized.";

	internal static string NegativeCharacterIndex = "Character index must be non-negative.";

	internal static string NoClientDataForObjectRun = "ClientData should be always provided for object runs.";

	internal static string UnknownDOTypeInTextArray = "Unknown DependencyObject type stored in TextContainer.";

	internal static string NegativeObjectWidth = "Negative object's width within a text line.";

	internal static string NoUIElementForObjectPosition = "TextContainer does not have a UIElement for position of Object type.";

	internal static string InlineObjectCacheCorrupted = "Paragraph's inline object cache is corrupted.";

	internal static void Assert(bool condition, string message)
	{
	}

	internal static void Assert(bool condition, string format, params object[] args)
	{
	}
}
