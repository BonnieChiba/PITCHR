using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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

            // Identidad propia en la barra de tareas: así Windows no reutiliza el ícono viejo que tenía guardado
            try { SetCurrentProcessExplicitAppUserModelID("PITCHR.Aplicacion"); } catch (Exception) { }
            PonerIconoPitchr();

            // Animación de inicio con el logo (se salta con clic o cualquier tecla)
            using (PantallaInicio inicio = new PantallaInicio())
            {
                inicio.ShowDialog();
            }

            Application.Run(new HomePage());
        }

        private static Icon _icono;

        /// <summary>Ícono de PITCHR (círculo con la R). Lo usan todas las ventanas.</summary>
        public static Icon Icono
        {
            get
            {
                if (_icono != null) return _icono;
                try
                {
                    // 1) El que va dentro del programa (EmbeddedResource PITCHR.ico)
                    using (System.IO.Stream datos = Assembly.GetExecutingAssembly().GetManifestResourceStream("PITCHR.PITCHR.ico"))
                        if (datos != null) return _icono = new Icon(datos);

                    // 2) El ícono del .exe (ApplicationIcon del proyecto)
                    return _icono = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                }
                catch (Exception)
                {
                    return null;   // se queda el ícono normal de Windows
                }
            }
        }

        // ---------- Ícono en la barra de tareas ----------
        private const int WM_SETICON = 0x0080, ICON_SMALL = 0, ICON_BIG = 1;
        private const int SM_CXICON = 11, SM_CYICON = 12, SM_CXSMICON = 49, SM_CYSMICON = 50;
        private const int GCL_HICON = -14, GCL_HICONSM = -34;

        [DllImport("shell32.dll")]
        private static extern int SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string id);
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int indice);
        [DllImport("user32.dll", EntryPoint = "SetClassLongPtr")]
        private static extern IntPtr SetClassLongPtr64(IntPtr hWnd, int indice, IntPtr valor);
        [DllImport("user32.dll", EntryPoint = "SetClassLong")]
        private static extern int SetClassLong32(IntPtr hWnd, int indice, int valor);

        // Se guardan aquí para que el recolector de basura no libere los íconos mientras el programa está abierto
        private static Icon _iconoGrande, _iconoChico;

        /// <summary>
        /// Pone el círculo con la R en una ventana. Además de la propiedad Icon, se lo manda directo a Windows
        /// en el tamaño que pide la barra de tareas (según la escala de la pantalla).
        /// </summary>
        public static void PonerIcono(Form ventana)
        {
            if (Icono == null) return;
            ventana.Icon = Icono;
            ventana.HandleCreated += (s, e) =>
            {
                try
                {
                    if (_iconoGrande == null) _iconoGrande = new Icon(Icono, GetSystemMetrics(SM_CXICON), GetSystemMetrics(SM_CYICON));
                    if (_iconoChico == null) _iconoChico = new Icon(Icono, GetSystemMetrics(SM_CXSMICON), GetSystemMetrics(SM_CYSMICON));
                    IntPtr h = ventana.Handle;
                    SendMessage(h, WM_SETICON, (IntPtr)ICON_BIG, _iconoGrande.Handle);
                    SendMessage(h, WM_SETICON, (IntPtr)ICON_SMALL, _iconoChico.Handle);
                    if (IntPtr.Size == 8)
                    {
                        SetClassLongPtr64(h, GCL_HICON, _iconoGrande.Handle);
                        SetClassLongPtr64(h, GCL_HICONSM, _iconoChico.Handle);
                    }
                    else
                    {
                        SetClassLong32(h, GCL_HICON, _iconoGrande.Handle.ToInt32());
                        SetClassLong32(h, GCL_HICONSM, _iconoChico.Handle.ToInt32());
                    }
                }
                catch (Exception)
                {
                    // Si Windows no lo acepta se queda el de la propiedad Icon
                }
            };
        }

        /// <summary>
        /// También lo pone como ícono por defecto de Form, para las ventanas que no lo asignan.
        /// </summary>
        private static void PonerIconoPitchr()
        {
            try
            {
                if (Icono == null) return;
                FieldInfo porDefecto = typeof(Form).GetField("defaultIcon", BindingFlags.NonPublic | BindingFlags.Static);
                if (porDefecto != null) porDefecto.SetValue(null, Icono);
            }
            catch (Exception)
            {
                // Si algo falla se queda el ícono normal de Windows; el programa sigue igual
            }
        }
    }
}
