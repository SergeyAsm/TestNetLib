using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    public enum DisconnectReason
    {
        UNKNOWN_REASON,
        VERSIONS_MISMATCH,
        ONE_SIDE_CONNECTION
    }
}