using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ModIOTermsOfUse_v1 : MonoBehaviour
{
	private void OnEnable()
	{
		if (ControllerBehaviour.Instance)
		{
			ControllerBehaviour.Instance.OnAction += this.PostUpdate;
		}
	}

	private void OnDisable()
	{
		if (ControllerBehaviour.Instance)
		{
			ControllerBehaviour.Instance.OnAction -= this.PostUpdate;
		}
	}

	private void PostUpdate()
	{
		if (ControllerBehaviour.Instance.IsLeftStick)
		{
			this.TurnPage(-1);
		}
		if (ControllerBehaviour.Instance.IsRightStick)
		{
			this.TurnPage(1);
		}
		if (this.waitingForAcknowledge)
		{
			this.acceptButtonDown = ControllerBehaviour.Instance.ButtonDown;
		}
	}

	private async void Start()
	{
		while (!this.hasTermsOfUse)
		{
			await Task.Yield();
		}
		PrivateUIRoom.AddUI(this.uiParent);
		TaskAwaiter<bool> taskAwaiter = this.UpdateTextFromTerms().GetAwaiter();
		if (!taskAwaiter.IsCompleted)
		{
			await taskAwaiter;
			TaskAwaiter<bool> taskAwaiter2;
			taskAwaiter = taskAwaiter2;
			taskAwaiter2 = default(TaskAwaiter<bool>);
		}
		if (taskAwaiter.GetResult())
		{
			await this.WaitForAcknowledgement();
			Action<bool> action = this.termsAcknowledgedCallback;
			if (action != null)
			{
				action(this.accepted);
			}
			PrivateUIRoom.RemoveUI(this.uiParent);
			Object.Destroy(base.gameObject);
			return;
		}
		for (;;)
		{
			await Task.Yield();
		}
	}

	private async Task<bool> UpdateTextFromTerms()
	{
		this.tmpTitle.text = this.title;
		this.tmpBody.text = "Loading...";
		bool flag = await this.UpdateTextWithFullTerms();
		if (!flag)
		{
			this.tmpBody.text = "Failed to retrieve full Terms of Use text from mod.io.\n\nPlease restart the game and try again.";
			this.tmpBody.pageToDisplay = 1;
			this.tmpPage.text = string.Empty;
		}
		return flag;
	}

	public async Task<bool> UpdateTextWithFullTerms()
	{
		return true;
	}

	private string GetStringForListItemIdx_LowerAlpha(int idx)
	{
		switch (idx)
		{
		case 0:
			return "  a. <indent=5%>";
		case 1:
			return "  b. <indent=5%>";
		case 2:
			return "  c. <indent=5%>";
		case 3:
			return "  d. <indent=5%>";
		case 4:
			return "  e. <indent=5%>";
		case 5:
			return "  f. <indent=5%>";
		case 6:
			return "  g. <indent=5%>";
		case 7:
			return "  h. <indent=5%>";
		case 8:
			return "  i. <indent=5%>";
		case 9:
			return "  j. <indent=5%>";
		case 10:
			return "  k. <indent=5%>";
		case 11:
			return "  l. <indent=5%>";
		case 12:
			return "  m. <indent=5%>";
		case 13:
			return "  n. <indent=5%>";
		case 14:
			return "  o. <indent=5%>";
		case 15:
			return "  p. <indent=5%>";
		case 16:
			return "  q. <indent=5%>";
		case 17:
			return "  r. <indent=5%>";
		case 18:
			return "  s. <indent=5%>";
		case 19:
			return "  t. <indent=5%>";
		case 20:
			return "  u. <indent=5%>";
		case 21:
			return "  v. <indent=5%>";
		case 22:
			return "  w. <indent=5%>";
		case 23:
			return "  x. <indent=5%>";
		case 24:
			return "  y. <indent=5%>";
		case 25:
			return "  z. <indent=5%>";
		default:
			return "";
		}
	}

	private async Task WaitForAcknowledgement()
	{
		this.accepted = false;
		float progress = 0f;
		this.progressBar.transform.localScale = new Vector3(0f, 1f, 1f);
		while (progress < 1f)
		{
			if (this.acceptButtonDown)
			{
				progress += Time.deltaTime / this.holdTime;
			}
			else
			{
				progress = 0f;
			}
			this.progressBar.transform.localScale = new Vector3(Mathf.Clamp01(progress), 1f, 1f);
			this.progressBar.textureScale = new Vector2(Mathf.Clamp01(progress), -1f);
			await Task.Yield();
		}
		if (progress >= 1f)
		{
			this.Acknowledge(this.acceptButtonDown);
		}
	}

	public void TurnPage(int i)
	{
		this.tmpBody.pageToDisplay = Mathf.Clamp(this.tmpBody.pageToDisplay + i, 1, this.tmpBody.textInfo.pageCount);
		this.tmpPage.text = string.Format("page {0} of {1}", this.tmpBody.pageToDisplay, this.tmpBody.textInfo.pageCount);
		this.nextButton.SetActive(this.tmpBody.pageToDisplay < this.tmpBody.textInfo.pageCount);
		this.prevButton.SetActive(this.tmpBody.pageToDisplay > 1);
		this.ActivateAcceptButtonGroup();
	}

	private void ActivateAcceptButtonGroup()
	{
		bool flag = this.tmpBody.pageToDisplay == this.tmpBody.textInfo.pageCount;
		this.yesNoButtons.SetActive(flag);
		this.waitingForAcknowledge = flag;
	}

	public void Acknowledge(bool didAccept)
	{
		this.accepted = didAccept;
	}

	[SerializeField]
	private Transform uiParent;

	[SerializeField]
	private string title;

	[SerializeField]
	private TMP_Text tmpBody;

	[SerializeField]
	private TMP_Text tmpTitle;

	[SerializeField]
	private TMP_Text tmpPage;

	[SerializeField]
	public GameObject yesNoButtons;

	[SerializeField]
	public GameObject nextButton;

	[SerializeField]
	public GameObject prevButton;

	private bool hasTermsOfUse;

	private Action<bool> termsAcknowledgedCallback;

	private string cachedTermsText;

	private bool waitingForAcknowledge;

	private bool accepted;

	private bool acceptButtonDown;

	[SerializeField]
	private float holdTime = 5f;

	[SerializeField]
	private LineRenderer progressBar;
}
