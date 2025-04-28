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
            if (user != null)
            {
                Session["SecurityUser"] = user.Username;
                return RedirectToAction("LatestVisitor", "Security");
            }
            else
            {
                ViewBag.Message = "Invalid login. Please try again.";
                return View();
            }
        }
        public ActionResult Dashboard()
        {
            var visitors = dbo.TblVisitors
                              .Where(v => v.Status == "Completed" || v.Status == "CheckedOut") // Show only completed or checked out
                              .OrderByDescending(v => v.UpdatedAt) // Optional: Latest completed first
                              .ToList();

            ViewBag.SecurityUser = Session["SecurityUser"] as string; // (If you want to show Welcome message too)
            return View(visitors);
        }
        //public ActionResult CameraScan()
        //{
        //    if (Session["SecurityUser"] == null)
        //        return RedirectToAction("Login");

        //    return View();
        //}
        [HttpPost]
        public ActionResult Logout()
        {
            Session.Clear(); // Clear session on logout
            return RedirectToAction("Login", "Security");
        }

        [HttpPost]
        public ActionResult CheckIn(int id)
        {
            var visitor = dbo.TblVisitors.Find(id);
            if(visitor != null)
            {
                visitor.Status = "CheckedIn";
                visitor.CreatedAt = DateTime.Now;
                if (dbo.SaveChanges() > 0)
                {
                    TempData["Message"] = "Visitor successfully checked in.";
                }
            }
            return RedirectToAction("LatestVisitor");
        }
       
        [HttpPost]
        public ActionResult CheckOut(int id)
        {
            var visitor = dbo.TblVisitors.Find(id);
            if(visitor != null)
            {
                visitor.Status = "CheckedOut";
                visitor.UpdatedAt = DateTime.Now;
                if (dbo.SaveChanges() > 0)
                {
                    TempData["Message"] = "Visitor successfully checked out.";
                }
            }
            return RedirectToAction("VisitorList");
        }
        
        [HttpPost]
        public ActionResult VisitorComplete(int id)
        {
            var visitor = dbo.TblVisitors.Find(id);
            if(visitor != null)
            {
                visitor.Status = "CheckedOut";
                visitor.UpdatedAt = DateTime.Now;
                if (dbo.SaveChanges() > 0)
                {
                    TempData["Message"] = "Visitor marked as completed.";
                }
            }
            return RedirectToAction("Dashboard", "Security");
        }
        public ActionResult LatestVisitor(int? id)
        {
            if (Session["SecurityUser"] == null)
                return RedirectToAction("Login");

            TblVisitor visitor = null;

            if (id.HasValue)
            {
                visitor = dbo.TblVisitors.FirstOrDefault(v => v.Id == id.Value);
            }
            else
            {
                // Fallback: If no id provided, get latest Pending/CheckedIn visitor
                visitor = dbo.TblVisitors
                            .Where(v => v.Status == "Pending" || v.Status == "CheckedIn")
                            .OrderByDescending(v => v.CreatedAt)
                            .FirstOrDefault();
            }

            if (visitor == null)
            {
                ViewBag.Message = "No visitor found.";
                return View("VisitorPassWithActions");
            }

            return View("VisitorPassWithActions", visitor);
        }
        public ActionResult VisitorList()
        {
            if (Session["SecurityUser"] == null)
                return RedirectToAction("Login");

            var visitor = dbo.TblVisitors.OrderByDescending(v => v.CreatedAt).ToList();
            return View(visitor);
        }
        [HttpPost]
        public ActionResult DeleteVisitor(int id)
        {
            var visitor = dbo.TblVisitors.Find(id);
            if (visitor != null)
            {
                dbo.TblVisitors.Remove(visitor);
                dbo.SaveChanges();
                TempData["Message"] = "Visitor deleted successfully.";
            }
            return RedirectToAction("Dashboard", "Security");
        }
    }
}
