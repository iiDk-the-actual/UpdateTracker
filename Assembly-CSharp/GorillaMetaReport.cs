using System;
using System.Collections;
using System.Runtime.CompilerServices;
using GorillaLocomotion;
using Oculus.Platform;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class GorillaMetaReport : MonoBehaviour
{
	private GTPlayer localPlayer
	{
		get
		{
			return GTPlayer.Instance;
		}
	}

	private void Start()
	{
		this.localPlayer.inOverlay = false;
		MothershipClientApiUnity.OnMessageNotificationSocket += this.OnNotification;
		base.gameObject.SetActive(false);
	}

	private void OnDisable()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		this.localPlayer.inOverlay = false;
		base.StopAllCoroutines();
	}

	private void OnReportButtonIntentNotif(Message<string> message)
	{
		if (message.IsError)
		{
			AbuseReport.ReportRequestHandled(ReportRequestResponse.Unhandled);
			return;
		}
		if (!PhotonNetwork.InRoom)
		{
			this.ReportText.SetActive(true);
			AbuseReport.ReportRequestHandled(ReportRequestResponse.Handled);
			this.StartOverlay(false);
			return;
		}
		if (!message.IsError)
		{
			AbuseReport.ReportRequestHandled(ReportRequestResponse.Handled);
			this.StartOverlay(false);
		}
	}

	private void OnNotification(NotificationsMessageResponse notification, [NativeInteger] IntPtr _)
	{
		string title = notification.Title;
		if (title == "Warning")
		{
			this.OnWarning(notification.Body);
			GorillaTelemetry.PostNotificationEvent("Warning");
			return;
		}
		if (title == "Mute")
		{
			this.OnMuteSanction(notification.Body);
			GorillaTelemetry.PostNotificationEvent("Mute");
			return;
		}
		if (!(title == "Unmute"))
		{
			return;
		}
		if (GorillaTagger.hasInstance)
		{
			GorillaTagger.moderationMutedTime = -1f;
		}
		GorillaTelemetry.PostNotificationEvent("Unmute");
	}

	private void OnWarning(string warningNotification)
	{
		string[] array = warningNotification.Split('|', StringSplitOptions.None);
		if (array.Length != 2)
		{
			Debug.LogError("Invalid warning notification");
			return;
		}
		string text = array[0];
		string[] array2 = array[1].Split(',', StringSplitOptions.None);
		if (array2.Length == 0)
		{
			Debug.LogError("Missing warning notification reasons");
			return;
		}
		string text2 = GorillaMetaReport.FormatListToString(in array2);
		this.ReportText.GetComponent<Text>().text = text.ToUpper() + " WARNING FOR " + text2.ToUpper();
		this.StartOverlay(true);
	}

	private void OnMuteSanction(string muteNotification)
	{
		string[] array = muteNotification.Split('|', StringSplitOptions.None);
		if (array.Length != 3)
		{
			Debug.LogError("Invalid mute notification");
			return;
		}
		if (!array[0].Equals("voice", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		int num;
		if (array[2].Length > 0 && int.TryParse(array[2], out num))
		{
			int num2 = num / 60;
			this.ReportText.GetComponent<Text>().text = string.Format("MUTED FOR {0} MINUTES\nBAD MONKE", num2);
			if (GorillaTagger.hasInstance)
			{
				GorillaTagger.moderationMutedTime = (float)num;
			}
		}
		else
		{
			this.ReportText.GetComponent<Text>().text = "MUTED FOREVER";
			if (GorillaTagger.hasInstance)
			{
				GorillaTagger.moderationMutedTime = float.PositiveInfinity;
			}
		}
		this.StartOverlay(true);
	}

	private static string FormatListToString(in string[] list)
	{
		int num = list.Length;
		string text3;
		if (num != 1)
		{
			if (num != 2)
			{
				string text = RuntimeHelpers.GetSubArray<string>(list, Range.EndAt(new Index(1, true))).Join(", ");
				string text2 = ", AND ";
				string[] array = list;
				text3 = text + text2 + array[array.Length - 1];
			}
			else
			{
				text3 = list[0] + " AND " + list[1];
			}
		}
		else
		{
			text3 = list[0];
		}
		return text3;
	}

	private IEnumerator Submitted()
	{
		yield return new WaitForSeconds(1.5f);
		this.Teardown();
		yield break;
	}

	private void DuplicateScoreboard()
	{
		this.currentScoreboard.gameObject.SetActive(true);
		if (GorillaScoreboardTotalUpdater.instance != null)
		{
			GorillaScoreboardTotalUpdater.instance.UpdateScoreboard(this.currentScoreboard);
		}
		Vector3 vector;
		Quaternion quaternion;
		Vector3 vector2;
		this.GetIdealScreenPositionRotation(out vector, out quaternion, out vector2);
		this.currentScoreboard.transform.SetPositionAndRotation(vector, quaternion);
		this.reportScoreboard.transform.SetPositionAndRotation(vector, quaternion);
	}

	private void ToggleLevelVisibility(bool state)
	{
		Camera component = GorillaTagger.Instance.mainCamera.GetComponent<Camera>();
		if (state)
		{
			component.cullingMask = this.savedCullingLayers;
			return;
		}
		this.savedCullingLayers = component.cullingMask;
		component.cullingMask = this.visibleLayers;
	}

	private void Teardown()
	{
		this.ReportText.GetComponent<Text>().text = "NOT CURRENTLY CONNECTED TO A ROOM";
		this.ReportText.SetActive(false);
		this.localPlayer.inOverlay = false;
		this.localPlayer.disableMovement = false;
		this.closeButton.selected = false;
		this.closeButton.isOn = false;
		this.closeButton.UpdateColor();
		this.localPlayer.InReportMenu = false;
		this.ToggleLevelVisibility(true);
		base.gameObject.SetActive(false);
		foreach (GorillaPlayerScoreboardLine gorillaPlayerScoreboardLine in this.currentScoreboard.lines)
		{
			gorillaPlayerScoreboardLine.doneReporting = false;
		}
		GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
	}

	private void CheckReportSubmit()
	{
		if (this.currentScoreboard == null)
		{
			return;
		}
		foreach (GorillaPlayerScoreboardLine gorillaPlayerScoreboardLine in this.currentScoreboard.lines)
		{
			if (gorillaPlayerScoreboardLine.doneReporting)
			{
				this.ReportText.SetActive(true);
				this.ReportText.GetComponent<Text>().text = "REPORTED " + gorillaPlayerScoreboardLine.playerNameVisible;
				this.currentScoreboard.gameObject.SetActive(false);
				base.StartCoroutine(this.Submitted());
			}
		}
	}

	private void GetIdealScreenPositionRotation(out Vector3 position, out Quaternion rotation, out Vector3 scale)
	{
		GameObject mainCamera = GorillaTagger.Instance.mainCamera;
		rotation = Quaternion.Euler(0f, mainCamera.transform.eulerAngles.y, 0f);
		scale = this.localPlayer.turnParent.transform.localScale;
		position = mainCamera.transform.position + rotation * this.playerLocalScreenPosition * scale.x;
	}

	private void StartOverlay(bool isSanction = false)
	{
		Vector3 vector;
		Quaternion quaternion;
		Vector3 vector2;
		this.GetIdealScreenPositionRotation(out vector, out quaternion, out vector2);
		this.currentScoreboard.transform.localScale = vector2 * 2f;
		this.reportScoreboard.transform.localScale = vector2;
		this.leftHandObject.transform.localScale = vector2;
		this.rightHandObject.transform.localScale = vector2;
		this.occluder.transform.localScale = vector2;
		if (this.localPlayer.InReportMenu && !PhotonNetwork.InRoom)
		{
			return;
		}
		this.localPlayer.InReportMenu = true;
		this.localPlayer.disableMovement = true;
		this.localPlayer.inOverlay = true;
		base.gameObject.SetActive(true);
		if (PhotonNetwork.InRoom && !isSanction)
		{
			this.DuplicateScoreboard();
		}
		else
		{
			this.ReportText.SetActive(true);
			this.reportScoreboard.transform.SetPositionAndRotation(vector, quaternion);
			this.currentScoreboard.transform.SetPositionAndRotation(vector, quaternion);
		}
		this.ToggleLevelVisibility(false);
		Transform controllerTransform = this.localPlayer.GetControllerTransform(true);
		Transform controllerTransform2 = this.localPlayer.GetControllerTransform(false);
		this.rightHandObject.transform.SetPositionAndRotation(controllerTransform2.position, controllerTransform2.rotation);
		this.leftHandObject.transform.SetPositionAndRotation(controllerTransform.position, controllerTransform.rotation);
		if (isSanction)
		{
			this.currentScoreboard.gameObject.SetActive(false);
			return;
		}
		this.currentScoreboard.gameObject.SetActive(true);
	}

	private void CheckDistance()
	{
		Vector3 vector;
		Quaternion quaternion;
		Vector3 vector2;
		this.GetIdealScreenPositionRotation(out vector, out quaternion, out vector2);
		float num = Vector3.Distance(this.reportScoreboard.transform.position, vector);
		float num2 = 1f;
		if (num > num2 && !this.isMoving)
		{
			this.isMoving = true;
			this.movementTime = 0f;
		}
		if (this.isMoving)
		{
			this.movementTime += Time.deltaTime;
			float num3 = this.movementTime;
			this.reportScoreboard.transform.SetPositionAndRotation(Vector3.Lerp(this.reportScoreboard.transform.position, vector, num3), Quaternion.Lerp(this.reportScoreboard.transform.rotation, quaternion, num3));
			if (this.currentScoreboard != null)
			{
				this.currentScoreboard.transform.SetPositionAndRotation(Vector3.Lerp(this.currentScoreboard.transform.position, vector, num3), Quaternion.Lerp(this.currentScoreboard.transform.rotation, quaternion, num3));
			}
			if (num3 >= 1f)
			{
				this.isMoving = false;
				this.movementTime = 0f;
			}
		}
	}

	private void Update()
	{
		if (this.blockButtonsUntilTimestamp > Time.time)
		{
			return;
		}
		if (SteamVR_Actions.gorillaTag_System.GetState(SteamVR_Input_Sources.LeftHand) && this.localPlayer.InReportMenu)
		{
			this.Teardown();
			this.blockButtonsUntilTimestamp = Time.time + 0.75f;
		}
		if (this.localPlayer.InReportMenu)
		{
			this.localPlayer.inOverlay = true;
			this.occluder.transform.position = GorillaTagger.Instance.mainCamera.transform.position;
			Transform controllerTransform = this.localPlayer.GetControllerTransform(true);
			Transform controllerTransform2 = this.localPlayer.GetControllerTransform(false);
			this.rightHandObject.transform.SetPositionAndRotation(controllerTransform2.position, controllerTransform2.rotation);
			this.leftHandObject.transform.SetPositionAndRotation(controllerTransform.position, controllerTransform.rotation);
			this.CheckDistance();
			this.CheckReportSubmit();
		}
		if (this.closeButton.selected)
		{
			this.Teardown();
		}
		if (this.testPress)
		{
			this.testPress = false;
			this.StartOverlay(false);
		}
	}

	[SerializeField]
	private GameObject occluder;

	[SerializeField]
	private GameObject reportScoreboard;

	[SerializeField]
	private GameObject ReportText;

	[SerializeField]
	private LayerMask visibleLayers;

	[SerializeField]
	private GorillaReportButton closeButton;

	[SerializeField]
	private GameObject leftHandObject;

	[SerializeField]
	private GameObject rightHandObject;

	[SerializeField]
	private Vector3 playerLocalScreenPosition;

	private float blockButtonsUntilTimestamp;

	[SerializeField]
	private GorillaScoreBoard currentScoreboard;

	private int savedCullingLayers;

	public bool testPress;

	public bool isMoving;

	private float movementTime;
}
