using FirstWebApp.Data;
using FirstWebApp.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
var app = builder.Build();

// minimal api
app.MapGet("/", () => "Hello World!"); // 200
app.MapGet("/me", () => 
    new {
        FirstName = "Orhan",
        LastName = "Ekici",
        Age = 36
    }
);

// CRUD
// Create, Read, Update, Delete
app.MapGet("/todos", (AppDbContext db) => db.Todos.ToArray());

app.MapGet("/todos/{id:int}", (int id, AppDbContext db) =>
{
    return db.Todos.Find(id) is Todo todo ? Results.Ok(todo) : Results.NotFound();
    
    // var todo = db.Todos.Find(id);
    // if (todo == null)
    // {
    //     return Results.NotFound();
    // }
    // return Results.Ok(todo);
    // eğer herhangi bir koşulda status dönersek o zaman her return ifadesinin status dönmesini istiyor
});

app.Run();