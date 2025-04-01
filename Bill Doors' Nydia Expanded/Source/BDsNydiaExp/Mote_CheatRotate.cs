using Verse;

namespace BDsNydiaExp
{
    public class Mote_CheatRotate : MoteAttached
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            rotationRate = 10;
        }

        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);
            exactRotation += rotationRate * deltaTime;
        }
    }
}
