using Verse;
using BillDoorsPredefinedCharacter;

namespace BDsArknightLib
{
    public class ModExtension_Projectile : DefModExtension
    {
        public EffecterDef launchEffecter;
        public float muzzleOffset = 0.7f;
        public float muzzleScale = 1;

        public EffecterDef hitEffecter;
        public float hitEffecterScale = 1;

        public PredefinedCharacterParmDef characterDef;


        public FleckDef beamLineFleckDef;

        public EffecterDef beamEndEffecterDef;

        public EffecterDef beamOriginEffecterDef;

        public float beamOriginOffset;
    }
}
