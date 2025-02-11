using System;
using System.Resources;
using System.Runtime.CompilerServices;
using FxResources.PresentationCore;

namespace MS.Internal.PresentationCore;

internal static class SR
{
	private static ResourceManager s_resourceManager;

	internal static ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new ResourceManager(typeof(FxResources.PresentationCore.SR)));

	internal static string AccessKeyManager_NotAUnicodeCharacter => GetResourceString("AccessKeyManager_NotAUnicodeCharacter");

	internal static string AcquireBreakRecordFailure => GetResourceString("AcquireBreakRecordFailure");

	internal static string AcquirePenaltyModuleFailure => GetResourceString("AcquirePenaltyModuleFailure");

	internal static string AddText_Invalid => GetResourceString("AddText_Invalid");

	internal static string AllGesturesMustExistAlone => GetResourceString("AllGesturesMustExistAlone");

	internal static string Animation_AnimationTimelineTypeMismatch => GetResourceString("Animation_AnimationTimelineTypeMismatch");

	internal static string Animation_CalculatedValueIsInvalidForProperty => GetResourceString("Animation_CalculatedValueIsInvalidForProperty");

	internal static string Animation_ChildMustBeKeyFrame => GetResourceString("Animation_ChildMustBeKeyFrame");

	internal static string Animation_ChildTypeMismatch => GetResourceString("Animation_ChildTypeMismatch");

	internal static string Animation_DependencyPropertyIsNotAnimatable => GetResourceString("Animation_DependencyPropertyIsNotAnimatable");

	internal static string Animation_Exception => GetResourceString("Animation_Exception");

	internal static string Animation_InvalidBaseValue => GetResourceString("Animation_InvalidBaseValue");

	internal static string Animation_InvalidResolvedKeyTimes => GetResourceString("Animation_InvalidResolvedKeyTimes");

	internal static string Animation_InvalidTimeKeyTime => GetResourceString("Animation_InvalidTimeKeyTime");

	internal static string Animation_Invalid_DefaultValue => GetResourceString("Animation_Invalid_DefaultValue");

	internal static string Animation_KeySpline_InvalidValue => GetResourceString("Animation_KeySpline_InvalidValue");

	internal static string Animation_KeyTime_InvalidPercentValue => GetResourceString("Animation_KeyTime_InvalidPercentValue");

	internal static string Animation_KeyTime_LessThanZero => GetResourceString("Animation_KeyTime_LessThanZero");

	internal static string Animation_NoAnimationsSpecified => GetResourceString("Animation_NoAnimationsSpecified");

	internal static string Animation_NoTextChildren => GetResourceString("Animation_NoTextChildren");

	internal static string Animation_ReturnedUnsetValueInstance => GetResourceString("Animation_ReturnedUnsetValueInstance");

	internal static string Animation_UnrecognizedHandoffBehavior => GetResourceString("Animation_UnrecognizedHandoffBehavior");

	internal static string AnimEffect_AlreadyAttached => GetResourceString("AnimEffect_AlreadyAttached");

	internal static string AnimEffect_CollectionInUse => GetResourceString("AnimEffect_CollectionInUse");

	internal static string AnimEffect_NoVisual => GetResourceString("AnimEffect_NoVisual");

	internal static string ApplicationGestureArrayLengthIsZero => GetResourceString("ApplicationGestureArrayLengthIsZero");

	internal static string ApplicationGestureIsInvalid => GetResourceString("ApplicationGestureIsInvalid");

	internal static string AutomationDispatcherShutdown => GetResourceString("AutomationDispatcherShutdown");

	internal static string AutomationTimeout => GetResourceString("AutomationTimeout");

	internal static string Automation_InvalidConnectedPeer => GetResourceString("Automation_InvalidConnectedPeer");

	internal static string Automation_InvalidEventId => GetResourceString("Automation_InvalidEventId");

	internal static string Automation_InvalidSynchronizedInputType => GetResourceString("Automation_InvalidSynchronizedInputType");

	internal static string Automation_RecursivePublicCall => GetResourceString("Automation_RecursivePublicCall");

	internal static string Automation_UnsupportedUIAutomationEventAssociation => GetResourceString("Automation_UnsupportedUIAutomationEventAssociation");

	internal static string BitmapCacheBrush_OpacityChanged => GetResourceString("BitmapCacheBrush_OpacityChanged");

	internal static string BitmapCacheBrush_RelativeTransformChanged => GetResourceString("BitmapCacheBrush_RelativeTransformChanged");

	internal static string BitmapCacheBrush_TransformChanged => GetResourceString("BitmapCacheBrush_TransformChanged");

	internal static string BrowseBackKeyDisplayString => GetResourceString("BrowseBackKeyDisplayString");

	internal static string BrowseBackText => GetResourceString("BrowseBackText");

	internal static string BrowseForwardKeyDisplayString => GetResourceString("BrowseForwardKeyDisplayString");

	internal static string BrowseForwardText => GetResourceString("BrowseForwardText");

	internal static string BrowseHomeKeyDisplayString => GetResourceString("BrowseHomeKeyDisplayString");

	internal static string BrowseHomeText => GetResourceString("BrowseHomeText");

	internal static string BrowseStopKeyDisplayString => GetResourceString("BrowseStopKeyDisplayString");

	internal static string BrowseStopText => GetResourceString("BrowseStopText");

	internal static string BrushUnknownBamlType => GetResourceString("BrushUnknownBamlType");

	internal static string ByteRangeDownloaderDisposed => GetResourceString("ByteRangeDownloaderDisposed");

	internal static string ByteRangeDownloaderErroredOut => GetResourceString("ByteRangeDownloaderErroredOut");

	internal static string ByteRangeRequestIsNotSupported => GetResourceString("ByteRangeRequestIsNotSupported");

	internal static string CancelPrintText => GetResourceString("CancelPrintText");

	internal static string CannotAttachVisualTwice => GetResourceString("CannotAttachVisualTwice");

	internal static string CannotBothBeNull => GetResourceString("CannotBothBeNull");

	internal static string CannotConvertStringToType => GetResourceString("CannotConvertStringToType");

	internal static string CannotConvertType => GetResourceString("CannotConvertType");

	internal static string CannotModifyReadOnlyContainer => GetResourceString("CannotModifyReadOnlyContainer");

	internal static string CannotModifyVisualChildrenDuringTreeWalk => GetResourceString("CannotModifyVisualChildrenDuringTreeWalk");

	internal static string CannotNavigateToApplicationResourcesInWebBrowser => GetResourceString("CannotNavigateToApplicationResourcesInWebBrowser");

	internal static string CannotRetrievePartsOfWriteOnlyContainer => GetResourceString("CannotRetrievePartsOfWriteOnlyContainer");

	internal static string Channel_InvalidCommandBufferPointer => GetResourceString("Channel_InvalidCommandBufferPointer");

	internal static string CharacterMetrics_MissingRequiredField => GetResourceString("CharacterMetrics_MissingRequiredField");

	internal static string CharacterMetrics_NegativeHorizontalAdvance => GetResourceString("CharacterMetrics_NegativeHorizontalAdvance");

	internal static string CharacterMetrics_NegativeVerticalAdvance => GetResourceString("CharacterMetrics_NegativeVerticalAdvance");

	internal static string CharacterMetrics_TooManyFields => GetResourceString("CharacterMetrics_TooManyFields");

	internal static string ClassTypeIllegal => GetResourceString("ClassTypeIllegal");

	internal static string CloneBreakRecordFailure => GetResourceString("CloneBreakRecordFailure");

	internal static string CloseText => GetResourceString("CloseText");

	internal static string ClusterMapEntriesShouldNotDecrease => GetResourceString("ClusterMapEntriesShouldNotDecrease");

	internal static string ClusterMapEntryShouldPointWithinGlyphIndices => GetResourceString("ClusterMapEntryShouldPointWithinGlyphIndices");

	internal static string ClusterMapFirstEntryMustBeZero => GetResourceString("ClusterMapFirstEntryMustBeZero");

	internal static string CodePointOutOfRange => GetResourceString("CodePointOutOfRange");

	internal static string CollectionDuplicateKey => GetResourceString("CollectionDuplicateKey");

	internal static string CollectionEnumerationError => GetResourceString("CollectionEnumerationError");

	internal static string CollectionIsFixedSize => GetResourceString("CollectionIsFixedSize");

	internal static string CollectionNumberOfElementsMustBeGreaterThanZero => GetResourceString("CollectionNumberOfElementsMustBeGreaterThanZero");

	internal static string CollectionNumberOfElementsMustBeLessOrEqualTo => GetResourceString("CollectionNumberOfElementsMustBeLessOrEqualTo");

	internal static string CollectionNumberOfElementsShouldBeEqualTo => GetResourceString("CollectionNumberOfElementsShouldBeEqualTo");

	internal static string CollectionOnlyAcceptsCommandBindings => GetResourceString("CollectionOnlyAcceptsCommandBindings");

	internal static string CollectionOnlyAcceptsInputBindings => GetResourceString("CollectionOnlyAcceptsInputBindings");

	internal static string CollectionOnlyAcceptsInputGestures => GetResourceString("CollectionOnlyAcceptsInputGestures");

	internal static string Collection_BadDestArray => GetResourceString("Collection_BadDestArray");

	internal static string Collection_BadRank => GetResourceString("Collection_BadRank");

	internal static string Collection_BadType => GetResourceString("Collection_BadType");

	internal static string Collection_CopyTo_ArrayCannotBeMultidimensional => GetResourceString("Collection_CopyTo_ArrayCannotBeMultidimensional");

	internal static string Collection_CopyTo_IndexGreaterThanOrEqualToArrayLength => GetResourceString("Collection_CopyTo_IndexGreaterThanOrEqualToArrayLength");

	internal static string Collection_CopyTo_NumberOfElementsExceedsArrayLength => GetResourceString("Collection_CopyTo_NumberOfElementsExceedsArrayLength");

	internal static string Collection_NoNull => GetResourceString("Collection_NoNull");

	internal static string ColorContext_FileTooLarge => GetResourceString("ColorContext_FileTooLarge");

	internal static string Color_ColorContextNotsRGB_or_scRGB => GetResourceString("Color_ColorContextNotsRGB_or_scRGB");

	internal static string Color_ColorContextTypeMismatch => GetResourceString("Color_ColorContextTypeMismatch");

	internal static string Color_DimensionMismatch => GetResourceString("Color_DimensionMismatch");

	internal static string Color_NullColorContext => GetResourceString("Color_NullColorContext");

	internal static string CompatibilityPreferencesSealed => GetResourceString("CompatibilityPreferencesSealed");

	internal static string CompileFeatureSet_InvalidTypographyProperties => GetResourceString("CompileFeatureSet_InvalidTypographyProperties");

	internal static string CompositeFontAttributeValue1 => GetResourceString("CompositeFontAttributeValue1");

	internal static string CompositeFontAttributeValue2 => GetResourceString("CompositeFontAttributeValue2");

	internal static string CompositeFontInvalidUnicodeRange => GetResourceString("CompositeFontInvalidUnicodeRange");

	internal static string CompositeFontMissingAttribute => GetResourceString("CompositeFontMissingAttribute");

	internal static string CompositeFontMissingElement => GetResourceString("CompositeFontMissingElement");

	internal static string CompositeFontSignificantWhitespace => GetResourceString("CompositeFontSignificantWhitespace");

	internal static string CompositeFontUnknownAttribute => GetResourceString("CompositeFontUnknownAttribute");

	internal static string CompositeFontUnknownElement => GetResourceString("CompositeFontUnknownElement");

	internal static string CompositeFont_DuplicateTypeface => GetResourceString("CompositeFont_DuplicateTypeface");

	internal static string CompositeFont_TooManyFamilyMaps => GetResourceString("CompositeFont_TooManyFamilyMaps");

	internal static string CompositionTarget_RootVisual_HasParent => GetResourceString("CompositionTarget_RootVisual_HasParent");

	internal static string ConstructorRecursion => GetResourceString("ConstructorRecursion");

	internal static string ContextMenuKeyDisplayString => GetResourceString("ContextMenuKeyDisplayString");

	internal static string ContextMenuText => GetResourceString("ContextMenuText");

	internal static string Converter_ConvertFromNotSupported => GetResourceString("Converter_ConvertFromNotSupported");

	internal static string Converter_ConvertToNotSupported => GetResourceString("Converter_ConvertToNotSupported");

	internal static string CopyKeyDisplayString => GetResourceString("CopyKeyDisplayString");

	internal static string CopyText => GetResourceString("CopyText");

	internal static string CorrectionListKey => GetResourceString("CorrectionListKey");

	internal static string CorrectionListKeyDisplayString => GetResourceString("CorrectionListKeyDisplayString");

	internal static string CorrectionListText => GetResourceString("CorrectionListText");

	internal static string CountOfBitsGreatThanRemainingBits => GetResourceString("CountOfBitsGreatThanRemainingBits");

	internal static string CountOfBitsOutOfRange => GetResourceString("CountOfBitsOutOfRange");

	internal static string CreateBreaksFailure => GetResourceString("CreateBreaksFailure");

	internal static string CreateContextFailure => GetResourceString("CreateContextFailure");

	internal static string CreateLineFailure => GetResourceString("CreateLineFailure");

	internal static string CreateParaBreakingSessionFailure => GetResourceString("CreateParaBreakingSessionFailure");

	internal static string CurrentDispatcherNotFound => GetResourceString("CurrentDispatcherNotFound");

	internal static string Cursor_InvalidStream => GetResourceString("Cursor_InvalidStream");

	internal static string Cursor_LoadImageFailure => GetResourceString("Cursor_LoadImageFailure");

	internal static string Cursor_UnsupportedFormat => GetResourceString("Cursor_UnsupportedFormat");

	internal static string CutKeyDisplayString => GetResourceString("CutKeyDisplayString");

	internal static string CutText => GetResourceString("CutText");

	internal static string D3DImage_AARequires9Ex => GetResourceString("D3DImage_AARequires9Ex");

	internal static string D3DImage_InvalidDevice => GetResourceString("D3DImage_InvalidDevice");

	internal static string D3DImage_InvalidPool => GetResourceString("D3DImage_InvalidPool");

	internal static string D3DImage_InvalidUsage => GetResourceString("D3DImage_InvalidUsage");

	internal static string D3DImage_MustHaveBackBuffer => GetResourceString("D3DImage_MustHaveBackBuffer");

	internal static string D3DImage_SurfaceTooBig => GetResourceString("D3DImage_SurfaceTooBig");

	internal static string DataObject_CannotSetDataOnAFozenOLEDataDbject => GetResourceString("DataObject_CannotSetDataOnAFozenOLEDataDbject");

	internal static string DataObject_DataFormatNotPresentOnDataObject => GetResourceString("DataObject_DataFormatNotPresentOnDataObject");

	internal static string DataObject_DataObjectMustHaveAtLeastOneFormat => GetResourceString("DataObject_DataObjectMustHaveAtLeastOneFormat");

	internal static string DataObject_EmptyFormatNotAllowed => GetResourceString("DataObject_EmptyFormatNotAllowed");

	internal static string DataObject_FileDropListHasInvalidFileDropPath => GetResourceString("DataObject_FileDropListHasInvalidFileDropPath");

	internal static string DataObject_FileDropListIsEmpty => GetResourceString("DataObject_FileDropListIsEmpty");

	internal static string DataObject_NotImplementedEnumFormatEtc => GetResourceString("DataObject_NotImplementedEnumFormatEtc");

	internal static string DecompressPacketDataFailed => GetResourceString("DecompressPacketDataFailed");

	internal static string DecompressPropertyFailed => GetResourceString("DecompressPropertyFailed");

	internal static string DecreaseZoomKey => GetResourceString("DecreaseZoomKey");

	internal static string DecreaseZoomKeyDisplayString => GetResourceString("DecreaseZoomKeyDisplayString");

	internal static string DecreaseZoomText => GetResourceString("DecreaseZoomText");

	internal static string DeleteKeyDisplayString => GetResourceString("DeleteKeyDisplayString");

	internal static string DeleteText => GetResourceString("DeleteText");

	internal static string DirectoryNotFoundExceptionWithFileName => GetResourceString("DirectoryNotFoundExceptionWithFileName");

	internal static string DragDrop_DragActionInvalid => GetResourceString("DragDrop_DragActionInvalid");

	internal static string DragDrop_DragDropEffectsInvalid => GetResourceString("DragDrop_DragDropEffectsInvalid");

	internal static string DrawingContext_TooManyPops => GetResourceString("DrawingContext_TooManyPops");

	internal static string DrawingGroup_AlreadyOpen => GetResourceString("DrawingGroup_AlreadyOpen");

	internal static string DrawingGroup_CannotAppendToFrozenCollection => GetResourceString("DrawingGroup_CannotAppendToFrozenCollection");

	internal static string DrawingGroup_CannotAppendToNullCollection => GetResourceString("DrawingGroup_CannotAppendToNullCollection");

	internal static string DuplicateApplicationGestureFound => GetResourceString("DuplicateApplicationGestureFound");

	internal static string DuplicateEventName => GetResourceString("DuplicateEventName");

	internal static string DuplicateStrokeAdded => GetResourceString("DuplicateStrokeAdded");

	internal static string Effect_20ShaderUsing30Registers => GetResourceString("Effect_20ShaderUsing30Registers");

	internal static string Effect_CombinedLegacyAndNew => GetResourceString("Effect_CombinedLegacyAndNew");

	internal static string Effect_No_ContextInputSource => GetResourceString("Effect_No_ContextInputSource");

	internal static string Effect_No_InputSource => GetResourceString("Effect_No_InputSource");

	internal static string Effect_PixelFormat => GetResourceString("Effect_PixelFormat");

	internal static string Effect_RenderThreadError => GetResourceString("Effect_RenderThreadError");

	internal static string Effect_Shader20ConstantRegisterLimit => GetResourceString("Effect_Shader20ConstantRegisterLimit");

	internal static string Effect_Shader20SamplerRegisterLimit => GetResourceString("Effect_Shader20SamplerRegisterLimit");

	internal static string Effect_Shader30BoolConstantRegisterLimit => GetResourceString("Effect_Shader30BoolConstantRegisterLimit");

	internal static string Effect_Shader30FloatConstantRegisterLimit => GetResourceString("Effect_Shader30FloatConstantRegisterLimit");

	internal static string Effect_Shader30IntConstantRegisterLimit => GetResourceString("Effect_Shader30IntConstantRegisterLimit");

	internal static string Effect_Shader30SamplerRegisterLimit => GetResourceString("Effect_Shader30SamplerRegisterLimit");

	internal static string Effect_ShaderBytecodeSize => GetResourceString("Effect_ShaderBytecodeSize");

	internal static string Effect_ShaderBytecodeSource => GetResourceString("Effect_ShaderBytecodeSource");

	internal static string Effect_ShaderConstantType => GetResourceString("Effect_ShaderConstantType");

	internal static string Effect_ShaderEffectPadding => GetResourceString("Effect_ShaderEffectPadding");

	internal static string Effect_ShaderPixelShaderSet => GetResourceString("Effect_ShaderPixelShaderSet");

	internal static string Effect_ShaderSamplerType => GetResourceString("Effect_ShaderSamplerType");

	internal static string Effect_ShaderSeekableStream => GetResourceString("Effect_ShaderSeekableStream");

	internal static string Effect_SourceUriMustBeFileOrPack => GetResourceString("Effect_SourceUriMustBeFileOrPack");

	internal static string EmptyArray => GetResourceString("EmptyArray");

	internal static string EmptyArrayNotAllowedAsArgument => GetResourceString("EmptyArrayNotAllowedAsArgument");

	internal static string EmptyDataToLoad => GetResourceString("EmptyDataToLoad");

	internal static string EmptyScToReplace => GetResourceString("EmptyScToReplace");

	internal static string EmptyScToReplaceWith => GetResourceString("EmptyScToReplaceWith");

	internal static string EndHitTestingCalled => GetResourceString("EndHitTestingCalled");

	internal static string EndOfStreamReached => GetResourceString("EndOfStreamReached");

	internal static string Enumerator_CollectionChanged => GetResourceString("Enumerator_CollectionChanged");

	internal static string Enumerator_NotStarted => GetResourceString("Enumerator_NotStarted");

	internal static string Enumerator_ReachedEnd => GetResourceString("Enumerator_ReachedEnd");

	internal static string Enumerator_VerifyContext => GetResourceString("Enumerator_VerifyContext");

	internal static string EnumLineFailure => GetResourceString("EnumLineFailure");

	internal static string Enum_Invalid => GetResourceString("Enum_Invalid");

	internal static string EPExists => GetResourceString("EPExists");

	internal static string EPGuidNotFound => GetResourceString("EPGuidNotFound");

	internal static string EPNotFound => GetResourceString("EPNotFound");

	internal static string EventArgIsNull => GetResourceString("EventArgIsNull");

	internal static string ExtendSelectionDownKeyDisplayString => GetResourceString("ExtendSelectionDownKeyDisplayString");

	internal static string ExtendSelectionDownText => GetResourceString("ExtendSelectionDownText");

	internal static string ExtendSelectionLeftKeyDisplayString => GetResourceString("ExtendSelectionLeftKeyDisplayString");

	internal static string ExtendSelectionLeftText => GetResourceString("ExtendSelectionLeftText");

	internal static string ExtendSelectionRightKeyDisplayString => GetResourceString("ExtendSelectionRightKeyDisplayString");

	internal static string ExtendSelectionRightText => GetResourceString("ExtendSelectionRightText");

	internal static string ExtendSelectionUpKeyDisplayString => GetResourceString("ExtendSelectionUpKeyDisplayString");

	internal static string ExtendSelectionUpText => GetResourceString("ExtendSelectionUpText");

	internal static string FaceIndexMustBePositiveOrZero => GetResourceString("FaceIndexMustBePositiveOrZero");

	internal static string FaceIndexValidOnlyForTTC => GetResourceString("FaceIndexValidOnlyForTTC");

	internal static string FamilyCollection_CannotFindCompositeFontsLocation => GetResourceString("FamilyCollection_CannotFindCompositeFontsLocation");

	internal static string FamilyMap_TargetNotSet => GetResourceString("FamilyMap_TargetNotSet");

	internal static string FavoritesKeyDisplayString => GetResourceString("FavoritesKeyDisplayString");

	internal static string FavoritesText => GetResourceString("FavoritesText");

	internal static string FileFormatException => GetResourceString("FileFormatException");

	internal static string FileFormatExceptionWithFileName => GetResourceString("FileFormatExceptionWithFileName");

	internal static string FileNotFoundExceptionWithFileName => GetResourceString("FileNotFoundExceptionWithFileName");

	internal static string FindKeyDisplayString => GetResourceString("FindKeyDisplayString");

	internal static string FindText => GetResourceString("FindText");

	internal static string FirstPageKey => GetResourceString("FirstPageKey");

	internal static string FirstPageKeyDisplayString => GetResourceString("FirstPageKeyDisplayString");

	internal static string FirstPageText => GetResourceString("FirstPageText");

	internal static string FloatUnknownBamlType => GetResourceString("FloatUnknownBamlType");

	internal static string FlushNotSupported => GetResourceString("FlushNotSupported");

	internal static string FontFamily_ReadOnly => GetResourceString("FontFamily_ReadOnly");

	internal static string Freezable_CantBeFrozen => GetResourceString("Freezable_CantBeFrozen");

	internal static string Freezable_CloneInvalidType => GetResourceString("Freezable_CloneInvalidType");

	internal static string Freezable_Reentrant => GetResourceString("Freezable_Reentrant");

	internal static string Freezable_UnexpectedChange => GetResourceString("Freezable_UnexpectedChange");

	internal static string GeneralTransform_TransformFailed => GetResourceString("GeneralTransform_TransformFailed");

	internal static string General_BadType => GetResourceString("General_BadType");

	internal static string General_Expected_Type => GetResourceString("General_Expected_Type");

	internal static string General_ObjectIsReadOnly => GetResourceString("General_ObjectIsReadOnly");

	internal static string Geometry_BadNumber => GetResourceString("Geometry_BadNumber");

	internal static string GestureRecognizerNotAvailable => GetResourceString("GestureRecognizerNotAvailable");

	internal static string GetPenaltyModuleHandleFailure => GetResourceString("GetPenaltyModuleHandleFailure");

	internal static string GetResponseFailed => GetResourceString("GetResponseFailed");

	internal static string GlyphAreaTooBig => GetResourceString("GlyphAreaTooBig");

	internal static string GlyphCoordinateTooBig => GetResourceString("GlyphCoordinateTooBig");

	internal static string GlyphIndexOutOfRange => GetResourceString("GlyphIndexOutOfRange");

	internal static string GlyphTypefaceNotRecorded => GetResourceString("GlyphTypefaceNotRecorded");

	internal static string GoToPageKey => GetResourceString("GoToPageKey");

	internal static string GoToPageKeyDisplayString => GetResourceString("GoToPageKeyDisplayString");

	internal static string GoToPageText => GetResourceString("GoToPageText");

	internal static string HandlerTypeIllegal => GetResourceString("HandlerTypeIllegal");

	internal static string HelpKeyDisplayString => GetResourceString("HelpKeyDisplayString");

	internal static string HelpText => GetResourceString("HelpText");

	internal static string HitTest_Invalid => GetResourceString("HitTest_Invalid");

	internal static string HitTest_Singular => GetResourceString("HitTest_Singular");

	internal static string HwndSourceDisposed => GetResourceString("HwndSourceDisposed");

	internal static string HwndTarget_HardwareNotSupportDueToProtocolMismatch => GetResourceString("HwndTarget_HardwareNotSupportDueToProtocolMismatch");

	internal static string HwndTarget_InvalidWindowHandle => GetResourceString("HwndTarget_InvalidWindowHandle");

	internal static string HwndTarget_InvalidWindowProcess => GetResourceString("HwndTarget_InvalidWindowProcess");

	internal static string HwndTarget_InvalidWindowThread => GetResourceString("HwndTarget_InvalidWindowThread");

	internal static string HwndTarget_WindowAlreadyHasContent => GetResourceString("HwndTarget_WindowAlreadyHasContent");

	internal static string IAnimatable_CantAnimateSealedDO => GetResourceString("IAnimatable_CantAnimateSealedDO");

	internal static string Image_AlphaThresholdOutOfRange => GetResourceString("Image_AlphaThresholdOutOfRange");

	internal static string Image_BadDimensions => GetResourceString("Image_BadDimensions");

	internal static string Image_BadMetadataHeader => GetResourceString("Image_BadMetadataHeader");

	internal static string Image_BadPixelFormat => GetResourceString("Image_BadPixelFormat");

	internal static string Image_BadStreamData => GetResourceString("Image_BadStreamData");

	internal static string Image_BadVersion => GetResourceString("Image_BadVersion");

	internal static string Image_CannotCreateTempFile => GetResourceString("Image_CannotCreateTempFile");

	internal static string Image_CantBeFrozen => GetResourceString("Image_CantBeFrozen");

	internal static string Image_CantDealWithStream => GetResourceString("Image_CantDealWithStream");

	internal static string Image_CantDealWithUri => GetResourceString("Image_CantDealWithUri");

	internal static string Image_CodecPresent => GetResourceString("Image_CodecPresent");

	internal static string Image_ColorContextInvalid => GetResourceString("Image_ColorContextInvalid");

	internal static string Image_ColorTransformInvalid => GetResourceString("Image_ColorTransformInvalid");

	internal static string Image_ComponentNotFound => GetResourceString("Image_ComponentNotFound");

	internal static string Image_ContentTypeDoesNotMatchDecoder => GetResourceString("Image_ContentTypeDoesNotMatchDecoder");

	internal static string Image_DecoderError => GetResourceString("Image_DecoderError");

	internal static string Image_DisplayStateInvalid => GetResourceString("Image_DisplayStateInvalid");

	internal static string Image_DuplicateMetadataPresent => GetResourceString("Image_DuplicateMetadataPresent");

	internal static string Image_EncoderNoColorContext => GetResourceString("Image_EncoderNoColorContext");

	internal static string Image_EncoderNoGlobalMetadata => GetResourceString("Image_EncoderNoGlobalMetadata");

	internal static string Image_EncoderNoGlobalThumbnail => GetResourceString("Image_EncoderNoGlobalThumbnail");

	internal static string Image_EncoderNoPreview => GetResourceString("Image_EncoderNoPreview");

	internal static string Image_EndInitWithoutBeginInit => GetResourceString("Image_EndInitWithoutBeginInit");

	internal static string Image_FrameMissing => GetResourceString("Image_FrameMissing");

	internal static string Image_FreezableCloneNotAllowed => GetResourceString("Image_FreezableCloneNotAllowed");

	internal static string Image_GuidEmpty => GetResourceString("Image_GuidEmpty");

	internal static string Image_HeaderError => GetResourceString("Image_HeaderError");

	internal static string Image_IndexedPixelFormatRequiresPalette => GetResourceString("Image_IndexedPixelFormatRequiresPalette");

	internal static string Image_InInitialize => GetResourceString("Image_InInitialize");

	internal static string Image_InitializationIncomplete => GetResourceString("Image_InitializationIncomplete");

	internal static string Image_InplaceMetadataNoCopy => GetResourceString("Image_InplaceMetadataNoCopy");

	internal static string Image_InsufficientBuffer => GetResourceString("Image_InsufficientBuffer");

	internal static string Image_InsufficientBufferSize => GetResourceString("Image_InsufficientBufferSize");

	internal static string Image_InternalError => GetResourceString("Image_InternalError");

	internal static string Image_InvalidArrayForPixel => GetResourceString("Image_InvalidArrayForPixel");

	internal static string Image_InvalidColorContext => GetResourceString("Image_InvalidColorContext");

	internal static string Image_InvalidQueryCharacter => GetResourceString("Image_InvalidQueryCharacter");

	internal static string Image_InvalidQueryRequest => GetResourceString("Image_InvalidQueryRequest");

	internal static string Image_LockCountLimit => GetResourceString("Image_LockCountLimit");

	internal static string Image_MetadataInitializationIncomplete => GetResourceString("Image_MetadataInitializationIncomplete");

	internal static string Image_MetadataNotCompatible => GetResourceString("Image_MetadataNotCompatible");

	internal static string Image_MetadataNotSupported => GetResourceString("Image_MetadataNotSupported");

	internal static string Image_MetadataReadOnly => GetResourceString("Image_MetadataReadOnly");

	internal static string Image_MetadataSizeFixed => GetResourceString("Image_MetadataSizeFixed");

	internal static string Image_MustBeLocked => GetResourceString("Image_MustBeLocked");

	internal static string Image_NeitherArgument => GetResourceString("Image_NeitherArgument");

	internal static string Image_NoArgument => GetResourceString("Image_NoArgument");

	internal static string Image_NoCodecsFound => GetResourceString("Image_NoCodecsFound");

	internal static string Image_NoDecodeFrames => GetResourceString("Image_NoDecodeFrames");

	internal static string Image_NoFrames => GetResourceString("Image_NoFrames");

	internal static string Image_NoPalette => GetResourceString("Image_NoPalette");

	internal static string Image_NoPixelFormatFound => GetResourceString("Image_NoPixelFormatFound");

	internal static string Image_NoThumbnail => GetResourceString("Image_NoThumbnail");

	internal static string Image_NotInitialized => GetResourceString("Image_NotInitialized");

	internal static string Image_OnlyOneInit => GetResourceString("Image_OnlyOneInit");

	internal static string Image_OnlyOneSave => GetResourceString("Image_OnlyOneSave");

	internal static string Image_OnlyOrthogonal => GetResourceString("Image_OnlyOrthogonal");

	internal static string Image_OriginalStreamReadOnly => GetResourceString("Image_OriginalStreamReadOnly");

	internal static string Image_Overflow => GetResourceString("Image_Overflow");

	internal static string Image_PaletteColorsDoNotMatchFormat => GetResourceString("Image_PaletteColorsDoNotMatchFormat");

	internal static string Image_PaletteFixedType => GetResourceString("Image_PaletteFixedType");

	internal static string Image_PaletteZeroColors => GetResourceString("Image_PaletteZeroColors");

	internal static string Image_PropertyNotFound => GetResourceString("Image_PropertyNotFound");

	internal static string Image_PropertyNotSupported => GetResourceString("Image_PropertyNotSupported");

	internal static string Image_PropertySize => GetResourceString("Image_PropertySize");

	internal static string Image_PropertyUnexpectedType => GetResourceString("Image_PropertyUnexpectedType");

	internal static string Image_RequestOnlyValidAtMetadataRoot => GetResourceString("Image_RequestOnlyValidAtMetadataRoot");

	internal static string Image_SetPropertyOutsideBeginEndInit => GetResourceString("Image_SetPropertyOutsideBeginEndInit");

	internal static string Image_SingularMatrix => GetResourceString("Image_SingularMatrix");

	internal static string Image_SizeOptionsAngle => GetResourceString("Image_SizeOptionsAngle");

	internal static string Image_SizeOutOfRange => GetResourceString("Image_SizeOutOfRange");

	internal static string Image_StreamNotAvailable => GetResourceString("Image_StreamNotAvailable");

	internal static string Image_StreamRead => GetResourceString("Image_StreamRead");

	internal static string Image_StreamWrite => GetResourceString("Image_StreamWrite");

	internal static string Image_TooManyScanlines => GetResourceString("Image_TooManyScanlines");

	internal static string Image_TooMuchMetadata => GetResourceString("Image_TooMuchMetadata");

	internal static string Image_UnexpectedMetadataType => GetResourceString("Image_UnexpectedMetadataType");

	internal static string Image_UnknownFormat => GetResourceString("Image_UnknownFormat");

	internal static string Image_UnsupportedOperation => GetResourceString("Image_UnsupportedOperation");

	internal static string Image_UnsupportedPixelFormat => GetResourceString("Image_UnsupportedPixelFormat");

	internal static string Image_WrongState => GetResourceString("Image_WrongState");

	internal static string IncompatibleStylusPointDescriptions => GetResourceString("IncompatibleStylusPointDescriptions");

	internal static string IncreaseZoomKey => GetResourceString("IncreaseZoomKey");

	internal static string IncreaseZoomKeyDisplayString => GetResourceString("IncreaseZoomKeyDisplayString");

	internal static string IncreaseZoomText => GetResourceString("IncreaseZoomText");

	internal static string InInitialization => GetResourceString("InInitialization");

	internal static string InitializationIncomplete => GetResourceString("InitializationIncomplete");

	internal static string InitializingCompressorFailed => GetResourceString("InitializingCompressorFailed");

	internal static string InnerRequestNotAllowed => GetResourceString("InnerRequestNotAllowed");

	internal static string InputBinding_ExpectedInputGesture => GetResourceString("InputBinding_ExpectedInputGesture");

	internal static string InputLanguageManager_NotReadyToChangeCurrentLanguage => GetResourceString("InputLanguageManager_NotReadyToChangeCurrentLanguage");

	internal static string InputMethod_InvalidConversionMode => GetResourceString("InputMethod_InvalidConversionMode");

	internal static string InputMethod_InvalidSentenceMode => GetResourceString("InputMethod_InvalidSentenceMode");

	internal static string InputProviderSiteDisposed => GetResourceString("InputProviderSiteDisposed");

	internal static string InputScope_InvalidInputScopeName => GetResourceString("InputScope_InvalidInputScopeName");

	internal static string IntegerCollectionLengthLessThanZero => GetResourceString("IntegerCollectionLengthLessThanZero");

	internal static string InvalidAbsoluteUriInFontFamilyName => GetResourceString("InvalidAbsoluteUriInFontFamilyName");

	internal static string InvalidAdditionalDataForStylusPoint => GetResourceString("InvalidAdditionalDataForStylusPoint");

	internal static string InvalidBufferLength => GetResourceString("InvalidBufferLength");

	internal static string InvalidByteRanges => GetResourceString("InvalidByteRanges");

	internal static string InvalidCursorType => GetResourceString("InvalidCursorType");

	internal static string InvalidDataInISF => GetResourceString("InvalidDataInISF");

	internal static string InvalidDataTypeForExtendedProperty => GetResourceString("InvalidDataTypeForExtendedProperty");

	internal static string InvalidDiameter => GetResourceString("InvalidDiameter");

	internal static string InvalidDrawingAttributesHeight => GetResourceString("InvalidDrawingAttributesHeight");

	internal static string InvalidDrawingAttributesWidth => GetResourceString("InvalidDrawingAttributesWidth");

	internal static string InvalidEpInIsf => GetResourceString("InvalidEpInIsf");

	internal static string InvalidEventHandle => GetResourceString("InvalidEventHandle");

	internal static string InvalidGuid => GetResourceString("InvalidGuid");

	internal static string InvalidIsButtonForId => GetResourceString("InvalidIsButtonForId");

	internal static string InvalidIsButtonForId2 => GetResourceString("InvalidIsButtonForId2");

	internal static string InvalidMatrixContainsInfinity => GetResourceString("InvalidMatrixContainsInfinity");

	internal static string InvalidMatrixContainsNaN => GetResourceString("InvalidMatrixContainsNaN");

	internal static string InvalidMinMaxForButton => GetResourceString("InvalidMinMaxForButton");

	internal static string InvalidPartName => GetResourceString("InvalidPartName");

	internal static string InvalidPermissionStateValue => GetResourceString("InvalidPermissionStateValue");

	internal static string InvalidPermissionType => GetResourceString("InvalidPermissionType");

	internal static string InvalidPressureValue => GetResourceString("InvalidPressureValue");

	internal static string InvalidRemovedStroke => GetResourceString("InvalidRemovedStroke");

	internal static string InvalidReplacedStroke => GetResourceString("InvalidReplacedStroke");

	internal static string InvalidScheme => GetResourceString("InvalidScheme");

	internal static string InvalidSiteOfOriginUri => GetResourceString("InvalidSiteOfOriginUri");

	internal static string InvalidSizeSpecified => GetResourceString("InvalidSizeSpecified");

	internal static string InvalidStream => GetResourceString("InvalidStream");

	internal static string InvalidSttValue => GetResourceString("InvalidSttValue");

	internal static string InvalidStylusPointCollectionZeroCount => GetResourceString("InvalidStylusPointCollectionZeroCount");

	internal static string InvalidStylusPointConstructionZeroLengthCollection => GetResourceString("InvalidStylusPointConstructionZeroLengthCollection");

	internal static string InvalidStylusPointDescription => GetResourceString("InvalidStylusPointDescription");

	internal static string InvalidStylusPointDescriptionButtonsMustBeLast => GetResourceString("InvalidStylusPointDescriptionButtonsMustBeLast");

	internal static string InvalidStylusPointDescriptionDuplicatesFound => GetResourceString("InvalidStylusPointDescriptionDuplicatesFound");

	internal static string InvalidStylusPointDescriptionSubset => GetResourceString("InvalidStylusPointDescriptionSubset");

	internal static string InvalidStylusPointDescriptionTooManyButtons => GetResourceString("InvalidStylusPointDescriptionTooManyButtons");

	internal static string InvalidStylusPointProperty => GetResourceString("InvalidStylusPointProperty");

	internal static string InvalidStylusPointPropertyInfoResolution => GetResourceString("InvalidStylusPointPropertyInfoResolution");

	internal static string InvalidStylusPointXYNaN => GetResourceString("InvalidStylusPointXYNaN");

	internal static string InvalidTempFileName => GetResourceString("InvalidTempFileName");

	internal static string InvalidTextDecorationCollectionString => GetResourceString("InvalidTextDecorationCollectionString");

	internal static string InvalidValueOfType => GetResourceString("InvalidValueOfType");

	internal static string InvalidValueType => GetResourceString("InvalidValueType");

	internal static string InvalidValueType1 => GetResourceString("InvalidValueType1");

	internal static string Invalid_IInputElement => GetResourceString("Invalid_IInputElement");

	internal static string Invalid_isfData_Length => GetResourceString("Invalid_isfData_Length");

	internal static string Invalid_URI => GetResourceString("Invalid_URI");

	internal static string IOBufferOverflow => GetResourceString("IOBufferOverflow");

	internal static string IOExceptionWithFileName => GetResourceString("IOExceptionWithFileName");

	internal static string IsfOperationFailed => GetResourceString("IsfOperationFailed");

	internal static string KeyboardSinkAlreadyOwned => GetResourceString("KeyboardSinkAlreadyOwned");

	internal static string KeyboardSinkMustBeAnElement => GetResourceString("KeyboardSinkMustBeAnElement");

	internal static string KeyboardSinkNotAChild => GetResourceString("KeyboardSinkNotAChild");

	internal static string KeyGesture_Invalid => GetResourceString("KeyGesture_Invalid");

	internal static string LastPageKey => GetResourceString("LastPageKey");

	internal static string LastPageKeyDisplayString => GetResourceString("LastPageKeyDisplayString");

	internal static string LastPageText => GetResourceString("LastPageText");

	internal static string LayoutManager_DeepRecursion => GetResourceString("LayoutManager_DeepRecursion");

	internal static string Manipulation_InvalidManipulationMode => GetResourceString("Manipulation_InvalidManipulationMode");

	internal static string Manipulation_ManipulationNotActive => GetResourceString("Manipulation_ManipulationNotActive");

	internal static string Manipulation_ManipulationNotEnabled => GetResourceString("Manipulation_ManipulationNotEnabled");

	internal static string Matrix3D_NotInvertible => GetResourceString("Matrix3D_NotInvertible");

	internal static string MatrixNotInvertible => GetResourceString("MatrixNotInvertible");

	internal static string MediaBoostBassKey => GetResourceString("MediaBoostBassKey");

	internal static string MediaBoostBassKeyDisplayString => GetResourceString("MediaBoostBassKeyDisplayString");

	internal static string MediaBoostBassText => GetResourceString("MediaBoostBassText");

	internal static string MediaChannelDownKey => GetResourceString("MediaChannelDownKey");

	internal static string MediaChannelDownKeyDisplayString => GetResourceString("MediaChannelDownKeyDisplayString");

	internal static string MediaChannelDownText => GetResourceString("MediaChannelDownText");

	internal static string MediaChannelUpKey => GetResourceString("MediaChannelUpKey");

	internal static string MediaChannelUpKeyDisplayString => GetResourceString("MediaChannelUpKeyDisplayString");

	internal static string MediaChannelUpText => GetResourceString("MediaChannelUpText");

	internal static string MediaContext_APINotAllowed => GetResourceString("MediaContext_APINotAllowed");

	internal static string MediaContext_InfiniteLayoutLoop => GetResourceString("MediaContext_InfiniteLayoutLoop");

	internal static string MediaContext_InfiniteTickLoop => GetResourceString("MediaContext_InfiniteTickLoop");

	internal static string MediaContext_NoBadShaderHandler => GetResourceString("MediaContext_NoBadShaderHandler");

	internal static string MediaContext_OutOfVideoMemory => GetResourceString("MediaContext_OutOfVideoMemory");

	internal static string MediaContext_RenderThreadError => GetResourceString("MediaContext_RenderThreadError");

	internal static string MediaDecreaseBassKey => GetResourceString("MediaDecreaseBassKey");

	internal static string MediaDecreaseBassKeyDisplayString => GetResourceString("MediaDecreaseBassKeyDisplayString");

	internal static string MediaDecreaseBassText => GetResourceString("MediaDecreaseBassText");

	internal static string MediaDecreaseMicrophoneVolumeKey => GetResourceString("MediaDecreaseMicrophoneVolumeKey");

	internal static string MediaDecreaseMicrophoneVolumeKeyDisplayString => GetResourceString("MediaDecreaseMicrophoneVolumeKeyDisplayString");

	internal static string MediaDecreaseMicrophoneVolumeText => GetResourceString("MediaDecreaseMicrophoneVolumeText");

	internal static string MediaDecreaseTrebleKey => GetResourceString("MediaDecreaseTrebleKey");

	internal static string MediaDecreaseTrebleKeyDisplayString => GetResourceString("MediaDecreaseTrebleKeyDisplayString");

	internal static string MediaDecreaseTrebleText => GetResourceString("MediaDecreaseTrebleText");

	internal static string MediaDecreaseVolumeKey => GetResourceString("MediaDecreaseVolumeKey");

	internal static string MediaDecreaseVolumeKeyDisplayString => GetResourceString("MediaDecreaseVolumeKeyDisplayString");

	internal static string MediaDecreaseVolumeText => GetResourceString("MediaDecreaseVolumeText");

	internal static string MediaFastForwardKey => GetResourceString("MediaFastForwardKey");

	internal static string MediaFastForwardKeyDisplayString => GetResourceString("MediaFastForwardKeyDisplayString");

	internal static string MediaFastForwardText => GetResourceString("MediaFastForwardText");

	internal static string MediaIncreaseBassKey => GetResourceString("MediaIncreaseBassKey");

	internal static string MediaIncreaseBassKeyDisplayString => GetResourceString("MediaIncreaseBassKeyDisplayString");

	internal static string MediaIncreaseBassText => GetResourceString("MediaIncreaseBassText");

	internal static string MediaIncreaseMicrophoneVolumeKey => GetResourceString("MediaIncreaseMicrophoneVolumeKey");

	internal static string MediaIncreaseMicrophoneVolumeKeyDisplayString => GetResourceString("MediaIncreaseMicrophoneVolumeKeyDisplayString");

	internal static string MediaIncreaseMicrophoneVolumeText => GetResourceString("MediaIncreaseMicrophoneVolumeText");

	internal static string MediaIncreaseTrebleKey => GetResourceString("MediaIncreaseTrebleKey");

	internal static string MediaIncreaseTrebleKeyDisplayString => GetResourceString("MediaIncreaseTrebleKeyDisplayString");

	internal static string MediaIncreaseTrebleText => GetResourceString("MediaIncreaseTrebleText");

	internal static string MediaIncreaseVolumeKey => GetResourceString("MediaIncreaseVolumeKey");

	internal static string MediaIncreaseVolumeKeyDisplayString => GetResourceString("MediaIncreaseVolumeKeyDisplayString");

	internal static string MediaIncreaseVolumeText => GetResourceString("MediaIncreaseVolumeText");

	internal static string MediaMuteMicrophoneVolumeKey => GetResourceString("MediaMuteMicrophoneVolumeKey");

	internal static string MediaMuteMicrophoneVolumeKeyDisplayString => GetResourceString("MediaMuteMicrophoneVolumeKeyDisplayString");

	internal static string MediaMuteMicrophoneVolumeText => GetResourceString("MediaMuteMicrophoneVolumeText");

	internal static string MediaMuteVolumeKey => GetResourceString("MediaMuteVolumeKey");

	internal static string MediaMuteVolumeKeyDisplayString => GetResourceString("MediaMuteVolumeKeyDisplayString");

	internal static string MediaMuteVolumeText => GetResourceString("MediaMuteVolumeText");

	internal static string MediaNextTrackKey => GetResourceString("MediaNextTrackKey");

	internal static string MediaNextTrackKeyDisplayString => GetResourceString("MediaNextTrackKeyDisplayString");

	internal static string MediaNextTrackText => GetResourceString("MediaNextTrackText");

	internal static string MediaPauseKey => GetResourceString("MediaPauseKey");

	internal static string MediaPauseKeyDisplayString => GetResourceString("MediaPauseKeyDisplayString");

	internal static string MediaPauseText => GetResourceString("MediaPauseText");

	internal static string MediaPlayKey => GetResourceString("MediaPlayKey");

	internal static string MediaPlayKeyDisplayString => GetResourceString("MediaPlayKeyDisplayString");

	internal static string MediaPlayText => GetResourceString("MediaPlayText");

	internal static string MediaPreviousTrackKey => GetResourceString("MediaPreviousTrackKey");

	internal static string MediaPreviousTrackKeyDisplayString => GetResourceString("MediaPreviousTrackKeyDisplayString");

	internal static string MediaPreviousTrackText => GetResourceString("MediaPreviousTrackText");

	internal static string MediaRecordKey => GetResourceString("MediaRecordKey");

	internal static string MediaRecordKeyDisplayString => GetResourceString("MediaRecordKeyDisplayString");

	internal static string MediaRecordText => GetResourceString("MediaRecordText");

	internal static string MediaRewindKey => GetResourceString("MediaRewindKey");

	internal static string MediaRewindKeyDisplayString => GetResourceString("MediaRewindKeyDisplayString");

	internal static string MediaRewindText => GetResourceString("MediaRewindText");

	internal static string MediaSelectKey => GetResourceString("MediaSelectKey");

	internal static string MediaSelectKeyDisplayString => GetResourceString("MediaSelectKeyDisplayString");

	internal static string MediaSelectText => GetResourceString("MediaSelectText");

	internal static string MediaStopKey => GetResourceString("MediaStopKey");

	internal static string MediaStopKeyDisplayString => GetResourceString("MediaStopKeyDisplayString");

	internal static string MediaStopText => GetResourceString("MediaStopText");

	internal static string MediaSystem_ApiInvalidContext => GetResourceString("MediaSystem_ApiInvalidContext");

	internal static string MediaSystem_OutOfOrderConnectOrDisconnect => GetResourceString("MediaSystem_OutOfOrderConnectOrDisconnect");

	internal static string MediaToggleMicrophoneOnOffKey => GetResourceString("MediaToggleMicrophoneOnOffKey");

	internal static string MediaToggleMicrophoneOnOffKeyDisplayString => GetResourceString("MediaToggleMicrophoneOnOffKeyDisplayString");

	internal static string MediaToggleMicrophoneOnOffText => GetResourceString("MediaToggleMicrophoneOnOffText");

	internal static string MediaTogglePlayPauseKey => GetResourceString("MediaTogglePlayPauseKey");

	internal static string MediaTogglePlayPauseKeyDisplayString => GetResourceString("MediaTogglePlayPauseKeyDisplayString");

	internal static string MediaTogglePlayPauseText => GetResourceString("MediaTogglePlayPauseText");

	internal static string Media_DownloadFailed => GetResourceString("Media_DownloadFailed");

	internal static string Media_FileFormatNotSupported => GetResourceString("Media_FileFormatNotSupported");

	internal static string Media_FileNotFound => GetResourceString("Media_FileNotFound");

	internal static string Media_HardwareVideoAccelerationNotAvailable => GetResourceString("Media_HardwareVideoAccelerationNotAvailable");

	internal static string Media_InsufficientVideoResources => GetResourceString("Media_InsufficientVideoResources");

	internal static string Media_InvalidArgument => GetResourceString("Media_InvalidArgument");

	internal static string Media_InvalidWmpVersion => GetResourceString("Media_InvalidWmpVersion");

	internal static string Media_LogonFailure => GetResourceString("Media_LogonFailure");

	internal static string Media_NotAllowedWhileTimingEngineInControl => GetResourceString("Media_NotAllowedWhileTimingEngineInControl");

	internal static string Media_PackURIsAreNotSupported => GetResourceString("Media_PackURIsAreNotSupported");

	internal static string Media_PlayerIsClosed => GetResourceString("Media_PlayerIsClosed");

	internal static string Media_PlaylistFormatNotSupported => GetResourceString("Media_PlaylistFormatNotSupported");

	internal static string Media_StreamClosed => GetResourceString("Media_StreamClosed");

	internal static string Media_UninitializedResource => GetResourceString("Media_UninitializedResource");

	internal static string Media_UnknownChannelType => GetResourceString("Media_UnknownChannelType");

	internal static string Media_UnknownMediaExecption => GetResourceString("Media_UnknownMediaExecption");

	internal static string Media_UriNotSpecified => GetResourceString("Media_UriNotSpecified");

	internal static string MethodCallNotAllowed => GetResourceString("MethodCallNotAllowed");

	internal static string MilErr_UnsupportedVersion => GetResourceString("MilErr_UnsupportedVersion");

	internal static string Mismatched_RoutedEvent => GetResourceString("Mismatched_RoutedEvent");

	internal static string MoveDownKeyDisplayString => GetResourceString("MoveDownKeyDisplayString");

	internal static string MoveDownText => GetResourceString("MoveDownText");

	internal static string MoveFocusBackKeyDisplayString => GetResourceString("MoveFocusBackKeyDisplayString");

	internal static string MoveFocusBackText => GetResourceString("MoveFocusBackText");

	internal static string MoveFocusDownKeyDisplayString => GetResourceString("MoveFocusDownKeyDisplayString");

	internal static string MoveFocusDownText => GetResourceString("MoveFocusDownText");

	internal static string MoveFocusForwardKeyDisplayString => GetResourceString("MoveFocusForwardKeyDisplayString");

	internal static string MoveFocusForwardText => GetResourceString("MoveFocusForwardText");

	internal static string MoveFocusPageDownKeyDisplayString => GetResourceString("MoveFocusPageDownKeyDisplayString");

	internal static string MoveFocusPageDownText => GetResourceString("MoveFocusPageDownText");

	internal static string MoveFocusPageUpKeyDisplayString => GetResourceString("MoveFocusPageUpKeyDisplayString");

	internal static string MoveFocusPageUpText => GetResourceString("MoveFocusPageUpText");

	internal static string MoveFocusUpKeyDisplayString => GetResourceString("MoveFocusUpKeyDisplayString");

	internal static string MoveFocusUpText => GetResourceString("MoveFocusUpText");

	internal static string MoveLeftKeyDisplayString => GetResourceString("MoveLeftKeyDisplayString");

	internal static string MoveLeftText => GetResourceString("MoveLeftText");

	internal static string MoveRightKeyDisplayString => GetResourceString("MoveRightKeyDisplayString");

	internal static string MoveRightText => GetResourceString("MoveRightText");

	internal static string MoveToEndKeyDisplayString => GetResourceString("MoveToEndKeyDisplayString");

	internal static string MoveToEndText => GetResourceString("MoveToEndText");

	internal static string MoveToHomeKeyDisplayString => GetResourceString("MoveToHomeKeyDisplayString");

	internal static string MoveToHomeText => GetResourceString("MoveToHomeText");

	internal static string MoveToPageDownKeyDisplayString => GetResourceString("MoveToPageDownKeyDisplayString");

	internal static string MoveToPageDownText => GetResourceString("MoveToPageDownText");

	internal static string MoveToPageUpKeyDisplayString => GetResourceString("MoveToPageUpKeyDisplayString");

	internal static string MoveToPageUpText => GetResourceString("MoveToPageUpText");

	internal static string MoveUpKeyDisplayString => GetResourceString("MoveUpKeyDisplayString");

	internal static string MoveUpText => GetResourceString("MoveUpText");

	internal static string MultiSingleton => GetResourceString("MultiSingleton");

	internal static string NavigateJournalKey => GetResourceString("NavigateJournalKey");

	internal static string NavigateJournalKeyDisplayString => GetResourceString("NavigateJournalKeyDisplayString");

	internal static string NavigateJournalText => GetResourceString("NavigateJournalText");

	internal static string NewKeyDisplayString => GetResourceString("NewKeyDisplayString");

	internal static string NewText => GetResourceString("NewText");

	internal static string NextPageKey => GetResourceString("NextPageKey");

	internal static string NextPageKeyDisplayString => GetResourceString("NextPageKeyDisplayString");

	internal static string NextPageText => GetResourceString("NextPageText");

	internal static string NonCLSException => GetResourceString("NonCLSException");

	internal static string NonPackAppAbsoluteUriNotAllowed => GetResourceString("NonPackAppAbsoluteUriNotAllowed");

	internal static string NonWhiteSpaceInAddText => GetResourceString("NonWhiteSpaceInAddText");

	internal static string NotACommandText => GetResourceString("NotACommandText");

	internal static string NotAllowedPackageUri => GetResourceString("NotAllowedPackageUri");

	internal static string NotAllowedToAccessStagingArea => GetResourceString("NotAllowedToAccessStagingArea");

	internal static string NotInInitialization => GetResourceString("NotInInitialization");

	internal static string NullBaseUriParam => GetResourceString("NullBaseUriParam");

	internal static string NullHwnd => GetResourceString("NullHwnd");

	internal static string OffsetNegative => GetResourceString("OffsetNegative");

	internal static string OleRegisterDragDropFailure => GetResourceString("OleRegisterDragDropFailure");

	internal static string OleRevokeDragDropFailure => GetResourceString("OleRevokeDragDropFailure");

	internal static string OleServicesContext_oleInitializeFailure => GetResourceString("OleServicesContext_oleInitializeFailure");

	internal static string OleServicesContext_ThreadMustBeSTA => GetResourceString("OleServicesContext_ThreadMustBeSTA");

	internal static string OnlyAcceptsKeyMessages => GetResourceString("OnlyAcceptsKeyMessages");

	internal static string OnlyOneInitialization => GetResourceString("OnlyOneInitialization");

	internal static string OpenKeyDisplayString => GetResourceString("OpenKeyDisplayString");

	internal static string OpenText => GetResourceString("OpenText");

	internal static string OptimalParagraphMustWrap => GetResourceString("OptimalParagraphMustWrap");

	internal static string PackageAlreadyExists => GetResourceString("PackageAlreadyExists");

	internal static string PackWebRequestCachePolicyIllegal => GetResourceString("PackWebRequestCachePolicyIllegal");

	internal static string PaginatorMissingContentPosition => GetResourceString("PaginatorMissingContentPosition");

	internal static string PaginatorNegativePageNumber => GetResourceString("PaginatorNegativePageNumber");

	internal static string ParameterCannotBeGreaterThan => GetResourceString("ParameterCannotBeGreaterThan");

	internal static string ParameterCannotBeLessThan => GetResourceString("ParameterCannotBeLessThan");

	internal static string ParameterCannotBeNegative => GetResourceString("ParameterCannotBeNegative");

	internal static string ParameterMustBeBetween => GetResourceString("ParameterMustBeBetween");

	internal static string ParameterMustBeGreaterThanZero => GetResourceString("ParameterMustBeGreaterThanZero");

	internal static string ParameterValueCannotBeInfinity => GetResourceString("ParameterValueCannotBeInfinity");

	internal static string ParameterValueCannotBeNaN => GetResourceString("ParameterValueCannotBeNaN");

	internal static string ParameterValueCannotBeNegative => GetResourceString("ParameterValueCannotBeNegative");

	internal static string ParameterValueMustBeGreaterThanZero => GetResourceString("ParameterValueMustBeGreaterThanZero");

	internal static string Parsers_IllegalToken => GetResourceString("Parsers_IllegalToken");

	internal static string Parsers_IllegalToken_250_Chars => GetResourceString("Parsers_IllegalToken_250_Chars");

	internal static string Parser_BadForm => GetResourceString("Parser_BadForm");

	internal static string Parser_Empty => GetResourceString("Parser_Empty");

	internal static string Parser_UnexpectedToken => GetResourceString("Parser_UnexpectedToken");

	internal static string PasteKeyDisplayString => GetResourceString("PasteKeyDisplayString");

	internal static string PasteText => GetResourceString("PasteText");

	internal static string PathGeometry_InternalReadBackError => GetResourceString("PathGeometry_InternalReadBackError");

	internal static string PathTooLongExceptionWithFileName => GetResourceString("PathTooLongExceptionWithFileName");

	internal static string Penservice_Disposed => GetResourceString("Penservice_Disposed");

	internal static string PenService_InvalidPacketData => GetResourceString("PenService_InvalidPacketData");

	internal static string PenService_WindowAlreadyRegistered => GetResourceString("PenService_WindowAlreadyRegistered");

	internal static string PenService_WindowNotRegistered => GetResourceString("PenService_WindowNotRegistered");

	internal static string PreviousPageKey => GetResourceString("PreviousPageKey");

	internal static string PreviousPageKeyDisplayString => GetResourceString("PreviousPageKeyDisplayString");

	internal static string PreviousPageText => GetResourceString("PreviousPageText");

	internal static string PrintKeyDisplayString => GetResourceString("PrintKeyDisplayString");

	internal static string PrintPreviewKeyDisplayString => GetResourceString("PrintPreviewKeyDisplayString");

	internal static string PrintPreviewText => GetResourceString("PrintPreviewText");

	internal static string PrintText => GetResourceString("PrintText");

	internal static string PropertiesKeyDisplayString => GetResourceString("PropertiesKeyDisplayString");

	internal static string PropertiesText => GetResourceString("PropertiesText");

	internal static string PropertyCannotBeNegative => GetResourceString("PropertyCannotBeNegative");

	internal static string PropertyMustBeGreaterThanZero => GetResourceString("PropertyMustBeGreaterThanZero");

	internal static string PropertyOfClassCannotBeGreaterThan => GetResourceString("PropertyOfClassCannotBeGreaterThan");

	internal static string PropertyOfClassCannotBeNull => GetResourceString("PropertyOfClassCannotBeNull");

	internal static string PropertyOfClassMustBeGreaterThanZero => GetResourceString("PropertyOfClassMustBeGreaterThanZero");

	internal static string PropertyValueCannotBeNaN => GetResourceString("PropertyValueCannotBeNaN");

	internal static string Quaternion_ZeroAxisSpecified => GetResourceString("Quaternion_ZeroAxisSpecified");

	internal static string QueryLineFailure => GetResourceString("QueryLineFailure");

	internal static string ReadCountNegative => GetResourceString("ReadCountNegative");

	internal static string ReadOnlyInputGesturesCollection => GetResourceString("ReadOnlyInputGesturesCollection");

	internal static string Rect3D_CannotCallMethod => GetResourceString("Rect3D_CannotCallMethod");

	internal static string Rect3D_CannotModifyEmptyRect => GetResourceString("Rect3D_CannotModifyEmptyRect");

	internal static string Rect_Empty => GetResourceString("Rect_Empty");

	internal static string RedoKeyDisplayString => GetResourceString("RedoKeyDisplayString");

	internal static string RedoText => GetResourceString("RedoText");

	internal static string ReentrantVisualTreeChangeError => GetResourceString("ReentrantVisualTreeChangeError");

	internal static string ReentrantVisualTreeChangeWarning => GetResourceString("ReentrantVisualTreeChangeWarning");

	internal static string RefreshKeyDisplayString => GetResourceString("RefreshKeyDisplayString");

	internal static string RefreshText => GetResourceString("RefreshText");

	internal static string RelievePenaltyResourceFailure => GetResourceString("RelievePenaltyResourceFailure");

	internal static string ReplaceKeyDisplayString => GetResourceString("ReplaceKeyDisplayString");

	internal static string ReplaceText => GetResourceString("ReplaceText");

	internal static string RequestAlreadyStarted => GetResourceString("RequestAlreadyStarted");

	internal static string RequiresSTA => GetResourceString("RequiresSTA");

	internal static string ResourceNotFoundUnderCacheOnlyPolicy => GetResourceString("ResourceNotFoundUnderCacheOnlyPolicy");

	internal static string RoutedEventArgsMustHaveRoutedEvent => GetResourceString("RoutedEventArgsMustHaveRoutedEvent");

	internal static string RoutedEventCannotChangeWhileRouting => GetResourceString("RoutedEventCannotChangeWhileRouting");

	internal static string SaveAsText => GetResourceString("SaveAsText");

	internal static string SaveKeyDisplayString => GetResourceString("SaveKeyDisplayString");

	internal static string SaveText => GetResourceString("SaveText");

	internal static string SCDataChanged => GetResourceString("SCDataChanged");

	internal static string SCErasePath => GetResourceString("SCErasePath");

	internal static string SCEraseShape => GetResourceString("SCEraseShape");

	internal static string SchemaInvalidForTransport => GetResourceString("SchemaInvalidForTransport");

	internal static string ScopeMustBeUIElementOrContent => GetResourceString("ScopeMustBeUIElementOrContent");

	internal static string ScrollByLineKey => GetResourceString("ScrollByLineKey");

	internal static string ScrollByLineKeyDisplayString => GetResourceString("ScrollByLineKeyDisplayString");

	internal static string ScrollByLineText => GetResourceString("ScrollByLineText");

	internal static string ScrollPageDownKeyDisplayString => GetResourceString("ScrollPageDownKeyDisplayString");

	internal static string ScrollPageDownText => GetResourceString("ScrollPageDownText");

	internal static string ScrollPageLeftKey => GetResourceString("ScrollPageLeftKey");

	internal static string ScrollPageLeftKeyDisplayString => GetResourceString("ScrollPageLeftKeyDisplayString");

	internal static string ScrollPageLeftText => GetResourceString("ScrollPageLeftText");

	internal static string ScrollPageRightKey => GetResourceString("ScrollPageRightKey");

	internal static string ScrollPageRightKeyDisplayString => GetResourceString("ScrollPageRightKeyDisplayString");

	internal static string ScrollPageRightText => GetResourceString("ScrollPageRightText");

	internal static string ScrollPageUpKeyDisplayString => GetResourceString("ScrollPageUpKeyDisplayString");

	internal static string ScrollPageUpText => GetResourceString("ScrollPageUpText");

	internal static string SearchKey => GetResourceString("SearchKey");

	internal static string SearchKeyDisplayString => GetResourceString("SearchKeyDisplayString");

	internal static string SearchText => GetResourceString("SearchText");

	internal static string SecurityExceptionForSettingSandboxExternalToTrue => GetResourceString("SecurityExceptionForSettingSandboxExternalToTrue");

	internal static string SeekNegative => GetResourceString("SeekNegative");

	internal static string SeekOriginInvalid => GetResourceString("SeekOriginInvalid");

	internal static string SelectAllKeyDisplayString => GetResourceString("SelectAllKeyDisplayString");

	internal static string SelectAllText => GetResourceString("SelectAllText");

	internal static string SelectToEndKeyDisplayString => GetResourceString("SelectToEndKeyDisplayString");

	internal static string SelectToEndText => GetResourceString("SelectToEndText");

	internal static string SelectToHomeKeyDisplayString => GetResourceString("SelectToHomeKeyDisplayString");

	internal static string SelectToHomeText => GetResourceString("SelectToHomeText");

	internal static string SelectToPageDownKeyDisplayString => GetResourceString("SelectToPageDownKeyDisplayString");

	internal static string SelectToPageDownText => GetResourceString("SelectToPageDownText");

	internal static string SelectToPageUpKeyDisplayString => GetResourceString("SelectToPageUpKeyDisplayString");

	internal static string SelectToPageUpText => GetResourceString("SelectToPageUpText");

	internal static string SetBreakingFailure => GetResourceString("SetBreakingFailure");

	internal static string SetDocFailure => GetResourceString("SetDocFailure");

	internal static string SetFocusFailed => GetResourceString("SetFocusFailed");

	internal static string SetLengthNotSupported => GetResourceString("SetLengthNotSupported");

	internal static string SetTabsFailure => GetResourceString("SetTabsFailure");

	internal static string SidewaysRTLTextIsNotSupported => GetResourceString("SidewaysRTLTextIsNotSupported");

	internal static string Size3D_CannotModifyEmptySize => GetResourceString("Size3D_CannotModifyEmptySize");

	internal static string Size3D_DimensionCannotBeNegative => GetResourceString("Size3D_DimensionCannotBeNegative");

	internal static string SourceNotSet => GetResourceString("SourceNotSet");

	internal static string SpecificNumberCultureRequired => GetResourceString("SpecificNumberCultureRequired");

	internal static string StopKeyDisplayString => GetResourceString("StopKeyDisplayString");

	internal static string StopText => GetResourceString("StopText");

	internal static string StreamGeometry_NeedBeginFigure => GetResourceString("StreamGeometry_NeedBeginFigure");

	internal static string StringEmpty => GetResourceString("StringEmpty");

	internal static string StrokeCollectionCountTooBig => GetResourceString("StrokeCollectionCountTooBig");

	internal static string StrokeCollectionIsReadOnly => GetResourceString("StrokeCollectionIsReadOnly");

	internal static string StrokeIsDuplicated => GetResourceString("StrokeIsDuplicated");

	internal static string StrokesNotContiguously => GetResourceString("StrokesNotContiguously");

	internal static string Stylus_CanOnlyCallForDownMoveOrUp => GetResourceString("Stylus_CanOnlyCallForDownMoveOrUp");

	internal static string Stylus_EnumeratorFailure => GetResourceString("Stylus_EnumeratorFailure");

	internal static string Stylus_IndexOutOfRange => GetResourceString("Stylus_IndexOutOfRange");

	internal static string Stylus_InvalidMax => GetResourceString("Stylus_InvalidMax");

	internal static string Stylus_MatrixNotInvertable => GetResourceString("Stylus_MatrixNotInvertable");

	internal static string Stylus_MustBeDownToCallReset => GetResourceString("Stylus_MustBeDownToCallReset");

	internal static string Stylus_PenContextFailure => GetResourceString("Stylus_PenContextFailure");

	internal static string Stylus_PlugInIsDuplicated => GetResourceString("Stylus_PlugInIsDuplicated");

	internal static string Stylus_PlugInIsNull => GetResourceString("Stylus_PlugInIsNull");

	internal static string Stylus_PlugInNotExist => GetResourceString("Stylus_PlugInNotExist");

	internal static string Stylus_StylusPointsCantBeEmpty => GetResourceString("Stylus_StylusPointsCantBeEmpty");

	internal static string TextBreakpointHasBeenDisposed => GetResourceString("TextBreakpointHasBeenDisposed");

	internal static string TextCompositionManager_NoInputManager => GetResourceString("TextCompositionManager_NoInputManager");

	internal static string TextCompositionManager_TextCompositionHasDone => GetResourceString("TextCompositionManager_TextCompositionHasDone");

	internal static string TextCompositionManager_TextCompositionHasStarted => GetResourceString("TextCompositionManager_TextCompositionHasStarted");

	internal static string TextCompositionManager_TextCompositionNotStarted => GetResourceString("TextCompositionManager_TextCompositionNotStarted");

	internal static string TextComposition_NullResultText => GetResourceString("TextComposition_NullResultText");

	internal static string TextFormatterReentranceProhibited => GetResourceString("TextFormatterReentranceProhibited");

	internal static string TextLineHasBeenDisposed => GetResourceString("TextLineHasBeenDisposed");

	internal static string TextObjectMetrics_WidthOutOfRange => GetResourceString("TextObjectMetrics_WidthOutOfRange");

	internal static string TextPenaltyModuleHasBeenDisposed => GetResourceString("TextPenaltyModuleHasBeenDisposed");

	internal static string TextProvider_InvalidChild => GetResourceString("TextProvider_InvalidChild");

	internal static string TextRangeProvider_InvalidRangeProvider => GetResourceString("TextRangeProvider_InvalidRangeProvider");

	internal static string TextRunPropertiesCannotBeNull => GetResourceString("TextRunPropertiesCannotBeNull");

	internal static string Timing_AccelAndDecelGreaterThanOne => GetResourceString("Timing_AccelAndDecelGreaterThanOne");

	internal static string Timing_CanSlipOnlyOnSimpleTimelines => GetResourceString("Timing_CanSlipOnlyOnSimpleTimelines");

	internal static string Timing_ChildMustBeTimeline => GetResourceString("Timing_ChildMustBeTimeline");

	internal static string Timing_CreateClockMustReturnNewClock => GetResourceString("Timing_CreateClockMustReturnNewClock");

	internal static string Timing_DifferentThreads => GetResourceString("Timing_DifferentThreads");

	internal static string Timing_EnumeratorInvalidated => GetResourceString("Timing_EnumeratorInvalidated");

	internal static string Timing_EnumeratorOutOfRange => GetResourceString("Timing_EnumeratorOutOfRange");

	internal static string Timing_InvalidArgAccelAndDecel => GetResourceString("Timing_InvalidArgAccelAndDecel");

	internal static string Timing_InvalidArgFiniteNonNegative => GetResourceString("Timing_InvalidArgFiniteNonNegative");

	internal static string Timing_InvalidArgFinitePositive => GetResourceString("Timing_InvalidArgFinitePositive");

	internal static string Timing_InvalidArgNonNegative => GetResourceString("Timing_InvalidArgNonNegative");

	internal static string Timing_InvalidArgPositive => GetResourceString("Timing_InvalidArgPositive");

	internal static string Timing_NoTextChildren => GetResourceString("Timing_NoTextChildren");

	internal static string Timing_NotTimeSpan => GetResourceString("Timing_NotTimeSpan");

	internal static string Timing_OperationEnqueuedOutOfOrder => GetResourceString("Timing_OperationEnqueuedOutOfOrder");

	internal static string Timing_RepeatBehaviorInvalidIterationCount => GetResourceString("Timing_RepeatBehaviorInvalidIterationCount");

	internal static string Timing_RepeatBehaviorInvalidRepeatDuration => GetResourceString("Timing_RepeatBehaviorInvalidRepeatDuration");

	internal static string Timing_RepeatBehaviorNotIterationCount => GetResourceString("Timing_RepeatBehaviorNotIterationCount");

	internal static string Timing_RepeatBehaviorNotRepeatDuration => GetResourceString("Timing_RepeatBehaviorNotRepeatDuration");

	internal static string Timing_SeekDestinationAmbiguousDueToSlip => GetResourceString("Timing_SeekDestinationAmbiguousDueToSlip");

	internal static string Timing_SeekDestinationIndefinite => GetResourceString("Timing_SeekDestinationIndefinite");

	internal static string Timing_SeekDestinationNegative => GetResourceString("Timing_SeekDestinationNegative");

	internal static string Timing_SkipToFillDestinationIndefinite => GetResourceString("Timing_SkipToFillDestinationIndefinite");

	internal static string Timing_SlipBehavior_SlipOnlyOnSimpleTimelines => GetResourceString("Timing_SlipBehavior_SlipOnlyOnSimpleTimelines");

	internal static string Timing_SlipBehavior_SyncOnlyWithSimpleParents => GetResourceString("Timing_SlipBehavior_SyncOnlyWithSimpleParents");

	internal static string TokenizerHelperEmptyToken => GetResourceString("TokenizerHelperEmptyToken");

	internal static string TokenizerHelperExtraDataEncountered => GetResourceString("TokenizerHelperExtraDataEncountered");

	internal static string TokenizerHelperMissingEndQuote => GetResourceString("TokenizerHelperMissingEndQuote");

	internal static string TokenizerHelperPrematureStringTermination => GetResourceString("TokenizerHelperPrematureStringTermination");

	internal static string TooManyGlyphRuns => GetResourceString("TooManyGlyphRuns");

	internal static string TooManyRoutedEvents => GetResourceString("TooManyRoutedEvents");

	internal static string Touch_Category => GetResourceString("Touch_Category");

	internal static string Touch_DeviceAlreadyActivated => GetResourceString("Touch_DeviceAlreadyActivated");

	internal static string Touch_DeviceNotActivated => GetResourceString("Touch_DeviceNotActivated");

	internal static string TreeLoop => GetResourceString("TreeLoop");

	internal static string TypeMetadataCannotChangeAfterUse => GetResourceString("TypeMetadataCannotChangeAfterUse");

	internal static string UIElement_Layout_InfinityArrange => GetResourceString("UIElement_Layout_InfinityArrange");

	internal static string UIElement_Layout_NaNMeasure => GetResourceString("UIElement_Layout_NaNMeasure");

	internal static string UIElement_Layout_NaNReturned => GetResourceString("UIElement_Layout_NaNReturned");

	internal static string UIElement_Layout_PositiveInfinityReturned => GetResourceString("UIElement_Layout_PositiveInfinityReturned");

	internal static string UnauthorizedAccessExceptionWithFileName => GetResourceString("UnauthorizedAccessExceptionWithFileName");

	internal static string UndoKeyDisplayString => GetResourceString("UndoKeyDisplayString");

	internal static string UndoText => GetResourceString("UndoText");

	internal static string UnexpectedParameterType => GetResourceString("UnexpectedParameterType");

	internal static string UnexpectedStroke => GetResourceString("UnexpectedStroke");

	internal static string UnknownPathOperationType => GetResourceString("UnknownPathOperationType");

	internal static string UnknownStroke => GetResourceString("UnknownStroke");

	internal static string UnknownStroke1 => GetResourceString("UnknownStroke1");

	internal static string UnknownStroke3 => GetResourceString("UnknownStroke3");

	internal static string UnspecifiedGestureConstructionException => GetResourceString("UnspecifiedGestureConstructionException");

	internal static string UnspecifiedGestureException => GetResourceString("UnspecifiedGestureException");

	internal static string UnspecifiedSetEnabledGesturesException => GetResourceString("UnspecifiedSetEnabledGesturesException");

	internal static string Unsupported_MouseAction => GetResourceString("Unsupported_MouseAction");

	internal static string UriMustBeAbsolute => GetResourceString("UriMustBeAbsolute");

	internal static string UriMustBeFileOrPack => GetResourceString("UriMustBeFileOrPack");

	internal static string UriNotAbsolute => GetResourceString("UriNotAbsolute");

	internal static string UriSchemeMismatch => GetResourceString("UriSchemeMismatch");

	internal static string UsesPerPixelOpacityIsObsolete => GetResourceString("UsesPerPixelOpacityIsObsolete");

	internal static string ValueNotValidForGuid => GetResourceString("ValueNotValidForGuid");

	internal static string Viewport2DVisual3D_MaterialGroupIsInteractiveMaterial => GetResourceString("Viewport2DVisual3D_MaterialGroupIsInteractiveMaterial");

	internal static string Viewport2DVisual3D_MultipleInteractiveMaterials => GetResourceString("Viewport2DVisual3D_MultipleInteractiveMaterials");

	internal static string VisualCannotBeDetached => GetResourceString("VisualCannotBeDetached");

	internal static string VisualCollection_EntryInUse => GetResourceString("VisualCollection_EntryInUse");

	internal static string VisualCollection_NotEnoughCapacity => GetResourceString("VisualCollection_NotEnoughCapacity");

	internal static string VisualCollection_ReadOnly => GetResourceString("VisualCollection_ReadOnly");

	internal static string VisualCollection_VisualHasParent => GetResourceString("VisualCollection_VisualHasParent");

	internal static string VisualTarget_AnotherTargetAlreadyConnected => GetResourceString("VisualTarget_AnotherTargetAlreadyConnected");

	internal static string Visual_ArgumentOutOfRange => GetResourceString("Visual_ArgumentOutOfRange");

	internal static string Visual_CannotTransformPoint => GetResourceString("Visual_CannotTransformPoint");

	internal static string Visual_HasParent => GetResourceString("Visual_HasParent");

	internal static string Visual_NoCommonAncestor => GetResourceString("Visual_NoCommonAncestor");

	internal static string Visual_NoPresentationSource => GetResourceString("Visual_NoPresentationSource");

	internal static string Visual_NotA3DVisual => GetResourceString("Visual_NotA3DVisual");

	internal static string Visual_NotADescendant => GetResourceString("Visual_NotADescendant");

	internal static string Visual_NotAnAncestor => GetResourceString("Visual_NotAnAncestor");

	internal static string Visual_NotAVisual => GetResourceString("Visual_NotAVisual");

	internal static string Visual_NotChild => GetResourceString("Visual_NotChild");

	internal static string WebRequestTimeout => GetResourceString("WebRequestTimeout");

	internal static string WebResponseCloseFailure => GetResourceString("WebResponseCloseFailure");

	internal static string WebResponseFailure => GetResourceString("WebResponseFailure");

	internal static string WebResponsePartNotFound => GetResourceString("WebResponsePartNotFound");

	internal static string WIC_NotInitialized => GetResourceString("WIC_NotInitialized");

	internal static string WriteNotSupported => GetResourceString("WriteNotSupported");

	internal static string WrongFirstSegment => GetResourceString("WrongFirstSegment");

	internal static string XmlLangGetCultureFailure => GetResourceString("XmlLangGetCultureFailure");

	internal static string XmlLangGetSpecificCulture => GetResourceString("XmlLangGetSpecificCulture");

	internal static string XmlLangMalformed => GetResourceString("XmlLangMalformed");

	internal static string ZoomKey => GetResourceString("ZoomKey");

	internal static string ZoomKeyDisplayString => GetResourceString("ZoomKeyDisplayString");

	internal static string ZoomText => GetResourceString("ZoomText");

	internal static string PenImcDllVerificationFailed => GetResourceString("PenImcDllVerificationFailed");

	internal static string PenImcSxSRegistrationFailed => GetResourceString("PenImcSxSRegistrationFailed");

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
