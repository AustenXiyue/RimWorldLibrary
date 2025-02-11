namespace WinRT;

internal class ErrorStrings
{
	internal static readonly string Arg_IndexOutOfRangeException = "Index was outside the bounds of the array.";

	internal static readonly string Arg_KeyNotFound = "The given key was not present in the dictionary.";

	internal static readonly string Arg_KeyNotFoundWithKey = "The given key '{0}' was not present in the dictionary.";

	internal static readonly string Arg_RankMultiDimNotSupported = "Only single dimensional arrays are supported for the requested action.";

	internal static readonly string Argument_AddingDuplicate = "An item with the same key has already been added.";

	internal static readonly string Argument_AddingDuplicateWithKey = "An item with the same key has already been added. Key: {0}";

	internal static readonly string Argument_IndexOutOfArrayBounds = "The specified index is out of bounds of the specified array.";

	internal static readonly string Argument_InsufficientSpaceToCopyCollection = "The specified space is not sufficient to copy the elements from this Collection.";

	internal static readonly string ArgumentOutOfRange_Index = "Index was out of range. Must be non-negative and less than the size of the collection.";

	internal static readonly string ArgumentOutOfRange_IndexLargerThanMaxValue = "This collection cannot work with indices larger than Int32.MaxValue - 1 (0x7FFFFFFF - 1).";

	internal static readonly string InvalidOperation_CannotRemoveLastFromEmptyCollection = "Cannot remove the last element from an empty collection.";

	internal static readonly string InvalidOperation_CollectionBackingDictionaryTooLarge = "The collection backing this Dictionary contains too many elements.";

	internal static readonly string InvalidOperation_CollectionBackingListTooLarge = "The collection backing this List contains too many elements.";

	internal static readonly string InvalidOperation_EnumEnded = "Enumeration already finished.";

	internal static readonly string InvalidOperation_EnumFailedVersion = "Collection was modified; enumeration operation may not execute.";

	internal static readonly string InvalidOperation_EnumNotStarted = "Enumeration has not started. Call MoveNext.";

	internal static readonly string NotSupported_KeyCollectionSet = "Mutating a key collection derived from a dictionary is not allowed.";

	internal static readonly string NotSupported_ValueCollectionSet = "Mutating a value collection derived from a dictionary is not allowed.";

	internal static string Format(string format, params object[] args)
	{
		return string.Format(format, args);
	}
}
