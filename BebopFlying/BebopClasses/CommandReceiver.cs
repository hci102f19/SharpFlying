using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BebopFlying.BebopClasses
{
    public static class CommandReceiver
    {
        private static readonly Dictionary<Tuple<string, int>, bool> _commandReceived = new Dictionary<Tuple<string, int>, bool>();

        public static bool HasKey(string channel, int seqId)
        {
            var key = new Tuple<string, int>(channel, seqId);
            return _commandReceived.ContainsKey(key);
        }

        public static void SetCommandReceived(string channel, int seqId, bool val)
        {
            var key = new Tuple<string, int>(channel, seqId);
            _commandReceived[key] = val;
        }

        public static bool IsCommandReceived(string channel, int seqId)
        {
            try
            {
                var key = new Tuple<string, int>(channel, seqId);
                return _commandReceived[key];
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }
    }
}