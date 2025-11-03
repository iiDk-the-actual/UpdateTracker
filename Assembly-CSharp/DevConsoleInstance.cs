using System;
using System.Collections.Generic;
using UnityEngine;

public class DevConsoleInstance : MonoBehaviour
{
	private void OnEnable()
	{
		base.gameObject.SetActive(false);
	}

	public GorillaDevButton[] buttons;

	public GameObject[] disableWhileActive;

	public GameObject[] enableWhileActive;

	public float maxHeight;

	public float lineHeight;

	public int targetLogIndex = -1;

	public int currentLogIndex;

	public int expandAmount = 20;

	public int expandedMessageIndex = -1;

	public bool canExpand = true;

	public List<DevConsole.DisplayedLogLine> logLines = new List<DevConsole.DisplayedLogLine>();

	public HashSet<LogType> selectedLogTypes = new HashSet<LogType>
	{
		LogType.Error,
		LogType.Exception,
		LogType.Log,
		LogType.Warning,
		LogType.Assert
	};

	[SerializeField]
	private GorillaDevButton[] logTypeButtons;

	[SerializeField]
	private GorillaDevButton BottomButton;

	public float lineStartHeight;

	public float lineStartZ;

	public float textStartHeight;

	public float lineStartTextWidth;

	public double textScale = 0.5;

	public bool isEnabled = true;

	[SerializeField]
	private GameObject ConsoleLineExample;
}
