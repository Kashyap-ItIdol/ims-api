using AutoMapper;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Entities;

namespace IMS_Application.Services
{
    public class ReportService : IReportService
    {
        private readonly ITicketService _ticketService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReportService(ITicketService ticketService, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _ticketService = ticketService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<TicketReportDto>> GetTicketReportsAsync(int currentUserId, DateTime? startDate, DateTime? endDate, int? supportEngineerId)
        {
            try
            {
                var ticketsResult = await _ticketService.GetAllTicketsForReportAsync(currentUserId);
                if (!ticketsResult.IsSuccess)
                {
                    return Result<TicketReportDto>.Failure(ticketsResult.Message, ticketsResult.StatusCode);
                }

                var allTicketsDto = ticketsResult.Data;
                var allTickets = new List<Ticket>();
                var repoTickets = await _unitOfWork.Tickets.GetAllWithAssignmentsAsync();
                var allUsers = await _unitOfWork.Users.GetAllWithRolesAsync();

                var supportEngineers = allUsers
                    .Where(u => u.Role != null && u.Role.Name == LogicStrings.SupportEngineerRole && !u.IsDeleted)
                    .ToList();

                var effectiveStartDate = startDate ?? DateTime.UtcNow.AddDays(-7);
                var effectiveEndDate = endDate?.AddDays(1).AddTicks(-1) ?? DateTime.UtcNow;

                var filteredTickets = repoTickets
                    .Where(t => t.CreatedAt >= effectiveStartDate && t.CreatedAt <= effectiveEndDate)
                    .ToList();

                if (supportEngineerId.HasValue)
                {
                    var assignedTicketIds = repoTickets
                        .SelectMany(t => t.TicketAssignments)
                        .Where(a => a.assignedTo == supportEngineerId.Value)
                        .Select(a => a.TicketId)
                        .Distinct();

                    filteredTickets = filteredTickets
                        .Where(t => assignedTicketIds.Contains(t.Id))
                        .ToList();
                }

                var totalTickets = filteredTickets.Count;
                var solvedTickets = filteredTickets.Count(t => t.Status == Status.Solved || t.Status == Status.Closed);
                var reopenedTickets = filteredTickets.Count(t => WasTicketReopened(t, repoTickets));
                var conflictTickets = reopenedTickets;

                var criticalSolved = solvedTickets > 0 ? filteredTickets.Count(t =>
                    (t.Status == Status.Solved || t.Status == Status.Closed) && t.TicketPriority == TicketPriority.Critical) : 0;
                var criticalReopened = reopenedTickets > 0 ? filteredTickets.Count(t =>
                    WasTicketReopened(t, repoTickets) && t.TicketPriority == TicketPriority.Critical) : 0;
                var criticalConflict = conflictTickets > 0 ? filteredTickets.Count(t =>
                    WasTicketReopened(t, repoTickets) && t.TicketPriority == TicketPriority.Critical) : 0;

                var previousPeriodMessage = totalTickets > 0 ? $"{totalTickets} vs previous period" : "No previous data";

                var activityOverview = filteredTickets
                    .GroupBy(t => t.CreatedAt.Date)
                    .ToDictionary(
                        g => g.Key.ToString("yyyy-MM-dd"),
                        g => new DayActivityDto
                        {
                            Assigned = g.Count(),
                            Solved = g.Count(t => t.Status == Status.Solved || t.Status == Status.Closed)
                        });

                var totalCount = filteredTickets.Count;
                var criticalCount = filteredTickets.Count(t => t.TicketPriority == TicketPriority.Critical);
                var highCount = filteredTickets.Count(t => t.TicketPriority == TicketPriority.High);
                var mediumCount = filteredTickets.Count(t => t.TicketPriority == TicketPriority.Medium);
                var lowCount = filteredTickets.Count(t => t.TicketPriority == TicketPriority.Low);

                var ticketTypeAnalysis = new TicketTypeAnalysisDto
                {
                    Hardware = filteredTickets.Count(t => t.TicketType == TicketType.Hardware),
                    Software = filteredTickets.Count(t => t.TicketType == TicketType.Software),
                    Website = filteredTickets.Count(t => t.TicketType == TicketType.Website),
                    Server = filteredTickets.Count(t => t.TicketType == TicketType.Server),
                    Other = filteredTickets.Count(t => t.TicketType != TicketType.Hardware &&
                                                       t.TicketType != TicketType.Software &&
                                                       t.TicketType != TicketType.Website &&
                                                       t.TicketType != TicketType.Server)
                };

                var allAssignments = repoTickets.SelectMany(t => t.TicketAssignments).ToList();
                var ticketPerEngineer = CalculateTicketPerSupportEngineer(filteredTickets, allAssignments, supportEngineers);

                var result = new TicketReportDto
                {
                    TotalTicketsAssignmentKpi = new ReportKpiDto
                    {
                        TotalCount = totalTickets,
                        PreviousPeriod = new PreviousPeriodDto
                        {
                            PercentageChange = 0,
                            Trend = "stable",
                            Message = previousPeriodMessage
                        },
                        CriticalCount = 0,
                        Message = $"{totalTickets} Total Tickets"
                    },
                    TotalTicketsSolvedKpi = new ReportKpiDto
                    {
                        TotalCount = solvedTickets,
                        PreviousPeriod = new PreviousPeriodDto
                        {
                            PercentageChange = 0,
                            Trend = "stable",
                            Message = solvedTickets > 0 ? $"{solvedTickets} solved" : "No tickets solved"
                        },
                        CriticalCount = criticalSolved,
                        Message = $"{solvedTickets} Tickets Solved"
                    },
                    TotalTicketsReopenedKpi = new ReportKpiDto
                    {
                        TotalCount = reopenedTickets,
                        PreviousPeriod = new PreviousPeriodDto
                        {
                            PercentageChange = 0,
                            Trend = "stable",
                            Message = reopenedTickets > 0 ? $"{reopenedTickets} reopened" : "No tickets reopened"
                        },
                        CriticalCount = criticalReopened,
                        Message = $"{reopenedTickets} Reopened Tickets"
                    },
                    TotalTicketsConflictHandledKpi = new ReportKpiDto
                    {
                        TotalCount = conflictTickets,
                        PreviousPeriod = new PreviousPeriodDto
                        {
                            PercentageChange = 0,
                            Trend = "stable",
                            Message = conflictTickets > 0 ? $"{conflictTickets} conflict" : "No conflict tickets"
                        },
                        CriticalCount = criticalConflict,
                        Message = $"{conflictTickets} Conflict Tickets Handled"
                    },
                    TicketActivityOverview = activityOverview,
                    PriorityDistribution = new PriorityDistributionDto
                    {
                        Critical = new PriorityItemDto
                        {
                            Count = criticalCount,
                            Percentage = totalCount > 0 ? (int)Math.Round((double)criticalCount / totalCount * 100) : 0
                        },
                        High = new PriorityItemDto
                        {
                            Count = highCount,
                            Percentage = totalCount > 0 ? (int)Math.Round((double)highCount / totalCount * 100) : 0
                        },
                        Medium = new PriorityItemDto
                        {
                            Count = mediumCount,
                            Percentage = totalCount > 0 ? (int)Math.Round((double)mediumCount / totalCount * 100) : 0
                        },
                        Low = new PriorityItemDto
                        {
                            Count = lowCount,
                            Percentage = totalCount > 0 ? (int)Math.Round((double)lowCount / totalCount * 100) : 0
                        }
                    },
                    TicketTypeAnalysis = ticketTypeAnalysis,
                    TicketPerSupportEngineer = ticketPerEngineer
                };

                return Result<TicketReportDto>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<TicketReportDto>.Failure(ex.Message, 500);
            }
        }

        private bool WasTicketReopened(Ticket ticket, IEnumerable<Ticket> allTickets)
        {
            if (ticket.Status == Status.Open || ticket.Status == Status.InProgress)
            {
                var history = ticket.TicketStatusHistories?.ToList();
                if (history != null && history.Any())
                {
                    return history.Any(h => h.OldStatusId == (int)Status.Solved || h.OldStatusId == (int)Status.Closed);
                }
            }
            return false;
        }

        private Dictionary<string, EngineerStageDto> CalculateTicketPerSupportEngineer(
            List<Ticket> filteredTickets,
            ICollection<TicketAssignment> allAssignments,
            List<User> supportEngineers)
        {
            var result = new Dictionary<string, EngineerStageDto>();

            var stages = new[] { "open", "in_progress", "solved", "closed" };

            if (!supportEngineers.Any())
            {
                return result;
            }

            foreach (var stage in stages)
            {
                var status = stage switch
                {
                    "open" => Status.Open,
                    "in_progress" => Status.InProgress,
                    "solved" => Status.Solved,
                    "closed" => Status.Closed,
                    _ => Status.Open
                };

                var ticketsInStage = filteredTickets.Where(t => t.Status == status).ToList();
                var engineerCounts = new Dictionary<string, int>();

                foreach (var engineer in supportEngineers)
                {
                    var assignedCount = allAssignments.Count(a =>
                        a.assignedTo == engineer.Id &&
                        ticketsInStage.Any(t => t.Id == a.TicketId));

                    engineerCounts[engineer.FullName] = assignedCount;
                }

                result[stage] = new EngineerStageDto { Engineers = engineerCounts };
            }

            return result;
        }
    }
}