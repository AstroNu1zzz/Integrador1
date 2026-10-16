using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integrador1.Classes
{
    internal class Usuario
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PalabraRecuperacion { get; set; } = string.Empty; 
    }
}
