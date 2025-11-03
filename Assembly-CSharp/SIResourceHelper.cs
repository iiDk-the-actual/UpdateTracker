using System;
using System.Collections.Generic;

public static class SIResourceHelper
{
	public static bool IsInOrder(this IList<SIResource.ResourceCost> cost)
	{
		SIResource.ResourceType resourceType = (SIResource.ResourceType)(-1);
		foreach (SIResource.ResourceCost resourceCost in cost)
		{
			if (resourceCost.type <= resourceType)
			{
				return false;
			}
			resourceType = resourceCost.type;
		}
		return true;
	}

	public static bool IsValid(this IList<SIResource.ResourceCost> cost)
	{
		if (cost == null || cost.Count == 0)
		{
			return false;
		}
		int num = 0;
		foreach (SIResource.ResourceCost resourceCost in cost)
		{
			int num2 = 1 << (int)resourceCost.type;
			if ((num & num2) != 0)
			{
				return false;
			}
			if (resourceCost.amount <= 0)
			{
				return false;
			}
			num |= num2;
		}
		return true;
	}

	public static bool IsValid_AllowZero(this IList<SIResource.ResourceCost> cost)
	{
		if (cost == null)
		{
			return false;
		}
		int num = 0;
		foreach (SIResource.ResourceCost resourceCost in cost)
		{
			int num2 = 1 << (int)resourceCost.type;
			if ((num & num2) != 0)
			{
				return false;
			}
			if (resourceCost.amount < 0)
			{
				return false;
			}
			num |= num2;
		}
		return true;
	}

	public static SIResource.ResourceCategoryCost GetCategoryCosts(this IList<SIResource.ResourceCost> costs)
	{
		int num = 0;
		int num2 = 0;
		foreach (SIResource.ResourceCost resourceCost in costs)
		{
			if (resourceCost.type == SIResource.ResourceType.TechPoint)
			{
				num += resourceCost.amount;
			}
			else
			{
				num2 += resourceCost.amount;
			}
		}
		return new SIResource.ResourceCategoryCost(num, num2);
	}

	public static List<SIResource.ResourceCost> GetTotalResourceCost(this IList<SIResource.ResourceCost> baseCost, IList<SIResource.ResourceCost> additiveCosts)
	{
		List<SIResource.ResourceCost> list = new List<SIResource.ResourceCost>(baseCost);
		foreach (SIResource.ResourceCost resourceCost in additiveCosts)
		{
			list.Add(resourceCost);
		}
		return list;
	}

	public static List<SIResource.ResourceCost> GetMax(this IList<SIResource.ResourceCost> baseCost, IList<SIResource.ResourceCost> additiveCosts)
	{
		List<SIResource.ResourceCost> list = new List<SIResource.ResourceCost>(baseCost);
		foreach (SIResource.ResourceCost resourceCost in additiveCosts)
		{
			list.Add(resourceCost);
		}
		list.Sort();
		return list;
	}

	public static int GetAmount(this IList<SIResource.ResourceCost> costs, SIResource.ResourceType resourceType)
	{
		foreach (SIResource.ResourceCost resourceCost in costs)
		{
			if (resourceCost.type == resourceType)
			{
				return resourceCost.amount;
			}
		}
		return 0;
	}

	public static void SetAmount(this List<SIResource.ResourceCost> costs, SIResource.ResourceType resourceType, int amount)
	{
		for (int i = 0; i < costs.Count; i++)
		{
			SIResource.ResourceCost resourceCost = costs[i];
			if (resourceCost.type == resourceType)
			{
				resourceCost.amount = amount;
				costs[i] = resourceCost;
				return;
			}
		}
		costs.Add(new SIResource.ResourceCost(resourceType, amount));
	}

	public static void AddResourceCost(this List<SIResource.ResourceCost> baseCost, SIResource.ResourceCost additiveCost)
	{
		for (int i = 0; i < baseCost.Count; i++)
		{
			SIResource.ResourceCost resourceCost = baseCost[i];
			if (resourceCost.type == additiveCost.type)
			{
				resourceCost.amount += additiveCost.amount;
				baseCost[i] = resourceCost;
				return;
			}
		}
		baseCost.Add(additiveCost);
	}

	public static void AddResourceCost(this List<SIResource.ResourceCost> baseCost, IList<SIResource.ResourceCost> additiveCost)
	{
		foreach (SIResource.ResourceCost resourceCost in additiveCost)
		{
			baseCost.AddResourceCost(resourceCost);
		}
	}

	public static int GetTechPointCost(this IList<SIResource.ResourceCost> costs)
	{
		int num = 0;
		foreach (SIResource.ResourceCost resourceCost in costs)
		{
			if (resourceCost.type == SIResource.ResourceType.TechPoint)
			{
				num += resourceCost.amount;
			}
		}
		return num;
	}

	public static int GetMiscCost(this IList<SIResource.ResourceCost> costs)
	{
		int num = 0;
		foreach (SIResource.ResourceCost resourceCost in costs)
		{
			if (resourceCost.type != SIResource.ResourceType.TechPoint)
			{
				num += resourceCost.amount;
			}
		}
		return num;
	}

	public static void SetResourceCost(this IList<SIResource.ResourceCost> costs, SIResource.ResourceCategoryCost desiredCosts)
	{
		costs.SetTechPointCost(desiredCosts.techPoints);
		costs.SetMiscCost(desiredCosts.misc);
	}

	public static void AddResourceCost(this IList<SIResource.ResourceCost> baseCost, SIResource.ResourceCategoryCost additiveCost)
	{
		baseCost.SetTechPointCost(baseCost.GetTechPointCost() + additiveCost.techPoints);
		baseCost.SetMiscCost(baseCost.GetMiscCost() + additiveCost.misc);
	}

	public static void SetTechPointCost(this IList<SIResource.ResourceCost> baseCost, int desiredCost)
	{
		for (int i = 0; i < baseCost.Count; i++)
		{
			SIResource.ResourceCost resourceCost = baseCost[i];
			if (resourceCost.type == SIResource.ResourceType.TechPoint)
			{
				resourceCost.amount = desiredCost;
				baseCost[i] = resourceCost;
				return;
			}
		}
		baseCost.Add(new SIResource.ResourceCost(SIResource.ResourceType.TechPoint, desiredCost));
	}

	public static void SetMiscCost(this IList<SIResource.ResourceCost> baseCost, int desiredCost)
	{
		int num = baseCost.GetMiscCost();
		if (num == desiredCost)
		{
			return;
		}
		for (int i = 0; i < baseCost.Count; i++)
		{
			SIResource.ResourceCost resourceCost = baseCost[i];
			if (resourceCost.type != SIResource.ResourceType.TechPoint)
			{
				resourceCost.amount += desiredCost - num;
				if (resourceCost.amount >= 1)
				{
					baseCost[i] = resourceCost;
					return;
				}
				baseCost.RemoveAt(i--);
				num = baseCost.GetMiscCost();
				if (num == desiredCost)
				{
					return;
				}
			}
		}
		if (desiredCost == num)
		{
			return;
		}
		baseCost.Add(new SIResource.ResourceCost(SIResource.ResourceType.StrangeWood, desiredCost - num));
	}
}
