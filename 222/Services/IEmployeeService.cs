using _222.EF;
using _222.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _222.Services
{
    interface IEmployeeService
{
        IEnumerable<Employee> GetAll(string name, string sort);
        void Create(CreateModel model);
        Employee Get(int Id);
        void Delete(int Id);

}
}
