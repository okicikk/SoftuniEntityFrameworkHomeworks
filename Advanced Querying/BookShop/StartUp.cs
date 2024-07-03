namespace BookShop
{
    using BookShop.Models.Enums;
    using Data;
    using Initializer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;
    using Microsoft.EntityFrameworkCore.ValueGeneration;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    public class StartUp
    {
        public static void Main()
        {
            using var db = new BookShopContext();
                        // Resets Database to its default version
            //DbInitializer.ResetDatabase(db);

            //Test solutions here:
            Console.WriteLine(GetGoldenBooks(db));
        }
        //2
        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            Enum.TryParse(command, true, out AgeRestriction ageRestriction);
            var bookTitles = context.Books
                .ToList()
                .Where(b => b.AgeRestriction == ageRestriction)
                .Select(b => b.Title)
                .OrderBy(b => b);
            return string.Join(Environment.NewLine, bookTitles);
        }
        //3
        public static string GetGoldenBooks(BookShopContext context)
        {
            var goldenBooks = context.Books
                .Where(b => b.EditionType == EditionType.Gold && b.Copies < 5000)
                .Select(b => new { b.Title, b.BookId })
                .OrderBy(b => b.BookId)
                .ToList();

            return string.Join(Environment.NewLine, goldenBooks.Select(b => b.Title));
        }
        //4
        public static string GetBooksByPrice(BookShopContext context)
        {
            var booksByPrice = context.Books
                .Where(b => b.Price > 40)
                .Select(b => new { b.Price, b.Title })
                .OrderByDescending(b => b.Price)
                .ToList();

            StringBuilder sb = new StringBuilder();
            foreach (var book in booksByPrice)
            {
                sb.AppendLine($"{book.Title} - ${book.Price:f2}");
            }

            return sb.ToString().TrimEnd();
        }
        //5
        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            var bookNotReleasedIn = context.Books
                .Where(b => b.ReleaseDate.Value.Year != year)
                .Select(b => new { b.BookId, b.Title })
                .ToList();
            string[] titles = bookNotReleasedIn.Select(b => b.Title).ToArray();
            return string.Join(Environment.NewLine, titles);
        }
        //6
        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            List<string> categories = input.ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
            var booksByCategory = context.BooksCategories
                .Where(bc => categories.Contains(bc.Category.Name.ToLower()))
                .Select(bc => bc.Book.Title)
                .OrderBy(t => t)
                .ToList();

            return string.Join(Environment.NewLine, booksByCategory);
        }
        //7
        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            DateTime inputDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var booksReleasedBefore = context.Books
                .Where(b => b.ReleaseDate < inputDate)
                .Select(b => new { b.Title, b.EditionType, b.Price, b.ReleaseDate })
                .OrderByDescending(b => b.ReleaseDate)
                .ToList();
            StringBuilder sb = new StringBuilder();
            foreach (var book in booksReleasedBefore)
            {
                sb.AppendLine($"{book.Title} - {book.EditionType} - ${book.Price:f2}");
            }
            return sb.ToString().TrimEnd();
        }
        //8
        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            List<string> authors =
                context.Authors
                .Where(a => a.FirstName.EndsWith(input))
                .OrderBy(a=>a.FirstName)
                .ThenBy(a=>a.LastName)
                .Select(a => a.FirstName + " " + a.LastName)
                .ToList();
            return string.Join(Environment.NewLine, authors);
        }
        //9
        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            var bookTitles = context.Books
                .Where(b => b.Title.ToLower().Contains(input.ToLower()))
                .OrderBy(b=>b.Title)
                .Select(b => b.Title)
                .ToList();
            return string.Join(Environment.NewLine, bookTitles);
        }
        //10
        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var booksByAuthor = context.Books
                .Where(b => b.Author.LastName.ToLower().StartsWith(input.ToLower()))
                .Select(b => new { b.Title, AuthorName = $"({b.Author.FirstName} {b.Author.LastName})", b.BookId })
                .OrderBy(b => b.BookId)
                .ToList();
            StringBuilder sb = new StringBuilder();
            foreach (var b in booksByAuthor)
            {
                sb.AppendLine($"{b.Title} {b.AuthorName}");
            }
            return sb.ToString().TrimEnd();
        }
        //11
        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            return context.Books.Where(b => b.Title.Length > lengthCheck).Count();
        }
        //12
        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var authorsTotalCopies = context.Authors
                .Select(a => new
                {
                    AuthorFullName = $"{a.FirstName} {a.LastName}",
                    TotalBookCopies = a.Books.Sum(b => b.Copies)
                })
                .OrderByDescending(a => a.TotalBookCopies)
                .ToList();
            StringBuilder sb = new();
            foreach (var a in authorsTotalCopies)
            {
                sb.AppendLine($"{a.AuthorFullName} - {a.TotalBookCopies}");
            }
            return sb.ToString().TrimEnd();
        }
        //13
        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var categoriesProfits = context.Categories
                .Select(c => new
                {
                    CategoryName = c.Name,
                    TotalProfit = c.CategoryBooks.Sum(cb => cb.Book.Price * cb.Book.Copies)
                })
                .OrderByDescending(cb => cb.TotalProfit)
                .ToList();
            return
                string.Join(Environment.NewLine, categoriesProfits.Select(c => $"{c.CategoryName} ${c.TotalProfit:f2}"));
        }

        //14
        public static string GetMostRecentBooks(BookShopContext context)
        {
            var recentBooksByCategory = context.Categories
                .Include(c=>c.CategoryBooks).ThenInclude(c=>c.Book)
                //.AsEnumerable()
                .Select(c => new
                {
                    CategoryName = c.Name,
                    Books = c.CategoryBooks.Select(cb => new
                    {
                        BookName = cb.Book.Title,
                        YearReleased = cb.Book.ReleaseDate
                    }).AsEnumerable().OrderByDescending(b=>b.YearReleased).Take(3).ToList()
                })
                .OrderBy(bc => bc.CategoryName).
                ToList();
            StringBuilder sb = new();
            foreach (var bc in recentBooksByCategory)
            {
                sb.AppendLine($"--{bc.CategoryName}");
                foreach (var b in bc.Books)
                {
                    sb.AppendLine($"{b.BookName} ({b.YearReleased.Value.Year})");
                }
            }
            return sb.ToString().TrimEnd();
        }
        //15
        public static void IncreasePrices(BookShopContext context)
        {
            var booksToBeIncreased = context.Books
                .Where(b => b.ReleaseDate.Value.Year < 2010)
                .ToList();
            foreach (var b in booksToBeIncreased)
            {
                b.Price += 5;
            }
            context.SaveChanges();
        }
        //16
        public static int RemoveBooks(BookShopContext context)
        {
            var booksToBeRemoved = context.Books
                .Where(b => b.Copies < 4200)
                .ToList();
            //int removedBooksCount = booksToBeRemoved.Count;
            foreach (var book in booksToBeRemoved)
            {
                context.Books.Remove(book);
            }
            context.SaveChanges();
            return booksToBeRemoved.Count();
        }
    }

}


