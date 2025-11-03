using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

internal abstract class WarningsServer : MonoBehaviour
{
	public abstract Task<PlayerAgeGateWarningStatus?> FetchPlayerData(CancellationToken token);

	public abstract Task<PlayerAgeGateWarningStatus?> GetOptInFollowUpMessage(CancellationToken token);

	public static volatile WarningsServer Instance;
}
