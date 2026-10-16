using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integrador1.Classes
{
    internal class ArchiveDrive
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Nombre { get; set; } 
        public string Tipo { get; set; } 
        public byte[] DatosMultimedia { get; set; }

        [Ignore]
        public string IconoArchivo
        {
            get
            {
                if (Tipo == "Audio") return "volumen.png";
                if (Tipo == "Video") return "video.png"; // Sustituye con el nombre de tu imagen de video
                if (Tipo == "Imagen") return "imagen.png"; // Sustituye con el nombre de tu imagen
                if (Tipo == "Texto") return "texto.png"; // Sustituye con el nombre de tu documento
                return "dotnet_bot.png";
            }
        }
    }
}
