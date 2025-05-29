using System.ComponentModel.DataAnnotations;
using AutoMapper;
using FirstWebApp;
using FirstWebApp.Data;
using FirstWebApp.Models;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// var configuration = new MapperConfiguration(cfg => 
// {
//     cfg.CreateMap<Todo, TodoDto>();
//     cfg.CreateMap<User, UserDto>();
// });

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

var todos = app.MapGroup("/todos");
// prefix = önüne ek
// suffix = sonuna ek

// tüm todoları okuma
todos.MapGet("/", (AppDbContext db, int skip = 0, int limit = 30, bool showAll = false) =>
{
    var total = db.Todos.Count();
    if (showAll)
    {
        limit = total;
    }
    
    // var mapper = configuration.CreateMapper();
    var todos = db.Todos.Skip(skip).Take(limit).Include(u => u.User).ToArray();
    // db.Todos.Skip(skip).Take(limit)
    // .Include(u => u.User)
    // .Select(t => new TodoDto(t))
    // .ToArray()
    
    return new
    {
        // dotnet add package Mapster
        Todos = todos.Adapt<TodoDto[]>(),
        skip,
        limit,
        total
    };
});

// tüm todolar üzerinde filtreleme örnekleri
todos.MapGet("/active", (AppDbContext db) => db.Todos.Where(x => !x.Completed).ToArray());
todos.MapGet("/completed", (AppDbContext db) => db.Todos.Where(x => x.Completed).ToArray());

// tek bir todo gösterme
todos.MapGet("/{id:int}", (int id, AppDbContext db) =>
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

// yeni ekleme
todos.MapPost("/", (AppDbContext db, TodoCreateDto newTodo) =>
{
    // var validationContext = new ValidationContext(todo);
    // var validationResults = new List<ValidationResult>();
    // Validator.TryValidateObject(todo, validationContext, validationResults, true);
    
    if (!ValidationHelper.ValidateModel(newTodo, out var validationResults))
    {
        return Results.BadRequest(validationResults);
    }
    
    // user ı kontrol etmezsem 500 hatası alabiliyorum
    // neden? çünkü o id'ye ait bir user olmayabilir
    // bu yüzden user'ı arayıp buluyorum.
    var user = db.Users.Find(newTodo.UserId);
    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    // user'ı bulup contexte aldığım için de. artık todo'ya ekstra include etmeme gerek kalmıyor.
    var todo = newTodo.Adapt<Todo>();
    db.Todos.Add(todo);
    // veya
    // var todo = new Todo
    // {
    //     Task = newTodo.Task,
    //     UserId = newTodo.UserId
    // };
    //
    // db.Todos.Add(todo);
    
    db.SaveChanges();
    // return todo;
    // ilk parametre oluşturduğum veriye nasıl erişirim yani bu verinin görüntülendiği yer neresi
    return Results.Created($"/{nameof(todos)}/{todo.Id}", todo.Adapt<TodoDto>());
});

// silme
todos.MapDelete("/{id:int}", (int id, AppDbContext db) =>
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

// güncelleme
todos.MapPut("/{id:int}", (AppDbContext db, int id, Todo updatedTodo) =>
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
// todos.MapPut("/{id:int}/completed", (int id, AppDbContext db) =>
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

// todos.MapPut("/{id:int}/active", (int id, AppDbContext db) =>
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

todos.MapPut("/{id:int}/{status:required}", (int id, string status, AppDbContext db) =>
{
    if (db.Todos.Find(id) is not Todo todo)
    {
        return Results.NotFound();
    }

    // pattern matching kullanamadık 🥲
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

var users = app.MapGroup("/users");
users.MapPost("/", (AppDbContext db, User user) =>
{
    
});

app.Run();