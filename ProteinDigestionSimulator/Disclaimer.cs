using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace ProteinDigestionSimulator
{
    public class Disclaimer : Form
    {
        #region  Windows Form Designer generated code 

        public Disclaimer() : base()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call
            InitializeControls();
        }

        // Form overrides dispose to clean up the component list.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        // Required by the Windows Form Designer
        private System.ComponentModel.IContainer components;

        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.
        // Do not modify it using the code editor.
        internal TextBox txtNotice;
        private Button _cmdOK;

        internal Button cmdOK
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdOK;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdOK != null)
                {
                    _cmdOK.Click -= cmdOK_Click;
                }

                _cmdOK = value;
                if (_cmdOK != null)
                {
                    _cmdOK.Click += cmdOK_Click;
                }
            }
        }

        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            txtNotice = new TextBox();
            _cmdOK = new Button();
            _cmdOK.Click += new EventHandler(cmdOK_Click);
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
            _cmdOK.Location = new Point(168, 200);
            _cmdOK.Name = "_cmdOK";
            _cmdOK.Size = new Size(104, 24);
            _cmdOK.TabIndex = 7;
            _cmdOK.Text = "&OK";
            // 
            // frmDisclaimer
            // 
            AutoScaleBaseSize = new Size(5, 13);
            ClientSize = new Size(456, 238);
            ControlBox = false;
            Controls.Add(_cmdOK);
            Controls.Add(txtNotice);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmDisclaimer";
            Text = "Normalized Elution Time (NET) Prediction Utility";
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        // Ignore Spelling: Kangas, Petritis, cmd, chk, txt, frm

        private const int FORM_CLOSE_DELAY_SECONDS = 2;
        private System.Timers.Timer _mCloseDelayTimer;

        protected System.Timers.Timer mCloseDelayTimer
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mCloseDelayTimer;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mCloseDelayTimer != null)
                {
                    _mCloseDelayTimer.Elapsed -= mCloseDelayTimer_Elapsed;
                }

                _mCloseDelayTimer = value;
                if (_mCloseDelayTimer != null)
                {
                    _mCloseDelayTimer.Elapsed += mCloseDelayTimer_Elapsed;
                }
            }
        }

        protected DateTime mTimerStartTime;

        public static string GetKangasPetritisDisclaimerText(bool addNewlines = true)
        {
            string newlineText;
            if (addNewlines)
            {
                newlineText = ControlChars.NewLine + ControlChars.NewLine;
            }
            else
            {
                newlineText = ": ";
            }

            return "NOTICE/DISCLAIMER" + newlineText + "The methods embodied in this software to derive the Kangas/Petritis retention time " + "prediction values are covered by U.S. patent 7,136,759 and pending patent 2005-0267688A1.  " + "The software is made available solely for non-commercial research purposes on an " + "\"as is\" basis by Battelle Memorial Institute.  If rights to deploy and distribute  " + "the code for commercial purposes are of interest, please contact proteomics@pnnl.gov";
        }

        public void InitializeControls()
        {
            txtNotice.Text = GetKangasPetritisDisclaimerText();
            txtNotice.SelectionStart = 0;
            // txtNotice.SelectionLength = 0

            cmdOK.Text = FORM_CLOSE_DELAY_SECONDS.ToString();
            cmdOK.Enabled = false;
            mTimerStartTime = DateTime.UtcNow;
            mCloseDelayTimer = new System.Timers.Timer(250d);
            mCloseDelayTimer.SynchronizingObject = this;
            mCloseDelayTimer.Start();
        }

        private void mCloseDelayTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            int secondsRemaining;
            secondsRemaining = (int)Math.Round(Math.Round(FORM_CLOSE_DELAY_SECONDS - DateTime.UtcNow.Subtract(mTimerStartTime).TotalSeconds, 0));
            if (secondsRemaining < 0)
                secondsRemaining = 0;
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