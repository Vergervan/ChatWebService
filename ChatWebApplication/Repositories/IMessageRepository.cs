using ChatWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatWebApp.Repositories
{
    public interface IMessageRepository
    {
        Task<IEnumerable<Message>> Get();
        Task<Message> Create(Message message);
        Task Clear();
    }
}
