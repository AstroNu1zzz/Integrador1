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
        public string Nombre { get; set; } 
        public string Tipo { get; set; } 
        public byte[] DatosMultimedia { get; set; } 
    }
}
