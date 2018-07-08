using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HelpDeskTrain.Models;
using System.Web.UI;
using System.Web.UI.WebControls;
namespace HelpDeskTrain.Controllers
{
    [Authorize(Roles = "Администратор, Модератор, Исполнитель")]
    public class UserController : Controller
    {
        public static string Encrypt(string s)
        {
            byte[] bytes = System.Text.Encoding.Unicode.GetBytes(s);
            string encpass = Convert.ToBase64String(bytes);
            return encpass;
        }
        private HelpdeskContext db = new HelpdeskContext();

        
        [HttpGet]
        public ActionResult Index()
        {
            var users = db.Users.Include(u => u.Department).Include(u=>u.Role).ToList();

            List<Department> departments = db.Departments.ToList();
            
            departments.Insert(0, new Department { Name = "Все", Id = 0 });
            ViewBag.Departments = new SelectList(departments, "Id", "Name");

            List<Role> roles = db.Roles.ToList();
            roles.Insert(0, new Role { Name = "Все", Id = 0 });
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            /*if (!HttpContext.User.IsInRole("Администратор"))
            {
                users.ForEach(x => { x.Login = null; x.Password = null; });
            }*/
            return View(users);
        }

        // poisk polzivatelei po deportamentu i statusa
        [HttpPost]
        public ActionResult Index(int department, int role)
        {
            IEnumerable<User> allUsers = null;
            if (role == 0 && department == 0)
            {
                return RedirectToAction("Index");
            }
            if (role == 0 && department != 0)
            {
                allUsers = from user in db.Users.Include(u => u.Department).Include(u=>u.Role)
                           where user.DepartmentId==department
                           select user;
            }
            else if (role != 0 && department == 0)
            {
                allUsers = from user in db.Users.Include(u => u.Department).Include(u => u.Role)
                           where user.RoleId==role
                           select user;
            }
            else
            {
                allUsers = from user in db.Users.Include(u => u.Department).Include(u => u.Role)
                           where user.DepartmentId == department && user.RoleId == role
                           select user;
            }

            List<Department> departments = db.Departments.ToList();
           
            departments.Insert(0, new Department { Name = "Все", Id = 0 });
            ViewBag.Departments = new SelectList(departments, "Id", "Name");

            List<Role> roles = db.Roles.ToList();
            roles.Insert(0, new Role { Name = "Все", Id = 0 });
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            return View(allUsers.ToList());
        }

        
        [HttpGet]
        [Authorize(Roles = "Администратор")]
        public ActionResult Create()
        {
            SelectList departments = new SelectList(db.Departments, "Id", "Name");
            ViewBag.Departments = departments;
            SelectList roles = new SelectList(db.Roles, "Id", "Name");
            ViewBag.Roles = roles;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Администратор")]
        public ActionResult Create(User user)
        {

            if (ModelState.IsValid)

            {
                User a = new User();
                a.Id = user.Id;
                a.Name = user.Name;
                a.Login = user.Login;
                a.Password = Encrypt(user.Password);
                a.Position = user.Position;
                a.DepartmentId = user.DepartmentId;
                a.RoleId = user.RoleId;
                db.Users.Add(a);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            SelectList departments = new SelectList(db.Departments, "Id", "Name");
            ViewBag.Departments = departments;
            SelectList roles = new SelectList(db.Roles, "Id", "Name");
            ViewBag.Roles = roles;

            return View(user);
        }
        [HttpGet]
        [Authorize(Roles = "Администратор")]
        public ActionResult Edit(int id)
        {
            User user = db.Users.Find(id);
            SelectList departments = new SelectList(db.Departments, "Id", "Name", user.DepartmentId);
            ViewBag.Departments = departments;
            SelectList roles = new SelectList(db.Roles, "Id", "Name", user.RoleId);
            ViewBag.Roles = roles;

            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Администратор")]
        public ActionResult Edit(User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            SelectList departments = new SelectList(db.Departments, "Id", "Name");
            ViewBag.Departments = departments;
            SelectList roles = new SelectList(db.Roles, "Id", "Name");
            ViewBag.Roles = roles;

            return View(user);
        }
        [Authorize(Roles = "Администратор")]
        public ActionResult Delete(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
