using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Verse.Profile
{
	[HasDebugOutput]
	public static class MemoryTracker
	{
		private class ReferenceData
		{
			public struct Link
			{
				public object target;

				public ReferenceData targetRef;

				public string path;
			}

			public List<Link> refers = new List<Link>();

			public List<Link> referredBy = new List<Link>();

			public string path;

			public int pathCost;
		}

		private struct ChildReference
		{
			public object child;

			public string path;
		}

		public class MarkupComplete : Attribute
		{
		}

		private static Dictionary<Type, HashSet<WeakReference>> tracked = new Dictionary<Type, HashSet<WeakReference>>();

		private static List<WeakReference> foundCollections = new List<WeakReference>();

		private static bool trackedLocked = false;

		private static List<object> trackedQueue = new List<object>();

		private static List<RuntimeTypeHandle> trackedTypeQueue = new List<RuntimeTypeHandle>();

		private const int updatesPerCull = 10;

		private static int updatesSinceLastCull = 0;

		private static int cullTargetIndex = 0;

		public static bool AnythingTracked => tracked.Count > 0;

		public static IEnumerable<WeakReference> FoundCollections
		{
			get
			{
				if (foundCollections.Count == 0)
				{
					LogObjectHoldPathsFor(null, null);
				}
				return foundCollections;
			}
		}

		public static void RegisterObject(object obj)
		{
			if (trackedLocked)
			{
				trackedQueue.Add(obj);
			}
			else
			{
				Type type = obj.GetType();
				HashSet<WeakReference> value = null;
				if (!tracked.TryGetValue(type, out value))
				{
					value = new HashSet<WeakReference>();
					tracked[type] = value;
				}
				value.Add(new WeakReference(obj));
			}
		}

		public static void RegisterType(RuntimeTypeHandle typeHandle)
		{
			if (trackedLocked)
			{
				trackedTypeQueue.Add(typeHandle);
			}
			else
			{
				Type typeFromHandle = Type.GetTypeFromHandle(typeHandle);
				if (!tracked.ContainsKey(typeFromHandle))
				{
					tracked[typeFromHandle] = new HashSet<WeakReference>();
				}
			}
		}

		private static void LockTracking()
		{
			if (trackedLocked)
			{
				throw new NotImplementedException();
			}
			trackedLocked = true;
		}

		private static void UnlockTracking()
		{
			if (!trackedLocked)
			{
				throw new NotImplementedException();
			}
			trackedLocked = false;
			foreach (object item in trackedQueue)
			{
				RegisterObject(item);
			}
			trackedQueue.Clear();
			foreach (RuntimeTypeHandle item2 in trackedTypeQueue)
			{
				RegisterType(item2);
			}
			trackedTypeQueue.Clear();
		}

		[DebugOutput]
		[Category("System")]
		private static void ObjectsLoaded()
		{
			if (tracked.Count == 0)
			{
				Log.Message("No objects tracked, memory tracker markup may not be applied.");
			}
			else
			{
				GC.Collect();
				LockTracking();
				try
				{
					foreach (HashSet<WeakReference> value in tracked.Values)
					{
						CullNulls(value);
					}
					StringBuilder stringBuilder = new StringBuilder();
					foreach (KeyValuePair<Type, HashSet<WeakReference>> item in from kvp in tracked
					orderby -kvp.Value.Count
					select kvp)
					{
						stringBuilder.AppendLine(string.Format("{0,6} {1}", item.Value.Count, item.Key));
					}
					Log.Message(stringBuilder.ToString());
				}
				finally
				{
					UnlockTracking();
				}
			}
		}

		[DebugOutput]
		[Category("System")]
		private static void ObjectHoldPaths()
		{
			if (tracked.Count == 0)
			{
				Log.Message("No objects tracked, memory tracker markup may not be applied.");
			}
			else
			{
				GC.Collect();
				LockTracking();
				try
				{
					foreach (HashSet<WeakReference> value in tracked.Values)
					{
						CullNulls(value);
					}
					List<Type> list = new List<Type>();
					list.Add(typeof(Map));
					List<FloatMenuOption> list2 = new List<FloatMenuOption>();
					foreach (Type item in list.Concat(from kvp in tracked
					orderby -kvp.Value.Count
					select kvp.Key).Take(30))
					{
						Type type = item;
						HashSet<WeakReference> trackedBatch = tracked.TryGetValue(type);
						if (trackedBatch == null)
						{
							trackedBatch = new HashSet<WeakReference>();
						}
						list2.Add(new FloatMenuOption($"{type} ({trackedBatch.Count})", delegate
						{
							LogObjectHoldPathsFor(trackedBatch, (WeakReference _) => 1);
						}));
						if (list2.Count == 30)
						{
							break;
						}
					}
					Find.WindowStack.Add(new FloatMenu(list2));
				}
				finally
				{
					UnlockTracking();
				}
			}
		}

		public static void LogObjectHoldPathsFor(IEnumerable<WeakReference> elements, Func<WeakReference, int> weight)
		{
			GC.Collect();
			LockTracking();
			try
			{
				Dictionary<object, ReferenceData> dictionary = new Dictionary<object, ReferenceData>();
				HashSet<object> hashSet = new HashSet<object>();
				foundCollections.Clear();
				Queue<object> queue = new Queue<object>();
				foreach (object item in from weakref in tracked.SelectMany((KeyValuePair<Type, HashSet<WeakReference>> kvp) => kvp.Value)
				where weakref.IsAlive
				select weakref.Target)
				{
					if (!hashSet.Contains(item))
					{
						hashSet.Add(item);
						queue.Enqueue(item);
					}
				}
				foreach (Type item2 in GenTypes.AllTypes.Union(tracked.Keys))
				{
					if (!item2.FullName.Contains("MemoryTracker") && !item2.FullName.Contains("CollectionsTracker") && !item2.ContainsGenericParameters)
					{
						AccumulateStaticMembers(item2, dictionary, hashSet, queue);
					}
				}
				int num = 0;
				while (queue.Count > 0)
				{
					if (num % 10000 == 0)
					{
						Debug.LogFormat("{0} / {1} (to process: {2})", num, num + queue.Count, queue.Count);
					}
					num++;
					AccumulateReferences(queue.Dequeue(), dictionary, hashSet, queue);
				}
				if (elements != null && weight != null)
				{
					int num2 = 0;
					CalculateReferencePaths(dictionary, from kvp in dictionary
					where !kvp.Value.path.NullOrEmpty()
					select kvp.Key, num2);
					num2 += 1000;
					CalculateReferencePaths(dictionary, from kvp in dictionary
					where kvp.Value.path.NullOrEmpty() && kvp.Value.referredBy.Count == 0
					select kvp.Key, num2);
					foreach (object item3 in from kvp in dictionary
					where kvp.Value.path.NullOrEmpty()
					select kvp.Key)
					{
						num2 += 1000;
						CalculateReferencePaths(dictionary, new object[1]
						{
							item3
						}, num2);
					}
					Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
					foreach (WeakReference element in elements)
					{
						object target = element.Target;
						if (target != null)
						{
							string path = dictionary[target].path;
							if (!dictionary2.ContainsKey(path))
							{
								dictionary2[path] = 0;
							}
							Dictionary<string, int> dictionary3;
							string key;
							(dictionary3 = dictionary2)[key = path] = dictionary3[key] + weight(element);
						}
					}
					StringBuilder stringBuilder = new StringBuilder();
					foreach (KeyValuePair<string, int> item4 in from kvp in dictionary2
					orderby -kvp.Value
					select kvp)
					{
						stringBuilder.AppendLine($"{item4.Value}: {item4.Key}");
					}
					Log.Message(stringBuilder.ToString());
				}
			}
			finally
			{
				UnlockTracking();
			}
		}

		private static void AccumulateReferences(object obj, Dictionary<object, ReferenceData> references, HashSet<object> seen, Queue<object> toProcess)
		{
			ReferenceData value = null;
			if (!references.TryGetValue(obj, out value))
			{
				value = (references[obj] = new ReferenceData());
			}
			foreach (ChildReference item in GetAllReferencedClassesFromClassOrStruct(obj, GetFieldsFromHierarchy(obj.GetType(), BindingFlags.Instance), obj, string.Empty))
			{
				ChildReference current = item;
				if (!current.child.GetType().IsClass)
				{
					throw new ApplicationException();
				}
				ReferenceData value2 = null;
				if (!references.TryGetValue(current.child, out value2))
				{
					value2 = new ReferenceData();
					references[current.child] = value2;
				}
				value2.referredBy.Add(new ReferenceData.Link
				{
					target = obj,
					targetRef = value,
					path = current.path
				});
				value.refers.Add(new ReferenceData.Link
				{
					target = current.child,
					targetRef = value2,
					path = current.path
				});
				if (!seen.Contains(current.child))
				{
					seen.Add(current.child);
					toProcess.Enqueue(current.child);
				}
			}
		}

		private static void AccumulateStaticMembers(Type type, Dictionary<object, ReferenceData> references, HashSet<object> seen, Queue<object> toProcess)
		{
			foreach (ChildReference item in GetAllReferencedClassesFromClassOrStruct(null, GetFields(type, BindingFlags.Static), null, type.ToString() + "."))
			{
				ChildReference current = item;
				if (!current.child.GetType().IsClass)
				{
					throw new ApplicationException();
				}
				ReferenceData value = null;
				if (!references.TryGetValue(current.child, out value))
				{
					value = new ReferenceData();
					value.path = current.path;
					value.pathCost = 0;
					references[current.child] = value;
				}
				if (!seen.Contains(current.child))
				{
					seen.Add(current.child);
					toProcess.Enqueue(current.child);
				}
			}
		}

		private static IEnumerable<ChildReference> GetAllReferencedClassesFromClassOrStruct(object current, IEnumerable<FieldInfo> fields, object parent, string currentPath)
		{
			foreach (FieldInfo field in fields)
			{
				if (!field.FieldType.IsPrimitive)
				{
					object referenced = field.GetValue(current);
					if (referenced != null)
					{
						using (IEnumerator<ChildReference> enumerator2 = DistillChildReferencesFromObject(referenced, parent, currentPath + field.Name).GetEnumerator())
						{
							if (enumerator2.MoveNext())
							{
								ChildReference child2 = enumerator2.Current;
								yield return child2;
								/*Error: Unable to find new state assignment for yield return*/;
							}
						}
					}
				}
			}
			if (current != null && current is ICollection)
			{
				foundCollections.Add(new WeakReference(current));
				IEnumerator enumerator3 = (current as IEnumerable).GetEnumerator();
				try
				{
					while (enumerator3.MoveNext())
					{
						object entry = enumerator3.Current;
						if (entry != null && !entry.GetType().IsPrimitive)
						{
							using (IEnumerator<ChildReference> enumerator4 = DistillChildReferencesFromObject(entry, parent, currentPath + "[]").GetEnumerator())
							{
								if (enumerator4.MoveNext())
								{
									ChildReference child = enumerator4.Current;
									yield return child;
									/*Error: Unable to find new state assignment for yield return*/;
								}
							}
						}
					}
				}
				finally
				{
					IDisposable disposable;
					IDisposable disposable2 = disposable = (enumerator3 as IDisposable);
					if (disposable != null)
					{
						disposable2.Dispose();
					}
				}
			}
			yield break;
			IL_02f3:
			/*Error near IL_02f4: Unexpected return in MoveNext()*/;
		}

		private static IEnumerable<ChildReference> DistillChildReferencesFromObject(object current, object parent, string currentPath)
		{
			Type type = current.GetType();
			if (type.IsClass)
			{
				yield return new ChildReference
				{
					child = current,
					path = currentPath
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (!type.IsPrimitive)
			{
				if (!type.IsValueType)
				{
					throw new NotImplementedException();
				}
				using (IEnumerator<ChildReference> enumerator = GetAllReferencedClassesFromClassOrStruct(currentPath: currentPath + ".", current: current, fields: GetFieldsFromHierarchy(type, BindingFlags.Instance), parent: parent).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						ChildReference childReference = enumerator.Current;
						yield return childReference;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_017e:
			/*Error near IL_017f: Unexpected return in MoveNext()*/;
		}

		private static IEnumerable<FieldInfo> GetFieldsFromHierarchy(Type type, BindingFlags bindingFlags)
		{
			while (type != null)
			{
				using (IEnumerator<FieldInfo> enumerator = GetFields(type, bindingFlags).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						FieldInfo field = enumerator.Current;
						yield return field;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				type = type.BaseType;
			}
			yield break;
			IL_00e0:
			/*Error near IL_00e1: Unexpected return in MoveNext()*/;
		}

		private static IEnumerable<FieldInfo> GetFields(Type type, BindingFlags bindingFlags)
		{
			FieldInfo[] fields = type.GetFields(bindingFlags | BindingFlags.Public | BindingFlags.NonPublic);
			int num = 0;
			if (num < fields.Length)
			{
				FieldInfo field = fields[num];
				yield return field;
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		private static void CalculateReferencePaths(Dictionary<object, ReferenceData> references, IEnumerable<object> objects, int pathCost)
		{
			Queue<object> queue = new Queue<object>(objects);
			while (queue.Count > 0)
			{
				object obj = queue.Dequeue();
				if (references[obj].path.NullOrEmpty())
				{
					references[obj].path = $"???.{obj.GetType()}";
					references[obj].pathCost = pathCost;
				}
				CalculateObjectReferencePath(obj, references, queue);
			}
		}

		private static void CalculateObjectReferencePath(object obj, Dictionary<object, ReferenceData> references, Queue<object> queue)
		{
			ReferenceData referenceData = references[obj];
			foreach (ReferenceData.Link refer in referenceData.refers)
			{
				ReferenceData.Link current = refer;
				ReferenceData referenceData2 = references[current.target];
				string text = referenceData.path + "." + current.path;
				int num = referenceData.pathCost + 1;
				if (referenceData2.path.NullOrEmpty())
				{
					queue.Enqueue(current.target);
					referenceData2.path = text;
					referenceData2.pathCost = num;
				}
				else if (referenceData2.pathCost == num && referenceData2.path.CompareTo(text) < 0)
				{
					referenceData2.path = text;
				}
				else if (referenceData2.pathCost > num)
				{
					throw new ApplicationException();
				}
			}
		}

		public static void Update()
		{
			if (tracked.Count != 0 && updatesSinceLastCull++ >= 10)
			{
				updatesSinceLastCull = 0;
				HashSet<WeakReference> value = tracked.ElementAtOrDefault(cullTargetIndex++).Value;
				if (value == null)
				{
					cullTargetIndex = 0;
				}
				else
				{
					CullNulls(value);
				}
			}
		}

		private static void CullNulls(HashSet<WeakReference> table)
		{
			table.RemoveWhere((WeakReference element) => !element.IsAlive);
		}
	}
}
