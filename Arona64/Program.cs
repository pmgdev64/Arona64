using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Arona64
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 f1 = new Form1();
            f1.OnButtonSignal += (signal) =>
            {
                if (signal == "A")
                {
                    MessageBox.Show("Tín hiệu A: đóng app");
                }
                else if (signal == "B")
                {
                    MessageBox.Show("Tín hiệu B: nút thứ 2 đã được nhấn");
                }
            };
            Application.Run(f1);
        }
    }
}
