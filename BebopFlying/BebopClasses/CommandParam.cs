using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BebopFlying.BebopClasses
{
    public class CommandParam
    {
        public List<object> Parameters { get; protected set; } = new List<object>();

        public void AddData(byte data)
        {
            Parameters.Add(data);
        }

        public string Format()
        {
            string fmt = "";
            foreach (object obj in Parameters)
            {
                fmt += StructConverter.GetFormatSpecifierFor(obj);
            }

            return fmt;
        }

        public int PacketSize()
        {
            return StructConverter.PacketSize(Format());
        }

    }
}