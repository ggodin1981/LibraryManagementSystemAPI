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
