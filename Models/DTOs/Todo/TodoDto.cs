using FirstWebApp.Models.DTOs.User;

namespace FirstWebApp.Models.DTOs.Todo;

public class TodoDto
{
    public int Id { get; set; }
    public string Task { get; set; }
    public bool Completed { get; set; }
}