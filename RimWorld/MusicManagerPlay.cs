using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[HasDebugOutput]
	public class MusicManagerPlay
	{
		private enum MusicManagerState
		{
			Normal,
			Fadeout
		}

		private AudioSource audioSource;

		private MusicManagerState state;

		private float fadeoutFactor = 1f;

		private float nextSongStartTime = 12f;

		private SongDef lastStartedSong;

		private Queue<SongDef> recentSongs = new Queue<SongDef>();

		public bool disabled;

		private SongDef forcedNextSong;

		private bool songWasForced;

		private bool ignorePrefsVolumeThisSong;

		public float subtleAmbienceSoundVolumeMultiplier = 1f;

		private bool gameObjectCreated;

		private static readonly FloatRange SongIntervalRelax = new FloatRange(85f, 105f);

		private static readonly FloatRange SongIntervalTension = new FloatRange(2f, 5f);

		private const float FadeoutDuration = 10f;

		private float CurTime => Time.time;

		private bool DangerMusicMode
		{
			get
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					if (maps[i].dangerWatcher.DangerRating == StoryDanger.High)
					{
						return true;
					}
				}
				return false;
			}
		}

		private float CurVolume
		{
			get
			{
				float num = (!ignorePrefsVolumeThisSong) ? Prefs.VolumeMusic : 1f;
				if (lastStartedSong == null)
				{
					return num;
				}
				return lastStartedSong.volume * num * fadeoutFactor;
			}
		}

		public float CurSanitizedVolume => AudioSourceUtility.GetSanitizedVolume(CurVolume, "MusicManagerPlay");

		public bool IsPlaying => audioSource.isPlaying;

		public void ForceSilenceFor(float time)
		{
			nextSongStartTime = CurTime + time;
		}

		public void MusicUpdate()
		{
			if (!gameObjectCreated)
			{
				gameObjectCreated = true;
				GameObject gameObject = new GameObject("MusicAudioSourceDummy");
				gameObject.transform.parent = Find.Root.soundRoot.sourcePool.sourcePoolCamera.cameraSourcesContainer.transform;
				audioSource = gameObject.AddComponent<AudioSource>();
				audioSource.bypassEffects = true;
				audioSource.bypassListenerEffects = true;
				audioSource.bypassReverbZones = true;
				audioSource.priority = 0;
			}
			UpdateSubtleAmbienceSoundVolumeMultiplier();
			if (!disabled)
			{
				if (songWasForced)
				{
					state = MusicManagerState.Normal;
					fadeoutFactor = 1f;
				}
				if (audioSource.isPlaying && !songWasForced && ((DangerMusicMode && !lastStartedSong.tense) || (!DangerMusicMode && lastStartedSong.tense)))
				{
					state = MusicManagerState.Fadeout;
				}
				audioSource.volume = CurSanitizedVolume;
				if (audioSource.isPlaying)
				{
					if (state == MusicManagerState.Fadeout)
					{
						fadeoutFactor -= Time.deltaTime / 10f;
						if (fadeoutFactor <= 0f)
						{
							audioSource.Stop();
							state = MusicManagerState.Normal;
							fadeoutFactor = 1f;
						}
					}
				}
				else
				{
					if (DangerMusicMode)
					{
						float num = nextSongStartTime;
						float curTime = CurTime;
						FloatRange songIntervalTension = SongIntervalTension;
						if (num > curTime + songIntervalTension.max)
						{
							nextSongStartTime = CurTime + SongIntervalTension.RandomInRange;
						}
					}
					if (nextSongStartTime < CurTime - 5f)
					{
						float num2 = (!DangerMusicMode) ? SongIntervalRelax.RandomInRange : SongIntervalTension.RandomInRange;
						nextSongStartTime = CurTime + num2;
					}
					if (CurTime >= nextSongStartTime)
					{
						ignorePrefsVolumeThisSong = false;
						StartNewSong();
					}
				}
			}
		}

		private void UpdateSubtleAmbienceSoundVolumeMultiplier()
		{
			if (IsPlaying && CurSanitizedVolume > 0.001f)
			{
				subtleAmbienceSoundVolumeMultiplier -= Time.deltaTime * 0.1f;
			}
			else
			{
				subtleAmbienceSoundVolumeMultiplier += Time.deltaTime * 0.1f;
			}
			subtleAmbienceSoundVolumeMultiplier = Mathf.Clamp01(subtleAmbienceSoundVolumeMultiplier);
		}

		private void StartNewSong()
		{
			lastStartedSong = ChooseNextSong();
			audioSource.clip = lastStartedSong.clip;
			audioSource.volume = CurSanitizedVolume;
			audioSource.spatialBlend = 0f;
			audioSource.Play();
			recentSongs.Enqueue(lastStartedSong);
		}

		public void ForceStartSong(SongDef song, bool ignorePrefsVolume)
		{
			forcedNextSong = song;
			ignorePrefsVolumeThisSong = ignorePrefsVolume;
			StartNewSong();
		}

		private SongDef ChooseNextSong()
		{
			songWasForced = false;
			if (forcedNextSong != null)
			{
				SongDef result = forcedNextSong;
				forcedNextSong = null;
				songWasForced = true;
				return result;
			}
			IEnumerable<SongDef> source = from song in DefDatabase<SongDef>.AllDefs
			where AppropriateNow(song)
			select song;
			while (recentSongs.Count > 7)
			{
				recentSongs.Dequeue();
			}
			while (!source.Any() && recentSongs.Count > 0)
			{
				recentSongs.Dequeue();
			}
			if (!source.Any())
			{
				Log.Error("Could not get any appropriate song. Getting random and logging song selection data.");
				SongSelectionData();
				return DefDatabase<SongDef>.GetRandom();
			}
			return source.RandomElementByWeight((SongDef s) => s.commonality);
		}

		private bool AppropriateNow(SongDef song)
		{
			if (!song.playOnMap)
			{
				return false;
			}
			if (DangerMusicMode)
			{
				if (!song.tense)
				{
					return false;
				}
			}
			else if (song.tense)
			{
				return false;
			}
			Map map = Find.AnyPlayerHomeMap ?? Find.CurrentMap;
			if (!song.allowedSeasons.NullOrEmpty())
			{
				if (map == null)
				{
					return false;
				}
				if (!song.allowedSeasons.Contains(GenLocalDate.Season(map)))
				{
					return false;
				}
			}
			if (recentSongs.Contains(song))
			{
				return false;
			}
			if (song.allowedTimeOfDay != TimeOfDay.Any)
			{
				if (map == null)
				{
					return true;
				}
				if (song.allowedTimeOfDay == TimeOfDay.Night)
				{
					return GenLocalDate.DayPercent(map) < 0.2f || GenLocalDate.DayPercent(map) > 0.7f;
				}
				return GenLocalDate.DayPercent(map) > 0.2f && GenLocalDate.DayPercent(map) < 0.7f;
			}
			return true;
		}

		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("MusicManagerMap");
			stringBuilder.AppendLine("state: " + state);
			stringBuilder.AppendLine("lastStartedSong: " + lastStartedSong);
			stringBuilder.AppendLine("fadeoutFactor: " + fadeoutFactor);
			stringBuilder.AppendLine("nextSongStartTime: " + nextSongStartTime);
			stringBuilder.AppendLine("CurTime: " + CurTime);
			stringBuilder.AppendLine("recentSongs: " + (from s in recentSongs
			select s.defName).ToCommaList(useAnd: true));
			stringBuilder.AppendLine("disabled: " + disabled);
			return stringBuilder.ToString();
		}

		[DebugOutput]
		public void SongSelectionData()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Most recent song: " + ((lastStartedSong == null) ? "None" : lastStartedSong.defName));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Songs appropriate to play now:");
			foreach (SongDef item in from s in DefDatabase<SongDef>.AllDefs
			where AppropriateNow(s)
			select s)
			{
				stringBuilder.AppendLine("   " + item.defName);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Recently played songs:");
			foreach (SongDef recentSong in recentSongs)
			{
				stringBuilder.AppendLine("   " + recentSong.defName);
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
