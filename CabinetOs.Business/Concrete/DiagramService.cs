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

public class DiagramService : IDiagramService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;

    public DiagramService(IUnitOfWork unitOfWork, IValidationService validationService)
    {
        _unitOfWork = unitOfWork;
        _validationService = validationService;
    }

    public async Task<Result<DiagramDto>> GetAsync(Guid cabinetId, CancellationToken cancellationToken = default)
    {
        var cabinet = await _unitOfWork.Cabinets.GetAsync(
            select: c => new DiagramCabinetDto
            {
                Id = c.Id,
                Name = c.Name,
                CompanyId = c.CompanyId,
                DeviceStatusId = c.DeviceStatusId,
                DeviceStatusName = c.DeviceStatus!.Name,
                LastSeen = c.LastSeen,
                IsActive = c.IsActive,
                ScadaIsEnabled = c.ScadaIsEnabled,
                ScadaLastIngestAt = c.ScadaLastIngestAt
            },
            where: c => c.Id == cabinetId && c.IsActive,
            cancellationToken: cancellationToken);

        if (cabinet == null)
            return Result<DiagramDto>.NotFound(description: "Kabin bulunamadi veya pasif durumda");

        var devices = await _unitOfWork.Devices.GetAllAsync(
            select: d => new DiagramDeviceDto
            {
                Id = d.Id,
                Name = d.Name,
                CoordinateX = d.CoordinateX,
                CoordinateY = d.CoordinateY,
                Rotation = d.Rotation,
                ZIndex = d.ZIndex,
                IsLocked = d.IsLocked,
                IsVisible = d.IsVisible,
                IsActive = d.IsActive,
                ComponentTemplateId = d.ComponentTemplateId,
                ExternalCode = d.ExternalCode,
                DeviceStatusId = d.DeviceStatusId,
                DeviceStatusName = d.DeviceStatus!.Name,
                LastSeen = d.LastSeen,
                // Sablon ozeti cihazla birlikte tasinir: sablon pasife alinsa bile
                // kabin dogru boyut ve renkle render olmali.
                Template = new DiagramTemplateDto
                {
                    Id = d.ComponentTemplate!.Id,
                    Name = d.ComponentTemplate.Name,
                    DeviceTypeId = d.ComponentTemplate.DeviceTypeId,
                    Width = d.ComponentTemplate.Width,
                    Height = d.ComponentTemplate.Height,
                    BackgroundColor = d.ComponentTemplate.BackgroundColor,
                    BackgroundImageUrl = d.ComponentTemplate.BackgroundImageUrl
                },
                // Pin ve IoChannel ISoftDeletableEntity: silinmis satirlari global
                // query filter zaten eliyor, burada tekrar filtrelemeye gerek yok.
                Pins = d.Pins!.Select(p => new DiagramPinDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    RelativeX = p.RelativeX,
                    RelativeY = p.RelativeY,
                    Side = p.Side,
                    Function = p.Function,
                    Direction = p.Direction,
                    VoltageLevel = p.VoltageLevel,
                    ChannelNumber = p.ChannelNumber,
                    ComponentTemplatePinId = p.ComponentTemplatePinId,
                    IoChannelId = p.IoChannelId
                }).ToList(),
                IoChannels = d.IoChannels!.Select(c => new DiagramIoChannelDto
                {
                    Id = c.Id,
                    ChannelNumber = c.ChannelNumber,
                    Direction = c.Direction,
                    IsEnabled = c.IsEnabled,
                    Name = c.Name
                }).ToList()
            },
            where: d => d.CabinetId == cabinetId && d.IsActive,
            orderBy: q => q.OrderBy(d => d.ZIndex).ThenBy(d => d.Name),
            cancellationToken: cancellationToken);

        // Kablolar: WaypointsJson bellek icinde ayristirilir (JSON okuma SQL'e
        // cevrilemez), bu yuzden once ara bir satir sekline projekte edilir.
        var connectionRows = await _unitOfWork.Connections.GetAllAsync(
            select: c => new ConnectionRow(
                c.Id,
                c.CabinetId,
                c.SourcePinId,
                c.TargetPinId,
                c.SourcePin!.DeviceId,
                c.TargetPin!.DeviceId,
                c.Label,
                c.WireType,
                c.Color,
                c.LineStyle,
                c.StrokeWidth,
                c.Routing,
                c.WaypointsJson,
                c.ZIndex),
            // Savunmaci eleme. Iki ayri bozulma yolu var ve ikisi de React Flow'da
            // "var olmayan node'a bagli edge" hatasi uretir:
            //   1) Pin soft-delete edilmis  -> query filter pini gizler ama kablo ayakta kalir,
            //      navigasyon NULL olur ve DeviceId projeksiyonu patlardi.
            //   2) Cihaz pasife alinmis     -> pin durur ama cihaz devices[] listesinde yoktur.
            where: c => c.CabinetId == cabinetId
                     && c.SourcePin != null && c.TargetPin != null
                     && c.SourcePin.Device!.IsActive && c.TargetPin.Device!.IsActive,
            orderBy: q => q.OrderBy(c => c.ZIndex),
            cancellationToken: cancellationToken);

        var connections = (connectionRows ?? [])
            .Select(r => new DiagramConnectionDto
            {
                Id = r.Id,
                CabinetId = r.CabinetId,
                SourcePinId = r.SourcePinId,
                TargetPinId = r.TargetPinId,
                SourceDeviceId = r.SourceDeviceId,
                TargetDeviceId = r.TargetDeviceId,
                Label = r.Label,
                WireType = r.WireType,
                Color = r.Color,
                LineStyle = r.LineStyle,
                StrokeWidth = r.StrokeWidth,
                Routing = r.Routing,
                Waypoints = DiagramWaypoints.Parse(r.WaypointsJson),
                ZIndex = r.ZIndex
            })
            .ToList();

        var annotations = await _unitOfWork.DiagramAnnotations.GetAllAsync(
            select: a => new DiagramAnnotationItemDto
            {
                Id = a.Id,
                Name = a.Name,
                CoordinateX = a.CoordinateX,
                CoordinateY = a.CoordinateY,
                Width = a.Width,
                Height = a.Height,
                Rotation = a.Rotation,
                ZIndex = a.ZIndex,
                IsLocked = a.IsLocked,
                IsVisible = a.IsVisible,
                Text = a.Text,
                Shape = a.Shape,
                BackgroundColor = a.BackgroundColor,
                FontColor = a.FontColor,
                FontSize = a.FontSize,
                IsBold = a.IsBold,
                BorderColor = a.BorderColor
            },
            where: a => a.CabinetId == cabinetId,
            orderBy: q => q.OrderBy(a => a.ZIndex),
            cancellationToken: cancellationToken);

        var canvasSettings = await _unitOfWork.CanvasSettings.GetAsync(
            select: s => new DiagramCanvasSettingsDto
            {
                GridSize = s.GridSize,
                SnapToGrid = s.SnapToGrid,
                BackgroundVariant = s.BackgroundVariant,
                GridColor = s.GridColor,
                BackgroundColor = s.BackgroundColor,
                MinZoom = s.MinZoom,
                MaxZoom = s.MaxZoom
            },
            where: s => s.CabinetId == cabinetId,
            cancellationToken: cancellationToken);

        return Result<DiagramDto>.Success(new DiagramDto
        {
            Cabinet = cabinet,
            Devices = devices ?? [],
            Connections = connections,
            Annotations = annotations ?? [],
            // Kayitli ayar yoksa VARSAYILAN doner ve satir OLUSTURULMAZ: bir kabini
            // yalnizca acmak veritabanina yazmamali.
            CanvasSettings = canvasSettings ?? CreateDefaultCanvasSettings(),
            FetchedAtUtc = DateTime.UtcNow
        });
    }

    public async Task<Result<ICollection<ComponentTemplatePaletteDto>>> GetPaletteAsync(CancellationToken cancellationToken = default)
    {
        var templates = await _unitOfWork.ComponentTemplates.GetAllAsync(
            select: t => new ComponentTemplatePaletteDto
            {
                Id = t.Id,
                Name = t.Name,
                DeviceTypeId = t.DeviceTypeId,
                IsSystemTemplate = t.IsSystemTemplate,
                Width = t.Width,
                Height = t.Height,
                BackgroundColor = t.BackgroundColor,
                BackgroundImageUrl = t.BackgroundImageUrl,
                PinCount = t.ComponentTemplatePins!.Count()
            },
            // Palet bir SECIM kaynagidir: pasife alinmis sablon yeni cihaz uretmemeli.
            where: t => t.IsActive,
            orderBy: q => q.OrderBy(t => t.DeviceTypeId).ThenBy(t => t.Name),
            cancellationToken: cancellationToken);

        return Result<ICollection<ComponentTemplatePaletteDto>>.Success(templates ?? []);
    }

    public async Task<Result<DiagramCanvasSettingsDto>> UpsertCanvasSettingsAsync(
        Guid cabinetId,
        CanvasSettingsUpsertDto request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result<DiagramCanvasSettingsDto>.Validation(validationResult.Failures, description: "Validation failed for CanvasSettingsUpsertDto");

        var cabinetExists = await _unitOfWork.Cabinets.IsExistAsync(
            where: c => c.Id == cabinetId && c.IsActive,
            cancellationToken: cancellationToken);

        if (!cabinetExists)
            return Result<DiagramCanvasSettingsDto>.NotFound(description: "Kabin bulunamadi veya pasif durumda");

        var existing = await _unitOfWork.CanvasSettings.GetAsync(
            where: s => s.CabinetId == cabinetId,
            cancellationToken: cancellationToken);

        if (existing == null)
        {
            var created = new CanvasSettings
            {
                CabinetId = cabinetId,
                GridSize = request.GridSize,
                SnapToGrid = request.SnapToGrid,
                BackgroundVariant = request.BackgroundVariant,
                GridColor = request.GridColor,
                BackgroundColor = request.BackgroundColor,
                MinZoom = request.MinZoom,
                MaxZoom = request.MaxZoom
            };
            await _unitOfWork.CanvasSettings.AddAndSaveAsync(created, cancellationToken);
        }
        else
        {
            existing.GridSize = request.GridSize;
            existing.SnapToGrid = request.SnapToGrid;
            existing.BackgroundVariant = request.BackgroundVariant;
            existing.GridColor = request.GridColor;
            existing.BackgroundColor = request.BackgroundColor;
            existing.MinZoom = request.MinZoom;
            existing.MaxZoom = request.MaxZoom;
            await _unitOfWork.CanvasSettings.UpdateAndSaveAsync(existing, cancellationToken);
        }

        return Result<DiagramCanvasSettingsDto>.Success(new DiagramCanvasSettingsDto
        {
            GridSize = request.GridSize,
            SnapToGrid = request.SnapToGrid,
            BackgroundVariant = request.BackgroundVariant,
            GridColor = request.GridColor,
            BackgroundColor = request.BackgroundColor,
            MinZoom = request.MinZoom,
            MaxZoom = request.MaxZoom
        });
    }

    /// <summary>
    /// Kayitli ayari olmayan kabinin varsayilanlari. Bu degerler sozlesmenin parcasidir
    /// degistirilirse mevcut kabinlerin gorunumu sessizce degisir.
    /// </summary>
    private static DiagramCanvasSettingsDto CreateDefaultCanvasSettings() => new()
    {
        GridSize = 20,
        SnapToGrid = true,
        BackgroundVariant = BackgroundVariant.Dots,
        GridColor = "#E2E8F0",
        BackgroundColor = "#FFFFFF",
        MinZoom = 0.2,
        MaxZoom = 4
    };

    /// <summary>
    /// Kablo satirinin ara sekli. DTO'ya dogrudan projekte edemiyoruz cunku
    /// <c>Waypoints</c> bir JSON string'inden turer ve bu SQL'e cevrilemez.
    /// </summary>
    private sealed record ConnectionRow(
        Guid Id,
        Guid CabinetId,
        Guid SourcePinId,
        Guid TargetPinId,
        Guid SourceDeviceId,
        Guid TargetDeviceId,
        string? Label,
        WireType WireType,
        string Color,
        LineStyle LineStyle,
        double StrokeWidth,
        EdgeRouting Routing,
        string? WaypointsJson,
        int ZIndex);




    /// Palet yazarligi: sablon + pinleri TEK transaction'da olusturur.
    ///
    /// Generic CRUD sablonunun <c>*AndSaveAsync</c> konvansiyonu burada da BILEREK
    /// kirilir (ayni gerekce: <c>DiagramService.Save.cs</c>) — her pin icin ayri bir
    /// commit, yarim yazilmis bir sablon birakma riski demek olurdu.
    #region Template
    public async Task<Result<CreatedDto>> CreateTemplateAsync(
        DiagramTemplateCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result<CreatedDto>.Validation(validationResult.Failures, description: "Validation failed for DiagramTemplateCreateRequest");

        // DeviceTypeId ONCE kontrol edilir. FK'ya birakilsaydi gecersiz bir tip
        // kisit ihlali uretir ve 500 donerdi; oysa bu, istemcinin duzeltebilecegi
        // siradan bir girdi hatasi. Ayni yaklasim SaveAsync'te de var: referans
        // dogrulamalari transaction ACILMADAN once yapilir.
        var deviceTypeExists = await _unitOfWork.DeviceTypes.IsExistAsync(
            where: t => t.Id == request.DeviceTypeId,
            cancellationToken: cancellationToken);

        if (!deviceTypeExists)
        {
            return Result<CreatedDto>.Validation(
                new Dictionary<string, string[]> { ["DeviceTypeId"] = ["Cihaz tipi bulunamadi"] },
                description: "Sablon cihaz tipi gecersiz");
        }

        var template = new ComponentTemplate
        {
            Name = request.Name,
            DeviceTypeId = request.DeviceTypeId,
            Width = request.Width,
            Height = request.Height,
            BackgroundColor = request.BackgroundColor,
            BackgroundImageUrl = request.BackgroundImageUrl,
            // Yeni sablon AKTIF dogar: pasif dogsaydi palette hic gorunmez ve
            // kullanici onu neden goremedigini anlamazdi (B5'te ayni kusur
            // Cabinet ve Device icin duzeltilmisti).
            IsActive = true
        };

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            _unitOfWork.ComponentTemplates.Add(template);
            // Sablon ONCE yazilir: pinlerin FK'si icin gercek bir Id gerekiyor.
            // Iki SaveChanges tek transaction icinde — arada bir hata olursa
            // ikisi de geri alinir.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (request.Pins.Count > 0)
            {
                foreach (var draft in request.Pins)
                {
                    _unitOfWork.ComponentTemplatePins.Add(new ComponentTemplatePin
                    {
                        ComponentTemplateId = template.Id,
                        Name = draft.Name,
                        RelativeX = draft.RelativeX,
                        RelativeY = draft.RelativeY,
                        Side = draft.Side,
                        ChannelNumber = draft.ChannelNumber,
                        Function = draft.Function,
                        Direction = draft.Direction,
                        VoltageLevel = draft.VoltageLevel
                    });
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result<CreatedDto>.Success(new CreatedDto(template.Id));
        }
        catch
        {
            // Yutulmaz, yeniden firlatilir: global ExceptionHandleMiddleware yigini
            // loglayip ProblemDetails uretiyor. Result.Failure'a cevirmek,
            // beklenmedik bir DB hatasinin izini silerdi.
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
    #endregion


    /// Diyagram editorunun toplu kaydetme yolu — kod tabanindaki ILK gercek
    /// cok-varlikli transaction.
    ///
    /// Generic CRUD sablonunun <c>*AndSaveAsync</c> konvansiyonu burada BILEREK
    /// kirilir: o metotlarin her biri kendi <c>SaveChanges</c>'ini cagirir ve tek bir
    /// kaydetme icin sekiz ayri commit uretirdi.
    #region Save
    public async Task<Result<DiagramSaveResponse>> SaveAsync(Guid cabinetId, DiagramSaveRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result<DiagramSaveResponse>.Validation(validationResult.Failures, description: "Validation failed for DiagramSaveRequest");

        var cabinetExists = await _unitOfWork.Cabinets.IsExistAsync(
            where: c => c.Id == cabinetId && c.IsActive,
            cancellationToken: cancellationToken);

        if (!cabinetExists)
            return Result<DiagramSaveResponse>.NotFound(description: "Kabin bulunamadi veya pasif durumda");

        // Bos gonderi: transaction bile acilmaz. Istemcinin debounce'u zaman zaman
        // bos tetiklenebilir ve bunu 400 ile cezalandirmak yalnizca gurultu uretir.
        if (request.IsEmpty)
            return Result<DiagramSaveResponse>.Success(new DiagramSaveResponse { SavedAtUtc = DateTime.UtcNow });

        var context = await LoadSaveContextAsync(cabinetId, request, cancellationToken);

        // Referans dogrulamalari YAZMADAN ONCE yapilir: transaction'i acip sonra
        // geri almak yerine hic acmamak, kilit suresini de log gurultusunu de azaltir.
        var errors = ValidateReferences(request, context);
        if (errors.Count > 0)
            return Result<DiagramSaveResponse>.Validation(errors, description: "Diyagram kaydetme referanslari gecersiz");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // ---- FAZ 1: SILMELER ----
            // Silmeler ve olusturmalar AYRI SaveChanges'lerde akitilir. Sebep filtreli
            // unique index'ler: IX_Connection_SourcePinId_TargetPinId ve IX_Pin_DeviceId_Name
            // "WHERE IsDeleted = 0" ile calisir. Kullanici bir kabloyu silip AYNI iki pin
            // arasina yenisini cizdiginde (cizdi-vazgecti-yeniden cizdi, editorde siradan
            // bir dizi), silme bir UPDATE, olusturma bir INSERT'tur; tek batch'te EF'in
            // sirasi garanti degildir ve INSERT once giderse index ihlali 500 doner.
            ApplyDeletions(request, context);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ---- FAZ 1b: CEKISMELI PIN ADLARINI SAHNELE ----
            // Yalnizca bir pinin birakacagi adi ayni gonderide baskasi aliyorsa
            // calisir (ad takasi / zincir). Ayrintili gerekce: StagePinRenames.
            if (StagePinRenames(request, context))
                await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ---- FAZ 2: OLUSTURMALAR + GUNCELLEMELER ----
            var created = ApplyCreations(cabinetId, request, context);
            ApplyUpdates(request, context);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Id'ler ancak SaveChanges'ten SONRA okunur: yeni satirlarin anahtarlari
            // bu noktada kesinlesmistir ve kaynak ne olursa olsun (istemci uretimi,
            // sunucu uretimi) dogru degeri veririz.
            return Result<DiagramSaveResponse>.Success(new DiagramSaveResponse
            {
                Devices = ToIdMap(created.Devices, d => d.Id),
                Pins = ToIdMap(created.Pins, p => p.Id),
                Connections = ToIdMap(created.Connections, c => c.Id),
                Annotations = ToIdMap(created.Annotations, a => a.Id),
                InstantiatedPinCount = created.InstantiatedPinCount,
                SavedAtUtc = DateTime.UtcNow
            });
        }
        catch
        {
            // Yutulmaz, yeniden firlatilir: global ExceptionHandleMiddleware yigini
            // loglayip ProblemDetails uretiyor. Burada Result.Failure'a cevirmek,
            // beklenmedik bir DB hatasinin izini silerdi.
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Id secici disaridan gecilir: <c>IEntity</c> bos bir isaretci arayuz, <c>Id</c>
    /// tasimiyor. Anahtari yansimayla degil, cagiranin lambda'siyla okuyoruz.
    /// </summary>
    private static List<IdMapEntry> ToIdMap<TEntity>(List<(string TempId, TEntity Entity)> created, Func<TEntity, Guid> idOf)
        => created.Select(c => new IdMapEntry(c.TempId, idOf(c.Entity))).ToList();
    #endregion



    /// Toplu kaydetmenin ic adimlari: DB'den okuma, referans dogrulama ve uygulama.
    /// Orkestrasyon <c>DiagramService.Save.cs</c>'te.
    #region SaveInernals

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
        // ---- Sablonlar (yalnizca yeni cihaz varsa)
        var templateIds = request.Devices.Created.Select(d => d.ComponentTemplateId).Distinct().ToList();
        var activeTemplateIds = new HashSet<Guid>();
        if (templateIds.Count > 0)
        {
            var rows = await _unitOfWork.ComponentTemplates.GetAllAsync(
                select: t => t.Id,
                // Palet gibi burasi da bir SECIM kaynagi: pasif sablon yeni cihaz uretmemeli.
                where: t => templateIds.Contains(t.Id) && t.IsActive,
                cancellationToken: cancellationToken) ?? [];
            activeTemplateIds = rows.ToHashSet();
        }

        // ---- Sablon pinleri (yalnizca InstantiatePins isteyen sablonlar icin)
        var instantiateTemplateIds = request.Devices.Created
            .Where(d => d.InstantiatePins)
            .Select(d => d.ComponentTemplateId)
            .Distinct().ToList();

        var templatePins = new Dictionary<Guid, List<ComponentTemplatePin>>();
        if (instantiateTemplateIds.Count > 0)
        {
            // tracking: false — bunlar yalnizca KOPYALAMA kaynagi; takip edilirlerse
            // degistirilmedikleri halde change tracker'i sisirirler.
            var rows = await _unitOfWork.ComponentTemplatePins.GetAllAsync(
                where: p => instantiateTemplateIds.Contains(p.ComponentTemplateId),
                orderBy: q => q.OrderBy(p => p.Name),
                tracking: false,
                cancellationToken: cancellationToken) ?? [];
            templatePins = rows.GroupBy(p => p.ComponentTemplateId).ToDictionary(g => g.Key, g => g.ToList());
        }

        // ---- Cihazlar: guncellenen, silinen ve pin eklenecek olanlar
        var deviceIds = request.Devices.Updated.Select(d => d.Id)
            .Concat(request.Devices.Deleted)
            .Concat(request.Pins.Created.Where(p => p.DeviceId.HasValue).Select(p => p.DeviceId!.Value))
            .Distinct().ToList();

        var devices = new Dictionary<Guid, Device>();
        if (deviceIds.Count > 0)
        {
            var rows = await _unitOfWork.Devices.GetAllAsync(
                where: d => d.CabinetId == cabinetId && d.IsActive && deviceIds.Contains(d.Id),
                cancellationToken: cancellationToken) ?? [];
            devices = rows.ToDictionary(d => d.Id);
        }

        // ---- Pinler: dogrudan anilanlar + silinen cihazlarin pinleri (cascade icin)
        var deletedDeviceIds = request.Devices.Deleted;
        var referencedPinIds = request.Pins.Updated.Select(p => p.Id)
            .Concat(request.Pins.Deleted)
            .Concat(request.Connections.Created.SelectMany(c => new[] { c.SourcePinId, c.TargetPinId })
                .Where(id => id.HasValue).Select(id => id!.Value))
            .Distinct().ToList();

        var pins = new Dictionary<Guid, Pin>();
        if (referencedPinIds.Count > 0 || deletedDeviceIds.Count > 0)
        {
            var rows = await _unitOfWork.Pins.GetAllAsync(
                where: p => p.Device!.CabinetId == cabinetId
                         && (referencedPinIds.Contains(p.Id) || deletedDeviceIds.Contains(p.DeviceId)),
                cancellationToken: cancellationToken) ?? [];
            pins = rows.ToDictionary(p => p.Id);
        }

        // Bilinmeyen Id'ler buraya girmez; onlari ValidateReferences ayrica raporlar.
        // Boylece uygulama adimi sozluklerde her zaman var olan anahtarlarla calisir.
        var pinsBeingRemoved = new HashSet<Guid>(request.Pins.Deleted.Where(pins.ContainsKey));
        foreach (var pin in pins.Values)
        {
            if (deletedDeviceIds.Contains(pin.DeviceId))
                pinsBeingRemoved.Add(pin.Id);
        }

        // ---- Silinen pinlerin kablolari (cascade)
        var cascadeConnections = new List<Connection>();
        if (pinsBeingRemoved.Count > 0)
        {
            var removedPinIds = pinsBeingRemoved.ToList();
            var rows = await _unitOfWork.Connections.GetAllAsync(
                where: c => c.CabinetId == cabinetId
                         && (removedPinIds.Contains(c.SourcePinId) || removedPinIds.Contains(c.TargetPinId)),
                cancellationToken: cancellationToken) ?? [];
            cascadeConnections = rows.ToList();
        }

        // ---- Guncellenen / silinen kablolar
        var connectionIds = request.Connections.Updated.Select(c => c.Id)
            .Concat(request.Connections.Deleted).Distinct().ToList();

        var connections = new Dictionary<Guid, Connection>();
        if (connectionIds.Count > 0)
        {
            var rows = await _unitOfWork.Connections.GetAllAsync(
                where: c => c.CabinetId == cabinetId && connectionIds.Contains(c.Id),
                cancellationToken: cancellationToken) ?? [];
            connections = rows.ToDictionary(c => c.Id);
        }

        // ---- Guncellenen / silinen notlar
        var annotationIds = request.Annotations.Updated.Select(a => a.Id)
            .Concat(request.Annotations.Deleted).Distinct().ToList();

        var annotations = new Dictionary<Guid, DiagramAnnotation>();
        if (annotationIds.Count > 0)
        {
            var rows = await _unitOfWork.DiagramAnnotations.GetAllAsync(
                where: a => a.CabinetId == cabinetId && annotationIds.Contains(a.Id),
                cancellationToken: cancellationToken) ?? [];
            annotations = rows.ToDictionary(a => a.Id);
        }

        // ---- Halihazirda duran pin ciftleri (yeni kablo cakismasi icin)
        var endpointIds = request.Connections.Created
            .SelectMany(c => new[] { c.SourcePinId, c.TargetPinId })
            .Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();

        var existingPairs = new HashSet<(string, string)>();
        if (endpointIds.Count > 0)
        {
            var rows = await _unitOfWork.Connections.GetAllAsync(
                select: c => new PinPairRow(c.Id, c.SourcePinId, c.TargetPinId),
                where: c => c.CabinetId == cabinetId
                         && (endpointIds.Contains(c.SourcePinId) || endpointIds.Contains(c.TargetPinId)),
                cancellationToken: cancellationToken) ?? [];

            // Bu gonderide kalkacak kablolar cakisma sayilmaz: kullanicinin bir kabloyu
            // silip ayni iki pin arasina yenisini cizmesi mesru bir islemdir.
            var removedConnectionIds = new HashSet<Guid>(request.Connections.Deleted);
            foreach (var connection in cascadeConnections) removedConnectionIds.Add(connection.Id);

            foreach (var row in rows)
            {
                if (removedConnectionIds.Contains(row.Id)) continue;
                existingPairs.Add(PairKey(EndpointKey(row.SourcePinId, null), EndpointKey(row.TargetPinId, null)));
            }
        }

        // ---- Etkilenen cihazlarin CANLI pin adlari
        // IX_Pin_DeviceId_Name (unique, WHERE IsDeleted = 0) ihlali 500 doner; bir pini
        // var olan bir adla yeniden adlandirmak editorde tek tusluk bir islem oldugu
        // icin bunu 400'e cevirmek zorundayiz.
        var nameCheckDeviceIds = request.Pins.Created
            .Where(p => p.DeviceId.HasValue).Select(p => p.DeviceId!.Value)
            .Concat(request.Pins.Updated
                .Select(u => pins.TryGetValue(u.Id, out var p) ? p.DeviceId : Guid.Empty)
                .Where(id => id != Guid.Empty))
            .Distinct().ToList();

        var livePinNames = new Dictionary<Guid, HashSet<string>>();
        if (nameCheckDeviceIds.Count > 0)
        {
            var rows = await _unitOfWork.Pins.GetAllAsync(
                select: p => new PinNameRow(p.Id, p.DeviceId, p.Name),
                where: p => nameCheckDeviceIds.Contains(p.DeviceId),
                cancellationToken: cancellationToken) ?? [];

            foreach (var row in rows)
            {
                // Kalkan pinin adi serbest kalir.
                if (pinsBeingRemoved.Contains(row.Id)) continue;
                if (!livePinNames.TryGetValue(row.DeviceId, out var names))
                {
                    // OrdinalIgnoreCase: SQL Server'in varsayilan collation'i buyuk/kucuk
                    // harf duyarsizdir, yani "in1" ile "IN1" index'te CAKISIR.
                    names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    livePinNames[row.DeviceId] = names;
                }
                names.Add(row.Name);
            }
        }

        // ---- Kabindeki aktif cihazlarin dis kodlari
        // IX_Device_CabinetId_ExternalCode (unique, WHERE ExternalCode IS NOT NULL AND IsActive = 1).
        var needsCodeCheck = request.Devices.Created.Any(d => !string.IsNullOrWhiteSpace(d.ExternalCode))
                          || request.Devices.Updated.Any(d => !string.IsNullOrWhiteSpace(d.ExternalCode));

        var deviceExternalCodes = new Dictionary<Guid, string>();
        if (needsCodeCheck)
        {
            var rows = await _unitOfWork.Devices.GetAllAsync(
                select: d => new DeviceCodeRow(d.Id, d.ExternalCode!),
                where: d => d.CabinetId == cabinetId && d.IsActive && d.ExternalCode != null,
                cancellationToken: cancellationToken) ?? [];
            deviceExternalCodes = rows.ToDictionary(r => r.Id, r => r.ExternalCode);
        }

        // ---- Pine baglanacak IO kanallari
        // Iki ayri sorun icin: (1) olmayan bir Id FK ihlaliyle 500 dondururdu,
        // (2) BASKA bir kabinin kanali sessizce baglanabilirdi ve o kabinin telemetrisi
        // bu diyagramda gorunurdu. Sorgu kanali cihazi uzerinden kabine baglar.
        var ioChannelIds = request.Pins.Created.Where(p => p.IoChannelId.HasValue).Select(p => p.IoChannelId!.Value)
            .Concat(request.Pins.Updated.Where(p => p.IoChannelId.HasValue).Select(p => p.IoChannelId!.Value))
            .Distinct().ToList();

        var validIoChannelIds = new HashSet<Guid>();
        if (ioChannelIds.Count > 0)
        {
            var rows = await _unitOfWork.IoChannels.GetAllAsync(
                select: c => c.Id,
                where: c => ioChannelIds.Contains(c.Id) && c.Device!.CabinetId == cabinetId,
                cancellationToken: cancellationToken) ?? [];
            validIoChannelIds = rows.ToHashSet();
        }

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
            LivePinNames = livePinNames,
            DeviceExternalCodes = deviceExternalCodes,
            ValidIoChannelIds = validIoChannelIds
        };
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
        var errors = new DraftErrorBag();

        var deletedDeviceIds = new HashSet<Guid>(request.Devices.Deleted);
        var createdDeviceTempIds = request.Devices.Created.Select(d => d.TempId).ToHashSet(StringComparer.Ordinal);
        var createdPinsByTempId = request.Pins.Created
            .GroupBy(p => p.TempId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        var cascadeConnectionIds = context.CascadeConnections.Select(c => c.Id).ToHashSet();

        // ---- Cihazlar
        for (int i = 0; i < request.Devices.Created.Count; i++)
        {
            if (!context.ActiveTemplateIds.Contains(request.Devices.Created[i].ComponentTemplateId))
                errors.Add($"Devices.Created[{i}].ComponentTemplateId", "Sablon bulunamadi veya pasif durumda");
        }
        for (int i = 0; i < request.Devices.Updated.Count; i++)
        {
            if (!context.Devices.ContainsKey(request.Devices.Updated[i].Id))
                errors.Add($"Devices.Updated[{i}].Id", "Cihaz bu kabinde bulunamadi");
        }
        for (int i = 0; i < request.Devices.Deleted.Count; i++)
        {
            if (!context.Devices.ContainsKey(request.Devices.Deleted[i]))
                errors.Add($"Devices.Deleted[{i}]", "Cihaz bu kabinde bulunamadi");
        }

        // ---- Pinler
        for (int i = 0; i < request.Pins.Created.Count; i++)
        {
            var draft = request.Pins.Created[i];
            if (draft.DeviceId.HasValue)
            {
                if (!context.Devices.ContainsKey(draft.DeviceId.Value))
                    errors.Add($"Pins.Created[{i}].DeviceId", "Cihaz bu kabinde bulunamadi");
                else if (deletedDeviceIds.Contains(draft.DeviceId.Value))
                    errors.Add($"Pins.Created[{i}].DeviceId", "Ayni gonderide silinen bir cihaza pin eklenemez");
            }
            else if (!createdDeviceTempIds.Contains(draft.DeviceTempId!))
            {
                errors.Add($"Pins.Created[{i}].DeviceTempId", "Gecici cihaz kimligi bu gonderide bulunamadi");
            }
        }
        for (int i = 0; i < request.Pins.Updated.Count; i++)
        {
            var id = request.Pins.Updated[i].Id;
            if (!context.Pins.ContainsKey(id))
                errors.Add($"Pins.Updated[{i}].Id", "Pin bu kabinde bulunamadi");
            else if (context.PinsBeingRemoved.Contains(id))
                errors.Add($"Pins.Updated[{i}].Id", "Ayni gonderide silinen bir pin guncellenemez");
        }
        for (int i = 0; i < request.Pins.Deleted.Count; i++)
        {
            if (!context.Pins.ContainsKey(request.Pins.Deleted[i]))
                errors.Add($"Pins.Deleted[{i}]", "Pin bu kabinde bulunamadi");
        }

        for (int i = 0; i < request.Pins.Created.Count; i++)
        {
            var id = request.Pins.Created[i].IoChannelId;
            if (id.HasValue && !context.ValidIoChannelIds.Contains(id.Value))
                errors.Add($"Pins.Created[{i}].IoChannelId", "IO kanali bu kabinde bulunamadi");
        }
        for (int i = 0; i < request.Pins.Updated.Count; i++)
        {
            var id = request.Pins.Updated[i].IoChannelId;
            if (id.HasValue && !context.ValidIoChannelIds.Contains(id.Value))
                errors.Add($"Pins.Updated[{i}].IoChannelId", "IO kanali bu kabinde bulunamadi");
        }

        ValidatePinNames(request, context, errors);
        ValidateDeviceExternalCodes(request, context, deletedDeviceIds, errors);

        // ---- Kablolar
        var pairsInRequest = new HashSet<(string, string)>();
        for (int i = 0; i < request.Connections.Created.Count; i++)
        {
            var draft = request.Connections.Created[i];
            var source = ResolveEndpoint(draft.SourcePinId, draft.SourcePinTempId, context, createdPinsByTempId,
                errors, $"Connections.Created[{i}].SourcePin");
            var target = ResolveEndpoint(draft.TargetPinId, draft.TargetPinTempId, context, createdPinsByTempId,
                errors, $"Connections.Created[{i}].TargetPin");

            if (source == null || target == null) continue;

            if (source.Key == target.Key)
            {
                errors.Add($"Connections.Created[{i}].TargetPin", "Bir pin kendisine baglanamaz");
                continue;
            }

            var pair = PairKey(source.Key, target.Key);
            // Cift, YONSUZ karsilastirilir. DB'deki unique index (SourcePinId, TargetPinId)
            // sirali oldugu icin ters cizilmis ayni kabloyu YAKALAMAZ; ConnectionMode.Loose
            // ile "kaynak"/"hedef" zaten keyfi oldugundan burada daha katiyiz.
            if (!pairsInRequest.Add(pair) || context.ExistingPairs.Contains(pair))
            {
                errors.Add($"Connections.Created[{i}]", "Bu iki pin arasinda zaten bir kablo var");
                continue;
            }

            // Gerilim uyusmazligi: iki taraf da BELIRTILMISSE ve farkliysa reddedilir.
            // Biri null ise ("belirtilmemis") susulur — bilinmeyeni hata saymak,
            // gerilimi henuz girilmemis sablonlarla calismayi imkansiz kilardi.
            if (source.VoltageLevel.HasValue && target.VoltageLevel.HasValue
                && source.VoltageLevel.Value != target.VoltageLevel.Value)
            {
                errors.Add($"Connections.Created[{i}]", "Farkli gerilim seviyesindeki pinler baglanamaz");
            }
        }
        for (int i = 0; i < request.Connections.Updated.Count; i++)
        {
            var id = request.Connections.Updated[i].Id;
            if (!context.Connections.ContainsKey(id))
                errors.Add($"Connections.Updated[{i}].Id", "Kablo bu kabinde bulunamadi");
            else if (cascadeConnectionIds.Contains(id))
                errors.Add($"Connections.Updated[{i}].Id", "Bu kablo, pini silindigi icin kaldiriliyor; ayni gonderide guncellenemez");
        }
        for (int i = 0; i < request.Connections.Deleted.Count; i++)
        {
            if (!context.Connections.ContainsKey(request.Connections.Deleted[i]))
                errors.Add($"Connections.Deleted[{i}]", "Kablo bu kabinde bulunamadi");
        }

        // ---- Notlar
        for (int i = 0; i < request.Annotations.Updated.Count; i++)
        {
            if (!context.Annotations.ContainsKey(request.Annotations.Updated[i].Id))
                errors.Add($"Annotations.Updated[{i}].Id", "Not bu kabinde bulunamadi");
        }
        for (int i = 0; i < request.Annotations.Deleted.Count; i++)
        {
            if (!context.Annotations.ContainsKey(request.Annotations.Deleted[i]))
                errors.Add($"Annotations.Deleted[{i}]", "Not bu kabinde bulunamadi");
        }

        return errors.ToDictionary();
    }

    /// <summary>
    /// Pin adlarinin cihaz icinde benzersizligi — <c>IX_Pin_DeviceId_Name</c>'in
    /// bellekteki karsiligi. Karsilastirma buyuk/kucuk harf DUYARSIZ, cunku index
    /// SQL Server'in varsayilan collation'i altinda oyle davranir.
    /// </summary>
    private static void ValidatePinNames(DiagramSaveRequest request, SaveContext context, DraftErrorBag errors)
    {
        var namesByDevice = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        HashSet<string> NamesFor(string deviceKey)
        {
            if (!namesByDevice.TryGetValue(deviceKey, out var names))
            {
                names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                namesByDevice[deviceKey] = names;
            }
            return names;
        }

        foreach (var (deviceId, names) in context.LivePinNames)
            NamesFor(EndpointKey(deviceId, null)).UnionWith(names);

        // Yeni cihazin sablondan uretilecek pinleri de bu kumeye girer; sablon
        // pinlerinin kendi aralarindaki benzersizligini IX_ComponentTemplatePin zaten
        // garanti eder, ama ayni gonderide o cihaza ELLE eklenen bir pin cakisabilir.
        foreach (var draft in request.Devices.Created)
        {
            if (!draft.InstantiatePins) continue;
            if (context.TemplatePins.TryGetValue(draft.ComponentTemplateId, out var templatePins))
                NamesFor(EndpointKey(null, draft.TempId)).UnionWith(templatePins.Select(t => t.Name));
        }

        // Yeniden adlandirmalar IKI ADIMDA islenir: once TUM eski adlar dusurulur,
        // sonra yenileri eklenir. Tek adimda yapilsaydi iki pinin adini takas etmek
        // yanlislikla cakisma sayilirdi.
        foreach (var draft in request.Pins.Updated)
        {
            if (context.Pins.TryGetValue(draft.Id, out var pin))
                NamesFor(EndpointKey(pin.DeviceId, null)).Remove(pin.Name);
        }
        for (int i = 0; i < request.Pins.Updated.Count; i++)
        {
            var draft = request.Pins.Updated[i];
            if (!context.Pins.TryGetValue(draft.Id, out var pin)) continue;
            if (!NamesFor(EndpointKey(pin.DeviceId, null)).Add(draft.Name))
                errors.Add($"Pins.Updated[{i}].Name", "Bu cihazda ayni adli baska bir pin var");
        }

        for (int i = 0; i < request.Pins.Created.Count; i++)
        {
            var draft = request.Pins.Created[i];
            var deviceKey = draft.DeviceId.HasValue
                ? EndpointKey(draft.DeviceId.Value, null)
                : EndpointKey(null, draft.DeviceTempId);

            if (!NamesFor(deviceKey).Add(draft.Name))
                errors.Add($"Pins.Created[{i}].Name", "Bu cihazda ayni adli baska bir pin var");
        }
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
        DraftErrorBag errors)
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
                errors.Add($"Devices.Updated[{i}].ExternalCode", "Bu kabinde ayni dis koda sahip baska bir cihaz var");
        }
        for (int i = 0; i < request.Devices.Created.Count; i++)
        {
            var code = request.Devices.Created[i].ExternalCode;
            if (string.IsNullOrWhiteSpace(code)) continue;
            if (!codes.Add(code))
                errors.Add($"Devices.Created[{i}].ExternalCode", "Bu kabinde ayni dis koda sahip baska bir cihaz var");
        }
    }

    /// <summary>
    /// Bir kablo ucunu cozer: ya kabindeki kalici bir pin ya da ayni gonderide
    /// olusturulan bir pin. Cozulemezse hatayi yazar ve null doner.
    /// </summary>
    private static ResolvedEndpoint? ResolveEndpoint(
        Guid? pinId,
        string? pinTempId,
        SaveContext context,
        Dictionary<string, PinCreateDraft> createdPinsByTempId,
        DraftErrorBag errors,
        string errorKey)
    {
        if (pinId.HasValue)
        {
            if (!context.Pins.TryGetValue(pinId.Value, out var pin))
            {
                errors.Add($"{errorKey}Id", "Pin bu kabinde bulunamadi");
                return null;
            }
            if (context.PinsBeingRemoved.Contains(pinId.Value))
            {
                errors.Add($"{errorKey}Id", "Ayni gonderide silinen bir pine kablo cizilemez");
                return null;
            }
            return new ResolvedEndpoint(EndpointKey(pinId.Value, null), pin.VoltageLevel);
        }

        if (!createdPinsByTempId.TryGetValue(pinTempId!, out var draft))
        {
            errors.Add($"{errorKey}TempId", "Gecici pin kimligi bu gonderide bulunamadi");
            return null;
        }
        return new ResolvedEndpoint(EndpointKey(null, pinTempId), draft.VoltageLevel);
    }

    /// <summary>
    /// Kalici Id ve gecici kimligi TEK bir string uzayinda birlestirir; boylece
    /// "kalici pin ile yeni pin" gibi karisik ciftler de tek bir kumede
    /// karsilastirilabilir.
    /// </summary>
    private static string EndpointKey(Guid? id, string? tempId)
        => id.HasValue ? $"id:{id.Value}" : $"tmp:{tempId}";

    /// <summary>Ciftin YONSUZ anahtari: (a,b) ile (b,a) ayni kabloyu gosterir.</summary>
    private static (string, string) PairKey(string first, string second)
        => string.CompareOrdinal(first, second) <= 0 ? (first, second) : (second, first);

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
    /// Pin yeniden adlandirmalarini, gerekiyorsa once GECICI bir ada tasir.
    /// Geriye "ayri bir SaveChanges gerekiyor mu" bilgisini doner.
    ///
    /// <b>Neden gerekli.</b> <c>IX_Pin_DeviceId_Name</c> unique index'i STATEMENT
    /// bazinda dogrulanir. Iki pinin adini TAKAS etmek — ya da A→B, B→C zinciri
    /// kurmak — tek batch'te imkansizdir: EF iki ayri UPDATE gonderir ve aradaki
    /// anlik durumda ayni ad iki satirda bulunur. Ustelik hangi UPDATE'in once
    /// gidecegi garanti degildir, yani hata KARARSIZ olur: ayni islem bazen calisir,
    /// bazen 500 doner ve kullanicinin tum yigini geri alinir.
    ///
    /// Bellekteki dogrulama (bkz. <see cref="ValidatePinNames"/>) takasa dogru
    /// olarak izin verir; burasi o iznin DB'de de gecerli olmasini saglar.
    /// </summary>
    private static bool StagePinRenames(DiagramSaveRequest request, SaveContext context)
    {
        static string Key(Guid deviceId, string name) => $"{deviceId}|{name.ToUpperInvariant()}";

        var freedNames = new HashSet<string>(StringComparer.Ordinal);
        var renamedPins = new List<Pin>();

        foreach (var draft in request.Pins.Updated)
        {
            if (!context.Pins.TryGetValue(draft.Id, out var pin)) continue;
            if (string.Equals(pin.Name, draft.Name, StringComparison.OrdinalIgnoreCase)) continue;
            freedNames.Add(Key(pin.DeviceId, pin.Name));
            renamedPins.Add(pin);
        }

        if (renamedPins.Count == 0) return false;

        // Serbest kalacak bir adi AYNI gonderide baskasi aliyor mu? Almiyorsa
        // sahneleme gereksizdir ve fazladan bir gidis-donus etmeyiz.
        var contested = request.Pins.Updated.Any(d =>
                context.Pins.TryGetValue(d.Id, out var pin) && freedNames.Contains(Key(pin.DeviceId, d.Name)))
            || request.Pins.Created.Any(d =>
                d.DeviceId.HasValue && freedNames.Contains(Key(d.DeviceId.Value, d.Name)));

        if (!contested) return false;

        // Guid tabanli gecici ad: kendisi de cakisamaz. Bu deger asla commit
        // edilmez — nihai adlar ayni transaction icinde hemen ustune yazilir.
        foreach (var pin in renamedPins)
            pin.Name = $"~stg~{Guid.NewGuid():N}";

        return true;
    }

    /// <summary>
    /// Olusturmalar.
    ///
    /// Yeni satirlar birbirini FK SKALERIYLE DEGIL NAVIGASYONLA gosterir
    /// (<c>pin.Device = device</c>): Id'ler <c>SaveChanges</c>'e kadar kesinlesmedigi
    /// icin skaler atamak imkansiz olurdu. EF, ekleme sirasini ve FK degerlerini
    /// bu iliskilerden kendisi cozer.
    /// </summary>
    private CreatedEntities ApplyCreations(Guid cabinetId, DiagramSaveRequest request, SaveContext context)
    {
        var created = new CreatedEntities();
        var deviceByTempId = new Dictionary<string, Device>(StringComparer.Ordinal);
        var pinByTempId = new Dictionary<string, Pin>(StringComparer.Ordinal);

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
            deviceByTempId[draft.TempId] = device;
            created.Devices.Add((draft.TempId, device));

            if (!draft.InstantiatePins) continue;
            if (!context.TemplatePins.TryGetValue(draft.ComponentTemplateId, out var templatePins)) continue;

            // Ayni cihazda ayni kanal numarasi TEK bir IoChannel'dir. Sablonda iki
            // pin ayni kanali gosteriyorsa (or. bir girisin besleme ve donus ucu)
            // ikisi de ayni kanala baglanir; ayri ayri uretmek
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

                // Kanal numarasi tasiyan her sablon pini icin bir IoChannel de dogar.
                //
                // Bu olmadan SCADA ingest'inin yazacagi HICBIR SATIR olmazdi: ingest
                // kanali (DeviceId, ChannelNumber) ile cozuyor ve tanimadigi kanali
                // sessizce atliyor (K7). Kanallari ureten baska bir yol da yok —
                // urunde cihaz yaratmanin tek yolu paletten birakmak, yonetim CRUD
                // ekranlari ise bu turun kapsami disinda.
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

                    // Skaler FK degil NAVIGASYON: Id'ler SaveChanges'e kadar yok.
                    pin.IoChannel = channel;
                }

                _unitOfWork.Pins.Add(pin);
                created.InstantiatedPinCount++;
            }
        }

        foreach (var draft in request.Pins.Created)
        {
            var pin = new Pin
            {
                Name = draft.Name,
                RelativeX = draft.RelativeX,
                RelativeY = draft.RelativeY,
                Side = draft.Side,
                Function = draft.Function,
                Direction = draft.Direction,
                VoltageLevel = draft.VoltageLevel,
                ChannelNumber = draft.ChannelNumber,
                IoChannelId = draft.IoChannelId
            };

            if (draft.DeviceId.HasValue) pin.DeviceId = draft.DeviceId.Value;
            else pin.Device = deviceByTempId[draft.DeviceTempId!];

            _unitOfWork.Pins.Add(pin);
            pinByTempId[draft.TempId] = pin;
            created.Pins.Add((draft.TempId, pin));
        }

        foreach (var draft in request.Connections.Created)
        {
            var connection = new Connection
            {
                CabinetId = cabinetId,
                Label = draft.Label,
                WireType = draft.WireType,
                Color = draft.Color,
                LineStyle = draft.LineStyle,
                StrokeWidth = draft.StrokeWidth,
                Routing = draft.Routing,
                WaypointsJson = DiagramWaypoints.Serialize(draft.Waypoints),
                ZIndex = draft.ZIndex
            };

            if (draft.SourcePinId.HasValue) connection.SourcePinId = draft.SourcePinId.Value;
            else connection.SourcePin = pinByTempId[draft.SourcePinTempId!];

            if (draft.TargetPinId.HasValue) connection.TargetPinId = draft.TargetPinId.Value;
            else connection.TargetPin = pinByTempId[draft.TargetPinTempId!];

            _unitOfWork.Connections.Add(connection);
            created.Connections.Add((draft.TempId, connection));
        }

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

        return created;
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

        foreach (var draft in request.Pins.Updated)
        {
            var pin = context.Pins[draft.Id];
            pin.Name = draft.Name;
            pin.RelativeX = draft.RelativeX;
            pin.RelativeY = draft.RelativeY;
            pin.Side = draft.Side;
            pin.Function = draft.Function;
            pin.Direction = draft.Direction;
            pin.VoltageLevel = draft.VoltageLevel;
            pin.ChannelNumber = draft.ChannelNumber;
            pin.IoChannelId = draft.IoChannelId;
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
        /// <summary>Bu gonderide kalkacak pinler: dogrudan silinenler + cihaziyla gidenler.</summary>
        public required HashSet<Guid> PinsBeingRemoved { get; init; }
        /// <summary>Pini kalktigi icin birlikte silinecek kablolar.</summary>
        public required List<Connection> CascadeConnections { get; init; }
        /// <summary>Kaydetmeden sonra da ayakta kalacak pin ciftleri (yonsuz anahtar).</summary>
        public required HashSet<(string, string)> ExistingPairs { get; init; }
        /// <summary>Adi kontrol edilecek cihazlarin canli pin adlari (kalkanlar haric).</summary>
        public required Dictionary<Guid, HashSet<string>> LivePinNames { get; init; }
        /// <summary>Kabindeki aktif cihazlarin dis kodlari (yalnizca gerektiginde okunur).</summary>
        public required Dictionary<Guid, string> DeviceExternalCodes { get; init; }
        /// <summary>Bu kabine ait olan ve pine baglanabilecek IO kanallari.</summary>
        public required HashSet<Guid> ValidIoChannelIds { get; init; }
    }

    private sealed class CreatedEntities
    {
        public List<(string TempId, Device Entity)> Devices { get; } = [];
        public List<(string TempId, Pin Entity)> Pins { get; } = [];
        public List<(string TempId, Connection Entity)> Connections { get; } = [];
        public List<(string TempId, DiagramAnnotation Entity)> Annotations { get; } = [];
        public int InstantiatedPinCount { get; set; }
    }

    private sealed record ResolvedEndpoint(string Key, VoltageLevel? VoltageLevel);

    private sealed record PinPairRow(Guid Id, Guid SourcePinId, Guid TargetPinId);

    private sealed record PinNameRow(Guid Id, Guid DeviceId, string Name);

    private sealed record DeviceCodeRow(Guid Id, string ExternalCode);

    /// <summary>
    /// Ayni alanda birden fazla hata birikebilsin diye kucuk bir toplayici.
    /// <c>ProblemDetails.errors</c> sozlugunun sekli: alan -> mesaj dizisi.
    /// </summary>
    private sealed class DraftErrorBag
    {
        private readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);

        public void Add(string key, string message)
        {
            if (!_errors.TryGetValue(key, out var messages))
            {
                messages = [];
                _errors[key] = messages;
            }
            messages.Add(message);
        }

        public Dictionary<string, string[]> ToDictionary()
            => _errors.ToDictionary(e => e.Key, e => e.Value.ToArray(), StringComparer.Ordinal);
    }
    #endregion
}
