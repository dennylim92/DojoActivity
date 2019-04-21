using System;
using System.Collections.Generic;

namespace Exam.Models
{
  public class User : DateModel
  {
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }


    // NAVIGATION
    public List<Activity> Activities { get; set; }
    public List<Participant> Participating { get; set; }
  }
}