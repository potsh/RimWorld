using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public sealed class LetterStack : IExposable
	{
		private List<Letter> letters = new List<Letter>();

		private int mouseoverLetterIndex = -1;

		private float lastTopYInt;

		private const float LettersBottomY = 350f;

		public const float LetterSpacing = 12f;

		public List<Letter> LettersListForReading => letters;

		public float LastTopY => lastTopYInt;

		public void ReceiveLetter(string label, string text, LetterDef textLetterDef, LookTargets lookTargets, Faction relatedFaction = null, string debugInfo = null)
		{
			ChoiceLetter let = LetterMaker.MakeLetter(label, text, textLetterDef, lookTargets, relatedFaction);
			ReceiveLetter(let, debugInfo);
		}

		public void ReceiveLetter(string label, string text, LetterDef textLetterDef, string debugInfo = null)
		{
			ChoiceLetter let = LetterMaker.MakeLetter(label, text, textLetterDef);
			ReceiveLetter(let, debugInfo);
		}

		public void ReceiveLetter(Letter let, string debugInfo = null)
		{
			if (let.CanShowInLetterStack)
			{
				let.def.arriveSound.PlayOneShotOnCamera();
				if (let.def.pauseIfPauseOnUrgentLetter && Prefs.PauseOnUrgentLetter)
				{
					Find.TickManager.Pause();
				}
				else if (let.def.forcedSlowdown)
				{
					Find.TickManager.slower.SignalForceNormalSpeedShort();
				}
				let.arrivalTime = Time.time;
				let.arrivalTick = Find.TickManager.TicksGame;
				let.debugInfo = debugInfo;
				letters.Add(let);
				Find.Archive.Add(let);
				let.Received();
			}
		}

		public void RemoveLetter(Letter let)
		{
			letters.Remove(let);
			let.Removed();
		}

		public void LettersOnGUI(float baseY)
		{
			float num = baseY - 30f;
			for (int num2 = letters.Count - 1; num2 >= 0; num2--)
			{
				letters[num2].DrawButtonAt(num);
				num -= 42f;
			}
			lastTopYInt = num;
			if (Event.current.type == EventType.Repaint)
			{
				num = baseY - 30f;
				for (int num3 = letters.Count - 1; num3 >= 0; num3--)
				{
					letters[num3].CheckForMouseOverTextAt(num);
					num -= 42f;
				}
			}
		}

		public void LetterStackTick()
		{
			int num = Find.TickManager.TicksGame + 1;
			int num2 = 0;
			LetterWithTimeout letterWithTimeout;
			while (true)
			{
				if (num2 >= letters.Count)
				{
					return;
				}
				letterWithTimeout = (letters[num2] as LetterWithTimeout);
				if (letterWithTimeout != null && letterWithTimeout.TimeoutActive && letterWithTimeout.disappearAtTick == num)
				{
					break;
				}
				num2++;
			}
			letterWithTimeout.OpenLetter();
		}

		public void LetterStackUpdate()
		{
			if (mouseoverLetterIndex >= 0 && mouseoverLetterIndex < letters.Count)
			{
				letters[mouseoverLetterIndex].lookTargets.TryHighlight();
			}
			mouseoverLetterIndex = -1;
			for (int num = letters.Count - 1; num >= 0; num--)
			{
				if (!letters[num].CanShowInLetterStack)
				{
					RemoveLetter(letters[num]);
				}
			}
		}

		public void Notify_LetterMouseover(Letter let)
		{
			mouseoverLetterIndex = letters.IndexOf(let);
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref letters, "letters", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				letters.RemoveAll((Letter x) => x == null);
			}
		}
	}
}
