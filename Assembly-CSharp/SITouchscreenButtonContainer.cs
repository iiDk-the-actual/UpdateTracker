using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SITouchscreenButtonContainer : MonoBehaviour
{
	public SITouchscreenButton.SITouchscreenButtonType type;

	public string buttonTextString;

	public int data;

	public Image backGround;

	public Image foreGround;

	public TextMeshProUGUI buttonText;

	public ITouchScreenStation station;

	public SITouchscreenButton button;

	[SerializeField]
	private bool autoConfigure = true;
}
