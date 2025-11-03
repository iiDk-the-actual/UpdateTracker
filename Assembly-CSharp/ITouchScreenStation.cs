using System;
using UnityEngine;

public interface ITouchScreenStation
{
	GameObject gameObject { get; }

	SIScreenRegion ScreenRegion { get; }

	void AddButton(SITouchscreenButton button, bool isPopupButton = false);

	void TouchscreenButtonPressed(SITouchscreenButton.SITouchscreenButtonType buttonType, int data, int actorNr);
}
