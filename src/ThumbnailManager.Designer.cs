namespace MorphTool
{
    partial class ThumbnailManager
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
            this.ThumbType_label = new System.Windows.Forms.Label();
            this.Thumb_pictureBox = new System.Windows.Forms.PictureBox();
            this.ThumbImport_button = new System.Windows.Forms.Button();
            this.ThumbExport_button = new System.Windows.Forms.Button();
            this.ThumbSave_button = new System.Windows.Forms.Button();
            this.ThumbRemove_button = new System.Windows.Forms.Button();
            this.ThumbCancel_button = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.Thumb_pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // ThumbType_label
            // 
            this.ThumbType_label.AutoSize = true;
            this.ThumbType_label.Location = new System.Drawing.Point(9, 7);
            this.ThumbType_label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ThumbType_label.Name = "ThumbType_label";
            this.ThumbType_label.Size = new System.Drawing.Size(99, 13);
            this.ThumbType_label.TabIndex = 0;
            this.ThumbType_label.Text = "Current Thumbnail: ";
            // 
            // Thumb_pictureBox
            // 
            this.Thumb_pictureBox.Location = new System.Drawing.Point(144, 55);
            this.Thumb_pictureBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Thumb_pictureBox.Name = "Thumb_pictureBox";
            this.Thumb_pictureBox.Size = new System.Drawing.Size(100, 100);
            this.Thumb_pictureBox.TabIndex = 1;
            this.Thumb_pictureBox.TabStop = false;
            // 
            // ThumbImport_button
            // 
            this.ThumbImport_button.Location = new System.Drawing.Point(9, 55);
            this.ThumbImport_button.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ThumbImport_button.Name = "ThumbImport_button";
            this.ThumbImport_button.Size = new System.Drawing.Size(111, 37);
            this.ThumbImport_button.TabIndex = 2;
            this.ThumbImport_button.Text = "Import";
            this.ThumbImport_button.UseVisualStyleBackColor = true;
            this.ThumbImport_button.Click += new System.EventHandler(this.ThumbImport_button_Click);
            // 
            // ThumbExport_button
            // 
            this.ThumbExport_button.Location = new System.Drawing.Point(9, 97);
            this.ThumbExport_button.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ThumbExport_button.Name = "ThumbExport_button";
            this.ThumbExport_button.Size = new System.Drawing.Size(111, 37);
            this.ThumbExport_button.TabIndex = 3;
            this.ThumbExport_button.Text = "Export";
            this.ThumbExport_button.UseVisualStyleBackColor = true;
            this.ThumbExport_button.Click += new System.EventHandler(this.ThumbExport_button_Click);
            // 
            // ThumbSave_button
            // 
            this.ThumbSave_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ThumbSave_button.Location = new System.Drawing.Point(9, 235);
            this.ThumbSave_button.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ThumbSave_button.Name = "ThumbSave_button";
            this.ThumbSave_button.Size = new System.Drawing.Size(111, 41);
            this.ThumbSave_button.TabIndex = 4;
            this.ThumbSave_button.Text = "Save";
            this.ThumbSave_button.UseVisualStyleBackColor = true;
            this.ThumbSave_button.Click += new System.EventHandler(this.ThumbSave_button_Click);
            // 
            // ThumbRemove_button
            // 
            this.ThumbRemove_button.Location = new System.Drawing.Point(9, 138);
            this.ThumbRemove_button.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ThumbRemove_button.Name = "ThumbRemove_button";
            this.ThumbRemove_button.Size = new System.Drawing.Size(111, 37);
            this.ThumbRemove_button.TabIndex = 5;
            this.ThumbRemove_button.Text = "Remove";
            this.ThumbRemove_button.UseVisualStyleBackColor = true;
            this.ThumbRemove_button.Click += new System.EventHandler(this.ThumbRemove_button_Click);
            // 
            // ThumbCancel_button
            // 
            this.ThumbCancel_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ThumbCancel_button.Location = new System.Drawing.Point(144, 235);
            this.ThumbCancel_button.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ThumbCancel_button.Name = "ThumbCancel_button";
            this.ThumbCancel_button.Size = new System.Drawing.Size(111, 41);
            this.ThumbCancel_button.TabIndex = 6;
            this.ThumbCancel_button.Text = "Cancel";
            this.ThumbCancel_button.UseVisualStyleBackColor = true;
            this.ThumbCancel_button.Click += new System.EventHandler(this.ThumbCancel_button_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(72, 192);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 26);
            this.label2.TabIndex = 7;
            this.label2.Text = "Face presets: 100x100\r\nBody presets: 104x148";
            // 
            // ThumbnailManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(266, 286);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ThumbCancel_button);
            this.Controls.Add(this.ThumbRemove_button);
            this.Controls.Add(this.ThumbSave_button);
            this.Controls.Add(this.ThumbExport_button);
            this.Controls.Add(this.ThumbImport_button);
            this.Controls.Add(this.Thumb_pictureBox);
            this.Controls.Add(this.ThumbType_label);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "ThumbnailManager";
            this.Text = "ThumbnailManager";
            ((System.ComponentModel.ISupportInitialize)(this.Thumb_pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label ThumbType_label;
        private System.Windows.Forms.PictureBox Thumb_pictureBox;
        private System.Windows.Forms.Button ThumbImport_button;
        private System.Windows.Forms.Button ThumbExport_button;
        private System.Windows.Forms.Button ThumbSave_button;
        private System.Windows.Forms.Button ThumbRemove_button;
        private System.Windows.Forms.Button ThumbCancel_button;
        private System.Windows.Forms.Label label2;
    }
}