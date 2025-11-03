using System;
using UnityEngine;

public class KIDHandReference : MonoBehaviour
{
	public static GameObject LeftHand
	{
		get
		{
			return KIDHandReference._leftHandRef;
		}
	}

	public static GameObject RightHand
	{
		get
		{
			return KIDHandReference._rightHandRef;
		}
	}

	private void Awake()
	{
		KIDHandReference._leftHandRef = this._leftHand;
		KIDHandReference._rightHandRef = this._rightHand;
	}

	[SerializeField]
	private GameObject _leftHand;

	[SerializeField]
	private GameObject _rightHand;

	private static GameObject _leftHandRef;

	private static GameObject _rightHandRef;
}
