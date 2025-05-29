namespace FirstWebApp.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Todo> Todos { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    // boş obje açabilmek için de bunu ekleyebiliriz
    public UserDto() { }
    public UserDto(User user) => (Id, Name) = (user.Id, user.Name);
}