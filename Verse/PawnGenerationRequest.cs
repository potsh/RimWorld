using RimWorld;
using System;
using System.Runtime.InteropServices;

namespace Verse
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct PawnGenerationRequest
	{
		public PawnKindDef KindDef
		{
			get;
			private set;
		}

		public PawnGenerationContext Context
		{
			get;
			private set;
		}

		public Faction Faction
		{
			get;
			private set;
		}

		public int Tile
		{
			get;
			private set;
		}

		public bool ForceGenerateNewPawn
		{
			get;
			private set;
		}

		public bool Newborn
		{
			get;
			private set;
		}

		public bool AllowDead
		{
			get;
			private set;
		}

		public bool AllowDowned
		{
			get;
			private set;
		}

		public bool CanGeneratePawnRelations
		{
			get;
			private set;
		}

		public bool MustBeCapableOfViolence
		{
			get;
			private set;
		}

		public float ColonistRelationChanceFactor
		{
			get;
			private set;
		}

		public bool ForceAddFreeWarmLayerIfNeeded
		{
			get;
			private set;
		}

		public bool AllowGay
		{
			get;
			private set;
		}

		public bool AllowFood
		{
			get;
			private set;
		}

		public bool Inhabitant
		{
			get;
			private set;
		}

		public bool CertainlyBeenInCryptosleep
		{
			get;
			private set;
		}

		public bool ForceRedressWorldPawnIfFormerColonist
		{
			get;
			private set;
		}

		public bool WorldPawnFactionDoesntMatter
		{
			get;
			private set;
		}

		public Predicate<Pawn> ValidatorPreGear
		{
			get;
			private set;
		}

		public Predicate<Pawn> ValidatorPostGear
		{
			get;
			private set;
		}

		public float? MinChanceToRedressWorldPawn
		{
			get;
			private set;
		}

		public float? FixedBiologicalAge
		{
			get;
			private set;
		}

		public float? FixedChronologicalAge
		{
			get;
			private set;
		}

		public Gender? FixedGender
		{
			get;
			private set;
		}

		public float? FixedMelanin
		{
			get;
			private set;
		}

		public string FixedLastName
		{
			get;
			private set;
		}

		public PawnGenerationRequest(PawnKindDef kind, Faction faction = null, PawnGenerationContext context = PawnGenerationContext.NonPlayer, int tile = -1, bool forceGenerateNewPawn = false, bool newborn = false, bool allowDead = false, bool allowDowned = false, bool canGeneratePawnRelations = true, bool mustBeCapableOfViolence = false, float colonistRelationChanceFactor = 1f, bool forceAddFreeWarmLayerIfNeeded = false, bool allowGay = true, bool allowFood = true, bool inhabitant = false, bool certainlyBeenInCryptosleep = false, bool forceRedressWorldPawnIfFormerColonist = false, bool worldPawnFactionDoesntMatter = false, Predicate<Pawn> validatorPreGear = null, Predicate<Pawn> validatorPostGear = null, float? minChanceToRedressWorldPawn = default(float?), float? fixedBiologicalAge = default(float?), float? fixedChronologicalAge = default(float?), Gender? fixedGender = default(Gender?), float? fixedMelanin = default(float?), string fixedLastName = null)
		{
			this = default(PawnGenerationRequest);
			if (context == PawnGenerationContext.All)
			{
				Log.Error("Should not generate pawns with context 'All'");
				context = PawnGenerationContext.NonPlayer;
			}
			if (inhabitant && (tile == -1 || Current.Game.FindMap(tile) == null))
			{
				Log.Error("Trying to generate an inhabitant but map is null.");
				inhabitant = false;
			}
			KindDef = kind;
			Context = context;
			Faction = faction;
			Tile = tile;
			ForceGenerateNewPawn = forceGenerateNewPawn;
			Newborn = newborn;
			AllowDead = allowDead;
			AllowDowned = allowDowned;
			CanGeneratePawnRelations = canGeneratePawnRelations;
			MustBeCapableOfViolence = mustBeCapableOfViolence;
			ColonistRelationChanceFactor = colonistRelationChanceFactor;
			ForceAddFreeWarmLayerIfNeeded = forceAddFreeWarmLayerIfNeeded;
			AllowGay = allowGay;
			AllowFood = allowFood;
			Inhabitant = inhabitant;
			CertainlyBeenInCryptosleep = certainlyBeenInCryptosleep;
			ForceRedressWorldPawnIfFormerColonist = forceRedressWorldPawnIfFormerColonist;
			WorldPawnFactionDoesntMatter = worldPawnFactionDoesntMatter;
			ValidatorPreGear = validatorPreGear;
			ValidatorPostGear = validatorPostGear;
			MinChanceToRedressWorldPawn = minChanceToRedressWorldPawn;
			FixedBiologicalAge = fixedBiologicalAge;
			FixedChronologicalAge = fixedChronologicalAge;
			FixedGender = fixedGender;
			FixedMelanin = fixedMelanin;
			FixedLastName = fixedLastName;
		}

		public void SetFixedLastName(string fixedLastName)
		{
			if (FixedLastName != null)
			{
				Log.Error("Last name is already a fixed value: " + FixedLastName + ".");
			}
			else
			{
				FixedLastName = fixedLastName;
			}
		}

		public void SetFixedMelanin(float fixedMelanin)
		{
			if (FixedMelanin.HasValue)
			{
				Log.Error("Melanin is already a fixed value: " + FixedMelanin + ".");
			}
			else
			{
				FixedMelanin = fixedMelanin;
			}
		}

		public override string ToString()
		{
			return "kindDef=" + KindDef + ", context=" + Context + ", faction=" + Faction + ", tile=" + Tile + ", forceGenerateNewPawn=" + ForceGenerateNewPawn + ", newborn=" + Newborn + ", allowDead=" + AllowDead + ", allowDowned=" + AllowDowned + ", canGeneratePawnRelations=" + CanGeneratePawnRelations + ", mustBeCapableOfViolence=" + MustBeCapableOfViolence + ", colonistRelationChanceFactor=" + ColonistRelationChanceFactor + ", forceAddFreeWarmLayerIfNeeded=" + ForceAddFreeWarmLayerIfNeeded + ", allowGay=" + AllowGay + ", allowFood=" + AllowFood + ", inhabitant=" + Inhabitant + ", certainlyBeenInCryptosleep=" + CertainlyBeenInCryptosleep + ", validatorPreGear=" + ValidatorPreGear + ", validatorPostGear=" + ValidatorPostGear + ", fixedBiologicalAge=" + FixedBiologicalAge + ", fixedChronologicalAge=" + FixedChronologicalAge + ", fixedGender=" + FixedGender + ", fixedMelanin=" + FixedMelanin + ", fixedLastName=" + FixedLastName;
		}
	}
}
