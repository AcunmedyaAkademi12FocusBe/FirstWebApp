using System.ComponentModel.DataAnnotations;

namespace FirstWebApp.Models;

public class Todo
{
    public int Id { get; set; }
    // data annotations
    [Required(ErrorMessage = "{0} zorunludur"), MaxLength(200, ErrorMessage = "{0} en fazla {1} karakter olabilir.")] // db tarafında Task'i oluştururken nvarchar(MAX) yerine nvarchar(200) yapıyor
    public string Task { get; set; }
    [Required]
    public bool Completed { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
}

// data transform object
// mevcut datamızı aldık ama içindekileri değiştirdik, belki eksilttik ve sunmak istediğimiz formata getirdik
public class TodoDto
{
    public int Id { get; set; }
    public string Task { get; set; }
    public int UserId { get; set; }
    public UserDto User { get; set; }
    public bool Completed { get; set; }

    public TodoDto() { }
    // public TodoDto(Todo todo) => 
    //     (Id, Task, UserId, User, Completed) = (todo.Id, todo.Task, todo.UserId, new UserDto(todo.User), todo.Completed);
}

public class TodoCreateDto
{
    [Required]
    public string Task { get; set; }
    [Required]
    public int UserId { get; set; }
}

// migration neler yaptı biliyoruz
// gidip yayındaki db de ilgili field'ı kendimiz açarız
// içine veri ekleyip boş olmamasını sağlarız
// sonra da bu alanı tekrar zorunlu yaparız
// migration yapıyorsak bunu sadece geliştirme ortamında yapıp
