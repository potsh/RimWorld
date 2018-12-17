using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[HasDebugOutput]
	public static class PawnApparelGenerator
	{
		private class PossibleApparelSet
		{
			private List<ThingStuffPair> aps = new List<ThingStuffPair>();

			private HashSet<ApparelUtility.LayerGroupPair> lgps = new HashSet<ApparelUtility.LayerGroupPair>();

			private BodyDef body;

			private ThingDef raceDef;

			private const float StartingMinTemperature = 12f;

			private const float TargetMinTemperature = -40f;

			private const float StartingMaxTemperature = 32f;

			private const float TargetMaxTemperature = 30f;

			public int Count => aps.Count;

			public float TotalPrice => aps.Sum((ThingStuffPair pa) => pa.Price);

			public float TotalInsulationCold => aps.Sum((ThingStuffPair a) => a.InsulationCold);

			public void Reset(BodyDef body, ThingDef raceDef)
			{
				aps.Clear();
				lgps.Clear();
				this.body = body;
				this.raceDef = raceDef;
			}

			public void Add(ThingStuffPair pair)
			{
				aps.Add(pair);
				ApparelUtility.GenerateLayerGroupPairs(body, pair.thing, delegate(ApparelUtility.LayerGroupPair lgp)
				{
					lgps.Add(lgp);
				});
			}

			public bool PairOverlapsAnything(ThingStuffPair pair)
			{
				bool conflicts = false;
				ApparelUtility.GenerateLayerGroupPairs(body, pair.thing, delegate(ApparelUtility.LayerGroupPair lgp)
				{
					conflicts |= lgps.Contains(lgp);
				});
				return conflicts;
			}

			public bool CoatButNoShirt()
			{
				bool flag = false;
				bool flag2 = false;
				for (int i = 0; i < aps.Count; i++)
				{
					ThingStuffPair thingStuffPair = aps[i];
					if (thingStuffPair.thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
					{
						int num = 0;
						while (true)
						{
							int num2 = num;
							ThingStuffPair thingStuffPair2 = aps[i];
							if (num2 >= thingStuffPair2.thing.apparel.layers.Count)
							{
								break;
							}
							ThingStuffPair thingStuffPair3 = aps[i];
							ApparelLayerDef apparelLayerDef = thingStuffPair3.thing.apparel.layers[num];
							if (apparelLayerDef == ApparelLayerDefOf.OnSkin)
							{
								flag2 = true;
							}
							if (apparelLayerDef == ApparelLayerDefOf.Shell || apparelLayerDef == ApparelLayerDefOf.Middle)
							{
								flag = true;
							}
							num++;
						}
					}
				}
				return flag && !flag2;
			}

			public bool Covers(BodyPartGroupDef bp)
			{
				for (int i = 0; i < aps.Count; i++)
				{
					ThingStuffPair thingStuffPair = aps[i];
					if (thingStuffPair.thing.apparel.bodyPartGroups.Contains(bp))
					{
						return true;
					}
				}
				return false;
			}

			public bool IsNaked(Gender gender)
			{
				switch (gender)
				{
				case Gender.Male:
					return !Covers(BodyPartGroupDefOf.Legs);
				case Gender.Female:
					return !Covers(BodyPartGroupDefOf.Legs) || !Covers(BodyPartGroupDefOf.Torso);
				case Gender.None:
					return false;
				default:
					return false;
				}
			}

			public bool SatisfiesNeededWarmth(NeededWarmth warmth, bool mustBeSafe = false, float mapTemperature = 21f)
			{
				if (warmth == NeededWarmth.Any)
				{
					return true;
				}
				if (!mustBeSafe || GenTemperature.SafeTemperatureRange(raceDef, aps).Includes(mapTemperature))
				{
					switch (warmth)
					{
					case NeededWarmth.Cool:
					{
						float num2 = aps.Sum((ThingStuffPair a) => a.InsulationHeat);
						return num2 >= -2f;
					}
					case NeededWarmth.Warm:
					{
						float num = aps.Sum((ThingStuffPair a) => a.InsulationCold);
						return num >= 52f;
					}
					default:
						throw new NotImplementedException();
					}
				}
				return false;
			}

			public void AddFreeWarmthAsNeeded(NeededWarmth warmth, float mapTemperature)
			{
				if (warmth != 0 && warmth != NeededWarmth.Cool)
				{
					if (DebugViewSettings.logApparelGeneration)
					{
						debugSb.AppendLine();
						debugSb.AppendLine("Trying to give free warm layer.");
					}
					for (int i = 0; i < 3; i++)
					{
						if (!SatisfiesNeededWarmth(warmth, mustBeSafe: true, mapTemperature))
						{
							if (DebugViewSettings.logApparelGeneration)
							{
								debugSb.AppendLine("Checking to give free torso-cover at max price " + freeWarmParkaMaxPrice);
							}
							Predicate<ThingStuffPair> parkaPairValidator = delegate(ThingStuffPair pa)
							{
								if (pa.Price > freeWarmParkaMaxPrice)
								{
									return false;
								}
								if (pa.InsulationCold <= 0f)
								{
									return false;
								}
								if (!pa.thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
								{
									return false;
								}
								float replacedInsulationCold2 = GetReplacedInsulationCold(pa);
								if (replacedInsulationCold2 >= pa.InsulationCold)
								{
									return false;
								}
								return true;
							};
							int num = 0;
							while (true)
							{
								if (num < 2)
								{
									ThingStuffPair candidate;
									if (num == 0)
									{
										if (!(from pa in allApparelPairs
										where parkaPairValidator(pa) && pa.InsulationCold < 40f
										select pa).TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality / (pa.Price * pa.Price), out candidate))
										{
											goto IL_023f;
										}
									}
									else if (!(from pa in allApparelPairs
									where parkaPairValidator(pa)
									select pa).TryMaxBy((ThingStuffPair x) => x.InsulationCold - GetReplacedInsulationCold(x), out candidate))
									{
										goto IL_023f;
									}
									if (DebugViewSettings.logApparelGeneration)
									{
										debugSb.AppendLine("Giving free torso-cover: " + candidate + " insulation=" + candidate.InsulationCold);
										foreach (ThingStuffPair item in from a in aps
										where !ApparelUtility.CanWearTogether(a.thing, candidate.thing, body)
										select a)
										{
											debugSb.AppendLine("    -replaces " + item.ToString() + " InsulationCold=" + item.InsulationCold);
										}
									}
									aps.RemoveAll((ThingStuffPair pa) => !ApparelUtility.CanWearTogether(pa.thing, candidate.thing, body));
									aps.Add(candidate);
								}
								break;
								IL_023f:
								num++;
							}
						}
						if (GenTemperature.SafeTemperatureRange(raceDef, aps).Includes(mapTemperature))
						{
							break;
						}
					}
					if (!SatisfiesNeededWarmth(warmth, mustBeSafe: true, mapTemperature))
					{
						if (DebugViewSettings.logApparelGeneration)
						{
							debugSb.AppendLine("Checking to give free hat at max price " + freeWarmHatMaxPrice);
						}
						Predicate<ThingStuffPair> hatPairValidator = delegate(ThingStuffPair pa)
						{
							if (pa.Price > freeWarmHatMaxPrice)
							{
								return false;
							}
							if (pa.InsulationCold < 7f)
							{
								return false;
							}
							if (!pa.thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead) && !pa.thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead))
							{
								return false;
							}
							float replacedInsulationCold = GetReplacedInsulationCold(pa);
							if (replacedInsulationCold >= pa.InsulationCold)
							{
								return false;
							}
							return true;
						};
						if ((from pa in allApparelPairs
						where hatPairValidator(pa)
						select pa).TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality / (pa.Price * pa.Price), out ThingStuffPair hatPair))
						{
							if (DebugViewSettings.logApparelGeneration)
							{
								debugSb.AppendLine("Giving free hat: " + hatPair + " insulation=" + hatPair.InsulationCold);
								foreach (ThingStuffPair item2 in from a in aps
								where !ApparelUtility.CanWearTogether(a.thing, hatPair.thing, body)
								select a)
								{
									debugSb.AppendLine("    -replaces " + item2.ToString() + " InsulationCold=" + item2.InsulationCold);
								}
							}
							aps.RemoveAll((ThingStuffPair pa) => !ApparelUtility.CanWearTogether(pa.thing, hatPair.thing, body));
							aps.Add(hatPair);
						}
					}
					if (DebugViewSettings.logApparelGeneration)
					{
						debugSb.AppendLine("New TotalInsulationCold: " + TotalInsulationCold);
					}
				}
			}

			public void GiveToPawn(Pawn pawn)
			{
				for (int i = 0; i < aps.Count; i++)
				{
					ThingStuffPair thingStuffPair = aps[i];
					ThingDef thing = thingStuffPair.thing;
					ThingStuffPair thingStuffPair2 = aps[i];
					Apparel apparel = (Apparel)ThingMaker.MakeThing(thing, thingStuffPair2.stuff);
					PawnGenerator.PostProcessGeneratedGear(apparel, pawn);
					if (ApparelUtility.HasPartsToWear(pawn, apparel.def))
					{
						pawn.apparel.Wear(apparel, dropReplacedApparel: false);
					}
				}
				for (int j = 0; j < aps.Count; j++)
				{
					for (int k = 0; k < aps.Count; k++)
					{
						if (j != k)
						{
							ThingStuffPair thingStuffPair3 = aps[j];
							ThingDef thing2 = thingStuffPair3.thing;
							ThingStuffPair thingStuffPair4 = aps[k];
							if (!ApparelUtility.CanWearTogether(thing2, thingStuffPair4.thing, pawn.RaceProps.body))
							{
								Log.Error(pawn + " generated with apparel that cannot be worn together: " + aps[j] + ", " + aps[k]);
								return;
							}
						}
					}
				}
			}

			private float GetReplacedInsulationCold(ThingStuffPair newAp)
			{
				float num = 0f;
				for (int i = 0; i < aps.Count; i++)
				{
					ThingStuffPair thingStuffPair = aps[i];
					if (!ApparelUtility.CanWearTogether(thingStuffPair.thing, newAp.thing, body))
					{
						num += aps[i].InsulationCold;
					}
				}
				return num;
			}

			public override string ToString()
			{
				string str = "[";
				for (int i = 0; i < aps.Count; i++)
				{
					str = str + aps[i].ToString() + ", ";
				}
				return str + "]";
			}
		}

		private static List<ThingStuffPair> allApparelPairs;

		private static float freeWarmParkaMaxPrice;

		private static float freeWarmHatMaxPrice;

		private static PossibleApparelSet workingSet;

		private static List<ThingStuffPair> usableApparel;

		private static StringBuilder debugSb;

		static PawnApparelGenerator()
		{
			allApparelPairs = new List<ThingStuffPair>();
			workingSet = new PossibleApparelSet();
			usableApparel = new List<ThingStuffPair>();
			debugSb = null;
			Reset();
		}

		public static void Reset()
		{
			allApparelPairs = ThingStuffPair.AllWith((ThingDef td) => td.IsApparel);
			freeWarmParkaMaxPrice = (float)(int)(StatDefOf.MarketValue.Worker.GetValueAbstract(ThingDefOf.Apparel_Parka, ThingDefOf.Cloth) * 1.3f);
			freeWarmHatMaxPrice = (float)(int)(StatDefOf.MarketValue.Worker.GetValueAbstract(ThingDefOf.Apparel_Tuque, ThingDefOf.Cloth) * 1.3f);
		}

		public static void GenerateStartingApparelFor(Pawn pawn, PawnGenerationRequest request)
		{
			if (pawn.RaceProps.ToolUser && pawn.RaceProps.IsFlesh)
			{
				pawn.apparel.DestroyAll();
				float randomInRange = pawn.kindDef.apparelMoney.RandomInRange;
				float mapTemperature;
				NeededWarmth neededWarmth = ApparelWarmthNeededNow(pawn, request, out mapTemperature);
				bool flag = Rand.Value < pawn.kindDef.apparelAllowHeadgearChance;
				debugSb = null;
				if (DebugViewSettings.logApparelGeneration)
				{
					debugSb = new StringBuilder();
					debugSb.AppendLine("Generating apparel for " + pawn);
					debugSb.AppendLine("Money: " + randomInRange.ToString("F0"));
					debugSb.AppendLine("Needed warmth: " + neededWarmth);
					debugSb.AppendLine("Headgear allowed: " + flag);
				}
				if (randomInRange < 0.001f)
				{
					GenerateWorkingPossibleApparelSetFor(pawn, randomInRange, flag);
				}
				else
				{
					int num = 0;
					while (true)
					{
						GenerateWorkingPossibleApparelSetFor(pawn, randomInRange, flag);
						if (DebugViewSettings.logApparelGeneration)
						{
							debugSb.Append(num.ToString().PadRight(5) + "Trying: " + workingSet.ToString());
						}
						if (num < 10 && Rand.Value < 0.85f)
						{
							float num2 = Rand.Range(0.45f, 0.8f);
							float totalPrice = workingSet.TotalPrice;
							if (totalPrice < randomInRange * num2)
							{
								if (DebugViewSettings.logApparelGeneration)
								{
									debugSb.AppendLine(" -- Failed: Spent $" + totalPrice.ToString("F0") + ", < " + (num2 * 100f).ToString("F0") + "% of money.");
								}
								goto IL_035e;
							}
						}
						if (num < 20 && Rand.Value < 0.97f && !workingSet.Covers(BodyPartGroupDefOf.Torso))
						{
							if (DebugViewSettings.logApparelGeneration)
							{
								debugSb.AppendLine(" -- Failed: Does not cover torso.");
							}
						}
						else if (num < 30 && Rand.Value < 0.8f && workingSet.CoatButNoShirt())
						{
							if (DebugViewSettings.logApparelGeneration)
							{
								debugSb.AppendLine(" -- Failed: Coat but no shirt.");
							}
						}
						else
						{
							if (num < 50)
							{
								bool mustBeSafe = num < 17;
								if (!workingSet.SatisfiesNeededWarmth(neededWarmth, mustBeSafe, mapTemperature))
								{
									if (DebugViewSettings.logApparelGeneration)
									{
										debugSb.AppendLine(" -- Failed: Wrong warmth.");
									}
									goto IL_035e;
								}
							}
							if (num >= 80 || !workingSet.IsNaked(pawn.gender))
							{
								break;
							}
							if (DebugViewSettings.logApparelGeneration)
							{
								debugSb.AppendLine(" -- Failed: Naked.");
							}
						}
						goto IL_035e;
						IL_035e:
						num++;
					}
					if (DebugViewSettings.logApparelGeneration)
					{
						debugSb.Append(" -- Approved! Total price: $" + workingSet.TotalPrice.ToString("F0") + ", TotalInsulationCold: " + workingSet.TotalInsulationCold);
					}
				}
				if ((!pawn.kindDef.apparelIgnoreSeasons || request.ForceAddFreeWarmLayerIfNeeded) && !workingSet.SatisfiesNeededWarmth(neededWarmth, mustBeSafe: true, mapTemperature))
				{
					workingSet.AddFreeWarmthAsNeeded(neededWarmth, mapTemperature);
				}
				if (DebugViewSettings.logApparelGeneration)
				{
					Log.Message(debugSb.ToString());
				}
				workingSet.GiveToPawn(pawn);
				workingSet.Reset(null, null);
				if (pawn.kindDef.apparelColor != Color.white)
				{
					List<Apparel> wornApparel = pawn.apparel.WornApparel;
					for (int i = 0; i < wornApparel.Count; i++)
					{
						wornApparel[i].SetColor(pawn.kindDef.apparelColor, reportFailure: false);
					}
				}
			}
		}

		private static void GenerateWorkingPossibleApparelSetFor(Pawn pawn, float money, bool headwearAllowed)
		{
			workingSet.Reset(pawn.RaceProps.body, pawn.def);
			float num = money;
			List<ThingDef> reqApparel = pawn.kindDef.apparelRequired;
			if (reqApparel != null)
			{
				int i;
				for (i = 0; i < reqApparel.Count; i++)
				{
					ThingStuffPair pair = (from pa in allApparelPairs
					where pa.thing == reqApparel[i]
					select pa).RandomElementByWeight((ThingStuffPair pa) => pa.Commonality);
					workingSet.Add(pair);
					num -= pair.Price;
				}
			}
			int @int = Rand.Int;
			while (!(Rand.Value < 0.1f))
			{
				usableApparel.Clear();
				for (int j = 0; j < allApparelPairs.Count; j++)
				{
					ThingStuffPair thingStuffPair = allApparelPairs[j];
					if (CanUsePair(thingStuffPair, pawn, num, headwearAllowed, @int))
					{
						usableApparel.Add(thingStuffPair);
					}
				}
				ThingStuffPair result;
				bool flag = usableApparel.TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality, out result);
				usableApparel.Clear();
				if (!flag)
				{
					break;
				}
				workingSet.Add(result);
				num -= result.Price;
			}
		}

		private static bool CanUsePair(ThingStuffPair pair, Pawn pawn, float moneyLeft, bool allowHeadgear, int fixedSeed)
		{
			if (pair.Price > moneyLeft)
			{
				return false;
			}
			if (!allowHeadgear && IsHeadgear(pair.thing))
			{
				return false;
			}
			if (pair.stuff != null && pawn.Faction != null && !pawn.Faction.def.CanUseStuffForApparel(pair.stuff))
			{
				return false;
			}
			if (workingSet.PairOverlapsAnything(pair))
			{
				return false;
			}
			if (!pawn.kindDef.apparelTags.NullOrEmpty())
			{
				bool flag = false;
				for (int i = 0; i < pawn.kindDef.apparelTags.Count; i++)
				{
					for (int j = 0; j < pair.thing.apparel.tags.Count; j++)
					{
						if (pawn.kindDef.apparelTags[i] == pair.thing.apparel.tags[j])
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			if (pair.thing.generateAllowChance < 1f && !Rand.ChanceSeeded(pair.thing.generateAllowChance, fixedSeed ^ pair.thing.shortHash ^ 0x3D28557))
			{
				return false;
			}
			return true;
		}

		public static bool IsHeadgear(ThingDef td)
		{
			return td.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead) || td.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead);
		}

		private static NeededWarmth ApparelWarmthNeededNow(Pawn pawn, PawnGenerationRequest request, out float mapTemperature)
		{
			int tile = request.Tile;
			if (tile == -1)
			{
				Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
				if (anyPlayerHomeMap != null)
				{
					tile = anyPlayerHomeMap.Tile;
				}
			}
			if (tile == -1)
			{
				mapTemperature = 21f;
				return NeededWarmth.Any;
			}
			NeededWarmth neededWarmth = NeededWarmth.Any;
			Twelfth twelfth = GenLocalDate.Twelfth(tile);
			mapTemperature = GenTemperature.AverageTemperatureAtTileForTwelfth(tile, twelfth);
			for (int i = 0; i < 2; i++)
			{
				NeededWarmth neededWarmth2 = CalculateNeededWarmth(pawn, tile, twelfth);
				if (neededWarmth2 != 0)
				{
					neededWarmth = neededWarmth2;
					break;
				}
				twelfth = twelfth.NextTwelfth();
			}
			if (pawn.kindDef.apparelIgnoreSeasons)
			{
				if (request.ForceAddFreeWarmLayerIfNeeded && neededWarmth == NeededWarmth.Warm)
				{
					return neededWarmth;
				}
				return NeededWarmth.Any;
			}
			return neededWarmth;
		}

		public static NeededWarmth CalculateNeededWarmth(Pawn pawn, int tile, Twelfth twelfth)
		{
			float num = GenTemperature.AverageTemperatureAtTileForTwelfth(tile, twelfth);
			if (num < pawn.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin) - 4f)
			{
				return NeededWarmth.Warm;
			}
			if (num > pawn.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin) + 4f)
			{
				return NeededWarmth.Cool;
			}
			return NeededWarmth.Any;
		}

		[DebugOutput]
		private static void ApparelPairs()
		{
			DebugTables.MakeTablesDialog(from p in allApparelPairs
			orderby p.thing.defName descending
			select p, new TableDataGetter<ThingStuffPair>("thing", (ThingStuffPair p) => p.thing.defName), new TableDataGetter<ThingStuffPair>("stuff", (ThingStuffPair p) => (p.stuff == null) ? string.Empty : p.stuff.defName), new TableDataGetter<ThingStuffPair>("price", (ThingStuffPair p) => p.Price.ToString()), new TableDataGetter<ThingStuffPair>("commonality", (ThingStuffPair p) => (p.Commonality * 100f).ToString("F4")), new TableDataGetter<ThingStuffPair>("generateCommonality", (ThingStuffPair p) => p.thing.generateCommonality.ToString("F4")), new TableDataGetter<ThingStuffPair>("insulationCold", (ThingStuffPair p) => (p.InsulationCold != 0f) ? p.InsulationCold.ToString() : string.Empty), new TableDataGetter<ThingStuffPair>("headgear", (ThingStuffPair p) => (!IsHeadgear(p.thing)) ? string.Empty : "*"));
		}

		[DebugOutput]
		private static void ApparelPairsByThing()
		{
			DebugOutputsGeneral.MakeTablePairsByThing(allApparelPairs);
		}
	}
}
