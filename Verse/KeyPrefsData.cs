using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class KeyPrefsData
	{
		public Dictionary<KeyBindingDef, KeyBindingData> keyPrefs = new Dictionary<KeyBindingDef, KeyBindingData>();

		public void ResetToDefaults()
		{
			keyPrefs.Clear();
			AddMissingDefaultBindings();
		}

		public void AddMissingDefaultBindings()
		{
			foreach (KeyBindingDef allDef in DefDatabase<KeyBindingDef>.AllDefs)
			{
				if (!keyPrefs.ContainsKey(allDef))
				{
					keyPrefs.Add(allDef, new KeyBindingData(allDef.defaultKeyCodeA, allDef.defaultKeyCodeB));
				}
			}
		}

		public bool SetBinding(KeyBindingDef keyDef, KeyPrefs.BindingSlot slot, KeyCode keyCode)
		{
			if (keyPrefs.TryGetValue(keyDef, out KeyBindingData value))
			{
				switch (slot)
				{
				case KeyPrefs.BindingSlot.A:
					value.keyBindingA = keyCode;
					break;
				case KeyPrefs.BindingSlot.B:
					value.keyBindingB = keyCode;
					break;
				default:
					Log.Error("Tried to set a key binding for \"" + keyDef.LabelCap + "\" on a nonexistent slot: " + slot.ToString());
					return false;
				}
				return true;
			}
			Log.Error("Key not found in keyprefs: \"" + keyDef.LabelCap + "\"");
			return false;
		}

		public KeyCode GetBoundKeyCode(KeyBindingDef keyDef, KeyPrefs.BindingSlot slot)
		{
			if (keyPrefs.TryGetValue(keyDef, out KeyBindingData value))
			{
				switch (slot)
				{
				case KeyPrefs.BindingSlot.A:
					return value.keyBindingA;
				case KeyPrefs.BindingSlot.B:
					return value.keyBindingB;
				default:
					throw new InvalidOperationException();
				}
			}
			Log.Error("Key not found in keyprefs: \"" + keyDef.LabelCap + "\"");
			return KeyCode.None;
		}

		private IEnumerable<KeyBindingDef> ConflictingBindings(KeyBindingDef keyDef, KeyCode code)
		{
			using (IEnumerator<KeyBindingDef> enumerator = DefDatabase<KeyBindingDef>.AllDefs.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					_003CConflictingBindings_003Ec__Iterator0 _003CConflictingBindings_003Ec__Iterator = (_003CConflictingBindings_003Ec__Iterator0)/*Error near IL_0058: stateMachine*/;
					KeyBindingDef def = enumerator.Current;
					KeyBindingData prefData;
					if (def != keyDef && ((def.category == keyDef.category && def.category.selfConflicting) || keyDef.category.checkForConflicts.Contains(def.category) || (keyDef.extraConflictTags != null && def.extraConflictTags != null && keyDef.extraConflictTags.Any((string tag) => def.extraConflictTags.Contains(tag)))) && keyPrefs.TryGetValue(def, out prefData) && (prefData.keyBindingA == code || prefData.keyBindingB == code))
					{
						yield return def;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_01eb:
			/*Error near IL_01ec: Unexpected return in MoveNext()*/;
		}

		public void EraseConflictingBindingsForKeyCode(KeyBindingDef keyDef, KeyCode keyCode, Action<KeyBindingDef> callBackOnErase = null)
		{
			foreach (KeyBindingDef item in ConflictingBindings(keyDef, keyCode))
			{
				KeyBindingData keyBindingData = keyPrefs[item];
				if (keyBindingData.keyBindingA == keyCode)
				{
					keyBindingData.keyBindingA = KeyCode.None;
				}
				if (keyBindingData.keyBindingB == keyCode)
				{
					keyBindingData.keyBindingB = KeyCode.None;
				}
				callBackOnErase?.Invoke(item);
			}
		}

		public void CheckConflictsFor(KeyBindingDef keyDef, KeyPrefs.BindingSlot slot)
		{
			KeyCode boundKeyCode = GetBoundKeyCode(keyDef, slot);
			if (boundKeyCode != 0)
			{
				EraseConflictingBindingsForKeyCode(keyDef, boundKeyCode);
				SetBinding(keyDef, slot, boundKeyCode);
			}
		}

		public KeyPrefsData Clone()
		{
			KeyPrefsData keyPrefsData = new KeyPrefsData();
			foreach (KeyValuePair<KeyBindingDef, KeyBindingData> keyPref in keyPrefs)
			{
				keyPrefsData.keyPrefs[keyPref.Key] = new KeyBindingData(keyPref.Value.keyBindingA, keyPref.Value.keyBindingB);
			}
			return keyPrefsData;
		}

		public void ErrorCheck()
		{
			foreach (KeyBindingDef allDef in DefDatabase<KeyBindingDef>.AllDefs)
			{
				ErrorCheckOn(allDef, KeyPrefs.BindingSlot.A);
				ErrorCheckOn(allDef, KeyPrefs.BindingSlot.B);
			}
		}

		private void ErrorCheckOn(KeyBindingDef keyDef, KeyPrefs.BindingSlot slot)
		{
			KeyCode boundKeyCode = GetBoundKeyCode(keyDef, slot);
			if (boundKeyCode != 0)
			{
				foreach (KeyBindingDef item in ConflictingBindings(keyDef, boundKeyCode))
				{
					bool flag = boundKeyCode != keyDef.GetDefaultKeyCode(slot);
					Log.Error("Key binding conflict: " + item + " and " + keyDef + " are both bound to " + boundKeyCode + "." + ((!flag) ? string.Empty : " Fixed automatically."));
					if (flag)
					{
						if (slot == KeyPrefs.BindingSlot.A)
						{
							keyPrefs[keyDef].keyBindingA = keyDef.defaultKeyCodeA;
						}
						else
						{
							keyPrefs[keyDef].keyBindingB = keyDef.defaultKeyCodeB;
						}
						KeyPrefs.Save();
					}
				}
			}
		}
	}
}
