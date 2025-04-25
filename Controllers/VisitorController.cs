using QRCoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Visitor_Management_System.Models;

namespace Visitor_Management_System.Controllers
{
    public class VisitorController : Controller
    {
        DB_VisitorEntities dbo = new DB_VisitorEntities();
        // GET: Visitor
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

            visitor.Status = "Approved";
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
        public ActionResult Scan(Guid token)
        {
            var visitor = dbo.TblVisitors.FirstOrDefault(v => v.QRCodeToken == token);

            if(visitor != null)
            {
                if (visitor.Status == "Approved")
                    visitor.Status = "CheckedIn";
                else if (visitor.Status == "CheckedIn")
                    visitor.Status = "CheckedOut";

                visitor.UpdatedAt = DateTime.Now;
                if(dbo.SaveChanges() > 0)
                {
                    return View(visitor);
                }
            }
            return HttpNotFound();

        }
        //public ActionResult VisitorPass(Guid token)
        //{
        //    // Look up the visitor by their QR token
        //    var visitor = dbo.TblVisitors
        //                    .FirstOrDefault(v => v.QRCodeToken == token);
        //    if (visitor == null)
        //        return HttpNotFound();

        //    // Pass it to the view
        //    return RedirectToAction("VisitorPass", new { token = visitor.QRCodeToken });
        //}

    }
}