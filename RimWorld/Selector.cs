using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Selector
	{
		public DragBox dragBox = new DragBox();

		private List<object> selected = new List<object>();

		private const float PawnSelectRadius = 1f;

		private const int MaxNumSelected = 80;

		private bool ShiftIsHeld => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

		public List<object> SelectedObjects => selected;

		public List<object> SelectedObjectsListForReading => selected;

		public Thing SingleSelectedThing
		{
			get
			{
				if (selected.Count != 1)
				{
					return null;
				}
				if (selected[0] is Thing)
				{
					return (Thing)selected[0];
				}
				return null;
			}
		}

		public object FirstSelectedObject
		{
			get
			{
				if (selected.Count == 0)
				{
					return null;
				}
				return selected[0];
			}
		}

		public object SingleSelectedObject
		{
			get
			{
				if (selected.Count != 1)
				{
					return null;
				}
				return selected[0];
			}
		}

		public int NumSelected => selected.Count;

		public Zone SelectedZone
		{
			get
			{
				if (selected.Count == 0)
				{
					return null;
				}
				return selected[0] as Zone;
			}
			set
			{
				ClearSelection();
				if (value != null)
				{
					Select(value);
				}
			}
		}

		public void SelectorOnGUI()
		{
			HandleMapClicks();
			if (KeyBindingDefOf.Cancel.KeyDownEvent && selected.Count > 0)
			{
				ClearSelection();
				Event.current.Use();
			}
			if (NumSelected > 0 && Find.MainTabsRoot.OpenTab == null && !WorldRendererUtility.WorldRenderedNow)
			{
				Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Inspect, playSound: false);
			}
		}

		private void HandleMapClicks()
		{
			if (Event.current.type == EventType.MouseDown)
			{
				if (Event.current.button == 0)
				{
					if (Event.current.clickCount == 1)
					{
						dragBox.active = true;
						dragBox.start = UI.MouseMapPosition();
					}
					if (Event.current.clickCount == 2)
					{
						SelectAllMatchingObjectUnderMouseOnScreen();
					}
					Event.current.Use();
				}
				if (Event.current.button == 1 && selected.Count > 0)
				{
					if (selected.Count == 1 && selected[0] is Pawn)
					{
						FloatMenuMakerMap.TryMakeFloatMenu((Pawn)selected[0]);
					}
					else
					{
						for (int i = 0; i < selected.Count; i++)
						{
							Pawn pawn = selected[i] as Pawn;
							if (pawn != null)
							{
								MassTakeFirstAutoTakeableOption(pawn, UI.MouseCell());
							}
						}
					}
					Event.current.Use();
				}
			}
			if (Event.current.rawType == EventType.MouseUp)
			{
				if (Event.current.button == 0 && dragBox.active)
				{
					dragBox.active = false;
					if (!dragBox.IsValid)
					{
						SelectUnderMouse();
					}
					else
					{
						SelectInsideDragBox();
					}
				}
				Event.current.Use();
			}
		}

		public bool IsSelected(object obj)
		{
			return selected.Contains(obj);
		}

		public void ClearSelection()
		{
			SelectionDrawer.Clear();
			selected.Clear();
		}

		public void Deselect(object obj)
		{
			if (selected.Contains(obj))
			{
				selected.Remove(obj);
			}
		}

		public void Select(object obj, bool playSound = true, bool forceDesignatorDeselect = true)
		{
			if (obj == null)
			{
				Log.Error("Cannot select null.");
			}
			else
			{
				Thing thing = obj as Thing;
				if (thing == null && !(obj is Zone))
				{
					Log.Error("Tried to select " + obj + " which is neither a Thing nor a Zone.");
				}
				else if (thing != null && thing.Destroyed)
				{
					Log.Error("Cannot select destroyed thing.");
				}
				else
				{
					Pawn pawn = obj as Pawn;
					if (pawn != null && pawn.IsWorldPawn())
					{
						Log.Error("Cannot select world pawns.");
					}
					else
					{
						if (forceDesignatorDeselect)
						{
							Find.DesignatorManager.Deselect();
						}
						if (SelectedZone != null && !(obj is Zone))
						{
							ClearSelection();
						}
						if (obj is Zone && SelectedZone == null)
						{
							ClearSelection();
						}
						Map map = (thing == null) ? ((Zone)obj).Map : thing.Map;
						for (int num = selected.Count - 1; num >= 0; num--)
						{
							Thing thing2 = selected[num] as Thing;
							Map map2 = (thing2 == null) ? ((Zone)selected[num]).Map : thing2.Map;
							if (map2 != map)
							{
								Deselect(selected[num]);
							}
						}
						if (selected.Count < 80 && !IsSelected(obj))
						{
							if (map != Find.CurrentMap)
							{
								Current.Game.CurrentMap = map;
								SoundDefOf.MapSelected.PlayOneShotOnCamera();
								IntVec3 cell = thing?.Position ?? ((Zone)obj).Cells[0];
								Find.CameraDriver.JumpToCurrentMapLoc(cell);
							}
							if (playSound)
							{
								PlaySelectionSoundFor(obj);
							}
							selected.Add(obj);
							SelectionDrawer.Notify_Selected(obj);
						}
					}
				}
			}
		}

		public void Notify_DialogOpened()
		{
			dragBox.active = false;
		}

		private void PlaySelectionSoundFor(object obj)
		{
			if (obj is Pawn && ((Pawn)obj).Faction == Faction.OfPlayer && ((Pawn)obj).RaceProps.Humanlike)
			{
				SoundDefOf.ColonistSelected.PlayOneShotOnCamera();
			}
			else if (obj is Thing)
			{
				SoundDefOf.ThingSelected.PlayOneShotOnCamera();
			}
			else if (obj is Zone)
			{
				SoundDefOf.ZoneSelected.PlayOneShotOnCamera();
			}
			else
			{
				Log.Warning("Can't determine selection sound for " + obj);
			}
		}

		private void SelectInsideDragBox()
		{
			if (!ShiftIsHeld)
			{
				ClearSelection();
			}
			bool selectedSomething = false;
			List<Thing> list = Find.ColonistBar.MapColonistsOrCorpsesInScreenRect(dragBox.ScreenRect);
			for (int i = 0; i < list.Count; i++)
			{
				selectedSomething = true;
				Select(list[i]);
			}
			if (!selectedSomething)
			{
				List<Caravan> list2 = Find.ColonistBar.CaravanMembersCaravansInScreenRect(dragBox.ScreenRect);
				for (int j = 0; j < list2.Count; j++)
				{
					if (!selectedSomething)
					{
						CameraJumper.TryJumpAndSelect(list2[j]);
						selectedSomething = true;
					}
					else
					{
						Find.WorldSelector.Select(list2[j]);
					}
				}
				if (!selectedSomething)
				{
					List<Thing> boxThings = ThingSelectionUtility.MultiSelectableThingsInScreenRectDistinct(dragBox.ScreenRect).ToList();
					Func<Predicate<Thing>, bool> func = delegate(Predicate<Thing> predicate)
					{
						foreach (Thing item in from t in boxThings
						where predicate(t)
						select t)
						{
							Select(item);
							selectedSomething = true;
						}
						return selectedSomething;
					};
					Predicate<Thing> arg = (Thing t) => t.def.category == ThingCategory.Pawn && ((Pawn)t).RaceProps.Humanlike && t.Faction == Faction.OfPlayer;
					if (!func(arg))
					{
						Predicate<Thing> arg2 = (Thing t) => t.def.category == ThingCategory.Pawn && ((Pawn)t).RaceProps.Humanlike;
						if (!func(arg2))
						{
							Predicate<Thing> arg3 = (Thing t) => t.def.CountAsResource;
							if (!func(arg3))
							{
								Predicate<Thing> arg4 = (Thing t) => t.def.category == ThingCategory.Pawn;
								if (!func(arg4) && !func((Thing t) => t.def.selectable))
								{
									List<Zone> list3 = ThingSelectionUtility.MultiSelectableZonesInScreenRectDistinct(dragBox.ScreenRect).ToList();
									foreach (Zone item2 in list3)
									{
										selectedSomething = true;
										Select(item2);
									}
									if (!selectedSomething)
									{
										SelectUnderMouse();
									}
								}
							}
						}
					}
				}
			}
		}

		private IEnumerable<object> SelectableObjectsUnderMouse()
		{
			Vector2 mousePos = UI.MousePositionOnUIInverted;
			Thing colonistOrCorpse = Find.ColonistBar.ColonistOrCorpseAt(mousePos);
			if (colonistOrCorpse != null && colonistOrCorpse.Spawned)
			{
				yield return (object)colonistOrCorpse;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (UI.MouseCell().InBounds(Find.CurrentMap))
			{
				List<Thing> selectableList = GenUI.ThingsUnderMouse(clickParams: new TargetingParameters
				{
					mustBeSelectable = true,
					canTargetPawns = true,
					canTargetBuildings = true,
					canTargetItems = true,
					mapObjectTargetsMustBeAutoAttackable = false
				}, clickPos: UI.MouseMapPosition(), pawnWideClickRadius: 1f);
				if (selectableList.Count > 0 && selectableList[0] is Pawn && (selectableList[0].DrawPos - UI.MouseMapPosition()).MagnitudeHorizontal() < 0.4f)
				{
					for (int num = selectableList.Count - 1; num >= 0; num--)
					{
						Thing thing = selectableList[num];
						if (thing.def.category == ThingCategory.Pawn && (thing.DrawPos - UI.MouseMapPosition()).MagnitudeHorizontal() > 0.4f)
						{
							selectableList.Remove(thing);
						}
					}
				}
				int i = 0;
				if (i < selectableList.Count)
				{
					yield return (object)selectableList[i];
					/*Error: Unable to find new state assignment for yield return*/;
				}
				Zone z = Find.CurrentMap.zoneManager.ZoneAt(UI.MouseCell());
				if (z != null)
				{
					yield return (object)z;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		public static IEnumerable<object> SelectableObjectsAt(IntVec3 c, Map map)
		{
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing t = thingList[i];
				if (ThingSelectionUtility.SelectableByMapClick(t))
				{
					yield return (object)t;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			Zone z = map.zoneManager.ZoneAt(c);
			if (z != null)
			{
				yield return (object)z;
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		private void SelectUnderMouse()
		{
			Caravan caravan = Find.ColonistBar.CaravanMemberCaravanAt(UI.MousePositionOnUIInverted);
			if (caravan != null)
			{
				CameraJumper.TryJumpAndSelect(caravan);
			}
			else
			{
				Thing thing = Find.ColonistBar.ColonistOrCorpseAt(UI.MousePositionOnUIInverted);
				if (thing != null && !thing.Spawned)
				{
					CameraJumper.TryJump(thing);
				}
				else
				{
					List<object> list = SelectableObjectsUnderMouse().ToList();
					if (list.Count == 0)
					{
						if (!ShiftIsHeld)
						{
							ClearSelection();
						}
					}
					else if (list.Count == 1)
					{
						object obj2 = list[0];
						if (!ShiftIsHeld)
						{
							ClearSelection();
							Select(obj2);
						}
						else if (!selected.Contains(obj2))
						{
							Select(obj2);
						}
						else
						{
							Deselect(obj2);
						}
					}
					else if (list.Count > 1)
					{
						object obj3 = (from obj in list
						where selected.Contains(obj)
						select obj).FirstOrDefault();
						if (obj3 != null)
						{
							if (!ShiftIsHeld)
							{
								int num = list.IndexOf(obj3) + 1;
								if (num >= list.Count)
								{
									num -= list.Count;
								}
								ClearSelection();
								Select(list[num]);
							}
							else
							{
								foreach (object item in list)
								{
									if (selected.Contains(item))
									{
										Deselect(item);
									}
								}
							}
						}
						else
						{
							if (!ShiftIsHeld)
							{
								ClearSelection();
							}
							Select(list[0]);
						}
					}
				}
			}
		}

		public void SelectNextAt(IntVec3 c, Map map)
		{
			if (SelectedObjects.Count() != 1)
			{
				Log.Error("Cannot select next at with < or > 1 selected.");
			}
			else
			{
				List<object> list = SelectableObjectsAt(c, map).ToList();
				int num = list.IndexOf(SingleSelectedThing) + 1;
				if (num >= list.Count)
				{
					num -= list.Count;
				}
				ClearSelection();
				Select(list[num]);
			}
		}

		private void SelectAllMatchingObjectUnderMouseOnScreen()
		{
			List<object> list = SelectableObjectsUnderMouse().ToList();
			if (list.Count != 0)
			{
				Thing clickedThing = list.FirstOrDefault((object o) => o is Pawn && ((Pawn)o).Faction == Faction.OfPlayer && !((Pawn)o).IsPrisoner) as Thing;
				clickedThing = (list.FirstOrDefault((object o) => o is Pawn) as Thing);
				if (clickedThing == null)
				{
					clickedThing = ((from o in list
					where o is Thing && !((Thing)o).def.neverMultiSelect
					select o).FirstOrDefault() as Thing);
				}
				Rect rect = new Rect(0f, 0f, (float)UI.screenWidth, (float)UI.screenHeight);
				if (clickedThing == null)
				{
					object obj = list.FirstOrDefault((object o) => o is Zone && ((Zone)o).IsMultiselectable);
					if (obj != null)
					{
						IEnumerable<Zone> enumerable = ThingSelectionUtility.MultiSelectableZonesInScreenRectDistinct(rect);
						foreach (Zone item in enumerable)
						{
							if (!IsSelected(item))
							{
								Select(item);
							}
						}
					}
				}
				else
				{
					IEnumerable enumerable2 = ThingSelectionUtility.MultiSelectableThingsInScreenRectDistinct(rect);
					Predicate<Thing> predicate = delegate(Thing t)
					{
						if (t.def != clickedThing.def || t.Faction != clickedThing.Faction || IsSelected(t))
						{
							return false;
						}
						Pawn pawn = clickedThing as Pawn;
						if (pawn != null)
						{
							Pawn pawn2 = t as Pawn;
							if (pawn2.RaceProps != pawn.RaceProps)
							{
								return false;
							}
							if (pawn2.HostFaction != pawn.HostFaction)
							{
								return false;
							}
						}
						return true;
					};
					IEnumerator enumerator2 = enumerable2.GetEnumerator();
					try
					{
						while (enumerator2.MoveNext())
						{
							Thing obj2 = (Thing)enumerator2.Current;
							if (predicate(obj2))
							{
								Select(obj2);
							}
						}
					}
					finally
					{
						IDisposable disposable;
						if ((disposable = (enumerator2 as IDisposable)) != null)
						{
							disposable.Dispose();
						}
					}
				}
			}
		}

		private static void MassTakeFirstAutoTakeableOption(Pawn pawn, IntVec3 dest)
		{
			FloatMenuOption floatMenuOption = null;
			foreach (FloatMenuOption item in FloatMenuMakerMap.ChoicesAtFor(dest.ToVector3Shifted(), pawn))
			{
				if (!item.Disabled && item.autoTakeable && (floatMenuOption == null || item.autoTakeablePriority > floatMenuOption.autoTakeablePriority))
				{
					floatMenuOption = item;
				}
			}
			floatMenuOption?.Chosen(colonistOrdering: true, null);
		}
	}
}
