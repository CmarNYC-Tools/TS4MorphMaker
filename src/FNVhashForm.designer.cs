namespace MorphTool
{
    partial class FNVhashForm
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
            this.hashStr = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.FNV24hexStr = new System.Windows.Forms.TextBox();
            this.FNV32hexStr = new System.Windows.Forms.TextBox();
            this.FNV64hexStr = new System.Windows.Forms.TextBox();
            this.FNVhashButton = new System.Windows.Forms.Button();
            this.Copy24 = new System.Windows.Forms.Button();
            this.Copy32 = new System.Windows.Forms.Button();
            this.Copy64 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // hashStr
            // 
            this.hashStr.Location = new System.Drawing.Point(66, 8);
            this.hashStr.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.hashStr.Name = "hashStr";
            this.hashStr.Size = new System.Drawing.Size(226, 20);
            this.hashStr.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Enter Text: ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1, 50);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "FNV24 Hash: ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1, 77);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "FNV32 Hash: ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1, 104);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "FNV64 Hash: ";
            // 
            // FNV24hexStr
            // 
            this.FNV24hexStr.Location = new System.Drawing.Point(77, 50);
            this.FNV24hexStr.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.FNV24hexStr.Name = "FNV24hexStr";
            this.FNV24hexStr.Size = new System.Drawing.Size(76, 20);
            this.FNV24hexStr.TabIndex = 5;
            // 
            // FNV32hexStr
            // 
            this.FNV32hexStr.Location = new System.Drawing.Point(77, 77);
            this.FNV32hexStr.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.FNV32hexStr.Name = "FNV32hexStr";
            this.FNV32hexStr.Size = new System.Drawing.Size(76, 20);
            this.FNV32hexStr.TabIndex = 6;
            // 
            // FNV64hexStr
            // 
            this.FNV64hexStr.Location = new System.Drawing.Point(77, 104);
            this.FNV64hexStr.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.FNV64hexStr.Name = "FNV64hexStr";
            this.FNV64hexStr.Size = new System.Drawing.Size(133, 20);
            this.FNV64hexStr.TabIndex = 7;
            // 
            // FNVhashButton
            // 
            this.FNVhashButton.Location = new System.Drawing.Point(104, 135);
            this.FNVhashButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.FNVhashButton.Name = "FNVhashButton";
            this.FNVhashButton.Size = new System.Drawing.Size(89, 28);
            this.FNVhashButton.TabIndex = 8;
            this.FNVhashButton.Text = "Compute!";
            this.FNVhashButton.UseVisualStyleBackColor = true;
            this.FNVhashButton.Click += new System.EventHandler(this.FNVhashButton_Click);
            // 
            // Copy24
            // 
            this.Copy24.Location = new System.Drawing.Point(235, 49);
            this.Copy24.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Copy24.Name = "Copy24";
            this.Copy24.Size = new System.Drawing.Size(56, 21);
            this.Copy24.TabIndex = 9;
            this.Copy24.Text = "Copy";
            this.Copy24.UseVisualStyleBackColor = true;
            this.Copy24.Click += new System.EventHandler(this.Copy24_Click);
            // 
            // Copy32
            // 
            this.Copy32.Location = new System.Drawing.Point(235, 76);
            this.Copy32.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Copy32.Name = "Copy32";
            this.Copy32.Size = new System.Drawing.Size(56, 21);
            this.Copy32.TabIndex = 10;
            this.Copy32.Text = "Copy";
            this.Copy32.UseVisualStyleBackColor = true;
            this.Copy32.Click += new System.EventHandler(this.Copy32_Click);
            // 
            // Copy64
            // 
            this.Copy64.Location = new System.Drawing.Point(235, 102);
            this.Copy64.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Copy64.Name = "Copy64";
            this.Copy64.Size = new System.Drawing.Size(56, 22);
            this.Copy64.TabIndex = 11;
            this.Copy64.Text = "Copy";
            this.Copy64.UseVisualStyleBackColor = true;
            this.Copy64.Click += new System.EventHandler(this.Copy64_Click);
            // 
            // FNVhashForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(297, 176);
            this.Controls.Add(this.Copy64);
            this.Controls.Add(this.Copy32);
            this.Controls.Add(this.Copy24);
            this.Controls.Add(this.FNVhashButton);
            this.Controls.Add(this.FNV64hexStr);
            this.Controls.Add(this.FNV32hexStr);
            this.Controls.Add(this.FNV24hexStr);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.hashStr);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "FNVhashForm";
            this.Text = "FNV Hash Form";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox hashStr;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox FNV24hexStr;
        private System.Windows.Forms.TextBox FNV32hexStr;
        private System.Windows.Forms.TextBox FNV64hexStr;
        private System.Windows.Forms.Button FNVhashButton;
        private System.Windows.Forms.Button Copy24;
        private System.Windows.Forms.Button Copy32;
        private System.Windows.Forms.Button Copy64;
    }
}