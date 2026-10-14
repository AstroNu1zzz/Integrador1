using SQLite;
using Integrador1.Classes;

namespace Integrador1;

public partial class LoginPage : ContentPage
{

    private readonly SQLiteConnection _conexionDb;

    public LoginPage()
    {
        InitializeComponent();

        // Inicializar la conexión y crear las tablas
        string rutaDb = Path.Combine(FileSystem.AppDataDirectory, "localdrive.db3");
        _conexionDb = new SQLiteConnection(rutaDb);

        // Crea las tablas si no existen
        _conexionDb.CreateTable<Usuario>();
        _conexionDb.CreateTable<ArchiveDrive>(); // La clase de la respuesta anterior
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string usuario = TxtUsername.Text;
        string password = TxtPassword.Text;

        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Debes llenar todos los campos", "OK");
            return;
        }

        // Consultar si existe el usuario con esa contraseña
        var usuarioDb = _conexionDb.Table<Usuario>()
                                   .FirstOrDefault(u => u.Username == usuario && u.Password == password);

        if (usuarioDb != null)
        {
            // Limpiar cajas de texto
            TxtUsername.Text = string.Empty;
            TxtPassword.Text = string.Empty;

            // Navegar a MainPage
            await Navigation.PushAsync(new MainPage());
        }
        else
        {
            await DisplayAlert("Error", "Credenciales incorrectas", "OK");
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        string usuario = TxtUsername.Text;
        string password = TxtPassword.Text;

        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Debes llenar todos los campos", "OK");
            return;
        }

        // Validar que el nombre de usuario no esté repetido
        var existe = _conexionDb.Table<Usuario>().FirstOrDefault(u => u.Username == usuario);
        if (existe != null)
        {
            await DisplayAlert("Error", "Ese nombre de usuario ya existe", "OK");
            return;
        }

        // Insertar nuevo usuario
        var nuevoUsuario = new Usuario
        {
            Username = usuario,
            Password = password
        };

        _conexionDb.Insert(nuevoUsuario);

        await DisplayAlert("Éxito", "Cuenta creada. Ahora puedes ingresar.", "OK");
    }
}