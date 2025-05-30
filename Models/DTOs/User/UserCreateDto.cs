using System.ComponentModel.DataAnnotations;

namespace FirstWebApp.Models.DTOs.User;

public class UserCreateDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; }
}