using System;
using UnityEngine;

public class CosmeticRefRegistry : MonoBehaviour
{
	private void Awake()
	{
		foreach (CosmeticRefTarget cosmeticRefTarget in this.builtInRefTargets)
		{
			this.Register(cosmeticRefTarget.id, cosmeticRefTarget.gameObject);
		}
	}

	public void Register(CosmeticRefID partID, GameObject part)
	{
		this.partsTable[(int)partID] = part;
	}

	public GameObject Get(CosmeticRefID partID)
	{
		return this.partsTable[(int)partID];
	}

	private GameObject[] partsTable = new GameObject[8];

	[SerializeField]
	private CosmeticRefTarget[] builtInRefTargets;
}
