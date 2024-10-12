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
