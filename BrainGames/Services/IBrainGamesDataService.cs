﻿using System;
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
        Task<DataSchemas.BrainGameSessionSchema> AddBrainGameSessionEntryAsync(DataSchemas.BrainGameSessionSchema entry);
        Task<DataSchemas.BrainGameSessionSchema> UpdateBrainGameSessionEntryAsync(DataSchemas.BrainGameSessionSchema entry);
        Task<DataSchemas.UserSchema> AddUserEntryAsync(DataSchemas.UserSchema entry);
    }
}
