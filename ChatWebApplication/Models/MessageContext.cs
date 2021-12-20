using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatWebApp.Models
{
    public class MessageContext : DbContext
    {
        public MessageContext(DbContextOptions<MessageContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<Message> Messages { get; set; }
    }
}
