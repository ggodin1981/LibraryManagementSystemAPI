using LibraryManagementSystem.Interface;
using LibraryManagementSystem.Model;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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

        [HttpPost]
        [SwaggerOperation(Summary = "Add a new book", Description = "Adds a book with title, author, and ISBN.")]
        [SwaggerResponse(200, "Book added successfully.")]
        /// <summary>
        ///Add a new book
        /// </summary>
        public IActionResult AddBook([FromBody] BookDto bookDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _libraryService.AddBook(bookDto);
            return Ok("Book added successfully.");
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