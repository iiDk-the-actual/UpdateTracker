using System;
using TMPro;
using UnityEngine;

public class GorillaDebugUI : MonoBehaviour
{
	private readonly float Delay = 0.5f;

	public GameObject parentCanvas;

	public GameObject rayInteractorLeft;

	public GameObject rayInteractorRight;

	[SerializeField]
	private TMP_Dropdown playfabIdDropdown;

	[SerializeField]
	private TMP_Dropdown roomIdDropdown;

	[SerializeField]
	private TMP_Dropdown locationDropdown;

	[SerializeField]
	private TMP_Dropdown playerNameDropdown;

	[SerializeField]
	private TMP_Dropdown gameModeDropdown;

	[SerializeField]
	private TMP_Dropdown timeOfDayDropdown;

	[SerializeField]
	private TMP_Text networkStateTextBox;

	[SerializeField]
	private TMP_Text gameModeTextBox;

	[SerializeField]
	private TMP_Text currentRoomTextBox;

	[SerializeField]
	private TMP_Text playerCountTextBox;

	[SerializeField]
	private TMP_Text roomVisibilityTextBox;

	[SerializeField]
	private TMP_Text timeMultiplierTextBox;

	[SerializeField]
	private TMP_Text versionTextBox;
}
