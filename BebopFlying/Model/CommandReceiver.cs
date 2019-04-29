using System;
using System.Collections.Generic;

namespace BebopFlying.Model
{
    public static class CommandReceiver
    {
        private static readonly Dictionary<Tuple<string, int>, bool> CommandReceived = new Dictionary<Tuple<string, int>, bool>();

        public static bool HasKey(string channel, int seqId)
        {
            return CommandReceived.ContainsKey(Key(channel, seqId));
        }

        public static void SetCommandReceived(string channel, int seqId, bool val)
        {
            CommandReceived[Key(channel, seqId)] = val;
        }

        public static bool IsCommandReceived(string channel, int seqId)
        {
            return HasKey(channel, seqId) && CommandReceived[Key(channel, seqId)];
        }

        private static Tuple<string, int> Key(string channel, int seqId)
        {
            return new Tuple<string, int>(channel, seqId);
        }
    }
}