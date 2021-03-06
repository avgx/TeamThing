﻿using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TeamThing.Web.Core.Helpers;
using TeamThing.Web.Core.Mappers;
using DomainModel = TeamThing.Model;
using ServiceModel = TeamThing.Web.Models.API;

namespace TeamThing.Web.Controllers
{
    public class UserController : ApiController
    {
        // GET /api/user
        private TeamThing.Model.TeamThingContext context;

        public UserController()
        {
            this.context = new TeamThing.Model.TeamThingContext();
        }

        public IQueryable<ServiceModel.UserBasic> Get()
        {
            return context.GetAll<DomainModel.User>().MapToServiceModel();
        }

        // GET /api/user/5
        public HttpResponseMessage Get(int id)
        {
            var item = context.GetAll<TeamThing.Model.User>()
                              .FirstOrDefault(t => t.Id == id);
            if (item == null)
            {
                ModelState.AddModelError("", "Invalid User");
                return new HttpResponseMessage<JsonValue>(ModelState.ToJson(), HttpStatusCode.BadRequest);
            }

            var sUser = item.MapToServiceModel();
            var response = new HttpResponseMessage<ServiceModel.User>(sUser, HttpStatusCode.OK);
            response.Headers.Location = new Uri(Request.RequestUri, "/api/user/" + sUser.Id.ToString());
            return response;
        }

        // GET /api/user/5/
        public IEnumerable<ServiceModel.Thing> Things(int id, int teamId)
        {
            var item = context.GetAll<TeamThing.Model.User>()
                              .FirstOrDefault(t => t.Id == id);

            var team = context.GetAll<TeamThing.Model.Team>()
                               .FirstOrDefault(t => t.Id == teamId);

            var things = team.TeamThings.Where(t => t.AssignedTo.Any(at => at.AssignedToUserId == id));

            return things.MapToServiceModel();
        }

        [HttpPost]
        public HttpResponseMessage SignIn(ServiceModel.SignInViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new HttpResponseMessage<JsonValue>(ModelState.ToJson(), HttpStatusCode.BadRequest);
            }

            var existingUser = context.GetAll<DomainModel.User>()
                                      .FirstOrDefault(u => u.EmailAddress.Equals(model.EmailAddress, StringComparison.OrdinalIgnoreCase));

            if (existingUser == null)
            {
                ModelState.AddModelError("", "A user does not exist with this user name.");
                return new HttpResponseMessage<JsonValue>(ModelState.ToJson(), HttpStatusCode.BadRequest);
            }

            return new HttpResponseMessage<ServiceModel.User>(existingUser.MapToServiceModel());
        }

        [HttpPost]
        public HttpResponseMessage Register(ServiceModel.AddUserModel value)
        {
            if (!ModelState.IsValid)
            {
                return new HttpResponseMessage<JsonValue>(ModelState.ToJson(), HttpStatusCode.BadRequest);
            }

            var existingUser = context.GetAll<DomainModel.User>()
                                      .FirstOrDefault(u => u.EmailAddress.Equals(value.EmailAddress, StringComparison.OrdinalIgnoreCase));

            if (existingUser != null)
            {
                ModelState.AddModelError("", "A user with this email address has already registered!");
                return new HttpResponseMessage<JsonValue>(ModelState.ToJson(), HttpStatusCode.BadRequest);
            }

            var user = new DomainModel.User(value.EmailAddress);
            var defaultImage = new Uri(Request.RequestUri, "/images/GenericUserImage.gif");
            user.ImagePath = defaultImage.ToString();
            context.Add(user);
            context.SaveChanges();

            var sUser = user.MapToServiceModel();
            var response = new HttpResponseMessage<ServiceModel.User>(sUser, HttpStatusCode.Created);
            response.Headers.Location = new Uri(Request.RequestUri, "/api/user/" + sUser.Id.ToString());
            return response;
        }

        // PUT /api/user/5
        public void Put(int id, string value)
        {
        }

        // DELETE /api/user/5
        public void Delete(int id)
        {
            //TODO: Mark inactive
        }
    }
}
