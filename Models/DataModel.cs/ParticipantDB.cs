using System;
namespace Exam.Models
{
  public class Participant : DateModel
  {
    public int ParticipantId { get; set; }
    public int UserId { get; set; }
    public int ActivityId { get; set; }



    // NAVIGATION
    public User User { get; set; }
    public Activity Activity { get; set; }
  }
}