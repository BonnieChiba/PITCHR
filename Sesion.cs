namespace PITCHR
{
    /// <summary>
    /// Datos de la persona que inició sesión. Se llenan en IniciarSesion y cualquier pantalla puede leerlos
    /// (por ejemplo, para mostrar la foto de perfil en lugar del botón "Iniciar Sesión").
    /// </summary>
    public static class Sesion
    {
        public static int IdUsuario { get; private set; }
        public static string Usuario { get; private set; }
        public static int Rol { get; private set; }          // 1 = emprendedor, 0 = inversor
        public static bool Activa { get; private set; }

        public static void Iniciar(int idUsuario, string usuario, int rol)
        {
            IdUsuario = idUsuario;
            Usuario = usuario;
            Rol = rol;
            Activa = true;
        }

        public static void Cerrar()
        {
            IdUsuario = 0;
            Usuario = null;
            Rol = 0;
            Activa = false;
        }
    }
}
