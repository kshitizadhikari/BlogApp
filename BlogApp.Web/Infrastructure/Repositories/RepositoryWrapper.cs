using BlogApp.Web.Data;
using BlogApp.Web.Infrastructure.Interfaces;

namespace BlogApp.Web.Infrastructure.Repositories
{
    public class RepositoryWrapper: IRepositoryWrapper
    {

        private AppDbContext _dbContext;
        private IAppUserRepository _userRepository;
        private IPostRepository _postRepository;
        private ICommentRepository _commentRepository;

        public RepositoryWrapper(AppDbContext dbContext, IAppUserRepository userRepository, IPostRepository postRepository, ICommentRepository commentRepository)
        {
            _dbContext = dbContext;
            _userRepository = userRepository;
            _postRepository = postRepository;
            _commentRepository = commentRepository;
        }

        public IAppUserRepository AppUser => _userRepository;

        public IPostRepository Post => _postRepository;

        public ICommentRepository Comment => _commentRepository;

        public async Task Save()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
