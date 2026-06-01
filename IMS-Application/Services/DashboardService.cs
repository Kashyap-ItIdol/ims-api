﻿﻿using System.Linq;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IMS_Application.Services
{
        public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardStatsDto> GetStatsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null)
        {

            Console.WriteLine($"[Dashboard] GetStatsAsync called (startDate={startDate}, endDate={endDate})");
            var now = DateTime.UtcNow;

            var rangeEnd = endDate ?? now;

            var rangeStart = startDate ??
                new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            rangeStart = rangeStart.Date;
            rangeEnd = rangeEnd.Date.AddDays(1).AddTicks(-1);

            var prevEnd = rangeStart.AddTicks(-1);
            var prevStart = rangeStart - (rangeEnd - rangeStart);

            var assets = await _unitOfWork.Assets.GetAllAsync();
            var assetsForDashboard = await _unitOfWork.Assets.GetAllForDashboardAsync();
            var tickets = await _unitOfWork.Tickets.GetAllAsync();

            Console.WriteLine($"[Dashboard] GetStatsAsync assets from GetAllAsync count={assets.Count}");
            foreach (var a in assets)
            {
                Console.WriteLine($"[Dashboard] GetStatsAsync Asset Id={a.Id} SerialNo={a.SerialNo} IsActive={a.IsActive} IsDeleted={a.IsDeleted} StatusId={a.StatusId} AssetStatus={(a.AssetStatus?.Status ?? "null")}");
            }

            int totalCurrent = assets.Count;
            int availableCurrent = assetsForDashboard.Count(a =>
                string.Equals((a.AssetStatus?.Status ?? string.Empty).Trim(), "Available", StringComparison.OrdinalIgnoreCase));


            Console.WriteLine($"[Dashboard] GetStatsAsync ticket totals: totalTickets={tickets.Count()}");
            Console.WriteLine($"[Dashboard] GetStatsAsync ticket date window (current): rangeStart={rangeStart:o}, rangeEnd={rangeEnd:o}");
            var openAll = tickets.Count(t => t.Status == Status.Open);
            var inProgressAll = tickets.Count(t => t.Status == Status.InProgress);
            var solvedAll = tickets.Count(t => t.Status == Status.Solved);
            Console.WriteLine($"[Dashboard] GetStatsAsync ticket status totals (all time): open={openAll}, inProgress={inProgressAll}, solved={solvedAll}");

            Console.WriteLine($"[Dashboard] GetStatsAsync tickets within current date window: {tickets.Count(t => t.CreatedAt >= rangeStart && t.CreatedAt <= rangeEnd)}");


            int openCurrent = tickets.Count(t =>
                t.CreatedAt >= rangeStart &&
                t.CreatedAt <= rangeEnd &&
                t.Status == Status.Open);


            int inProgressCurrent = tickets.Count(t =>
                t.CreatedAt >= rangeStart &&
                t.CreatedAt <= rangeEnd &&
                t.Status == Status.InProgress);

            int resolvedCurrent = tickets.Count(t =>
                t.CreatedAt >= rangeStart &&
                t.CreatedAt <= rangeEnd &&
                t.Status == Status.Solved);

            Console.WriteLine($"[Dashboard] GetStatsAsync ticket KPIs (current): open={openCurrent}, inProgress={inProgressCurrent}, resolved={resolvedCurrent}");

            Console.WriteLine($"[Dashboard] GetStatsAsync ticket date window (previous): prevStart={prevStart:o}, prevEnd={prevEnd:o}");

            int totalPrevious = assets.Count(a =>
                a.CreatedAt >= prevStart &&
                a.CreatedAt <= prevEnd);

            int availablePrevious = assetsForDashboard.Count(a =>
                string.Equals((a.AssetStatus?.Status ?? string.Empty).Trim(), "Available", StringComparison.OrdinalIgnoreCase));

            int openPrevious = tickets.Count(t =>
                t.CreatedAt >= prevStart &&
                t.CreatedAt <= prevEnd &&
                t.Status == Status.Open);

            int inProgressPrevious = tickets.Count(t =>
                t.CreatedAt >= prevStart &&
                t.CreatedAt <= prevEnd &&
                t.Status == Status.InProgress);

            int resolvedPrevious = tickets.Count(t =>
                t.CreatedAt >= prevStart &&
                t.CreatedAt <= prevEnd &&
                t.Status == Status.Solved);

            Console.WriteLine($"[Dashboard] GetStatsAsync ticket KPIs (previous): open={openPrevious}, inProgress={inProgressPrevious}, resolved={resolvedPrevious}");


            (double percentageAbs, string trend, double changePercentageSigned, string comparisonText) Calc(int current, int previous)
            {
                if (previous == 0)
                {
                    if (current == 0)
                    {
                        return (0d, "down", 0d, "No change");
                    }

                    var signed = 100d;
                    var localTrend = signed >= 0 ? "up" : "down";
                    var localAbsPct = Math.Abs(signed);
                    var txt = $"{localTrend.ToUpperInvariant()}: {localAbsPct:0.#}%";
                    return (localAbsPct, localTrend, signed, txt);
                }

                var signedPct = ((current - previous) / (double)previous) * 100d;
                signedPct = Math.Round(signedPct, 1, MidpointRounding.AwayFromZero);

                var trend = signedPct >= 0 ? "up" : "down";
                var absPct = Math.Abs(signedPct);

                var comparisonText = $"{trend.ToUpperInvariant()}: {absPct:0.#}%";

                return (absPct, trend, signedPct, comparisonText);
            }

            var (totalPctAbs, totalTrend, totalChangeSigned, totalComparisonText) =
                Calc(totalCurrent, totalPrevious);

            var (availablePctAbs, availableTrend, availableChangeSigned, availableComparisonText) =
                Calc(availableCurrent, availablePrevious);

            var (openPctAbs, openTrend, openChangeSigned, openComparisonText) =
                Calc(openCurrent, openPrevious);

            var (inProgressPctAbs, inProgressTrend, inProgressChangeSigned, inProgressComparisonText) =
                Calc(inProgressCurrent, inProgressPrevious);

            var (resolvedPctAbs, resolvedTrend, resolvedChangeSigned, resolvedComparisonText) =
                Calc(resolvedCurrent, resolvedPrevious);

            return new DashboardStatsDto
            {
                TotalAssets = totalCurrent,
                AvailableAssets = availableCurrent,
                OpenTickets = openCurrent,
                InProgressTickets = inProgressCurrent,
                ResolvedTickets = resolvedCurrent,

                TotalAssetsPercentage = totalPctAbs,
                TotalAssetsTrend = totalTrend,
                AvailableAssetsPercentage = availablePctAbs,
                AvailableAssetsTrend = availableTrend,
                OpenTicketsPercentage = openPctAbs,
                OpenTicketsTrend = openTrend,
                InProgressTicketsPercentage = inProgressPctAbs,
                InProgressTicketsTrend = inProgressTrend,
                ResolvedTicketsPercentage = resolvedPctAbs,
                ResolvedTicketsTrend = resolvedTrend,

                TotalAssetsChangePercentage = totalChangeSigned,
                TotalAssetsComparisonText = totalComparisonText,

                AvailableAssetsChangePercentage = availableChangeSigned,
                AvailableAssetsComparisonText = availableComparisonText,

                OpenTicketsChangePercentage = openChangeSigned,
                OpenTicketsComparisonText = openComparisonText,

                InProgressTicketsChangePercentage = inProgressChangeSigned,
                InProgressTicketsComparisonText = inProgressComparisonText,

                ClosedTicketsChangePercentage = resolvedChangeSigned,
                ClosedTicketsComparisonText = resolvedComparisonText
            };
        }

        public async Task<List<TicketChartDto>> GetTicketChartAsync()
        {
            var tickets = await _unitOfWork.Tickets.GetAllAsync();

            var chartData = tickets
                .GroupBy(t => t.CreatedAt.Date)
                .Select(g => new TicketChartDto
                {
                    Day = g.Key.ToString("ddd"),
                    Opened = g.Count(t =>
                        t.Status == Status.Open ||
                        t.Status == Status.InProgress),

                    Solved = g.Count(t =>
                        t.Status == Status.Solved ||
                        t.Status == Status.Closed)
                })
                .OrderBy(x => x.Day)
                .ToList();

            return chartData;
        }

        public async Task<List<RecentTicketDto>> GetRecentTicketsAsync()
        {
            var tickets = await _unitOfWork.Tickets.GetAllAsync();

            var recentTickets = tickets
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .Select(t => new RecentTicketDto
                {
                    TicketId = $"T-{t.Id}",
                    RaisedBy = GetUserName(t.CreatedBy),
                    IssueType = t.TicketType.ToString(),
                    AssignedTo = GetAssignedUserName(t),
                    Priority = t.TicketPriority.ToString()
                })
                .ToList();

            return recentTickets;
        }

        public async Task<DashboardResponseDto> GetDashboardAsync()
        {
            var stats = await GetStatsAsync();

            await GetTicketChartAsync();

            var ticketActivityLast7Days = await GetTicketActivityLast7DaysAsync();

            var ticketConflictStatus = await GetTicketConflictStatusAsync();

            var assetDistribution = await GetAssetDistributionAsync();

            var recentActivities = await GetRecentActivitiesAsync();

            var requiresAttention = await GetRequiresAttentionAsync();

            var recentlyDeleted = await GetRecentlyDeletedAsync();

            return new DashboardResponseDto
            {
                Stats = stats,

                TicketActivity = ticketActivityLast7Days,

                TicketConflictStatus = ticketConflictStatus,

                AssetDistribution = assetDistribution,

                RecentActivities = recentActivities,

                RequiresAttention = requiresAttention,

                RecentlyDeleted = recentlyDeleted
            };
        }

        private string GetUserName(int userId)
        {
            var user = _unitOfWork.Users
                .GetByIdAsync(userId).Result;

            return user?.FullName ?? "Unknown";
        }

        private async Task<List<TicketActivityDto>> GetTicketActivityLast7DaysAsync()
        {
            var utcToday = DateTime.UtcNow.Date;
            var from = utcToday.AddDays(-6);

            var tickets = await _unitOfWork.Tickets.GetAllAsync();

            var openedByDay = tickets
                .Where(t => t.CreatedAt.Date >= from && t.CreatedAt.Date <= utcToday)
                .GroupBy(t => t.CreatedAt.Date)
                .ToDictionary(g => g.Key, g => g.Count(t => t.Status == Status.Open || t.Status == Status.InProgress));

            var resolvedByDay = tickets
                .SelectMany(t => t.TicketStatusHistories.Select(h => new { h, t }))
                .Where(x => x.h.NewStatusId == (int)Status.Solved || x.h.NewStatusId == (int)Status.Closed)
                .Where(x => x.h.ChangedAt.Date >= from && x.h.ChangedAt.Date <= utcToday)
                .GroupBy(x => x.h.ChangedAt.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var result = new List<TicketActivityDto>();
            for (var d = from; d <= utcToday; d = d.AddDays(1))
            {
                result.Add(new TicketActivityDto
                {

                    Day = d.ToString("ddd"),
                    Opened = openedByDay.TryGetValue(d, out var opened) ? opened : 0,
                    Resolved = resolvedByDay.TryGetValue(d, out var resolved) ? resolved : 0
                });
            }

            return result;
        }

        private async Task<TicketConflictStatusDto> GetTicketConflictStatusAsync()
        {
            var tickets = await _unitOfWork.Tickets.GetAllAsync();

            int critical = tickets.Count(t => t.TicketPriority == TicketPriority.Critical);
            int high = tickets.Count(t => t.TicketPriority == TicketPriority.High);
            int medium = tickets.Count(t => t.TicketPriority == TicketPriority.Medium);
            int low = tickets.Count(t => t.TicketPriority == TicketPriority.Low);

            return new TicketConflictStatusDto
            {
                Critical = critical,
                High = high,
                Medium = medium,
                Low = low
            };
        }

        private async Task<List<AssetDistributionItemDto>> GetAssetDistributionAsync()
        {
            var assets = await _unitOfWork.Assets.GetAllForDashboardAsync();

            Console.WriteLine($"[Dashboard] GetAssetDistributionAsync assets from GetAllForDashboardAsync count={assets.Count}");
            foreach (var a in assets)
            {
                Console.WriteLine($"[Dashboard] GetAssetDistributionAsync Asset Id={a.Id} SerialNo={a.SerialNo} IsActive={a.IsActive} IsDeleted={a.IsDeleted} StatusId={a.StatusId} AssetStatus={(a.AssetStatus?.Status ?? "null")}");
            }

            string? GetStatus(Asset a) => a.AssetStatus?.Status;

            int CountBy(params string[] statusNames)
            {
                var set = new HashSet<string>(statusNames, StringComparer.OrdinalIgnoreCase);
                return assets.Count(a =>
                    !string.IsNullOrWhiteSpace(GetStatus(a)) &&
                    set.Contains(GetStatus(a)!.Trim()));
            }
            var available = CountBy("Available");
            var assigned = CountBy("Assigned");
            var underRepair = CountBy("UnderRepair", "Under Repair", "InRepair", "Repair");
            var scrap = CountBy("Scrap");
            var returned = CountBy("Returned");

            return new List<AssetDistributionItemDto>
            {
                new AssetDistributionItemDto { Label = "Available", Value = available },
                new AssetDistributionItemDto { Label = "Assigned", Value = assigned },
                new AssetDistributionItemDto { Label = "Under Repair", Value = underRepair },
                new AssetDistributionItemDto { Label = "Scrap", Value = scrap },
                new AssetDistributionItemDto { Label = "Returned", Value = returned },
            };
        }
        
        private static string ToRelativeTime(DateTime? utc)
        {
            if (utc == null)
                return string.Empty;


            var now = DateTime.UtcNow;
            var span = now - utc.Value;

            if (span.TotalSeconds < 0)
                span = TimeSpan.FromSeconds(Math.Abs(span.TotalSeconds));

            if (span.TotalMinutes < 60)
                return $"{Math.Max(1, (int)span.TotalMinutes)} minutes ago";
            if (span.TotalHours < 24)
                return $"{Math.Max(1, (int)span.TotalHours)} hours ago";
            return $"{Math.Max(1, (int)span.TotalDays)} days ago";
        }

        private async Task<List<RecentActivityDto>> GetRecentActivitiesAsync()
        {
            var now = DateTime.UtcNow;
            var from = now.AddDays(-7);

            var tickets = await _unitOfWork.Tickets.GetAllAsync();
            var assets = await _unitOfWork.Assets.GetAllAsync();

            var activities = new List<RecentActivityDto>();

            activities.AddRange(tickets
                .Where(t => !t.IsDeleted && t.CreatedAt >= from)
                .OrderByDescending(t => t.CreatedAt)
                .Take(20)
                .Select(t => new RecentActivityDto
                {
                    Title = $"New Ticket Created - {t.Title}",
                    Time = ToRelativeTime(t.CreatedAt),
                    Type = "ticket_created"
                }));
            activities.AddRange(tickets
                .Where(t => !t.IsDeleted && t.TicketPriority == TicketPriority.Critical && t.CreatedAt >= from)
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .Select(t => new RecentActivityDto
                {
                    Title = $"Ticket {t.Id} marked as Conflict",
                    Time = ToRelativeTime(t.CreatedAt),
                    Type = "ticket_conflict"
                }));

            activities.AddRange(assets
                .Where(a => !a.IsDeleted && a.AssignedTo.HasValue && a.AssignDate.HasValue && a.AssignDate.Value >= from)
                .OrderByDescending(a => a.AssignDate!.Value)
                .Take(20)
                .Select(a => new RecentActivityDto
                {
                    Title = $"Asset {a.SerialNo} assigned",
                    Time = ToRelativeTime(a.AssignDate),
                    Type = "asset_assigned"
                }));

            activities.AddRange(assets
                .Where(a => !a.IsDeleted && a.AssignedTo == null && a.ExpectedReturnDate.HasValue && a.ExpectedReturnDate.Value >= from)
                .OrderByDescending(a => a.ExpectedReturnDate!.Value)
                .Take(20)
                .Select(a => new RecentActivityDto
                {
                    Title = $"Client Asset {a.SerialNo} returned",
                    Time = ToRelativeTime(a.ExpectedReturnDate),
                    Type = "asset_returned"
                }));

            return activities
                .Take(10)
                .ToList();

        }

        private async Task<List<AttentionItemDto>> GetRequiresAttentionAsync()
        {
            var tickets = await _unitOfWork.Tickets.GetAllAsync();
            var assets = await _unitOfWork.Assets.GetAllAsync();

            var attention = new List<AttentionItemDto>();

            attention.Add(new AttentionItemDto
            {
                Label = "Critical Conflict Tickets",
                Count = tickets.Count(t => !t.IsDeleted && t.TicketPriority == TicketPriority.Critical),
                Severity = "critical"
            });

            var now = DateTime.UtcNow;
            var soon = now.AddDays(30);
            attention.Add(new AttentionItemDto
            {
                Label = "Warranty Expiring Soon",
                Count = assets.Count(a => !a.IsDeleted && a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value >= now && a.WarrantyExpiry.Value <= soon),
                Severity = "warning"
            });

            attention.Add(new AttentionItemDto
            {
                Label = "Assets Under Maintenance",
                Count = assets.Count(a => !a.IsDeleted && a.AssetStatus != null &&
                    string.Equals(a.AssetStatus.Status, "UnderRepair", StringComparison.OrdinalIgnoreCase)),
                Severity = "info"
            });

            attention.Add(new AttentionItemDto
            {
                Label = "Unassigned Assets",
                Count = assets.Count(a => !a.IsDeleted && a.AssignedTo == null),
                Severity = "warning"
            });

            return attention;
        }

        private async Task<List<DeletedItemDto>> GetRecentlyDeletedAsync()
        {
            var utcNow = DateTime.UtcNow;
            var from = utcNow.AddDays(-7);

            var deletedAssets = (await _unitOfWork.Assets.GetAllAsync())
                .Where(a => a.IsDeleted && a.DeletedAt.HasValue && a.DeletedAt.Value >= from)
                .OrderByDescending(a => a.DeletedAt)
                .Select(a => new DeletedItemDto
                {
                    Title = a.SerialNo,
                    Time = ToRelativeTime(a.DeletedAt),
                    Type = "asset"
                })
                .ToList();

            var deletedTickets = (await _unitOfWork.Tickets.GetAllAsync())
                .Where(t => t.IsDeleted && t.DeletedAt >= from)
                .OrderByDescending(t => t.DeletedAt)
                .Select(t => new DeletedItemDto
                {
                    Title = t.Title,
                    Time = ToRelativeTime(t.DeletedAt),
                    Type = "ticket"
                })
                .ToList();

            var merged = deletedAssets
                .Concat(deletedTickets)
                .Take(4)
                .ToList();

            return merged;
        }

        private string GetAssignedUserName(Ticket ticket)
        {
            var latestAssignment = ticket.TicketAssignments
                .OrderByDescending(ta => ta.assigned_at)
                .FirstOrDefault();

            if (latestAssignment != null)
            {
                var user = _unitOfWork.Users
                    .GetByIdAsync(latestAssignment.assignedTo).Result;

                return user?.FullName ?? "Unassigned";
            }
            return "Unassigned";
        }

    }
}

