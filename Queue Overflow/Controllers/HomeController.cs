﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QueueOverflow.Filters;
using System.Web.Security;
using Newtonsoft.Json;
using QueueOverflow.Models;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Configuration;
using Utilities;
using QueueOverflow.Libraries;
using System.Web.Services;
namespace QueueOverflow.Controllers
{
    public class HomeController : BaseController
    {
        private static readonly log4net.ILog _Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        string BaseURL = ConfigurationManager.AppSettings["UserServiceURL"].ToString();

        #region Register

        public ActionResult Register()
        {
            ViewBag.Pass = string.Empty;
            return View(new User());
        }
        #endregion

        #region RegisterUser
        [WebMethod]
        public JsonResult RegisterUser(User objUser)
        {
            _Logger.Info("Method Start");

            string URL = BaseURL + "Create";
            _Logger.Debug("URL = " + URL);

            objUser.UserId = Guid.NewGuid().ToString();
            try
            {
                LogHelper.LogMaker(objUser);
                string ResponseFromServer = ServiceConsumer.Post(URL, objUser);
                _Logger.Debug("ResponseFromServer = " + ResponseFromServer);

            }

            catch (Exception exception)
            {
                _Logger.Error(exception.Message, exception);
                var ErrorDetail = JObject.Parse(exception.Message)["ErrorDetail"].ToString();
                return Json(new { Status = ErrorDetail });
            }

            _Logger.Info("Method End");
            return Json(new { Status = "Success" });
        }
        #endregion

        #region Login
        public ActionResult Login()
        {
            _Logger.Debug("URLs = ");
            ViewBag.Pass = string.Empty;
            if (Request.Cookies["MXAuthCookie"] != null)
            {
                return RedirectToAction("Index", "User");
            }
            return View();
        }
        #endregion

        #region Login Post
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Login(User user) //passing the username and password
        {
            string URL = BaseURL + "RetrieveUser";
            _Logger.Debug("URL = " + URL);
            try
            {
                LogHelper.LogMaker(user);
                string UserDataResponse = ServiceConsumer.Post(URL, user);
                _Logger.Debug("UserDataResponse = " + UserDataResponse);
                User UserData = JsonConvert.DeserializeObject<User>(UserDataResponse);
                LogHelper.LogMaker(UserData);
                if (String.IsNullOrEmpty(UserData.UserName))
                {
                    ViewBag.Pass = "UserName or Password is incorrect";
                    ModelState.Remove("Password");
                    return View();
                }

                SigninManagement.SetAuthCookie(user, UserDataResponse);
                return RedirectToAction("Index", "User");

            }

            catch (Exception exception)
            {
                _Logger.Error(exception.Message, exception);
                throw exception;
            }

        }


        #endregion

        #region Logout
        public ActionResult Logout()
        {

            HttpContext.Response.Cookies.Remove("MXAuthCookie");
            HttpContext.Response.Cookies["MXAuthCookie"].Value = null;
            //Clearing the cookies of the response doesn't instruct the
            //browser to clear the cookie, it merely does not send the cookie back to the browser.
            //To instruct the browser to clear the cookie you need to tell it the cookie has expired
            HttpContext.Response.Cookies["MXAuthCookie"].Expires = DateTime.Now.AddMonths(-1);
            return RedirectToAction("Login", "Home");
        }
        #endregion

        #region Password
        public ActionResult Password()
        {
            ViewBag.Pass = string.Empty;
            return View();
        }
        #endregion

        #region Password Post
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Password(User user) //passing the username and email
        {
            string URL = BaseURL + "RetrieveUser";
            _Logger.Debug("URL = " + URL);
            try
            {
                LogHelper.LogMaker(user);
                string UserDataResponse = ServiceConsumer.Post(URL, user);
                _Logger.Debug("UserDataResponse = " + UserDataResponse);

                User UserData = JsonConvert.DeserializeObject<User>(UserDataResponse);
                LogHelper.LogMaker(UserData);

                if (UserData.Password != null)
                {
                    ViewBag.Pass = "Password : " + UserData.Password;
                    return View();
                }
                else
                {
                    ViewBag.Pass = "Username or Email is incorrect";
                    return View();
                }
            }

            catch (Exception exception)
            {
                _Logger.Error(exception.Message, exception);
                throw exception;
            }
        }
        #endregion

    }
}
