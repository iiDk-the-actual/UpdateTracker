using System;
using UnityEngine;

namespace BoingKit
{
	[AttributeUsage(AttributeTargets.Field)]
	public class ConditionalFieldAttribute : PropertyAttribute
	{
		public bool ShowRange
		{
			get
			{
				return this.Min != this.Max;
			}
		}

		public ConditionalFieldAttribute(string propertyToCheck = null, object compareValue = null, object compareValue2 = null, object compareValue3 = null, object compareValue4 = null, object compareValue5 = null, object compareValue6 = null)
		{
			this.PropertyToCheck = propertyToCheck;
			this.CompareValue = compareValue;
			this.CompareValue2 = compareValue2;
			this.CompareValue3 = compareValue3;
			this.CompareValue4 = compareValue4;
			this.CompareValue5 = compareValue5;
			this.CompareValue6 = compareValue6;
			this.Label = "";
			this.Tooltip = "";
			this.Min = 0f;
			this.Max = 0f;
		}

		public string PropertyToCheck;

		public object CompareValue;

		public object CompareValue2;

		public object CompareValue3;

		public object CompareValue4;

		public object CompareValue5;

		public object CompareValue6;

		public string Label;

		public string Tooltip;

		public float Min;

		public float Max;
	}
}
