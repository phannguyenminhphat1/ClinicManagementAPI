using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace clinic_management.infrastructure.Models;

public partial class ClinicManagementContext : DbContext
{
    public ClinicManagementContext()
    {
    }

    public ClinicManagementContext(DbContextOptions<ClinicManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AppointmentStatusHistory> AppointmentStatusHistories { get; set; }

    public virtual DbSet<Billing> Billings { get; set; }

    public virtual DbSet<BillingDetail> BillingDetails { get; set; }

    public virtual DbSet<BillingMedicine> BillingMedicines { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<MedicalRecordDetail> MedicalRecordDetails { get; set; }

    public virtual DbSet<MedicalRecordSummary> MedicalRecordSummaries { get; set; }

    public virtual DbSet<MedicalTest> MedicalTests { get; set; }

    public virtual DbSet<MedicalTestResult> MedicalTestResults { get; set; }

    public virtual DbSet<Medicine> Medicines { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentDetail> PaymentDetails { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<PaymentStatus> PaymentStatuses { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<PrescriptionDetail> PrescriptionDetails { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Specialty> Specialties { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__appointm__A50828FC9A95F0A5");

            entity.ToTable("appointments");

            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.ScheduledDate)
                .HasColumnType("datetime")
                .HasColumnName("scheduled_date");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Doctor).WithMany(p => p.AppointmentDoctors)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("FK__appointme__docto__3B75D760");

            entity.HasOne(d => d.Patient).WithMany(p => p.AppointmentPatients)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK__appointme__patie__3A81B327");

            entity.HasOne(d => d.Status).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK__appointme__statu__3C69FB99");
        });

        modelBuilder.Entity<AppointmentStatusHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__appointm__096AA2E9BD249231");

            entity.ToTable("appointment_status_history");

            entity.Property(e => e.HistoryId).HasColumnName("history_id");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Note)
                .HasMaxLength(255)
                .HasColumnName("note");
            entity.Property(e => e.StatusId).HasColumnName("status_id");

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentStatusHistories)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__appointme__appoi__02FC7413");

            entity.HasOne(d => d.Status).WithMany(p => p.AppointmentStatusHistories)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__appointme__statu__03F0984C");
        });

        modelBuilder.Entity<Billing>(entity =>
        {
            entity.HasKey(e => e.BillingId).HasName("PK__billings__50157129988A1BC8");

            entity.ToTable("billings");

            entity.Property(e => e.BillingId).HasColumnName("billing_id");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_amount");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Billings)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK__billings__appoin__0B91BA14");
        });

        modelBuilder.Entity<BillingDetail>(entity =>
        {
            entity.HasKey(e => e.BillingDetailId).HasName("PK__billing___84483EA72AC7D977");

            entity.ToTable("billing_details");

            entity.Property(e => e.BillingDetailId).HasColumnName("billing_detail_id");
            entity.Property(e => e.BillingId).HasColumnName("billing_id");
            entity.Property(e => e.PaymentStatusId).HasColumnName("payment_status_id");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");

            entity.HasOne(d => d.Billing).WithMany(p => p.BillingDetails)
                .HasForeignKey(d => d.BillingId)
                .HasConstraintName("FK__billing_d__billi__114A936A");

            entity.HasOne(d => d.PaymentStatus).WithMany(p => p.BillingDetails)
                .HasForeignKey(d => d.PaymentStatusId)
                .HasConstraintName("FK__billing_d__payme__1CBC4616");

            entity.HasOne(d => d.Service).WithMany(p => p.BillingDetails)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK__billing_d__servi__123EB7A3");
        });

        modelBuilder.Entity<BillingMedicine>(entity =>
        {
            entity.HasKey(e => e.BillingMedicineId).HasName("PK__billing___EFEFF7E0BC370060");

            entity.ToTable("billing_medicines");

            entity.Property(e => e.BillingMedicineId).HasColumnName("billing_medicine_id");
            entity.Property(e => e.BillingId).HasColumnName("billing_id");
            entity.Property(e => e.MedicineId).HasColumnName("medicine_id");
            entity.Property(e => e.PaymentStatusId).HasColumnName("payment_status_id");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Billing).WithMany(p => p.BillingMedicines)
                .HasForeignKey(d => d.BillingId)
                .HasConstraintName("FK__billing_m__billi__625A9A57");

            entity.HasOne(d => d.Medicine).WithMany(p => p.BillingMedicines)
                .HasForeignKey(d => d.MedicineId)
                .HasConstraintName("FK__billing_m__medic__634EBE90");

            entity.HasOne(d => d.PaymentStatus).WithMany(p => p.BillingMedicines)
                .HasForeignKey(d => d.PaymentStatusId)
                .HasConstraintName("FK__billing_m__payme__6442E2C9");
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.ConversationId).HasName("PK__conversa__311E7E9A5D586651");

            entity.ToTable("conversations");

            entity.HasIndex(e => new { e.UserMinId, e.UserMaxId }, "uq_conversation_pair").IsUnique();

            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.LastMessageId).HasColumnName("last_message_id");
            entity.Property(e => e.User1Id).HasColumnName("user1_id");
            entity.Property(e => e.User2Id).HasColumnName("user2_id");
            entity.Property(e => e.UserMaxId)
                .HasComputedColumnSql("(case when [user1_id]<[user2_id] then [user2_id] else [user1_id] end)", true)
                .HasColumnName("user_max_id");
            entity.Property(e => e.UserMinId)
                .HasComputedColumnSql("(case when [user1_id]<[user2_id] then [user1_id] else [user2_id] end)", true)
                .HasColumnName("user_min_id");

            entity.HasOne(d => d.LastMessage).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.LastMessageId)
                .HasConstraintName("fk_conversations_last_message");

            entity.HasOne(d => d.User1).WithMany(p => p.ConversationUser1s)
                .HasForeignKey(d => d.User1Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_conversations_user1");

            entity.HasOne(d => d.User2).WithMany(p => p.ConversationUser2s)
                .HasForeignKey(d => d.User2Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_conversations_user2");
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.MedicalRecordId).HasName("PK__medical___05C4C30AAC63E644");

            entity.ToTable("medical_records");

            entity.HasIndex(e => e.PatientId, "UQ__medical___4D5CE4774BA70FFA").IsUnique();

            entity.Property(e => e.MedicalRecordId).HasColumnName("medical_record_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");

            entity.HasOne(d => d.Patient).WithOne(p => p.MedicalRecord)
                .HasForeignKey<MedicalRecord>(d => d.PatientId)
                .HasConstraintName("FK__medical_r__patie__4222D4EF");
        });

        modelBuilder.Entity<MedicalRecordDetail>(entity =>
        {
            entity.HasKey(e => e.MedicalRecordDetailId).HasName("PK__medical___8B1DAC0788B9272C");

            entity.ToTable("medical_record_detail");

            entity.HasIndex(e => e.AppointmentId, "UQ__medical___A50828FDD5E2A937").IsUnique();

            entity.Property(e => e.MedicalRecordDetailId).HasColumnName("medical_record_detail_id");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Diagnosis).HasColumnName("diagnosis");
            entity.Property(e => e.MedicalRecordId).HasColumnName("medical_record_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.RequiresTest).HasColumnName("requires_test");
            entity.Property(e => e.Symptoms).HasColumnName("symptoms");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Appointment).WithOne(p => p.MedicalRecordDetail)
                .HasForeignKey<MedicalRecordDetail>(d => d.AppointmentId)
                .HasConstraintName("FK__medical_r__appoi__47DBAE45");

            entity.HasOne(d => d.MedicalRecord).WithMany(p => p.MedicalRecordDetails)
                .HasForeignKey(d => d.MedicalRecordId)
                .HasConstraintName("FK__medical_r__medic__46E78A0C");
        });

        modelBuilder.Entity<MedicalRecordSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("MedicalRecordSummary");

            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.Diagnosis).HasColumnName("diagnosis");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.DoctorName)
                .HasMaxLength(250)
                .HasColumnName("doctor_name");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.MedicalRecordDetailId).HasColumnName("medical_record_detail_id");
            entity.Property(e => e.MedicalRecordId).HasColumnName("medical_record_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.PatientName)
                .HasMaxLength(250)
                .HasColumnName("patient_name");
            entity.Property(e => e.Prescriptions).HasColumnName("prescriptions");
            entity.Property(e => e.RequiresTest).HasColumnName("requires_test");
            entity.Property(e => e.ScheduledDate)
                .HasColumnType("datetime")
                .HasColumnName("scheduled_date");
            entity.Property(e => e.SpecialtyName)
                .HasMaxLength(100)
                .HasColumnName("specialty_name");
            entity.Property(e => e.StatusName)
                .HasMaxLength(100)
                .HasColumnName("status_name");
            entity.Property(e => e.Symptoms).HasColumnName("symptoms");
            entity.Property(e => e.Tests).HasColumnName("tests");
        });

        modelBuilder.Entity<MedicalTest>(entity =>
        {
            entity.HasKey(e => e.MedicalTestId).HasName("PK__medical___8D8FB34FBCAD8BD6");

            entity.ToTable("medical_tests");

            entity.HasIndex(e => new { e.MedicalRecordDetailId, e.ServiceId }, "UQ_MedicalTest").IsUnique();

            entity.Property(e => e.MedicalTestId).HasColumnName("medical_test_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.MedicalRecordDetailId).HasColumnName("medical_record_detail_id");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.MedicalRecordDetail).WithMany(p => p.MedicalTests)
                .HasForeignKey(d => d.MedicalRecordDetailId)
                .HasConstraintName("FK__medical_t__medic__5070F446");

            entity.HasOne(d => d.Service).WithMany(p => p.MedicalTests)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK__medical_t__servi__5165187F");

            entity.HasOne(d => d.Status).WithMany(p => p.MedicalTests)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK__medical_t__statu__52593CB8");
        });

        modelBuilder.Entity<MedicalTestResult>(entity =>
        {
            entity.HasKey(e => e.MedicalTestResultId).HasName("PK__medical___FB9D72233EC78904");

            entity.ToTable("medical_test_results");

            entity.Property(e => e.MedicalTestResultId).HasColumnName("medical_test_result_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Image)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("image");
            entity.Property(e => e.MedicalTestId).HasColumnName("medical_test_id");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.Parameter)
                .HasMaxLength(255)
                .HasColumnName("parameter");
            entity.Property(e => e.ReferenceRange)
                .HasMaxLength(255)
                .HasColumnName("reference_range");
            entity.Property(e => e.Unit)
                .HasMaxLength(50)
                .HasColumnName("unit");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.Value)
                .HasMaxLength(255)
                .HasColumnName("value");

            entity.HasOne(d => d.MedicalTest).WithMany(p => p.MedicalTestResults)
                .HasForeignKey(d => d.MedicalTestId)
                .HasConstraintName("FK__medical_t__medic__571DF1D5");
        });

        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.HasKey(e => e.MedicineId).HasName("PK__medicine__E7148EBB807DA8C0");

            entity.ToTable("medicines");

            entity.Property(e => e.MedicineId).HasColumnName("medicine_id");
            entity.Property(e => e.MedicineName)
                .HasMaxLength(100)
                .HasColumnName("medicine_name");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.Stock).HasColumnName("stock");
            entity.Property(e => e.Unit)
                .HasMaxLength(50)
                .HasColumnName("unit");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__messages__0BBF6EE66EF7B7A7");

            entity.ToTable("messages");

            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ConversationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_messages_conversation");

            entity.HasOne(d => d.Sender).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_messages_sender");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__payments__ED1FC9EA76D71063");

            entity.ToTable("payments");

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.BillingId).HasColumnName("billing_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Billing).WithMany(p => p.Payments)
                .HasForeignKey(d => d.BillingId)
                .HasConstraintName("FK__payments__billin__19DFD96B");
        });

        modelBuilder.Entity<PaymentDetail>(entity =>
        {
            entity.HasKey(e => e.PaymentDetailId).HasName("PK__payment___C66E6E36D9E918C5");

            entity.ToTable("payment_details");

            entity.Property(e => e.PaymentDetailId).HasColumnName("payment_detail_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.BillingDetailId).HasColumnName("billing_detail_id");
            entity.Property(e => e.BillingMedicineId).HasColumnName("billing_medicine_id");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("payment_date");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.PaymentMethodId).HasColumnName("payment_method_id");

            entity.HasOne(d => d.BillingDetail).WithMany(p => p.PaymentDetails)
                .HasForeignKey(d => d.BillingDetailId)
                .HasConstraintName("FK__payment_d__billi__22751F6C");

            entity.HasOne(d => d.BillingMedicine).WithMany(p => p.PaymentDetails)
                .HasForeignKey(d => d.BillingMedicineId)
                .HasConstraintName("FK__payment_d__billi__65370702");

            entity.HasOne(d => d.Payment).WithMany(p => p.PaymentDetails)
                .HasForeignKey(d => d.PaymentId)
                .HasConstraintName("FK__payment_d__payme__2180FB33");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.PaymentDetails)
                .HasForeignKey(d => d.PaymentMethodId)
                .HasConstraintName("FK__payment_d__payme__236943A5");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("PK__payment___8A3EA9EBC9B97CC5");

            entity.ToTable("payment_methods");

            entity.HasIndex(e => e.MethodName, "UQ__payment___2DA2FAEE4230814E").IsUnique();

            entity.Property(e => e.PaymentMethodId).HasColumnName("payment_method_id");
            entity.Property(e => e.MethodName)
                .HasMaxLength(100)
                .HasColumnName("method_name");
        });

        modelBuilder.Entity<PaymentStatus>(entity =>
        {
            entity.HasKey(e => e.PaymentStatusId).HasName("PK__payment___E6BF50152DCB0297");

            entity.ToTable("payment_status");

            entity.HasIndex(e => e.StatusName, "UQ__payment___501B3753E894A84C").IsUnique();

            entity.Property(e => e.PaymentStatusId).HasColumnName("payment_status_id");
            entity.Property(e => e.StatusName)
                .HasMaxLength(100)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.PrescriptionId).HasName("PK__prescrip__3EE444F8814CCB54");

            entity.ToTable("prescriptions");

            entity.HasIndex(e => e.MedicalRecordDetailId, "UQ_prescriptions_medical_record_detail_id").IsUnique();

            entity.Property(e => e.PrescriptionId).HasColumnName("prescription_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.MedicalRecordDetailId).HasColumnName("medical_record_detail_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.MedicalRecordDetail).WithOne(p => p.Prescription)
                .HasForeignKey<Prescription>(d => d.MedicalRecordDetailId)
                .HasConstraintName("FK__prescript__medic__5BE2A6F2");
        });

        modelBuilder.Entity<PrescriptionDetail>(entity =>
        {
            entity.HasKey(e => e.PrescriptionDetailId).HasName("PK__prescrip__CD190C8B06358C59");

            entity.ToTable("prescription_details");

            entity.HasIndex(e => new { e.PrescriptionId, e.MedicineId }, "UQ_prescription_details_prescription_medicine").IsUnique();

            entity.Property(e => e.PrescriptionDetailId).HasColumnName("prescription_detail_id");
            entity.Property(e => e.MedicineId).HasColumnName("medicine_id");
            entity.Property(e => e.PrescriptionId).HasColumnName("prescription_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Usage).HasColumnName("usage");

            entity.HasOne(d => d.Medicine).WithMany(p => p.PrescriptionDetails)
                .HasForeignKey(d => d.MedicineId)
                .HasConstraintName("FK__prescript__medic__6383C8BA");

            entity.HasOne(d => d.Prescription).WithMany(p => p.PrescriptionDetails)
                .HasForeignKey(d => d.PrescriptionId)
                .HasConstraintName("FK__prescript__presc__628FA481");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId).HasName("PK__refresh___B0A1F7C7BDACD8B0");

            entity.ToTable("refresh_tokens");

            entity.Property(e => e.RefreshTokenId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("refresh_token_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresTime)
                .HasColumnType("datetime")
                .HasColumnName("expires_time");
            entity.Property(e => e.Token)
                .IsUnicode(false)
                .HasColumnName("token");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__refresh_t__user___33D4B598");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__roles__760965CC5A4FEC14");

            entity.ToTable("roles");

            entity.HasIndex(e => e.RoleName, "UQ__roles__783254B14EA4606F").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(100)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__services__3E0DB8AF5CB98BD5");

            entity.ToTable("services");

            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(200)
                .HasColumnName("service_name");
        });

        modelBuilder.Entity<Specialty>(entity =>
        {
            entity.HasKey(e => e.SpecialtyId).HasName("PK__specialt__B90D8D12361A7167");

            entity.ToTable("specialties");

            entity.Property(e => e.SpecialtyId).HasColumnName("specialty_id");
            entity.Property(e => e.SpecialtyName)
                .HasMaxLength(100)
                .HasColumnName("specialty_name");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__status__3683B5315E6C4043");

            entity.ToTable("status");

            entity.HasIndex(e => e.StatusName, "UQ__status__501B3753B0B065D9").IsUnique();

            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.StatusName)
                .HasMaxLength(100)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__users__B9BE370FB79BDA8C");

            entity.ToTable("users");

            entity.HasIndex(e => e.Phone, "UQ__users__B43B145F57DC0D5E").IsUnique();

            entity.Property(e => e.UserId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("user_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.BirthDate)
                .HasDefaultValue(new DateOnly(1900, 1, 1))
                .HasColumnName("birth_date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(250)
                .HasColumnName("fullname");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.Image)
                .IsUnicode(false)
                .HasColumnName("image");
            entity.Property(e => e.Password)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.SpecialtyId).HasColumnName("specialty_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.Weight).HasColumnName("weight");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__users__role_id__2E1BDC42");

            entity.HasOne(d => d.Specialty).WithMany(p => p.Users)
                .HasForeignKey(d => d.SpecialtyId)
                .HasConstraintName("FK__users__specialty__2D27B809");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
