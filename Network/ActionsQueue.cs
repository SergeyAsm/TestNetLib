using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    public class ActionsQueue
    {
        private ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

        internal bool TryDequeue(out Action act)
        {
            return actions.TryDequeue(out act);
        }
        internal void Enqueue(Action act)
        {
            actions.Enqueue(act);
        }
    }
}