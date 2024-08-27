namespace API.Persistance
{
    public class ApiDbContext : MDbContext
    {
        public ApiDbContext(DbContextOptions options, IMediator mediator) : base(options, mediator)
        {
        }

        public ApiDbContext() : base(new DbContextOptions<ApiDbContext>(), new NoMediator())
        {
        }
    }
}
