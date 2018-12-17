using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class GenCollection
	{
		private static class SortStableTempList<T>
		{
			public static List<Pair<T, int>> list = new List<Pair<T, int>>();

			public static bool working;
		}

		public static bool SharesElementWith<T>(this IEnumerable<T> source, IEnumerable<T> other)
		{
			return source.Any((T item) => other.Contains(item));
		}

		public static IEnumerable<T> InRandomOrder<T>(this IEnumerable<T> source, IList<T> workingList = null)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (workingList == null)
			{
				workingList = source.ToList();
			}
			else
			{
				workingList.Clear();
				foreach (T item in source)
				{
					workingList.Add(item);
				}
			}
			int countUnChosen = workingList.Count;
			if (countUnChosen > 0)
			{
				int rand = Rand.Range(0, countUnChosen);
				yield return workingList[rand];
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public static T RandomElement<T>(this IEnumerable<T> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			IList<T> list = source as IList<T>;
			if (list == null)
			{
				list = source.ToList();
			}
			if (list.Count == 0)
			{
				Log.Warning("Getting random element from empty collection.");
				return default(T);
			}
			return list[Rand.Range(0, list.Count)];
		}

		public static T RandomElementWithFallback<T>(this IEnumerable<T> source, T fallback = default(T))
		{
			if (source.TryRandomElement(out T result))
			{
				return result;
			}
			return fallback;
		}

		public static bool TryRandomElement<T>(this IEnumerable<T> source, out T result)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			IList<T> list = source as IList<T>;
			if (list != null)
			{
				if (list.Count == 0)
				{
					result = default(T);
					return false;
				}
			}
			else
			{
				list = source.ToList();
				if (!list.Any())
				{
					result = default(T);
					return false;
				}
			}
			result = list.RandomElement();
			return true;
		}

		public static T RandomElementByWeight<T>(this IEnumerable<T> source, Func<T, float> weightSelector)
		{
			float num = 0f;
			IList<T> list = source as IList<T>;
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					float num2 = weightSelector(list[i]);
					if (num2 < 0f)
					{
						Log.Error("Negative weight in selector: " + num2 + " from " + list[i]);
						num2 = 0f;
					}
					num += num2;
				}
				if (list.Count == 1 && num > 0f)
				{
					return list[0];
				}
			}
			else
			{
				int num3 = 0;
				foreach (T item in source)
				{
					num3++;
					float num4 = weightSelector(item);
					if (num4 < 0f)
					{
						Log.Error("Negative weight in selector: " + num4 + " from " + item);
						num4 = 0f;
					}
					num += num4;
				}
				if (num3 == 1 && num > 0f)
				{
					return source.First();
				}
			}
			if (num <= 0f)
			{
				Log.Error("RandomElementByWeight with totalWeight=" + num + " - use TryRandomElementByWeight.");
				return default(T);
			}
			float num5 = Rand.Value * num;
			float num6 = 0f;
			if (list != null)
			{
				for (int j = 0; j < list.Count; j++)
				{
					float num7 = weightSelector(list[j]);
					if (!(num7 <= 0f))
					{
						num6 += num7;
						if (num6 >= num5)
						{
							return list[j];
						}
					}
				}
			}
			else
			{
				foreach (T item2 in source)
				{
					float num8 = weightSelector(item2);
					if (!(num8 <= 0f))
					{
						num6 += num8;
						if (num6 >= num5)
						{
							return item2;
						}
					}
				}
			}
			return default(T);
		}

		public static T RandomElementByWeightWithFallback<T>(this IEnumerable<T> source, Func<T, float> weightSelector, T fallback = default(T))
		{
			if (source.TryRandomElementByWeight(weightSelector, out T result))
			{
				return result;
			}
			return fallback;
		}

		public static bool TryRandomElementByWeight<T>(this IEnumerable<T> source, Func<T, float> weightSelector, out T result)
		{
			IList<T> list = source as IList<T>;
			if (list != null)
			{
				float num = 0f;
				for (int i = 0; i < list.Count; i++)
				{
					float num2 = weightSelector(list[i]);
					if (num2 < 0f)
					{
						Log.Error("Negative weight in selector: " + num2 + " from " + list[i]);
						num2 = 0f;
					}
					num += num2;
				}
				if (list.Count == 1 && num > 0f)
				{
					result = list[0];
					return true;
				}
				if (num == 0f)
				{
					result = default(T);
					return false;
				}
				num *= Rand.Value;
				for (int j = 0; j < list.Count; j++)
				{
					float num3 = weightSelector(list[j]);
					if (!(num3 <= 0f))
					{
						num -= num3;
						if (num <= 0f)
						{
							result = list[j];
							return true;
						}
					}
				}
			}
			IEnumerator<T> enumerator = source.GetEnumerator();
			result = default(T);
			float num4 = 0f;
			while (num4 == 0f && enumerator.MoveNext())
			{
				result = enumerator.Current;
				num4 = weightSelector(result);
				if (num4 < 0f)
				{
					Log.Error("Negative weight in selector: " + num4 + " from " + result);
					num4 = 0f;
				}
			}
			if (num4 == 0f)
			{
				result = default(T);
				return false;
			}
			while (enumerator.MoveNext())
			{
				T current = enumerator.Current;
				float num5 = weightSelector(current);
				if (num5 < 0f)
				{
					Log.Error("Negative weight in selector: " + num5 + " from " + current);
					num5 = 0f;
				}
				if (Rand.Range(0f, num4 + num5) >= num4)
				{
					result = current;
				}
				num4 += num5;
			}
			return true;
		}

		public static T RandomElementByWeightWithDefault<T>(this IEnumerable<T> source, Func<T, float> weightSelector, float defaultValueWeight)
		{
			if (defaultValueWeight < 0f)
			{
				Log.Error("Negative default value weight.");
				defaultValueWeight = 0f;
			}
			float num = 0f;
			foreach (T item in source)
			{
				float num2 = weightSelector(item);
				if (num2 < 0f)
				{
					Log.Error("Negative weight in selector: " + num2 + " from " + item);
					num2 = 0f;
				}
				num += num2;
			}
			float num3 = defaultValueWeight + num;
			if (num3 <= 0f)
			{
				Log.Error("RandomElementByWeightWithDefault with totalWeight=" + num3);
				return default(T);
			}
			if (Rand.Value < defaultValueWeight / num3 || num == 0f)
			{
				return default(T);
			}
			return source.RandomElementByWeight(weightSelector);
		}

		public static T FirstOrFallback<T>(this IEnumerable<T> source, T fallback = default(T))
		{
			using (IEnumerator<T> enumerator = source.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					return enumerator.Current;
				}
				return fallback;
			}
		}

		public static T FirstOrFallback<T>(this IEnumerable<T> source, Func<T, bool> predicate, T fallback = default(T))
		{
			return source.Where(predicate).FirstOrFallback(fallback);
		}

		public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
		{
			return source.MaxBy(selector, Comparer<TKey>.Default);
		}

		public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}
			if (comparer == null)
			{
				throw new ArgumentNullException("comparer");
			}
			using (IEnumerator<TSource> enumerator = source.GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					throw new InvalidOperationException("Sequence contains no elements");
				}
				TSource val = enumerator.Current;
				TKey y = selector(val);
				while (enumerator.MoveNext())
				{
					TSource current = enumerator.Current;
					TKey val2 = selector(current);
					if (comparer.Compare(val2, y) > 0)
					{
						val = current;
						y = val2;
					}
				}
				return val;
			}
		}

		public static TSource MaxByWithFallback<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, TSource fallback = default(TSource))
		{
			return source.MaxByWithFallback(selector, Comparer<TKey>.Default, fallback);
		}

		public static TSource MaxByWithFallback<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer, TSource fallback = default(TSource))
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}
			if (comparer == null)
			{
				throw new ArgumentNullException("comparer");
			}
			using (IEnumerator<TSource> enumerator = source.GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					return fallback;
				}
				TSource val = enumerator.Current;
				TKey y = selector(val);
				while (enumerator.MoveNext())
				{
					TSource current = enumerator.Current;
					TKey val2 = selector(current);
					if (comparer.Compare(val2, y) > 0)
					{
						val = current;
						y = val2;
					}
				}
				return val;
			}
		}

		public static bool TryMaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, out TSource value)
		{
			return source.TryMaxBy(selector, Comparer<TKey>.Default, out value);
		}

		public static bool TryMaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer, out TSource value)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}
			if (comparer == null)
			{
				throw new ArgumentNullException("comparer");
			}
			using (IEnumerator<TSource> enumerator = source.GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					value = default(TSource);
					return false;
				}
				TSource val = enumerator.Current;
				TKey y = selector(val);
				while (enumerator.MoveNext())
				{
					TSource current = enumerator.Current;
					TKey val2 = selector(current);
					if (comparer.Compare(val2, y) > 0)
					{
						val = current;
						y = val2;
					}
				}
				value = val;
				return true;
			}
		}

		public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
		{
			return source.MinBy(selector, Comparer<TKey>.Default);
		}

		public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}
			if (comparer == null)
			{
				throw new ArgumentNullException("comparer");
			}
			using (IEnumerator<TSource> enumerator = source.GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					throw new InvalidOperationException("Sequence contains no elements");
				}
				TSource val = enumerator.Current;
				TKey y = selector(val);
				while (enumerator.MoveNext())
				{
					TSource current = enumerator.Current;
					TKey val2 = selector(current);
					if (comparer.Compare(val2, y) < 0)
					{
						val = current;
						y = val2;
					}
				}
				return val;
			}
		}

		public static bool TryMinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, out TSource value)
		{
			return source.TryMinBy(selector, Comparer<TKey>.Default, out value);
		}

		public static bool TryMinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer, out TSource value)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}
			if (comparer == null)
			{
				throw new ArgumentNullException("comparer");
			}
			using (IEnumerator<TSource> enumerator = source.GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					value = default(TSource);
					return false;
				}
				TSource val = enumerator.Current;
				TKey y = selector(val);
				while (enumerator.MoveNext())
				{
					TSource current = enumerator.Current;
					TKey val2 = selector(current);
					if (comparer.Compare(val2, y) < 0)
					{
						val = current;
						y = val2;
					}
				}
				value = val;
				return true;
			}
		}

		public static void SortBy<T, TSortBy>(this List<T> list, Func<T, TSortBy> selector) where TSortBy : IComparable<TSortBy>
		{
			if (list.Count > 1)
			{
				list.Sort((T a, T b) => selector(a).CompareTo(selector(b)));
			}
		}

		public static void SortBy<T, TSortBy, TThenBy>(this List<T> list, Func<T, TSortBy> selector, Func<T, TThenBy> thenBySelector) where TSortBy : IComparable<TSortBy>, IEquatable<TSortBy> where TThenBy : IComparable<TThenBy>
		{
			if (list.Count > 1)
			{
				list.Sort(delegate(T a, T b)
				{
					TSortBy val = selector(a);
					TSortBy other = selector(b);
					if (!val.Equals(other))
					{
						return val.CompareTo(other);
					}
					return thenBySelector(a).CompareTo(thenBySelector(b));
				});
			}
		}

		public static void SortByDescending<T, TSortByDescending>(this List<T> list, Func<T, TSortByDescending> selector) where TSortByDescending : IComparable<TSortByDescending>
		{
			if (list.Count > 1)
			{
				list.Sort((T a, T b) => selector(b).CompareTo(selector(a)));
			}
		}

		public static void SortByDescending<T, TSortByDescending, TThenByDescending>(this List<T> list, Func<T, TSortByDescending> selector, Func<T, TThenByDescending> thenByDescendingSelector) where TSortByDescending : IComparable<TSortByDescending>, IEquatable<TSortByDescending> where TThenByDescending : IComparable<TThenByDescending>
		{
			if (list.Count > 1)
			{
				list.Sort(delegate(T a, T b)
				{
					TSortByDescending other = selector(a);
					TSortByDescending other2 = selector(b);
					if (!other.Equals(other2))
					{
						return other2.CompareTo(other);
					}
					return thenByDescendingSelector(b).CompareTo(thenByDescendingSelector(a));
				});
			}
		}

		public static void SortStable<T>(this IList<T> list, Func<T, T, int> comparator)
		{
			if (list.Count > 1)
			{
				List<Pair<T, int>> list2;
				bool flag;
				if (SortStableTempList<T>.working)
				{
					list2 = new List<Pair<T, int>>();
					flag = false;
				}
				else
				{
					list2 = SortStableTempList<T>.list;
					SortStableTempList<T>.working = true;
					flag = true;
				}
				try
				{
					list2.Clear();
					for (int i = 0; i < list.Count; i++)
					{
						list2.Add(new Pair<T, int>(list[i], i));
					}
					list2.Sort(delegate(Pair<T, int> lhs, Pair<T, int> rhs)
					{
						int num = comparator(lhs.First, rhs.First);
						if (num != 0)
						{
							return num;
						}
						return lhs.Second.CompareTo(rhs.Second);
					});
					list.Clear();
					for (int j = 0; j < list2.Count; j++)
					{
						list.Add(list2[j].First);
					}
					list2.Clear();
				}
				finally
				{
					if (flag)
					{
						SortStableTempList<T>.working = false;
					}
				}
			}
		}

		public static int RemoveAll<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Predicate<KeyValuePair<TKey, TValue>> predicate)
		{
			List<TKey> list = null;
			try
			{
				foreach (KeyValuePair<TKey, TValue> item in dictionary)
				{
					if (predicate(item))
					{
						if (list == null)
						{
							list = SimplePool<List<TKey>>.Get();
						}
						list.Add(item.Key);
					}
				}
				if (list != null)
				{
					int i = 0;
					for (int count = list.Count; i < count; i++)
					{
						dictionary.Remove(list[i]);
					}
					return list.Count;
				}
				return 0;
			}
			finally
			{
				if (list != null)
				{
					list.Clear();
					SimplePool<List<TKey>>.Return(list);
				}
			}
		}

		public static void RemoveAll<T>(this List<T> list, Func<T, int, bool> predicate)
		{
			int i = 0;
			int count;
			for (count = list.Count; i < count && !predicate(list[i], i); i++)
			{
			}
			if (i < count)
			{
				int j = i + 1;
				while (j < count)
				{
					for (; j < count && predicate(list[j], j); j++)
					{
					}
					if (j < count)
					{
						list[i++] = list[j++];
					}
				}
			}
		}

		public static void RemoveLast<T>(this List<T> list)
		{
			list.RemoveAt(list.Count - 1);
		}

		public static T Pop<T>(this List<T> list)
		{
			T result = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			return result;
		}

		public static bool Any<T>(this List<T> list, Predicate<T> predicate)
		{
			return list.FindIndex(predicate) != -1;
		}

		public static bool Any<T>(this List<T> list)
		{
			return list.Count != 0;
		}

		public static void AddRange<T>(this HashSet<T> set, List<T> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				set.Add(list[i]);
			}
		}

		public static void AddRange<T>(this HashSet<T> set, HashSet<T> other)
		{
			foreach (T item in other)
			{
				set.Add(item);
			}
		}

		public static float AverageWeighted<T>(this IEnumerable<T> list, Func<T, float> weight, Func<T, float> value)
		{
			float num = 0f;
			float num2 = 0f;
			foreach (T item in list)
			{
				float num3 = weight(item);
				num += num3;
				num2 += value(item) * num3;
			}
			return num2 / num;
		}

		public static void ExecuteEnumerable(this IEnumerable enumerable)
		{
			IEnumerator enumerator = enumerable.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object current = enumerator.Current;
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}

		public static int FirstIndexOf<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
		{
			int num = 0;
			foreach (T item in enumerable)
			{
				if (predicate(item))
				{
					return num;
				}
				num++;
			}
			return num;
		}

		public static V TryGetValue<T, V>(this IDictionary<T, V> dict, T key, V fallback = default(V))
		{
			if (!dict.TryGetValue(key, out V value))
			{
				return fallback;
			}
			return value;
		}

		public static IEnumerable<Pair<T, V>> Cross<T, V>(this IEnumerable<T> lhs, IEnumerable<V> rhs)
		{
			T[] lhsv = lhs.ToArray();
			V[] rhsv = rhs.ToArray();
			int j = 0;
			int i;
			while (true)
			{
				if (j >= lhsv.Length)
				{
					yield break;
				}
				i = 0;
				if (i < rhsv.Length)
				{
					break;
				}
				j++;
			}
			yield return new Pair<T, V>(lhsv[j], rhsv[i]);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public static IEnumerable<T> Concat<T>(this IEnumerable<T> lhs, T rhs)
		{
			using (IEnumerator<T> enumerator = lhs.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					T t = enumerator.Current;
					yield return t;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield return rhs;
			/*Error: Unable to find new state assignment for yield return*/;
			IL_00d8:
			/*Error near IL_00d9: Unexpected return in MoveNext()*/;
		}

		public static LocalTargetInfo FirstValid(this List<LocalTargetInfo> source)
		{
			if (source == null)
			{
				return LocalTargetInfo.Invalid;
			}
			for (int i = 0; i < source.Count; i++)
			{
				if (source[i].IsValid)
				{
					return source[i];
				}
			}
			return LocalTargetInfo.Invalid;
		}

		public static IEnumerable<T> Except<T>(this IEnumerable<T> lhs, T rhs) where T : class
		{
			foreach (T lh in lhs)
			{
				if (lh != rhs)
				{
					yield return lh;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_00cf:
			/*Error near IL_00d0: Unexpected return in MoveNext()*/;
		}

		public static bool ListsEqual<T>(List<T> a, List<T> b) where T : class
		{
			if (a == b)
			{
				return true;
			}
			if (a.NullOrEmpty() && b.NullOrEmpty())
			{
				return true;
			}
			if (a.NullOrEmpty() || b.NullOrEmpty())
			{
				return false;
			}
			if (a.Count != b.Count)
			{
				return false;
			}
			EqualityComparer<T> @default = EqualityComparer<T>.Default;
			for (int i = 0; i < a.Count; i++)
			{
				if (!@default.Equals(a[i], b[i]))
				{
					return false;
				}
			}
			return true;
		}

		public static IEnumerable<T> TakeRandom<T>(this List<T> list, int count)
		{
			if (!list.NullOrEmpty())
			{
				int i = 0;
				if (i < count)
				{
					yield return list[Rand.Range(0, list.Count)];
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		public static void AddDistinct<T>(this List<T> list, T element) where T : class
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == element)
				{
					return;
				}
			}
			list.Add(element);
		}
	}
}
