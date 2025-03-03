using System;
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
//using System.IO;
using Application = Microsoft.Office.Interop.Word.Application;
using System.Threading;
using CdsTestCaseLibrary.Models.Project;
//using static EoL_Automatik_Ladetest.Form1;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Linq;
using PdfSharp.Fonts;
using static System.Net.Mime.MediaTypeNames;
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
        private bool inProzess = false;
        private bool erk = false;
        private SourceSink senke;
        private string _project;
        private string testCaseResult;
        private string pruefFeld;
        private int CDSverloren = 0;
        private bool mode;
        private bool DC1fullTest = true;
        private bool DC2fullTest = true;
        private int Schritt;
        private bool PDF = false;

        public Form1()
        {
            InitializeComponent();

            mode = true;
            Schritt = 3;
            
            // -- Struct TESTS --
            tests[0] = new Test(Resources.notAusTest, "Emergency button test" , false);
            tests[1] = new Test(Resources.tuerKontaktTest, "Door contact test" , true);
            tests[2] = new Test(Resources.DC1LadeTest, "DC1 charging test" , true);
            tests[3] = new Test(Resources.DC1IsoTest, "DC1 isolation test" , true);
            tests[4] = new Test(Resources.DC2LadeTest, "DC2 charging test" , true);
            tests[5] = new Test(Resources.DC2IsoTest, "DC2 isolation test" , true);


            // -- TestCase Handler --
            ipAdresse = new IPAddress(new byte[] { 192, 168, 30, 30 });
            testPath = @"D:\ChargingDiscover\Projects\SoP_Prueba";
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
               if (mode && prozess > 2 && Schritt >= 3) 
                { 
                    TempWeiter.Start();
                    Console.WriteLine("00 ACTIVE EL TEMP");
                }
                if (!mode && prozess > 1 && Schritt >= 3)
                {
                    TempWeiter.Start();
                    Console.WriteLine("00 ACTIVE EL TEMP");
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
            
            if (state == CdsTestCaseLibrary.Enums.ConnectionState.Connected)
            {
                _testCaseHandler.SendCdsSourceSinkRequest();
                _testCaseHandler.SendCdsInfoRequest();
                btnStarten.Enabled = true;
            }
            else
            {
                btnStarten.Enabled= false;
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
                    Console.WriteLine("00 LLAME A LA Fn EOL-Automatik");
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

            //checkBoxNotausTest.Checked = false;
            //checkBoxNotausTest.Enabled = false;

            //checkBoxTurkontaktTest.Checked = true;
            //checkBoxTestLinks.Checked = true;
            //checkBoxIsoTestLinks.Checked = true;
            //checkBoxTestRechts.Checked = true;
            //checkBoxIsoTestRechts.Checked = true;

            btnStarten.Enabled = false;
            //btnStop.Enabled = false;

            automatikToolStripMenuItem_Click(sender, e);
        }

        private void pruffeld1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string aktuellIP = getIPAdresse("ipPF1");
            string neueIP = Interaction.InputBox("IP Prüffeld 1:", "Ändern IP Adresse", aktuellIP);
            if (!string.IsNullOrEmpty(neueIP)) setIPAdresse("ipPF1",neueIP);
        }

        private void pruffeld2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string aktuellIP = getIPAdresse("ipPF2");
            string neueIP = Interaction.InputBox("IP Prüffeld 2:", "Ändern IP Adresse", aktuellIP);
            if (!string.IsNullOrEmpty(neueIP)) setIPAdresse("ipPF2", neueIP);
        }

        private void pruffeld3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string aktuellIP = getIPAdresse("ipPF3");
            string neueIP = Interaction.InputBox("IP Prüffeld 3:", "Ändern IP Adresse", aktuellIP);
            if (!string.IsNullOrEmpty(neueIP)) setIPAdresse("ipPF3", neueIP);
        }

        private void pruffeld4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string aktuellIP = getIPAdresse("ipPF4");
            string neueIP = Interaction.InputBox("IP Prüffeld 4:", "Ändern IP Adresse", aktuellIP);
            if (!string.IsNullOrEmpty(neueIP)) setIPAdresse("ipPF4", neueIP);
        }

        private void automatikToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mode = true;
            modeToolStripMenuItem.Text = Resources.mode_automatik;

            if(checkBoxNotaus.Checked) checkBoxNotausTest.Checked = true;
            else checkBoxNotausTest.Checked = false;
            checkBoxNotausTest.Enabled = false;

            checkBoxTurkontaktTest.Enabled = false;
            checkBoxTurkontaktTest.Checked = true;

            checkBoxTestLinks.Enabled = false;
            checkBoxTestLinks.Checked = true;

            checkBoxTestRechts.Enabled = false;
            checkBoxTestRechts.Checked = true;

            checkBoxIsoTestLinks.Enabled = false;
            checkBoxIsoTestLinks.Checked = true;

            checkBoxIsoTestRechts.Enabled=false;
            checkBoxIsoTestRechts.Checked=true;

        }

        private void wartungToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mode = false;
            modeToolStripMenuItem.Text = Resources.mode_wartung;

            if (!checkBoxERK.Checked)
            {
                checkBoxNotausTest.Enabled = true;
                checkBoxNotaus.Enabled = true;
            }
            
            checkBoxTurkontaktTest.Enabled = true;

            checkBoxTestLinks.Enabled = true;

            checkBoxTestRechts.Enabled = true;

            checkBoxIsoTestLinks.Enabled = true;

            checkBoxIsoTestRechts.Enabled = true;

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
                if (checkBoxNotaus.Checked == true)
                {
                    checkBoxNotausTest.Enabled = true;
                }
                else
                {
                    checkBoxNotausTest.Enabled = false;
                }
            }
        }

        private void checkBoxNotaus_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxNotaus.Checked == true)
            {
                if (!mode) checkBoxNotausTest.Enabled = true;
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

        public struct Test
        {
            public string name { get; set; }
            public string englishName { get; set; }
            public bool testErfordelich { get; set; }
            public bool testBestanden { get; set; }
            public int testGearbeitet { get; set; }

            public List<List<List<string>>> tabelleDatei { get; set; }

            public Test(string testname, string eName, bool erfordelich)
            {
                name = testname;
                englishName = eName;
                testErfordelich = erfordelich;
                testBestanden = false;
                testGearbeitet = 0;
                tabelleDatei = new List<List<List<string>>>();
            }
        }

        public class ChargerTest
        {
            public string FA {  get; set; }
            public string CDS_SerialNumber { get; set; }
            public string CDS_FwVersion { get; set; }
            public string Sink { get; set; }

            public Test[] tests { get; set; }

            public ChargerTest(string serienNummer, Test[] test)
            {
                FA = serienNummer;
                tests = test;
                CDS_SerialNumber = "--";
                CDS_FwVersion = "--";
                Sink = "--";
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
            try
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
                            Console.WriteLine("00 EoL-A => ACTIVE EL TEMP");

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
                                            TexteHinzufuegen(Resources.notAusTest + " " + Resources.m_starten);

                                            antworte = MessageBox.Show(Resources.m_f_LadePistgesteckt, Resources.m_bestaetigt, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                                            if (antworte == DialogResult.OK)
                                            {
                                                Console.WriteLine("Intentar Iniciar Test");

                                                if (testStarten(pruefFeld + "tna.cdpj", Charger.tests[0].name))
                                                {
                                                    Charger.tests[0].testGearbeitet++;
                                                    Console.Write("01,5  EoL-A => ACTIVE EL TEMP");
                                                    TempWeiter.Start();
                                                }
                                                else
                                                {
                                                    endProgram();
                                                    Console.WriteLine("00 EoL-A =>  LLAME A PARAR EL PROGRAMA");
                                                }
                                            }
                                            else
                                            {
                                                TexteHinzufuegen(Resources.m_testStopt);
                                                Console.WriteLine("01 EoL-A =>  LLAME A PARAR EL PROGRAMA");
                                                endProgram();
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
                                                Console.WriteLine("01 EoL-A =>  ACTIVE EL TEMP");
                                                Charger.tests[0].testGearbeitet++;
                                            }
                                            else
                                            {
                                                Console.WriteLine("No se puede activar temporizador de Notaus test, porque el CDS aún no se ha iniciado");
                                                TempWeiter.Start();
                                                Console.WriteLine("02 EoL-A =>  ACTIVE EL TEMP");
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
                                                    Console.WriteLine("03 EoL-A =>  ACTIVE EL TEMP");
                                                }
                                                else
                                                {
                                                    endProgram();
                                                    Console.WriteLine("02 EoL-A =>  LLAME A PARAR EL PROGRAMA");
                                                }
                                            }
                                            break;
                                        default:
                                            break;
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
                                        TexteHinzufuegen("  ");
                                        prozess++;

                                        TempWeiter.Start();
                                        Console.WriteLine("04 EoL-A =>  ACTIVE EL TEMP");
                                    }
                                    else
                                    {
                                        endProgram();
                                        Console.WriteLine("03 EoL-A =>  LLAME A PARAR EL PROGRAMA");
                                    }
                                }
                            }
                            else
                            {
                                prozess++;
                                TempWeiter.Start();
                                Console.WriteLine("05 EoL-A =>  ACTIVE EL TEMP");
                            }
                            break;

                        //Türkontakt Test
                        case 2:
                            if (Charger.tests[1].testGearbeitet < 10)
                            {
                                switch (Charger.tests[1].testGearbeitet)
                                {
                                    case 0:
                                        //empezar test
                                        TexteHinzufuegen(Resources.tuerKontaktTest + " " + Resources.m_starten);
                                        Console.WriteLine("Intentar Iniciar Test");
                                        if (Charger.tests[0].testErfordelich)
                                        {
                                            if (testStarten(pruefFeld + "tna.cdpj", Charger.tests[1].name))
                                            {
                                                Charger.tests[1].testGearbeitet++;
                                                Console.WriteLine("05,5  EoL-A => ACTIVE TEMP");
                                                TempWeiter.Start();
                                            }
                                            else
                                            {
                                                endProgram();
                                                Console.WriteLine("04 EoL-A =>  LLAME A PARAR EL PROGRAMA");
                                            }
                                        }
                                        else
                                        {
                                            antworte = MessageBox.Show(Resources.m_f_LadePistgesteckt, Resources.m_bestaetigt, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                                            if (antworte == DialogResult.OK)
                                            {
                                                Console.WriteLine("Intentar Iniciar Test");

                                                if (testStarten(pruefFeld + "tna.cdpj", Charger.tests[0].name))
                                                {
                                                    Charger.tests[1].testGearbeitet++;
                                                    Console.WriteLine("05,6  EoL-A => ACTIVE TEMP");
                                                    TempWeiter.Start();
                                                }
                                                else
                                                {
                                                    endProgram();
                                                    Console.WriteLine("05 EoL-A =>  LLAME A PARAR EL PROGRAMA");
                                                }
                                            }
                                            else
                                            {
                                                TexteHinzufuegen(Resources.m_testStopt);
                                                Console.WriteLine("06 EoL-A =>  LLAME A PARAR EL PROGRAMA");
                                                endProgram();
                                            }
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
                                            Console.WriteLine("06 EoL-A =>  ACTIVE EL TEMP");
                                            Charger.tests[1].testGearbeitet++;
                                        }
                                        else
                                        {
                                            Console.WriteLine("No se puede activar temporizador de Notaus test, porque el CDS aún no se ha iniciado");
                                            TempWeiter.Start();
                                            Console.WriteLine("07 EoL-A =>  ACTIVE EL TEMP");
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
                                                Console.WriteLine("08 EoL-A => ACTIVE EL TEMP");
                                            }
                                            else
                                            {
                                                endProgram();
                                                Console.WriteLine("07 EoL-A => LLAME A PARAR EL PROGRAMA");
                                            }
                                        }
                                        break;
                                    default:
                                        break;
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
                                    TexteHinzufuegen("  ");
                                    prozess++;

                                    TempWeiter.Start();
                                    Console.WriteLine("09 EoL-A => ACTIVE EL TEMP");
                                }
                                else
                                {
                                    endProgram();
                                    Console.WriteLine("08 EoL-A => LLAME A PARAR EL PROGRAMA");
                                }
                            }
                            break;

                        //DC1 Ladetest + Isotest
                        case 3:
                            //DC1 Ladetest + Isotest
                            if (Charger.tests[2].testGearbeitet == 0)
                            {
                                //Iniciar Test
                                TexteHinzufuegen(Resources.DC1LadeTest + " " + Resources.m_starten);
                                antworte = MessageBox.Show(Resources.DC1 + ": " + Resources.m_f_LadePistgesteckt, Resources.m_bestaetigt, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                                if (antworte == DialogResult.OK)
                                {
                                    /*
                                     * Namen projecte:
                                     * fullmultiLeftTest5m.cdpj
                                     * fullmultiLeftTest1m.cdpj
                                     * fullmultiRightTest5m.cdpj
                                     * fullmultiRightTest1m.cdpj
                                     * fullTest5m.cdpj
                                     * fullTest1m.cdpj
                                    */
                                    string projectName = "full";
                                    if (pruefFeld == "PF2" || pruefFeld == "PF3") projectName = projectName + "multiLeft";
                                    if (erk) projectName = projectName + "Test1m.cdpj";
                                    else projectName = projectName + "Test5m.cdpj";
                                    if (testStarten(projectName, Resources.DC1LadeTest))
                                    {
                                        Charger.tests[2].testGearbeitet = 1;

                                        int cantidad = 1;
                                        foreach (string testCase in _testCaseHandler.GetTestCases(projectName))
                                        {
                                            List<List<string>> datei2 = new List<List<string>>();
                                            foreach (Parameter p in _testCaseHandler.GetParameters(testCase, projectName))
                                            {
                                                string spName = p.ParamValues[0].Value;
                                                string spValue = p.ParamValues[1].Value;
                                                string spUnit = p.ParamValues[2].Value;

                                                datei2.Add(new List<string> { spName, spValue + spUnit });

                                            }
                                            // Obtener el último elemento de tabelleDatei si existe
                                            var tabelleDatei = Charger.tests[2].tabelleDatei;
                                            List<List<string>> lastDatei2 = tabelleDatei.Count > 0 ? tabelleDatei[tabelleDatei.Count - 1] : null;

                                            // Comparar datei2 con el último elemento
                                            if (lastDatei2 != null && AreListsEqual(datei2, lastDatei2))
                                            {
                                                // Si es igual, incrementar la cantidad
                                                cantidad++;
                                            }
                                            else
                                            {
                                                // Si es diferente, agregar la línea con el número de ejecuciones al último elemento
                                                if (lastDatei2 != null)
                                                {
                                                    lastDatei2.Add(new List<string> { "Number of executions", cantidad.ToString() });
                                                }

                                                // Reiniciar la cantidad y agregar el nuevo dato
                                                cantidad = 1;

                                                Charger.tests[2].tabelleDatei.Add(datei2);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        endProgram();
                                        Console.WriteLine("09 EoL-A => LLAME A PARAR EL PROGRAMA");
                                    }
                                }
                                else
                                {
                                    TexteHinzufuegen(Resources.m_testStopt);
                                    endProgram();
                                    Console.WriteLine("10 EoL-A => LLAME A PARAR EL PROGRAMA");
                                }
                            }
                            else if (Charger.tests[2].testGearbeitet == 1)
                            {
                                Charger.tests[2].testBestanden = true;
                                Charger.tests[3].testBestanden = true;
                                string result = "passed";
                                string resultIso = "passed";
                                int testCase = 0;

                                //Thread.Sleep(5000);
                                _testCaseHandler.SendCdsTestCaseResultRequest(1);
                                string ergebnisWartung = _testCaseHandler.GetTestCaseResult().ToString();
                                int versuch = 0;
                                while (ergebnisWartung != "passed" && versuch < 5)
                                {
                                    Console.WriteLine("Esperando a que termine el test: " + ergebnisWartung);
                                    Thread.Sleep(1000);
                                    ergebnisWartung = _testCaseHandler.GetTestCaseResult().ToString();
                                    versuch++;
                                }

                                if (pruefFeld == "PF2" || pruefFeld == "PF3") testCase++;
                                for (int i = testCase; i <= 2 + testCase; i++)
                                {
                                    _testCaseHandler.SendCdsTestCaseResultRequest(i);
                                    Console.WriteLine(_testCaseHandler.GetTestCaseResult().ToString());
                                    if (_testCaseHandler.GetTestCaseResult().ToString() != "passed")
                                    {
                                        Charger.tests[2].testBestanden = false;
                                        result = "failed";
                                    }
                                }
                                _testCaseHandler.SendCdsTestCaseResultRequest(3 + testCase);
                                Console.WriteLine(_testCaseHandler.GetTestCaseResult().ToString());
                                if (_testCaseHandler.GetTestCaseResult().ToString() != "passed")
                                {
                                    Charger.tests[3].testBestanden = false;
                                    result = "failed";
                                }

                                TexteHinzufuegen(Resources.DC1LadeTest + " " + Resources.m_endet);
                                TexteHinzufuegen("  ");


                                // Obtener el último elemento de tabelleDatei si existe
                                var tabelleDatei2 = Charger.tests[2].tabelleDatei;
                                List<List<string>> lastDatei01 = tabelleDatei2.Count > 0 ? tabelleDatei2[tabelleDatei2.Count - 1] : null;
                                Charger.tests[2].tabelleDatei.Remove(Charger.tests[2].tabelleDatei[tabelleDatei2.Count - 1]);
                                Charger.tests[2].tabelleDatei[Charger.tests[2].tabelleDatei.Count - 1].Add(new List<string> { "result", result });

                                Charger.tests[3].testErfordelich = true;
                                if (resultIso == "passed") Charger.tests[3].testBestanden = true;
                                else Charger.tests[3].testBestanden = false;
                                Charger.tests[3].tabelleDatei.Add(lastDatei01);
                                Charger.tests[3].tabelleDatei[Charger.tests[3].tabelleDatei.Count - 1].Add(new List<string> { "result", resultIso });

                                prozess++;
                                TempWeiter.Start();
                            }
                            break;

                        //DC2 Ladetest + Isotest
                        case 4:
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
                                        Console.WriteLine("11 EoL-A => LLAME A PARAR EL PROGRAMA");
                                    }
                                }
                            }
                            else if (Charger.tests[4].testGearbeitet == 1)
                            {
                                string projectName = "full";
                                if (pruefFeld == "PF2" || pruefFeld == "PF3") projectName = projectName + "multiRight";
                                if (erk) projectName = projectName + "Test1m.cdpj";
                                else projectName = projectName + "Test5m.cdpj";
                                if (testStarten(projectName, Resources.DC1LadeTest))
                                {
                                    Charger.tests[4].testGearbeitet = 2;

                                    int cantidad = 1;
                                    foreach (string testCase in _testCaseHandler.GetTestCases(projectName))
                                    {
                                        List<List<string>> datei2 = new List<List<string>>();
                                        foreach (Parameter p in _testCaseHandler.GetParameters(testCase, projectName))
                                        {
                                            string spName = p.ParamValues[0].Value;
                                            string spValue = p.ParamValues[1].Value;
                                            string spUnit = p.ParamValues[2].Value;

                                            datei2.Add(new List<string> { spName, spValue + spUnit });

                                        }
                                        // Obtener el último elemento de tabelleDatei si existe
                                        var tabelleDatei = Charger.tests[4].tabelleDatei;
                                        List<List<string>> lastDatei2 = tabelleDatei.Count > 0 ? tabelleDatei[tabelleDatei.Count - 1] : null;

                                        // Comparar datei2 con el último elemento
                                        if (lastDatei2 != null && AreListsEqual(datei2, lastDatei2))
                                        {
                                            // Si es igual, incrementar la cantidad
                                            cantidad++;
                                        }
                                        else
                                        {
                                            // Si es diferente, agregar la línea con el número de ejecuciones al último elemento
                                            if (lastDatei2 != null)
                                            {
                                                lastDatei2.Add(new List<string> { "Number of executions", cantidad.ToString() });
                                            }

                                            // Reiniciar la cantidad y agregar el nuevo dato
                                            cantidad = 1;
                                            Charger.tests[4].tabelleDatei.Add(datei2);
                                        }
                                    }

                                }
                                else
                                {
                                    endProgram();
                                }
                            }
                            else if (Charger.tests[4].testGearbeitet == 2)
                            {
                                Charger.tests[4].testBestanden = true;
                                Charger.tests[5].testBestanden = true;
                                int testCase = 0;
                                string result = "passed";
                                string resultIso = "passed";

                                _testCaseHandler.SendCdsTestCaseResultRequest(1);
                                string ergebnisWartung = _testCaseHandler.GetTestCaseResult().ToString();
                                int versuch = 0;
                                while (ergebnisWartung != "passed" && versuch < 5)
                                {
                                    Console.WriteLine("Esperando a que termine el test: " + ergebnisWartung);
                                    Thread.Sleep(1000);
                                    ergebnisWartung = _testCaseHandler.GetTestCaseResult().ToString();
                                    versuch++;
                                }

                                if (pruefFeld == "PF2" || pruefFeld == "PF3") testCase++;
                                for (int i = testCase; i <= 2 + testCase; i++)
                                {
                                    _testCaseHandler.SendCdsTestCaseResultRequest(i);
                                    Console.WriteLine(_testCaseHandler.GetTestCaseResult().ToString());
                                    if (_testCaseHandler.GetTestCaseResult().ToString() != "passed")
                                    {
                                        Charger.tests[4].testBestanden = false;
                                        result = "failed";
                                    }
                                }

                                _testCaseHandler.SendCdsTestCaseResultRequest(3 + testCase);
                                Console.WriteLine(_testCaseHandler.GetTestCaseResult().ToString());
                                if (_testCaseHandler.GetTestCaseResult().ToString() != "passed")
                                {
                                    Charger.tests[5].testBestanden = false;
                                    result = "failed";
                                }

                                TexteHinzufuegen(Resources.DC2LadeTest + " " + Resources.m_endet);
                                TexteHinzufuegen("  ");

                                // Obtener el último elemento de tabelleDatei si existe
                                var tabelleDatei3 = Charger.tests[4].tabelleDatei;
                                List<List<string>> lastDatei03 = tabelleDatei3.Count > 0 ? tabelleDatei3[tabelleDatei3.Count - 1] : null;
                                Charger.tests[4].tabelleDatei.Remove(Charger.tests[4].tabelleDatei[tabelleDatei3.Count - 1]);
                                Charger.tests[4].tabelleDatei[Charger.tests[4].tabelleDatei.Count - 1].Add(new List<string> { "result", result });

                                Charger.tests[5].testErfordelich = true;
                                Charger.tests[5].tabelleDatei.Add(lastDatei03);
                                Charger.tests[5].tabelleDatei[Charger.tests[5].tabelleDatei.Count - 1].Add(new List<string> { "result", resultIso });


                                prozess++;
                                TempWeiter.Start();
                            }
                            break;

                        //end program
                        case 5:
                            endProgram();
                            Console.WriteLine("12 EoL-A => LLAME A PARAR EL PROGRAMA");
                            foreach (Test t in Charger.tests)
                            {
                                if (t.testErfordelich)
                                {
                                    if (t.testBestanden)
                                    {
                                        TexteHinzufuegen(t.name + " " + Resources.m_bestanden);
                                    }
                                    else
                                    {
                                        TexteHinzufuegen(t.name + " " + Resources.m_bestandenNicht);
                                    }
                                }
                            }
                            PDF = true;
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
                        Console.WriteLine("13 EoL-A => LLAME A PARAR EL PROGRAMA");
                    }

                    else
                    {
                        CDSverloren++;
                        TempWeiter.Start();
                        Console.WriteLine("10 EoL-A => ACTIVE EL TEMP");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                TexteHinzufuegen("Error: " + ex.Message);
                endProgram();
                Console.WriteLine("14 EoL-A => LLAME A PARAR EL PROGRAMA");
            }
        }
        
        private void EoL_LadeTest()
        {
            try
            {
                if (lblStatusVerbindung.Text == "Connected")
                {
                    DialogResult antworte;
                    if (!inProzess)
                    {
                        //Start
                        TexteHinzufuegen("");
                        TexteHinzufuegen(Resources.m_starten);

                        TexteHinzufuegen(Resources.m_cdsConnected);

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
                        Console.WriteLine("00 EoL => ACTIVE EL TEMP");

                        inProzess = true;
                    }

                    if (Charger.tests[prozess].testErfordelich)
                    {
                        if (prozess < 2)
                        {
                            //Notaus oder Türkontakt Test
                            switch (Charger.tests[prozess].testGearbeitet)
                            {
                                case 0:
                                    //Test starten
                                    TexteHinzufuegen(Charger.tests[prozess].name + " " + Resources.m_starten);
                                    antworte = MessageBox.Show(Resources.m_f_LadePistgesteckt, Resources.m_bestaetigt, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                                    if (antworte == DialogResult.OK)
                                    {
                                        Console.WriteLine("Intentar Iniciar Test");
                                        if (testStarten(pruefFeld + "tna.cdpj", Charger.tests[prozess].name))
                                        {
                                            Charger.tests[prozess].testGearbeitet++;
                                            Console.Write("01 EoL => ACTIVE EL TEMP");
                                            TempWeiter.Start();
                                        }
                                        else
                                        {
                                            endProgram();
                                            Console.WriteLine("00 EoL =>  LLAME A PARAR EL PROGRAMA");
                                        }
                                    }
                                    else
                                    {
                                        TexteHinzufuegen(Resources.m_testStopt);
                                        Console.WriteLine("01 EoL =>  LLAME A PARAR EL PROGRAMA");
                                        endProgram();
                                    }
                                    break;
                                case 1:
                                    //si activo --> activar temporizador
                                    Console.WriteLine("Intentar Activar temporizador de Notaus/Türkontakt test");
                                    if (lblCDSstatus.Text == "active")
                                    {
                                        Console.WriteLine("Se activo temporizador de Notaus/Türkontakt test");
                                        TempWeiter.Interval = 60000;
                                        TempWeiter.Start();
                                        Console.WriteLine("02 EoL => ACTIVE EL TEMP");
                                        Charger.tests[prozess].testGearbeitet++;
                                    }
                                    else
                                    {
                                        Console.WriteLine("No se puede activar temporizador de Notaus test, porque el CDS aún no se ha iniciado");
                                        TempWeiter.Start();
                                        Console.WriteLine("03 EoL => ACTIVE EL TEMP");
                                    }
                                    break;
                                case 2:
                                    //si activo y temporizador se desbordo --> solicitar presionar el boton
                                    Console.WriteLine("El temporizador se desbordo");
                                    if (lblCDSstatus.Text == "active")
                                    {
                                        string text;
                                        if (prozess == 0) text = Resources.m_notausDruecken;
                                        else text = Resources.m_tuerOeffnen;

                                        antworte = MessageBox.Show(text + "\n" + Resources.m_f_errorFlagsLesen, Charger.tests[prozess].name, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                        if (antworte == DialogResult.Yes)
                                        {
                                            _testCaseHandler.StopTest();
                                            Charger.tests[prozess].testGearbeitet = 10;
                                            TempWeiter.Interval = 3000;
                                            TempWeiter.Start();
                                            Console.WriteLine("04 EoL => ACTIVE EL TEMP");
                                        }
                                        else
                                        {
                                            endProgram();
                                            Console.WriteLine("02 EoL => LLAME A PARAR EL PROGRAMA");
                                        }
                                    }
                                    break;
                                case 10:
                                    string text1;
                                    if (prozess == 1) text1 = Resources.m_NotausNormailizieren;
                                    else text1 = Resources.m_tuerNormalisieren;

                                    antworte = MessageBox.Show(text1 + "\n" + Resources.m_f_chargerGruen, Charger.tests[prozess].name, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                    if (antworte == DialogResult.Yes)
                                    {
                                        TexteHinzufuegen(Charger.tests[prozess].name + " " + Resources.m_bestanden);
                                        Charger.tests[prozess].testBestanden = true;

                                        TexteHinzufuegen(Charger.tests[prozess].name + " " + Resources.m_endet);
                                        TexteHinzufuegen("  ");
                                        prozess++;

                                        TempWeiter.Start();
                                        Console.WriteLine("06 EoL => ACTIVE EL TEMP");
                                    }
                                    else
                                    {
                                        endProgram();
                                        Console.WriteLine("03 EoL => LLAME A PARAR EL PROGRAMA");
                                    }
                                    break;
                                default:
                                    break;
                            }

                        }
                        else if (prozess < 6)
                        {
                            //Laden Test
                            if (Charger.tests[prozess].testGearbeitet == 0)
                            {
                                //Iniciar Test
                                TexteHinzufuegen(Charger.tests[prozess].name + " " + Resources.m_starten);

                                string text3 = "";
                                bool multi = false;
                                bool anfragen = false;
                                string site = "";

                                if (pruefFeld == "PF1" || pruefFeld == "PF4")
                                {
                                    if (prozess == 2 || prozess == 3)
                                    {
                                        text3 = "DC1";
                                        if (prozess == 3 && Charger.tests[2].testErfordelich) anfragen = false;
                                        else anfragen = true;
                                    }
                                    else if (prozess == 4 || prozess == 5)
                                    {
                                        text3 = "DC2";
                                        if (prozess == 5 && Charger.tests[4].testErfordelich) anfragen = false;
                                        else anfragen = true;
                                    }
                                }
                                else
                                {
                                    multi = true;
                                    if (prozess == 2 || prozess == 3)
                                    {
                                        site = "Left";
                                        if (prozess == 2)
                                        {
                                            anfragen = true;
                                            if (Charger.tests[4].testErfordelich || Charger.tests[5].testErfordelich)
                                            {
                                                text3 = "DC1 & DC2";
                                            }
                                            else
                                            {
                                                text3 = "DC1";
                                            }
                                        }
                                        else if (!Charger.tests[2].testErfordelich && prozess == 3)
                                        {
                                            if (Charger.tests[4].testErfordelich || Charger.tests[5].testErfordelich)
                                            {
                                                text3 = "DC1 & DC2";
                                                anfragen = true;
                                            }
                                            else
                                            {
                                                text3 = "DC1";
                                                anfragen = true;
                                            }
                                        }
                                        else
                                        {
                                            anfragen = false;
                                        }
                                    }
                                    else if (prozess == 4 || prozess == 5)
                                    {
                                        site = "Right";
                                        text3 = "DC2";
                                        if (!Charger.tests[2].testErfordelich && !Charger.tests[3].testErfordelich)
                                        {
                                            if (prozess == 4) anfragen = true;
                                            else if (!Charger.tests[4].testErfordelich) anfragen = true;
                                            else anfragen = false;
                                        }
                                        else anfragen = false;
                                    }
                                }

                                if (anfragen)
                                {
                                    antworte = MessageBox.Show(text3 + ": " + Resources.m_f_LadePistgesteckt, Resources.m_bestaetigt, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                                }
                                else antworte = DialogResult.OK;

                                if (antworte == DialogResult.OK)
                                {
                                    /*
                                     * Namen projecte:
                                     * multiLeftLaden5m.cdpj
                                     * multiLeftLaden1m.cdpj
                                     * multiLeftIso1m.cdpj
                                     * multiRightLaden5m.cdpj
                                     * multiRightLaden1m.cdpj
                                     * multiRightIso1m.cdpj
                                     * Laden5m.cdpj
                                     * Laden1m.cdpj
                                     * Iso.cdpj
                                    */

                                    string projectName = "";
                                    if (multi) projectName = projectName + "multi" + site;
                                    if (prozess == 2 || prozess == 4)
                                    {
                                        projectName = projectName + "Laden";
                                        if (erk) projectName = projectName + "1m.cdpj";
                                        else projectName = projectName + "5m.cdpj";
                                    }
                                    else projectName = projectName + "Iso.cdpj";

                                    if (testStarten(projectName, Resources.DC1LadeTest))
                                    {
                                        Charger.tests[prozess].testGearbeitet = 1;

                                        int cantidad = 1;
                                        foreach (string testCase in _testCaseHandler.GetTestCases(projectName))
                                        {
                                            List<List<string>> datei2 = new List<List<string>>();
                                            foreach (Parameter p in _testCaseHandler.GetParameters(testCase, projectName))
                                            {
                                                string spName = p.ParamValues[0].Value;
                                                string spValue = p.ParamValues[1].Value;
                                                string spUnit = p.ParamValues[2].Value;

                                                datei2.Add(new List<string> { spName, spValue + spUnit });

                                            }
                                            // Obtener el último elemento de tabelleDatei si existe
                                            var tabelleDatei = Charger.tests[prozess].tabelleDatei;
                                            List<List<string>> lastDatei2 = tabelleDatei.Count > 0 ? tabelleDatei[tabelleDatei.Count - 1] : null;

                                            // Comparar datei2 con el último elemento
                                            if (lastDatei2 != null && AreListsEqual(datei2, lastDatei2))
                                            {
                                                // Si es igual, incrementar la cantidad
                                                cantidad++;
                                            }
                                            else
                                            {
                                                // Si es diferente, agregar la línea con el número de ejecuciones al último elemento
                                                if (lastDatei2 != null)
                                                {
                                                    lastDatei2.Add(new List<string> { "Number of executions", cantidad.ToString() });
                                                }

                                                // Reiniciar la cantidad y agregar el nuevo dato
                                                cantidad = 1;

                                                Charger.tests[prozess].tabelleDatei.Add(datei2);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        endProgram();
                                        Console.WriteLine("04 EoL => LLAME A PARAR EL PROGRAMA");
                                    }
                                }
                                else
                                {
                                    TexteHinzufuegen(Resources.m_testStopt);
                                    endProgram();
                                    Console.WriteLine("05 EoL => LLAME A PARAR EL PROGRAMA");
                                }
                            }
                            else if (Charger.tests[prozess].testGearbeitet == 1)
                            {
                                Charger.tests[prozess].testBestanden = true;
                                int testCase = 0;
                                if (pruefFeld == "PF2" || pruefFeld == "PF3") testCase++;
                                for (int i = testCase; i <= 3 + testCase; i++)
                                {
                                    _testCaseHandler.SendCdsTestCaseResultRequest(i);
                                    if (_testCaseHandler.GetTestCaseResult().ToString() != "passed") Charger.tests[prozess].testBestanden = false;
                                }
                                string result;
                                if (Charger.tests[prozess].testBestanden)
                                {
                                    result = "passed";
                                    TexteHinzufuegen(Charger.tests[prozess].name + " " + Resources.m_bestanden);
                                }
                                else
                                {
                                    result = "failed";
                                    TexteHinzufuegen(Charger.tests[prozess].name + " " + Resources.m_bestandenNicht);
                                }
                                TexteHinzufuegen(Resources.DC1LadeTest + " " + Resources.m_endet);
                                TexteHinzufuegen("  ");


                                // Obtener el último elemento de tabelleDatei si existe
                                var tabelleDatei2 = Charger.tests[prozess].tabelleDatei;
                                List<List<string>> lastDatei01 = tabelleDatei2.Count > 0 ? tabelleDatei2[tabelleDatei2.Count - 1] : null;
                                Charger.tests[prozess].tabelleDatei.Remove(Charger.tests[prozess].tabelleDatei[tabelleDatei2.Count - 1]);
                                Charger.tests[prozess].tabelleDatei[Charger.tests[prozess].tabelleDatei.Count - 1].Add(new List<string> { "result", result });


                                prozess++;
                                TempWeiter.Start();
                            }
                        }
                        else
                        {
                            //Ende
                            endProgram();
                            Console.WriteLine("06 EoL-A => LLAME A PARAR EL PROGRAMA");
                        }

                    }
                    else
                    {
                        //Test ist nicht erfordelich
                        prozess++;
                        TempWeiter.Start();
                        Console.WriteLine("00 EoL => " + Charger.tests[prozess - 1].name + " nicht erfordelich => ACTIVE EL TEMP");
                    }
                }
                else
                {
                    TexteHinzufuegen(Resources.m_cdsNotConnected);
                    if (CDSverloren > 3)
                    {
                        endProgram();
                        Console.WriteLine("13 EoL-A => LLAME A PARAR EL PROGRAMA");
                    }

                    else
                    {
                        CDSverloren++;
                        TempWeiter.Start();
                        Console.WriteLine("10 EoL-A => ACTIVE EL TEMP");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                TexteHinzufuegen("Error: " + ex.Message);
                endProgram();
                Console.WriteLine("14 EoL => LLAME A PARAR EL PROGRAMA");
            }
        }

        private bool testStarten(string projectName, string testName)
        {
            Schritt = 1;
            bool erfoglichStart = false;
            int Versuch = 0;

            try
            {
                while (Schritt < 3)
                {
                    if (Schritt == 1)
                    {
                        if (lblCDSstatus.Text == "inactive")
                        {
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al iniciar el test: " + ex.Message);
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
            inProzess = false;
            CDSverloren = 0;
        }

        private void resetAllesTest()
        {
            for (int i = 0; i < tests.Length; i++)
            {
                //tests[i].testBestanden = false;
                tests[i].testGearbeitet = 0;
            }
        }

        private void btnStarten_Click(object sender, EventArgs e)
        {
            if (lblCDSstatus.Text == "inactive")
            {


                DC1fullTest = true;
                DC2fullTest = true;

                // -- Einstelllungen nehmen --
                if (checkBoxERK.Checked) erk = true;
                else erk = false;

                if (checkBoxNotausTest.Checked) tests[0].testErfordelich = true;
                else tests[0].testErfordelich = false;

                if (checkBoxTurkontaktTest.Checked) tests[1].testErfordelich = true;
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
                PDF = false;
                inProzess = false;

                Charger = new ChargerTest(tbFA.Text, tests);

                TempWeiter.Start();
            }
            else
            {
                MessageBox.Show(Resources.m_CdsFehler);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            int p_ = prozess;
            prozess = 0;
            Schritt = 3;
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
                FileName = Charger.FA + "_TestCaseResport_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf"
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
                                if (test.testErfordelich)
                                {
                                    if (!test.testBestanden) result = false;
                                }
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
                    }

                    // Insertar un párrafo vacío antes de agregar el primer título de la tabla
                    LeerenAbsatzEinfuegen(wordDoc);

                    foreach (Test test in Charger.tests)
                    {
                        if (test.testErfordelich)
                        {
                            if (test.name.Contains(Resources.tuerKontaktTest) || test.name.Contains(Resources.notAusTest))
                            {
                                string result;
                                if (test.testBestanden) result = "passed";
                                else result = "failed";
                                var tabelleDatei = new TabelleDatei(test.englishName, new List<List<string>>
                                {
                                    new List<string> { "Result", result}
                                });
                                TabelleHinzufuegen(wordDoc, tabelleDatei, true);
                            }
                            else if (test.name.Contains("Ladetest") || test.name.Contains("Isolation"))
                            {
                                bool titel = true;
                                foreach (List<List<string>> strings in test.tabelleDatei)
                                {
                                    if (strings.Count > 1)
                                    {
                                        TabelleHinzufuegen(wordDoc, new TabelleDatei(test.englishName, strings), titel);
                                        titel = false;
                                    }
                                }
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
                        if (paragraph.Range.Text.Contains(Charger.tests[0].englishName) || paragraph.Range.Text.Contains(Charger.tests[1].englishName) || paragraph.Range.Text.Contains(Charger.tests[2].englishName) || paragraph.Range.Text.Contains(Charger.tests[3].englishName) || paragraph.Range.Text.Contains(Charger.tests[4].englishName) || paragraph.Range.Text.Contains(Charger.tests[5].englishName))
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

        private void TabelleHinzufuegen(Document wordDoc, TabelleDatei tablaDatos, bool titelErfordelich)
        {
            // Mover el cursor al final del documento
            object endOfDoc = "\\endofdoc";
            object missing = Type.Missing;
            Range wordRange = wordDoc.Bookmarks.get_Item(ref endOfDoc).Range;

            if (titelErfordelich)
            {
                // Añadir título para la tabla
                Paragraph title = wordDoc.Content.Paragraphs.Add(ref missing);
                title.Range.Text = tablaDatos.Titel;
                title.Range.Font.Bold = 1;
                title.Range.InsertParagraphAfter();

                // Mover el cursor al final del documento nuevamente
                wordRange = wordDoc.Bookmarks.get_Item(ref endOfDoc).Range;
            }
            
            // Crear la tabla
            int numRows = tablaDatos.Dateien.Count;
            int numCols = tablaDatos.Dateien[0].Count;
            //int numCols = 3; //new
            Table table = wordDoc.Tables.Add(wordRange, numRows, numCols);
            table.Borders.Enable = 1;

            // Rellenar la tabla con datos
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    string cellText = tablaDatos.Dateien[i][j];
                    table.Cell(i + 1, j + 1).Range.Text = cellText;

                    // Pintar de verde si el contenido es "passed"
                    if (cellText == "passed")
                    {
                        table.Cell(i + 1, j + 1).Shading.BackgroundPatternColor = WdColor.wdColorLightGreen;
                    }
                }
            }
            
            // Añadir un salto de línea después de la tabla
            Paragraph afterTable = wordDoc.Content.Paragraphs.Add(ref missing);
            afterTable.Range.InsertParagraphAfter();
        }

        private void btnPDF_Click(object sender, EventArgs e)
        {
            if ( PDF ) PDFerstellen();
            else
            {
                MessageBox.Show("PDF ist nicht aktiviert");
            }
        }

        // Método para comparar dos listas de listas de cadenas
        private bool AreListsEqual(List<List<string>> list1, List<List<string>> list2)
        {
            if (list1.Count != list2.Count)
                return false;

            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i].Count != list2[i].Count)
                    return false;

                for (int j = 0; j < list1[i].Count; j++)
                {
                    if (list1[i][j] != list2[i][j])
                        return false;
                }
            }

            return true;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            /*
             * Test bestanden
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
            */

            /*
             * PDF prüfen
            Charger = new ChargerTest(tbFA.Text, tests);

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
            //Charger.Sink = senke.ParamValues[0].Value;
            TexteHinzufuegen("CDS S/N: " + Charger.CDS_SerialNumber);
            TexteHinzufuegen("CDS Fw Version: " + Charger.CDS_FwVersion);
            TexteHinzufuegen("Senke: " + Charger.Sink);

            string pr = tbFA.Text;
          
            List<List<List<string>>> pruebaLista = new List<List<List<string>>>();

            int cantidad = 1;

            foreach (string testCase in _testCaseHandler.GetTestCases(pr))
            {
                List<List<string>> datei2 = new List<List<string>>();
                foreach (Parameter p in _testCaseHandler.GetParameters(testCase, pr))
                {
                    string spName = p.ParamValues[0].Value;
                    string spValue = p.ParamValues[1].Value;
                    string spUnit = p.ParamValues[2].Value;

                    TexteHinzufuegen(spName + ": " + spValue + spUnit);
                    datei2.Add(new List<string> { spName, spValue + spUnit });
                }
                TexteHinzufuegen("------------------------");
                pruebaLista.Add(datei2);
                //Charger.tests[2].tabelleDatei.Add(datei2);
                //Charger.tests[4].tabelleDatei.Add(datei2);
                
                // Obtener el último elemento de tabelleDatei si existe
                var tabelleDatei = Charger.tests[2].tabelleDatei;
                List<List<string>> lastDatei2 = tabelleDatei.Count > 0 ? tabelleDatei[tabelleDatei.Count - 1] : null;

                // Comparar datei2 con el último elemento
                if (lastDatei2 != null && AreListsEqual(datei2, lastDatei2))
                {
                    // Si es igual, incrementar la cantidad
                    cantidad++;
                }
                else
                {
                    // Si es diferente, agregar la línea con el número de ejecuciones al último elemento
                    if (lastDatei2 != null)
                    {
                        lastDatei2.Add(new List<string> { "Number of executions", cantidad.ToString() });
                    }

                    // Reiniciar la cantidad y agregar el nuevo dato
                    cantidad = 1;
                    
                    Charger.tests[2].tabelleDatei.Add(datei2);
                    Charger.tests[4].tabelleDatei.Add(datei2);
                    
                }
            }

            // Obtener el último elemento de tabelleDatei si existe
            var tabelleDatei2 = Charger.tests[2].tabelleDatei;
            List<List<string>> lastDatei01 = tabelleDatei2.Count > 0 ? tabelleDatei2[tabelleDatei2.Count - 1] : null;
            Charger.tests[2].tabelleDatei.Remove(Charger.tests[2].tabelleDatei[tabelleDatei2.Count - 1]);
            //Charger.tests[2].tabelleDatei[Charger.tests[2].tabelleDatei.Count - 1].Add(new List<string> { "result", "passed" });

            Charger.tests[3].testErfordelich = true;
            Charger.tests[3].tabelleDatei.Add(lastDatei01);
            //Charger.tests[3].tabelleDatei[Charger.tests[3].tabelleDatei.Count - 1].Add(new List<string> { "result", "passed" });




            // Obtener el último elemento de tabelleDatei si existe
            var tabelleDatei3 = Charger.tests[4].tabelleDatei;
            List<List<string>> lastDatei03 = tabelleDatei3.Count > 0 ? tabelleDatei3[tabelleDatei3.Count - 1] : null;
            Charger.tests[4].tabelleDatei.Remove(Charger.tests[4].tabelleDatei[tabelleDatei3.Count - 1]);
            //Charger.tests[4].tabelleDatei[Charger.tests[4].tabelleDatei.Count - 1].Add(new List<string> { "result", "passed" });

            Charger.tests[5].testErfordelich = true;
            Charger.tests[5].tabelleDatei.Add(lastDatei03);
            Charger.tests[5].tabelleDatei[Charger.tests[5].tabelleDatei.Count - 1].Add(new List<string> { "result", "passed" });



            TexteHinzufuegen("+++++++++++++++++++++++++++");
            int j = 1;
            foreach (List<List<string>> s in pruebaLista)
            {
                string valores; 
                foreach (List<string> s2 in s)
                {
                    valores = "Test numero " + j.ToString() + ": ";
                    foreach (string s3 in s2)
                    {
                        valores += s3 + " ";
                    }
                    TexteHinzufuegen(valores);
                }
                j++;
                TexteHinzufuegen("------------------------");
            }
            */


            // testCase results anfragen
            int testCase = 0;
            if (pruefFeld == "PF2" || pruefFeld == "PF3") testCase++;
            TexteHinzufuegen("+++++++++++++++++++++++++++");
            TexteHinzufuegen("Test Case Results");
            for (int i = testCase; i <= 3 + testCase; i++)
            {
                _testCaseHandler.SendCdsTestCaseResultRequest(i);
                TexteHinzufuegen(i.ToString() + ": " + _testCaseHandler.GetTestCaseResult().ToString());
                //Console.WriteLine(_testCaseHandler.GetTestCaseResult().ToString());
            }
            TexteHinzufuegen("------------------------");
        }
        
    }
}
