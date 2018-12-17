using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class Site : MapParent
	{
		public string customLabel;

		public SiteCore core;

		public List<SitePart> parts = new List<SitePart>();

		public bool sitePartsKnown;

		public bool factionMustRemainHostile;

		public float desiredThreatPoints;

		private bool startedCountdown;

		private bool anyEnemiesInitially;

		private Material cachedMat;

		private static List<string> tmpSitePartsLabels = new List<string>();

		public override string Label
		{
			get
			{
				if (!customLabel.NullOrEmpty())
				{
					return customLabel;
				}
				if (MainSiteDef == SiteCoreDefOf.PreciousLump && core.parms.preciousLumpResources != null)
				{
					return "PreciousLumpLabel".Translate(core.parms.preciousLumpResources.label);
				}
				return MainSiteDef.label;
			}
		}

		public override Texture2D ExpandingIcon => MainSiteDef.ExpandingIconTexture;

		public override Material Material
		{
			get
			{
				if (cachedMat == null)
				{
					cachedMat = MaterialPool.MatFrom(color: (!MainSiteDef.applyFactionColorToSiteTexture || base.Faction == null) ? Color.white : base.Faction.Color, texPath: MainSiteDef.siteTexture, shader: ShaderDatabase.WorldOverlayTransparentLit, renderQueue: WorldMaterials.WorldObjectRenderQueue);
				}
				return cachedMat;
			}
		}

		public override bool AppendFactionToInspectString => MainSiteDef.applyFactionColorToSiteTexture || MainSiteDef.showFactionInInspectString;

		private SiteCoreOrPartBase MainSiteCoreOrPart
		{
			get
			{
				if (core.def == SiteCoreDefOf.Nothing && parts.Any())
				{
					return parts[0];
				}
				return core;
			}
		}

		private SiteCoreOrPartDefBase MainSiteDef => MainSiteCoreOrPart.Def;

		public override IEnumerable<GenStepWithParams> ExtraGenStepDefs
		{
			get
			{
				using (IEnumerator<GenStepWithParams> enumerator = base.ExtraGenStepDefs.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						GenStepWithParams g = enumerator.Current;
						yield return g;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				GenStepParams coreGenStepParms = new GenStepParams
				{
					siteCoreOrPart = core
				};
				List<GenStepDef> coreGenStepDefs = core.def.ExtraGenSteps;
				int k = 0;
				if (k < coreGenStepDefs.Count)
				{
					yield return new GenStepWithParams(coreGenStepDefs[k], coreGenStepParms);
					/*Error: Unable to find new state assignment for yield return*/;
				}
				int j = 0;
				GenStepParams partGenStepParams;
				List<GenStepDef> partGenStepDefs;
				int i;
				while (true)
				{
					if (j >= parts.Count)
					{
						yield break;
					}
					partGenStepParams = new GenStepParams
					{
						siteCoreOrPart = parts[j]
					};
					partGenStepDefs = parts[j].def.ExtraGenSteps;
					i = 0;
					if (i < partGenStepDefs.Count)
					{
						break;
					}
					j++;
				}
				yield return new GenStepWithParams(partGenStepDefs[i], partGenStepParams);
				/*Error: Unable to find new state assignment for yield return*/;
				IL_0258:
				/*Error near IL_0259: Unexpected return in MoveNext()*/;
			}
		}

		public string ApproachOrderString => (!MainSiteDef.approachOrderString.NullOrEmpty()) ? string.Format(MainSiteDef.approachOrderString, Label) : "ApproachSite".Translate(Label);

		public string ApproachingReportString => (!MainSiteDef.approachingReportString.NullOrEmpty()) ? string.Format(MainSiteDef.approachingReportString, Label) : "ApproachingSite".Translate(Label);

		public float ActualThreatPoints
		{
			get
			{
				float num = core.parms.threatPoints;
				for (int i = 0; i < parts.Count; i++)
				{
					num += parts[i].parms.threatPoints;
				}
				return num;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref customLabel, "customLabel");
			Scribe_Deep.Look(ref core, "core");
			Scribe_Collections.Look(ref parts, "parts", LookMode.Deep);
			Scribe_Values.Look(ref startedCountdown, "startedCountdown", defaultValue: false);
			Scribe_Values.Look(ref anyEnemiesInitially, "anyEnemiesInitially", defaultValue: false);
			Scribe_Values.Look(ref sitePartsKnown, "sitePartsKnown", defaultValue: false);
			Scribe_Values.Look(ref factionMustRemainHostile, "factionMustRemainHostile", defaultValue: false);
			Scribe_Values.Look(ref desiredThreatPoints, "desiredThreatPoints", 0f);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.SitePostLoadInit(this);
			}
		}

		public override void Tick()
		{
			base.Tick();
			core.def.Worker.SiteCoreWorkerTick(this);
			for (int i = 0; i < parts.Count; i++)
			{
				parts[i].def.Worker.SitePartWorkerTick(this);
			}
			if (base.HasMap)
			{
				CheckStartForceExitAndRemoveMapCountdown();
			}
		}

		public override void PostMapGenerate()
		{
			base.PostMapGenerate();
			Map map = base.Map;
			core.def.Worker.PostMapGenerate(map);
			for (int i = 0; i < parts.Count; i++)
			{
				parts[i].def.Worker.PostMapGenerate(map);
			}
			anyEnemiesInitially = GenHostility.AnyHostileActiveThreatToPlayer(base.Map);
		}

		public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
		{
			alsoRemoveWorldObject = true;
			return !base.Map.mapPawns.AnyPawnBlockingMapRemoval;
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			using (IEnumerator<FloatMenuOption> enumerator = base.GetFloatMenuOptions(caravan).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					FloatMenuOption f2 = enumerator.Current;
					yield return f2;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			using (IEnumerator<FloatMenuOption> enumerator2 = core.def.Worker.GetFloatMenuOptions(caravan, this).GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					FloatMenuOption f = enumerator2.Current;
					yield return f;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_016b:
			/*Error near IL_016c: Unexpected return in MoveNext()*/;
		}

		public override IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptions(IEnumerable<IThingHolder> pods, CompLaunchable representative)
		{
			using (IEnumerator<FloatMenuOption> enumerator = base.GetTransportPodsFloatMenuOptions(pods, representative).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					FloatMenuOption o2 = enumerator.Current;
					yield return o2;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			using (IEnumerator<FloatMenuOption> enumerator2 = core.def.Worker.GetTransportPodsFloatMenuOptions(pods, representative, this).GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					FloatMenuOption o = enumerator2.Current;
					yield return o;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_0177:
			/*Error near IL_0178: Unexpected return in MoveNext()*/;
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
			if (base.HasMap && Find.WorldSelector.SingleSelectedObject == this)
			{
				yield return (Gizmo)SettleInExistingMapUtility.SettleCommand(base.Map, requiresNoEnemies: true);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_010d:
			/*Error near IL_010e: Unexpected return in MoveNext()*/;
		}

		private void CheckStartForceExitAndRemoveMapCountdown()
		{
			if (!startedCountdown && !GenHostility.AnyHostileActiveThreatToPlayer(base.Map))
			{
				startedCountdown = true;
				int num = Mathf.RoundToInt(core.def.forceExitAndRemoveMapCountdownDurationDays * 60000f);
				string text = (!anyEnemiesInitially) ? "MessageSiteCountdownBecauseNoEnemiesInitially".Translate(TimedForcedExit.GetForceExitAndRemoveMapCountdownTimeLeftString(num)) : "MessageSiteCountdownBecauseNoMoreEnemies".Translate(TimedForcedExit.GetForceExitAndRemoveMapCountdownTimeLeftString(num));
				Messages.Message(text, this, MessageTypeDefOf.PositiveEvent);
				GetComponent<TimedForcedExit>().StartForceExitAndRemoveMapCountdown(num);
				TaleRecorder.RecordTale(TaleDefOf.CaravanAssaultSuccessful, base.Map.mapPawns.FreeColonists.RandomElement());
			}
		}

		public override bool AllMatchingObjectsOnScreenMatchesWith(WorldObject other)
		{
			Site site = other as Site;
			return site != null && site.MainSiteDef == MainSiteDef;
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			if (sitePartsKnown)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.AppendLine();
				}
				tmpSitePartsLabels.Clear();
				for (int i = 0; i < parts.Count; i++)
				{
					if (!parts[i].def.alwaysHidden)
					{
						tmpSitePartsLabels.Add(parts[i].def.Worker.GetPostProcessedThreatLabel(this, parts[i]));
					}
				}
				if (tmpSitePartsLabels.Count == 0)
				{
					stringBuilder.Append("KnownSiteThreatsNone".Translate());
				}
				else if (tmpSitePartsLabels.Count == 1)
				{
					stringBuilder.Append("KnownSiteThreat".Translate(tmpSitePartsLabels[0].CapitalizeFirst()));
				}
				else
				{
					stringBuilder.Append("KnownSiteThreats".Translate(tmpSitePartsLabels.ToCommaList(useAnd: true).CapitalizeFirst()));
				}
			}
			return stringBuilder.ToString();
		}

		public override string GetDescription()
		{
			string text = MainSiteDef.description;
			string description = base.GetDescription();
			if (!description.NullOrEmpty())
			{
				if (!text.NullOrEmpty())
				{
					text += "\n\n";
				}
				text += description;
			}
			return text;
		}
	}
}
