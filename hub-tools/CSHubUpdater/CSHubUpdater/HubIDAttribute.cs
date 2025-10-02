using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSHubUpdater
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class HubIDAttribute(int DevID) : Attribute
    {
        public int ID { get; private set; } = DevID;
    }
}
