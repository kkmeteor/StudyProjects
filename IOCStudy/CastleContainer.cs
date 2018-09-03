using Castle.MicroKernel;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Castle.Windsor.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOCStudy
{
    public class CastleContainer
    {
        private static readonly object _locker = new object();
        private static CastleContainer _instance;
        private static IKernel _kernel;

        private CastleContainer()
        {
            Castle.Core.Resource.ConfigResource source = new Castle.Core.Resource.ConfigResource("castle");
            XmlInterpreter interpreter = new XmlInterpreter(source);
            IWindsorContainer windsor = new WindsorContainer(interpreter);
            _kernel = windsor.Kernel;
        }

        public static CastleContainer CreateInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    if (_instance == null)
                    {
                        _instance = new CastleContainer();
                    }
                }
            }
            return _instance;
        }

        public T Resolve<T>()
        {
            return _kernel.Resolve<T>();
        }


    }
}