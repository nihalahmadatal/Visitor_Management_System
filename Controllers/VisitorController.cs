using QRCoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Navigation;
using Visitor_Management_System.Models;

namespace Visitor_Management_System.Controllers
{
    public class VisitorController : Controller
    {
        DB_VisitorEntities dbo = new DB_VisitorEntities();
        // GET: Visitor
        public ActionResult ShowRegisterQR()
        {
            string registerUrl = "https://localhost:44337/Visitor/Register";

            QRCodeGenerator qr = new QRCodeGenerator();
            QRCodeData qrData = qr.CreateQrCode(registerUrl, QRCodeGenerator.ECCLevel.Q);
            Base64QRCode qrCode = new Base64QRCode(qrData);
            string qrCodeImage = qrCode.GetGraphic(20);

            ViewBag.qrCodeImage = "data:image/png;base64," + qrCodeImage;

            return View();
        }
        
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(TblVisitor visitor,HttpPostedFileBase photo)
        {
            if(photo != null)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(photo.FileName);
                string path = Path.Combine(Server.MapPath("~/VisitorPhotos/"), fileName);
                photo.SaveAs(path);
                visitor.PhotoPath = "/VisitorPhotos/" + fileName;
            }

            visitor.Status = "Pending";
            visitor.QRCodeToken = Guid.NewGuid();
            visitor.CreatedAt = DateTime.Now;
            dbo.TblVisitors.Add(visitor);   
            if(dbo.SaveChanges() > 0)
            {
                return RedirectToAction("ShowPass", new {token = visitor.QRCodeToken});
            }
            return View();
        }

        public ActionResult ShowPass(Guid token)
        {
            var visitor = dbo.TblVisitors.FirstOrDefault(v=>v.QRCodeToken == token);
            return View(visitor);
        }
        public ActionResult GenerateQR(Guid token)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(Url.Action("Scan", "Visitor", new { token }, Request.Url.Scheme), QRCodeGenerator.ECCLevel.Q);
            using (var qrCode = new QRCode(qrCodeData))
            using (var bitmap = qrCode.GetGraphic(20))
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream,System.Drawing.Imaging.ImageFormat.Png);
                return File(stream.ToArray(), "image/png");
            }

        }

        [HttpPost]
        public ActionResult RedirectToSecurity(int id)
        {
            // Store visitor ID temporarily (optional if you're passing it in query)
            TempData["VisitorId"] = id;

            // Redirect to LatestVisitor on Security controller
            return RedirectToAction("LatestVisitor", "Security", new { id = id });
        }
    }
}
