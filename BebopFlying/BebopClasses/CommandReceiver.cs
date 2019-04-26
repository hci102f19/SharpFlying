using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BebopFlying.BebopClasses
{
    public static class CommandReceiver
    {
        private static readonly Dictionary<Tuple<string, int>, bool> CommandReceived = new Dictionary<Tuple<string, int>, bool>();

        public static bool HasKey(string channel, int seqId)
        {
            Tuple<string, int> key = new Tuple<string, int>(channel, seqId);
            return CommandReceived.ContainsKey(key);
        }

        public static void SetCommandReceived(string channel, int seqId, bool val)
        {
            Tuple<string, int> key = new Tuple<string, int>(channel, seqId);
            CommandReceived[key] = val;
        }

        public static bool IsCommandReceived(string channel, int seqId)
        {
            if (!HasKey(channel, seqId))
                return false;

            Tuple<string, int> key = new Tuple<string, int>(channel, seqId);
            return CommandReceived[key];

        }
    }
}