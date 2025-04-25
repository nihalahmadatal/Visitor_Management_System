using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Visitor_Management_System.Models;
using Visitor_Management_System.Models.Helpers;

namespace Visitor_Management_System.Controllers
{
    public class SecurityController : Controller
    {
        DB_VisitorEntities dbo = new DB_VisitorEntities();
        // GET: Security
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(string Username, string Password, string ConfirmPassword)
        {
            if(Password != ConfirmPassword)
            {
                ViewBag.Message = "Password do not match.";
                return View();
            }

            if(dbo.TblLogins.Any(u=>u.Username == Username))
            {
                ViewBag.Message = "Username already exists.";
                return View();
            }

            var newUser = new TblLogin
            {
                Username = Username,
                Password = PasswordHelper.HashPassword(Password),
                Role = "Security"
            };

            dbo.TblLogins.Add(newUser);
            if(dbo.SaveChanges() > 0)
            {
                Session["SecurityUser"] = newUser.Username;
                TempData["Message"] = "Registered successfully! Welcome to the dashboard.";

            }
            return RedirectToAction("Login", "Security");
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            string hashedInput = PasswordHelper.HashPassword(password);

            var user = dbo.TblLogins.FirstOrDefault(u => u.Username == username && u.Password == hashedInput);

            if (user != null && user.Role == "Security")
            {
                Session["SecurityUser"] = user.Username;
                return RedirectToAction("ShowVisitorPass", "Security");
            }
            ViewBag.Error = "Invalid Username or Password";
            return View();
        }
        public ActionResult Dashboard()
        {
            if (Session["SecurityUser"] == null)
                return RedirectToAction("Login");

            ViewBag.Username = Session["SecurityUser"];
            return View();
        }
        public ActionResult CameraScan()
        {
            if (Session["SecurityUser"] == null)
                return RedirectToAction("Login");

            return View();
        }
        public ActionResult Logout()
        {
            if (Session["SecurityUser"] == null)
                return RedirectToAction("Login");

            return View();
        }
        public ActionResult CheckIn()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CheckIn(int id)
        {
            var visitor = dbo.TblVisitors.Find(id);
            if(visitor != null)
            {
                visitor.Status = "CheckedIn";
                if(dbo.SaveChanges() > 0)
                {
                    TempData["Message"] = "Visitor successfully checked in.";
                }
            }
            return RedirectToAction("Scan", "Visitor", new { token = visitor.QRCodeToken });
        }
        public ActionResult CheckOut()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CheckOut(int id)
        {
            var visitor = dbo.TblVisitors.Find(id);
            if(visitor != null)
            {
                visitor.Status = "CheckedOut";
                if(dbo.SaveChanges() > 0)
                {
                    TempData["Message"] = "Visitor successfully checked out.";
                }
            }
            return RedirectToAction("Scan", "Visitor", new { token = visitor.QRCodeToken });
        }
        public ActionResult ShowVisitorPass()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ShowVisitorPass(int? visitorId)
        {
            if (Session["SecurityUser"] == null)
                return RedirectToAction("Login", "Security");

            TblVisitor visitor;
            if(visitorId.HasValue)
            {
                visitor = dbo.TblVisitors.FirstOrDefault(v => v.Id == visitorId.Value); 
            }
            else
            {
                visitor = dbo.TblVisitors.Where(v=>v.Status == "Pending" || v.Status == "CheckedIn").OrderByDescending(v=>v.CreatedAt).FirstOrDefault();
            }
            return View("Scan", visitor);
        }
    }
}