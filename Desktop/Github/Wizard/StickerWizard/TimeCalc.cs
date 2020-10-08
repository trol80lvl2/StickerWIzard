using PdfSharp.Drawing;
using PdfSharp.Pdf;
using SautinSoft;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StickerWizard
{
    public static class TimeCalc
    {
        private static long timeForPages { get; set; }
        private static long timeForJpeg { get; set; }
        private static long timeForPdf { get; set; }
        private static int pages { get; set; }
        private static long timeLeft { get; set; }
        private static string prefixFilePath = "PDF\\1.jpeg";
        private static Stopwatch sw = new Stopwatch();

        private static long TimeCutPage(string fileName)
        {
            SautinSoft.PdfFocus f = new PdfFocus();
            long timeFor1 = 0;
            long timeFor2 = 0;
            f.OpenPdf(fileName);
            f.ImageOptions.Dpi = 320;
            pages = f.PageCount;
            sw.Start();
            f.ToImage(prefixFilePath, 1);
            sw.Stop();
            timeFor1 = sw.ElapsedMilliseconds;
            sw.Start();
            f.ToImage(prefixFilePath, 2);
            sw.Stop();
            timeFor2 = sw.ElapsedMilliseconds;
            long allTime = timeFor1 + ((timeFor2 - timeFor1) * (f.PageCount - 1));
            f.ClosePdf();
            timeForJpeg = allTime;
            sw.Reset();
            return timeForJpeg;
        }
        private static long TimeGeneratePage()
        {
            sw.Start();
            int x = 120, y = 150, width = 705, height = 360;
            Bitmap source = new Bitmap(prefixFilePath);
            List<Bitmap> pics = new List<Bitmap>();
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Bitmap CroppedImage = source.Clone(new System.Drawing.Rectangle(x, y, width, height), source.PixelFormat);
                    if (Cutter.IsEmpty(CroppedImage))
                        break;
                    pics.Add(CroppedImage);
                    x += 717;
                }
                x = 120;
                y += 366;
            }
            for (int i = 0; i < pics.Count - 1; i++)
            {
                pics[i] = Cutter.ResizeQR(pics[i], "1", i);
            }
            Cutter.GeneratePage(pics, 1);
            sw.Stop();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            timeForPages = sw.ElapsedMilliseconds;
            sw.Reset();
            return timeForPages*pages;
        }
        private static long TimeGeneratePdf()
        {
            using (PdfDocument doc = new PdfDocument())
            {
                string folderPath = "PDF\\CUTTED\\QR\\";
                DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
                sw.Start();
                foreach (var file in dirInfo.GetFiles("*.jpeg"))
                {
                    PdfPage page_ = null;
                    XImage img = null;
                    XGraphics graphics = null;
                    try
                    {
                        page_ = doc.AddPage();
                        img = XImage.FromFile(file.FullName.ToString());
                        graphics = XGraphics.FromPdfPage(page_);
                        graphics.DrawImage(img, 0, 0, (int)page_.Width, (int)page_.Height);
                    }
                    finally
                    {
                        page_.Close();
                        img.Dispose();
                        graphics.Dispose();
                    }
                }
                sw.Stop();
            }
            timeForPdf = sw.ElapsedMilliseconds * pages;
            return timeForPdf;
        }
        public static double CalculateTimeForFiles(string filePath)
        {
            double time = 0;
            time += TimeCalc.TimeCutPage(filePath);
            time += TimeCalc.TimeGeneratePage();
            time += TimeCalc.TimeGeneratePdf();
            return (time)/1.065;
        }
        public static void MinuteSeconds(double time,Label label)
        {
            time /= 1000;
            if (time < 15)
            {
                label.Invoke(new Action(() => label.Text = "Майже готово..."));
            }
            else if(time >= 15 && time < 30){
                label.Invoke(new Action(() => label.Text = "Менше 30с..."));
            }
            else if(time >= 30 && time < 60)
            {
                label.Invoke(new Action(() => label.Text = "Менше 60с..."));
            }
            else if(time>=60)
            {
                int minutes = Convert.ToInt32(Math.Truncate(time / 60));
                double seconds = Convert.ToInt32(time % 60);
                //запис в лейбел
                label.Invoke(new Action(() => label.Text = "Приблизно " + minutes.ToString() + " хв " + seconds.ToString()+" с"));
            }
        }

    }
}
