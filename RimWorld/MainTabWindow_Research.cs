using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class MainTabWindow_Research : MainTabWindow
	{
		protected ResearchProjectDef selectedProject;

		private bool noBenchWarned;

		private bool requiredByThisFound;

		private Vector2 leftScrollPosition = Vector2.zero;

		private float leftScrollViewHeight;

		private Vector2 rightScrollPosition = default(Vector2);

		private float rightViewWidth;

		private float rightViewHeight;

		private ResearchTabDef curTabInt;

		private bool editMode;

		private ResearchProjectDef draggingTab;

		private const float LeftAreaWidth = 200f;

		private const int ModeSelectButHeight = 40;

		private const float ProjectTitleHeight = 50f;

		private const float ProjectTitleLeftMargin = 0f;

		private const float localPadding = 20f;

		private const int ResearchItemW = 140;

		private const int ResearchItemH = 50;

		private const int ResearchItemPaddingW = 50;

		private const int ResearchItemPaddingH = 50;

		private const float PrereqsLineSpacing = 15f;

		private const int ColumnMaxProjects = 6;

		private static readonly Texture2D ResearchBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 0.85f));

		private static readonly Texture2D ResearchBarBGTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f));

		private static readonly Color FulfilledPrerequisiteColor = Color.green;

		private static readonly Color MissingPrerequisiteColor = Color.red;

		private static readonly Color ProjectWithMissingPrerequisiteLabelColor = Color.gray;

		private static List<Building> tmpAllBuildings = new List<Building>();

		private ResearchTabDef CurTab
		{
			get
			{
				return curTabInt;
			}
			set
			{
				if (value != curTabInt)
				{
					curTabInt = value;
					Vector2 vector = ViewSize(CurTab);
					rightViewWidth = vector.x;
					rightViewHeight = vector.y;
					rightScrollPosition = Vector2.zero;
				}
			}
		}

		public override Vector2 InitialSize
		{
			get
			{
				Vector2 initialSize = base.InitialSize;
				float b = (float)(UI.screenHeight - 35);
				float b2 = Margin + 10f + 32f + 10f + DefDatabase<ResearchTabDef>.AllDefs.Max(delegate(ResearchTabDef tab)
				{
					Vector2 vector = ViewSize(tab);
					return vector.y;
				}) + 10f + 10f + Margin;
				float a = Mathf.Max(initialSize.y, b2);
				initialSize.y = Mathf.Min(a, b);
				return initialSize;
			}
		}

		private Vector2 ViewSize(ResearchTabDef tab)
		{
			List<ResearchProjectDef> allDefsListForReading = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
			float num = 0f;
			float num2 = 0f;
			Text.Font = GameFont.Small;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				ResearchProjectDef researchProjectDef = allDefsListForReading[i];
				if (researchProjectDef.tab == tab)
				{
					Rect rect = new Rect(0f, 0f, 140f, 0f);
					Widgets.LabelCacheHeight(ref rect, GetLabel(researchProjectDef), renderLabel: false);
					num = Mathf.Max(num, PosX(researchProjectDef) + 140f);
					num2 = Mathf.Max(num2, PosY(researchProjectDef) + rect.height);
				}
			}
			return new Vector2(num + 20f, num2 + 20f);
		}

		public override void PreOpen()
		{
			base.PreOpen();
			selectedProject = Find.ResearchManager.currentProj;
			if (CurTab == null)
			{
				if (selectedProject != null)
				{
					CurTab = selectedProject.tab;
				}
				else
				{
					CurTab = ResearchTabDefOf.Main;
				}
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.DoWindowContents(inRect);
			windowRect.width = (float)UI.screenWidth;
			if (!noBenchWarned)
			{
				bool flag = false;
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					if (maps[i].listerBuildings.ColonistsHaveResearchBench())
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					Find.WindowStack.Add(new Dialog_MessageBox("ResearchMenuWithoutBench".Translate()));
				}
				noBenchWarned = true;
			}
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			Rect leftOutRect = new Rect(0f, 0f, 200f, inRect.height);
			Rect rightOutRect = new Rect(leftOutRect.xMax + 10f, 0f, inRect.width - leftOutRect.width - 10f, inRect.height);
			DrawLeftRect(leftOutRect);
			DrawRightRect(rightOutRect);
		}

		private void DrawLeftRect(Rect leftOutRect)
		{
			Rect position = leftOutRect;
			GUI.BeginGroup(position);
			if (selectedProject != null)
			{
				Rect outRect = new Rect(0f, 0f, position.width, 500f);
				Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, leftScrollViewHeight);
				Widgets.BeginScrollView(outRect, ref leftScrollPosition, viewRect);
				float num = 0f;
				Text.Font = GameFont.Medium;
				GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
				Rect rect = new Rect(0f, num, viewRect.width, 50f);
				Widgets.LabelCacheHeight(ref rect, selectedProject.LabelCap);
				GenUI.ResetLabelAlign();
				Text.Font = GameFont.Small;
				num += rect.height;
				Rect rect2 = new Rect(0f, num, viewRect.width, 0f);
				Widgets.LabelCacheHeight(ref rect2, selectedProject.description);
				num += rect2.height + 10f;
				string text = "ProjectTechLevel".Translate().CapitalizeFirst() + ": " + selectedProject.techLevel.ToStringHuman().CapitalizeFirst() + "\n" + "YourTechLevel".Translate().CapitalizeFirst() + ": " + Faction.OfPlayer.def.techLevel.ToStringHuman().CapitalizeFirst();
				float num2 = selectedProject.CostFactor(Faction.OfPlayer.def.techLevel);
				if (num2 != 1f)
				{
					string text2 = text;
					text = text2 + "\n\n" + "ResearchCostMultiplier".Translate().CapitalizeFirst() + ": " + num2.ToStringPercent() + "\n" + "ResearchCostComparison".Translate(selectedProject.baseCost.ToString("F0"), selectedProject.CostApparent.ToString("F0"));
				}
				Rect rect3 = new Rect(0f, num, viewRect.width, 0f);
				Widgets.LabelCacheHeight(ref rect3, text);
				num = rect3.yMax + 10f;
				float num3 = DrawResearchPrereqs(rect: new Rect(0f, num, viewRect.width, 500f), project: selectedProject);
				if (num3 > 0f)
				{
					num += num3 + 15f;
				}
				num += DrawResearchBenchRequirements(rect: new Rect(0f, num, viewRect.width, 500f), project: selectedProject);
				num = (leftScrollViewHeight = num + 3f);
				Widgets.EndScrollView();
				bool flag = Prefs.DevMode && selectedProject != Find.ResearchManager.currentProj && !selectedProject.IsFinished;
				Rect rect6 = new Rect(0f, 0f, 90f, 50f);
				if (flag)
				{
					rect6.x = (outRect.width - (rect6.width * 2f + 20f)) / 2f;
				}
				else
				{
					rect6.x = (outRect.width - rect6.width) / 2f;
				}
				rect6.y = outRect.y + outRect.height + 20f;
				if (selectedProject.IsFinished)
				{
					Widgets.DrawMenuSection(rect6);
					Text.Anchor = TextAnchor.MiddleCenter;
					Widgets.Label(rect6, "Finished".Translate());
					Text.Anchor = TextAnchor.UpperLeft;
				}
				else if (selectedProject == Find.ResearchManager.currentProj)
				{
					Widgets.DrawMenuSection(rect6);
					Text.Anchor = TextAnchor.MiddleCenter;
					Widgets.Label(rect6, "InProgress".Translate());
					Text.Anchor = TextAnchor.UpperLeft;
				}
				else if (!selectedProject.CanStartNow)
				{
					Widgets.DrawMenuSection(rect6);
					Text.Anchor = TextAnchor.MiddleCenter;
					Widgets.Label(rect6, "Locked".Translate());
					Text.Anchor = TextAnchor.UpperLeft;
				}
				else if (Widgets.ButtonText(rect6, "Research".Translate()))
				{
					SoundDefOf.ResearchStart.PlayOneShotOnCamera();
					Find.ResearchManager.currentProj = selectedProject;
					TutorSystem.Notify_Event("StartResearchProject");
				}
				if (flag)
				{
					Rect rect7 = rect6;
					rect7.x += rect7.width + 20f;
					if (Widgets.ButtonText(rect7, "Debug Insta-finish"))
					{
						Find.ResearchManager.currentProj = selectedProject;
						Find.ResearchManager.FinishProject(selectedProject);
					}
				}
				Rect rect8 = new Rect(15f, rect6.y + rect6.height + 20f, position.width - 30f, 35f);
				Widgets.FillableBar(rect8, selectedProject.ProgressPercent, ResearchBarFillTex, ResearchBarBGTex, doBorder: true);
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect8, selectedProject.ProgressApparent.ToString("F0") + " / " + selectedProject.CostApparent.ToString("F0"));
				Text.Anchor = TextAnchor.UpperLeft;
			}
			GUI.EndGroup();
		}

		private float CoordToPixelsX(float x)
		{
			return x * 190f;
		}

		private float CoordToPixelsY(float y)
		{
			return y * 100f;
		}

		private float PixelsToCoordX(float x)
		{
			return x / 190f;
		}

		private float PixelsToCoordY(float y)
		{
			return y / 100f;
		}

		private float PosX(ResearchProjectDef d)
		{
			return CoordToPixelsX(d.ResearchViewX);
		}

		private float PosY(ResearchProjectDef d)
		{
			return CoordToPixelsY(d.ResearchViewY);
		}

		private void DrawRightRect(Rect rightOutRect)
		{
			rightOutRect.yMin += 32f;
			Widgets.DrawMenuSection(rightOutRect);
			List<TabRecord> list = new List<TabRecord>();
			foreach (ResearchTabDef allDef in DefDatabase<ResearchTabDef>.AllDefs)
			{
				ResearchTabDef localTabDef = allDef;
				list.Add(new TabRecord(localTabDef.LabelCap, delegate
				{
					CurTab = localTabDef;
				}, CurTab == localTabDef));
			}
			TabDrawer.DrawTabs(rightOutRect, list);
			if (Prefs.DevMode)
			{
				Rect rect = rightOutRect;
				rect.yMax = rect.yMin + 20f;
				rect.xMin = rect.xMax - 80f;
				Rect butRect = rect.RightPartPixels(30f);
				rect = rect.LeftPartPixels(rect.width - 30f);
				Widgets.CheckboxLabeled(rect, "Edit", ref editMode);
				if (Widgets.ButtonImageFitted(butRect, TexButton.Copy))
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (ResearchProjectDef item in from def in DefDatabase<ResearchProjectDef>.AllDefsListForReading
					where def.Debug_IsPositionModified()
					select def)
					{
						stringBuilder.AppendLine(item.defName);
						stringBuilder.AppendLine(string.Format("  <researchViewX>{0}</researchViewX>", item.ResearchViewX.ToString("F2")));
						stringBuilder.AppendLine(string.Format("  <researchViewY>{0}</researchViewY>", item.ResearchViewY.ToString("F2")));
						stringBuilder.AppendLine();
					}
					GUIUtility.systemCopyBuffer = stringBuilder.ToString();
					Messages.Message("Modified data copied to clipboard.", MessageTypeDefOf.SituationResolved, historical: false);
				}
			}
			else
			{
				editMode = false;
			}
			Rect outRect = rightOutRect.ContractedBy(10f);
			Rect rect2 = new Rect(0f, 0f, rightViewWidth, rightViewHeight);
			Rect rect3 = rect2.ContractedBy(10f);
			rect2.width = rightViewWidth;
			rect3 = rect2.ContractedBy(10f);
			Vector2 start = default(Vector2);
			Vector2 end = default(Vector2);
			Widgets.ScrollHorizontal(outRect, ref rightScrollPosition, rect2);
			Widgets.BeginScrollView(outRect, ref rightScrollPosition, rect2);
			GUI.BeginGroup(rect3);
			List<ResearchProjectDef> allDefsListForReading = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < allDefsListForReading.Count; j++)
				{
					ResearchProjectDef researchProjectDef = allDefsListForReading[j];
					if (researchProjectDef.tab == CurTab)
					{
						start.x = PosX(researchProjectDef);
						start.y = PosY(researchProjectDef) + 25f;
						for (int k = 0; k < researchProjectDef.prerequisites.CountAllowNull(); k++)
						{
							ResearchProjectDef researchProjectDef2 = researchProjectDef.prerequisites[k];
							if (researchProjectDef2 != null && researchProjectDef2.tab == CurTab)
							{
								end.x = PosX(researchProjectDef2) + 140f;
								end.y = PosY(researchProjectDef2) + 25f;
								if (selectedProject == researchProjectDef || selectedProject == researchProjectDef2)
								{
									if (i == 1)
									{
										Widgets.DrawLine(start, end, TexUI.HighlightLineResearchColor, 4f);
									}
								}
								else if (i == 0)
								{
									Widgets.DrawLine(start, end, TexUI.DefaultLineResearchColor, 2f);
								}
							}
						}
					}
				}
			}
			for (int l = 0; l < allDefsListForReading.Count; l++)
			{
				ResearchProjectDef researchProjectDef3 = allDefsListForReading[l];
				if (researchProjectDef3.tab == CurTab)
				{
					Rect source = new Rect(PosX(researchProjectDef3), PosY(researchProjectDef3), 140f, 50f);
					string label = GetLabel(researchProjectDef3);
					Rect rect4 = new Rect(source);
					Color textColor = Widgets.NormalOptionColor;
					Color color = default(Color);
					Color color2 = default(Color);
					bool flag = !researchProjectDef3.IsFinished && !researchProjectDef3.CanStartNow;
					if (researchProjectDef3 == Find.ResearchManager.currentProj)
					{
						color = TexUI.ActiveResearchColor;
					}
					else if (researchProjectDef3.IsFinished)
					{
						color = TexUI.FinishedResearchColor;
					}
					else if (flag)
					{
						color = TexUI.LockedResearchColor;
					}
					else if (researchProjectDef3.CanStartNow)
					{
						color = TexUI.AvailResearchColor;
					}
					if (selectedProject == researchProjectDef3)
					{
						color += TexUI.HighlightBgResearchColor;
						color2 = TexUI.HighlightBorderResearchColor;
					}
					else
					{
						color2 = TexUI.DefaultBorderResearchColor;
					}
					if (flag)
					{
						textColor = ProjectWithMissingPrerequisiteLabelColor;
					}
					for (int m = 0; m < researchProjectDef3.prerequisites.CountAllowNull(); m++)
					{
						ResearchProjectDef researchProjectDef4 = researchProjectDef3.prerequisites[m];
						if (researchProjectDef4 != null && selectedProject == researchProjectDef4)
						{
							color2 = TexUI.HighlightLineResearchColor;
						}
					}
					if (requiredByThisFound)
					{
						for (int n = 0; n < researchProjectDef3.requiredByThis.CountAllowNull(); n++)
						{
							ResearchProjectDef researchProjectDef5 = researchProjectDef3.requiredByThis[n];
							if (selectedProject == researchProjectDef5)
							{
								color2 = TexUI.HighlightLineResearchColor;
							}
						}
					}
					if (Widgets.CustomButtonText(ref rect4, label, color, textColor, color2, cacheHeight: true))
					{
						SoundDefOf.Click.PlayOneShotOnCamera();
						selectedProject = researchProjectDef3;
					}
					if (editMode && Mouse.IsOver(rect4) && Input.GetMouseButtonDown(0))
					{
						draggingTab = researchProjectDef3;
					}
				}
			}
			GUI.EndGroup();
			Widgets.EndScrollView();
			if (!Input.GetMouseButton(0))
			{
				draggingTab = null;
			}
			if (draggingTab != null && !Input.GetMouseButtonDown(0) && Event.current.type == EventType.Layout)
			{
				ResearchProjectDef researchProjectDef6 = draggingTab;
				Vector2 delta = Event.current.delta;
				float x = PixelsToCoordX(delta.x);
				Vector2 delta2 = Event.current.delta;
				researchProjectDef6.Debug_ApplyPositionDelta(new Vector2(x, PixelsToCoordY(delta2.y)));
			}
		}

		private float DrawResearchPrereqs(ResearchProjectDef project, Rect rect)
		{
			if (project.prerequisites.NullOrEmpty())
			{
				return 0f;
			}
			float yMin = rect.yMin;
			Widgets.LabelCacheHeight(ref rect, "ResearchPrerequisites".Translate() + ":");
			rect.yMin += rect.height;
			for (int i = 0; i < project.prerequisites.Count; i++)
			{
				SetPrerequisiteStatusColor(project.prerequisites[i].IsFinished, project);
				Widgets.LabelCacheHeight(ref rect, "  " + project.prerequisites[i].LabelCap);
				rect.yMin += rect.height;
			}
			GUI.color = Color.white;
			return rect.yMin - yMin;
		}

		private float DrawResearchBenchRequirements(ResearchProjectDef project, Rect rect)
		{
			float yMin = rect.yMin;
			if (project.requiredResearchBuilding != null)
			{
				bool present = false;
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					if (maps[i].listerBuildings.allBuildingsColonist.Find((Building x) => x.def == project.requiredResearchBuilding) != null)
					{
						present = true;
						break;
					}
				}
				Widgets.LabelCacheHeight(ref rect, "RequiredResearchBench".Translate() + ":");
				rect.yMin += rect.height;
				SetPrerequisiteStatusColor(present, project);
				Widgets.LabelCacheHeight(ref rect, "  " + project.requiredResearchBuilding.LabelCap);
				rect.yMin += rect.height;
				GUI.color = Color.white;
			}
			if (!project.requiredResearchFacilities.NullOrEmpty())
			{
				Widgets.LabelCacheHeight(ref rect, "RequiredResearchBenchFacilities".Translate() + ":");
				rect.yMin += rect.height;
				Building_ResearchBench building_ResearchBench = FindBenchFulfillingMostRequirements(project.requiredResearchBuilding, project.requiredResearchFacilities);
				CompAffectedByFacilities bestMatchingBench = null;
				if (building_ResearchBench != null)
				{
					bestMatchingBench = building_ResearchBench.TryGetComp<CompAffectedByFacilities>();
				}
				for (int j = 0; j < project.requiredResearchFacilities.Count; j++)
				{
					DrawResearchBenchFacilityRequirement(project.requiredResearchFacilities[j], bestMatchingBench, project, ref rect);
					rect.yMin += 15f;
				}
			}
			GUI.color = Color.white;
			return rect.yMin - yMin;
		}

		private string GetLabel(ResearchProjectDef r)
		{
			return r.LabelCap + "\n(" + r.CostApparent.ToString("F0") + ")";
		}

		private void SetPrerequisiteStatusColor(bool present, ResearchProjectDef project)
		{
			if (!project.IsFinished)
			{
				if (present)
				{
					GUI.color = FulfilledPrerequisiteColor;
				}
				else
				{
					GUI.color = MissingPrerequisiteColor;
				}
			}
		}

		private void DrawResearchBenchFacilityRequirement(ThingDef requiredFacility, CompAffectedByFacilities bestMatchingBench, ResearchProjectDef project, ref Rect rect)
		{
			Thing thing = null;
			Thing thing2 = null;
			if (bestMatchingBench != null)
			{
				thing = bestMatchingBench.LinkedFacilitiesListForReading.Find((Thing x) => x.def == requiredFacility);
				thing2 = bestMatchingBench.LinkedFacilitiesListForReading.Find((Thing x) => x.def == requiredFacility && bestMatchingBench.IsFacilityActive(x));
			}
			SetPrerequisiteStatusColor(thing2 != null, project);
			string text = requiredFacility.LabelCap;
			if (thing != null && thing2 == null)
			{
				text = text + " (" + "InactiveFacility".Translate() + ")";
			}
			Widgets.LabelCacheHeight(ref rect, "  " + text);
		}

		private Building_ResearchBench FindBenchFulfillingMostRequirements(ThingDef requiredResearchBench, List<ThingDef> requiredFacilities)
		{
			tmpAllBuildings.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				tmpAllBuildings.AddRange(maps[i].listerBuildings.allBuildingsColonist);
			}
			float num = 0f;
			Building_ResearchBench building_ResearchBench = null;
			for (int j = 0; j < tmpAllBuildings.Count; j++)
			{
				Building_ResearchBench building_ResearchBench2 = tmpAllBuildings[j] as Building_ResearchBench;
				if (building_ResearchBench2 != null && (requiredResearchBench == null || building_ResearchBench2.def == requiredResearchBench))
				{
					float researchBenchRequirementsScore = GetResearchBenchRequirementsScore(building_ResearchBench2, requiredFacilities);
					if (building_ResearchBench == null || researchBenchRequirementsScore > num)
					{
						num = researchBenchRequirementsScore;
						building_ResearchBench = building_ResearchBench2;
					}
				}
			}
			tmpAllBuildings.Clear();
			return building_ResearchBench;
		}

		private float GetResearchBenchRequirementsScore(Building_ResearchBench bench, List<ThingDef> requiredFacilities)
		{
			float num = 0f;
			int i;
			for (i = 0; i < requiredFacilities.Count; i++)
			{
				CompAffectedByFacilities benchComp = bench.GetComp<CompAffectedByFacilities>();
				if (benchComp != null)
				{
					List<Thing> linkedFacilitiesListForReading = benchComp.LinkedFacilitiesListForReading;
					if (linkedFacilitiesListForReading.Find((Thing x) => x.def == requiredFacilities[i] && benchComp.IsFacilityActive(x)) != null)
					{
						num += 1f;
					}
					else if (linkedFacilitiesListForReading.Find((Thing x) => x.def == requiredFacilities[i]) != null)
					{
						num += 0.6f;
					}
				}
			}
			return num;
		}
	}
}
