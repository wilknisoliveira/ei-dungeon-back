using AutoMapper;
using ei_back.Application.Api.User.Dtos;
using ei_back.Application.Usecases.User.Interfaces;
using ei_back.Domain.User.Interfaces;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;
using System.Security.Cryptography;

namespace ei_back.Application.Usecases.User
{
    public class ChangePasswordUseCase(
        IUserService userService,
        IMapper mapper) : IChangePasswordUseCase
    {
        private readonly IUserService _userService = userService;
        private readonly IMapper _mapper = mapper;

        public async Task<UserGetDtoResponse> Handler(PasswordDtoRequest passwordDtoRequest)
        {
            var user = await _userService.FindByIdAsync(passwordDtoRequest.Id) ??
                throw new NotFoundException($"The user id '{passwordDtoRequest.Id}' doesn't exist.");

            var currentPasswordEncrypted = _userService.ComputeHash(passwordDtoRequest.CurrentPassword, SHA256.Create());

            if (currentPasswordEncrypted != user.Password)
                throw new BadRequestException("Wrong password!");

            user.Password = _userService.ComputeHash(passwordDtoRequest.NewPassword, SHA256.Create());

            var response = _userService.Update(user);
            
            return _mapper.Map<UserGetDtoResponse>(response);
        }
    }
}
