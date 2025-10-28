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
        readonly IHubConnection hub;
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
            programButton.Enabled = true;
            progressUpdate.Value = 0;
            // NB: Last 10% will be resetting the headstage
            int maxValue = (int)((bitfile.Data.Length / 4) / 0.9);
            Progress<int> progress = new(value =>
            {
                progressUpdate.Value = value;
            });
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
                Close();
                MessageBox.Show(ex.Message, "Failure while updating", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            cancelButton.Enabled = true;
            isUpdating = false;
            if (hub.SafeFirmware)
            {
                MessageBox.Show("Hub booted into backup firmware. This implies a failed update. " +
                    "Please try again or contact support", "Update failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                programButton.Enabled = true;
            }
            else
            {
                MessageBox.Show("Hub update successful", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cancelButton.Text = "Close";
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void UpdateHub_Load(object sender, EventArgs e)
        {
            try
            {
                var hub = await HubConnection.CreateFromHubInfoAsync("riffa", 0, portIndex, bitfile.HubId);
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
                MessageBox.Show(ex.Message, "Failure to open hardware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }
    }
}
