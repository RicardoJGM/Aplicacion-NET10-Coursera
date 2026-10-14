using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

const string ApiToken = "TechHive2026";
var usuarios = new List<Usuario>
{
    new Usuario
    {
        Id = 1,
        Nombre = "Juan Pérez",
        Correo = "juan@correo.com"
    },
    new Usuario
    {
        Id = 2,
        Nombre = "Ana López",
        Correo = "ana@correo.com"
    }
};
// Middlewares 

//Middleware para control de errores global
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"ERROR: {ex.Message}");

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            Mensaje = "Ha ocurrido un error interno.",
            Error = ex.Message
        });
    }
});

//Middleware para acceso por token
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/usuarios"))
    {
        if (!context.Request.Headers.TryGetValue(
            "Authorization",
            out var token))
        {
            context.Response.StatusCode = 401;

            await context.Response.WriteAsJsonAsync(new
            {
                Mensaje = "Token no proporcionado."
            });

            return;
        }

        if (token != $"Bearer {ApiToken}")
        {
            context.Response.StatusCode = 401;

            await context.Response.WriteAsJsonAsync(new
            {
                Mensaje = "Token inválido."
            });

            return;
        }
    }

    await next();
});

// Middleware para logging
app.Use(async (context, next) =>
{
    Console.WriteLine(
        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] " +
        $"Solicitud: {context.Request.Method} {context.Request.Path}");

    await next();

    Console.WriteLine(
        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] " +
        $"Respuesta: {context.Response.StatusCode}");
});

//Endpoints
app.MapGet("/", () => "Bienvenido al sistema de TechHive Solutions!");

// GET - Obtener todos los usuarios
app.MapGet("/usuarios", () =>
{
    return Results.Ok(usuarios);
});

// GET - Obtener usuario por Id
app.MapGet("/usuarios/{id:int}", (int id) =>
{
    var usuario = usuarios.FirstOrDefault(x => x.Id == id);

    if (usuario is null)
    {
        return Results.NotFound(new
        {
            Mensaje = $"No existe un usuario con Id {id}"
        });
    }

    return Results.Ok(usuario);
});


// POST - Crear usuario
app.MapPost("/usuarios", (Usuario nuevoUsuario) =>
{
    if (string.IsNullOrWhiteSpace(nuevoUsuario.Nombre))
        return Results.BadRequest("El nombre es obligatorio.");

    if (string.IsNullOrWhiteSpace(nuevoUsuario.Correo))
        return Results.BadRequest("El correo es obligatorio.");

    if (!nuevoUsuario.Correo.Contains("@"))
        return Results.BadRequest("El correo proporcionado no es válido");

    if (usuarios.Any(x =>
        x.Correo.Equals(nuevoUsuario.Correo,
            StringComparison.OrdinalIgnoreCase)))
    {
        return Results.BadRequest(
            "Ya existe un usuario con ese correo.");
    }

    nuevoUsuario.Id = usuarios.Count == 0
        ? 1
        : usuarios.Max(x => x.Id) + 1;

    usuarios.Add(nuevoUsuario);

    return Results.Created(
        $"/usuarios/{nuevoUsuario.Id}",
        nuevoUsuario);
});

// PUT - Actualizar usuario
app.MapPut("/usuarios/{id:int}", (int id, Usuario usuarioActualizado) =>
{
    var usuario = usuarios.FirstOrDefault(x => x.Id == id);

    if (usuario is null)
        return Results.NotFound($"Usuario con Id {id} no encontrado");

    if (string.IsNullOrWhiteSpace(usuarioActualizado.Nombre))
        return Results.BadRequest("El nombre es obligatorio.");

    if (string.IsNullOrWhiteSpace(usuarioActualizado.Correo))
        return Results.BadRequest("El correo es obligatorio.");

    usuario.Nombre = usuarioActualizado.Nombre;
    usuario.Correo = usuarioActualizado.Correo;

    return Results.Ok(usuario);   
});

// DELETE - Eliminar usuario
app.MapDelete("/usuarios/{id:int}", (int id) =>
{
    var usuario = usuarios.FirstOrDefault(x => x.Id == id);

    if (usuario is null)
        return Results.NotFound($"Usuario con Id {id} no encontrado");

    usuarios.Remove(usuario);

    return Results.NoContent();
});

app.Run();

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
};
