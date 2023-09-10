using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkPlayer : NetworkBehaviour
{
    public static NetworkPlayer Local { get; private set; }

    //Es como el Awake para cuando este objeto sea instanciado en Red
    public override void Spawned()
    {
        if (Object.HasInputAuthority)
            Local = this;
    }
}
