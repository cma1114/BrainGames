using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using BrainGames.Models;

namespace BrainGames.Services
{
    public class BrainGamesAPIDataService : BaseHttpService, IBrainGamesDataService
    {
        readonly Uri _baseUri;
        readonly IDictionary<string, string> _headers;

        public BrainGamesAPIDataService(Uri baseUri)
        {
            _baseUri = baseUri;
            _headers = new Dictionary<string, string>();

            // TODO: Add header with auth-based token in chapter 7
            _headers.Add("zumo-api-version", "2.0.0");
        }

        public async Task<IList<DataSchemas.ITGameRecordSchema>> GetITGameRecordEntriesAsync()
        {
            var url = new Uri(_baseUri, "/tables/bgitgamerecord");
            var response = await SendRequestAsync<DataSchemas.ITGameRecordSchema[]>(url, HttpMethod.Get, _headers);

            return response;
        }

        public async Task<DataSchemas.ITGameRecordSchema> AddITGameRecordEntryAsync(DataSchemas.ITGameRecordSchema entry)
        {
            var url = new Uri(_baseUri, "/tables/bgitgamerecord");
            var response = await SendRequestAsync<DataSchemas.ITGameRecordSchema>(url, HttpMethod.Post, _headers, entry);

            return response;
        }

        public async Task<DataSchemas.ITGameRecordSchema> UpdateITGameRecordEntryAsync(DataSchemas.ITGameRecordSchema entry)
        {
            var url = new Uri(_baseUri, string.Format("/tables/bgitgamerecord/{0}", entry.Id));
            var response = await SendRequestAsync<DataSchemas.ITGameRecordSchema>(url, new HttpMethod("PATCH"), _headers, entry);

            return response;
        }

        public async Task<DataSchemas.RTGameRecordSchema> AddRTGameRecordEntryAsync(DataSchemas.RTGameRecordSchema entry)
        {
            var url = new Uri(_baseUri, "/tables/bgrtgamerecord");
            var response = await SendRequestAsync<DataSchemas.RTGameRecordSchema>(url, HttpMethod.Post, _headers, entry);

            return response;
        }

        public async Task<DataSchemas.RTGameRecordSchema> UpdateRTGameRecordEntryAsync(DataSchemas.RTGameRecordSchema entry)
        {
            var url = new Uri(_baseUri, string.Format("/tables/bgrtgamerecord/{0}", entry.Id));
            var response = await SendRequestAsync<DataSchemas.RTGameRecordSchema>(url, new HttpMethod("PATCH"), _headers, entry);

            return response;
        }

        public async Task<DataSchemas.StroopGameRecordSchema> AddStroopGameRecordEntryAsync(DataSchemas.StroopGameRecordSchema entry)
        {
            var url = new Uri(_baseUri, "/tables/bgstroopgamerecord");
            var response = await SendRequestAsync<DataSchemas.StroopGameRecordSchema>(url, HttpMethod.Post, _headers, entry);

            return response;
        }

        public async Task<DataSchemas.StroopGameRecordSchema> UpdateStroopGameRecordEntryAsync(DataSchemas.StroopGameRecordSchema entry)
        {
            var url = new Uri(_baseUri, string.Format("/tables/bgstroopgamerecord/{0}", entry.Id));
            var response = await SendRequestAsync<DataSchemas.StroopGameRecordSchema>(url, new HttpMethod("PATCH"), _headers, entry);

            return response;
        }

        public async Task<DataSchemas.DSGameRecordSchema> AddDSGameRecordEntryAsync(DataSchemas.DSGameRecordSchema entry)
        {
            var url = new Uri(_baseUri, "/tables/bgdsgamerecord");
            var response = await SendRequestAsync<DataSchemas.DSGameRecordSchema>(url, HttpMethod.Post, _headers, entry);

            return response;
        }

        public async Task<DataSchemas.DSGameRecordSchema> UpdateDSGameRecordEntryAsync(DataSchemas.DSGameRecordSchema entry)
        {
            var url = new Uri(_baseUri, string.Format("/tables/bgdsgamerecord/{0}", entry.Id));
            var response = await SendRequestAsync<DataSchemas.DSGameRecordSchema>(url, new HttpMethod("PATCH"), _headers, entry);

            return response;
        }

        public async Task<DataSchemas.LSGameRecordSchema> AddLSGameRecordEntryAsync(DataSchemas.LSGameRecordSchema entry)
        {
            var url = new Uri(_baseUri, "/tables/bglsgamerecord");
            var response = await SendRequestAsync<DataSchemas.LSGameRecordSchema>(url, HttpMethod.Post, _headers, entry);

            return response;
        }

        public async Task<DataSchemas.LSGameRecordSchema> UpdateLSGameRecordEntryAsync(DataSchemas.LSGameRecordSchema entry)
        {
            var url = new Uri(_baseUri, string.Format("/tables/bglsgamerecord/{0}", entry.Id));
            var response = await SendRequestAsync<DataSchemas.LSGameRecordSchema>(url, new HttpMethod("PATCH"), _headers, entry);

            return response;
        }

        public async Task<DataSchemas.UserSchema> AddUserEntryAsync(DataSchemas.UserSchema entry)
        {
            var url = new Uri(_baseUri, "/tables/bguser");
            var response = await SendRequestAsync<DataSchemas.UserSchema>(url, HttpMethod.Post, _headers, entry);

            return response;
        }

        public async Task<DataSchemas.UserSchema> UpdateUserEntryAsync(DataSchemas.UserSchema entry)
        {
            var url = new Uri(_baseUri, string.Format("/tables/bguser/{0}", entry.Id));
            var response = await SendRequestAsync<DataSchemas.UserSchema>(url, new HttpMethod("PATCH"), _headers, entry);

            return response;
        }

        public async Task<DataSchemas.SharingUsersSchema> AddSharingEntryAsync(DataSchemas.SharingUsersSchema entry)
        {
            var url = new Uri(_baseUri, "/tables/bgsharingusers");
            var response = await SendRequestAsync<DataSchemas.SharingUsersSchema>(url, HttpMethod.Post, _headers, entry);

            return response;
        }

        public async Task<DataSchemas.SharingUsersSchema> UpdateSharingEntryAsync(DataSchemas.SharingUsersSchema entry)
        {
            var url = new Uri(_baseUri, string.Format("/tables/bgsharingusers/{0}", entry.Id));
            var response = await SendRequestAsync<DataSchemas.SharingUsersSchema>(url, new HttpMethod("PATCH"), _headers, entry);

            return response;
        }

        public async Task<DataSchemas.BrainGameSessionSchema> AddBrainGameSessionEntryAsync(DataSchemas.BrainGameSessionSchema entry)
        {
            var url = new Uri(_baseUri, "/tables/bgsession");
            var response = await SendRequestAsync<DataSchemas.BrainGameSessionSchema>(url, HttpMethod.Post, _headers, entry);

            return response;
        }
        public async Task<DataSchemas.BrainGameSessionSchema> UpdateBrainGameSessionEntryAsync(DataSchemas.BrainGameSessionSchema entry)
        {
            var url = new Uri(_baseUri, string.Format("/tables/bgsession/{0}", entry.Id));
            var response = await SendRequestAsync<DataSchemas.BrainGameSessionSchema>(url, new HttpMethod("PATCH"), _headers, entry);

            return response;
        }
    }
}
