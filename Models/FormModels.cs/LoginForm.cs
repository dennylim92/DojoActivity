using System;
using System.ComponentModel.DataAnnotations;

namespace Exam.Models
{
    public class LoginForm
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string lEmail {get;set;}

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string lPassword {get;set;}
    }
}