using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BookMaintain.Models
{
    public class BookService
    {
        //取得連線字串
        private string GetDBConnectionString()
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings["DBConn"].ConnectionString.ToString();
        }

        //依搜尋條件取得所需的書籍資料
        public List<Models.Book> GetBookDataByCondition(Models.BookSearchArg arg)
        {
            DataTable dt = new DataTable();
            string sql = @"SELECT data.BOOK_ID AS BookId,
                                data.BOOK_NAME AS BookName,
                                class.BOOK_CLASS_NAME AS BookClassName,
                                CONVERT(VARCHAR(10),data.BOOK_BOUGHT_DATE,111) AS BookBoughtDate,
                                data.BOOK_STATUS AS BookStatusId,
                                code.CODE_NAME AS BookStatusName,
                                member.USER_ENAME AS BookKeeperEName
                          FROM BOOK_DATA data
                            LEFT JOIN BOOK_CLASS class
                                ON data.BOOK_CLASS_ID = class.BOOK_CLASS_ID
                            LEFT JOIN BOOK_CODE code
                                ON data.BOOK_STATUS = code.CODE_ID
                            LEFT JOIN MEMBER_M member
                                ON data.BOOK_KEEPER = member.USER_ID
                          WHERE (UPPER(data.BOOK_NAME) LIKE UPPER ('%' + @BookName + '%') OR @BookName = '')
                            AND (data.BOOK_CLASS_ID = @BookClassId OR @BookClassId = '')
                            AND (data.BOOK_KEEPER = @BookKeeperId OR @BookKeeperId = '')
                            AND (data.BOOK_STATUS = @BookStatusId OR @BookStatusId = '')
                            AND code.CODE_TYPE = 'BOOK_STATUS'
                          ORDER BY BookBoughtDate DESC, BookName";
            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@BookName", arg.BookName == null ? string.Empty : arg.BookName));
                cmd.Parameters.Add(new SqlParameter("@BookClassId", arg.BookClassId == null ? string.Empty : arg.BookClassId));
                cmd.Parameters.Add(new SqlParameter("@BookKeeperId", arg.BookKeeperId == null ? string.Empty : arg.BookKeeperId));
                cmd.Parameters.Add(new SqlParameter("@BookStatusId", arg.BookStatusId == null ? string.Empty : arg.BookStatusId));
                SqlDataAdapter sqlAdapter = new SqlDataAdapter(cmd);
                sqlAdapter.Fill(dt);
                conn.Close();
            }
            return this.MapBookDataToList(dt);
        }

        //把資料map到list中
        private List<Models.Book> MapBookDataToList(DataTable Book)
        {
            List<Models.Book> result = new List<Book>();
            foreach (DataRow row in Book.Rows)
            {
                result.Add(new Book()
                {
                    BookId = (int)row["BookId"],
                    BookName = row["BookName"].ToString(),
                    BookClassName = row["BookClassName"].ToString(),
                    BookBoughtDate = row["BookBoughtDate"].ToString(),
                    BookStatusId = row["BookStatusId"].ToString(),
                    BookStatusName = row["BookStatusName"].ToString(),
                    BookKeeperEName = row["BookKeeperEName"].ToString()
                });
            }
            return result;
        }

        //新增書籍
        public int InsertBook(Models.Book Book)
        {
            string sql = @"INSERT INTO BOOK_DATA
                            ( BOOK_NAME,    BOOK_CLASS_ID,    BOOK_AUTHOR,    BOOK_BOUGHT_DATE,    BOOK_PUBLISHER,
                              BOOK_NOTE,    BOOK_STATUS,    BOOK_KEEPER,    CREATE_DATE,    CREATE_USER,
                              MODIFY_DATE,    MODIFY_USER )
                           VALUES
                            ( @BookName,    @BookClassId,    @BookAuthor,    @BookBoughtDate,    @BookPublisher,
                              @BookNote,    @BookStatusId,    @BookKeeperId,    GETDATE(),    '3139',
                              GETDATE(),    '3139' )";
            int BookId;
            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@BookName", Uri.UnescapeDataString(Book.BookName)));//特殊符號解碼
                cmd.Parameters.Add(new SqlParameter("@BookClassId", Book.BookClassId));
                cmd.Parameters.Add(new SqlParameter("@BookAuthor", Uri.UnescapeDataString(Book.BookAuthor)));
                cmd.Parameters.Add(new SqlParameter("@BookBoughtDate", Book.BookBoughtDate));
                cmd.Parameters.Add(new SqlParameter("@BookPublisher", Uri.UnescapeDataString(Book.BookPublisher)));
                cmd.Parameters.Add(new SqlParameter("@BookNote", Uri.UnescapeDataString(Book.BookNote)));
                cmd.Parameters.Add(new SqlParameter("@BookStatusId", "A"));
                cmd.Parameters.Add(new SqlParameter("@BookKeeperId", String.Empty));
                SqlTransaction tran = conn.BeginTransaction();
                cmd.Transaction = tran;
                try
                {
                    BookId = Convert.ToInt32(cmd.ExecuteScalar());
                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
                finally
                {
                    conn.Close();
                }
            }
            return BookId;
        }

        //取得書籍的詳細資料
        public Models.Book GetBookDatail(String BookId)
        {
            DataTable dt = new DataTable();
            string sql = @"SELECT data.BOOK_ID AS BookId,
                                  data.BOOK_NAME AS BookName,
                                  data.BOOK_AUTHOR AS BookAuthor,
                                  data.BOOK_PUBLISHER AS BookPublisher,
                                  data.BOOK_NOTE AS BookNote,
                                  CONVERT(VARCHAR(10),data.BOOK_BOUGHT_DATE,111) AS BookBoughtDate,
                                  data.BOOK_CLASS_ID AS BookClassId,
                                  class.BOOK_CLASS_NAME AS BookClassName,
                                  data.BOOK_STATUS AS BookStatusId,
                                  code.CODE_NAME AS BookStatusName,
                                  data.BOOK_KEEPER AS BookKeeperId,
                                  member.USER_CNAME + '-' + member.USER_ENAME AS BookKeeperName,
                                  CONVERT(VARCHAR(10),data.CREATE_DATE,111) AS CreateDate,
                                  CONVERT(VARCHAR(10),data.MODIFY_DATE,111) AS ModifyDate
                           FROM BOOK_DATA data
                            LEFT JOIN BOOK_CLASS class
                                ON data.BOOK_CLASS_ID = class.BOOK_CLASS_ID
                            LEFT JOIN BOOK_CODE code
                                ON data.BOOK_STATUS = code.CODE_ID
                            LEFT JOIN MEMBER_M member
                                ON data.BOOK_KEEPER = member.USER_ID
                           WHERE data.BOOK_ID = @BookId
                            AND code.CODE_TYPE = 'BOOK_STATUS' ";
            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@BookId", BookId));

                SqlDataAdapter sqlAdapter = new SqlDataAdapter(cmd);
                sqlAdapter.Fill(dt);
                conn.Close();
            }
            return this.MapBookDetailToList(dt);
        }

        //把Detail的資料map到list中
        private Models.Book MapBookDetailToList(DataTable Book)
        {
            Models.Book result = new Book();
            foreach (DataRow row in Book.Rows)
            {

                result.BookId = (int)row["BookId"];
                result.BookName = row["BookName"].ToString();
                result.BookAuthor = row["BookAuthor"].ToString();
                result.BookPublisher = row["BookPublisher"].ToString();
                result.BookNote = row["BookNote"].ToString();
                result.BookBoughtDate = row["BookBoughtDate"].ToString();
                result.BookClassId = row["BookClassId"].ToString();
                result.BookClassName = row["BookClassName"].ToString();
                result.BookStatusId = row["BookStatusId"].ToString();
                result.BookStatusName = row["BookStatusName"].ToString();
                result.BookKeeperId = row["BookKeeperId"].ToString();
                result.BookKeeperName = row["BookKeeperName"].ToString();
                result.CreateDate = row["CreateDate"].ToString();
                result.ModifyDate = row["ModifyDate"].ToString();
                
            }
            return result;
        }

        public string GetBookStatusId(string BookId)
        {
            string sql = @"SELECT BOOK_STATUS 
                           FROM BOOK_DATA 
                           WHERE BOOK_ID = @BookId";
            string bookstatusid;
            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@BookId", BookId));
                SqlTransaction tran = conn.BeginTransaction();
                cmd.Transaction = tran;
                try
                {
                    bookstatusid = (string) cmd.ExecuteScalar();
                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
                finally
                {
                    conn.Close();
                }
            }
            return bookstatusid;
        }

        //刪除書籍
        public bool DeleteBook(string BookId)
        {
            bool deletesuccess=false;
            string bookstatus = this.GetBookStatusId(BookId);
            if(bookstatus == "A" || bookstatus == "U")
            {
                string sql = @"DELETE FROM BOOK_DATA WHERE BOOK_ID = @BookId";
                using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.Add(new SqlParameter("@BookId", BookId));
                    SqlTransaction tran = conn.BeginTransaction();
                    cmd.Transaction = tran;
                    try
                    {
                        cmd.ExecuteNonQuery();
                        tran.Commit();
                        deletesuccess = true;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            return deletesuccess;
        }

        //更新書籍
        public void UpdateBook(Models.Book Book)
        {
            string sql = @"UPDATE BOOK_DATA
                           SET BOOK_NAME = @BookName,
                               BOOK_AUTHOR = @BookAuthor,
                               BOOK_PUBLISHER = @BookPublisher,
                               BOOK_NOTE = @BookNote,
                               BOOK_BOUGHT_DATE = @BookBoughtDate,
                               BOOK_CLASS_ID = @BookClassId,
                               BOOK_STATUS = @BookStatusId,
                               BOOK_KEEPER = @BookKeeperId,
                               MODIFY_DATE = GETDATE()
                           WHERE BOOK_ID = @BookId";
            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@BookName", Uri.UnescapeDataString(Book.BookName)));
                cmd.Parameters.Add(new SqlParameter("@BookClassId", Book.BookClassId));
                cmd.Parameters.Add(new SqlParameter("@BookAuthor", Uri.UnescapeDataString(Book.BookAuthor)));
                cmd.Parameters.Add(new SqlParameter("@BookBoughtDate", Book.BookBoughtDate));
                cmd.Parameters.Add(new SqlParameter("@BookPublisher", Uri.UnescapeDataString(Book.BookPublisher)));
                cmd.Parameters.Add(new SqlParameter("@BookNote", Uri.UnescapeDataString(Book.BookNote)));
                cmd.Parameters.Add(new SqlParameter("@BookStatusId", Book.BookStatusId));
                cmd.Parameters.Add(new SqlParameter("@BookKeeperId", Book.BookKeeperId == null ? string.Empty : Book.BookKeeperId));
                cmd.Parameters.Add(new SqlParameter("@BookId", Book.BookId));
                SqlTransaction tran = conn.BeginTransaction();
                cmd.Transaction = tran;
                try
                {
                    cmd.ExecuteNonQuery();
                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        //為了自動完成書名 取得所有書籍名稱
        public List<string> GetBookName()
        {
            DataTable dt = new DataTable();
            string sql = @"SELECT DISTINCT BOOK_NAME FROM BOOK_DATA";
            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlDataAdapter sqlAdapter = new SqlDataAdapter(sql, conn);
                sqlAdapter.Fill(dt);
                conn.Close();
            }

            List<String> result = new List<String>();
            foreach (DataRow row in dt.Rows)
            {
                result.Add((row["BOOK_NAME"].ToString()));
            }
            return result;
        }

    }
}