﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OneLogin.Requests;
using OneLogin.Responses;

namespace OneLogin
{
    public partial class OneLoginClient
    {
        /// <summary>
        /// This call returns a list of all OneLogin event types available to the Events API, providing event type names, event type IDs, and descriptions.
        /// </summary>
        /// <returns></returns>
        public async Task<GetEventTypesResponse> GetEventTypes()
        {
            return await GetResource<GetEventTypesResponse>($"{Endpoints.ONELOGIN_EVENTS}/types");
        }

        /// <summary>
        /// An event that was recorded in OneLogin.
        /// </summary>
        /// <param name="clientId">Client ID used to generate the access token that made the API call that generated the event.</param>
        /// <param name="createdAt">Time and date at which the event was created. This value is autogenerated by OneLogin.</param>
        /// <param name="directoryId"></param>
        /// <param name="eventTypeId">Type of event triggered.</param>
        /// <param name="id">Event’s unique ID in OneLogin. Autogenerated by OneLogin.</param>
        /// <param name="resolution"></param>
        /// <param name="since"></param>
        /// <param name="until"></param>
        /// <param name="userId">ID of the user that was acted upon to trigger the event.</param>
        /// <returns></returns>
        public async Task<GetEventsResponse> GetEvents(string clientId = null, DateTime? createdAt = null, 
            string directoryId = null, int? eventTypeId = null, long? id = null, string resolution = null, 
            DateTime? since = null, DateTime? until = null, int? userId = null)
        {
            var parameters = new Dictionary<string, string>
                {
                    {"clientId", clientId},
                    {"created_at", createdAt.ToString()},
                    {"directory_id", directoryId},
                    {"event_type_id", eventTypeId.ToString()},
                    {"id", id.ToString()},
                    {"resolution", resolution},
                    {"since", since.ToString()},
                    {"until", until.ToString()},
                    {"user_id", userId.ToString()},
                };

            return await GetResource<GetEventsResponse>(Endpoints.ONELOGIN_EVENTS, parameters);
        }

        /// <summary>
        /// An event that was recorded in OneLogin.
        /// </summary>
        /// <param name="id">Set to the id of the event that you want to return. If you don’t know the event’s  id, use the Get Events API call to return all events and their id values.</param>
        /// <returns></returns>
        public async Task<GetEventsResponse> GetEventById(long id)
        {
            return await GetResource<GetEventsResponse>($"{Endpoints.ONELOGIN_EVENTS}/{id}");
        }

        /// <summary>
        /// Use to create an event in the OneLogin event log.
        /// From individual user actions, to administrative operations, provisioning, and OTP device registration, everything that happens within your OneLogin account can be tracked.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<EmptyResponse> CreateEvent(CreateEventRequest message)
        {
            return await PostResource<EmptyResponse>($"{Endpoints.ONELOGIN_EVENTS}", message);
        }

        //todo: webhooks
    }
}
