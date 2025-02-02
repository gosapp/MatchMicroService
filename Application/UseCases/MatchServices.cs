﻿
using Application.Interfaces;
using Application.Models;
using Domain.Entities;

namespace Application.UseCases
{
    public class MatchServices : IMatchServices
    {
        private readonly IMatchCommands _commands;
        private readonly IMatchQueries _queries;
        private readonly IChatApiServices _chatApiServices;
        private readonly IUserApiServices _userApiServices;

        public MatchServices(IMatchCommands commands, IMatchQueries queries, IChatApiServices chatApiServices, IUserApiServices userApiServices)
        {
            _commands = commands;
            _queries = queries;
            _chatApiServices = chatApiServices;
            _userApiServices = userApiServices;
        }

        public async Task<MatchResponse2> CreateMatch(MatchRequest request)
        {
            Match match = new Match()
            {
                User1Id = request.User1,
                User2Id = request.User2,
                CreatedAt = DateTime.UtcNow,
                View1 = false,
                View2 = false
            };

            Match create = await _commands.CreateMatch(match);

            if(create != null)
            {
                //Creo una instancia de Chat -> Chat API
                var chatResp = await _chatApiServices.CreateChat(match.User1Id, match.User2Id);

                if(chatResp != null)
                {
                    MatchResponse2 response = new MatchResponse2()
                    {
                        Id = create.MatchId,
                        ChatId = chatResp.ChatId
                    };

                    return response;
                }
                else
                {
                    MatchResponse2 noChatResp = new MatchResponse2()
                    {
                        Id = create.MatchId,
                        ChatId = -1
                    };

                    return noChatResp;
                }
            }
            else
            {
                return null;
            }
            
        }

        public async Task DeleteMatch(int id)
        {
            await _commands.DeleteMatch(id);
        }

        public async Task<MatchResponse> GetById(int id)
        {
            Match match = await _queries.GetById(id);

            if (match == null)
            {
                return null;
            }

            MatchResponse response = new MatchResponse()
            {
                Id = match.MatchId,
                User1 = match.User1Id,
                User2 = match.User2Id,
            };

            return response;
        }

        public async Task<IList<MatchResponse>> GetByUserId(int userId)
        {
            IList<Match> matches = await _queries.GetByUserId(userId);

            IList<MatchResponse> matchResponses = new List<MatchResponse>();

            if (matches.Count == 0)
            {
                return matchResponses;
            }

            foreach (Match match in matches)
            {
                // Se pone al UserId buscado siempre en el User1 para simplificar busquedas.
                MatchResponse response = new MatchResponse()
                {
                    Id = match.MatchId,
                    User1 = userId == match.User1Id ? match.User1Id : match.User2Id,
                    User2 = userId != match.User2Id ? match.User2Id : match.User1Id,
                    View1 = match.View1,
                    View2 = match.View2
                };

                matchResponses.Add(response);
            }

            return matchResponses;
        }

        public async Task<MatchResponse> GetByUsersIds(int userId1, int userId2)
        {
            Match match = await _queries.GetByUsersIds(userId1, userId2);

            if (match == null)
            {
                return null;
            }


            MatchResponse response = new MatchResponse()
            {
                Id = match.MatchId,
                User1 = match.User1Id,
                User2 = match.User2Id,
                View1 = match.View1,
                View2 = match.View2
            };

            return response;
        }
        public async Task<IList<MatchResponse>> GetAll()
        {
            IList<MatchResponse> matches = await _queries.GetAllMatch();
            return matches;
        }

        public async Task<bool> ExistMatch(int userId1, int userId2)
        {
            bool exist = await _queries.Exist(userId1, userId2);
            return exist;
        }

        public async Task<bool> UpdateMatch(MatchRequestUpdate request)
        {
            try
            {
                Match match = new Match()
                {
                    User1Id = request.User1,
                    User2Id = request.User2,
                    CreatedAt = DateTime.UtcNow,
                    View1 = request.View1,
                    View2 = request.View2
                };

                Match update = await _commands.UpdateMatch(match);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<IEnumerable<RankResponse2>> GetTopMatchUser()
        {
            try
            {
                var topUsers = await _queries.ListTopMatchUsers();
                List<int> listTopUsers = new List<int>();
                List<RankResponse2> rankResponse = new List<RankResponse2>();

                foreach (var user in topUsers)
                {
                    listTopUsers.Add(user.UserId);
                }

                if(topUsers != null)
                {
                    var topUsersInfo = await _userApiServices.GetUsers(listTopUsers);
                    foreach (var user in topUsers)
                    {
                        var userInfo = new RankResponse2
                        {
                            UserId = user.UserId,
                            MatchQty = user.MatchQty,
                            UserResponse = topUsersInfo.FirstOrDefault(x => x.UserId == user.UserId)
                        };

                        rankResponse.Add(userInfo);
                    }

                    return rankResponse;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
