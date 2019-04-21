using System;
using System.Collections.Generic;

namespace Exam.Models
{
  public class Activity : DateModel
  {
    public int ActivityId { get; set; }
    public string Title { get; set; }
    public DateTime Date { get; set; }
    public string Duration { get; set; }
    public string Description { get; set; }
    public int UserId { get; set; }




    // NAVIGATION
    public User User { get; set; }
    public List<Participant> Participants { get; set; }

    

  }
}