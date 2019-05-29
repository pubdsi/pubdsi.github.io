using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;
 
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Text;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace bbsales.common
{
    /// <summary>
    /// $codebehindclassname$ 的摘要描述
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class bbsales : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            string cmdstr="";
            if (context.Request.Params["cs"] == "bindpdtcate")
            {
                cmdstr = @"SELECT         id, cate FROM pdtcate";
                context.Response.Write(tojson(cmdstr));
            }
            if (context.Request.Params["cs"] == "supply")
            {
                string array = context.Request.Params["array"] ?? string.Empty;
                List<pdt> pdt = JsonConvert.DeserializeObject<List<pdt>>(array);
                cmdstr = @"INSERT INTO supply (empno)VALUES         (@empno);SELECT SCOPE_IDENTITY()";
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["bearbigConnectionString"].ToString());
                SqlCommand cmd = new SqlCommand(cmdstr, conn);
                 
                cmd.Parameters.AddWithValue("@empno", HttpContext.Current.Session["empno"]);


                conn.Open();
                string supid = cmd.ExecuteScalar().ToString();
                conn.Close();
                cmdstr = "";
                conn.Open();
                for (int i = 0; i < pdt.Count; i++)
                {
                    cmdstr = "INSERT INTO supplydtl (supid, pdtid, amt)VALUES(@odrid,@pdtid, @amt)";
                    cmd = new SqlCommand(cmdstr, conn);
                    cmd.Parameters.AddWithValue("@odrid", supid);
                    cmd.Parameters.AddWithValue("@pdtid", pdt[i].pdtid);
                    cmd.Parameters.AddWithValue("@amt", pdt[i].amt);
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
                  cmd = new SqlCommand("", conn);
                  cmd.CommandText = "supply2stock";
                  cmd.Parameters.AddWithValue("@id", supid);
                 
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();


            }
            if (context.Request.Params["cs"] == "odr")
            {
                string array = context.Request.Params["array"] ?? string.Empty;
                List<pdt> pdt = JsonConvert.DeserializeObject<List<pdt>>(array);
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["bearbigConnectionString"].ToString());
                SqlCommand cmd = new SqlCommand();
                string odrid = "";
                string valid="";
                //foreach(var pdts in pdt ){
                    if (pdt.Count==0)
                        valid = "無商品";
                //}
                //檢查未領訂單
                cmdstr = @"SELECT COUNT(*) AS Expr1 FROM odr WHERE         (empno = @empno) AND (chk = 0)";
                cmd = new SqlCommand(cmdstr, conn);
                cmd.Parameters.AddWithValue("@empno", HttpContext.Current.Session["empno"]);
                conn.Open();
                string openodrcnt = cmd.ExecuteScalar().ToString();
                conn.Close();
                if(openodrcnt!="0")
                    valid = "未領訂單";
                if (valid=="")
                {
                    cmdstr = @"INSERT INTO odr (empno)VALUES         (@empno);SELECT SCOPE_IDENTITY()";


                    cmd = new SqlCommand(cmdstr, conn);
                    //cmd.CommandText = "odr2stock";
                    cmd.Parameters.AddWithValue("@empno", HttpContext.Current.Session["empno"]);


                    conn.Open();
                    odrid = cmd.ExecuteScalar().ToString();
                    conn.Close();

                    cmdstr = "";
                    conn.Open();
                    for (int i = 0; i < pdt.Count; i++)
                    {
                        if (pdt[i] != null)
                        {
                            cmdstr = "INSERT INTO odrdtl (odrid, pdtid, amt)VALUES(@odrid,@pdtid, @amt)";
                            cmd = new SqlCommand(cmdstr, conn);
                            cmd.Parameters.AddWithValue("@odrid", odrid);
                            cmd.Parameters.AddWithValue("@pdtid", pdt[i].pdtid);
                            cmd.Parameters.AddWithValue("@amt", pdt[i].amt);

                            cmd.ExecuteNonQuery();
                        }
                    }
                    conn.Close();
                    valid = "成功";
                }
                context.Response.Write( valid ); 
                  
                 
                 

            }

            if (context.Request.Params["cs"] == "getpdt")
            {

                cmdstr = @"select   pdt1.id, loc, pdtname, unit, maxoffer, safestock,stock, cateid,cate,status
 from pdt1 inner join pdtcate on cateid=pdtcate.id where 1=1 ";
                
                context.Response.Write(tojson(cmdstr));
            }

            if (context.Request.Params["cs"] == "getodrpdt")
            {
                cmdstr = @"select   pdt1.id, loc, pdtname, unit, maxoffer, safestock, stock, cateid,cate,status
 from pdt1 inner join pdtcate on cateid=pdtcate.id where status=1 ";
                if (context.Request.Params["key"] != "")
                {
                    cmdstr += "and pdtname like '%" + context.Request.Params["key"] + "%'";
                }
                //if (context.Request.Params["cate"] != "")
                //{
                //    cmdstr += "and cateid = '" + context.Request.Params["cate"] + "'";
                //}
                context.Response.Write(tojson(cmdstr));
            }
            if (context.Request.Params["cs"] == "getodr")
            {
                cmdstr = @"SELECT         o.id,(SELECT cast(pdtname AS NVARCHAR )+'*'+cast(amt AS NVARCHAR ) + ',' from odrdtl
inner join pdt1 p on pdtid=p.id
where odrid =  o.id
FOR XML PATH('')) as items, orderno ,o.purchasedate, o.chk, o.empno, aa.name, o.crtime

FROM             odr AS o 
--  INNER JOIN pdt AS p ON o.pdtid = p.id 
inner join ideas_sso.dbo.ad_account aa on aa.empno=o.empno 
where chk=0";

                context.Response.Write(tojson(cmdstr));
            }
            if (context.Request.Params["cs"] == "purchase") {
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["bearbigConnectionString"].ToString());
                SqlCommand cmd = new SqlCommand(cmdstr, conn);
                cmd.CommandText = "odr2stock";
                cmd.Parameters.AddWithValue("@odrid", context.Request.Params["id"]);
                cmd.Parameters.AddWithValue("@cardnum", context.Request.Params["cardnum"]);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                string ret = cmd.ExecuteScalar().ToString();
                if (ret == "1")
                {
                    context.Response.Write("刷卡成功");
                }
                else {
                    context.Response.Write("刷卡失敗,洽管理員#2531");
                }
                conn.Close();
            }
            if (context.Request.Params["cs"] == "replenish") { 
            
            }
            if (context.Request.Params["cs"] == "getodrdtl")
            {
                cmdstr = @"SELECT        p.id, pdtname, amt ,v.name,unit from odrdtl
inner join pdt1 p on pdtid=p.id INNER JOIN
                          odr ON odrdtl.odrid = odr.id INNER JOIN
                          IDEAS_SSO.dbo.View_所內在職同仁 AS v ON v.EMPNO = odr.empno

where odrid =  '" + context.Request.Params["id"]+"' ";

                context.Response.Write(tojson(cmdstr));
            }
            if (context.Request.Params["cs"] == "getproductdtl")
            {
                string rtn =
                tojson(@"SELECT       pd.商品編號, pd.顏色, ps.數量, pd.idproductdetail, ps.尺寸, pd.filename, pd.iconname
FROM          productdetail pd LEFT JOIN
                    pdtstock ps ON pd.idproductdetail = ps.psid
WHERE       (pd.商品編號 = '" + context.Request.Params["pno"] + "')");
                context.Response.Write(rtn);
            }


            if (context.Request.Params["cs"] == "getproductlist")
            {
                string rtn =
                tojson(@"SELECT       p.idproduct, p.商品編號, p.名稱, p.價格,p.cno  ,(SELECT top 1       filename
FROM          productdetail
WHERE        商品編號 = p.商品編號  order by idproductdetail   ) as filename 
                      
FROM          product p ");
                context.Response.Write(rtn);
            }





            if (context.Request.Params["cs"] == "addproductdtl")
            {
                 cmdstr=@"INSERT INTO productdetail
                    (商品編號, 顏色  )
VALUES       ('" + context.Request.Params["pno"] + "','" + context.Request.Params["color"] + "')";
               SqlConnection objConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["bearbigConnectionString"].ToString());
               SqlCommand objCommand = new SqlCommand(cmdstr, objConnection);
               objConnection.Open();
               objCommand.ExecuteNonQuery();
               objConnection.Close();
            }

            if (context.Request.Params["cs"] == "savePdt")
            {
                 string[] arys=context.Request.Params["ary"].Split(',');
                //string rtn = HttpContext.Current.Server.UrlDecode(context.Request.Params["parameter1"]);


                //HttpContext.Current.Session["cart"] = HttpContext.Current.Server.UrlDecode(context.Request.Params["parameter1"]);
                   cmdstr = "update product set 名稱 ='" + arys[1] + "', 價格 ='" + arys[2] + "', cno ='" + arys[3] + "' where idproduct='" + arys[0] + "'";


                 SqlConnection objConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["bearbigConnectionString"].ToString());
                 SqlCommand objCommand = new SqlCommand( cmdstr, objConnection);
                 objConnection.Open();
                 context.Response.Write(objCommand.ExecuteNonQuery());
                 objConnection.Close();

                 
            }
            if (context.Request.Params["cs"] == "insertPdt")
            {
                string[] arys = context.Request.Params["ary"].Split(',');
                //string rtn = HttpContext.Current.Server.UrlDecode(context.Request.Params["parameter1"]);


                //HttpContext.Current.Session["cart"] = HttpContext.Current.Server.UrlDecode(context.Request.Params["parameter1"]);
                  cmdstr = @"INSERT INTO product
                    ( 商品編號, 名稱, 價格, cno)  
VALUES       ('" + arys[0] + "',N'" + arys[1] + "','" + arys[2] + "','" + arys[3] + "')  ";


                SqlConnection objConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["bearbigConnectionString"].ToString());
                SqlCommand objCommand = new SqlCommand(cmdstr, objConnection);
                objConnection.Open();
                context.Response.Write(objCommand.ExecuteNonQuery());
                objConnection.Close();


            }
        }
        public class pdt
        {
            public string amt { get; set; }
            public string  pdtname { get; set; }
            public string pdtid { get; set; }
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }


        StringBuilder sb = new StringBuilder();
        public string tojson(string cmdstr)
        {
            sb.Length = 0;
            sb.Append("[");
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["bearbigConnectionString"].ToString());
            conn.Open();
            SqlCommand cmd = new SqlCommand();


            cmd.CommandText = cmdstr;
            cmd.Connection = conn;
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                sb.Append("{");
                for (int i = 0; i < sdr.FieldCount; i++)
                {
                    try
                    {
                        sb.Append("\"" + sdr.GetName(i) + "\":\"" + sdr[i].ToString().Replace("\"", "\\\"") + "\",");
                    }
                    catch (Exception ee)
                    {

                    }

                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append("},");

            }
            if (sdr.HasRows)
                sb.Remove(sb.Length - 1, 1);

            sb.Append("]");
            conn.Close();

            return sb.ToString();
        }





    }
}
