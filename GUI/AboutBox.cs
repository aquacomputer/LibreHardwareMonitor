﻿// Mozilla Public License 2.0
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Copyright (C) LibreHardwareMonitor and Contributors
// All Rights Reserved

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace OpenHardwareMonitor.GUI
{
    public partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
            this.Font = SystemFonts.MessageBoxFont;
            this.label3.Text = "Version " + System.Windows.Forms.Application.ProductVersion;
            projectLinkLabel.Links.Remove(projectLinkLabel.Links[0]);
            projectLinkLabel.Links.Add(0, projectLinkLabel.Text.Length, "https://github.com/LibreHardwareMonitor/LibreHardwareMonitor");
            licenseLinkLabel.Links.Remove(licenseLinkLabel.Links[0]);
            licenseLinkLabel.Links.Add(0, licenseLinkLabel.Text.Length, "https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/blob/master/LICENSE");
        }

        private void LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Link.LinkData.ToString()));
            }
            catch { }
        }
    }
}
