﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using System.Windows.Forms;
using System.Configuration;
using Configuration = System.Configuration.Configuration;
using Microsoft.VisualBasic;
using CdsTestCaseLibrary;
using Timer = System.Timers.Timer;
using EoL_Automatik_Ladetest.Properties;
using CdsTestCaseLibrary.Enums;
using CdsTestCaseLibrary.Models;
using Microsoft.Office.Interop.Word;
using System.IO;
using Application = Microsoft.Office.Interop.Word.Application;
using System.Threading;
using CdsTestCaseLibrary.Models.Project;
//using Application = Microsoft.Office.Interop.Word.Application;

namespace EoL_Automatik_Ladetest
{
    public partial class Form1 : Form
    {
        private IPAddress ipAdresse;
        private string testPath;
        private TestCaseHandler _testCaseHandler;
        private Test[] tests = new Test[6];
        //private List<Test> tests = new List<Test>();
        private ChargerTest Charger;
        private Timer TempWeiter = new Timer(2000);
        //private Timer TempWeiter2 = new Timer(2000);
        private List<Timer> activeTimers = new List<Timer>();
        private int prozess = 0;
        private bool erk = false;
        private SourceSink senke;
        private string _project;
        private string testCaseResult;
        private string pruefFeld;
        private int CDSverloren = 0;
        private string serie_number_charger;
        private string serie_number_CDS;
        private string norm;
        private bool mode;
        private bool DC1fullTest = true;
        private bool DC2fullTest = true;

        public Form1()
        {
            InitializeComponent();

            serie_number_CDS = "CDS";
            serie_number_charger = "17000xxxx";
            norm = "DINxxxx";
            mode = true;
            
            // -- Struct TESTS --
            tests[0] = new Test(Resources.notAusTest, false);
            tests[1] = new Test(Resources.tuerKontaktTest, true);
            tests[2] = new Test(Resources.DC1LadeTest, true);
            tests[3] = new Test(Resources.DC1IsoTest, true);
            tests[4] = new Test(Resources.DC2LadeTest, true);
            tests[5] = new Test(Resources.DC2IsoTest, true);


            // -- TestCase Handler --
            ipAdresse = new IPAddress(new byte[] { 192, 168, 30, 30 });
            testPath = @"D:\ChargingDiscover\Parametrization\SoP_Prueba";
            //testPath = ConfigurationManager.AppSettings[path];

            _testCaseHandler = new TestCaseHandler(testPath);
            _testCaseHandler.ConnectionStateChangedEvent += UpdateConnectionsStatus;
            _testCaseHandler.CdsStatusUpdatedEvent += UpdateCdsStatus;
            _testCaseHandler.CdsTestCaseResultReceived += _testCaseHandler_CdsTestCaseResultReceived;

            lblStatusVerbindung.Text = _testCaseHandler.Connection().ToString();
            
            // -- Timers --
            TempWeiter.Elapsed += OnTimedEventWeiter;
            activeTimers.Add(TempWeiter);

        }

        private void UpdateCdsStatus(CdsStatus status)
        {
            if (Disposing || IsDisposed)
                return;
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(() =>
                { UpdateCdsStatus(status); }));
                return;
            }
            if (lblCDSstatus.Text == "active" && status.ToString() != "active")
            {
                if (prozess > 0 && prozess <= 6)
                {
                    if (pruefFeld == "PF1" || pruefFeld == "PF4") _testCaseHandler.SendCdsTestCaseResultRequest(0);
                    if (pruefFeld == "PF2" || pruefFeld == "PF3") _testCaseHandler.SendCdsTestCaseResultRequest(1);
                    if (prozess > 2) { TempWeiter.Start(); Console.WriteLine("00 ACTIVE EL TEMP"); }
                }
            }
            lblCDSstatus.Text = status.ToString();
        }

        private void _testCaseHandler_CdsTestCaseResultReceived(object sender, EventArgs e)
        {
            if (Disposing || IsDisposed)
                return;
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(() =>
                _testCaseHandler_CdsTestCaseResultReceived(sender, e)));
                return;
            }
            testCaseResult = _testCaseHandler.GetTestCaseResult().ToString();
            lblResult.Text = testCaseResult;
        }

        private void UpdateConnectionsStatus(CdsTestCaseLibrary.Enums.ConnectionState state)
        {
            if (Disposing || IsDisposed)
                return;
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(() => { UpdateConnectionsStatus(state); }));
                return;
            }
            lblStatusVerbindung.Text = state.ToString();
            
            //ConnectionState = state;
            if (state == CdsTestCaseLibrary.Enums.ConnectionState.Connected)
            {
                _testCaseHandler.SendCdsSourceSinkRequest();
                _testCaseHandler.SendCdsInfoRequest();
            }
            
        }

        private void OnTimedEventWeiter(object sender, ElapsedEventArgs e)
        {
            Timer timer = sender as Timer;
            if (timer != null)
            {
                timer.Stop();
                TempWeiter.Interval = 2000;
                foreach (var t in activeTimers)
                {
                    t.Stop();
                }
                //activeTimers.Clear();
                if (mode)
                {
                    EoL_AutomatikTest();
                }
                else
                {
                    EoL_LadeTest();
                    Console.WriteLine("00 LLAME A LA Fn EOL");
                }
                
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cbPruffeld.Text = "PF1";
            checkBoxERK.Checked = false;
            checkBoxNotaus.Checked = false;

            checkBoxNotausTest.Checked = false;
            checkBoxNotausTest.Enabled = false;

            checkBoxTurkontaktTest.Checked = true;
            checkBoxTestLinks.Checked = true;
            checkBoxIsoTestLinks.Checked = true;
            checkBoxTestRechts.Checked = true;
            checkBoxIsoTestRechts.Checked = true;
        }

        private void pruffeld1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string aktuellIP = getIPAdresse("ipPF1");
            string neueIP = Interaction.InputBox("IP Prüffeld 1:", "Ändern IP Adresse", aktuellIP);
            setIPAdresse("ipPF1",neueIP);
            //cbPruffeld_SelectedIndexChanged(sender,e);
        }

        private void pruffeld2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string aktuellIP = getIPAdresse("ipPF2");
            string neueIP = Interaction.InputBox("IP Prüffeld 2:", "Ändern IP Adresse", aktuellIP);
            setIPAdresse("ipPF2", neueIP);
            //cbPruffeld_SelectedIndexChanged(sender, e);
        }

        private void pruffeld3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string aktuellIP = getIPAdresse("ipPF3");
            string neueIP = Interaction.InputBox("IP Prüffeld 3:", "Ändern IP Adresse", aktuellIP);
            setIPAdresse("ipPF3", neueIP);
            //cbPruffeld_SelectedIndexChanged(sender, e);
        }

        private void pruffeld4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string aktuellIP = getIPAdresse("ipPF4");
            string neueIP = Interaction.InputBox("IP Prüffeld 4:", "Ändern IP Adresse", aktuellIP);
            setIPAdresse("ipPF4", neueIP);
            //cbPruffeld_SelectedIndexChanged(sender, e);
        }

        private void checkBoxERK_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxERK.Checked == true)
            {
                checkBoxTestLinks.Text = Resources.DC1LadeTest + Resources._3m1m;
                checkBoxTestRechts.Text = Resources.DC2LadeTest + Resources._3m1m;
                checkBoxNotaus.Enabled = false;
                checkBoxNotaus.Checked = false;
                checkBoxNotausTest.Enabled = false;
                checkBoxNotausTest.Checked = false;
            }
            else
            {
                checkBoxTestLinks.Text = Resources.DC1LadeTest + Resources._3m5m;
                checkBoxTestRechts.Text = Resources.DC2LadeTest + Resources._3m5m;
                checkBoxNotaus.Enabled = true;
                checkBoxNotausTest.Enabled = true;
            }
        }

        private void checkBoxNotaus_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxNotaus.Checked == true)
            {
                checkBoxNotausTest.Enabled = true;
                checkBoxNotausTest.Checked = true;
            }
            else
            {
                checkBoxNotausTest.Enabled = false;
                checkBoxNotausTest.Checked = false;
            }
        }

        
        //Speichern IP Adresse den Prüffelder
        public void setIPAdresse(string pf, string ip)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[pf].Value = ip;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public string getIPAdresse(string pf)
        {
            return ConfigurationManager.AppSettings[pf];
        }

        private void cbPruffeld_SelectedIndexChanged(object sender, EventArgs e)
        {
            //string pf = "ip" + cbPruffeld.Text;
            //string ip = getIPAdresse(pf);
            //ipAdresse = IPAddress.Parse(ip);
        }

        public struct Test
        {
            public string name { get; set; }
            public bool testErfordelich { get; set; }
            public bool testBestanden { get; set; }
            public int testGearbeitet { get; set; }
            public string spannung { get; set; }
            public string strom { get; set; }
            public string zeit { get; set; }

            public Test(string testname, bool erfordelich)
            {
                name = testname;
                testErfordelich = erfordelich;
                testBestanden = false;
                testGearbeitet = 0;
                spannung = null;
                strom = null;
                zeit = null;
            }
        }

        public class ChargerTest
        {
            public string FA {  get; set; }
            public string CDS_SerialNumber { get; set; }
            public string CDS_FwVersion { get; set; }
            public string Sink { get; set; }
            public string MaxDCPower { get; set; }
            public string Norm { get; set; }

            public Test[] tests { get; set; }

            public ChargerTest(string serienNummer, Test[] test)
            {
                FA = serienNummer;
                tests = test;
                CDS_SerialNumber = "--";
                CDS_FwVersion = "--";
                Sink = "--";
                MaxDCPower = "--";
                Norm = "--";
            }

        }

        private void TexteHinzufuegen(string neueText)
        {
            if (tBNachrichten.InvokeRequired)
            {
                tBNachrichten.Invoke(new Action<string>(TexteHinzufuegen), neueText);
            }
            else
            {
                tBNachrichten.AppendText(neueText + Environment.NewLine);
                tBNachrichten.ScrollToCaret();
            }
        }

        private void EoL_AutomatikTest()
        {
            if (lblStatusVerbindung.Text == "Connected")
            {
                DialogResult antworte;
                switch (prozess)
                {
                    //Start
                    case 0:
                        TexteHinzufuegen("");
                        TexteHinzufuegen(Resources.m_starten);

                        TexteHinzufuegen(Resources.m_cdsConnected);

                        prozess++;
                        Charger.CDS_SerialNumber = _testCaseHandler.GetCdsInfo().SerialNumber;
                        Charger.CDS_FwVersion = _testCaseHandler.GetCdsInfo().FwVersion;
                        List<CdsTestCaseLibrary.Models.SourceSink> AvailableSinks = _testCaseHandler.GetSinks();
                        if (AvailableSinks.Count > 0)
                        {
                            for (var i = 0; i < AvailableSinks.Count; i++)
                            {
                                senke = AvailableSinks[i];

                            }
                        }
                        Charger.Sink = senke.ParamValues[0].Value;
                        TexteHinzufuegen("CDS S/N: " + Charger.CDS_SerialNumber);
                        TexteHinzufuegen("CDS Fw Version: " + Charger.CDS_FwVersion);
                        TexteHinzufuegen("Senke: " + Charger.Sink);
                        TempWeiter.Start();
                        Console.WriteLine("02 ACTIVE EL TEMP");

                        break;
                    //Notaus Test
                    case 1:
                        if (Charger.tests[0].testErfordelich)
                        {
                            if (Charger.tests[0].testGearbeitet < 10)
                            {
                                switch (Charger.tests[0].testGearbeitet)
                                {
                                    case 0:
                                        //empezar test
                                        antworte = MessageBox.Show(Resources.m_f_LadePistgesteckt, Resources.m_bestaetigt, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                                        if (antworte == DialogResult.OK)
                                        {
                                            Console.WriteLine("Intentar Iniciar Test");
                                        
                                        if (testStarten(pruefFeld + "tna.cdpj", Charger.tests[0].name))
                                        {
                                            Charger.tests[0].testGearbeitet++;
                                            TempWeiter.Start();
                                        }
                                        else
                                        {
                                            endProgram();
                                            Console.WriteLine("02 LLAME A PARAR EL PROGRAMA");
                                        }
                                        }
                                        else
                                        {
                                            TexteHinzufuegen(Resources.m_testStopt);
                                            Console.WriteLine("00 LLAME A PARAR EL PROGRAMA");
                                            endProgram();
                                        }
                                        
                                        /*
                                        List<CdsTestCaseLibrary.Models.SourceSink> AvailableSinks = _testCaseHandler.GetSinks();
                                        if (AvailableSinks.Count > 0)
                                        {
                                            for (var i = 0; i < AvailableSinks.Count; i++)
                                            {
                                                senke = AvailableSinks[i];
                                            }
                                        }
                                        if (lblCDSstatus.Text == "inactive")
                                        {
                                            TexteHinzufuegen(Resources.notAusTest + " " + Resources.m_starten);
                                            Console.WriteLine("Iniciando Test: " + pruefFeld + "tna.cdpj, con la fuente:" + senke.Name.ToString());
                                            Console.WriteLine("MANDE A  INICIAR TEST");
                                            _testCaseHandler.StartTest(pruefFeld + "tna.cdpj", null, senke, CdsTestCaseLibrary.Enums.ControlMode.Test, "SICHARGE_D_350_kW_Prototype.evse");
                                            //TempWeiter.Interval = 5000;
                                            tests[0].testGearbeitet++;
                                        }
                                        else if (lblCDSstatus.Text == "error")
                                        {
                                            _testCaseHandler.ResetErrors();
                                            _testCaseHandler.SendCdsSourceSinkRequest();
                                            TempWeiter.Start();
                                            Console.WriteLine("03 ACTIVE EL TEMP");
                                        }
                                        else
                                        {
                                            TempWeiter.Start();
                                            Console.WriteLine("04 ACTIVE EL TEMP");
                                        }

                                        */
                                        
                                        break;
                                    case 1:
                                        //si activo --> activar temporizador
                                        Console.WriteLine("Intentar Activar temporizador de Notaus test");
                                        if (lblCDSstatus.Text == "active")
                                        {
                                            Console.WriteLine("Se activo temporizador de Notaus test");
                                            TempWeiter.Interval = 60000;
                                            TempWeiter.Start();
                                            Console.WriteLine("05 ACTIVE EL TEMP");
                                            Charger.tests[0].testGearbeitet++;
                                        }
                                        else
                                        {
                                            Console.WriteLine("No se puede activar temporizador de Notaus test, porque el CDS aún no se ha iniciado");
                                            TempWeiter.Start();
                                            Console.WriteLine("06 ACTIVE EL TEMP");
                                        }
                                        break;
                                    case 2:
                                        //si activo y temporizador se desbordo --> solicitar presionar el boton
                                        Console.WriteLine("El temporizador se desbordo");
                                        if (lblCDSstatus.Text == "active")
                                        {
                                            //antworte = 0;
                                            antworte = MessageBox.Show(Resources.m_notausDruecken + "\n" + Resources.m_f_errorFlagsLesen, Resources.notAusTest, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                            if (antworte == DialogResult.Yes)
                                            {
                                                Charger.tests[0].testGearbeitet++;
                                                _testCaseHandler.StopTest();
                                                Charger.tests[0].testGearbeitet = 10;
                                                TempWeiter.Interval = 3000;
                                                TempWeiter.Start();
                                                Console.WriteLine("07 ACTIVE EL TEMP");
                                            }
                                            else
                                            {
                                                endProgram();
                                                Console.WriteLine("02 LLAME A PARAR EL PROGRAMA");
                                            }
                                        }
                                        break;
                                        /*
                                    case 3:
                                        if (lblCDSstatus.Text != "active") //unknown
                                        {
                                            Charger.tests[0].testGearbeitet = 10;
                                            //prozess++;
                                            TempWeiter.Start();
                                            Console.WriteLine("08 ACTIVE EL TEMP");
                                        }
                                        else
                                        {
                                            TempWeiter.Interval = 10000;
                                            TempWeiter.Start();
                                            Console.WriteLine("09 ACTIVE EL TEMP");
                                            Console.WriteLine("CDS aun activa");
                                            _testCaseHandler.StopTest();
                                        }
                                        break;
                                        */
                                }
                            }
                            else
                            {
                                antworte = MessageBox.Show(Resources.m_NotausNormailizieren + "\n" + Resources.m_f_chargerGruen, Resources.notAusTest, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                if (antworte == DialogResult.Yes)
                                {
                                    TexteHinzufuegen(Resources.notAusTest + " " + Resources.m_bestanden);
                                    Charger.tests[0].testBestanden = true;

                                    TexteHinzufuegen(Resources.notAusTest + " " + Resources.m_endet);

                                    prozess++;

                                    TempWeiter.Start();
                                    Console.WriteLine("10 ACTIVE EL TEMP");
                                }
                                else
                                {
                                    endProgram();
                                    Console.WriteLine("03 LLAME A PARAR EL PROGRAMA");
                                }
                            }
                        }
                        else
                        {
                            prozess++;
                            TempWeiter.Start();
                            Console.WriteLine("11 ACTIVE EL TEMP");
                        }
                        break;
                    
                    //Türkontakt Test
                    case 2:
                        if (Charger.tests[1].testErfordelich)
                        {
                            if (Charger.tests[1].testGearbeitet < 10)
                            {
                                switch (Charger.tests[1].testGearbeitet)
                                {
                                    case 0:
                                        //empezar test
                                        Console.WriteLine("Intentar Iniciar Test");

                                        if (testStarten(pruefFeld + "tna.cdpj", Charger.tests[1].name))
                                        {
                                            Charger.tests[1].testGearbeitet++;
                                            TempWeiter.Start();
                                        }
                                        break;
                                    case 1:
                                        //si activo --> activar temporizador
                                        Console.WriteLine("Intentar Activar temporizador de Notaus test");
                                        if (lblCDSstatus.Text == "active")
                                        {
                                            Console.WriteLine("Se activo temporizador de Notaus test");
                                            TempWeiter.Interval = 60000;
                                            TempWeiter.Start();
                                            Console.WriteLine("14 ACTIVE EL TEMP");
                                            Charger.tests[1].testGearbeitet++;
                                        }
                                        else
                                        {
                                            Console.WriteLine("No se puede activar temporizador de Notaus test, porque el CDS aún no se ha iniciado");
                                            TempWeiter.Start();
                                            Console.WriteLine("15 ACTIVE EL TEMP");
                                        }
                                        break;
                                    case 2:
                                        //si activo y temporizador se desbordo --> solicitar presionar el boton
                                        Console.WriteLine("El temporizador se desbordo");
                                        if (lblCDSstatus.Text == "active")
                                        {
                                            antworte = MessageBox.Show(Resources.m_tuerOeffnen + "\n" + Resources.m_f_errorFlagsLesen, Resources.tuerKontaktTest, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                            if (antworte == DialogResult.Yes)
                                            {
                                                Charger.tests[1].testGearbeitet++;
                                                _testCaseHandler.StopTest();
                                                Charger.tests[1].testGearbeitet = 10;
                                                TempWeiter.Interval = 3000;
                                                TempWeiter.Start();
                                                Console.WriteLine("16 ACTIVE EL TEMP");
                                            }
                                            else
                                            {
                                                endProgram();
                                                Console.WriteLine("04 LLAME A PARAR EL PROGRAMA");
                                            }
                                        }
                                        break;
                                        /*
                                    case 3:
                                        if (lblCDSstatus.Text != "active") //unknown
                                        {
                                            //si resultado es passed --> testBestanden = true
                                            Console.WriteLine("El resultado es: " + testCaseResult);

                                            tests[1].testGearbeitet = 10;
                                            TempWeiter.Start();
                                            Console.WriteLine("17 ACTIVE EL TEMP");
                                        }
                                        else
                                        {
                                            TempWeiter.Interval = 10000;
                                            TempWeiter.Start();
                                            Console.WriteLine("18 ACTIVE EL TEMP");
                                            Console.WriteLine("CDS aun activa");
                                            _testCaseHandler.StopTest();
                                        }
                                        break;
                                        */
                                }
                            }
                            else
                            {
                                antworte = MessageBox.Show(Resources.m_tuerNormalisieren + "\n" + Resources.m_f_chargerGruen, Resources.tuerKontaktTest, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                if (antworte == DialogResult.Yes)
                                {
                                    TexteHinzufuegen(Resources.tuerKontaktTest + " " + Resources.m_bestanden);
                                    Charger.tests[1].testBestanden = true;

                                    TexteHinzufuegen(Resources.tuerKontaktTest + " " + Resources.m_endet);

                                    prozess++;

                                    TempWeiter.Start();
                                    Console.WriteLine("19 ACTIVE EL TEMP");
                                }
                                else
                                {
                                    endProgram();
                                    Console.WriteLine("05 LLAME A PARAR EL PROGRAMA");
                                }
                            }
                        }
                        else
                        {
                            prozess++;
                            TempWeiter.Start();
                            Console.WriteLine("20 ACTIVE EL TEMP");
                        }
                        break;
                    
                    //DC1 Ladetest
                    case 3:
                        //DC1 Ladetest
                        if (DC1fullTest)
                        {
                            //DC1 Test complett
                            if (Charger.tests[2].testGearbeitet == 0)
                            {
                                //Iniciar Test
                                TexteHinzufuegen(Resources.DC1LadeTest + " " + Resources.m_starten);
                                antworte = MessageBox.Show(Resources.DC1 + ": " + Resources.m_f_LadePistgesteckt, Resources.m_bestaetigt, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                                if (antworte == DialogResult.OK)
                                {
                                    string projectName = pruefFeld;
                                    if (pruefFeld == "PF2" || pruefFeld == "PF3") projectName = projectName + "Left";
                                    if (erk) projectName = projectName + "Test1m.cdpj";
                                    else projectName = projectName + "Test3m.cdpj";
                                    if (testStarten(projectName, Resources.DC1LadeTest))
                                    {
                                        Charger.tests[2].testGearbeitet = 1;

                                    }
                                    else
                                    {
                                        endProgram();
                                    }
                                }
                                else
                                {
                                    TexteHinzufuegen(Resources.m_testStopt);
                                    endProgram();
                                    Console.WriteLine("06 LLAME A PARAR EL PROGRAMA");
                                }
                            }
                            else if (Charger.tests[2].testGearbeitet == 1)
                            {
                                Charger.tests[2].testBestanden = true;
                                int testCase = 0;
                                if (pruefFeld == "PF2" || pruefFeld == "PF3") testCase++;
                                for (int i = testCase; i <= 3 + testCase; i++)
                                {
                                    _testCaseHandler.SendCdsTestCaseResultRequest(i);
                                    if (_testCaseHandler.GetTestCaseResult().ToString() != "passed") Charger.tests[2].testBestanden = false;
                                }
                                if (Charger.tests[2].testBestanden) TexteHinzufuegen(Charger.tests[2].name + " " + Resources.m_bestanden);
                                else TexteHinzufuegen(Charger.tests[2].name + " " + Resources.m_bestandenNicht);
                                prozess = 5;
                                TempWeiter.Start();
                            }
                        }
                        else
                        {
                            if (Charger.tests[2].testErfordelich)
                            {
                                if (Charger.tests[2].testGearbeitet < 10)
                                {
                                    if (tests[2].testGearbeitet == 0)
                                    {
                                        TexteHinzufuegen(Resources.DC1LadeTest + " " + Resources.m_starten);
                                        antworte = MessageBox.Show(Resources.DC1 + ": " + Resources.m_f_LadePistgesteckt, Resources.m_bestaetigt, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                                        if (antworte == DialogResult.OK)
                                        {
                                            tests[2].testGearbeitet++;
                                            //prozess++;
                                            TempWeiter.Start();
                                            Console.WriteLine("21 ACTIVE EL TEMP");
                                            tests[2].testBestanden = true;
                                        }
                                        else
                                        {
                                            TexteHinzufuegen(Resources.m_testStopt);
                                            endProgram();
                                            Console.WriteLine("06 LLAME A PARAR EL PROGRAMA");
                                        }
                                    }

                                    if (tests[2].testGearbeitet > 0 && tests[2].testGearbeitet <= 3)
                                    {
                                        //List<CdsTestCaseLibrary.Models.SourceSink> AvailableSinks = _testCaseHandler.GetSinks();
                                        //if (AvailableSinks.Count > 0)
                                        //{
                                            //for (var i = 0; i < AvailableSinks.Count; i++)
                                            //{
                                                //senke = AvailableSinks[i];

                                            //}
                                        //}
                                        if (lblCDSstatus.Text == "inactive")
                                        {
                                            TexteHinzufuegen(Resources.DC1LadeTest + " " + tests[2].testGearbeitet.ToString() + " " + Resources.m_starten);
                                            Console.WriteLine(Resources.DC1LadeTest + " " + tests[2].testGearbeitet.ToString() + " " + Resources.m_starten);
                                            if (erk) _testCaseHandler.StartTest(pruefFeld + "lt1m.cdpj", null, senke, CdsTestCaseLibrary.Enums.ControlMode.Test, "SICHARGE_D_350_kW_Prototype.evse");
                                            else _testCaseHandler.StartTest(pruefFeld + "lt3m.cdpj", null, senke, CdsTestCaseLibrary.Enums.ControlMode.Test, "SICHARGE_D_350_kW_Prototype.evse");
                                            tests[2].testGearbeitet++;
                                        }
                                        else if (lblCDSstatus.Text == "error")
                                        {
                                            _testCaseHandler.ResetErrors();
                                            _testCaseHandler.SendCdsSourceSinkRequest();
                                            TempWeiter.Start();
                                            Console.WriteLine("22 ACTIVE EL TEMP");
                                        }
                                        else
                                        {
                                            TempWeiter.Start();
                                            Console.WriteLine("23 ACTIVE EL TEMP");
                                        }

                                        if (tests[2].testGearbeitet > 1)
                                        {
                                            if (tests[2].testBestanden)
                                            {
                                                if (testCaseResult == "passed") tests[2].testBestanden = true;
                                                else tests[2].testBestanden = false;
                                            }
                                        }

                                        //tests[2].testGearbeitet++;
                                    }
                                    else
                                    {
                                        tests[2].testGearbeitet = 10;
                                        TempWeiter.Start();
                                    }
                                }
                                else
                                {
                                    if (tests[2].testBestanden)
                                    {
                                        if (testCaseResult == "passed")
                                        {
                                            tests[2].testBestanden = true;
                                            TexteHinzufuegen(Resources.DC1LadeTest + " " + Resources.m_bestanden);

                                        }
                                        else
                                        {
                                            tests[2].testBestanden = false;
                                            TexteHinzufuegen(Resources.DC1LadeTest + " " + Resources.m_bestandenNicht);

                                        }
                                    }


                                    //TEST
                                    //prozess = 7;
                                    prozess++;



                                    TempWeiter.Start();
                                }
                            }
                            else
                            {
                                prozess++;
                                TempWeiter.Start();
                            }
                        }
                            break;
                    
                    //DC1 Isolationsprüfung
                    case 4:
                        //DC1 Isotest
                        if (tests[3].testErfordelich)
                        {
                            if (tests[3].testGearbeitet < 10)
                            {
                                if (tests[3].testGearbeitet == 0)
                                {
                                    TexteHinzufuegen(Resources.DC1IsoTest + " " + Resources.m_starten);
                                    if (!tests[2].testErfordelich)
                                    {
                                        antworte = MessageBox.Show(Resources.DC1 + ": " + Resources.m_f_LadePistgesteckt, Resources.m_bestaetigt, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                                        if (antworte == DialogResult.OK)
                                        {
                                            tests[3].testGearbeitet++;
                                            //prozess++;
                                            TempWeiter.Start();
                                            Console.WriteLine("24 ACTIVE EL TEMP");
                                            tests[3].testBestanden = true;
                                        }
                                        else
                                        {
                                            TexteHinzufuegen(Resources.m_testStopt);
                                            endProgram();
                                            Console.WriteLine("07 LLAME A PARAR EL PROGRAMA");
                                        }
                                    }
                                    else
                                    {
                                        tests[3].testGearbeitet++;
                                        //prozess++;
                                        TempWeiter.Start();
                                        Console.WriteLine("25 ACTIVE EL TEMP");
                                        tests[3].testBestanden = true;
                                    }
                                }
                                else if (tests[3].testGearbeitet == 1)
                                {
                                    /*
                                    List<CdsTestCaseLibrary.Models.SourceSink> AvailableSinks = _testCaseHandler.GetSinks();
                                    if (AvailableSinks.Count > 0)
                                    {
                                        for (var i = 0; i < AvailableSinks.Count; i++)
                                        {
                                            senke = AvailableSinks[i];

                                        }
                                    }
                                    */
                                    if (lblCDSstatus.Text == "inactive")
                                    {
                                        TexteHinzufuegen(Resources.DC1IsoTest + " " + tests[3].testGearbeitet.ToString() + " " + Resources.m_starten);
                                        Console.WriteLine(Resources.DC1IsoTest + " " + tests[3].testGearbeitet.ToString() + " " + Resources.m_starten);
                                        _testCaseHandler.StartTest(pruefFeld + "it.cdpj", null, senke, CdsTestCaseLibrary.Enums.ControlMode.Test, "SICHARGE_D_350_kW_Prototype.evse");
                                        tests[3].testGearbeitet++;
                                    }
                                    else if (lblCDSstatus.Text == "error")
                                    {
                                        _testCaseHandler.ResetErrors();
                                        _testCaseHandler.SendCdsSourceSinkRequest();
                                        TempWeiter.Start();
                                        Console.WriteLine("26 ACTIVE EL TEMP");
                                    }
                                    else
                                    {
                                        TempWeiter.Start();
                                        Console.WriteLine("27 ACTIVE EL TEMP");
                                    }
                                }
                                else
                                {
                                    tests[3].testGearbeitet = 10;
                                    TempWeiter.Start();
                                }
                            }
                            else
                            {
                                if (testCaseResult == "passed")
                                {
                                    tests[3].testBestanden = true;
                                    TexteHinzufuegen(Resources.DC1IsoTest + " " + Resources.m_bestanden);
                                }
                                else
                                {
                                    tests[3].testBestanden = false;
                                    TexteHinzufuegen(Resources.DC1IsoTest + " " + Resources.m_bestandenNicht);
                                }

                                //TEST
                                //prozess = 7;
                                prozess++;



                                TempWeiter.Start();
                            }
                        }
                        else
                        {
                            prozess++;
                            TempWeiter.Start();
                        }
                        break;
                    
                    //DC2 Ladetest
                    case 5:
                        //DC2 Ladetest
                        if (DC2fullTest)
                        {
                            //DC1 Test complett
                            if (Charger.tests[4].testGearbeitet == 0)
                            {
                                //Iniciar Test
                                TexteHinzufuegen(Resources.DC2LadeTest + " " + Resources.m_starten);
                                if (Charger.tests[2].testGearbeitet == 2 && (pruefFeld == "PF2" || pruefFeld == "PF3"))
                                {
                                    Charger.tests[4].testGearbeitet = 1;
                                    TempWeiter.Start();
                                }
                                else
                                {
                                    antworte = MessageBox.Show(Resources.DC2 + ": " + Resources.m_f_LadePistgesteckt, Resources.m_bestaetigt, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                                    if (antworte == DialogResult.OK)
                                    {
                                        Charger.tests[4].testGearbeitet = 1;
                                        TempWeiter.Start();
                                    }
                                    else
                                    {
                                        TexteHinzufuegen(Resources.m_testStopt);
                                        endProgram();
                                        Console.WriteLine("06 LLAME A PARAR EL PROGRAMA");
                                    }
                                }
                            }
                            else if (Charger.tests[4].testGearbeitet == 1)
                            {
                                TexteHinzufuegen(Resources.DC2LadeTest + " " + Resources.m_starten);
                                string projectName = pruefFeld;
                                if (pruefFeld == "PF2" || pruefFeld == "PF3") projectName = projectName + "Right";
                                if (erk) projectName = projectName + "Test1m.cdpj";
                                else projectName = projectName + "Test3m.cdpj";
                                if (testStarten(projectName, Resources.DC1LadeTest))
                                {
                                    Charger.tests[4].testGearbeitet = 2;
                                }
                                else
                                {
                                    endProgram();
                                }
                            }
                            else if (Charger.tests[4].testGearbeitet == 2)
                            {
                                Charger.tests[4].testBestanden = true;
                                int testCase = 0;
                                if (pruefFeld == "PF2" || pruefFeld == "PF3") testCase++;
                                for (int i = testCase; i <= 3 + testCase; i++)
                                {
                                    _testCaseHandler.SendCdsTestCaseResultRequest(i);
                                    if (_testCaseHandler.GetTestCaseResult().ToString() != "passed") Charger.tests[4].testBestanden = false;
                                }
                                if (Charger.tests[4].testBestanden) TexteHinzufuegen(Charger.tests[4].name + " " + Resources.m_bestanden);
                                else TexteHinzufuegen(Charger.tests[4].name + " " + Resources.m_bestandenNicht);
                                prozess = 7;
                                TempWeiter.Start();
                            }
                        }
                        else
                        {
                            if (tests[4].testErfordelich)
                            {
                                if (tests[4].testGearbeitet < 10)
                                {
                                    if (tests[4].testGearbeitet == 0)
                                    {
                                        TexteHinzufuegen(Resources.DC2LadeTest + " " + Resources.m_starten);
                                        antworte = MessageBox.Show(Resources.DC2 + ": " + Resources.m_f_LadePistgesteckt, Resources.m_bestaetigt, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                                        if (antworte == DialogResult.OK)
                                        {
                                            tests[4].testGearbeitet++;
                                            //prozess++;
                                            TempWeiter.Start();
                                            Console.WriteLine("28 ACTIVE EL TEMP");
                                            tests[4].testBestanden = true;
                                        }
                                        else
                                        {
                                            TexteHinzufuegen(Resources.m_testStopt);
                                            endProgram();
                                            Console.WriteLine("06 LLAME A PARAR EL PROGRAMA");
                                        }
                                    }

                                    if (tests[4].testGearbeitet > 0 && tests[4].testGearbeitet <= 3)
                                    {
                                        /*
                                        List<CdsTestCaseLibrary.Models.SourceSink> AvailableSinks = _testCaseHandler.GetSinks();
                                        if (AvailableSinks.Count > 0)
                                        {
                                            for (var i = 0; i < AvailableSinks.Count; i++)
                                            {
                                                senke = AvailableSinks[i];

                                            }
                                        }
                                        */
                                        if (lblCDSstatus.Text == "inactive")
                                        {
                                            TexteHinzufuegen(Resources.DC2LadeTest + " " + tests[4].testGearbeitet.ToString() + " " + Resources.m_starten);
                                            Console.WriteLine(Resources.DC2LadeTest + " " + tests[4].testGearbeitet.ToString() + " " + Resources.m_starten);
                                            if (erk) _testCaseHandler.StartTest(pruefFeld + "lt1m.cdpj", null, senke, CdsTestCaseLibrary.Enums.ControlMode.Test, "SICHARGE_D_350_kW_Prototype.evse");
                                            else _testCaseHandler.StartTest(pruefFeld + "lt3m.cdpj", null, senke, CdsTestCaseLibrary.Enums.ControlMode.Test, "SICHARGE_D_350_kW_Prototype.evse");
                                            tests[4].testGearbeitet++;
                                        }
                                        else if (lblCDSstatus.Text == "error")
                                        {
                                            _testCaseHandler.ResetErrors();
                                            _testCaseHandler.SendCdsSourceSinkRequest();
                                            TempWeiter.Start();
                                            Console.WriteLine("29 ACTIVE EL TEMP");
                                        }
                                        else
                                        {
                                            TempWeiter.Start();
                                            Console.WriteLine("30 ACTIVE EL TEMP");
                                        }

                                        if (tests[4].testGearbeitet > 1)
                                        {
                                            if (tests[4].testBestanden)
                                            {
                                                if (testCaseResult == "passed") tests[4].testBestanden = true;
                                                else tests[4].testBestanden = false;
                                            }
                                        }

                                        //tests[4].testGearbeitet++;
                                    }
                                    else
                                    {
                                        tests[4].testGearbeitet = 10;
                                        TempWeiter.Start();
                                    }
                                }
                                else
                                {
                                    if (tests[4].testBestanden)
                                    {
                                        if (testCaseResult == "passed")
                                        {
                                            tests[4].testBestanden = true;
                                            TexteHinzufuegen(Resources.DC2LadeTest + " " + Resources.m_bestanden);
                                        }
                                        else
                                        {
                                            tests[4].testBestanden = false;
                                            TexteHinzufuegen(Resources.DC2LadeTest + " " + Resources.m_bestandenNicht);
                                        }
                                    }


                                    //TEST
                                    //prozess = 7;
                                    prozess++;



                                    TempWeiter.Start();
                                }
                            }
                            else
                            {
                                prozess++;
                                TempWeiter.Start();
                            }
                        }
                        break;
                    
                    //DC2 Isolationsprüfung
                    case 6:
                        //DC2 Isotest
                        if (tests[5].testErfordelich)
                        {
                            if (tests[5].testGearbeitet < 10)
                            {
                                if (tests[5].testGearbeitet == 0)
                                {
                                    TexteHinzufuegen(Resources.DC2IsoTest + " " + Resources.m_starten);
                                    if (!tests[4].testErfordelich)
                                    {
                                        antworte = MessageBox.Show(Resources.DC2 + ": " + Resources.m_f_LadePistgesteckt, Resources.m_bestaetigt, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                                        if (antworte == DialogResult.OK)
                                        {
                                            tests[5].testGearbeitet++;
                                            //prozess++;
                                            TempWeiter.Start();
                                            Console.WriteLine("31 ACTIVE EL TEMP");
                                            tests[5].testBestanden = true;
                                        }
                                        else
                                        {
                                            TexteHinzufuegen(Resources.m_testStopt);
                                            endProgram();
                                            Console.WriteLine("06 LLAME A PARAR EL PROGRAMA");
                                        }

                                    }
                                    else
                                    {
                                        tests[5].testGearbeitet++;
                                        //prozess++;
                                        TempWeiter.Start();
                                        Console.WriteLine("32 ACTIVE EL TEMP");
                                        tests[5].testBestanden = true;
                                    }

                                }
                                else if (tests[5].testGearbeitet == 1)
                                {
                                    /*
                                    List<CdsTestCaseLibrary.Models.SourceSink> AvailableSinks = _testCaseHandler.GetSinks();
                                    if (AvailableSinks.Count > 0)
                                    {
                                        for (var i = 0; i < AvailableSinks.Count; i++)
                                        {
                                            senke = AvailableSinks[i];

                                        }
                                    }
                                    */
                                    if (lblCDSstatus.Text == "inactive")
                                    {
                                        TexteHinzufuegen(Resources.DC2IsoTest + " " + tests[5].testGearbeitet.ToString() + " " + Resources.m_starten);
                                        Console.WriteLine(Resources.DC2IsoTest + " " + tests[5].testGearbeitet.ToString() + " " + Resources.m_starten);
                                        _testCaseHandler.StartTest(pruefFeld + "it.cdpj", null, senke, CdsTestCaseLibrary.Enums.ControlMode.Test, "SICHARGE_D_350_kW_Prototype.evse");
                                        tests[5].testGearbeitet++;
                                    }
                                    else if (lblCDSstatus.Text == "error")
                                    {
                                        _testCaseHandler.ResetErrors();
                                        _testCaseHandler.SendCdsSourceSinkRequest();
                                        TempWeiter.Start();
                                        Console.WriteLine("33 ACTIVE EL TEMP");
                                    }
                                    else
                                    {
                                        TempWeiter.Start();
                                        Console.WriteLine("34 ACTIVE EL TEMP");
                                    }
                                }
                                else
                                {
                                    tests[5].testGearbeitet = 10;
                                    TempWeiter.Start();
                                }
                            }
                            else
                            {
                                if (testCaseResult == "passed")
                                {
                                    tests[5].testBestanden = true;
                                    TexteHinzufuegen(Resources.DC2IsoTest + " " + Resources.m_bestanden);
                                }
                                else
                                {
                                    tests[5].testBestanden = false;
                                    TexteHinzufuegen(Resources.DC2IsoTest + " " + Resources.m_bestandenNicht);
                                }

                                //TEST
                                //prozess = 7;
                                prozess++;



                                TempWeiter.Start();
                            }
                        }
                        else
                        {
                            prozess++;
                            TempWeiter.Start();
                        }
                        break;
                    
                    //end program
                    case 7:
                        endProgram();
                        Console.WriteLine("07 LLAME A PARAR EL PROGRAMA");
                        break;
                    default:
                        break;

                }
            }
            else
            {
                TexteHinzufuegen(Resources.m_cdsNotConnected);
                if (CDSverloren > 3)
                {
                    endProgram();
                    Console.WriteLine("08 LLAME A PARAR EL PROGRAMA");
                }

                else
                {
                    CDSverloren++;
                    TempWeiter.Start();
                    Console.WriteLine("ULT ACTIVE EL TEMP");
                }
            }
        }
        private void EoL_LadeTest()
        {
            if(lblStatusVerbindung.Text == "Connected")
            {
                DialogResult antworte;
                switch (prozess)
                {
                    //Start
                    case 0:
                        TexteHinzufuegen("");
                        TexteHinzufuegen(Resources.m_starten);

                        if(lblStatusVerbindung.Text == "Connected")
                        {
                            TexteHinzufuegen(Resources.m_cdsConnected);
                            antworte = MessageBox.Show(Resources.m_f_LadePistgesteckt, Resources.m_bestaetigt, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                            if (antworte == DialogResult.OK)
                            {
                                prozess++;
                                TempWeiter.Start();
                                Console.WriteLine("02 ACTIVE EL TEMP");
                            }
                            else
                            {
                                TexteHinzufuegen(Resources.m_testStopt);
                                Console.WriteLine("00 LLAME A PARAR EL PROGRAMA");
                                endProgram();
                            }
                        }
                        else
                        {
                            TexteHinzufuegen(Resources.m_cdsNotConnected);
                            endProgram();
                            Console.WriteLine("01 LLAME A PARAR EL PROGRAMA");
                        }

                        break;
                    //Notaus Test
                    case 1:
                        if (tests[0].testErfordelich)
                        {
                            if (tests[0].testGearbeitet < 10)
                            {
                                switch (tests[0].testGearbeitet)
                                {
                                    case 0:
                                        //empezar test
                                        Console.WriteLine("Intentar Iniciar Test");
                                        List<CdsTestCaseLibrary.Models.SourceSink> AvailableSinks = _testCaseHandler.GetSinks();
                                        if (AvailableSinks.Count > 0)
                                        {
                                            for (var i = 0; i < AvailableSinks.Count; i++)
                                            {
                                                senke = AvailableSinks[i];
                                            }
                                        }
                                        if (lblCDSstatus.Text == "inactive")
                                        {
                                            TexteHinzufuegen(Resources.notAusTest + " " + Resources.m_starten);
                                            Console.WriteLine("Iniciando Test: " + pruefFeld + "tna.cdpj, con la fuente:" + senke.Name.ToString());
                                            Console.WriteLine("MANDE A  INICIAR TEST");
                                            _testCaseHandler.StartTest(pruefFeld + "tna.cdpj", null, senke, CdsTestCaseLibrary.Enums.ControlMode.Test, "SICHARGE_D_350_kW_Prototype.evse");
                                            //TempWeiter.Interval = 5000;
                                            tests[0].testGearbeitet++;
                                        }
                                        else if(lblCDSstatus.Text == "error")
                                        {
                                            _testCaseHandler.ResetErrors();
                                            _testCaseHandler.SendCdsSourceSinkRequest();
                                            TempWeiter.Start();
                                            Console.WriteLine("03 ACTIVE EL TEMP");
                                        }
                                        else
                                        {
                                            TempWeiter.Start();
                                            Console.WriteLine("04 ACTIVE EL TEMP");
                                        }
                                        
                                        //TempWeiter.Start();
                                        break;
                                    case 1:
                                        //si activo --> activar temporizador
                                        Console.WriteLine("Intentar Activar temporizador de Notaus test");
                                        if (lblCDSstatus.Text == "active")
                                        {
                                            Console.WriteLine("Se activo temporizador de Notaus test");
                                            TempWeiter.Interval = 60000;
                                            TempWeiter.Start();
                                            Console.WriteLine("05 ACTIVE EL TEMP");
                                            tests[0].testGearbeitet++;
                                        }
                                        else
                                        {
                                            Console.WriteLine("No se puede activar temporizador de Notaus test, porque el CDS aún no se ha iniciado");
                                            TempWeiter.Start();
                                            Console.WriteLine("06 ACTIVE EL TEMP");
                                        }
                                        break;
                                    case 2:
                                        //si activo y temporizador se desbordo --> solicitar presionar el boton
                                        Console.WriteLine("El temporizador se desbordo");
                                        if (lblCDSstatus.Text == "active")
                                        {
                                            //antworte = 0;
                                            antworte = MessageBox.Show(Resources.m_notausDruecken + "\n" + Resources.m_f_errorFlagsLesen, Resources.notAusTest, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                            if (antworte == DialogResult.Yes)
                                            {
                                                tests[0].testGearbeitet++;
                                                TempWeiter.Interval = 3000;
                                                TempWeiter.Start();
                                                Console.WriteLine("07 ACTIVE EL TEMP");
                                            }
                                            else
                                            {
                                                endProgram();
                                                Console.WriteLine("02 LLAME A PARAR EL PROGRAMA");
                                            }
                                        }
                                        break;
                                    case 3:
                                        if(lblCDSstatus.Text != "active") //unknown
                                        {
                                            //consultar resultado
                                            //Console.WriteLine("consultar resultado");
                                            //if (pruefFeld == "PF1" || pruefFeld == "PF4") _testCaseHandler.SendCdsTestCaseResultRequest(0);
                                            //else _testCaseHandler.SendCdsTestCaseResultRequest(1);

                                            //si resultado es passed --> testBestanden = true
                                            Console.WriteLine("El resultado es: " + testCaseResult);
                                            //if (testCaseResult == "Passed" || testCaseResult == "unknown")
                                            //{
                                                //tests[0].testBestanden = true;
                                            //}
                                            //else
                                            //{
                                                //tests[0].testBestanden = false;
                                            //}
                                            tests[0].testGearbeitet = 10;
                                            //prozess++;
                                            TempWeiter.Start();
                                            Console.WriteLine("08 ACTIVE EL TEMP");
                                        }
                                        else
                                        {
                                            TempWeiter.Interval = 10000;
                                            TempWeiter.Start();
                                            Console.WriteLine("09 ACTIVE EL TEMP");
                                            Console.WriteLine("CDS aun activa");
                                            _testCaseHandler.StopTest();
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                //if (tests[0].testBestanden)
                                //{
                                    //TexteHinzufuegen(Resources.notAusTest + " " + Resources.m_bestanden);
                                //}
                                //else
                                //{
                                    //TexteHinzufuegen(Resources.notAusTest + " " + Resources.m_bestandenNicht);
                                //}
                                

                                antworte = MessageBox.Show(Resources.m_NotausNormailizieren + "\n" + Resources.m_f_chargerGruen, Resources.notAusTest, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                if (antworte == DialogResult.Yes)
                                {
                                    TexteHinzufuegen(Resources.notAusTest + " " + Resources.m_bestanden);
                                    tests[0].testBestanden = true;

                                    TexteHinzufuegen(Resources.notAusTest + " " + Resources.m_endet);
                                    
                                    prozess++;

                                    TempWeiter.Start();
                                    Console.WriteLine("10 ACTIVE EL TEMP");
                                }
                                else
                                {
                                    endProgram();
                                    Console.WriteLine("03 LLAME A PARAR EL PROGRAMA");
                                }
                            }
                        }
                        else
                        {
                            prozess++;
                            TempWeiter.Start();
                            Console.WriteLine("11 ACTIVE EL TEMP");
                        }
                        break;
                    //Türkontakt Test
                    case 2:
                        if (tests[1].testErfordelich)
                        {
                            if (tests[1].testGearbeitet < 10)
                            {
                                switch (tests[1].testGearbeitet)
                                {
                                    case 0:
                                        //empezar test
                                        Console.WriteLine("Intentar Iniciar Test");
                                        List<CdsTestCaseLibrary.Models.SourceSink> AvailableSinks = _testCaseHandler.GetSinks();
                                        if (AvailableSinks.Count > 0)
                                        {
                                            for (var i = 0; i < AvailableSinks.Count; i++)
                                            {
                                                senke = AvailableSinks[i];

                                            }
                                        }
                                        if (lblCDSstatus.Text == "inactive")
                                        {
                                            TexteHinzufuegen(Resources.tuerKontaktTest + " " + Resources.m_starten);
                                            Console.WriteLine("Iniciando Test: " + pruefFeld + "tna.cdpj con la fuente:" + senke.Name.ToString());
                                            _testCaseHandler.StartTest(pruefFeld + "tna.cdpj", null, senke, CdsTestCaseLibrary.Enums.ControlMode.Test, "SICHARGE_D_350_kW_Prototype.evse");
                                            Console.WriteLine("MANDE A  INICIAR TEST");
                                            //TempWeiter.Interval = 5000;
                                            tests[1].testGearbeitet++;
                                        }
                                        else if (lblCDSstatus.Text == "error")
                                        {
                                            _testCaseHandler.ResetErrors();
                                            _testCaseHandler.SendCdsSourceSinkRequest();
                                            TempWeiter.Start();
                                            Console.WriteLine("12 ACTIVE EL TEMP");
                                        }
                                        else
                                        {
                                            TempWeiter.Start();
                                            Console.WriteLine("13 ACTIVE EL TEMP");
                                        }
                                        break;
                                    case 1:
                                        //si activo --> activar temporizador
                                        Console.WriteLine("Intentar Activar temporizador de Notaus test");
                                        if (lblCDSstatus.Text == "active")
                                        {
                                            Console.WriteLine("Se activo temporizador de Notaus test");
                                            TempWeiter.Interval = 60000;
                                            TempWeiter.Start();
                                            Console.WriteLine("14 ACTIVE EL TEMP");
                                            tests[1].testGearbeitet++;
                                        }
                                        else
                                        {
                                            Console.WriteLine("No se puede activar temporizador de Notaus test, porque el CDS aún no se ha iniciado");
                                            TempWeiter.Start();
                                            Console.WriteLine("15 ACTIVE EL TEMP");
                                        }
                                        break;
                                    case 2:
                                        //si activo y temporizador se desbordo --> solicitar presionar el boton
                                        Console.WriteLine("El temporizador se desbordo");
                                        if (lblCDSstatus.Text == "active")
                                        {
                                            antworte = MessageBox.Show(Resources.m_tuerOeffnen + "\n" + Resources.m_f_errorFlagsLesen, Resources.tuerKontaktTest, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                            if (antworte == DialogResult.Yes)
                                            {
                                                tests[1].testGearbeitet++;
                                                TempWeiter.Interval = 3000;
                                                TempWeiter.Start();
                                                Console.WriteLine("16 ACTIVE EL TEMP");
                                            }
                                            else
                                            {
                                                endProgram();
                                                Console.WriteLine("04 LLAME A PARAR EL PROGRAMA");
                                            }
                                        }
                                        break;
                                    case 3:
                                        if (lblCDSstatus.Text != "active") //unknown
                                        {
                                            //si resultado es passed --> testBestanden = true
                                            Console.WriteLine("El resultado es: " + testCaseResult);
                                            
                                            tests[1].testGearbeitet = 10;
                                            TempWeiter.Start();
                                            Console.WriteLine("17 ACTIVE EL TEMP");
                                        }
                                        else
                                        {
                                            TempWeiter.Interval = 10000;
                                            TempWeiter.Start();
                                            Console.WriteLine("18 ACTIVE EL TEMP");
                                            Console.WriteLine("CDS aun activa");
                                            _testCaseHandler.StopTest();
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                antworte = MessageBox.Show(Resources.m_tuerNormalisieren + "\n" + Resources.m_f_chargerGruen, Resources.tuerKontaktTest, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                if (antworte == DialogResult.Yes)
                                {
                                    TexteHinzufuegen(Resources.tuerKontaktTest + " " + Resources.m_bestanden);
                                    tests[1].testBestanden = true;

                                    TexteHinzufuegen(Resources.tuerKontaktTest + " " + Resources.m_endet);

                                    prozess++;

                                    TempWeiter.Start();
                                    Console.WriteLine("19 ACTIVE EL TEMP");
                                }
                                else
                                {
                                    endProgram();
                                    Console.WriteLine("05 LLAME A PARAR EL PROGRAMA");
                                }
                            }
                        }
                        else
                        {
                            prozess++;
                            TempWeiter.Start();
                            Console.WriteLine("20 ACTIVE EL TEMP");
                        }
                        break;
                    //DC1 Ladetest
                    case 3:
                        //DC1 Ladetest
                        if (tests[2].testErfordelich)
                        {
                            if (tests[2].testGearbeitet <10)
                            {
                                if(tests[2].testGearbeitet == 0)
                                {
                                    TexteHinzufuegen(Resources.DC1LadeTest + " " + Resources.m_starten);
                                    antworte = MessageBox.Show(Resources.DC1 + ": " + Resources.m_f_LadePistgesteckt, Resources.m_bestaetigt, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                                    if (antworte == DialogResult.OK)
                                    {
                                        tests[2].testGearbeitet++;
                                        //prozess++;
                                        TempWeiter.Start();
                                        Console.WriteLine("21 ACTIVE EL TEMP");
                                        tests[2].testBestanden = true;
                                    }
                                    else
                                    {
                                        TexteHinzufuegen(Resources.m_testStopt);
                                        endProgram();
                                        Console.WriteLine("06 LLAME A PARAR EL PROGRAMA");
                                    }
                                }
                                
                                if (tests[2].testGearbeitet >0 && tests[2].testGearbeitet <= 3)
                                {
                                    List<CdsTestCaseLibrary.Models.SourceSink> AvailableSinks = _testCaseHandler.GetSinks();
                                    if (AvailableSinks.Count > 0)
                                    {
                                        for (var i = 0; i < AvailableSinks.Count; i++)
                                        {
                                            senke = AvailableSinks[i];

                                        }
                                    }
                                    if (lblCDSstatus.Text == "inactive")
                                    {
                                        TexteHinzufuegen(Resources.DC1LadeTest + " " + tests[2].testGearbeitet.ToString() + " " + Resources.m_starten);
                                        Console.WriteLine(Resources.DC1LadeTest + " " + tests[2].testGearbeitet.ToString() + " " + Resources.m_starten);
                                        if (erk) _testCaseHandler.StartTest(pruefFeld + "lt1m.cdpj", null, senke, CdsTestCaseLibrary.Enums.ControlMode.Test, "SICHARGE_D_350_kW_Prototype.evse");
                                        else _testCaseHandler.StartTest(pruefFeld + "lt3m.cdpj", null, senke, CdsTestCaseLibrary.Enums.ControlMode.Test, "SICHARGE_D_350_kW_Prototype.evse");
                                        tests[2].testGearbeitet++;
                                    }
                                    else if (lblCDSstatus.Text == "error")
                                    {
                                        _testCaseHandler.ResetErrors();
                                        _testCaseHandler.SendCdsSourceSinkRequest();
                                        TempWeiter.Start();
                                        Console.WriteLine("22 ACTIVE EL TEMP");
                                    }
                                    else
                                    {
                                        TempWeiter.Start();
                                        Console.WriteLine("23 ACTIVE EL TEMP");
                                    }

                                    if (tests[2].testGearbeitet > 1)
                                    {
                                        if (tests[2].testBestanden)
                                        {
                                            if (testCaseResult == "passed") tests[2].testBestanden = true;
                                            else tests[2].testBestanden = false;
                                        }
                                    }
                                    
                                    //tests[2].testGearbeitet++;
                                }
                                else
                                {
                                    tests[2].testGearbeitet = 10;
                                    TempWeiter.Start();
                                }
                            }
                            else
                            {
                                if (tests[2].testBestanden)
                                {
                                    if (testCaseResult == "passed")
                                    {
                                        tests[2].testBestanden = true;
                                        TexteHinzufuegen(Resources.DC1LadeTest + " " + Resources.m_bestanden);
                                        
                                    }
                                    else
                                    {
                                        tests[2].testBestanden = false;
                                        TexteHinzufuegen(Resources.DC1LadeTest + " " + Resources.m_bestandenNicht);
                                        
                                    }
                                }
                                
                                
                                //TEST
                                //prozess = 7;
                                prozess++;
                                
                                
                                
                                TempWeiter.Start();
                            }
                        }
                        else
                        {
                            prozess++;
                            TempWeiter.Start();
                        }
                        break;
                    //DC1 Isolationsprüfung
                    case 4:
                        //DC1 Isotest
                        if (tests[3].testErfordelich)
                        {
                            if (tests[3].testGearbeitet < 10)
                            {
                                if (tests[3].testGearbeitet == 0)
                                {
                                    TexteHinzufuegen(Resources.DC1IsoTest + " " + Resources.m_starten);
                                    if (!tests[2].testErfordelich)
                                    {
                                        antworte = MessageBox.Show(Resources.DC1 + ": " + Resources.m_f_LadePistgesteckt, Resources.m_bestaetigt, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                                        if (antworte == DialogResult.OK)
                                        {
                                            tests[3].testGearbeitet++;
                                            //prozess++;
                                            TempWeiter.Start();
                                            Console.WriteLine("24 ACTIVE EL TEMP");
                                            tests[3].testBestanden = true;
                                        }
                                        else
                                        {
                                            TexteHinzufuegen(Resources.m_testStopt);
                                            endProgram();
                                            Console.WriteLine("07 LLAME A PARAR EL PROGRAMA");
                                        }
                                    }
                                    else
                                    {
                                        tests[3].testGearbeitet++;
                                        //prozess++;
                                        TempWeiter.Start();
                                        Console.WriteLine("25 ACTIVE EL TEMP");
                                        tests[3].testBestanden = true;
                                    }
                                }
                                else if (tests[3].testGearbeitet == 1)
                                {
                                    List<CdsTestCaseLibrary.Models.SourceSink> AvailableSinks = _testCaseHandler.GetSinks();
                                    if (AvailableSinks.Count > 0)
                                    {
                                        for (var i = 0; i < AvailableSinks.Count; i++)
                                        {
                                            senke = AvailableSinks[i];

                                        }
                                    }
                                    if (lblCDSstatus.Text == "inactive")
                                    {
                                        TexteHinzufuegen(Resources.DC1IsoTest + " " + tests[3].testGearbeitet.ToString() + " " + Resources.m_starten);
                                        Console.WriteLine(Resources.DC1IsoTest + " " + tests[3].testGearbeitet.ToString() + " " + Resources.m_starten);
                                        _testCaseHandler.StartTest(pruefFeld + "it.cdpj", null, senke, CdsTestCaseLibrary.Enums.ControlMode.Test, "SICHARGE_D_350_kW_Prototype.evse");
                                        tests[3].testGearbeitet++;
                                    }
                                    else if (lblCDSstatus.Text == "error")
                                    {
                                        _testCaseHandler.ResetErrors();
                                        _testCaseHandler.SendCdsSourceSinkRequest();
                                        TempWeiter.Start();
                                        Console.WriteLine("26 ACTIVE EL TEMP");
                                    }
                                    else
                                    {
                                        TempWeiter.Start();
                                        Console.WriteLine("27 ACTIVE EL TEMP");
                                    }
                                }
                                else
                                {
                                    tests[3].testGearbeitet = 10;
                                    TempWeiter.Start();
                                }
                            }
                            else
                            {
                                if (testCaseResult == "passed")
                                {
                                    tests[3].testBestanden = true;
                                    TexteHinzufuegen(Resources.DC1IsoTest + " " + Resources.m_bestanden);
                                }
                                else
                                {
                                    tests[3].testBestanden = false;
                                    TexteHinzufuegen(Resources.DC1IsoTest + " " + Resources.m_bestandenNicht);
                                }

                                //TEST
                                //prozess = 7;
                                prozess++;



                                TempWeiter.Start();
                            }
                        }
                        else
                        {
                            prozess++;
                            TempWeiter.Start();
                        }
                        break;
                    //DC2 Ladetest
                    case 5:
                        //DC2 Ladetest
                        if (tests[4].testErfordelich)
                        {
                            if (tests[4].testGearbeitet < 10)
                            {
                                if (tests[4].testGearbeitet == 0)
                                {
                                    TexteHinzufuegen(Resources.DC2LadeTest + " " + Resources.m_starten);
                                    antworte = MessageBox.Show(Resources.DC2 + ": " + Resources.m_f_LadePistgesteckt, Resources.m_bestaetigt, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                                    if (antworte == DialogResult.OK)
                                    {
                                        tests[4].testGearbeitet++;
                                        //prozess++;
                                        TempWeiter.Start();
                                        Console.WriteLine("28 ACTIVE EL TEMP");
                                        tests[4].testBestanden = true;
                                    }
                                    else
                                    {
                                        TexteHinzufuegen(Resources.m_testStopt);
                                        endProgram();
                                        Console.WriteLine("06 LLAME A PARAR EL PROGRAMA");
                                    }
                                }

                                if (tests[4].testGearbeitet > 0 && tests[4].testGearbeitet <= 3)
                                {
                                    List<CdsTestCaseLibrary.Models.SourceSink> AvailableSinks = _testCaseHandler.GetSinks();
                                    if (AvailableSinks.Count > 0)
                                    {
                                        for (var i = 0; i < AvailableSinks.Count; i++)
                                        {
                                            senke = AvailableSinks[i];

                                        }
                                    }
                                    if (lblCDSstatus.Text == "inactive")
                                    {
                                        TexteHinzufuegen(Resources.DC2LadeTest + " " + tests[4].testGearbeitet.ToString() + " " + Resources.m_starten);
                                        Console.WriteLine(Resources.DC2LadeTest + " " + tests[4].testGearbeitet.ToString() + " " + Resources.m_starten);
                                        if (erk) _testCaseHandler.StartTest(pruefFeld + "lt1m.cdpj", null, senke, CdsTestCaseLibrary.Enums.ControlMode.Test, "SICHARGE_D_350_kW_Prototype.evse");
                                        else _testCaseHandler.StartTest(pruefFeld + "lt3m.cdpj", null, senke, CdsTestCaseLibrary.Enums.ControlMode.Test, "SICHARGE_D_350_kW_Prototype.evse");
                                        tests[4].testGearbeitet++;
                                    }
                                    else if (lblCDSstatus.Text == "error")
                                    {
                                        _testCaseHandler.ResetErrors();
                                        _testCaseHandler.SendCdsSourceSinkRequest();
                                        TempWeiter.Start();
                                        Console.WriteLine("29 ACTIVE EL TEMP");
                                    }
                                    else
                                    {
                                        TempWeiter.Start();
                                        Console.WriteLine("30 ACTIVE EL TEMP");
                                    }

                                    if (tests[4].testGearbeitet > 1)
                                    {
                                        if (tests[4].testBestanden)
                                        {
                                            if (testCaseResult == "passed") tests[4].testBestanden = true;
                                            else tests[4].testBestanden = false;
                                        }
                                    }

                                    //tests[4].testGearbeitet++;
                                }
                                else
                                {
                                    tests[4].testGearbeitet = 10;
                                    TempWeiter.Start();
                                }
                            }
                            else
                            {
                                if (tests[4].testBestanden)
                                {
                                    if (testCaseResult == "passed")
                                    {
                                        tests[4].testBestanden = true;
                                        TexteHinzufuegen(Resources.DC2LadeTest + " " + Resources.m_bestanden);
                                    }
                                    else
                                    {
                                        tests[4].testBestanden = false;
                                        TexteHinzufuegen(Resources.DC2LadeTest + " " + Resources.m_bestandenNicht);
                                    }
                                }


                                //TEST
                                //prozess = 7;
                                prozess++;



                                TempWeiter.Start();
                            }
                        }
                        else
                        {
                            prozess++;
                            TempWeiter.Start();
                        }
                        break;
                    //DC2 Isolationsprüfung
                    case 6:
                        //DC2 Isotest
                        if (tests[5].testErfordelich)
                        {
                            if (tests[5].testGearbeitet < 10)
                            {
                                if (tests[5].testGearbeitet == 0)
                                {
                                    TexteHinzufuegen(Resources.DC2IsoTest + " " + Resources.m_starten);
                                    if (!tests[4].testErfordelich)
                                    {
                                        antworte = MessageBox.Show(Resources.DC2 + ": " + Resources.m_f_LadePistgesteckt, Resources.m_bestaetigt, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                                        if (antworte == DialogResult.OK)
                                        {
                                            tests[5].testGearbeitet++;
                                            //prozess++;
                                            TempWeiter.Start();
                                            Console.WriteLine("31 ACTIVE EL TEMP");
                                            tests[5].testBestanden = true;
                                        }
                                        else
                                        {
                                            TexteHinzufuegen(Resources.m_testStopt);
                                            endProgram();
                                            Console.WriteLine("06 LLAME A PARAR EL PROGRAMA");
                                        }

                                    }
                                    else
                                    {
                                        tests[5].testGearbeitet++;
                                        //prozess++;
                                        TempWeiter.Start();
                                        Console.WriteLine("32 ACTIVE EL TEMP");
                                        tests[5].testBestanden = true;
                                    }
                                    
                                }
                                else if (tests[5].testGearbeitet == 1)
                                {
                                    List<CdsTestCaseLibrary.Models.SourceSink> AvailableSinks = _testCaseHandler.GetSinks();
                                    if (AvailableSinks.Count > 0)
                                    {
                                        for (var i = 0; i < AvailableSinks.Count; i++)
                                        {
                                            senke = AvailableSinks[i];

                                        }
                                    }
                                    if (lblCDSstatus.Text == "inactive")
                                    {
                                        TexteHinzufuegen(Resources.DC2IsoTest + " " + tests[5].testGearbeitet.ToString() + " " + Resources.m_starten);
                                        Console.WriteLine(Resources.DC2IsoTest + " " + tests[5].testGearbeitet.ToString() + " " + Resources.m_starten);
                                        _testCaseHandler.StartTest(pruefFeld + "it.cdpj", null, senke, CdsTestCaseLibrary.Enums.ControlMode.Test, "SICHARGE_D_350_kW_Prototype.evse");
                                        tests[5].testGearbeitet++;
                                    }
                                    else if (lblCDSstatus.Text == "error")
                                    {
                                        _testCaseHandler.ResetErrors();
                                        _testCaseHandler.SendCdsSourceSinkRequest();
                                        TempWeiter.Start();
                                        Console.WriteLine("33 ACTIVE EL TEMP");
                                    }
                                    else
                                    {
                                        TempWeiter.Start();
                                        Console.WriteLine("34 ACTIVE EL TEMP");
                                    }
                                }
                                else
                                {
                                    tests[5].testGearbeitet = 10;
                                    TempWeiter.Start();
                                }
                            }
                            else
                            {
                                if (testCaseResult == "passed")
                                {
                                    tests[5].testBestanden = true;
                                    TexteHinzufuegen(Resources.DC2IsoTest + " " + Resources.m_bestanden);
                                }
                                else
                                {
                                    tests[5].testBestanden = false;
                                    TexteHinzufuegen(Resources.DC2IsoTest + " " + Resources.m_bestandenNicht);
                                }

                                //TEST
                                //prozess = 7;
                                prozess++;



                                TempWeiter.Start();
                            }
                        }
                        else
                        {
                            prozess++;
                            TempWeiter.Start();
                        }
                        break;
                    case 7:
                        endProgram();
                        Console.WriteLine("07 LLAME A PARAR EL PROGRAMA");
                        break;
                    default:
                        break;

                }
            }
            else
            {
                TexteHinzufuegen(Resources.m_cdsNotConnected);
                if (CDSverloren > 3)
                {
                    endProgram();
                    Console.WriteLine("08 LLAME A PARAR EL PROGRAMA");
                } 
                    
                else
                {
                    CDSverloren++;
                    TempWeiter.Start();
                    Console.WriteLine("ULT ACTIVE EL TEMP");
                }
            }
            
        }

        private bool testStarten(string projectName, string testName)
        {
            int Schritt = 1;
            bool erfoglichStart = false;
            int Versuch = 0;

            //List<CdsTestCaseLibrary.Models.SourceSink> AvailableSinks = _testCaseHandler.GetSinks();
            //if (AvailableSinks.Count > 0)
            //{
                //for (var i = 0; i < AvailableSinks.Count; i++)
                //{
                    //senke = AvailableSinks[i];
                //}
            //}

            while (Schritt < 3)
            {
                if (Schritt == 1)
                {
                    if (lblCDSstatus.Text == "inactive")
                    {
                        // Iniciar Test
                        //_testCaseHandler.ResetErrors();
                        TexteHinzufuegen("Project: " + projectName);
                        Console.WriteLine(testName + " " + Resources.m_starten + " project: " + projectName);
                        _testCaseHandler.StartTest(projectName, null, senke, CdsTestCaseLibrary.Enums.ControlMode.Test, "SICHARGE_D_350_kW_Prototype.evse");
                        Schritt = 2;

                        

                        Thread.Sleep(5000);
                    }
                    else if (lblCDSstatus.Text == "error")
                    {
                        _testCaseHandler.ResetErrors();
                        _testCaseHandler.SendCdsSourceSinkRequest();
                        Thread.Sleep(5000);
                    }
                    else
                    {
                        Thread.Sleep(3000);
                    }
                }
                else if (Schritt == 2)
                {
                    if (lblCDSstatus.Text == "active")
                    {
                        Schritt = 3;
                        erfoglichStart = true;
                    }
                    else
                    {
                        if (Versuch < 3)
                        {
                            Versuch++;
                            _testCaseHandler.ResetErrors();
                            Thread.Sleep(5000);
                            Schritt = 1;
                        }
                        else
                        {
                            erfoglichStart = false;
                            Schritt = 3;
                        }
                    }
                }
            }
            return erfoglichStart;
        }

        private void endProgram()
        {
            TempWeiter.Stop();
            TexteHinzufuegen("Ende Programm");
            TexteHinzufuegen("----------------------");
            resetAllesTest();
            prozess = 0;
            CDSverloren = 0;
        }

        private void resetAllesTest()
        {
            for (int i = 0; i < tests.Length; i++)
            {
                tests[i].testBestanden = false;
                tests[i].testGearbeitet = 0;
            }
        }

        private void btnStarten_Click(object sender, EventArgs e)
        {
            DC1fullTest = true;
            DC2fullTest = true;

            // -- Einstelllungen nehmen --
            if (checkBoxERK.Checked) erk = true;
            else erk = false;

            if(checkBoxNotausTest.Checked) tests[0].testErfordelich = true;
            else tests[0].testErfordelich=false;

            if(checkBoxTurkontaktTest.Checked) tests[1].testErfordelich = true;
            else tests[1].testErfordelich = false;

            if (checkBoxTestLinks.Checked) tests[2].testErfordelich = true;
            else
            {
                tests[2].testErfordelich = false;
                DC1fullTest = false;
            }

            if (checkBoxIsoTestLinks.Checked) tests[3].testErfordelich = true;
            else
            {
                tests[3].testErfordelich = false;
                DC1fullTest = false;
            }

            if (checkBoxTestRechts.Checked) tests[4].testErfordelich = true;
            else
            {
                tests[4].testErfordelich = false;
                DC2fullTest = false;
            }

            if (checkBoxIsoTestRechts.Checked) tests[5].testErfordelich = true;
            else
            {
                tests[5].testErfordelich = false;
                DC2fullTest = false;
            }

            pruefFeld = cbPruffeld.Text;

            prozess = 0;
            CDSverloren = 0;

            Charger = new ChargerTest(tbFA.Text, tests);

            TempWeiter.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            int p_ = prozess;
            prozess = 0;
            TempWeiter.Stop();
            _testCaseHandler.StopTest();
            if (p_ > 0)
            {
                
                resetAllesTest();
                TexteHinzufuegen("Test wurde gestopet");
                endProgram();
                Console.WriteLine("09 LLAME A PARAR EL PROGRAMA");
            }
            else if(p_ == 0)
            {
                TexteHinzufuegen("Der Test läuft nicht");
            }
        }

        private void btnCDSVerbinden_Click(object sender, EventArgs e)
        {
            // -- CDS Verbinden --
            int port = 50002;
            string pf = "ip" + cbPruffeld.Text;
            string ip = getIPAdresse(pf);
            ipAdresse = IPAddress.Parse(ip);
            //if (cbPruffeld.Text == "PF1" || cbPruffeld.Text == "PF4") _project = "Project.cdpj";
            //if (cbPruffeld.Text == "PF2" || cbPruffeld.Text == "PF3") _project = "Project_Multiplexer.cdpj";

            _testCaseHandler.Connect(ipAdresse, port);

            lblStatusVerbindung.Text = _testCaseHandler.Connection().ToString();
        }

        private void btnCDSTrennen_Click(object sender, EventArgs e)
        {
            _testCaseHandler.Disconnect();
        }

        private void btnCDSReset_Click(object sender, EventArgs e)
        {
            _testCaseHandler.ResetErrors();
            _testCaseHandler.SendCdsSourceSinkRequest();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _testCaseHandler.StopTest();
            _testCaseHandler.Disconnect();
            _testCaseHandler.Dispose();
        }

        private class TabelleDatei
        {
            public string Titel { get; set; }
            public List<List<string>> Dateien { get; set; }

            public TabelleDatei(string titel, List<List<string>> dateien)
            {
                Titel = titel;
                Dateien = dateien;
            }
        }

        private void PDFerstellen()
        {

            // Mostrar el cuadro de dialogo para guardar el archivo
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = Resources.m_SpeichernAlsPDF,
                FileName = Resources.bericht + "_" + Charger.FA + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf"
            };
            //string pdfFilePath = @"C:\Users\z004kszj\source\repos\EoL_Automatik_Ladetest\Reporte.pdf";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string pdfFilePath = saveFileDialog.FileName;
                // Ruta del documento de Word
                string wordFilePath = @"C:\Users\z004kszj\source\repos\EoL_Automatik_Ladetest\Bericht.docx";

                // Crear una instancia de la aplicación de Word
                Application wordApp = new Application();
                Document wordDoc = null;

                try
                {
                    // Abrir el documento de Word
                    wordDoc = wordApp.Documents.Open(wordFilePath);

                    // Limpiar el documento de párrafos vacíos antes de agregar contenido nuevo
                    //LimpiarParrafosVacios(wordDoc);

                    // Rellenar los campos en el documento de Word
                    foreach (Field field in wordDoc.Fields)
                    {
                        if (field.Code.Text.Contains("SERIAL_NUMBER_CHARGER"))
                        {
                            field.Result.Text = Charger.FA; // Número de serie dinámico
                        }
                        else if (field.Code.Text.Contains("TOTAL_RESULT"))
                        {
                            bool result = true;
                            foreach(Test test in Charger.tests)
                            {
                                if (!test.testBestanden) result = false;
                            }
                            if (result) field.Result.Text = "passed";    // Resultado del Test
                            else field.Result.Text = "failed";
                        }
                        else if (field.Code.Text.Contains("DATE"))
                        {
                            field.Result.Text = DateTime.Now.ToString("dd/MM/yyyy"); // Fecha actual
                        }
                        else if (field.Code.Text.Contains("SERIAL_NUMBER_CDS"))
                        {
                            field.Result.Text = Charger.CDS_SerialNumber; // Numero de serie de CDS
                        }
                        else if (field.Code.Text.Contains("CDS_FW_VERSION"))
                        {
                            field.Result.Text = Charger.CDS_FwVersion; // Numero de serie de CDS
                        }
                        else if (field.Code.Text.Contains("SINK"))
                        {
                            field.Result.Text = Charger.Sink; // Fuente
                        }
                        //else if (field.Code.Text.Contains("MAX_DC_POWER"))
                        //{
                            //field.Result.Text = "10kw"; // Maxima potencia
                        //}
                        //else if (field.Code.Text.Contains("NORM"))
                        //{
                            //field.Result.Text = norm; // Norma utilizada
                        //}
                    }
                    // Insertar un párrafo vacío antes de agregar el primer título de la tabla
                    LeerenAbsatzEinfuegen(wordDoc);

                    foreach (Test test in Charger.tests)
                    {
                        if (test.testErfordelich)
                        {
                            if (test.name.Contains(Resources.tuerKontaktTest) || test.name.Contains(Resources.notAusTest))
                            {
                                var tabelleDatei = new TabelleDatei(test.name, new List<List<string>>
                                {
                                    new List<string> { Resources.ergebnis, test.testBestanden.ToString()}
                                });
                                TabelleHinzufuegen(wordDoc, tabelleDatei);
                            }
                            else if (test.name.Contains("Ladetest"))
                            {
                                List<List<string>> datei;
                                if (erk)
                                {
                                    datei = new List<List<string>>{
                                        new List<string> { "Current", "18A" },
                                        new List<string> { "Voltage", "550V" },
                                        new List<string> { "Duration", "60s" },
                                        new List<string> { "Quantity", "3" },
                                        new List<string> { "Result", test.testBestanden.ToString() },
                                        new List<string> { "Isolationtest", "passed" }
                                    };
                                }
                                else
                                {
                                    datei = new List<List<string>>{
                                        new List<string> { "Current", "18A" },
                                        new List<string> { "Voltage", "550V" },
                                        new List<string> { "Duration", "30m0s" },
                                        new List<string> { "Quantity", "3" },
                                        new List<string> { "Result", test.testBestanden.ToString() },
                                        new List<string> { "Isolationtest", "passed" }
                                    };
                                }
                                //var tabelleDatei = new TabelleDatei(test.name, new List<List<string>>
                                //{
                                    //new List<string> { "Power", "8,1kw" },
                                    //new List<string> { "Current", "18A" },
                                    //new List<string> { "Voltage", "450V" },
                                    //new List<string> { "Duration", "1m" },
                                    //new List<string> { "Quantity", "3" },
                                    //new List<string> { "Result", test.testBestanden.ToString() },
                                    //new List<string> { "Isolationtest", "Aprobed" }
                                //});
                                var tabelleDatei = new TabelleDatei(test.name, datei);
                                TabelleHinzufuegen(wordDoc, tabelleDatei);
                            }

                        }
                    }
                    // Guardar el documento de Word como PDF
                    wordDoc.SaveAs2(pdfFilePath, WdSaveFormat.wdFormatPDF);

                    // Eliminar las tablas del documento de Word para mantenerlo limpio
                    foreach (Table table in wordDoc.Tables)
                    {
                        table.Delete();
                    }
                    // Eliminar los títulos de las tablas
                    //foreach (Paragraph paragraph in wordDoc.Paragraphs)
                    for (int i = wordDoc.Paragraphs.Count; i > 0; i--)
                    {
                        Paragraph paragraph = wordDoc.Paragraphs[i];
                        //if (paragraph.Range.Text.Contains(Resources.notAusTest) || paragraph.Range.Text.Contains(Resources.tuerKontaktTest) || paragraph.Range.Text.Contains(Resources.DC1LadeTest) || paragraph.Range.Text.Contains(Resources.DC2LadeTest))
                        if (paragraph.Range.Text.Contains(Resources.notAusTest) || paragraph.Range.Text.Contains(Resources.tuerKontaktTest) || paragraph.Range.Text.Contains(Resources.DC1LadeTest) || paragraph.Range.Text.Contains(Resources.DC2LadeTest))
                        {
                            //paragraph.Range.Delete();
                            if (i <= wordDoc.Paragraphs.Count)
                            {
                                wordDoc.Paragraphs[i].Range.Delete();
                            }
                        }
                    }
                    // Eliminar los párrafos vacíos
                    foreach (Paragraph paragraph in wordDoc.Paragraphs)
                    {
                        if (string.IsNullOrWhiteSpace(paragraph.Range.Text))
                        {
                            paragraph.Range.Delete();    
                        }
                    }
                    //for (int i = wordDoc.Paragraphs.Count; i > 1; i--)
                    //{
                        //Paragraph paragraph = wordDoc.Paragraphs[i];
                        //if (string.IsNullOrWhiteSpace(paragraph.Range.Text))
                        //{
                            //paragraph.Range.Delete();
                        //}
                    //}
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    // Cerrar el documento y la aplicación de Word
                    wordDoc.Close();
                    wordApp.Quit();
                }
            }
        }

        private void LeerenAbsatzEinfuegen(Document wordDoc)
        {
            // Mover el cursor al final del documento
            object endOfDoc = "\\endofdoc";
            object missing = Type.Missing;
            Range wordRange = wordDoc.Bookmarks.get_Item(ref endOfDoc).Range;

            // Añadir un párrafo vacío
            Paragraph emptyParagraph = wordDoc.Content.Paragraphs.Add(ref missing);
            emptyParagraph.Range.InsertParagraphAfter();
        }

        private void TabelleHinzufuegen(Document wordDoc, TabelleDatei tablaDatos)
        {
            // Mover el cursor al final del documento
            object endOfDoc = "\\endofdoc";
            object missing = Type.Missing;
            Range wordRange = wordDoc.Bookmarks.get_Item(ref endOfDoc).Range;

            // Añadir título para la tabla
            Paragraph title = wordDoc.Content.Paragraphs.Add(ref missing);
            title.Range.Text = tablaDatos.Titel;
            title.Range.Font.Bold = 1;
            title.Range.InsertParagraphAfter();

            // Mover el cursor al final del documento nuevamente
            wordRange = wordDoc.Bookmarks.get_Item(ref endOfDoc).Range;

            // Crear la tabla
            int numRows = tablaDatos.Dateien.Count;
            int numCols = tablaDatos.Dateien[0].Count;
            Table table = wordDoc.Tables.Add(wordRange, numRows, numCols);
            table.Borders.Enable = 1;

            // Rellenar la tabla con datos
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    table.Cell(i + 1, j + 1).Range.Text = tablaDatos.Dateien[i][j];
                }
            }

            // Añadir un salto de línea después de la tabla
            Paragraph afterTable = wordDoc.Content.Paragraphs.Add(ref missing);
            afterTable.Range.InsertParagraphAfter();
        }

        private void btnPDF_Click(object sender, EventArgs e)
        {
            PDFerstellen();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 6; i++)
            {
                tests[i].testErfordelich = true;
                tests[i].testBestanden = true;
            }
            //List<CdsTestCaseLibrary.Models.SourceSink> AvailableSinks = _testCaseHandler.GetSinks();
            //if (AvailableSinks.Count > 0)
            //{
            //for (var i = 0; i < AvailableSinks.Count; i++)
            //{
            //senke = AvailableSinks[i];
            //}
            //}
            //_testCaseHandler.StartTest(tbFA.Text, null, senke, CdsTestCaseLibrary.Enums.ControlMode.Test, "SICHARGE_D_350_kW_Prototype.evse");

            string pr = tbFA.Text;
            string[] testCases = _testCaseHandler.GetTestCases(pr).ToArray();
            //string tc = _testCaseHandler.GetTestCases(pr)[1];
            //string selectedTestCase = testCases[1];
            string spName = _testCaseHandler.GetParameters(_testCaseHandler.GetTestCases(pr)[1], pr)[0].ParamValues[0].Value;
            string spValue = _testCaseHandler.GetParameters(_testCaseHandler.GetTestCases(pr)[1], pr)[0].ParamValues[1].Value;
            string spUnit = _testCaseHandler.GetParameters(_testCaseHandler.GetTestCases(pr)[1], pr)[0].ParamValues[2].Value;

            TexteHinzufuegen(spName + ": " + spValue + spUnit);

            string stName = _testCaseHandler.GetParameters(_testCaseHandler.GetTestCases(pr)[1], pr)[1].ParamValues[0].Value;
            string stValue = _testCaseHandler.GetParameters(_testCaseHandler.GetTestCases(pr)[1], pr)[1].ParamValues[1].Value;
            string stUnit = _testCaseHandler.GetParameters(_testCaseHandler.GetTestCases(pr)[1], pr)[1].ParamValues[2].Value;
            TexteHinzufuegen(stName + ": " + stValue + stUnit);

            string durName = _testCaseHandler.GetParameters(_testCaseHandler.GetTestCases(pr)[1], pr)[2].ParamValues[0].Value;
            string durValue = _testCaseHandler.GetParameters(_testCaseHandler.GetTestCases(pr)[1], pr)[2].ParamValues[1].Value;
            string durUnit = _testCaseHandler.GetParameters(_testCaseHandler.GetTestCases(pr)[1], pr)[2].ParamValues[2].Value;
            TexteHinzufuegen(durName + ": " + durValue + durUnit);

            /*
            foreach (string testCase in testCases)
            {
                foreach (Parameter parameterStructure in _testCaseHandler.GetParameters(testCase, pr))
                {
                    TexteHinzufuegen(parameterStructure.ToString());
                    TexteHinzufuegen(parameterStructure.Id.ToString());
                    foreach (ParamValue paramValue in parameterStructure.ParamValues)
                    {
                        TexteHinzufuegen("Name: " + paramValue.Name);
                        TexteHinzufuegen("Value: " + paramValue.Value);
                    }
                }
            }
            */

        }
    }
}
