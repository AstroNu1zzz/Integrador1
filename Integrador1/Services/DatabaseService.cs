using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Integrador1.Classes; // Agregado

namespace Integrador1.Services
{
    internal class DatabaseService
    {
        SQLiteAsyncConnection db;
        async Task Init()
        {
            if (db != null)
                return;
            string ruta = Path.Combine(FileSystem.AppDataDirectory, "estudiantes.db3");
            db = new SQLiteAsyncConnection(ruta);
            await db.CreateTableAsync<ArchiveDrive>(); // Corregido el nombre
        }

        public async Task<List<ArchiveDrive>> ObtenerTodos() // Corregido a ArchiveDrive
        {
            await Init();
            return await db.Table<ArchiveDrive>().ToListAsync();
        }

        public async Task Guardar(ArchiveDrive archivo) // Corregido
        {
            await Init();
            if (archivo.Id != 0)
                await db.UpdateAsync(archivo);
            else
                await db.InsertAsync(archivo);
        }

        public async Task Eliminar(ArchiveDrive archivo) // Corregido
        {
            await Init();
            await db.DeleteAsync(archivo);
        }
        public async Task<List<ArchiveDrive>> BuscarPorNombre (string texto, int ID)
        {
            await Init();

            return await db.Table<ArchiveDrive>()
                           .Where(a => a.UsuarioId == ID && a.Nombre.Contains(texto))
                              .ToListAsync();
        }

    }
}