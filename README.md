## **Library Management System API**
To implement the Repository pattern and refactor the Library Management System API using Clean Architecture principles, we will structure the application into layers, separating concerns and improving maintainability, testability, and scalability. Here's how you can achieve this:

---
## **Project Structure**

We'll organize the project into different layers:

1. **Domain Layer** 
   - Contains core business logic and entities.
2. **Application Layer**
   - Contains application logic and interfaces.
3. **Infrastructure Layer**
   - Implements data access and repositories.
4. **Presentation Layer**
   - Contains API controllers.
5. **Access the Singleton Instance**
   - Provide a way to access the single instance of the Library Service class.

1.)  **Domain Layer** 

   **Entities** 
   - Create a folder named Domain and define the Book entity.
   
```csharp  
public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string ISBN { get; set; }
    public bool IsBorrowed { get; set; }
}
```
  **Interfaces** 
  
```csharp
public interface IBookRepository
{
    void Add(Book book);
    Book GetById(int id);
    IEnumerable<Book> GetAll();
    void Update(Book book);
}
```

2.) **Application Layer** 
 
   **Services** 
   - Create a folder named Application and define a service for library operations.    
	
```csharp
public class LibraryService : ILibraryService
{
    private readonly IBookRepository _bookRepository;

    public LibraryService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public void AddBook(BookDto bookDto)
    {
        var book = new Book
        {
            Id = GetNextId(),
            Title = bookDto.Title,
            Author = bookDto.Author,
            ISBN = bookDto.ISBN,
            IsBorrowed = false
        };
        _bookRepository.Add(book);
    }

    public BookDto BorrowBook(int id)
    {
        var book = _bookRepository.GetById(id);
        if (book == null || book.IsBorrowed)
        {
            return null;
        }
        
        book.IsBorrowed = true;
        _bookRepository.Update(book);
        return MapToDto(book);
    }

    public BookDto ReturnBook(int id)
    {
        var book = _bookRepository.GetById(id);
        if (book == null || !book.IsBorrowed)
        {
            return null;
        }
        
        book.IsBorrowed = false;
        _bookRepository.Update(book);
        return MapToDto(book);
    }

    public IEnumerable<BookDto> GetAllBooks()
    {
        return _bookRepository.GetAll().Select(MapToDto);
    }

    private BookDto MapToDto(Book book)
    {
        return new BookDto
        {
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN
        };
    }

    private int GetNextId()
    {
        var allBooks = _bookRepository.GetAll();
        return allBooks.Any() ? allBooks.Max(b => b.Id) + 1 : 1;
    }
}
```

 **DTOs**

 **Create a DTO for book operations.**
   
```csharp
public class BookDto
{
    public string Title { get; set; }
    public string Author { get; set; }
    public string ISBN { get; set; }
}
```
 
3.) **Infrastructure Layer**

 **In-Memory Repository** 
 - Create a folder named Infrastructure and implement the in-memory repository.
	
```csharp 
public class InMemoryBookRepository : IBookRepository
{
    private readonly List<Book> _books;
    private readonly object _lock = new();

    public InMemoryBookRepository()
    {
        _books = new List<Book>();
    }

    public void Add(Book book)
    {
        lock (_lock)
        {
            _books.Add(book);
        }
    }

    public Book GetById(int id)
    {
        lock (_lock)
        {
            return _books.FirstOrDefault(b => b.Id == id);
        }
    }

    public IEnumerable<Book> GetAll()
    {
        lock (_lock)
        {
            return _books.ToList();
        }
    }

    public void Update(Book book)
    {
        lock (_lock)
        {
            var existingBook = GetById(book.Id);
            if (existingBook != null)
            {
                existingBook.Title = book.Title;
                existingBook.Author = book.Author;
                existingBook.ISBN = book.ISBN;
                existingBook.IsBorrowed = book.IsBorrowed;
            }
        }
    }
}
```
4. **Presentation Layer**

    **Controllers** 
	- Update the controller to use the ILibraryService.
	
```csharp
[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly ILibraryService _libraryService;

    public BooksController(ILibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    [HttpPost]
    public IActionResult AddBook([FromBody] BookDto bookDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        _libraryService.AddBook(bookDto);
        return CreatedAtAction(nameof(GetAllBooks), new { title = bookDto.Title }, bookDto);
    }

    [HttpPut("{id}/borrow")]
    public IActionResult BorrowBook(int id)
    {
        var book = _libraryService.BorrowBook(id);
        if (book == null)
        {
            return NotFound("Book is either not available or already borrowed.");
        }
        return Ok(book);
    }

    [HttpPut("{id}/return")]
    public IActionResult ReturnBook(int id)
    {
        var book = _libraryService.ReturnBook(id);
        if (book == null)
        {
            return NotFound("Book is either not found or wasn't borrowed.");
        }
        return Ok(book);
    }

    [HttpGet]
    public IActionResult GetAllBooks()
    {
        var books = _libraryService.GetAllBooks();
        return Ok(books);
    }
}
```
5.) **Dependency Injection Setup** 	
  - To properly configure dependency injection in your ASP.NET Core application using Startup.cs or Program.cs (depending on the version of .NET you're using), here’s how you can set it up for the Library Management System API.
	
  **For .NET 6 and Later: Using Program.cs**
  - If you're using .NET 6 or later, the Startup.cs class is typically replaced by a simplified Program.cs file. Here’s how to configure dependency injection in that case:

```csharp

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Application.Services; // Adjust namespace as necessary
using Domain.Interfaces;
using Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddControllers();
builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>();
builder.Services.AddTransient<ILibraryService, LibraryService>();

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Library Management API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library Management API V1");
});

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
```
	
	
  **Conclusion**
  - This refactored code now uses the Repository pattern to manage data access through the InMemoryBookRepository and applies Clean Architecture principles by separating the application into layers (Domain, Application, Infrastructure, Presentation).

  **Maintainability:**
  - Each component has a clear responsibility, making the code easier to understand and modify.
  
  **Scalability:**
  - New features can be added with minimal impact on existing code.
   
  **Testability:** 
  - The separation of concerns allows for easier unit testing of individual components.

You can now build and test the API, which should work with in-memory data storage. If you have any further questions or need more adjustments, feel free to ask!
	
  **Summary of Changes**
  
  **Dependency Injection:**
  - The repository and service interfaces are registered with the DI container, ensuring they can be injected into your controllers.
   
  **Swagger Configuration:**
  - Swagger is set up to provide an interactive API documentation interface, making it easy to test your endpoints.
  
  **Testing the API**
 
 Once you've set up the Program.cs, you can run your application and navigate to http://localhost:<port>/swagger to see the Swagger UI, which will allow you to interact with your API endpoints.

If you need further adjustments or assistance, feel free to ask!

	