using HMT_Tech.Models;
using Microsoft.EntityFrameworkCore;

namespace HMT_Tech.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Register> Registers { get; set; }

        public DbSet<AddStationery> Stationeries { get; set; }

        public DbSet<Contact> ContactsData { get; set; }

        public DbSet<StationeryViewModel> StationeryRequests { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<NotificationUser> NotificationsUser { get; set; }




    }
}
