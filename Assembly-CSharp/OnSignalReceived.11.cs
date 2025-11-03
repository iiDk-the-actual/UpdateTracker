using System;

public delegate void OnSignalReceived<in T1, in T2>(T1 arg1, T2 arg2, PhotonSignalInfo info);
