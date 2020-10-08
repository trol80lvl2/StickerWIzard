using PdfSharp.Drawing;
using PdfSharp.Pdf;
using SautinSoft;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StickerWizard
{

    public static class Cutter
    {
        private static string pathToCut ="PDF\\CUTTED\\";
        private static int x = 100, y = 150, width = 705, height = 360;
        private static int smCoef = 126;
        private static int widthA4 = smCoef * 21, heightA4 = Convert.ToInt32(smCoef * 29.7);
        private static Bitmap result = new Bitmap(widthA4, heightA4);
        private static SolidBrush whiteBrush = new SolidBrush(Color.White);
        private static Rectangle rectangle = new Rectangle(0, 0, widthA4, heightA4);
        private static Graphics g = Graphics.FromImage(result);

        private static void progress(ProgressBar progressBar)
        {
            progressBar.Invoke(new Action(() => progressBar.PerformStep()));
        }
        private static void setMaximumAndStep(ProgressBar progressBar,int value)
        {
            progressBar.Invoke(new Action(() => progressBar.Maximum=value));
            progressBar.Invoke(new Action(() => progressBar.Step = 1));
        }

        public static void ConvertToImg(string path, ref ProgressBar progressBar,ref double time,ref Label label)
        {
            //Stopwatch sw = new Stopwatch();
            // sw.Start();
            SautinSoft.PdfFocus f = new PdfFocus();
            f.OpenPdf(path);
            int pageCount = f.PageCount;
            string prefixFilePath = "PDF\\";
            f.ImageOptions.Dpi = 320;
            Stopwatch sw = new Stopwatch();
            Cutter.setMaximumAndStep(progressBar,f.PageCount * 2);

            for (int i = 1; i <= f.PageCount; i++)
            {
                sw.Start();
                f.ToImage(prefixFilePath + i.ToString() + ".jpeg", i);
                sw.Stop();
                time -= sw.ElapsedMilliseconds;
                TimeCalc.MinuteSeconds(time, label);
                sw.Reset();
            }

            f.ClosePdf();

            for (int i = 1; i <= pageCount; i++)
            {
                sw.Start();
                Cutter.Cut(prefixFilePath + i.ToString() + ".jpeg", i,progressBar);
                sw.Stop();
                time -= sw.ElapsedMilliseconds;
                TimeCalc.MinuteSeconds(time, label);
                sw.Reset();
            }
        }
        public static void Cut(string path, int name,ProgressBar progressBar)
        {
            x = 120;
            y = 150;
            Bitmap source = new Bitmap(path);
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
                pics[i] = Cutter.ResizeQR(pics[i], name.ToString(), i);
            }
            Cutter.GeneratePage(pics, name);
            Cutter.progress(progressBar);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public static Bitmap ResizeQR(Bitmap Label, string name, int j)
        {
            Bitmap result = new Bitmap(750, 410);
            Bitmap QR = Label.Clone(new System.Drawing.Rectangle(438, 0, 265, 350), Label.PixelFormat);
            Bitmap QRResize = new Bitmap(QR, new Size(Convert.ToInt32(QR.Width * 1.2), Convert.ToInt32(QR.Height * 1.2)));
            Graphics g = Graphics.FromImage(result);
            SolidBrush whiteBrush = new SolidBrush(Color.White);
            Rectangle rectangle = new Rectangle(0, 0, 750, 395);
            QRResize.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\zhopka\\" + name + ".jpg");
            g.FillRectangle(whiteBrush, rectangle);
            g.DrawImage(Label, 0, 5, Label.Width, Label.Height);
            //Console.WriteLine($"Width:{QRResize.Width},Height{QRResize.Height}");
            g.DrawImage(QRResize, 420, 0, QRResize.Width, QRResize.Height);
            return result;
            //  result.Save(pathToCut + "QR" + $"\\{name +  "-" + j.ToString() + ".jpeg"}");
            //QRResize.Save(pathToCut+ "QR" + $"\\{name}");
        }

        public static void GeneratePage(List<Bitmap> bitmaps, int name)
        {
            int counter = 0;
            
            int cellHeight = 360;
            int cellWidth = 705;
            int x = 0, y = 0;
            int stickerWidth = Convert.ToInt32(smCoef * 6.4), stickerHeight = Convert.ToInt32(smCoef * 3.2);
            
            //int marginX = (widthA4 - cellWidth) / 4;
            //int marginX = (stickerWidth - cellWidth) / 2;
            int marginX = smCoef - 20;
            int marginY = 80;
            
            g.FillRectangle(whiteBrush, rectangle);


            for (int i = 0; i < (bitmaps.Count / 3) + 1; i++)
            {
                for (int j = 0; j < 3 && counter < bitmaps.Count; j++)
                {

                    x = stickerWidth / 2 - cellWidth / 2;
                    y = stickerHeight / 2 - cellHeight / 2;
                    //if(x == 0 && y == 0)
                    //{
                    //    g.DrawImage(bitmaps[counter], stickerWidth * j + x, stickerHeight * i + y, cellWidth, cellHeight);
                    //    counter++;
                    //    continue;
                    //}
                    if (j % 3 == 0)
                    {
                        marginX = 0;
                    }
                    else if (j != 0 && j % 2 == 0)
                    {
                        marginX = 150;
                    }
                    else
                    {
                        marginX = smCoef - 33;
                    }
                    g.DrawImage(bitmaps[counter], stickerWidth * j + x + marginX, stickerHeight * i + y + marginY, cellWidth, cellHeight);
                    counter++;
                    //717
                    //  x += (int)6.7*smCoef;
                }
                // x = 120;
                // y += 366;
            }
            result.Save(pathToCut + "QR" + $"\\{name}" + ".jpeg");
        }


        public static bool IsEmpty(Bitmap sheet)
        {
            int counter = 0;
            for (int i = sheet.Width / 4; i < sheet.Width; i++)
            {
                for (int j = 0; j < sheet.Height / 4; j++)
                {
                    var color = sheet.GetPixel(i, j);
                    // Console.WriteLine(color.Name);
                    if (color.Name != "ffffffff")
                    {
                        counter++;
                        if (counter > 2000)
                        {
                            return false;
                        }
                        //  Console.WriteLine(counter);
                    }
                }
            }

            return true;
        }

        public static void GeneratePdf(ref ProgressBar progressBar,string name, ref double time, ref Label label,string fileName)
        {
            using (PdfDocument doc = new PdfDocument())
            {
                string folderPath = "PDF\\CUTTED\\QR\\";
                DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
                Stopwatch sw = new Stopwatch();
                foreach (var file in dirInfo.GetFiles("*.jpeg"))
                {
                    PdfPage page_ = null;
                    XImage img = null;
                    XGraphics graphics = null;
                    sw.Start();
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
                        progress(progressBar);

                    }
                    sw.Stop();
                    time -= sw.ElapsedMilliseconds;
                    TimeCalc.MinuteSeconds(time, label);
                    sw.Reset();
                }
                string[] fileNames = fileName.Split(new char[] { '\\' });
                doc.Save($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\PDF\\{fileNames[fileNames.Length-1]}");
            }
            Cutter.DeleteJpeg();
        }
        public static void DeleteJpeg()
        {
            string folderPath = "PDF\\CUTTED\\QR\\";
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
            foreach (var file in dirInfo.GetFiles("*.jpeg"))
            {
                file.Delete();
            }
            dirInfo = new DirectoryInfo("PDF\\");
            foreach (var file in dirInfo.GetFiles("*.jpeg"))
            {
                file.Delete();
            }
        }
    }
}
