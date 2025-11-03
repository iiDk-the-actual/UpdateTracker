using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevConsoleHand : DevConsoleInstance
{
	public List<GameObject> otherButtonsList;

	public bool isStillEnabled = true;

	public bool isLeftHand;

	public ConsoleMode mode;

	public double debugScale;

	public double inspectorScale;

	public double componentInspectorScale;

	public List<GameObject> consoleButtons;

	public List<GameObject> inspectorButtons;

	public List<GameObject> componentInspectorButtons;

	public GorillaDevButton consoleButton;

	public GorillaDevButton inspectorButton;

	public GorillaDevButton componentInspectorButton;

	public GorillaDevButton showNonStarItems;

	public GorillaDevButton showPrivateItems;

	public Text componentInspectionText;

	public DevInspector selectedInspector;
}
