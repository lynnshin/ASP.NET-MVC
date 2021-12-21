$(document).ready(function () {
    //書籍類別的選單
    $("#BookClassId").kendoDropDownList({
        optionLabel: "請選擇...",
        dataTextField: "Text",
        dataValueField: "Value",
        dataSource: {
            transport: {
                read: {
                    url: "/Book/GetDropDownListData",
                    type: "post",
                    dataType: "json",
                    data: { category: "BookClassId" }
                }
            }
        }
    });

    //借閱人的選單
    $("#BookKeeperId").kendoDropDownList({
        optionLabel: "請選擇...",
        dataTextField: "Text",
        dataValueField: "Value",
        dataSource: {
            transport: {
                read: {
                    url: "/Book/GetDropDownListData",
                    type: "post",
                    dataType: "json",
                    data: { category: "BookKeeperId" }
                }
            }
        }
    });

    //書籍狀態的選單
    $("#BookStatusId").kendoDropDownList({
        optionLabel: "請選擇...",
        dataTextField: "Text",
        dataValueField: "Value",
        dataSource: {
            transport: {
                read: {
                    url: "/Book/GetDropDownListData",
                    type: "post",
                    dataType: "json",
                    data: { category: "BookStatusId" }
                }
            }
        }
    });

    //購書日期
    $("#BookBoughtDate").kendoDatePicker({
        format: "yyyy/MM/dd",
        value: new Date(),
        dateInput: true,
    });

    //Notification
    $("#notification").kendoNotification({
        height: 70,
        width: 180,
        position: {
            top: 100,
            right: 30
        },
        autoHideAfter: 2000,
        animation: {
            open: {
                effects: "slideIn:down"
            },
            close: {
                effects: "slideIn:down",
                reverse: true
            }
        }
    });     

    /* 確認是否刪除的視窗 */
    $("#make_sure_window").kendoWindow({
        width: "200px",
        title: "確認刪除",
        modal: true
    });

    /*按下將不執行刪除動作 */
    $("#NO").kendoButton({
        click: function () {
            //把確認視窗關掉
            $("#make_sure_window").data("kendoWindow").close();
        }
    });

    //查詢書籍
    $("#SearchBook").kendoButton({
        click: SearchBook
    });

    //清空查詢表單
    $("#ClearSearch").kendoButton({
        click: function () {
            $("#BookName").val("");
            $("#BookClassId").data("kendoDropDownList").select(0);
            $("#BookKeeperId").data("kendoDropDownList").select(0);
            $("#BookStatusId").data("kendoDropDownList").select(0);
        }
    });

    //進入新增頁面
    $("#InsertBook").kendoButton({
        click: function () {
            window.location.href = '/Book/InsertBook';
        }
    });

    //進入修改畫面
    $("#UpdateBook").kendoButton({
        click: function () {
            window.location.href = '/Book/UpdateBook?BookId=' + GetId();
        }
    });   

    //在Search裡面的刪除確定鍵
    $("#DeleteinSearchYES").kendoButton({
        click: function () {
            $.ajax({
                url: "/Book/DeleteBook",
                type: "post",
                dataType: "json",
                data: {
                    BookId: BookData.BookId
                },
                success: function (response) {
                    if (response == true) {
                        $("#BookGrid").data("kendoGrid").dataSource.data().remove(BookData);
                        $("#notification").data("kendoNotification").show("刪除成功", "success");
                    }
                    else {
                        $("#notification").data("kendoNotification").show("已借出，無法刪除", "error");
                    }
                },
                error: function () {
                    $("#notification").data("kendoNotification").show("系統發生錯誤", "error");
                }
            });            
            $("#make_sure_window").data("kendoWindow").close();
        }
    });    

    //在Update頁面的刪除鍵 會打開確認視窗
    $("#DeleteBook").kendoButton({
        click: function () {
            var BookStatus = $("#BookStatusId").data("kendoDropDownList").value();
            //是A-可借閱以及U-不可借閱才能刪
            if (BookStatus == "A" || BookStatus == "U") {
                $("#make_sure_window").data("kendoWindow").center().open();
            }
            else if (BookStatus == "B" || BookStatus == "C") {
                $("#notification").data("kendoNotification").show("已借出，無法刪除", "info");
            }
        }
    });

    //在Update裡面的刪除確定鍵
    $("#DeleteinUpdateYES").kendoButton({
        click: function () {
            $.ajax({
                url: "/Book/DeleteBook",
                type: "post",
                dataType: "json",
                data: {
                    BookId: GetId()
                },
                success: function (response) {
                    if (response == true) {
                        $("#notification").data("kendoNotification").show("刪除成功", "success");
                        //隔一秒後跳轉頁面
                        setTimeout(function () {
                            window.location.href = "/Book/SearchBook";
                        }, 1000);
                    }
                    else {
                        $("#notification").data("kendoNotification").show("已借出，無法刪除", "error");
                    }
                },
                error: function () {
                    $("#notification").data("kendoNotification").show("系統發生錯誤", "error");
                }
            });
            $("#make_sure_window").data("kendoWindow").close();
        }
    });

    //確認Update表格有填完整
    $("#UpdateForm").kendoValidator();

    //在Update頁面按存檔執行修改動作
    $("#SaveUpdate").kendoButton({
        click: function () {
            var validatable = $("#UpdateForm").kendoValidator().data("kendoValidator");
            if (validatable.validate()) {
                $.ajax({
                    url: "/Book/UpdateBook",
                    type: "post",
                    dataType: "json",
                    data: {
                        BookId: GetId(),                        
                        BookName: encodeURIComponent($("#BookName").val()),//特殊符號編碼
                        BookAuthor: encodeURIComponent($("#BookAuthor").val()),
                        BookPublisher: encodeURIComponent($("#BookPublisher").val()),
                        BookNote: encodeURIComponent($("#BookNote").val()),
                        BookBoughtDate: kendo.toString($("#BookBoughtDate").data("kendoDatePicker").value(), 'yyyy/MM/dd'),
                        BookClassId: $("#BookClassId").data("kendoDropDownList").value(),
                        BookStatusId: $("#BookStatusId").data("kendoDropDownList").value(),
                        BookKeeperId: $("#BookKeeperId").data("kendoDropDownList").value()
                    },
                    success: function (response) {
                        if (response) {
                            $("#notification").data("kendoNotification").show("修改成功", "success");
                            //隔一秒後跳轉頁面
                            setTimeout(function () {
                                window.location.href = "/Book/DetailBook?BookId=" + GetId();
                            }, 1000);
                        }
                        else {
                            //選擇已借出可是沒選借閱人
                            $("#notification").data("kendoNotification").show("請選借閱人", "info");
                        }
                    },
                    error: function () {
                        $("#notification").data("kendoNotification").show("系統發生錯誤", "error");
                        window.location.href = '/Book/DetailBook?BookId=' + GetId();
                    }
                });
            }
        }
    });

    //確認Insert表格有填完整
    $("#InsertForm").kendoValidator();

    //在Insert頁面按存檔執行新增動作
    $("#SaveInsert").kendoButton({
        click: function () {
            if ($("#InsertForm").kendoValidator().data("kendoValidator").validate()) {
                $.ajax({
                    url: "/Book/InsertBook",
                    type: "post",
                    dataType: "json",
                    data: {
                        BookName: encodeURIComponent($("#BookName").val()),
                        BookAuthor: encodeURIComponent($("#BookAuthor").val()),
                        BookPublisher: encodeURIComponent($("#BookPublisher").val()),
                        BookNote: encodeURIComponent($("#BookNote").val()),
                        BookBoughtDate: kendo.toString($("#BookBoughtDate").data("kendoDatePicker").value(), 'yyyy/MM/dd'),
                        BookClassId: $("#BookClassId").data("kendoDropDownList").value(),
                    },
                    success: function (response) {
                        if (response == true) {
                            $("#notification").data("kendoNotification").show("新增成功", "success");
                            //清空表單
                            $("#BookName").val("");
                            $("#BookAuthor").val("");
                            $("#BookPublisher").val("");
                            $("#BookNote").val("");
                            $("#BookClassId").data("kendoDropDownList").select(0);
                        }
                        else {
                            $("#notification").data("kendoNotification").show("新增失敗", "error");
                        }
                    },
                    error: function () {
                        $("#notification").data("kendoNotification").show("系統發生錯誤", "error");
                    }
                });
            }
        }
    });

    //取消新增
    $("#CancelInsert").kendoButton({
        click: function () {
            window.location.href = '/Book/SearchBook';
        }
    });    

});

//搜尋功能
function SearchBook() {
    $("#BookGrid").remove();
    $("#ForGrid").append("<div id='BookGrid'></div>");

    $("#BookGrid").kendoGrid({
        dataSource: {
            transport: {
                read: {
                    url: "/Book/SearchBook",
                    type: "post",
                    dataType: "json",
                    data: {
                        BookName: $("#BookName").val(),
                        BookClassId: $("#BookClassId").data("kendoDropDownList").value(),
                        BookKeeperId: $("#BookKeeperId").data("kendoDropDownList").value(),
                        BookStatusId: $("#BookStatusId").data("kendoDropDownList").value()
                    }
                }
            },
            pageSize: 20,
        },
        height: 550,
        sortable: true,
        pageable: {
            input: true,
            numeric: false
        },
        columns: [
            { field: "BookClassName", title: "圖書類別", width: "20%" },
            { field: "BookName", title: "圖書名稱", width: "48%", template: '<a href="/Book/DetailBook?BookId=#=BookId#">#=BookName#</a>' },
            { field: "BookBoughtDate", title: "購書日期", width: "12%" },
            { field: "BookStatusName", title: "借閱狀態", width: "11.5%" },
            { field: "BookKeeperEName", title: "借閱人", width: "10%" },
            { command: { text: "編輯", click: EditBook }, title: " ", width: "100px" },
            { command: { text: "刪除", click: Delete }, title: " ", width: "100px" }
        ]
    });
}

//取得BookId
function GetId() {
    var url = window.location.href; /*取得整串url*/
    var index = url.indexOf("?BookId="); /*找到"?BookId"的起始位置*/
    if (index != -1) { /*確認是否有資料傳遞*/
        return url.substring(index + 8); /*從?後面第8個字開始取*/
    }
}

//Grid的編輯動作
function EditBook(e) {
    e.preventDefault();
    var BookData = this.dataItem($(e.target).closest("tr"));
    window.location.href = '/Book/UpdateBook?BookId=' + BookData.BookId;
}

//Grid的刪除鍵 會打開確認視窗
function Delete(e) {
    e.preventDefault();
    BookData = this.dataItem($(e.target).closest("tr"));
    $.ajax({
        url: "/Book/CheckBookStatus",
        type: "post",
        dataType: "json",
        data: {
            BookId: BookData.BookId
        },
        success: function (response) {
            if (response == true) {
                $("#make_sure_window").data("kendoWindow").center().open();
            }
            else if (response == false) {
                $('#BookGrid').data('kendoGrid').dataSource.read();
                $("#notification").data("kendoNotification").show("已被借出，無法刪除", "error");
            }
            else {
                $('#BookGrid').data('kendoGrid').dataSource.read();
                $("#notification").data("kendoNotification").show("此書已不存在", "error");
            }
        },
        error: function () {
            $("#notification").data("kendoNotification").show("系統發生錯誤", "error");
        }
    });    
}