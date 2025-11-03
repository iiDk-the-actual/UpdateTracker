using System;
using UnityEngine;

public class PlatformerCollectiblesMain : MonoBehaviour
{
	public void Start()
	{
		int num = 0;
		while ((float)num < this.CoinGridCount)
		{
			float num2 = -0.5f * this.CoinGridSize + this.CoinGridSize * (float)num / (this.CoinGridCount - 1f);
			int num3 = 0;
			while ((float)num3 < this.CoinGridCount)
			{
				float num4 = -0.5f * this.CoinGridSize + this.CoinGridSize * (float)num3 / (this.CoinGridCount - 1f);
				Object.Instantiate<GameObject>(this.Coin).transform.position = new Vector3(num2, 0.2f, num4);
				num3++;
			}
			num++;
		}
	}

	public GameObject Coin;

	public float CoinGridCount = 5f;

	public float CoinGridSize = 7f;
}
