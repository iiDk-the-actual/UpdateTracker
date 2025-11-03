using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class WeightedList<T>
{
	public int Count
	{
		get
		{
			return this.items.Count;
		}
	}

	public List<T> Items
	{
		get
		{
			return this.items;
		}
	}

	public void Add(T item, float weight)
	{
		if (weight <= 0f)
		{
			throw new ArgumentException("Weight must be greater than zero.");
		}
		this.totalWeight += weight;
		this.items.Add(item);
		this.weights.Add(weight);
		this.cumulativeWeights.Add(this.totalWeight);
	}

	[TupleElementNames(new string[] { "Item", "Weight" })]
	public ValueTuple<T, float> this[int index]
	{
		[return: TupleElementNames(new string[] { "Item", "Weight" })]
		get
		{
			if (index < 0 || index >= this.items.Count)
			{
				throw new IndexOutOfRangeException();
			}
			return new ValueTuple<T, float>(this.items[index], this.weights[index]);
		}
	}

	public T GetRandomItem()
	{
		return this.items[this.GetRandomIndex()];
	}

	public int GetRandomIndex()
	{
		if (this.items.Count == 0)
		{
			throw new InvalidOperationException("The list is empty.");
		}
		float num = Random.value * this.totalWeight;
		int num2 = this.cumulativeWeights.BinarySearch(num);
		if (num2 < 0)
		{
			num2 = ~num2;
		}
		return num2;
	}

	public bool Remove(T item)
	{
		int num = this.items.IndexOf(item);
		if (num == -1)
		{
			return false;
		}
		this.RemoveAt(num);
		return true;
	}

	public void RemoveAt(int index)
	{
		if (index < 0 || index >= this.items.Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		this.totalWeight -= this.weights[index];
		this.items.RemoveAt(index);
		this.weights.RemoveAt(index);
		this.RecalculateCumulativeWeights();
	}

	private void RecalculateCumulativeWeights()
	{
		this.cumulativeWeights.Clear();
		float num = 0f;
		foreach (float num2 in this.weights)
		{
			num += num2;
			this.cumulativeWeights.Add(num);
		}
		this.totalWeight = num;
	}

	public void Clear()
	{
		this.items.Clear();
		this.weights.Clear();
		this.cumulativeWeights.Clear();
		this.totalWeight = 0f;
	}

	private List<T> items = new List<T>();

	private List<float> weights = new List<float>();

	private List<float> cumulativeWeights = new List<float>();

	private float totalWeight;
}
