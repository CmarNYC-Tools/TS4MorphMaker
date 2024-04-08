namespace MorphTool
{
    partial class Modifier_Chooser_Form
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.EA_radioButton = new System.Windows.Forms.RadioButton();
            this.Custom_radioButton = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.SMOD_Chooser_comboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Chooser_Go_button = new System.Windows.Forms.Button();
            this.Chooser_Cancel_button = new System.Windows.Forms.Button();
            this.Chooser_Remove_button = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.EA_radioButton);
            this.panel1.Controls.Add(this.Custom_radioButton);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(454, 51);
            this.panel1.TabIndex = 0;
            // 
            // EA_radioButton
            // 
            this.EA_radioButton.AutoSize = true;
            this.EA_radioButton.Location = new System.Drawing.Point(233, 13);
            this.EA_radioButton.Name = "EA_radioButton";
            this.EA_radioButton.Size = new System.Drawing.Size(148, 21);
            this.EA_radioButton.TabIndex = 2;
            this.EA_radioButton.Text = "Game Sim Modifier";
            this.EA_radioButton.UseVisualStyleBackColor = true;
            this.EA_radioButton.CheckedChanged += new System.EventHandler(this.EA_radioButton_CheckedChanged);
            // 
            // Custom_radioButton
            // 
            this.Custom_radioButton.AutoSize = true;
            this.Custom_radioButton.Checked = true;
            this.Custom_radioButton.Location = new System.Drawing.Point(70, 13);
            this.Custom_radioButton.Name = "Custom_radioButton";
            this.Custom_radioButton.Size = new System.Drawing.Size(157, 21);
            this.Custom_radioButton.TabIndex = 1;
            this.Custom_radioButton.TabStop = true;
            this.Custom_radioButton.Text = "Custom Sim Modifier";
            this.Custom_radioButton.UseVisualStyleBackColor = true;
            this.Custom_radioButton.CheckedChanged += new System.EventHandler(this.Custom_radioButton_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Choose:";
            // 
            // SMOD_Chooser_comboBox
            // 
            this.SMOD_Chooser_comboBox.FormattingEnabled = true;
            this.SMOD_Chooser_comboBox.Location = new System.Drawing.Point(72, 92);
            this.SMOD_Chooser_comboBox.Name = "SMOD_Chooser_comboBox";
            this.SMOD_Chooser_comboBox.Size = new System.Drawing.Size(394, 24);
            this.SMOD_Chooser_comboBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Select:";
            // 
            // Chooser_Go_button
            // 
            this.Chooser_Go_button.Location = new System.Drawing.Point(104, 187);
            this.Chooser_Go_button.Name = "Chooser_Go_button";
            this.Chooser_Go_button.Size = new System.Drawing.Size(131, 45);
            this.Chooser_Go_button.TabIndex = 3;
            this.Chooser_Go_button.Text = "Save";
            this.Chooser_Go_button.UseVisualStyleBackColor = true;
            this.Chooser_Go_button.Click += new System.EventHandler(this.SMOD_Chooser_Go_button_Click);
            // 
            // Chooser_Cancel_button
            // 
            this.Chooser_Cancel_button.Location = new System.Drawing.Point(249, 187);
            this.Chooser_Cancel_button.Name = "Chooser_Cancel_button";
            this.Chooser_Cancel_button.Size = new System.Drawing.Size(131, 45);
            this.Chooser_Cancel_button.TabIndex = 4;
            this.Chooser_Cancel_button.Text = "Cancel";
            this.Chooser_Cancel_button.UseVisualStyleBackColor = true;
            this.Chooser_Cancel_button.Click += new System.EventHandler(this.SMOD_Chooser_Cancel_button_Click);
            // 
            // Chooser_Remove_button
            // 
            this.Chooser_Remove_button.Location = new System.Drawing.Point(125, 122);
            this.Chooser_Remove_button.Name = "Chooser_Remove_button";
            this.Chooser_Remove_button.Size = new System.Drawing.Size(234, 45);
            this.Chooser_Remove_button.TabIndex = 5;
            this.Chooser_Remove_button.Text = "Remove Sim Modifier";
            this.Chooser_Remove_button.UseVisualStyleBackColor = true;
            this.Chooser_Remove_button.Click += new System.EventHandler(this.SMOD_Chooser_Remove_button_Click);
            // 
            // Modifier_Chooser_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 249);
            this.Controls.Add(this.Chooser_Remove_button);
            this.Controls.Add(this.Chooser_Cancel_button);
            this.Controls.Add(this.Chooser_Go_button);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.SMOD_Chooser_comboBox);
            this.Controls.Add(this.panel1);
            this.Name = "Modifier_Chooser_Form";
            this.Text = "SMOD_Chooser_Form";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton EA_radioButton;
        private System.Windows.Forms.RadioButton Custom_radioButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox SMOD_Chooser_comboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button Chooser_Go_button;
        private System.Windows.Forms.Button Chooser_Cancel_button;
        private System.Windows.Forms.Button Chooser_Remove_button;
    }
}