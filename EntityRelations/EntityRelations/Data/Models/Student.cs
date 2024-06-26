using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace P01_StudentSystem.Data.Models
{

    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [Unicode(true)]
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MinLength(10)]
        [MaxLength(10)]
        [Unicode(false)]
        public string PhoneNumber { get; set; } = null!;

        [Required] public DateTime RegisteredOn { get; set; }

        public DateTime? Birthday { get; set; }
        public ICollection<Homework> Homeworks { get; set; }
        public ICollection<StudentCourse> StudentsCourses { get; set; }

    }
}
