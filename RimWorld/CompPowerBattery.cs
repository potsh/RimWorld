using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompPowerBattery : CompPower
	{
		private float storedEnergy;

		private const float SelfDischargingWatts = 5f;

		public float AmountCanAccept
		{
			get
			{
				if (parent.IsBrokenDown())
				{
					return 0f;
				}
				CompProperties_Battery props = Props;
				return (props.storedEnergyMax - storedEnergy) / props.efficiency;
			}
		}

		public float StoredEnergy => storedEnergy;

		public float StoredEnergyPct => storedEnergy / Props.storedEnergyMax;

		public new CompProperties_Battery Props => (CompProperties_Battery)props;

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref storedEnergy, "storedPower", 0f);
			CompProperties_Battery props = Props;
			if (storedEnergy > props.storedEnergyMax)
			{
				storedEnergy = props.storedEnergyMax;
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			DrawPower(Mathf.Min(5f * CompPower.WattsToWattDaysPerTick, storedEnergy));
		}

		public void AddEnergy(float amount)
		{
			if (amount < 0f)
			{
				Log.Error("Cannot add negative energy " + amount);
			}
			else
			{
				if (amount > AmountCanAccept)
				{
					amount = AmountCanAccept;
				}
				amount *= Props.efficiency;
				storedEnergy += amount;
			}
		}

		public void DrawPower(float amount)
		{
			storedEnergy -= amount;
			if (storedEnergy < 0f)
			{
				Log.Error("Drawing power we don't have from " + parent);
				storedEnergy = 0f;
			}
		}

		public void SetStoredEnergyPct(float pct)
		{
			pct = Mathf.Clamp01(pct);
			storedEnergy = Props.storedEnergyMax * pct;
		}

		public override void ReceiveCompSignal(string signal)
		{
			if (signal == "Breakdown")
			{
				DrawPower(StoredEnergy);
			}
		}

		public override string CompInspectStringExtra()
		{
			CompProperties_Battery props = Props;
			string text = "PowerBatteryStored".Translate() + ": " + storedEnergy.ToString("F0") + " / " + props.storedEnergyMax.ToString("F0") + " Wd";
			string text2 = text;
			text = text2 + "\n" + "PowerBatteryEfficiency".Translate() + ": " + (props.efficiency * 100f).ToString("F0") + "%";
			if (storedEnergy > 0f)
			{
				text2 = text;
				text = text2 + "\n" + "SelfDischarging".Translate() + ": " + 5f.ToString("F0") + " W";
			}
			return text + "\n" + base.CompInspectStringExtra();
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			using (IEnumerator<Gizmo> enumerator = base.CompGetGizmosExtra().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Gizmo c = enumerator.Current;
					yield return c;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (Prefs.DevMode)
			{
				yield return (Gizmo)new Command_Action
				{
					defaultLabel = "DEBUG: Fill",
					action = delegate
					{
						((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_00e3: stateMachine*/)._0024this.SetStoredEnergyPct(1f);
					}
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_016f:
			/*Error near IL_0170: Unexpected return in MoveNext()*/;
		}
	}
}
