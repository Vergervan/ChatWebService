using ChatWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatWebApp.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly MessageContext _context;

        public MessageRepository(MessageContext context)
        {
            _context = context;
        }

        public async Task<Message> Create(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<IEnumerable<Message>> Get()
        {
            return await _context.Messages.ToListAsync();
        }
        public async Task Clear()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
            await _context.SaveChangesAsync();
        }
    }
}
