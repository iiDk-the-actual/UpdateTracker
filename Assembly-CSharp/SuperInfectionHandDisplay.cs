using System;
using UnityEngine;

public class SuperInfectionHandDisplay : MonoBehaviour
{
	public void EnableHands(bool on)
	{
		for (int i = 0; i < this.gameObjects.Length; i++)
		{
			this.gameObjects[i].SetActive(on);
		}
	}

	[SerializeField]
	private GameObject[] gameObjects;
}
