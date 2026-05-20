using System;
using System.Windows.Forms;

namespace Blindrun1
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Показываем меню. Если нажали "Играть" — запускаем игру.
            using (var menu = new FormMenu())
            {
                if (menu.ShowDialog() != DialogResult.OK)
                    return; // нажали Выйти или закрыли окно
            }

            Application.Run(new Form1());
        }
    }
}