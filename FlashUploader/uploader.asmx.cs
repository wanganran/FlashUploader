using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using System.Web.Services.Protocols;
using System.Web.Script.Serialization;
using System.IO;

namespace FlashUploader
{
    /// <summary>
    /// uploader 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。
    // [System.Web.Script.Services.ScriptService]
    public class uploader : System.Web.Services.WebService
    {
        public static SqliteDBAdapter adapter = new SqliteDBAdapter("D:\\FlashUploader.db3");
        private void ReturnJson(object o)
        {
            var res = new JavaScriptSerializer().Serialize(o);

            HttpContext.Current.Response.Write(res);
            HttpContext.Current.Response.End();
        }

        [WebMethod]
        public void HelloWorld()
        {
            HttpContext.Current.Response.Write("sss");
            HttpContext.Current.Response.End();
            //return "Hello World";
        }

        struct EntryResult
        {
            public int ID;
            public string filename;
            public int length;
            public long time;
        }

        [WebMethod]
        public void GetFileList()
        {
            List<EntryResult> result = new List<EntryResult>();
            var res=adapter.QueryAll().Select(en => new EntryResult { ID = en.ID, filename = en.filename, length = en.length, time = en.time }).ToArray();
            ReturnJson(res);
        }

        struct RemoveResult
        {
            public bool succeed;
        }

        [WebMethod]
        public void RemoveFile(int ID)
        {
            var entry = adapter.QueryByID(ID);
            if (entry == null) ReturnJson(new RemoveResult { succeed = false });
            adapter.RemoveByID(ID);
            File.Delete(entry.path);
            ReturnJson(new RemoveResult { succeed = true });
        }

        [WebMethod]
        public void GetFile(int ID)
        {
            var fileEntry = adapter.QueryByID(ID);
            if (fileEntry == null || !File.Exists(fileEntry.path))
            {
                HttpContext.Current.Response.StatusCode = 404;
                HttpContext.Current.Response.End();
            }
            var resp=HttpContext.Current.Response;
            
            resp.AddHeader("Content-Length",new FileInfo(fileEntry.path).Length.ToString());
            resp.AddHeader("Content-Disposition", "attachment; filename=" + fileEntry.filename);
            
            BinaryReader sr = new BinaryReader(new FileStream(fileEntry.path, FileMode.Open));
            byte[] buffer = new byte[1024];
            while (true)
            {
                int len = sr.Read(buffer, 0, 1024);
                if (len <= 0) break;
                if (!resp.IsClientConnected) break;
                resp.OutputStream.Write(buffer, 0, len);
                resp.Flush();
            }
            sr.Close();
            resp.End();
        }

        struct QueryResult
        {
            public bool exists;
            public int ID_if_any;
        }

        [WebMethod]
        public void CheckSize(int size)
        {
            var res=adapter.QueryByLength(size);
            ReturnJson(res == null ? new QueryResult { exists = false, ID_if_any = 0 } : new QueryResult { exists = (res.Count!=0), ID_if_any = 0 });
        }

        [WebMethod]
        public void CheckMD5(string hex)
        {
            var res=adapter.QueryByChecksum(hex);
            ReturnJson(res == null ? new QueryResult { exists = false, ID_if_any = 0 } : new QueryResult { exists = true, ID_if_any = res.ID });
        }

        struct UploadResult{
            public bool success;
            public int ID;
        }

        public static int ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }  

        [WebMethod]
        public void UploadExisting(string name, int ID)
        {
            var entry = adapter.QueryByID(ID);
            entry.filename = name;
            entry.time = ConvertDateTimeInt(System.DateTime.Now);
            var _ID = adapter.Insert(entry);
            if (_ID < 0) ReturnJson(new UploadResult { success = false, ID = 0 });
            else ReturnJson(new UploadResult { success = true, ID = _ID });
        }

    }
}
