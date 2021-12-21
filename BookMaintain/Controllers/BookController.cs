using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BookMaintain.Controllers
{
    public class BookController : Controller
    {
        Models.CodeService codeService = new Models.CodeService();
        Models.BookService bookService = new Models.BookService();

        //查詢書籍
        [HttpGet()]
        public ActionResult SearchBook()
        {
            return View();
        }

        //查詢書籍
        [HttpPost()]
        public JsonResult SearchBook(Models.BookSearchArg arg)
        {
            List<Models.Book> SearchResult = bookService.GetBookDataByCondition(arg);
            return Json(SearchResult);
        }

        //新增書籍
        [HttpGet()]
        public ActionResult InsertBook()
        {
            return View();
        }

        //新增書籍
        [HttpPost()]
        public JsonResult InsertBook(Models.Book Book)
        {
            try
            {
                bookService.InsertBook(Book);
                return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }

        //書籍詳細資料
        [HttpGet()]
        public ActionResult DetailBook()
        {
            return View();
        }

        //書籍詳細資料
        [HttpPost()]
        public JsonResult DetailBook(String BookId)
        {
            Models.Book Detailbook = bookService.GetBookDatail(BookId);
            Detailbook.BookNote = Detailbook.BookNote.Replace("<BR>", "\r\n");
            return Json(Detailbook);
        }

        //刪除書籍
        [HttpPost()]
        public JsonResult DeleteBook(String BookId)
        {
            try
            {
                return Json(bookService.DeleteBook(BookId));
            }
            catch (Exception ex)
            {
                return Json(new { msg = ex.Message });
            }
        }

        //確認書籍狀態
        [HttpPost()]
        public JsonResult CheckBookStatus(String BookId)
        {
            string BookStatusId = bookService.GetBookStatusId(BookId);

            if (BookStatusId == "A" || BookStatusId == "U")
            {           
                return Json(true);
            }
            else if(BookStatusId == "B" || BookStatusId == "C")
            {
                return Json(false);
            }
            else
            {
                return Json("此書已不存在");
            }
        }

        //修改書籍
        [HttpGet()]
        public ActionResult UpdateBook()
        {
            return View();
        }

        //修改書籍
        [HttpPost()]
        public JsonResult UpdateBook(Models.Book Book)
        {
            //B-已借閱 C-已借未領 有借閱人
            //A-可以借閱 U-不可借閱 沒有借閱人
            if (((Book.BookStatusId == "B" || Book.BookStatusId == "C") && (Book.BookKeeperId != null)) ||
                ((Book.BookStatusId == "A" || Book.BookStatusId == "U") && (Book.BookKeeperId == null)))
            {
                bookService.UpdateBook(Book);
                return Json(true);
            }
            else
            {
                return Json(false);
            }
        }

        //取得下拉式選單資料
        [HttpPost()]
        public JsonResult GetDropDownListData(string category)
        {
            List<SelectListItem> ListResult = null;
            switch (category)
            {
                case "BookClassId":
                    ListResult = codeService.GetBookClass();
                    break;
                case "BookKeeperId":
                    ListResult = codeService.GetBookKeeper();
                    break;
                case "BookStatusId":
                    ListResult = codeService.GetBookStatus();
                    break;
            }
            return Json(ListResult);
        }

        //自動完成書名
        [HttpPost()]
        public JsonResult AutoCompleteBookName()
        {
            return Json(bookService.GetBookName());
        }
    }
}