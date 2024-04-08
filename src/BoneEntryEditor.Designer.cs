namespace MorphTool
{
    partial class BoneEntryEditor
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.multiplier = new System.Windows.Forms.TextBox();
            this.Save_button = new System.Windows.Forms.Button();
            this.Cancel_button = new System.Windows.Forms.Button();
            this.BoneEntry_dataGridView = new System.Windows.Forms.DataGridView();
            this.bones_comboBox = new System.Windows.Forms.ComboBox();
            this.Add_button = new System.Windows.Forms.Button();
            this.dataGridViewButtonColumn1 = new System.Windows.Forms.DataGridViewButtonColumn();
            this.bHash = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bDelete = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this.BoneEntry_dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 306);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Bone:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(94, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(260, 26);
            this.label2.TabIndex = 1;
            this.label2.Text = "Sets a limit on how much of a bone delta is applied, to\r\nprevent clipping and rel" +
    "ated problems in animations";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(276, 306);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Multiplier:";
            // 
            // multiplier
            // 
            this.multiplier.Location = new System.Drawing.Point(333, 303);
            this.multiplier.Name = "multiplier";
            this.multiplier.Size = new System.Drawing.Size(89, 20);
            this.multiplier.TabIndex = 4;
            // 
            // Save_button
            // 
            this.Save_button.Location = new System.Drawing.Point(141, 348);
            this.Save_button.Name = "Save_button";
            this.Save_button.Size = new System.Drawing.Size(80, 35);
            this.Save_button.TabIndex = 5;
            this.Save_button.Text = "Save";
            this.Save_button.UseVisualStyleBackColor = true;
            this.Save_button.Click += new System.EventHandler(this.Save_button_Click);
            // 
            // Cancel_button
            // 
            this.Cancel_button.Location = new System.Drawing.Point(227, 348);
            this.Cancel_button.Name = "Cancel_button";
            this.Cancel_button.Size = new System.Drawing.Size(80, 35);
            this.Cancel_button.TabIndex = 6;
            this.Cancel_button.Text = "Cancel";
            this.Cancel_button.UseVisualStyleBackColor = true;
            this.Cancel_button.Click += new System.EventHandler(this.Cancel_button_Click);
            // 
            // BoneEntry_dataGridView
            // 
            this.BoneEntry_dataGridView.AllowUserToAddRows = false;
            this.BoneEntry_dataGridView.AllowUserToDeleteRows = false;
            this.BoneEntry_dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.BoneEntry_dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.bHash,
            this.bName,
            this.bDelete});
            this.BoneEntry_dataGridView.Location = new System.Drawing.Point(12, 108);
            this.BoneEntry_dataGridView.Name = "BoneEntry_dataGridView";
            this.BoneEntry_dataGridView.ReadOnly = true;
            this.BoneEntry_dataGridView.RowHeadersVisible = false;
            this.BoneEntry_dataGridView.Size = new System.Drawing.Size(410, 180);
            this.BoneEntry_dataGridView.TabIndex = 7;
            this.BoneEntry_dataGridView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.BoneEntry_dataGridView_CellClick);
            // 
            // bones_comboBox
            // 
            this.bones_comboBox.FormattingEnabled = true;
            this.bones_comboBox.Location = new System.Drawing.Point(50, 302);
            this.bones_comboBox.Name = "bones_comboBox";
            this.bones_comboBox.Size = new System.Drawing.Size(171, 21);
            this.bones_comboBox.TabIndex = 8;
            // 
            // Add_button
            // 
            this.Add_button.Location = new System.Drawing.Point(319, 69);
            this.Add_button.Name = "Add_button";
            this.Add_button.Size = new System.Drawing.Size(103, 33);
            this.Add_button.TabIndex = 9;
            this.Add_button.Text = "Add Entry";
            this.Add_button.UseVisualStyleBackColor = true;
            this.Add_button.Click += new System.EventHandler(this.Add_button_Click);
            // 
            // dataGridViewButtonColumn1
            // 
            this.dataGridViewButtonColumn1.HeaderText = "Delete Bone";
            this.dataGridViewButtonColumn1.Name = "dataGridViewButtonColumn1";
            this.dataGridViewButtonColumn1.ReadOnly = true;
            this.dataGridViewButtonColumn1.Text = "Delete";
            // 
            // bHash
            // 
            this.bHash.HeaderText = "Bone Hash";
            this.bHash.Name = "bHash";
            this.bHash.ReadOnly = true;
            // 
            // bName
            // 
            this.bName.HeaderText = "Bone Name";
            this.bName.Name = "bName";
            this.bName.ReadOnly = true;
            this.bName.Width = 200;
            // 
            // bDelete
            // 
            this.bDelete.HeaderText = "Delete";
            this.bDelete.Name = "bDelete";
            this.bDelete.ReadOnly = true;
            this.bDelete.Text = "";
            this.bDelete.UseColumnTextForButtonValue = true;
            // 
            // BoneEntryEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 411);
            this.Controls.Add(this.Add_button);
            this.Controls.Add(this.bones_comboBox);
            this.Controls.Add(this.BoneEntry_dataGridView);
            this.Controls.Add(this.Cancel_button);
            this.Controls.Add(this.Save_button);
            this.Controls.Add(this.multiplier);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "BoneEntryEditor";
            this.Text = "BoneEntryEditor";
            ((System.ComponentModel.ISupportInitialize)(this.BoneEntry_dataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox multiplier;
        private System.Windows.Forms.Button Save_button;
        private System.Windows.Forms.Button Cancel_button;
        private System.Windows.Forms.DataGridView BoneEntry_dataGridView;
        private System.Windows.Forms.ComboBox bones_comboBox;
        private System.Windows.Forms.Button Add_button;
        private System.Windows.Forms.DataGridViewButtonColumn dataGridViewButtonColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn bHash;
        private System.Windows.Forms.DataGridViewTextBoxColumn bName;
        private System.Windows.Forms.DataGridViewButtonColumn bDelete;
    }
}