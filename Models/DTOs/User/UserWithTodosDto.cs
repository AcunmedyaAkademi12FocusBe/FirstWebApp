using FirstWebApp.Models.DTOs.Todo;

namespace FirstWebApp.Models.DTOs.User;

public class UserWithTodosDto : UserDto
{
    public ICollection<TodoDto> Todos { get; set; }
}