using System.Runtime.InteropServices;
using System.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Exam.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Activity = Exam.Models.Activity;

namespace Exam.Controllers
{
  public class HomeController : Controller
  {
    private MyContext dbContext;
    public HomeController(MyContext context)
    {
      dbContext = context;
    }
    [HttpGet("")]
    public IActionResult Index()
    {
      return View();
    }

    [HttpGet("home")]
    public IActionResult Home()
    {
      if (HttpContext.Session.GetInt32("LoggedID") != null)
      {
        List<Activity> allActivities = dbContext.Activities
          .OrderBy(a => a.Date)
          .Include(a => a.User)
          .Include(a => a.Participants)
          .ThenInclude(p => p.User)
          .ToList();
        int? idUser = HttpContext.Session.GetInt32("LoggedID");
        ViewBag.idUser = idUser;
        ViewBag.User = dbContext.Users
          .Where(u => u.UserId == idUser)
          .SingleOrDefault();
        return View(allActivities);
      }
      ModelState.AddModelError("lEmail", "Please login to continue.");
      return View("Index");
    }

    [HttpPost("register")]
    public IActionResult RegisterUser(RegisterForm userForm)
    {
      if (ModelState.IsValid)
      {
        if (dbContext.Users.Any(u => u.Email == userForm.Email))
        {
          ModelState.AddModelError("Email", "Email already in use!");
          return View("Index");
        }
        User newUser = new User()
        {
          Name = userForm.Name,
          Email = userForm.Email,
          Password = userForm.Password
        };
        PasswordHasher<RegisterForm> Hasher = new PasswordHasher<RegisterForm>();
        newUser.Password = Hasher.HashPassword(userForm, newUser.Password);
        dbContext.Add(newUser);
        dbContext.SaveChanges();
        HttpContext.Session.SetInt32("LoggedID", newUser.UserId);
        return RedirectToAction("Home");
      }
      return View("Index");
    }

    [HttpPost("login")]
    public IActionResult LoginUser(LoginForm userSubmission)
    {
      if (ModelState.IsValid)
      {
        User userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.lEmail);
        if (userInDb == null)
        {
          ModelState.AddModelError("lEmail", "Invalid Email");
          return View("Index");
        }
        var hasher = new PasswordHasher<LoginForm>();
        var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.lPassword);
        if (result == 0)
        {
          ModelState.AddModelError("Password", "Invalid Password");
          return View("Index");
        }
        HttpContext.Session.SetInt32("LoggedID", userInDb.UserId);
        int? idUser = HttpContext.Session.GetInt32("LoggedID");
        return RedirectToAction("Home");
      }
      return View("Index");
    }

    [HttpGet("activity/{activityID}")]
    public IActionResult ActivityInfo(int activityID)
    {
      if (HttpContext.Session.GetInt32("LoggedID") != null)
      {
          int? idUser = HttpContext.Session.GetInt32("LoggedID");
          Activity anActivity = dbContext.Activities
            .Where(a => a.ActivityId == activityID)
            .Include(a => a.User)
            .Include(a => a.Participants)
            .ThenInclude(r => r.User)
            .SingleOrDefault();
          List<Participant> allParticipants = dbContext.Participants
            .Where(p => p.ActivityId == activityID)
            .Include(p => p.User)
            .ToList();
          Wrapper stuff = new Wrapper()
          {
            Activity = anActivity,
            People = allParticipants
          };
          ViewBag.userID = (int)idUser;
          return View(stuff);
      }
      ModelState.AddModelError("lEmail", "Please login to continue.");
      return View("Index");
    }

    [HttpGet("new")]
    public IActionResult CreateActivity()
    {
      if (HttpContext.Session.GetInt32("LoggedID") != null)
      {
        return View();
      }
      ModelState.AddModelError("lEmail", "Please login to continue.");
      return View("Index");
    }

    [HttpPost("create/activity")]
    public IActionResult AddingActivity(ActivityForm form, Activity newActivity)
    {
      if (ModelState.IsValid)
      {
        int activityDate = form.Date.DayOfYear;
        int Today = DateTime.Now.DayOfYear;
        if (activityDate < Today)
        {
          ModelState.AddModelError("Date", "Must create an activity for a future date.");
          return View("CreateActivity");
        }
        int? idUser = HttpContext.Session.GetInt32("LoggedID");
        DateTime activityDay = form.Date;
        TimeSpan activityTime = form.Time;
        newActivity.Date = activityDay + activityTime;
        newActivity.UserId = (int)idUser;
        string HMD = form.HMD;
        newActivity.Duration = form.Duration + " " + HMD;
        dbContext.Add(newActivity);
        dbContext.SaveChanges();
        // SELECTING ADDED Activity ID
        Activity thisActivityId = dbContext.Activities
        .Where(w => w.ActivityId == newActivity.ActivityId)
        .SingleOrDefault();
        return RedirectToAction("ActivityInfo", new { activityId = thisActivityId.ActivityId });
      }
      return View("CreateActivity");
    }

    [HttpGet("delete/{activityID}")]
    public IActionResult Delete(int activityID)
    {
      if (HttpContext.Session.GetInt32("LoggedID") != null)
      {
        int? idUser = HttpContext.Session.GetInt32("LoggedID");
        dbContext.Remove(dbContext.Activities
        .Where(a => a.ActivityId == activityID)
        .Where(a => a.UserId == idUser)
        .SingleOrDefault());
        dbContext.SaveChanges();
        return RedirectToAction("Home");
      }
      ModelState.AddModelError("lEmail", "Please login to continue.");
      return View("Index");
    }

    [HttpGet("join/{activityID}")]
    public IActionResult AddParticipant(int activityID)
    {
      if (HttpContext.Session.GetInt32("LoggedID") != null)
      {
        int? idUser = HttpContext.Session.GetInt32("LoggedID");
        Participant newParticipant = new Participant();
        newParticipant.UserId = (int)idUser;
        newParticipant.ActivityId = activityID;
        dbContext.Add(newParticipant);
        dbContext.SaveChanges();
        return RedirectToAction("Home");
      }
      ModelState.AddModelError("lEmail", "Please login to continue.");
      return View("Index");
    }

    [HttpGet("leave/{activityID}")]
    public IActionResult RemoveGuest(int activityID)
    {
      if (HttpContext.Session.GetInt32("LoggedID") != null)
      {
        int? idUser = HttpContext.Session.GetInt32("LoggedID");
        dbContext.Remove(dbContext.Participants
            .Where(r => r.ActivityId == activityID)
            .Where(r => r.UserId == (int)idUser)
            .SingleOrDefault());
        dbContext.SaveChanges();
        return RedirectToAction("Home");
      }
      ModelState.AddModelError("lEmail", "Please login to continue.");
      return View("Index");
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
      HttpContext.Session.Clear();
      return View("Index");
    }
  }
}

