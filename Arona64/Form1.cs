using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Arona64.Win32GuiBinding;

namespace Arona64
{
    public partial class Form1 : Form
    {
        private WaveOutEvent waveOut;
        private BufferedWaveProvider buffer;
        private System.Threading.Timer timer;
        private Stopwatch stopwatch;
        private int sampleRate = 8000;
        private Func<int, byte> formulaFunc;
        public event Action<string> OnButtonSignal;
        public Form1()
        {
            InitializeComponent();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            int elapsedSamples = (int)(stopwatch.Elapsed.TotalSeconds * sampleRate);
            int bufferedSamples = buffer.BufferedBytes;
            int missing = elapsedSamples - bufferedSamples;

            if (missing <= 0 || missing > 8000) return;

            byte[] data = new byte[missing];

            for (int i = 0; i < missing; i++)
            {
                int t = elapsedSamples + i;
                data[i] = formulaFunc(t);
            }

            buffer.AddSamples(data, 0, data.Length);
        }

        private void GenerateBytebeat()
        {
            int durationSeconds = 10;
            int sampleRate = 8000;

            byte[] data = new byte[sampleRate * durationSeconds];
            for (int t = 0; t < data.Length; t++)
            {
                // Thay công thức bytebeat ở đây nếu muốn
                byte sample = (byte)((t * (t >> 5 | t >> 8)) & 0xFF);
                data[t] = sample;
            }

            buffer.AddSamples(data, 0, data.Length);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //this.Size = new Size(745, 445);
            this.MaximizeBox = false;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        /*private void button1_Click(object sender, EventArgs e)
        {
            // Gửi tín hiệu A rồi đóng cửa sổ
            OnButtonSignal?.Invoke("A");
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Gửi tín hiệu B và hiển thị MessageBox
            OnButtonSignal?.Invoke("B");
            MessageBox.Show("Bạn đã nhấn Button B");
        }*/

        private void DrawBouncingCircle(Graphics g, int width, int height)
        {
            int circleX = 100, circleY = 100;
            int dx = 5, dy = 3;
            int radius = 50;
            // Cập nhật vị trí
            circleX += dx;
            circleY += dy;

            // Va chạm biên -> đảo hướng
            if (circleX < 0 || circleX + radius > width)
                dx = -dx;
            if (circleY < 0 || circleY + radius > height)
                dy = -dy;

            // Vẽ hình tròn
            Brush brush = new SolidBrush(Color.FromArgb(255, 255, 0, 0)); // đỏ
            g.FillEllipse(brush, circleX, circleY, radius, radius);
        }
        private void PlayBytebeat()
        {
            /*int sampleRate = 22050;
            int durationSeconds = 250;
            var waveFormat = new WaveFormat(sampleRate, 8, 1);
            var buffer = new byte[sampleRate * durationSeconds];

            for (int t = 0; t < buffer.Length; t++)
            {
                byte value = (byte)(t >> 6 ^ t & t >> 9 ^ t >> 12 | (t << (t >> 6) % 4 ^ -t & -t >> 13) % 128 ^ -t >> 1);
                buffer[t] = value;
            }

            using (var ms = new RawSourceWaveStream(new System.IO.MemoryStream(buffer), waveFormat))
            using (var wo = new WaveOutEvent())
            {
                wo.Init(ms);
                wo.Play();

                while (wo.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(100);
                }
            }*/
            int sampleRate = 44100;
            int durationSeconds = 200;
            var waveFormat = new WaveFormat(sampleRate, 8, 1);
            var buffer = new byte[sampleRate * durationSeconds];

            Random rand = new Random();

            for (int t = 0; t < buffer.Length; t++)
            {
                double T = t / 5000.0;
                double b = Math.Abs((T % 32) - 16);

                double K = 1 + ((1 - ((int)(T) >> 5 & 3)) % 2 - 1) / 6.0;

                double part1 = (
                    Math.Pow(2, (new[] { -1, 4, 7 }[((int)(T * 4) % 3 | 0)] / 12.0 + K)) * t
                ) % 256;

                double part2 = (12 - T % 12) / 12.0;

                double part3 = (
                    Math.Pow(2, K + (int)(b / 5) - ((5514 >> ((int)b % ((int)T >> 9 != 0 ? 4 : 5) * 4)) & 15) / 12.0) * t
                ) % 256;

                double M = (((int)part1 & 128) * part2 + ((int)part3 & 128));

                double final = (
                    M / 3 + M / 5 +
                    (((int)T >> 7) != 0
                        ? (Math.Pow(2, (((((int)T & 4) != 0) ? ((int)T & 7) - 1 : 0) / 6.0 - 4)) * t % 64) * ((197 >> ((int)T % 8)) & 1)
                        : 0)
                    + rand.NextDouble() * 32 * (1 - ((T + 4) % 8) / 8.0)
                );

                buffer[t] = (byte)((int)final & 0xFF);
            }

            using (var ms = new RawSourceWaveStream(new System.IO.MemoryStream(buffer), waveFormat))
            using (var wo = new WaveOutEvent())
            {
                wo.Init(ms);
                wo.Play();

                while (wo.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(100);
                }
            }
        }

        private float hue = 0f;

        private void RunGdiEffects()
        {
            int w = Win32ApiBinding.GetSystemMetrics(0); // width
            int h = Win32ApiBinding.GetSystemMetrics(1); // height
            Random rand = new Random();

            // Danh sách hiệu ứng GDI
            Action<IntPtr, int, int> effect1 = (hdc, width, height) =>
            {
                Win32GuiBinding.BitBlt(hdc, 0, 0, width, height, hdc, 1, 1, Win32GuiBinding.SRCPAINT);
                Win32GuiBinding.StretchBlt(hdc, 30, 30, width, height, hdc, 3, 3, width, height, Win32GuiBinding.DSTINVERT);
            };

            Action<IntPtr, int, int> effect2 = (hdc, width, height) =>
            {
                Win32GuiBinding.PatBlt(hdc, rand.Next(width), rand.Next(height), 100, 100, Win32GuiBinding.PATINVERT);
            };

            Action<IntPtr, int, int> effect3 = (hdc, width, height) =>
            {
                Win32GuiBinding.StretchBlt(hdc, 10, 10, width - 20, height - 20, hdc, 0, 0, width, height, Win32GuiBinding.SRCCOPY);
                Win32GuiBinding.PatBlt(hdc, rand.Next(width), rand.Next(height), 100 * rand.Next(100), 100 * rand.Next(100), Win32GuiBinding.PATINVERT);
            };

            Action<IntPtr, int, int> effect4 = (hdc, width, height) =>
            {
                // Chụp toàn bộ màn hình vào memory DC
                /*IntPtr memDC = Win32GuiBinding.CreateCompatibleDC(hdc);
                IntPtr bmp = Win32GuiBinding.CreateCompatibleBitmap(hdc, width, height);
                IntPtr oldBmp = Win32GuiBinding.SelectObject(memDC, bmp);

                Win32GuiBinding.BitBlt(memDC, 0, 0, width, height, hdc, 0, 0, Win32GuiBinding.SRCCOPY);

                // Tạo mảng POINT cho PlgBlt (3 điểm của hình bình hành)
                Win32GuiBinding.POINT[] pts = new Win32GuiBinding.POINT[3];
                pts[0] = new Win32GuiBinding.POINT { X = 50, Y = 50 };                // Top-left
                pts[1] = new Win32GuiBinding.POINT { X = width - 50, Y = 20 };        // Top-right (nghiêng)
                pts[2] = new Win32GuiBinding.POINT { X = 20, Y = height - 50 };       // Bottom-left

                // Gọi PlgBlt để dán ảnh đã capture thành parallelogram
                Win32GuiBinding.PlgBlt(hdc, pts, memDC, 0, 0, width, height, IntPtr.Zero, 0, 0);

                // Giải phóng
                Win32GuiBinding.SelectObject(memDC, oldBmp);
                Win32GuiBinding.DeleteObject(bmp);*/
                // ===== Hiệu ứng nền giống effect6 =====
                // ===== Hiệu ứng nền giống effect6 =====
                Random rnd = new Random();

                // Lặp nhiều lần để vẽ nhiều logo ngẫu nhiên
                for (int i = 0; i < 20; i++) // 10 logo mỗi frame
                {
                    int rectW = 10;
                    int rectH = 10;
                    int rectX = rnd.Next(0, width - rectW);
                    int rectY = rnd.Next(0, height - rectH);

                    // Nền khung bằng PatBlt
                    IntPtr hBrush = Win32GuiBinding.CreateSolidBrush(rnd.Next(0x000000, 0xFFFFFF)); // màu random
                    IntPtr oldBrush = Win32GuiBinding.SelectObject(hdc, hBrush);

                    Win32GuiBinding.PatBlt(hdc, rectX, rectY, rectW, rectH, Win32GuiBinding.PATCOPY);

                    Win32GuiBinding.SelectObject(hdc, oldBrush);
                    Win32GuiBinding.DeleteObject(hBrush);

                    // Vẽ logo từ resource
                    using (Bitmap bmp = Properties.Resources.PatBlt)
                    {
                        IntPtr hdcMem = Win32GuiBinding.CreateCompatibleDC(hdc);
                        IntPtr hBmp = bmp.GetHbitmap();
                        Win32GuiBinding.SelectObject(hdcMem, hBmp);

                        Win32GuiBinding.BitBlt(
                            hdc,
                            rectX + (rectW - bmp.Width) / 2,
                            rectY + (rectH - bmp.Height) / 2,
                            bmp.Width,
                            bmp.Height,
                            hdcMem,
                            0,
                            0,
                            Win32GuiBinding.SRCCOPY
                        );

                        Win32GuiBinding.DeleteObject(hBmp);
                        Win32GuiBinding.DeleteDC(hdcMem);
                    }
                }
            };

            Action<IntPtr, int, int> effect5 = (hdc, width, height) =>
            {
                int size = 1000;
                Random rand1 = new Random();

                int x = rand.Next(0, width - 500 + size - 1) - size / 2;
                int y = rand.Next(0, height - 500 + size - 1) - size / 2;

                for (int i = 0; i < size; i += 100)
                {
                    int xi = (int)(x - i / 2.0);
                    int yi = (int)(y - i / 2.0);

                    IntPtr hrgn = Win32GuiBinding.CreateEllipticRgn(xi, yi, xi + i, yi + i);

                    Win32GuiBinding.SelectClipRgn(hdc, hrgn);

                    // Áp dụng hiệu ứng đảo ngược màu trong vùng ellip
                    Win32GuiBinding.BitBlt(hdc, xi, yi, i, i, hdc, xi, yi, Win32GuiBinding.NOTSRCCOPY);

                    // Xóa vùng clipping
                    Win32GuiBinding.SelectClipRgn(hdc, IntPtr.Zero);

                    Win32GuiBinding.DeleteObject(hrgn);
                }
            };

            Action<IntPtr, int, int> effect6 = (hdc, width, height) =>
            {
                double angle = 3;
                int scalingFactor = 5;

                for (int i = 0; i < width + height; i += scalingFactor)
                {
                    int offsetX = (int)(Math.Sin(angle) * 20 * scalingFactor);
                    Win32GuiBinding.BitBlt(hdc, 0, i, width, scalingFactor, hdc, offsetX, i, Win32GuiBinding.SRCCOPY);
                    angle += Math.PI / 40;
                }
            };

            Action<IntPtr, int, int> effect7 = (hdc, width, height) =>
            {
                Win32GuiBinding.POINT[] points = new Win32GuiBinding.POINT[4];
                for (int i = 0; i < 4; i++)
                {
                    points[i] = new Win32GuiBinding.POINT(rand.Next(width), rand.Next(height));
                }

                hue += 0.02f;
                if (hue > 1.0f) hue = 0f;

                Color rgb = ColorFromHSV(hue * 360, 1.0, 1.0);

                IntPtr hPen = Win32GuiBinding.CreatePen(0, 5, (uint)ColorTranslator.ToWin32(rgb));
                IntPtr old = Win32GuiBinding.SelectObject(hdc, hPen);

                Win32GuiBinding.PolyBezier(hdc, points, (uint)points.Length);

                Win32GuiBinding.SelectObject(hdc, old);
                Win32GuiBinding.DeleteObject(hPen);
                BitBlt(hdc, rand.Next(1, 10) % 2, rand.Next(1, 10) % 2, width, height, hdc, rand.Next(1, 1000) % 2, rand.Next(1, 1000) % 2,Win32GuiBinding.SRCPAINT);
            };

            // Thêm tất cả hiệu ứng vào danh sách
            var effects = new List<Action<IntPtr, int, int>> { effect1, effect2, effect3, effect4, effect5, effect6, effect7 };

            foreach (var effect in effects)
            {
                DateTime start = DateTime.Now;
                TimeSpan runFor = TimeSpan.FromSeconds(8); // mỗi payload 8s

                while (DateTime.Now - start < runFor)
                {
                    IntPtr hdc = Win32ApiBinding.GetDC(IntPtr.Zero);

                    effect(hdc, w, h);

                    Win32ApiBinding.ReleaseDC(IntPtr.Zero, hdc);
                    Thread.Sleep(30);
                }
            }

            // Sau khi chạy xong các hiệu ứng
            Invoke(new Action(() =>
            {
                Application.Exit(); // hoặc this.Close();
            }));
        }

        private Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            switch (hi)
            {
                case 0: return Color.FromArgb(255, v, t, p);
                case 1: return Color.FromArgb(255, q, v, p);
                case 2: return Color.FromArgb(255, p, v, t);
                case 3: return Color.FromArgb(255, p, q, v);
                case 4: return Color.FromArgb(255, t, p, v);
                default: return Color.FromArgb(255, v, p, q);
            }
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            this.Hide();
            var war1 = MessageBox.Show("You are running PmgDev64's malware and it can make your pc unbootable.\nPmgDev64 is not responsible for any damage. \nAre you sure to running this malware, the result is an unusable machine?\n(but dont worry, this version is harmless and not overwrite mbr, your data will not loss)", "Risk when running this malware", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (war1 == DialogResult.Cancel)
            {
                this.Close();
            }
            else if (war1 == DialogResult.OK)
            {
                // Thread phát Bytebeat
                Thread audioThread = new Thread(() =>
                {
                    PlayBytebeat();
                });
                audioThread.IsBackground = true;
                audioThread.Start();

                // Thread hiệu ứng GDI
                Thread gdiThread = new Thread(() =>
                {
                    RunGdiEffects();
                });
                gdiThread.IsBackground = true;
                gdiThread.Start();
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
