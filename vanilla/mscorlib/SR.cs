using System.Globalization;

internal static class SR
{
	public const string Arg_AccessException = "Cannot access member.";

	public const string Arg_AccessViolationException = "Attempted to read or write protected memory. This is often an indication that other memory is corrupt.";

	public const string Arg_ApplicationException = "Error in the application.";

	public const string Arg_ArgumentException = "Value does not fall within the expected range.";

	public const string Arg_ArgumentOutOfRangeException = "Specified argument was out of the range of valid values.";

	public const string Arg_ArithmeticException = "Overflow or underflow in the arithmetic operation.";

	public const string Arg_ArrayPlusOffTooSmall = "Destination array is not long enough to copy all the items in the collection. Check array index and length.";

	public const string Arg_ArrayTypeMismatchException = "Attempted to access an element as a type incompatible with the array.";

	public const string Arg_ArrayZeroError = "Array must not be of length zero.";

	public const string Arg_BadImageFormatException = "Format of the executable (.exe) or library (.dll) is invalid.";

	public const string Arg_BogusIComparer = "Unable to sort because the IComparer.Compare() method returns inconsistent results. Either a value does not compare equal to itself, or one value repeatedly compared to another value yields different results. IComparer: '{0}'.";

	public const string Arg_CannotBeNaN = "TimeSpan does not accept floating point Not-a-Number values.";

	public const string Arg_CannotHaveNegativeValue = "String cannot contain a minus sign if the base is not 10.";

	public const string Arg_CopyNonBlittableArray = "Arrays must contain only blittable data in order to be copied to unmanaged memory.";

	public const string Arg_CopyOutOfRange = "Requested range extends past the end of the array.";

	public const string Arg_CryptographyException = "Error occurred during a cryptographic operation.";

	public const string Arg_DataMisalignedException = "A datatype misalignment was detected in a load or store instruction.";

	public const string Arg_DateTimeRange = "Combination of arguments to the DateTime constructor is out of the legal range.";

	public const string Arg_DirectoryNotFoundException = "Attempted to access a path that is not on the disk.";

	public const string Arg_DecBitCtor = "Decimal byte array constructor requires an array of length four containing valid decimal bytes.";

	public const string Arg_DivideByZero = "Attempted to divide by zero.";

	public const string Arg_DlgtNullInst = "Delegate to an instance method cannot have null 'this'.";

	public const string Arg_DlgtTypeMis = "Delegates must be of the same type.";

	public const string Arg_DuplicateWaitObjectException = "Duplicate objects in argument.";

	public const string Arg_EnumAndObjectMustBeSameType = "Object must be the same type as the enum. The type passed in was '{0}'; the enum type was '{1}'.";

	public const string Arg_EntryPointNotFoundException = "Entry point was not found.";

	public const string Arg_EntryPointNotFoundExceptionParameterized = "Unable to find an entry point named '{0}' in DLL '{1}'.";

	public const string Arg_EnumIllegalVal = "Illegal enum value: {0}.";

	public const string Arg_ExecutionEngineException = "Internal error in the runtime.";

	public const string Arg_ExternalException = "External component has thrown an exception.";

	public const string Arg_FieldAccessException = "Attempted to access a field that is not accessible by the caller.";

	public const string Arg_FormatException = "One of the identified items was in an invalid format.";

	public const string Arg_GuidArrayCtor = "Byte array for GUID must be exactly {0} bytes long.";

	public const string Arg_HexStyleNotSupported = "The number style AllowHexSpecifier is not supported on floating point data types.";

	public const string Arg_HTCapacityOverflow = "Hashtable's capacity overflowed and went negative. Check load factor, capacity and the current size of the table.";

	public const string Arg_IndexOutOfRangeException = "Index was outside the bounds of the array.";

	public const string Arg_InsufficientExecutionStackException = "Insufficient stack to continue executing the program safely. This can happen from having too many functions on the call stack or function on the stack using too much stack space.";

	public const string Arg_InvalidBase = "Invalid Base.";

	public const string Arg_InvalidCastException = "Specified cast is not valid.";

	public const string Arg_InvalidHexStyle = "With the AllowHexSpecifier bit set in the enum bit field, the only other valid bits that can be combined into the enum value must be a subset of those in HexNumber.";

	public const string Arg_InvalidOperationException = "Operation is not valid due to the current state of the object.";

	public const string Arg_OleAutDateInvalid = " Not a legal OleAut date.";

	public const string Arg_OleAutDateScale = "OleAut date did not convert to a DateTime correctly.";

	public const string Arg_InvalidRuntimeTypeHandle = "Invalid RuntimeTypeHandle.";

	public const string Arg_IOException = "I/O error occurred.";

	public const string Arg_KeyNotFound = "The given key was not present in the dictionary.";

	public const string Arg_LongerThanSrcString = "Source string was not long enough. Check sourceIndex and count.";

	public const string Arg_LowerBoundsMustMatch = "The arrays' lower bounds must be identical.";

	public const string Arg_MissingFieldException = "Attempted to access a non-existing field.";

	public const string Arg_MethodAccessException = "Attempt to access the method failed.";

	public const string Arg_MissingMemberException = "Attempted to access a missing member.";

	public const string Arg_MissingMethodException = "Attempted to access a missing method.";

	public const string Arg_MulticastNotSupportedException = "Attempted to add multiple callbacks to a delegate that does not support multicast.";

	public const string Arg_MustBeBoolean = "Object must be of type Boolean.";

	public const string Arg_MustBeByte = "Object must be of type Byte.";

	public const string Arg_MustBeChar = "Object must be of type Char.";

	public const string Arg_MustBeDateTime = "Object must be of type DateTime.";

	public const string Arg_MustBeDateTimeOffset = "Object must be of type DateTimeOffset.";

	public const string Arg_MustBeDecimal = "Object must be of type Decimal.";

	public const string Arg_MustBeDouble = "Object must be of type Double.";

	public const string Arg_MustBeEnum = "Type provided must be an Enum.";

	public const string Arg_MustBeGuid = "Object must be of type GUID.";

	public const string Arg_MustBeInt16 = "Object must be of type Int16.";

	public const string Arg_MustBeInt32 = "Object must be of type Int32.";

	public const string Arg_MustBeInt64 = "Object must be of type Int64.";

	public const string Arg_MustBePrimArray = "Object must be an array of primitives.";

	public const string Arg_MustBeSByte = "Object must be of type SByte.";

	public const string Arg_MustBeSingle = "Object must be of type Single.";

	public const string Arg_MustBeStatic = "Method must be a static method.";

	public const string Arg_MustBeString = "Object must be of type String.";

	public const string Arg_MustBeStringPtrNotAtom = "The pointer passed in as a String must not be in the bottom 64K of the process's address space.";

	public const string Arg_MustBeTimeSpan = "Object must be of type TimeSpan.";

	public const string Arg_MustBeUInt16 = "Object must be of type UInt16.";

	public const string Arg_MustBeUInt32 = "Object must be of type UInt32.";

	public const string Arg_MustBeUInt64 = "Object must be of type UInt64.";

	public const string Arg_MustBeVersion = "Object must be of type Version.";

	public const string Arg_NeedAtLeast1Rank = "Must provide at least one rank.";

	public const string Arg_Need2DArray = "Array was not a two-dimensional array.";

	public const string Arg_Need3DArray = "Array was not a three-dimensional array.";

	public const string Arg_NegativeArgCount = "Argument count must not be negative.";

	public const string Arg_NotFiniteNumberException = "Arg_NotFiniteNumberException = Number encountered was not a finite quantity.";

	public const string Arg_NonZeroLowerBound = "The lower bound of target array must be zero.";

	public const string Arg_NotGenericParameter = "Method may only be called on a Type for which Type.IsGenericParameter is true.";

	public const string Arg_NotImplementedException = "The method or operation is not implemented.";

	public const string Arg_NotSupportedException = "Specified method is not supported.";

	public const string Arg_NotSupportedNonZeroLowerBound = "Arrays with non-zero lower bounds are not supported.";

	public const string Arg_NullReferenceException = "Object reference not set to an instance of an object.";

	public const string Arg_ObjObjEx = "Object of type '{0}' cannot be converted to type '{1}'.";

	public const string Arg_OverflowException = "Arithmetic operation resulted in an overflow.";

	public const string Arg_OutOfMemoryException = "Insufficient memory to continue the execution of the program.";

	public const string Arg_PlatformNotSupported = "Operation is not supported on this platform.";

	public const string Arg_ParamName_Name = "Parameter name: {0}";

	public const string Arg_PathIllegal = "The path is not of a legal form.";

	public const string Arg_PathIllegalUNC = "The UNC path should be of the form \\\\\\\\server\\\\share.";

	public const string Arg_RankException = "Attempted to operate on an array with the incorrect number of dimensions.";

	public const string Arg_RankIndices = "Indices length does not match the array rank.";

	public const string Arg_RankMultiDimNotSupported = "Only single dimensional arrays are supported for the requested action.";

	public const string Arg_RanksAndBounds = "Number of lengths and lowerBounds must match.";

	public const string Arg_RegGetOverflowBug = "RegistryKey.GetValue does not allow a String that has a length greater than Int32.MaxValue.";

	public const string Arg_RegKeyNotFound = "The specified registry key does not exist.";

	public const string Arg_SecurityException = "Security error.";

	public const string Arg_StackOverflowException = "Operation caused a stack overflow.";

	public const string Arg_SynchronizationLockException = "Object synchronization method was called from an unsynchronized block of code.";

	public const string Arg_SystemException = "System error.";

	public const string Arg_TargetInvocationException = "Exception has been thrown by the target of an invocation.";

	public const string Arg_TargetParameterCountException = "Number of parameters specified does not match the expected number.";

	public const string Arg_DefaultValueMissingException = "Missing parameter does not have a default value.";

	public const string Arg_ThreadStartException = "Thread failed to start.";

	public const string Arg_ThreadStateException = "Thread was in an invalid state for the operation being executed.";

	public const string Arg_TimeoutException = "The operation has timed out.";

	public const string Arg_TypeAccessException = "Attempt to access the type failed.";

	public const string Arg_TypeLoadException = "Failure has occurred while loading a type.";

	public const string Arg_UnauthorizedAccessException = "Attempted to perform an unauthorized operation.";

	public const string Arg_VersionString = "Version string portion was too short or too long.";

	public const string Arg_WrongType = "The value '{0}' is not of type '{1}' and cannot be used in this generic collection.";

	public const string Argument_AbsolutePathRequired = "Absolute path information is required.";

	public const string Argument_AddingDuplicate = "An item with the same key has already been added. Key: {0}";

	public const string Argument_AddingDuplicate__ = "Item has already been added. Key in dictionary: '{0}'  Key being added: '{1}'";

	public const string Argument_AdjustmentRulesNoNulls = "The AdjustmentRule array cannot contain null elements.";

	public const string Argument_AdjustmentRulesOutOfOrder = "The elements of the AdjustmentRule array must be in chronological order and must not overlap.";

	public const string Argument_BadFormatSpecifier = "Format specifier was invalid.";

	public const string Argument_CodepageNotSupported = "{0} is not a supported code page.";

	public const string Argument_CompareOptionOrdinal = "CompareOption.Ordinal cannot be used with other options.";

	public const string Argument_ConflictingDateTimeRoundtripStyles = "The DateTimeStyles value RoundtripKind cannot be used with the values AssumeLocal, AssumeUniversal or AdjustToUniversal.";

	public const string Argument_ConflictingDateTimeStyles = "The DateTimeStyles values AssumeLocal and AssumeUniversal cannot be used together.";

	public const string Argument_ConversionOverflow = "Conversion buffer overflow.";

	public const string Argument_ConvertMismatch = "The conversion could not be completed because the supplied DateTime did not have the Kind property set correctly.  For example, when the Kind property is DateTimeKind.Local, the source time zone must be TimeZoneInfo.Local.";

	public const string Argument_CultureInvalidIdentifier = "{0} is an invalid culture identifier.";

	public const string Argument_CultureIetfNotSupported = "Culture IETF Name {0} is not a recognized IETF name.";

	public const string Argument_CultureIsNeutral = "Culture ID {0} (0x{0:X4}) is a neutral culture; a region cannot be created from it.";

	public const string Argument_CultureNotSupported = "Culture is not supported.";

	public const string Argument_CustomCultureCannotBePassedByNumber = "Customized cultures cannot be passed by LCID, only by name.";

	public const string Argument_DateTimeBadBinaryData = "The binary data must result in a DateTime with ticks between DateTime.MinValue.Ticks and DateTime.MaxValue.Ticks.";

	public const string Argument_DateTimeHasTicks = "The supplied DateTime must have the Year, Month, and Day properties set to 1.  The time cannot be specified more precisely than whole milliseconds.";

	public const string Argument_DateTimeHasTimeOfDay = "The supplied DateTime includes a TimeOfDay setting.   This is not supported.";

	public const string Argument_DateTimeIsInvalid = "The supplied DateTime represents an invalid time.  For example, when the clock is adjusted forward, any time in the period that is skipped is invalid.";

	public const string Argument_DateTimeIsNotAmbiguous = "The supplied DateTime is not in an ambiguous time range.";

	public const string Argument_DateTimeKindMustBeUnspecified = "The supplied DateTime must have the Kind property set to DateTimeKind.Unspecified.";

	public const string Argument_DateTimeOffsetInvalidDateTimeStyles = "The DateTimeStyles value 'NoCurrentDateDefault' is not allowed when parsing DateTimeOffset.";

	public const string Argument_DateTimeOffsetIsNotAmbiguous = "The supplied DateTimeOffset is not in an ambiguous time range.";

	public const string Argument_EmptyDecString = "Decimal separator cannot be the empty string.";

	public const string Argument_EmptyName = "Empty name is not legal.";

	public const string Argument_EmptyWaithandleArray = "Waithandle array may not be empty.";

	public const string Argument_EncoderFallbackNotEmpty = "Must complete Convert() operation or call Encoder.Reset() before calling GetBytes() or GetByteCount(). Encoder '{0}' fallback '{1}'.";

	public const string Argument_EncodingConversionOverflowBytes = "The output byte buffer is too small to contain the encoded data, encoding '{0}' fallback '{1}'.";

	public const string Argument_EncodingConversionOverflowChars = "The output char buffer is too small to contain the decoded characters, encoding '{0}' fallback '{1}'.";

	public const string Argument_EncodingNotSupported = "'{0}' is not a supported encoding name. For information on defining a custom encoding, see the documentation for the Encoding.RegisterProvider method.";

	public const string Argument_EnumTypeDoesNotMatch = "The argument type, '{0}', is not the same as the enum type '{1}'.";

	public const string Argument_FallbackBufferNotEmpty = "Cannot change fallback when buffer is not empty. Previous Convert() call left data in the fallback buffer.";

	public const string Argument_IdnBadLabelSize = "IDN labels must be between 1 and 63 characters long.";

	public const string Argument_IdnBadPunycode = "Invalid IDN encoded string.";

	public const string Argument_IdnIllegalName = "Decoded string is not a valid IDN name.";

	public const string Argument_ImplementIComparable = "At least one object must implement IComparable.";

	public const string Argument_InvalidArgumentForComparison = "Type of argument is not compatible with the generic comparer.";

	public const string Argument_InvalidArrayLength = "Length of the array must be {0}.";

	public const string Argument_InvalidArrayType = "Target array type is not compatible with the type of items in the collection.";

	public const string Argument_InvalidCalendar = "Not a valid calendar for the given culture.";

	public const string Argument_InvalidCharSequence = "Invalid Unicode code point found at index {0}.";

	public const string Argument_InvalidCharSequenceNoIndex = "String contains invalid Unicode code points.";

	public const string Argument_InvalidCodePageBytesIndex = "Unable to translate bytes {0} at index {1} from specified code page to Unicode.";

	public const string Argument_InvalidCodePageConversionIndex = "Unable to translate Unicode character \\\\u{0:X4} at index {1} to specified code page.";

	public const string Argument_InvalidCultureName = "Culture name '{0}' is not supported.";

	public const string Argument_InvalidDateTimeKind = "Invalid DateTimeKind value.";

	public const string Argument_InvalidDateTimeStyles = "An undefined DateTimeStyles value is being used.";

	public const string Argument_InvalidDigitSubstitution = "The DigitSubstitution property must be of a valid member of the DigitShapes enumeration. Valid entries include Context, NativeNational or None.";

	public const string Argument_InvalidEnumValue = "The value '{0}' is not valid for this usage of the type {1}.";

	public const string Argument_InvalidFlag = "Value of flags is invalid.";

	public const string Argument_InvalidGroupSize = "Every element in the value array should be between one and nine, except for the last element, which can be zero.";

	public const string Argument_InvalidHighSurrogate = "Found a high surrogate char without a following low surrogate at index: {0}. The input may not be in this encoding, or may not contain valid Unicode (UTF-16) characters.";

	public const string Argument_InvalidId = "The specified ID parameter '{0}' is not supported.";

	public const string Argument_InvalidLowSurrogate = "Found a low surrogate char without a preceding high surrogate at index: {0}. The input may not be in this encoding, or may not contain valid Unicode (UTF-16) characters.";

	public const string Argument_InvalidNativeDigitCount = "The NativeDigits array must contain exactly ten members.";

	public const string Argument_InvalidNativeDigitValue = "Each member of the NativeDigits array must be a single text element (one or more UTF16 code points) with a Unicode Nd (Number, Decimal Digit) property indicating it is a digit.";

	public const string Argument_InvalidNeutralRegionName = "The region name {0} should not correspond to neutral culture; a specific culture name is required.";

	public const string Argument_InvalidNormalizationForm = "Invalid normalization form.";

	public const string Argument_InvalidNumberStyles = "An undefined NumberStyles value is being used.";

	public const string Argument_InvalidOffLen = "Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.";

	public const string Argument_InvalidPathChars = "Illegal characters in path.";

	public const string Argument_InvalidREG_TZI_FORMAT = "The REG_TZI_FORMAT structure is corrupt.";

	public const string Argument_InvalidResourceCultureName = "The given culture name '{0}' cannot be used to locate a resource file. Resource filenames must consist of only letters, numbers, hyphens or underscores.";

	public const string Argument_InvalidSerializedString = "The specified serialized string '{0}' is not supported.";

	public const string Argument_InvalidTimeSpanStyles = "An undefined TimeSpanStyles value is being used.";

	public const string Argument_MustBeFalse = "Argument must be initialized to false";

	public const string Argument_NoEra = "No Era was supplied.";

	public const string Argument_NoRegionInvariantCulture = "There is no region associated with the Invariant Culture (Culture ID: 0x7F).";

	public const string Argument_NotIsomorphic = "Object contains non-primitive or non-blittable data.";

	public const string Argument_OffsetLocalMismatch = "The UTC Offset of the local dateTime parameter does not match the offset argument.";

	public const string Argument_OffsetPrecision = "Offset must be specified in whole minutes.";

	public const string Argument_OffsetOutOfRange = "Offset must be within plus or minus 14 hours.";

	public const string Argument_OffsetUtcMismatch = "The UTC Offset for Utc DateTime instances must be 0.";

	public const string Argument_OneOfCulturesNotSupported = "Culture name {0} or {1} is not supported.";

	public const string Argument_OnlyMscorlib = "Only mscorlib's assembly is valid.";

	public const string Argument_OutOfOrderDateTimes = "The DateStart property must come before the DateEnd property.";

	public const string ArgumentOutOfRange_HugeArrayNotSupported = "Arrays larger than 2GB are not supported.";

	public const string ArgumentOutOfRange_Index = "Index was out of range. Must be non-negative and less than the size of the collection.";

	public const string ArgumentOutOfRange_Length = "The specified length exceeds maximum capacity of SecureString.";

	public const string ArgumentOutOfRange_LengthTooLarge = "The specified length exceeds the maximum value of {0}.";

	public const string ArgumentOutOfRange_NeedNonNegNum = "Non-negative number required.";

	public const string ArgumentOutOfRange_NeedNonNegNumRequired = "Non-negative number required.";

	public const string Argument_PathFormatNotSupported = "The given path's format is not supported.";

	public const string Argument_RecursiveFallback = "Recursive fallback not allowed for character \\\\u{0:X4}.";

	public const string Argument_RecursiveFallbackBytes = "Recursive fallback not allowed for bytes {0}.";

	public const string Argument_ResultCalendarRange = "The result is out of the supported range for this calendar. The result should be between {0} (Gregorian date) and {1} (Gregorian date), inclusive.";

	public const string Argument_SemaphoreInitialMaximum = "The initial count for the semaphore must be greater than or equal to zero and less than the maximum count.";

	public const string Argument_TimeSpanHasSeconds = "The TimeSpan parameter cannot be specified more precisely than whole minutes.";

	public const string Argument_TimeZoneNotFound = "The time zone ID '{0}' was not found on the local computer.";

	public const string Argument_TimeZoneInfoBadTZif = "The tzfile does not begin with the magic characters 'TZif'.  Please verify that the file is not corrupt.";

	public const string Argument_TimeZoneInfoInvalidTZif = "The TZif data structure is corrupt.";

	public const string Argument_ToExclusiveLessThanFromExclusive = "fromInclusive must be less than or equal to toExclusive.";

	public const string Argument_TransitionTimesAreIdentical = "The DaylightTransitionStart property must not equal the DaylightTransitionEnd property.";

	public const string Argument_UTCOutOfRange = "The UTC time represented when the offset is applied must be between year 0 and 10,000.";

	public const string Argument_WaitHandleNameTooLong = "The name can be no more than {0} characters in length.";

	public const string ArgumentException_OtherNotArrayOfCorrectLength = "Object is not a array with the same number of elements as the array to compare it to.";

	public const string ArgumentException_TupleIncorrectType = "Argument must be of type {0}.";

	public const string ArgumentException_TupleLastArgumentNotATuple = "The last element of an eight element tuple must be a Tuple.";

	public const string ArgumentException_ValueTupleIncorrectType = "Argument must be of type {0}.";

	public const string ArgumentException_ValueTupleLastArgumentNotAValueTuple = "The last element of an eight element ValueTuple must be a ValueTuple.";

	public const string ArgumentNull_Array = "Array cannot be null.";

	public const string ArgumentNull_ArrayElement = "At least one element in the specified array was null.";

	public const string ArgumentNull_ArrayValue = "Found a null value within an array.";

	public const string ArgumentNull_Generic = "Value cannot be null.";

	public const string ArgumentNull_Key = "Key cannot be null.";

	public const string ArgumentNull_Obj = "Object cannot be null.";

	public const string ArgumentNull_String = "String reference not set to an instance of a String.";

	public const string ArgumentNull_Type = "Type cannot be null.";

	public const string ArgumentNull_Waithandles = "The waitHandles parameter cannot be null.";

	public const string ArgumentNull_WithParamName = "Parameter '{0}' cannot be null.";

	public const string ArgumentOutOfRange_AddValue = "Value to add was out of range.";

	public const string ArgumentOutOfRange_ActualValue = "Actual value was {0}.";

	public const string ArgumentOutOfRange_BadYearMonthDay = "Year, Month, and Day parameters describe an un-representable DateTime.";

	public const string ArgumentOutOfRange_BadHourMinuteSecond = "Hour, Minute, and Second parameters describe an un-representable DateTime.";

	public const string ArgumentOutOfRange_BiggerThanCollection = "Must be less than or equal to the size of the collection.";

	public const string ArgumentOutOfRange_Bounds_Lower_Upper = "Argument must be between {0} and {1}.";

	public const string ArgumentOutOfRange_CalendarRange = "Specified time is not supported in this calendar. It should be between {0} (Gregorian date) and {1} (Gregorian date), inclusive.";

	public const string ArgumentOutOfRange_Capacity = "Capacity exceeds maximum capacity.";

	public const string ArgumentOutOfRange_Count = "Count must be positive and count must refer to a location within the string/array/collection.";

	public const string ArgumentOutOfRange_DateArithmetic = "The added or subtracted value results in an un-representable DateTime.";

	public const string ArgumentOutOfRange_DateTimeBadMonths = "Months value must be between +/-120000.";

	public const string ArgumentOutOfRange_DateTimeBadTicks = "Ticks must be between DateTime.MinValue.Ticks and DateTime.MaxValue.Ticks.";

	public const string ArgumentOutOfRange_DateTimeBadYears = "Years value must be between +/-10000.";

	public const string ArgumentOutOfRange_Day = "Day must be between 1 and {0} for month {1}.";

	public const string ArgumentOutOfRange_DayOfWeek = "The DayOfWeek enumeration must be in the range 0 through 6.";

	public const string ArgumentOutOfRange_DayParam = "The Day parameter must be in the range 1 through 31.";

	public const string ArgumentOutOfRange_DecimalRound = "Decimal can only round to between 0 and 28 digits of precision.";

	public const string ArgumentOutOfRange_DecimalScale = "Decimal's scale value must be between 0 and 28, inclusive.";

	public const string ArgumentOutOfRange_EndIndexStartIndex = "endIndex cannot be greater than startIndex.";

	public const string ArgumentOutOfRange_Enum = "Enum value was out of legal range.";

	public const string ArgumentOutOfRange_Era = "Time value was out of era range.";

	public const string ArgumentOutOfRange_FileTimeInvalid = "Not a valid Win32 FileTime.";

	public const string ArgumentOutOfRange_GenericPositive = "Value must be positive.";

	public const string ArgumentOutOfRange_GetByteCountOverflow = "Too many characters. The resulting number of bytes is larger than what can be returned as an int.";

	public const string ArgumentOutOfRange_GetCharCountOverflow = "Too many bytes. The resulting number of chars is larger than what can be returned as an int.";

	public const string ArgumentOutOfRange_IndexCount = "Index and count must refer to a location within the string.";

	public const string ArgumentOutOfRange_IndexCountBuffer = "Index and count must refer to a location within the buffer.";

	public const string ArgumentOutOfRange_IndexLength = "Index and length must refer to a location within the string.";

	public const string ArgumentOutOfRange_IndexString = "Index was out of range. Must be non-negative and less than the length of the string.";

	public const string ArgumentOutOfRange_InvalidEraValue = "Era value was not valid.";

	public const string ArgumentOutOfRange_InvalidHighSurrogate = "A valid high surrogate character is between 0xd800 and 0xdbff, inclusive.";

	public const string ArgumentOutOfRange_InvalidLowSurrogate = "A valid low surrogate character is between 0xdc00 and 0xdfff, inclusive.";

	public const string ArgumentOutOfRange_InvalidUTF32 = "A valid UTF32 value is between 0x000000 and 0x10ffff, inclusive, and should not include surrogate codepoint values (0x00d800 ~ 0x00dfff).";

	public const string ArgumentOutOfRange_LengthGreaterThanCapacity = "The length cannot be greater than the capacity.";

	public const string ArgumentOutOfRange_ListInsert = "Index must be within the bounds of the List.";

	public const string ArgumentOutOfRange_ListItem = "Index was out of range. Must be non-negative and less than the size of the list.";

	public const string ArgumentOutOfRange_ListRemoveAt = "Index was out of range. Must be non-negative and less than the size of the list.";

	public const string ArgumentOutOfRange_Month = "Month must be between one and twelve.";

	public const string ArgumentOutOfRange_MonthParam = "The Month parameter must be in the range 1 through 12.";

	public const string ArgumentOutOfRange_MustBeNonNegInt32 = "Value must be non-negative and less than or equal to Int32.MaxValue.";

	public const string ArgumentOutOfRange_MustBeNonNegNum = "'{0}' must be non-negative.";

	public const string ArgumentOutOfRange_MustBePositive = "'{0}' must be greater than zero.";

	public const string ArgumentOutOfRange_NeedNonNegOrNegative1 = "Number must be either non-negative and less than or equal to Int32.MaxValue or -1.";

	public const string ArgumentOutOfRange_NeedPosNum = "Positive number required.";

	public const string ArgumentOutOfRange_NegativeCapacity = "Capacity must be positive.";

	public const string ArgumentOutOfRange_NegativeCount = "Count cannot be less than zero.";

	public const string ArgumentOutOfRange_NegativeLength = "Length cannot be less than zero.";

	public const string ArgumentOutOfRange_OffsetLength = "Offset and length must refer to a position in the string.";

	public const string ArgumentOutOfRange_OffsetOut = "Either offset did not refer to a position in the string, or there is an insufficient length of destination character array.";

	public const string ArgumentOutOfRange_PartialWCHAR = "Pointer startIndex and length do not refer to a valid string.";

	public const string ArgumentOutOfRange_Range = "Valid values are between {0} and {1}, inclusive.";

	public const string ArgumentOutOfRange_RoundingDigits = "Rounding digits must be between 0 and 15, inclusive.";

	public const string ArgumentOutOfRange_SmallCapacity = "capacity was less than the current size.";

	public const string ArgumentOutOfRange_SmallMaxCapacity = "MaxCapacity must be one or greater.";

	public const string ArgumentOutOfRange_StartIndex = "StartIndex cannot be less than zero.";

	public const string ArgumentOutOfRange_StartIndexLargerThanLength = "startIndex cannot be larger than length of string.";

	public const string ArgumentOutOfRange_StartIndexLessThanLength = "startIndex must be less than length of string.";

	public const string ArgumentOutOfRange_UtcOffset = "The TimeSpan parameter must be within plus or minus 14.0 hours.";

	public const string ArgumentOutOfRange_UtcOffsetAndDaylightDelta = "The sum of the BaseUtcOffset and DaylightDelta properties must within plus or minus 14.0 hours.";

	public const string ArgumentOutOfRange_Version = "Version's parameters must be greater than or equal to zero.";

	public const string ArgumentOutOfRange_Week = "The Week parameter must be in the range 1 through 5.";

	public const string ArgumentOutOfRange_Year = "Year must be between 1 and 9999.";

	public const string Arithmetic_NaN = "Function does not accept floating point Not-a-Number values.";

	public const string ArrayTypeMismatch_CantAssignType = "Source array type cannot be assigned to destination array type.";

	public const string BadImageFormatException_CouldNotLoadFileOrAssembly = "Could not load file or assembly '{0}'. An attempt was made to load a program with an incorrect format.";

	public const string CollectionCorrupted = "A prior operation on this collection was interrupted by an exception. Collection's state is no longer trusted.";

	public const string Exception_EndOfInnerExceptionStack = "--- End of inner exception stack trace ---";

	public const string Exception_WasThrown = "Exception of type '{0}' was thrown.";

	public const string Format_BadBase64Char = "The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.";

	public const string Format_BadBase64CharArrayLength = "Invalid length for a Base-64 char array or string.";

	public const string Format_BadBoolean = "String was not recognized as a valid Boolean.";

	public const string Format_BadFormatSpecifier = "Format specifier was invalid.";

	public const string Format_BadQuote = "Cannot find a matching quote character for the character '{0}'.";

	public const string Format_EmptyInputString = "Input string was either empty or contained only whitespace.";

	public const string Format_GuidHexPrefix = "Expected hex 0x in '{0}'.";

	public const string Format_GuidInvLen = "Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).";

	public const string Format_GuidInvalidChar = "Guid string should only contain hexadecimal characters.";

	public const string Format_GuidBrace = "Expected {0xdddddddd, etc}.";

	public const string Format_GuidComma = "Could not find a comma, or the length between the previous token and the comma was zero (i.e., '0x,'etc.).";

	public const string Format_GuidBraceAfterLastNumber = "Could not find a brace, or the length between the previous token and the brace was zero (i.e., '0x,'etc.).";

	public const string Format_GuidDashes = "Dashes are in the wrong position for GUID parsing.";

	public const string Format_GuidEndBrace = "Could not find the ending brace.";

	public const string Format_ExtraJunkAtEnd = "Additional non-parsable characters are at the end of the string.";

	public const string Format_GuidUnrecognized = "Unrecognized Guid format.";

	public const string Format_IndexOutOfRange = "Index (zero based) must be greater than or equal to zero and less than the size of the argument list.";

	public const string Format_InvalidGuidFormatSpecification = "Format String can be only 'D', 'd', 'N', 'n', 'P', 'p', 'B', 'b', 'X' or 'x'.";

	public const string Format_InvalidString = "Input string was not in a correct format.";

	public const string Format_NeedSingleChar = "String must be exactly one character long.";

	public const string Format_NoParsibleDigits = "Could not find any recognizable digits.";

	public const string Format_BadTimeSpan = "String was not recognized as a valid TimeSpan.";

	public const string InsufficientMemory_MemFailPoint = "Insufficient available memory to meet the expected demands of an operation at this time.  Please try again later.";

	public const string InsufficientMemory_MemFailPoint_TooBig = "Insufficient memory to meet the expected demands of an operation, and this system is likely to never satisfy this request.  If this is a 32 bit system, consider booting in 3 GB mode.";

	public const string InsufficientMemory_MemFailPoint_VAFrag = "Insufficient available memory to meet the expected demands of an operation at this time, possibly due to virtual address space fragmentation.  Please try again later.";

	public const string InvalidCast_CannotCastNullToValueType = "Null object cannot be converted to a value type.";

	public const string InvalidCast_DownCastArrayElement = "At least one element in the source array could not be cast down to the destination array type.";

	public const string InvalidCast_FromTo = "Invalid cast from '{0}' to '{1}'.";

	public const string InvalidCast_IConvertible = "Object must implement IConvertible.";

	public const string InvalidCast_StoreArrayElement = "Object cannot be stored in an array of this type.";

	public const string InvalidOperation_Calling = "WinRT Interop has already been initialized and cannot be initialized again.";

	public const string InvalidOperation_DateTimeParsing = "Internal Error in DateTime and Calendar operations.";

	public const string InvalidOperation_EnumEnded = "Enumeration already finished.";

	public const string InvalidOperation_EnumFailedVersion = "Collection was modified; enumeration operation may not execute.";

	public const string InvalidOperation_EnumNotStarted = "Enumeration has not started. Call MoveNext.";

	public const string InvalidOperation_EnumOpCantHappen = "Enumeration has either not started or has already finished.";

	public const string InvalidOperation_HandleIsNotInitialized = "Handle is not initialized.";

	public const string InvalidOperation_IComparerFailed = "Failed to compare two elements in the array.";

	public const string InvalidOperation_NoValue = "Nullable object must have a value.";

	public const string InvalidOperation_NullArray = "The underlying array is null.";

	public const string InvalidOperation_Overlapped_Pack = "Cannot pack a packed Overlapped again.";

	public const string InvalidOperation_ReadOnly = "Instance is read-only.";

	public const string InvalidOperation_ThreadWrongThreadStart = "The thread was created with a ThreadStart delegate that does not accept a parameter.";

	public const string InvalidOperation_UnknownEnumType = "Unknown enum type.";

	public const string InvalidOperation_WriteOnce = "This property has already been set and cannot be modified.";

	public const string InvalidOperation_ArrayCreateInstance_NotARuntimeType = "Array.CreateInstance() can only accept Type objects created by the runtime.";

	public const string InvalidOperation_TooEarly = "Internal Error: This operation cannot be invoked in an eager class constructor.";

	public const string InvalidOperation_NullContext = "Cannot call Set on a null context";

	public const string InvalidOperation_CannotUseAFCOtherThread = "AsyncFlowControl object must be used on the thread where it was created.";

	public const string InvalidOperation_CannotRestoreUnsupressedFlow = "Cannot restore context flow when it is not suppressed.";

	public const string InvalidOperation_CannotSupressFlowMultipleTimes = "Context flow is already suppressed.";

	public const string InvalidOperation_CannotUseAFCMultiple = "AsyncFlowControl object can be used only once to call Undo().";

	public const string InvalidOperation_AsyncFlowCtrlCtxMismatch = "AsyncFlowControl objects can be used to restore flow only on a Context that had its flow suppressed.";

	public const string InvalidProgram_Default = "Common Language Runtime detected an invalid program.";

	public const string InvalidProgram_Specific = "Common Language Runtime detected an invalid program. The body of method '{0}' is invalid.";

	public const string InvalidProgram_Vararg = "Method '{0}' has a variable argument list. Variable argument lists are not supported in .NET Core.";

	public const string InvalidProgram_CallVirtFinalize = "Object.Finalize() can not be called directly. It is only callable by the runtime.";

	public const string InvalidTimeZone_InvalidRegistryData = "The time zone ID '{0}' was found on the local computer, but the registry information was corrupt.";

	public const string IO_FileExists_Name = "The file '{0}' already exists.";

	public const string IO_FileName_Name = "File name: '{0}'";

	public const string IO_FileNotFound = "Unable to find the specified file.";

	public const string IO_FileNotFound_FileName = "Could not load file or assembly '{0}'. The system cannot find the file specified.";

	public const string IO_FileLoad = "Could not load the specified file.";

	public const string IO_FileLoad_FileName = "Could not load the file '{0}'.";

	public const string IO_PathNotFound_NoPathName = "Could not find a part of the path.";

	public const string IO_PathNotFound_Path = "Could not find a part of the path '{0}'.";

	public const string IO_PathTooLong = "The specified path, file name, or both are too long. The fully qualified file name must be less than 260 characters, and the directory name must be less than 248 characters.";

	public const string IO_SharingViolation_File = "The process cannot access the file '{0}' because it is being used by another process.";

	public const string IO_SharingViolation_NoFileName = "The process cannot access the file because it is being used by another process.";

	public const string IO_AlreadyExists_Name = "Cannot create '{0}' because a file or directory with the same name already exists.";

	public const string UnauthorizedAccess_IODenied_NoPathName = "Access to the path is denied.";

	public const string UnauthorizedAccess_IODenied_Path = "Access to the path '{0}' is denied.";

	public const string Lazy_CreateValue_NoParameterlessCtorForT = "The lazily-initialized type does not have a public, parameterless constructor.";

	public const string Lazy_ctor_ModeInvalid = "The mode argument specifies an invalid value.";

	public const string Lazy_StaticInit_InvalidOperation = "ValueFactory returned null.";

	public const string Lazy_ToString_ValueNotCreated = "Value is not created.";

	public const string Lazy_Value_RecursiveCallsToValue = "ValueFactory attempted to access the Value property of this instance.";

	public const string MissingConstructor_Name = "Constructor on type '{0}' not found.";

	public const string MustUseCCRewrite = "An assembly (probably '{1}') must be rewritten using the code contracts binary rewriter (CCRewrite) because it is calling Contract.{0} and the CONTRACTS_FULL symbol is defined.  Remove any explicit definitions of the CONTRACTS_FULL symbol from your project and rebuild.  CCRewrite can be downloaded from http://go.microsoft.com/fwlink/?LinkID=169180. \\r\\nAfter the rewriter is installed, it can be enabled in Visual Studio from the project's Properties page on the Code Contracts pane.  Ensure that 'Perform Runtime Contract Checking' is enabled, which will define CONTRACTS_FULL.";

	public const string NotSupported_FixedSizeCollection = "Collection was of a fixed size.";

	public const string NotSupported_MaxWaitHandles = "The number of WaitHandles must be less than or equal to 64.";

	public const string NotSupported_NoCodepageData = "No data is available for encoding {0}. For information on defining a custom encoding, see the documentation for the Encoding.RegisterProvider method.";

	public const string NotSupported_ReadOnlyCollection = "Collection is read-only.";

	public const string NotSupported_StringComparison = "The string comparison type passed in is currently not supported.";

	public const string NotSupported_VoidArray = "Arrays of System.Void are not supported.";

	public const string NotSupported_ByRefLike = "Cannot create boxed ByRef-like values.";

	public const string NotSupported_Type = "Type is not supported.";

	public const string NotSupported_WaitAllSTAThread = "WaitAll for multiple handles on a STA thread is not supported.";

	public const string ObjectDisposed_Generic = "Cannot access a disposed object.";

	public const string ObjectDisposed_ObjectName_Name = "Object name: '{0}'.";

	public const string Overflow_Byte = "Value was either too large or too small for an unsigned byte.";

	public const string Overflow_Char = "Value was either too large or too small for a character.";

	public const string Overflow_Decimal = "Value was either too large or too small for a Decimal.";

	public const string Overflow_Double = "Value was either too large or too small for a Double.";

	public const string Overflow_TimeSpanElementTooLarge = "The TimeSpan could not be parsed because at least one of the numeric components is out of range or contains too many digits.";

	public const string Overflow_Duration = "The duration cannot be returned for TimeSpan.MinValue because the absolute value of TimeSpan.MinValue exceeds the value of TimeSpan.MaxValue.";

	public const string Overflow_Int16 = "Value was either too large or too small for an Int16.";

	public const string Overflow_Int32 = "Value was either too large or too small for an Int32.";

	public const string Overflow_Int64 = "Value was either too large or too small for an Int64.";

	public const string Overflow_NegateTwosCompNum = "Negating the minimum value of a twos complement number is invalid.";

	public const string Overflow_NegativeUnsigned = "The string was being parsed as an unsigned number and could not have a negative sign.";

	public const string Overflow_SByte = "Value was either too large or too small for a signed byte.";

	public const string Overflow_Single = "Value was either too large or too small for a Single.";

	public const string Overflow_TimeSpanTooLong = "TimeSpan overflowed because the duration is too long.";

	public const string Overflow_UInt16 = "Value was either too large or too small for a UInt16.";

	public const string Overflow_UInt32 = "Value was either too large or too small for a UInt32.";

	public const string Overflow_UInt64 = "Value was either too large or too small for a UInt64.";

	public const string Rank_MultiDimNotSupported = "Only single dimension arrays are supported here.";

	public const string RuntimeWrappedException = "An object that does not derive from System.Exception has been wrapped in a RuntimeWrappedException.";

	public const string SpinWait_SpinUntil_ArgumentNull = "The condition argument is null.";

	public const string Serialization_CorruptField = "The value of the field '{0}' is invalid.  The serialized data is corrupt.";

	public const string Serialization_InvalidData = "An error occurred while deserializing the object.  The serialized data is corrupt.";

	public const string Serialization_InvalidEscapeSequence = "The serialized data contained an invalid escape sequence '\\\\{0}'.";

	public const string Serialization_InvalidType = "Only system-provided types can be passed to the GetUninitializedObject method. '{0}' is not a valid instance of a type.";

	public const string SpinWait_SpinUntil_TimeoutWrong = "The timeout must represent a value between -1 and Int32.MaxValue, inclusive.";

	public const string Threading_AbandonedMutexException = "The wait completed due to an abandoned mutex.";

	public const string Threading_SemaphoreFullException = "Adding the specified count to the semaphore would cause it to exceed its maximum count.";

	public const string Threading_ThreadInterrupted = "Thread was interrupted from a waiting state.";

	public const string Threading_WaitHandleCannotBeOpenedException = "No handle of the given name exists.";

	public const string Threading_WaitHandleCannotBeOpenedException_InvalidHandle = "A WaitHandle with system-wide name '{0}' cannot be created. A WaitHandle of a different type might have the same name.";

	public const string TimeZoneNotFound_MissingRegistryData = "The time zone ID '{0}' was not found on the local computer.";

	public const string TypeInitialization_Default = "Type constructor threw an exception.";

	public const string TypeInitialization_Type = "The type initializer for '{0}' threw an exception.";

	public const string TypeInitialization_Type_NoTypeAvailable = "A type initializer threw an exception. To determine which type, inspect the InnerException's StackTrace property.";

	public const string Verification_Exception = "Operation could destabilize the runtime.";

	public const string Arg_EnumFormatUnderlyingTypeAndObjectMustBeSameType = "Enum underlying type and the object must be same type or object. Type passed in was '{0}'; the enum underlying type was '{1}'.";

	public const string Format_InvalidEnumFormatSpecification = "Format String can be only 'G', 'g', 'X', 'x', 'F', 'f', 'D' or 'd'.";

	public const string Arg_MustBeEnumBaseTypeOrEnum = "The value passed in must be an enum base or an underlying type for an enum, such as an Int32.";

	public const string Arg_EnumUnderlyingTypeAndObjectMustBeSameType = "Enum underlying type and the object must be same type or object must be a String. Type passed in was '{0}'; the enum underlying type was '{1}'.";

	public const string Arg_MustBeType = "Type must be a type provided by the runtime.";

	public const string Arg_MustContainEnumInfo = "Must specify valid information for parsing in the string.";

	public const string Arg_EnumValueNotFound = "Requested value '{0}' was not found.";

	public const string Argument_StringZeroLength = "String cannot be of zero length.";

	public const string Argument_StringFirstCharIsZero = "The first char in the string is the null character.";

	public const string Argument_LongEnvVarValue = "Environment variable name or value is too long.";

	public const string Argument_IllegalEnvVarName = "Environment variable name cannot contain equal character.";

	public const string AssumptionFailed = "Assumption failed.";

	public const string AssumptionFailed_Cnd = "Assumption failed: {0}";

	public const string AssertionFailed = "Assertion failed.";

	public const string AssertionFailed_Cnd = "Assertion failed: {0}";

	public const string PreconditionFailed = "Precondition failed.";

	public const string PreconditionFailed_Cnd = "Precondition failed: {0}";

	public const string PostconditionFailed = "Postcondition failed.";

	public const string PostconditionFailed_Cnd = "Postcondition failed: {0}";

	public const string PostconditionOnExceptionFailed = "Postcondition failed after throwing an exception.";

	public const string PostconditionOnExceptionFailed_Cnd = "Postcondition failed after throwing an exception: {0}";

	public const string InvariantFailed = "Invariant failed.";

	public const string InvariantFailed_Cnd = "Invariant failed: {0}";

	public const string MissingEncodingNameResource = "Could not find a resource entry for the encoding codepage '{0} - {1}'";

	public const string Globalization_cp_1200 = "Unicode";

	public const string Globalization_cp_1201 = "Unicode (Big-Endian)";

	public const string Globalization_cp_12000 = "Unicode (UTF-32)";

	public const string Globalization_cp_12001 = "Unicode (UTF-32 Big-Endian)";

	public const string Globalization_cp_20127 = "US-ASCII";

	public const string Globalization_cp_28591 = "Western European (ISO)";

	public const string Globalization_cp_65000 = "Unicode (UTF-7)";

	public const string Globalization_cp_65001 = "Unicode (UTF-8)";

	public const string DebugAssertBanner = "---- DEBUG ASSERTION FAILED ----";

	public const string DebugAssertLongMessage = "---- Assert Long Message ----";

	public const string DebugAssertShortMessage = "---- Assert Short Message ----";

	public const string InvalidCast_Empty = "Object cannot be cast to Empty.";

	public const string Arg_UnknownTypeCode = "Unknown TypeCode value.";

	public const string Format_BadDatePattern = "Could not determine the order of year, month, and date from '{0}'.";

	public const string Format_BadDateTime = "String was not recognized as a valid DateTime.";

	public const string Format_BadDateTimeCalendar = "The DateTime represented by the string is not supported in calendar {0}.";

	public const string Format_BadDayOfWeek = "String was not recognized as a valid DateTime because the day of week was incorrect.";

	public const string Format_DateOutOfRange = "The DateTime represented by the string is out of range.";

	public const string Format_MissingIncompleteDate = "There must be at least a partial date with a year present in the input.";

	public const string Format_OffsetOutOfRange = "The time zone offset must be within plus or minus 14 hours.";

	public const string Format_RepeatDateTimePattern = "DateTime pattern '{0}' appears more than once with different values.";

	public const string Format_UnknowDateTimeWord = "The string was not recognized as a valid DateTime. There is an unknown word starting at index {0}.";

	public const string Format_UTCOutOfRange = "The UTC representation of the date falls outside the year range 1-9999.";

	public const string RFLCT_Ambiguous = "Ambiguous match found.";

	public const string AggregateException_ctor_DefaultMessage = "One or more errors occurred.";

	public const string AggregateException_ctor_InnerExceptionNull = "An element of innerExceptions was null.";

	public const string AggregateException_DeserializationFailure = "The serialization stream contains no inner exceptions.";

	public const string AggregateException_InnerException = "(Inner Exception #{0}) ";

	public const string ArgumentOutOfRange_TimeoutTooLarge = "Time-out interval must be less than 2^32-2.";

	public const string ArgumentOutOfRange_PeriodTooLarge = "Period must be less than 2^32-2.";

	public const string TaskScheduler_FromCurrentSynchronizationContext_NoCurrent = "The current SynchronizationContext may not be used as a TaskScheduler.";

	public const string TaskScheduler_ExecuteTask_WrongTaskScheduler = "ExecuteTask may not be called for a task which was previously queued to a different TaskScheduler.";

	public const string TaskScheduler_InconsistentStateAfterTryExecuteTaskInline = "The TryExecuteTaskInline call to the underlying scheduler succeeded, but the task body was not invoked.";

	public const string TaskSchedulerException_ctor_DefaultMessage = "An exception was thrown by a TaskScheduler.";

	public const string Task_MultiTaskContinuation_FireOptions = "It is invalid to exclude specific continuation kinds for continuations off of multiple tasks.";

	public const string Task_ContinueWith_ESandLR = "The specified TaskContinuationOptions combined LongRunning and ExecuteSynchronously.  Synchronous continuations should not be long running.";

	public const string Task_MultiTaskContinuation_EmptyTaskList = "The tasks argument contains no tasks.";

	public const string Task_MultiTaskContinuation_NullTask = "The tasks argument included a null value.";

	public const string Task_FromAsync_PreferFairness = "It is invalid to specify TaskCreationOptions.PreferFairness in calls to FromAsync.";

	public const string Task_FromAsync_LongRunning = "It is invalid to specify TaskCreationOptions.LongRunning in calls to FromAsync.";

	public const string AsyncMethodBuilder_InstanceNotInitialized = "The builder was not properly initialized.";

	public const string TaskT_TransitionToFinal_AlreadyCompleted = "An attempt was made to transition a task to a final state when it had already completed.";

	public const string TaskT_DebuggerNoResult = "{Not yet computed}";

	public const string OperationCanceled = "The operation was canceled.";

	public const string CancellationToken_CreateLinkedToken_TokensIsEmpty = "No tokens were supplied.";

	public const string CancellationTokenSource_Disposed = "The CancellationTokenSource has been disposed.";

	public const string CancellationToken_SourceDisposed = "The CancellationTokenSource associated with this CancellationToken has been disposed.";

	public const string TaskExceptionHolder_UnknownExceptionType = "(Internal)Expected an Exception or an IEnumerable<Exception>";

	public const string TaskExceptionHolder_UnhandledException = "A Task's exception(s) were not observed either by Waiting on the Task or accessing its Exception property. As a result, the unobserved exception was rethrown by the finalizer thread.";

	public const string Task_Delay_InvalidMillisecondsDelay = "The value needs to be either -1 (signifying an infinite timeout), 0 or a positive integer.";

	public const string Task_Delay_InvalidDelay = "The value needs to translate in milliseconds to -1 (signifying an infinite timeout), 0 or a positive integer less than or equal to Int32.MaxValue.";

	public const string Task_Dispose_NotCompleted = "A task may only be disposed if it is in a completion state (RanToCompletion, Faulted or Canceled).";

	public const string Task_WaitMulti_NullTask = "The tasks array included at least one null element.";

	public const string Task_ContinueWith_NotOnAnything = "The specified TaskContinuationOptions excluded all continuation kinds.";

	public const string Task_RunSynchronously_AlreadyStarted = "RunSynchronously may not be called on a task that was already started.";

	public const string Task_ThrowIfDisposed = "The task has been disposed.";

	public const string Task_RunSynchronously_TaskCompleted = "RunSynchronously may not be called on a task that has already completed.";

	public const string Task_RunSynchronously_Promise = "RunSynchronously may not be called on a task not bound to a delegate, such as the task returned from an asynchronous method.";

	public const string Task_RunSynchronously_Continuation = "RunSynchronously may not be called on a continuation task.";

	public const string Task_Start_AlreadyStarted = "Start may not be called on a task that was already started.";

	public const string Task_Start_ContinuationTask = "Start may not be called on a continuation task.";

	public const string Task_Start_Promise = "Start may not be called on a promise-style task.";

	public const string Task_Start_TaskCompleted = "Start may not be called on a task that has completed.";

	public const string TaskCanceledException_ctor_DefaultMessage = "A task was canceled.";

	public const string TaskCompletionSourceT_TrySetException_NoExceptions = "The exceptions collection was empty.";

	public const string TaskCompletionSourceT_TrySetException_NullException = "The exceptions collection included at least one null element.";

	public const string Argument_MinMaxValue = "'{0}' cannot be greater than {1}.";

	public const string ExecutionContext_ExceptionInAsyncLocalNotification = "An exception was not handled in an AsyncLocal<T> notification callback.";

	public const string InvalidOperation_WrongAsyncResultOrEndCalledMultiple = "Either the IAsyncResult object did not come from the corresponding async method on this type, or the End method was called multiple times with the same IAsyncResult.";

	public const string SpinLock_IsHeldByCurrentThread = "Thread tracking is disabled.";

	public const string SpinLock_TryEnter_LockRecursionException = "The calling thread already holds the lock.";

	public const string SpinLock_Exit_SynchronizationLockException = "The calling thread does not hold the lock.";

	public const string SpinLock_TryReliableEnter_ArgumentException = "The tookLock argument must be set to false before calling this method.";

	public const string SpinLock_TryEnter_ArgumentOutOfRange = "The timeout must be a value between -1 and Int32.MaxValue, inclusive.";

	public const string ManualResetEventSlim_Disposed = "The event has been disposed.";

	public const string ManualResetEventSlim_ctor_SpinCountOutOfRange = "The spinCount argument must be in the range 0 to {0}, inclusive.";

	public const string ManualResetEventSlim_ctor_TooManyWaiters = "There are too many threads currently waiting on the event. A maximum of {0} waiting threads are supported.";

	public const string InvalidOperation_SendNotSupportedOnWindowsRTSynchronizationContext = "Send is not supported in the Windows Runtime SynchronizationContext";

	public const string InvalidOperation_SetData_OnlyOnce = "SetData can only be used to set the value of a given name once.";

	public const string SemaphoreSlim_Disposed = "The semaphore has been disposed.";

	public const string SemaphoreSlim_Release_CountWrong = "The releaseCount argument must be greater than zero.";

	public const string SemaphoreSlim_Wait_TimeoutWrong = "The timeout must represent a value between -1 and Int32.MaxValue, inclusive.";

	public const string SemaphoreSlim_ctor_MaxCountWrong = "The maximumCount argument must be a positive number. If a maximum is not required, use the constructor without a maxCount parameter.";

	public const string SemaphoreSlim_ctor_InitialCountWrong = "The initialCount argument must be non-negative and less than or equal to the maximumCount.";

	public const string ThreadLocal_ValuesNotAvailable = "The ThreadLocal object is not tracking values. To use the Values property, use a ThreadLocal constructor that accepts the trackAllValues parameter and set the parameter to true.";

	public const string ThreadLocal_Value_RecursiveCallsToValue = "ValueFactory attempted to access the Value property of this instance.";

	public const string ThreadLocal_Disposed = "The ThreadLocal object has been disposed.";

	public const string LockRecursionException_WriteAfterReadNotAllowed = "Write lock may not be acquired with read lock held. This pattern is prone to deadlocks. Please ensure that read locks are released before taking a write lock. If an upgrade is necessary, use an upgrade lock in place of the read lock.";

	public const string LockRecursionException_RecursiveWriteNotAllowed = "Recursive write lock acquisitions not allowed in this mode.";

	public const string LockRecursionException_ReadAfterWriteNotAllowed = "A read lock may not be acquired with the write lock held in this mode.";

	public const string LockRecursionException_RecursiveUpgradeNotAllowed = "Recursive upgradeable lock acquisitions not allowed in this mode.";

	public const string LockRecursionException_RecursiveReadNotAllowed = "Recursive read lock acquisitions not allowed in this mode.";

	public const string SynchronizationLockException_IncorrectDispose = "The lock is being disposed while still being used. It either is being held by a thread and/or has active waiters waiting to acquire the lock.";

	public const string SynchronizationLockException_MisMatchedWrite = "The write lock is being released without being held.";

	public const string LockRecursionException_UpgradeAfterReadNotAllowed = "Upgradeable lock may not be acquired with read lock held.";

	public const string LockRecursionException_UpgradeAfterWriteNotAllowed = "Upgradeable lock may not be acquired with write lock held in this mode. Acquiring Upgradeable lock gives the ability to read along with an option to upgrade to a writer.";

	public const string SynchronizationLockException_MisMatchedUpgrade = "The upgradeable lock is being released without being held.";

	public const string SynchronizationLockException_MisMatchedRead = "The read lock is being released without being held.";

	public const string InvalidOperation_TimeoutsNotSupported = "Timeouts are not supported on this stream.";

	public const string NotSupported_UnreadableStream = "Stream does not support reading.";

	public const string NotSupported_UnwritableStream = "Stream does not support writing.";

	public const string ObjectDisposed_StreamClosed = "Cannot access a closed Stream.";

	public const string NotSupported_SubclassOverride = "Derived classes must provide an implementation.";

	public const string InvalidOperation_NoPublicRemoveMethod = "Cannot remove the event handler since no public remove method exists for the event.";

	public const string InvalidOperation_NoPublicAddMethod = "Cannot add the event handler since no public add method exists for the event.";

	public const string SerializationException = "Serialization error.";

	public const string Serialization_NotFound = "Member '{0}' was not found.";

	public const string Serialization_OptionalFieldVersionValue = "Version value must be positive.";

	public const string Serialization_SameNameTwice = "Cannot add the same member twice to a SerializationInfo object.";

	public const string NotSupported_AbstractNonCLS = "This non-CLS method is not implemented.";

	public const string NotSupported_NoTypeInfo = "Cannot resolve {0} to a TypeInfo object.";

	public const string Arg_CustomAttributeFormatException = "Binary format of the specified custom attribute was invalid.";

	public const string Argument_InvalidMemberForNamedArgument = "The member must be either a field or a property.";

	public const string Arg_InvalidFilterCriteriaException = "Specified filter criteria was invalid.";

	public const string Arg_ParmArraySize = "Must specify one or more parameters.";

	public const string Arg_MustBePointer = "Type must be a Pointer.";

	public const string Arg_InvalidHandle = "Invalid handle.";

	public const string Argument_InvalidEnum = "The Enum type should contain one and only one instance field.";

	public const string Argument_MustHaveAttributeBaseClass = "Type passed in must be derived from System.Attribute or System.Attribute itself.";

	public const string InvalidFilterCriteriaException_CritString = "A String must be provided for the filter criteria.";

	public const string InvalidFilterCriteriaException_CritInt = "An Int32 must be provided for the filter criteria.";

	public const string InvalidOperation_NotSupportedOnWinRTEvent = "Adding or removing event handlers dynamically is not supported on WinRT events.";

	public const string PlatformNotSupported_ReflectionOnly = "ReflectionOnly loading is not supported on this platform.";

	public const string PlatformNotSupported_OSXFileLocking = "Locking/unlocking file regions is not supported on this platform. Use FileShare on the entire file instead.";

	public const string MissingMember_Name = "Member '{0}' not found.";

	public const string MissingMethod_Name = "Method '{0}' not found.";

	public const string MissingField_Name = "Field '{0}' not found.";

	public const string Format_StringZeroLength = "String cannot have zero length.";

	public const string Security_CannotReadRegistryData = "The time zone ID '{0}' was found on the local computer, but the application does not have permission to read the registry information.";

	public const string Security_InvalidAssemblyPublicKey = "Invalid assembly public key.";

	public const string Security_RegistryPermission = "Requested registry access is not allowed.";

	public const string ClassLoad_General = "Could not load type '{0}' from assembly '{1}'.";

	public const string ClassLoad_RankTooLarge = "'{0}' from assembly '{1}' has too many dimensions.";

	public const string ClassLoad_ExplicitGeneric = "Could not load type '{0}' from assembly '{1}' because generic types cannot have explicit layout.";

	public const string ClassLoad_BadFormat = "Could not load type '{0}' from assembly '{1}' because the format is invalid.";

	public const string ClassLoad_ValueClassTooLarge = "Array of type '{0}' from assembly '{1}' cannot be created because base value type is too large.";

	public const string ClassLoad_ExplicitLayout = "Could not load type '{0}' from assembly '{1}' because it contains an object field at offset '{2}' that is incorrectly aligned or overlapped by a non-object field.";

	public const string EE_MissingMethod = "Method not found: '{0}'.";

	public const string EE_MissingField = "Field not found: '{0}'.";

	public const string UnauthorizedAccess_RegistryKeyGeneric_Key = "Access to the registry key '{0}' is denied.";

	public const string UnknownError_Num = "Unknown error '{0}'.";

	public const string Argument_NeedStructWithNoRefs = "The specified Type must be a struct containing no references.";

	public const string ArgumentNull_Buffer = "Buffer cannot be null.";

	public const string ArgumentOutOfRange_AddressSpace = "The number of bytes cannot exceed the virtual address space on a 32 bit machine.";

	public const string ArgumentOutOfRange_UIntPtrMaxMinusOne = "The length of the buffer must be less than the maximum UIntPtr value for your platform.";

	public const string Arg_BufferTooSmall = "Not enough space available in the buffer.";

	public const string InvalidOperation_MustCallInitialize = "You must call Initialize on this object instance before using it.";

	public const string ArgumentException_BufferNotFromPool = "The buffer is not associated with this pool and may not be returned to it.";

	public const string Argument_InvalidSafeBufferOffLen = "Offset and length were greater than the size of the SafeBuffer.";

	public const string Argument_InvalidSeekOrigin = "Invalid seek origin.";

	public const string Argument_NotEnoughBytesToRead = "There are not enough bytes remaining in the accessor to read at this position.";

	public const string Argument_NotEnoughBytesToWrite = "There are not enough bytes remaining in the accessor to write at this position.";

	public const string Argument_OffsetAndCapacityOutOfBounds = "Offset and capacity were greater than the size of the view.";

	public const string ArgumentOutOfRange_UnmanagedMemStreamLength = "UnmanagedMemoryStream length must be non-negative and less than 2^63 - 1 - baseAddress.";

	public const string Argument_UnmanagedMemAccessorWrapAround = "The UnmanagedMemoryAccessor capacity and offset would wrap around the high end of the address space.";

	public const string ArgumentOutOfRange_StreamLength = "Stream length must be non-negative and less than 2^31 - 1 - origin.";

	public const string ArgumentOutOfRange_UnmanagedMemStreamWrapAround = "The UnmanagedMemoryStream capacity would wrap around the high end of the address space.";

	public const string InvalidOperation_CalledTwice = "The method cannot be called twice on the same instance.";

	public const string IO_FixedCapacity = "Unable to expand length of this stream beyond its capacity.";

	public const string IO_SeekBeforeBegin = "An attempt was made to move the position before the beginning of the stream.";

	public const string IO_StreamTooLong = "Stream was too long.";

	public const string Arg_BadDecimal = "Read an invalid decimal value from the buffer.";

	public const string NotSupported_Reading = "Accessor does not support reading.";

	public const string NotSupported_UmsSafeBuffer = "This operation is not supported for an UnmanagedMemoryStream created from a SafeBuffer.";

	public const string NotSupported_Writing = "Accessor does not support writing.";

	public const string NotSupported_UnseekableStream = "Stream does not support seeking.";

	public const string IndexOutOfRange_UMSPosition = "Unmanaged memory stream position was beyond the capacity of the stream.";

	public const string ObjectDisposed_StreamIsClosed = "Cannot access a closed Stream.";

	public const string ObjectDisposed_ViewAccessorClosed = "Cannot access a closed accessor.";

	public const string ArgumentOutOfRange_PositionLessThanCapacityRequired = "The position may not be greater or equal to the capacity of the accessor.";

	public const string IO_EOF_ReadBeyondEOF = "Unable to read beyond the end of the stream.";

	public const string Arg_EndOfStreamException = "Attempted to read past the end of the stream.";

	public const string ObjectDisposed_FileClosed = "Cannot access a closed file.";

	public const string Arg_InvalidSearchPattern = "Search pattern cannot contain \\\"..\\\" to move up directories and can be contained only internally in file/directory names, as in \\\"a..b\\\".";

	public const string ArgumentOutOfRange_FileLengthTooBig = "Specified file length was too large for the file system.";

	public const string Argument_InvalidHandle = "'handle' has been disposed or is an invalid handle.";

	public const string Argument_AlreadyBoundOrSyncHandle = "'handle' has already been bound to the thread pool, or was not opened for asynchronous I/O.";

	public const string Argument_PreAllocatedAlreadyAllocated = "'preAllocated' is already in use.";

	public const string Argument_NativeOverlappedAlreadyFree = "'overlapped' has already been freed.";

	public const string Argument_NativeOverlappedWrongBoundHandle = "'overlapped' was not allocated by this ThreadPoolBoundHandle instance.";

	public const string Arg_HandleNotAsync = "Handle does not support asynchronous operations. The parameters to the FileStream constructor may need to be changed to indicate that the handle was opened synchronously (that is, it was not opened for overlapped I/O).";

	public const string ArgumentNull_Path = "Path cannot be null.";

	public const string Argument_EmptyPath = "Empty path name is not legal.";

	public const string Argument_InvalidFileModeAndAccessCombo = "Combining FileMode: {0} with FileAccess: {1} is invalid.";

	public const string Argument_InvalidAppendMode = "Append access can be requested only in write-only mode.";

	public const string IO_UnknownFileName = "[Unknown]";

	public const string IO_FileStreamHandlePosition = "The OS handle's position is not what FileStream expected. Do not use a handle simultaneously in one FileStream and in Win32 code or another FileStream. This may cause data loss.";

	public const string NotSupported_FileStreamOnNonFiles = "FileStream was asked to open a device that was not a file. For support for devices like 'com1:' or 'lpt1:', call CreateFile, then use the FileStream constructors that take an OS handle as an IntPtr.";

	public const string IO_BindHandleFailed = "BindHandle for ThreadPool failed on this handle.";

	public const string Arg_HandleNotSync = "Handle does not support synchronous operations. The parameters to the FileStream constructor may need to be changed to indicate that the handle was opened asynchronously (that is, it was opened explicitly for overlapped I/O).";

	public const string IO_SetLengthAppendTruncate = "Unable to truncate data that previously existed in a file opened in Append mode.";

	public const string IO_SeekAppendOverwrite = "Unable seek backward to overwrite data that previously existed in a file opened in Append mode.";

	public const string IO_FileTooLongOrHandleNotSync = "IO operation will not work. Most likely the file will become too long or the handle was not opened to support synchronous IO operations.";

	public const string IndexOutOfRange_IORaceCondition = "Probable I/O race condition detected while copying memory. The I/O package is not thread safe by default. In multithreaded applications, a stream must be accessed in a thread-safe way, such as a thread-safe wrapper returned by TextReader's or TextWriter's Synchronized methods. This also applies to classes like StreamWriter and StreamReader.";

	public const string Arg_ResourceFileUnsupportedVersion = "The ResourceReader class does not know how to read this version of .resources files.";

	public const string Resources_StreamNotValid = "Stream is not a valid resource file.";

	public const string BadImageFormat_ResourcesHeaderCorrupted = "Corrupt .resources file. Unable to read resources from this file because of invalid header information. Try regenerating the .resources file.";

	public const string Argument_StreamNotReadable = "Stream was not readable.";

	public const string BadImageFormat_NegativeStringLength = "Corrupt .resources file. String length must be non-negative.";

	public const string BadImageFormat_ResourcesNameInvalidOffset = "Corrupt .resources file. The Invalid offset into name section is .";

	public const string BadImageFormat_TypeMismatch = "Corrupt .resources file.  The specified type doesn't match the available data in the stream.";

	public const string BadImageFormat_ResourceNameCorrupted_NameIndex = "Corrupt .resources file. The resource name for name index that extends past the end of the stream is ";

	public const string BadImageFormat_ResourcesDataInvalidOffset = "Corrupt .resources file. Invalid offset  into data section is ";

	public const string Format_Bad7BitInt32 = "Too many bytes in what should have been a 7 bit encoded Int32.";

	public const string BadImageFormat_InvalidType = "Corrupt .resources file.  The specified type doesn't exist.";

	public const string ResourceReaderIsClosed = "ResourceReader is closed.";

	public const string Arg_MissingManifestResourceException = "Unable to find manifest resource.";

	public const string Serialization_MissingKeys = "The keys for this dictionary are missing.";

	public const string Serialization_NullKey = "One of the serialized keys is null.";

	public const string NotSupported_KeyCollectionSet = "Mutating a key collection derived from a dictionary is not allowed.";

	public const string NotSupported_ValueCollectionSet = "Mutating a value collection derived from a dictionary is not allowed.";

	public const string IO_IO_StreamTooLong = "Stream was too long.";

	public const string UnauthorizedAccess_MemStreamBuffer = "MemoryStream's internal buffer cannot be accessed.";

	public const string NotSupported_MemStreamNotExpandable = "Memory stream is not expandable.";

	public const string IO_IO_SeekBeforeBegin = "An attempt was made to move the position before the beginning of the stream.";

	public const string ArgumentNull_Stream = "Stream cannot be null.";

	public const string IO_IO_InvalidStringLen_Len = "BinaryReader encountered an invalid string length of {0} characters.";

	public const string ArgumentOutOfRange_BinaryReaderFillBuffer = "The number of bytes requested does not fit into BinaryReader's internal buffer.";

	public const string Serialization_InsufficientDeserializationState = "Insufficient state to deserialize the object. Missing field '{0}'.";

	public const string NotSupported_UnitySerHolder = "The UnitySerializationHolder object is designed to transmit information about other types and is not serializable itself.";

	public const string Serialization_UnableToFindModule = "The given module {0} cannot be found within the assembly {1}.";

	public const string Argument_InvalidUnity = "Invalid Unity type.";

	public const string InvalidOperation_InvalidHandle = "The handle is invalid.";

	public const string PlatformNotSupported_NamedSynchronizationPrimitives = "The named version of this synchronization primitive is not supported on this platform.";

	public const string InvalidOperation_EmptyQueue = "Queue empty.";

	public const string Overflow_MutexReacquireCount = "The current thread attempted to reacquire a mutex that has reached its maximum acquire count.";

	public const string Serialization_InsufficientState = "Insufficient state to return the real object.";

	public const string Serialization_UnknownMember = "Cannot get the member '{0}'.";

	public const string Serialization_NullSignature = "The method signature cannot be null.";

	public const string Serialization_MemberTypeNotRecognized = "Unknown member type.";

	public const string Serialization_BadParameterInfo = "Non existent ParameterInfo. Position bigger than member's parameters length.";

	public const string Serialization_NoParameterInfo = "Serialized member does not have a ParameterInfo.";

	public const string ArgumentNull_Assembly = "Assembly cannot be null.";

	public const string Arg_InvalidNeutralResourcesLanguage_Asm_Culture = "The NeutralResourcesLanguageAttribute on the assembly \"{0}\" specifies an invalid culture name: \"{1}\".";

	public const string Arg_InvalidNeutralResourcesLanguage_FallbackLoc = "The NeutralResourcesLanguageAttribute specifies an invalid or unrecognized ultimate resource fallback location: \"{0}\".";

	public const string Arg_InvalidSatelliteContract_Asm_Ver = "Satellite contract version attribute on the assembly '{0}' specifies an invalid version: {1}.";

	public const string Arg_ResMgrNotResSet = "Type parameter must refer to a subclass of ResourceSet.";

	public const string BadImageFormat_ResourceNameCorrupted = "Corrupt .resources file. A resource name extends past the end of the stream.";

	public const string BadImageFormat_ResourcesNameTooLong = "Corrupt .resources file. Resource name extends past the end of the file.";

	public const string InvalidOperation_ResMgrBadResSet_Type = "'{0}': ResourceSet derived classes must provide a constructor that takes a String file name and a constructor that takes a Stream.";

	public const string InvalidOperation_ResourceNotStream_Name = "Resource '{0}' was not a Stream - call GetObject instead.";

	public const string MissingManifestResource_MultipleBlobs = "A case-insensitive lookup for resource file \"{0}\" in assembly \"{1}\" found multiple entries. Remove the duplicates or specify the exact case.";

	public const string MissingManifestResource_NoNeutralAsm = "Could not find any resources appropriate for the specified culture or the neutral culture.  Make sure \"{0}\" was correctly embedded or linked into assembly \"{1}\" at compile time, or that all the satellite assemblies required are loadable and fully signed.";

	public const string MissingManifestResource_NoNeutralDisk = "Could not find any resources appropriate for the specified culture (or the neutral culture) on disk.";

	public const string MissingManifestResource_NoPRIresources = "Unable to open Package Resource Index.";

	public const string MissingManifestResource_ResWFileNotLoaded = "Unable to load resources for resource file \"{0}\" in package \"{1}\".";

	public const string MissingSatelliteAssembly_Culture_Name = "The satellite assembly named \"{1}\" for fallback culture \"{0}\" either could not be found or could not be loaded. This is generally a setup problem. Please consider reinstalling or repairing the application.";

	public const string MissingSatelliteAssembly_Default = "Resource lookup fell back to the ultimate fallback resources in a satellite assembly, but that satellite either was not found or could not be loaded. Please consider reinstalling or repairing the application.";

	public const string NotSupported_ObsoleteResourcesFile = "Found an obsolete .resources file in assembly '{0}'. Rebuild that .resources file then rebuild that assembly.";

	public const string NotSupported_ResourceObjectSerialization = "Cannot read resources that depend on serialization.";

	public const string ObjectDisposed_ResourceSet = "Cannot access a closed resource set.";

	public const string Arg_ResourceNameNotExist = "The specified resource name \"{0}\" does not exist in the resource file.";

	public const string BadImageFormat_ResourceDataLengthInvalid = "Corrupt .resources file.  The specified data length '{0}' is not a valid position in the stream.";

	public const string BadImageFormat_ResourcesIndexTooLong = "Corrupt .resources file. String for name index '{0}' extends past the end of the file.";

	public const string InvalidOperation_ResourceNotString_Name = "Resource '{0}' was not a String - call GetObject instead.";

	public const string InvalidOperation_ResourceNotString_Type = "Resource was of type '{0}' instead of String - call GetObject instead.";

	public const string NotSupported_WrongResourceReader_Type = "This .resources file should not be read with this reader. The resource reader type is \"{0}\".";

	public const string Arg_MustBeDelegate = "Type must derive from Delegate.";

	public const string NotSupported_GlobalMethodSerialization = "Serialization of global methods (including implicit serialization via the use of asynchronous delegates) is not supported.";

	public const string NotSupported_DelegateSerHolderSerial = "DelegateSerializationHolder objects are designed to represent a delegate during serialization and are not serializable themselves.";

	public const string DelegateSer_InsufficientMetadata = "The delegate cannot be serialized properly due to missing metadata for the target method.";

	public const string Argument_NoUninitializedStrings = "Uninitialized Strings cannot be created.";

	public const string ArgumentOutOfRangeException_NoGCRegionSizeTooLarge = "totalSize is too large. For more information about setting the maximum size, see \\\"Latency Modes\\\" in http://go.microsoft.com/fwlink/?LinkId=522706.";

	public const string InvalidOperationException_AlreadyInNoGCRegion = "The NoGCRegion mode was already in progress.";

	public const string InvalidOperationException_NoGCRegionAllocationExceeded = "Allocated memory exceeds specified memory for NoGCRegion mode.";

	public const string InvalidOperationException_NoGCRegionInduced = "Garbage collection was induced in NoGCRegion mode.";

	public const string InvalidOperationException_NoGCRegionNotInProgress = "NoGCRegion mode must be set.";

	public const string InvalidOperationException_SetLatencyModeNoGC = "The NoGCRegion mode is in progress. End it and then set a different mode.";

	public const string InvalidOperation_NotWithConcurrentGC = "This API is not available when the concurrent GC is enabled.";

	public const string ThreadState_AlreadyStarted = "Thread is running or terminated; it cannot restart.";

	public const string ThreadState_Dead_Priority = "Thread is dead; priority cannot be accessed.";

	public const string ThreadState_Dead_State = "Thread is dead; state cannot be accessed.";

	public const string ThreadState_NotStarted = "Thread has not been started.";

	public const string ThreadState_SetPriorityFailed = "Unable to set thread priority.";

	public const string Serialization_InvalidFieldState = "Object fields may not be properly initialized.";

	public const string Acc_CreateAbst = "Cannot create an abstract class.";

	public const string Acc_CreateGeneric = "Cannot create a type for which Type.ContainsGenericParameters is true.";

	public const string Argument_InvalidValue = "Value was invalid.";

	public const string NotSupported_ManagedActivation = "Cannot create uninitialized instances of types requiring managed activation.";

	public const string PlatformNotSupported_ResourceManager_ResWFileUnsupportedMethod = "ResourceManager method '{0}' is not supported when reading from .resw resource files.";

	public const string PlatformNotSupported_ResourceManager_ResWFileUnsupportedProperty = "ResourceManager property '{0}' is not supported when reading from .resw resource files.";

	public const string Serialization_NonSerType = "Type '{0}' in Assembly '{1}' is not marked as serializable.";

	public const string InvalidCast_DBNull = "Object cannot be cast to DBNull.";

	public const string NotSupported_NYI = "This feature is not currently implemented.";

	public const string Delegate_GarbageCollected = "The corresponding delegate has been garbage collected. Please make sure the delegate is still referenced by managed code when you are using the marshalled native function pointer.";

	public const string Arg_AmbiguousMatchException = "Ambiguous match found.";

	public const string NotSupported_ChangeType = "ChangeType operation is not supported.";

	public const string Arg_EmptyArray = "Array may not be empty.";

	public const string MissingMember = "Member not found.";

	public const string MissingField = "Field not found.";

	public const string InvalidCast_FromDBNull = "Object cannot be cast from DBNull to other types.";

	public const string NotSupported_DBNullSerial = "Only one DBNull instance may exist, and calls to DBNull deserialization methods are not allowed.";

	public const string Serialization_StringBuilderCapacity = "The serialized Capacity property of StringBuilder must be positive, less than or equal to MaxCapacity and greater than or equal to the String length.";

	public const string Serialization_StringBuilderMaxCapacity = "The serialized MaxCapacity property of StringBuilder must be positive and greater than or equal to the String length.";

	public const string PlatformNotSupported_Remoting = "Remoting is not supported on this platform.";

	public const string PlatformNotSupported_StrongNameSigning = "Strong-name signing is not supported on this platform.";

	public const string Serialization_MissingDateTimeData = "Invalid serialized DateTime data. Unable to find 'ticks' or 'dateData'.";

	public const string Serialization_DateTimeTicksOutOfRange = "Invalid serialized DateTime data. Ticks must be between DateTime.MinValue.Ticks and DateTime.MaxValue.Ticks.";

	public const string Arg_InvalidANSIString = "The ANSI string passed in could not be converted from the default ANSI code page to Unicode.";

	public const string Arg_ExpectedNulTermination = "The value passed was not NUL terminated.";

	public const string PlatformNotSupported_ArgIterator = "ArgIterator is not supported on this platform.";

	public const string Arg_TypeUnloadedException = "Type had been unloaded.";

	public const string Overflow_Currency = "Value was either too large or too small for a Currency.";

	public const string PlatformNotSupported_SecureBinarySerialization = "Secure binary serialization is not supported on this platform.";

	public const string Serialization_InvalidPtrValue = "An IntPtr or UIntPtr with an eight byte value cannot be deserialized on a machine with a four byte word size.";

	public const string EventSource_ListenerNotFound = "Listener not found.";

	public const string EventSource_ToString = "EventSource({0}, {1})";

	public const string EventSource_ImplementGetMetadata = "Please implement the GetMetadata method in your derived class";

	public const string EventSource_NeedGuid = "The Guid of an EventSource must be non zero.";

	public const string EventSource_NeedName = "The name of an EventSource must not be null.";

	public const string EventSource_NeedDescriptors = "The descriptor of an EventSource must be non-null.";

	public const string EventSource_NeedManifest = "The manifest of an EventSource must be non-null.";

	public const string EventSource_EventSourceGuidInUse = "An instance of EventSource with Guid {0} already exists.";

	public const string EventSource_ListenerWriteFailure = "An error occurred when writing to a listener.";

	public const string EventSource_NoManifest = "A manifest could not be generated for this EventSource because it contains one or more ill-formed event methods.";

	public const string Argument_StreamNotWritable = "Stream was not writable.";

	public const string Arg_SurrogatesNotAllowedAsSingleChar = "Unicode surrogate characters must be written out as pairs together in the same call, not individually. Consider passing in a character array instead.";

	public const string CustomAttributeFormat_InvalidFieldFail = "'{0}' field specified was not found.";

	public const string CustomAttributeFormat_InvalidPropertyFail = "'{0}' property specified was not found.";

	public const string NotSupported_CannotCallEqualsOnSpan = "Equals() on Span and ReadOnlySpan is not supported. Use operator== instead.";

	public const string NotSupported_CannotCallGetHashCodeOnSpan = "GetHashCode() on Span and ReadOnlySpan is not supported.";

	public const string Argument_DestinationTooShort = "Destination is too short.";

	public const string Argument_InvalidTypeWithPointersNotSupported = "Cannot use type '{0}'. Only value types without pointers or references are supported.";

	public const string ArrayTypeMismatch_ConstrainedCopy = "Array.ConstrainedCopy will only work on array types that are provably compatible, without any form of boxing, unboxing, widening, or casting of each array element.  Change the array types (i.e., copy a Derived[] to a Base[]), or use a mitigation strategy in the CER for Array.Copy's less powerful reliability contract, such as cloning the array or throwing away the potentially corrupt destination array.";

	public const string Arg_DllNotFoundException = "Dll was not found.";

	public const string Arg_DllNotFoundExceptionParameterized = "Unable to load DLL '{0}': The specified module could not be found.";

	public const string WrongSizeArrayInNStruct = "Type could not be marshaled because the length of an embedded array instance does not match the declared length in the layout.";

	public const string Arg_InteropMarshalUnmappableChar = "Cannot marshal: Encountered unmappable character.";

	public const string Arg_MarshalDirectiveException = "Marshaling directives are invalid.";

	public const string BlockingCollection_Add_ConcurrentCompleteAdd = "CompleteAdding may not be used concurrently with additions to the collection.";

	public const string BlockingCollection_Add_Failed = "The underlying collection didn't accept the item.";

	public const string BlockingCollection_CantAddAnyWhenCompleted = "At least one of the specified collections is marked as complete with regards to additions.";

	public const string BlockingCollection_CantTakeAnyWhenAllDone = "All collections are marked as complete with regards to additions.";

	public const string BlockingCollection_CantTakeWhenDone = "The collection argument is empty and has been marked as complete with regards to additions.";

	public const string BlockingCollection_Completed = "The collection has been marked as complete with regards to additions.";

	public const string BlockingCollection_CopyTo_IncorrectType = "The array argument is of the incorrect type.";

	public const string BlockingCollection_CopyTo_MultiDim = "The array argument is multidimensional.";

	public const string BlockingCollection_CopyTo_NonNegative = "The index argument must be greater than or equal zero.";

	public const string Collection_CopyTo_TooManyElems = "The number of elements in the collection is greater than the available space from index to the end of the destination array.";

	public const string BlockingCollection_ctor_BoundedCapacityRange = "The boundedCapacity argument must be positive.";

	public const string BlockingCollection_ctor_CountMoreThanCapacity = "The collection argument contains more items than are allowed by the boundedCapacity.";

	public const string BlockingCollection_Disposed = "The collection has been disposed.";

	public const string BlockingCollection_Take_CollectionModified = "The underlying collection was modified from outside of the BlockingCollection<T>.";

	public const string BlockingCollection_TimeoutInvalid = "The specified timeout must represent a value between -1 and {0}, inclusive.";

	public const string BlockingCollection_ValidateCollectionsArray_DispElems = "The collections argument contains at least one disposed element.";

	public const string BlockingCollection_ValidateCollectionsArray_LargeSize = "The collections length is greater than the supported range for 32 bit machine.";

	public const string BlockingCollection_ValidateCollectionsArray_NullElems = "The collections argument contains at least one null element.";

	public const string BlockingCollection_ValidateCollectionsArray_ZeroSize = "The collections argument is a zero-length array.";

	public const string Common_OperationCanceled = "The operation was canceled.";

	public const string ConcurrentBag_Ctor_ArgumentNullException = "The collection argument is null.";

	public const string ConcurrentBag_CopyTo_ArgumentNullException = "The array argument is null.";

	public const string Collection_CopyTo_ArgumentOutOfRangeException = "The index argument must be greater than or equal zero.";

	public const string ConcurrentCollection_SyncRoot_NotSupported = "The SyncRoot property may not be used for the synchronization of concurrent collections.";

	public const string ConcurrentDictionary_ArrayIncorrectType = "The array is multidimensional, or the type parameter for the set cannot be cast automatically to the type of the destination array.";

	public const string ConcurrentDictionary_SourceContainsDuplicateKeys = "The source argument contains duplicate keys.";

	public const string ConcurrentDictionary_ConcurrencyLevelMustBePositive = "The concurrencyLevel argument must be positive.";

	public const string ConcurrentDictionary_CapacityMustNotBeNegative = "The capacity argument must be greater than or equal to zero.";

	public const string ConcurrentDictionary_IndexIsNegative = "The index argument is less than zero.";

	public const string ConcurrentDictionary_ArrayNotLargeEnough = "The index is equal to or greater than the length of the array, or the number of elements in the dictionary is greater than the available space from index to the end of the destination array.";

	public const string ConcurrentDictionary_KeyAlreadyExisted = "The key already existed in the dictionary.";

	public const string ConcurrentDictionary_ItemKeyIsNull = "TKey is a reference type and item.Key is null.";

	public const string ConcurrentDictionary_TypeOfKeyIncorrect = "The key was of an incorrect type for this dictionary.";

	public const string ConcurrentDictionary_TypeOfValueIncorrect = "The value was of an incorrect type for this dictionary.";

	public const string ConcurrentStack_PushPopRange_CountOutOfRange = "The count argument must be greater than or equal to zero.";

	public const string ConcurrentStack_PushPopRange_InvalidCount = "The sum of the startIndex and count arguments must be less than or equal to the collection's Count.";

	public const string ConcurrentStack_PushPopRange_StartOutOfRange = "The startIndex argument must be greater than or equal to zero.";

	public const string Partitioner_DynamicPartitionsNotSupported = "Dynamic partitions are not supported by this partitioner.";

	public const string PartitionerStatic_CanNotCallGetEnumeratorAfterSourceHasBeenDisposed = "Can not call GetEnumerator on partitions after the source enumerable is disposed";

	public const string PartitionerStatic_CurrentCalledBeforeMoveNext = "MoveNext must be called at least once before calling Current.";

	public const string ConcurrentBag_Enumerator_EnumerationNotStartedOrAlreadyFinished = "Enumeration has either not started or has already finished.";

	public const string ArrayTypeMustBeExactMatch = "The array type must be exactly {0}.";

	public const string CannotCallEqualsOnSpan = "Equals() on Span and ReadOnlySpan is not supported. Use operator== instead.";

	public const string CannotCallGetHashCodeOnSpan = "GetHashCode() on Span and ReadOnlySpan is not supported.";

	public const string Argument_EmptyValue = "Value cannot be empty.";

	public const string PlatformNotSupported_RuntimeInformation = "RuntimeInformation is not supported for Portable Class Libraries.";

	public const string MemoryDisposed = "Memory<T> has been disposed.";

	public const string OutstandingReferences = "Release all references before disposing this instance.";

	internal static string GetString(string name, params object[] args)
	{
		return GetString(CultureInfo.InvariantCulture, name, args);
	}

	internal static string GetString(CultureInfo culture, string name, params object[] args)
	{
		return string.Format(culture, name, args);
	}

	internal static string GetString(string name)
	{
		return name;
	}

	internal static string GetString(CultureInfo culture, string name)
	{
		return name;
	}

	internal static string Format(string resourceFormat, params object[] args)
	{
		if (args != null)
		{
			return string.Format(CultureInfo.InvariantCulture, resourceFormat, args);
		}
		return resourceFormat;
	}

	internal static string Format(string resourceFormat, object p1)
	{
		return string.Format(CultureInfo.InvariantCulture, resourceFormat, p1);
	}

	internal static string Format(string resourceFormat, object p1, object p2)
	{
		return string.Format(CultureInfo.InvariantCulture, resourceFormat, p1, p2);
	}

	internal static string Format(string resourceFormat, object p1, object p2, object p3)
	{
		return string.Format(CultureInfo.InvariantCulture, resourceFormat, p1, p2, p3);
	}
}
