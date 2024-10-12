## **Library Management System API**
To implement the Repository pattern and refactor the Library Management System API using Clean Architecture principles, we will structure the application into layers, separating concerns and improving maintainability, testability, and scalability. Here's how you can achieve this:

## **Enhanced Requirements:**

1. **Book Class:**
   - Add properties: Id, Title, Author, ISBN, and IsBorrowed.
   
3. **Library Operations:**
  - Implement the Singleton pattern for the Library class.
  - Add methods for:
	- Adding a book.
  	- Borrowing a book.
   	- Returning a book.
    	- Listing all books (with borrowed status).
     	- Get a Book By Id
3. **API Endpoints:**
   - Add a book (POST /api/books).
   - Borrow a book (PUT /api/books/{id}/borrow).
   - Return a book (PUT /api/books/{id}/return).
   - List all books (GET /api/books).
   - Get a specific book (GET /api/books/{id}).
4. **Design Patterns:**
   - Use the Singleton pattern for managing the library instance.
   - Implement the Repository pattern to handle data access.
   - Use Clean Architecture with layers for Domain, Application, and Infrastructure.

5. **Error Handling:**
   - Add meaningful error responses for scenarios like book not found, invalid book data, or book already borrowed.

---

## **Organize the project into different layers:**

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

  **Enhanced Code:**
  
 **Model** 
```csharp  
namespace LibraryManagementSystem.Model
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public bool IsBorrowed { get; set; }
    }   
    public class BookDto
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
    }
}

```
  **Interfaces** 
  
```csharp
using LibraryManagementSystem.Model;
namespace LibraryManagementSystem.Interface
{  
    public interface IBookRepository
    {
        void Add(Book book);
        Book GetById(int id);
        IEnumerable<Book> GetAll();
        void Update(Book book);
    }   
}

using LibraryManagementSystem.Model;
namespace LibraryManagementSystem.Interface
{
    public interface ILibraryService
    {
        Book GetById(int id);
        Book AddBook(BookDto bookDto);
        BookDto BorrowBook(int id);
        BookDto ReturnBook(int id);
        IEnumerable<BookDto> GetAllBooks();
    }
}
```

2.) **Application Layer** 
 
   **Service Layer** 
   - Create a folder named Application and define a service for library operations.    
	
```csharp
using LibraryManagementSystem.Interface;
using LibraryManagementSystem.Model;
namespace LibraryManagementSystem.ServiceLayer
{
    
    public class LibraryService : ILibraryService
    {
        private readonly IBookRepository _bookRepository;

        public LibraryService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public Book GetById(int id)
        {
            var book = _bookRepository.GetById(id);
            return book;
        }

        public Book AddBook(BookDto bookDto)
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
            return book;
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
using LibraryManagementSystem.Model;
using LibraryManagementSystem.Interface;

namespace LibraryManagementSystem.Infrastructure
{
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
}
```

4. **Presentation Layer**

    **Controllers** 
	- Update the controller to use the ILibraryService.
	
```csharp
using LibraryManagementSystem.Interface;
using LibraryManagementSystem.Model;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static System.Reflection.Metadata.BlobBuilder;

namespace LibraryManagementSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly ILibraryService _libraryService;

        public BooksController(ILibraryService libraryService)
        {
            _libraryService = libraryService;
        }
       
        [SwaggerOperation(Summary = "Add a new book", Description = "Adds a book with title, author, and ISBN.")]
        [SwaggerResponse(200, "Book added successfully.")]
        /// <summary>
        ///Add a new book
        /// </summary>
        [HttpPost]
        public IActionResult AddBook([FromBody] BookDto book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (book == null || string.IsNullOrWhiteSpace(book.Title) || string.IsNullOrWhiteSpace(book.Author))
            {
                return BadRequest("Invalid book data.");
            }
            var newBook = _libraryService.AddBook(book);         
            return Ok(newBook);
        }

        [HttpPut("{id}/borrow")]
        [SwaggerOperation(Summary = "Borrow a book", Description = "Marks a book as borrowed.")]
        [SwaggerResponse(200, "Book borrowed successfully.")]
        [SwaggerResponse(404, "Book not available for borrowing.")]
        /// <summary>
        /// Borrow a book
        /// </summary>
        public IActionResult BorrowBook(int id)
        {
            var book = _libraryService.BorrowBook(id);
            if (book == null)
            {
                return NotFound("Book not available for borrowing.");
            }
            return Ok("Book borrowed successfully.");
        }

        [HttpPut("{id}/return")]
        [SwaggerOperation(Summary = "Return a book", Description = "Marks a book as returned.")]
        [SwaggerResponse(200, "Return Book successfully.")]
        [SwaggerResponse(404, "Book is either not found or wasn't borrowed.")]
        /// <summary>
        /// Return a book
        /// </summary>
        public IActionResult ReturnBook(int id)
        {
            var book = _libraryService.ReturnBook(id);
            if (book == null)
            {
                return NotFound("Book is either not found or wasn't borrowed.");
            }
            return Ok("Book returned successfully.");
        }
        [SwaggerOperation(Summary = "Get By Id", Description = "Get Book by id")]
        [HttpGet("{id}")]
        public IActionResult GetBook(int id)
        {
            var book = _libraryService.GetById(id);
            if (book == null)
            {
                return NotFound("Book not found.");
            }
            return Ok(book);
        }

        [HttpGet]
        [SwaggerOperation(Summary = "List all books", Description = "Marks a book as returned.")]
        /// <summary>
        /// List all books
        /// </summary>
        public IActionResult GetAllBooks()
        {
            var books = _libraryService.GetAllBooks();
            return Ok(books);
        }
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

**Properties:** 

**launchSettings:**
 
```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:42650",
      "sslPort": 44307
    }
  },
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:5148",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:7185;http://localhost:5148",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

  ## **Key Enhancements:**
  
  **Repository Pattern:**   
  - IBookRepository separates data access logic from business logic, making it easier to switch out storage implementations (e.g., in-memory, database).
     
  **Singleton:**
  - The LibraryService ensures there is only one instance of the service, which uses the repository for data operations.
     
  **Error Handling:**
  - Added validation and proper HTTP responses for errors like invalid book data, book not found, and already borrowed or returned books.
    
  **Clean Architecture:**
  - Separation of concerns with domain models, service layer (application), and controllers (presentation layer).
    
  **Pattern:**
  - The Library Service handles book GetNextId with unique IDs.
    
 
	
  ## **Conclusion**
  - This refactored code now uses the Repository pattern to manage data access through the InMemoryBookRepository and applies Clean Architecture principles by separating the application into layers (Domain, Application, Infrastructure, Presentation).

  **Maintainability:**
  - Each component has a clear responsibility, making the code easier to understand and modify.
  
  **Scalability:**
  - New features can be added with minimal impact on existing code.
   
  **Testability:** 
  - The separation of concerns allows for easier unit testing of individual components.

You can now build and test the API, which should work with in-memory data storage. If you have any further questions or need more adjustments, feel free to ask!




	
  ## **Summary of Changes**
  
  **Dependency Injection:**
  - The repository and service interfaces are registered with the DI container, ensuring they can be injected into your controllers.
   
  **Swagger Configuration:**
  - Swagger is set up to provide an interactive API documentation interface, making it easy to test your endpoints.
  
  **Testing the API**
  
 
 Once you've set up the Program.cs, you can run your application and navigate to http://localhost:<port>/swagger to see the Swagger UI, which will allow you to interact with your API endpoints.

If you need further adjustments or assistance, feel free to ask!

	
