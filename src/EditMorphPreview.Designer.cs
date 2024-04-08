namespace MorphTool
{
    partial class EditMorphPreview
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
            this.components = new System.ComponentModel.Container();
            this.ShowMorph_checkBox = new System.Windows.Forms.CheckBox();
            this.SaveHead_button = new System.Windows.Forms.Button();
            this.SaveBody_button = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.morphPreview = new System.Windows.Forms.Integration.ElementHost();
            this.morphPreview1 = new MorphTool.MorphPreview();
            this.ShowOverlay_checkBox = new System.Windows.Forms.CheckBox();
            this.AgeGender_listBox = new System.Windows.Forms.ListBox();
            this.Type1_listBox = new System.Windows.Forms.ListBox();
            this.Type2_listBox = new System.Windows.Forms.ListBox();
            this.Species_listBox = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // ShowMorph_checkBox
            // 
            this.ShowMorph_checkBox.AutoSize = true;
            this.ShowMorph_checkBox.Checked = true;
            this.ShowMorph_checkBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowMorph_checkBox.Location = new System.Drawing.Point(14, 387);
            this.ShowMorph_checkBox.Margin = new System.Windows.Forms.Padding(2);
            this.ShowMorph_checkBox.Name = "ShowMorph_checkBox";
            this.ShowMorph_checkBox.Size = new System.Drawing.Size(86, 17);
            this.ShowMorph_checkBox.TabIndex = 4;
            this.ShowMorph_checkBox.Text = "Show Morph";
            this.ShowMorph_checkBox.UseVisualStyleBackColor = true;
            this.ShowMorph_checkBox.CheckedChanged += new System.EventHandler(this.ShowMorph_checkBox_CheckedChanged);
            // 
            // SaveHead_button
            // 
            this.SaveHead_button.Location = new System.Drawing.Point(10, 456);
            this.SaveHead_button.Name = "SaveHead_button";
            this.SaveHead_button.Size = new System.Drawing.Size(96, 57);
            this.SaveHead_button.TabIndex = 5;
            this.SaveHead_button.Text = "Save Morphed Head";
            this.toolTip1.SetToolTip(this.SaveHead_button, "Click to save as .obj\r\nControl-click to save as .simgeom\r\nShift-click to save as " +
        ".ms3d\r\n\r\n\r\n");
            this.SaveHead_button.UseVisualStyleBackColor = true;
            this.SaveHead_button.Click += new System.EventHandler(this.SaveHeadMorph_button_Click);
            // 
            // SaveBody_button
            // 
            this.SaveBody_button.Location = new System.Drawing.Point(10, 519);
            this.SaveBody_button.Name = "SaveBody_button";
            this.SaveBody_button.Size = new System.Drawing.Size(96, 57);
            this.SaveBody_button.TabIndex = 6;
            this.SaveBody_button.Text = "Save Morphed Whole Sim";
            this.toolTip1.SetToolTip(this.SaveBody_button, "Click to save as .obj\r\nControl-click to save as .simgeom\r\nShift-click to save as " +
        ".ms3d");
            this.SaveBody_button.UseVisualStyleBackColor = true;
            this.SaveBody_button.Click += new System.EventHandler(this.SaveBodyMorph_button_Click);
            // 
            // morphPreview
            // 
            this.morphPreview.Location = new System.Drawing.Point(111, 11);
            this.morphPreview.Margin = new System.Windows.Forms.Padding(2);
            this.morphPreview.Name = "morphPreview";
            this.morphPreview.Size = new System.Drawing.Size(500, 600);
            this.morphPreview.TabIndex = 0;
            this.morphPreview.Text = "MorphPreview";
            this.morphPreview.Child = this.morphPreview1;
            // 
            // ShowOverlay_checkBox
            // 
            this.ShowOverlay_checkBox.AutoSize = true;
            this.ShowOverlay_checkBox.Checked = true;
            this.ShowOverlay_checkBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowOverlay_checkBox.Location = new System.Drawing.Point(14, 410);
            this.ShowOverlay_checkBox.Name = "ShowOverlay_checkBox";
            this.ShowOverlay_checkBox.Size = new System.Drawing.Size(92, 17);
            this.ShowOverlay_checkBox.TabIndex = 7;
            this.ShowOverlay_checkBox.Text = "Show Overlay";
            this.ShowOverlay_checkBox.UseVisualStyleBackColor = true;
            this.ShowOverlay_checkBox.CheckedChanged += new System.EventHandler(this.ShowOverlay_checkBox_CheckedChanged);
            // 
            // AgeGender_listBox
            // 
            this.AgeGender_listBox.FormattingEnabled = true;
            this.AgeGender_listBox.Items.AddRange(new object[] {
            "Infant",
            "Toddler",
            "Child",
            "Adult Male",
            "Adult Female"});
            this.AgeGender_listBox.Location = new System.Drawing.Point(10, 137);
            this.AgeGender_listBox.Name = "AgeGender_listBox";
            this.AgeGender_listBox.Size = new System.Drawing.Size(96, 82);
            this.AgeGender_listBox.TabIndex = 8;
            this.AgeGender_listBox.SelectedIndexChanged += new System.EventHandler(this.AgeGender_listBox_SelectedIndexChanged);
            // 
            // Type1_listBox
            // 
            this.Type1_listBox.FormattingEnabled = true;
            this.Type1_listBox.Items.AddRange(new object[] {
            "Male Frame",
            "Female Frame"});
            this.Type1_listBox.Location = new System.Drawing.Point(10, 225);
            this.Type1_listBox.Name = "Type1_listBox";
            this.Type1_listBox.Size = new System.Drawing.Size(96, 56);
            this.Type1_listBox.TabIndex = 9;
            this.Type1_listBox.SelectedIndexChanged += new System.EventHandler(this.Type1_listBox_SelectedIndexChanged);
            // 
            // Type2_listBox
            // 
            this.Type2_listBox.FormattingEnabled = true;
            this.Type2_listBox.Items.AddRange(new object[] {
            "Skintight",
            "Robe"});
            this.Type2_listBox.Location = new System.Drawing.Point(10, 287);
            this.Type2_listBox.Name = "Type2_listBox";
            this.Type2_listBox.Size = new System.Drawing.Size(96, 69);
            this.Type2_listBox.TabIndex = 10;
            this.Type2_listBox.SelectedIndexChanged += new System.EventHandler(this.Type2_listBox_SelectedIndexChanged);
            // 
            // Species_listBox
            // 
            this.Species_listBox.FormattingEnabled = true;
            this.Species_listBox.Items.AddRange(new object[] {
            "Human",
            "Dog",
            "Cat",
            "Little Dog",
            "Werewolf",
            "Horse"});
            this.Species_listBox.Location = new System.Drawing.Point(10, 49);
            this.Species_listBox.Name = "Species_listBox";
            this.Species_listBox.Size = new System.Drawing.Size(96, 82);
            this.Species_listBox.TabIndex = 11;
            this.Species_listBox.SelectedIndexChanged += new System.EventHandler(this.Species_listBox_SelectedIndexChanged);
            // 
            // EditMorphPreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(617, 616);
            this.Controls.Add(this.Species_listBox);
            this.Controls.Add(this.Type2_listBox);
            this.Controls.Add(this.Type1_listBox);
            this.Controls.Add(this.AgeGender_listBox);
            this.Controls.Add(this.ShowOverlay_checkBox);
            this.Controls.Add(this.SaveBody_button);
            this.Controls.Add(this.SaveHead_button);
            this.Controls.Add(this.ShowMorph_checkBox);
            this.Controls.Add(this.morphPreview);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "EditMorphPreview";
            this.Text = "EditMorphPreview";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost morphPreview;
        private MorphPreview morphPreview1;
        private System.Windows.Forms.CheckBox ShowMorph_checkBox;
        private System.Windows.Forms.Button SaveHead_button;
        private System.Windows.Forms.Button SaveBody_button;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox ShowOverlay_checkBox;
        private System.Windows.Forms.ListBox AgeGender_listBox;
        private System.Windows.Forms.ListBox Type1_listBox;
        private System.Windows.Forms.ListBox Type2_listBox;
        private System.Windows.Forms.ListBox Species_listBox;
    }
}