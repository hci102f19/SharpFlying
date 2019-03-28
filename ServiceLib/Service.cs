using System;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace ServiceLib
{
    public abstract class Service
    {
        public void Start()
        {
            Task.Factory.StartNew(() => { Run(); });
        }

        protected virtual void Run()
        {
            throw new NotImplementedException();
        }

        public virtual void Input(Image<Bgr, byte> frame)
        {
            throw new NotImplementedException();
        }

        public virtual Response GetLatestResult()
        {
            throw new NotImplementedException();
        }

        public virtual string GetLatestResultKage()
        {
            throw new NotImplementedException();
        }

    }
}