using System;

internal struct PlayerAgeGateWarningStatus
{
	public string header;

	public string body;

	public string leftButtonText;

	public string rightButtonText;

	public WarningButtonResult leftButtonResult;

	public WarningButtonResult rightButtonResult;

	public EImageVisibility showImage;

	public Action onLeftButtonPressedAction;

	public Action onRightButtonPressedAction;
}
