using System.ComponentModel.DataAnnotations;

namespace FirstWebApp.Models;

public class Todo
{
    public int Id { get; set; }
    // data annotations
    [MaxLength(200)]
    public string Task { get; set; }
    public bool Completed { get; set; }
}