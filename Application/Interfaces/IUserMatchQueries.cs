﻿using Domain.Entities;
using System.Windows.Input;

namespace Application.Interfaces
{
    public interface IUserMatchQueries
    {
        Task<IList<UserMatch>> GetAllMatch();
        Task<UserMatch> WasSeen(int userId1, int userId2);
        Task<UserMatch> Saw(int userId1, int userId2);

    }
}
