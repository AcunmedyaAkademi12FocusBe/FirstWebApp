using System.ComponentModel.DataAnnotations;

namespace FirstWebApp.Models.DTOs.Todo;

public class TodoUpdateDto
{
    [Required(ErrorMessage = "{0} zorunludur"), MaxLength(200, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
    public string Task { get; set; }
}