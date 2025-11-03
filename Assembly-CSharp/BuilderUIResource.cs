using System;
using GorillaTagScripts;
using TMPro;
using UnityEngine;

public class BuilderUIResource : MonoBehaviour
{
	public void SetResourceCost(BuilderResourceQuantity resourceCost, BuilderTable table)
	{
		BuilderResourceType type = resourceCost.type;
		int count = resourceCost.count;
		int availableResources = table.GetAvailableResources(type);
		if (this.resourceNameLabel != null)
		{
			this.resourceNameLabel.text = this.GetResourceName(type);
		}
		if (this.costLabel != null)
		{
			this.costLabel.text = count.ToString();
		}
		if (this.availableLabel != null)
		{
			this.availableLabel.text = availableResources.ToString();
		}
	}

	private string GetResourceName(BuilderResourceType type)
	{
		switch (type)
		{
		case BuilderResourceType.Basic:
			return "Basic";
		case BuilderResourceType.Decorative:
			return "Decorative";
		case BuilderResourceType.Functional:
			return "Functional";
		default:
			return "Resource Needs Name";
		}
	}

	public TextMeshPro resourceNameLabel;

	public TextMeshPro costLabel;

	public TextMeshPro availableLabel;
}
