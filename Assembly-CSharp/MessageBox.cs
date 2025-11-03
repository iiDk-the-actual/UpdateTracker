using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MessageBox : MonoBehaviour
{
	public MessageBoxResult Result { get; private set; }

	public string Header
	{
		get
		{
			return this._headerText.text;
		}
		set
		{
			this._headerText.text = value;
			this._headerText.gameObject.SetActive(!string.IsNullOrEmpty(value));
		}
	}

	public string Body
	{
		get
		{
			return this._bodyText.text;
		}
		set
		{
			this._bodyText.text = value;
		}
	}

	public string LeftButton
	{
		get
		{
			return this._leftButtonText.text;
		}
		set
		{
			this._leftButtonText.text = value;
			this._leftButton.SetActive(!string.IsNullOrEmpty(value));
			if (string.IsNullOrEmpty(value))
			{
				RectTransform component = this._rightButton.GetComponent<RectTransform>();
				component.anchorMin = new Vector2(0.5f, 0.5f);
				component.anchorMax = new Vector2(0.5f, 0.5f);
				component.pivot = new Vector2(0.5f, 0.5f);
				component.anchoredPosition = Vector3.zero;
				return;
			}
			RectTransform component2 = this._rightButton.GetComponent<RectTransform>();
			component2.anchorMin = new Vector2(1f, 0.5f);
			component2.anchorMax = new Vector2(1f, 0.5f);
			component2.pivot = new Vector2(1f, 0.5f);
			component2.anchoredPosition = Vector3.zero;
		}
	}

	public string RightButton
	{
		get
		{
			return this._rightButtonText.text;
		}
		set
		{
			this._rightButtonText.text = value;
			this._rightButton.SetActive(!string.IsNullOrEmpty(value));
			if (string.IsNullOrEmpty(value))
			{
				RectTransform component = this._leftButton.GetComponent<RectTransform>();
				component.anchorMin = new Vector2(0.5f, 0.5f);
				component.anchorMax = new Vector2(0.5f, 0.5f);
				component.pivot = new Vector2(0.5f, 0.5f);
				component.anchoredPosition3D = Vector3.zero;
				return;
			}
			RectTransform component2 = this._leftButton.GetComponent<RectTransform>();
			component2.anchorMin = new Vector2(0f, 0.5f);
			component2.anchorMax = new Vector2(0f, 0.5f);
			component2.pivot = new Vector2(0f, 0.5f);
			component2.anchoredPosition3D = Vector3.zero;
		}
	}

	public UnityEvent LeftButtonCallback
	{
		get
		{
			return this._leftButtonCallback;
		}
	}

	public UnityEvent RightButtonCallback
	{
		get
		{
			return this._rightButtonCallback;
		}
	}

	private void Start()
	{
		this.Result = MessageBoxResult.None;
	}

	private void Update()
	{
	}

	public void ShowQuitButtonAsPrimary()
	{
		this._leftButton.SetActive(false);
		RectTransform component = this._rightButton.GetComponent<RectTransform>();
		component.anchorMin = new Vector2(0.5f, 0.5f);
		component.anchorMax = new Vector2(0.5f, 0.5f);
		component.pivot = new Vector2(0.5f, 0.5f);
		component.anchoredPosition = Vector3.zero;
	}

	public void OnClickLeftButton()
	{
		this.Result = MessageBoxResult.Left;
		this._leftButtonCallback.Invoke();
	}

	public void OnClickRightButton()
	{
		this.Result = MessageBoxResult.Right;
		this._rightButtonCallback.Invoke();
	}

	public GameObject GetCanvas()
	{
		return base.GetComponentInChildren<Canvas>(true).gameObject;
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

	[SerializeField]
	private TMP_Text _headerText;

	[SerializeField]
	private TMP_Text _bodyText;

	[SerializeField]
	private TMP_Text _leftButtonText;

	[SerializeField]
	private TMP_Text _rightButtonText;

	[SerializeField]
	private GameObject _leftButton;

	[SerializeField]
	private GameObject _rightButton;

	[SerializeField]
	private UnityEvent _leftButtonCallback = new UnityEvent();

	[SerializeField]
	private UnityEvent _rightButtonCallback = new UnityEvent();
}
