using System;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DevWatchSelectableItem : MonoBehaviour
{
	public void Init(NetworkObject obj)
	{
		this.SelectedObject = obj;
		this.ItemName.text = obj.name;
		this.Button.onClick.AddListener(delegate
		{
			this.OnSelected(this.ItemName.text, this.SelectedObject);
		});
	}

	public Button Button;

	public TextMeshProUGUI ItemName;

	public NetworkObject SelectedObject;

	public Action<string, NetworkObject> OnSelected;
}
