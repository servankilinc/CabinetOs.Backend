using CabinetOs.Business.Abstract;
using CabinetOs.Business.Utils;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Core.Utils.Validation;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Dtos.Diagram.Commands;
using CabinetOs.Model.Dtos.Diagram.Queries;
using CabinetOs.Model.Dtos.Diagram.Queries.Items;
using CabinetOs.Model.Entities;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Business.Concrete;

public partial class DiagramService : IDiagramService
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
}
