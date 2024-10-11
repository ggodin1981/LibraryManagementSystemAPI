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

 - **Domain Layer**  
    - **Entities** Create a folder named Domain and define the Book entity.
```csharp
 // Domain/Entities/Book.cs
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
 // Domain/Interfaces/IBookRepository.cs
public interface IBookRepository
{
    void Add(Book book);
    Book GetById(int id);
    IEnumerable<Book> GetAll();
    void Update(Book book);
}
 ```
 
 
 
 