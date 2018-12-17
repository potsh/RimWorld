using System;

namespace Verse.AI
{
	public struct ThinkResult : IEquatable<ThinkResult>
	{
		private Job jobInt;

		private ThinkNode sourceNodeInt;

		private JobTag? tag;

		private bool fromQueue;

		public Job Job => jobInt;

		public ThinkNode SourceNode => sourceNodeInt;

		public JobTag? Tag => tag;

		public bool FromQueue => fromQueue;

		public bool IsValid => Job != null;

		public static ThinkResult NoJob => new ThinkResult(null, null);

		public ThinkResult(Job job, ThinkNode sourceNode, JobTag? tag = default(JobTag?), bool fromQueue = false)
		{
			jobInt = job;
			sourceNodeInt = sourceNode;
			this.tag = tag;
			this.fromQueue = fromQueue;
		}

		public override string ToString()
		{
			string text = (Job == null) ? "null" : Job.ToString();
			string text2 = (SourceNode == null) ? "null" : SourceNode.ToString();
			return "(job=" + text + " sourceNode=" + text2 + ")";
		}

		public override int GetHashCode()
		{
			int seed = 0;
			seed = Gen.HashCombine(seed, jobInt);
			seed = Gen.HashCombine(seed, sourceNodeInt);
			seed = Gen.HashCombine(seed, tag);
			return Gen.HashCombineStruct(seed, fromQueue);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ThinkResult))
			{
				return false;
			}
			return Equals((ThinkResult)obj);
		}

		public bool Equals(ThinkResult other)
		{
			int result;
			if (jobInt == other.jobInt && sourceNodeInt == other.sourceNodeInt)
			{
				JobTag? jobTag = tag;
				JobTag valueOrDefault = jobTag.GetValueOrDefault();
				JobTag? jobTag2 = other.tag;
				if (valueOrDefault == jobTag2.GetValueOrDefault() && jobTag.HasValue == jobTag2.HasValue)
				{
					result = ((fromQueue == other.fromQueue) ? 1 : 0);
					goto IL_006d;
				}
			}
			result = 0;
			goto IL_006d;
			IL_006d:
			return (byte)result != 0;
		}

		public static bool operator ==(ThinkResult lhs, ThinkResult rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(ThinkResult lhs, ThinkResult rhs)
		{
			return !(lhs == rhs);
		}
	}
}
