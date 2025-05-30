using System.ComponentModel.DataAnnotations;

namespace FirstWebApp.Models.DTOs.Todo;

public class TodoCreateDto
{
    [Required(ErrorMessage = "{0} zorunludur"), MaxLength(200, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
    public string Task { get; set; }
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }
}