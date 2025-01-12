using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace Rim3D
{
    public static class DebugLog
    {
        private static Dictionary<string, float> lastLogTimes = new Dictionary<string, float>();
        private const float LOG_INTERVAL = 2f;

        public static void Log(string message)
        {
            string messageHash = message.GetHashCode().ToString();
            float currentTime = Time.realtimeSinceStartup;
            if (!lastLogTimes.ContainsKey(messageHash))
            {
                lastLogTimes[messageHash] = 0f;
            }
            if (currentTime - lastLogTimes[messageHash] >= LOG_INTERVAL)
            {
                lastLogTimes[messageHash] = currentTime;
                Verse.Log.Message(message);
            }
        }

        public static void Clear()
        {
            lastLogTimes.Clear();
        }
    }
}
