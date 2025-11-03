using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace GorillaTag.GuidedRefs
{
	public static class GRef
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ShouldResolveNow(GRef.EResolveModes mode)
		{
			return Application.isPlaying && (mode & GRef.EResolveModes.Runtime) == GRef.EResolveModes.Runtime;
		}

		public static bool IsAnyResolveModeOn(GRef.EResolveModes mode)
		{
			return mode > GRef.EResolveModes.None;
		}

		[Flags]
		public enum EResolveModes
		{
			None = 0,
			Runtime = 1,
			SceneProcessing = 2
		}
	}
}
