using CommunityToolkit.Maui.Views;
using Integrador1.Classes;
using SQLite;

namespace Integrador1
{
    public partial class MainPage : ContentPage
    {
        private readonly SQLiteConnection _conexionDb;
        private readonly int _usuarioIdActual;

        public MainPage(int usuarioId)
        {
            InitializeComponent();
            _usuarioIdActual = usuarioId;

            // Inicializar conexión a la misma base de datos usada en el Login
            string rutaDb = Path.Combine(FileSystem.AppDataDirectory, "localdrive.db3");
            _conexionDb = new SQLiteConnection(rutaDb);

            // Cargar los archivos guardados al abrir la página
            CargarArchivos();
        }

        private void CargarArchivos()
        {
            var archivos = _conexionDb.Table<ArchiveDrive>()
                               .Where(a => a.UsuarioId == _usuarioIdActual)
                               .ToList();
            ListaArchivos.ItemsSource = archivos;
        }

        private void OnArchivoSeleccionado(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ArchiveDrive archivoSeleccionado)
            {
                ReproductorMultimedia.Stop();
                // Ocultar ambos controles primero
                VisualizadorImagen.IsVisible = false;
                ReproductorMultimedia.IsVisible = false;
                LblPlaceholder.IsVisible = false;
                VisualizadorTexto.IsVisible = false;

                if (archivoSeleccionado.Tipo == "Imagen")
                {
                    VisualizadorImagen.Source = ImageSource.FromStream(() => new MemoryStream(archivoSeleccionado.DatosMultimedia));
                    VisualizadorImagen.IsVisible = true;
                }
                else if (archivoSeleccionado.Tipo == "Video" || archivoSeleccionado.Tipo == "Audio")
                {
                    string rutaTemporal = Path.Combine(FileSystem.CacheDirectory, archivoSeleccionado.Nombre);

                    File.WriteAllBytes(rutaTemporal, archivoSeleccionado.DatosMultimedia);

                    ReproductorMultimedia.Source = MediaSource.FromFile(rutaTemporal);
                    ReproductorMultimedia.IsVisible = true;
                    ReproductorMultimedia.Play();
                }
                else if (archivoSeleccionado.Tipo == "Texto")
                {
                    // Convertir los bytes guardados en SQLite nuevamente a string
                    string texto = System.Text.Encoding.UTF8.GetString(archivoSeleccionado.DatosMultimedia);
                    LblContenidoTexto.Text = texto;
                    VisualizadorTexto.IsVisible = true;
                }
            }
        }

        private async void OnUploadClicked(object sender, EventArgs e)
        {
            var resultado = await FilePicker.Default.PickAsync();

            if (resultado != null)
            {
                using var stream = await resultado.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                byte[] archivoEnBytes = memoryStream.ToArray();

                string tipo = "Desconocido";
                if (resultado.FileName.EndsWith(".jpg") || resultado.FileName.EndsWith(".png")) tipo = "Imagen";
                else if (resultado.FileName.EndsWith(".mp4")) tipo = "Video";
                else if (resultado.FileName.EndsWith(".mp3")) tipo = "Audio";
                else if (resultado.FileName.EndsWith(".txt")) tipo = "Texto";

                var nuevoArchivo = new ArchiveDrive
                {
                    UsuarioId = _usuarioIdActual, // Asigna el dueño al archivo
                    Nombre = resultado.FileName,
                    Tipo = tipo,
                    DatosMultimedia = archivoEnBytes
                };

                // Crear (Create) el registro en la base de datos
                _conexionDb.Insert(nuevoArchivo);

                // Actualizar la lista visual
                CargarArchivos();
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            // Obtener el botón que fue presionado
            var button = sender as ImageButton;

            // Extraer el ID que pasamos en el CommandParameter desde el XAML
            if (button?.CommandParameter is int id)
            {
                bool confirmar = await DisplayAlert("Eliminar", "¿Estás seguro de que deseas eliminar este archivo?", "Sí", "No");

                if (confirmar)
                {
                    // Eliminar (Delete) de la base de datos mediante el ID
                    _conexionDb.Delete<ArchiveDrive>(id);

                    // Resetear la vista por si el archivo eliminado estaba reproduciéndose
                    VisualizadorImagen.IsVisible = false;
                    ReproductorMultimedia.IsVisible = false;
                    VisualizadorTexto.IsVisible = false;
                    ReproductorMultimedia.Stop();
                    LblPlaceholder.IsVisible = true;

                    // Actualizar la lista visual
                    CargarArchivos();
                }
            }
        }
        private async void OnRenameClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is int id)
            {
                // Buscar el archivo en la BD
                var archivo = _conexionDb.Table<ArchiveDrive>().FirstOrDefault(a => a.Id == id);

                if (archivo != null)
                {
                    // Mostrar ventana emergente para escribir el nuevo nombre
                    string nuevoNombre = await DisplayPromptAsync("Renombrar Archivo", "Ingresa el nuevo nombre:", "Guardar", "Cancelar", archivo.Nombre, -1, Keyboard.Text, archivo.Nombre);

                    if (!string.IsNullOrWhiteSpace(nuevoNombre) && nuevoNombre != archivo.Nombre)
                    {
                        // Asegurar que mantenga la misma extensión original (ej. .mp4)
                        string extensionOriginal = Path.GetExtension(archivo.Nombre);
                        string extensionNueva = Path.GetExtension(nuevoNombre);

                        if (string.IsNullOrEmpty(extensionNueva) || extensionNueva != extensionOriginal)
                        {
                            nuevoNombre += extensionOriginal;
                        }

                        // Actualizar (Update) en la base de datos
                        archivo.Nombre = nuevoNombre;
                        _conexionDb.Update(archivo);

                        // Refrescar la vista
                        CargarArchivos();
                    }
                }
            }
        }

        private async void OnDownloadClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is int id)
            {
                var archivo = _conexionDb.Table<ArchiveDrive>().FirstOrDefault(a => a.Id == id);

                if (archivo != null)
                {
                    try
                    {
                        string rutaDestino = "";

#if ANDROID
                        // Llama al sistema nativo de Android para obtener la ruta pública de Descargas
                        var carpetaDescargas = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
                        rutaDestino = Path.Combine(carpetaDescargas, archivo.Nombre);
                        File.WriteAllBytes(rutaDestino, archivo.DatosMultimedia);
#elif WINDOWS
                // Alternativa por si ejecutas el emulador local de Windows
                var carpetaDescargas = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                rutaDestino = Path.Combine(carpetaDescargas, archivo.Nombre);
                File.WriteAllBytes(rutaDestino, archivo.DatosMultimedia);
#endif

                        await DisplayAlert("Éxito", $"Archivo descargado en:\n{rutaDestino}", "OK");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        await DisplayAlert("Error de permisos", "El sistema bloqueó la escritura. Asegúrate de tener los permisos de almacenamiento habilitados.", "OK");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", $"No se pudo descargar: {ex.Message}", "OK");
                    }
                }
            }
        }
    }
}