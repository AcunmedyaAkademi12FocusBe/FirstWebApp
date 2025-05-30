using System.ComponentModel.DataAnnotations;

namespace FirstWebApp.Models;

public class User
{
    public int Id { get; set; }
    [MaxLength(100)]
    public string Name { get; set; }
    public ICollection<Todo> Todos { get; set; }
}
