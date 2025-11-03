using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SIDispenserGadgetListEntry : MonoBehaviour
{
	public void SetStation(ITouchScreenStation station, Transform imageTarget, Transform textTarget)
	{
		this.dispenseButton.button.buttonPressed.RemoveAllListeners();
		this.dispenseButton.button.buttonPressed.AddListener(new UnityAction<SITouchscreenButton.SITouchscreenButtonType, int, int>(station.TouchscreenButtonPressed));
		this.infoButton.button.buttonPressed.RemoveAllListeners();
		this.infoButton.button.buttonPressed.AddListener(new UnityAction<SITouchscreenButton.SITouchscreenButtonType, int, int>(station.TouchscreenButtonPressed));
		station.AddButton(this.dispenseButton.button, false);
		station.AddButton(this.infoButton.button, false);
		this.image1.overrideParentTransform = imageTarget;
		this.image2.overrideParentTransform = imageTarget;
		this.text1.overrideParentTransform = textTarget;
		this.text2.overrideParentTransform = textTarget;
		this.image1.enabled = true;
		this.image2.enabled = true;
		this.text1.enabled = true;
		this.text2.enabled = true;
	}

	public void SetTechTreeNode(SITechTreeNode node)
	{
		base.name = (this.gadgetText.text = node.nickName);
		int nodeId = node.upgradeType.GetNodeId();
		SIDispenserGadgetListEntry.<SetTechTreeNode>g__ConfigureButton|8_0(this.dispenseButton.button, SITouchscreenButton.SITouchscreenButtonType.Dispense, nodeId);
		SIDispenserGadgetListEntry.<SetTechTreeNode>g__ConfigureButton|8_0(this.infoButton.button, SITouchscreenButton.SITouchscreenButtonType.Select, nodeId);
	}

	[CompilerGenerated]
	internal static void <SetTechTreeNode>g__ConfigureButton|8_0(SITouchscreenButton button, SITouchscreenButton.SITouchscreenButtonType type, int data)
	{
		button.buttonType = type;
		button.data = data;
	}

	[SerializeField]
	private TextMeshProUGUI gadgetText;

	[SerializeField]
	private SITouchscreenButtonContainer dispenseButton;

	[SerializeField]
	private SITouchscreenButtonContainer infoButton;

	public ObjectHierarchyFlattener image1;

	public ObjectHierarchyFlattener image2;

	public ObjectHierarchyFlattener text1;

	public ObjectHierarchyFlattener text2;
}
