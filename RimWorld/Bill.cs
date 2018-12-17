using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public abstract class Bill : IExposable, ILoadReferenceable
	{
		[Unsaved]
		public BillStack billStack;

		private int loadID = -1;

		public RecipeDef recipe;

		public bool suspended;

		public ThingFilter ingredientFilter;

		public float ingredientSearchRadius = 999f;

		public IntRange allowedSkillRange = new IntRange(0, 20);

		public Pawn pawnRestriction;

		public bool deleted;

		public int lastIngredientSearchFailTicks = -99999;

		public const int MaxIngredientSearchRadius = 999;

		public const float ButSize = 24f;

		private const float InterfaceBaseHeight = 53f;

		private const float InterfaceStatusLineHeight = 17f;

		public Map Map => billStack.billGiver.Map;

		public virtual string Label => recipe.label;

		public virtual string LabelCap => Label.CapitalizeFirst();

		public virtual bool CheckIngredientsIfSociallyProper => true;

		public virtual bool CompletableEver => true;

		protected virtual string StatusString => null;

		protected virtual float StatusLineMinHeight => 0f;

		protected virtual bool CanCopy => true;

		public bool DeletedOrDereferenced
		{
			get
			{
				if (deleted)
				{
					return true;
				}
				Thing thing = billStack.billGiver as Thing;
				if (thing != null && thing.Destroyed)
				{
					return true;
				}
				return false;
			}
		}

		public Bill()
		{
		}

		public Bill(RecipeDef recipe)
		{
			this.recipe = recipe;
			ingredientFilter = new ThingFilter();
			ingredientFilter.CopyAllowancesFrom(recipe.defaultIngredientFilter);
			InitializeAfterClone();
		}

		public void InitializeAfterClone()
		{
			loadID = Find.UniqueIDsManager.GetNextBillID();
		}

		public virtual void ExposeData()
		{
			Scribe_Values.Look(ref loadID, "loadID", 0);
			Scribe_Defs.Look(ref recipe, "recipe");
			Scribe_Values.Look(ref suspended, "suspended", defaultValue: false);
			Scribe_Values.Look(ref ingredientSearchRadius, "ingredientSearchRadius", 999f);
			Scribe_Values.Look(ref allowedSkillRange, "allowedSkillRange");
			Scribe_References.Look(ref pawnRestriction, "pawnRestriction");
			if (Scribe.mode == LoadSaveMode.Saving && recipe.fixedIngredientFilter != null)
			{
				foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
				{
					if (!recipe.fixedIngredientFilter.Allows(allDef))
					{
						ingredientFilter.SetAllow(allDef, allow: false);
					}
				}
			}
			Scribe_Deep.Look(ref ingredientFilter, "ingredientFilter");
		}

		public virtual bool PawnAllowedToStartAnew(Pawn p)
		{
			if (pawnRestriction != null)
			{
				return pawnRestriction == p;
			}
			if (recipe.workSkill != null)
			{
				int level = p.skills.GetSkill(recipe.workSkill).Level;
				if (level < allowedSkillRange.min)
				{
					JobFailReason.Is("UnderAllowedSkill".Translate(allowedSkillRange.min), Label);
					return false;
				}
				if (level > allowedSkillRange.max)
				{
					JobFailReason.Is("AboveAllowedSkill".Translate(allowedSkillRange.max), Label);
					return false;
				}
			}
			return true;
		}

		public virtual void Notify_PawnDidWork(Pawn p)
		{
		}

		public virtual void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
		{
		}

		public abstract bool ShouldDoNow();

		public virtual void Notify_DoBillStarted(Pawn billDoer)
		{
		}

		protected virtual void DoConfigInterface(Rect rect, Color baseColor)
		{
			rect.yMin += 29f;
			Vector2 center = rect.center;
			float y = center.y;
			float num = rect.xMax - (rect.yMax - y);
			Widgets.InfoCardButton(num - 12f, y - 12f, recipe);
		}

		public virtual void DoStatusLineInterface(Rect rect)
		{
		}

		public Rect DoInterface(float x, float y, float width, int index)
		{
			Rect rect = new Rect(x, y, width, 53f);
			float num = 0f;
			if (!StatusString.NullOrEmpty())
			{
				num = Mathf.Max(17f, StatusLineMinHeight);
			}
			rect.height += num;
			Color color = Color.white;
			if (!ShouldDoNow())
			{
				color = new Color(1f, 0.7f, 0.7f, 0.7f);
			}
			GUI.color = color;
			Text.Font = GameFont.Small;
			if (index % 2 == 0)
			{
				Widgets.DrawAltRect(rect);
			}
			GUI.BeginGroup(rect);
			Rect rect2 = new Rect(0f, 0f, 24f, 24f);
			if (billStack.IndexOf(this) > 0)
			{
				if (Widgets.ButtonImage(rect2, TexButton.ReorderUp, color))
				{
					billStack.Reorder(this, -1);
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
				}
				TooltipHandler.TipRegion(rect2, "ReorderBillUpTip".Translate());
			}
			if (billStack.IndexOf(this) < billStack.Count - 1)
			{
				Rect rect3 = new Rect(0f, 24f, 24f, 24f);
				if (Widgets.ButtonImage(rect3, TexButton.ReorderDown, color))
				{
					billStack.Reorder(this, 1);
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				}
				TooltipHandler.TipRegion(rect3, "ReorderBillDownTip".Translate());
			}
			Rect rect4 = new Rect(28f, 0f, rect.width - 48f - 20f, rect.height + 5f);
			Widgets.Label(rect4, LabelCap);
			DoConfigInterface(rect.AtZero(), color);
			Rect rect5 = new Rect(rect.width - 24f, 0f, 24f, 24f);
			if (Widgets.ButtonImage(rect5, TexButton.DeleteX, color, color * GenUI.SubtleMouseoverColor))
			{
				billStack.Delete(this);
				SoundDefOf.Click.PlayOneShotOnCamera();
			}
			TooltipHandler.TipRegion(rect5, "DeleteBillTip".Translate());
			Rect rect6;
			if (!CanCopy)
			{
				rect6 = new Rect(rect5);
			}
			else
			{
				Rect rect7 = new Rect(rect5);
				rect7.x -= rect7.width + 4f;
				if (Widgets.ButtonImageFitted(rect7, TexButton.Copy, color))
				{
					BillUtility.Clipboard = Clone();
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
				}
				TooltipHandler.TipRegion(rect7, "CopyBillTip".Translate());
				rect6 = new Rect(rect7);
			}
			rect6.x -= rect6.width + 4f;
			if (Widgets.ButtonImage(rect6, TexButton.Suspend, color))
			{
				suspended = !suspended;
				SoundDefOf.Click.PlayOneShotOnCamera();
			}
			TooltipHandler.TipRegion(rect6, "SuspendBillTip".Translate());
			if (!StatusString.NullOrEmpty())
			{
				Text.Font = GameFont.Tiny;
				Rect rect8 = new Rect(24f, rect.height - num, rect.width - 24f, num);
				Widgets.Label(rect8, StatusString);
				DoStatusLineInterface(rect8);
			}
			GUI.EndGroup();
			if (suspended)
			{
				Text.Font = GameFont.Medium;
				Text.Anchor = TextAnchor.MiddleCenter;
				Rect rect9 = new Rect(rect.x + rect.width / 2f - 70f, rect.y + rect.height / 2f - 20f, 140f, 40f);
				GUI.DrawTexture(rect9, TexUI.GrayTextBG);
				Widgets.Label(rect9, "SuspendedCaps".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				Text.Font = GameFont.Small;
			}
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			return rect;
		}

		public bool IsFixedOrAllowedIngredient(Thing thing)
		{
			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				IngredientCount ingredientCount = recipe.ingredients[i];
				if (ingredientCount.IsFixedIngredient && ingredientCount.filter.Allows(thing))
				{
					return true;
				}
			}
			return recipe.fixedIngredientFilter.Allows(thing) && ingredientFilter.Allows(thing);
		}

		public bool IsFixedOrAllowedIngredient(ThingDef def)
		{
			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				IngredientCount ingredientCount = recipe.ingredients[i];
				if (ingredientCount.IsFixedIngredient && ingredientCount.filter.Allows(def))
				{
					return true;
				}
			}
			return recipe.fixedIngredientFilter.Allows(def) && ingredientFilter.Allows(def);
		}

		public static void CreateNoPawnsWithSkillDialog(RecipeDef recipe)
		{
			string str = "RecipeRequiresSkills".Translate(recipe.LabelCap);
			str += "\n\n";
			str += recipe.MinSkillString;
			Find.WindowStack.Add(new Dialog_MessageBox(str));
		}

		public virtual BillStoreModeDef GetStoreMode()
		{
			return BillStoreModeDefOf.BestStockpile;
		}

		public virtual Zone_Stockpile GetStoreZone()
		{
			return null;
		}

		public virtual void SetStoreMode(BillStoreModeDef mode, Zone_Stockpile zone = null)
		{
			Log.ErrorOnce("Tried to set store mode of a non-production bill", 27190980);
		}

		public virtual Bill Clone()
		{
			Bill bill = (Bill)Activator.CreateInstance(GetType());
			bill.recipe = recipe;
			bill.suspended = suspended;
			bill.ingredientFilter = new ThingFilter();
			bill.ingredientFilter.CopyAllowancesFrom(ingredientFilter);
			bill.ingredientSearchRadius = ingredientSearchRadius;
			bill.allowedSkillRange = allowedSkillRange;
			bill.pawnRestriction = pawnRestriction;
			return bill;
		}

		public virtual void ValidateSettings()
		{
			if (pawnRestriction != null && (pawnRestriction.Dead || pawnRestriction.Faction != Faction.OfPlayer || pawnRestriction.IsKidnapped()))
			{
				if (this != BillUtility.Clipboard)
				{
					Messages.Message("MessageBillValidationPawnUnavailable".Translate(pawnRestriction.LabelShortCap, LabelCap, billStack.billGiver.LabelShort.CapitalizeFirst()), billStack.billGiver as Thing, MessageTypeDefOf.NegativeEvent);
				}
				pawnRestriction = null;
			}
		}

		public string GetUniqueLoadID()
		{
			return "Bill_" + recipe.defName + "_" + loadID;
		}

		public override string ToString()
		{
			return GetUniqueLoadID();
		}
	}
}
