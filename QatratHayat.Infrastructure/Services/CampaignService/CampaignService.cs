using Microsoft.EntityFrameworkCore;
using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Common.Exceptions;
using QatratHayat.Application.Features.Campaigns.DTOS;
using QatratHayat.Application.Features.Campaigns.Interfaces;
using QatratHayat.Domain.Entities;
using QatratHayat.Domain.Enums;
using QatratHayat.Infrastructure.Persistence;

namespace QatratHayat.Application.Features.Campaigns.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly AppDbContext _context;

        public CampaignService(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // Get Campaign Statistics
        // ============================================================

        public async Task<CampaignStatisticsResponseDto> GetStatisticsAsync(
            int currentUserId,
            bool isAdmin
        )
        {
            var campaignsQuery = _context
                .Campaigns
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .AsQueryable();

            if (!isAdmin)
            {
                var managerBranchId = await GetRequiredBranchManagerBranchIdAsync(currentUserId);

                campaignsQuery = campaignsQuery.Where(c => c.BranchId == managerBranchId);
            }

            return new CampaignStatisticsResponseDto
            {
                TotalCampaigns = await campaignsQuery.CountAsync(),

                PlannedCampaigns = await campaignsQuery.CountAsync(c =>
                    c.Status == CampaignStatus.Planned
                ),

                ActiveCampaigns = await campaignsQuery.CountAsync(c =>
                    c.Status == CampaignStatus.Active
                ),

                CompletedCampaigns = await campaignsQuery.CountAsync(c =>
                    c.Status == CampaignStatus.Completed
                ),

                CancelledCampaigns = await campaignsQuery.CountAsync(c =>
                    c.Status == CampaignStatus.Cancelled
                ),

                InternalCampaigns = await campaignsQuery.CountAsync(c =>
                    c.Type == CampaignType.Internal
                ),

                ExternalCampaigns = await campaignsQuery.CountAsync(c =>
                    c.Type == CampaignType.External
                ),

                LastUpdate = await campaignsQuery
                    .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                    .Select(c => (DateTime?)(c.UpdatedAt ?? c.CreatedAt))
                    .FirstOrDefaultAsync(),
            };
        }

        // ============================================================
        // Get All Campaigns
        // ============================================================

        public async Task<PagedResultDto<CampaignResponseDto>> GetAllCampaignsAsync(
            CampaignQueryDto query,
            int currentUserId,
            bool isAdmin
        )
        {
            NormalizePaging(query);

            var campaignsQuery = _context
                .Campaigns
                .AsNoTracking()
                .Include(c => c.Branch)
                .Include(c => c.TargetBloodTypes)
                .Include(c => c.DonationIntents)
                .Include(c => c.Donations)
                .Where(c => !c.IsDeleted)
                .AsQueryable();

            if (!isAdmin)
            {
                var managerBranchId = await GetRequiredBranchManagerBranchIdAsync(currentUserId);

                campaignsQuery = campaignsQuery.Where(c => c.BranchId == managerBranchId);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var searchTerm = query.SearchTerm.Trim();

                campaignsQuery = campaignsQuery.Where(c =>
                    c.TitleAr.Contains(searchTerm)
                    || c.TitleEn.Contains(searchTerm)
                    || c.DescriptionAr.Contains(searchTerm)
                    || c.DescriptionEn.Contains(searchTerm)
                    || (c.Location != null && c.Location.Contains(searchTerm))
                    || (
                        c.Branch != null
                        && (
                            c.Branch.BranchNameAr.Contains(searchTerm)
                            || c.Branch.BranchNameEn.Contains(searchTerm)
                        )
                    )
                );
            }

            if (query.Status.HasValue)
            {
                campaignsQuery = campaignsQuery.Where(c => c.Status == query.Status.Value);
            }

            if (query.Type.HasValue)
            {
                campaignsQuery = campaignsQuery.Where(c => c.Type == query.Type.Value);
            }

            if (query.BranchId.HasValue)
            {
                campaignsQuery = campaignsQuery.Where(c => c.BranchId == query.BranchId.Value);
            }

            if (query.BloodType.HasValue)
            {
                campaignsQuery = campaignsQuery.Where(c =>
                    c.TargetBloodTypes.Any(t => t.BloodType == query.BloodType.Value)
                );
            }

            var totalCount = await campaignsQuery.CountAsync();

            var campaigns = await campaignsQuery
                .OrderByDescending(c => c.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var items = campaigns.Select(MapCampaignToDto).ToList();

            return new PagedResultDto<CampaignResponseDto>
            {
                Items = items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount,
            };
        }

        // ============================================================
        // Get Campaign By Id
        // ============================================================

        public async Task<CampaignResponseDto> GetCampaignByIdAsync(
            int campaignId,
            int currentUserId,
            bool isAdmin
        )
        {
            var campaignsQuery = _context
                .Campaigns
                .AsNoTracking()
                .Include(c => c.Branch)
                .Include(c => c.TargetBloodTypes)
                .Include(c => c.DonationIntents)
                .Include(c => c.Donations)
                .Where(c => c.Id == campaignId && !c.IsDeleted)
                .AsQueryable();

            if (!isAdmin)
            {
                var managerBranchId = await GetRequiredBranchManagerBranchIdAsync(currentUserId);

                campaignsQuery = campaignsQuery.Where(c => c.BranchId == managerBranchId);
            }

            var campaign = await campaignsQuery.FirstOrDefaultAsync();

            if (campaign is null)
            {
                throw new NotFoundException(
                    "Campaign was not found.",
                    ErrorCodes.CampaignNotFound
                );
            }

            return MapCampaignToDto(campaign);
        }

        // ============================================================
        // Create Campaign
        // ============================================================

        public async Task<CampaignResponseDto> CreateCampaignAsync(
            CreateCampaignRequestDto request,
            int createdByUserId,
            bool isAdmin
        )
        {
            ValidateDateRange(request.StartDate, request.EndDate);

            ValidateTargetBloodTypes(request.TargetBloodTypes);

            var effectiveBranchId = request.BranchId;

            if (!isAdmin)
            {
                effectiveBranchId = await GetRequiredBranchManagerBranchIdAsync(createdByUserId);
            }

            await ValidateCampaignLocationRulesAsync(
                request.Type,
                effectiveBranchId,
                request.Location
            );

            await ValidateCampaignTitleUniquenessAsync(
                request.TitleAr,
                request.TitleEn
            );

            var campaign = new Campaign
            {
                TitleAr = request.TitleAr.Trim(),
                TitleEn = request.TitleEn.Trim(),
                DescriptionAr = request.DescriptionAr.Trim(),
                DescriptionEn = request.DescriptionEn.Trim(),
                Type = request.Type,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = CampaignStatus.Planned,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Location = string.IsNullOrWhiteSpace(request.Location)
                    ? null
                    : request.Location.Trim(),
                CreatedByUserId = createdByUserId,
                BranchId = effectiveBranchId,
                TargetBloodTypes = request.TargetBloodTypes
                    .Distinct()
                    .Select(bloodType => new CampaignTargetBloodType
                    {
                        BloodType = bloodType,
                    })
                    .ToList(),
            };

            _context.Campaigns.Add(campaign);

            await _context.SaveChangesAsync();

            var createdCampaign = await _context
                .Campaigns
                .AsNoTracking()
                .Include(c => c.Branch)
                .Include(c => c.TargetBloodTypes)
                .Include(c => c.DonationIntents)
                .Include(c => c.Donations)
                .FirstAsync(c => c.Id == campaign.Id);

            return MapCampaignToDto(createdCampaign);
        }

        // ============================================================
        // Update Campaign
        // ============================================================

        public async Task<CampaignResponseDto> UpdateCampaignAsync(
            int campaignId,
            UpdateCampaignRequestDto request,
            int currentUserId,
            bool isAdmin
        )
        {
            var campaign = await _context
                .Campaigns
                .Include(c => c.TargetBloodTypes)
                .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);

            if (campaign is null)
            {
                throw new NotFoundException(
                    "Campaign was not found.",
                    ErrorCodes.CampaignNotFound
                );
            }

            var effectiveBranchId = request.BranchId;

            if (!isAdmin)
            {
                var managerBranchId = await GetRequiredBranchManagerBranchIdAsync(currentUserId);

                if (campaign.BranchId != managerBranchId)
                {
                    throw new BadRequestException(
                        "You are not allowed to manage this campaign.",
                        ErrorCodes.CampaignAccessDenied
                    );
                }

                effectiveBranchId = managerBranchId;
            }

            ValidateDateRange(request.StartDate, request.EndDate);

            ValidateTargetBloodTypes(request.TargetBloodTypes);

            await ValidateCampaignLocationRulesAsync(
                request.Type,
                effectiveBranchId,
                request.Location
            );

            await ValidateCampaignTitleUniquenessAsync(
                request.TitleAr,
                request.TitleEn,
                excludedCampaignId: campaignId
            );

            campaign.TitleAr = request.TitleAr.Trim();
            campaign.TitleEn = request.TitleEn.Trim();
            campaign.DescriptionAr = request.DescriptionAr.Trim();
            campaign.DescriptionEn = request.DescriptionEn.Trim();
            campaign.Type = request.Type;
            campaign.Status = request.Status;
            campaign.StartDate = request.StartDate;
            campaign.EndDate = request.EndDate;
            campaign.Location = string.IsNullOrWhiteSpace(request.Location)
                ? null
                : request.Location.Trim();
            campaign.BranchId = effectiveBranchId;
            campaign.UpdatedAt = DateTime.UtcNow;

            _context.CampaignTargetBloodTypes.RemoveRange(campaign.TargetBloodTypes);

            campaign.TargetBloodTypes = request.TargetBloodTypes
                .Distinct()
                .Select(bloodType => new CampaignTargetBloodType
                {
                    CampaignId = campaign.Id,
                    BloodType = bloodType,
                })
                .ToList();

            await _context.SaveChangesAsync();

            var updatedCampaign = await _context
                .Campaigns
                .AsNoTracking()
                .Include(c => c.Branch)
                .Include(c => c.TargetBloodTypes)
                .Include(c => c.DonationIntents)
                .Include(c => c.Donations)
                .FirstAsync(c => c.Id == campaign.Id);

            return MapCampaignToDto(updatedCampaign);
        }

        // ============================================================
        // Soft Delete Campaign
        // ============================================================

        public async Task SoftDeleteCampaignAsync(
            int campaignId,
            int currentUserId,
            bool isAdmin
        )
        {
            var campaign = await _context.Campaigns.FirstOrDefaultAsync(c =>
                c.Id == campaignId && !c.IsDeleted
            );

            if (campaign is null)
            {
                throw new NotFoundException(
                    "Campaign was not found.",
                    ErrorCodes.CampaignNotFound
                );
            }

            await EnsureCampaignCanBeManagedAsync(
                campaign,
                currentUserId,
                isAdmin
            );

            var hasDonationIntents = await _context.DonationIntents.AnyAsync(di =>
                di.CampaignId == campaignId
            );

            if (hasDonationIntents)
            {
                throw new ConflictException(
                    "Campaign cannot be deleted because it has linked donation intents.",
                    ErrorCodes.CampaignHasLinkedDonationIntents
                );
            }

            var hasDonations = await _context.Donations.AnyAsync(d =>
                d.CampaignId == campaignId
            );

            if (hasDonations)
            {
                throw new ConflictException(
                    "Campaign cannot be deleted because it has linked donations.",
                    ErrorCodes.CampaignHasLinkedDonations
                );
            }

            campaign.IsDeleted = true;
            campaign.Status = CampaignStatus.Cancelled;
            campaign.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ============================================================
        // Activate Campaign
        // ============================================================

        public async Task ActivateCampaignAsync(
            int campaignId,
            int currentUserId,
            bool isAdmin
        )
        {
            var campaign = await _context.Campaigns.FirstOrDefaultAsync(c =>
                c.Id == campaignId && !c.IsDeleted
            );

            if (campaign is null)
            {
                throw new NotFoundException(
                    "Campaign was not found.",
                    ErrorCodes.CampaignNotFound
                );
            }

            await EnsureCampaignCanBeManagedAsync(
                campaign,
                currentUserId,
                isAdmin
            );

            if (campaign.Status != CampaignStatus.Planned)
            {
                throw new BadRequestException(
                    "Only planned campaigns can be activated.",
                    ErrorCodes.CampaignInvalidStatus
                );
            }

            ValidateDateRange(campaign.StartDate, campaign.EndDate);

            campaign.Status = CampaignStatus.Active;
            campaign.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ============================================================
        // Complete Campaign
        // ============================================================

        public async Task CompleteCampaignAsync(
            int campaignId,
            int currentUserId,
            bool isAdmin
        )
        {
            var campaign = await _context.Campaigns.FirstOrDefaultAsync(c =>
                c.Id == campaignId && !c.IsDeleted
            );

            if (campaign is null)
            {
                throw new NotFoundException(
                    "Campaign was not found.",
                    ErrorCodes.CampaignNotFound
                );
            }

            await EnsureCampaignCanBeManagedAsync(
                campaign,
                currentUserId,
                isAdmin
            );

            if (campaign.Status != CampaignStatus.Active)
            {
                throw new BadRequestException(
                    "Only active campaigns can be completed.",
                    ErrorCodes.CampaignInvalidStatus
                );
            }

            campaign.Status = CampaignStatus.Completed;
            campaign.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ============================================================
        // Cancel Campaign
        // ============================================================

        public async Task CancelCampaignAsync(
            int campaignId,
            int currentUserId,
            bool isAdmin
        )
        {
            var campaign = await _context.Campaigns.FirstOrDefaultAsync(c =>
                c.Id == campaignId && !c.IsDeleted
            );

            if (campaign is null)
            {
                throw new NotFoundException(
                    "Campaign was not found.",
                    ErrorCodes.CampaignNotFound
                );
            }

            await EnsureCampaignCanBeManagedAsync(
                campaign,
                currentUserId,
                isAdmin
            );

            if (campaign.Status == CampaignStatus.Completed)
            {
                throw new BadRequestException(
                    "Completed campaigns cannot be cancelled.",
                    ErrorCodes.CampaignInvalidStatus
                );
            }

            if (campaign.Status == CampaignStatus.Cancelled)
            {
                throw new BadRequestException(
                    "Campaign is already cancelled.",
                    ErrorCodes.CampaignInvalidStatus
                );
            }

            campaign.Status = CampaignStatus.Cancelled;
            campaign.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ============================================================
        // Validation Helpers
        // ============================================================

        private static void ValidateDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
            {
                throw new BadRequestException(
                    "Campaign start date must be before end date.",
                    ErrorCodes.CampaignInvalidDateRange
                );
            }
        }

        private static void ValidateTargetBloodTypes(List<BloodType> targetBloodTypes)
        {
            if (targetBloodTypes is null || targetBloodTypes.Count == 0)
            {
                throw new BadRequestException(
                    "At least one target blood type is required.",
                    ErrorCodes.CampaignTargetBloodTypesRequired
                );
            }

            if (targetBloodTypes.Count != targetBloodTypes.Distinct().Count())
            {
                throw new BadRequestException(
                    "Duplicate campaign target blood types are not allowed.",
                    ErrorCodes.DuplicateCampaignTargetBloodTypes
                );
            }
        }

        private async Task ValidateCampaignLocationRulesAsync(
            CampaignType type,
            int? branchId,
            string? location
        )
        {
            if (type == CampaignType.Internal)
            {
                if (!branchId.HasValue)
                {
                    throw new BadRequestException(
                        "Branch is required for internal campaigns.",
                        ErrorCodes.CampaignBranchRequired
                    );
                }

                await ValidateBranchIsActiveAsync(branchId.Value);

                return;
            }

            if (type == CampaignType.External)
            {
                if (string.IsNullOrWhiteSpace(location))
                {
                    throw new BadRequestException(
                        "Location is required for external campaigns.",
                        ErrorCodes.CampaignLocationRequired
                    );
                }

                if (branchId.HasValue)
                {
                    await ValidateBranchIsActiveAsync(branchId.Value);
                }
            }
        }

        private async Task ValidateBranchIsActiveAsync(int branchId)
        {
            var branchExists = await _context.Branches.AnyAsync(b =>
                b.Id == branchId && b.IsActive && !b.IsDeleted
            );

            if (!branchExists)
            {
                throw new NotFoundException(
                    "Branch was not found or is inactive.",
                    ErrorCodes.BranchInactiveOrNotFound
                );
            }
        }

        private async Task ValidateCampaignTitleUniquenessAsync(
            string titleAr,
            string titleEn,
            int? excludedCampaignId = null
        )
        {
            var normalizedTitleAr = titleAr.Trim();
            var normalizedTitleEn = titleEn.Trim();

            var exists = await _context.Campaigns.AnyAsync(c =>
                !c.IsDeleted
                && (
                    c.TitleAr == normalizedTitleAr
                    || c.TitleEn == normalizedTitleEn
                )
                && (!excludedCampaignId.HasValue || c.Id != excludedCampaignId.Value)
            );

            if (exists)
            {
                throw new ConflictException(
                    "A campaign with the same Arabic or English title already exists.",
                    ErrorCodes.CampaignAlreadyExists
                );
            }
        }

        private static void NormalizePaging(CampaignQueryDto query)
        {
            if (query.PageNumber < 1)
            {
                query.PageNumber = 1;
            }

            if (query.PageSize < 1)
            {
                query.PageSize = 10;
            }

            if (query.PageSize > 100)
            {
                query.PageSize = 100;
            }
        }

        // ============================================================
        // Authorization Helpers
        // ============================================================

        private async Task<int> GetRequiredBranchManagerBranchIdAsync(int userId)
        {
            var branchId = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId && u.IsActive && !u.IsDeleted)
                .Select(u => u.BranchId)
                .FirstOrDefaultAsync();

            if (!branchId.HasValue)
            {
                throw new BadRequestException(
                    "Branch manager must be assigned to an active branch.",
                    ErrorCodes.BranchManagerBranchRequired
                );
            }

            var branchExists = await _context.Branches.AnyAsync(b =>
                b.Id == branchId.Value && b.IsActive && !b.IsDeleted
            );

            if (!branchExists)
            {
                throw new NotFoundException(
                    "Branch was not found or is inactive.",
                    ErrorCodes.BranchInactiveOrNotFound
                );
            }

            return branchId.Value;
        }

        private async Task EnsureCampaignCanBeManagedAsync(
            Campaign campaign,
            int currentUserId,
            bool isAdmin
        )
        {
            if (isAdmin)
            {
                return;
            }

            var managerBranchId = await GetRequiredBranchManagerBranchIdAsync(currentUserId);

            if (campaign.BranchId != managerBranchId)
            {
                throw new BadRequestException(
                    "You are not allowed to manage this campaign.",
                    ErrorCodes.CampaignAccessDenied
                );
            }
        }

        // ============================================================
        // Mapping Helper
        // ============================================================

        private static CampaignResponseDto MapCampaignToDto(Campaign campaign)
        {
            return new CampaignResponseDto
            {
                Id = campaign.Id,
                TitleAr = campaign.TitleAr,
                TitleEn = campaign.TitleEn,
                DescriptionAr = campaign.DescriptionAr,
                DescriptionEn = campaign.DescriptionEn,
                Type = campaign.Type,
                Status = campaign.Status,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                IsDeleted = campaign.IsDeleted,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt,
                Location = campaign.Location,
                CreatedByUserId = campaign.CreatedByUserId,
                BranchId = campaign.BranchId,
                BranchNameAr = campaign.Branch?.BranchNameAr,
                BranchNameEn = campaign.Branch?.BranchNameEn,
                TargetBloodTypes = campaign.TargetBloodTypes
                    .Select(t => t.BloodType)
                    .ToList(),
                DonationIntentsCount = campaign.DonationIntents.Count,
                DonationsCount = campaign.Donations.Count,
            };
        }
    }
}