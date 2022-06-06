using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Statistics;

namespace Network
{
    public class NetworkStatistics
    {
        private MaxValue maxMessageSize;
        private MeanValue meanMessageSize;
        private Dispersion dispersionMessageSize = new Dispersion();

        public void AddOutcoming(long value,string description = null)
        {
            maxMessageSize.Add(value);
            meanMessageSize.Add(value);
            dispersionMessageSize.Add(value,description);
        }
        public override string ToString()
        {
            return $"maxMessageSize={maxMessageSize.Max()} meanMessageSize={meanMessageSize.Mean()} dispersionMessageSize=(\n{dispersionMessageSize.DispersionString()})";
        }
    }
}
