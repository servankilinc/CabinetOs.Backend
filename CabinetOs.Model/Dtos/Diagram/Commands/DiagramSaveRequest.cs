using System.Text.Json.Serialization;
using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.Diagram.Commands;

public class DiagramSaveRequest : IDto
{
    public EntityDelta<DeviceCreateDraft, DeviceUpdateDraft> Devices { get; set; } = new();
    public EntityDelta<ConnectionCreateDraft, ConnectionUpdateDraft> Connections { get; set; } = new();
    public EntityDelta<AnnotationCreateDraft, AnnotationUpdateDraft> Annotations { get; set; } = new();

    /// <summary> Hicbir sey degismemisse sunucu transaction bile acmaz. </summary>
    [JsonIgnore]
    public bool IsEmpty => Devices.IsEmpty && Connections.IsEmpty && Annotations.IsEmpty;

    /// <summary>
    /// Tum gecici kimlikler GLOBAL dogrulanir: React Flow'da cihaz ve not node'lari ayni id uzayinda yasar,
    /// dolayisiyla bir cihazla bir notun ayni gecici kimligi tasimasi istemcinin kendi node haritasini bozardi.
    /// </summary>
    public IEnumerable<string> AllTempIds() =>
        Devices.Created.Select(d => d.TempId)
            .Concat(Connections.Created.Select(c => c.TempId))
            .Concat(Annotations.Created.Select(a => a.TempId));
}

public class DiagramSaveRequestValidator : AbstractValidator<DiagramSaveRequest>
{
    public DiagramSaveRequestValidator()
    {
        RuleForEach(v => v.Devices.Created).SetValidator(new DeviceCreateDraftValidator());
        RuleForEach(v => v.Devices.Updated).SetValidator(new DeviceUpdateDraftValidator());
        RuleForEach(v => v.Connections.Created).SetValidator(new ConnectionCreateDraftValidator());
        RuleForEach(v => v.Connections.Updated).SetValidator(new ConnectionUpdateDraftValidator());
        RuleForEach(v => v.Annotations.Created).SetValidator(new AnnotationCreateDraftValidator());
        RuleForEach(v => v.Annotations.Updated).SetValidator(new AnnotationUpdateDraftValidator());

        RuleFor(v => v).Must(HasUniqueTempIds)
            .OverridePropertyName("TempId")
            .WithMessage("Ayni gecici kimlik birden fazla taslakta kullanilmis");

        AddDeltaConsistencyRules(v => v.Devices, "Devices", "cihaz");
        AddDeltaConsistencyRules(v => v.Connections, "Connections", "kablo");
        AddDeltaConsistencyRules(v => v.Annotations, "Annotations", "not");
    }

    private static bool HasUniqueTempIds(DiagramSaveRequest request)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        return request.AllTempIds().All(seen.Add);
    }

    /// <summary> Bir ailenin kendi icindeki celiskileri: ayni Id'nin iki kez guncellenmesi, iki kez silinmesi ya da hem guncellenip hem silinmesi. </summary>
    private void AddDeltaConsistencyRules<TCreate, TUpdate>(Func<DiagramSaveRequest, EntityDelta<TCreate, TUpdate>> selector, string propertyName, string label) where TUpdate : IIdentifiableDraft
    {
        RuleFor(v => selector(v))
            .Must(d => d.Updated.Select(u => u.Id).Distinct().Count() == d.Updated.Count)
            .OverridePropertyName($"{propertyName}.Updated")
            .WithMessage($"Ayni {label} guncelleme listesinde birden fazla kez var");

        RuleFor(v => selector(v))
            .Must(d => d.Deleted.Distinct().Count() == d.Deleted.Count)
            .OverridePropertyName($"{propertyName}.Deleted")
            .WithMessage($"Ayni {label} silme listesinde birden fazla kez var");

        RuleFor(v => selector(v))
            .Must(d => !d.Updated.Select(u => u.Id).Intersect(d.Deleted).Any())
            .OverridePropertyName($"{propertyName}.Deleted")
            .WithMessage($"Ayni {label} hem guncelleniyor hem siliniyor");
    }
}
