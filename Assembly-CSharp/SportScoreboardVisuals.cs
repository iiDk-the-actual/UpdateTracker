using System;
using GorillaTag.Sports;
using UnityEngine;

public class SportScoreboardVisuals : MonoBehaviour
{
	private void Awake()
	{
		SportScoreboard.Instance.RegisterTeamVisual(this.TeamIndex, this);
	}

	[SerializeField]
	public MaterialUVOffsetListSetter score1s;

	[SerializeField]
	public MaterialUVOffsetListSetter score10s;

	[SerializeField]
	private int TeamIndex;
}
