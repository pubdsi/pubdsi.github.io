using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;

namespace bbsales.common
{
    /// <summary>
    ///fileUploader 的摘要描述
    /// </summary>
    public class fileUploader : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            try
            {
                string dirFullPath = HttpContext.Current.Server.MapPath("~/UploadFile/");
                string[] files;
                //int numFiles;
                files = System.IO.Directory.GetFiles(dirFullPath);
                //numFiles = files.Length;
                //numFiles = numFiles + 1;
                string str_image = "";

                foreach (string s in context.Request.Files)
                {
                    HttpPostedFile file = context.Request.Files[s];
                    string fileName = file.FileName;
                    string fileExtension = file.ContentType;

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        //string fName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                        string fName = "";
                         
                            fName = fileName.Substring(fileName.LastIndexOf("\\") + 1);

                        //string fname = files.tostringsubstring(filename.lastIndexOf("\\") + 1, filename.lastIndexOf("."))
                        fileExtension = Path.GetExtension(fileName);
                        str_image = context.Request["filename"] + fileExtension;
                        string pathToSave_100 = HttpContext.Current.Server.MapPath("~/UploadFile/"  ) +context.Request["id"]+ ".jpg";
                        string filepath = HttpContext.Current.Server.MapPath("~/UploadFile/"  );
                        if (!Directory.Exists(filepath))
                        {
                            Directory.CreateDirectory(filepath);
                        }
                        file.SaveAs(pathToSave_100);
                         
                    }
                }
                //  database record update logic here  ()

                context.Response.Write(str_image);
            }
            catch (Exception ac)
            {

            }
        }
         
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

    }
}