namespace CSHubUpdater
{
    partial class UpdaterApp
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdaterApp));
            searchFileButton = new Button();
            label1 = new Label();
            fileNameTextBox = new TextBox();
            groupBox1 = new GroupBox();
            fwVerTextBox = new TextBox();
            hwRevTextBox = new TextBox();
            hubNameTextBox = new TextBox();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            portComboBox = new ComboBox();
            label5 = new Label();
            programButton = new Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // searchFileButton
            // 
            searchFileButton.Location = new Point(514, 48);
            searchFileButton.Margin = new Padding(3, 4, 3, 4);
            searchFileButton.Name = "searchFileButton";
            searchFileButton.Size = new Size(47, 45);
            searchFileButton.TabIndex = 1;
            searchFileButton.Text = "...";
            searchFileButton.UseVisualStyleBackColor = true;
            searchFileButton.Click += searchFileButton_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(13, 58);
            label1.Name = "label1";
            label1.Size = new Size(119, 25);
            label1.TabIndex = 1;
            label1.Text = "Firmware File:";
            // 
            // fileNameTextBox
            // 
            fileNameTextBox.Location = new Point(139, 54);
            fileNameTextBox.Margin = new Padding(3, 4, 3, 4);
            fileNameTextBox.Name = "fileNameTextBox";
            fileNameTextBox.Size = new Size(368, 31);
            fileNameTextBox.TabIndex = 0;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(fwVerTextBox);
            groupBox1.Controls.Add(hwRevTextBox);
            groupBox1.Controls.Add(hubNameTextBox);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label2);
            groupBox1.Location = new Point(18, 120);
            groupBox1.Margin = new Padding(3, 4, 3, 4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(3, 4, 3, 4);
            groupBox1.Size = new Size(339, 205);
            groupBox1.TabIndex = 3;
            groupBox1.TabStop = false;
            groupBox1.Text = "Target:";
            // 
            // fwVerTextBox
            // 
            fwVerTextBox.Location = new Point(121, 136);
            fwVerTextBox.Margin = new Padding(3, 4, 3, 4);
            fwVerTextBox.Name = "fwVerTextBox";
            fwVerTextBox.ReadOnly = true;
            fwVerTextBox.Size = new Size(180, 31);
            fwVerTextBox.TabIndex = 6;
            fwVerTextBox.TabStop = false;
            // 
            // hwRevTextBox
            // 
            hwRevTextBox.Location = new Point(121, 85);
            hwRevTextBox.Margin = new Padding(3, 4, 3, 4);
            hwRevTextBox.Name = "hwRevTextBox";
            hwRevTextBox.ReadOnly = true;
            hwRevTextBox.Size = new Size(180, 31);
            hwRevTextBox.TabIndex = 5;
            hwRevTextBox.TabStop = false;
            // 
            // hubNameTextBox
            // 
            hubNameTextBox.Location = new Point(121, 39);
            hubNameTextBox.Margin = new Padding(3, 4, 3, 4);
            hubNameTextBox.Name = "hubNameTextBox";
            hubNameTextBox.ReadOnly = true;
            hubNameTextBox.Size = new Size(180, 31);
            hubNameTextBox.TabIndex = 4;
            hubNameTextBox.TabStop = false;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(8, 140);
            label4.Name = "label4";
            label4.Size = new Size(105, 25);
            label4.TabIndex = 2;
            label4.Text = "FW Version:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(7, 89);
            label3.Name = "label3";
            label3.Size = new Size(112, 25);
            label3.TabIndex = 1;
            label3.Text = "HW revision:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(16, 42);
            label2.Name = "label2";
            label2.Size = new Size(102, 25);
            label2.TabIndex = 0;
            label2.Text = "Hub Name:";
            // 
            // portComboBox
            // 
            portComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            portComboBox.FormattingEnabled = true;
            portComboBox.Items.AddRange(new object[] { "A", "B" });
            portComboBox.Location = new Point(428, 149);
            portComboBox.Margin = new Padding(3, 4, 3, 4);
            portComboBox.Name = "portComboBox";
            portComboBox.Size = new Size(134, 33);
            portComboBox.TabIndex = 2;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(374, 152);
            label5.Name = "label5";
            label5.Size = new Size(48, 25);
            label5.TabIndex = 5;
            label5.Text = "Port:";
            // 
            // programButton
            // 
            programButton.Enabled = false;
            programButton.Location = new Point(379, 209);
            programButton.Margin = new Padding(3, 4, 3, 4);
            programButton.Name = "programButton";
            programButton.Size = new Size(183, 58);
            programButton.TabIndex = 3;
            programButton.Text = "program";
            programButton.UseVisualStyleBackColor = true;
            programButton.Click += programButton_Click;
            // 
            // UpdaterApp
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(626, 340);
            Controls.Add(programButton);
            Controls.Add(label5);
            Controls.Add(portComboBox);
            Controls.Add(groupBox1);
            Controls.Add(fileNameTextBox);
            Controls.Add(label1);
            Controls.Add(searchFileButton);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            Name = "UpdaterApp";
            Text = "ONIX Hub Updater";
            Load += UpdaterApp_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button searchFileButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox fileNameTextBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox fwVerTextBox;
        private System.Windows.Forms.TextBox hwRevTextBox;
        private System.Windows.Forms.TextBox hubNameTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox portComboBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button programButton;
    }
}