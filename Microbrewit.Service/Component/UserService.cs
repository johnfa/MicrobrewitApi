﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microbrewit.Model;
using Microbrewit.Model.DTOs;
using Microbrewit.Repository;
using Microbrewit.Service.Elasticsearch.Interface;
using Microbrewit.Service.Interface;

namespace Microbrewit.Service.Component
{
    public class UserService : IUserService
    {
        private readonly IUserElasticsearch _userElasticsearch;
        private readonly IUserRepository _userRepository;

        public UserService(IUserElasticsearch userElasticsearch, IUserRepository userRepository)
        {
            _userElasticsearch = userElasticsearch;
            _userRepository = userRepository;
        }

        public async  Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var userDtos = await _userElasticsearch.GetAllAsync();
            if (userDtos.Any()) return userDtos;
            var users = await _userRepository.GetAllAsync();
            return Mapper.Map<IEnumerable<User>, IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto> GetSingleAsync(string username)
        {
            var userDto = await _userElasticsearch.GetSingleAsync(username);
            if (userDto != null) return userDto;
            var user = await _userRepository.GetSingleAsync(o => o.Username == username);
            return Mapper.Map<User, UserDto>(user);
        }

        public async Task<UserDto> AddAsync(UserDto userDto)
        {
            var user = Mapper.Map<UserDto, User>(userDto);
            await _userRepository.AddAsync(user);
            var result = await _userRepository.GetSingleAsync(o => o.Username == user.Username);
            var mappedResult = Mapper.Map<User, UserDto>(result);
            await _userElasticsearch.UpdateAsync(mappedResult);
            return mappedResult;

        }

        public async Task<UserDto> DeleteAsync(string username)
        {
            var user = await _userRepository.GetSingleAsync(o => o.Username == username);
            var userDto = await _userElasticsearch.GetSingleAsync(username);
            if(user != null) await _userRepository.RemoveAsync(user);
            if (userDto != null) await _userElasticsearch.DeleteAsync(username);
            return userDto;
        }

        public async Task UpdateAsync(UserDto userDto)
        {
            var user = Mapper.Map<UserDto, User>(userDto);
            await _userRepository.UpdateAsync(user);
            var result = await _userRepository.GetSingleAsync(o => o.Username == userDto.Username);
            var mappedResult = Mapper.Map<User, UserDto>(result);
            await _userElasticsearch.UpdateAsync(mappedResult);
        }

        public async Task<IEnumerable<UserDto>> SearchAsync(string query, int @from, int size)
        {
            return await _userElasticsearch.SearchAsync(query,from,size);
        }

        public async Task ReIndexElasticSearch()
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = Mapper.Map<IList<User>, IList<UserDto>>(users);
            await _userElasticsearch.UpdateAllAsync(userDtos);
        }
    }
}