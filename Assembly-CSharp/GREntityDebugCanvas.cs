using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class GREntityDebugCanvas : MonoBehaviour
{
	private void Awake()
	{
		this.builder = new StringBuilder(50);
	}

	private void Start()
	{
		if (this.text == null && this.textPanelPrefab != null)
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.textPanelPrefab, base.transform.position + this.prefabAttachOffset, Quaternion.identity, base.transform);
			this.text = gameObject.GetComponent<TMP_Text>();
		}
		if (this.text != null)
		{
			this.text.gameObject.SetActive(false);
		}
	}

	private bool UpdateActive()
	{
		bool entityDebugEnabled = GhostReactorManager.entityDebugEnabled;
		if (this.text != null)
		{
			this.text.gameObject.SetActive(entityDebugEnabled);
		}
		return entityDebugEnabled;
	}

	private void Update()
	{
	}

	private void UpdateText()
	{
		if (this.text)
		{
			this.builder.Clear();
			List<IGameEntityDebugComponent> list = new List<IGameEntityDebugComponent>();
			base.GetComponents<IGameEntityDebugComponent>(list);
			foreach (IGameEntityDebugComponent gameEntityDebugComponent in list)
			{
				List<string> list2 = new List<string>();
				gameEntityDebugComponent.GetDebugTextLines(out list2);
				foreach (string text in list2)
				{
					this.builder.AppendLine(text);
				}
			}
			this.text.text = this.builder.ToString();
		}
	}

	[SerializeField]
	public TMP_Text text;

	public GameObject textPanelPrefab;

	public Vector3 prefabAttachOffset = new Vector3(0f, 0.5f, 0f);

	private StringBuilder builder;
}
