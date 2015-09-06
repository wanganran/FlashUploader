using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using System.IO;

namespace FlashUploader
{
    public partial class Upload : System.Web.UI.Page
    {
        private void ReturnJson(object o)
        {
            var res = new JavaScriptSerializer().Serialize(o);

            HttpContext.Current.Response.Write(res);
            HttpContext.Current.Response.End();
        }
        struct UploadResult
        {
            public bool success;
            public int ID;
        }
        private string ChangePath(string path, string suffix)
        {
            int i = 2;
            while (File.Exists(path + "(" + i.ToString() + ")" + suffix)) i++;
            return path + "(" + i.ToString() + ")" + suffix;
        }

        private string MultiPath(string path)
        {
            int i = path.Length - 1;
            while (i >= 0)
            {
                if (path[i] == '/' || path[i] == '\\') return ChangePath(path, "");
                if (path[i] == '.') return ChangePath(path.Substring(0, i), path.Substring(i + 1));
                i--;
            }
            return ChangePath(path, "");
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            var req = HttpContext.Current.Request;

            string name = req.QueryString["name"];
            if (name == null) return;
            int len = int.Parse(req.QueryString["len"]);
            if(req.Files["file"].ContentLength>0){
                if (req.Files["file"].ContentLength != len)
                {
                    ReturnJson(new UploadResult { success = false, ID=0 });
                    return;
                }
                var path=Server.MapPath("~/attachments/"+name);
                while (File.Exists(path))
                {
                    path = MultiPath(path);
                }

                req.Files["file"].SaveAs(path);
                
                //calc MD5
                var md5=System.Security.Cryptography.MD5.Create();
                byte[] md5arr=md5.ComputeHash(new FileStream(path,FileMode.Open));
                string hex = "";
                for (int i = 0; i < 16; i++)
                    hex += md5arr[i].ToString("x2");
                //update database
                var _ID = uploader.adapter.Insert(new Entry
                {
                    ID = 0,
                    filename = name,
                    path = path,
                    checksum = hex,
                    length = len,
                    time = uploader.ConvertDateTimeInt(DateTime.Now)
                });

                if (_ID < 0) ReturnJson(new UploadResult { success = false, ID = 0 });
                else ReturnJson(new UploadResult { success = true, ID = _ID });
            }
            else ReturnJson(new UploadResult { success = false, ID = 0 });
        
        }
    }
}