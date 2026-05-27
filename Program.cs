using System;
using System.Windows.Forms;

namespace Blindrun1
{
    public enum Difficulty { Easy, Medium, Hard }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var menu = new FormMenu())
            {
                if (menu.ShowDialog() != DialogResult.OK)
                    return;

                Application.Run(new Form1(menu.SelectedDifficulty));
            }
        }
    }
}