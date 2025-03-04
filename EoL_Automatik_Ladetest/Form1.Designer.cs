namespace EoL_Automatik_Ladetest
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.cbPruffeld = new System.Windows.Forms.ComboBox();
            this.lblPruffeld = new System.Windows.Forms.Label();
            this.gbEinstellungen = new System.Windows.Forms.GroupBox();
            this.checkBoxNotaus = new System.Windows.Forms.CheckBox();
            this.checkBoxERK = new System.Windows.Forms.CheckBox();
            this.gbTests = new System.Windows.Forms.GroupBox();
            this.checkBoxIsoTestRechts = new System.Windows.Forms.CheckBox();
            this.checkBoxTestRechts = new System.Windows.Forms.CheckBox();
            this.checkBoxIsoTestLinks = new System.Windows.Forms.CheckBox();
            this.checkBoxTestLinks = new System.Windows.Forms.CheckBox();
            this.checkBoxTurkontaktTest = new System.Windows.Forms.CheckBox();
            this.checkBoxNotausTest = new System.Windows.Forms.CheckBox();
            this.btnStarten = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.pruffeld1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pruffeld2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pruffeld3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pruffeld4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.automatikToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wartungToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tBNachrichten = new System.Windows.Forms.TextBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.gbCDS = new System.Windows.Forms.GroupBox();
            this.lblCDSstatus = new System.Windows.Forms.Label();
            this.btnCDSReset = new System.Windows.Forms.Button();
            this.btnCDSTrennen = new System.Windows.Forms.Button();
            this.btnCDSVerbinden = new System.Windows.Forms.Button();
            this.lblStatusVerbindung = new System.Windows.Forms.Label();
            this.btnPDF = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.tbFA = new System.Windows.Forms.TextBox();
            this.lblFA = new System.Windows.Forms.Label();
            this.gbEinstellungen.SuspendLayout();
            this.gbTests.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.gbCDS.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbPruffeld
            // 
            this.cbPruffeld.FormattingEnabled = true;
            this.cbPruffeld.Items.AddRange(new object[] {
            "PF1",
            "PF2",
            "PF3",
            "PF4"});
            this.cbPruffeld.Location = new System.Drawing.Point(505, 76);
            this.cbPruffeld.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbPruffeld.Name = "cbPruffeld";
            this.cbPruffeld.Size = new System.Drawing.Size(121, 24);
            this.cbPruffeld.TabIndex = 0;
            // 
            // lblPruffeld
            // 
            this.lblPruffeld.AutoSize = true;
            this.lblPruffeld.Location = new System.Drawing.Point(392, 76);
            this.lblPruffeld.Name = "lblPruffeld";
            this.lblPruffeld.Size = new System.Drawing.Size(97, 16);
            this.lblPruffeld.TabIndex = 1;
            this.lblPruffeld.Text = "Prüffeld wählen";
            // 
            // gbEinstellungen
            // 
            this.gbEinstellungen.Controls.Add(this.checkBoxNotaus);
            this.gbEinstellungen.Controls.Add(this.checkBoxERK);
            this.gbEinstellungen.Location = new System.Drawing.Point(98, 76);
            this.gbEinstellungen.Margin = new System.Windows.Forms.Padding(4);
            this.gbEinstellungen.Name = "gbEinstellungen";
            this.gbEinstellungen.Padding = new System.Windows.Forms.Padding(4);
            this.gbEinstellungen.Size = new System.Drawing.Size(187, 89);
            this.gbEinstellungen.TabIndex = 2;
            this.gbEinstellungen.TabStop = false;
            this.gbEinstellungen.Text = "Einstellungen";
            // 
            // checkBoxNotaus
            // 
            this.checkBoxNotaus.AutoSize = true;
            this.checkBoxNotaus.Location = new System.Drawing.Point(8, 52);
            this.checkBoxNotaus.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxNotaus.Name = "checkBoxNotaus";
            this.checkBoxNotaus.Size = new System.Drawing.Size(72, 20);
            this.checkBoxNotaus.TabIndex = 1;
            this.checkBoxNotaus.Text = "Notaus";
            this.checkBoxNotaus.UseVisualStyleBackColor = true;
            this.checkBoxNotaus.CheckedChanged += new System.EventHandler(this.checkBoxNotaus_CheckedChanged);
            // 
            // checkBoxERK
            // 
            this.checkBoxERK.AutoSize = true;
            this.checkBoxERK.Location = new System.Drawing.Point(8, 23);
            this.checkBoxERK.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxERK.Name = "checkBoxERK";
            this.checkBoxERK.Size = new System.Drawing.Size(56, 20);
            this.checkBoxERK.TabIndex = 0;
            this.checkBoxERK.Text = "ERK";
            this.checkBoxERK.UseVisualStyleBackColor = true;
            this.checkBoxERK.CheckedChanged += new System.EventHandler(this.checkBoxERK_CheckedChanged);
            // 
            // gbTests
            // 
            this.gbTests.AutoSize = true;
            this.gbTests.Controls.Add(this.checkBoxIsoTestRechts);
            this.gbTests.Controls.Add(this.checkBoxTestRechts);
            this.gbTests.Controls.Add(this.checkBoxIsoTestLinks);
            this.gbTests.Controls.Add(this.checkBoxTestLinks);
            this.gbTests.Controls.Add(this.checkBoxTurkontaktTest);
            this.gbTests.Controls.Add(this.checkBoxNotausTest);
            this.gbTests.Location = new System.Drawing.Point(98, 187);
            this.gbTests.Margin = new System.Windows.Forms.Padding(4);
            this.gbTests.Name = "gbTests";
            this.gbTests.Padding = new System.Windows.Forms.Padding(4);
            this.gbTests.Size = new System.Drawing.Size(267, 231);
            this.gbTests.TabIndex = 3;
            this.gbTests.TabStop = false;
            this.gbTests.Text = "Tests";
            // 
            // checkBoxIsoTestRechts
            // 
            this.checkBoxIsoTestRechts.AutoSize = true;
            this.checkBoxIsoTestRechts.Location = new System.Drawing.Point(25, 180);
            this.checkBoxIsoTestRechts.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxIsoTestRechts.Name = "checkBoxIsoTestRechts";
            this.checkBoxIsoTestRechts.Size = new System.Drawing.Size(159, 20);
            this.checkBoxIsoTestRechts.TabIndex = 5;
            this.checkBoxIsoTestRechts.Text = "DC2 Isolationsprüfung";
            this.checkBoxIsoTestRechts.UseVisualStyleBackColor = true;
            // 
            // checkBoxTestRechts
            // 
            this.checkBoxTestRechts.AutoSize = true;
            this.checkBoxTestRechts.Location = new System.Drawing.Point(25, 151);
            this.checkBoxTestRechts.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxTestRechts.Name = "checkBoxTestRechts";
            this.checkBoxTestRechts.Size = new System.Drawing.Size(149, 20);
            this.checkBoxTestRechts.TabIndex = 4;
            this.checkBoxTestRechts.Text = "DC2 3 Mal 5 Minuten";
            this.checkBoxTestRechts.UseVisualStyleBackColor = true;
            // 
            // checkBoxIsoTestLinks
            // 
            this.checkBoxIsoTestLinks.AutoSize = true;
            this.checkBoxIsoTestLinks.Location = new System.Drawing.Point(25, 123);
            this.checkBoxIsoTestLinks.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxIsoTestLinks.Name = "checkBoxIsoTestLinks";
            this.checkBoxIsoTestLinks.Size = new System.Drawing.Size(159, 20);
            this.checkBoxIsoTestLinks.TabIndex = 3;
            this.checkBoxIsoTestLinks.Text = "DC1 Isolationsprüfung";
            this.checkBoxIsoTestLinks.UseVisualStyleBackColor = true;
            // 
            // checkBoxTestLinks
            // 
            this.checkBoxTestLinks.AutoSize = true;
            this.checkBoxTestLinks.Location = new System.Drawing.Point(25, 95);
            this.checkBoxTestLinks.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxTestLinks.Name = "checkBoxTestLinks";
            this.checkBoxTestLinks.Size = new System.Drawing.Size(207, 20);
            this.checkBoxTestLinks.TabIndex = 2;
            this.checkBoxTestLinks.Text = "DC1 Ladetest: 3 Mal 5 Minuten";
            this.checkBoxTestLinks.UseVisualStyleBackColor = true;
            // 
            // checkBoxTurkontaktTest
            // 
            this.checkBoxTurkontaktTest.AutoSize = true;
            this.checkBoxTurkontaktTest.Location = new System.Drawing.Point(25, 66);
            this.checkBoxTurkontaktTest.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxTurkontaktTest.Name = "checkBoxTurkontaktTest";
            this.checkBoxTurkontaktTest.Size = new System.Drawing.Size(92, 20);
            this.checkBoxTurkontaktTest.TabIndex = 1;
            this.checkBoxTurkontaktTest.Text = "Türkontakt";
            this.checkBoxTurkontaktTest.UseVisualStyleBackColor = true;
            // 
            // checkBoxNotausTest
            // 
            this.checkBoxNotausTest.AutoSize = true;
            this.checkBoxNotausTest.Location = new System.Drawing.Point(25, 38);
            this.checkBoxNotausTest.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxNotausTest.Name = "checkBoxNotausTest";
            this.checkBoxNotausTest.Size = new System.Drawing.Size(93, 20);
            this.checkBoxNotausTest.TabIndex = 0;
            this.checkBoxNotausTest.Text = "Notaustest";
            this.checkBoxNotausTest.UseVisualStyleBackColor = true;
            // 
            // btnStarten
            // 
            this.btnStarten.Location = new System.Drawing.Point(98, 437);
            this.btnStarten.Margin = new System.Windows.Forms.Padding(4);
            this.btnStarten.Name = "btnStarten";
            this.btnStarten.Size = new System.Drawing.Size(100, 28);
            this.btnStarten.TabIndex = 4;
            this.btnStarten.Text = "STARTEN";
            this.btnStarten.UseVisualStyleBackColor = true;
            this.btnStarten.Click += new System.EventHandler(this.btnStarten_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.modeToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(826, 28);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pruffeld1ToolStripMenuItem,
            this.pruffeld2ToolStripMenuItem,
            this.pruffeld3ToolStripMenuItem,
            this.pruffeld4ToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(160, 24);
            this.toolStripMenuItem1.Text = "Prüffelder IP Adresse";
            // 
            // pruffeld1ToolStripMenuItem
            // 
            this.pruffeld1ToolStripMenuItem.Name = "pruffeld1ToolStripMenuItem";
            this.pruffeld1ToolStripMenuItem.Size = new System.Drawing.Size(156, 26);
            this.pruffeld1ToolStripMenuItem.Text = "Prüffeld 1";
            this.pruffeld1ToolStripMenuItem.Click += new System.EventHandler(this.pruffeld1ToolStripMenuItem_Click);
            // 
            // pruffeld2ToolStripMenuItem
            // 
            this.pruffeld2ToolStripMenuItem.Name = "pruffeld2ToolStripMenuItem";
            this.pruffeld2ToolStripMenuItem.Size = new System.Drawing.Size(156, 26);
            this.pruffeld2ToolStripMenuItem.Text = "Prüffeld 2";
            this.pruffeld2ToolStripMenuItem.Click += new System.EventHandler(this.pruffeld2ToolStripMenuItem_Click);
            // 
            // pruffeld3ToolStripMenuItem
            // 
            this.pruffeld3ToolStripMenuItem.Name = "pruffeld3ToolStripMenuItem";
            this.pruffeld3ToolStripMenuItem.Size = new System.Drawing.Size(156, 26);
            this.pruffeld3ToolStripMenuItem.Text = "Prüffeld 3";
            this.pruffeld3ToolStripMenuItem.Click += new System.EventHandler(this.pruffeld3ToolStripMenuItem_Click);
            // 
            // pruffeld4ToolStripMenuItem
            // 
            this.pruffeld4ToolStripMenuItem.Name = "pruffeld4ToolStripMenuItem";
            this.pruffeld4ToolStripMenuItem.Size = new System.Drawing.Size(156, 26);
            this.pruffeld4ToolStripMenuItem.Text = "Prüffeld 4";
            this.pruffeld4ToolStripMenuItem.Click += new System.EventHandler(this.pruffeld4ToolStripMenuItem_Click);
            // 
            // modeToolStripMenuItem
            // 
            this.modeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.automatikToolStripMenuItem,
            this.wartungToolStripMenuItem});
            this.modeToolStripMenuItem.Name = "modeToolStripMenuItem";
            this.modeToolStripMenuItem.Size = new System.Drawing.Size(138, 24);
            this.modeToolStripMenuItem.Text = "Mode: Automatik";
            // 
            // automatikToolStripMenuItem
            // 
            this.automatikToolStripMenuItem.Name = "automatikToolStripMenuItem";
            this.automatikToolStripMenuItem.Size = new System.Drawing.Size(161, 26);
            this.automatikToolStripMenuItem.Text = "Automatik";
            this.automatikToolStripMenuItem.Click += new System.EventHandler(this.automatikToolStripMenuItem_Click);
            // 
            // wartungToolStripMenuItem
            // 
            this.wartungToolStripMenuItem.Name = "wartungToolStripMenuItem";
            this.wartungToolStripMenuItem.Size = new System.Drawing.Size(161, 26);
            this.wartungToolStripMenuItem.Text = "Wartung";
            this.wartungToolStripMenuItem.Click += new System.EventHandler(this.wartungToolStripMenuItem_Click);
            // 
            // tBNachrichten
            // 
            this.tBNachrichten.Location = new System.Drawing.Point(98, 482);
            this.tBNachrichten.Multiline = true;
            this.tBNachrichten.Name = "tBNachrichten";
            this.tBNachrichten.ReadOnly = true;
            this.tBNachrichten.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tBNachrichten.Size = new System.Drawing.Size(444, 253);
            this.tBNachrichten.TabIndex = 7;
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(436, 437);
            this.btnStop.Margin = new System.Windows.Forms.Padding(4);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(100, 28);
            this.btnStop.TabIndex = 8;
            this.btnStop.Text = "STOPEN";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // gbCDS
            // 
            this.gbCDS.Controls.Add(this.lblCDSstatus);
            this.gbCDS.Controls.Add(this.btnCDSReset);
            this.gbCDS.Controls.Add(this.btnCDSTrennen);
            this.gbCDS.Controls.Add(this.btnCDSVerbinden);
            this.gbCDS.Controls.Add(this.lblStatusVerbindung);
            this.gbCDS.Location = new System.Drawing.Point(426, 115);
            this.gbCDS.Name = "gbCDS";
            this.gbCDS.Size = new System.Drawing.Size(200, 158);
            this.gbCDS.TabIndex = 9;
            this.gbCDS.TabStop = false;
            this.gbCDS.Text = "CDS";
            // 
            // lblCDSstatus
            // 
            this.lblCDSstatus.AutoSize = true;
            this.lblCDSstatus.Location = new System.Drawing.Point(7, 121);
            this.lblCDSstatus.Name = "lblCDSstatus";
            this.lblCDSstatus.Size = new System.Drawing.Size(75, 16);
            this.lblCDSstatus.TabIndex = 13;
            this.lblCDSstatus.Text = "CDS Status";
            // 
            // btnCDSReset
            // 
            this.btnCDSReset.Location = new System.Drawing.Point(10, 95);
            this.btnCDSReset.Name = "btnCDSReset";
            this.btnCDSReset.Size = new System.Drawing.Size(100, 23);
            this.btnCDSReset.TabIndex = 12;
            this.btnCDSReset.Text = "Reset";
            this.btnCDSReset.UseVisualStyleBackColor = true;
            this.btnCDSReset.Click += new System.EventHandler(this.btnCDSReset_Click);
            // 
            // btnCDSTrennen
            // 
            this.btnCDSTrennen.Location = new System.Drawing.Point(10, 66);
            this.btnCDSTrennen.Name = "btnCDSTrennen";
            this.btnCDSTrennen.Size = new System.Drawing.Size(100, 23);
            this.btnCDSTrennen.TabIndex = 11;
            this.btnCDSTrennen.Text = "Trennen";
            this.btnCDSTrennen.UseVisualStyleBackColor = true;
            this.btnCDSTrennen.Click += new System.EventHandler(this.btnCDSTrennen_Click);
            // 
            // btnCDSVerbinden
            // 
            this.btnCDSVerbinden.Location = new System.Drawing.Point(10, 37);
            this.btnCDSVerbinden.Name = "btnCDSVerbinden";
            this.btnCDSVerbinden.Size = new System.Drawing.Size(100, 23);
            this.btnCDSVerbinden.TabIndex = 10;
            this.btnCDSVerbinden.Text = "Verbinden";
            this.btnCDSVerbinden.UseVisualStyleBackColor = true;
            this.btnCDSVerbinden.Click += new System.EventHandler(this.btnCDSVerbinden_Click);
            // 
            // lblStatusVerbindung
            // 
            this.lblStatusVerbindung.AutoSize = true;
            this.lblStatusVerbindung.Location = new System.Drawing.Point(7, 18);
            this.lblStatusVerbindung.Name = "lblStatusVerbindung";
            this.lblStatusVerbindung.Size = new System.Drawing.Size(118, 16);
            this.lblStatusVerbindung.TabIndex = 0;
            this.lblStatusVerbindung.Text = "Verbindungsstatus";
            // 
            // btnPDF
            // 
            this.btnPDF.Location = new System.Drawing.Point(98, 765);
            this.btnPDF.Name = "btnPDF";
            this.btnPDF.Size = new System.Drawing.Size(114, 23);
            this.btnPDF.TabIndex = 10;
            this.btnPDF.Text = "PDF Erstellen";
            this.btnPDF.UseVisualStyleBackColor = true;
            this.btnPDF.Click += new System.EventHandler(this.btnPDF_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(426, 765);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(114, 23);
            this.button1.TabIndex = 11;
            this.button1.Text = "Generar Datos";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tbFA
            // 
            this.tbFA.Location = new System.Drawing.Point(230, 34);
            this.tbFA.Name = "tbFA";
            this.tbFA.Size = new System.Drawing.Size(112, 22);
            this.tbFA.TabIndex = 12;
            // 
            // lblFA
            // 
            this.lblFA.AutoSize = true;
            this.lblFA.Location = new System.Drawing.Point(95, 37);
            this.lblFA.Name = "lblFA";
            this.lblFA.Size = new System.Drawing.Size(87, 16);
            this.lblFA.TabIndex = 13;
            this.lblFA.Text = "Serienummer";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(826, 835);
            this.Controls.Add(this.lblFA);
            this.Controls.Add(this.tbFA);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnPDF);
            this.Controls.Add(this.gbCDS);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.tBNachrichten);
            this.Controls.Add(this.btnStarten);
            this.Controls.Add(this.gbTests);
            this.Controls.Add(this.gbEinstellungen);
            this.Controls.Add(this.lblPruffeld);
            this.Controls.Add(this.cbPruffeld);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Form1";
            this.Text = "EoL Ladetest durchführen";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.gbEinstellungen.ResumeLayout(false);
            this.gbEinstellungen.PerformLayout();
            this.gbTests.ResumeLayout(false);
            this.gbTests.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.gbCDS.ResumeLayout(false);
            this.gbCDS.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbPruffeld;
        private System.Windows.Forms.Label lblPruffeld;
        private System.Windows.Forms.GroupBox gbEinstellungen;
        private System.Windows.Forms.CheckBox checkBoxNotaus;
        private System.Windows.Forms.CheckBox checkBoxERK;
        private System.Windows.Forms.GroupBox gbTests;
        private System.Windows.Forms.CheckBox checkBoxIsoTestRechts;
        private System.Windows.Forms.CheckBox checkBoxTestRechts;
        private System.Windows.Forms.CheckBox checkBoxIsoTestLinks;
        private System.Windows.Forms.CheckBox checkBoxTestLinks;
        private System.Windows.Forms.CheckBox checkBoxTurkontaktTest;
        private System.Windows.Forms.CheckBox checkBoxNotausTest;
        private System.Windows.Forms.Button btnStarten;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem pruffeld1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pruffeld2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pruffeld3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pruffeld4ToolStripMenuItem;
        private System.Windows.Forms.TextBox tBNachrichten;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.GroupBox gbCDS;
        private System.Windows.Forms.Button btnCDSReset;
        private System.Windows.Forms.Button btnCDSTrennen;
        private System.Windows.Forms.Button btnCDSVerbinden;
        private System.Windows.Forms.Label lblStatusVerbindung;
        private System.Windows.Forms.Label lblCDSstatus;
        private System.Windows.Forms.Button btnPDF;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox tbFA;
        private System.Windows.Forms.Label lblFA;
        private System.Windows.Forms.ToolStripMenuItem modeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem automatikToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wartungToolStripMenuItem;
    }
}

