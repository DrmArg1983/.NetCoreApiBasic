using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using netCoreWebApi.Model;
using netCoreWebApi.Proxies;

namespace netCoreWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        readonly IPostsClient postsClient;

        public PostsController(IPostsClient postsClient)
        {
            this.postsClient = postsClient;
        }

        [HttpGet("{id}")]

        public async Task<ActionResult<Post>> Get(int id)
        {
            PostResult result = null;
            Post post;

            try
            {
                result = await postsClient.GetPostById(id);
 
                post = new Post()
                {
                    Id = id,
                    Title = result.Title,
                    Body = result.Body 
                    
                };
            }
            catch (HttpRequestException ex)
            {
               //log something with ex
                return StatusCode(StatusCodes.Status502BadGateway, "Failed request to external resource.");
            }
            catch (Exception ex)
            {
                //log something with ex
                return StatusCode(StatusCodes.Status500InternalServerError, "Request could not be processed.");
            }
            return Ok(post);
        }
    }
}