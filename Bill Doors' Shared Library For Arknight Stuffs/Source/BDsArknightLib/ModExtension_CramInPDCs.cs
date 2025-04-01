using BillDoorsPredefinedCharacter;
using RimWorld;
using System.Collections.Generic;
using System.Xml;
using Verse;

namespace BDsArknightLib
{
    public class ModExtension_CramInPDCs : DefModExtension
    {
        public static ModExtension_CramInPDCs extCache;

        public List<PDCChance> characters = new List<PDCChance>();

        public IEnumerable<Pawn> GetCharacters(Faction faction)
        {
            foreach (var character in characters)
            {
                if (Rand.Chance(character.chance) && PDCharacterUtil.AtHome(character.def))
                {
                    yield return PredefinedCharacterMaker.GetPawn(character.def, faction: faction);
                }
            }
        }


        public class PDCChance
        {
            public PredefinedCharacterParmDef def;

            public float chance = 1;

            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", xmlRoot.Name);
                if (xmlRoot.HasChildNodes)
                {
                    chance = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);

                }
            }
        }
    }
}
