using oni;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSHubUpdater
{
    [HubID(2)]
    internal class HubHS64(string driver, int index, int portIndex) : HubConnection(driver, index, portIndex)
    {
        protected override async Task<double?> PowerHub()
        {
            
            const double MinVoltage = 3.3;
            const double MaxVoltage = 6.0;
            const double VoltageOffset = 3.4;
            const double VoltageIncrement = 0.2;

            await Task.Delay(1000);
            double voltage = MaxVoltage;
            for (; voltage >= MinVoltage; voltage -= VoltageIncrement)
            {
                SetVoltage(voltage);
                await Task.Delay(200);
                if (!CheckLinkState())
                {
                    if (voltage == MaxVoltage)
                    {
                        return null;
                    }
                    else break;
                }

            }
            SetVoltage(MinVoltage);
            SetVoltage(0);
            await Task.Delay(1000);
            voltage += VoltageOffset;
            SetVoltage(voltage);
            await Task.Delay(200);
            return CheckLinkState() ? voltage : null;
        }
    }
}
