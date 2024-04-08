namespace MorphTool
{
    partial class GetIDStartNumber
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
            this.startNumber = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Go_button = new System.Windows.Forms.Button();
            this.Cancel_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // startNumber
            // 
            this.startNumber.Location = new System.Drawing.Point(168, 52);
            this.startNumber.Name = "startNumber";
            this.startNumber.Size = new System.Drawing.Size(100, 20);
            this.startNumber.TabIndex = 0;
            this.startNumber.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(38, 55);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Enter starting ID number:";
            // 
            // Go_button
            // 
            this.Go_button.Location = new System.Drawing.Point(111, 97);
            this.Go_button.Name = "Go_button";
            this.Go_button.Size = new System.Drawing.Size(93, 29);
            this.Go_button.TabIndex = 2;
            this.Go_button.Text = "Okay";
            this.Go_button.UseVisualStyleBackColor = true;
            this.Go_button.Click += new System.EventHandler(this.Go_button_Click);
            // 
            // Cancel_button
            // 
            this.Cancel_button.Location = new System.Drawing.Point(111, 132);
            this.Cancel_button.Name = "Cancel_button";
            this.Cancel_button.Size = new System.Drawing.Size(93, 29);
            this.Cancel_button.TabIndex = 3;
            this.Cancel_button.Text = "Cancel";
            this.Cancel_button.UseVisualStyleBackColor = true;
            this.Cancel_button.Click += new System.EventHandler(this.Cancel_button_Click);
            // 
            // GetIDStartNumber
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(316, 208);
            this.Controls.Add(this.Cancel_button);
            this.Controls.Add(this.Go_button);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.startNumber);
            this.Name = "GetIDStartNumber";
            this.Text = "GetIDStartNumber";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox startNumber;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Go_button;
        private System.Windows.Forms.Button Cancel_button;
    }
}