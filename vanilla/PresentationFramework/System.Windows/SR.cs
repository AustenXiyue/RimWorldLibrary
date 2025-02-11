using System.Resources;
using System.Runtime.CompilerServices;
using FxResources.PresentationFramework;

namespace System.Windows;

internal static class SR
{
	private static ResourceManager s_resourceManager;

	internal static ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new ResourceManager(typeof(FxResources.PresentationFramework.SR)));

	internal static string AbsoluteUriNotAllowed => GetResourceString("AbsoluteUriNotAllowed");

	internal static string AbsoluteUriOnly => GetResourceString("AbsoluteUriOnly");

	internal static string AccessCollectionAfterShutDown => GetResourceString("AccessCollectionAfterShutDown");

	internal static string AddAnnotationsNotImplemented => GetResourceString("AddAnnotationsNotImplemented");

	internal static string AddedItemNotAtIndex => GetResourceString("AddedItemNotAtIndex");

	internal static string AddedItemNotInCollection => GetResourceString("AddedItemNotInCollection");

	internal static string AdornedElementNotFound => GetResourceString("AdornedElementNotFound");

	internal static string AdornedElementPlaceholderMustBeInTemplate => GetResourceString("AdornedElementPlaceholderMustBeInTemplate");

	internal static string AdornerNotFound => GetResourceString("AdornerNotFound");

	internal static string AffectedByMsCtfIssue => GetResourceString("AffectedByMsCtfIssue");

	internal static string AlreadyHasLogicalChildren => GetResourceString("AlreadyHasLogicalChildren");

	internal static string AlreadyHasParent => GetResourceString("AlreadyHasParent");

	internal static string AnnotationAdorner_NotUIElement => GetResourceString("AnnotationAdorner_NotUIElement");

	internal static string AnnotationAlreadyExistInService => GetResourceString("AnnotationAlreadyExistInService");

	internal static string AnnotationAlreadyExists => GetResourceString("AnnotationAlreadyExists");

	internal static string AnnotationIsNull => GetResourceString("AnnotationIsNull");

	internal static string AnnotationServiceAlreadyExists => GetResourceString("AnnotationServiceAlreadyExists");

	internal static string AnnotationServiceIsAlreadyEnabled => GetResourceString("AnnotationServiceIsAlreadyEnabled");

	internal static string AnnotationServiceNotEnabled => GetResourceString("AnnotationServiceNotEnabled");

	internal static string AppActivationException => GetResourceString("AppActivationException");

	internal static string ApplicationAlreadyRunning => GetResourceString("ApplicationAlreadyRunning");

	internal static string ApplicationShuttingDown => GetResourceString("ApplicationShuttingDown");

	internal static string ArgumentLengthMismatch => GetResourceString("ArgumentLengthMismatch");

	internal static string ArgumentPropertyMustNotBeNull => GetResourceString("ArgumentPropertyMustNotBeNull");

	internal static string Argument_InvalidOffLen => GetResourceString("Argument_InvalidOffLen");

	internal static string ArrangeReentrancyInvalid => GetResourceString("ArrangeReentrancyInvalid");

	internal static string AssemblyIdNegative => GetResourceString("AssemblyIdNegative");

	internal static string AssemblyIdOutOfSequence => GetResourceString("AssemblyIdOutOfSequence");

	internal static string AssemblyTagMissing => GetResourceString("AssemblyTagMissing");

	internal static string AttachablePropertyNotFound => GetResourceString("AttachablePropertyNotFound");

	internal static string AudioVideo_CannotControlMedia => GetResourceString("AudioVideo_CannotControlMedia");

	internal static string AudioVideo_InvalidDependencyObject => GetResourceString("AudioVideo_InvalidDependencyObject");

	internal static string AudioVideo_InvalidMediaState => GetResourceString("AudioVideo_InvalidMediaState");

	internal static string AuxiliaryFilterReturnedAnomalousCountOfCharacters => GetResourceString("AuxiliaryFilterReturnedAnomalousCountOfCharacters");

	internal static string AxNoConnectionPoint => GetResourceString("AxNoConnectionPoint");

	internal static string AxNoConnectionPointContainer => GetResourceString("AxNoConnectionPointContainer");

	internal static string AxNoEventInterface => GetResourceString("AxNoEventInterface");

	internal static string AXNohWnd => GetResourceString("AXNohWnd");

	internal static string AxNoSinkAdvise => GetResourceString("AxNoSinkAdvise");

	internal static string AxNoSinkImplementation => GetResourceString("AxNoSinkImplementation");

	internal static string AxRequiresApartmentThread => GetResourceString("AxRequiresApartmentThread");

	internal static string AxWindowlessControl => GetResourceString("AxWindowlessControl");

	internal static string BadDistance => GetResourceString("BadDistance");

	internal static string BadFixedTextPosition => GetResourceString("BadFixedTextPosition");

	internal static string BadTargetArray => GetResourceString("BadTargetArray");

	internal static string BadTextPositionOrder => GetResourceString("BadTextPositionOrder");

	internal static string BamlAssemblyIdNotFound => GetResourceString("BamlAssemblyIdNotFound");

	internal static string BamlBadExtensionValue => GetResourceString("BamlBadExtensionValue");

	internal static string BamlIsNotSupportedOutsideOfApplicationResources => GetResourceString("BamlIsNotSupportedOutsideOfApplicationResources");

	internal static string BamlReaderClosed => GetResourceString("BamlReaderClosed");

	internal static string BamlReaderNoOwnerType => GetResourceString("BamlReaderNoOwnerType");

	internal static string BamlScopeError => GetResourceString("BamlScopeError");

	internal static string BamlTypeIdNotFound => GetResourceString("BamlTypeIdNotFound");

	internal static string BamlWriterBadAssembly => GetResourceString("BamlWriterBadAssembly");

	internal static string BamlWriterBadScope => GetResourceString("BamlWriterBadScope");

	internal static string BamlWriterBadStream => GetResourceString("BamlWriterBadStream");

	internal static string BamlWriterBadXmlns => GetResourceString("BamlWriterBadXmlns");

	internal static string BamlWriterClosed => GetResourceString("BamlWriterClosed");

	internal static string BamlWriterNoInElement => GetResourceString("BamlWriterNoInElement");

	internal static string BamlWriterStartDoc => GetResourceString("BamlWriterStartDoc");

	internal static string BamlWriterUnknownMarkupExtension => GetResourceString("BamlWriterUnknownMarkupExtension");

	internal static string BindingCollectionContainsNonBinding => GetResourceString("BindingCollectionContainsNonBinding");

	internal static string BindingConflict => GetResourceString("BindingConflict");

	internal static string BindingExpressionIsDetached => GetResourceString("BindingExpressionIsDetached");

	internal static string BindingExpressionStatusChanged => GetResourceString("BindingExpressionStatusChanged");

	internal static string BindingGroup_CannotChangeGroups => GetResourceString("BindingGroup_CannotChangeGroups");

	internal static string BindingGroup_NoEntry => GetResourceString("BindingGroup_NoEntry");

	internal static string BindingGroup_ValueUnavailable => GetResourceString("BindingGroup_ValueUnavailable");

	internal static string BindingListCannotCustomFilter => GetResourceString("BindingListCannotCustomFilter");

	internal static string BindingListCanOnlySortByOneProperty => GetResourceString("BindingListCanOnlySortByOneProperty");

	internal static string BufferOffsetNegative => GetResourceString("BufferOffsetNegative");

	internal static string BufferTooSmall => GetResourceString("BufferTooSmall");

	internal static string ByteRangeDownloaderNotInitialized => GetResourceString("ByteRangeDownloaderNotInitialized");

	internal static string CalendarAutomationPeer_BlackoutDayHelpText => GetResourceString("CalendarAutomationPeer_BlackoutDayHelpText");

	internal static string CalendarAutomationPeer_CalendarButtonLocalizedControlType => GetResourceString("CalendarAutomationPeer_CalendarButtonLocalizedControlType");

	internal static string CalendarAutomationPeer_DayButtonLocalizedControlType => GetResourceString("CalendarAutomationPeer_DayButtonLocalizedControlType");

	internal static string CalendarAutomationPeer_DecadeMode => GetResourceString("CalendarAutomationPeer_DecadeMode");

	internal static string CalendarAutomationPeer_MonthMode => GetResourceString("CalendarAutomationPeer_MonthMode");

	internal static string CalendarAutomationPeer_YearMode => GetResourceString("CalendarAutomationPeer_YearMode");

	internal static string CalendarCollection_MultiThreadedCollectionChangeNotSupported => GetResourceString("CalendarCollection_MultiThreadedCollectionChangeNotSupported");

	internal static string CalendarNamePropertyValueNotValid => GetResourceString("CalendarNamePropertyValueNotValid");

	internal static string Calendar_CheckSelectionMode_InvalidOperation => GetResourceString("Calendar_CheckSelectionMode_InvalidOperation");

	internal static string Calendar_NextButtonName => GetResourceString("Calendar_NextButtonName");

	internal static string Calendar_OnDisplayModePropertyChanged_InvalidValue => GetResourceString("Calendar_OnDisplayModePropertyChanged_InvalidValue");

	internal static string Calendar_OnFirstDayOfWeekChanged_InvalidValue => GetResourceString("Calendar_OnFirstDayOfWeekChanged_InvalidValue");

	internal static string Calendar_OnSelectedDateChanged_InvalidOperation => GetResourceString("Calendar_OnSelectedDateChanged_InvalidOperation");

	internal static string Calendar_OnSelectedDateChanged_InvalidValue => GetResourceString("Calendar_OnSelectedDateChanged_InvalidValue");

	internal static string Calendar_OnSelectionModeChanged_InvalidValue => GetResourceString("Calendar_OnSelectionModeChanged_InvalidValue");

	internal static string Calendar_PreviousButtonName => GetResourceString("Calendar_PreviousButtonName");

	internal static string Calendar_UnSelectableDates => GetResourceString("Calendar_UnSelectableDates");

	internal static string CancelEditNotSupported => GetResourceString("CancelEditNotSupported");

	internal static string CancelledText => GetResourceString("CancelledText");

	internal static string CancelledTitle => GetResourceString("CancelledTitle");

	internal static string CannotBeInsidePopup => GetResourceString("CannotBeInsidePopup");

	internal static string CannotBeSelfParent => GetResourceString("CannotBeSelfParent");

	internal static string CannotCallRunFromBrowserHostedApp => GetResourceString("CannotCallRunFromBrowserHostedApp");

	internal static string CannotCallRunMultipleTimes => GetResourceString("CannotCallRunMultipleTimes");

	internal static string CannotChangeAfterSealed => GetResourceString("CannotChangeAfterSealed");

	internal static string CannotChangeLiveShaping => GetResourceString("CannotChangeLiveShaping");

	internal static string CannotChangeMainWindowInBrowser => GetResourceString("CannotChangeMainWindowInBrowser");

	internal static string CannotDetermineSortByPropertiesForCollection => GetResourceString("CannotDetermineSortByPropertiesForCollection");

	internal static string CannotEditPlaceholder => GetResourceString("CannotEditPlaceholder");

	internal static string CannotFilterView => GetResourceString("CannotFilterView");

	internal static string CannotFindRemovedItem => GetResourceString("CannotFindRemovedItem");

	internal static string CannotGroupView => GetResourceString("CannotGroupView");

	internal static string CannotHaveEventHandlersInThemeStyle => GetResourceString("CannotHaveEventHandlersInThemeStyle");

	internal static string CannotHaveOverridesDefaultStyleInThemeStyle => GetResourceString("CannotHaveOverridesDefaultStyleInThemeStyle");

	internal static string CannotHavePropertyInStyle => GetResourceString("CannotHavePropertyInStyle");

	internal static string CannotHavePropertyInTemplate => GetResourceString("CannotHavePropertyInTemplate");

	internal static string CannotHookupFCERoot => GetResourceString("CannotHookupFCERoot");

	internal static string CannotInvokeScript => GetResourceString("CannotInvokeScript");

	internal static string CannotModifyLogicalChildrenDuringTreeWalk => GetResourceString("CannotModifyLogicalChildrenDuringTreeWalk");

	internal static string CannotMoveToUnknownPosition => GetResourceString("CannotMoveToUnknownPosition");

	internal static string CannotParseId => GetResourceString("CannotParseId");

	internal static string CannotProcessInkCommand => GetResourceString("CannotProcessInkCommand");

	internal static string CannotQueryPropertiesWhenPageNotInTreeWithWindow => GetResourceString("CannotQueryPropertiesWhenPageNotInTreeWithWindow");

	internal static string CannotRecyleHeterogeneousTypes => GetResourceString("CannotRecyleHeterogeneousTypes");

	internal static string CannotRemoveUnrealizedItems => GetResourceString("CannotRemoveUnrealizedItems");

	internal static string CannotSelectNotSelectableItem => GetResourceString("CannotSelectNotSelectableItem");

	internal static string CannotSerializeInvalidInstance => GetResourceString("CannotSerializeInvalidInstance");

	internal static string CannotSetNegativePosition => GetResourceString("CannotSetNegativePosition");

	internal static string CannotSetOwnerToItself => GetResourceString("CannotSetOwnerToItself");

	internal static string CannotSortView => GetResourceString("CannotSortView");

	internal static string CannotUseItemsSource => GetResourceString("CannotUseItemsSource");

	internal static string CannotWriteToReadOnly => GetResourceString("CannotWriteToReadOnly");

	internal static string CanOnlyHaveOneChild => GetResourceString("CanOnlyHaveOneChild");

	internal static string CantSetInMarkup => GetResourceString("CantSetInMarkup");

	internal static string CantSetOwnerAfterDialogIsShown => GetResourceString("CantSetOwnerAfterDialogIsShown");

	internal static string CantSetOwnerToClosedWindow => GetResourceString("CantSetOwnerToClosedWindow");

	internal static string CantSetOwnerWhosHwndIsNotCreated => GetResourceString("CantSetOwnerWhosHwndIsNotCreated");

	internal static string CantShowMBServiceWithOwner => GetResourceString("CantShowMBServiceWithOwner");

	internal static string CantShowModalOnNonInteractive => GetResourceString("CantShowModalOnNonInteractive");

	internal static string CantShowOnDifferentThread => GetResourceString("CantShowOnDifferentThread");

	internal static string CantSwitchVirtualizationModePostMeasure => GetResourceString("CantSwitchVirtualizationModePostMeasure");

	internal static string ChangeNotAllowedAfterShow => GetResourceString("ChangeNotAllowedAfterShow");

	internal static string ChangeSealedBinding => GetResourceString("ChangeSealedBinding");

	internal static string ChangingCollectionNotSupported => GetResourceString("ChangingCollectionNotSupported");

	internal static string ChangingIdNotAllowed => GetResourceString("ChangingIdNotAllowed");

	internal static string ChangingTypeNotAllowed => GetResourceString("ChangingTypeNotAllowed");

	internal static string ChildHasWrongType => GetResourceString("ChildHasWrongType");

	internal static string ChildNameMustBeNonEmpty => GetResourceString("ChildNameMustBeNonEmpty");

	internal static string ChildNameNamePatternReserved => GetResourceString("ChildNameNamePatternReserved");

	internal static string ChildTemplateInstanceDoesNotExist => GetResourceString("ChildTemplateInstanceDoesNotExist");

	internal static string ChildWindowMustHaveCorrectParent => GetResourceString("ChildWindowMustHaveCorrectParent");

	internal static string ChildWindowNotCreated => GetResourceString("ChildWindowNotCreated");

	internal static string CircularOwnerChild => GetResourceString("CircularOwnerChild");

	internal static string ClearHighlight => GetResourceString("ClearHighlight");

	internal static string ClipboardCopyMode_Disabled => GetResourceString("ClipboardCopyMode_Disabled");

	internal static string ClipToBoundsNotSupported => GetResourceString("ClipToBoundsNotSupported");

	internal static string CollectionAddEventMissingItem => GetResourceString("CollectionAddEventMissingItem");

	internal static string CollectionChangeIndexOutOfRange => GetResourceString("CollectionChangeIndexOutOfRange");

	internal static string CollectionContainerMustBeUniqueForComposite => GetResourceString("CollectionContainerMustBeUniqueForComposite");

	internal static string CollectionViewTypeIsInitOnly => GetResourceString("CollectionViewTypeIsInitOnly");

	internal static string CollectionView_MissingSynchronizationCallback => GetResourceString("CollectionView_MissingSynchronizationCallback");

	internal static string CollectionView_NameTypeDuplicity => GetResourceString("CollectionView_NameTypeDuplicity");

	internal static string CollectionView_ViewTypeInsufficient => GetResourceString("CollectionView_ViewTypeInsufficient");

	internal static string CollectionView_WrongType => GetResourceString("CollectionView_WrongType");

	internal static string Collection_NoNull => GetResourceString("Collection_NoNull");

	internal static string ColorConvertedBitmapExtensionNoSourceImage => GetResourceString("ColorConvertedBitmapExtensionNoSourceImage");

	internal static string ColorConvertedBitmapExtensionNoSourceProfile => GetResourceString("ColorConvertedBitmapExtensionNoSourceProfile");

	internal static string ColorConvertedBitmapExtensionSyntax => GetResourceString("ColorConvertedBitmapExtensionSyntax");

	internal static string CompatibilityPreferencesSealed => GetResourceString("CompatibilityPreferencesSealed");

	internal static string ComponentAlreadyInPresentationContext => GetResourceString("ComponentAlreadyInPresentationContext");

	internal static string ComponentNotInPresentationContext => GetResourceString("ComponentNotInPresentationContext");

	internal static string CompositeCollectionResetOnlyOnClear => GetResourceString("CompositeCollectionResetOnlyOnClear");

	internal static string ConditionCannotUseBothPropertyAndBinding => GetResourceString("ConditionCannotUseBothPropertyAndBinding");

	internal static string ConditionValueOfExpressionNotSupported => GetResourceString("ConditionValueOfExpressionNotSupported");

	internal static string ConditionValueOfMarkupExtensionNotSupported => GetResourceString("ConditionValueOfMarkupExtensionNotSupported");

	internal static string ContentControlCannotHaveMultipleContent => GetResourceString("ContentControlCannotHaveMultipleContent");

	internal static string ContentTypeNotSupported => GetResourceString("ContentTypeNotSupported");

	internal static string ContextMenuInDifferentDispatcher => GetResourceString("ContextMenuInDifferentDispatcher");

	internal static string CopyToNotEnoughSpace => GetResourceString("CopyToNotEnoughSpace");

	internal static string CorePropertyEnumeratorPositionedOutOfBounds => GetResourceString("CorePropertyEnumeratorPositionedOutOfBounds");

	internal static string CreateHighlight => GetResourceString("CreateHighlight");

	internal static string CreateInkNote => GetResourceString("CreateInkNote");

	internal static string CreateRootPopup_ChildHasLogicalParent => GetResourceString("CreateRootPopup_ChildHasLogicalParent");

	internal static string CreateRootPopup_ChildHasVisualParent => GetResourceString("CreateRootPopup_ChildHasVisualParent");

	internal static string CreateTextNote => GetResourceString("CreateTextNote");

	internal static string CrossThreadAccessOfUnshareableFreezable => GetResourceString("CrossThreadAccessOfUnshareableFreezable");

	internal static string CustomContentStateMustBeSerializable => GetResourceString("CustomContentStateMustBeSerializable");

	internal static string CustomDictionaryFailedToLoadDictionaryUri => GetResourceString("CustomDictionaryFailedToLoadDictionaryUri");

	internal static string CustomDictionaryFailedToUnLoadDictionaryUri => GetResourceString("CustomDictionaryFailedToUnLoadDictionaryUri");

	internal static string CustomDictionaryItemAlreadyExists => GetResourceString("CustomDictionaryItemAlreadyExists");

	internal static string CustomDictionaryNullItem => GetResourceString("CustomDictionaryNullItem");

	internal static string CustomDictionarySourcesUnsupportedURI => GetResourceString("CustomDictionarySourcesUnsupportedURI");

	internal static string CyclicStyleReferenceDetected => GetResourceString("CyclicStyleReferenceDetected");

	internal static string CyclicThemeStyleReferenceDetected => GetResourceString("CyclicThemeStyleReferenceDetected");

	internal static string DataGridCellItemAutomationPeer_LocalizedControlType => GetResourceString("DataGridCellItemAutomationPeer_LocalizedControlType");

	internal static string DataGridCellItemAutomationPeer_NameCoreFormat => GetResourceString("DataGridCellItemAutomationPeer_NameCoreFormat");

	internal static string DataGridColumnHeaderItemAutomationPeer_NameCoreFormat => GetResourceString("DataGridColumnHeaderItemAutomationPeer_NameCoreFormat");

	internal static string DataGridColumnHeaderItemAutomationPeer_Unresizable => GetResourceString("DataGridColumnHeaderItemAutomationPeer_Unresizable");

	internal static string DataGridColumnHeaderItemAutomationPeer_Unsupported => GetResourceString("DataGridColumnHeaderItemAutomationPeer_Unsupported");

	internal static string DataGridLength_Infinity => GetResourceString("DataGridLength_Infinity");

	internal static string DataGridLength_InvalidType => GetResourceString("DataGridLength_InvalidType");

	internal static string DataGridRow_CannotSelectRowWhenCells => GetResourceString("DataGridRow_CannotSelectRowWhenCells");

	internal static string DataGrid_AutomationInvokeFailed => GetResourceString("DataGrid_AutomationInvokeFailed");

	internal static string DataGrid_CannotSelectCell => GetResourceString("DataGrid_CannotSelectCell");

	internal static string DataGrid_ColumnDisplayIndexOutOfRange => GetResourceString("DataGrid_ColumnDisplayIndexOutOfRange");

	internal static string DataGrid_ColumnIndexOutOfRange => GetResourceString("DataGrid_ColumnIndexOutOfRange");

	internal static string DataGrid_ColumnIsReadOnly => GetResourceString("DataGrid_ColumnIsReadOnly");

	internal static string DataGrid_DisplayIndexOutOfRange => GetResourceString("DataGrid_DisplayIndexOutOfRange");

	internal static string DataGrid_DuplicateDisplayIndex => GetResourceString("DataGrid_DuplicateDisplayIndex");

	internal static string DataGrid_InvalidColumnReuse => GetResourceString("DataGrid_InvalidColumnReuse");

	internal static string DataGrid_InvalidSortDescription => GetResourceString("DataGrid_InvalidSortDescription");

	internal static string DataGrid_NewColumnInvalidDisplayIndex => GetResourceString("DataGrid_NewColumnInvalidDisplayIndex");

	internal static string DataGrid_NullColumn => GetResourceString("DataGrid_NullColumn");

	internal static string DataGrid_ProbableInvalidSortDescription => GetResourceString("DataGrid_ProbableInvalidSortDescription");

	internal static string DataGrid_ReadonlyCellsItemsSource => GetResourceString("DataGrid_ReadonlyCellsItemsSource");

	internal static string DataTypeCannotBeObject => GetResourceString("DataTypeCannotBeObject");

	internal static string DatePickerAutomationPeer_LocalizedControlType => GetResourceString("DatePickerAutomationPeer_LocalizedControlType");

	internal static string DatePickerTextBox_DefaultWatermarkText => GetResourceString("DatePickerTextBox_DefaultWatermarkText");

	internal static string DatePickerTextBox_TemplatePartIsOfIncorrectType => GetResourceString("DatePickerTextBox_TemplatePartIsOfIncorrectType");

	internal static string DatePicker_DropDownButtonName => GetResourceString("DatePicker_DropDownButtonName");

	internal static string DatePicker_OnSelectedDateFormatChanged_InvalidValue => GetResourceString("DatePicker_OnSelectedDateFormatChanged_InvalidValue");

	internal static string DatePicker_WatermarkText => GetResourceString("DatePicker_WatermarkText");

	internal static string DeferringLoaderNoContext => GetResourceString("DeferringLoaderNoContext");

	internal static string DeferringLoaderNoSave => GetResourceString("DeferringLoaderNoSave");

	internal static string DeferSelectionActive => GetResourceString("DeferSelectionActive");

	internal static string DeferSelectionNotActive => GetResourceString("DeferSelectionNotActive");

	internal static string DeleteAnnotations => GetResourceString("DeleteAnnotations");

	internal static string DeleteNotes => GetResourceString("DeleteNotes");

	internal static string DeployText => GetResourceString("DeployText");

	internal static string DeployTitle => GetResourceString("DeployTitle");

	internal static string DesignerMetadata_CustomCategory_Accessibility => GetResourceString("DesignerMetadata_CustomCategory_Accessibility");

	internal static string DesignerMetadata_CustomCategory_Content => GetResourceString("DesignerMetadata_CustomCategory_Content");

	internal static string DesignerMetadata_CustomCategory_Navigation => GetResourceString("DesignerMetadata_CustomCategory_Navigation");

	internal static string DialogResultMustBeSetAfterShowDialog => GetResourceString("DialogResultMustBeSetAfterShowDialog");

	internal static string DisplayMemberPathAndItemTemplateDefined => GetResourceString("DisplayMemberPathAndItemTemplateDefined");

	internal static string DisplayMemberPathAndItemTemplateSelectorDefined => GetResourceString("DisplayMemberPathAndItemTemplateSelectorDefined");

	internal static string DocumentApplicationCannotInitializeUI => GetResourceString("DocumentApplicationCannotInitializeUI");

	internal static string DocumentApplicationContextMenuFirstPageInputGesture => GetResourceString("DocumentApplicationContextMenuFirstPageInputGesture");

	internal static string DocumentApplicationContextMenuLastPageInputGesture => GetResourceString("DocumentApplicationContextMenuLastPageInputGesture");

	internal static string DocumentApplicationContextMenuNextPageHeader => GetResourceString("DocumentApplicationContextMenuNextPageHeader");

	internal static string DocumentApplicationContextMenuNextPageInputGesture => GetResourceString("DocumentApplicationContextMenuNextPageInputGesture");

	internal static string DocumentApplicationContextMenuPreviousPageHeader => GetResourceString("DocumentApplicationContextMenuPreviousPageHeader");

	internal static string DocumentApplicationContextMenuPreviousPageInputGesture => GetResourceString("DocumentApplicationContextMenuPreviousPageInputGesture");

	internal static string DocumentApplicationNotInFullTrust => GetResourceString("DocumentApplicationNotInFullTrust");

	internal static string DocumentApplicationRegistryKeyNotFound => GetResourceString("DocumentApplicationRegistryKeyNotFound");

	internal static string DocumentApplicationStatusLoaded => GetResourceString("DocumentApplicationStatusLoaded");

	internal static string DocumentApplicationUnableToOpenDocument => GetResourceString("DocumentApplicationUnableToOpenDocument");

	internal static string DocumentApplicationUnknownFileFormat => GetResourceString("DocumentApplicationUnknownFileFormat");

	internal static string DocumentGridInvalidViewMode => GetResourceString("DocumentGridInvalidViewMode");

	internal static string DocumentGridVisualTreeContainsNonBorderAsFirstElement => GetResourceString("DocumentGridVisualTreeContainsNonBorderAsFirstElement");

	internal static string DocumentGridVisualTreeContainsNonDocumentGridPage => GetResourceString("DocumentGridVisualTreeContainsNonDocumentGridPage");

	internal static string DocumentGridVisualTreeContainsNonUIElement => GetResourceString("DocumentGridVisualTreeContainsNonUIElement");

	internal static string DocumentGridVisualTreeOutOfSync => GetResourceString("DocumentGridVisualTreeOutOfSync");

	internal static string DocumentPageView_ParentNotDocumentPageHost => GetResourceString("DocumentPageView_ParentNotDocumentPageHost");

	internal static string DocumentReadOnly => GetResourceString("DocumentReadOnly");

	internal static string DocumentReferenceHasInvalidDocument => GetResourceString("DocumentReferenceHasInvalidDocument");

	internal static string DocumentReferenceNotFound => GetResourceString("DocumentReferenceNotFound");

	internal static string DocumentReferenceUnsupportedMimeType => GetResourceString("DocumentReferenceUnsupportedMimeType");

	internal static string DocumentStructureUnexpectedParameterType1 => GetResourceString("DocumentStructureUnexpectedParameterType1");

	internal static string DocumentStructureUnexpectedParameterType2 => GetResourceString("DocumentStructureUnexpectedParameterType2");

	internal static string DocumentStructureUnexpectedParameterType4 => GetResourceString("DocumentStructureUnexpectedParameterType4");

	internal static string DocumentStructureUnexpectedParameterType6 => GetResourceString("DocumentStructureUnexpectedParameterType6");

	internal static string DocumentViewerArgumentMustBeInteger => GetResourceString("DocumentViewerArgumentMustBeInteger");

	internal static string DocumentViewerArgumentMustBePercentage => GetResourceString("DocumentViewerArgumentMustBePercentage");

	internal static string DocumentViewerCanHaveOnlyOneChild => GetResourceString("DocumentViewerCanHaveOnlyOneChild");

	internal static string DocumentViewerChildMustImplementIDocumentPaginatorSource => GetResourceString("DocumentViewerChildMustImplementIDocumentPaginatorSource");

	internal static string DocumentViewerOneMasterPage => GetResourceString("DocumentViewerOneMasterPage");

	internal static string DocumentViewerOnlySupportsFixedDocumentSequence => GetResourceString("DocumentViewerOnlySupportsFixedDocumentSequence");

	internal static string DocumentViewerPageViewsCollectionEmpty => GetResourceString("DocumentViewerPageViewsCollectionEmpty");

	internal static string DocumentViewerSearchCompleteTitle => GetResourceString("DocumentViewerSearchCompleteTitle");

	internal static string DocumentViewerSearchDownCompleteLabel => GetResourceString("DocumentViewerSearchDownCompleteLabel");

	internal static string DocumentViewerSearchUpCompleteLabel => GetResourceString("DocumentViewerSearchUpCompleteLabel");

	internal static string DocumentViewerStyleMustIncludeContentHost => GetResourceString("DocumentViewerStyleMustIncludeContentHost");

	internal static string DocumentViewerViewFitToHeightCommandText => GetResourceString("DocumentViewerViewFitToHeightCommandText");

	internal static string DocumentViewerViewFitToMaxPagesAcrossCommandText => GetResourceString("DocumentViewerViewFitToMaxPagesAcrossCommandText");

	internal static string DocumentViewerViewFitToWidthCommandText => GetResourceString("DocumentViewerViewFitToWidthCommandText");

	internal static string DocumentViewerViewThumbnailsCommandText => GetResourceString("DocumentViewerViewThumbnailsCommandText");

	internal static string DownloadText => GetResourceString("DownloadText");

	internal static string DownloadTitle => GetResourceString("DownloadTitle");

	internal static string DragMoveFail => GetResourceString("DragMoveFail");

	internal static string DuplicatedCompatibleUri => GetResourceString("DuplicatedCompatibleUri");

	internal static string DuplicatedUri => GetResourceString("DuplicatedUri");

	internal static string DuplicatesNotAllowed => GetResourceString("DuplicatesNotAllowed");

	internal static string ElementMustBeInPopup => GetResourceString("ElementMustBeInPopup");

	internal static string ElementMustBelongToTemplate => GetResourceString("ElementMustBelongToTemplate");

	internal static string EmptySelectionNotSupported => GetResourceString("EmptySelectionNotSupported");

	internal static string EndInitWithoutBeginInitNotSupported => GetResourceString("EndInitWithoutBeginInitNotSupported");

	internal static string EntryAssemblyIsNull => GetResourceString("EntryAssemblyIsNull");

	internal static string EnumeratorCollectionDisposed => GetResourceString("EnumeratorCollectionDisposed");

	internal static string EnumeratorInvalidOperation => GetResourceString("EnumeratorInvalidOperation");

	internal static string EnumeratorNotStarted => GetResourceString("EnumeratorNotStarted");

	internal static string EnumeratorReachedEnd => GetResourceString("EnumeratorReachedEnd");

	internal static string EnumeratorVersionChanged => GetResourceString("EnumeratorVersionChanged");

	internal static string EventTriggerBadAction => GetResourceString("EventTriggerBadAction");

	internal static string EventTriggerDoesNotEnterExit => GetResourceString("EventTriggerDoesNotEnterExit");

	internal static string EventTriggerDoNotSetProperties => GetResourceString("EventTriggerDoNotSetProperties");

	internal static string EventTriggerEventUnresolvable => GetResourceString("EventTriggerEventUnresolvable");

	internal static string EventTriggerNeedEvent => GetResourceString("EventTriggerNeedEvent");

	internal static string EventTriggerOnStyleNotAllowedToHaveTarget => GetResourceString("EventTriggerOnStyleNotAllowedToHaveTarget");

	internal static string EventTriggerTargetNameUnresolvable => GetResourceString("EventTriggerTargetNameUnresolvable");

	internal static string ExceptionInGetPage => GetResourceString("ExceptionInGetPage");

	internal static string ExceptionValidationRuleValidateNotSupported => GetResourceString("ExceptionValidationRuleValidateNotSupported");

	internal static string ExpectedBamlSchemaContext => GetResourceString("ExpectedBamlSchemaContext");

	internal static string ExpectedBinaryContent => GetResourceString("ExpectedBinaryContent");

	internal static string ExpectedResourceDictionaryTarget => GetResourceString("ExpectedResourceDictionaryTarget");

	internal static string FailedResumePageFunction => GetResourceString("FailedResumePageFunction");

	internal static string FailedToConvertResource => GetResourceString("FailedToConvertResource");

	internal static string FailToLaunchDefaultBrowser => GetResourceString("FailToLaunchDefaultBrowser");

	internal static string FailToNavigateUsingHyperlinkTarget => GetResourceString("FailToNavigateUsingHyperlinkTarget");

	internal static string FileDialogCreatePrompt => GetResourceString("FileDialogCreatePrompt");

	internal static string FileDialogFileNotFound => GetResourceString("FileDialogFileNotFound");

	internal static string FileDialogInvalidFilter => GetResourceString("FileDialogInvalidFilter");

	internal static string FileDialogInvalidFilterIndex => GetResourceString("FileDialogInvalidFilterIndex");

	internal static string FileDialogOverwritePrompt => GetResourceString("FileDialogOverwritePrompt");

	internal static string FileNameMustNotBeNull => GetResourceString("FileNameMustNotBeNull");

	internal static string FileNameNullOrEmpty => GetResourceString("FileNameNullOrEmpty");

	internal static string FileToFilterNotLoaded => GetResourceString("FileToFilterNotLoaded");

	internal static string FilterBindRegionNotImplemented => GetResourceString("FilterBindRegionNotImplemented");

	internal static string FilterEndOfChunks => GetResourceString("FilterEndOfChunks");

	internal static string FilterGetChunkNoStream => GetResourceString("FilterGetChunkNoStream");

	internal static string FilterGetTextBufferOverflow => GetResourceString("FilterGetTextBufferOverflow");

	internal static string FilterGetTextNotSupported => GetResourceString("FilterGetTextNotSupported");

	internal static string FilterGetValueAlreadyCalledOnCurrentChunk => GetResourceString("FilterGetValueAlreadyCalledOnCurrentChunk");

	internal static string FilterGetValueMustBeStringOrDateTime => GetResourceString("FilterGetValueMustBeStringOrDateTime");

	internal static string FilterGetValueNotSupported => GetResourceString("FilterGetValueNotSupported");

	internal static string FilterInitInvalidAttributes => GetResourceString("FilterInitInvalidAttributes");

	internal static string FilterIPersistFileIsReadOnly => GetResourceString("FilterIPersistFileIsReadOnly");

	internal static string FilterIPersistStreamIsReadOnly => GetResourceString("FilterIPersistStreamIsReadOnly");

	internal static string FilterLoadInvalidModeFlag => GetResourceString("FilterLoadInvalidModeFlag");

	internal static string FilterNullGetTextBufferPointer => GetResourceString("FilterNullGetTextBufferPointer");

	internal static string FilterPropSpecUnknownUnionSelector => GetResourceString("FilterPropSpecUnknownUnionSelector");

	internal static string FixedDocumentExpectsDependencyObject => GetResourceString("FixedDocumentExpectsDependencyObject");

	internal static string FixedDocumentReadonly => GetResourceString("FixedDocumentReadonly");

	internal static string FlowDocumentFormattingReentrancy => GetResourceString("FlowDocumentFormattingReentrancy");

	internal static string FlowDocumentInvalidContnetChange => GetResourceString("FlowDocumentInvalidContnetChange");

	internal static string FlowDocumentPageViewerOnlySupportsFlowDocument => GetResourceString("FlowDocumentPageViewerOnlySupportsFlowDocument");

	internal static string FlowDocumentReaderCanHaveOnlyOneChild => GetResourceString("FlowDocumentReaderCanHaveOnlyOneChild");

	internal static string FlowDocumentReaderCannotDisableAllViewingModes => GetResourceString("FlowDocumentReaderCannotDisableAllViewingModes");

	internal static string FlowDocumentReaderDecoratorMarkedAsContentHostMustHaveNoContent => GetResourceString("FlowDocumentReaderDecoratorMarkedAsContentHostMustHaveNoContent");

	internal static string FlowDocumentReaderViewingModeEnabledConflict => GetResourceString("FlowDocumentReaderViewingModeEnabledConflict");

	internal static string FlowDocumentReader_MultipleViewProvider_PageViewName => GetResourceString("FlowDocumentReader_MultipleViewProvider_PageViewName");

	internal static string FlowDocumentReader_MultipleViewProvider_ScrollViewName => GetResourceString("FlowDocumentReader_MultipleViewProvider_ScrollViewName");

	internal static string FlowDocumentReader_MultipleViewProvider_TwoPageViewName => GetResourceString("FlowDocumentReader_MultipleViewProvider_TwoPageViewName");

	internal static string FlowDocumentScrollViewerCanHaveOnlyOneChild => GetResourceString("FlowDocumentScrollViewerCanHaveOnlyOneChild");

	internal static string FlowDocumentScrollViewerDocumentBelongsToAnotherFlowDocumentScrollViewerAlready => GetResourceString("FlowDocumentScrollViewerDocumentBelongsToAnotherFlowDocumentScrollViewerAlready");

	internal static string FlowDocumentScrollViewerMarkedAsContentHostMustHaveNoContent => GetResourceString("FlowDocumentScrollViewerMarkedAsContentHostMustHaveNoContent");

	internal static string FormatRestrictionsExceeded => GetResourceString("FormatRestrictionsExceeded");

	internal static string FrameNoAddChild => GetResourceString("FrameNoAddChild");

	internal static string FrameworkElementFactoryAlreadyParented => GetResourceString("FrameworkElementFactoryAlreadyParented");

	internal static string FrameworkElementFactoryCannotAddText => GetResourceString("FrameworkElementFactoryCannotAddText");

	internal static string FrameworkElementFactoryMustBeSealed => GetResourceString("FrameworkElementFactoryMustBeSealed");

	internal static string GenerationInProgress => GetResourceString("GenerationInProgress");

	internal static string GenerationNotInProgress => GetResourceString("GenerationNotInProgress");

	internal static string Generator_CountIsWrong => GetResourceString("Generator_CountIsWrong");

	internal static string Generator_Inconsistent => GetResourceString("Generator_Inconsistent");

	internal static string Generator_ItemIsWrong => GetResourceString("Generator_ItemIsWrong");

	internal static string Generator_MoreErrors => GetResourceString("Generator_MoreErrors");

	internal static string Generator_Readme0 => GetResourceString("Generator_Readme0");

	internal static string Generator_Readme1 => GetResourceString("Generator_Readme1");

	internal static string Generator_Readme2 => GetResourceString("Generator_Readme2");

	internal static string Generator_Readme3 => GetResourceString("Generator_Readme3");

	internal static string Generator_Readme4 => GetResourceString("Generator_Readme4");

	internal static string Generator_Readme5 => GetResourceString("Generator_Readme5");

	internal static string Generator_Readme6 => GetResourceString("Generator_Readme6");

	internal static string Generator_Readme7 => GetResourceString("Generator_Readme7");

	internal static string Generator_Readme8 => GetResourceString("Generator_Readme8");

	internal static string Generator_Readme9 => GetResourceString("Generator_Readme9");

	internal static string Generator_Unnamed => GetResourceString("Generator_Unnamed");

	internal static string GetResponseFailed => GetResourceString("GetResponseFailed");

	internal static string GetStreamFailed => GetResourceString("GetStreamFailed");

	internal static string GlyphsAdvanceWidthCannotBeNegative => GetResourceString("GlyphsAdvanceWidthCannotBeNegative");

	internal static string GlyphsCaretStopsContainsHexDigits => GetResourceString("GlyphsCaretStopsContainsHexDigits");

	internal static string GlyphsCaretStopsLengthCorrespondsToUnicodeString => GetResourceString("GlyphsCaretStopsLengthCorrespondsToUnicodeString");

	internal static string GlyphsClusterBadCharactersBeforeBracket => GetResourceString("GlyphsClusterBadCharactersBeforeBracket");

	internal static string GlyphsClusterMisplacedSeparator => GetResourceString("GlyphsClusterMisplacedSeparator");

	internal static string GlyphsClusterNoMatchingBracket => GetResourceString("GlyphsClusterNoMatchingBracket");

	internal static string GlyphsClusterNoNestedClusters => GetResourceString("GlyphsClusterNoNestedClusters");

	internal static string GlyphsIndexRequiredIfNoUnicode => GetResourceString("GlyphsIndexRequiredIfNoUnicode");

	internal static string GlyphsIndexRequiredWithinCluster => GetResourceString("GlyphsIndexRequiredWithinCluster");

	internal static string GlyphsTooManyCommas => GetResourceString("GlyphsTooManyCommas");

	internal static string GlyphsUnicodeStringAndIndicesCannotBothBeEmpty => GetResourceString("GlyphsUnicodeStringAndIndicesCannotBothBeEmpty");

	internal static string GlyphsUnicodeStringIsTooShort => GetResourceString("GlyphsUnicodeStringIsTooShort");

	internal static string GridCollection_CannotModifyReadOnly => GetResourceString("GridCollection_CannotModifyReadOnly");

	internal static string GridCollection_DestArrayInvalidLength => GetResourceString("GridCollection_DestArrayInvalidLength");

	internal static string GridCollection_DestArrayInvalidLowerBound => GetResourceString("GridCollection_DestArrayInvalidLowerBound");

	internal static string GridCollection_DestArrayInvalidRank => GetResourceString("GridCollection_DestArrayInvalidRank");

	internal static string GridCollection_InOtherCollection => GetResourceString("GridCollection_InOtherCollection");

	internal static string GridCollection_MustBeCertainType => GetResourceString("GridCollection_MustBeCertainType");

	internal static string Grid_UnexpectedParameterType => GetResourceString("Grid_UnexpectedParameterType");

	internal static string HandlerTypeIllegal => GetResourceString("HandlerTypeIllegal");

	internal static string HasLogicalParent => GetResourceString("HasLogicalParent");

	internal static string HostedWindowMustBeAChildWindow => GetResourceString("HostedWindowMustBeAChildWindow");

	internal static string HostingStatusCancelled => GetResourceString("HostingStatusCancelled");

	internal static string HostingStatusDownloadApp => GetResourceString("HostingStatusDownloadApp");

	internal static string HostingStatusDownloadAppInfo => GetResourceString("HostingStatusDownloadAppInfo");

	internal static string HostingStatusFailed => GetResourceString("HostingStatusFailed");

	internal static string HostingStatusPreparingToRun => GetResourceString("HostingStatusPreparingToRun");

	internal static string HostingStatusVerifying => GetResourceString("HostingStatusVerifying");

	internal static string HwndHostDoesNotSupportChildKeyboardSinks => GetResourceString("HwndHostDoesNotSupportChildKeyboardSinks");

	internal static string HyperLinkTargetNotFound => GetResourceString("HyperLinkTargetNotFound");

	internal static string HyphenatorDisposed => GetResourceString("HyphenatorDisposed");

	internal static string IconMustBeBitmapFrame => GetResourceString("IconMustBeBitmapFrame");

	internal static string IDPInvalidContentPosition => GetResourceString("IDPInvalidContentPosition");

	internal static string IDPNegativePageNumber => GetResourceString("IDPNegativePageNumber");

	internal static string IllegalTreeChangeDetected => GetResourceString("IllegalTreeChangeDetected");

	internal static string IllegalTreeChangeDetectedPostAction => GetResourceString("IllegalTreeChangeDetectedPostAction");

	internal static string Illegal_InheritanceBehaviorSettor => GetResourceString("Illegal_InheritanceBehaviorSettor");

	internal static string ImplementOtherMembersWithSort => GetResourceString("ImplementOtherMembersWithSort");

	internal static string InavalidStartItem => GetResourceString("InavalidStartItem");

	internal static string IncompatibleCLRText => GetResourceString("IncompatibleCLRText");

	internal static string IncompatibleWinFXText => GetResourceString("IncompatibleWinFXText");

	internal static string InconsistentBindingList => GetResourceString("InconsistentBindingList");

	internal static string IncorrectAnchorLength => GetResourceString("IncorrectAnchorLength");

	internal static string IncorrectFlowDirection => GetResourceString("IncorrectFlowDirection");

	internal static string IncorrectLocatorPartType => GetResourceString("IncorrectLocatorPartType");

	internal static string IndexedPropDescNotImplemented => GetResourceString("IndexedPropDescNotImplemented");

	internal static string InDifferentParagraphs => GetResourceString("InDifferentParagraphs");

	internal static string InDifferentScope => GetResourceString("InDifferentScope");

	internal static string InDifferentTextContainers => GetResourceString("InDifferentTextContainers");

	internal static string InkCanvasDeselectKeyDisplayString => GetResourceString("InkCanvasDeselectKeyDisplayString");

	internal static string InputScopeAttribute_E_OUTOFMEMORY => GetResourceString("InputScopeAttribute_E_OUTOFMEMORY");

	internal static string InputStreamMustBeReadable => GetResourceString("InputStreamMustBeReadable");

	internal static string InsertInDeferSelectionActive => GetResourceString("InsertInDeferSelectionActive");

	internal static string IntegerCollectionLengthLessThanZero => GetResourceString("IntegerCollectionLengthLessThanZero");

	internal static string InvalidAnchorPosition => GetResourceString("InvalidAnchorPosition");

	internal static string InvalidAttachedAnchor => GetResourceString("InvalidAttachedAnchor");

	internal static string InvalidAttachedAnnotation => GetResourceString("InvalidAttachedAnnotation");

	internal static string InvalidAttributeValue => GetResourceString("InvalidAttributeValue");

	internal static string InvalidByteRanges => GetResourceString("InvalidByteRanges");

	internal static string InvalidClipboardFormat => GetResourceString("InvalidClipboardFormat");

	internal static string InvalidCompositionTarget => GetResourceString("InvalidCompositionTarget");

	internal static string InvalidControlTemplateTargetType => GetResourceString("InvalidControlTemplateTargetType");

	internal static string InvalidCtorParameterNoInfinity => GetResourceString("InvalidCtorParameterNoInfinity");

	internal static string InvalidCtorParameterNoNaN => GetResourceString("InvalidCtorParameterNoNaN");

	internal static string InvalidCtorParameterNoNegative => GetResourceString("InvalidCtorParameterNoNegative");

	internal static string InvalidCtorParameterUnknownFigureUnitType => GetResourceString("InvalidCtorParameterUnknownFigureUnitType");

	internal static string InvalidCtorParameterUnknownGridUnitType => GetResourceString("InvalidCtorParameterUnknownGridUnitType");

	internal static string InvalidCtorParameterUnknownVirtualizationCacheLengthUnitType => GetResourceString("InvalidCtorParameterUnknownVirtualizationCacheLengthUnitType");

	internal static string InvalidCustomSerialize => GetResourceString("InvalidCustomSerialize");

	internal static string InvalidDeployText => GetResourceString("InvalidDeployText");

	internal static string InvalidDeployTitle => GetResourceString("InvalidDeployTitle");

	internal static string InvalidDeSerialize => GetResourceString("InvalidDeSerialize");

	internal static string InvalidDiameter => GetResourceString("InvalidDiameter");

	internal static string InvalidDSContentType => GetResourceString("InvalidDSContentType");

	internal static string InvalidEmptyArray => GetResourceString("InvalidEmptyArray");

	internal static string InvalidEmptyStrokeCollection => GetResourceString("InvalidEmptyStrokeCollection");

	internal static string InvalidEndOfBaml => GetResourceString("InvalidEndOfBaml");

	internal static string InvalidEventHandle => GetResourceString("InvalidEventHandle");

	internal static string InvalidGuid => GetResourceString("InvalidGuid");

	internal static string InvalidHighlightColor => GetResourceString("InvalidHighlightColor");

	internal static string InvalidInkForeground => GetResourceString("InvalidInkForeground");

	internal static string InvalidItemContainer => GetResourceString("InvalidItemContainer");

	internal static string InvalidLocalizabilityValue => GetResourceString("InvalidLocalizabilityValue");

	internal static string InvalidLocatorPart => GetResourceString("InvalidLocatorPart");

	internal static string InvalidLocCommentTarget => GetResourceString("InvalidLocCommentTarget");

	internal static string InvalidLocCommentValue => GetResourceString("InvalidLocCommentValue");

	internal static string InvalidNamespace => GetResourceString("InvalidNamespace");

	internal static string InvalidOperationDuringClosing => GetResourceString("InvalidOperationDuringClosing");

	internal static string InvalidOperation_AddBackEntryNoContent => GetResourceString("InvalidOperation_AddBackEntryNoContent");

	internal static string InvalidOperation_CannotClearFwdStack => GetResourceString("InvalidOperation_CannotClearFwdStack");

	internal static string InvalidOperation_CannotReenterPageFunction => GetResourceString("InvalidOperation_CannotReenterPageFunction");

	internal static string InvalidOperation_CantChangeJournalOwnership => GetResourceString("InvalidOperation_CantChangeJournalOwnership");

	internal static string InvalidOperation_IComparerFailed => GetResourceString("InvalidOperation_IComparerFailed");

	internal static string InvalidOperation_MustImplementIPCCSOrHandleNavigating => GetResourceString("InvalidOperation_MustImplementIPCCSOrHandleNavigating");

	internal static string InvalidOperation_NoJournal => GetResourceString("InvalidOperation_NoJournal");

	internal static string InvalidPageFunctionType => GetResourceString("InvalidPageFunctionType");

	internal static string InvalidPoint => GetResourceString("InvalidPoint");

	internal static string InvalidPropertyValue => GetResourceString("InvalidPropertyValue");

	internal static string InvalidSelectionPages => GetResourceString("InvalidSelectionPages");

	internal static string InvalidSetterValue => GetResourceString("InvalidSetterValue");

	internal static string InvalidSFContentType => GetResourceString("InvalidSFContentType");

	internal static string InvalidStartNodeForTextSelection => GetResourceString("InvalidStartNodeForTextSelection");

	internal static string InvalidStartOfBaml => GetResourceString("InvalidStartOfBaml");

	internal static string InvalidStickyNoteAnnotation => GetResourceString("InvalidStickyNoteAnnotation");

	internal static string InvalidStickyNoteTemplate => GetResourceString("InvalidStickyNoteTemplate");

	internal static string InvalidStoryFragmentsMarkup => GetResourceString("InvalidStoryFragmentsMarkup");

	internal static string InvalidStringCornerRadius => GetResourceString("InvalidStringCornerRadius");

	internal static string InvalidStringThickness => GetResourceString("InvalidStringThickness");

	internal static string InvalidStringVirtualizationCacheLength => GetResourceString("InvalidStringVirtualizationCacheLength");

	internal static string InvalidSubTreeProcessor => GetResourceString("InvalidSubTreeProcessor");

	internal static string InvalidTempFileName => GetResourceString("InvalidTempFileName");

	internal static string InvalidValueForTopLeft => GetResourceString("InvalidValueForTopLeft");

	internal static string InvalidValueSpecified => GetResourceString("InvalidValueSpecified");

	internal static string InvalidXmlContent => GetResourceString("InvalidXmlContent");

	internal static string ItemCollectionHasNoCollection => GetResourceString("ItemCollectionHasNoCollection");

	internal static string ItemCollectionRemoveArgumentOutOfRange => GetResourceString("ItemCollectionRemoveArgumentOutOfRange");

	internal static string ItemCollectionShouldUseInnerSyncRoot => GetResourceString("ItemCollectionShouldUseInnerSyncRoot");

	internal static string ItemsControl_ParentNotFrameworkElement => GetResourceString("ItemsControl_ParentNotFrameworkElement");

	internal static string ItemsPanelNotAPanel => GetResourceString("ItemsPanelNotAPanel");

	internal static string ItemsPanelNotSingleNode => GetResourceString("ItemsPanelNotSingleNode");

	internal static string ItemsSourceInUse => GetResourceString("ItemsSourceInUse");

	internal static string ItemTemplateSelectorBreaksDisplayMemberPath => GetResourceString("ItemTemplateSelectorBreaksDisplayMemberPath");

	internal static string JumpItemsRejectedEventArgs_CountMismatch => GetResourceString("JumpItemsRejectedEventArgs_CountMismatch");

	internal static string JumpList_CantApplyUntilEndInit => GetResourceString("JumpList_CantApplyUntilEndInit");

	internal static string JumpList_CantCallUnbalancedEndInit => GetResourceString("JumpList_CantCallUnbalancedEndInit");

	internal static string JumpList_CantNestBeginInitCalls => GetResourceString("JumpList_CantNestBeginInitCalls");

	internal static string KeyAlignCenterDisplayString => GetResourceString("KeyAlignCenterDisplayString");

	internal static string KeyAlignJustifyDisplayString => GetResourceString("KeyAlignJustifyDisplayString");

	internal static string KeyAlignLeftDisplayString => GetResourceString("KeyAlignLeftDisplayString");

	internal static string KeyAlignRightDisplayString => GetResourceString("KeyAlignRightDisplayString");

	internal static string KeyAltUndoDisplayString => GetResourceString("KeyAltUndoDisplayString");

	internal static string KeyApplyBackground => GetResourceString("KeyApplyBackground");

	internal static string KeyApplyBackgroundDisplayString => GetResourceString("KeyApplyBackgroundDisplayString");

	internal static string KeyApplyDoubleSpaceDisplayString => GetResourceString("KeyApplyDoubleSpaceDisplayString");

	internal static string KeyApplyFontFamily => GetResourceString("KeyApplyFontFamily");

	internal static string KeyApplyFontFamilyDisplayString => GetResourceString("KeyApplyFontFamilyDisplayString");

	internal static string KeyApplyFontSize => GetResourceString("KeyApplyFontSize");

	internal static string KeyApplyFontSizeDisplayString => GetResourceString("KeyApplyFontSizeDisplayString");

	internal static string KeyApplyForeground => GetResourceString("KeyApplyForeground");

	internal static string KeyApplyForegroundDisplayString => GetResourceString("KeyApplyForegroundDisplayString");

	internal static string KeyApplyOneAndAHalfSpaceDisplayString => GetResourceString("KeyApplyOneAndAHalfSpaceDisplayString");

	internal static string KeyApplySingleSpaceDisplayString => GetResourceString("KeyApplySingleSpaceDisplayString");

	internal static string KeyBackspaceDisplayString => GetResourceString("KeyBackspaceDisplayString");

	internal static string KeyCollectionHasInvalidKey => GetResourceString("KeyCollectionHasInvalidKey");

	internal static string KeyCopyDisplayString => GetResourceString("KeyCopyDisplayString");

	internal static string KeyCopyFormatDisplayString => GetResourceString("KeyCopyFormatDisplayString");

	internal static string KeyCorrectionList => GetResourceString("KeyCorrectionList");

	internal static string KeyCorrectionListDisplayString => GetResourceString("KeyCorrectionListDisplayString");

	internal static string KeyCtrlInsertDisplayString => GetResourceString("KeyCtrlInsertDisplayString");

	internal static string KeyCutDisplayString => GetResourceString("KeyCutDisplayString");

	internal static string KeyDecreaseFontSizeDisplayString => GetResourceString("KeyDecreaseFontSizeDisplayString");

	internal static string KeyDecreaseIndentationDisplayString => GetResourceString("KeyDecreaseIndentationDisplayString");

	internal static string KeyDeleteColumnsDisplayString => GetResourceString("KeyDeleteColumnsDisplayString");

	internal static string KeyDeleteDisplayString => GetResourceString("KeyDeleteDisplayString");

	internal static string KeyDeleteNextWordDisplayString => GetResourceString("KeyDeleteNextWordDisplayString");

	internal static string KeyDeletePreviousWordDisplayString => GetResourceString("KeyDeletePreviousWordDisplayString");

	internal static string KeyDeleteRows => GetResourceString("KeyDeleteRows");

	internal static string KeyDeleteRowsDisplayString => GetResourceString("KeyDeleteRowsDisplayString");

	internal static string KeyEnterLineBreakDisplayString => GetResourceString("KeyEnterLineBreakDisplayString");

	internal static string KeyEnterParagraphBreakDisplayString => GetResourceString("KeyEnterParagraphBreakDisplayString");

	internal static string KeyIncreaseFontSizeDisplayString => GetResourceString("KeyIncreaseFontSizeDisplayString");

	internal static string KeyIncreaseIndentationDisplayString => GetResourceString("KeyIncreaseIndentationDisplayString");

	internal static string KeyInsertColumnsDisplayString => GetResourceString("KeyInsertColumnsDisplayString");

	internal static string KeyInsertRowsDisplayString => GetResourceString("KeyInsertRowsDisplayString");

	internal static string KeyInsertTableDisplayString => GetResourceString("KeyInsertTableDisplayString");

	internal static string KeyMergeCellsDisplayString => GetResourceString("KeyMergeCellsDisplayString");

	internal static string KeyMoveDownByLineDisplayString => GetResourceString("KeyMoveDownByLineDisplayString");

	internal static string KeyMoveDownByPageDisplayString => GetResourceString("KeyMoveDownByPageDisplayString");

	internal static string KeyMoveDownByParagraphDisplayString => GetResourceString("KeyMoveDownByParagraphDisplayString");

	internal static string KeyMoveLeftByCharacterDisplayString => GetResourceString("KeyMoveLeftByCharacterDisplayString");

	internal static string KeyMoveLeftByWordDisplayString => GetResourceString("KeyMoveLeftByWordDisplayString");

	internal static string KeyMoveRightByCharacterDisplayString => GetResourceString("KeyMoveRightByCharacterDisplayString");

	internal static string KeyMoveRightByWordDisplayString => GetResourceString("KeyMoveRightByWordDisplayString");

	internal static string KeyMoveToColumnEndDisplayString => GetResourceString("KeyMoveToColumnEndDisplayString");

	internal static string KeyMoveToColumnStartDisplayString => GetResourceString("KeyMoveToColumnStartDisplayString");

	internal static string KeyMoveToDocumentEndDisplayString => GetResourceString("KeyMoveToDocumentEndDisplayString");

	internal static string KeyMoveToDocumentStartDisplayString => GetResourceString("KeyMoveToDocumentStartDisplayString");

	internal static string KeyMoveToLineEndDisplayString => GetResourceString("KeyMoveToLineEndDisplayString");

	internal static string KeyMoveToLineStartDisplayString => GetResourceString("KeyMoveToLineStartDisplayString");

	internal static string KeyMoveToWindowBottomDisplayString => GetResourceString("KeyMoveToWindowBottomDisplayString");

	internal static string KeyMoveToWindowTopDisplayString => GetResourceString("KeyMoveToWindowTopDisplayString");

	internal static string KeyMoveUpByLineDisplayString => GetResourceString("KeyMoveUpByLineDisplayString");

	internal static string KeyMoveUpByPageDisplayString => GetResourceString("KeyMoveUpByPageDisplayString");

	internal static string KeyMoveUpByParagraphDisplayString => GetResourceString("KeyMoveUpByParagraphDisplayString");

	internal static string KeyPasteFormatDisplayString => GetResourceString("KeyPasteFormatDisplayString");

	internal static string KeyRedoDisplayString => GetResourceString("KeyRedoDisplayString");

	internal static string KeyRemoveListMarkersDisplayString => GetResourceString("KeyRemoveListMarkersDisplayString");

	internal static string KeyResetFormatDisplayString => GetResourceString("KeyResetFormatDisplayString");

	internal static string KeySelectAllDisplayString => GetResourceString("KeySelectAllDisplayString");

	internal static string KeySelectDownByLineDisplayString => GetResourceString("KeySelectDownByLineDisplayString");

	internal static string KeySelectDownByPageDisplayString => GetResourceString("KeySelectDownByPageDisplayString");

	internal static string KeySelectDownByParagraphDisplayString => GetResourceString("KeySelectDownByParagraphDisplayString");

	internal static string KeySelectLeftByCharacterDisplayString => GetResourceString("KeySelectLeftByCharacterDisplayString");

	internal static string KeySelectLeftByWordDisplayString => GetResourceString("KeySelectLeftByWordDisplayString");

	internal static string KeySelectRightByCharacterDisplayString => GetResourceString("KeySelectRightByCharacterDisplayString");

	internal static string KeySelectRightByWordDisplayString => GetResourceString("KeySelectRightByWordDisplayString");

	internal static string KeySelectToColumnEndDisplayString => GetResourceString("KeySelectToColumnEndDisplayString");

	internal static string KeySelectToColumnStartDisplayString => GetResourceString("KeySelectToColumnStartDisplayString");

	internal static string KeySelectToDocumentEndDisplayString => GetResourceString("KeySelectToDocumentEndDisplayString");

	internal static string KeySelectToDocumentStartDisplayString => GetResourceString("KeySelectToDocumentStartDisplayString");

	internal static string KeySelectToLineEndDisplayString => GetResourceString("KeySelectToLineEndDisplayString");

	internal static string KeySelectToLineStartDisplayString => GetResourceString("KeySelectToLineStartDisplayString");

	internal static string KeySelectToWindowBottomDisplayString => GetResourceString("KeySelectToWindowBottomDisplayString");

	internal static string KeySelectToWindowTopDisplayString => GetResourceString("KeySelectToWindowTopDisplayString");

	internal static string KeySelectUpByLineDisplayString => GetResourceString("KeySelectUpByLineDisplayString");

	internal static string KeySelectUpByPageDisplayString => GetResourceString("KeySelectUpByPageDisplayString");

	internal static string KeySelectUpByParagraphDisplayString => GetResourceString("KeySelectUpByParagraphDisplayString");

	internal static string KeyShiftBackspaceDisplayString => GetResourceString("KeyShiftBackspaceDisplayString");

	internal static string KeyShiftDeleteDisplayString => GetResourceString("KeyShiftDeleteDisplayString");

	internal static string KeyShiftInsertDisplayString => GetResourceString("KeyShiftInsertDisplayString");

	internal static string KeyShiftSpaceDisplayString => GetResourceString("KeyShiftSpaceDisplayString");

	internal static string KeySpaceDisplayString => GetResourceString("KeySpaceDisplayString");

	internal static string KeySplitCellDisplayString => GetResourceString("KeySplitCellDisplayString");

	internal static string KeySwitchViewingModeDisplayString => GetResourceString("KeySwitchViewingModeDisplayString");

	internal static string KeyTabBackwardDisplayString => GetResourceString("KeyTabBackwardDisplayString");

	internal static string KeyTabForwardDisplayString => GetResourceString("KeyTabForwardDisplayString");

	internal static string KeyToggleBoldDisplayString => GetResourceString("KeyToggleBoldDisplayString");

	internal static string KeyToggleBulletsDisplayString => GetResourceString("KeyToggleBulletsDisplayString");

	internal static string KeyToggleInsertDisplayString => GetResourceString("KeyToggleInsertDisplayString");

	internal static string KeyToggleItalicDisplayString => GetResourceString("KeyToggleItalicDisplayString");

	internal static string KeyToggleNumberingDisplayString => GetResourceString("KeyToggleNumberingDisplayString");

	internal static string KeyToggleSpellCheck => GetResourceString("KeyToggleSpellCheck");

	internal static string KeyToggleSpellCheckDisplayString => GetResourceString("KeyToggleSpellCheckDisplayString");

	internal static string KeyToggleSubscriptDisplayString => GetResourceString("KeyToggleSubscriptDisplayString");

	internal static string KeyToggleSuperscriptDisplayString => GetResourceString("KeyToggleSuperscriptDisplayString");

	internal static string KeyToggleUnderlineDisplayString => GetResourceString("KeyToggleUnderlineDisplayString");

	internal static string KeyUndoDisplayString => GetResourceString("KeyUndoDisplayString");

	internal static string KillBitEnforcedShutdown => GetResourceString("KillBitEnforcedShutdown");

	internal static string KnownTypeIdNegative => GetResourceString("KnownTypeIdNegative");

	internal static string LengthFormatError => GetResourceString("LengthFormatError");

	internal static string ListBoxInvalidAnchorItem => GetResourceString("ListBoxInvalidAnchorItem");

	internal static string ListBoxSelectAllKeyDisplayString => GetResourceString("ListBoxSelectAllKeyDisplayString");

	internal static string ListBoxSelectAllSelectionMode => GetResourceString("ListBoxSelectAllSelectionMode");

	internal static string ListBoxSelectAllText => GetResourceString("ListBoxSelectAllText");

	internal static string ListElementItemNotAChildOfList => GetResourceString("ListElementItemNotAChildOfList");

	internal static string ListView_GridViewColumnCollectionIsReadOnly => GetResourceString("ListView_GridViewColumnCollectionIsReadOnly");

	internal static string ListView_IllegalChildrenType => GetResourceString("ListView_IllegalChildrenType");

	internal static string ListView_MissingParameterlessConstructor => GetResourceString("ListView_MissingParameterlessConstructor");

	internal static string ListView_NotAllowShareColumnToTwoColumnCollection => GetResourceString("ListView_NotAllowShareColumnToTwoColumnCollection");

	internal static string ListView_ViewCannotBeShared => GetResourceString("ListView_ViewCannotBeShared");

	internal static string LogicalTreeLoop => GetResourceString("LogicalTreeLoop");

	internal static string LoopDetected => GetResourceString("LoopDetected");

	internal static string MarkupExtensionBadStatic => GetResourceString("MarkupExtensionBadStatic");

	internal static string MarkupExtensionDynamicOrBindingInCollection => GetResourceString("MarkupExtensionDynamicOrBindingInCollection");

	internal static string MarkupExtensionDynamicOrBindingOnClrProp => GetResourceString("MarkupExtensionDynamicOrBindingOnClrProp");

	internal static string MarkupExtensionNoContext => GetResourceString("MarkupExtensionNoContext");

	internal static string MarkupExtensionProperty => GetResourceString("MarkupExtensionProperty");

	internal static string MarkupExtensionResourceKey => GetResourceString("MarkupExtensionResourceKey");

	internal static string MarkupExtensionResourceNotFound => GetResourceString("MarkupExtensionResourceNotFound");

	internal static string MarkupExtensionStaticMember => GetResourceString("MarkupExtensionStaticMember");

	internal static string MarkupWriter_CannotSerializeGenerictype => GetResourceString("MarkupWriter_CannotSerializeGenerictype");

	internal static string MarkupWriter_CannotSerializeNestedPublictype => GetResourceString("MarkupWriter_CannotSerializeNestedPublictype");

	internal static string MarkupWriter_CannotSerializeNonPublictype => GetResourceString("MarkupWriter_CannotSerializeNonPublictype");

	internal static string MaximumNoteSizeExceeded => GetResourceString("MaximumNoteSizeExceeded");

	internal static string MaxLengthExceedsBufferSize => GetResourceString("MaxLengthExceedsBufferSize");

	internal static string MeasureReentrancyInvalid => GetResourceString("MeasureReentrancyInvalid");

	internal static string MediaElement_CannotSetSourceOnMediaElementDrivenByClock => GetResourceString("MediaElement_CannotSetSourceOnMediaElementDrivenByClock");

	internal static string MemberNotAllowedDuringAddOrEdit => GetResourceString("MemberNotAllowedDuringAddOrEdit");

	internal static string MemberNotAllowedDuringTransaction => GetResourceString("MemberNotAllowedDuringTransaction");

	internal static string MemberNotAllowedForView => GetResourceString("MemberNotAllowedForView");

	internal static string MissingAnnotationHighlightLayer => GetResourceString("MissingAnnotationHighlightLayer");

	internal static string MissingContentSource => GetResourceString("MissingContentSource");

	internal static string MissingTagInNamespace => GetResourceString("MissingTagInNamespace");

	internal static string MissingTriggerProperty => GetResourceString("MissingTriggerProperty");

	internal static string MissingValueConverter => GetResourceString("MissingValueConverter");

	internal static string ModificationEarlierThanCreation => GetResourceString("ModificationEarlierThanCreation");

	internal static string ModifyingLogicalTreeViaStylesNotImplemented => GetResourceString("ModifyingLogicalTreeViaStylesNotImplemented");

	internal static string MoreThanOneAttachedAnnotation => GetResourceString("MoreThanOneAttachedAnnotation");

	internal static string MoreThanOneStartingParts => GetResourceString("MoreThanOneStartingParts");

	internal static string MoveInDeferSelectionActive => GetResourceString("MoveInDeferSelectionActive");

	internal static string MultiBindingHasNoConverter => GetResourceString("MultiBindingHasNoConverter");

	internal static string MultipleAssemblyMatches => GetResourceString("MultipleAssemblyMatches");

	internal static string MultiSelectorSelectAll => GetResourceString("MultiSelectorSelectAll");

	internal static string MultiSingleton => GetResourceString("MultiSingleton");

	internal static string MultiThreadedCollectionChangeNotSupported => GetResourceString("MultiThreadedCollectionChangeNotSupported");

	internal static string MustBaseOnStyleOfABaseType => GetResourceString("MustBaseOnStyleOfABaseType");

	internal static string MustBeCondition => GetResourceString("MustBeCondition");

	internal static string MustBeFrameworkDerived => GetResourceString("MustBeFrameworkDerived");

	internal static string MustBeFrameworkOr3DDerived => GetResourceString("MustBeFrameworkOr3DDerived");

	internal static string MustBeOfType => GetResourceString("MustBeOfType");

	internal static string MustBeTriggerAction => GetResourceString("MustBeTriggerAction");

	internal static string MustBeTypeOrString => GetResourceString("MustBeTypeOrString");

	internal static string MustImplementIUriContext => GetResourceString("MustImplementIUriContext");

	internal static string MustNotTemplateUnassociatedControl => GetResourceString("MustNotTemplateUnassociatedControl");

	internal static string MustUseWindowStyleNone => GetResourceString("MustUseWindowStyleNone");

	internal static string NamedObjectMustBeFrameworkElement => GetResourceString("NamedObjectMustBeFrameworkElement");

	internal static string NameNotEmptyString => GetResourceString("NameNotEmptyString");

	internal static string NameNotFound => GetResourceString("NameNotFound");

	internal static string NameScopeDuplicateNamesNotAllowed => GetResourceString("NameScopeDuplicateNamesNotAllowed");

	internal static string NameScopeInvalidIdentifierName => GetResourceString("NameScopeInvalidIdentifierName");

	internal static string NameScopeNameNotEmptyString => GetResourceString("NameScopeNameNotEmptyString");

	internal static string NameScopeNameNotFound => GetResourceString("NameScopeNameNotFound");

	internal static string NameScopeNotFound => GetResourceString("NameScopeNotFound");

	internal static string NamesNotSupportedInsideResourceDictionary => GetResourceString("NamesNotSupportedInsideResourceDictionary");

	internal static string NavWindowMenuCurrentPage => GetResourceString("NavWindowMenuCurrentPage");

	internal static string NeedToBeComVisible => GetResourceString("NeedToBeComVisible");

	internal static string NegativeValue => GetResourceString("NegativeValue");

	internal static string NestedBeginInitNotSupported => GetResourceString("NestedBeginInitNotSupported");

	internal static string NoAddChild => GetResourceString("NoAddChild");

	internal static string NoAttachedAnnotationToModify => GetResourceString("NoAttachedAnnotationToModify");

	internal static string NoBackEntry => GetResourceString("NoBackEntry");

	internal static string NoCheckOrChangeWhenDeferred => GetResourceString("NoCheckOrChangeWhenDeferred");

	internal static string NoDefaultUpdateSourceTrigger => GetResourceString("NoDefaultUpdateSourceTrigger");

	internal static string NoElement => GetResourceString("NoElement");

	internal static string NoElementObject => GetResourceString("NoElementObject");

	internal static string NoForwardEntry => GetResourceString("NoForwardEntry");

	internal static string NoMulticastHandlers => GetResourceString("NoMulticastHandlers");

	internal static string NonClsActivationException => GetResourceString("NonClsActivationException");

	internal static string NonCLSException => GetResourceString("NonCLSException");

	internal static string NonPackAppAbsoluteUriNotAllowed => GetResourceString("NonPackAppAbsoluteUriNotAllowed");

	internal static string NonPackSooAbsoluteUriNotAllowed => GetResourceString("NonPackSooAbsoluteUriNotAllowed");

	internal static string NonWhiteSpaceInAddText => GetResourceString("NonWhiteSpaceInAddText");

	internal static string NoPresentationContextForGivenElement => GetResourceString("NoPresentationContextForGivenElement");

	internal static string NoProcessorForSelectionType => GetResourceString("NoProcessorForSelectionType");

	internal static string NoScopingElement => GetResourceString("NoScopingElement");

	internal static string NotAllowedBeforeShow => GetResourceString("NotAllowedBeforeShow");

	internal static string NotHighlightAnnotationType => GetResourceString("NotHighlightAnnotationType");

	internal static string NotInAssociatedContainer => GetResourceString("NotInAssociatedContainer");

	internal static string NotInAssociatedTree => GetResourceString("NotInAssociatedTree");

	internal static string NotInThisTree => GetResourceString("NotInThisTree");

	internal static string NotSupported => GetResourceString("NotSupported");

	internal static string NotSupportedInBrowser => GetResourceString("NotSupportedInBrowser");

	internal static string NoUpdateSourceTriggerForInnerBindingOfMultiBinding => GetResourceString("NoUpdateSourceTriggerForInnerBindingOfMultiBinding");

	internal static string NullParentNode => GetResourceString("NullParentNode");

	internal static string NullPropertyIllegal => GetResourceString("NullPropertyIllegal");

	internal static string NullTypeIllegal => GetResourceString("NullTypeIllegal");

	internal static string NullUri => GetResourceString("NullUri");

	internal static string ObjectDataProviderCanHaveOnlyOneSource => GetResourceString("ObjectDataProviderCanHaveOnlyOneSource");

	internal static string ObjectDataProviderHasNoSource => GetResourceString("ObjectDataProviderHasNoSource");

	internal static string ObjectDataProviderNonCLSException => GetResourceString("ObjectDataProviderNonCLSException");

	internal static string ObjectDataProviderNonCLSExceptionInvoke => GetResourceString("ObjectDataProviderNonCLSExceptionInvoke");

	internal static string ObjectDataProviderParameterCollectionIsNotInUse => GetResourceString("ObjectDataProviderParameterCollectionIsNotInUse");

	internal static string ObjectDisposed_StoreClosed => GetResourceString("ObjectDisposed_StoreClosed");

	internal static string OnlyFlowAndFixedSupported => GetResourceString("OnlyFlowAndFixedSupported");

	internal static string OnlyFlowFixedSupported => GetResourceString("OnlyFlowFixedSupported");

	internal static string PageCacheSizeNotAllowed => GetResourceString("PageCacheSizeNotAllowed");

	internal static string PageCannotHaveMultipleContent => GetResourceString("PageCannotHaveMultipleContent");

	internal static string PageContentNotFound => GetResourceString("PageContentNotFound");

	internal static string PageContentUnsupportedMimeType => GetResourceString("PageContentUnsupportedMimeType");

	internal static string PageContentUnsupportedPageType => GetResourceString("PageContentUnsupportedPageType");

	internal static string PanelIsNotItemsHost => GetResourceString("PanelIsNotItemsHost");

	internal static string Panel_BoundPanel_NoChildren => GetResourceString("Panel_BoundPanel_NoChildren");

	internal static string Panel_ItemsControlNotFound => GetResourceString("Panel_ItemsControlNotFound");

	internal static string Panel_NoNullChildren => GetResourceString("Panel_NoNullChildren");

	internal static string Panel_NoNullVisualParent => GetResourceString("Panel_NoNullVisualParent");

	internal static string ParameterMustBeLogicalNode => GetResourceString("ParameterMustBeLogicalNode");

	internal static string ParentOfPageMustBeWindowOrFrame => GetResourceString("ParentOfPageMustBeWindowOrFrame");

	internal static string ParserAbandonedTypeConverterText => GetResourceString("ParserAbandonedTypeConverterText");

	internal static string ParserAsyncOnRoot => GetResourceString("ParserAsyncOnRoot");

	internal static string ParserAttachedPropInheritError => GetResourceString("ParserAttachedPropInheritError");

	internal static string ParserAttributeArgsLow => GetResourceString("ParserAttributeArgsLow");

	internal static string ParserAttributeNamespaceMisMatch => GetResourceString("ParserAttributeNamespaceMisMatch");

	internal static string ParserBadAssemblyName => GetResourceString("ParserBadAssemblyName");

	internal static string ParserBadAssemblyPath => GetResourceString("ParserBadAssemblyPath");

	internal static string ParserBadChild => GetResourceString("ParserBadChild");

	internal static string ParserBadConstructorParams => GetResourceString("ParserBadConstructorParams");

	internal static string ParserBadEncoding => GetResourceString("ParserBadEncoding");

	internal static string ParserBadKey => GetResourceString("ParserBadKey");

	internal static string ParserBadMemberReference => GetResourceString("ParserBadMemberReference");

	internal static string ParserBadName => GetResourceString("ParserBadName");

	internal static string ParserBadNullableType => GetResourceString("ParserBadNullableType");

	internal static string ParserBadString => GetResourceString("ParserBadString");

	internal static string ParserBadSyncMode => GetResourceString("ParserBadSyncMode");

	internal static string ParserBadTypeInArrayProperty => GetResourceString("ParserBadTypeInArrayProperty");

	internal static string ParserBadUidOrNameME => GetResourceString("ParserBadUidOrNameME");

	internal static string ParserBamlEvent => GetResourceString("ParserBamlEvent");

	internal static string ParserBamlVersion => GetResourceString("ParserBamlVersion");

	internal static string ParserCannotAddAnyChildren => GetResourceString("ParserCannotAddAnyChildren");

	internal static string ParserCannotAddAnyChildren2 => GetResourceString("ParserCannotAddAnyChildren2");

	internal static string ParserCannotAddChild => GetResourceString("ParserCannotAddChild");

	internal static string ParserCannotConvertInitializationText => GetResourceString("ParserCannotConvertInitializationText");

	internal static string ParserCannotConvertPropertyValue => GetResourceString("ParserCannotConvertPropertyValue");

	internal static string ParserCannotConvertPropertyValueString => GetResourceString("ParserCannotConvertPropertyValueString");

	internal static string ParserCannotConvertString => GetResourceString("ParserCannotConvertString");

	internal static string ParserCannotReuseXamlReader => GetResourceString("ParserCannotReuseXamlReader");

	internal static string ParserCannotSetValue => GetResourceString("ParserCannotSetValue");

	internal static string ParserCanOnlyHaveOneChild => GetResourceString("ParserCanOnlyHaveOneChild");

	internal static string ParserCantCreateDelegate => GetResourceString("ParserCantCreateDelegate");

	internal static string ParserCantCreateInstanceType => GetResourceString("ParserCantCreateInstanceType");

	internal static string ParserCantCreateTextComplexProp => GetResourceString("ParserCantCreateTextComplexProp");

	internal static string ParserCantGetDPOrPi => GetResourceString("ParserCantGetDPOrPi");

	internal static string ParserCantGetProperty => GetResourceString("ParserCantGetProperty");

	internal static string ParserCantSetAttribute => GetResourceString("ParserCantSetAttribute");

	internal static string ParserCantSetContentProperty => GetResourceString("ParserCantSetContentProperty");

	internal static string ParserCantSetTriggerCondition => GetResourceString("ParserCantSetTriggerCondition");

	internal static string ParserCompatDuplicate => GetResourceString("ParserCompatDuplicate");

	internal static string ParserContentMustBeContiguous => GetResourceString("ParserContentMustBeContiguous");

	internal static string ParserDefaultConverterElement => GetResourceString("ParserDefaultConverterElement");

	internal static string ParserDefaultConverterProperty => GetResourceString("ParserDefaultConverterProperty");

	internal static string ParserDeferContentAsync => GetResourceString("ParserDeferContentAsync");

	internal static string ParserDefSharedOnlyInCompiled => GetResourceString("ParserDefSharedOnlyInCompiled");

	internal static string ParserDefTag => GetResourceString("ParserDefTag");

	internal static string ParserDictionarySealed => GetResourceString("ParserDictionarySealed");

	internal static string ParserDupDictionaryKey => GetResourceString("ParserDupDictionaryKey");

	internal static string ParserDuplicateMarkupExtensionProperty => GetResourceString("ParserDuplicateMarkupExtensionProperty");

	internal static string ParserDuplicateProperty1 => GetResourceString("ParserDuplicateProperty1");

	internal static string ParserDuplicateProperty2 => GetResourceString("ParserDuplicateProperty2");

	internal static string ParserEmptyComplexProp => GetResourceString("ParserEmptyComplexProp");

	internal static string ParserEntityReference => GetResourceString("ParserEntityReference");

	internal static string ParserErrorContext_File => GetResourceString("ParserErrorContext_File");

	internal static string ParserErrorContext_File_Line => GetResourceString("ParserErrorContext_File_Line");

	internal static string ParserErrorContext_Line => GetResourceString("ParserErrorContext_Line");

	internal static string ParserErrorContext_Type => GetResourceString("ParserErrorContext_Type");

	internal static string ParserErrorContext_Type_File => GetResourceString("ParserErrorContext_Type_File");

	internal static string ParserErrorContext_Type_File_Line => GetResourceString("ParserErrorContext_Type_File_Line");

	internal static string ParserErrorContext_Type_Line => GetResourceString("ParserErrorContext_Type_Line");

	internal static string ParserErrorCreatingInstance => GetResourceString("ParserErrorCreatingInstance");

	internal static string ParserErrorParsingAttrib => GetResourceString("ParserErrorParsingAttrib");

	internal static string ParserErrorParsingAttribType => GetResourceString("ParserErrorParsingAttribType");

	internal static string ParserEventDelegateTypeNotAccessible => GetResourceString("ParserEventDelegateTypeNotAccessible");

	internal static string ParserFailedEndInit => GetResourceString("ParserFailedEndInit");

	internal static string ParserFailedToCreateFromConstructor => GetResourceString("ParserFailedToCreateFromConstructor");

	internal static string ParserFailFindType => GetResourceString("ParserFailFindType");

	internal static string ParserFilterXmlReaderNoDefinitionPrefixChangeAllowed => GetResourceString("ParserFilterXmlReaderNoDefinitionPrefixChangeAllowed");

	internal static string ParserFilterXmlReaderNoIndexAttributeAccess => GetResourceString("ParserFilterXmlReaderNoIndexAttributeAccess");

	internal static string ParserIAddChildText => GetResourceString("ParserIAddChildText");

	internal static string ParserIEnumerableIAddChild => GetResourceString("ParserIEnumerableIAddChild");

	internal static string ParserInvalidContentPropertyAttribute => GetResourceString("ParserInvalidContentPropertyAttribute");

	internal static string ParserInvalidIdentifierName => GetResourceString("ParserInvalidIdentifierName");

	internal static string ParserInvalidStaticMember => GetResourceString("ParserInvalidStaticMember");

	internal static string ParserKeyOnExplicitDictionary => GetResourceString("ParserKeyOnExplicitDictionary");

	internal static string ParserKeysAreStrings => GetResourceString("ParserKeysAreStrings");

	internal static string ParserLineAndOffset => GetResourceString("ParserLineAndOffset");

	internal static string ParserMapPIMissingAssembly => GetResourceString("ParserMapPIMissingAssembly");

	internal static string ParserMapPIMissingKey => GetResourceString("ParserMapPIMissingKey");

	internal static string ParserMappingUriInvalid => GetResourceString("ParserMappingUriInvalid");

	internal static string ParserMarkupExtensionBadConstructorParam => GetResourceString("ParserMarkupExtensionBadConstructorParam");

	internal static string ParserMarkupExtensionBadDelimiter => GetResourceString("ParserMarkupExtensionBadDelimiter");

	internal static string ParserMarkupExtensionDelimiterBeforeFirstAttribute => GetResourceString("ParserMarkupExtensionDelimiterBeforeFirstAttribute");

	internal static string ParserMarkupExtensionInvalidClosingBracketCharacers => GetResourceString("ParserMarkupExtensionInvalidClosingBracketCharacers");

	internal static string ParserMarkupExtensionMalformedBracketCharacers => GetResourceString("ParserMarkupExtensionMalformedBracketCharacers");

	internal static string ParserMarkupExtensionNoEndCurlie => GetResourceString("ParserMarkupExtensionNoEndCurlie");

	internal static string ParserMarkupExtensionNoNameValue => GetResourceString("ParserMarkupExtensionNoNameValue");

	internal static string ParserMarkupExtensionNoQuotesInName => GetResourceString("ParserMarkupExtensionNoQuotesInName");

	internal static string ParserMarkupExtensionTrailingGarbage => GetResourceString("ParserMarkupExtensionTrailingGarbage");

	internal static string ParserMarkupExtensionUnknownAttr => GetResourceString("ParserMarkupExtensionUnknownAttr");

	internal static string ParserMetroUnknownAttribute => GetResourceString("ParserMetroUnknownAttribute");

	internal static string ParserMultiBamls => GetResourceString("ParserMultiBamls");

	internal static string ParserMultiRoot => GetResourceString("ParserMultiRoot");

	internal static string ParserNestedComplexProp => GetResourceString("ParserNestedComplexProp");

	internal static string ParserNoAttrArray => GetResourceString("ParserNoAttrArray");

	internal static string ParserNoChildrenTag => GetResourceString("ParserNoChildrenTag");

	internal static string ParserNoComplexMulti => GetResourceString("ParserNoComplexMulti");

	internal static string ParserNoDefaultConstructor => GetResourceString("ParserNoDefaultConstructor");

	internal static string ParserNoDefaultPropConstructor => GetResourceString("ParserNoDefaultPropConstructor");

	internal static string ParserNoDictionaryKey => GetResourceString("ParserNoDictionaryKey");

	internal static string ParserNoDictionaryName => GetResourceString("ParserNoDictionaryName");

	internal static string ParserNoDigitEnums => GetResourceString("ParserNoDigitEnums");

	internal static string ParserNoDPOnOwner => GetResourceString("ParserNoDPOnOwner");

	internal static string ParserNoElementCreate2 => GetResourceString("ParserNoElementCreate2");

	internal static string ParserNoEvents => GetResourceString("ParserNoEvents");

	internal static string ParserNoEventTag => GetResourceString("ParserNoEventTag");

	internal static string ParserNoMatchingArray => GetResourceString("ParserNoMatchingArray");

	internal static string ParserNoMatchingIDictionary => GetResourceString("ParserNoMatchingIDictionary");

	internal static string ParserNoMatchingIList => GetResourceString("ParserNoMatchingIList");

	internal static string ParserNoNameOnType => GetResourceString("ParserNoNameOnType");

	internal static string ParserNoNamespace => GetResourceString("ParserNoNamespace");

	internal static string ParserNoNameUnderDefinitionScopeType => GetResourceString("ParserNoNameUnderDefinitionScopeType");

	internal static string ParserNoNestedXmlDataIslands => GetResourceString("ParserNoNestedXmlDataIslands");

	internal static string ParserNoPropOnComplexProp => GetResourceString("ParserNoPropOnComplexProp");

	internal static string ParserNoPropType => GetResourceString("ParserNoPropType");

	internal static string ParserNoResource => GetResourceString("ParserNoResource");

	internal static string ParserNoSerializer => GetResourceString("ParserNoSerializer");

	internal static string ParserNoSetterChild => GetResourceString("ParserNoSetterChild");

	internal static string ParserNotAllowedInternalType => GetResourceString("ParserNotAllowedInternalType");

	internal static string ParserNotMarkedPublic => GetResourceString("ParserNotMarkedPublic");

	internal static string ParserNotMarkupExtension => GetResourceString("ParserNotMarkupExtension");

	internal static string ParserNoType => GetResourceString("ParserNoType");

	internal static string ParserNoTypeConv => GetResourceString("ParserNoTypeConv");

	internal static string ParserNullPropertyCollection => GetResourceString("ParserNullPropertyCollection");

	internal static string ParserNullReturned => GetResourceString("ParserNullReturned");

	internal static string ParserOwnerEventMustBePublic => GetResourceString("ParserOwnerEventMustBePublic");

	internal static string ParserParentDO => GetResourceString("ParserParentDO");

	internal static string ParserPrefixNSElement => GetResourceString("ParserPrefixNSElement");

	internal static string ParserPrefixNSProperty => GetResourceString("ParserPrefixNSProperty");

	internal static string ParserPropertyCollectionClosed => GetResourceString("ParserPropertyCollectionClosed");

	internal static string ParserPropNoValue => GetResourceString("ParserPropNoValue");

	internal static string ParserProvideValueCantSetUri => GetResourceString("ParserProvideValueCantSetUri");

	internal static string ParserPublicType => GetResourceString("ParserPublicType");

	internal static string ParserReadOnlyNullProperty => GetResourceString("ParserReadOnlyNullProperty");

	internal static string ParserReadOnlyProp => GetResourceString("ParserReadOnlyProp");

	internal static string ParserResourceKeyType => GetResourceString("ParserResourceKeyType");

	internal static string ParserSyncOnRoot => GetResourceString("ParserSyncOnRoot");

	internal static string ParserTextInComplexProp => GetResourceString("ParserTextInComplexProp");

	internal static string ParserTextInvalidInArrayOrDictionary => GetResourceString("ParserTextInvalidInArrayOrDictionary");

	internal static string ParserTooManyAssemblies => GetResourceString("ParserTooManyAssemblies");

	internal static string ParserTypeConverterTextNeedsEndElement => GetResourceString("ParserTypeConverterTextNeedsEndElement");

	internal static string ParserTypeConverterTextUnusable => GetResourceString("ParserTypeConverterTextUnusable");

	internal static string ParserUndeclaredNS => GetResourceString("ParserUndeclaredNS");

	internal static string ParserUnexpectedEndEle => GetResourceString("ParserUnexpectedEndEle");

	internal static string ParserUnexpInBAML => GetResourceString("ParserUnexpInBAML");

	internal static string ParserUnknownAttribute => GetResourceString("ParserUnknownAttribute");

	internal static string ParserUnknownBaml => GetResourceString("ParserUnknownBaml");

	internal static string ParserUnknownDefAttribute => GetResourceString("ParserUnknownDefAttribute");

	internal static string ParserUnknownDefAttributeCompiler => GetResourceString("ParserUnknownDefAttributeCompiler");

	internal static string ParserUnknownPresentationOptionsAttribute => GetResourceString("ParserUnknownPresentationOptionsAttribute");

	internal static string ParserUnknownTag => GetResourceString("ParserUnknownTag");

	internal static string ParserUnknownXmlType => GetResourceString("ParserUnknownXmlType");

	internal static string ParserWriterNoSeekEnd => GetResourceString("ParserWriterNoSeekEnd");

	internal static string ParserWriterUnknownOrigin => GetResourceString("ParserWriterUnknownOrigin");

	internal static string ParserXmlIslandMissing => GetResourceString("ParserXmlIslandMissing");

	internal static string ParserXmlIslandUnexpected => GetResourceString("ParserXmlIslandUnexpected");

	internal static string ParserXmlLangPropertyValueInvalid => GetResourceString("ParserXmlLangPropertyValueInvalid");

	internal static string ParserXmlReaderNoLineInfo => GetResourceString("ParserXmlReaderNoLineInfo");

	internal static string PartialTrustPrintDialogMustBeInvoked => GetResourceString("PartialTrustPrintDialogMustBeInvoked");

	internal static string PasswordBoxInvalidTextContainer => GetResourceString("PasswordBoxInvalidTextContainer");

	internal static string PathParameterIsNull => GetResourceString("PathParameterIsNull");

	internal static string PathParametersIndexOutOfRange => GetResourceString("PathParametersIndexOutOfRange");

	internal static string PathSyntax => GetResourceString("PathSyntax");

	internal static string PlatformRequirementTitle => GetResourceString("PlatformRequirementTitle");

	internal static string PopupReopeningNotAllowed => GetResourceString("PopupReopeningNotAllowed");

	internal static string PositionalArgumentsWrongLength => GetResourceString("PositionalArgumentsWrongLength");

	internal static string PrevoiusPartialPageContentOutstanding => GetResourceString("PrevoiusPartialPageContentOutstanding");

	internal static string PrevoiusUninitializedDocumentReferenceOutstanding => GetResourceString("PrevoiusUninitializedDocumentReferenceOutstanding");

	internal static string PrintDialogInstallPrintSupportCaption => GetResourceString("PrintDialogInstallPrintSupportCaption");

	internal static string PrintDialogInstallPrintSupportMessageBox => GetResourceString("PrintDialogInstallPrintSupportMessageBox");

	internal static string PrintDialogInvalidPageRange => GetResourceString("PrintDialogInvalidPageRange");

	internal static string PrintDialogPageRange => GetResourceString("PrintDialogPageRange");

	internal static string PrintDialogZeroNotAllowed => GetResourceString("PrintDialogZeroNotAllowed");

	internal static string PrintJobDescription => GetResourceString("PrintJobDescription");

	internal static string ProgressBarReadOnly => GetResourceString("ProgressBarReadOnly");

	internal static string PropertyFoundOutsideStartElement => GetResourceString("PropertyFoundOutsideStartElement");

	internal static string PropertyIdOutOfSequence => GetResourceString("PropertyIdOutOfSequence");

	internal static string PropertyIsImmutable => GetResourceString("PropertyIsImmutable");

	internal static string PropertyIsInitializeOnly => GetResourceString("PropertyIsInitializeOnly");

	internal static string PropertyMustHaveValue => GetResourceString("PropertyMustHaveValue");

	internal static string PropertyNotBindable => GetResourceString("PropertyNotBindable");

	internal static string PropertyNotFound => GetResourceString("PropertyNotFound");

	internal static string PropertyNotSupported => GetResourceString("PropertyNotSupported");

	internal static string PropertyOutOfOrder => GetResourceString("PropertyOutOfOrder");

	internal static string PropertyPathIndexWrongType => GetResourceString("PropertyPathIndexWrongType");

	internal static string PropertyPathInvalidAccessor => GetResourceString("PropertyPathInvalidAccessor");

	internal static string PropertyPathNoOwnerType => GetResourceString("PropertyPathNoOwnerType");

	internal static string PropertyPathNoProperty => GetResourceString("PropertyPathNoProperty");

	internal static string PropertyPathSyntaxError => GetResourceString("PropertyPathSyntaxError");

	internal static string PropertyToSortByNotFoundOnType => GetResourceString("PropertyToSortByNotFoundOnType");

	internal static string PropertyTriggerCycleDetected => GetResourceString("PropertyTriggerCycleDetected");

	internal static string PropertyTriggerLayerLimitExceeded => GetResourceString("PropertyTriggerLayerLimitExceeded");

	internal static string PTSError => GetResourceString("PTSError");

	internal static string PTSInvalidHandle => GetResourceString("PTSInvalidHandle");

	internal static string RangeActionsNotSupported => GetResourceString("RangeActionsNotSupported");

	internal static string ReadCountNegative => GetResourceString("ReadCountNegative");

	internal static string ReadNotSupported => GetResourceString("ReadNotSupported");

	internal static string ReadOnlyPropertyNotAllowed => GetResourceString("ReadOnlyPropertyNotAllowed");

	internal static string RecordOutOfOrder => GetResourceString("RecordOutOfOrder");

	internal static string Rect_WidthAndHeightCannotBeNegative => GetResourceString("Rect_WidthAndHeightCannotBeNegative");

	internal static string RelativeSourceInvalidAncestorLevel => GetResourceString("RelativeSourceInvalidAncestorLevel");

	internal static string RelativeSourceModeInvalid => GetResourceString("RelativeSourceModeInvalid");

	internal static string RelativeSourceModeIsImmutable => GetResourceString("RelativeSourceModeIsImmutable");

	internal static string RelativeSourceNeedsAncestorType => GetResourceString("RelativeSourceNeedsAncestorType");

	internal static string RelativeSourceNeedsMode => GetResourceString("RelativeSourceNeedsMode");

	internal static string RelativeSourceNotInFindAncestorMode => GetResourceString("RelativeSourceNotInFindAncestorMode");

	internal static string RemovedItemNotFound => GetResourceString("RemovedItemNotFound");

	internal static string RemoveRequiresOffsetZero => GetResourceString("RemoveRequiresOffsetZero");

	internal static string RemoveRequiresPositiveCount => GetResourceString("RemoveRequiresPositiveCount");

	internal static string RemovingPlaceholder => GetResourceString("RemovingPlaceholder");

	internal static string ReparentModelChildIllegal => GetResourceString("ReparentModelChildIllegal");

	internal static string RequestNavigateEventMustHaveRoutedEvent => GetResourceString("RequestNavigateEventMustHaveRoutedEvent");

	internal static string RequiredAttributeMissing => GetResourceString("RequiredAttributeMissing");

	internal static string RequiresExplicitCulture => GetResourceString("RequiresExplicitCulture");

	internal static string RequiresXmlNamespaceMapping => GetResourceString("RequiresXmlNamespaceMapping");

	internal static string RequiresXmlNamespaceMappingUri => GetResourceString("RequiresXmlNamespaceMappingUri");

	internal static string ReshowNotAllowed => GetResourceString("ReshowNotAllowed");

	internal static string ResourceDictionaryDeferredContentFailure => GetResourceString("ResourceDictionaryDeferredContentFailure");

	internal static string ResourceDictionaryDuplicateDeferredContent => GetResourceString("ResourceDictionaryDuplicateDeferredContent");

	internal static string ResourceDictionaryInvalidMergedDictionary => GetResourceString("ResourceDictionaryInvalidMergedDictionary");

	internal static string ResourceDictionaryIsReadOnly => GetResourceString("ResourceDictionaryIsReadOnly");

	internal static string ResourceDictionaryLoadFromFailure => GetResourceString("ResourceDictionaryLoadFromFailure");

	internal static string ReturnEventHandlerMustBeOnParentPage => GetResourceString("ReturnEventHandlerMustBeOnParentPage");

	internal static string RichTextBox_CantSetDocumentInsideChangeBlock => GetResourceString("RichTextBox_CantSetDocumentInsideChangeBlock");

	internal static string RichTextBox_DocumentBelongsToAnotherRichTextBoxAlready => GetResourceString("RichTextBox_DocumentBelongsToAnotherRichTextBoxAlready");

	internal static string RichTextBox_PointerNotInSameDocument => GetResourceString("RichTextBox_PointerNotInSameDocument");

	internal static string RowCacheCannotModifyNonExistentLayout => GetResourceString("RowCacheCannotModifyNonExistentLayout");

	internal static string RowCachePageNotFound => GetResourceString("RowCachePageNotFound");

	internal static string RowCacheRecalcWithNoPageCache => GetResourceString("RowCacheRecalcWithNoPageCache");

	internal static string RuntimeTypeRequired => GetResourceString("RuntimeTypeRequired");

	internal static string ScrollBar_ContextMenu_Bottom => GetResourceString("ScrollBar_ContextMenu_Bottom");

	internal static string ScrollBar_ContextMenu_LeftEdge => GetResourceString("ScrollBar_ContextMenu_LeftEdge");

	internal static string ScrollBar_ContextMenu_PageDown => GetResourceString("ScrollBar_ContextMenu_PageDown");

	internal static string ScrollBar_ContextMenu_PageLeft => GetResourceString("ScrollBar_ContextMenu_PageLeft");

	internal static string ScrollBar_ContextMenu_PageRight => GetResourceString("ScrollBar_ContextMenu_PageRight");

	internal static string ScrollBar_ContextMenu_PageUp => GetResourceString("ScrollBar_ContextMenu_PageUp");

	internal static string ScrollBar_ContextMenu_RightEdge => GetResourceString("ScrollBar_ContextMenu_RightEdge");

	internal static string ScrollBar_ContextMenu_ScrollDown => GetResourceString("ScrollBar_ContextMenu_ScrollDown");

	internal static string ScrollBar_ContextMenu_ScrollHere => GetResourceString("ScrollBar_ContextMenu_ScrollHere");

	internal static string ScrollBar_ContextMenu_ScrollLeft => GetResourceString("ScrollBar_ContextMenu_ScrollLeft");

	internal static string ScrollBar_ContextMenu_ScrollRight => GetResourceString("ScrollBar_ContextMenu_ScrollRight");

	internal static string ScrollBar_ContextMenu_ScrollUp => GetResourceString("ScrollBar_ContextMenu_ScrollUp");

	internal static string ScrollBar_ContextMenu_Top => GetResourceString("ScrollBar_ContextMenu_Top");

	internal static string ScrollViewer_CannotBeNaN => GetResourceString("ScrollViewer_CannotBeNaN");

	internal static string ScrollViewer_OutOfRange => GetResourceString("ScrollViewer_OutOfRange");

	internal static string SeekFailed => GetResourceString("SeekFailed");

	internal static string SeekNegative => GetResourceString("SeekNegative");

	internal static string SeekNotSupported => GetResourceString("SeekNotSupported");

	internal static string SelectedCellsCollection_DuplicateItem => GetResourceString("SelectedCellsCollection_DuplicateItem");

	internal static string SelectedCellsCollection_InvalidItem => GetResourceString("SelectedCellsCollection_InvalidItem");

	internal static string SelectionChangeActive => GetResourceString("SelectionChangeActive");

	internal static string SelectionChangeNotActive => GetResourceString("SelectionChangeNotActive");

	internal static string SelectionDoesNotResolveToAPage => GetResourceString("SelectionDoesNotResolveToAPage");

	internal static string SelectionMustBeServiceProvider => GetResourceString("SelectionMustBeServiceProvider");

	internal static string SerializerProviderAlreadyRegistered => GetResourceString("SerializerProviderAlreadyRegistered");

	internal static string SerializerProviderCannotLoad => GetResourceString("SerializerProviderCannotLoad");

	internal static string SerializerProviderDefaultFileExtensionNull => GetResourceString("SerializerProviderDefaultFileExtensionNull");

	internal static string SerializerProviderDisplayNameNull => GetResourceString("SerializerProviderDisplayNameNull");

	internal static string SerializerProviderManufacturerNameNull => GetResourceString("SerializerProviderManufacturerNameNull");

	internal static string SerializerProviderManufacturerWebsiteNull => GetResourceString("SerializerProviderManufacturerWebsiteNull");

	internal static string SerializerProviderNotRegistered => GetResourceString("SerializerProviderNotRegistered");

	internal static string SerializerProviderUnknownSerializer => GetResourceString("SerializerProviderUnknownSerializer");

	internal static string SerializerProviderWrongVersion => GetResourceString("SerializerProviderWrongVersion");

	internal static string SetFocusFailed => GetResourceString("SetFocusFailed");

	internal static string SetInDeferSelectionActive => GetResourceString("SetInDeferSelectionActive");

	internal static string SetLengthNotSupported => GetResourceString("SetLengthNotSupported");

	internal static string SetPositionNotSupported => GetResourceString("SetPositionNotSupported");

	internal static string SetterOnStyleNotAllowedToHaveTarget => GetResourceString("SetterOnStyleNotAllowedToHaveTarget");

	internal static string SetterValueCannotBeUnset => GetResourceString("SetterValueCannotBeUnset");

	internal static string SetterValueOfMarkupExtensionNotSupported => GetResourceString("SetterValueOfMarkupExtensionNotSupported");

	internal static string SharedAttributeInLooseXaml => GetResourceString("SharedAttributeInLooseXaml");

	internal static string ShowDialogOnModal => GetResourceString("ShowDialogOnModal");

	internal static string ShowDialogOnVisible => GetResourceString("ShowDialogOnVisible");

	internal static string ShowNonActivatedAndMaximized => GetResourceString("ShowNonActivatedAndMaximized");

	internal static string ShutdownModeWhenAppShutdown => GetResourceString("ShutdownModeWhenAppShutdown");

	internal static string SourceNameNotSupportedForDataTriggers => GetResourceString("SourceNameNotSupportedForDataTriggers");

	internal static string SourceNameNotSupportedForStyleTriggers => GetResourceString("SourceNameNotSupportedForStyleTriggers");

	internal static string Stack_VisualInDifferentSubTree => GetResourceString("Stack_VisualInDifferentSubTree");

	internal static string StartIndexExceedsBufferSize => GetResourceString("StartIndexExceedsBufferSize");

	internal static string StartNodeMustBeDocumentPageViewOrFixedPage => GetResourceString("StartNodeMustBeDocumentPageViewOrFixedPage");

	internal static string StartNodeMustBeFixedPageProxy => GetResourceString("StartNodeMustBeFixedPageProxy");

	internal static string StaticResourceInXamlOnly => GetResourceString("StaticResourceInXamlOnly");

	internal static string Storyboard_AnimationMismatch => GetResourceString("Storyboard_AnimationMismatch");

	internal static string Storyboard_BeginStoryboardNameNotFound => GetResourceString("Storyboard_BeginStoryboardNameNotFound");

	internal static string Storyboard_BeginStoryboardNameRequired => GetResourceString("Storyboard_BeginStoryboardNameRequired");

	internal static string Storyboard_BeginStoryboardNoStoryboard => GetResourceString("Storyboard_BeginStoryboardNoStoryboard");

	internal static string Storyboard_ComplexPathNotSupported => GetResourceString("Storyboard_ComplexPathNotSupported");

	internal static string Storyboard_ImmutableTargetNotSupported => GetResourceString("Storyboard_ImmutableTargetNotSupported");

	internal static string Storyboard_MediaElementNotFound => GetResourceString("Storyboard_MediaElementNotFound");

	internal static string Storyboard_MediaElementRequired => GetResourceString("Storyboard_MediaElementRequired");

	internal static string Storyboard_NameNotFound => GetResourceString("Storyboard_NameNotFound");

	internal static string Storyboard_NeverApplied => GetResourceString("Storyboard_NeverApplied");

	internal static string Storyboard_NoNameScope => GetResourceString("Storyboard_NoNameScope");

	internal static string Storyboard_NoTarget => GetResourceString("Storyboard_NoTarget");

	internal static string Storyboard_PropertyPathEmpty => GetResourceString("Storyboard_PropertyPathEmpty");

	internal static string Storyboard_PropertyPathFrozenCheckFailed => GetResourceString("Storyboard_PropertyPathFrozenCheckFailed");

	internal static string Storyboard_PropertyPathIncludesNonAnimatableProperty => GetResourceString("Storyboard_PropertyPathIncludesNonAnimatableProperty");

	internal static string Storyboard_PropertyPathMustPointToDependencyObject => GetResourceString("Storyboard_PropertyPathMustPointToDependencyObject");

	internal static string Storyboard_PropertyPathMustPointToDependencyProperty => GetResourceString("Storyboard_PropertyPathMustPointToDependencyProperty");

	internal static string Storyboard_PropertyPathObjectNotFound => GetResourceString("Storyboard_PropertyPathObjectNotFound");

	internal static string Storyboard_PropertyPathPropertyNotFound => GetResourceString("Storyboard_PropertyPathPropertyNotFound");

	internal static string Storyboard_PropertyPathSealedCheckFailed => GetResourceString("Storyboard_PropertyPathSealedCheckFailed");

	internal static string Storyboard_PropertyPathUnresolved => GetResourceString("Storyboard_PropertyPathUnresolved");

	internal static string Storyboard_StoryboardReferenceRequired => GetResourceString("Storyboard_StoryboardReferenceRequired");

	internal static string Storyboard_TargetNameNotAllowedInStyle => GetResourceString("Storyboard_TargetNameNotAllowedInStyle");

	internal static string Storyboard_TargetNameNotDependencyObject => GetResourceString("Storyboard_TargetNameNotDependencyObject");

	internal static string Storyboard_TargetPropertyRequired => GetResourceString("Storyboard_TargetPropertyRequired");

	internal static string Storyboard_UnableToFreeze => GetResourceString("Storyboard_UnableToFreeze");

	internal static string Storyboard_UnrecognizedHandoffBehavior => GetResourceString("Storyboard_UnrecognizedHandoffBehavior");

	internal static string Storyboard_UnrecognizedTimeSeekOrigin => GetResourceString("Storyboard_UnrecognizedTimeSeekOrigin");

	internal static string StreamCannotBeWritten => GetResourceString("StreamCannotBeWritten");

	internal static string StreamDoesNotSupportSeek => GetResourceString("StreamDoesNotSupportSeek");

	internal static string StreamDoesNotSupportWrite => GetResourceString("StreamDoesNotSupportWrite");

	internal static string StreamNotSet => GetResourceString("StreamNotSet");

	internal static string StreamObjectDisposed => GetResourceString("StreamObjectDisposed");

	internal static string StringIdOutOfSequence => GetResourceString("StringIdOutOfSequence");

	internal static string StyleBasedOnHasLoop => GetResourceString("StyleBasedOnHasLoop");

	internal static string StyleCannotBeBasedOnSelf => GetResourceString("StyleCannotBeBasedOnSelf");

	internal static string StyleDataTriggerBindingHasBadValue => GetResourceString("StyleDataTriggerBindingHasBadValue");

	internal static string StyleDataTriggerBindingMissing => GetResourceString("StyleDataTriggerBindingMissing");

	internal static string StyleForWrongType => GetResourceString("StyleForWrongType");

	internal static string StyleHasTooManyElements => GetResourceString("StyleHasTooManyElements");

	internal static string StyleImpliedAndComplexChildren => GetResourceString("StyleImpliedAndComplexChildren");

	internal static string StyleInvalidElementTag => GetResourceString("StyleInvalidElementTag");

	internal static string StyleKnownTagWrongLocation => GetResourceString("StyleKnownTagWrongLocation");

	internal static string StyleNoClrEvent => GetResourceString("StyleNoClrEvent");

	internal static string StyleNoDef => GetResourceString("StyleNoDef");

	internal static string StyleNoDictionaryKey => GetResourceString("StyleNoDictionaryKey");

	internal static string StyleNoEventSetters => GetResourceString("StyleNoEventSetters");

	internal static string StyleNoPropOrEvent => GetResourceString("StyleNoPropOrEvent");

	internal static string StyleNoSetterResource => GetResourceString("StyleNoSetterResource");

	internal static string StyleNoTarget => GetResourceString("StyleNoTarget");

	internal static string StyleNoTemplateBindInSetters => GetResourceString("StyleNoTemplateBindInSetters");

	internal static string StyleNoTemplateBindInVisualTrigger => GetResourceString("StyleNoTemplateBindInVisualTrigger");

	internal static string StyleNoTopLevelElement => GetResourceString("StyleNoTopLevelElement");

	internal static string StylePropertyCustom => GetResourceString("StylePropertyCustom");

	internal static string StylePropertyInStyleNotAllowed => GetResourceString("StylePropertyInStyleNotAllowed");

	internal static string StylePropertySetterMinAttrs => GetResourceString("StylePropertySetterMinAttrs");

	internal static string StylePropTriggerPropMissing => GetResourceString("StylePropTriggerPropMissing");

	internal static string StyleSetterUnknownProp => GetResourceString("StyleSetterUnknownProp");

	internal static string StyleTagNotSupported => GetResourceString("StyleTagNotSupported");

	internal static string StyleTargetTypeMismatchWithElement => GetResourceString("StyleTargetTypeMismatchWithElement");

	internal static string StyleTextNotSupported => GetResourceString("StyleTextNotSupported");

	internal static string StyleTriggersCannotTargetTheTemplate => GetResourceString("StyleTriggersCannotTargetTheTemplate");

	internal static string StyleUnknownProp => GetResourceString("StyleUnknownProp");

	internal static string StyleUnknownTrigger => GetResourceString("StyleUnknownTrigger");

	internal static string StyleValueOfExpressionNotSupported => GetResourceString("StyleValueOfExpressionNotSupported");

	internal static string SystemResourceForTypeIsNotStyle => GetResourceString("SystemResourceForTypeIsNotStyle");

	internal static string TableCollectionCountNeedNonNegNum => GetResourceString("TableCollectionCountNeedNonNegNum");

	internal static string TableCollectionElementTypeExpected => GetResourceString("TableCollectionElementTypeExpected");

	internal static string TableCollectionInOtherCollection => GetResourceString("TableCollectionInOtherCollection");

	internal static string TableCollectionInvalidOffLen => GetResourceString("TableCollectionInvalidOffLen");

	internal static string TableCollectionNotEnoughCapacity => GetResourceString("TableCollectionNotEnoughCapacity");

	internal static string TableCollectionOutOfRange => GetResourceString("TableCollectionOutOfRange");

	internal static string TableCollectionOutOfRangeNeedNonNegNum => GetResourceString("TableCollectionOutOfRangeNeedNonNegNum");

	internal static string TableCollectionRangeOutOfRange => GetResourceString("TableCollectionRangeOutOfRange");

	internal static string TableCollectionRankMultiDimNotSupported => GetResourceString("TableCollectionRankMultiDimNotSupported");

	internal static string TableCollectionWrongProxyParent => GetResourceString("TableCollectionWrongProxyParent");

	internal static string TableInvalidParentNodeType => GetResourceString("TableInvalidParentNodeType");

	internal static string TargetNameNotFound => GetResourceString("TargetNameNotFound");

	internal static string TargetNameNotSupportedForStyleSetters => GetResourceString("TargetNameNotSupportedForStyleSetters");

	internal static string Template3DValueOnly => GetResourceString("Template3DValueOnly");

	internal static string TemplateBadDictionaryKey => GetResourceString("TemplateBadDictionaryKey");

	internal static string TemplateCannotHaveNestedContentPresenterAndGridViewRowPresenter => GetResourceString("TemplateCannotHaveNestedContentPresenterAndGridViewRowPresenter");

	internal static string TemplateChildIndexOutOfRange => GetResourceString("TemplateChildIndexOutOfRange");

	internal static string TemplateCircularReferenceFound => GetResourceString("TemplateCircularReferenceFound");

	internal static string TemplateContentSetTwice => GetResourceString("TemplateContentSetTwice");

	internal static string TemplateDupName => GetResourceString("TemplateDupName");

	internal static string TemplateFindNameInInvalidElement => GetResourceString("TemplateFindNameInInvalidElement");

	internal static string TemplateHasNestedNameScope => GetResourceString("TemplateHasNestedNameScope");

	internal static string TemplateInvalidBamlRecord => GetResourceString("TemplateInvalidBamlRecord");

	internal static string TemplateInvalidRootElementTag => GetResourceString("TemplateInvalidRootElementTag");

	internal static string TemplateKnownTagWrongLocation => GetResourceString("TemplateKnownTagWrongLocation");

	internal static string TemplateMustBeFE => GetResourceString("TemplateMustBeFE");

	internal static string TemplateNoMultipleRoots => GetResourceString("TemplateNoMultipleRoots");

	internal static string TemplateNoProp => GetResourceString("TemplateNoProp");

	internal static string TemplateNoTarget => GetResourceString("TemplateNoTarget");

	internal static string TemplateNoTemplateBindInVisualTrigger => GetResourceString("TemplateNoTemplateBindInVisualTrigger");

	internal static string TemplateNoTriggerTarget => GetResourceString("TemplateNoTriggerTarget");

	internal static string TemplateNotTargetType => GetResourceString("TemplateNotTargetType");

	internal static string TemplateTagNotSupported => GetResourceString("TemplateTagNotSupported");

	internal static string TemplateTargetTypeMismatch => GetResourceString("TemplateTargetTypeMismatch");

	internal static string TemplateTextNotSupported => GetResourceString("TemplateTextNotSupported");

	internal static string TemplateUnknownProp => GetResourceString("TemplateUnknownProp");

	internal static string TextBoxBase_CantSetIsUndoEnabledInsideChangeBlock => GetResourceString("TextBoxBase_CantSetIsUndoEnabledInsideChangeBlock");

	internal static string TextBoxBase_UnmatchedEndChange => GetResourceString("TextBoxBase_UnmatchedEndChange");

	internal static string TextBoxDecoratorMarkedAsTextBoxContentMustHaveNoContent => GetResourceString("TextBoxDecoratorMarkedAsTextBoxContentMustHaveNoContent");

	internal static string TextBoxInvalidChild => GetResourceString("TextBoxInvalidChild");

	internal static string TextBoxInvalidTextContainer => GetResourceString("TextBoxInvalidTextContainer");

	internal static string TextBoxMinMaxLinesMismatch => GetResourceString("TextBoxMinMaxLinesMismatch");

	internal static string TextBoxScrollViewerMarkedAsTextBoxContentMustHaveNoContent => GetResourceString("TextBoxScrollViewerMarkedAsTextBoxContentMustHaveNoContent");

	internal static string TextBox_ContextMenu_Copy => GetResourceString("TextBox_ContextMenu_Copy");

	internal static string TextBox_ContextMenu_Cut => GetResourceString("TextBox_ContextMenu_Cut");

	internal static string TextBox_ContextMenu_Description_DBCSSpace => GetResourceString("TextBox_ContextMenu_Description_DBCSSpace");

	internal static string TextBox_ContextMenu_Description_SBCSSpace => GetResourceString("TextBox_ContextMenu_Description_SBCSSpace");

	internal static string TextBox_ContextMenu_IgnoreAll => GetResourceString("TextBox_ContextMenu_IgnoreAll");

	internal static string TextBox_ContextMenu_More => GetResourceString("TextBox_ContextMenu_More");

	internal static string TextBox_ContextMenu_NoSpellingSuggestions => GetResourceString("TextBox_ContextMenu_NoSpellingSuggestions");

	internal static string TextBox_ContextMenu_Paste => GetResourceString("TextBox_ContextMenu_Paste");

	internal static string TextContainerChangingReentrancyInvalid => GetResourceString("TextContainerChangingReentrancyInvalid");

	internal static string TextContainerDoesNotContainElement => GetResourceString("TextContainerDoesNotContainElement");

	internal static string TextContainer_UndoManagerCreatedMoreThanOnce => GetResourceString("TextContainer_UndoManagerCreatedMoreThanOnce");

	internal static string TextEditorCanNotRegisterCommandHandler => GetResourceString("TextEditorCanNotRegisterCommandHandler");

	internal static string TextEditorCopyPaste_EntryPartIsMissingInXamlPackage => GetResourceString("TextEditorCopyPaste_EntryPartIsMissingInXamlPackage");

	internal static string TextEditorPropertyIsNotApplicableForTextFormatting => GetResourceString("TextEditorPropertyIsNotApplicableForTextFormatting");

	internal static string TextEditorSpellerInteropHasBeenDisposed => GetResourceString("TextEditorSpellerInteropHasBeenDisposed");

	internal static string TextEditorTypeOfParameterIsNotAppropriateForFormattingProperty => GetResourceString("TextEditorTypeOfParameterIsNotAppropriateForFormattingProperty");

	internal static string TextElementCollection_CannotCopyToArrayNotSufficientMemory => GetResourceString("TextElementCollection_CannotCopyToArrayNotSufficientMemory");

	internal static string TextElementCollection_IndexOutOfRange => GetResourceString("TextElementCollection_IndexOutOfRange");

	internal static string TextElementCollection_ItemHasUnexpectedType => GetResourceString("TextElementCollection_ItemHasUnexpectedType");

	internal static string TextElementCollection_NextSiblingDoesNotBelongToThisCollection => GetResourceString("TextElementCollection_NextSiblingDoesNotBelongToThisCollection");

	internal static string TextElementCollection_NoEnumerator => GetResourceString("TextElementCollection_NoEnumerator");

	internal static string TextElementCollection_PreviousSiblingDoesNotBelongToThisCollection => GetResourceString("TextElementCollection_PreviousSiblingDoesNotBelongToThisCollection");

	internal static string TextElementCollection_TextElementTypeExpected => GetResourceString("TextElementCollection_TextElementTypeExpected");

	internal static string TextElement_UnmatchedEndPointer => GetResourceString("TextElement_UnmatchedEndPointer");

	internal static string TextPanelIllegalParaTypeForIAddChild => GetResourceString("TextPanelIllegalParaTypeForIAddChild");

	internal static string TextPointer_CannotInsertTextElementBecauseItBelongsToAnotherTree => GetResourceString("TextPointer_CannotInsertTextElementBecauseItBelongsToAnotherTree");

	internal static string TextPositionIsFrozen => GetResourceString("TextPositionIsFrozen");

	internal static string TextProvider_InvalidChildElement => GetResourceString("TextProvider_InvalidChildElement");

	internal static string TextProvider_InvalidPoint => GetResourceString("TextProvider_InvalidPoint");

	internal static string TextProvider_TextSelectionNotSupported => GetResourceString("TextProvider_TextSelectionNotSupported");

	internal static string TextRangeEdit_InvalidStructuralPropertyApply => GetResourceString("TextRangeEdit_InvalidStructuralPropertyApply");

	internal static string TextRangeProvider_EmptyStringParameter => GetResourceString("TextRangeProvider_EmptyStringParameter");

	internal static string TextRangeProvider_InvalidParameterValue => GetResourceString("TextRangeProvider_InvalidParameterValue");

	internal static string TextRangeProvider_WrongTextRange => GetResourceString("TextRangeProvider_WrongTextRange");

	internal static string TextRange_InvalidParameterValue => GetResourceString("TextRange_InvalidParameterValue");

	internal static string TextRange_PropertyCannotBeIncrementedOrDecremented => GetResourceString("TextRange_PropertyCannotBeIncrementedOrDecremented");

	internal static string TextRange_UnrecognizedStructureInDataFormat => GetResourceString("TextRange_UnrecognizedStructureInDataFormat");

	internal static string TextRange_UnsupportedDataFormat => GetResourceString("TextRange_UnsupportedDataFormat");

	internal static string TextSchema_CannotInsertContentInThisPosition => GetResourceString("TextSchema_CannotInsertContentInThisPosition");

	internal static string TextSchema_CannotSplitElement => GetResourceString("TextSchema_CannotSplitElement");

	internal static string TextSchema_ChildTypeIsInvalid => GetResourceString("TextSchema_ChildTypeIsInvalid");

	internal static string TextSchema_IllegalElement => GetResourceString("TextSchema_IllegalElement");

	internal static string TextSchema_IllegalHyperlinkChild => GetResourceString("TextSchema_IllegalHyperlinkChild");

	internal static string TextSchema_TextIsNotAllowed => GetResourceString("TextSchema_TextIsNotAllowed");

	internal static string TextSchema_TextIsNotAllowedInThisContext => GetResourceString("TextSchema_TextIsNotAllowedInThisContext");

	internal static string TextSchema_TheChildElementBelongsToAnotherTreeAlready => GetResourceString("TextSchema_TheChildElementBelongsToAnotherTreeAlready");

	internal static string TextSchema_ThisBlockUIContainerHasAChildUIElementAlready => GetResourceString("TextSchema_ThisBlockUIContainerHasAChildUIElementAlready");

	internal static string TextSchema_ThisInlineUIContainerHasAChildUIElementAlready => GetResourceString("TextSchema_ThisInlineUIContainerHasAChildUIElementAlready");

	internal static string TextSchema_UIElementNotAllowedInThisPosition => GetResourceString("TextSchema_UIElementNotAllowedInThisPosition");

	internal static string TextSegmentsMustNotOverlap => GetResourceString("TextSegmentsMustNotOverlap");

	internal static string TextStore_BadIMECharOffset => GetResourceString("TextStore_BadIMECharOffset");

	internal static string TextStore_BadLockFlags => GetResourceString("TextStore_BadLockFlags");

	internal static string TextStore_CompositionRejected => GetResourceString("TextStore_CompositionRejected");

	internal static string TextStore_CONNECT_E_CANNOTCONNECT => GetResourceString("TextStore_CONNECT_E_CANNOTCONNECT");

	internal static string TextStore_CONNECT_E_NOCONNECTION => GetResourceString("TextStore_CONNECT_E_NOCONNECTION");

	internal static string TextStore_E_NOINTERFACE => GetResourceString("TextStore_E_NOINTERFACE");

	internal static string TextStore_E_NOTIMPL => GetResourceString("TextStore_E_NOTIMPL");

	internal static string TextStore_NoSink => GetResourceString("TextStore_NoSink");

	internal static string TextStore_ReentrantRequestLock => GetResourceString("TextStore_ReentrantRequestLock");

	internal static string TextStore_TS_E_FORMAT => GetResourceString("TextStore_TS_E_FORMAT");

	internal static string TextStore_TS_E_INVALIDPOINT => GetResourceString("TextStore_TS_E_INVALIDPOINT");

	internal static string TextStore_TS_E_NOLAYOUT => GetResourceString("TextStore_TS_E_NOLAYOUT");

	internal static string TextStore_TS_E_READONLY => GetResourceString("TextStore_TS_E_READONLY");

	internal static string TextViewInvalidLayout => GetResourceString("TextViewInvalidLayout");

	internal static string ThemeDictionaryExtension_Name => GetResourceString("ThemeDictionaryExtension_Name");

	internal static string ThemeDictionaryExtension_Source => GetResourceString("ThemeDictionaryExtension_Source");

	internal static string ToolBar_InvalidStyle_ToolBarOverflowPanel => GetResourceString("ToolBar_InvalidStyle_ToolBarOverflowPanel");

	internal static string ToolBar_InvalidStyle_ToolBarPanel => GetResourceString("ToolBar_InvalidStyle_ToolBarPanel");

	internal static string ToolTipStaysOpenFalseNotAllowed => GetResourceString("ToolTipStaysOpenFalseNotAllowed");

	internal static string ToStringFormatString_Control => GetResourceString("ToStringFormatString_Control");

	internal static string ToStringFormatString_GridView => GetResourceString("ToStringFormatString_GridView");

	internal static string ToStringFormatString_GridViewColumn => GetResourceString("ToStringFormatString_GridViewColumn");

	internal static string ToStringFormatString_GridViewRowPresenter => GetResourceString("ToStringFormatString_GridViewRowPresenter");

	internal static string ToStringFormatString_GridViewRowPresenterBase => GetResourceString("ToStringFormatString_GridViewRowPresenterBase");

	internal static string ToStringFormatString_HeaderedContentControl => GetResourceString("ToStringFormatString_HeaderedContentControl");

	internal static string ToStringFormatString_HeaderedItemsControl => GetResourceString("ToStringFormatString_HeaderedItemsControl");

	internal static string ToStringFormatString_ItemsControl => GetResourceString("ToStringFormatString_ItemsControl");

	internal static string ToStringFormatString_RangeBase => GetResourceString("ToStringFormatString_RangeBase");

	internal static string ToStringFormatString_ToggleButton => GetResourceString("ToStringFormatString_ToggleButton");

	internal static string Track_SameButtons => GetResourceString("Track_SameButtons");

	internal static string TransformNotSupported => GetResourceString("TransformNotSupported");

	internal static string TriggerActionAlreadySealed => GetResourceString("TriggerActionAlreadySealed");

	internal static string TriggerActionMustBelongToASingleTrigger => GetResourceString("TriggerActionMustBelongToASingleTrigger");

	internal static string TriggerOnStyleNotAllowedToHaveSource => GetResourceString("TriggerOnStyleNotAllowedToHaveSource");

	internal static string TriggersSupportsEventTriggersOnly => GetResourceString("TriggersSupportsEventTriggersOnly");

	internal static string TrustNotGrantedText => GetResourceString("TrustNotGrantedText");

	internal static string TrustNotGrantedTitle => GetResourceString("TrustNotGrantedTitle");

	internal static string TwoWayBindingNeedsPath => GetResourceString("TwoWayBindingNeedsPath");

	internal static string TypeIdOutOfSequence => GetResourceString("TypeIdOutOfSequence");

	internal static string TypeMustImplementIAddChild => GetResourceString("TypeMustImplementIAddChild");

	internal static string TypeNameMustBeSpecified => GetResourceString("TypeNameMustBeSpecified");

	internal static string TypeValueSerializerUnavailable => GetResourceString("TypeValueSerializerUnavailable");

	internal static string UIA_OperationCannotBePerformed => GetResourceString("UIA_OperationCannotBePerformed");

	internal static string UiLessPageFunctionNotCallingOnReturn => GetResourceString("UiLessPageFunctionNotCallingOnReturn");

	internal static string UnableToConvertInt32 => GetResourceString("UnableToConvertInt32");

	internal static string UnableToLocateResource => GetResourceString("UnableToLocateResource");

	internal static string UndefinedHighlightAnchor => GetResourceString("UndefinedHighlightAnchor");

	internal static string UndoContainerTypeMismatch => GetResourceString("UndoContainerTypeMismatch");

	internal static string UndoManagerAlreadyAttached => GetResourceString("UndoManagerAlreadyAttached");

	internal static string UndoNoOpenParentUnit => GetResourceString("UndoNoOpenParentUnit");

	internal static string UndoNoOpenUnit => GetResourceString("UndoNoOpenUnit");

	internal static string UndoNotInNormalState => GetResourceString("UndoNotInNormalState");

	internal static string UndoServiceDisabled => GetResourceString("UndoServiceDisabled");

	internal static string UndoUnitAlreadyOpen => GetResourceString("UndoUnitAlreadyOpen");

	internal static string UndoUnitCantBeAddedTwice => GetResourceString("UndoUnitCantBeAddedTwice");

	internal static string UndoUnitCantBeOpenedTwice => GetResourceString("UndoUnitCantBeOpenedTwice");

	internal static string UndoUnitLocked => GetResourceString("UndoUnitLocked");

	internal static string UndoUnitNotFound => GetResourceString("UndoUnitNotFound");

	internal static string UndoUnitNotOnTopOfStack => GetResourceString("UndoUnitNotOnTopOfStack");

	internal static string UndoUnitOpen => GetResourceString("UndoUnitOpen");

	internal static string UnexpectedAttribute => GetResourceString("UnexpectedAttribute");

	internal static string UnexpectedCollectionChangeAction => GetResourceString("UnexpectedCollectionChangeAction");

	internal static string UnexpectedProperty => GetResourceString("UnexpectedProperty");

	internal static string UnexpectedType => GetResourceString("UnexpectedType");

	internal static string UnexpectedValueTypeForCondition => GetResourceString("UnexpectedValueTypeForCondition");

	internal static string UnexpectedValueTypeForDataTrigger => GetResourceString("UnexpectedValueTypeForDataTrigger");

	internal static string UnexpectedXmlNodeInXmlFixedPageInfoConstructor => GetResourceString("UnexpectedXmlNodeInXmlFixedPageInfoConstructor");

	internal static string UnknownBamlRecord => GetResourceString("UnknownBamlRecord");

	internal static string UnknownContainerFormat => GetResourceString("UnknownContainerFormat");

	internal static string UnknownErrorText => GetResourceString("UnknownErrorText");

	internal static string UnknownErrorTitle => GetResourceString("UnknownErrorTitle");

	internal static string UnknownIndexType => GetResourceString("UnknownIndexType");

	internal static string UnmatchedBracket => GetResourceString("UnmatchedBracket");

	internal static string UnmatchedLocComment => GetResourceString("UnmatchedLocComment");

	internal static string UnmatchedParen => GetResourceString("UnmatchedParen");

	internal static string UnRecognizedBamlNodeType => GetResourceString("UnRecognizedBamlNodeType");

	internal static string UnserializableKeyValue => GetResourceString("UnserializableKeyValue");

	internal static string UnsupportedTriggerInStyle => GetResourceString("UnsupportedTriggerInStyle");

	internal static string UnsupportedTriggerInTemplate => GetResourceString("UnsupportedTriggerInTemplate");

	internal static string Untitled => GetResourceString("Untitled");

	internal static string UntitledPrintJobDescription => GetResourceString("UntitledPrintJobDescription");

	internal static string UriNotMatchWithRootType => GetResourceString("UriNotMatchWithRootType");

	internal static string ValidationRule_UnexpectedValue => GetResourceString("ValidationRule_UnexpectedValue");

	internal static string ValidationRule_UnknownStep => GetResourceString("ValidationRule_UnknownStep");

	internal static string Validation_ConversionFailed => GetResourceString("Validation_ConversionFailed");

	internal static string ValueMustBeXamlReader => GetResourceString("ValueMustBeXamlReader");

	internal static string ValueNotBetweenInt32MinMax => GetResourceString("ValueNotBetweenInt32MinMax");

	internal static string ValueSerializerContextUnavailable => GetResourceString("ValueSerializerContextUnavailable");

	internal static string VirtualizedCellInfoCollection_DoesNotSupportIndexChanges => GetResourceString("VirtualizedCellInfoCollection_DoesNotSupportIndexChanges");

	internal static string VirtualizedCellInfoCollection_IsReadOnly => GetResourceString("VirtualizedCellInfoCollection_IsReadOnly");

	internal static string VirtualizedElement => GetResourceString("VirtualizedElement");

	internal static string VisualTreeRootIsFrameworkElement => GetResourceString("VisualTreeRootIsFrameworkElement");

	internal static string VisualTriggerSettersIncludeUnsupportedSetterType => GetResourceString("VisualTriggerSettersIncludeUnsupportedSetterType");

	internal static string WebBrowserNoCastToIWebBrowser2 => GetResourceString("WebBrowserNoCastToIWebBrowser2");

	internal static string WebBrowserOverlap => GetResourceString("WebBrowserOverlap");

	internal static string WebRequestCreationFailed => GetResourceString("WebRequestCreationFailed");

	internal static string WindowAlreadyClosed => GetResourceString("WindowAlreadyClosed");

	internal static string WindowMustBeRoot => GetResourceString("WindowMustBeRoot");

	internal static string WindowPassedShouldBeOnApplicationThread => GetResourceString("WindowPassedShouldBeOnApplicationThread");

	internal static string WpfPayload_InvalidImageSource => GetResourceString("WpfPayload_InvalidImageSource");

	internal static string WriteNotSupported => GetResourceString("WriteNotSupported");

	internal static string WrongNavigateRootElement => GetResourceString("WrongNavigateRootElement");

	internal static string WrongSelectionType => GetResourceString("WrongSelectionType");

	internal static string XamlFilterNestedFixedPage => GetResourceString("XamlFilterNestedFixedPage");

	internal static string XmlGlyphRunInfoIsNonGraphic => GetResourceString("XmlGlyphRunInfoIsNonGraphic");

	internal static string XmlNodeAlreadyOwned => GetResourceString("XmlNodeAlreadyOwned");

	internal static string XpsValidatingLoaderDiscardControlHasIncorrectType => GetResourceString("XpsValidatingLoaderDiscardControlHasIncorrectType");

	internal static string XpsValidatingLoaderDuplicateReference => GetResourceString("XpsValidatingLoaderDuplicateReference");

	internal static string XpsValidatingLoaderMoreThanOneDiscardControlInPackage => GetResourceString("XpsValidatingLoaderMoreThanOneDiscardControlInPackage");

	internal static string XpsValidatingLoaderMoreThanOnePrintTicketPart => GetResourceString("XpsValidatingLoaderMoreThanOnePrintTicketPart");

	internal static string XpsValidatingLoaderMoreThanOneThumbnailInPackage => GetResourceString("XpsValidatingLoaderMoreThanOneThumbnailInPackage");

	internal static string XpsValidatingLoaderMoreThanOneThumbnailPart => GetResourceString("XpsValidatingLoaderMoreThanOneThumbnailPart");

	internal static string XpsValidatingLoaderPrintTicketHasIncorrectType => GetResourceString("XpsValidatingLoaderPrintTicketHasIncorrectType");

	internal static string XpsValidatingLoaderRestrictedFontHasIncorrectType => GetResourceString("XpsValidatingLoaderRestrictedFontHasIncorrectType");

	internal static string XpsValidatingLoaderThumbnailHasIncorrectType => GetResourceString("XpsValidatingLoaderThumbnailHasIncorrectType");

	internal static string XpsValidatingLoaderUnlistedResource => GetResourceString("XpsValidatingLoaderUnlistedResource");

	internal static string XpsValidatingLoaderUnsupportedEncoding => GetResourceString("XpsValidatingLoaderUnsupportedEncoding");

	internal static string XpsValidatingLoaderUnsupportedMimeType => GetResourceString("XpsValidatingLoaderUnsupportedMimeType");

	internal static string XpsValidatingLoaderUnsupportedRootNamespaceUri => GetResourceString("XpsValidatingLoaderUnsupportedRootNamespaceUri");

	internal static string XpsValidatingLoaderUriNotInSamePackage => GetResourceString("XpsValidatingLoaderUriNotInSamePackage");

	internal static string Animation_ChildMustBeKeyFrame => GetResourceString("Animation_ChildMustBeKeyFrame");

	internal static string Animation_InvalidAnimationUsingKeyFramesDuration => GetResourceString("Animation_InvalidAnimationUsingKeyFramesDuration");

	internal static string Animation_InvalidBaseValue => GetResourceString("Animation_InvalidBaseValue");

	internal static string Animation_InvalidResolvedKeyTimes => GetResourceString("Animation_InvalidResolvedKeyTimes");

	internal static string Animation_InvalidTimeKeyTime => GetResourceString("Animation_InvalidTimeKeyTime");

	internal static string Animation_Invalid_DefaultValue => GetResourceString("Animation_Invalid_DefaultValue");

	internal static string Animation_NoTextChildren => GetResourceString("Animation_NoTextChildren");

	internal static string BrowserHostingNotSupported => GetResourceString("BrowserHostingNotSupported");

	internal static string CannotConvertStringToType => GetResourceString("CannotConvertStringToType");

	internal static string CannotConvertType => GetResourceString("CannotConvertType");

	internal static string CannotModifyReadOnlyContainer => GetResourceString("CannotModifyReadOnlyContainer");

	internal static string CannotRetrievePartsOfWriteOnlyContainer => GetResourceString("CannotRetrievePartsOfWriteOnlyContainer");

	internal static string CollectionNumberOfElementsMustBeLessOrEqualTo => GetResourceString("CollectionNumberOfElementsMustBeLessOrEqualTo");

	internal static string Collection_BadType => GetResourceString("Collection_BadType");

	internal static string Collection_CopyTo_ArrayCannotBeMultidimensional => GetResourceString("Collection_CopyTo_ArrayCannotBeMultidimensional");

	internal static string Collection_CopyTo_IndexGreaterThanOrEqualToArrayLength => GetResourceString("Collection_CopyTo_IndexGreaterThanOrEqualToArrayLength");

	internal static string Collection_CopyTo_NumberOfElementsExceedsArrayLength => GetResourceString("Collection_CopyTo_NumberOfElementsExceedsArrayLength");

	internal static string Enumerator_VerifyContext => GetResourceString("Enumerator_VerifyContext");

	internal static string Enum_Invalid => GetResourceString("Enum_Invalid");

	internal static string FileFormatException => GetResourceString("FileFormatException");

	internal static string FileFormatExceptionWithFileName => GetResourceString("FileFormatExceptionWithFileName");

	internal static string Freezable_CantBeFrozen => GetResourceString("Freezable_CantBeFrozen");

	internal static string InvalidPermissionStateValue => GetResourceString("InvalidPermissionStateValue");

	internal static string InvalidPermissionType => GetResourceString("InvalidPermissionType");

	internal static string ParameterCannotBeNegative => GetResourceString("ParameterCannotBeNegative");

	internal static string SecurityExceptionForSettingSandboxExternalToTrue => GetResourceString("SecurityExceptionForSettingSandboxExternalToTrue");

	internal static string StringEmpty => GetResourceString("StringEmpty");

	internal static string TokenizerHelperEmptyToken => GetResourceString("TokenizerHelperEmptyToken");

	internal static string TokenizerHelperExtraDataEncountered => GetResourceString("TokenizerHelperExtraDataEncountered");

	internal static string TokenizerHelperMissingEndQuote => GetResourceString("TokenizerHelperMissingEndQuote");

	internal static string TokenizerHelperPrematureStringTermination => GetResourceString("TokenizerHelperPrematureStringTermination");

	internal static string TypeMetadataCannotChangeAfterUse => GetResourceString("TypeMetadataCannotChangeAfterUse");

	internal static string UnexpectedParameterType => GetResourceString("UnexpectedParameterType");

	internal static string Visual_ArgumentOutOfRange => GetResourceString("Visual_ArgumentOutOfRange");

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool UsingResourceKeys()
	{
		return false;
	}

	internal static string GetResourceString(string resourceKey)
	{
		string result = null;
		try
		{
			result = ResourceManager.GetString(resourceKey);
		}
		catch (MissingManifestResourceException)
		{
		}
		return result;
	}

	internal static string GetResourceString(string resourceKey, string defaultString)
	{
		string resourceString = GetResourceString(resourceKey);
		if (defaultString != null && resourceKey.Equals(resourceString, StringComparison.Ordinal))
		{
			return defaultString;
		}
		return resourceString;
	}

	internal static string Format(string resourceFormat, params object[] args)
	{
		if (args != null)
		{
			if (UsingResourceKeys())
			{
				return resourceFormat + string.Join(", ", args);
			}
			return string.Format(resourceFormat, args);
		}
		return resourceFormat;
	}

	internal static string Format(string resourceFormat, object p1)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1);
		}
		return string.Format(resourceFormat, p1);
	}

	internal static string Format(string resourceFormat, object p1, object p2)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2);
		}
		return string.Format(resourceFormat, p1, p2);
	}

	internal static string Format(string resourceFormat, object p1, object p2, object p3)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2, p3);
		}
		return string.Format(resourceFormat, p1, p2, p3);
	}
}
