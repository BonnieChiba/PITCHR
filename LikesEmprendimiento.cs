using MySqlConnector;
using System;
using System.Collections.Generic;

namespace PITCHR
{
    /// <summary>
    /// Likes de los emprendimientos, compartidos por la tarjeta (SeccionEmprendimientos) y la página del
    /// emprendimiento (SeccionProducto). Así el like que das en un lado se ve igual en el otro.
    /// Tabla integrative.entrepreneurshipLike: un renglón por cada like (emprendimiento + persona + rol).
    /// La columna likes de entrepreneurship guarda el total.
    /// </summary>
    public static class LikesEmprendimiento
    {
        /// <summary>Ids de los emprendimientos a los que la persona que inició sesión ya les dio like.</summary>
        public static HashSet<int> MisLikes()
        {
            HashSet<int> likes = new HashSet<int>();
            if (!Sesion.Activa) return likes;
            using (DBConn dbTemp = new DBConn())
            {
                MySqlCommand buscar = new MySqlCommand(
                    "SELECT idEntrepreneurship FROM integrative.entrepreneurshipLike WHERE idUser = @yo AND role = @rol", dbTemp.dbConnection);
                buscar.Parameters.AddWithValue("@yo", Sesion.IdUsuario);
                buscar.Parameters.AddWithValue("@rol", Sesion.Rol);
                dbTemp.dbConnection.Open();
                using (MySqlDataReader lector = buscar.ExecuteReader())
                {
                    while (lector.Read()) likes.Add(Convert.ToInt32(lector["idEntrepreneurship"]));
                }
            }
            return likes;
        }

        /// <summary>true si la persona que inició sesión ya le dio like a este emprendimiento.</summary>
        public static bool TieneLike(int idEmprendimiento)
        {
            if (!Sesion.Activa) return false;
            using (DBConn dbTemp = new DBConn())
            {
                MySqlCommand buscar = new MySqlCommand(
                    "SELECT COUNT(*) FROM integrative.entrepreneurshipLike WHERE idEntrepreneurship = @id AND idUser = @yo AND role = @rol",
                    dbTemp.dbConnection);
                buscar.Parameters.AddWithValue("@id", idEmprendimiento);
                buscar.Parameters.AddWithValue("@yo", Sesion.IdUsuario);
                buscar.Parameters.AddWithValue("@rol", Sesion.Rol);
                dbTemp.dbConnection.Open();
                return Convert.ToInt32(buscar.ExecuteScalar()) > 0;
            }
        }

        /// <summary>Sin sesión iniciada no se puede dar like: muestra el aviso y regresa false.</summary>
        public static bool PuedeDarLike()
        {
            if (Sesion.Activa) return true;
            MessageBox.Show("Inicia sesión para darle like a un emprendimiento.", "PitchR");   // con la "i", igual que los demás avisos de iniciar sesión
            return false;
        }

        /// <summary>
        /// Si no le había dado like lo agrega (INSERT); si ya le había dado lo quita (DELETE).
        /// Actualiza el total en entrepreneurship.likes y lo regresa.
        /// </summary>
        public static int Alternar(int idEmprendimiento, bool yaTeniaLike)
        {
            using (DBConn dbTemp = new DBConn())
            {
                dbTemp.dbConnection.Open();

                MySqlCommand cambiar = new MySqlCommand(yaTeniaLike
                    ? "DELETE FROM integrative.entrepreneurshipLike WHERE idEntrepreneurship = @id AND idUser = @yo AND role = @rol"
                    : "INSERT IGNORE INTO integrative.entrepreneurshipLike (idEntrepreneurship, idUser, role) VALUES (@id, @yo, @rol)",
                    dbTemp.dbConnection);
                cambiar.Parameters.AddWithValue("@id", idEmprendimiento);
                cambiar.Parameters.AddWithValue("@yo", Sesion.IdUsuario);
                cambiar.Parameters.AddWithValue("@rol", Sesion.Rol);
                cambiar.ExecuteNonQuery();

                MySqlCommand total = new MySqlCommand(
                    "UPDATE integrative.entrepreneurship SET likes = " +
                    "(SELECT COUNT(*) FROM integrative.entrepreneurshipLike WHERE idEntrepreneurship = @id) " +
                    "WHERE idEntrepreneurship = @id", dbTemp.dbConnection);
                total.Parameters.AddWithValue("@id", idEmprendimiento);
                total.ExecuteNonQuery();

                MySqlCommand leer = new MySqlCommand(
                    "SELECT likes FROM integrative.entrepreneurship WHERE idEntrepreneurship = @id", dbTemp.dbConnection);
                leer.Parameters.AddWithValue("@id", idEmprendimiento);
                return Convert.ToInt32(leer.ExecuteScalar());
            }
        }
    }
}
