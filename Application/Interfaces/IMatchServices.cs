﻿using Application.Models;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IMatchServices
    {
        Task<MatchResponse> CreateMatch(MatchRequest request);
        Task DeleteMatch(int id);
        Task<IList<Match>> GetAll();
        Task<MatchResponse> GetById(int id);
        Task<IList<MatchResponse>> GetByUserId(int userId);
        Task<MatchResponse> GetByUsersIds(int userId1, int userId2);
    }
}
