using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.XPath;
using System.Security.Cryptography;
using System.Text;
using System.Data;

namespace HandsOnLab_Ver1._2.Controllers
{
    public class HandsOnLabController : Controller
    {
        readonly Models.EmployeeService employeeService = new Models.EmployeeService();
        readonly Models.FileService fileService = new Models.FileService();
        readonly Models.MeowService meowService = new Models.MeowService();
        readonly Models.CodeService codeService = new Models.CodeService();
        readonly Models.Logger logger = new Models.Logger();

        /// <summary>
        /// 登入畫面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 驗證登入使用者帳密
        /// </summary>
        /// <param name="employees"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Index(Models.Employees employees)
        {
            if (employeeService.IsValidUser(employees))
            {
                //string encrypted = fileService.aesEncryptBase64(employees.Account, "123");

                HttpCookie EmpCookies = new HttpCookie("EmpCookies");
                //EmpCookies.Value = encrypted;
                EmpCookies.Value = employees.Account;
                EmpCookies.Expires = DateTime.Now.AddMinutes(10);
                Response.Cookies.Add(EmpCookies);

                Models.Logger.Write(Models.Logger.LogCategoryEnum.Information, employees.Account + "嘗試登入，以密碼：" + employees.Password + "成功登入。");
                return Json(true);
            }
            else
            {
                Models.Logger.Write(Models.Logger.LogCategoryEnum.Information, employees.Account + "嘗試登入，以密碼：" + employees.Password + "登入失敗。");
                return Json(false);
            }
        }
        /// <summary>
        /// 註冊畫面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        /// <summary>
        /// 使用者註冊
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Register(Models.Employees employees, string checkPassword)
        {
            if (employees.Password == checkPassword)
            {
                if (employeeService.Register(employees))
                {
                    return Json(true);
                }
                else
                {
                    return Json(false);
                }
            }
            else
            {
                return Json(false);
            }
        }
        /// <summary>
        /// 選擇畫面
        /// </summary>
        /// <returns></returns>
        public ActionResult SectionList()
        {
            return View();
        }
        /// <summary>
        /// 相關資料下載區畫面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DataDownLoad()
        {
            var dir = new DirectoryInfo(Server.MapPath("~/App_Data/DownLoad/"));
            System.IO.FileInfo[] fileNames = dir.GetFiles("*.*");
            List<string> items = new List<string>();
            foreach (var file in fileNames)
            {
                items.Add(file.Name);
            }
            return View(items);
        }
        /// <summary>
        /// 下載功能
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Download(string fileName)
        {
            var filePath = "~/App_Data/DownLoad/" + fileName;
            //if (fileService.CheckFilePath(filePath))
            //{
            return File(filePath, "application/" + Path.GetExtension(filePath).Substring(1), Path.GetFileName(filePath));
            //}
            //else
            //    return File("~/App_Data/Error/Nope.txt", "application/txt", Path.GetFileName("~/App_Data/Error/Nope.txt"));
        }
        /// <summary>
        /// 檢視個人資料畫面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult PersonalProfile()
        {
            return View();
        }
        /// <summary>
        /// 查詢員工資料畫面
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult PersonalProfile(Models.Employees employee, string cookie)
        {
            //// Decrypt the bytes to a string.
            //string decrypted = fileService.aesDecryptBase64(cookie, "123");
            //List<int> permission = employeeService.CheckEmpPermissionLevel(decrypted);

            List<int> permission = employeeService.CheckEmpPermissionLevel(cookie);

            List<Models.Employees> employees = employeeService.GetEmployeesProfileByCondition(employee, permission);
            return Json(employees);
        }
        /// <summary>
        /// 資料加解密畫面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DataDecrypt()
        {
            return View();
        }
        [HttpPost]
        public JsonResult DataDecrypt(string password)
        {
            if (meowService.PasswordCheck(password))
                return Json(true);
            else
                return Json(false);
        }
        /// <summary>
        /// 取得部門資料
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetDropDownListData(string category)
        {
            List<SelectListItem> ListResult = null;
            switch (category)
            {
                case "Document":
                    ListResult = codeService.GetEmployeeDocument();
                    break;
                case "Gender":
                    ListResult = codeService.GetEmployeeGender();
                    break;
            }
            return Json(ListResult);
        }
        /// <summary>
        /// 修改員工資料畫面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult UpdateEmployeeProfile()
        {
            return View();
        }
        /// <summary>
        /// 修改員工資料
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="cookie"></param>
        /// <param name="checkPassword"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateEmployeeProfile(Models.Employees employee, string newPassword, string checkPassword)
        {
            if (employeeService.IsValidUser(employee) && newPassword == checkPassword)
            {
                if (employeeService.UpdateEmployeeProfile(employee, newPassword))
                {
                    return Json(true);
                }
                else
                {
                    return Json(false);
                }
            }
            else
            {
                return Json(false);
            }
        }
        [HttpPost]
        public JsonResult GetEmployeeDataByAccount(Models.Employees employee)
        {
            Models.Employees employees = employeeService.GetEmployeesProfileByAccount(employee);
            return Json(employees);
        }
        /// <summary>
        /// XSS Demo
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult XSSDemo()
        {
            return View();
        }
        
        [HttpGet]
        public ActionResult XXEDemo()
        {
            StreamReader sr = new StreamReader(Server.MapPath(@"\App_Data\LeaveOff.xml"));
            ViewBag.Text = sr.ReadToEnd();
            sr.Close();

            return View();
        }
        /// <summary>
        /// XXE Demo
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult XXEDemo(string context)
        {
            XmlDocument contextXML = new XmlDocument();
            //disable DTD ,Set XMLResolver to Null value
            //XmlReaderSettings rs = new XmlReaderSettings();
            //rs.DtdProcessing = DtdProcessing.Prohibit;
            //contextXML.XmlResolver = null;
            contextXML.LoadXml(context);
            XmlElement root = contextXML.DocumentElement;
            XPathNavigator nav = contextXML.CreateNavigator();
            //Parse the XML text we need .

            var test = contextXML.InnerText;

            return Json(contextXML.InnerText);
        }
        /// <summary>
        /// 敏感資訊透過錯誤訊息顯示至前端
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ExposureViaErrorMessage()
        {
            //try
            //{
                int i, j;
                i = 0;
                j = 1 / i;
                System.Data.SqlClient.SqlConnectionStringBuilder builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
                builder["Data Source"] = "(LocalDB)\\MSSQLLocalDB; AttachDbFilename =| DataDirectory | Sample.mdf";
                builder["AttachDbFilename"] = "C:\\Users\\User\\source\\repos\\HandsOnLab_ver1.0\\HandsOnLab_ver1.0\\App_Data\\Sample.mdf";
                builder["integrated Security"] = true;
                return View();
            //}
            //catch (Exception ex)
            //{
            //    Models.Logger.Write(Models.Logger.LogCategoryEnum.Error, ex.ToString());
            //    return View("Error");
            //}
        }
        /// <summary>
        /// Error Handle
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Error()
        {
            return View();
        }
    }
}