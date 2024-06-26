using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P01_StudentSystem.Data.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        [Unicode(true)]
        [MaxLength(80)]
        public string Name { get; set; }

        [MaxLength(5000)]
        [Unicode(true)]
        public string Description { get; set; } = null!;
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Required]
        public decimal Price { get; set; }
        public ICollection<StudentCourse> StudentsCourses { get; set; }
        public ICollection<Resource> Resources { get; set; }
        public ICollection<Homework> Homeworks { get; set; }



    }
}
