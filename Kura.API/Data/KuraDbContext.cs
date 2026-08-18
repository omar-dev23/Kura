using Kura.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Kura.API.Data
{
    public class KuraDbContext : DbContext
    {
        public KuraDbContext(DbContextOptions<KuraDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Patient)
                .WithOne(p => p.User)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Doctor)
                .WithOne(d => d.User)
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.Patient)
                .WithMany(p => p.Documents)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorPatientConnection>()
                .HasOne(c => c.Patient)
                .WithMany(p => p.Connections)
                .HasForeignKey(c => c.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DoctorPatientConnection>()
                .HasOne(c => c.Doctor)
                .WithMany(d => d.Connections)
                .HasForeignKey(c => c.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DoctorCertificate>()
                .HasOne(c => c.Doctor)
                .WithMany(d => d.Certificates)
                .HasForeignKey(c => c.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorService>()
                .HasOne(s => s.Doctor)
                .WithMany(d => d.Services)
                .HasForeignKey(s => s.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Organization
            modelBuilder.Entity<User>()
                .HasOne(u => u.Organization)
                .WithOne(o => o.User)
                .HasForeignKey<Organization>(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrganizationService>()
                .HasOne(s => s.Organization)
                .WithMany(o => o.Services)
                .HasForeignKey(s => s.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrganizationDepartment>()
                .HasOne(d => d.Organization)
                .WithMany(o => o.Departments)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Doctor Workplaces
            modelBuilder.Entity<DoctorWorkplace>()
                .HasOne(w => w.Doctor)
                .WithMany(d => d.Workplaces)
                .HasForeignKey(w => w.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorWorkplace>()
                .HasOne(w => w.Organization)
                .WithMany()
                .HasForeignKey(w => w.OrganizationId)
                .OnDelete(DeleteBehavior.NoAction);

            // Prescriptions
            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Doctor)
                .WithMany(d => d.Prescriptions)
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Patient)
                .WithMany(p => p.Prescriptions)
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PrescriptionMedicine>()
                .HasOne(m => m.Prescription)
                .WithMany(p => p.Medicines)
                .HasForeignKey(m => m.PrescriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Appointments
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Messages
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<OrganizationSpecialty>()
                .HasOne(s => s.Organization)
                .WithMany(o => o.Specialties)
                .HasForeignKey(s => s.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrganizationPharmacist>()
                .HasOne(p => p.Organization)
                .WithMany(o => o.Pharmacists)
                .HasForeignKey(p => p.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrganizationLabDoctor>()
                .HasOne(d => d.Organization)
                .WithMany(o => o.LabDoctors)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Patient-Organization Connections
            modelBuilder.Entity<PatientOrganizationConnection>()
                .HasOne(c => c.Patient)
                .WithMany(p => p.OrganizationConnections)
                .HasForeignKey(c => c.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PatientOrganizationConnection>()
                .HasOne(c => c.Organization)
                .WithMany(o => o.PatientConnections)
                .HasForeignKey(c => c.OrganizationId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<DoctorPatientConnection> DoctorPatientConnections { get; set; }
        public DbSet<DoctorCertificate> DoctorCertificates { get; set; }
        public DbSet<DoctorService> DoctorServices { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<OrganizationService> OrganizationServices { get; set; }
        public DbSet<OrganizationDepartment> OrganizationDepartments { get; set; }
        public DbSet<DoctorWorkplace> DoctorWorkplaces { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionMedicine> PrescriptionMedicines { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<OrganizationSpecialty> OrganizationSpecialties { get; set; }
        public DbSet<OrganizationPharmacist> OrganizationPharmacists { get; set; }
        public DbSet<OrganizationLabDoctor> OrganizationLabDoctors { get; set; }
        public DbSet<PatientOrganizationConnection> PatientOrganizationConnections { get; set; }
    }
}