using System;
using UnityEngine;

[Serializable]
public class GRBonusEntry
{
	private GRBonusEntry()
	{
		GRBonusEntry.idCounter++;
		this.id = GRBonusEntry.idCounter;
	}

	public int id { get; private set; }

	public int GetBonusValue()
	{
		return (int)(this.bonusValue * 100f);
	}

	public override string ToString()
	{
		bool flag = this.customBonus != null;
		return string.Format("GRBonusEntry BonusType {0} AttributeType {1} BonusValue {2} Id {3} CustomBonusSet {4}", new object[] { this.bonusType, this.attributeType, this.bonusValue, this.id, flag });
	}

	private static int idCounter;

	public GRBonusEntry.GRBonusType bonusType;

	public GRAttributeType attributeType;

	[SerializeField]
	private float bonusValue;

	public Func<int, GRBonusEntry, int> customBonus;

	public enum GRBonusType
	{
		None,
		Additive,
		Multiplicative
	}
}
