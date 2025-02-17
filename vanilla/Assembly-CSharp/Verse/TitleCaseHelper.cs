using System.Collections.Generic;

namespace Verse;

public static class TitleCaseHelper
{
	private static HashSet<string> NonUppercaseWords = new HashSet<string>
	{
		"a", "aboard", "about", "above", "absent", "across", "after", "against", "along", "alongside",
		"amid", "amidst", "among", "amongst", "an", "and", "around", "as", "as", "aslant",
		"astride", "at", "athwart", "atop", "barring", "before", "behind", "below", "beneath", "beside",
		"besides", "between", "beyond", "but", "by", "despite", "down", "during", "except", "failing",
		"following", "for", "from", "in", "inside", "into", "like", "mid", "minus", "near",
		"next", "nor", "of", "off", "on", "onto", "opposite", "or", "out", "outside",
		"over", "past", "per", "plus", "regarding", "round", "save", "since", "so", "than",
		"the", "through", "throughout", "till", "times", "to", "toward", "towards", "under", "underneath",
		"unlike", "until", "up", "upon", "via", "vs.", "vs", "when", "with", "within",
		"without", "worth", "yet"
	};

	public static bool IsUppercaseTitleWord(string word)
	{
		if (word.Length <= 1)
		{
			return false;
		}
		return !NonUppercaseWords.Contains(word);
	}
}
