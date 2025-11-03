using System;

public delegate void OnSignalReceived<in T1>(T1 arg1, PhotonSignalInfo info);
