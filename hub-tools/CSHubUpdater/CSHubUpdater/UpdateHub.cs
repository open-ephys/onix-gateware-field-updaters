using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSHubUpdater
{
    public partial class UpdateHub : Form
    {
         IHubConnection hub;
        readonly IHubBitFile bitfile;
        bool isUpdating = false;
        int portIndex;

        public UpdateHub(IHubBitFile file, int portIndex)
        {
            InitializeComponent();
            bitfile = file;

            fHubNameTextBox.Text = file.HubId.ToString();
            fFwVerTextBox.Text = file.FwVer.ToString();
            fHwRevTextBox.Text = file.HwRevision.ToString();
            this.portIndex = portIndex;
        }

        private async void programButton_Click(object sender, EventArgs e)
        {
            isUpdating = true;
            cancelButton.Enabled = false;
            programButton.Enabled = false;
            progressUpdate.Value = 0;
            // NB: Last 10% will be resetting the headstage
            int maxValue = (int)((bitfile.Data.Length / 4) / 0.9);
            progressUpdate.Maximum = maxValue;
            Progress<int> progress = new(value =>
            {
                progressUpdate.Value = value;
            });
            UseWaitCursor = true;
            try
            {
                await hub.UpdateFirmware(bitfile, progress);
                await hub.RestartHeadstage();
                progressUpdate.Value = maxValue;
                if (hub.HubId != bitfile.HubId || hub.HwRevision != bitfile.HwRevision)
                {
                    // NB: This should never happen, it it happens, something VERY wrong has happened
                    throw new IOException($"Programmed IDs do not match. Reported hub id: {hub.HubId} hw rev: {hub.HwRevision}. " +
                        $"Expected hub id {bitfile.HubId} hw rev: {bitfile.HwRevision}");
                }
            }
            catch (Exception ex)
            {
                isUpdating = false;
                MessageBox.Show(ex.Message, "Failure while updating", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            cancelButton.Enabled = true;
            isUpdating = false;
            UseWaitCursor = false;
            programButton.Enabled = true;
            if (hub.SafeFirmware)
            {
                MessageBox.Show("Hub booted into backup firmware. This implies a failed update. " +
                    "Please try again or contact support", "Update failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (hub.FwVersion != bitfile.FwVer)
            {
                MessageBox.Show("Incorrect firmware version detected in hub. This implies a failed update. " +
                    "Please try again or contact support", "Update failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("Hub update successful", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void UpdateHub_Load(object sender, EventArgs e)
        {
            UseWaitCursor = true;
            try
            {
                hub = await HubConnection.CreateFromHubInfoAsync("riffa", 0, portIndex, bitfile.HubId);
                //hub = await VirtualHubTest.CreateFromHubInfoAsync("riffa", 0, portIndex, bitfile.HubId);
                if (hub.HwRevision != bitfile.HwRevision)
                {
                    throw new ArgumentException($"Hardware recision mismatch. File expected {bitfile.HwRevision} Hardware reported {hub.HwRevision}");
                }

                hHubNameTextBox.DataBindings.Add("Text", hub, nameof(IHubConnection.HubId));
                hHwRevTextBox.DataBindings.Add("Text", hub, nameof(IHubConnection.HwRevision));
                hFwVerTextBox.DataBindings.Add("Text", hub, nameof(IHubConnection.FwVersion));
                hSafeFwVer.DataBindings.Add("Text", hub, nameof(IHubConnection.SafeFwVersion));
                Binding modeBinding = new("Text", hub, nameof(IHubConnection.SafeFirmware));
                modeBinding.Format += (sender, e) =>
                {
                    if (e.DesiredType != typeof(string)) return;
                    e.Value = ((bool)e.Value) ? "Backup" : "Normal";
                };
                hMode.DataBindings.Add(modeBinding);
            }
            catch (Exception ex)
            {
                // NB: Close connection quickly here to avoid potential hs damage
                hub?.Dispose();
                hub = null;
                MessageBox.Show(ex.Message, "Failure to open hardware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
            programButton.Enabled = true;
            cancelButton.Enabled = true;
            UseWaitCursor = false;
        }
    }
}
