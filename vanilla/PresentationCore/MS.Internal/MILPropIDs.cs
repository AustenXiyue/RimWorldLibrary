namespace MS.Internal;

internal enum MILPropIDs : uint
{
	MILPropertyItemIdInvalid = 0u,
	MILPropertyItemIdNewSubfileType = 254u,
	MILPropertyItemIdSubfileType = 255u,
	MILPropertyItemIdImageWidth = 256u,
	MILPropertyItemIdImageHeight = 257u,
	MILPropertyItemIdBitsPerSample = 258u,
	MILPropertyItemIdCompression = 259u,
	MILPropertyItemIdPhotometricInterp = 262u,
	MILPropertyItemIdThreshHolding = 263u,
	MILPropertyItemIdCellWidth = 264u,
	MILPropertyItemIdCellHeight = 265u,
	MILPropertyItemIdFillOrder = 266u,
	MILPropertyItemIdDocumentName = 269u,
	MILPropertyItemIdImageDescription = 270u,
	MILPropertyItemIdEquipMake = 271u,
	MILPropertyItemIdEquipModel = 272u,
	MILPropertyItemIdStripOffsets = 273u,
	MILPropertyItemIdOrientation = 274u,
	MILPropertyItemIdSamplesPerPixel = 277u,
	MILPropertyItemIdRowsPerStrip = 278u,
	MILPropertyItemIdStripBytesCount = 279u,
	MILPropertyItemIdMinSampleValue = 280u,
	MILPropertyItemIdMaxSampleValue = 281u,
	MILPropertyItemIdXResolution = 282u,
	MILPropertyItemIdYResolution = 283u,
	MILPropertyItemIdPlanarConfig = 284u,
	MILPropertyItemIdPageName = 285u,
	MILPropertyItemIdXPosition = 286u,
	MILPropertyItemIdYPosition = 287u,
	MILPropertyItemIdFreeOffset = 288u,
	MILPropertyItemIdFreeByteCounts = 289u,
	MILPropertyItemIdGrayResponseUnit = 290u,
	MILPropertyItemIdGrayResponseCurve = 291u,
	MILPropertyItemIdT4Option = 292u,
	MILPropertyItemIdT6Option = 293u,
	MILPropertyItemIdResolutionUnit = 296u,
	MILPropertyItemIdPageNumber = 297u,
	MILPropertyItemIdTransferFuncition = 301u,
	MILPropertyItemIdSoftwareUsed = 305u,
	MILPropertyItemIdDateTime = 306u,
	MILPropertyItemIdArtist = 315u,
	MILPropertyItemIdHostComputer = 316u,
	MILPropertyItemIdPredictor = 317u,
	MILPropertyItemIdWhitePoint = 318u,
	MILPropertyItemIdPrimaryChromaticities = 319u,
	MILPropertyItemIdColorMap = 320u,
	MILPropertyItemIdHalftoneHints = 321u,
	MILPropertyItemIdTileWidth = 322u,
	MILPropertyItemIdTileLength = 323u,
	MILPropertyItemIdTileOffset = 324u,
	MILPropertyItemIdTileByteCounts = 325u,
	MILPropertyItemIdInkSet = 332u,
	MILPropertyItemIdInkNames = 333u,
	MILPropertyItemIdNumberOfInks = 334u,
	MILPropertyItemIdDotRange = 336u,
	MILPropertyItemIdTargetPrinter = 337u,
	MILPropertyItemIdExtraSamples = 338u,
	MILPropertyItemIdSampleFormat = 339u,
	MILPropertyItemIdSMinSampleValue = 340u,
	MILPropertyItemIdSMaxSampleValue = 341u,
	MILPropertyItemIdTransferRange = 342u,
	MILPropertyItemIdScreenWidth = 343u,
	MILPropertyItemIdScreenHeight = 344u,
	MILPropertyItemIdJPEGProc = 512u,
	MILPropertyItemIdJPEGInterFormat = 513u,
	MILPropertyItemIdJPEGInterLength = 514u,
	MILPropertyItemIdJPEGRestartInterval = 515u,
	MILPropertyItemIdJPEGLosslessPredictors = 517u,
	MILPropertyItemIdJPEGPointTransforms = 518u,
	MILPropertyItemIdJPEGQTables = 519u,
	MILPropertyItemIdJPEGDCTables = 520u,
	MILPropertyItemIdJPEGACTables = 521u,
	MILPropertyItemIdYCbCrCoefficients = 529u,
	MILPropertyItemIdYCbCrSubsampling = 530u,
	MILPropertyItemIdYCbCrPositioning = 531u,
	MILPropertyItemIdREFBlackWhite = 532u,
	MILPropertyItemIdInterlaced = 533u,
	MILPropertyItemIdGamma = 769u,
	MILPropertyItemIdICCProfileDescriptor = 770u,
	MILPropertyItemIdSRGBRenderingIntent = 771u,
	MILPropertyItemIdICCProfile = 34675u,
	MILPropertyItemIdImageTitle = 800u,
	MILPropertyItemIdCopyright = 33432u,
	MILPropertyItemIdResolutionXUnit = 20481u,
	MILPropertyItemIdResolutionYUnit = 20482u,
	MILPropertyItemIdResolutionXLengthUnit = 20483u,
	MILPropertyItemIdResolutionYLengthUnit = 20484u,
	MILPropertyItemIdPrintFlags = 20485u,
	MILPropertyItemIdPrintFlagsVersion = 20486u,
	MILPropertyItemIdPrintFlagsCrop = 20487u,
	MILPropertyItemIdPrintFlagsBleedWidth = 20488u,
	MILPropertyItemIdPrintFlagsBleedWidthScale = 20489u,
	MILPropertyItemIdHalftoneLPI = 20490u,
	MILPropertyItemIdHalftoneLPIUnit = 20491u,
	MILPropertyItemIdHalftoneDegree = 20492u,
	MILPropertyItemIdHalftoneShape = 20493u,
	MILPropertyItemIdHalftoneMisc = 20494u,
	MILPropertyItemIdHalftoneScreen = 20495u,
	MILPropertyItemIdJPEGQuality = 20496u,
	MILPropertyItemIdGridSize = 20497u,
	MILPropertyItemIdThumbnailFormat = 20498u,
	MILPropertyItemIdThumbnailWidth = 20499u,
	MILPropertyItemIdThumbnailHeight = 20500u,
	MILPropertyItemIdThumbnailColorDepth = 20501u,
	MILPropertyItemIdThumbnailPlanes = 20502u,
	MILPropertyItemIdThumbnailRawBytes = 20503u,
	MILPropertyItemIdThumbnailSize = 20504u,
	MILPropertyItemIdThumbnailCompressedSize = 20505u,
	MILPropertyItemIdColorTransferFunction = 20506u,
	MILPropertyItemIdThumbnailData = 20507u,
	MILPropertyItemIdThumbnailImageWidth = 20512u,
	MILPropertyItemIdThumbnailImageHeight = 20513u,
	MILPropertyItemIdThumbnailBitsPerSample = 20514u,
	MILPropertyItemIdThumbnailCompression = 20515u,
	MILPropertyItemIdThumbnailPhotometricInterp = 20516u,
	MILPropertyItemIdThumbnailImageDescription = 20517u,
	MILPropertyItemIdThumbnailEquipMake = 20518u,
	MILPropertyItemIdThumbnailEquipModel = 20519u,
	MILPropertyItemIdThumbnailStripOffsets = 20520u,
	MILPropertyItemIdThumbnailOrientation = 20521u,
	MILPropertyItemIdThumbnailSamplesPerPixel = 20522u,
	MILPropertyItemIdThumbnailRowsPerStrip = 20523u,
	MILPropertyItemIdThumbnailStripBytesCount = 20524u,
	MILPropertyItemIdThumbnailResolutionX = 20525u,
	MILPropertyItemIdThumbnailResolutionY = 20526u,
	MILPropertyItemIdThumbnailPlanarConfig = 20527u,
	MILPropertyItemIdThumbnailResolutionUnit = 20528u,
	MILPropertyItemIdThumbnailTransferFunction = 20529u,
	MILPropertyItemIdThumbnailSoftwareUsed = 20530u,
	MILPropertyItemIdThumbnailDateTime = 20531u,
	MILPropertyItemIdThumbnailArtist = 20532u,
	MILPropertyItemIdThumbnailWhitePoint = 20533u,
	MILPropertyItemIdThumbnailPrimaryChromaticities = 20534u,
	MILPropertyItemIdThumbnailYCbCrCoefficients = 20535u,
	MILPropertyItemIdThumbnailYCbCrSubsampling = 20536u,
	MILPropertyItemIdThumbnailYCbCrPositioning = 20537u,
	MILPropertyItemIdThumbnailRefBlackWhite = 20538u,
	MILPropertyItemIdThumbnailCopyRight = 20539u,
	MILPropertyItemIdLuminanceTable = 20624u,
	MILPropertyItemIdChrominanceTable = 20625u,
	MILPropertyItemIdFrameDelay = 20736u,
	MILPropertyItemIdLoopCount = 20737u,
	MILPropertyItemIdGlobalPalette = 20738u,
	MILPropertyItemIdIndexBackground = 20739u,
	MILPropertyItemIdIndexTransparent = 20740u,
	MILPropertyItemIdPixelUnit = 20752u,
	MILPropertyItemIdPixelPerUnitX = 20753u,
	MILPropertyItemIdPixelPerUnitY = 20754u,
	MILPropertyItemIdPaletteHistogram = 20755u,
	MILPropertyItemIdExifIFD = 34665u,
	MILPropertyItemIdExifExposureTime = 33434u,
	MILPropertyItemIdExifFNumber = 33437u,
	MILPropertyItemIdExifExposureProg = 34850u,
	MILPropertyItemIdExifSpectralSense = 34852u,
	MILPropertyItemIdExifISOSpeed = 34855u,
	MILPropertyItemIdExifOECF = 34856u,
	MILPropertyItemIdExifVer = 36864u,
	MILPropertyItemIdExifDTOrig = 36867u,
	MILPropertyItemIdExifDTDigitized = 36868u,
	MILPropertyItemIdExifCompConfig = 37121u,
	MILPropertyItemIdExifCompBPP = 37122u,
	MILPropertyItemIdExifShutterSpeed = 37377u,
	MILPropertyItemIdExifAperture = 37378u,
	MILPropertyItemIdExifBrightness = 37379u,
	MILPropertyItemIdExifExposureBias = 37380u,
	MILPropertyItemIdExifMaxAperture = 37381u,
	MILPropertyItemIdExifSubjectDist = 37382u,
	MILPropertyItemIdExifMeteringMode = 37383u,
	MILPropertyItemIdExifLightSource = 37384u,
	MILPropertyItemIdExifFlash = 37385u,
	MILPropertyItemIdExifFocalLength = 37386u,
	MILPropertyItemIdExifMakerNote = 37500u,
	MILPropertyItemIdExifUserComment = 37510u,
	MILPropertyItemIdExifDTSubsec = 37520u,
	MILPropertyItemIdExifDTOrigSS = 37521u,
	MILPropertyItemIdExifDTDigSS = 37522u,
	MILPropertyItemIdExifFPXVer = 40960u,
	MILPropertyItemIdExifColorSpace = 40961u,
	MILPropertyItemIdExifPixXDim = 40962u,
	MILPropertyItemIdExifPixYDim = 40963u,
	MILPropertyItemIdExifRelatedWav = 40964u,
	MILPropertyItemIdExifInterop = 40965u,
	MILPropertyItemIdExifFlashEnergy = 41483u,
	MILPropertyItemIdExifSpatialFR = 41484u,
	MILPropertyItemIdExifFocalXRes = 41486u,
	MILPropertyItemIdExifFocalYRes = 41487u,
	MILPropertyItemIdExifFocalResUnit = 41488u,
	MILPropertyItemIdExifSubjectLoc = 41492u,
	MILPropertyItemIdExifExposureIndex = 41493u,
	MILPropertyItemIdExifSensingMethod = 41495u,
	MILPropertyItemIdExifFileSource = 41728u,
	MILPropertyItemIdExifSceneType = 41729u,
	MILPropertyItemIdExifCfaPattern = 41730u,
	MILPropertyItemIdMax = 65535u
}
