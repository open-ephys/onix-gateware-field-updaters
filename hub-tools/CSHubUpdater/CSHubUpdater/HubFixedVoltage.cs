using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSHubUpdater
{
    internal class HubFixedVoltage(string driver, int index, int portIndex, double voltage) : HubConnection(driver, index, portIndex)
    {
        protected override async Task<double?> PowerHub()
        {
            SetVoltage(voltage); 
            await Task.Delay(500);
            return CheckLinkState() ? voltage : null;
        }
    }
}
