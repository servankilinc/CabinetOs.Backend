using CabinetOs.Business.Abstract;
using CabinetOs.Business.Utils;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Core.Utils.Validation;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Dtos.Diagram.Commands;
using CabinetOs.Model.Dtos.Diagram.Queries;
using CabinetOs.Model.Entities;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Business.Concrete;

/// <summary>
/// Toplu kaydetmenin ic adimlari: DB'den okuma, referans dogrulama ve uygulama.
/// Orkestrasyon <c>DiagramService.Save.cs</c>'teki <c>SaveAsync</c>'te.
/// </summary>
public partial class DiagramService
{

    // ==================== YUKLEME ====================

    /// <summary>
    /// Kaydetme icin gereken her seyi DB'den TEK SEFERDE okur.
    ///
    /// Taslak basina sorgu atmak, 50 node'luk bir kaydetmede 50 gidis-donus ederdi;
    /// buradaki her sorgu bir AILEYI toplu ceker. Guncellenecek ve silinecek
    /// varliklar TAKIPLI (tracking) okunur — EF degisikligi kendisi yakalar,
    /// ayrica <c>Update()</c> cagirmaya gerek kalmaz.
    ///
    /// Tum sorgular kabine baglanir (<c>CabinetId == cabinetId</c>): baska bir
    /// kabinin satirini Id'siyle gondermek boylece "bulunamadi" olur, sessiz bir
    /// capraz-kabin duzenlemesi degil.
    /// </summary>
    private async Task<SaveContext> LoadSaveContextAsync(Guid cabinetId, DiagramSaveRequest request, CancellationToken cancellationToken)
    {
        // Yukleyiciler arasi bagimliliklar parametre listelerinde GORUNUR: pin kumesi
        // silinen cihazlardan, cascade kablolar o pinlerden, mevcut ciftler de
        // cascade'den turer. Sira tesaduf degil.
        var templateIds = request.Devices.Created.Select(d => d.ComponentTemplateId).Distinct().ToList();
        var referencedPinIds = request.Connections.Created
            .SelectMany(c => new[] { c.SourcePinId, c.TargetPinId })
            .Distinct().ToList();

        var activeTemplateIds = await LoadActiveTemplateIdsAsync(templateIds, cancellationToken);
        var templatePins = await LoadTemplatePinsAsync(templateIds, cancellationToken);
        var devices = await LoadDevicesAsync(cabinetId, request, cancellationToken);
        var pins = await LoadPinsAsync(cabinetId, referencedPinIds, request.Devices.Deleted, cancellationToken);

        // Pin yalnizca cihaziyla birlikte kalkar — cihaz uzerinde tekil pin silme yok.
        var pinsBeingRemoved = new HashSet<Guid>(
            pins.Values.Where(p => request.Devices.Deleted.Contains(p.DeviceId)).Select(p => p.Id));

        var cascadeConnections = await LoadCascadeConnectionsAsync(cabinetId, pinsBeingRemoved, cancellationToken);
        var connections = await LoadConnectionsAsync(cabinetId, request, cancellationToken);
        var annotations = await LoadAnnotationsAsync(cabinetId, request, cancellationToken);
        var existingPairs = await LoadExistingPairsAsync(cabinetId, referencedPinIds, request, cascadeConnections, cancellationToken);
        var deviceExternalCodes = await LoadDeviceExternalCodesAsync(cabinetId, request, cancellationToken);

        return new SaveContext
        {
            ActiveTemplateIds = activeTemplateIds,
            TemplatePins = templatePins,
            Devices = devices,
            Pins = pins,
            Connections = connections,
            Annotations = annotations,
            PinsBeingRemoved = pinsBeingRemoved,
            CascadeConnections = cascadeConnections,
            ExistingPairs = existingPairs,
            DeviceExternalCodes = deviceExternalCodes
        };
    }

    /// <summary>
    /// Yeni cihazlarin sablonlari. Palet gibi burasi da bir SECIM kaynagi:
    /// pasif sablon yeni cihaz uretmemeli.
    /// </summary>
    private async Task<HashSet<Guid>> LoadActiveTemplateIdsAsync(List<Guid> templateIds, CancellationToken cancellationToken)
    {
        if (templateIds.Count == 0) return [];

        var rows = await _unitOfWork.ComponentTemplates.GetAllAsync(
            select: t => t.Id,
            where: t => templateIds.Contains(t.Id) && t.IsActive,
            cancellationToken: cancellationToken) ?? [];

        return rows.ToHashSet();
    }

    /// <summary>
    /// Yeni cihazlarin pinleri her zaman bunlardan uretilir. Pini olmayan sablon
    /// burada hic anahtar acmaz ve cihaz pinsiz dogar.
    ///
    /// <c>tracking: false</c> — bunlar yalnizca KOPYALAMA kaynagi; takip edilirlerse
    /// degistirilmedikleri halde change tracker'i sisirirler.
    /// </summary>
    private async Task<Dictionary<Guid, List<ComponentTemplatePin>>> LoadTemplatePinsAsync(List<Guid> templateIds, CancellationToken cancellationToken)
    {
        if (templateIds.Count == 0) return [];

        var rows = await _unitOfWork.ComponentTemplatePins.GetAllAsync(
            where: p => templateIds.Contains(p.ComponentTemplateId),
            orderBy: q => q.OrderBy(p => p.Name),
            tracking: false,
            cancellationToken: cancellationToken) ?? [];

        return rows.GroupBy(p => p.ComponentTemplateId).ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>Guncellenecek ve silinecek cihazlar; TAKIPLI okunur.</summary>
    private async Task<Dictionary<Guid, Device>> LoadDevicesAsync(Guid cabinetId, DiagramSaveRequest request, CancellationToken cancellationToken)
    {
        var deviceIds = request.Devices.Updated.Select(d => d.Id)
            .Concat(request.Devices.Deleted)
            .Distinct().ToList();

        if (deviceIds.Count == 0) return [];

        var rows = await _unitOfWork.Devices.GetAllAsync(
            where: d => d.CabinetId == cabinetId && d.IsActive && deviceIds.Contains(d.Id),
            cancellationToken: cancellationToken) ?? [];

        return rows.ToDictionary(d => d.Id);
    }

    /// <summary>Yeni kablolarin uclari + silinen cihazlarin pinleri (cascade icin).</summary>
    private async Task<Dictionary<Guid, Pin>> LoadPinsAsync(
        Guid cabinetId,
        List<Guid> referencedPinIds,
        List<Guid> deletedDeviceIds,
        CancellationToken cancellationToken)
    {
        if (referencedPinIds.Count == 0 && deletedDeviceIds.Count == 0) return [];

        var rows = await _unitOfWork.Pins.GetAllAsync(
            where: p => p.Device!.CabinetId == cabinetId
                     && (referencedPinIds.Contains(p.Id) || deletedDeviceIds.Contains(p.DeviceId)),
            cancellationToken: cancellationToken) ?? [];

        return rows.ToDictionary(p => p.Id);
    }

    /// <summary>Pini kalktigi icin birlikte silinecek kablolar.</summary>
    private async Task<List<Connection>> LoadCascadeConnectionsAsync(Guid cabinetId, HashSet<Guid> pinsBeingRemoved, CancellationToken cancellationToken)
    {
        if (pinsBeingRemoved.Count == 0) return [];

        var removedPinIds = pinsBeingRemoved.ToList();
        var rows = await _unitOfWork.Connections.GetAllAsync(
            where: c => c.CabinetId == cabinetId
                     && (removedPinIds.Contains(c.SourcePinId) || removedPinIds.Contains(c.TargetPinId)),
            cancellationToken: cancellationToken) ?? [];

        return rows.ToList();
    }

    /// <summary>Guncellenecek ve silinecek kablolar; TAKIPLI okunur.</summary>
    private async Task<Dictionary<Guid, Connection>> LoadConnectionsAsync(Guid cabinetId, DiagramSaveRequest request, CancellationToken cancellationToken)
    {
        var connectionIds = request.Connections.Updated.Select(c => c.Id)
            .Concat(request.Connections.Deleted).Distinct().ToList();

        if (connectionIds.Count == 0) return [];

        var rows = await _unitOfWork.Connections.GetAllAsync(
            where: c => c.CabinetId == cabinetId && connectionIds.Contains(c.Id),
            cancellationToken: cancellationToken) ?? [];

        return rows.ToDictionary(c => c.Id);
    }

    /// <summary>Guncellenecek ve silinecek notlar; TAKIPLI okunur.</summary>
    private async Task<Dictionary<Guid, DiagramAnnotation>> LoadAnnotationsAsync(Guid cabinetId, DiagramSaveRequest request, CancellationToken cancellationToken)
    {
        var annotationIds = request.Annotations.Updated.Select(a => a.Id)
            .Concat(request.Annotations.Deleted).Distinct().ToList();

        if (annotationIds.Count == 0) return [];

        var rows = await _unitOfWork.DiagramAnnotations.GetAllAsync(
            where: a => a.CabinetId == cabinetId && annotationIds.Contains(a.Id),
            cancellationToken: cancellationToken) ?? [];

        return rows.ToDictionary(a => a.Id);
    }

    /// <summary>
    /// Kaydetmeden SONRA da ayakta kalacak pin ciftleri — yeni kablonun cakisip
    /// cakismadigi buna bakilarak anlasilir.
    ///
    /// Bu gonderide kalkacak kablolar cakisma sayilmaz: kullanicinin bir kabloyu
    /// silip ayni iki pin arasina yenisini cizmesi mesru bir islemdir.
    /// </summary>
    private async Task<HashSet<(Guid, Guid)>> LoadExistingPairsAsync(
        Guid cabinetId,
        List<Guid> referencedPinIds,
        DiagramSaveRequest request,
        List<Connection> cascadeConnections,
        CancellationToken cancellationToken)
    {
        if (referencedPinIds.Count == 0) return [];

        var rows = await _unitOfWork.Connections.GetAllAsync(
            select: c => new PinPairRow(c.Id, c.SourcePinId, c.TargetPinId),
            where: c => c.CabinetId == cabinetId
                     && (referencedPinIds.Contains(c.SourcePinId) || referencedPinIds.Contains(c.TargetPinId)),
            cancellationToken: cancellationToken) ?? [];

        var removedConnectionIds = new HashSet<Guid>(request.Connections.Deleted);
        foreach (var connection in cascadeConnections) removedConnectionIds.Add(connection.Id);

        var existingPairs = new HashSet<(Guid, Guid)>();
        foreach (var row in rows)
        {
            if (removedConnectionIds.Contains(row.Id)) continue;
            existingPairs.Add(PairKey(row.SourcePinId, row.TargetPinId));
        }

        return existingPairs;
    }

    /// <summary>
    /// Kabindeki aktif cihazlarin dis kodlari —
    /// <c>IX_Device_CabinetId_ExternalCode</c> (unique, WHERE ExternalCode IS NOT NULL
    /// AND IsActive = 1). Gonderide hic kod yoksa sorgu ATILMAZ.
    /// </summary>
    private async Task<Dictionary<Guid, string>> LoadDeviceExternalCodesAsync(Guid cabinetId, DiagramSaveRequest request, CancellationToken cancellationToken)
    {
        var needsCodeCheck = request.Devices.Created.Any(d => !string.IsNullOrWhiteSpace(d.ExternalCode))
                          || request.Devices.Updated.Any(d => !string.IsNullOrWhiteSpace(d.ExternalCode));

        if (!needsCodeCheck) return [];

        var rows = await _unitOfWork.Devices.GetAllAsync(
            select: d => new DeviceCodeRow(d.Id, d.ExternalCode!),
            where: d => d.CabinetId == cabinetId && d.IsActive && d.ExternalCode != null,
            cancellationToken: cancellationToken) ?? [];

        return rows.ToDictionary(r => r.Id, r => r.ExternalCode);
    }

    // ==================== REFERANS DOGRULAMA ====================

    /// <summary>
    /// FluentValidation'in goremedigi her sey: taslaklarin isaret ettigi satirlarin
    /// gercekten VAR OLDUGU, BU KABINE ait oldugu ve birbirleriyle celismedigi.
    ///
    /// Hepsi 400 doner. DB kisitina carpip 500 uretmek yerine burada yakalamak,
    /// kullaniciya hangi taslagin hatali oldugunu indeksiyle soyleyebilmek demek.
    /// </summary>
    private static Dictionary<string, string[]> ValidateReferences(DiagramSaveRequest request, SaveContext context)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        ValidateDevices(request, context, errors);
        ValidateDeviceExternalCodes(request, context, new HashSet<Guid>(request.Devices.Deleted), errors);
        ValidateConnections(request, context, errors);
        ValidateAnnotations(request, context, errors);

        return errors.ToDictionary(e => e.Key, e => e.Value.ToArray(), StringComparer.Ordinal);
    }

    /// <summary>Cihaz taslaklarinin sablonu ve hedef satirlari var mi.</summary>
    private static void ValidateDevices(DiagramSaveRequest request, SaveContext context, Dictionary<string, List<string>> errors)
    {
        for (int i = 0; i < request.Devices.Created.Count; i++)
        {
            if (!context.ActiveTemplateIds.Contains(request.Devices.Created[i].ComponentTemplateId))
                AddError(errors, $"Devices.Created[{i}].ComponentTemplateId", "Sablon bulunamadi veya pasif durumda");
        }
        for (int i = 0; i < request.Devices.Updated.Count; i++)
        {
            if (!context.Devices.ContainsKey(request.Devices.Updated[i].Id))
                AddError(errors, $"Devices.Updated[{i}].Id", "Cihaz bu kabinde bulunamadi");
        }
        for (int i = 0; i < request.Devices.Deleted.Count; i++)
        {
            if (!context.Devices.ContainsKey(request.Devices.Deleted[i]))
                AddError(errors, $"Devices.Deleted[{i}]", "Cihaz bu kabinde bulunamadi");
        }
    }

    /// <summary>
    /// Kablo taslaklari: uclar cozulebiliyor mu, cift zaten var mi, gerilimler uyuyor mu.
    /// </summary>
    private static void ValidateConnections(DiagramSaveRequest request, SaveContext context, Dictionary<string, List<string>> errors)
    {
        var cascadeConnectionIds = context.CascadeConnections.Select(c => c.Id).ToHashSet();
        var pairsInRequest = new HashSet<(Guid, Guid)>();

        for (int i = 0; i < request.Connections.Created.Count; i++)
        {
            var draft = request.Connections.Created[i];
            var source = ResolveEndpoint(draft.SourcePinId, context, errors, $"Connections.Created[{i}].SourcePinId");
            var target = ResolveEndpoint(draft.TargetPinId, context, errors, $"Connections.Created[{i}].TargetPinId");

            if (source == null || target == null) continue;

            var pair = PairKey(draft.SourcePinId, draft.TargetPinId);
            // Cift, YONSUZ karsilastirilir. DB'deki unique index (SourcePinId, TargetPinId)
            // sirali oldugu icin ters cizilmis ayni kabloyu YAKALAMAZ; ConnectionMode.Loose
            // ile "kaynak"/"hedef" zaten keyfi oldugundan burada daha katiyiz.
            if (!pairsInRequest.Add(pair) || context.ExistingPairs.Contains(pair))
            {
                AddError(errors, $"Connections.Created[{i}]", "Bu iki pin arasinda zaten bir kablo var");
                continue;
            }

            // Gerilim uyusmazligi: iki taraf da BELIRTILMISSE ve farkliysa reddedilir.
            // Biri null ise ("belirtilmemis") susulur — bilinmeyeni hata saymak,
            // gerilimi henuz girilmemis sablonlarla calismayi imkansiz kilardi.
            if (source.VoltageLevel.HasValue && target.VoltageLevel.HasValue
                && source.VoltageLevel.Value != target.VoltageLevel.Value)
            {
                AddError(errors, $"Connections.Created[{i}]", "Farkli gerilim seviyesindeki pinler baglanamaz");
            }
        }
        for (int i = 0; i < request.Connections.Updated.Count; i++)
        {
            var id = request.Connections.Updated[i].Id;
            if (!context.Connections.ContainsKey(id))
                AddError(errors, $"Connections.Updated[{i}].Id", "Kablo bu kabinde bulunamadi");
            else if (cascadeConnectionIds.Contains(id))
                AddError(errors, $"Connections.Updated[{i}].Id", "Bu kablo, pini silindigi icin kaldiriliyor; ayni gonderide guncellenemez");
        }
        for (int i = 0; i < request.Connections.Deleted.Count; i++)
        {
            if (!context.Connections.ContainsKey(request.Connections.Deleted[i]))
                AddError(errors, $"Connections.Deleted[{i}]", "Kablo bu kabinde bulunamadi");
        }
    }

    /// <summary>Not taslaklarinin hedef satirlari bu kabinde var mi.</summary>
    private static void ValidateAnnotations(DiagramSaveRequest request, SaveContext context, Dictionary<string, List<string>> errors)
    {
        for (int i = 0; i < request.Annotations.Updated.Count; i++)
        {
            if (!context.Annotations.ContainsKey(request.Annotations.Updated[i].Id))
                AddError(errors, $"Annotations.Updated[{i}].Id", "Not bu kabinde bulunamadi");
        }
        for (int i = 0; i < request.Annotations.Deleted.Count; i++)
        {
            if (!context.Annotations.ContainsKey(request.Annotations.Deleted[i]))
                AddError(errors, $"Annotations.Deleted[{i}]", "Not bu kabinde bulunamadi");
        }
    }

    /// <summary>
    /// Ayni alanda birden fazla hata birikebilsin diye.
    /// <c>ProblemDetails.errors</c> sozlugunun sekli: alan -> mesaj dizisi.
    /// </summary>
    private static void AddError(Dictionary<string, List<string>> errors, string key, string message)
    {
        if (!errors.TryGetValue(key, out var messages)) errors[key] = messages = [];
        messages.Add(message);
    }

    /// <summary>
    /// Dis kodlarin kabin icinde benzersizligi — <c>IX_Device_CabinetId_ExternalCode</c>.
    /// Kod SCADA tarafindaki kimliktir; ayni kodun iki cihaza dusmesi telemetriyi
    /// yanlis cihaza yazardi, dolayisiyla index yalnizca bir performans detayi degil.
    /// </summary>
    private static void ValidateDeviceExternalCodes(
        DiagramSaveRequest request,
        SaveContext context,
        HashSet<Guid> deletedDeviceIds,
        Dictionary<string, List<string>> errors)
    {
        var updatedDeviceIds = request.Devices.Updated.Select(d => d.Id).ToHashSet();
        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (deviceId, code) in context.DeviceExternalCodes)
        {
            // Pasife alinan cihaz index filtresinden duser, guncellenenin yeni degeri
            // asagida eklenecek — ikisi de mevcut kod sayilmaz.
            if (deletedDeviceIds.Contains(deviceId) || updatedDeviceIds.Contains(deviceId)) continue;
            codes.Add(code);
        }

        for (int i = 0; i < request.Devices.Updated.Count; i++)
        {
            var code = request.Devices.Updated[i].ExternalCode;
            if (string.IsNullOrWhiteSpace(code)) continue;
            if (!codes.Add(code))
                AddError(errors, $"Devices.Updated[{i}].ExternalCode", "Bu kabinde ayni dis koda sahip baska bir cihaz var");
        }
        for (int i = 0; i < request.Devices.Created.Count; i++)
        {
            var code = request.Devices.Created[i].ExternalCode;
            if (string.IsNullOrWhiteSpace(code)) continue;
            if (!codes.Add(code))
                AddError(errors, $"Devices.Created[{i}].ExternalCode", "Bu kabinde ayni dis koda sahip baska bir cihaz var");
        }
    }

    /// <summary>
    /// Bir kablo ucunu cozer. Uc her zaman KALICI bir pindir; cozulemezse hatayi
    /// yazar ve null doner. Doner deger gerilim karsilastirmasi icin kullanilir.
    /// </summary>
    private static Pin? ResolveEndpoint(Guid pinId, SaveContext context, Dictionary<string, List<string>> errors, string errorKey)
    {
        if (!context.Pins.TryGetValue(pinId, out var pin))
        {
            AddError(errors, errorKey, "Pin bu kabinde bulunamadi");
            return null;
        }
        if (context.PinsBeingRemoved.Contains(pinId))
        {
            AddError(errors, errorKey, "Cihazi ayni gonderide silinen bir pine kablo cizilemez");
            return null;
        }
        return pin;
    }

    /// <summary>Ciftin YONSUZ anahtari: (a,b) ile (b,a) ayni kabloyu gosterir.</summary>
    private static (Guid, Guid) PairKey(Guid first, Guid second)
        => first.CompareTo(second) <= 0 ? (first, second) : (second, first);

    // ==================== UYGULAMA ====================

    /// <summary>
    /// Silmeler. Sira TERS BAGIMLILIK yonunde: once kablolar, sonra pinler, en son
    /// cihazlar — aksi halde silinen bir pine bagli kablo bir an icin oksuz kalirdi.
    /// </summary>
    private void ApplyDeletions(DiagramSaveRequest request, SaveContext context)
    {
        // 1) Kablolar: dogrudan silinenler + pini/cihazi kalkanlar
        var connectionsToRemove = request.Connections.Deleted
            .Select(id => context.Connections[id])
            .Concat(context.CascadeConnections)
            .DistinctBy(c => c.Id)
            .ToList();
        if (connectionsToRemove.Count > 0)
            _unitOfWork.Connections.Delete(connectionsToRemove);

        // 2) Pinler (ISoftDeletableEntity: interceptor Remove'u IsDeleted=true'ya cevirir)
        var pinsToRemove = context.PinsBeingRemoved.Select(id => context.Pins[id]).ToList();
        if (pinsToRemove.Count > 0)
            _unitOfWork.Pins.Delete(pinsToRemove);

        // 3) Cihazlar: Remove() CAGRILMAZ. Device IActivatableEntity oldugu icin
        //    EntityLifecycleInterceptor hard delete'te exception atar ve 500 doner.
        foreach (var id in request.Devices.Deleted)
            context.Devices[id].IsActive = false;

        // 4) Notlar: sistemdeki TEK hard delete. DiagramAnnotation ne
        //    IActivatableEntity ne ISoftDeletableEntity oldugundan interceptor araya
        //    girmez ve satir gercekten silinir; kimse ona FK ile bagli olmadigi icin
        //    oksuz satir birakmaz.
        var annotationsToRemove = request.Annotations.Deleted.Select(id => context.Annotations[id]).ToList();
        if (annotationsToRemove.Count > 0)
            _unitOfWork.DiagramAnnotations.Delete(annotationsToRemove);
    }

    /// <summary>
    /// Olusturmalar. Uc aile birbirinden bagimsiz; sira onemli degil.
    /// </summary>
    private CreatedEntities ApplyCreations(Guid cabinetId, DiagramSaveRequest request, SaveContext context)
    {
        var created = new CreatedEntities();

        CreateDevices(cabinetId, request, context, created);
        CreateConnections(cabinetId, request, created);
        CreateAnnotations(cabinetId, request, created);

        return created;
    }

    /// <summary>
    /// Yeni cihazlar ve — sablonu pinliyse — onlarla birlikte dogan pin/kanallar.
    /// </summary>
    private void CreateDevices(Guid cabinetId, DiagramSaveRequest request, SaveContext context, CreatedEntities created)
    {
        foreach (var draft in request.Devices.Created)
        {
            var device = new Device
            {
                CabinetId = cabinetId,
                ComponentTemplateId = draft.ComponentTemplateId,
                Name = draft.Name,
                CoordinateX = draft.CoordinateX,
                CoordinateY = draft.CoordinateY,
                Rotation = draft.Rotation,
                ZIndex = draft.ZIndex,
                IsLocked = draft.IsLocked,
                IsVisible = draft.IsVisible,
                ExternalCode = draft.ExternalCode,
                // B5: create DTO'sunda IsActive yok, servis ACIKCA true yazar.
                // Yazilmazsa kayit pasif dogar ve diyagram okumasindan dusmus olur.
                IsActive = true
            };
            _unitOfWork.Devices.Add(device);
            created.Devices.Add((draft.TempId, device));

            // Pini olmayan sablon: cihaz pinsiz dogar. Bu bir SECIM degil, sablonun
            // sonucu — pin yazarligi sablon ekranina aittir.
            if (context.TemplatePins.TryGetValue(draft.ComponentTemplateId, out var templatePins))
                InstantiateTemplatePins(device, templatePins, created);
        }
    }

    /// <summary>
    /// Sablonun pin semasini cihaza kopyalar ve kanal numarasi tasiyan her pin icin
    /// bir <c>IoChannel</c> uretir.
    ///
    /// <b>Kanallar neden burada dogar.</b> Bu olmadan SCADA ingest'inin yazacagi
    /// HICBIR SATIR olmazdi: ingest kanali <c>(DeviceId, ChannelNumber)</c> ile
    /// cozuyor ve tanimadigi kanali sessizce atliyor (K7). Kanallari ureten baska bir
    /// yol da yok — urunde cihaz yaratmanin tek yolu paletten birakmak.
    ///
    /// <b>Neden navigasyon, neden skaler FK degil.</b> <c>pin.Device = device</c> ve
    /// <c>pin.IoChannel = channel</c> yaziliyor cunku Id'ler <c>SaveChanges</c>'e
    /// kadar kesinlesmiyor. EF ekleme sirasini ve FK degerlerini bu iliskilerden
    /// kendisi cozer.
    /// </summary>
    private void InstantiateTemplatePins(Device device, List<ComponentTemplatePin> templatePins, CreatedEntities created)
    {
        // Ayni cihazda ayni kanal numarasi TEK bir IoChannel'dir. Sablonda iki pin
        // ayni kanali gosteriyorsa (or. bir girisin besleme ve donus ucu) ikisi de
        // ayni kanala baglanir; ayri ayri uretmek
        // IX_IoChannel_DeviceId_ChannelNumber'i ihlal ederdi.
        var channelsByNumber = new Dictionary<int, IoChannel>();

        foreach (var templatePin in templatePins)
        {
            var pin = new Pin
            {
                Device = device,
                ComponentTemplatePinId = templatePin.Id,
                Name = templatePin.Name,
                RelativeX = templatePin.RelativeX,
                RelativeY = templatePin.RelativeY,
                Side = templatePin.Side,
                Function = templatePin.Function,
                Direction = templatePin.Direction,
                VoltageLevel = templatePin.VoltageLevel,
                ChannelNumber = templatePin.ChannelNumber
            };

            if (templatePin.ChannelNumber is int channelNumber)
            {
                if (!channelsByNumber.TryGetValue(channelNumber, out var channel))
                {
                    channel = new IoChannel
                    {
                        Device = device,
                        ChannelNumber = channelNumber,
                        Direction = templatePin.Direction,
                        IsEnabled = true,
                        Name = templatePin.Name
                    };
                    _unitOfWork.IoChannels.Add(channel);
                    channelsByNumber[channelNumber] = channel;
                }

                pin.IoChannel = channel;
            }

            _unitOfWork.Pins.Add(pin);
            created.InstantiatedPinCount++;
        }
    }

    /// <summary>
    /// Yeni kablolar. Uclar HER ZAMAN kalici pinlerdir, dolayisiyla navigasyon degil
    /// skaler FK atanir.
    /// </summary>
    private void CreateConnections(Guid cabinetId, DiagramSaveRequest request, CreatedEntities created)
    {
        foreach (var draft in request.Connections.Created)
        {
            var connection = new Connection
            {
                CabinetId = cabinetId,
                SourcePinId = draft.SourcePinId,
                TargetPinId = draft.TargetPinId,
                Label = draft.Label,
                WireType = draft.WireType,
                Color = draft.Color,
                LineStyle = draft.LineStyle,
                StrokeWidth = draft.StrokeWidth,
                Routing = draft.Routing,
                WaypointsJson = DiagramWaypoints.Serialize(draft.Waypoints),
                ZIndex = draft.ZIndex
            };

            _unitOfWork.Connections.Add(connection);
            created.Connections.Add((draft.TempId, connection));
        }
    }

    /// <summary>Yeni notlar.</summary>
    private void CreateAnnotations(Guid cabinetId, DiagramSaveRequest request, CreatedEntities created)
    {
        foreach (var draft in request.Annotations.Created)
        {
            var annotation = new DiagramAnnotation
            {
                CabinetId = cabinetId,
                Name = draft.Name,
                CoordinateX = draft.CoordinateX,
                CoordinateY = draft.CoordinateY,
                Width = draft.Width,
                Height = draft.Height,
                Rotation = draft.Rotation,
                ZIndex = draft.ZIndex,
                IsLocked = draft.IsLocked,
                IsVisible = draft.IsVisible,
                Text = draft.Text,
                Shape = draft.Shape,
                BackgroundColor = draft.BackgroundColor,
                FontColor = draft.FontColor,
                FontSize = draft.FontSize,
                IsBold = draft.IsBold,
                BorderColor = draft.BorderColor
            };
            _unitOfWork.DiagramAnnotations.Add(annotation);
            created.Annotations.Add((draft.TempId, annotation));
        }
    }

    /// <summary>
    /// Guncellemeler. <c>Update()</c> CAGRILMAZ: varliklar takipli okundugu icin EF
    /// degisen alanlari kendisi tespit eder ve yalnizca onlari UPDATE'e koyar.
    /// <c>Update()</c> cagirmak butun kolonlari degismis isaretler ve telemetri
    /// alanlarini eski degerleriyle geri yazma riski dogurur.
    /// </summary>
    private static void ApplyUpdates(DiagramSaveRequest request, SaveContext context)
    {
        foreach (var draft in request.Devices.Updated)
        {
            var device = context.Devices[draft.Id];
            device.Name = draft.Name;
            device.CoordinateX = draft.CoordinateX;
            device.CoordinateY = draft.CoordinateY;
            device.Rotation = draft.Rotation;
            device.ZIndex = draft.ZIndex;
            device.IsLocked = draft.IsLocked;
            device.IsVisible = draft.IsVisible;
            device.ExternalCode = draft.ExternalCode;
            // DeviceStatusId / LastSeen / IpAddress / MacAddress DOKUNULMAZ.
        }

        foreach (var draft in request.Connections.Updated)
        {
            var connection = context.Connections[draft.Id];
            connection.Label = draft.Label;
            connection.WireType = draft.WireType;
            connection.Color = draft.Color;
            connection.LineStyle = draft.LineStyle;
            connection.StrokeWidth = draft.StrokeWidth;
            connection.Routing = draft.Routing;
            connection.WaypointsJson = DiagramWaypoints.Serialize(draft.Waypoints);
            connection.ZIndex = draft.ZIndex;
            // SourcePinId / TargetPinId DOKUNULMAZ — uc degistirmek sil + olustur.
        }

        foreach (var draft in request.Annotations.Updated)
        {
            var annotation = context.Annotations[draft.Id];
            annotation.Name = draft.Name;
            annotation.CoordinateX = draft.CoordinateX;
            annotation.CoordinateY = draft.CoordinateY;
            annotation.Width = draft.Width;
            annotation.Height = draft.Height;
            annotation.Rotation = draft.Rotation;
            annotation.ZIndex = draft.ZIndex;
            annotation.IsLocked = draft.IsLocked;
            annotation.IsVisible = draft.IsVisible;
            annotation.Text = draft.Text;
            annotation.Shape = draft.Shape;
            annotation.BackgroundColor = draft.BackgroundColor;
            annotation.FontColor = draft.FontColor;
            annotation.FontSize = draft.FontSize;
            annotation.IsBold = draft.IsBold;
            annotation.BorderColor = draft.BorderColor;
        }
    }

    // ==================== YARDIMCI TIPLER ====================

    /// <summary>Kaydetme icin DB'den bir kez okunan her sey.</summary>
    private sealed class SaveContext
    {
        public required HashSet<Guid> ActiveTemplateIds { get; init; }
        public required Dictionary<Guid, List<ComponentTemplatePin>> TemplatePins { get; init; }
        public required Dictionary<Guid, Device> Devices { get; init; }
        public required Dictionary<Guid, Pin> Pins { get; init; }
        public required Dictionary<Guid, Connection> Connections { get; init; }
        public required Dictionary<Guid, DiagramAnnotation> Annotations { get; init; }
        /// <summary>Bu gonderide cihaziyla birlikte kalkacak pinler.</summary>
        public required HashSet<Guid> PinsBeingRemoved { get; init; }
        /// <summary>Pini kalktigi icin birlikte silinecek kablolar.</summary>
        public required List<Connection> CascadeConnections { get; init; }
        /// <summary>Kaydetmeden sonra da ayakta kalacak pin ciftleri (yonsuz anahtar).</summary>
        public required HashSet<(Guid, Guid)> ExistingPairs { get; init; }
        /// <summary>Kabindeki aktif cihazlarin dis kodlari (yalnizca gerektiginde okunur).</summary>
        public required Dictionary<Guid, string> DeviceExternalCodes { get; init; }
    }

    private sealed class CreatedEntities
    {
        public List<(string TempId, Device Entity)> Devices { get; } = [];
        public List<(string TempId, Connection Entity)> Connections { get; } = [];
        public List<(string TempId, DiagramAnnotation Entity)> Annotations { get; } = [];
        public int InstantiatedPinCount { get; set; }
    }

    private sealed record PinPairRow(Guid Id, Guid SourcePinId, Guid TargetPinId);

    private sealed record DeviceCodeRow(Guid Id, string ExternalCode);
}
