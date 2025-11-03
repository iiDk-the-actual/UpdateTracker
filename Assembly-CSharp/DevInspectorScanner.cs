using System;
using UnityEngine;
using UnityEngine.UI;

public class DevInspectorScanner : MonoBehaviour
{
	public Text hintTextOutput;

	public float scanDistance = 10f;

	public float scanAngle = 30f;

	public LayerMask scanLayerMask;

	public string targetComponentName;

	public float rayPerDegree = 10f;
}
