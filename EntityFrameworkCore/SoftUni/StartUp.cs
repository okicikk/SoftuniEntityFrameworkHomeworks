using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SoftUni.Data;
using SoftUni.Models;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;

namespace SoftUni
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            SoftUniContext context = new SoftUniContext();
            Console.WriteLine(GetEmployeesByFirstNameStartingWithSa(context));
        }
        public static string RemoveTown(SoftUniContext context)
        {
            Town townToBeRemoved = context.Towns.Where(t => t.Name == "Seattle").FirstOrDefault();
            var addressesToBeRemoved = context.Addresses.Where(a => a.Town == townToBeRemoved);
            var employeesToBeUpdated = context.Employees.Where(e => e.Address.Town == townToBeRemoved);

            int addressesToBeRemovedCount = addressesToBeRemoved.Count();

            foreach (var employee in employeesToBeUpdated)
            {
                employee.Address = null;
            }
            foreach (var address in addressesToBeRemoved)
            {
                context.Addresses.Remove(address);
            }
            context.Towns.Remove(townToBeRemoved);
            context.SaveChanges();

            return $"{addressesToBeRemovedCount} addresses in Seattle were deleted";
        }
        public static string DeleteProjectById(SoftUniContext context)
        {
            var projectToBeDeleted = context.Projects.Find(2);
            var epToBeDeleted = context.EmployeesProjects.Where(ep => ep.Project == projectToBeDeleted);

            foreach (var ep in epToBeDeleted)
            {
                context.EmployeesProjects.Remove(ep);
            }
            context.Projects.Remove(projectToBeDeleted);
            context.SaveChanges();

            var projects = context.Projects
                .Take(10)
                .Select(p => new { p.Name })
                .ToList();
            StringBuilder sb = new StringBuilder();
            foreach (var p in projects)
            {
                sb.AppendLine($"{p.Name}");
            }
            return sb.ToString().TrimEnd();
        }
        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            var employeesSa = context.Employees
                .Where(e => e.FirstName.StartsWith("Sa"))
                .Select(e => new { e.FirstName, e.LastName, e.JobTitle, e.Salary })
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToList();
            StringBuilder sb = new StringBuilder();
            foreach (var employee in employeesSa)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle} - (${employee.Salary:f2})");
            }


            return sb.ToString().TrimEnd();
        }
        public static string IncreaseSalaries(SoftUniContext context)
        {
            var employeesToBePromoted = context.Employees
                .Where(e => new string[] { "Engineering", "Tool Design", "Marketing", "Information Services" }.Contains(e.Department.Name))
                //.Select(e => new { e.FirstName, e.LastName, e.Salary})
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToList();
            StringBuilder sb = new StringBuilder();
            foreach (var e in employeesToBePromoted)
            {
                e.Salary *= (decimal)1.12;
            }
            context.SaveChanges();
            foreach (var e in employeesToBePromoted)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} (${e.Salary:f2})");
            }
            return sb.ToString().TrimEnd();
        }
        public static string GetLatestProjects(SoftUniContext context)
        {
            var projects = context.Projects
                .Select(p => new
                {
                    p.Name,
                    p.Description,
                    p.StartDate
                })
                .OrderByDescending(p => p.StartDate)
                .Take(10)
                .OrderBy(p => p.Name)
                .ToList();
            StringBuilder sb = new StringBuilder();
            foreach (var project in projects)
            {
                sb.AppendLine($"{project.Name}");
                sb.AppendLine($"{project.Description}");
                sb.AppendLine($"{project.StartDate:M/d/yyyy h:mm:ss tt}");

            }
            return sb.ToString().TrimEnd();
        }
        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            var departments = context.Departments
                .Where(d => d.Employees.Count > 5)
                .Select(d => new
                {
                    d.Name,
                    Employees = d.Employees.Select(e => new
                    {
                        e.FirstName,
                        e.LastName,
                        Jobtitle = e.JobTitle
                    }).OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToList(),
                    ManagerName = $"{d.Manager.FirstName} {d.Manager.LastName}"
                })
                .OrderBy(d => d.Employees.Count);

            StringBuilder sb = new StringBuilder();
            foreach (var d in departments)
            {
                sb.AppendLine($"{d.Name} - {d.ManagerName}");
                foreach (var e in d.Employees)
                {
                    sb.AppendLine($"{e.FirstName} {e.LastName} - {e.Jobtitle}");
                }
            }
            return sb.ToString().TrimEnd();
        }
        public static string GetAddressesByTown(SoftUniContext context)
        {
            var addresses = context.Addresses
                .Select
                (a => new
                {
                    AddressTown = a.Town.Name
                ,
                    AddressText = a.AddressText
                ,
                    EmployeesCount = a.Employees.Count()
                })
                .OrderByDescending(a => a.EmployeesCount)
                .ThenBy(a => a.AddressTown)
                .ThenBy(a => a.AddressText)
                .Take(10)
                .ToList();
            StringBuilder sb = new StringBuilder();
            foreach (var a in addresses)
            {
                sb.AppendLine($"{a.AddressText}, {a.AddressTown} - {a.EmployeesCount} employees");
            }
            return sb.ToString().TrimEnd();
        }
        public static string GetEmployee147(SoftUniContext context)
        {
            var employee = context.Employees
                .Where(e => e.EmployeeId == 147)
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.JobTitle,
                    Projects = e.EmployeesProjects.Select(ep => ep.Project.Name).OrderBy(p => p).ToList()
                })
                .FirstOrDefault();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle}");
            foreach (var p in employee.Projects)
            {
                sb.AppendLine($"{p}");
            }
            return sb.ToString().TrimEnd();
        }
        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            Address address = new Address() { AddressText = "Vitoshka 15", TownId = 4 };
            var employeeToBeUpdated = context.Employees.Where(e => e.LastName == "Nakov").FirstOrDefault();

            employeeToBeUpdated.Address = address;

            context.SaveChanges();

            return string.Join(Environment.NewLine, context.Employees
                .OrderByDescending(e => e.AddressId)
                .Take(10)
                .Select(e => $"{e.Address.AddressText}"));
        }
        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var employees = context.Employees
                .Take(10)
                .Select(e => new
                {
                    EmployeeName = $"{e.FirstName} {e.LastName}",
                    ManagerName = $" - Manager: {e.Manager.FirstName} {e.Manager.LastName}",
                    EmployeesProjects = e.EmployeesProjects
                    .Select(ep => new { ep.Project, ep.Project.StartDate, ep.Project.EndDate })
                    .Where(ep => ep.Project.StartDate <= new DateTime(2003, 1, 1)
                        && ep.Project.StartDate >= new DateTime(2001, 1, 1))
                    .ToList()
                })
                .ToList();
            foreach (var e in employees)
            {
                sb.AppendLine($"{e.EmployeeName}{e.ManagerName}");
                foreach (var ep in e.EmployeesProjects)
                {
                    if (!ep.Project.EndDate.HasValue)
                    {
                        sb.AppendLine($"--{ep.Project.Name} - {ep.Project.StartDate} - not finished");
                        continue;
                    }
                    sb.AppendLine
                        ($"--{ep.Project.Name} - {ep.Project.StartDate} - {ep.Project.EndDate:M/d/yyyy h:mm:ss tt}");
                }
            }

            return sb.ToString().TrimEnd();
        }
        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(e => e.Department.Name == "Research and Development")
                .OrderBy(e => e.Salary)
                .ThenByDescending(e => e.FirstName)
                .Select(e => $"{e.FirstName} {e.LastName} from {e.Department.Name} - ${e.Salary:f2}")
                .ToList();

            return string.Join(Environment.NewLine, employees);
        }
        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(e => e.Salary > 50_000)
                .OrderBy(e => e.FirstName)
                .Select(e => $"{e.FirstName} - {e.Salary:f2}");

            return string.Join(Environment.NewLine, employees);
        }
        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            var employeesInfo = context.Employees
                .OrderBy(e => e.EmployeeId)
                .Select
                (e => $"{e.FirstName} {e.LastName} {e.MiddleName} {e.JobTitle} {e.Salary:f2}")
                .ToList();

            return string.Join(Environment.NewLine, employeesInfo);
        }
    }
}
