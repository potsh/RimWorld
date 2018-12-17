using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public abstract class SiteCoreOrPartWorkerBase
	{
		public SiteCoreOrPartDefBase def;

		public virtual void PostMapGenerate(Map map)
		{
		}

		public virtual bool FactionCanOwn(Faction faction)
		{
			return true;
		}

		public virtual string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
		{
			preferredLetterDef = def.arrivedLetterDef;
			lookTargets = null;
			return def.arrivedLetter;
		}

		public virtual string GetPostProcessedDescriptionDialogue(Site site, SiteCoreOrPartBase siteCoreOrPart)
		{
			return def.descriptionDialogue;
		}

		public virtual string GetPostProcessedThreatLabel(Site site, SiteCoreOrPartBase siteCoreOrPart)
		{
			return def.label;
		}

		public virtual SiteCoreOrPartParams GenerateDefaultParams(Site site, float myThreatPoints)
		{
			SiteCoreOrPartParams siteCoreOrPartParams = new SiteCoreOrPartParams();
			siteCoreOrPartParams.randomValue = Rand.Int;
			siteCoreOrPartParams.threatPoints = ((!def.wantsThreatPoints) ? 0f : myThreatPoints);
			return siteCoreOrPartParams;
		}
	}
}
