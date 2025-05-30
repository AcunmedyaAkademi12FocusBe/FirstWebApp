using System.ComponentModel.DataAnnotations;

namespace FirstWebApp.Models;

public class Todo
{
    public int Id { get; set; }
    [MaxLength(200)]
    public string Task { get; set; }
    public bool Completed { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
}
