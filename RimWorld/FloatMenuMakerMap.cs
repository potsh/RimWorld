using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public static class FloatMenuMakerMap
	{
		public static Pawn makingFor;

		private static FloatMenuOption[] equivalenceGroupTempStorage;

		private static bool CanTakeOrder(Pawn pawn)
		{
			return pawn.IsColonistPlayerControlled;
		}

		public static void TryMakeFloatMenu(Pawn pawn)
		{
			if (CanTakeOrder(pawn))
			{
				if (pawn.Downed)
				{
					Messages.Message("IsIncapped".Translate(pawn.LabelCap, pawn), pawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				else if (pawn.Map == Find.CurrentMap)
				{
					List<FloatMenuOption> list = ChoicesAtFor(UI.MouseMapPosition(), pawn);
					if (list.Count != 0)
					{
						bool flag = true;
						FloatMenuOption floatMenuOption = null;
						for (int i = 0; i < list.Count; i++)
						{
							if (list[i].Disabled || !list[i].autoTakeable)
							{
								flag = false;
								break;
							}
							if (floatMenuOption == null || list[i].autoTakeablePriority > floatMenuOption.autoTakeablePriority)
							{
								floatMenuOption = list[i];
							}
						}
						if (flag && floatMenuOption != null)
						{
							floatMenuOption.Chosen(colonistOrdering: true, null);
						}
						else
						{
							FloatMenuMap floatMenuMap = new FloatMenuMap(list, pawn.LabelCap, UI.MouseMapPosition());
							floatMenuMap.givesColonistOrders = true;
							Find.WindowStack.Add(floatMenuMap);
						}
					}
				}
			}
		}

		public static List<FloatMenuOption> ChoicesAtFor(Vector3 clickPos, Pawn pawn)
		{
			IntVec3 intVec = IntVec3.FromVector3(clickPos);
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			if (!intVec.InBounds(pawn.Map) || !CanTakeOrder(pawn))
			{
				return list;
			}
			if (pawn.Map == Find.CurrentMap)
			{
				makingFor = pawn;
				try
				{
					if (!intVec.Fogged(pawn.Map))
					{
						if (pawn.Drafted)
						{
							AddDraftedOrders(clickPos, pawn, list);
						}
						if (pawn.RaceProps.Humanlike)
						{
							AddHumanlikeOrders(clickPos, pawn, list);
						}
						if (!pawn.Drafted)
						{
							AddUndraftedOrders(clickPos, pawn, list);
						}
						{
							foreach (FloatMenuOption item in pawn.GetExtraFloatMenuOptionsFor(intVec))
							{
								list.Add(item);
							}
							return list;
						}
					}
					if (!pawn.Drafted)
					{
						return list;
					}
					FloatMenuOption floatMenuOption = GotoLocationOption(intVec, pawn);
					if (floatMenuOption == null)
					{
						return list;
					}
					if (floatMenuOption.Disabled)
					{
						return list;
					}
					list.Add(floatMenuOption);
					return list;
				}
				finally
				{
					makingFor = null;
				}
			}
			return list;
		}

		private static void AddDraftedOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
		{
			IntVec3 clickCell = IntVec3.FromVector3(clickPos);
			foreach (LocalTargetInfo item in GenUI.TargetsAt(clickPos, TargetingParameters.ForAttackHostile(), thingsOnly: true))
			{
				LocalTargetInfo attackTarg = item;
				string empty;
				Action action;
				MenuOptionPriority priority;
				Thing thing;
				if (pawn.equipment.Primary != null && !pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.IsMeleeAttack)
				{
					string failStr;
					Action rangedAct = FloatMenuUtility.GetRangedAttackAction(pawn, attackTarg, out failStr);
					string text = "FireAt".Translate(attackTarg.Thing.Label, attackTarg.Thing);
					empty = string.Empty;
					action = null;
					priority = MenuOptionPriority.High;
					thing = item.Thing;
					FloatMenuOption floatMenuOption = new FloatMenuOption(empty, action, priority, null, thing);
					if (rangedAct == null)
					{
						text = text + " (" + failStr + ")";
					}
					else
					{
						floatMenuOption.autoTakeable = (!attackTarg.HasThing || attackTarg.Thing.HostileTo(Faction.OfPlayer));
						floatMenuOption.autoTakeablePriority = 40f;
						floatMenuOption.action = delegate
						{
							MoteMaker.MakeStaticMote(attackTarg.Thing.DrawPos, attackTarg.Thing.Map, ThingDefOf.Mote_FeedbackShoot);
							rangedAct();
						};
					}
					floatMenuOption.Label = text;
					opts.Add(floatMenuOption);
				}
				string failStr2;
				Action meleeAct = FloatMenuUtility.GetMeleeAttackAction(pawn, attackTarg, out failStr2);
				Pawn pawn2 = attackTarg.Thing as Pawn;
				string text2 = (pawn2 == null || !pawn2.Downed) ? "MeleeAttack".Translate(attackTarg.Thing.Label, attackTarg.Thing) : "MeleeAttackToDeath".Translate(attackTarg.Thing.Label, attackTarg.Thing);
				MenuOptionPriority menuOptionPriority = (!attackTarg.HasThing || !pawn.HostileTo(attackTarg.Thing)) ? MenuOptionPriority.VeryLow : MenuOptionPriority.AttackEnemy;
				empty = string.Empty;
				action = null;
				priority = menuOptionPriority;
				thing = attackTarg.Thing;
				FloatMenuOption floatMenuOption2 = new FloatMenuOption(empty, action, priority, null, thing);
				if (meleeAct == null)
				{
					text2 = text2 + " (" + failStr2 + ")";
				}
				else
				{
					floatMenuOption2.autoTakeable = (!attackTarg.HasThing || attackTarg.Thing.HostileTo(Faction.OfPlayer));
					floatMenuOption2.autoTakeablePriority = 30f;
					floatMenuOption2.action = delegate
					{
						MoteMaker.MakeStaticMote(attackTarg.Thing.DrawPos, attackTarg.Thing.Map, ThingDefOf.Mote_FeedbackMelee);
						meleeAct();
					};
				}
				floatMenuOption2.Label = text2;
				opts.Add(floatMenuOption2);
			}
			AddJobGiverWorkOrders(clickCell, pawn, opts, drafted: true);
			FloatMenuOption floatMenuOption3 = GotoLocationOption(clickCell, pawn);
			if (floatMenuOption3 != null)
			{
				opts.Add(floatMenuOption3);
			}
		}

		private static void AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
		{
			IntVec3 c = IntVec3.FromVector3(clickPos);
			if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				foreach (LocalTargetInfo item9 in GenUI.TargetsAt(clickPos, TargetingParameters.ForArrest(pawn), thingsOnly: true))
				{
					LocalTargetInfo dest = item9;
					bool flag = dest.HasThing && dest.Thing is Pawn && ((Pawn)dest.Thing).IsWildMan();
					if (pawn.Drafted || flag)
					{
						if (!pawn.CanReach(dest, PathEndMode.OnCell, Danger.Deadly))
						{
							opts.Add(new FloatMenuOption("CannotArrest".Translate() + " (" + "NoPath".Translate() + ")", null));
						}
						else
						{
							Pawn pTarg = (Pawn)dest.Thing;
							Action action = delegate
							{
								Building_Bed building_Bed3 = RestUtility.FindBedFor(pTarg, pawn, sleeperWillBePrisoner: true, checkSocialProperness: false);
								if (building_Bed3 == null)
								{
									building_Bed3 = RestUtility.FindBedFor(pTarg, pawn, sleeperWillBePrisoner: true, checkSocialProperness: false, ignoreOtherReservations: true);
								}
								if (building_Bed3 == null)
								{
									Messages.Message("CannotArrest".Translate() + ": " + "NoPrisonerBed".Translate(), pTarg, MessageTypeDefOf.RejectInput, historical: false);
								}
								else
								{
									Job job18 = new Job(JobDefOf.Arrest, pTarg, building_Bed3)
									{
										count = 1
									};
									pawn.jobs.TryTakeOrderedJob(job18);
									if (pTarg.Faction != null && pTarg.Faction != Faction.OfPlayer && !pTarg.Faction.def.hidden)
									{
										TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.ArrestingCreatesEnemies);
									}
								}
							};
							string label = "TryToArrest".Translate(dest.Thing.LabelCap, dest.Thing);
							Action action2 = action;
							MenuOptionPriority priority = MenuOptionPriority.High;
							Thing thing = dest.Thing;
							opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action2, priority, null, thing), pawn, pTarg));
						}
					}
				}
			}
			foreach (Thing thing2 in c.GetThingList(pawn.Map))
			{
				Thing t = thing2;
				if (t.def.ingestible != null && pawn.RaceProps.CanEverEat(t) && t.IngestibleNow)
				{
					string text = (!t.def.ingestible.ingestCommandString.NullOrEmpty()) ? string.Format(t.def.ingestible.ingestCommandString, t.LabelShort) : "ConsumeThing".Translate(t.LabelShort, t);
					if (!t.IsSociallyProper(pawn))
					{
						text = text + " (" + "ReservedForPrisoners".Translate() + ")";
					}
					FloatMenuOption item4;
					if (t.def.IsNonMedicalDrug && pawn.IsTeetotaler())
					{
						item4 = new FloatMenuOption(text + " (" + TraitDefOf.DrugDesire.DataAtDegree(-1).label + ")", null);
					}
					else if (!pawn.CanReach(t, PathEndMode.OnCell, Danger.Deadly))
					{
						item4 = new FloatMenuOption(text + " (" + "NoPath".Translate() + ")", null);
					}
					else
					{
						MenuOptionPriority priority2 = (!(t is Corpse)) ? MenuOptionPriority.Default : MenuOptionPriority.Low;
						item4 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate
						{
							t.SetForbidden(value: false);
							Job job17 = new Job(JobDefOf.Ingest, t)
							{
								count = FoodUtility.WillIngestStackCountOf(pawn, t.def, t.GetStatValue(StatDefOf.Nutrition))
							};
							pawn.jobs.TryTakeOrderedJob(job17);
						}, priority2), pawn, t);
					}
					opts.Add(item4);
				}
			}
			foreach (LocalTargetInfo item10 in GenUI.TargetsAt(clickPos, TargetingParameters.ForQuestPawnsWhoWillJoinColony(pawn), thingsOnly: true))
			{
				Pawn toHelpPawn = (Pawn)item10.Thing;
				FloatMenuOption item5;
				if (!pawn.CanReach(item10, PathEndMode.Touch, Danger.Deadly))
				{
					item5 = new FloatMenuOption("CannotGoNoPath".Translate(), null);
				}
				else
				{
					string text2 = (!toHelpPawn.IsPrisoner) ? "OfferHelp".Translate() : "FreePrisoner".Translate();
					string label = text2;
					Action action2 = delegate
					{
						pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.OfferHelp, toHelpPawn));
					};
					MenuOptionPriority priority = MenuOptionPriority.RescueOrCapture;
					Pawn revalidateClickTarget = toHelpPawn;
					item5 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action2, priority, null, revalidateClickTarget), pawn, toHelpPawn);
				}
				opts.Add(item5);
			}
			if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				foreach (LocalTargetInfo item11 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), thingsOnly: true))
				{
					Pawn victim = (Pawn)item11.Thing;
					if (!victim.InBed() && pawn.CanReserveAndReach(victim, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, ignoreOtherReservations: true) && !victim.mindState.WillJoinColonyIfRescued)
					{
						if (!victim.IsPrisonerOfColony && !victim.InMentalState && (victim.Faction == Faction.OfPlayer || victim.Faction == null || !victim.Faction.HostileTo(Faction.OfPlayer)))
						{
							string label = "Rescue".Translate(victim.LabelCap, victim);
							Action action2 = delegate
							{
								Building_Bed building_Bed2 = RestUtility.FindBedFor(victim, pawn, sleeperWillBePrisoner: false, checkSocialProperness: false);
								if (building_Bed2 == null)
								{
									building_Bed2 = RestUtility.FindBedFor(victim, pawn, sleeperWillBePrisoner: false, checkSocialProperness: false, ignoreOtherReservations: true);
								}
								if (building_Bed2 == null)
								{
									Messages.Message(string.Concat(str2: (!victim.RaceProps.Animal) ? "NoNonPrisonerBed".Translate() : "NoAnimalBed".Translate(), str0: "CannotRescue".Translate(), str1: ": "), victim, MessageTypeDefOf.RejectInput, historical: false);
								}
								else
								{
									Job job16 = new Job(JobDefOf.Rescue, victim, building_Bed2)
									{
										count = 1
									};
									pawn.jobs.TryTakeOrderedJob(job16);
									PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Rescuing, KnowledgeAmount.Total);
								}
							};
							MenuOptionPriority priority = MenuOptionPriority.RescueOrCapture;
							Pawn revalidateClickTarget = victim;
							opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action2, priority, null, revalidateClickTarget), pawn, victim));
						}
						if (victim.RaceProps.Humanlike && (victim.InMentalState || victim.Faction != Faction.OfPlayer || (victim.Downed && (victim.guilt.IsGuilty || victim.IsPrisonerOfColony))))
						{
							string text3 = "Capture".Translate(victim.LabelCap, victim);
							if (victim.Faction != null && victim.Faction != Faction.OfPlayer && !victim.Faction.def.hidden && !victim.Faction.HostileTo(Faction.OfPlayer) && !victim.IsPrisonerOfColony)
							{
								text3 = text3 + " (" + "AngersFaction".Translate() + ")";
							}
							string label = text3;
							Action action2 = delegate
							{
								Building_Bed building_Bed = RestUtility.FindBedFor(victim, pawn, sleeperWillBePrisoner: true, checkSocialProperness: false);
								if (building_Bed == null)
								{
									building_Bed = RestUtility.FindBedFor(victim, pawn, sleeperWillBePrisoner: true, checkSocialProperness: false, ignoreOtherReservations: true);
								}
								if (building_Bed == null)
								{
									Messages.Message("CannotCapture".Translate() + ": " + "NoPrisonerBed".Translate(), victim, MessageTypeDefOf.RejectInput, historical: false);
								}
								else
								{
									Job job15 = new Job(JobDefOf.Capture, victim, building_Bed)
									{
										count = 1
									};
									pawn.jobs.TryTakeOrderedJob(job15);
									PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Capturing, KnowledgeAmount.Total);
									if (victim.Faction != null && victim.Faction != Faction.OfPlayer && !victim.Faction.def.hidden && !victim.Faction.HostileTo(Faction.OfPlayer) && !victim.IsPrisonerOfColony)
									{
										Messages.Message("MessageCapturingWillAngerFaction".Translate(victim.Named("PAWN")).AdjustedFor(victim), victim, MessageTypeDefOf.CautionInput, historical: false);
									}
								}
							};
							MenuOptionPriority priority = MenuOptionPriority.RescueOrCapture;
							Pawn revalidateClickTarget = victim;
							opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action2, priority, null, revalidateClickTarget), pawn, victim));
						}
					}
				}
				foreach (LocalTargetInfo item12 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), thingsOnly: true))
				{
					LocalTargetInfo localTargetInfo = item12;
					Pawn victim2 = (Pawn)localTargetInfo.Thing;
					if (victim2.Downed && pawn.CanReserveAndReach(victim2, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, ignoreOtherReservations: true) && Building_CryptosleepCasket.FindCryptosleepCasketFor(victim2, pawn, ignoreOtherReservations: true) != null)
					{
						string text4 = "CarryToCryptosleepCasket".Translate(localTargetInfo.Thing.LabelCap, localTargetInfo.Thing);
						JobDef jDef = JobDefOf.CarryToCryptosleepCasket;
						Action action3 = delegate
						{
							Building_CryptosleepCasket building_CryptosleepCasket = Building_CryptosleepCasket.FindCryptosleepCasketFor(victim2, pawn);
							if (building_CryptosleepCasket == null)
							{
								building_CryptosleepCasket = Building_CryptosleepCasket.FindCryptosleepCasketFor(victim2, pawn, ignoreOtherReservations: true);
							}
							if (building_CryptosleepCasket == null)
							{
								Messages.Message("CannotCarryToCryptosleepCasket".Translate() + ": " + "NoCryptosleepCasket".Translate(), victim2, MessageTypeDefOf.RejectInput, historical: false);
							}
							else
							{
								Job job14 = new Job(jDef, victim2, building_CryptosleepCasket)
								{
									count = 1
								};
								pawn.jobs.TryTakeOrderedJob(job14);
							}
						};
						string label = text4;
						Action action2 = action3;
						Pawn revalidateClickTarget = victim2;
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action2, MenuOptionPriority.Default, null, revalidateClickTarget), pawn, victim2));
					}
				}
			}
			foreach (LocalTargetInfo item13 in GenUI.TargetsAt(clickPos, TargetingParameters.ForStrip(pawn), thingsOnly: true))
			{
				LocalTargetInfo stripTarg = item13;
				FloatMenuOption item6 = pawn.CanReach(stripTarg, PathEndMode.ClosestTouch, Danger.Deadly) ? FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Strip".Translate(stripTarg.Thing.LabelCap, stripTarg.Thing), delegate
				{
					stripTarg.Thing.SetForbidden(value: false, warnOnFail: false);
					pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.Strip, stripTarg));
				}), pawn, stripTarg) : new FloatMenuOption("CannotStrip".Translate(stripTarg.Thing.LabelCap, stripTarg.Thing) + " (" + "NoPath".Translate() + ")", null);
				opts.Add(item6);
			}
			if (pawn.equipment != null)
			{
				ThingWithComps equipment = null;
				List<Thing> thingList = c.GetThingList(pawn.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					if (thingList[i].TryGetComp<CompEquippable>() != null)
					{
						equipment = (ThingWithComps)thingList[i];
						break;
					}
				}
				if (equipment != null)
				{
					string labelShort = equipment.LabelShort;
					FloatMenuOption item7;
					if (equipment.def.IsWeapon && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
					{
						item7 = new FloatMenuOption("CannotEquip".Translate(labelShort) + " (" + "IsIncapableOfViolenceLower".Translate(pawn.LabelShort, pawn) + ")", null);
					}
					else if (!pawn.CanReach(equipment, PathEndMode.ClosestTouch, Danger.Deadly))
					{
						item7 = new FloatMenuOption("CannotEquip".Translate(labelShort) + " (" + "NoPath".Translate() + ")", null);
					}
					else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
					{
						item7 = new FloatMenuOption("CannotEquip".Translate(labelShort) + " (" + "Incapable".Translate() + ")", null);
					}
					else if (equipment.IsBurning())
					{
						item7 = new FloatMenuOption("CannotEquip".Translate(labelShort) + " (" + "BurningLower".Translate() + ")", null);
					}
					else
					{
						string text5 = "Equip".Translate(labelShort);
						if (equipment.def.IsRangedWeapon && pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler))
						{
							text5 = text5 + " " + "EquipWarningBrawler".Translate();
						}
						item7 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text5, delegate
						{
							equipment.SetForbidden(value: false);
							pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.Equip, equipment));
							MoteMaker.MakeStaticMote(equipment.DrawPos, equipment.Map, ThingDefOf.Mote_FeedbackEquip);
							PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.EquippingWeapons, KnowledgeAmount.Total);
						}, MenuOptionPriority.High), pawn, equipment);
					}
					opts.Add(item7);
				}
			}
			if (pawn.apparel != null)
			{
				Apparel apparel = pawn.Map.thingGrid.ThingAt<Apparel>(c);
				if (apparel != null)
				{
					FloatMenuOption item8 = (!pawn.CanReach(apparel, PathEndMode.ClosestTouch, Danger.Deadly)) ? new FloatMenuOption("CannotWear".Translate(apparel.Label, apparel) + " (" + "NoPath".Translate() + ")", null) : (apparel.IsBurning() ? new FloatMenuOption("CannotWear".Translate(apparel.Label, apparel) + " (" + "BurningLower".Translate() + ")", null) : (ApparelUtility.HasPartsToWear(pawn, apparel.def) ? FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("ForceWear".Translate(apparel.LabelShort, apparel), delegate
					{
						apparel.SetForbidden(value: false);
						Job job13 = new Job(JobDefOf.Wear, apparel);
						pawn.jobs.TryTakeOrderedJob(job13);
					}, MenuOptionPriority.High), pawn, apparel) : new FloatMenuOption("CannotWear".Translate(apparel.Label, apparel) + " (" + "CannotWearBecauseOfMissingBodyParts".Translate() + ")", null)));
					opts.Add(item8);
				}
			}
			if (pawn.IsFormingCaravan())
			{
				Thing item = c.GetFirstItem(pawn.Map);
				if (item != null && item.def.EverHaulable)
				{
					Pawn packTarget = GiveToPackAnimalUtility.UsablePackAnimalWithTheMostFreeSpace(pawn) ?? pawn;
					JobDef jobDef = (packTarget != pawn) ? JobDefOf.GiveToPackAnimal : JobDefOf.TakeInventory;
					if (!pawn.CanReach(item, PathEndMode.ClosestTouch, Danger.Deadly))
					{
						opts.Add(new FloatMenuOption("CannotLoadIntoCaravan".Translate(item.Label, item) + " (" + "NoPath".Translate() + ")", null));
					}
					else if (MassUtility.WillBeOverEncumberedAfterPickingUp(packTarget, item, 1))
					{
						opts.Add(new FloatMenuOption("CannotLoadIntoCaravan".Translate(item.Label, item) + " (" + "TooHeavy".Translate() + ")", null));
					}
					else
					{
						LordJob_FormAndSendCaravan lordJob = (LordJob_FormAndSendCaravan)pawn.GetLord().LordJob;
						float capacityLeft = CaravanFormingUtility.CapacityLeft(lordJob);
						if (item.stackCount == 1)
						{
							float capacityLeft2 = capacityLeft - item.GetStatValue(StatDefOf.Mass);
							opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(CaravanFormingUtility.AppendOverweightInfo("LoadIntoCaravan".Translate(item.Label, item), capacityLeft2), delegate
							{
								item.SetForbidden(value: false, warnOnFail: false);
								Job job12 = new Job(jobDef, item)
								{
									count = 1,
									checkEncumbrance = (packTarget == pawn)
								};
								pawn.jobs.TryTakeOrderedJob(job12);
							}, MenuOptionPriority.High), pawn, item));
						}
						else
						{
							if (MassUtility.WillBeOverEncumberedAfterPickingUp(packTarget, item, item.stackCount))
							{
								opts.Add(new FloatMenuOption("CannotLoadIntoCaravanAll".Translate(item.Label, item) + " (" + "TooHeavy".Translate() + ")", null));
							}
							else
							{
								float capacityLeft3 = capacityLeft - (float)item.stackCount * item.GetStatValue(StatDefOf.Mass);
								opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(CaravanFormingUtility.AppendOverweightInfo("LoadIntoCaravanAll".Translate(item.Label, item), capacityLeft3), delegate
								{
									item.SetForbidden(value: false, warnOnFail: false);
									Job job11 = new Job(jobDef, item)
									{
										count = item.stackCount,
										checkEncumbrance = (packTarget == pawn)
									};
									pawn.jobs.TryTakeOrderedJob(job11);
								}, MenuOptionPriority.High), pawn, item));
							}
							opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("LoadIntoCaravanSome".Translate(item.LabelNoCount, item), delegate
							{
								int to3 = Mathf.Min(MassUtility.CountToPickUpUntilOverEncumbered(packTarget, item), item.stackCount);
								Dialog_Slider window3 = new Dialog_Slider(delegate(int val)
								{
									float capacityLeft4 = capacityLeft - (float)val * item.GetStatValue(StatDefOf.Mass);
									return CaravanFormingUtility.AppendOverweightInfo(string.Format("LoadIntoCaravanCount".Translate(item.LabelNoCount, item), val), capacityLeft4);
								}, 1, to3, delegate(int count)
								{
									item.SetForbidden(value: false, warnOnFail: false);
									Job job10 = new Job(jobDef, item)
									{
										count = count,
										checkEncumbrance = (packTarget == pawn)
									};
									pawn.jobs.TryTakeOrderedJob(job10);
								});
								Find.WindowStack.Add(window3);
							}, MenuOptionPriority.High), pawn, item));
						}
					}
				}
			}
			if (!pawn.Map.IsPlayerHome && !pawn.IsFormingCaravan())
			{
				Thing item2 = c.GetFirstItem(pawn.Map);
				if (item2 != null && item2.def.EverHaulable)
				{
					if (!pawn.CanReach(item2, PathEndMode.ClosestTouch, Danger.Deadly))
					{
						opts.Add(new FloatMenuOption("CannotPickUp".Translate(item2.Label, item2) + " (" + "NoPath".Translate() + ")", null));
					}
					else if (MassUtility.WillBeOverEncumberedAfterPickingUp(pawn, item2, 1))
					{
						opts.Add(new FloatMenuOption("CannotPickUp".Translate(item2.Label, item2) + " (" + "TooHeavy".Translate() + ")", null));
					}
					else if (item2.stackCount == 1)
					{
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUp".Translate(item2.Label, item2), delegate
						{
							item2.SetForbidden(value: false, warnOnFail: false);
							Job job9 = new Job(JobDefOf.TakeInventory, item2)
							{
								count = 1,
								checkEncumbrance = true
							};
							pawn.jobs.TryTakeOrderedJob(job9);
						}, MenuOptionPriority.High), pawn, item2));
					}
					else
					{
						if (MassUtility.WillBeOverEncumberedAfterPickingUp(pawn, item2, item2.stackCount))
						{
							opts.Add(new FloatMenuOption("CannotPickUpAll".Translate(item2.Label, item2) + " (" + "TooHeavy".Translate() + ")", null));
						}
						else
						{
							opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUpAll".Translate(item2.Label, item2), delegate
							{
								item2.SetForbidden(value: false, warnOnFail: false);
								Job job8 = new Job(JobDefOf.TakeInventory, item2)
								{
									count = item2.stackCount,
									checkEncumbrance = true
								};
								pawn.jobs.TryTakeOrderedJob(job8);
							}, MenuOptionPriority.High), pawn, item2));
						}
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUpSome".Translate(item2.LabelNoCount, item2), delegate
						{
							int to2 = Mathf.Min(MassUtility.CountToPickUpUntilOverEncumbered(pawn, item2), item2.stackCount);
							Dialog_Slider window2 = new Dialog_Slider("PickUpCount".Translate(item2.LabelNoCount, item2), 1, to2, delegate(int count)
							{
								item2.SetForbidden(value: false, warnOnFail: false);
								Job job7 = new Job(JobDefOf.TakeInventory, item2)
								{
									count = count,
									checkEncumbrance = true
								};
								pawn.jobs.TryTakeOrderedJob(job7);
							});
							Find.WindowStack.Add(window2);
						}, MenuOptionPriority.High), pawn, item2));
					}
				}
			}
			if (!pawn.Map.IsPlayerHome && !pawn.IsFormingCaravan())
			{
				Thing item3 = c.GetFirstItem(pawn.Map);
				if (item3 != null && item3.def.EverHaulable)
				{
					Pawn bestPackAnimal = GiveToPackAnimalUtility.UsablePackAnimalWithTheMostFreeSpace(pawn);
					if (bestPackAnimal != null)
					{
						if (!pawn.CanReach(item3, PathEndMode.ClosestTouch, Danger.Deadly))
						{
							opts.Add(new FloatMenuOption("CannotGiveToPackAnimal".Translate(item3.Label, item3) + " (" + "NoPath".Translate() + ")", null));
						}
						else if (MassUtility.WillBeOverEncumberedAfterPickingUp(bestPackAnimal, item3, 1))
						{
							opts.Add(new FloatMenuOption("CannotGiveToPackAnimal".Translate(item3.Label, item3) + " (" + "TooHeavy".Translate() + ")", null));
						}
						else if (item3.stackCount == 1)
						{
							opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("GiveToPackAnimal".Translate(item3.Label, item3), delegate
							{
								item3.SetForbidden(value: false, warnOnFail: false);
								Job job6 = new Job(JobDefOf.GiveToPackAnimal, item3)
								{
									count = 1
								};
								pawn.jobs.TryTakeOrderedJob(job6);
							}, MenuOptionPriority.High), pawn, item3));
						}
						else
						{
							if (MassUtility.WillBeOverEncumberedAfterPickingUp(bestPackAnimal, item3, item3.stackCount))
							{
								opts.Add(new FloatMenuOption("CannotGiveToPackAnimalAll".Translate(item3.Label, item3) + " (" + "TooHeavy".Translate() + ")", null));
							}
							else
							{
								opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("GiveToPackAnimalAll".Translate(item3.Label, item3), delegate
								{
									item3.SetForbidden(value: false, warnOnFail: false);
									Job job5 = new Job(JobDefOf.GiveToPackAnimal, item3)
									{
										count = item3.stackCount
									};
									pawn.jobs.TryTakeOrderedJob(job5);
								}, MenuOptionPriority.High), pawn, item3));
							}
							opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("GiveToPackAnimalSome".Translate(item3.LabelNoCount, item3), delegate
							{
								int to = Mathf.Min(MassUtility.CountToPickUpUntilOverEncumbered(bestPackAnimal, item3), item3.stackCount);
								Dialog_Slider window = new Dialog_Slider("GiveToPackAnimalCount".Translate(item3.LabelNoCount, item3), 1, to, delegate(int count)
								{
									item3.SetForbidden(value: false, warnOnFail: false);
									Job job4 = new Job(JobDefOf.GiveToPackAnimal, item3)
									{
										count = count
									};
									pawn.jobs.TryTakeOrderedJob(job4);
								});
								Find.WindowStack.Add(window);
							}, MenuOptionPriority.High), pawn, item3));
						}
					}
				}
			}
			if (!pawn.Map.IsPlayerHome && pawn.Map.exitMapGrid.MapUsesExitGrid)
			{
				foreach (LocalTargetInfo item14 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), thingsOnly: true))
				{
					Pawn p = (Pawn)item14.Thing;
					if (p.Faction == Faction.OfPlayer || p.IsPrisonerOfColony || CaravanUtility.ShouldAutoCapture(p, Faction.OfPlayer))
					{
						IntVec3 exitSpot;
						if (!pawn.CanReach(p, PathEndMode.ClosestTouch, Danger.Deadly))
						{
							opts.Add(new FloatMenuOption("CannotCarryToExit".Translate(p.Label, p) + " (" + "NoPath".Translate() + ")", null));
						}
						else if (!RCellFinder.TryFindBestExitSpot(pawn, out exitSpot))
						{
							opts.Add(new FloatMenuOption("CannotCarryToExit".Translate(p.Label, p) + " (" + "NoPath".Translate() + ")", null));
						}
						else
						{
							string label2 = (p.Faction != Faction.OfPlayer && !p.IsPrisonerOfColony) ? "CarryToExitAndCapture".Translate(p.Label, p) : "CarryToExit".Translate(p.Label, p);
							opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label2, delegate
							{
								Job job3 = new Job(JobDefOf.CarryDownedPawnToExit, p, exitSpot)
								{
									count = 1
								};
								pawn.jobs.TryTakeOrderedJob(job3);
							}, MenuOptionPriority.High), pawn, item14));
						}
					}
				}
			}
			if (pawn.equipment != null && pawn.equipment.Primary != null && GenUI.TargetsAt(clickPos, TargetingParameters.ForSelf(pawn), thingsOnly: true).Any())
			{
				Action action4 = delegate
				{
					pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.DropEquipment, pawn.equipment.Primary));
				};
				string label = "Drop".Translate(pawn.equipment.Primary.Label, pawn.equipment.Primary);
				Action action2 = action4;
				Pawn revalidateClickTarget = pawn;
				opts.Add(new FloatMenuOption(label, action2, MenuOptionPriority.Default, null, revalidateClickTarget));
			}
			foreach (LocalTargetInfo item15 in GenUI.TargetsAt(clickPos, TargetingParameters.ForTrade(), thingsOnly: true))
			{
				LocalTargetInfo dest2 = item15;
				if (!pawn.CanReach(dest2, PathEndMode.OnCell, Danger.Deadly))
				{
					opts.Add(new FloatMenuOption("CannotTrade".Translate() + " (" + "NoPath".Translate() + ")", null));
				}
				else if (pawn.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
				{
					opts.Add(new FloatMenuOption("CannotPrioritizeWorkTypeDisabled".Translate(SkillDefOf.Social.LabelCap), null));
				}
				else
				{
					Pawn pTarg2 = (Pawn)dest2.Thing;
					Action action5 = delegate
					{
						Job job2 = new Job(JobDefOf.TradeWithPawn, pTarg2)
						{
							playerForced = true
						};
						pawn.jobs.TryTakeOrderedJob(job2);
						PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.InteractingWithTraders, KnowledgeAmount.Total);
					};
					string str = string.Empty;
					if (pTarg2.Faction != null)
					{
						str = " (" + pTarg2.Faction.Name + ")";
					}
					string label = "TradeWith".Translate(pTarg2.LabelShort + ", " + pTarg2.TraderKind.label) + str;
					Action action2 = action5;
					MenuOptionPriority priority = MenuOptionPriority.InitiateSocial;
					Thing thing = dest2.Thing;
					opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action2, priority, null, thing), pawn, pTarg2));
				}
			}
			foreach (LocalTargetInfo item16 in GenUI.TargetsAt(clickPos, TargetingParameters.ForOpen(pawn), thingsOnly: true))
			{
				if (!pawn.CanReach(item16, PathEndMode.OnCell, Danger.Deadly))
				{
					opts.Add(new FloatMenuOption("CannotOpen".Translate(item16.Thing) + " (" + "NoPath".Translate() + ")", null));
				}
				else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
				{
					opts.Add(new FloatMenuOption("CannotOpen".Translate(item16.Thing) + " (" + "Incapable".Translate() + ")", null));
				}
				else
				{
					opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Open".Translate(item16.Thing), delegate
					{
						Job job = new Job(JobDefOf.Open, item16.Thing)
						{
							ignoreDesignations = true
						};
						pawn.jobs.TryTakeOrderedJob(job);
					}, MenuOptionPriority.High), pawn, item16.Thing));
				}
			}
			foreach (Thing item17 in pawn.Map.thingGrid.ThingsAt(c))
			{
				foreach (FloatMenuOption floatMenuOption in item17.GetFloatMenuOptions(pawn))
				{
					opts.Add(floatMenuOption);
				}
			}
		}

		private static void AddUndraftedOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
		{
			if (equivalenceGroupTempStorage == null || equivalenceGroupTempStorage.Length != DefDatabase<WorkGiverEquivalenceGroupDef>.DefCount)
			{
				equivalenceGroupTempStorage = new FloatMenuOption[DefDatabase<WorkGiverEquivalenceGroupDef>.DefCount];
			}
			IntVec3 intVec = IntVec3.FromVector3(clickPos);
			bool flag = false;
			bool flag2 = false;
			foreach (Thing item in pawn.Map.thingGrid.ThingsAt(intVec))
			{
				flag2 = true;
				if (pawn.CanReach(item, PathEndMode.Touch, Danger.Deadly))
				{
					flag = true;
					break;
				}
			}
			if (flag2 && !flag)
			{
				opts.Add(new FloatMenuOption("(" + "NoPath".Translate() + ")", null));
			}
			else
			{
				AddJobGiverWorkOrders(intVec, pawn, opts, drafted: false);
			}
		}

		private static void AddJobGiverWorkOrders(IntVec3 clickCell, Pawn pawn, List<FloatMenuOption> opts, bool drafted)
		{
			JobGiver_Work jobGiver_Work = pawn.thinker.TryGetMainTreeThinkNode<JobGiver_Work>();
			if (jobGiver_Work != null)
			{
				foreach (Thing item in pawn.Map.thingGrid.ThingsAt(clickCell))
				{
					bool flag = false;
					foreach (WorkTypeDef item2 in DefDatabase<WorkTypeDef>.AllDefsListForReading)
					{
						for (int i = 0; i < item2.workGiversByPriority.Count; i++)
						{
							WorkGiverDef workGiver = item2.workGiversByPriority[i];
							if (!drafted || workGiver.canBeDoneWhileDrafted)
							{
								WorkGiver_Scanner workGiver_Scanner = workGiver.Worker as WorkGiver_Scanner;
								if (workGiver_Scanner != null && workGiver_Scanner.def.directOrderable && !workGiver_Scanner.ShouldSkip(pawn, forced: true))
								{
									JobFailReason.Clear();
									if (workGiver_Scanner.PotentialWorkThingRequest.Accepts(item) || (workGiver_Scanner.PotentialWorkThingsGlobal(pawn) != null && workGiver_Scanner.PotentialWorkThingsGlobal(pawn).Contains(item)))
									{
										string text = null;
										Action action = null;
										PawnCapacityDef pawnCapacityDef = workGiver_Scanner.MissingRequiredCapacity(pawn);
										if (pawnCapacityDef != null)
										{
											text = "CannotMissingHealthActivities".Translate(pawnCapacityDef.label);
										}
										else
										{
											Job job = workGiver_Scanner.HasJobOnThing(pawn, item, forced: true) ? workGiver_Scanner.JobOnThing(pawn, item, forced: true) : null;
											if (job == null)
											{
												if (JobFailReason.HaveReason)
												{
													text = (JobFailReason.CustomJobString.NullOrEmpty() ? "CannotGenericWork".Translate(workGiver_Scanner.def.verb, item.LabelShort, item) : "CannotGenericWorkCustom".Translate(JobFailReason.CustomJobString));
													text = text + " (" + JobFailReason.Reason + ")";
												}
												else
												{
													if (!item.IsForbidden(pawn))
													{
														continue;
													}
													text = (item.Position.InAllowedArea(pawn) ? "CannotPrioritizeForbidden".Translate(item.Label, item) : ("CannotPrioritizeForbiddenOutsideAllowedArea".Translate() + " (" + pawn.playerSettings.EffectiveAreaRestriction.Label + ")"));
												}
											}
											else
											{
												WorkTypeDef workType = workGiver_Scanner.def.workType;
												if (pawn.story != null && pawn.story.WorkTagIsDisabled(workGiver_Scanner.def.workTags))
												{
													text = "CannotPrioritizeWorkGiverDisabled".Translate(workGiver_Scanner.def.label);
												}
												else if (pawn.jobs.curJob != null && pawn.jobs.curJob.JobIsSameAs(job))
												{
													text = "CannotGenericAlreadyAm".Translate(workGiver_Scanner.def.gerund, item.LabelShort, item);
												}
												else if (pawn.workSettings.GetPriority(workType) == 0)
												{
													text = (pawn.story.WorkTypeIsDisabled(workType) ? "CannotPrioritizeWorkTypeDisabled".Translate(workType.gerundLabel) : ((!"CannotPrioritizeNotAssignedToWorkType".CanTranslate()) ? "CannotPrioritizeWorkTypeDisabled".Translate(workType.pawnLabel) : "CannotPrioritizeNotAssignedToWorkType".Translate(workType.gerundLabel)));
												}
												else if (job.def == JobDefOf.Research && item is Building_ResearchBench)
												{
													text = "CannotPrioritizeResearch".Translate();
												}
												else if (item.IsForbidden(pawn))
												{
													text = (item.Position.InAllowedArea(pawn) ? "CannotPrioritizeForbidden".Translate(item.Label, item) : ("CannotPrioritizeForbiddenOutsideAllowedArea".Translate() + " (" + pawn.playerSettings.EffectiveAreaRestriction.Label + ")"));
												}
												else if (!pawn.CanReach(item, workGiver_Scanner.PathEndMode, Danger.Deadly))
												{
													text = (item.Label + ": " + "NoPath".Translate()).CapitalizeFirst();
												}
												else
												{
													text = "PrioritizeGeneric".Translate(workGiver_Scanner.def.gerund, item.Label);
													Job localJob = job;
													WorkGiver_Scanner localScanner = workGiver_Scanner;
													action = delegate
													{
														if (pawn.jobs.TryTakeOrderedJobPrioritizedWork(localJob, localScanner, clickCell) && workGiver.forceMote != null)
														{
															MoteMaker.MakeStaticMote(clickCell, pawn.Map, workGiver.forceMote);
														}
													};
												}
											}
										}
										if (DebugViewSettings.showFloatMenuWorkGivers)
										{
											text += $" (from {workGiver.defName})";
										}
										FloatMenuOption menuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, action), pawn, item);
										if (drafted && workGiver.autoTakeablePriorityDrafted != -1)
										{
											menuOption.autoTakeable = true;
											menuOption.autoTakeablePriority = (float)workGiver.autoTakeablePriorityDrafted;
										}
										if (!opts.Any((FloatMenuOption op) => op.Label == menuOption.Label))
										{
											if (workGiver.equivalenceGroup != null)
											{
												if (equivalenceGroupTempStorage[workGiver.equivalenceGroup.index] == null || (equivalenceGroupTempStorage[workGiver.equivalenceGroup.index].Disabled && !menuOption.Disabled))
												{
													equivalenceGroupTempStorage[workGiver.equivalenceGroup.index] = menuOption;
													flag = true;
												}
											}
											else
											{
												opts.Add(menuOption);
											}
										}
									}
								}
							}
						}
					}
					if (flag)
					{
						for (int j = 0; j < equivalenceGroupTempStorage.Length; j++)
						{
							if (equivalenceGroupTempStorage[j] != null)
							{
								opts.Add(equivalenceGroupTempStorage[j]);
								equivalenceGroupTempStorage[j] = null;
							}
						}
					}
				}
				foreach (WorkTypeDef item3 in DefDatabase<WorkTypeDef>.AllDefsListForReading)
				{
					for (int k = 0; k < item3.workGiversByPriority.Count; k++)
					{
						WorkGiverDef workGiver2 = item3.workGiversByPriority[k];
						if (!drafted || workGiver2.canBeDoneWhileDrafted)
						{
							WorkGiver_Scanner workGiver_Scanner2 = workGiver2.Worker as WorkGiver_Scanner;
							if (workGiver_Scanner2 != null && workGiver_Scanner2.def.directOrderable && !workGiver_Scanner2.ShouldSkip(pawn, forced: true))
							{
								JobFailReason.Clear();
								if (workGiver_Scanner2.PotentialWorkCellsGlobal(pawn).Contains(clickCell))
								{
									Action action2 = null;
									string label = null;
									PawnCapacityDef pawnCapacityDef2 = workGiver_Scanner2.MissingRequiredCapacity(pawn);
									if (pawnCapacityDef2 != null)
									{
										label = "CannotMissingHealthActivities".Translate(pawnCapacityDef2.label);
									}
									else
									{
										Job job2 = workGiver_Scanner2.HasJobOnCell(pawn, clickCell, forced: true) ? workGiver_Scanner2.JobOnCell(pawn, clickCell, forced: true) : null;
										if (job2 == null)
										{
											if (JobFailReason.HaveReason)
											{
												if (!JobFailReason.CustomJobString.NullOrEmpty())
												{
													label = "CannotGenericWorkCustom".Translate(JobFailReason.CustomJobString);
												}
												else
												{
													label = "CannotGenericWork".Translate(workGiver_Scanner2.def.verb, "AreaLower".Translate());
												}
												label = label + " (" + JobFailReason.Reason + ")";
											}
											else
											{
												if (!clickCell.IsForbidden(pawn))
												{
													continue;
												}
												if (!clickCell.InAllowedArea(pawn))
												{
													label = "CannotPrioritizeForbiddenOutsideAllowedArea".Translate() + " (" + pawn.playerSettings.EffectiveAreaRestriction.Label + ")";
												}
												else
												{
													label = "CannotPrioritizeCellForbidden".Translate();
												}
											}
										}
										else
										{
											WorkTypeDef workType2 = workGiver_Scanner2.def.workType;
											if (pawn.jobs.curJob != null && pawn.jobs.curJob.JobIsSameAs(job2))
											{
												label = "CannotGenericAlreadyAm".Translate(workGiver_Scanner2.def.gerund, "AreaLower".Translate());
											}
											else if (pawn.workSettings.GetPriority(workType2) == 0)
											{
												if (pawn.story.WorkTypeIsDisabled(workType2))
												{
													label = "CannotPrioritizeWorkTypeDisabled".Translate(workType2.gerundLabel);
												}
												else if ("CannotPrioritizeNotAssignedToWorkType".CanTranslate())
												{
													label = "CannotPrioritizeNotAssignedToWorkType".Translate(workType2.gerundLabel);
												}
												else
												{
													label = "CannotPrioritizeWorkTypeDisabled".Translate(workType2.pawnLabel);
												}
											}
											else if (clickCell.IsForbidden(pawn))
											{
												if (!clickCell.InAllowedArea(pawn))
												{
													label = "CannotPrioritizeForbiddenOutsideAllowedArea".Translate() + " (" + pawn.playerSettings.EffectiveAreaRestriction.Label + ")";
												}
												else
												{
													label = "CannotPrioritizeCellForbidden".Translate();
												}
											}
											else if (!pawn.CanReach(clickCell, PathEndMode.Touch, Danger.Deadly))
											{
												label = "AreaLower".Translate().CapitalizeFirst() + ": " + "NoPath".Translate();
											}
											else
											{
												label = "PrioritizeGeneric".Translate(workGiver_Scanner2.def.gerund, "AreaLower".Translate());
												Job localJob2 = job2;
												WorkGiver_Scanner localScanner2 = workGiver_Scanner2;
												action2 = delegate
												{
													if (pawn.jobs.TryTakeOrderedJobPrioritizedWork(localJob2, localScanner2, clickCell) && workGiver2.forceMote != null)
													{
														MoteMaker.MakeStaticMote(clickCell, pawn.Map, workGiver2.forceMote);
													}
												};
											}
										}
									}
									if (!opts.Any((FloatMenuOption op) => op.Label == label.TrimEnd()))
									{
										FloatMenuOption floatMenuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action2), pawn, clickCell);
										if (drafted && workGiver2.autoTakeablePriorityDrafted != -1)
										{
											floatMenuOption.autoTakeable = true;
											floatMenuOption.autoTakeablePriority = (float)workGiver2.autoTakeablePriorityDrafted;
										}
										opts.Add(floatMenuOption);
									}
								}
							}
						}
					}
				}
			}
		}

		private static FloatMenuOption GotoLocationOption(IntVec3 clickCell, Pawn pawn)
		{
			int num = GenRadial.NumCellsInRadius(2.9f);
			IntVec3 curLoc;
			for (int i = 0; i < num; i++)
			{
				curLoc = GenRadial.RadialPattern[i] + clickCell;
				if (curLoc.Standable(pawn.Map))
				{
					if (curLoc != pawn.Position)
					{
						if (!pawn.CanReach(curLoc, PathEndMode.OnCell, Danger.Deadly))
						{
							return new FloatMenuOption("CannotGoNoPath".Translate(), null);
						}
						Action action = delegate
						{
							IntVec3 intVec = RCellFinder.BestOrderedGotoDestNear(curLoc, pawn);
							Job job = new Job(JobDefOf.Goto, intVec);
							if (pawn.Map.exitMapGrid.IsExitCell(UI.MouseCell()))
							{
								job.exitMapOnArrival = true;
							}
							else if (!pawn.Map.IsPlayerHome && !pawn.Map.exitMapGrid.MapUsesExitGrid && CellRect.WholeMap(pawn.Map).IsOnEdge(UI.MouseCell(), 3) && pawn.Map.Parent.GetComponent<FormCaravanComp>() != null && MessagesRepeatAvoider.MessageShowAllowed("MessagePlayerTriedToLeaveMapViaExitGrid-" + pawn.Map.uniqueID, 60f))
							{
								FormCaravanComp component = pawn.Map.Parent.GetComponent<FormCaravanComp>();
								if (component.CanFormOrReformCaravanNow)
								{
									Messages.Message("MessagePlayerTriedToLeaveMapViaExitGrid_CanReform".Translate(), pawn.Map.Parent, MessageTypeDefOf.RejectInput, historical: false);
								}
								else
								{
									Messages.Message("MessagePlayerTriedToLeaveMapViaExitGrid_CantReform".Translate(), pawn.Map.Parent, MessageTypeDefOf.RejectInput, historical: false);
								}
							}
							if (pawn.jobs.TryTakeOrderedJob(job))
							{
								MoteMaker.MakeStaticMote(intVec, pawn.Map, ThingDefOf.Mote_FeedbackGoto);
							}
						};
						FloatMenuOption floatMenuOption = new FloatMenuOption("GoHere".Translate(), action, MenuOptionPriority.GoHere);
						floatMenuOption.autoTakeable = true;
						floatMenuOption.autoTakeablePriority = 10f;
						return floatMenuOption;
					}
					return null;
				}
			}
			return null;
		}
	}
}
