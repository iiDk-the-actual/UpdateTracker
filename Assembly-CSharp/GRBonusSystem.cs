using System;
using System.Collections.Generic;
using UnityEngine;

public class GRBonusSystem
{
	public void Init(GRAttributes attributes)
	{
		this.defaultAttributes = attributes;
	}

	public GRAttributes GetDefaultAttributes()
	{
		return this.defaultAttributes;
	}

	public void AddBonus(GRBonusEntry entry)
	{
		if (entry.bonusType == GRBonusEntry.GRBonusType.None)
		{
			return;
		}
		if (!this.currentAdditiveBonuses.ContainsKey(entry.attributeType))
		{
			this.currentAdditiveBonuses[entry.attributeType] = new List<GRBonusEntry>();
		}
		if (!this.currentMultiplicativeBonuses.ContainsKey(entry.attributeType))
		{
			this.currentMultiplicativeBonuses[entry.attributeType] = new List<GRBonusEntry>();
		}
		if (entry.bonusType == GRBonusEntry.GRBonusType.Additive)
		{
			this.currentAdditiveBonuses[entry.attributeType].Add(entry);
			return;
		}
		if (entry.bonusType == GRBonusEntry.GRBonusType.Multiplicative)
		{
			this.currentMultiplicativeBonuses[entry.attributeType].Add(entry);
		}
	}

	public void RemoveBonus(GRBonusEntry entry)
	{
		foreach (List<GRBonusEntry> list in this.currentAdditiveBonuses.Values)
		{
			list.Remove(entry);
		}
		foreach (List<GRBonusEntry> list2 in this.currentMultiplicativeBonuses.Values)
		{
			list2.Remove(entry);
		}
	}

	public bool HasValueForAttribute(GRAttributeType attributeType)
	{
		return this.defaultAttributes != null && this.defaultAttributes.defaultAttributes.ContainsKey(attributeType);
	}

	public int CalculateFinalValueForAttribute(GRAttributeType attributeType)
	{
		if (this.defaultAttributes == null)
		{
			Debug.LogErrorFormat("CalculateFinalValueForAttribute DefaultAttributes null.  Please fix configuration.", Array.Empty<object>());
			return 0;
		}
		if (!this.defaultAttributes.defaultAttributes.ContainsKey(attributeType))
		{
			Debug.LogErrorFormat("CalculateFinalValueForAttribute DefaultAttributes Does not have entry for {0}.  Please fix configuration.", new object[] { attributeType });
			return 0;
		}
		int num = this.defaultAttributes.defaultAttributes[attributeType];
		if (this.currentAdditiveBonuses.ContainsKey(attributeType))
		{
			foreach (GRBonusEntry grbonusEntry in this.currentAdditiveBonuses[attributeType])
			{
				if (grbonusEntry.customBonus != null)
				{
					num = grbonusEntry.customBonus(num, grbonusEntry);
				}
				else
				{
					num += grbonusEntry.GetBonusValue();
				}
			}
		}
		if (this.currentMultiplicativeBonuses.ContainsKey(attributeType))
		{
			foreach (GRBonusEntry grbonusEntry2 in this.currentMultiplicativeBonuses[attributeType])
			{
				if (grbonusEntry2.customBonus != null)
				{
					num = grbonusEntry2.customBonus(num, grbonusEntry2);
				}
				else
				{
					float num2 = (float)grbonusEntry2.GetBonusValue() / 100f;
					num = (int)((float)num * num2);
				}
			}
		}
		return num;
	}

	private GRAttributes defaultAttributes;

	private Dictionary<GRAttributeType, List<GRBonusEntry>> currentAdditiveBonuses = new Dictionary<GRAttributeType, List<GRBonusEntry>>();

	private Dictionary<GRAttributeType, List<GRBonusEntry>> currentMultiplicativeBonuses = new Dictionary<GRAttributeType, List<GRBonusEntry>>();
}
