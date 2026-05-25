using AutoMapper;
using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;

namespace IMS_Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }


        private IUserRepository? _users;

        public IUserRepository Users =>
            _users ??= new UserRepository(_context);

        private IRepository<Role>? _roles;
        public IRepository<Role> Roles =>
            _roles ??= new Repository<Role>(_context);

        private IRepository<Department>? _departments;
        public IRepository<Department> Departments =>
            _departments ??= new Repository<Department>(_context);

        private ICategoryRepository? _categories;
        public ICategoryRepository Categories =>
            _categories ??= new CategoryRepository(_context);

        private ISubCategoryRepository? _subCategories;
        public ISubCategoryRepository SubCategories =>
            _subCategories ??= new SubCategoryRepository(_context);

        private ITicketRepository? _tickets;
        public ITicketRepository Tickets =>
            _tickets ??= new TicketRepository(_context);

        private IAssetRepository? _assets;
        public IAssetRepository Assets =>
            _assets ??= new AssetRepository(_context);

        private INetworkDetailsRepository? _networkDetails;
        public INetworkDetailsRepository NetworkDetails =>
            _networkDetails ??= new NetworkDetailsRepository(_context);

        private IClientAssetRepository? _clientAssets;
        public IClientAssetRepository ClientAssets =>
            _clientAssets ??= new ClientAssetRepository(_context);

        private IAssetAssignmentRepository? _assetAssignments;
        public IAssetAssignmentRepository AssetAssignments =>
            _assetAssignments ??= new AssetAssignmentRepository(_context);

        private IRepository<TicketAttachment>? _ticketAttachments;
        public IRepository<TicketAttachment> TicketAttachments =>
            _ticketAttachments ??= new Repository<TicketAttachment>(_context);

        private INotificationRepository? _notifications;
        public INotificationRepository Notifications =>
            _notifications ??= new NotificationRepository(_context);


        public UnitOfWork(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)

        {
            return await _context.SaveChangesAsync(cancellationToken);
        }



        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}