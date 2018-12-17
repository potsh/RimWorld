using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class MapParent : WorldObject, IThingHolder
	{
		private HashSet<IncidentTargetTagDef> hibernatableIncidentTargets;

		private static readonly Texture2D ShowMapCommand = ContentFinder<Texture2D>.Get("UI/Commands/ShowMap");

		public bool HasMap => Map != null;

		protected virtual bool UseGenericEnterMapFloatMenuOption => true;

		public Map Map => Current.Game.FindMap(this);

		public virtual MapGeneratorDef MapGeneratorDef => (def.mapGenerator == null) ? MapGeneratorDefOf.Encounter : def.mapGenerator;

		public virtual IEnumerable<GenStepWithParams> ExtraGenStepDefs
		{
			get
			{
				yield break;
			}
		}

		public override bool ExpandMore => base.ExpandMore || HasMap;

		public virtual void PostMapGenerate()
		{
			List<WorldObjectComp> allComps = base.AllComps;
			for (int i = 0; i < allComps.Count; i++)
			{
				allComps[i].PostMapGenerate();
			}
		}

		public virtual void Notify_MyMapRemoved(Map map)
		{
			List<WorldObjectComp> allComps = base.AllComps;
			for (int i = 0; i < allComps.Count; i++)
			{
				allComps[i].PostMyMapRemoved();
			}
		}

		public virtual void Notify_CaravanFormed(Caravan caravan)
		{
			List<WorldObjectComp> allComps = base.AllComps;
			for (int i = 0; i < allComps.Count; i++)
			{
				allComps[i].PostCaravanFormed(caravan);
			}
		}

		public virtual void Notify_HibernatableChanged()
		{
			RecalculateHibernatableIncidentTargets();
		}

		public virtual void FinalizeLoading()
		{
			RecalculateHibernatableIncidentTargets();
		}

		public virtual bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
		{
			alsoRemoveWorldObject = false;
			return false;
		}

		public override void PostRemove()
		{
			base.PostRemove();
			if (HasMap)
			{
				Current.Game.DeinitAndRemoveMap(Map);
			}
		}

		public override void Tick()
		{
			base.Tick();
			CheckRemoveMapNow();
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			using (IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Gizmo g = enumerator.Current;
					yield return g;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (HasMap)
			{
				yield return (Gizmo)new Command_Action
				{
					defaultLabel = "CommandShowMap".Translate(),
					defaultDesc = "CommandShowMapDesc".Translate(),
					icon = ShowMapCommand,
					hotKey = KeyBindingDefOf.Misc1,
					action = delegate
					{
						Current.Game.CurrentMap = ((_003CGetGizmos_003Ec__Iterator1)/*Error near IL_011f: stateMachine*/)._0024this.Map;
						if (!CameraJumper.TryHideWorld())
						{
							SoundDefOf.TabClose.PlayOneShotOnCamera();
						}
					}
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_0159:
			/*Error near IL_015a: Unexpected return in MoveNext()*/;
		}

		public override IEnumerable<IncidentTargetTagDef> IncidentTargetTags()
		{
			using (IEnumerator<IncidentTargetTagDef> enumerator = base.IncidentTargetTags().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					IncidentTargetTagDef type2 = enumerator.Current;
					yield return type2;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (hibernatableIncidentTargets != null && hibernatableIncidentTargets.Count > 0)
			{
				using (HashSet<IncidentTargetTagDef>.Enumerator enumerator2 = hibernatableIncidentTargets.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						IncidentTargetTagDef type = enumerator2.Current;
						yield return type;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_016b:
			/*Error near IL_016c: Unexpected return in MoveNext()*/;
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			using (IEnumerator<FloatMenuOption> enumerator = base.GetFloatMenuOptions(caravan).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					FloatMenuOption o = enumerator.Current;
					yield return o;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (UseGenericEnterMapFloatMenuOption)
			{
				using (IEnumerator<FloatMenuOption> enumerator2 = CaravanArrivalAction_Enter.GetFloatMenuOptions(caravan, this).GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						FloatMenuOption f = enumerator2.Current;
						yield return f;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_0166:
			/*Error near IL_0167: Unexpected return in MoveNext()*/;
		}

		public override IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptions(IEnumerable<IThingHolder> pods, CompLaunchable representative)
		{
			_003CGetTransportPodsFloatMenuOptions_003Ec__Iterator4 _003CGetTransportPodsFloatMenuOptions_003Ec__Iterator = (_003CGetTransportPodsFloatMenuOptions_003Ec__Iterator4)/*Error near IL_0038: stateMachine*/;
			using (IEnumerator<FloatMenuOption> enumerator = base.GetTransportPodsFloatMenuOptions(pods, representative).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					FloatMenuOption o = enumerator.Current;
					yield return o;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (TransportPodsArrivalAction_LandInSpecificCell.CanLandInSpecificCell(pods, this))
			{
				yield return new FloatMenuOption("LandInExistingMap".Translate(Label), delegate
				{
					_003CGetTransportPodsFloatMenuOptions_003Ec__Iterator4 _003CGetTransportPodsFloatMenuOptions_003Ec__Iterator2 = _003CGetTransportPodsFloatMenuOptions_003Ec__Iterator;
					Map myMap = representative.parent.Map;
					Map map = _003CGetTransportPodsFloatMenuOptions_003Ec__Iterator._0024this.Map;
					Current.Game.CurrentMap = map;
					CameraJumper.TryHideWorld();
					Find.Targeter.BeginTargeting(TargetingParameters.ForDropPodsDestination(), delegate(LocalTargetInfo x)
					{
						representative.TryLaunch(_003CGetTransportPodsFloatMenuOptions_003Ec__Iterator2._0024this.Tile, new TransportPodsArrivalAction_LandInSpecificCell(_003CGetTransportPodsFloatMenuOptions_003Ec__Iterator2._0024this, x.Cell));
					}, null, delegate
					{
						if (Find.Maps.Contains(myMap))
						{
							Current.Game.CurrentMap = myMap;
						}
					}, CompLaunchable.TargeterMouseAttachment);
				});
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_0160:
			/*Error near IL_0161: Unexpected return in MoveNext()*/;
		}

		public void CheckRemoveMapNow()
		{
			if (HasMap && ShouldRemoveMapNow(out bool alsoRemoveWorldObject))
			{
				Map map = Map;
				Current.Game.DeinitAndRemoveMap(map);
				if (alsoRemoveWorldObject)
				{
					Find.WorldObjects.Remove(this);
				}
			}
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString();
			if (this.EnterCooldownBlocksEntering())
			{
				if (!text.NullOrEmpty())
				{
					text += "\n";
				}
				text += "EnterCooldown".Translate(this.EnterCooldownDaysLeft().ToString("0.#"));
			}
			return text;
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return null;
		}

		public virtual void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
			if (HasMap)
			{
				outChildren.Add(Map);
			}
		}

		private void RecalculateHibernatableIncidentTargets()
		{
			hibernatableIncidentTargets = null;
			foreach (ThingWithComps item in Map.listerThings.ThingsOfDef(ThingDefOf.Ship_Reactor).OfType<ThingWithComps>())
			{
				CompHibernatable compHibernatable = item.TryGetComp<CompHibernatable>();
				if (compHibernatable != null && compHibernatable.State == HibernatableStateDefOf.Starting && compHibernatable.Props.incidentTargetWhileStarting != null)
				{
					if (hibernatableIncidentTargets == null)
					{
						hibernatableIncidentTargets = new HashSet<IncidentTargetTagDef>();
					}
					hibernatableIncidentTargets.Add(compHibernatable.Props.incidentTargetWhileStarting);
				}
			}
		}
	}
}
