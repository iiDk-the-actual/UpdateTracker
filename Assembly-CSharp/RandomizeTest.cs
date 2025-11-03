using System;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeTest : MonoBehaviour
{
	private void Start()
	{
		for (int i = 0; i < 10; i++)
		{
			this.testList.Add(i);
		}
		for (int j = 0; j < 10; j++)
		{
			this.testListArray[j] = 0;
		}
		for (int k = 0; k < this.testList.Count; k++)
		{
			this.testListArray[k] = this.testList[k];
		}
		this.RandomizeList(ref this.testList);
		for (int l = 0; l < 10; l++)
		{
			this.testListArray[l] = 0;
		}
		for (int m = 0; m < this.testList.Count; m++)
		{
			this.testListArray[m] = this.testList[m];
		}
	}

	public void RandomizeList(ref List<int> listToRandomize)
	{
		this.randomIterator = 0;
		while (this.randomIterator < listToRandomize.Count)
		{
			this.tempRandIndex = Random.Range(this.randomIterator, listToRandomize.Count);
			this.tempRandValue = listToRandomize[this.randomIterator];
			listToRandomize[this.randomIterator] = listToRandomize[this.tempRandIndex];
			listToRandomize[this.tempRandIndex] = this.tempRandValue;
			this.randomIterator++;
		}
	}

	public List<int> testList = new List<int>();

	public int[] testListArray = new int[10];

	public int randomIterator;

	public int tempRandIndex;

	public int tempRandValue;
}
