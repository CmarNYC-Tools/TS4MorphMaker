namespace MorphTool
{
    partial class ClonePickerForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.CloneControl_label = new System.Windows.Forms.Label();
            this.CloneControl_Go_button = new System.Windows.Forms.Button();
            this.CloneControl_dataGridView = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.CloneDefaultAll_radioButton = new System.Windows.Forms.RadioButton();
            this.CloneDefault_radioButton = new System.Windows.Forms.RadioButton();
            this.CloneCustom_radioButton = new System.Windows.Forms.RadioButton();
            this.CloneControlTGI = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CloneControlName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ControlCloneRegion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Species = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Occult = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ControlCloneAgeGender = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ControlCloneFrame = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ControlCloneMorphs = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ControlCloneSelect = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.CloneControl_dataGridView)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // CloneControl_label
            // 
            this.CloneControl_label.AutoSize = true;
            this.CloneControl_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CloneControl_label.Location = new System.Drawing.Point(9, 7);
            this.CloneControl_label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.CloneControl_label.Name = "CloneControl_label";
            this.CloneControl_label.Size = new System.Drawing.Size(96, 13);
            this.CloneControl_label.TabIndex = 10;
            this.CloneControl_label.Text = "List of Controls:";
            // 
            // CloneControl_Go_button
            // 
            this.CloneControl_Go_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CloneControl_Go_button.Location = new System.Drawing.Point(662, 450);
            this.CloneControl_Go_button.Margin = new System.Windows.Forms.Padding(2);
            this.CloneControl_Go_button.Name = "CloneControl_Go_button";
            this.CloneControl_Go_button.Size = new System.Drawing.Size(217, 35);
            this.CloneControl_Go_button.TabIndex = 9;
            this.CloneControl_Go_button.Text = "Clone Selected";
            this.CloneControl_Go_button.UseVisualStyleBackColor = true;
            this.CloneControl_Go_button.Click += new System.EventHandler(this.CloneControl_Go_button_Click);
            // 
            // CloneControl_dataGridView
            // 
            this.CloneControl_dataGridView.AllowUserToAddRows = false;
            this.CloneControl_dataGridView.AllowUserToDeleteRows = false;
            this.CloneControl_dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CloneControl_dataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.CloneControl_dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.CloneControl_dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.CloneControlTGI,
            this.CloneControlName,
            this.ControlCloneRegion,
            this.Species,
            this.Occult,
            this.ControlCloneAgeGender,
            this.ControlCloneFrame,
            this.ControlCloneMorphs,
            this.ControlCloneSelect});
            this.CloneControl_dataGridView.Location = new System.Drawing.Point(9, 24);
            this.CloneControl_dataGridView.Margin = new System.Windows.Forms.Padding(2);
            this.CloneControl_dataGridView.Name = "CloneControl_dataGridView";
            this.CloneControl_dataGridView.RowHeadersVisible = false;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.CloneControl_dataGridView.RowsDefaultCellStyle = dataGridViewCellStyle1;
            this.CloneControl_dataGridView.RowTemplate.Height = 24;
            this.CloneControl_dataGridView.Size = new System.Drawing.Size(872, 420);
            this.CloneControl_dataGridView.TabIndex = 6;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panel1.Controls.Add(this.CloneDefaultAll_radioButton);
            this.panel1.Controls.Add(this.CloneDefault_radioButton);
            this.panel1.Controls.Add(this.CloneCustom_radioButton);
            this.panel1.Location = new System.Drawing.Point(12, 456);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(570, 35);
            this.panel1.TabIndex = 11;
            // 
            // CloneDefaultAll_radioButton
            // 
            this.CloneDefaultAll_radioButton.AutoSize = true;
            this.CloneDefaultAll_radioButton.Location = new System.Drawing.Point(261, 3);
            this.CloneDefaultAll_radioButton.Name = "CloneDefaultAll_radioButton";
            this.CloneDefaultAll_radioButton.Size = new System.Drawing.Size(251, 17);
            this.CloneDefaultAll_radioButton.TabIndex = 2;
            this.CloneDefaultAll_radioButton.TabStop = true;
            this.CloneDefaultAll_radioButton.Text = "Default Replacement With All Morph Resources";
            this.CloneDefaultAll_radioButton.UseVisualStyleBackColor = true;
            // 
            // CloneDefault_radioButton
            // 
            this.CloneDefault_radioButton.AutoSize = true;
            this.CloneDefault_radioButton.Location = new System.Drawing.Point(131, 2);
            this.CloneDefault_radioButton.Margin = new System.Windows.Forms.Padding(2);
            this.CloneDefault_radioButton.Name = "CloneDefault_radioButton";
            this.CloneDefault_radioButton.Size = new System.Drawing.Size(125, 17);
            this.CloneDefault_radioButton.TabIndex = 1;
            this.CloneDefault_radioButton.Text = "Default Replacement";
            this.CloneDefault_radioButton.UseVisualStyleBackColor = true;
            // 
            // CloneCustom_radioButton
            // 
            this.CloneCustom_radioButton.AutoSize = true;
            this.CloneCustom_radioButton.Checked = true;
            this.CloneCustom_radioButton.Location = new System.Drawing.Point(2, 2);
            this.CloneCustom_radioButton.Margin = new System.Windows.Forms.Padding(2);
            this.CloneCustom_radioButton.Name = "CloneCustom_radioButton";
            this.CloneCustom_radioButton.Size = new System.Drawing.Size(125, 17);
            this.CloneCustom_radioButton.TabIndex = 0;
            this.CloneCustom_radioButton.TabStop = true;
            this.CloneCustom_radioButton.Text = "New Custom Content";
            this.CloneCustom_radioButton.UseVisualStyleBackColor = true;
            // 
            // CloneControlTGI
            // 
            this.CloneControlTGI.HeaderText = "TGI";
            this.CloneControlTGI.Name = "CloneControlTGI";
            this.CloneControlTGI.Width = 260;
            // 
            // CloneControlName
            // 
            this.CloneControlName.HeaderText = "Name";
            this.CloneControlName.Name = "CloneControlName";
            this.CloneControlName.Width = 150;
            // 
            // ControlCloneRegion
            // 
            this.ControlCloneRegion.HeaderText = "Region";
            this.ControlCloneRegion.Name = "ControlCloneRegion";
            this.ControlCloneRegion.ReadOnly = true;
            // 
            // Species
            // 
            this.Species.HeaderText = "Species";
            this.Species.Name = "Species";
            // 
            // Occult
            // 
            this.Occult.HeaderText = "Occult";
            this.Occult.Name = "Occult";
            // 
            // ControlCloneAgeGender
            // 
            this.ControlCloneAgeGender.HeaderText = "AgeGender";
            this.ControlCloneAgeGender.Name = "ControlCloneAgeGender";
            this.ControlCloneAgeGender.ReadOnly = true;
            // 
            // ControlCloneFrame
            // 
            this.ControlCloneFrame.HeaderText = "Frame";
            this.ControlCloneFrame.Name = "ControlCloneFrame";
            this.ControlCloneFrame.ReadOnly = true;
            this.ControlCloneFrame.Width = 75;
            // 
            // ControlCloneMorphs
            // 
            this.ControlCloneMorphs.HeaderText = "Morphs";
            this.ControlCloneMorphs.Name = "ControlCloneMorphs";
            this.ControlCloneMorphs.Width = 300;
            // 
            // ControlCloneSelect
            // 
            this.ControlCloneSelect.HeaderText = "Select";
            this.ControlCloneSelect.Name = "ControlCloneSelect";
            this.ControlCloneSelect.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ControlCloneSelect.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.ControlCloneSelect.Width = 75;
            // 
            // ClonePickerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(890, 502);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.CloneControl_label);
            this.Controls.Add(this.CloneControl_Go_button);
            this.Controls.Add(this.CloneControl_dataGridView);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ClonePickerForm";
            this.Text = "ClonePickerForm";
            ((System.ComponentModel.ISupportInitialize)(this.CloneControl_dataGridView)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label CloneControl_label;
        private System.Windows.Forms.Button CloneControl_Go_button;
        private System.Windows.Forms.DataGridView CloneControl_dataGridView;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton CloneDefault_radioButton;
        private System.Windows.Forms.RadioButton CloneCustom_radioButton;
        private System.Windows.Forms.RadioButton CloneDefaultAll_radioButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn CloneControlTGI;
        private System.Windows.Forms.DataGridViewTextBoxColumn CloneControlName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ControlCloneRegion;
        private System.Windows.Forms.DataGridViewTextBoxColumn Species;
        private System.Windows.Forms.DataGridViewTextBoxColumn Occult;
        private System.Windows.Forms.DataGridViewTextBoxColumn ControlCloneAgeGender;
        private System.Windows.Forms.DataGridViewTextBoxColumn ControlCloneFrame;
        private System.Windows.Forms.DataGridViewTextBoxColumn ControlCloneMorphs;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ControlCloneSelect;
    }
}