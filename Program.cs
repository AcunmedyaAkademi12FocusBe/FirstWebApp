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
app.MapGet("/todos", (AppDbContext db, int skip = 0, int limit = 30, bool showAll = false) =>
{
    var total = db.Todos.Count();
    if (showAll)
    {
        limit = total;
    }
    
    return new
    {
        Todos = db.Todos.Skip(skip).Take(limit).ToArray(),
        skip,
        limit,
        total
    };
});

app.MapGet("/todos/active", (AppDbContext db) => db.Todos.Where(x => !x.Completed).ToArray());
app.MapGet("/todos/completed", (AppDbContext db) => db.Todos.Where(x => x.Completed).ToArray());

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

app.MapPost("/todos", (AppDbContext db, Todo todo) =>
{
    if (todo.Task == null)
    {
        return Results.BadRequest("ayıp ayıp");
    }
    
    db.Todos.Add(todo);
    db.SaveChanges();
    // return todo;
    // ilk parametre oluşturduğum veriye nasıl erişirim yani bu verinin görüntülendiği yer neresi
    return Results.Created($"/todos/{todo.Id}", todo);
});

app.MapDelete("/todos/{id:int}", (int id, AppDbContext db) =>
{
    if (db.Todos.Find(id) is not Todo todo)
    {
        return Results.NotFound();
    }
    
    db.Todos.Remove(todo);
    db.SaveChanges();
    return Results.NoContent();
    
    // var todo = db.Todos.Find(id);
    // if (todo == null)
    // {
    //     return Results.NotFound();
    // }
    //
    // db.Todos.Remove(todo);
    // db.SaveChanges();
    // return Results.NoContent();

});

app.Run();