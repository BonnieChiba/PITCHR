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

        // Cuenta que se acaba de crear (CrearCuenta la guarda). Cuando esa misma cuenta inicia sesión
        // por primera vez, se le manda directo a editar su perfil.
        private static int _nuevaId = -1;
        private static int _nuevaRol = -1;

        public static void GuardarCuentaNueva(int idUsuario, int rol)
        {
            _nuevaId = idUsuario;
            _nuevaRol = rol;
        }

        // true si la sesión actual es de la cuenta recién creada (solo la primera vez)
        public static bool EsPrimeraVez()
        {
            bool primeraVez = Activa && IdUsuario == _nuevaId && Rol == _nuevaRol;
            if (primeraVez) { _nuevaId = -1; _nuevaRol = -1; }
            return primeraVez;
        }

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
