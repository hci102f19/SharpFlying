using System.Collections.Generic;

namespace BebopFlying.Model
{
    public class CommandParam
    {
        public List<object> Parameters { get; protected set; } = new List<object>();

        public void AddData<T>(T data)
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
    }
}