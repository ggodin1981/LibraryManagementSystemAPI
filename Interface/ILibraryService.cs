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
