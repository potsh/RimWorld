using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.Grammar
{
	public abstract class Rule
	{
		public struct ConstantConstraint
		{
			[MayTranslate]
			public string key;

			[MayTranslate]
			public string value;

			public bool equality;
		}

		[MayTranslate]
		public string keyword;

		public List<ConstantConstraint> constantConstraints;

		public abstract float BaseSelectionWeight
		{
			get;
		}

		public virtual Rule DeepCopy()
		{
			Rule rule = (Rule)Activator.CreateInstance(GetType());
			rule.keyword = keyword;
			if (constantConstraints != null)
			{
				rule.constantConstraints = constantConstraints.ToList();
			}
			return rule;
		}

		public abstract string Generate();

		public virtual void Init()
		{
		}

		public void AddConstantConstraint(string key, string value, bool equality)
		{
			if (constantConstraints == null)
			{
				constantConstraints = new List<ConstantConstraint>();
			}
			constantConstraints.Add(new ConstantConstraint
			{
				key = key,
				value = value,
				equality = equality
			});
		}
	}
}
