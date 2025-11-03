using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SIGadgetListEntry : MonoBehaviour
{
	public int Id { get; private set; } = -1;

	public void Configure(ITouchScreenStation station, SITechTreePage page, Transform imageTarget, Transform textTarget, SITouchscreenButton.SITouchscreenButtonType buttonType = SITouchscreenButton.SITouchscreenButtonType.Select, int index = 0, float verticalOffset = 0f)
	{
		base.name = (this.gadgetText.text = page.nickName);
		SITouchscreenButton button = this.buttonContainer.button;
		button.buttonType = buttonType;
		this.Id = (button.data = (int)page.pageId);
		button.buttonPressed.RemoveAllListeners();
		button.buttonPressed.AddListener(new UnityAction<SITouchscreenButton.SITouchscreenButtonType, int, int>(station.TouchscreenButtonPressed));
		station.AddButton(button, false);
		base.transform.localPosition += new Vector3(0f, (float)index * verticalOffset, 0f);
		this.imageFlattener.overrideParentTransform = imageTarget;
		this.textFlattener.overrideParentTransform = textTarget;
		this.imageFlattener.enabled = true;
		this.textFlattener.enabled = true;
	}

	[SerializeField]
	private TextMeshProUGUI gadgetText;

	[SerializeField]
	private SITouchscreenButtonContainer buttonContainer;

	public ObjectHierarchyFlattener imageFlattener;

	public ObjectHierarchyFlattener textFlattener;

	public GameObject selectionIndicator;
}
