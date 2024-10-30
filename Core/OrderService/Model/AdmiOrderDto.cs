using DataAccess.Enums;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.OrderService.Model
{
    public class AdmiOrderDto
    {
        public int Id { get; set; }

        public DateTime Payed { get; set; }

        public string UserId { get; set; }

        public string Address { get; set; }

        public string Mobile { get; set; }


        public Status Status { get; set; }
 
        public string UserName { get; set; }

        public List<string> Items { get; set; }
    }
}
