using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSHubUpdater
{
    public partial class UpdaterApp : Form
    {

        IHubBitFile bitFile;
        CancellationTokenSource loadCancellationToken;
        readonly SemaphoreSlim loadSemaphore = new(1, 1);
        bool enterAction = false;
        string lastPath = string.Empty;

        public UpdaterApp()
        {
            InitializeComponent();
        }

        void UpdaterApp_Load(object sender, EventArgs e)
        {
            portComboBox.SelectedIndex = 0;
        }

        void searchFileButton_Click(object sender, EventArgs e)
        {
            string initialPath = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(fileNameTextBox.Text))
                {
                    if (Directory.Exists(fileNameTextBox.Text))
                    {
                        initialPath = fileNameTextBox.Text;
                    }
                    else
                    {
                        initialPath = Path.GetDirectoryName(fileNameTextBox.Text);
                    }
                }
            }

            catch (ArgumentException)
            {
     
            }

            using OpenFileDialog dialog = new();


            dialog.InitialDirectory = initialPath;
            dialog.Filter = "All Files (*.*)|*.*|ONIX hub firmware files (*.onix)|*.onix";
            dialog.FilterIndex = 2;
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                fileNameTextBox.Text = dialog.FileName;
                _ = LoadBitFile();
            }

        }

        async Task LoadBitFile()
        {
            if (lastPath == fileNameTextBox.Text) return;
            lastPath = fileNameTextBox.Text;
            bitFile = null;
            programButton.Enabled = false;
            try
            {
                var file = new FileInfo(fileNameTextBox.Text);

                Task<IHubBitFile> localTask;
                //Protected zone
                await loadSemaphore.WaitAsync();
                try
                {
                    loadCancellationToken?.Cancel();
                    loadCancellationToken?.Dispose();
                    loadCancellationToken = new CancellationTokenSource();

                    localTask = HubBitFile.CreateAsync(file, loadCancellationToken.Token);
                }
                finally
                {
                    loadSemaphore.Release();
                }
                bitFile = await localTask;
                hubNameTextBox.Text = bitFile.HubId.ToString();
                hwRevTextBox.Text = bitFile.HwRevision.ToString();
                fwVerTextBox.Text = bitFile.FwVer.ToString();
                programButton.Enabled = true;

            }
            catch (TaskCanceledException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failure to open firmware file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                hubNameTextBox.Text = string.Empty;
                hwRevTextBox.Text = string.Empty;
                fwVerTextBox.Text = string.Empty;
            }


        }

        private void programButton_Click(object sender, EventArgs e)
        {
            Enabled = false;

            using (UpdateHub updater = new(bitFile, portComboBox.SelectedIndex))
            {
                updater.ShowDialog();
            }


            Enabled = true;

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                loadCancellationToken?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void fileNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                enterAction = true;
                e.SuppressKeyPress = true;
                SelectNextControl((Control)sender, true, true, true, true);
                _ = LoadBitFile();
                enterAction = false;
            }
        }

        private void fileNameTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (!enterAction)
            {
                _ = LoadBitFile();
            }
        }
    }
}
