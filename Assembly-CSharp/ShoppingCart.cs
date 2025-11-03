using System;
using UnityEngine;

public class ShoppingCart : MonoBehaviour
{
	public void Awake()
	{
		if (ShoppingCart.instance == null)
		{
			ShoppingCart.instance = this;
			return;
		}
		if (ShoppingCart.instance != this)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	[OnEnterPlay_SetNull]
	public static volatile ShoppingCart instance;
}
