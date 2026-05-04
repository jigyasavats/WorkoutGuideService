using Repository;

namespace UserService;

public sealed class UserManager
{
    private readonly UserRepository _repository;

    public UserManager(UserRepository repository)
    {
        _repository = repository;
    }

    public async Task AddUserAsync()
    {
        Console.Write("\nEnter user name: ");
        var name = Console.ReadLine();

        Console.Write("Enter email: ");
        var email = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("Name and email are required.\n");
            return;
        }

        try
        {
            var user = User.Create(name, email);
            var createdUser = await _repository.CreateUserAsync(user);
            Console.WriteLine($"✓ User created successfully!");
            Console.WriteLine($"  ID: {createdUser.Id}");
            Console.WriteLine($"  Email: {createdUser.Email}");
            Console.WriteLine($"  Name: {createdUser.Name}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating user: {ex.Message}\n");
        }
    }

    public async Task GetUserProfileAsync()
    {
        Console.Write("\nEnter email to search: ");
        var email = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("Email is required.\n");
            return;
        }

        try
        {
            var user = await _repository.GetUserByEmailAsync(email);
            if (user is null)
            {
                Console.WriteLine($"No user found with email: {email}\n");
            }
            else
            {
                Console.WriteLine($"\nUser found:");
                Console.WriteLine($"  ID: {user.Id}");
                Console.WriteLine($"  Email: {user.Email}");
                Console.WriteLine($"  Name: {user.Name}\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving user: {ex.Message}\n");
        }
    }
}
