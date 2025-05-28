using System.ComponentModel.DataAnnotations;

namespace FirstWebApp.Models;

public class Todo
{
    public int Id { get; set; }
    // data annotations
    [Required(ErrorMessage = "{0} zorunludur"), MaxLength(200, ErrorMessage = "{0} en fazla {1} karakter olabilir.")] // db tarafında Task'i oluştururken nvarchar(MAX) yerine nvarchar(200) yapıyor
    public string Task { get; set; }
    public bool Completed { get; set; }
}