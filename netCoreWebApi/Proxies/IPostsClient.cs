using System;
using Refit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace netCoreWebApi.Proxies
{
    public interface IPostsClient
    {
        [Get("/{id}")]
        Task<PostResult> GetPostById(int id);
    }
}
