using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models.ViewModels
{
    public class ReviewVM
    {
        public int ReviewId { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CommentedOn { get; set; }



        public int ProductId { get; set; }
       


      
    }
}
