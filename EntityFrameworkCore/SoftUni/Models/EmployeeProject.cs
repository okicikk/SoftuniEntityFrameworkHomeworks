﻿using System.ComponentModel.DataAnnotations.Schema;

namespace SoftUni.Models
{
    public partial class EmployeeProject
    {
        public EmployeeProject()
        {

        }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
    }
}