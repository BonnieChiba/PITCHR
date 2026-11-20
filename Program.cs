using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PITCHR
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Animación de inicio con el logo (se salta con clic o cualquier tecla)
            using (PantallaInicio inicio = new PantallaInicio())
            {
                inicio.ShowDialog();
            }

            Application.Run(new HomePage());
        }
    }
}
