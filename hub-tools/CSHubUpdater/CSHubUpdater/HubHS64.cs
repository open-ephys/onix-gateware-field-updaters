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
            const double ChargeVoltage = 10.0;
            const double MinVoltage = 3.3;
            const double MaxVoltage = 6.5;
            const double VoltageIncrement = 0.2;

            SetVoltage(0.0);
            Thread.Sleep(1000);

            SetVoltage(ChargeVoltage);
            Thread.Sleep(10);

            double voltage = MaxVoltage;
            for (; voltage >= MinVoltage; voltage -= VoltageIncrement)
            {
                SetVoltage(voltage);
                Thread.Sleep(200);
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
            Thread.Sleep(100);
            SetVoltage(0);
            await Task.Delay(1000);

            voltage += 0.4 * voltage + 2.0; // NB: Empirical from tethers of different lengths in order to get 5.0V to 5.3V at headstage
            SetVoltage(voltage);
            await Task.Delay(200);
            return CheckLinkState() ? voltage : null;
        }
    }
}
