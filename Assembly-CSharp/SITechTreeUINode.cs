using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SITechTreeUINode : MonoBehaviour
{
	public List<Image> UpgradeLines { get; } = new List<Image>();

	public List<SITechTreeUINode> Parents { get; } = new List<SITechTreeUINode>();

	public bool IsConfigured
	{
		get
		{
			return this._node != null;
		}
	}

	public void SetTechTreeNode(SITechTreeStation techTreeStation, SIUpgradeType nodeUpgradeType)
	{
		if (!techTreeStation.techTreeSO.TryGetNode(nodeUpgradeType, out this._node))
		{
			Debug.LogError(string.Format("Node {0} doesn't exist in tree.  Disabling.", nodeUpgradeType));
			base.gameObject.SetActive(false);
			return;
		}
		this.upgradeType = nodeUpgradeType;
		float num = (float)(Mathf.Min(this.GetMaxWordLength(this._node.Value.nickName), 14) * 4);
		Vector2 sizeDelta = this.nodeNickName.rectTransform.sizeDelta;
		if (sizeDelta.x < num)
		{
			sizeDelta.x = num;
			this.nodeNickName.rectTransform.sizeDelta = sizeDelta;
		}
		base.name = (this.nodeNickName.text = this._node.Value.nickName);
		this.button.data = this._node.Value.upgradeType.GetNodeId();
		this.button.buttonPressed.RemoveAllListeners();
		this.button.buttonPressed.AddListener(new UnityAction<SITouchscreenButton.SITouchscreenButtonType, int, int>(techTreeStation.TouchscreenButtonPressed));
		this.SetGadgetUnlockNode(this._node.Value.unlockedGadgetPrefab);
	}

	public void SetNodeLockStateColor(Color color)
	{
		if (color == Color.red)
		{
			this.circle.sharedMaterial = this.redMat;
		}
		else if (color == Color.black)
		{
			this.circle.sharedMaterial = this.blackMat;
		}
		else if (color == Color.green)
		{
			this.circle.sharedMaterial = this.greenMat;
		}
		foreach (Image image in this.UpgradeLines)
		{
			image.color = color;
		}
	}

	private void SetGadgetUnlockNode(bool isUnlockNode)
	{
		this.triangle.gameObject.SetActive(isUnlockNode);
	}

	private int GetMaxWordLength(string text)
	{
		string[] array = text.Split(' ', StringSplitOptions.None);
		int num = 0;
		foreach (string text2 in array)
		{
			if (text2.Length > num)
			{
				num = text2.Length;
			}
		}
		return num;
	}

	public SIUpgradeType upgradeType;

	public TextMeshProUGUI nodeNickName;

	public MeshRenderer circle;

	public MeshRenderer triangle;

	public SITouchscreenButton button;

	public Material greenMat;

	public Material redMat;

	public Material blackMat;

	public ObjectHierarchyFlattener imageFlattener;

	public ObjectHierarchyFlattener textFlattener;

	private GraphNode<SITechTreeNode> _node;
}
