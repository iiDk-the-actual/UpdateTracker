using System;
using TMPro;
using UnityEngine;

public class RandomizeLabel : MonoBehaviour
{
	public void Randomize()
	{
		this.strings.distinct = this.distinct;
		this.label.text = this.strings.NextItem();
	}

	public TMP_Text label;

	public RandomStrings strings;

	public bool distinct;
}
