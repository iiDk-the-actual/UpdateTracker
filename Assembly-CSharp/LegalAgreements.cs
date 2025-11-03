using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GorillaNetworking;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.CloudScriptModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(1)]
public class LegalAgreements : MonoBehaviour
{
	public static LegalAgreements instance { get; private set; }

	protected virtual void Awake()
	{
		if (LegalAgreements.instance != null)
		{
			Debug.LogError("Trying to set [LegalAgreements] instance but it is not null", this);
			base.gameObject.SetActive(false);
			return;
		}
		LegalAgreements.instance = this;
		this.stickHeldDuration = 0f;
		this.scrollSpeed = this._minScrollSpeed;
		base.enabled = false;
	}

	private void Update()
	{
		if (!this.legalAgreementsStarted)
		{
			return;
		}
		float num = Time.deltaTime * this.scrollSpeed;
		if (ControllerBehaviour.Instance.IsUpStick || ControllerBehaviour.Instance.IsDownStick)
		{
			if (ControllerBehaviour.Instance.IsDownStick)
			{
				num *= -1f;
			}
			this.scrollBar.value = Mathf.Clamp(this.scrollBar.value + num, 0f, 1f);
			if (this.scrollBar.value > 0f && this.scrollBar.value < 1f)
			{
				HandRayController.Instance.PulseActiveHandray(this._stickVibrationStrength, this._stickVibrationDuration);
			}
			this.stickHeldDuration += Time.deltaTime;
			this.scrollTime = Mathf.Clamp01(this.stickHeldDuration / this._scrollInterpTime);
			this.scrollSpeed = Mathf.Lerp(this._minScrollSpeed, this._maxScrollSpeed, this._scrollInterpCurve.Evaluate(this.scrollTime));
			this.scrollSpeed *= Mathf.Abs(ControllerBehaviour.Instance.StickYValue);
		}
		else
		{
			this.stickHeldDuration = 0f;
			this.scrollSpeed = this._minScrollSpeed;
		}
		if (this._scrollToBottomText)
		{
			if ((double)this.scrollBar.value < 0.001)
			{
				this._scrollToBottomText.gameObject.SetActive(false);
				this._pressAndHoldToConfirmButton.gameObject.SetActive(true);
				return;
			}
			this._scrollToBottomText.text = LegalAgreements.SCROLL_TO_END_MESSAGE;
			this._scrollToBottomText.gameObject.SetActive(true);
			this._pressAndHoldToConfirmButton.gameObject.SetActive(false);
		}
	}

	public virtual async Task StartLegalAgreements()
	{
		if (!this.legalAgreementsStarted)
		{
			this.legalAgreementsStarted = true;
			while (!PlayFabClientAPI.IsClientLoggedIn())
			{
				if (PlayFabAuthenticator.instance && PlayFabAuthenticator.instance.loginFailed)
				{
					return;
				}
				await Task.Yield();
			}
			Dictionary<string, string> agreementResults = await this.GetAcceptedAgreements(this.legalAgreementScreens);
			foreach (LegalAgreementTextAsset screen in this.legalAgreementScreens)
			{
				string latestVersion = await this.GetTitleDataAsync(screen.latestVersionKey);
				if (!string.IsNullOrEmpty(latestVersion))
				{
					string empty = string.Empty;
					if (agreementResults == null || !agreementResults.TryGetValue(screen.playFabKey, out empty) || !(latestVersion == empty))
					{
						base.enabled = true;
						PrivateUIRoom.ForceStartOverlay();
						if (!screen.confirmString.IsNullOrEmpty())
						{
							this._pressAndHoldToConfirmButton.SetText(screen.confirmString);
						}
						PrivateUIRoom.AddUI(this.uiParent);
						HandRayController.Instance.EnableHandRays();
						TaskAwaiter<bool> taskAwaiter = this.UpdateText(screen, latestVersion).GetAwaiter();
						if (!taskAwaiter.IsCompleted)
						{
							await taskAwaiter;
							TaskAwaiter<bool> taskAwaiter2;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter<bool>);
						}
						if (!taskAwaiter.GetResult())
						{
							for (;;)
							{
								await Task.Yield();
							}
						}
						else
						{
							await this.WaitForAcknowledgement();
							this.scrollBar.value = 1f;
							PrivateUIRoom.RemoveUI(this.uiParent);
							if (agreementResults == null)
							{
								agreementResults = new Dictionary<string, string>();
							}
							agreementResults.AddOrUpdate(screen.playFabKey, latestVersion);
							if (this.optIn)
							{
								LegalAgreementTextAsset.PostAcceptAction optInAction = screen.optInAction;
							}
							latestVersion = null;
							screen = null;
						}
					}
				}
			}
			LegalAgreementTextAsset[] array = null;
			base.enabled = false;
			await this.SubmitAcceptedAgreements(agreementResults);
		}
	}

	public void OnAccepted(int currentAge)
	{
		this._accepted = true;
	}

	protected async Task WaitForAcknowledgement()
	{
		this._accepted = false;
		while (!this._accepted)
		{
			await Task.Yield();
		}
		this._accepted = false;
	}

	private async Task<bool> UpdateText(LegalAgreementTextAsset asset, string version)
	{
		this.optional = asset.optional;
		this.tmpTitle.text = asset.title;
		bool flag = await this.UpdateTextFromPlayFabTitleData(asset.playFabKey, version, this.tmpBody);
		if (!flag)
		{
			this.tmpBody.text = asset.errorMessage + "\n\nPlease restart the game and try again.";
			this.scrollBar.value = 0f;
			this.scrollBar.size = 1f;
		}
		return flag;
	}

	public async Task<bool> UpdateTextFromPlayFabTitleData(string key, string version, TMP_Text target)
	{
		string text = key + "_" + version;
		this.state = 0;
		PlayFabTitleDataCache.Instance.GetTitleData(text, new Action<string>(this.OnTitleDataReceived), new Action<PlayFabError>(this.OnPlayFabError), false);
		while (this.state == 0)
		{
			await Task.Yield();
		}
		bool flag;
		if (this.state == 1)
		{
			string text2 = Regex.Unescape(this.cachedText.Substring(1, this.cachedText.Length - 2));
			try
			{
				if (string.IsNullOrEmpty(text2))
				{
					Debug.LogError("[LOCALIZATION] TItle Data for Legal Agreements is NULL or Empty. Unable to deserialize or proceed.");
					return false;
				}
				text2 = JsonConvert.DeserializeObject<TitleDataLocalization>(text2).GetLocalizedText();
			}
			catch (Exception)
			{
				if (text2.StartsWith('{') && text2.EndsWith('}'))
				{
					Debug.LogError("[LOCALIZATION] TItle Data for Legal Agreements is likely in JSON format, but failed to deserialize into [TitleDataLocalization]");
					return false;
				}
			}
			target.text = text2;
			flag = true;
		}
		else
		{
			flag = false;
		}
		return flag;
	}

	private void OnPlayFabError(PlayFabError error)
	{
		this.state = -1;
	}

	private void OnTitleDataReceived(string obj)
	{
		this.cachedText = obj;
		this.state = 1;
	}

	private async Task<string> GetTitleDataAsync(string key)
	{
		int state = 0;
		string result = null;
		PlayFabTitleDataCache.Instance.GetTitleData(key, delegate(string res)
		{
			result = res;
			state = 1;
		}, delegate(PlayFabError err)
		{
			result = null;
			state = -1;
			Debug.LogError(err.ErrorMessage);
		}, false);
		while (state == 0)
		{
			await Task.Yield();
		}
		return (state == 1) ? result : null;
	}

	private async Task<Dictionary<string, string>> GetAcceptedAgreements(LegalAgreementTextAsset[] agreements)
	{
		int state = 0;
		Dictionary<string, string> returnValue = new Dictionary<string, string>();
		string[] array = agreements.Select((LegalAgreementTextAsset x) => x.playFabKey).ToArray<string>();
		GorillaServer.Instance.GetAcceptedAgreements(new GetAcceptedAgreementsRequest
		{
			AgreementKeys = array
		}, delegate(Dictionary<string, string> result)
		{
			state = 1;
			returnValue = result;
		}, delegate(PlayFabError error)
		{
			Debug.LogError(error.ErrorMessage);
			state = -1;
		});
		while (state == 0)
		{
			await Task.Yield();
		}
		return returnValue;
	}

	private async Task SubmitAcceptedAgreements(Dictionary<string, string> agreements)
	{
		int state = 0;
		GorillaServer.Instance.SubmitAcceptedAgreements(new SubmitAcceptedAgreementsRequest
		{
			Agreements = agreements
		}, delegate(ExecuteFunctionResult result)
		{
			state = 1;
		}, delegate(PlayFabError error)
		{
			state = -1;
		});
		while (state == 0)
		{
			await Task.Yield();
		}
	}

	public void OnDisable()
	{
		KIDAudioManager instance = KIDAudioManager.Instance;
		if (instance == null)
		{
			return;
		}
		instance.PlaySoundWithDelay(KIDAudioManager.KIDSoundType.PageTransition);
	}

	private static string SCROLL_TO_END_MESSAGE = "<b>Scroll to the bottom</b> to continue.";

	[Header("Scroll Behavior")]
	[SerializeField]
	protected float _minScrollSpeed = 0.02f;

	[SerializeField]
	private float _maxScrollSpeed = 3f;

	[SerializeField]
	private float _scrollInterpTime = 3f;

	[SerializeField]
	private AnimationCurve _scrollInterpCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	protected Transform uiParent;

	[SerializeField]
	protected TMP_Text tmpBody;

	[SerializeField]
	protected TMP_Text tmpTitle;

	[SerializeField]
	protected Scrollbar scrollBar;

	[SerializeField]
	private LegalAgreementTextAsset[] legalAgreementScreens;

	[SerializeField]
	protected KIDUIButton _pressAndHoldToConfirmButton;

	[SerializeField]
	private TMP_Text _scrollToBottomText;

	[SerializeField]
	private float _stickVibrationStrength = 0.1f;

	[SerializeField]
	private float _stickVibrationDuration = 0.05f;

	protected float stickHeldDuration;

	protected float scrollSpeed;

	private float scrollTime;

	protected bool legalAgreementsStarted;

	protected bool _accepted;

	private string cachedText;

	private int state;

	private bool optIn;

	private bool optional;
}
