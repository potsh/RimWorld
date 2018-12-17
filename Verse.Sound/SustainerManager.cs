using System.Collections.Generic;
using System.Linq;

namespace Verse.Sound
{
	public class SustainerManager
	{
		private List<Sustainer> allSustainers = new List<Sustainer>();

		private static Dictionary<SoundDef, List<Sustainer>> playingPerDef = new Dictionary<SoundDef, List<Sustainer>>();

		public List<Sustainer> AllSustainers => allSustainers;

		public void RegisterSustainer(Sustainer newSustainer)
		{
			allSustainers.Add(newSustainer);
		}

		public void DeregisterSustainer(Sustainer oldSustainer)
		{
			allSustainers.Remove(oldSustainer);
		}

		public bool SustainerExists(SoundDef def)
		{
			for (int i = 0; i < allSustainers.Count; i++)
			{
				if (allSustainers[i].def == def)
				{
					return true;
				}
			}
			return false;
		}

		public void SustainerManagerUpdate()
		{
			for (int num = allSustainers.Count - 1; num >= 0; num--)
			{
				allSustainers[num].SustainerUpdate();
			}
			UpdateAllSustainerScopes();
		}

		public void UpdateAllSustainerScopes()
		{
			for (int i = 0; i < allSustainers.Count; i++)
			{
				Sustainer sustainer = allSustainers[i];
				if (!playingPerDef.ContainsKey(sustainer.def))
				{
					List<Sustainer> list = SimplePool<List<Sustainer>>.Get();
					list.Add(sustainer);
					playingPerDef.Add(sustainer.def, list);
				}
				else
				{
					playingPerDef[sustainer.def].Add(sustainer);
				}
			}
			foreach (KeyValuePair<SoundDef, List<Sustainer>> item in playingPerDef)
			{
				SoundDef key = item.Key;
				List<Sustainer> value = item.Value;
				int num = value.Count - key.maxVoices;
				if (num < 0)
				{
					for (int j = 0; j < value.Count; j++)
					{
						value[j].scopeFader.inScope = true;
					}
				}
				else
				{
					for (int k = 0; k < value.Count; k++)
					{
						value[k].scopeFader.inScope = false;
					}
					int num2 = 0;
					foreach (Sustainer item2 in from lo in value
					orderby lo.CameraDistanceSquared
					select lo)
					{
						item2.scopeFader.inScope = true;
						num2++;
						if (num2 >= key.maxVoices)
						{
							break;
						}
					}
					for (int l = 0; l < value.Count; l++)
					{
						if (!value[l].scopeFader.inScope)
						{
							value[l].scopeFader.inScopePercent = 0f;
						}
					}
				}
			}
			foreach (KeyValuePair<SoundDef, List<Sustainer>> item3 in playingPerDef)
			{
				item3.Value.Clear();
				SimplePool<List<Sustainer>>.Return(item3.Value);
			}
			playingPerDef.Clear();
		}

		public void EndAllInMap(Map map)
		{
			for (int num = allSustainers.Count - 1; num >= 0; num--)
			{
				if (allSustainers[num].info.Maker.Map == map)
				{
					allSustainers[num].End();
				}
			}
		}
	}
}
