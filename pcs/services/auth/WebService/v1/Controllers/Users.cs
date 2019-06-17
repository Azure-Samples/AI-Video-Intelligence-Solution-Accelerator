// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.Auth.Services;
using Microsoft.Azure.IoTSolutions.Auth.WebService.Auth;
using Microsoft.Azure.IoTSolutions.Auth.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.Auth.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.Auth.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class UsersController : Controller
    {
        private readonly IUsers users;

        public UsersController(IUsers users)
        {
            this.users = users;
        }

        [HttpGet("{id}")]
        public UserApiModel Get(string id)
        {
            var user = this.users.GetUserInfo(this.Request.GetCurrentUserClaims());

            if (id != "current" && id != user.Id) return null;

            return new UserApiModel(user);
        }

        /// <summary>
        /// This action is used by other services to get allowed action based on
        /// user roles extracted from JWT token but not requiring to pass the token.
        /// </summary>
        /// <param name="id">user object id</param>
        /// <param name="roles">a list of role names</param>
        /// <returns>a list of allowed actions</returns>
        [HttpPost("{id}/allowedActions")]
        public IEnumerable<string> GetAllowedActions([FromRoute]string id, [FromBody]IEnumerable<string> roles)
        {
            return this.users.GetAllowedActions(roles);
        }

        /// <summary>
        /// This action is used by Web UI and other services to get ARM token for
        /// the application to perform resource management task.
        /// </summary>
        /// <param name="id">user object id</param>
        /// <param name="audience">audience of the token, use ARM as default audience</param>
        /// <returns>token for the audience</returns>
        [HttpGet("{id}/token")]
        [Authorize("AcquireToken")]
        public async Task<TokenApiModel> GetToken([FromRoute]string id, [FromQuery]string audience)
        {
            var token = await this.users.GetToken(audience);
            return new TokenApiModel(token);
        }
    }
}
