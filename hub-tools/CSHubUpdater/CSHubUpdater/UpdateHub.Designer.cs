namespace CSHubUpdater
{
    partial class UpdateHub
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBox1 = new GroupBox();
            fFwVerTextBox = new TextBox();
            fHwRevTextBox = new TextBox();
            fHubNameTextBox = new TextBox();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            groupBox2 = new GroupBox();
            hSafeFwVer = new TextBox();
            label8 = new Label();
            hMode = new TextBox();
            label7 = new Label();
            hFwVerTextBox = new TextBox();
            hHwRevTextBox = new TextBox();
            hHubNameTextBox = new TextBox();
            label1 = new Label();
            label5 = new Label();
            label6 = new Label();
            progressUpdate = new ProgressBar();
            programButton = new Button();
            cancelButton = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(fFwVerTextBox);
            groupBox1.Controls.Add(fHwRevTextBox);
            groupBox1.Controls.Add(fHubNameTextBox);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label2);
            groupBox1.Location = new Point(12, 13);
            groupBox1.Margin = new Padding(3, 4, 3, 4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(3, 4, 3, 4);
            groupBox1.Size = new Size(339, 206);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "File:";
            // 
            // fFwVerTextBox
            // 
            fFwVerTextBox.Location = new Point(121, 136);
            fFwVerTextBox.Margin = new Padding(3, 4, 3, 4);
            fFwVerTextBox.Name = "fFwVerTextBox";
            fFwVerTextBox.ReadOnly = true;
            fFwVerTextBox.Size = new Size(180, 31);
            fFwVerTextBox.TabIndex = 6;
            fFwVerTextBox.TabStop = false;
            // 
            // fHwRevTextBox
            // 
            fHwRevTextBox.Location = new Point(121, 85);
            fHwRevTextBox.Margin = new Padding(3, 4, 3, 4);
            fHwRevTextBox.Name = "fHwRevTextBox";
            fHwRevTextBox.ReadOnly = true;
            fHwRevTextBox.Size = new Size(180, 31);
            fHwRevTextBox.TabIndex = 5;
            fHwRevTextBox.TabStop = false;
            // 
            // fHubNameTextBox
            // 
            fHubNameTextBox.Location = new Point(121, 39);
            fHubNameTextBox.Margin = new Padding(3, 4, 3, 4);
            fHubNameTextBox.Name = "fHubNameTextBox";
            fHubNameTextBox.ReadOnly = true;
            fHubNameTextBox.Size = new Size(180, 31);
            fHubNameTextBox.TabIndex = 4;
            fHubNameTextBox.TabStop = false;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(8, 141);
            label4.Name = "label4";
            label4.Size = new Size(105, 25);
            label4.TabIndex = 2;
            label4.Text = "FW Version:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(7, 90);
            label3.Name = "label3";
            label3.Size = new Size(112, 25);
            label3.TabIndex = 1;
            label3.Text = "HW revision:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(16, 43);
            label2.Name = "label2";
            label2.Size = new Size(102, 25);
            label2.TabIndex = 0;
            label2.Text = "Hub Name:";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(hSafeFwVer);
            groupBox2.Controls.Add(label8);
            groupBox2.Controls.Add(hMode);
            groupBox2.Controls.Add(label7);
            groupBox2.Controls.Add(hFwVerTextBox);
            groupBox2.Controls.Add(hHwRevTextBox);
            groupBox2.Controls.Add(hHubNameTextBox);
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(label6);
            groupBox2.Location = new Point(371, 13);
            groupBox2.Margin = new Padding(3, 4, 3, 4);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(3, 4, 3, 4);
            groupBox2.Size = new Size(346, 287);
            groupBox2.TabIndex = 7;
            groupBox2.TabStop = false;
            groupBox2.Text = "Device:";
            // 
            // hSafeFwVer
            // 
            hSafeFwVer.Location = new Point(137, 240);
            hSafeFwVer.Margin = new Padding(3, 4, 3, 4);
            hSafeFwVer.Name = "hSafeFwVer";
            hSafeFwVer.ReadOnly = true;
            hSafeFwVer.Size = new Size(180, 31);
            hSafeFwVer.TabIndex = 10;
            hSafeFwVer.TabStop = false;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(9, 243);
            label8.Name = "label8";
            label8.Size = new Size(126, 25);
            label8.TabIndex = 9;
            label8.Text = "Backup fw ver:";
            // 
            // hMode
            // 
            hMode.Location = new Point(137, 189);
            hMode.Margin = new Padding(3, 4, 3, 4);
            hMode.Name = "hMode";
            hMode.ReadOnly = true;
            hMode.Size = new Size(180, 31);
            hMode.TabIndex = 8;
            hMode.TabStop = false;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(66, 192);
            label7.Name = "label7";
            label7.Size = new Size(63, 25);
            label7.TabIndex = 7;
            label7.Text = "Mode:";
            // 
            // hFwVerTextBox
            // 
            hFwVerTextBox.Location = new Point(137, 136);
            hFwVerTextBox.Margin = new Padding(3, 4, 3, 4);
            hFwVerTextBox.Name = "hFwVerTextBox";
            hFwVerTextBox.ReadOnly = true;
            hFwVerTextBox.Size = new Size(180, 31);
            hFwVerTextBox.TabIndex = 6;
            hFwVerTextBox.TabStop = false;
            // 
            // hHwRevTextBox
            // 
            hHwRevTextBox.Location = new Point(137, 85);
            hHwRevTextBox.Margin = new Padding(3, 4, 3, 4);
            hHwRevTextBox.Name = "hHwRevTextBox";
            hHwRevTextBox.ReadOnly = true;
            hHwRevTextBox.Size = new Size(180, 31);
            hHwRevTextBox.TabIndex = 5;
            hHwRevTextBox.TabStop = false;
            // 
            // hHubNameTextBox
            // 
            hHubNameTextBox.Location = new Point(137, 39);
            hHubNameTextBox.Margin = new Padding(3, 4, 3, 4);
            hHubNameTextBox.Name = "hHubNameTextBox";
            hHubNameTextBox.ReadOnly = true;
            hHubNameTextBox.Size = new Size(180, 31);
            hHubNameTextBox.TabIndex = 4;
            hHubNameTextBox.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(24, 142);
            label1.Name = "label1";
            label1.Size = new Size(105, 25);
            label1.TabIndex = 2;
            label1.Text = "FW Version:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(23, 91);
            label5.Name = "label5";
            label5.Size = new Size(112, 25);
            label5.TabIndex = 1;
            label5.Text = "HW revision:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(32, 44);
            label6.Name = "label6";
            label6.Size = new Size(102, 25);
            label6.TabIndex = 0;
            label6.Text = "Hub Name:";
            // 
            // progressUpdate
            // 
            progressUpdate.Location = new Point(12, 307);
            progressUpdate.Name = "progressUpdate";
            progressUpdate.Size = new Size(705, 34);
            progressUpdate.TabIndex = 8;
            // 
            // programButton
            // 
            programButton.Location = new Point(12, 247);
            programButton.Name = "programButton";
            programButton.Size = new Size(112, 34);
            programButton.TabIndex = 9;
            programButton.Text = "Program";
            programButton.UseVisualStyleBackColor = true;
            programButton.Click += programButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.Location = new Point(239, 247);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(112, 34);
            cancelButton.TabIndex = 10;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += cancelButton_Click;
            // 
            // UpdateHub
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(734, 356);
            ControlBox = false;
            Controls.Add(cancelButton);
            Controls.Add(programButton);
            Controls.Add(progressUpdate);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            MaximizeBox = false;
            Name = "UpdateHub";
            Text = "Update ready";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private TextBox fFwVerTextBox;
        private TextBox fHwRevTextBox;
        private TextBox fHubNameTextBox;
        private Label label4;
        private Label label3;
        private Label label2;
        private GroupBox groupBox2;
        private TextBox hFwVerTextBox;
        private TextBox hHwRevTextBox;
        private TextBox hHubNameTextBox;
        private Label label1;
        private Label label5;
        private Label label6;
        private ProgressBar progressUpdate;
        private Button programButton;
        private Button cancelButton;
        private TextBox hMode;
        private Label label7;
        private TextBox hSafeFwVer;
        private Label label8;
    }
}