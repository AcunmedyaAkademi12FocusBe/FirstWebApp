using System.ComponentModel.DataAnnotations;
using FirstWebApp;
using FirstWebApp.Data;
using FirstWebApp.Models;
using FirstWebApp.Models.DTOs.Todo;
using FirstWebApp.Models.DTOs.User;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

var todos = app.MapGroup("/todos");

todos.MapGet("/", (AppDbContext db, int skip = 0, int limit = 30, bool showAll = false) =>
{
    var total = db.Todos.Count();
    if (showAll)
    {
        limit = total;
    }
    
    var todos = db.Todos
        .Skip(skip)
        .Take(limit)
        .Include(u => u.User)
        .ToArray();
    
    return new
    {
        // dotnet add package Mapster
        Todos = todos.Adapt<TodoWithUserDto[]>(),
        skip,
        limit,
        total
    };
});

// tüm todolar üzerinde filtreleme örnekleri
todos.MapGet("/active", (AppDbContext db) => db.Todos.Where(x => !x.Completed).ToArray());
todos.MapGet("/completed", (AppDbContext db) => db.Todos.Where(x => x.Completed).ToArray());

todos.MapGet("/{id:int}", (int id, AppDbContext db) => 
    db.Todos.Find(id) is Todo todo ? Results.Ok(todo) : Results.NotFound());

todos.MapPost("/", (AppDbContext db, TodoCreateDto newTodo) =>
{
    if (!ValidationHelper.ValidateModel(newTodo, out var validationResults))
    {
        return Results.BadRequest(validationResults);
    }
    
    var user = db.Users.Find(newTodo.UserId);
    if (user == null)
    {
        // eğer doğrulama mesaj formatını bozmak istemiyorsak. Aşağıdaki yaklaşım güzel çalışıyor.
        validationResults.Add(new ValidationResult("User not found.", new[] {"UserId"}));
        return Results.NotFound(validationResults);
    }

    // user'ı bulup contexte aldığım için de. artık todo'ya ekstra include etmeme gerek kalmıyor.
    var todo = newTodo.Adapt<Todo>();
    db.Todos.Add(todo);
    db.SaveChanges();
    return Results.Created($"/{nameof(todos)}/{todo.Id}", todo.Adapt<TodoDto>());
});

todos.MapDelete("/{id:int}", (int id, AppDbContext db) =>
{
    if (db.Todos.Find(id) is not Todo todo)
    {
        return Results.NotFound();
    }

    db.Todos.Remove(todo);
    db.SaveChanges();
    return Results.NoContent();
});

todos.MapPut("/{id:int}", (AppDbContext db, int id, TodoUpdateDto updatedTodo) =>
{
    if (!ValidationHelper.ValidateModel(updatedTodo, out var validationResults))
    {
        return Results.BadRequest(validationResults);
    }
    
    if (db.Todos.Find(id) is not Todo todo)
    {
        return Results.NotFound();
    }
    
    todo.Task = updatedTodo.Task;
    db.SaveChanges();
    return Results.NoContent();
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
users.MapGet("/", (AppDbContext db) => db.Users.ToArray());

users.MapPost("/", (AppDbContext db, UserCreateDto newUserDto) =>
{
    if (!ValidationHelper.ValidateModel(newUserDto, out var validationResults))
    {
        return Results.BadRequest(validationResults);
    }
    
    var newUser = newUserDto.Adapt<User>();
    db.Users.Add(newUser);
    db.SaveChanges();
    return Results.Created($"/{nameof(users)}/{newUser.Id}", newUser.Adapt<UserDto>());
});

users.MapGet("/{id:int}", (int id, AppDbContext db) => 
    db.Users.Find(id) is User user ? Results.Ok(user.Adapt<UserDto>()) : Results.NotFound());

// users.MapGet("/{id:int}/todos", (int id, AppDbContext db) => 
//     db.Users.Where(u => u.Id == id).Include(t => t.Todos).FirstOrDefault() is User user ? 
//         Results.Ok(user.Adapt<UserWithTodosDto>()) : Results.NotFound());

users.MapGet("/{id:int}/todos", (int id, AppDbContext db) =>
{
    var user = db.Users.Find(id);
    if (user == null)
    {
        return Results.NotFound();
    }
    
    var todos = db.Todos.Where(t => t.UserId == id).ToArray();
    return Results.Ok(todos.Adapt<TodoDto[]>());
    // eğer user'ı da göstermek istiyorsak ama her todo'da user yazar. ona göre
    //return Results.Ok(todos.Adapt<TodoWithUserDto[]>());
});

// db.Todos.Where(u => u.Id == id).ToArray()

// todoları listeliyim ama hepsinde user bilgisi olsun
// ilk başta user çıksın altında todos yazsın hepsi gelsin
// ya da dümdüz ilgili user'a ait todolar gelsin

app.Run();