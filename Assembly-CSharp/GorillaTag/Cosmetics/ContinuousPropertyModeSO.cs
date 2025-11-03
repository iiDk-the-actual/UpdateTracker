using System;
using System.Linq;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class ContinuousPropertyModeSO : ScriptableObject
	{
		private string GetTestDescription
		{
			get
			{
				if (this.castData.Length == 0)
				{
					return "";
				}
				return "Sample Description: " + this.GetDescriptionForCast(this.castData[0].target);
			}
		}

		public bool IsCastValid(ContinuousProperty.Cast cast)
		{
			for (int i = 0; i < this.castData.Length; i++)
			{
				if (ContinuousProperty.CastMatches(this.castData[i].target, cast))
				{
					return true;
				}
			}
			return false;
		}

		public ContinuousProperty.Cast GetClosestCast(ContinuousProperty.Cast cast)
		{
			for (int i = 0; i < this.castData.Length; i++)
			{
				if (ContinuousProperty.CastMatches(this.castData[i].target, cast))
				{
					return this.castData[i].target;
				}
			}
			return ContinuousProperty.Cast.Null;
		}

		public ContinuousProperty.DataFlags GetFlagsForCast(ContinuousProperty.Cast cast)
		{
			for (int i = 0; i < this.castData.Length; i++)
			{
				if (this.castData[i].target == cast)
				{
					return this.castData[i].additionalFlags | this.flags;
				}
			}
			return this.flags;
		}

		public ContinuousProperty.DataFlags GetFlagsForClosestCast(ContinuousProperty.Cast cast)
		{
			for (int i = 0; i < this.castData.Length; i++)
			{
				if (ContinuousProperty.CastMatches(this.castData[i].target, cast))
				{
					return this.castData[i].additionalFlags | this.flags;
				}
			}
			return this.flags;
		}

		public string GetDescriptionForCast(ContinuousProperty.Cast cast)
		{
			for (int i = 0; i < this.castData.Length; i++)
			{
				if (ContinuousProperty.CastMatches(this.castData[i].target, cast) || this.castData.Length == 1)
				{
					if (!this.replaceDescription.IsNullOrEmpty())
					{
						return this.replaceDescription;
					}
					switch (this.descriptionStyle)
					{
					case ContinuousPropertyModeSO.DescriptionStyle.Continuous:
						return string.Concat(new string[]
						{
							"sets the ",
							this.castData[i].whatItSets,
							" on the ",
							this.castData[i].target.ToString(),
							" using the height of the curve at the provided time.",
							(" " + this.afterSentence).TrimEnd()
						});
					case ContinuousPropertyModeSO.DescriptionStyle.SingleThreshold:
						return this.castData[i].whatItSets + " the " + this.type.ToString() + " when entering the 'true' part of the range.";
					case ContinuousPropertyModeSO.DescriptionStyle.DualThreshold:
					{
						string[] array = this.castData[i].whatItSets.Split('|', StringSplitOptions.None);
						if (array.Length != 2)
						{
							return string.Format("Error! '{0}'s '{1}.{2}' does not have two string separated by '|'.", base.name, this.castData[i].target, "whatItSets");
						}
						return string.Concat(new string[]
						{
							array[0],
							" the ",
							this.castData[i].target.ToString(),
							" when entering the 'true' part of the range, ",
							array[1],
							" the ",
							this.castData[i].target.ToString(),
							" when entering the 'false' part of the range."
						});
					}
					}
				}
			}
			return "Invalid target\n\n" + this.ListValidCasts();
		}

		public string ListValidCasts()
		{
			return "Valid targets: " + string.Join<ContinuousProperty.Cast>(", ", this.castData.Select((ContinuousPropertyModeSO.CastData x) => x.target));
		}

		public ContinuousProperty.Type type;

		public ContinuousProperty.DataFlags flags;

		public ContinuousPropertyModeSO.CastData[] castData;

		[Space]
		public ContinuousPropertyModeSO.DescriptionStyle descriptionStyle;

		[TextArea]
		public string afterSentence;

		[TextArea]
		public string replaceDescription;

		[Serializable]
		public struct CastData
		{
			public ContinuousProperty.Cast target;

			public ContinuousProperty.DataFlags additionalFlags;

			public string whatItSets;
		}

		public enum DescriptionStyle
		{
			Continuous,
			SingleThreshold,
			DualThreshold
		}
	}
}
