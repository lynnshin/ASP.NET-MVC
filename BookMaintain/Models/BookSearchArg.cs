using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BookMaintain.Models
{
    public class BookSearchArg
    {
        //書名
        [DisplayName("書名")]
        public string BookName { get; set; }

        //圖書類別代號
        [DisplayName("圖書類別")]
        public string BookClassId { get; set; }

        //借閱人代號
        [DisplayName("借閱人")]
        public string BookKeeperId { get; set; }

        //借閱狀態代號
        [DisplayName("借閱狀態")]
        public string BookStatusId { get; set; }
    }
}