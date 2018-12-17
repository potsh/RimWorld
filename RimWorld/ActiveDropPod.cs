using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class ActiveDropPod : Thing, IActiveDropPod, IThingHolder
	{
		public int age;

		private ActiveDropPodInfo contents;

		public ActiveDropPodInfo Contents
		{
			get
			{
				return contents;
			}
			set
			{
				if (contents != null)
				{
					contents.parent = null;
				}
				if (value != null)
				{
					value.parent = this;
				}
				contents = value;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref age, "age", 0);
			Scribe_Deep.Look(ref contents, "contents", this);
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return null;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
			if (contents != null)
			{
				outChildren.Add(contents);
			}
		}

		public override void Tick()
		{
			if (contents != null)
			{
				contents.innerContainer.ThingOwnerTick();
				if (base.Spawned)
				{
					age++;
					if (age > contents.openDelay)
					{
						PodOpen();
					}
				}
			}
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			if (contents != null)
			{
				contents.innerContainer.ClearAndDestroyContents();
			}
			Map map = base.Map;
			base.Destroy(mode);
			if (mode == DestroyMode.KillFinalize)
			{
				for (int i = 0; i < 1; i++)
				{
					Thing thing = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel);
					GenPlace.TryPlaceThing(thing, base.Position, map, ThingPlaceMode.Near);
				}
			}
		}

		private void PodOpen()
		{
			for (int num = contents.innerContainer.Count - 1; num >= 0; num--)
			{
				Thing thing = contents.innerContainer[num];
				Thing lastResultingThing = default(Thing);
				GenPlace.TryPlaceThing(thing, base.Position, base.Map, ThingPlaceMode.Near, out lastResultingThing, delegate(Thing placedThing, int count)
				{
					if (Find.TickManager.TicksGame < 1200 && TutorSystem.TutorialMode && placedThing.def.category == ThingCategory.Item)
					{
						Find.TutorialState.AddStartingItem(placedThing);
					}
				});
				Pawn pawn = lastResultingThing as Pawn;
				if (pawn != null)
				{
					if (pawn.RaceProps.Humanlike)
					{
						TaleRecorder.RecordTale(TaleDefOf.LandedInPod, pawn);
					}
					if (pawn.IsColonist && pawn.Spawned && !base.Map.IsPlayerHome)
					{
						pawn.drafter.Drafted = true;
					}
				}
			}
			contents.innerContainer.ClearAndDestroyContents();
			if (contents.leaveSlag)
			{
				for (int i = 0; i < 1; i++)
				{
					Thing thing2 = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel);
					GenPlace.TryPlaceThing(thing2, base.Position, base.Map, ThingPlaceMode.Near);
				}
			}
			SoundDefOf.DropPod_Open.PlayOneShot(new TargetInfo(base.Position, base.Map));
			Destroy();
		}
	}
}
