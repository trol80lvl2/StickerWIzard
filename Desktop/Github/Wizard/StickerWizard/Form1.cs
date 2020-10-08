using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StickerWizard
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            button1.Cursor = Cursors.Hand;
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
            this.DragOver += new DragEventHandler(Form1_DragOver);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dlg.Filter = "Pdf Files|*.pdf";
            dlg.Multiselect = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                double time = 0;
                button1.Enabled = false;
                label2.Text = "Розраховується час виконання...";
                label2.Visible = true;
                for (int i = 0; i < dlg.FileNames.Length; i++)
                {
                    time += TimeCalc.CalculateTimeForFiles(dlg.FileNames[i]);
                    //Cutter.DeleteJpeg();
                }

                label1.Visible = true;
                //label2.Text = TimeCalc.MinuteSeconds(TimeCalc.CalculateTimeForFiles(dlg.FileNames[i])).ToString();

                for (int i = 0; i < dlg.FileNames.Length; i++)
                {
                    label2.Text = $"Файл {i + 1} з {dlg.FileNames.Length}";

                    //label2.Text = TimeCalc.TimeCutPage(dlg.FileNames[i]).ToString()+"\n";
                    //label2.Text += TimeCalc.TimeGeneratePage()+"\n";
                    ProgressBar progressBar = new ProgressBar();
                    progressBar.Name = progressBar + i.ToString();
                    progressBar.Location = new Point(12, 100);
                    progressBar.Width = 284;
                    progressBar.Height = 30;
                    this.Controls.Add(progressBar);
                    await Task.Run(() => Cutter.ConvertToImg(dlg.FileNames[i], ref progressBar,ref time,ref label1));
                    await Task.Run(() => Cutter.GeneratePdf(ref progressBar,i.ToString(),ref time, ref label1,dlg.FileNames[i]));
                    this.Controls.Remove(progressBar);
                   // label2.Visible = false;
                }
            }
            label1.Text = "";
            label2.Text = "";
            button1.Enabled = true;
        }

        private void button1_DragEnter(object sender, DragEventArgs e)
        {

        }

        private async void button1_DragDrop(object sender, DragEventArgs e)
        {

        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private async void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] FileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            button1.Enabled = false;
            double time = 0;
            button1.Enabled = false;
            label2.Text = "Розраховується час виконання...";
            label2.Visible = true;
            for (int i = 0; i < FileNames.Length; i++)
            {
                time += TimeCalc.CalculateTimeForFiles(FileNames[i]);
                //Cutter.DeleteJpeg();
            }

            label1.Visible = true;
            //label2.Text = TimeCalc.MinuteSeconds(TimeCalc.CalculateTimeForFiles(dlg.FileNames[i])).ToString();

            for (int i = 0; i < FileNames.Length; i++)
            {
                label2.Text = $"Файл {i + 1} з {FileNames.Length}";

                //label2.Text = TimeCalc.TimeCutPage(dlg.FileNames[i]).ToString()+"\n";
                //label2.Text += TimeCalc.TimeGeneratePage()+"\n";
                ProgressBar progressBar = new ProgressBar();
                progressBar.Name = progressBar + i.ToString();
                progressBar.Location = new Point(12, 100);
                progressBar.Width = 284;
                progressBar.Height = 30;
                this.Controls.Add(progressBar);
                await Task.Run(() => Cutter.ConvertToImg(FileNames[i], ref progressBar, ref time, ref label1));
                await Task.Run(() => Cutter.GeneratePdf(ref progressBar, i.ToString(), ref time, ref label1, FileNames[i]));
                this.Controls.Remove(progressBar);
                // label2.Visible = false;
            }
            label1.Text = "";
            label2.Text = "";
            button1.Enabled = true;
        }

        private void Form1_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }
    }
    }

