using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProteinDigestionSimulator
{
    public class Disclaimer : Form
    {
        public Disclaimer()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call
            InitializeControls();
        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private TextBox txtNotice;
        private Button cmdOK;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtNotice = new TextBox();
            cmdOK = new Button();
            cmdOK.Click += cmdOK_Click;
            SuspendLayout();
            //
            // txtNotice
            //
            txtNotice.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtNotice.Location = new Point(8, 16);
            txtNotice.Multiline = true;
            txtNotice.Name = "txtNotice";
            txtNotice.ReadOnly = true;
            txtNotice.Size = new Size(440, 176);
            txtNotice.TabIndex = 2;
            //
            // cmdOK
            //
            cmdOK.Location = new Point(168, 200);
            cmdOK.Name = "cmdOK";
            cmdOK.Size = new Size(104, 24);
            cmdOK.TabIndex = 7;
            cmdOK.Text = "&OK";
            //
            // frmDisclaimer
            //
            AutoScaleBaseSize = new Size(5, 13);
            ClientSize = new Size(456, 238);
            ControlBox = false;
            Controls.Add(cmdOK);
            Controls.Add(txtNotice);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Disclaimer";
            Text = "Normalized Elution Time (NET) Prediction Utility";
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();
        }

        // Ignore Spelling: Kangas, Petritis, cmd, chk, txt, frm

        private const int FORM_CLOSE_DELAY_SECONDS = 2;

        private System.Timers.Timer mCloseDelayTimer;
        private DateTime mTimerStartTime;

        public static string GetKangasPetritisDisclaimerText(bool addNewlines = true)
        {
            string newlineText;
            if (addNewlines)
            {
                newlineText = Environment.NewLine + Environment.NewLine;
            }
            else
            {
                newlineText = ": ";
            }

            return "NOTICE/DISCLAIMER" +
                   newlineText +
                   "The methods embodied in this software to derive the Kangas/Petritis retention time " +
                   "prediction values are covered by U.S. patent 7,136,759 and pending patent 2005-0267688A1.  " +
                   "The software is made available solely for non-commercial research purposes on an " +
                   "\"as is\" basis by Battelle Memorial Institute.  If rights to deploy and distribute  " +
                   "the code for commercial purposes are of interest, please contact proteomics@pnnl.gov";
        }

        public void InitializeControls()
        {
            txtNotice.Text = GetKangasPetritisDisclaimerText();
            txtNotice.SelectionStart = 0;
            //txtNotice.SelectionLength = 0;

            cmdOK.Text = FORM_CLOSE_DELAY_SECONDS.ToString();
            cmdOK.Enabled = false;

            mTimerStartTime = DateTime.UtcNow;

            mCloseDelayTimer = new System.Timers.Timer(250d);
            mCloseDelayTimer.Elapsed += CloseDelayTimer_Elapsed;
            mCloseDelayTimer.SynchronizingObject = this;

            mCloseDelayTimer.Start();
        }

        private void CloseDelayTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var secondsRemaining = (int)Math.Round(Math.Round(FORM_CLOSE_DELAY_SECONDS - DateTime.UtcNow.Subtract(mTimerStartTime).TotalSeconds, 0));
            if (secondsRemaining < 0)
            {
                secondsRemaining = 0;
            }

            if (secondsRemaining > 0)
            {
                cmdOK.Text = secondsRemaining.ToString();
                Application.DoEvents();
            }
            else
            {
                cmdOK.Text = "&OK";
                cmdOK.Enabled = true;
                mCloseDelayTimer.Enabled = false;
                Application.DoEvents();
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}