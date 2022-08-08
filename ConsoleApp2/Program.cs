using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputSimulator = new InputSimulator();
            Random rnd = new Random();

            Console.WriteLine("wait 5s to take baseline...");
            Thread.Sleep(5000);
            CaptureMyScreen(@"d:\baseline.png");
            Console.WriteLine("wait another 5s to start the loop...");
            Thread.Sleep(5000);
            while (true)
            {
                inputSimulator.Keyboard.KeyDown(VirtualKeyCode.VK_K);
                int timetopress = rnd.Next(10, 50);
                Thread.Sleep(timetopress);
                inputSimulator.Keyboard.KeyUp(VirtualKeyCode.VK_K);

                Thread.Sleep(2000);
                CaptureMyScreen(@"d:\tmp.png");
                var kv = CompareAndFind(@"d:\baseline.png", @"d:\tmp.png");

                inputSimulator.Mouse.MoveMouseTo((1300.0 + kv.Key) / 3440 * 65535, (100.0 + kv.Value)/ 1440 * 65535);

                var starttime = DateTime.UtcNow;
                CaptureMyScreenSmall(@"d:\tmp1.png", kv.Key, kv.Value);
                double totalScore = 0;
                double baselineScore = 0;
                double lastScore = 0;
                int baselineCnt = 0;
                int cnt = 0;
                while (true)
                {
                    var currenttime = DateTime.UtcNow;
                    if (currenttime - starttime > new TimeSpan(0, 0, 20)) break;
                    CaptureMyScreenSmall(@"d:\tmp2.png", kv.Key, kv.Value);
                    cnt++;
                    var score = CompareAndFindSmall(@"d:\tmp1.png", @"d:\tmp2.png");

                    if (cnt > 10 && cnt <=30)
                    {
                        totalScore += score;
                        baselineCnt++;
                        baselineScore = totalScore / baselineCnt;
                    }
                    
                    if (cnt >30 && lastScore + score > baselineScore * 4)
                    {
                        // click
                        inputSimulator.Mouse.RightButtonDown();
                        timetopress = rnd.Next(10, 50);
                        Thread.Sleep(timetopress);
                        inputSimulator.Mouse.RightButtonUp();
                        Thread.Sleep(3000);
                        break;
                    }

                    Thread.Sleep(100);
                    cnt++;
                    lastScore = score;
                }
            }

            Console.ReadLine();
        }

        private static double CompareAndFindSmall(string filename1, string filename2)
        {
            try
            {
                double total = 0;
                using (Bitmap baseline = new Bitmap(filename1))
                {
                    using (Bitmap tmp = new Bitmap(filename2))
                    {
                        for (int i = 0; i < 100; i++)
                            for (int j = 0; j < 100; j++)
                            {
                                total += Math.Pow(tmp.GetPixel(i, j).R - baseline.GetPixel(i, j).R, 2) + Math.Pow(tmp.GetPixel(i, j).G - baseline.GetPixel(i, j).G,2);
                            }
                    }
                }

                Console.WriteLine(total);
                return total;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return 0;
        }

        private static KeyValuePair<int,int> CompareAndFind(string filename1, string filename2)
        {
            try
            {
                using (Bitmap baseline = new Bitmap(filename1))
                {
                    using (Bitmap tmp = new Bitmap(filename2))
                    {
                        int maxi=0, maxj=0, maxdelta = 0;

                        for (int i=0;i<800;i++)
                            for (int j=0;j<200;j++)
                            {
                                if (tmp.GetPixel(i, j).R > baseline.GetPixel(i, j).R && tmp.GetPixel(i, j).R - baseline.GetPixel(i, j).R > maxdelta)
                                {
                                    maxdelta = tmp.GetPixel(i, j).R - baseline.GetPixel(i, j).R;
                                    maxi = i;maxj = j;
                                }
                            }

                        return new KeyValuePair<int, int>(maxi, maxj);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return new KeyValuePair<int, int>(0, 0);
        }

        private static void CaptureMyScreen(string filename)
        {
            try
            {
                //Creating a new Bitmap object
                Bitmap captureBitmap = new Bitmap(800, 200, PixelFormat.Format32bppArgb);
                //Bitmap captureBitmap = new Bitmap(int width, int height, PixelFormat);
                //Creating a Rectangle object which will
                //capture our Current Screen
                Rectangle captureRectangle = new Rectangle(1300, 100, 800, 200);
                //Creating a New Graphics Object
                Graphics captureGraphics = Graphics.FromImage(captureBitmap);
                //Copying Image from The Screen
                captureGraphics.CopyFromScreen(captureRectangle.Left, captureRectangle.Top, 0, 0, captureRectangle.Size);
                //Saving the Image File (I am here Saving it in My E drive).
                captureBitmap.Save(filename, ImageFormat.Png);
                //Displaying the Successfull Result

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void CaptureMyScreenSmall(string filename, int x, int y)
        {
            try
            {
                //Creating a new Bitmap object
                Bitmap captureBitmap = new Bitmap(100, 100, PixelFormat.Format32bppArgb);
                //Bitmap captureBitmap = new Bitmap(int width, int height, PixelFormat);
                //Creating a Rectangle object which will
                //capture our Current Screen
                Rectangle captureRectangle = new Rectangle(1300 + x - 50, 100 + y - 50, 100, 100);
                //Creating a New Graphics Object
                Graphics captureGraphics = Graphics.FromImage(captureBitmap);
                //Copying Image from The Screen
                captureGraphics.CopyFromScreen(captureRectangle.Left, captureRectangle.Top, 0, 0, captureRectangle.Size);
                //Saving the Image File (I am here Saving it in My E drive).
                captureBitmap.Save(filename, ImageFormat.Png);
                //Displaying the Successfull Result

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
