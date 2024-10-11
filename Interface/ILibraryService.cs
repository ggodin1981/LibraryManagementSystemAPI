using LibraryManagementSystem.Model;

namespace LibraryManagementSystem.Interface
{
    public interface ILibraryService
    {
        void AddBook(BookDto bookDto);
        BookDto BorrowBook(int id);
        BookDto ReturnBook(int id);
        IEnumerable<BookDto> GetAllBooks();
    }


}
