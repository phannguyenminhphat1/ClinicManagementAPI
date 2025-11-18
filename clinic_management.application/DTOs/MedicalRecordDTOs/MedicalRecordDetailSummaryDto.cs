using System.Text.Json.Serialization;

public class MedicalRecordDetailSummaryDto
{
    [JsonPropertyName("patient_id")]
    public Guid PatientId { get; set; }

    [JsonPropertyName("patient_name")]
    public string? PatientName { get; set; }

    [JsonPropertyName("gender")]
    public byte? Gender { get; set; }

    [JsonPropertyName("birth_date")]
    public DateOnly BirthDate { get; set; }

    [JsonPropertyName("medical_record_id")]
    public int MedicalRecordId { get; set; }

    [JsonPropertyName("medical_record_detail_id")]
    public int MedicalRecordDetailId { get; set; }

    [JsonPropertyName("appointment_id")]
    public int AppointmentId { get; set; }

    [JsonPropertyName("scheduled_date")]
    public DateTime ScheduledDate { get; set; }

    [JsonPropertyName("status_name")]
    public string StatusName { get; set; } = null!;

    [JsonPropertyName("doctor_id")]
    public Guid? DoctorId { get; set; }

    [JsonPropertyName("doctor_name")]
    public string? DoctorName { get; set; }

    [JsonPropertyName("specialty_name")]
    public string? SpecialtyName { get; set; }

    [JsonPropertyName("symptoms")]
    public string Symptoms { get; set; } = null!;

    [JsonPropertyName("diagnosis")]
    public string? Diagnosis { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("requires_test")]
    public bool RequiresTest { get; set; }

    // ✅ Giờ tests là một danh sách (JSON array)
    [JsonPropertyName("tests")]
    public List<TestDto>? Tests { get; set; }

    // ✅ prescriptions cũng là danh sách
    [JsonPropertyName("prescriptions")]
    public List<PrescriptionDto>? Prescriptions { get; set; }
}

public class TestDto
{
    [JsonPropertyName("medical_test_id")]
    public int MedicalTestId { get; set; }

    [JsonPropertyName("service_name")]
    public string ServiceName { get; set; } = null!;

    [JsonPropertyName("test_results")]
    public List<TestResultDto>? TestResults { get; set; }
}

public class TestResultDto
{
    [JsonPropertyName("parameter")]
    public string? Parameter { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("reference_range")]
    public string? ReferenceRange { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }
}

public class PrescriptionDto
{
    [JsonPropertyName("prescription_id")]
    public int PrescriptionId { get; set; }

    [JsonPropertyName("medicines")]
    public List<MedicineDto>? Medicines { get; set; }
}

public class MedicineDto
{
    [JsonPropertyName("medicine_name")]
    public string MedicineName { get; set; } = null!;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("usage")]
    public string Usage { get; set; } = null!;
}
