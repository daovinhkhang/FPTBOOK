using FPTBook_v3.Data;
using FPTBook_v3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FPTBook_v3.Controllers
{

    [Authorize(Roles = "Owner")]
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<BookController> _logger;

        public BookController(ILogger<BookController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [Route("/Owner/Book")]
        public async Task<IActionResult>  Index()
        {
            IEnumerable<Book> books = await GetBooks();
            IEnumerable<Category> categorys = await _db.Categorys.ToListAsync(); ;
            Models.BookDisplayModel bookModel = new Models.BookDisplayModel
            {
                Books = books,
                Categorys = categorys
            };
            return View(bookModel);
        }


        [Route("/Owner/Book/Create")]
        public IActionResult Create()
        {
            ViewData["Cate_Id"] = new SelectList(_db.Categorys.Where(c => c.cate_Status == "processed").ToList(), "cate_Id", "cate_Name");
            return View();
        }

        [HttpPost]
        [Route("/Owner/Book/Create")]
        public IActionResult Create(Book book)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string uniqueFileName = UploadFile(book);
                    book.book_ImagURL = uniqueFileName;
                    if (book.book_Quantity < 0 || book.book_Price < 0)
                    {
                        TempData["Fail"] = "Quantity and Price must be greater than 0";
                        ViewData["Cate_Id"] = new SelectList(_db.Categorys.Where(c => c.cate_Status == "processed").ToList(), "cate_Id", "cate_Name");
                        return View(book);
                    }
                    _db.Books.Add(book);
                    _db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    ViewData["Cate_Id"] = new SelectList(_db.Categorys.Where(c => c.cate_Status == "processed").ToList(), "cate_Id", "cate_Name");
                    TempData["Error"] = "";
                    return View(book);
                }
                
            }
            return View(book);
        }


        [Route("/Owner/Book/Edit/{id:}")]
        public IActionResult Edit(int id)
        {
            Book book = _db.Books.Find(id);
            if (book == null)
            {
                return RedirectToAction("Index");
            }
            ViewData["Cate_Id"] = new SelectList(_db.Categorys, "cate_Id", "cate_Name");
            return View(book);
        }

        [HttpPost]
        [Route("/Owner/Book/Edit/{id:}")]
        public IActionResult Edit(int id, Book book,string img)
        {
            book.book_Id = id;
            if (ModelState.IsValid)
            {
                try
                {
                    if (book.book_Img == null)
                    {
                        book.book_ImagURL = img;
                        if (book.book_Quantity < 0 || book.book_Price < 0)
                        {
                            TempData["Fail"] = "Quantity and Price must be greater than 0";
                            ViewData["Cate_Id"] = new SelectList(_db.Categorys.Where(c => c.cate_Status == "processed").ToList(), "cate_Id", "cate_Name");
                            return View(book);
                        }
                        _db.Books.Update(book);
                        _db.SaveChanges();
                    }
                    else
                    {
                        string uniqueFileName = UploadFile(book);
                        book.book_ImagURL = uniqueFileName;

                        if (book.book_Quantity < 0 || book.book_Price < 0)
                        {
                            TempData["Fail"] = "Quantity and Price must be greater than 0";
                            ViewData["Cate_Id"] = new SelectList(_db.Categorys.Where(c => c.cate_Status == "processed").ToList(), "cate_Id", "cate_Name");
                            return View(book);
                        }
                        _db.Books.Update(book);
                        _db.SaveChanges();

                        img = Path.Combine("wwwroot", "uploads", img);
                        FileInfo infor = new FileInfo(img);
                        if (infor != null)
                        {
                            System.IO.File.Delete(img);
                            infor.Delete();
                        }
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["Error"] = "";
                    ViewData["Cate_Id"] = new SelectList(_db.Categorys.Where(c => c.cate_Status == "processed").ToList(), "cate_Id", "cate_Name");
                    return View(book);
                }
                
            }

            return View(book);
        }

        


        [Route("/Owner/Book/Delete/{id:}")]
        public ActionResult Delete(int id, string img)
        {
            Book book = _db.Books.Find(id);
            if (book == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                img = Path.Combine("wwwroot", "uploads", img);
                FileInfo infor = new FileInfo(img);
                if (infor != null)
                {
                    System.IO.File.Delete(img);
                    infor.Delete();
                }
                _db.Books.Remove(book);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
        }

        public string UploadFile(Book book)
        {
            string uniqueFileName = null;

            if (book.book_Img != null)
            {
                var ext = Path.GetExtension(book.book_Img.FileName);
                var allowedExtensions = new string[] { ".jpg", ".png", ".jpeg" };
                if (!allowedExtensions.Contains(ext))
                {
                    string msg = string.Format("Only {0} extensions are allowed", string.Join(",", allowedExtensions));
                    throw new Exception(msg);
                }
                
                string uploadsFoder = Path.Combine("wwwroot", "uploads");
                uniqueFileName = Guid.NewGuid().ToString() + book.book_Img.FileName;
                string filePath = Path.Combine(uploadsFoder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    book.book_Img.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }



        public async Task<IEnumerable<Book>> GetBooks()
        {
            IEnumerable<Book> books = await (from book in _db.Books
                                             join genre in _db.Categorys
                                             on book.cate_Id equals genre.cate_Id
                                             select new Book
                                             {
                                                 book_Id = book.book_Id,
                                                 book_ImagURL = book.book_ImagURL,
                                                 category = book.category,
                                                 book_Title = book.book_Title,
                                                 cate_Id = book.cate_Id,
                                                 book_Price = book.book_Price,
                                                 book_Quantity = book.book_Quantity,
                                                 publication_date = book.publication_date,
                                                 book_Description = book.book_Description
                                             }
                         ).ToListAsync();
            return books;

        }


    }
}
