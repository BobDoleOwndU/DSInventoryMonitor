using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace DSInventoryMonitor
{
    public partial class MainForm : Form
    {
        private const int CONTROL_SPACING = 24;
        private const int LABEL_SPACING = 3;

        private TextBox[] textBoxes = new TextBox[2048];
        private Timer processTimer = new Timer();
        private Timer updateTimer = new Timer();

        public MainForm()
        {
            InitializeComponent();
            processTimer.Interval = 1000;
            processTimer.Tick += FindProcess;
            updateTimer.Interval = 1000;
            updateTimer.Tick += Update;
            CreateTextBoxes();

            processTimer.Start();
        } //constructor

        private void CreateTextBoxes()
        {
            int heightOffset = 0;

            mainPanel.Controls.Clear();

            for (int i = 0; i < 2048; i++)
            {
                if (i % 16 == 0)
                    heightOffset = 0;

                Label label = new Label();
                label.Location = new System.Drawing.Point(160 * (i / 16), heightOffset + LABEL_SPACING);
                label.Width = 32;
                label.Text = (i).ToString();
                mainPanel.Controls.Add(label);

                textBoxes[i] = new TextBox();
                textBoxes[i].Location = new System.Drawing.Point(label.Right, heightOffset);

                textBoxes[i].ReadOnly = true;
                mainPanel.Controls.Add(textBoxes[i]);

                heightOffset += CONTROL_SPACING;
            } //for
        } //CreateTextBoxes

        private void FindProcess(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcesses();

            foreach (Process p in processes)
            {
                if (p.MainWindowTitle == Remastered.WindowTitle)
                {
                    Remastered.process = p;

                    processTimer.Stop();
                    updateTimer.Start();
                } //if
            } //foreach
        } //LoopStarter

        private void Update(object sender, EventArgs e)
        {
            InventorySlot[] inventorySlots;
            Equipment equipment;

            if (Remastered.process.HasExited)
            {
                updateTimer.Stop();
                runningLabel.Text = "Not Running";
                runningLabel.ForeColor = Color.DarkRed;

                processTimer.Start();
                return;
            } //if

            try
            {
                inventorySlots = Remastered.ReadInventoryArray();
                equipment = Remastered.ReadEquipment();

                runningLabel.Text = "Running";
                runningLabel.ForeColor = Color.DarkGreen;

                for (int i = 0; i < 2048; i++)
                {
                    try
                    {
                        textBoxes[i - 0].Text = ItemDatabase.mainDict[inventorySlots[i].itemType][inventorySlots[i].id];
                        //textBoxes[i - 64].Text = inventorySlots[i].itemType.ToString("x");
                    } //try
                    catch
                    {
                        textBoxes[i - 0].Text = inventorySlots[i].id.ToString();
                    } //catch
                } //for

                leftHand1TextBox.Text = equipment.leftHand1.ToString();
                rightHand1TextBox.Text = equipment.rightHand1.ToString();
                leftHand2TextBox.Text = equipment.leftHand2.ToString();
                rightHand2TextBox.Text = equipment.rightHand2.ToString();
                arrow1TextBox.Text = equipment.arrow1.ToString();
                bolt1TextBox.Text = equipment.bolt1.ToString();
                arrow2TextBox.Text = equipment.arrow2.ToString();
                bolt2TextBox.Text = equipment.bolt2.ToString();
                headTextBox.Text = equipment.head.ToString();
                chestTextBox.Text = equipment.chest.ToString();
                handsTextBox.Text = equipment.hands.ToString();
                legsTextBox.Text = equipment.legs.ToString();
                ring1TextBox.Text = equipment.ring1.ToString();
                ring2TextBox.Text = equipment.ring2.ToString();
                good1TextBox.Text = equipment.good1.ToString();
                good2TextBox.Text = equipment.good2.ToString();
                good3TextBox.Text = equipment.good3.ToString();
                good4TextBox.Text = equipment.good4.ToString();
                good5TextBox.Text = equipment.good5.ToString();
            } //try
            catch (Exception ex)
            {
                updateTimer.Stop();
                runningLabel.Text = "Not Running";
                runningLabel.ForeColor = Color.DarkRed;

                processTimer.Start();

                MessageBox.Show(ex.StackTrace, ex.Message);
            } //catch
        } //Loop
    } //class
} //namespace
