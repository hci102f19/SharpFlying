using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLib
{
    public abstract class Service
    {
        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                Run();
            });
        }

        protected virtual void Run()
        {
            throw new NotImplementedException();
        }
    }
}
