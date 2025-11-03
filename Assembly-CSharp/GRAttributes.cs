using System;
using System.Collections.Generic;
using UnityEngine;

public class GRAttributes : MonoBehaviour
{
	private void Awake()
	{
		foreach (GRAttributes.GRAttributePair grattributePair in this.startingAttributes)
		{
			this.defaultAttributes[grattributePair.type] = (int)(grattributePair.value * 100f);
		}
		this.bonusSystem.Init(this);
	}

	public bool HasBeenInitialized()
	{
		return this.bonusSystem.GetDefaultAttributes() != null;
	}

	public void AddAttribute(GRAttributeType type, float value)
	{
		this.defaultAttributes[type] = (int)(value * 100f);
	}

	public void AddBonus(GRBonusEntry entry)
	{
		this.bonusSystem.AddBonus(entry);
	}

	public void RemoveBonus(GRBonusEntry entry)
	{
		this.bonusSystem.RemoveBonus(entry);
	}

	public float CalculateFinalFloatValueForAttribute(GRAttributeType attributeType)
	{
		int num = this.bonusSystem.CalculateFinalValueForAttribute(attributeType);
		float num2 = 0f;
		if (num > 0)
		{
			num2 = (float)num / 100f;
		}
		return num2;
	}

	public int CalculateFinalValueForAttribute(GRAttributeType attributeType)
	{
		int num = this.bonusSystem.CalculateFinalValueForAttribute(attributeType);
		if (num > 0)
		{
			num /= 100;
		}
		return num;
	}

	public bool HasValueForAttribute(GRAttributeType attributeType)
	{
		return this.bonusSystem.HasValueForAttribute(attributeType);
	}

	[SerializeField]
	private List<GRAttributes.GRAttributePair> startingAttributes;

	[NonSerialized]
	private GRBonusSystem bonusSystem = new GRBonusSystem();

	public Dictionary<GRAttributeType, int> defaultAttributes = new Dictionary<GRAttributeType, int>();

	[Serializable]
	public struct GRAttributePair
	{
		public GRAttributeType type;

		public float value;
	}
}
