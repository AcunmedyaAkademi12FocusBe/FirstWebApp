using FirstWebApp.Models.DTOs.User;

namespace FirstWebApp.Models.DTOs.Todo;

public class TodoWithUserDto :  TodoDto
{
    public UserDto User { get; set; }
}