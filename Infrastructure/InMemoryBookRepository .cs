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
