using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GRBay : MonoBehaviour
{
	private void Awake()
	{
		if (this.playerName != null)
		{
			this.playerName.text = null;
		}
		if (this.maxDropText != null)
		{
			this.maxDropText.text = null;
		}
	}

	public void Setup(GhostReactor reactor)
	{
		this.reactor = reactor;
		if (this.shuttleLoc != GRShuttleGroupLoc.Invalid && this.shuttleIndex >= 0 && this.shuttleIndex < 10)
		{
			this.unlockShuttle = GRElevatorManager._instance.GetPlayerShuttle(this.shuttleLoc, this.shuttleIndex);
			if (this.unlockShuttle != null)
			{
				this.unlockShuttle.SetBay(this);
			}
		}
		this.Refresh();
	}

	public void SetOpen(bool open)
	{
		if (this.hideWhenOpen != null)
		{
			for (int i = 0; i < this.hideWhenOpen.Count; i++)
			{
				if (this.hideWhenOpen[i] != null)
				{
					this.hideWhenOpen[i].SetActive(!open);
				}
				else
				{
					Debug.LogErrorFormat("Why is hideWhenOpen null {0} at {1}", new object[]
					{
						base.gameObject.name,
						i
					});
				}
			}
		}
		else
		{
			Debug.LogErrorFormat("Why is hideWhenOpen null {0}", new object[] { base.gameObject.name });
		}
		if (this.hideWhenClosed != null)
		{
			for (int j = 0; j < this.hideWhenClosed.Count; j++)
			{
				if (this.hideWhenClosed[j] != null)
				{
					this.hideWhenClosed[j].SetActive(open);
				}
				else
				{
					Debug.LogErrorFormat("Why is hideWhenClosed null {0} at {1} ", new object[]
					{
						base.gameObject.name,
						j
					});
				}
			}
		}
		else
		{
			Debug.LogErrorFormat("Why is hideWhenClosed null {0}", new object[] { base.gameObject.name });
		}
		if (this.bayDoorAnimation != null && this.isOpen != open)
		{
			if (open)
			{
				this.bayDoorAnimation.Play("BayDoor_Open");
				this.bayDoorAnimation.PlayQueued("BayDoor_Open_Idle");
			}
			else
			{
				this.bayDoorAnimation.Play("BayDoor_Close");
				this.bayDoorAnimation.PlayQueued("BayDoor_Close_Idle");
			}
		}
		this.isOpen = open;
	}

	public void Refresh()
	{
		bool flag = true;
		if (this.unlockShuttle != null)
		{
			NetPlayer owner = this.unlockShuttle.GetOwner();
			bool flag2 = owner != null && this.unlockShuttle.IsPodUnlocked();
			flag = this.unlockShuttle.GetState() == GRShuttleState.Docked && flag2;
			if (this.playerName != null)
			{
				this.playerName.text = ((!flag2) ? null : owner.SanitizedNickName);
			}
			if (this.maxDropText != null)
			{
				int num = this.unlockShuttle.GetMaxDropFloor() + 1;
				this.maxDropText.text = ((!flag2) ? null : num.ToString());
			}
			for (int i = 0; i < this.showWhenOwned.Count; i++)
			{
				this.showWhenOwned[i].SetActive(flag2);
			}
			for (int j = 0; j < this.showWhenNotOwned.Count; j++)
			{
				this.showWhenNotOwned[j].SetActive(!flag2);
			}
		}
		else if (this.unlockByDrillLevel > 0)
		{
			flag = (this.reactor != null && this.reactor.GetDepthLevel() >= this.unlockByDrillLevel) || GhostReactorManager.bayUnlockEnabled;
		}
		this.SetOpen(flag);
	}

	public List<GameObject> hideWhenOpen;

	public List<GameObject> hideWhenClosed;

	public Animation bayDoorAnimation;

	private bool isOpen;

	public TMP_Text playerName;

	public TMP_Text maxDropText;

	public List<GameObject> showWhenOwned;

	public List<GameObject> showWhenNotOwned;

	public int unlockByDrillLevel = -1;

	public GRShuttleGroupLoc shuttleLoc = GRShuttleGroupLoc.Invalid;

	public int shuttleIndex = -1;

	[NonSerialized]
	public bool debugForceUnlockedByLevel;

	private GRShuttle unlockShuttle;

	private GhostReactor reactor;
}
