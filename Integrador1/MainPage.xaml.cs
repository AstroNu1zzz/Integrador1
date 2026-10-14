using CommunityToolkit.Maui.Views;
using Integrador1.Classes;
using SQLite;

namespace Integrador1
{
    public partial class MainPage : ContentPage
    {
        private readonly SQLiteConnection _conexionDb;

        public MainPage()
        {
            InitializeComponent();

            // Inicializar conexión a la misma base de datos usada en el Login
            string rutaDb = Path.Combine(FileSystem.AppDataDirectory, "localdrive.db3");
            _conexionDb = new SQLiteConnection(rutaDb);

            // Cargar los archivos guardados al abrir la página
            CargarArchivos();
        }

        private void CargarArchivos()
        {
            // Leer (Read) todos los archivos de la base de datos y mostrarlos en el CollectionView
            var archivos = _conexionDb.Table<ArchiveDrive>().ToList();
            ListaArchivos.ItemsSource = archivos;
        }

        private void OnArchivoSeleccionado(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ArchiveDrive archivoSeleccionado)
            {
                // Ocultar ambos controles primero
                VisualizadorImagen.IsVisible = false;
                ReproductorMultimedia.IsVisible = false;
                LblPlaceholder.IsVisible = false;

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

                var nuevoArchivo = new ArchiveDrive
                {
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
                    ReproductorMultimedia.Stop();
                    LblPlaceholder.IsVisible = true;

                    // Actualizar la lista visual
                    CargarArchivos();
                }
            }
        }
    }
}