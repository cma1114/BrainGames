using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrainGames.Models;

namespace BrainGames.Services
{
    public interface IBrainGamesDataService
    {
        Task<IList<DataSchemas.ITGameRecordSchema>> GetITGameRecordEntriesAsync();
        Task<DataSchemas.ITGameRecordSchema> AddITGameRecordEntryAsync(DataSchemas.ITGameRecordSchema entry);
        Task<DataSchemas.ITGameRecordSchema> UpdateITGameRecordEntryAsync(DataSchemas.ITGameRecordSchema entry);
        Task<DataSchemas.RTGameRecordSchema> AddRTGameRecordEntryAsync(DataSchemas.RTGameRecordSchema entry);
        Task<DataSchemas.RTGameRecordSchema> UpdateRTGameRecordEntryAsync(DataSchemas.RTGameRecordSchema entry);
        Task<DataSchemas.StroopGameRecordSchema> AddStroopGameRecordEntryAsync(DataSchemas.StroopGameRecordSchema entry);
        Task<DataSchemas.StroopGameRecordSchema> UpdateStroopGameRecordEntryAsync(DataSchemas.StroopGameRecordSchema entry);
        Task<DataSchemas.DSGameRecordSchema> AddDSGameRecordEntryAsync(DataSchemas.DSGameRecordSchema entry);
        Task<DataSchemas.DSGameRecordSchema> UpdateDSGameRecordEntryAsync(DataSchemas.DSGameRecordSchema entry);
        Task<DataSchemas.LSGameRecordSchema> AddLSGameRecordEntryAsync(DataSchemas.LSGameRecordSchema entry);
        Task<DataSchemas.LSGameRecordSchema> UpdateLSGameRecordEntryAsync(DataSchemas.LSGameRecordSchema entry);
        Task<DataSchemas.BrainGameSessionSchema> AddBrainGameSessionEntryAsync(DataSchemas.BrainGameSessionSchema entry);
        Task<DataSchemas.BrainGameSessionSchema> UpdateBrainGameSessionEntryAsync(DataSchemas.BrainGameSessionSchema entry);
        Task<DataSchemas.UserSchema> AddUserEntryAsync(DataSchemas.UserSchema entry);
        Task<DataSchemas.UserSchema> UpdateUserEntryAsync(DataSchemas.UserSchema entry);
        Task<DataSchemas.SharingUsersSchema> AddSharingEntryAsync(DataSchemas.SharingUsersSchema entry);
        Task<DataSchemas.SharingUsersSchema> UpdateSharingEntryAsync(DataSchemas.SharingUsersSchema entry);
        Task<DataSchemas.UserFeedbackSchema> AddUserFeedbackEntryAsync(DataSchemas.UserFeedbackSchema entry);
        Task<DataSchemas.ConnectedUsersSchema> AddConnectedEntryAsync(DataSchemas.ConnectedUsersSchema entry);
        Task<DataSchemas.ConnectedUsersSchema> UpdateConnectedEntryAsync(DataSchemas.ConnectedUsersSchema entry);
        Task<DataSchemas.UserStatsSchema> AddUserStatsEntryAsync(DataSchemas.UserStatsSchema entry);
        Task<DataSchemas.UserStatsSchema> UpdateUserStatsEntryAsync(DataSchemas.UserStatsSchema entry);
    }
}
