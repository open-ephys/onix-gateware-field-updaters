using oni;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSHubUpdater
{
    [HubID(4)]
    internal class HubRHS2116(string driver, int index, int portIndex) : HubConnection(driver, index, portIndex)
    {
        protected override async Task<double?> PowerHub()
        {
            const double MinVoltage = 3.3;
            const double MaxVoltage = 4.4;
            const double VoltageOffset = 2.0;
            const double VoltageIncrement = 0.2;

            double voltage = MinVoltage;
            for (; voltage <= MaxVoltage; voltage += VoltageIncrement)
            {
                await SetPortVoltage(voltage);
                if (CheckLinkState())
                {
                    voltage += VoltageOffset;
                    await SetPortVoltage(voltage);
                    return CheckLinkState() ? voltage : null;
                }
            }
            return null;
        }

        async Task SetPortVoltage(double voltage)
        {
            SetVoltage(0.0);
            await Task.Delay(500);
            SetVoltage(voltage);
            await Task.Delay(500);
        }
    }
}
