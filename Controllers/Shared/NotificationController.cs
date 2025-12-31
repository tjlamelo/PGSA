using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Models;
using PGSA_Licence3.Services.Notifications;
using System.Security.Claims;
using PGSA_Licence3.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PGSA_Licence3.Controllers
{
    [Route("Notification")]
    [Authorize] 
    public class NotificationController : Controller
    {
        private readonly NotificationService _notificationService;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<NotificationService> _logger;

        public NotificationController(ApplicationDbContext db, ILogger<NotificationService> logger)
        {
            _db = db;
            _logger = logger;
            _notificationService = new NotificationService(db, logger);
        }

        
}}