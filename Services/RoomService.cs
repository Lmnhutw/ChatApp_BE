﻿using ChatApp_BE.Data;
using ChatApp_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp_BE.Services
{
    public class RoomService
    {
        private readonly ChatAppContext _ChatAppContext;

        public RoomService(ChatAppContext ChatAppCxt)
        {
            _ChatAppContext = ChatAppCxt;
        }
    }
}