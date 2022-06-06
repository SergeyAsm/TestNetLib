using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    public class PeerMessagesFilter
    {
        private ConcurrentDictionary<int, long> senders = new ConcurrentDictionary<int, long>();
        public bool IsPassed(IPeerMessage mess)
        {
            var mkey = mess.GetOriginalSenderId();
            var mvalue = mess.GetMessageId();
            bool result = true;
            senders.AddOrUpdate(mkey, mvalue, (int k, long v) => {
                if (v >= mvalue)
                {
                    result=false;
                    return v;
                }
                return mvalue; });
            return result;
        }
    }
}