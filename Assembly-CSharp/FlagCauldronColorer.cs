using System;
using UnityEngine;

public class FlagCauldronColorer : MonoBehaviour
{
	public FlagCauldronColorer.ColorMode mode;

	public Transform colorPoint;

	public enum ColorMode
	{
		None,
		Red,
		Green,
		Blue,
		Black,
		Clear
	}
}
