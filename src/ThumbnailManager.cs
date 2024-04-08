/* TS4 MorphMaker, a tool for creating custom content for The Sims 4,
   Copyright (C) 2014  C. Marinetti

   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with this program.  If not, see <http://www.gnu.org/licenses/>. 
   The author may be contacted at modthesims.info, username cmarNYC. */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using s4pi.Interfaces;
using s4pi.Package;
using s4pi.ImageResource;

namespace MorphTool
{
    public partial class ThumbnailManager : Form
    {
        string PNGfilter = "PNG image files (*.png)|*.png|All files (*.*)|*.*";
        ThumbnailResource thumb;
        public ThumbnailResource Thumb
        {
            get { return thumb; }
        }

        public ThumbnailManager(ThumbnailResource thumbnail, bool isCustom)
        {
            InitializeComponent();
            thumb = thumbnail;
            if (thumb != null)
            {
                SetPictureBox(thumb.Image);
                ThumbType_label.Text = "Current Thumbnail: " + (isCustom ? "Custom" : "EA");
            }
            else
            {
                Thumb_pictureBox.Image = new Bitmap(10, 10);
                ThumbType_label.Text = "Current Thumbnail: None";
            }
        }

        private void SetPictureBox(Image image)
        {
            if (image.Height > 100)
            {
                this.Height += image.Height - 100;
            }
            Thumb_pictureBox.Size = new Size(image.Width, image.Height);
            Thumb_pictureBox.Image = thumb.Image;
        }

        private void ThumbImport_button_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.Filter = PNGfilter;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (FileStream myStream = new FileStream(openFileDialog1.FileName, FileMode.Open, FileAccess.Read))
                {
                    thumb = new ThumbnailResource(1, myStream);
                    SetPictureBox(thumb.Image);
                    ThumbType_label.Text = "Current Thumbnail: Custom";
                }
            }
        }

        private void ThumbExport_button_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = PNGfilter;
            saveFileDialog1.Title = "Save Thumbnail Image File";
            saveFileDialog1.DefaultExt = "png";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.CheckFileExists = false;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.AddExtension = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (FileStream myStream = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write))
                {
                    thumb.Image.Save(myStream, System.Drawing.Imaging.ImageFormat.Png);
                    myStream.Close();
                }
            }
        }

        private void ThumbRemove_button_Click(object sender, EventArgs e)
        {
            thumb = null;
            Thumb_pictureBox.Image = new Bitmap(10, 10);
            ThumbType_label.Text = "Custom thumbnail removed:" + Environment.NewLine + "Thumbnail will revert to EA, if any";
        }

        private void ThumbSave_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void ThumbCancel_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
