using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TestTask1
{
    public class Message
    {
        public DateTime Date { get; set; }
        public string Nickname { get; set; }
        public string MessageText { get; set; }

        public override string ToString()
        {
            return $"{Date.Day}.{Date.Month}.{Date.Year} {Nickname}: {MessageText}";
        }
    }
}
