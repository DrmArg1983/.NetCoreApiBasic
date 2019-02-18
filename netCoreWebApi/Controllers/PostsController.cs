using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using netCoreWebApi.Model;
using netCoreWebApi.Proxies;
using Polly.Timeout;

namespace netCoreWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        readonly IPostsClient postsClient;
        readonly ILogger<PostsController> logger;

        public PostsController(
            IPostsClient postsClient,
            ILoggerFactory logger)
        {
            this.postsClient = postsClient;
            this.logger = logger.CreateLogger<PostsController>();
        }


        [HttpGet("{id}")]
        /// <summary>
        /// Retrieve a post based on id.
        /// </summary>
        /// <param id="id">id od the post.</param>
        /// <returns>Detailed post information.</returns>
        public async Task<ActionResult<Post>> Get(int id)
        {
            PostResult result = null;
            Post post;

            try
            {
                logger.LogInformation("Acquiring details for {Posts}.", id);

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
                logger.LogWarning(ex, "Http request failed.");
                return StatusCode(StatusCodes.Status502BadGateway, "Failed request to external resource.");
            }
            catch (TimeoutRejectedException ex)
            {
                logger.LogWarning(ex, "Timeout occurred when retrieving details for {Posts}.", id); //sematic logging
                return StatusCode(StatusCodes.Status504GatewayTimeout, "Timeout on external web request.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unknown exception occurred while retrieving posts details.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Request could not be processed.");
            }
            return Ok(post);
        }
    }
}