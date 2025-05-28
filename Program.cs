using System.ComponentModel.DataAnnotations;
using FirstWebApp;
using FirstWebApp.Data;
using FirstWebApp.Models;
using Microsoft.AspNetCore.Mvc;
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
    
    // var validationContext = new ValidationContext(todo);
    // var validationResults = new List<ValidationResult>();
    // Validator.TryValidateObject(todo, validationContext, validationResults, true);
    
    if (!ValidationHelper.ValidateModel(todo, out var validationResults))
    {
        return Results.BadRequest(validationResults);
    }
    
    db.Todos.Add(todo);
    db.SaveChanges();
    // return todo;
    // ilk parametre oluşturduğum veriye nasıl erişirim yani bu verinin görüntülendiği yer neresi
    return Results.Created($"/todos/{todo.Id}", todo);
});

app.MapDelete("/todos/{id:int}", (int id, AppDbContext db) =>
{
    // anti forgery 
    // çık
    
    if (db.Todos.Find(id) is not Todo todo)
    {
        return Results.NotFound();
    }
    
    // captcha ok mi?
    // çık
    
    db.Todos.Remove(todo);
    db.SaveChanges();
    return Results.NoContent();
    
    // veya eski usül
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

app.MapPut("/todos/{id:int}", (AppDbContext db, int id, Todo updatedTodo) =>
{
    // eğer completed gelmezse o zaman completed false olur :)
    if (db.Todos.Find(id) is not Todo todo)
    {
        return Results.NotFound();
    }
    
    todo.Task = updatedTodo.Task;
    // ben completed'ı böyle güncellemiyorum
    // todo.Completed = updatedTodo.Completed;

    db.SaveChanges();
    
    return Results.NoContent();
    // Task ve Completed
    // Completed gelmezse tamamlanmış olan bir todo'yu tamamlanmadı olarak işaretlerim
});

// active = aktif/tamamlanmadı
// completed = tamamlandı
// mark complete = tamamlandı olarak işaretle
// app.MapPut("/todos/{id:int}/completed", (int id, AppDbContext db) =>
// {
//     // no content
//     // todo'yu bulamama ihtimalimiz var no todo
//     if (db.Todos.Find(id) is Todo todo)
//     {
//         todo.Completed = true;
//         db.SaveChanges();
//         return Results.NoContent();
//     }
//     
//     return Results.NotFound();
// });

// app.MapPut("/todos/{id:int}/active", (int id, AppDbContext db) =>
// {
//     // no content
//     // todo'yu bulamama ihtimalimiz var no todo
//     if (db.Todos.Find(id) is Todo todo)
//     {
//         todo.Completed = false;
//         db.SaveChanges();
//         return Results.NoContent();
//     }
//     
//     return Results.NotFound();
// });

// get, put, patch
app.MapPut("/todos/{id:int}/{status:required}", (int id, string status, AppDbContext db) =>
{
    if (db.Todos.Find(id) is not Todo todo)
    {
        return Results.NotFound();
    }

    switch (status)
    {
        case "active":
            todo.Completed = false;
        break;
        case "completed":
            todo.Completed = true;
            break;
        default:
            return Results.BadRequest();
    }

    db.SaveChanges();
    return Results.NoContent();
});

// kalabalık ekip ile çalışmak için
// event driven architecture

app.Run();